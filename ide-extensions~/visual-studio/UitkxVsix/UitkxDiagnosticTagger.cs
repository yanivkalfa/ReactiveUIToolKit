using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using Newtonsoft.Json.Linq;

namespace UitkxVsix;

// ── Error tag tagger provider ───────────────────────────────────────────────

[Export(typeof(ITaggerProvider))]
[ContentType("uitkx")]
[TagType(typeof(IErrorTag))]
internal sealed class UitkxDiagnosticTaggerProvider : ITaggerProvider
{
    public ITagger<T>? CreateTagger<T>(ITextBuffer buffer) where T : ITag
    {
        if (typeof(T) != typeof(IErrorTag))
            return null;

        return buffer.Properties.GetOrCreateSingletonProperty(
            () => new UitkxDiagnosticTagger(buffer)
        ) as ITagger<T>;
    }
}

// ── Unreachable code dim tagger provider ────────────────────────────────────

[Export(typeof(ITaggerProvider))]
[ContentType("uitkx")]
[TagType(typeof(IClassificationTag))]
internal sealed class UitkxUnreachableCodeTaggerProvider : ITaggerProvider
{
    [Import]
    internal IClassificationTypeRegistryService ClassificationRegistry { get; set; } = null!;

    public ITagger<T>? CreateTagger<T>(ITextBuffer buffer) where T : ITag
    {
        if (typeof(T) != typeof(IClassificationTag))
            return null;

        var excludedCode = ClassificationRegistry.GetClassificationType("excluded code")
            ?? ClassificationRegistry.GetClassificationType("text");

        return buffer.Properties.GetOrCreateSingletonProperty(
            typeof(UitkxUnreachableCodeTagger),
            () => new UitkxUnreachableCodeTagger(buffer, excludedCode)
        ) as ITagger<T>;
    }
}

// ── Tagger ──────────────────────────────────────────────────────────────────

/// <summary>
/// Listens for textDocument/publishDiagnostics notifications via the JsonRpc
/// pipe and produces IErrorTag spans that VS2022 renders as squiggles and
/// Error List entries.
/// </summary>
internal sealed class UitkxDiagnosticTagger : ITagger<IErrorTag>, IDisposable
{
    private readonly ITextBuffer _buffer;
    private List<LspDiagnostic> _diagnostics = new();
    private bool _disposed;
    private System.Threading.Timer? _debounceTimer;
    private System.Threading.Timer? _changeDebounceTimer;
    private List<LspDiagnostic>? _pendingDiagnostics;

    private static readonly string LogPath = Path.Combine(
        Path.GetTempPath(),
        "uitkx-diag-tagger.log"
    );

    private static void Log(string msg)
    {
        try
        {
            File.AppendAllText(LogPath, $"[{DateTime.UtcNow:O}] {msg}\n");
        }
        catch { }
    }

    public UitkxDiagnosticTagger(ITextBuffer buffer)
    {
        _buffer = buffer;
        Log("UitkxDiagnosticTagger created");
        UitkxDiagnosticStore.DiagnosticsChanged += OnDiagnosticsChanged;
        _buffer.Changed += OnBufferChanged;
    }

    /// <summary>
    /// VS2022 doesn't send textDocument/didChange for custom content types.
    /// We push buffer changes to the server ourselves so diagnostics update
    /// when users type errors. Debounced to 300ms to avoid excess traffic.
    /// </summary>
    private void OnBufferChanged(object sender, TextContentChangedEventArgs e)
    {
        if (_disposed)
            return;

        if (_changeDebounceTimer == null)
            _changeDebounceTimer = new System.Threading.Timer(OnChangeDebounceElapsed, null, 300, System.Threading.Timeout.Infinite);
        else
            _changeDebounceTimer.Change(300, System.Threading.Timeout.Infinite);
    }

