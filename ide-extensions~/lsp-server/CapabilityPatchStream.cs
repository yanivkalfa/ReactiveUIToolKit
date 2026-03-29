using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UitkxLanguageServer;

/// <summary>
/// Wraps the LSP stdout stream and patches the first InitializeResult message
/// to replace null capability providers with non-null values.
///
/// OmniSharp v0.19.9 uses dynamic registration when the client advertises
/// <c>dynamicRegistration: true</c>. This causes all providers in the static
/// <c>InitializeResult.capabilities</c> to be <c>null</c>. VS2022's built-in
/// LSP navigation handlers rely on static capabilities, so they see
/// <c>DefinitionProvider: null</c> and suppress Ctrl+Click / F12.
///
/// This stream intercepts the raw JSON-RPC output. When it detects the
/// <c>initialize</c> response (the one containing <c>"capabilities"</c>),
/// it patches null providers to <c>true</c> or <c>{}</c> so VS2022's handlers
/// recognize them.
/// </summary>
internal sealed class CapabilityPatchStream : Stream
{
    /// <summary>
    /// When true, patches are applied (capability injection + semantic token stripping).
    /// Only VS2022 needs these patches. VSCode handles dynamic registration correctly.
    /// Set from Program.cs based on --vs2022 command-line flag.
    /// </summary>
    internal static bool IsVisualStudio;

    private readonly Stream _inner;
    private bool _patched;
    private int _writeCount;

    public CapabilityPatchStream(Stream inner) => _inner = inner;

    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length => _inner.Length;
    public override long Position
    {
        get => _inner.Position;
        set => _inner.Position = value;
    }

