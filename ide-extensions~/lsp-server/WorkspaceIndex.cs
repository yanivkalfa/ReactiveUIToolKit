using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace UitkxLanguageServer;

/// <summary>
/// Scans the workspace for <c>*Props.cs</c> files on startup and maintains
/// a set of known element names together with their prop declarations and
/// source locations.
///
/// Example: <c>ButtonProps.cs</c> → element name <c>Button</c> with props
/// <c>string text</c>, <c>Action onClick</c>, etc.
///
/// Used by diagnostics (UITKX0105), completion, hover, and go-to-definition.
/// </summary>
public sealed class WorkspaceIndex : IOnLanguageServerStarted
{
    // ── Inner types ──────────────────────────────────────────────────────────

    /// <summary>A single publicly-declared prop on a *Props class.</summary>
    public sealed class PropInfo
    {
        public string Name   { get; init; } = "";
        public string Type   { get; init; } = "";
        /// <summary>Cleaned XML doc text (tags stripped), or empty string.</summary>
        public string XmlDoc { get; init; } = "";
        /// <summary>1-based line number of the declaration in the source file.</summary>
        public int    Line   { get; init; }
    }

    /// <summary>All info gathered for one UITKX element (its <c>*Props</c> class).</summary>
    public sealed class ElementInfo
    {
        public string         FilePath { get; init; } = "";
        /// <summary>1-based line of the class declaration.</summary>
        public int            FileLine { get; init; }
        public List<PropInfo> Props    { get; init; } = new();
    }

    // ── State ────────────────────────────────────────────────────────────────

    private readonly HashSet<string>                 _elements    = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ElementInfo> _elementInfo = new(StringComparer.Ordinal);
    private readonly ReaderWriterLockSlim            _lock        = new();

    // ── Patterns ─────────────────────────────────────────────────────────────

