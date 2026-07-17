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
            // wrong for every stamp-less (path-derived) file. Mode-aware (U-01): a new-syntax
            // importer is file-keyed.
            string? importerNs = EffectiveNamespace.Resolve(
                directives.HasExplicitNamespace, directives.Namespace, uitkxFilePath,
                fileKeyed: !directives.UsesLegacySyntax);

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
                // land in (explicit @namespace wins, else path-derived + config prefix). Mode from
                // the TARGET's own parse (U-01): a migrated target is file-keyed even when the
                // importer is legacy.
                string? tns = EffectiveNamespace.Resolve(
                    tds.HasExplicitNamespace, tds.Namespace, target,
                    fileKeyed: !tds.UsesLegacySyntax);
                if (string.IsNullOrEmpty(tns))
                    continue;

                var importedNames = new HashSet<string>(imp.Names, StringComparer.Ordinal);
                bool sameNs = string.Equals(tns, importerNs, StringComparison.Ordinal);

                if (!tds.UsesLegacySyntax)
                {
                    // ── New-mode target (ES-modules campaign, U-03 lowering table) ──
                    // Members (values/utils/hooks) live on the target's per-file `__Exports`
                    // container. Aliased member imports get typed BRIDGES (emitter-side, not
                    // usings); aliased component imports are plain alias-renames.
                    var aliasesN = imp.Aliases.IsDefaultOrEmpty
                        ? System.Collections.Immutable.ImmutableArray<string?>.Empty : imp.Aliases;
                    bool AliasedAt(int k) => k < aliasesN.Length && aliasesN[k] != null;

                    if (imp.IsStar && imp.StarAlias != null)
                    {
                        // `import * as X` → alias-to-type of the whole exports container.
                        if (!ReservedTypeAliases.Contains(imp.StarAlias))
                        {
                            string line = $"{imp.StarAlias} = {tns}.__Exports";
                            if (seen.Add(line))
                                result.Add(line);
                        }
                        continue;
                    }

                    if (imp.IsDefault && imp.DefaultAlias != null)
                    {
                        // Default import: component default → alias to the component type;
                        // member default → bridge (emitter-side, no using here).
                        string? defName = tds.DefaultExportName;
                        if (defName != null
                            && !tds.ComponentDeclarations.IsDefaultOrEmpty
                            && System.Linq.Enumerable.Any(tds.ComponentDeclarations, c => c.Name == defName)
                            && !ReservedTypeAliases.Contains(imp.DefaultAlias))
                        {
                            string line = $"{imp.DefaultAlias} = {tns}.{defName}";
                            if (seen.Add(line))
                                result.Add(line);
                        }
                        continue;
                    }

                    bool exportsContainerAdded = false;
                    for (int k = 0; k < imp.Names.Length; k++)
                    {
                        string name = imp.Names[k];

                        bool isComponent = !tds.ComponentDeclarations.IsDefaultOrEmpty
                            && System.Linq.Enumerable.Any(tds.ComponentDeclarations, c => c.IsExported && c.Name == name);
                        if (isComponent)
                        {
                            string bound = AliasedAt(k) ? aliasesN[k]! : name;
                            if (!ReservedTypeAliases.Contains(bound))
                            {
                                string line = $"{bound} = {tns}.{name}";
                                if (seen.Add(line))
                                    result.Add(line);
                            }
                            continue;
                        }

                        bool isMember = !tds.MemberDeclarations.IsDefaultOrEmpty
                            && System.Linq.Enumerable.Any(tds.MemberDeclarations, m => m.IsExported && m.Name == name);
                        if (isMember && !AliasedAt(k) && !exportsContainerAdded)
                        {
                            // Un-aliased member import → the whole container once per target
                            // file (C# has no per-member static import; per-name strictness
                            // stays a uitkx diagnostic, same rationale as the legacy hook path).
                            string line = $"static {tns}.__Exports";
                            if (seen.Add(line))
                                result.Add(line);
                            exportsContainerAdded = true;
                        }
                        // Aliased members → bridge emission (M3, consumer's __Exports) — no using.
                    }
                    continue;
                }

                // ── Legacy-mode target: today's payloads exactly (deprecation window) ──

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

        /// <summary>
        /// Imported-member bridge surface (ES-modules campaign, U-03): for every `as`-renamed or
        /// default MEMBER import (values/utils/hooks — components alias via usings instead), the
        /// bound name + signature the consumer's <c>__Exports</c> re-states. Consumed by the LSP
        /// virtual doc (scaffold stubs so the bound name resolves in setup code); the SG computes
        /// the equivalent from its pre-scan PeerExportsInfo table (no file IO in the incremental
        /// pipeline) — shape parity is pinned by tests. Filesystem-parses each target like
        /// <see cref="ComputeInjectedUsingPayloads"/>; degrades silently on unresolvable targets.
        /// </summary>
        public static IReadOnlyList<(string Alias, string? ReturnType, string? ParamsText, bool IsValue)>
            ComputeImportedMemberBridges(DirectiveSet directives, string uitkxFilePath)
        {
            var result = new List<(string, string?, string?, bool)>();
            if (directives.Imports.IsDefaultOrEmpty || string.IsNullOrEmpty(uitkxFilePath))
                return result;

            string importerDir = (Path.GetDirectoryName(uitkxFilePath) ?? string.Empty).Replace('\\', '/');
            string rootDir = EffectiveNamespace.UiSourceRootDir(uitkxFilePath) ?? importerDir;

            foreach (var imp in directives.Imports)
            {
                bool anyAlias = !imp.Aliases.IsDefaultOrEmpty && System.Linq.Enumerable.Any(imp.Aliases, a => a != null);
                if (!anyAlias && !imp.IsDefault)
                    continue;

                string? target = ImportResolver.MapSpecifierToPath(importerDir, imp.Specifier, rootDir, out _);
                if (target == null || !File.Exists(target))
                    continue;

                DirectiveSet tds;
                try
                {
                    tds = DirectiveParser.Parse(File.ReadAllText(target), target, new List<ParseDiagnostic>());
                }
                catch { continue; }
                if (tds.UsesLegacySyntax || tds.MemberDeclarations.IsDefaultOrEmpty)
                    continue;

                MemberDeclaration? Find(string name)
                {
                    foreach (var m in tds.MemberDeclarations)
                        if (m.IsExported && m.Name == name)
                            return m;
                    return null;
                }

                if (imp.IsDefault && imp.DefaultAlias != null && tds.DefaultExportName != null)
                {
                    bool defaultIsComponent = !tds.ComponentDeclarations.IsDefaultOrEmpty
                        && System.Linq.Enumerable.Any(tds.ComponentDeclarations, c => c.Name == tds.DefaultExportName);
                    if (!defaultIsComponent)
                    {
                        var dm = Find(tds.DefaultExportName);
                        if (dm != null)
                            result.Add((imp.DefaultAlias,
                                dm.ReturnTypeText ?? ExtractNewInitializerTypeName(dm.BodyText),
                                dm.ParamsText, dm.Kind == DeclKind.Value));
                    }
                    continue;
                }

                if (!anyAlias)
                    continue;
                for (int k = 0; k < imp.Names.Length && k < imp.Aliases.Length; k++)
                {
                    string? alias = imp.Aliases[k];
                    if (alias == null) continue;
                    var m = Find(imp.Names[k]);
                    if (m != null)
                        result.Add((alias,
                            m.ReturnTypeText ?? ExtractNewInitializerTypeName(m.BodyText),
                            m.ParamsText, m.Kind == DeclKind.Value));
                }
            }
            return result;
        }

        /// <summary>
        /// Rewrites a using payload for emission INSIDE a namespace block (ES-modules campaign,
        /// M7): inside-namespace usings resolve RELATIVE to the enclosing namespaces, so
        /// <c>using Samples.X;</c> inside <c>ReactiveUITK.Samples…</c> re-resolves `Samples`
        /// against <c>ReactiveUITK.Samples</c> and breaks (found live). <c>global::</c> makes the
        /// payload absolute: plain → <c>global::Ns</c>; <c>static T</c> → <c>static global::T</c>;
        /// <c>A = T</c> → <c>A = global::T</c>. Idempotent on already-global payloads.
        /// </summary>
        public static string GlobalizeUsingPayload(string payload)
        {
            string p = payload.Trim();
            if (p.StartsWith("static ", StringComparison.Ordinal))
            {
                string rest = p.Substring("static ".Length).TrimStart();
                return rest.StartsWith("global::", StringComparison.Ordinal)
                    ? p : "static global::" + rest;
            }
            int eq = p.IndexOf('=');
            if (eq > 0)
            {
                string left = p.Substring(0, eq).TrimEnd();
                string right = p.Substring(eq + 1).TrimStart();
                return right.StartsWith("global::", StringComparison.Ordinal)
                    ? p : left + " = global::" + right;
            }
            return p.StartsWith("global::", StringComparison.Ordinal) ? p : "global::" + p;
        }

        /// <summary>Extracts <c>T</c> from a <c>new T {...}</c>/<c>new T(...)</c> initializer
        /// (G-04 inference sugar) — the language-lib single source; the SG's ExportsEmitter keeps
        /// a thin call-through, HMR keeps a reflective-world mirror pinned by contract test.</summary>
        public static string? ExtractNewInitializerTypeName(string initText)
        {
            if (string.IsNullOrEmpty(initText))
                return null;
            string t = initText.TrimStart();
            if (!t.StartsWith("new", StringComparison.Ordinal))
                return null;
            int p = 3;
            while (p < t.Length && (t[p] == ' ' || t[p] == '\t')) p++;
            int start = p;
            int depth = 0;
            while (p < t.Length)
            {
                char c = t[p];
                if (c == '<') { depth++; p++; continue; }
                if (c == '>') { depth--; p++; continue; }
                if (depth == 0 && (c == '{' || c == '(' || c == ' ' || c == '\t' || c == '\r' || c == '\n'))
                    break;
                if (depth == 0 && c == '[')
                {
                    while (p < t.Length && t[p] != ']') p++;
                    if (p < t.Length) p++;
                    continue;
                }
                p++;
            }
            string name = t.Substring(start, p - start).Trim();
            return name.Length > 0 ? name : null;
        }

        /// <summary>
        /// Mode-aware container name (ES-modules campaign, U-06): a NEW-mode target's members all
        /// live on the per-file <c>__Exports</c> container; a legacy target keeps the historical
        /// <c>{Stem}Hooks</c> naming. Additive overload (U-12) — the 1-arg form below stays for
        /// legacy callers and the HMR reflection seam.
        /// </summary>
        public static string DeriveHookContainerClassName(string filePath, bool targetUsesLegacySyntax)
            => targetUsesLegacySyntax ? DeriveHookContainerClassName(filePath) : "__Exports";

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
