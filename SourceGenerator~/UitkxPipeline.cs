using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.CodeAnalysis;
using ReactiveUITK.Language;
using ReactiveUITK.Language.Lowering;
using ReactiveUITK.Language.Nodes;
using ReactiveUITK.Language.Parser;
using ReactiveUITK.SourceGenerator.Emitter;

namespace ReactiveUITK.SourceGenerator
{
    /// <summary>
    /// Top-level orchestrator for the UITKX compilation pipeline.
    ///
    /// Chain:
    ///   Stage 1 — DirectiveParser  — @namespace / @component / @using / @props / @key
    ///   Stage 2 — UitkxParser      — recursive-descent markup → AST
    ///   Stage 3 — PropsResolver    — maps tag names to V.* call patterns
    ///   Stage 4 — CSharpEmitter    — walks AST, produces compilable partial class
    ///
    /// Diagnostics produced:
    ///   UITKX0001   — unknown lowercase built-in element (warning)
    ///   UITKX0008   — unknown PascalCase function component (warning)
    ///   UITKX0113   — duplicate component name in one file (warning; duplicate skipped)
    ///   UITKX23xx   — strict import/export diagnostics (see DiagnosticCodes)
    ///   UITKX03xx   — parse errors
    /// </summary>
    public static class UitkxPipeline
    {
        public static UitkxPipelineResult Run(
            string source,
            string filePath,
            Compilation compilation,
            CancellationToken ct,
            ImmutableArray<PeerComponentInfo>? peerComponents = null,
            ImmutableArray<PeerHookContainerInfo>? peerHookContainers = null,
            ImmutableArray<PeerModuleInfo>? peerModules = null,
            IReadOnlyDictionary<string, string>? valueCycles = null
        )
        {
            ct.ThrowIfCancellationRequested();

            string fileName = Path.GetFileName(filePath);
            string hintName = BuildHintName(filePath);

            var parseDiags = new List<ParseDiagnostic>();

            // ── Stage 1: Directive parsing ────────────────────────────────────
            DirectiveSet directives = DirectiveParser.Parse(source, filePath, parseDiags);

            // ── Stage 1b: Effective-namespace seam (import/export grammar, §4) ─
            // Single point where namespace identity is resolved for every downstream
            // emitter/peer read. No-op with the feature flag off (returns the parsed
            // value); path-derived when strict imports are on.
            directives = directives with { Namespace = ResolveEffectiveNamespace(directives, filePath) };

            // §7 — the file's bare-hook-name → path-qualified-family-key map. Built once from the
            // now-resolved namespace + this file's own hooks + its imports; threaded into every
            // emitter (producer + consumer) so RegisterHook ids and customHookFamilyKeys agree.
            var hookFamilyKeyMap = BuildHookFamilyKeyMap(directives, peerHookContainers, filePath);

            ct.ThrowIfCancellationRequested();

            // ── Stage 1c: strict import diagnostics (StrictImports seam, §6) ──
            // Import validation (2300/2301/2308/2314) + reference detector (2305/2307) + unused import
            // (2304) + value-import cycle (2306). Runs for ALL files (component AND hook/module) BEFORE
            // the short-circuit below, because hook/module files are the usual value-cycle members and
            // can reference peer hooks/modules in their bodies. Appended to the parse-diag bag so errors
            // flow through the same #error path Unity relies on. Dormant with the flag off.
            if (UitkxFeatureFlags.StrictImports)
            {
                AppendImportValidationDiags(
                    directives, filePath,
                    peerComponents, peerHookContainers, peerModules, parseDiags);
                AppendStrictReferenceDiags(
                    directives, source, filePath,
                    peerComponents, peerHookContainers, peerModules, parseDiags);
                if (valueCycles != null &&
                    valueCycles.TryGetValue(filePath.Replace('\\', '/').TrimEnd('/'), out string? cycleMsg))
                {
                    parseDiags.Add(new ParseDiagnostic
                    {
                        Code = "UITKX2306",
                        Severity = ParseSeverity.Error,
                        SourceLine = directives.Imports.IsDefaultOrEmpty ? 1 : directives.Imports[0].Line,
                        Message = cycleMsg,
                    });
                }

                // UITKX2310 (no derivable namespace) is RETIRED by design: §4 now anchors
                // path-derivation at the nearest .asmdef, else the config root (see
                // ResolveDerivationAnchor), and ResolveEffectiveNamespace additionally falls
                // back to the legacy namespace — so every file Unity actually compiles (under
                // a project root) always resolves a namespace. The code slot stays reserved in
                // the frozen family table (2310) but is never emitted; a hard #error on the
                // mainstream no-asmdef Assembly-CSharp setup would have been a false positive.
            }

            // ── Short-circuit: hook/module files (no markup) ──────────────────
            if (directives.ComponentName == null
                && (!directives.HookDeclarations.IsDefaultOrEmpty
                    || !directives.ModuleDeclarations.IsDefaultOrEmpty))
            {
                // Guard: only emit into compilations that reference ReactiveUITK.Shared.
                if (compilation.GetTypeByMetadataName("ReactiveUITK.Core.VirtualNode") == null)
                    return new UitkxPipelineResult(hintName, null, ImmutableArray<Diagnostic>.Empty);
                if (!IsOwnedByCompilation(filePath, compilation.AssemblyName))
                    return new UitkxPipelineResult(hintName, null, ImmutableArray<Diagnostic>.Empty);

                var hookModuleDiags = new List<Diagnostic>(parseDiags.Count);
                foreach (var pd in parseDiags)
                    hookModuleDiags.Add(ParseDiagToRoslyn(pd));

                // Check for parse errors — emit #error directives
                int hmErrorCount = 0;
                foreach (var d in hookModuleDiags)
                    if (d.Severity == DiagnosticSeverity.Error) hmErrorCount++;

                if (hmErrorCount > 0)
                {
                    var errSb = new StringBuilder();
                    errSb.AppendLine("// <auto-generated/>");
                    errSb.AppendLine($"// UITKX parse errors in {fileName}");
                    string hmLinePath = filePath.Replace('\\', '/').Replace("\"", "\\\"");
                    foreach (var pd in parseDiags)
                        if (pd.Severity == ParseSeverity.Error)
                        {
                            int errLine = pd.SourceLine > 0 ? pd.SourceLine : 1;
                            errSb.AppendLine($"#line {errLine} \"{hmLinePath}\"");
                            errSb.AppendLine($"#error {pd.Message.Replace('\n', ' ').Replace('\r', ' ')}");
                        }
                    return new UitkxPipelineResult(hintName, errSb.ToString(), hookModuleDiags.ToImmutableArray());
                }

                // Inject imported peer hook containers + module aliases so a hook/module
                // BODY can reference imported peers. These files short-circuit here, before
                // the component injection seam (Stage 3d), so without this an imported
                // useX()/module member resolves to nothing → CS0103/CS0246 even though the
                // strict reference detector is satisfied (the name IS imported).
                // Gated behind StrictImports: with the flag OFF a hook/module-only file got
                // NO injection historically (there was no import syntax), and the flag-OFF
                // ResolveInjectedUsings exposes EVERY container — injecting them here would
                // change legacy output and risk CS0121 in the body. Flag ON only.
                if (UitkxFeatureFlags.StrictImports)
                    directives = directives with
                    {
                        Usings = ResolveInjectedUsings(
                            directives, peerHookContainers, filePath, strict: true, peerModules)
                    };

                // Emit hooks and modules as SEPARATE compilation units (each merges as a
                // partial across CUs). Concatenating them — as this path used to — places
                // the module unit's `using` directives after the hook unit's namespace → CS1529.
                string hmExtraHint(string section) =>
                    hintName.Substring(0, hintName.Length - ".g.cs".Length) + "." + section + ".g.cs";
                var hmExtra = ImmutableArray.CreateBuilder<(string, string)>();
                int hmDiagStart = hookModuleDiags.Count; // emit-stage diags (e.g. UITKX2311) start here
                string? hookSource = !directives.HookDeclarations.IsDefaultOrEmpty
                    ? HookEmitter.Emit(filePath, directives, hookModuleDiags, hookFamilyKeyMap) : null;
                string? moduleSource = !directives.ModuleDeclarations.IsDefaultOrEmpty
                    ? ModuleEmitter.Emit(filePath, directives, hookModuleDiags, peerComponents) : null;

                string? hmPrimary = null;
                if (!string.IsNullOrEmpty(hookSource))
                    hmPrimary = hookSource;
                if (!string.IsNullOrEmpty(moduleSource))
                {
                    if (hmPrimary == null)
                        hmPrimary = moduleSource;
                    else
                        hmExtra.Add((hmExtraHint("inlinemodules"), moduleSource!));
                }
                // Surface emit-stage Location.None diagnostics (e.g. UITKX2311 on a module-only
                // file) that Unity would otherwise drop.
                hmPrimary = SurfaceLocationlessDiagnostics(hmPrimary, hookModuleDiags, hmDiagStart);
                return new UitkxPipelineResult(
                    hintName,
                    hmPrimary,
                    hookModuleDiags.ToImmutableArray(),
                    hmExtra.ToImmutable()
                );
            }

            // ── Stage 2: Markup parsing ───────────────────────────────────────
            ImmutableArray<AstNode> parsedNodes = UitkxParser.Parse(
                source,
                filePath,
                directives,
                parseDiags
            );

            // ── Stage 2b: Canonical lowering ─────────────────────────────────
            ImmutableArray<AstNode> rootNodes = CanonicalLowering.LowerToRenderRoots(
                directives,
                parsedNodes,
                filePath
            );

            // ── Stage 2c: Parse setup-code JSX ───────────────────────────────
            // Local functions (e.g. PillBar, StatusBadge) and JSX variable
            // assignments contain embedded markup that isn't part of the main
            // render root AST.  Parse those ranges into nodes so validators
            // (hooks, missing key) can check them too.
            var setupJsxNodes = ParseSetupCodeJsx(source, filePath, directives, parseDiags);

            // Bridge: convert Roslyn-free ParseDiagnostic → Roslyn Diagnostic
            // so the rest of the pipeline (validators, emitter) keep their
            // existing List<Diagnostic> API and nothing else needs to change.
            var diagnostics = new List<Diagnostic>(parseDiags.Count);
            foreach (var pd in parseDiags)
                diagnostics.Add(ParseDiagToRoslyn(pd));

            ct.ThrowIfCancellationRequested();

            // Guard 1: only emit into compilations that reference ReactiveUITK.Shared.
            // The generator runs on every assembly in the project; assemblies that
            // don't reference Shared would get CS0246 errors for `using ReactiveUITK;`.
            if (compilation.GetTypeByMetadataName("ReactiveUITK.Core.VirtualNode") == null)
            {
                return new UitkxPipelineResult(
                    HintName: hintName,
                    Source: null,
                    Diagnostics: ImmutableArray<Diagnostic>.Empty
                );
            }

            // Guard 2: only emit into the compilation that *owns* this .uitkx file.
            // Multiple assemblies may reference Shared (e.g. ReactiveUITK.Editor and
            // ReactiveUITK.Samples both pass Guard 1). Emitting the same partial class
            // into both causes CS0101 / CS0436 duplicate-type errors.
            //
            // Ownership is determined by walking up the directory tree from the .uitkx
            // file looking for the nearest *.asmdef file. Unity always uses the nearest
            // ancestor .asmdef to decide which compiled assembly owns a given source file
            // — this generator uses the same rule so no companion .cs is required.
            //
            //   - Found .asmdef  → read its "name" field; owned when that equals
            //                      compilation.AssemblyName.
            //   - No .asmdef    → file lives in the default Assembly-CSharp (or
            //     found before       Assembly-CSharp-Editor) assembly. Unity decides
            //     reaching Assets/   between the two based on whether the file is
            //                      under an Editor/ folder. We use the same heuristic:
            //                      files under Editor/ → Assembly-CSharp-Editor,
            //                      all others → Assembly-CSharp.
            if (!IsOwnedByCompilation(filePath, compilation.AssemblyName))
            {
                return new UitkxPipelineResult(
                    HintName: hintName,
                    Source: null,
                    Diagnostics: ImmutableArray<Diagnostic>.Empty
                );
            }

            // If there are hard directive/parse errors, emit a .g.cs that contains
            // #error directives. This guarantees Unity's C# compiler surfaces the
            // errors in the Console — diagnostics with Location.None on a
            // non-SyntaxTree file are silently dropped by Unity otherwise.
            int errorCount = 0;
            foreach (var d in diagnostics)
                if (d.Severity == DiagnosticSeverity.Error)
                    errorCount++;

            if (errorCount > 0)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("// <auto-generated/>");
                sb.AppendLine(
                    $"// UITKX parse errors in {fileName} — fix the .uitkx file to regenerate."
                );
                // Use #line before each #error so Unity's C# compiler reports
                // the error as originating from the .uitkx file, enabling
                // click-to-navigate in the Unity Console.
                string linePath = filePath.Replace('\\', '/').Replace("\"", "\\\"");
                foreach (var pd in parseDiags)
                    if (pd.Severity == ParseSeverity.Error)
                    {
                        int line = pd.SourceLine > 0 ? pd.SourceLine : 1;
                        sb.AppendLine($"#line {line} \"{linePath}\"");
                        sb.AppendLine($"#error {pd.Message.Replace('\n', ' ').Replace('\r', ' ')}");
                    }

                return new UitkxPipelineResult(
                    HintName: hintName,
                    Source: sb.ToString(),
                    Diagnostics: diagnostics.ToImmutableArray()
                );
            }

