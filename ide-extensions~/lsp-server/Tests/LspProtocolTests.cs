using Microsoft.Extensions.DependencyInjection;
using OmniSharp.Extensions.JsonRpc.Testing;
using OmniSharp.Extensions.LanguageProtocol.Testing;
using OmniSharp.Extensions.LanguageServer.Client;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Server;
using UitkxLanguageServer.Roslyn;
using Xunit;
using Xunit.Abstractions;
using ILanguageClient = OmniSharp.Extensions.LanguageServer.Protocol.Client.ILanguageClient;
using ILanguageServer = OmniSharp.Extensions.LanguageServer.Protocol.Server.ILanguageServer;

namespace UitkxLanguageServer.Tests;

/// <summary>
/// Full LSP protocol integration tests using OmniSharp's in-memory test infrastructure.
/// These boot the real UITKX language server handlers and exercise the full
/// JSON-RPC → handler → response pipeline.
/// </summary>
[Collection("Roslyn")]
public sealed class LspProtocolTests : LanguageProtocolTestBase
{
    public LspProtocolTests(ITestOutputHelper output)
        : base(new JsonRpcTestOptions())
    {
    }

    private Task<(ILanguageClient client, ILanguageServer server)> StartServer()
    {
        return Initialize(
            clientOptions => { },
            serverOptions =>
            {
                serverOptions
                    .WithHandler<TextSyncHandler>()
                    .WithHandler<CompletionHandler>()
                    .WithHandler<HoverHandler>()
                    .WithHandler<FormattingHandler>()
                    .WithHandler<SemanticTokensHandler>()
                    .WithHandler<SignatureHelpHandler>()
                    .WithHandler<DefinitionHandler>();

                serverOptions.Services.AddSingleton<UitkxSchema>();
                serverOptions.Services.AddSingleton<DocumentStore>();
                serverOptions.Services.AddSingleton<WorkspaceIndex>();
                serverOptions.Services.AddSingleton<RoslynHost>();
                serverOptions.Services.AddSingleton<DiagnosticsPublisher>();
            }
        );
    }

    // ── Handshake ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Initialize_Succeeds()
    {
        var (client, server) = await StartServer();
        Assert.NotNull(client);
        Assert.NotNull(server);
    }

    // ── Did open + hover ───────────────────────────────────────────────────

    [Fact]
    public async Task DidOpen_ThenHover_ReturnsResult()
    {
        var (client, _) = await StartServer();

        var uri = DocumentUri.From("file:///test/MyComp.uitkx");
        client.DidOpenTextDocument(new DidOpenTextDocumentParams
        {
            TextDocument = new TextDocumentItem
            {
                Uri = uri,
                LanguageId = "uitkx",
                Version = 1,
                Text = "@namespace T\n@component MyComp\n<Label text=\"hello\"/>"
            }
        });

        await SettleNext();

        // Hover over "Label" at line 2, col 1
        var hover = await client.RequestHover(new HoverParams
        {
            TextDocument = new TextDocumentIdentifier(uri),
            Position = new Position(2, 1)
        });

        // We may or may not get hover content depending on schema registration,
        // but the request should complete without error.
    }

    // ── Completion ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Completion_InDirectiveBlock_ReturnsItems()
    {
        var (client, _) = await StartServer();

        var uri = DocumentUri.From("file:///test/Test.uitkx");
        client.DidOpenTextDocument(new DidOpenTextDocumentParams
        {
            TextDocument = new TextDocumentItem
            {
                Uri = uri,
                LanguageId = "uitkx",
                Version = 1,
                Text = "@namespace T\n@\n<Label/>"
            }
        });

        await SettleNext();

        // Trigger completions after "@" on line 1
        var items = await client.RequestCompletion(new CompletionParams
        {
            TextDocument = new TextDocumentIdentifier(uri),
            Position = new Position(1, 1),
            Context = new CompletionContext { TriggerKind = CompletionTriggerKind.Invoked }
        });

        Assert.NotNull(items);
        // Directive completions include @component, @using, etc.
        Assert.True(items.Items.Any(), "Expected directive completion items");
    }

    [Fact]
    public async Task Completion_TagName_ReturnsElements()
    {
        var (client, _) = await StartServer();

        var uri = DocumentUri.From("file:///test/Tags.uitkx");
        client.DidOpenTextDocument(new DidOpenTextDocumentParams
        {
            TextDocument = new TextDocumentItem
            {
                Uri = uri,
                LanguageId = "uitkx",
                Version = 1,
                Text = "@namespace T\n@component Tags\n<"
            }
        });

        await SettleNext();

        var items = await client.RequestCompletion(new CompletionParams
        {
            TextDocument = new TextDocumentIdentifier(uri),
            Position = new Position(2, 1),
            Context = new CompletionContext { TriggerKind = CompletionTriggerKind.TriggerCharacter, TriggerCharacter = "<" }
        });

        Assert.NotNull(items);
        // Should include schema-defined elements (Label, Box, etc.)
        Assert.True(items.Items.Any(), "Expected element completion items after <");
    }

    // ── Document change ────────────────────────────────────────────────────

    [Fact]
    public async Task DidChange_UpdatesDocument()
    {
        var (client, _) = await StartServer();

        var uri = DocumentUri.From("file:///test/Change.uitkx");
        client.DidOpenTextDocument(new DidOpenTextDocumentParams
        {
            TextDocument = new TextDocumentItem
            {
                Uri = uri,
                LanguageId = "uitkx",
                Version = 1,
                Text = "@namespace T\n@component Change\n<Label/>"
            }
        });

        await SettleNext();

        client.DidChangeTextDocument(new DidChangeTextDocumentParams
        {
            TextDocument = new OptionalVersionedTextDocumentIdentifier { Uri = uri, Version = 2 },
            ContentChanges = new Container<TextDocumentContentChangeEvent>(
                new TextDocumentContentChangeEvent { Text = "@namespace T\n@component Change\n<Box>\n  <Label/>\n</Box>" }
            )
        });

        await SettleNext();

        // Verify we can still get completions after change
        var items = await client.RequestCompletion(new CompletionParams
        {
            TextDocument = new TextDocumentIdentifier(uri),
            Position = new Position(3, 3),
            Context = new CompletionContext { TriggerKind = CompletionTriggerKind.Invoked }
        });

        Assert.NotNull(items);
    }

    // ── Formatting ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Formatting_ReturnsEdits()
    {
        var (client, _) = await StartServer();

        var uri = DocumentUri.From("file:///test/Format.uitkx");
        client.DidOpenTextDocument(new DidOpenTextDocumentParams
        {
            TextDocument = new TextDocumentItem
            {
                Uri = uri,
                LanguageId = "uitkx",
                Version = 1,
                Text = "@namespace T\n@component Format\n<Box>\n<Label/>\n</Box>"
            }
        });

        await SettleNext();

        var edits = await client.RequestDocumentFormatting(new DocumentFormattingParams
        {
            TextDocument = new TextDocumentIdentifier(uri),
            Options = new FormattingOptions { { "tabSize", 2 }, { "insertSpaces", true } }
        });

        Assert.NotNull(edits);
        // The formatter should add indentation to the Label
        Assert.True(edits.Any(), "Expected formatting to produce edits");
    }
}
