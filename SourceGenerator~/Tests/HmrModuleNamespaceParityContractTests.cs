using System.IO;
using System.Runtime.CompilerServices;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests;

/// <summary>
/// Source-text contract pins for HMR's effective-namespace threading
/// (<c>Editor/HMR/</c> lives in the Unity Editor asmdef and cannot be loaded by
/// this test runner — same approach as <see cref="HmrBuiltinTagDiscoveryContractTests"/>).
///
/// <para>The invariant: every HMR emit/threading site must use the EFFECTIVE
/// namespace, never the raw parsed <c>DirectiveSet.Namespace</c>. Raw is a parser
/// default for stamp-less (path-derived) files; using it anywhere splits the hot
/// unit from the real build's namespaces, which breaks (a) companion-module
/// partial-class merging (bare style refs → CS0103), (b) the module static
/// swapper's <c>Type.FullName</c> hot↔project match (silent no-swap on .style
/// edits), and (c) the batch path's <c>FullyQualifiedName</c> trampoline lookup.
/// Found live: creating <c>X.style.uitkx</c> mid-HMR-session and referencing
/// <c>container</c> failed CS0103 while the full build was clean.</para>
/// </summary>
public class HmrModuleNamespaceParityContractTests
{
    private static string WorkspaceRoot([CallerFilePath] string thisFile = "")
    {
        var dir = Path.GetDirectoryName(thisFile)!; // Tests/
        return Path.GetFullPath(Path.Combine(dir, "../.."));
    }

    private static string HookEmitterSource() =>
        File.ReadAllText(Path.Combine(WorkspaceRoot(), "Editor", "HMR", "HmrHookEmitter.cs"));

    private static string CompilerSource() =>
        File.ReadAllText(Path.Combine(WorkspaceRoot(), "Editor", "HMR", "UitkxHmrCompiler.cs"));

    [Fact]
    public void EmitModules_AcceptsEffectiveNamespace_AndPrefersIt()
    {
        var src = HookEmitterSource();
        Assert.Contains(
            "public static string EmitModules(object directives, string filePath, string effectiveNs = null)",
            src);
        // The namespace line must prefer the effective value (same rule as Emit()).
        Assert.Contains(
            "effectiveNs ?? (string)GetProp(directives, \"Namespace\")",
            src);
    }

    [Fact]
    public void Compiler_NeverCallsEmitModules_WithoutEffectiveNamespace()
    {
        var src = CompilerSource();
        // The two-argument (raw-namespace) call shapes must not reappear.
        Assert.DoesNotContain("EmitModules(directives, uitkxPath);", src);
        Assert.DoesNotContain("EmitModules(companionDir, file);", src);
        // Both live call sites thread ComputeEffectiveNs (directly or via a local).
        Assert.Contains("EmitModules(\n                    directives, uitkxPath, ComputeEffectiveNs(directives, uitkxPath))",
            src.Replace("\r\n", "\n"));
        Assert.Contains("EmitModules(companionDir, file, companionNs)", src);
    }

    [Fact]
    public void Compiler_NamespaceThreading_UsesEffective_NotRawParse()
    {
        var src = CompilerSource();
        // The raw assignment shape (`string ns = (string)GetProp(directives, "Namespace");`)
        // must not exist anywhere — result.Namespace, artifacts.Namespace, and
        // FullyQualifiedName all derive from ComputeEffectiveNs now. (The only
        // permitted raw read is INSIDE ComputeEffectiveNs itself, which assigns
        // to `rawNs` — a different text shape.)
        Assert.DoesNotContain("string ns = (string)GetProp(directives, \"Namespace\")", src);
    }
}
