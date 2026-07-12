using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUITK.Language.Parser;

namespace ReactiveUITK.Language
{
    /// <summary>
    /// The strict-mode reference detector (import/export grammar, leg 3, plan §6 "strict detector").
    /// A purely syntactic name-table match (NOT semantic): over a file's markup tags and setup/attr
    /// expressions it finds references to names that OTHER files in the same asmdef export, and
    /// reports the frozen family diagnostics when the reference is not backed by an <c>import</c>:
    /// <list type="bullet">
    ///   <item><description><b>UITKX2305</b> — the name is exported by a peer file but not imported
    ///   (carries the exact <c>import { … } from "…"</c> line to add).</description></item>
    ///   <item><description><b>UITKX2307</b> — a <c>useX()</c> hook-shaped call resolves to no peer
    ///   export and is not an ambient/builtin hook (hand-written C# hooks are exempt, A4).</description></item>
    /// </list>
    /// Pure and host-agnostic (filesystem + builtin allowlists injected) so it is shared by the source
    /// generator (build) and the LSP (live editor diagnostics) and is directly unit-testable.
    /// </summary>
    public static class StrictImportDetector
    {
        public enum ExportKind { Component, Hook, Module }

        /// <summary>An exported name in a peer file of the same asmdef.</summary>
        public sealed record PeerExport(string Name, string TargetFilePath, ExportKind Kind);

        /// <summary>One strict-mode finding: the frozen code, the rendered message, and its source line.</summary>
        public sealed record Finding(string Code, string Message, int Line);

        // <TagName ...   — a markup component tag (PascalCase peers only matter; builtins are lowercase/known).
        private static readonly System.Text.RegularExpressions.Regex s_tagRe =
            new(@"<\s*([A-Za-z_][A-Za-z0-9_]*)", System.Text.RegularExpressions.RegexOptions.Compiled);
        // useX( — a hook-shaped call (frozen scan shape \buse[A-Z_]\w*\s*\().
        private static readonly System.Text.RegularExpressions.Regex s_hookRe =
            new(@"\b(use[A-Z_][A-Za-z0-9_]*)\s*\(", System.Text.RegularExpressions.RegexOptions.Compiled);
        // Module. — a module member access.
        private static readonly System.Text.RegularExpressions.Regex s_moduleRe =
            new(@"\b([A-Za-z_][A-Za-z0-9_]*)\s*\.", System.Text.RegularExpressions.RegexOptions.Compiled);

