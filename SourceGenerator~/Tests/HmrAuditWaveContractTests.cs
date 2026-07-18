using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests;

/// <summary>
/// ES-modules audit wave (Plans~/ES_MODULES_AUDIT_FINDINGS.md, HMR cluster H1/H3/H4/M1/M2/L3):
/// source-text contract pins for the HMR-side fixes (<c>Editor/HMR/</c> cannot be loaded by this
/// runner — same approach as <see cref="HmrExportsParityContractTests"/>), plus a syntax gate
/// that PARSES every touched Editor/HMR source so a broken edit is caught here instead of at the
/// next Unity domain reload.
/// </summary>
public class HmrAuditWaveContractTests
{
    private static string WorkspaceRoot([CallerFilePath] string thisFile = "")
    {
        var dir = Path.GetDirectoryName(thisFile)!;
        return Path.GetFullPath(Path.Combine(dir, "../.."));
    }

    private static string Src(string file) =>
        File.ReadAllText(Path.Combine(WorkspaceRoot(), "Editor", "HMR", file));

    private static string LangSrc(string file) =>
        File.ReadAllText(Path.Combine(WorkspaceRoot(), "ide-extensions~", "language-lib", file));

    [Theory]
    [InlineData("UitkxHmrCompiler.cs")]
    [InlineData("HmrHookEmitter.cs")]
    [InlineData("HmrCSharpEmitter.cs")]
    [InlineData("UitkxHmrController.cs")]
    public void EditorHmrSources_ParseClean(string file)
    {
        var tree = CSharpSyntaxTree.ParseText(
            Src(file), new CSharpParseOptions(LanguageVersion.CSharp9));
        foreach (var d in tree.GetDiagnostics())
            Assert.True(d.Severity != DiagnosticSeverity.Error, $"{file}: {d}");
    }

    [Fact]
    public void H4_MemberRoute_InjectsImportPayloads()
    {
        var src = Src("UitkxHmrCompiler.cs");
        Assert.Contains("exportsCSharp = InjectUsings(", src);
    }

    [Fact]
    public void H1M2_ComponentPath_InlinesOwnExportsHotUnit()
    {
        var src = Src("UitkxHmrCompiler.cs");
        Assert.Contains("sources.Add(ownExports);", src);
    }

    [Fact]
    public void H1H4_Bridges_FlowFromSharedHelper_IntoEmitExports()
    {
        Assert.Contains("ComputeImportedMemberBridgeLines", Src("UitkxHmrCompiler.cs"));
        Assert.Contains("bridgeLines: ComputeBridgeLines(directives, uitkxPath)", Src("UitkxHmrCompiler.cs"));
        Assert.Contains("IReadOnlyList<string> bridgeLines = null", Src("HmrHookEmitter.cs"));
    }

    [Fact]
    public void H1H4_BridgeLineShape_MatchesSgExportsEmitter()
    {
        // The SG renders bridges from its peer tables; the shared helper renders them from disk
        // parses — the emitted line SHAPE must be byte-identical (value + method forms).
        var sg = File.ReadAllText(Path.Combine(
            WorkspaceRoot(), "SourceGenerator~", "Emitter", "ExportsEmitter.cs"));
        var shared = LangSrc("ImportScopeFacts.cs");
        Assert.Contains("internal static {type} {alias} => global::{targetNs}.__Exports.{target.Name};", sg);
        Assert.Contains("internal static {type} {alias} => global::{tns}.__Exports.{m.Name};", shared);
        Assert.Contains("internal static {ret} {alias}({paramList}) => global::{targetNs}.__Exports.{target.Name}({argNames});", sg);
        Assert.Contains("internal static {ret} {alias}({pl}) => global::{tns}.__Exports.{m.Name}({an});", shared);
    }

    [Fact]
    public void H3_HotEmitter_ResolvesDottedAndBoundTags()
    {
        var src = Src("HmrCSharpEmitter.cs");
        Assert.Contains("StarImportNamespaces.TryGetValue(lookupName.Substring(0, tagDot)", src);
        Assert.Contains("ImportAliasTypeMap.TryGetValue(lookupName", src);
        Assert.Contains("ComputeStarImportNamespaces", Src("UitkxHmrCompiler.cs"));
        Assert.Contains("ComputeImportAliasTypeMap", Src("UitkxHmrCompiler.cs"));
    }

    [Fact]
    public void H3_PropsTypeScan_HandlesQualifiedHeads()
    {
        var src = Src("HmrCSharpEmitter.cs");
        Assert.Contains("if (type.Name != simpleName)", src);
        Assert.Contains("string requiredNs = null;", src);
    }

    [Fact]
    public void M1_HookKeyMap_GatesOnExportedHooks_AndBindsAliases()
    {
        var src = Src("UitkxHmrCompiler.cs");
        Assert.Contains("!targetHookNames.Contains(nm)", src);
        Assert.Contains("map[bound] = targetNs + \".\" + targetContainer + \"::\" + nm;", src);
    }
}
