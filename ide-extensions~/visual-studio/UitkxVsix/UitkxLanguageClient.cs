using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Threading;
using StreamJsonRpc;

namespace UitkxVsix;

/// <summary>LSP language client for .uitkx files.</summary>
[Export(typeof(ILanguageClient))]
[LanguageClientContentType("uitkx")]
public class UitkxLanguageClient : ILanguageClient, ILanguageClientCustomMessage2, IDisposable
{
    // Exposed for UitkxCompletionSource and UitkxHoverSource to call LSP directly.
    internal static volatile JsonRpc? InternalRpc;
    private static readonly string LogFilePath = Path.Combine(
        Path.GetTempPath(),
        "uitkx-vsix-lsp.log"
    );
    private static readonly string StaticLogPath = Path.Combine(
        Path.GetTempPath(),
        "uitkx-vsix-static.log"
    );
    private Process? _serverProcess;

    // Static constructor fires when .NET first loads this type (during MEF scan or activation).
    static UitkxLanguageClient()
    {
        try
        {
            File.AppendAllText(
                StaticLogPath,
                $"[{DateTime.UtcNow:O}] UitkxLanguageClient type loaded by CLR.{Environment.NewLine}"
            );
        }
        catch { }
    }

    public UitkxLanguageClient()
    {
        Log("UitkxLanguageClient constructor called (MEF instantiated).");
    }

    // ── ILanguageClient ──────────────────────────────────────────────────────

    public string Name => "UITKX Language Server";

    public IEnumerable<string>? ConfigurationSections => null;
    public object? InitializationOptions => null;
    public IEnumerable<string>? FilesToWatch => null;
    public bool ShowNotificationOnInitializeFailed => true;

    public event AsyncEventHandler<EventArgs>? StartAsync;
    public event AsyncEventHandler<EventArgs>? StopAsync;

    public async Task<Connection?> ActivateAsync(CancellationToken token)
    {
        foreach (var (fileName, arguments, description) in FindServerCommands())
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    WorkingDirectory = Path.GetDirectoryName(fileName) ?? AppContext.BaseDirectory,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                psi.Environment["DOTNET_ROLL_FORWARD"] = "LatestMajor";

                var stderr = new StringBuilder();
                var process = new Process { StartInfo = psi };
                process.ErrorDataReceived += (_, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                        stderr.AppendLine(e.Data);
                };

                process.Start();
                process.BeginErrorReadLine();

                await Task.Delay(750, token);

                if (process.HasExited)
                {
                    Log(
                        $"LSP launch attempt failed ({description}). ExitCode={process.ExitCode}. Command='{fileName} {arguments}'. Stderr='{stderr}'."
                    );
                    process.Dispose();
                    continue;
                }

                _serverProcess = process;
                Log($"LSP launch succeeded ({description}). Command='{fileName} {arguments}'.");

                return new Connection(
                    _serverProcess.StandardOutput.BaseStream,
                    _serverProcess.StandardInput.BaseStream
                );
            }
            catch (Exception ex)
            {
                Log(
                    $"LSP launch attempt threw ({description}). Command='{fileName} {arguments}'. Error='{ex}'."
                );
            }
        }

        Log("LSP activation failed: no launch strategy succeeded.");
        return null;
    }

    public async Task OnLoadedAsync()
    {
        Log("OnLoadedAsync called — raising StartAsync.");
        // Raise StartAsync to signal VS that this client is ready.
        // VS calls ActivateAsync() in response — without this, ActivateAsync is never invoked.
        await (StartAsync?.InvokeAsync(this, EventArgs.Empty) ?? Task.CompletedTask);
        Log("OnLoadedAsync done — StartAsync raised.");
    }

    public Task OnServerInitializedAsync()
    {
        Log("OnServerInitializedAsync — LSP initialize handshake complete.");
        return Task.CompletedTask;
    }

    // ── ILanguageClientCustomMessage2 ────────────────────────────────────────
    // Captures the live JsonRpc pipe so UitkxCompletionSource / UitkxHoverSource
    // can call textDocument/completion and textDocument/hover directly.

    object? ILanguageClientCustomMessage2.MiddleLayer => null;
    object? ILanguageClientCustomMessage2.CustomMessageTarget => null;

    Task ILanguageClientCustomMessage2.AttachForCustomMessageAsync(JsonRpc rpc)
    {
        InternalRpc = rpc;
        Log("AttachForCustomMessageAsync \u2014 JsonRpc pipe captured for direct LSP calls.");
        return Task.CompletedTask;
    }

    public Task<InitializationFailureContext?> OnServerInitializeFailedAsync(
        ILanguageClientInitializationInfo initializationState
    ) => Task.FromResult<InitializationFailureContext?>(null);

    // ── IDisposable ──────────────────────────────────────────────────────────

    public void Dispose()
    {
        try
        {
            if (_serverProcess is { HasExited: false })
                _serverProcess.Kill();
        }
        catch
        { /* best effort */
        }
        _serverProcess?.Dispose();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static IEnumerable<(
        string fileName,
        string arguments,
        string description
    )> FindServerCommands()
    {
        // Prefer a `server/` subdirectory beside this assembly (bundled in VSIX)
        var asm = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var serverDir = Path.Combine(asm, "server");

        // Try running the exe directly (no dotnet CLI needed, just .NET runtime)
        var exe = Path.Combine(serverDir, "UitkxLanguageServer.exe");
        if (File.Exists(exe))
            yield return (exe, string.Empty, "native-exe");

        // Fall back to dotnet dll (requires dotnet CLI in PATH)
        var dll = Path.Combine(serverDir, "UitkxLanguageServer.dll");
        if (File.Exists(dll))
            yield return ("dotnet", $"\"{dll}\"", "dotnet-dll");

        // Last resort: try server executable by relative path from extension root.
        if (File.Exists(Path.Combine("server", "UitkxLanguageServer.exe")))
            yield return (
                Path.Combine("server", "UitkxLanguageServer.exe"),
                string.Empty,
                "relative-exe"
            );
    }

    private static void Log(string message)
    {
        try
        {
            var line = $"[{DateTime.UtcNow:O}] {message}{Environment.NewLine}";
            File.AppendAllText(LogFilePath, line);
        }
        catch { }
    }
}