            ct.ThrowIfCancellationRequested();

            // Diagnostics added from here on are EMIT-STAGE; the Location.None ones among
            // them (UITKX0113, UITKX2311, later-component parse errors) would be silently
            // dropped by Unity, so they are re-surfaced as #error/#warning before returning.
            int emitDiagStart = diagnostics.Count;

            // ── Stage 3: PropsResolver ────────────────────────────────────────
            var resolver = new PropsResolver(compilation, peerComponents);

            ct.ThrowIfCancellationRequested();

            // ── Stage 3b: Rules-of-Hooks validation ───────────────────────────
            HooksValidator.Validate(rootNodes, filePath, diagnostics);
            if (!setupJsxNodes.IsEmpty)
                HooksValidator.Validate(setupJsxNodes, filePath, diagnostics);

            // ── Stage 3c: Structural validation ───────────────────────────────
            StructureValidator.Validate(rootNodes, filePath, diagnostics, directives);
            if (!setupJsxNodes.IsEmpty)
                StructureValidator.ValidateNodes(setupJsxNodes, filePath, diagnostics);

            ct.ThrowIfCancellationRequested();

            // ── Stage 3d: Inject using-static for peer hook containers ────────
            // Hook files emit into a static class (e.g. TicTacToeHooks). Components
            // need `using static <HookNs>.<HookContainer>;` so hook methods like
            // useXxx() are directly callable without qualification, regardless of
            // whether the hook file lives in the same namespace as the consumer.
            // Asmdef ownership is already enforced one layer up in UitkxGenerator's
            // pre-scan via IsOwnedByCompilation, so every entry in peerHookContainers
            // belongs to the current Unity assembly.
            // Injection form is the StrictImports seam (§6.2): flag OFF exposes every container
            // (legacy, byte-identical); flag ON exposes only imported containers. See
            // ResolveInjectedUsings.
            directives = directives with
            {
                Usings = ResolveInjectedUsings(
                    directives, peerHookContainers, filePath, UitkxFeatureFlags.StrictImports, peerModules)
            };

