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
using Microsoft.VisualStudio.Utilities;

namespace UitkxVsix;

/// <summary>
/// LSP language client for .uitkx files.
/// NOT currently MEF-exported: VS 2022/2026's LanguageClientFactory has broken
/// MEF dependencies (ExperimentationService, sessionManager) that cause the
/// entire extension DLL to be rejected when [Export(typeof(ILanguageClient))]
/// is present, which also breaks content-type registration and colorization.
///
/// TODO: re-enable LSP via a proper AsyncPackage + [ProvideLanguageClient]
/// registration once VS stabilises the LanguageClientFactory composition.
/// </summary>
[Export(typeof(ILanguageClient))]
[Name("UITKX Language Server")]
[ContentType("uitkx")]
[RunOnContext(RunningContext.RunOnHost)]
public sealed class UitkxLanguageClient : ILanguageClient, IDisposable
{
    private static readonly string LogFilePath = Path.Combine(Path.GetTempPath(), "uitkx-vsix-lsp.log");
    private Process? _serverProcess;

    // ── ILanguageClient ──────────────────────────────────────────────────────

    public string Name => "UITKX Language Server";

    public IEnumerable<string>? ConfigurationSections => null;
    public object? InitializationOptions => null;
    public IEnumerable<string>? FilesToWatch => null;
    public bool ShowNotificationOnInitializeFailed => true;

#pragma warning disable CS0067  // Visual Studio calls these via reflection
    public event AsyncEventHandler<EventArgs>? StartAsync;
    public event AsyncEventHandler<EventArgs>? StopAsync;
#pragma warning restore CS0067

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
                    Log($"LSP launch attempt failed ({description}). ExitCode={process.ExitCode}. Command='{fileName} {arguments}'. Stderr='{stderr}'.");
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
                Log($"LSP launch attempt threw ({description}). Command='{fileName} {arguments}'. Error='{ex}'.");
            }
        }

        Log("LSP activation failed: no launch strategy succeeded.");
        return null;
    }

    public Task OnLoadedAsync() => Task.CompletedTask;

    public Task OnServerInitializedAsync() => Task.CompletedTask;

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

    private static IEnumerable<(string fileName, string arguments, string description)> FindServerCommands()
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
            yield return (Path.Combine("server", "UitkxLanguageServer.exe"), string.Empty, "relative-exe");
    }

    private static void Log(string message)
    {
        try
        {
            var line = $"[{DateTime.UtcNow:O}] {message}{Environment.NewLine}";
            File.AppendAllText(LogFilePath, line);
        }
        catch
        {
        }
    }
}
