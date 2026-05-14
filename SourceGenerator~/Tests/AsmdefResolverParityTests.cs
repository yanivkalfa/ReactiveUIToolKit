using System.IO;
using System.Reflection;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests;

// Parity check: the SG's UitkxPipeline.IsOwnedByCompilation /
// FindOwningAsmdefAssemblyName logic is mirrored verbatim by the editor-side
// AsmdefResolver (Editor/HMR/AsmdefResolver.cs) and the LSP-side
// AsmdefResolver (ide-extensions~/lsp-server/AsmdefResolver.cs). Keeping the
// three in sync is critical: a divergence means an asmdef boundary recognised
// by SG but not by HMR (or LSP) silently changes the using-static set between
// build / hot-reload / IDE squiggle, which is exactly the class of bug
// Issue #18 fixed.
//
// We can only call SG internals from this test project; the HMR/LSP copies
// live in projects with Unity / OmniSharp dependencies that this project
// can't reference. Instead we verify the SG fallback behaves as documented,
// then rely on code review + the file-level comments in each AsmdefResolver
// to keep the three in lockstep. The shared algorithm has only two branches
// (no-asmdef + Editor segment, no-asmdef + non-Editor segment), so the test
// surface here is small but pinned.
public class AsmdefResolverParityTests
{
    [Fact]
    public void NoAsmdef_NonEditorPath_ResolvesToAssemblyCSharp()
    {
        // Path is far away from any real .asmdef; SG's walk terminates without
        // a match and falls back to the convention.
        string path = Path.Combine(Path.GetTempPath(), "uitkx_asmdef_parity",
            "Runtime", "Foo.uitkx");
        Assert.True(InvokeIsOwnedByCompilation(path, "Assembly-CSharp"));
        Assert.False(InvokeIsOwnedByCompilation(path, "Assembly-CSharp-Editor"));
        Assert.False(InvokeIsOwnedByCompilation(path, "MyCompany.Runtime"));
    }

    [Fact]
    public void NoAsmdef_EditorPath_ResolvesToAssemblyCSharpEditor()
    {
        string path = Path.Combine(Path.GetTempPath(), "uitkx_asmdef_parity",
            "Editor", "Foo.uitkx");
        Assert.True(InvokeIsOwnedByCompilation(path, "Assembly-CSharp-Editor"));
        Assert.False(InvokeIsOwnedByCompilation(path, "Assembly-CSharp"));
    }

    [Fact]
    public void Asmdef_OwnsFile_WhenNameFieldMatches()
    {
        string root = Path.Combine(Path.GetTempPath(), "uitkx_asmdef_parity_match");
        Directory.CreateDirectory(root);
        string asmdef = Path.Combine(root, "MyAsm.asmdef");
        File.WriteAllText(asmdef, "{ \"name\": \"MyAsm\" }");

        string filePath = Path.Combine(root, "Sub", "Foo.uitkx");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        Assert.True(InvokeIsOwnedByCompilation(filePath, "MyAsm"));
        Assert.False(InvokeIsOwnedByCompilation(filePath, "Assembly-CSharp"));
    }

    private static bool InvokeIsOwnedByCompilation(string filePath, string asmName)
    {
        var t = typeof(UitkxPipeline);
        var mi = t.GetMethod(
            "IsOwnedByCompilation",
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        Assert.NotNull(mi);
        return (bool)mi!.Invoke(null, new object?[] { filePath, asmName })!;
    }
}