            // Mixed-decl (§7): a file with hooks/modules AND a component needs the file's OWN hook
            // container exposed too, so a component can call a hook declared in the same file. The
            // per-import injection above only covers IMPORTED containers.
            if (!directives.HookDeclarations.IsDefaultOrEmpty
                && !directives.ComponentDeclarations.IsDefaultOrEmpty)
            {
                string ownContainer = $"static {directives.Namespace}.{HookEmitter.DeriveContainerClassName(filePath)}";
                if (!directives.Usings.Contains(ownContainer))
                    directives = directives with { Usings = directives.Usings.Add(ownContainer) };
            }

            // ── Stage 4: CSharpEmitter (one partial per component; mixed = concat) ──
            bool multiOrMixed =
                (directives.ComponentDeclarations.IsDefaultOrEmpty ? 0 : directives.ComponentDeclarations.Length) > 1
                || !directives.HookDeclarations.IsDefaultOrEmpty
                || !directives.ModuleDeclarations.IsDefaultOrEmpty;

            if (!multiOrMixed)
            {
                // Single component, no hooks/modules — byte-identical to the pre-mixed-decl path.
                string generatedSource = CSharpEmitter.Emit(filePath, directives, rootNodes, resolver, diagnostics, hookFamilyKeyMap);
                ct.ThrowIfCancellationRequested();
                return new UitkxPipelineResult(hintName, generatedSource, diagnostics.ToImmutableArray());
            }

