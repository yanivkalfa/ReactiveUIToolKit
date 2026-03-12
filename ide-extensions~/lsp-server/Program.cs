using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Server;
using UitkxLanguageServer;
using UitkxLanguageServer.Roslyn;

// Redirect Console.Error to stderr (Console.Out is the LSP transport)
Console.InputEncoding = System.Text.Encoding.UTF8;
Console.OutputEncoding = System.Text.Encoding.UTF8;

ServerLog.Log(
    $"=== UitkxLanguageServer starting (PID={System.Diagnostics.Process.GetCurrentProcess().Id}) ==="
);

var server = await LanguageServer.From(options =>
    options
        .WithInput(Console.OpenStandardInput())
        .WithOutput(Console.OpenStandardOutput())
        .WithLoggerFactory(Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance)
        .WithHandler<TextSyncHandler>()
        .WithHandler<CompletionHandler>()
        .WithHandler<SignatureHelpHandler>()
        .WithHandler<HoverHandler>()
        .WithHandler<FormattingHandler>()
        .WithHandler<SemanticTokensHandler>()
        .WithHandler<DefinitionHandler>()
        .WithHandler<WatchedFilesHandler>()
        .WithServices(services =>
        {
            services.AddSingleton<UitkxSchema>();
            services.AddSingleton<DocumentStore>();
            services.AddSingleton<WorkspaceIndex>();
            services.AddSingleton<RoslynHost>();
            services.AddSingleton<DiagnosticsPublisher>();
            services.AddSingleton<OmniSharp.Extensions.LanguageServer.Protocol.Server.IOnLanguageServerStarted>(
                new StartupLogger()
            );
            services.AddSingleton<OmniSharp.Extensions.LanguageServer.Protocol.Server.IOnLanguageServerStarted>(
                sp => sp.GetRequiredService<WorkspaceIndex>()
            );
            // Notify RoslynHost of the workspace root once the server has completed
            // the Initialize handshake (rootUri / rootPath is available at that point).
            services.AddSingleton<OmniSharp.Extensions.LanguageServer.Protocol.Server.IOnLanguageServerStarted>(
                sp =>
                {
                    var roslynHost = sp.GetRequiredService<RoslynHost>();
                    return new RoslynHostStartup(roslynHost);
                });
        })
);

ServerLog.Log("=== LanguageServer.From completed — waiting for exit ===");
await server.WaitForExit;
