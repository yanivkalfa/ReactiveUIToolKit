using System;
using System.Collections.Generic;
using System.IO;
using ReactiveUITK.Language.Parser;

namespace ReactiveUITK.Language
{
    /// <summary>
    /// The single source of truth for "what <c>using</c> lines does a file's <c>import</c> set
    /// imply" (import/export grammar §6.2/§6.3): for every <c>import {{ X }} from "./file"</c>
    /// this resolves the specifier to its target, parses it, computes the target's EFFECTIVE
    /// (path-derived, config-aware) namespace, and yields the injection payloads:
    /// <list type="bullet">
    ///   <item><description>hook import → <c>static {Ns}.{Container}</c> (whole container — C# has
    ///   no per-method static import);</description></item>
    ///   <item><description>module import → <c>{Name} = {Ns}.{Name}</c> alias (skipped same-namespace
    ///   → CS0576, and for <see cref="ReservedTypeAliases"/> → CS1537);</description></item>
    ///   <item><description>component import → <c>{Name} = {Ns}.{Name}</c> alias, same guards — a
    ///   C# BODY reference to an imported component's type (<c>Preset.PresetProps</c>,
    ///   <c>TableView.Column</c>) needs it; tags alone don't (they emit FQN calls).</description></item>
    /// </list>
    /// <para>Consumed by the LSP virtual-document generator (typed) and by the Unity HMR compiler
    /// (via reflection, like <see cref="EffectiveNamespace"/>) so editor IntelliSense, hot reload,
    /// and the real build agree on what an import brings into scope. The source generator computes
    /// the equivalent from its pre-scan peer tables (no file IO in the incremental pipeline) —
    /// behavioral parity with it is pinned by tests.</para>
    /// <para>Filesystem-reads each target; degrades silently (no line) when a target is
    /// unresolvable/unreadable, matching the historical VDG behavior.</para>
    /// </summary>
    public static class ImportScopeFacts
    {
        /// <summary>
        /// Type-alias identifiers the emitters emit unconditionally (<c>using Color =
        /// UnityEngine.Color;</c>, …). An imported module/component whose name matches one cannot be
        /// injected as an alias (CS1537 duplicate alias). Mirrored by
        /// <c>UitkxPipeline.ReservedTypeAliases</c> (SG side) — keep in lockstep.
        /// </summary>
        public static readonly HashSet<string> ReservedTypeAliases = new HashSet<string>(StringComparer.Ordinal)
        {
            "Color", "UColor", "EasingFunction", "EasingMode", "BackgroundRepeat",
            "BackgroundPosition", "BackgroundSize", "TransformOrigin", "BackgroundPositionKeyword",
            "BackgroundSizeType", "Repeat", "Length", "StyleKeyword", "TextAutoSizeMode",
            "FilterFunction", "Ratio", "StyleRatio", "MaterialDefinition", "StyleMaterialDefinition",
        };

        /// <summary>
        /// Computes the injected-using payloads for <paramref name="directives"/>' imports (the text
        /// between <c>using </c> and <c>;</c> — e.g. <c>static My.Ns.FooHooks</c> or
        /// <c>Widget = My.Ns.Widget</c>), deduplicated, in import order. Empty when the file has no
        /// imports or none resolve.
        /// </summary>
        public static IReadOnlyList<string> ComputeInjectedUsingPayloads(
            DirectiveSet directives, string uitkxFilePath)
        {
            var result = new List<string>();
            if (directives.Imports.IsDefaultOrEmpty || string.IsNullOrEmpty(uitkxFilePath))
                return result;

            string importerDir = (Path.GetDirectoryName(uitkxFilePath) ?? string.Empty).Replace('\\', '/');
            string rootDir = EffectiveNamespace.UiSourceRootDir(uitkxFilePath) ?? importerDir;

            // The importer's EFFECTIVE namespace drives the same-namespace guards — raw would be
            // wrong for every stamp-less (path-derived) file.
            string? importerNs = EffectiveNamespace.Resolve(
                directives.HasExplicitNamespace, directives.Namespace, uitkxFilePath);

            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var imp in directives.Imports)
            {
                string? target = ImportResolver.MapSpecifierToPath(
                    importerDir, imp.Specifier, rootDir, out _);
                if (target == null || !File.Exists(target))
                    continue;

                DirectiveSet tds;
                try
                {
                    tds = DirectiveParser.Parse(File.ReadAllText(target), target, new List<ParseDiagnostic>());
                }
                catch { continue; }

                // EFFECTIVE namespace of the target — the namespace its generated types actually
                // land in (explicit @namespace wins, else path-derived + config prefix).
                string? tns = EffectiveNamespace.Resolve(
                    tds.HasExplicitNamespace, tds.Namespace, target);
                if (string.IsNullOrEmpty(tns))
                    continue;

                var importedNames = new HashSet<string>(imp.Names, StringComparer.Ordinal);
                bool sameNs = string.Equals(tns, importerNs, StringComparison.Ordinal);

                // Hook: expose the whole owning container. No same-ns skip — a redundant
                // `using static` is harmless (CS0105 is suppressed in generated output).
                if (!tds.HookDeclarations.IsDefaultOrEmpty)
                    foreach (var h in tds.HookDeclarations)
                        if (h.IsExported && importedNames.Contains(h.Name))
                        {
                            string line = $"static {tns}.{DeriveHookContainerClassName(target)}";
                            if (seen.Add(line))
                                result.Add(line);
                            break;
                        }

                // Module + component aliases (same guards as the SG's ResolveInjectedUsings).
                if (!sameNs && !tds.ModuleDeclarations.IsDefaultOrEmpty)
                    foreach (var m in tds.ModuleDeclarations)
                        if (m.IsExported && importedNames.Contains(m.Name)
                            && !ReservedTypeAliases.Contains(m.Name))
                        {
                            string line = $"{m.Name} = {tns}.{m.Name}";
                            if (seen.Add(line))
                                result.Add(line);
                        }

                if (!sameNs && !tds.ComponentDeclarations.IsDefaultOrEmpty)
                    foreach (var c in tds.ComponentDeclarations)
                        if (c.IsExported && importedNames.Contains(c.Name)
                            && !ReservedTypeAliases.Contains(c.Name))
                        {
                            string line = $"{c.Name} = {tns}.{c.Name}";
                            if (seen.Add(line))
                                result.Add(line);
                        }
            }
            return result;
        }

        /// <summary>Container class name from a hook file path — mirror of <c>HookEmitter.DeriveContainerClassName</c> (part before the first dot, PascalCased, + <c>Hooks</c>).</summary>
        public static string DeriveHookContainerClassName(string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            int dot = fileName.IndexOf('.');
            if (dot > 0)
                fileName = fileName.Substring(0, dot);
            if (fileName.Length > 0 && char.IsLower(fileName[0]))
                fileName = char.ToUpper(fileName[0]) + fileName.Substring(1);
            return fileName + "Hooks";
        }
    }
}