            // Mixed-decl / multi-component: emit each section as its OWN standalone
            // compilation unit (its own using preamble + namespace block) via a
            // separate AddSource, NOT concatenated into one file. Each unit merges
            // with the others as a `partial` across compilation units — identical to
            // how sibling .uitkx files already compose. Concatenating self-contained
            // units would place later `using` directives after a namespace → CS1529.
            string ExtraHint(string section) =>
                hintName.Substring(0, hintName.Length - ".g.cs".Length) + "." + section + ".g.cs";

            var extraSources = ImmutableArray.CreateBuilder<(string, string)>();
            if (!directives.HookDeclarations.IsDefaultOrEmpty)
            {
                string hookSrc = HookEmitter.Emit(filePath, directives, diagnostics, hookFamilyKeyMap);
                if (!string.IsNullOrEmpty(hookSrc))
                    extraSources.Add((ExtraHint("inlinehooks"), hookSrc));
            }
            if (!directives.ModuleDeclarations.IsDefaultOrEmpty)
            {
                string moduleSrc = ModuleEmitter.Emit(filePath, directives, diagnostics, peerComponents);
                if (!string.IsNullOrEmpty(moduleSrc))
                    extraSources.Add((ExtraHint("inlinemodules"), moduleSrc));
            }

            var componentsToEmit = directives.ComponentDeclarations.IsDefaultOrEmpty
                ? ImmutableArray<ComponentDeclaration>.Empty
                : directives.ComponentDeclarations;
            string? primarySource = null;
            var emittedComponentNames = new HashSet<string>(StringComparer.Ordinal);
            for (int ci = 0; ci < componentsToEmit.Length; ci++)
            {
                ct.ThrowIfCancellationRequested();

                // Two components with the same name in one file would emit duplicate
                // partial classes / members (CS0111/CS0101/CS0579). Flag UITKX0113 and
                // skip the duplicate so the generated code still compiles for the rest.
                if (!emittedComponentNames.Add(componentsToEmit[ci].Name))
                {
                    diagnostics.Add(Diagnostic.Create(
                        UitkxDiagnostics.DuplicateComponentInFile,
                        Location.None,
                        componentsToEmit[ci].Name));
                    continue;
                }

                var cd = SynthesizePerComponent(directives, componentsToEmit[ci]);

                ImmutableArray<AstNode> cRoots;
                if (ci == 0)
                {
                    cRoots = rootNodes; // the first component's markup is already parsed + validated
                }
                else
                {
                    var cThrow = new List<ParseDiagnostic>();
                    var cParsed = UitkxParser.Parse(source, filePath, cd, cThrow);
                    // Later components own their OWN setup-code JSX ranges (the first
                    // component owns rootNodes + setupJsxNodes), so parse + validate them
                    // here too — otherwise a Rules-of-Hooks / structural violation inside a
                    // later component's setup JSX is silently unreported.
                    var cSetupJsx = ParseSetupCodeJsx(source, filePath, cd, cThrow);
                    // Surface the later component's own parse errors (markup + setup JSX).
                    foreach (var pd in cThrow)
                        diagnostics.Add(ParseDiagToRoslyn(pd));
                    cRoots = CanonicalLowering.LowerToRenderRoots(cd, cParsed, filePath);
                    HooksValidator.Validate(cRoots, filePath, diagnostics);
                    if (!cSetupJsx.IsEmpty)
                        HooksValidator.Validate(cSetupJsx, filePath, diagnostics);
                    StructureValidator.Validate(cRoots, filePath, diagnostics, cd);
                    if (!cSetupJsx.IsEmpty)
                        StructureValidator.ValidateNodes(cSetupJsx, filePath, diagnostics);
                }

                string compSrc = CSharpEmitter.Emit(filePath, cd, cRoots, resolver, diagnostics, hookFamilyKeyMap);
                if (ci == 0)
                    primarySource = compSrc; // main component keeps the canonical {file}.g.cs hint
                else
                    // Index-PREFIXED so a component named "inlinehooks"/"inlinemodules" can
                    // never collide with those fixed section labels (→ duplicate AddSource
                    // hint → throw), and two components always get distinct hints via the index.
                    extraSources.Add((ExtraHint("c" + ci + "_" + (cd.ComponentName ?? "")), compSrc));
            }

            ct.ThrowIfCancellationRequested();
            primarySource = SurfaceLocationlessDiagnostics(primarySource, diagnostics, emitDiagStart);
            return new UitkxPipelineResult(
                hintName,
                primarySource,
                diagnostics.ToImmutableArray(),
                extraSources.ToImmutable());
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        // Matches the "name" field in a Unity .asmdef JSON file.
        private static readonly Regex s_asmdefNameRegex = new Regex(
            @"""name""\s*:\s*""([^""]+)""",
            RegexOptions.CultureInvariant
        );

        /// <summary>
        /// Walks up the directory tree from <paramref name="uitkxFilePath"/> looking
        /// for the nearest <c>*.asmdef</c> file. Returns the assembly name declared
        /// in that file's <c>"name"</c> JSON field, or <c>null</c> when no .asmdef
        /// is found (meaning the file belongs to the default <c>Assembly-CSharp</c>
        /// assembly). The walk stops at the <c>Assets</c> directory boundary.
        /// </summary>
        internal static bool IsOwnedByCompilation(string filePath, string? compilationAssemblyName)
        {
            string? ownerAsmName = FindOwningAsmdefAssemblyName(filePath);
            if (ownerAsmName != null)
            {
                return string.Equals(
                    ownerAsmName,
                    compilationAssemblyName,
                    StringComparison.Ordinal
                );
            }

            string asmName = compilationAssemblyName ?? string.Empty;
            bool isEditorAsm = asmName.Contains("Editor", StringComparison.Ordinal);
            bool isEditorFile = IsInsideEditorFolder(filePath);
            return asmName.StartsWith("Assembly-CSharp", StringComparison.Ordinal)
                && isEditorAsm == isEditorFile;
        }

