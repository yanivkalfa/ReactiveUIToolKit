using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using ReactiveUITK.Language;
using ReactiveUITK.Language.Diagnostics;
using ReactiveUITK.Language.Lowering;
using ReactiveUITK.Language.Parser;
using LspDiagnosticSeverity = OmniSharp.Extensions.LanguageServer.Protocol.Models.DiagnosticSeverity;
using LspRange = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace UitkxLanguageServer;

/// <summary>
/// Parses a .uitkx document, runs all diagnostic tiers, and pushes the results
/// to the LSP client as a <c>textDocument/publishDiagnostics</c> notification.
///
/// Trigger points (called by <see cref="TextSyncHandler"/>):
///   <c>textDocument/didOpen</c>, <c>textDocument/didChange</c>
///
/// Tier 1 (parser syntax)   — comes from <see cref="ParseResult.Diagnostics"/>
/// Tier 2 (structural)      — produced by <see cref="DiagnosticsAnalyzer"/>
/// </summary>
public sealed class DiagnosticsPublisher
{
    private readonly ILanguageServerFacade _server;
    private readonly DiagnosticsAnalyzer _analyzer = new DiagnosticsAnalyzer();

    public DiagnosticsPublisher(ILanguageServerFacade server)
    {
        _server = server;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Parse the document and push fresh diagnostics to the client.
    /// </summary>
    /// <param name="uri">The document URI (identifies the file to the client).</param>
    /// <param name="text">Current document text.</param>
    public void Publish(DocumentUri uri, string text)
    {
        string localPath = GetLocalPath(uri) ?? string.Empty;

        // ── Parse ────────────────────────────────────────────────────────────
        var parseDiags = new List<ParseDiagnostic>();
        var directives = DirectiveParser.Parse(text, localPath, parseDiags);
        var parsedNodes = UitkxParser.Parse(text, localPath, directives, parseDiags);
        var nodes = CanonicalLowering.LowerToRenderRoots(directives, parsedNodes, localPath);

        var parseResult = new ParseResult(
            directives,
            nodes,
            System.Collections.Immutable.ImmutableArray.CreateRange(parseDiags)
        );

        // ── T2 structural analysis ───────────────────────────────────────────
        var t2Diags = _analyzer.Analyze(parseResult, localPath);

        // ── Combine T1 + T2 ─────────────────────────────────────────────────
        var allDiags = parseResult.Diagnostics.Concat(t2Diags).Select(d => ToLsp(d)).ToArray();

        // ── Publish notification ─────────────────────────────────────────────
        _server.TextDocument.PublishDiagnostics(
            new PublishDiagnosticsParams
            {
                Uri = uri,
                Diagnostics = new Container<Diagnostic>(allDiags),
            }
        );
    }

    // ── Conversion helpers ────────────────────────────────────────────────────

    private static Diagnostic ToLsp(ParseDiagnostic d)
    {
        // SourceLine is 1-based; LSP is 0-based.
        int startLine = Math.Max(0, d.SourceLine - 1);
        int startChar = Math.Max(0, d.SourceColumn);

        // EndLine/EndColumn are 0 when not tracked → fall back to same position.
        int endLine = d.EndLine > 0 ? Math.Max(0, d.EndLine - 1) : startLine;
        int endChar =
            d.EndColumn > 0 ? d.EndColumn
            : startChar > 0 ? startChar // same column
            : 1; // minimal non-zero range

        return new Diagnostic
        {
            Range = new LspRange(
                new Position(startLine, startChar),
                new Position(endLine, endChar)
            ),
            Severity = ToLspSeverity(d.Severity),
            Code = (DiagnosticCode)d.Code,
            Source = "uitkx",
            Message = d.Message,
            Tags =
                d.Code == DiagnosticCodes.UnreachableAfterReturn
                    ? new Container<DiagnosticTag>(DiagnosticTag.Unnecessary)
                    : null,
        };
    }

    private static LspDiagnosticSeverity ToLspSeverity(ParseSeverity s) =>
        s switch
        {
            ParseSeverity.Error => LspDiagnosticSeverity.Error,
            ParseSeverity.Warning => LspDiagnosticSeverity.Warning,
            ParseSeverity.Information => LspDiagnosticSeverity.Information,
            ParseSeverity.Hint => LspDiagnosticSeverity.Hint,
            _ => LspDiagnosticSeverity.Information,
        };

    private static string? GetLocalPath(DocumentUri uri)
    {
        try
        {
            var sysUri = new Uri(uri.ToString());
            return sysUri.IsFile ? sysUri.LocalPath : null;
        }
        catch
        {
            return null;
        }
    }
}