    private void OnChangeDebounceElapsed(object? state)
    {
        if (_disposed)
            return;

        var rpc = UitkxLanguageClient.InternalRpc;
        if (rpc == null)
            return;

        if (!_buffer.Properties.TryGetProperty(typeof(ITextDocument), out ITextDocument doc))
            return;

        var uri = new Uri(doc.FilePath).AbsoluteUri;
        var text = _buffer.CurrentSnapshot.GetText();

        // Fire and forget — diagnostics will arrive via publishDiagnostics.
        System.Threading.Tasks.Task.Run(async () =>
        {
            try
            {
                await rpc.NotifyWithParameterObjectAsync(
                    "textDocument/didChange",
                    new
                    {
                        textDocument = new { uri, version = 1 },
                        contentChanges = new[] { new { text } },
                    }
                );
            }
            catch { }
        });
    }

    private void OnDiagnosticsChanged(string uri, List<LspDiagnostic> diagnostics)
    {
        if (_disposed)
            return;

        // Match by file URI.
        if (!_buffer.Properties.TryGetProperty(typeof(ITextDocument), out ITextDocument doc))
            return;

        var myUri = new Uri(doc.FilePath).AbsoluteUri;
        if (!string.Equals(myUri, uri, StringComparison.OrdinalIgnoreCase))
            return;

        // Debounce: the server pushes T1+T2 then T3 rapidly. Wait 200ms for
        // the final batch before raising TagsChanged to avoid squiggle flicker.
        _pendingDiagnostics = diagnostics;

        if (_debounceTimer == null)
            _debounceTimer = new System.Threading.Timer(OnDebounceElapsed, null, 200, System.Threading.Timeout.Infinite);
        else
            _debounceTimer.Change(200, System.Threading.Timeout.Infinite);
    }

    private void OnDebounceElapsed(object? state)
    {
        if (_disposed)
            return;

        var pending = _pendingDiagnostics;
        if (pending == null)
            return;

        _diagnostics = pending;
        _pendingDiagnostics = null;
        Log($"Applied {pending.Count} diagnostics (debounced)");

        try
        {
            var snapshot = _buffer.CurrentSnapshot;
            TagsChanged?.Invoke(
                this,
                new SnapshotSpanEventArgs(new SnapshotSpan(snapshot, 0, snapshot.Length))
            );
        }
        catch { }
    }

    public event EventHandler<SnapshotSpanEventArgs>? TagsChanged;

    public IEnumerable<ITagSpan<IErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
    {
        if (spans.Count == 0 || _diagnostics.Count == 0)
            yield break;

        var snapshot = spans[0].Snapshot;

        foreach (var diag in _diagnostics)
        {
            var start = GetPosition(snapshot, diag.StartLine, diag.StartChar);
            var end = GetPosition(snapshot, diag.EndLine, diag.EndChar);

            if (start < 0 || end < 0 || start >= end || end > snapshot.Length)
                continue;

            var span = new SnapshotSpan(snapshot, start, end - start);
            if (!spans.IntersectsWith(span))
                continue;

            var errorType = diag.Severity switch
            {
                1 => PredefinedErrorTypeNames.SyntaxError,   // Error
                2 => PredefinedErrorTypeNames.Warning,       // Warning
                3 => PredefinedErrorTypeNames.HintedSuggestion, // Information
                4 => PredefinedErrorTypeNames.HintedSuggestion, // Hint
                _ => PredefinedErrorTypeNames.SyntaxError,
            };

            // Unreachable code (DiagnosticTag.Unnecessary = 1) gets a dimmed style.
            if (diag.Tags != null && diag.Tags.Contains(1))
                errorType = PredefinedErrorTypeNames.HintedSuggestion;

            yield return new TagSpan<IErrorTag>(span, new ErrorTag(errorType, diag.Message));
        }
    }

    private static int GetPosition(ITextSnapshot snapshot, int line, int character)
    {
        if (line < 0 || line >= snapshot.LineCount)
            return -1;
        var snapshotLine = snapshot.GetLineFromLineNumber(line);
        var pos = snapshotLine.Start.Position + character;
        return pos <= snapshotLine.EndIncludingLineBreak.Position ? pos : snapshotLine.End.Position;
    }

