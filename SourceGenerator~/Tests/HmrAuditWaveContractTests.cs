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
    [InlineData("UitkxHmrFileWatcher.cs")]
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
    public void FieldWave_NewModeImportTargets_InlineExportsWhenUnreferenceable()
    {
        // Copy-rename field scenario: a file CREATED mid-HMR-session (reload locked) has no
        // {ns}.__Exports in the project assembly — the companion/import-target pass must
        // inline its container into the hot unit instead of feeding the legacy emitters a
        // plain-declaration set (which crashed) or letting the injected using dangle (CS0234).
        // Rename-flow hardening: the gate probes what the compile can actually REFERENCE
        // (project assemblies + the target's registered hot DLL), not the whole AppDomain —
        // the old whole-AppDomain probe let the target's own first hot compile disable
        // inlining forever while nothing referenced its DLL.
        var src = Src("UitkxHmrCompiler.cs");
        Assert.Contains("!TypeExistsInProjectAssemblies(exportsFqn)", src);
        Assert.Contains("&& !HotExportsAvailable(exportsFqn)", src);
        Assert.Contains("private static bool TypeExistsInProjectAssemblies(string fullTypeName)", src);
        Assert.DoesNotContain("TypeExistsInAppDomain", src);
        // Legacy module/hook emitters are unreachable for new-mode candidates.
        Assert.Contains("if (!candUsesLegacy)", src);
    }

    [Fact]
    public void RenameWave_MemberRoute_RegistersPerFileIdentity()
    {
        // Rename-flow root cause: the literal registry key "__Exports" collided every
        // member-only file onto one _hmrAssemblyPaths slot — each member compile deleted the
        // previous file's DLL and hijacked its cached compilation and cross-ref. The key is
        // now the per-file container FQN {ns}.__Exports; the container TYPE NAME stays
        // "__Exports" so SwapHooks still resolves it by HookContainerClass.
        var src = Src("UitkxHmrCompiler.cs");
        Assert.Contains(
            "string exKey = string.IsNullOrEmpty(exNs) ? \"__Exports\" : exNs + \".__Exports\";",
            src);
        Assert.Contains("result.ComponentName = exKey;", src);
        Assert.Contains("new[] { exportsCSharp }, exKey, uitkxPath", src);
        Assert.DoesNotContain("new[] { exportsCSharp }, \"__Exports\"", src);
        Assert.Contains("result.HookContainerClass = \"__Exports\";", src);
    }

    [Fact]
    public void RenameWave_MemberRoute_MarksGenuinelyNew_ForImporterCrossRefs()
    {
        // Mid-session member files have no project-assembly type, so importers can only bind
        // their values through the cross-ref registry — which BuildCrossRefs gates on the
        // genuinely-new set. Without this registration the import fan-out recompiles every
        // importer against NOTHING that carries {ns}.__Exports (the rename-flow dead end).
        // Null namespace arg: exKey already IS the container FQN the probe must check.
        var src = Src("UitkxHmrCompiler.cs");
        Assert.Contains("_memberRegistryKeysByFile[NormalizeRegistryPath(uitkxPath)] = exKey;", src);
        Assert.Contains("CheckIfGenuinelyNew(exKey, null);", src);
    }

    [Fact]
    public void RenameWave_DeleteAndRenameEvents_EvictStaleRegistrations()
    {
        // A rename (delete old path + create new path) must not leave the old identity's
        // DLL registered as a cross-ref, its hook-container index entry, or its outgoing
        // import edges. The watcher surfaces FSW Deleted and the Renamed OLD path through
        // the debounced deletion queue (exists-again guarded — delete-and-replace saves are
        // saves, not deletions); the controller evicts per-path state on that event.
        var compiler = Src("UitkxHmrCompiler.cs");
        Assert.Contains("public void EvictFileRegistration(string uitkxPath)", compiler);
        var controller = Src("UitkxHmrController.cs");
        Assert.Contains("_compiler?.EvictFileRegistration(uitkxPath);", controller);
        Assert.Contains("HookContainerRegistry.Invalidate(uitkxPath);", controller);
        var watcher = Src("UitkxHmrFileWatcher.cs");
        Assert.Contains("_watcher.Deleted += (s, e) => EnqueueDeletion(e.FullPath);", watcher);
        Assert.Contains("EnqueueDeletion(e.OldFullPath);", watcher);
        Assert.Contains("if (!File.Exists(path))", watcher);
    }

    [Fact]
    public void RenameWave_MemberValueEdits_PropagateViaImporterFanOutRecompile()
    {
        // Designed propagation for mid-session member files (no project type to static-swap
        // onto): the controller fans the change out to importers, forces each to rebuild
        // fresh, and the fresh build binds the latest hot DLL registered under the member
        // file's key. The compiler-side invalidation must therefore compute the SAME
        // per-file key for member files, or the fan-out invalidates a key nobody caches.
        var controller = Src("UitkxHmrController.cs");
        Assert.Contains("_compiler?.InvalidateCompilationForFile(importer);", controller);
        var compiler = Src("UitkxHmrCompiler.cs");
        Assert.Contains("? \"__Exports\" : invNs + \".__Exports\";", compiler);
    }

    [Fact]
    public void M1_HookKeyMap_GatesOnExportedHooks_AndBindsAliases()
    {
        var src = Src("UitkxHmrCompiler.cs");
        Assert.Contains("!targetHookNames.Contains(nm)", src);
        Assert.Contains("map[bound] = targetNs + \".\" + targetContainer + \"::\" + nm;", src);
    }
}
