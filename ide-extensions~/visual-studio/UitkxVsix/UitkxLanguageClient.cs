using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Reflection;
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
// [Export(typeof(ILanguageClient))]
// [ContentType("uitkx")]
// [RunOnContext(RunningContext.RunOnHost)]
public sealed class UitkxLanguageClient : ILanguageClient, IDisposable
{
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
        var (fileName, arguments) = FindServerCommand();
        if (fileName is null)
            return null;

        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        _serverProcess = new Process { StartInfo = psi };

        // Suppress stderr so it doesn't pop up in VS output windows
        _serverProcess.ErrorDataReceived += (_, _) => { };

        _serverProcess.Start();
        _serverProcess.BeginErrorReadLine();

        await Task.Yield();

        return new Connection(
            _serverProcess.StandardOutput.BaseStream,
            _serverProcess.StandardInput.BaseStream
        );
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

    private static (string? fileName, string arguments) FindServerCommand()
    {
        // Prefer a `server/` subdirectory beside this assembly (bundled in VSIX)
        var asm = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var serverDir = Path.Combine(asm, "server");

        // Try running the exe directly (no dotnet CLI needed, just .NET runtime)
        var exe = Path.Combine(serverDir, "UitkxLanguageServer.exe");
        if (File.Exists(exe))
            return (exe, string.Empty);

        // Fall back to dotnet dll (requires dotnet CLI in PATH)
        var dll = Path.Combine(serverDir, "UitkxLanguageServer.dll");
        if (File.Exists(dll))
            return ("dotnet", $"\"{dll}\"");

        return (null, string.Empty);
    }
}