        private static string? FindOwningAsmdefAssemblyName(string uitkxFilePath)
        {
            try
            {
                string? dir = Path.GetDirectoryName(uitkxFilePath);
                while (!string.IsNullOrEmpty(dir))
                {
                    foreach (string asmdef in Directory.GetFiles(dir, "*.asmdef"))
                    {
                        string json = File.ReadAllText(asmdef);
                        var m = s_asmdefNameRegex.Match(json);
                        if (m.Success)
                            return m.Groups[1].Value.Trim();
                    }

                    // Stop at the Assets folder — above it is the Unity project root,
                    // not part of any assembly.
                    string dirName = Path.GetFileName(dir);
                    if (string.Equals(dirName, "Assets", StringComparison.OrdinalIgnoreCase))
                        break;

                    dir = Path.GetDirectoryName(dir);
                }
            }
            catch
            {
                // Never crash the generator on filesystem errors.
            }
            return null;
        }


        /// <summary>
        /// The effective C# namespace for a parsed <c>.uitkx</c> file (import/export grammar, leg 3, §4).
        /// The single seam controlling namespace identity:
        /// <list type="bullet">
        ///   <item><description>Flag OFF (default) → the legacy value the parser already produced
        ///   (explicit <c>@namespace</c>, else companion-<c>.cs</c> inference / hard-coded fallback).
        ///   Byte-identical to pre-feature output.</description></item>
        ///   <item><description>Flag ON → explicit <c>@namespace</c> wins; otherwise the path-derived
        ///   default anchored at the owning asmdef. A <c>null</c> derivation (no owning asmdef) is the
        ///   UITKX2310 condition — the caller reports it; this falls back to the legacy value so the
        ///   emit never produces a null namespace.</description></item>
        /// </list>
        /// </summary>
        internal static string? ResolveEffectiveNamespace(DirectiveSet directives, string filePath)
            => EffectiveNamespace.Resolve(directives.HasExplicitNamespace, directives.Namespace, filePath);

        /// <summary>
        /// The full <c>using</c> list for a component file after hook-container injection (§6.2).
        /// The single seam controlling which hook containers a component sees:
        /// <list type="bullet">
        ///   <item><description>Flag OFF (<paramref name="strict"/> = false) → the legacy behavior:
        ///   every hook container in the asmdef is exposed via <c>using static</c> (the pre-feature,
        ///   CS0121-prone form). Byte-identical to prior output.</description></item>
        ///   <item><description>Flag ON → only the container(s) whose source file is named by one of
        ///   this file's <c>import</c> declarations (specifier → path via <see cref="ImportResolver"/>,
        ///   matched against the peer <see cref="PeerHookContainerInfo.SourceFilePath"/>). C# has no
        ///   per-method static import, so the whole matched container is exposed; per-NAME strictness
        ///   stays a uitkx diagnostic.</description></item>
        /// </list>
        /// Returns <see cref="DirectiveSet.Usings"/> plus the injected <c>static …</c> entries, order-
        /// and dedup-preserving. Pure/host-agnostic so both modes are directly unit-testable.
        /// </summary>
        public static ImmutableArray<string> ResolveInjectedUsings(
            DirectiveSet directives,
            ImmutableArray<PeerHookContainerInfo>? peerHookContainers,
            string filePath,
            bool strict,
            ImmutableArray<PeerModuleInfo>? peerModules = null)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var result = ImmutableArray.CreateBuilder<string>();
            foreach (var u in directives.Usings)
                if (seen.Add(u))
                    result.Add(u);

            bool haveHooks = peerHookContainers != null && !peerHookContainers.Value.IsDefaultOrEmpty;

            if (!strict)
            {
                if (haveHooks)
                    foreach (var phc in peerHookContainers.Value)
                    {
                        string fqn = $"static {phc.Namespace}.{phc.ClassName}";
                        if (seen.Add(fqn))
                            result.Add(fqn);
                    }
                return result.ToImmutable();
            }

            // Strict: expose only what this file actually imports.
            if (directives.Imports.IsDefaultOrEmpty)
                return result.ToImmutable();

            string importerDir = NormalizeAbs(Path.GetDirectoryName(filePath));
            string? projectRoot = AssetPathUtil.GetProjectRoot(filePath);
            string rootDir = projectRoot != null
                ? NormalizeAbs(projectRoot + "/" + UitkxConfig.LoadRoot(importerDir))
                : importerDir;

            var importedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var importedFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase); // "file\0name"
            foreach (var imp in directives.Imports)
            {
                string? candidate = ImportResolver.MapSpecifierToPath(
                    importerDir, imp.Specifier, rootDir, out _);
                if (candidate == null)
                    continue;
                string cn = NormalizeAbs(candidate);
                importedFiles.Add(cn);
                foreach (var name in imp.Names)
                    importedFileNames.Add(cn + "\0" + name);
            }

            // Hook: expose the whole owning container (C# has no per-method static import).
            if (haveHooks)
                foreach (var phc in peerHookContainers.Value)
                {
                    if (phc.SourceFilePath == null)
                        continue;
                    if (!importedFiles.Contains(NormalizeAbs(phc.SourceFilePath)))
                        continue;
                    string fqn = $"static {phc.Namespace}.{phc.ClassName}";
                    if (seen.Add(fqn))
                        result.Add(fqn);
                }