    public override void Flush() => _inner.Flush();
    public override Task FlushAsync(CancellationToken cancellationToken) =>
        _inner.FlushAsync(cancellationToken);
    public override int Read(byte[] buffer, int offset, int count) =>
        throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) =>
        throw new NotSupportedException();
    public override void SetLength(long value) => _inner.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count)
    {
        var patched = TryPatch(buffer, offset, count);
        if (patched != null)
            _inner.Write(patched, 0, patched.Length);
        else
            _inner.Write(buffer, offset, count);
    }

    public override Task WriteAsync(byte[] buffer, int offset, int count,
        CancellationToken cancellationToken)
    {
        var patched = TryPatch(buffer, offset, count);
        if (patched != null)
            return _inner.WriteAsync(patched, 0, patched.Length, cancellationToken);
        return _inner.WriteAsync(buffer, offset, count, cancellationToken);
    }

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer,
        CancellationToken cancellationToken = default)
    {
        // Convert to array for inspection
        var arr = buffer.ToArray();
        var patched = TryPatch(arr, 0, arr.Length);
        if (patched != null)
            return _inner.WriteAsync(patched.AsMemory(), cancellationToken);
        return _inner.WriteAsync(buffer, cancellationToken);
    }

    /// <summary>
    /// If the buffer contains the InitializeResult, patches it and returns
    /// the new byte array. Returns null if no patching needed.
    /// </summary>
    private byte[]? TryPatch(byte[] buffer, int offset, int count)
    {
        _writeCount++;

        // Only apply patches when the client is VS2022.
        // VSCode handles dynamic registration correctly and needs semantic tokens.
        if (!IsVisualStudio)
            return null;

        var text = Encoding.UTF8.GetString(buffer, offset, count);

        // Log first few writes to help debug
        if (_writeCount <= 5)
        {
            var preview = text.Length > 200 ? text.Substring(0, 200) : text;
            ServerLog.Log($"[CapabilityPatchStream] Write #{_writeCount} ({count} bytes): {preview.Replace("\r", "").Replace("\n", " ")}");
        }

        // Patch #1: Inject static capabilities into InitializeResult
        if (!_patched
            && text.IndexOf("\"capabilities\"", StringComparison.OrdinalIgnoreCase) >= 0
            && text.IndexOf("\"result\"", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            var patched = InjectCapabilities(text);
            if (patched != text)
            {
                _patched = true;
                var result = Encoding.UTF8.GetBytes(RewriteContentLength(patched));
                ServerLog.Log("[CapabilityPatchStream] Patched InitializeResult capabilities.");
                return result;
            }
        }

        // Patch #2: Strip textDocument/semanticTokens from client/registerCapability.
        // OmniSharp dynamically registers semanticTokens, which causes VS2022 to
        // override our IClassifier with LSP-based coloring. Our custom classifier
        // provides richer coloring, so we suppress the dynamic registration.
        if (text.IndexOf("client/registerCapability", StringComparison.Ordinal) >= 0
            && text.IndexOf("semanticTokens", StringComparison.Ordinal) >= 0)
        {
            var stripped = StripSemanticTokensRegistration(text);
            if (stripped != text)
            {
                var result = Encoding.UTF8.GetBytes(RewriteContentLength(stripped));
                ServerLog.Log("[CapabilityPatchStream] Stripped semanticTokens from client/registerCapability.");
                return result;
            }
        }

        return null;
    }

    /// <summary>
    /// Injects capability providers into the InitializeResult.
    /// OmniSharp sends a minimal capabilities object (just experimental + workspace)
    /// and registers providers via client/registerCapability instead.
    /// VS2022 ignores dynamic registration, so we inject providers directly.
    /// </summary>
    private static string InjectCapabilities(string text)
    {
        // Find "experimental":{} and inject providers before it
        const string providers =
            "\"definitionProvider\":true,"
            + "\"hoverProvider\":true,"
            + "\"completionProvider\":{\"triggerCharacters\":[\"<\",\" \",\".\",\"\\\"\",\"=\",\"/\"],\"resolveProvider\":false},"
            + "\"signatureHelpProvider\":{\"triggerCharacters\":[\"(\",\",\"]},"
            + "\"documentFormattingProvider\":true,"
            + "\"renameProvider\":{\"prepareProvider\":true},"
            + "\"referencesProvider\":true,"
            + "\"textDocumentSync\":{\"openClose\":true,\"change\":2},";

        // Insert providers right after "capabilities":{
        var idx = text.IndexOf("\"capabilities\"", StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return text;

        // Find the opening brace after "capabilities":
        var braceIdx = text.IndexOf('{', idx + "\"capabilities\"".Length);
        if (braceIdx < 0) return text;

        return text.Substring(0, braceIdx + 1) + providers + text.Substring(braceIdx + 1);
    }

    /// <summary>
    /// Recomputes <c>Content-Length</c> in the LSP header to match the patched body.
    /// LSP messages are: <c>Content-Length: N\r\n\r\n{json}</c>
    /// </summary>
    private static string RewriteContentLength(string message)
    {
        var headerEnd = message.IndexOf("\r\n\r\n", StringComparison.Ordinal);
        if (headerEnd < 0) return message;

        var body = message.Substring(headerEnd + 4);
        var bodyBytes = Encoding.UTF8.GetByteCount(body);
        return $"Content-Length: {bodyBytes}\r\n\r\n{body}";
    }

    /// <summary>
    /// Removes any registration entry whose <c>method</c> contains "semanticTokens"
    /// from a <c>client/registerCapability</c> JSON-RPC message.
    /// Uses simple string manipulation to avoid adding a JSON library dependency.
    /// </summary>
    private static string StripSemanticTokensRegistration(string text)
    {
        // The registrations array is: "registrations":[{...},{...},...]
        // Each entry is {"id":"...","method":"textDocument/semanticTokens/full","registerOptions":{...}}
        // We need to find and remove any entry containing "semanticTokens".

        // Locate the body (after headers)
        var headerEnd = text.IndexOf("\r\n\r\n", StringComparison.Ordinal);
        if (headerEnd < 0) return text;
        var body = text.Substring(headerEnd + 4);

        // Find the registrations array
        var regIdx = body.IndexOf("\"registrations\"", StringComparison.Ordinal);
        if (regIdx < 0) return text;

        var arrStart = body.IndexOf('[', regIdx);
        if (arrStart < 0) return text;

        // Walk the array and collect non-semanticTokens entries
        var entries = new System.Collections.Generic.List<string>();
        var pos = arrStart + 1;
        var removed = false;

        while (pos < body.Length)
        {
            // Skip whitespace/commas
            while (pos < body.Length && (body[pos] == ',' || body[pos] == ' ' || body[pos] == '\n' || body[pos] == '\r'))
                pos++;

            if (pos >= body.Length || body[pos] == ']')
                break;

            if (body[pos] != '{')
                break;

            // Find matching closing brace
            var depth = 0;
            var entryStart = pos;
            var inString = false;
            var escape = false;
            while (pos < body.Length)
            {
                var c = body[pos];
                if (escape) { escape = false; pos++; continue; }
                if (c == '\\' && inString) { escape = true; pos++; continue; }
                if (c == '"') { inString = !inString; pos++; continue; }
                if (!inString)
                {
                    if (c == '{') depth++;
                    else if (c == '}') { depth--; if (depth == 0) { pos++; break; } }
                }
                pos++;
            }

            var entry = body.Substring(entryStart, pos - entryStart);
            if (entry.IndexOf("semanticTokens", StringComparison.Ordinal) >= 0)
            {
                removed = true;
                ServerLog.Log($"[CapabilityPatchStream] Removed registration: {entry.Substring(0, Math.Min(100, entry.Length))}");
            }
            else
            {
                entries.Add(entry);
            }
        }

        if (!removed) return text;

        // Rebuild the body with filtered registrations
        var prefix = body.Substring(0, arrStart + 1);
        var arrEnd = body.IndexOf(']', pos - 1);
        if (arrEnd < 0) arrEnd = body.Length - 1;
        var suffix = body.Substring(arrEnd);

        var newBody = prefix + string.Join(",", entries) + suffix;
        return text.Substring(0, headerEnd + 4) + newBody;
    }
}
