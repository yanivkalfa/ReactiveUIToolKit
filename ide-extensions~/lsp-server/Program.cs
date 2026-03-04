using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Server;
using UitkxLanguageServer;

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
        .WithHandler<HoverHandler>()
        .WithHandler<FormattingHandler>()
        .WithServices(services =>
        {
            services.AddSingleton<UitkxSchema>();
            services.AddSingleton<DocumentStore>();
            services.AddSingleton<OmniSharp.Extensions.LanguageServer.Protocol.Server.IOnLanguageServerStarted>(
                new StartupLogger()
            );
        })
);

ServerLog.Log("=== LanguageServer.From completed — waiting for exit ===");
await server.WaitForExit;
