using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ReactiveUITK.Language;
using ReactiveUITK.Language.Lowering;
using ReactiveUITK.Language.Nodes;
using ReactiveUITK.Language.Parser;
using UitkxLanguageServer.Roslyn;
using Xunit;

namespace UitkxLanguageServer.Tests;

// LSP-side parity for SG Stage 3d cross-namespace hook resolution (Issue #18).
// The host's EnrichWithPeerHookUsings must inject `using static <HookNs>.<X>Hooks;`
// into the virtual document for any peer hook file that belongs to the same
// asmdef as the consumer .uitkx, regardless of whether it shares the consumer's
// namespace. This mirrors the SG fix in UitkxPipeline Stage 3d.
[Collection("Roslyn")]
public sealed class HookCrossNamespaceVirtualDocTests : IAsyncLifetime, IDisposable
{
    private RoslynHost _host = null!;
    private string _tempDir = null!;

    public Task InitializeAsync()
    {
        _host = new RoslynHost(null!, new UitkxSchema(), new WorkspaceIndex());
        _host.SetWorkspaceRoot(null);
        _tempDir = Path.Combine(Path.GetTempPath(),
            "uitkx_lsp_xns_" + Guid.NewGuid().ToString("N").Substring(0, 8));
        Directory.CreateDirectory(_tempDir);
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    public void Dispose()
    {
        try { if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, recursive: true); }
        catch { /* best-effort cleanup */ }
    }

    [Fact]
    public async Task PeerHook_InDifferentNamespace_StillContributesUsingStatic()
    {
        // Consumer in MyApp.UI consumes a hook declared in MyApp.Hooks. Both
        // files sit in the same temp directory so FindPeerUitkxFiles' same-dir
        // scan picks the hook up without any WorkspaceIndex priming.
        var hookContent =
            "@namespace MyApp.Hooks\n\n"
            + "hook useFlag() -> bool {\n"
            + "  var (v, _) = useState(false);\n"
            + "  return v;\n"
            + "}\n";
        var consumerContent =
            "@namespace MyApp.UI\n\n"
            + "component ConsumerComp {\n"
            + "  return (\n"
            + "    <VisualElement />\n"
            + "  );\n"
            + "}\n";

        string hookPath = Path.Combine(_tempDir, "UseFlag.hooks.uitkx");
        string consumerPath = Path.Combine(_tempDir, "ConsumerComp.uitkx");
        File.WriteAllText(hookPath, hookContent);
        File.WriteAllText(consumerPath, consumerContent);

        var parseResult = Parse(consumerContent, consumerPath);
        await _host.EnsureReadyAsync(consumerPath, consumerContent, parseResult, CancellationToken.None);

        var vdoc = _host.GetVirtualDocument(consumerPath);
        Assert.NotNull(vdoc);
        // FQN must use the HOOK file's namespace, not the consumer's.
        Assert.Contains("using static MyApp.Hooks.UseFlagHooks;", vdoc!.Text);
    }

    private static ParseResult Parse(string source, string path)
    {
        var diags = new List<ParseDiagnostic>();
        var directives = DirectiveParser.Parse(source, path, diags);
        var parsedNodes = UitkxParser.Parse(source, path, directives, diags);
        var nodes = CanonicalLowering.LowerToRenderRoots(directives, parsedNodes, path);
        return new ParseResult(directives, nodes, ImmutableArray.CreateRange(diags));
    }
}