    public void Dispose()
    {
        _disposed = true;
        _debounceTimer?.Dispose();
        _changeDebounceTimer?.Dispose();
        _buffer.Changed -= OnBufferChanged;
        UitkxDiagnosticStore.DiagnosticsChanged -= OnDiagnosticsChanged;
    }
}

// ── Unreachable code dim tagger ─────────────────────────────────────────────

/// <summary>
/// Produces IClassificationTag spans with "excluded code" classification for
/// diagnostics tagged with DiagnosticTag.Unnecessary (unreachable code after
/// return/break/continue). This gives the faded/dimmed visual appearance.
/// </summary>
internal sealed class UitkxUnreachableCodeTagger : ITagger<IClassificationTag>, IDisposable
{
    private readonly ITextBuffer _buffer;
    private readonly IClassificationType _excludedCode;
    private List<LspDiagnostic> _diagnostics = new();
    private bool _disposed;
    private System.Threading.Timer? _debounceTimer;
    private List<LspDiagnostic>? _pendingDiagnostics;

    public UitkxUnreachableCodeTagger(ITextBuffer buffer, IClassificationType excludedCode)
    {
        _buffer = buffer;
        _excludedCode = excludedCode;
        UitkxDiagnosticStore.DiagnosticsChanged += OnDiagnosticsChanged;
    }

    /// <summary>Returns the current unreachable (Unnecessary-tagged) diagnostics for this buffer.</summary>
    internal IReadOnlyList<LspDiagnostic> UnreachableDiagnostics => _diagnostics;

    /// <summary>Gets the unreachable code tagger instance for a buffer, if one exists.</summary>
    internal static UitkxUnreachableCodeTagger? GetForBuffer(ITextBuffer buffer)
    {
        buffer.Properties.TryGetProperty(typeof(UitkxUnreachableCodeTagger), out UitkxUnreachableCodeTagger tagger);
        return tagger;
    }

    private void OnDiagnosticsChanged(string uri, List<LspDiagnostic> diagnostics)
    {
        if (_disposed) return;
        if (!_buffer.Properties.TryGetProperty(typeof(ITextDocument), out ITextDocument doc)) return;
        if (!string.Equals(new Uri(doc.FilePath).AbsoluteUri, uri, StringComparison.OrdinalIgnoreCase)) return;

        _pendingDiagnostics = diagnostics;
        if (_debounceTimer == null)
            _debounceTimer = new System.Threading.Timer(OnDebounceElapsed, null, 200, System.Threading.Timeout.Infinite);
        else
            _debounceTimer.Change(200, System.Threading.Timeout.Infinite);
    }

    private void OnDebounceElapsed(object? state)
    {
        if (_disposed) return;
        var pending = _pendingDiagnostics;
        if (pending == null) return;
        _diagnostics = pending;
        _pendingDiagnostics = null;
        try
        {
            var snapshot = _buffer.CurrentSnapshot;
            TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(snapshot, 0, snapshot.Length)));
        }
        catch { }
    }

    public event EventHandler<SnapshotSpanEventArgs>? TagsChanged;

    public IEnumerable<ITagSpan<IClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
    {
        if (spans.Count == 0 || _diagnostics.Count == 0)
            yield break;

        var snapshot = spans[0].Snapshot;

        foreach (var diag in _diagnostics)
        {
            // Only dim diagnostics tagged with Unnecessary (1).
            if (diag.Tags == null || !diag.Tags.Contains(1))
                continue;

            var start = GetPosition(snapshot, diag.StartLine, diag.StartChar);
            var end = GetPosition(snapshot, diag.EndLine, diag.EndChar);
            if (start < 0 || end < 0 || start >= end || end > snapshot.Length)
                continue;

            var span = new SnapshotSpan(snapshot, start, end - start);
            if (!spans.IntersectsWith(span))
                continue;

            yield return new TagSpan<IClassificationTag>(span, new ClassificationTag(_excludedCode));
        }
    }

    private static int GetPosition(ITextSnapshot snapshot, int line, int character)
    {
        if (line < 0 || line >= snapshot.LineCount) return -1;
        var snapshotLine = snapshot.GetLineFromLineNumber(line);
        var pos = snapshotLine.Start.Position + character;
        return pos <= snapshotLine.EndIncludingLineBreak.Position ? pos : snapshotLine.End.Position;
    }

    public void Dispose()
    {
        _disposed = true;
        _debounceTimer?.Dispose();
        UitkxDiagnosticStore.DiagnosticsChanged -= OnDiagnosticsChanged;
    }
}

