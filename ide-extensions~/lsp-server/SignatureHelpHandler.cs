using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using ReactiveUITK.Language.Parser;
using UitkxLanguageServer.Roslyn;

namespace UitkxLanguageServer;

/// <summary>
/// LSP <c>textDocument/signatureHelp</c> handler for .uitkx files.
///
/// <para>When the user types <c>(</c> or <c>,</c> inside a method call within a
/// C# expression region (setup code, <c>@(expr)</c>, or <c>attr={expr}</c>),
/// this handler resolves the method's overloads via Roslyn's
/// <see cref="SemanticModel"/> and returns <see cref="SignatureHelp"/> so the
/// IDE can display parameter hints.</para>
///
/// <para>The implementation walks the syntax tree to find the enclosing
/// <see cref="InvocationExpressionSyntax"/> or
/// <see cref="ObjectCreationExpressionSyntax"/>, counts commas in the argument
/// list to determine the active parameter index, and collects all overloads
/// from the semantic model.  No internal Roslyn APIs are used.</para>
/// </summary>
public sealed class SignatureHelpHandler : ISignatureHelpHandler
{
    private readonly DocumentStore _store;
    private readonly RoslynHost _roslynHost;

    public SignatureHelpHandler(DocumentStore store, RoslynHost roslynHost)
    {
        _store = store;
        _roslynHost = roslynHost;
    }

    public SignatureHelpRegistrationOptions GetRegistrationOptions(
        SignatureHelpCapability capability,
        ClientCapabilities clientCapabilities) =>
        new SignatureHelpRegistrationOptions
        {
            DocumentSelector = new TextDocumentSelector(
                new TextDocumentFilter { Pattern = "**/*.uitkx" }
            ),
            TriggerCharacters    = new Container<string>("(", ","),
            RetriggerCharacters  = new Container<string>(",", ")"),
        };