    // Matches:  "class FooProps"  "record FooProps"  "struct FooProps"
    private static readonly Regex s_classPattern = new(
        @"\b(?:class|record|struct)\s+(\w+)Props\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    // Matches:  "class Props"  "record Props"  "struct Props"  (no prefix — component name = filename stem)
    private static readonly Regex s_nestedPropsPattern = new(
        @"\b(?:class|record|struct)\s+Props\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    // Matches public property declarations with a Pascal-case name.
    // E.g.  "public string Text {"  "public Action<int>? OnClick {"
    private static readonly Regex s_propPattern = new(
        @"^\s*public\s+(?<type>[\w<>\[\],\s\?]+?)\s+(?<name>[A-Z][A-Za-z0-9_]*)\s*\{",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    // Matches function-style component declarations inside a .uitkx file:
    //   component FooBar {
    //   @component FooBar
    private static readonly Regex s_uitkxComponentPattern = new(
        @"^(?:@component|component)\s+([A-Z][A-Za-z0-9_]*)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Multiline);

    // Extended declaration pattern that also captures the optional parameter list.
    // Group "name" = component name.  Group "params" = raw param list if present.
    //   component ShowcaseTopBar(string inputText = "", Action? onSetText = null)
    private static readonly Regex s_uitkxDeclPattern = new(
        @"^(?:@component|component)\s+(?<name>[A-Z][A-Za-z0-9_]*)(?:\s*\((?<params>[^)]*)\))?",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Multiline);

    // ── IOnLanguageServerStarted ─────────────────────────────────────────────

    /// <summary>
    /// Fired once on the thread-pool after the initial <see cref="ScanDirectory"/> completes.
    /// Subscribe to re-validate open documents that were diagnosed before the scan finished.
    /// </summary>
    public event Action? ScanCompleted;

    public Task OnStarted(ILanguageServer server, CancellationToken cancellationToken)
    {
        string? root = null;
        try
        {
            var rootUri = server.ClientSettings.RootUri?.ToString();
            if (!string.IsNullOrEmpty(rootUri))
                root = new Uri(rootUri).LocalPath;
        }
        catch { /* URI parse failure — fall through */ }

        if (string.IsNullOrEmpty(root))
            root = server.ClientSettings.RootPath;

        if (!string.IsNullOrEmpty(root))
            _ = Task.Run(() => { ScanDirectory(root); ScanCompleted?.Invoke(); }, cancellationToken);

        return Task.CompletedTask;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>A snapshot of all element names currently in the index.</summary>
    public HashSet<string> KnownElements
    {
        get
        {
            _lock.EnterReadLock();
            try { return new HashSet<string>(_elements, StringComparer.Ordinal); }
            finally { _lock.ExitReadLock(); }
        }
    }

    /// <summary>
    /// Returns <c>true</c> if any element names have been indexed (i.e. the
    /// background scan has completed at least partially).
    /// </summary>
    public bool IsReady
    {
        get
        {
            _lock.EnterReadLock();
            try { return _elements.Count > 0; }
            finally { _lock.ExitReadLock(); }
        }
    }

    /// <summary>Returns the props declared on the element, or an empty list.</summary>
    public IReadOnlyList<PropInfo> GetProps(string elementName)
    {
        _lock.EnterReadLock();
        try
        {
            return _elementInfo.TryGetValue(elementName, out var info)
                ? info.Props
                : (IReadOnlyList<PropInfo>)Array.Empty<PropInfo>();
        }
        finally { _lock.ExitReadLock(); }
    }

    /// <summary>Returns the <see cref="ElementInfo"/> for the element, or <c>null</c>.</summary>
    public ElementInfo? TryGetElementInfo(string elementName)
    {
        _lock.EnterReadLock();
        try
        {
            return _elementInfo.TryGetValue(elementName, out var info) ? info : null;
        }
        finally { _lock.ExitReadLock(); }
    }

    /// <summary>
    /// Re-index a single file that was added, changed, or deleted.
    /// Called by <see cref="DiagnosticsPublisher"/> on workspace file-change
    /// notifications.
    /// </summary>
    public void Refresh(string filePath)
    {
        if (filePath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
        {
            if (!File.Exists(filePath))
            {
                var dir = Path.GetDirectoryName(filePath);
                if (dir is not null) ScanDirectory(dir);
                return;
            }
            IndexFile(filePath);
        }
        else if (filePath.EndsWith(".uitkx", StringComparison.OrdinalIgnoreCase))
        {
            if (!File.Exists(filePath))
                return; // deletion — component will disappear on next full scan
            IndexUitkxFile(filePath);
        }
    }

    // ── Scanning ─────────────────────────────────────────────────────────────

    private void ScanDirectory(string rootPath)
    {
        try
        {
            ServerLog.Log($"WorkspaceIndex: scanning '{rootPath}' for *.cs and *.uitkx filesâ€¦");
            int count = 0;
            foreach (var file in Directory.EnumerateFiles(
                rootPath, "*.cs", SearchOption.AllDirectories))
            {
                IndexFile(file);
                count++;
            }
            ServerLog.Log(
                $"WorkspaceIndex: indexed {count} Props file(s) → {_elements.Count} element name(s).");

            // Also scan .uitkx files to discover function-style component names
            // (e.g. `component FooBar { … }` or `@component FooBar`).
            int uitkxCount = 0;
            foreach (var file in Directory.EnumerateFiles(
                rootPath, "*.uitkx", SearchOption.AllDirectories))
            {
                IndexUitkxFile(file);
                uitkxCount++;
            }
            ServerLog.Log(
                $"WorkspaceIndex: indexed {uitkxCount} .uitkx file(s) → {_elements.Count} total element name(s).");
        }
        catch (Exception ex)
        {
            ServerLog.Log($"WorkspaceIndex scan error: {ex.Message}");
        }
    }

    /// <summary>
    /// Extracts component names declared in a <c>.uitkx</c> file and adds them to the index.
    /// Handles both the function-style syntax (<c>component FooBar { … }</c>) and the
    /// classic directive syntax (<c>@component FooBar</c>).
    /// When a parameter list is present (<c>component FooBar(type name = default, …)</c>)
    /// the parameters are also stored as props so that UITKX0109 (unknown attribute) does
    /// not fire for valid attributes on those components.
    /// </summary>
    private void IndexUitkxFile(string filePath)
    {
        try
        {
            string content = File.ReadAllText(filePath);
            foreach (Match m in s_uitkxDeclPattern.Matches(content))
            {
                string componentName = m.Groups["name"].Value;
                if (string.IsNullOrEmpty(componentName))
                    continue;

                // Find the 1-based line number of the match
                int lineNumber = 1;
                for (int i = 0; i < m.Index && i < content.Length; i++)
                    if (content[i] == '\n') lineNumber++;

                // Parse function-style params if present.
                // Each param looks like:  type paramName  or  type paramName = default
                // Strategy: split by comma, then for each token split by "=" and take the
                // last whitespace-separated word from the part before "=".
                var props = new List<PropInfo>();
                string paramsText = m.Groups["params"].Value;
                if (!string.IsNullOrWhiteSpace(paramsText))
                {
                    foreach (string param in paramsText.Split(','))
                    {
                        string beforeEquals = param.Split('=')[0].Trim();
                        string[] tokens = beforeEquals.Split(
                            new[] { ' ', '\t', '\n', '\r' },
                            StringSplitOptions.RemoveEmptyEntries);
                        if (tokens.Length >= 2)
                        {
                            props.Add(new PropInfo
                            {
                                Name = tokens[tokens.Length - 1],
                                Type = string.Join(" ", tokens, 0, tokens.Length - 1),
                                Line = lineNumber,
                            });
                        }
                    }
                }

                CommitElement(filePath, componentName, lineNumber, props);
            }
        }
        catch
        {
            // File read errors are silently ignored — best-effort index.
        }
    }

    private void IndexFile(string filePath)
    {
        try
        {
            var lines = File.ReadAllLines(filePath);
            string? currentElement = null;
            int     currentLine   = 0;
            var     currentProps  = new List<PropInfo>();
            var     xmlBuf        = new List<string>();

            for (int i = 0; i < lines.Length; i++)
            {
                var raw     = lines[i];
                var trimmed = raw.Trim();

                // ── Class / record / struct *Props declaration ────────────
                var classMatch = s_classPattern.Match(trimmed);
                if (classMatch.Success)
                {
                    if (currentElement is not null)
                        CommitElement(filePath, currentElement, currentLine, currentProps);

                    currentElement = classMatch.Groups[1].Value;
                    currentLine    = i + 1;
                    currentProps   = new List<PropInfo>();
                    xmlBuf.Clear();
                    continue;
                }

                // ── Nested "class Props" without prefix — use filename stem ─
                var nestedPropsMatch = s_nestedPropsPattern.Match(trimmed);
                if (nestedPropsMatch.Success)
                {
                    if (currentElement is not null)
                        CommitElement(filePath, currentElement, currentLine, currentProps);

                    currentElement = Path.GetFileNameWithoutExtension(filePath);
                    currentLine    = i + 1;
                    currentProps   = new List<PropInfo>();
                    xmlBuf.Clear();
                    continue;
                }

                // ── XML doc comment ───────────────────────────────────────
                if (trimmed.StartsWith("///"))
                {
                    xmlBuf.Add(trimmed.Length > 3 ? trimmed[3..].TrimStart() : "");
                    continue;
                }

                // ── Public property declaration ───────────────────────────
                var propMatch = s_propPattern.Match(raw);
                if (propMatch.Success && currentElement is not null)
                {
                    currentProps.Add(new PropInfo
                    {
                        Name   = propMatch.Groups["name"].Value,
                        Type   = propMatch.Groups["type"].Value.Trim(),
                        XmlDoc = BuildDoc(xmlBuf),
                        Line   = i + 1,
                    });
                    xmlBuf.Clear();
                    continue;
                }

                // Any non-comment, non-empty line resets the doc buffer.
                if (!string.IsNullOrWhiteSpace(trimmed))
                    xmlBuf.Clear();
            }

            if (currentElement is not null)
                CommitElement(filePath, currentElement, currentLine, currentProps);
        }
        catch
        {
            // File read errors are silently ignored — best-effort index.
        }
    }

    private void CommitElement(
        string filePath, string elementName, int fileLine, List<PropInfo> props)
    {
        var info = new ElementInfo
        {
            FilePath = filePath,
            FileLine = fileLine,
            Props    = new List<PropInfo>(props),
        };
        _lock.EnterWriteLock();
        try
        {
            _elements.Add(elementName);
            _elementInfo[elementName] = info;
        }
        finally { _lock.ExitWriteLock(); }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static string BuildDoc(List<string> xmlBuf)
    {
        if (xmlBuf.Count == 0) return "";
        var raw = string.Join(" ", xmlBuf);
        return Regex.Replace(raw, @"<[^>]+>", "").Trim();
    }
}