// ── Simple diagnostic model ─────────────────────────────────────────────────

internal sealed class LspDiagnostic
{
    public int StartLine;
    public int StartChar;
    public int EndLine;
    public int EndChar;
    public int Severity; // 1=Error, 2=Warning, 3=Information, 4=Hint
    public string Message = "";
    public int[]? Tags;   // DiagnosticTag values (1=Unnecessary, 2=Deprecated)
}

// ── Static store for incoming publishDiagnostics ────────────────────────────

/// <summary>
/// Central store that receives textDocument/publishDiagnostics from the LSP
/// server and dispatches to per-buffer taggers.
/// </summary>
internal static class UitkxDiagnosticStore
{
    /// <summary>
    /// Fired when diagnostics for a URI are updated.
    /// Parameters: (string uri, List&lt;LspDiagnostic&gt; diagnostics)
    /// </summary>
    internal static event Action<string, List<LspDiagnostic>>? DiagnosticsChanged;

    private static readonly string LogPath = Path.Combine(
        Path.GetTempPath(),
        "uitkx-diag-store.log"
    );

    private static void Log(string msg)
    {
        try
        {
            File.AppendAllText(LogPath, $"[{DateTime.UtcNow:O}] {msg}\n");
        }
        catch { }
    }

    /// <summary>
    /// Called from UitkxMiddleLayer when a textDocument/publishDiagnostics
    /// notification arrives from the server.
    /// </summary>
    internal static void HandlePublishDiagnostics(JToken param)
    {
        try
        {
            var uri = param["uri"]?.ToString();
            if (string.IsNullOrEmpty(uri))
                return;

            var diagArray = param["diagnostics"] as JArray;
            var diagnostics = new List<LspDiagnostic>();

            if (diagArray != null)
            {
                foreach (var item in diagArray)
                {
                    var range = item["range"];
                    var start = range?["start"];
                    var end = range?["end"];

                    var diag = new LspDiagnostic
                    {
                        StartLine = start?["line"]?.Value<int>() ?? 0,
                        StartChar = start?["character"]?.Value<int>() ?? 0,
                        EndLine = end?["line"]?.Value<int>() ?? 0,
                        EndChar = end?["character"]?.Value<int>() ?? 0,
                        Severity = item["severity"]?.Value<int>() ?? 1,
                        Message = item["message"]?.ToString() ?? "",
                    };

                    // Parse diagnostic tags (Unnecessary=1, Deprecated=2).
                    var tagsArray = item["tags"] as JArray;
                    if (tagsArray != null && tagsArray.Count > 0)
                        diag.Tags = tagsArray.Select(t => t.Value<int>()).ToArray();

                    diagnostics.Add(diag);
                }
            }

            Log($"Parsed {diagnostics.Count} diagnostics for {uri}");
            foreach (var d in diagnostics)
                Log($"  sev={d.Severity} [{d.StartLine}:{d.StartChar}-{d.EndLine}:{d.EndChar}] tags={string.Join(",", d.Tags ?? Array.Empty<int>())} {d.Message}");
            DiagnosticsChanged?.Invoke(uri!, diagnostics);
        }
        catch (Exception ex)
        {
            Log($"Error parsing diagnostics: {ex.Message}");
        }
    }
}