    public async Task<SignatureHelp?> Handle(
        SignatureHelpParams request,
        CancellationToken ct)
    {
        ServerLog.Log(
            $"signatureHelp: {request.TextDocument.Uri}  pos={request.Position.Line}:{request.Position.Character}");

        string localPath;
        try
        {
            localPath = new Uri(request.TextDocument.Uri.ToString()).LocalPath;
        }
        catch
        {
            return null;
        }

        if (string.IsNullOrEmpty(localPath))
            return null;

        // ── 1. Get source text ────────────────────────────────────────────────
        if (!_store.TryGet(request.TextDocument.Uri, out var text) || text is null)
        {
            if (File.Exists(localPath))
            {
                text = File.ReadAllText(localPath);
                _store.Set(request.TextDocument.Uri, text);
            }
            else
                return null;
        }

        // ── 2. Parse and ensure Roslyn workspace ready ────────────────────────
        var parseDiags = new List<ReactiveUITK.Language.ParseDiagnostic>();
        var directives = DirectiveParser.Parse(text, localPath, parseDiags);
        var nodes      = UitkxParser.Parse(text, localPath, directives, parseDiags);
        var parseResult = new ParseResult(
            directives,
            nodes,
            ImmutableArray.CreateRange(parseDiags));

        await _roslynHost.EnsureReadyAsync(localPath, text, parseResult, ct)
            .ConfigureAwait(false);

        // ── 3. Map cursor offset → virtual C# document ───────────────────────
        int uitkxOffset = ToOffset(text, request.Position);

        var virtualDoc = _roslynHost.GetVirtualDocument(localPath);
        if (virtualDoc == null)
            return null;

        var virtualResult = virtualDoc.Map.ToVirtualOffset(uitkxOffset);
        if (!virtualResult.HasValue)
            return null;   // cursor is not in a C# region

        int virtualOffset = virtualResult.Value.VirtualOffset;

        var roslynDoc = _roslynHost.GetRoslynDocument(localPath);
        if (roslynDoc == null)
            return null;

        // ── 4. Find the enclosing invocation in the syntax tree ───────────────
        var syntaxRoot = await roslynDoc.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (syntaxRoot == null)
            return null;

        var semanticModel = await roslynDoc.GetSemanticModelAsync(ct).ConfigureAwait(false);
        if (semanticModel == null)
            return null;

        // Walk up from the cursor position to find the innermost invocation
        // or object-creation that encloses the cursor.
        var nodeAtCursor = syntaxRoot.FindToken(virtualOffset > 0 ? virtualOffset - 1 : 0).Parent;

        // Find the enclosing argument list first, then its parent invocation.
        var argList = nodeAtCursor?
            .AncestorsAndSelf()
            .OfType<ArgumentListSyntax>()
            .FirstOrDefault();

        if (argList == null)
            return null;

        // Determine active parameter: number of commas before the cursor.
        int activeParam = argList.Arguments
            .Select((_, i) => i)
            .Count(i =>
            {
                var span = argList.Arguments[i].FullSpan;
                return span.End < virtualOffset;
            });

        // ── 5. Resolve the method group from the semantic model ───────────────
        IEnumerable<IMethodSymbol>? overloads = null;

        switch (argList.Parent)
        {
            case InvocationExpressionSyntax inv:
            {
                var info = semanticModel.GetSymbolInfo(inv.Expression, ct);
                overloads = GetMethodCandidates(info);
                break;
            }
            case ObjectCreationExpressionSyntax oc:
            {
                var info = semanticModel.GetSymbolInfo(oc, ct);
                overloads = GetConstructorCandidates(info);
                break;
            }
            case ImplicitObjectCreationExpressionSyntax ioc:
            {
                var info = semanticModel.GetSymbolInfo(ioc, ct);
                overloads = GetConstructorCandidates(info);
                break;
            }
            default:
                return null;
        }

        if (overloads == null)
            return null;

        var methodList = overloads.ToList();
        if (methodList.Count == 0)
            return null;

        // ── 6. Build LSP SignatureHelp ────────────────────────────────────────
        // Determine which overload best matches the argument count (for the
        // "active signature" hint — VS Code highlights the best match).
        int activeSignature = 0;
        for (int i = 0; i < methodList.Count; i++)
        {
            if (methodList[i].Parameters.Length >= activeParam + 1)
            {
                activeSignature = i;
                break;
            }
        }

        var signatures = new List<SignatureInformation>(methodList.Count);
        foreach (var method in methodList)
        {
            var paramInfos = method.Parameters
                .Select(p => new ParameterInformation
                {
                    Label   = p.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                    Documentation = string.IsNullOrEmpty(p.GetDocumentationCommentXml())
                        ? null
                        : new StringOrMarkupContent(
                            new MarkupContent
                            {
                                Kind  = MarkupKind.Markdown,
                                Value = p.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                            }),
                })
                .ToArray();

            var sigLabel = method.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            signatures.Add(new SignatureInformation
            {
                Label         = sigLabel,
                Parameters    = new Container<ParameterInformation>(paramInfos),
                Documentation = null,
            });
        }

        ServerLog.Log(
            $"signatureHelp: {signatures.Count} overloads, activeParam={activeParam}");

        return new SignatureHelp
        {
            Signatures      = new Container<SignatureInformation>(signatures),
            ActiveSignature = activeSignature,
            ActiveParameter = Math.Min(activeParam, methodList[activeSignature].Parameters.Length > 0
                ? methodList[activeSignature].Parameters.Length - 1
                : 0),
        };
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static IEnumerable<IMethodSymbol>? GetMethodCandidates(SymbolInfo info)
    {
        if (info.Symbol is IMethodSymbol single)
            return new[] { single };
        if (info.CandidateSymbols.Length > 0)
            return info.CandidateSymbols.OfType<IMethodSymbol>();
        return null;
    }

    private static IEnumerable<IMethodSymbol>? GetConstructorCandidates(SymbolInfo info)
    {
        if (info.Symbol is IMethodSymbol single)
            return new[] { single };
        if (info.CandidateSymbols.Length > 0)
            return info.CandidateSymbols.OfType<IMethodSymbol>();
        return null;
    }

    private static int ToOffset(string text, Position position)
    {
        var lines = text.Split('\n');
        int line = (int)position.Line;
        int result = 0;
        for (int i = 0; i < line && i < lines.Length; i++)
            result += lines[i].Length + 1; // +1 for the '\n'
        result += (int)position.Character;
        return Math.Min(result, text.Length);
    }
}