            // Module (§6.3): alias each imported module to its FQN — but ONLY when it lives in a
            // DIFFERENT namespace than the importer. A same-namespace module is already in scope, and
            // aliasing it would conflict with the type declaration (CS0576) — the common case for the
            // migrated Samples (companions share their component's namespace), so this guard is what
            // keeps them compiling.
            if (peerModules != null)
                foreach (var pm in peerModules.Value)
                {
                    if (pm.SourceFilePath == null || !pm.IsExported)
                        continue;
                    if (!importedFileNames.Contains(NormalizeAbs(pm.SourceFilePath) + "\0" + pm.Name))
                        continue;
                    if (string.Equals(pm.Namespace, directives.Namespace, StringComparison.Ordinal))
                        continue; // same namespace → already accessible; an alias would be CS0576
                    // The component emitter unconditionally emits a fixed block of type aliases
                    // (using Color = UnityEngine.Color; using Length = …; etc. — see
                    // ReservedTypeAliases). A module whose name collides with one of those would
                    // inject a SECOND `using {Name} = …;` → CS1537 (duplicate using alias), a
                    // file-level hard error that obscures the whole file. Skip the alias so the
                    // built-in wins — the reserved style-type names are effectively keywords in
                    // .uitkx. A module deliberately named after one stays reachable by its FQN, and
                    // any bare-name use surfaces a localized error at the call site rather than a
                    // file-breaking CS1537. (No family-band code is spent here: 2315 stays reserved.)
                    if (ReservedTypeAliases.Contains(pm.Name))
                        continue;
                    string alias = $"{pm.Name} = {pm.Namespace}.{pm.Name}";
                    if (seen.Add(alias))
                        result.Add(alias);
                }