        /// <summary>
        /// Detect unimported references in <paramref name="importerFilePath"/>'s code.
        /// <paramref name="scannableCode"/> is the file's setup + markup text (string/comment content
        /// is blanked out by the caller). <paramref name="peerExports"/> are the exports of OTHER files
        /// in the same asmdef. <paramref name="isBuiltinHook"/> returns true for ambient/builtin hooks
        /// (e.g. <c>useState</c>) that need no import. Line numbers are approximate (1 when unknown) —
        /// callers that need precise anchors can enrich later.
        /// </summary>
        public static List<Finding> Detect(
            DirectiveSet directives,
            string importerFilePath,
            string scannableCode,
            IReadOnlyList<PeerExport> peerExports,
            Func<string, bool> isBuiltinHook)
        {
            var findings = new List<Finding>();
            if (peerExports.Count == 0)
                return findings;

            // Names this file already imports (any specifier) — those are satisfied.
            var imported = new HashSet<string>(StringComparer.Ordinal);
            if (!directives.Imports.IsDefaultOrEmpty)
                foreach (var imp in directives.Imports)
                    foreach (var n in imp.Names)
                        imported.Add(n);

            // Names declared locally in THIS file — never need importing.
            var selfNames = new HashSet<string>(StringComparer.Ordinal);
            if (!directives.ComponentDeclarations.IsDefaultOrEmpty)
                foreach (var c in directives.ComponentDeclarations) selfNames.Add(c.Name);
            if (!string.IsNullOrEmpty(directives.ComponentName)) selfNames.Add(directives.ComponentName!);
            if (!directives.HookDeclarations.IsDefaultOrEmpty)
                foreach (var h in directives.HookDeclarations) selfNames.Add(h.Name);
            if (!directives.ModuleDeclarations.IsDefaultOrEmpty)
                foreach (var m in directives.ModuleDeclarations) selfNames.Add(m.Name);

            // Index peer exports by (kind, name).
            var byKind = new Dictionary<ExportKind, Dictionary<string, PeerExport>>
            {
                [ExportKind.Component] = new(StringComparer.Ordinal),
                [ExportKind.Hook] = new(StringComparer.Ordinal),
                [ExportKind.Module] = new(StringComparer.Ordinal),
            };
            foreach (var pe in peerExports)
                byKind[pe.Kind][pe.Name] = pe;

            var reported = new HashSet<string>(StringComparer.Ordinal);

            void Report(string code, string message, int line, string dedupeKey)
            {
                if (reported.Add(dedupeKey))
                    findings.Add(new Finding(code, message, line));
            }

            string importerDir = DirOf(importerFilePath);

            void FlagUnimportedPeer(string name, ExportKind kind, int line)
            {
                if (selfNames.Contains(name) || imported.Contains(name)) return;
                if (!byKind[kind].TryGetValue(name, out var pe)) return;
                string spec = RelativeSpecifier(importerDir, pe.TargetFilePath);
                Report("UITKX2305",
                    $"`{name}` is defined in {ShortName(pe.TargetFilePath)} but not imported — add: import {{ {name} }} from \"{spec}\"",
                    line, "2305:" + name);
            }

            foreach (System.Text.RegularExpressions.Match m in s_tagRe.Matches(scannableCode))
                FlagUnimportedPeer(m.Groups[1].Value, ExportKind.Component, LineAt(scannableCode, m.Index));

            foreach (System.Text.RegularExpressions.Match m in s_moduleRe.Matches(scannableCode))
                FlagUnimportedPeer(m.Groups[1].Value, ExportKind.Module, LineAt(scannableCode, m.Index));

            foreach (System.Text.RegularExpressions.Match m in s_hookRe.Matches(scannableCode))
            {
                string name = m.Groups[1].Value;
                if (selfNames.Contains(name) || imported.Contains(name)) continue;
                if (byKind[ExportKind.Hook].ContainsKey(name))
                {
                    FlagUnimportedPeer(name, ExportKind.Hook, LineAt(scannableCode, m.Index));
                }
                else if (!isBuiltinHook(name))
                {
                    // In no export table and not ambient/builtin → strict "used like a hook but no
                    // file exports it". Hand-written C# hooks are exempt (they resolve ambiently),
                    // so this only fires when the name matches no known hook at all.
                    Report("UITKX2307",
                        $"`{name}` is used like a uitkx component/hook but no file exports it",
                        LineAt(scannableCode, m.Index), "2307:" + name);
                }
            }

            return findings;
        }