            return result.ToImmutable();
        }

        /// <summary>
        /// Maps each custom-hook name callable in <paramref name="filePath"/> to its PATH-QUALIFIED
        /// family key <c>{EffectiveNs}.{Container}::{HookName}</c> (§7). Same-file hooks qualify
        /// against this file's own effective namespace + container; imported hooks resolve the import
        /// specifier to the owning peer container (which was built with the SAME
        /// <see cref="ResolveEffectiveNamespace"/> + <c>DeriveContainerClassName</c> the producer uses,
        /// so producer id and consumer key are byte-identical). Consumers (component setup + hook
        /// bodies) qualify their extracted bare names through this map; the runtime matches producer
        /// ids to consumer keys by ordinal string equality, so the two MUST agree exactly.
        /// </summary>
        internal static Dictionary<string, string> BuildHookFamilyKeyMap(
            DirectiveSet directives,
            ImmutableArray<PeerHookContainerInfo>? peerHookContainers,
            string filePath)
        {
            var map = new Dictionary<string, string>(StringComparer.Ordinal);
            string ns = directives.Namespace ?? string.Empty;

            // Same-file hooks (mixed-decl) → this file's own container.
            if (!directives.HookDeclarations.IsDefaultOrEmpty)
            {
                string ownContainer = HookEmitter.DeriveContainerClassName(filePath);
                foreach (var h in directives.HookDeclarations)
                    if (!string.IsNullOrEmpty(h.Name))
                        map[h.Name] = $"{ns}.{ownContainer}::{h.Name}";
            }

            // Imported hooks → resolve each import specifier to its owning peer container.
            if (peerHookContainers != null && !peerHookContainers.Value.IsDefaultOrEmpty
                && !directives.Imports.IsDefaultOrEmpty)
            {
                string importerDir = NormalizeAbs(Path.GetDirectoryName(filePath));
                // Shared with HMR (EffectiveNamespace.UiSourceRootDir) so `~/` hook imports
                // resolve to the same target file on both sides.
                string rootDir = EffectiveNamespace.UiSourceRootDir(filePath) ?? importerDir;

                foreach (var imp in directives.Imports)
                {
                    string? candidate = ImportResolver.MapSpecifierToPath(
                        importerDir, imp.Specifier, rootDir, out _);
                    if (candidate == null)
                        continue;
                    string candN = NormalizeAbs(candidate);
                    foreach (var phc in peerHookContainers.Value)
                    {
                        if (phc.SourceFilePath == null
                            || !string.Equals(NormalizeAbs(phc.SourceFilePath), candN, StringComparison.OrdinalIgnoreCase))
                            continue;
                        foreach (var name in imp.Names)
                            if (phc.ExportedHookNames.Contains(name))
                                map[name] = $"{phc.Namespace}.{phc.ClassName}::{name}";
                        break;
                    }
                }
            }
            return map;
        }

        /// <summary>
        /// Type-alias identifiers the component/hook/module emitters emit unconditionally
        /// (<c>using Color = UnityEngine.Color;</c>, <c>using Length = …;</c>, …). An imported
        /// module whose name matches one of these cannot be injected as a <c>using</c> alias
        /// (CS1537). Kept in sync with the alias block in <c>CSharpEmitter</c>/<c>ModuleEmitter</c>.
        /// </summary>
        internal static readonly HashSet<string> ReservedTypeAliases = new HashSet<string>(StringComparer.Ordinal)
        {
            "Color", "UColor", "EasingFunction", "EasingMode", "BackgroundRepeat",
            "BackgroundPosition", "BackgroundSize", "TransformOrigin", "BackgroundPositionKeyword",
            "BackgroundSizeType", "Repeat", "Length", "StyleKeyword", "TextAutoSizeMode",
            "FilterFunction", "Ratio", "StyleRatio", "MaterialDefinition", "StyleMaterialDefinition",
        };

        private static string NormalizeAbs(string? p) =>
            (p ?? string.Empty).Replace('\\', '/').TrimEnd('/');

        /// <summary>
        /// Project a mixed-decl <see cref="DirectiveSet"/> down to a single component for emission
        /// (§7): the singular component fields are set from <paramref name="c"/>, and the hook/module
        /// declarations are cleared (they are emitted once by HookEmitter/ModuleEmitter, not per
        /// component). The shared preamble — injected <c>Usings</c>, <c>Imports</c>, <c>Namespace</c>,
        /// <c>@uss</c>, <c>@inject</c> — is preserved.
        /// </summary>
        private static DirectiveSet SynthesizePerComponent(DirectiveSet baseDir, ComponentDeclaration c) =>
            baseDir with
            {
                ComponentName = c.Name,
                PropsTypeName = c.PropsTypeName,
                DefaultKey = c.DefaultKey,
                FunctionParams = c.FunctionParams,
                FunctionSetupCode = c.FunctionSetupCode,
                FunctionSetupStartLine = c.FunctionSetupStartLine,
                FunctionSetupStartOffset = c.FunctionSetupStartOffset,
                MarkupStartLine = c.MarkupStartLine,
                MarkupStartIndex = c.MarkupStartIndex,
                MarkupEndIndex = c.MarkupEndIndex,
                ComponentDeclarationLine = c.DeclarationLine,
                ComponentNameColumn = c.NameColumn,
                FunctionReturnEndLine = c.ReturnEndLine,
                FunctionBodyEndLine = c.BodyEndLine,
                SetupCodeMarkupRanges = c.SetupCodeMarkupRanges,
                SetupCodeBareJsxRanges = c.SetupCodeBareJsxRanges,
                FunctionSetupGapOffset = c.FunctionSetupGapOffset,
                FunctionSetupGapLength = c.FunctionSetupGapLength,
                HookDeclarations = ImmutableArray<HookDeclaration>.Empty,
                ModuleDeclarations = ImmutableArray<ModuleDeclaration>.Empty,
                ComponentDeclarations = ImmutableArray.Create(c),
            };

        // Builtin/ambient hook names (from the single-source HookRegistry) that need no import.
        private static readonly HashSet<string> s_builtinHooks =
            new HashSet<string>(global::ReactiveUITK.Core.HookRegistry.CanonicalNames, StringComparer.Ordinal);

        /// <summary>
        /// Validates each <c>import</c> declaration (resolves the specifier + checks the imported
        /// names are exported by the target) and appends 2300/2301/2308/2314 to
        /// <paramref name="parseDiags"/>. Only invoked when <see cref="UitkxFeatureFlags.StrictImports"/>
        /// is on. Uses the real filesystem + asmdef walk; export membership comes from the peer tables.
        /// </summary>
        private static void AppendImportValidationDiags(
            DirectiveSet directives,
            string filePath,
            ImmutableArray<PeerComponentInfo>? peerComponents,
            ImmutableArray<PeerHookContainerInfo>? peerHookContainers,
            ImmutableArray<PeerModuleInfo>? peerModules,
            List<ParseDiagnostic> parseDiags)
        {
            if (directives.Imports.IsDefaultOrEmpty)
                return;

            string importerDir = NormalizeAbs(Path.GetDirectoryName(filePath));
            string? projectRoot = AssetPathUtil.GetProjectRoot(filePath);
            string rootDir = projectRoot != null
                ? NormalizeAbs(projectRoot + "/" + UitkxConfig.LoadRoot(importerDir))
                : importerDir;
            string? importerAsmdef = FindOwningAsmdefAssemblyName(filePath);

            bool IsExportedByFile(string name, string targetPath)
            {
                string t = NormalizeAbs(targetPath);
                if (peerComponents != null)
                    foreach (var pc in peerComponents.Value)
                        if (pc.IsExported && pc.Name == name
                            && string.Equals(NormalizeAbs(pc.SourceFilePath), t, StringComparison.OrdinalIgnoreCase))
                            return true;
                if (peerHookContainers != null)
                    foreach (var ph in peerHookContainers.Value)
                        if (string.Equals(NormalizeAbs(ph.SourceFilePath), t, StringComparison.OrdinalIgnoreCase)
                            && ph.ExportedHookNames.Contains(name))
                            return true;
                if (peerModules != null)
                    foreach (var pm in peerModules.Value)
                        if (pm.IsExported && pm.Name == name
                            && string.Equals(NormalizeAbs(pm.SourceFilePath), t, StringComparison.OrdinalIgnoreCase))
                            return true;
                return false;
            }

            var findings = StrictImportDetector.ValidateImports(
                directives, importerDir, rootDir, importerAsmdef,
                path => File.Exists(path),
                path => FindOwningAsmdefAssemblyName(path),
                IsExportedByFile);

            foreach (var f in findings)
                parseDiags.Add(new ParseDiagnostic
                {
                    Code = f.Code,
                    Severity = ParseSeverity.Error,
                    SourceLine = f.Line,
                    Message = f.Message,
                });
        }

        /// <summary>
        /// Runs <see cref="StrictImportDetector"/> over <paramref name="source"/> and appends its
        /// 2305/2307 findings to <paramref name="parseDiags"/>. The peer export table is built from
        /// the pre-scan peer arrays (exported names only — the frozen "exported-names ledger"). Only
        /// invoked when <see cref="UitkxFeatureFlags.StrictImports"/> is on.
        /// </summary>
        private static void AppendStrictReferenceDiags(
            DirectiveSet directives,
            string source,
            string filePath,
            ImmutableArray<PeerComponentInfo>? peerComponents,
            ImmutableArray<PeerHookContainerInfo>? peerHookContainers,
            ImmutableArray<PeerModuleInfo>? peerModules,
            List<ParseDiagnostic> parseDiags)
        {
            var peerExports = new List<StrictImportDetector.PeerExport>();

            if (peerComponents != null)
                foreach (var pc in peerComponents.Value)
                    if (pc.IsExported && !string.IsNullOrEmpty(pc.SourceFilePath))
                        peerExports.Add(new StrictImportDetector.PeerExport(
                            pc.Name, pc.SourceFilePath!, StrictImportDetector.ExportKind.Component));

            if (peerHookContainers != null)
                foreach (var ph in peerHookContainers.Value)
                    if (!string.IsNullOrEmpty(ph.SourceFilePath))
                        foreach (var hookName in ph.ExportedHookNames)
                            peerExports.Add(new StrictImportDetector.PeerExport(
                                hookName, ph.SourceFilePath!, StrictImportDetector.ExportKind.Hook));

            if (peerModules != null)
                foreach (var pm in peerModules.Value)
                    if (pm.IsExported && !string.IsNullOrEmpty(pm.SourceFilePath))
                        peerExports.Add(new StrictImportDetector.PeerExport(
                            pm.Name, pm.SourceFilePath!, StrictImportDetector.ExportKind.Module));

            if (peerExports.Count == 0)
                return;

            string scannable = StrictImportDetector.ScrubNonCode(source);
            var findings = StrictImportDetector.Detect(
                directives, filePath, scannable, peerExports, s_builtinHooks.Contains);
            findings.AddRange(StrictImportDetector.DetectUnusedImports(directives, scannable));

            foreach (var f in findings)
                parseDiags.Add(new ParseDiagnostic
                {
                    Code = f.Code,
                    Severity = f.Code == "UITKX2304" ? ParseSeverity.Warning : ParseSeverity.Error,
                    SourceLine = f.Line,
                    Message = f.Message,
                });
        }

        /// <summary>
        /// Returns <c>true</c> when the given path contains an <c>Editor</c>
        /// directory segment. Unity uses this convention to assign files without
        /// an .asmdef to <c>Assembly-CSharp-Editor</c>.
        /// </summary>
        private static bool IsInsideEditorFolder(string filePath)
        {
            foreach (
                string part in (filePath ?? string.Empty).Split(
                    new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }
                )
            )
            {
                if (string.Equals(part, "Editor", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Parses JSX ranges embedded in function-style setup code (local
        /// functions, variable assignments with JSX) into AST nodes so that
        /// hook and structural validators can inspect them.
        /// </summary>
        private static ImmutableArray<AstNode> ParseSetupCodeJsx(
            string source,
            string filePath,
            DirectiveSet directives,
            List<ParseDiagnostic> parseDiags)
        {
            var allRanges = directives.SetupCodeMarkupRanges;
            if (!directives.SetupCodeBareJsxRanges.IsDefaultOrEmpty)
            {
                allRanges = allRanges.IsDefaultOrEmpty
                    ? directives.SetupCodeBareJsxRanges
                    : allRanges.AddRange(directives.SetupCodeBareJsxRanges);
            }
            if (allRanges.IsDefaultOrEmpty)
                return ImmutableArray<AstNode>.Empty;

            var builder = ImmutableArray.CreateBuilder<AstNode>();
            foreach (var (jsxStart, jsxEnd, jsxLine) in allRanges)
            {
                var jsxDirectives = directives with
                {
                    MarkupStartIndex = jsxStart,
                    MarkupEndIndex = jsxEnd,
                    MarkupStartLine = jsxLine,
                };
                // Use a separate diag list so parse warnings from setup JSX
                // don't duplicate or interfere with the main parse diags that
                // have already been bridged to Roslyn Diagnostic objects.
                var jsxDiags = new List<ParseDiagnostic>();
                var jsxNodes = UitkxParser.Parse(source, filePath, jsxDirectives, jsxDiags);
                builder.AddRange(jsxNodes);
            }
            return builder.ToImmutable();
        }

        private static Diagnostic ParseDiagToRoslyn(ParseDiagnostic pd)
        {
            var severity =
                pd.Severity == ParseSeverity.Error ? DiagnosticSeverity.Error
                : pd.Severity == ParseSeverity.Warning ? DiagnosticSeverity.Warning
                : DiagnosticSeverity.Info;
            var desc = new DiagnosticDescriptor(
                pd.Code,
                title: "",
                messageFormat: pd.Message,
                category: "ReactiveUITK.Parser",
                severity,
                isEnabledByDefault: true
            );
            return Diagnostic.Create(desc, Location.None);
        }

        /// <summary>
        /// Prepends <c>#error</c>/<c>#warning</c> directives to a generated source for every
        /// EMIT-STAGE diagnostic (from <paramref name="fromIndex"/> onward) that was created with
        /// <see cref="Location.None"/>. Unity silently drops Location.None diagnostics on a .uitkx
        /// (it is not a C# SyntaxTree) — the same reason the directive/parse-error path emits
        /// <c>#error</c>. Real-location diagnostics (the validators' <c>Location.Create</c>) surface
        /// on their own and are left untouched. Covers UITKX0113 (duplicate component, warning),
        /// UITKX2311 (export mismatch, warning), and later-component parse errors (error, halts).
        /// </summary>
        private static string? SurfaceLocationlessDiagnostics(
            string? source, IReadOnlyList<Diagnostic> diagnostics, int fromIndex)
        {
            if (source == null)
                return null;
            System.Text.StringBuilder? sb = null;
            for (int i = fromIndex; i < diagnostics.Count; i++)
            {
                var d = diagnostics[i];
                if (d.Location != Location.None)
                    continue; // real location → Unity surfaces it directly
                string? directive = d.Severity == DiagnosticSeverity.Error ? "#error"
                    : d.Severity == DiagnosticSeverity.Warning ? "#warning"
                    : null;
                if (directive == null)
                    continue;
                (sb ??= new System.Text.StringBuilder())
                    .Append(directive).Append(' ')
                    .Append(d.Id).Append(": ")
                    .AppendLine(d.GetMessage().Replace('\n', ' ').Replace('\r', ' '));
            }
            return sb == null ? source : sb.ToString() + source;
        }

        private static string BuildHintName(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            string normalizedPath = (filePath ?? string.Empty)
                .Replace('\\', '/')
                .ToLowerInvariant();

            using var md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(normalizedPath));
            string suffix = BitConverter
                .ToString(hash, 0, 6)
                .Replace("-", string.Empty)
                .ToLowerInvariant();

            return $"{fileName}.{suffix}.g.cs";
        }
    }
}