        /// <summary>
        /// Validate a file's <c>import</c> declarations (plan §6): resolve each specifier and report
        /// the frozen family diagnostics — 2300 (unresolvable / engine-native), 2308 (crosses an
        /// asmdef boundary), 2314 (<c>~/</c> escapes the root), and 2301 (the name is not exported by
        /// the resolved target). Complementary to <see cref="Detect"/> (which flags references that
        /// have no import); this flags imports that don't resolve. Filesystem + asmdef + export
        /// lookups are injected so it is pure and unit-testable.
        /// </summary>
        public static List<Finding> ValidateImports(
            DirectiveSet directives,
            string importerDir,
            string rootDir,
            string? importerAsmdef,
            Func<string, bool> fileExists,
            Func<string, string?> owningAsmdefOf,
            Func<string, string, bool> isExportedByFile)
        {
            var findings = new List<Finding>();
            if (directives.Imports.IsDefaultOrEmpty)
                return findings;

            foreach (var imp in directives.Imports)
            {
                var res = ImportResolver.Resolve(
                    importerDir, imp.Specifier, rootDir, fileExists, owningAsmdefOf, importerAsmdef);

                switch (res.Status)
                {
                    case ImportResolveStatus.UnknownSpecifier:
                        findings.Add(new Finding("UITKX2300",
                            $"unknown import specifier `{imp.Specifier}` — no file at {imp.Specifier}(.uitkx)",
                            imp.Line));
                        break;
                    case ImportResolveStatus.CrossesBoundary:
                        findings.Add(new Finding("UITKX2308",
                            $"import crosses a module/root boundary ({importerAsmdef} -> {owningAsmdefOf(res.ProjectRelativePath ?? string.Empty)}) — imports are module-scoped in v1",
                            imp.Line));
                        break;
                    case ImportResolveStatus.RootEscape:
                        findings.Add(new Finding("UITKX2314",
                            $"'~/' root is not configured or resolves outside the project ('{imp.Specifier}')",
                            imp.Line));
                        break;
                    case ImportResolveStatus.Ok:
                        foreach (var name in imp.Names)
                            if (!isExportedByFile(name, res.ProjectRelativePath!))
                                findings.Add(new Finding("UITKX2301",
                                    $"`{name}` is not exported by {ShortName(res.ProjectRelativePath!)} — add `export` to its declaration",
                                    imp.Line));
                        break;
                }
            }
            return findings;
        }

        /// <summary>Blank C# string/char literals + comments (offset-preserving) so the scan ignores them.</summary>
        public static string ScrubNonCode(string text)
        {
            var sb = new StringBuilder(text);
            int i = 0;
            while (i < text.Length)
            {
                int start = i;
                if (CSharpLexFacts.TrySkipNonCode(text, ref i, text.Length) && i > start)
                {
                    for (int k = start; k < i && k < sb.Length; k++)
                        if (!char.IsWhiteSpace(sb[k])) sb[k] = ' ';
                    continue;
                }
                i++;
            }
            return sb.ToString();
        }

        /// <summary>1-based line of <paramref name="index"/> in <paramref name="text"/>.</summary>
        private static int LineAt(string text, int index)
        {
            int line = 1;
            int end = Math.Min(index, text.Length);
            for (int i = 0; i < end; i++)
                if (text[i] == '\n') line++;
            return line;
        }

        private static string ShortName(string path)
        {
            string p = path.Replace('\\', '/');
            int slash = p.LastIndexOf('/');
            return slash >= 0 ? p.Substring(slash + 1) : p;
        }

        private static string DirOf(string path)
        {
            string p = path.Replace('\\', '/').TrimEnd('/');
            int slash = p.LastIndexOf('/');
            return slash >= 0 ? p.Substring(0, slash) : string.Empty;
        }

        /// <summary><c>./</c>-relative extensionless specifier from importer dir to target file.</summary>
        internal static string RelativeSpecifier(string fromDir, string toFileAbs)
        {
            string to = toFileAbs.Replace('\\', '/');
            if (to.EndsWith(".uitkx", StringComparison.OrdinalIgnoreCase))
                to = to.Substring(0, to.Length - ".uitkx".Length);

            var fromSegs = fromDir.Length == 0 ? new List<string>() : new List<string>(fromDir.Split('/'));
            var toSegs = new List<string>(to.Split('/'));

            int common = 0;
            while (common < fromSegs.Count && common < toSegs.Count - 1 &&
                   string.Equals(fromSegs[common], toSegs[common], StringComparison.OrdinalIgnoreCase))
                common++;

            var rel = new List<string>();
            for (int i = common; i < fromSegs.Count; i++) rel.Add("..");
            for (int i = common; i < toSegs.Count; i++) rel.Add(toSegs[i]);

            string result = string.Join("/", rel);
            if (!result.StartsWith("../", StringComparison.Ordinal) && result != "..")
                result = "./" + result;
            return result;
        }
    }
}
