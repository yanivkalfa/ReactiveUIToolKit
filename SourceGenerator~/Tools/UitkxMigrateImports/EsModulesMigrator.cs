using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUITK.Language;
using ReactiveUITK.Language.Formatter;
using ReactiveUITK.Language.Parser;

namespace ReactiveUITK.SourceGenerator.Tools
{
    /// <summary>
    /// The ES-modules migration pass (Plans~/ES_MODULES_EXECUTION_PLAN.md §7.1, U-09): rewrites
    /// legacy wrapper-keyword files to plain declarations. Pipeline per G-10 (each step a no-op
    /// on already-migrated input, so the whole is idempotent):
    /// <list type="number">
    ///   <item><description>tidy — <see cref="UitkxMigrator.TidyUsings"/>;</description></item>
    ///   <item><description>export-normalize + import-insert — the existing
    ///   <see cref="UitkxMigrator.Migrate(IReadOnlyList{MigratorFile})"/> machinery;</description></item>
    ///   <item><description>wrapper rewrite — <c>component N</c> → <c>VirtualNode N(…)</c>;
    ///   <c>hook useN(p) -&gt; (r)</c> → <c>(r) useN(p)</c>; <c>module M { members }</c> hoisted
    ///   to plain member declarations (Roslyn-parsed);</description></item>
    ///   <item><description>import rewrite — <c>import { M }</c> of an exploded module →
    ///   <c>import * as M</c> (call sites <c>M.x</c> keep working verbatim), plus member imports
    ///   for names that used to resolve via companion partial-merging;</description></item>
    ///   <item><description>companion atomicity — sets (<c>X.uitkx</c> + <c>X.*.uitkx</c>) migrate
    ///   whole or not at all;</description></item>
    ///   <item><description>format — <see cref="AstFormatter"/> last.</description></item>
    /// </list>
    /// The zero-diagnostics gate (§7.1 step 7) is the caller's job (SamplesCorpusGateTests +
    /// VERIFY-UNITY in M7). Files whose shapes the plain dialect cannot express (generic hooks,
    /// modules with properties/nested types/initializer-less fields) are SKIPPED whole-set with a
    /// reported error — they stay legacy and keep compiling under the deprecation window.
    /// </summary>
    public static class EsModulesMigrator
    {
        public static Dictionary<string, string> Migrate(
            IReadOnlyList<MigratorFile> inputFiles, out List<UitkxMigrator.MigrationError> errors)
        {
            errors = new List<UitkxMigrator.MigrationError>();

            // Normalize to LF FIRST, before ANY parse: the parser's Body*Offset values are indices
            // into the text it was handed, and the line-span/slice surgery below reads the same
            // text — a CRLF file parsed raw but sliced normalized shifts every offset (found live:
            // ArgumentOutOfRange on the first CRLF working tree). The final AstFormatter pass
            // outputs LF-only anyway, so LF intake changes nothing downstream.
            var files = new List<MigratorFile>(inputFiles.Count);
            foreach (var inF in inputFiles)
                files.Add(inF with { Text = inF.Text.Replace("\r\n", "\n").Replace("\r", "\n") });

            // ── Step 1+2: tidy + export-normalize/import-insert (existing passes) ──
            // LEGACY files only: the legacy migrator's @namespace stamping (identity freezing)
            // and using-tidy are wrong for already-migrated (file-keyed) files — those pass
            // through byte-untouched, which is also what makes the whole pipeline idempotent.
            var newMode = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var f in files)
            {
                var dsProbe = DirectiveParser.Parse(f.Text, f.AbsPath, new List<ParseDiagnostic>());
                if (!dsProbe.UsesLegacySyntax)
                    newMode.Add(f.AbsPath);
            }

            var tidied = new List<MigratorFile>(files.Count);
            foreach (var f in files)
                tidied.Add(newMode.Contains(f.AbsPath) ? f : f with { Text = UitkxMigrator.TidyUsings(f.Text) });
            var step2 = UitkxMigrator.Migrate(tidied, out var step2Errors, tidyUsings: false, stampNamespace: false);
            errors.AddRange(step2Errors);
            var current = new List<MigratorFile>(tidied.Count);
            foreach (var f in tidied)
                current.Add(!newMode.Contains(f.AbsPath) && step2.TryGetValue(f.AbsPath, out var t)
                    ? f with { Text = t } : f);

            // ── Step 3: per-file wrapper rewrite (attempt) ──────────────────────
            // moduleExports: file → exploded module names (for the importer rewrite).
            // memberExports: file → hoisted member names (for companion member-import insertion).
            var rewritten = new Dictionary<string, string>(StringComparer.Ordinal);
            var failed = new Dictionary<string, string>(StringComparer.Ordinal); // path → reason
            var moduleNamesByFile = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            var memberNamesByFile = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var f in current)
            {
                var ds = DirectiveParser.Parse(f.Text, f.AbsPath, new List<ParseDiagnostic>());
                if (!ds.UsesLegacySyntax)
                {
                    rewritten[f.AbsPath] = f.Text; // already migrated — pass through (idempotence)
                    continue;
                }
                string? reason = TryRewriteFile(f, ds, out string newText, out var modNames, out var memNames);
                if (reason != null)
                {
                    failed[f.AbsPath] = reason;
                    continue;
                }
                rewritten[f.AbsPath] = newText;
                // Keyed by NORMALIZED path: ImportResolver.MapSpecifierToPath returns
                // forward-slash paths, while the CLI hands in Windows backslash AbsPaths —
                // an un-normalized key made every cross-file module lookup a silent miss
                // (found live: `import { Theme }` never became `import * as Theme`, 2301).
                if (modNames.Count > 0) moduleNamesByFile[Norm(f.AbsPath)] = modNames;
                if (memNames.Count > 0) memberNamesByFile[Norm(f.AbsPath)] = memNames;
            }

            // ── Step 5 (before 4 by necessity): companion atomicity ─────────────
            // A set = X.uitkx + X.*.uitkx in one folder. If ANY member failed, the whole set
            // reverts to its step-2 text (all-or-none — matrix row 8 is a broken build).
            var setsBySetKey = current
                .GroupBy(f => SetKey(f.AbsPath), StringComparer.OrdinalIgnoreCase)
                .ToList();
            var migratedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var set in setsBySetKey)
            {
                var members = set.ToList();
                var failures = members.Where(m => failed.ContainsKey(m.AbsPath)).ToList();
                if (failures.Count > 0)
                {
                    foreach (var m in members)
                    {
                        rewritten[m.AbsPath] = m.Text; // revert to step-2 (still-legacy) text
                        moduleNamesByFile.Remove(Norm(m.AbsPath));
                        memberNamesByFile.Remove(Norm(m.AbsPath));
                    }
                    foreach (var m in failures)
                        errors.Add(new UitkxMigrator.MigrationError(m.AbsPath,
                            $"companion set '{set.Key}' left legacy: {failed[m.AbsPath]}"));
                }
                else
                {
                    foreach (var m in members)
                        migratedFiles.Add(Norm(m.AbsPath));
                }
            }

            // ── Step 4: importer rewrite against MIGRATED targets only ──────────
            // `import { M }` where target exploded module M → `import * as M` (a star import of a
            // still-legacy target would be UITKX2109); companion member imports for bare refs.
            var output = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var f in current)
            {
                string text = rewritten[f.AbsPath];
                if (migratedFiles.Contains(Norm(f.AbsPath)))
                    text = RewriteImports(f.AbsPath, text, moduleNamesByFile, memberNamesByFile, migratedFiles);

                // ── Step 6: format last ─────────────────────────────────────────
                if (!string.Equals(text, f.Text, StringComparison.Ordinal)
                    || !string.Equals(text, files.First(x => x.AbsPath == f.AbsPath).Text, StringComparison.Ordinal))
                {
                    try { text = new AstFormatter(FormatterOptions.Default).Format(text, f.AbsPath); }
                    catch { /* never fail the migration on a formatter hiccup */ }
                }

                string original = files.First(x => x.AbsPath == f.AbsPath).Text;
                if (!string.Equals(text, original, StringComparison.Ordinal))
                    output[f.AbsPath] = text;
            }
            return output;
        }

        // ── Per-file wrapper rewrite ─────────────────────────────────────────────

        /// <summary>Null on success (out newText valid); else the skip reason.</summary>
        private static string? TryRewriteFile(
            MigratorFile f, DirectiveSet ds, out string newText,
            out List<string> explodedModuleNames, out List<string> hoistedMemberNames)
        {
            newText = f.Text;
            explodedModuleNames = new List<string>();
            hoistedMemberNames = new List<string>();

            string src = f.Text.Replace("\r\n", "\n");
            var lines = src.Split('\n').ToList();

            // Collect line-span replacements (1-based inclusive), applied bottom-up.
            var replacements = new List<(int StartLine, int EndLine, List<string> NewLines)>();

            // Hooks — regenerate from the parsed record (multi-line headers are span-replaced).
            if (!ds.HookDeclarations.IsDefaultOrEmpty)
                foreach (var h in ds.HookDeclarations)
                {
                    if (!string.IsNullOrEmpty(h.GenericParams))
                        return $"generic hook '{h.Name}' — the plain dialect has no generic declaration heads";
                    int bodyEndLine = LineAt(src, h.BodyEndOffset);
                    int closeLine = FindCloseBraceLine(lines, bodyEndLine);
                    string ret = string.IsNullOrEmpty(h.ReturnType) ? "void" : h.ReturnType!;
                    string paramList = string.Join(", ", h.Params.IsDefaultOrEmpty
                        ? Enumerable.Empty<string>()
                        : h.Params.Select(p => p.DefaultValue != null
                            ? $"{p.Type} {p.Name} = {p.DefaultValue}"
                            : $"{p.Type} {p.Name}"));
                    var body = SliceLines(src, h.BodyStartOffset, h.BodyEndOffset);
                    var repl = new List<string>
                    {
                        $"{(h.IsExported ? "export " : "")}{ret} {h.Name}({paramList}) {{"
                    };
                    repl.AddRange(body);
                    repl.Add("}");
                    replacements.Add((h.DeclarationLine, closeLine, repl));
                }

            // Modules — Roslyn-parse the body, hoist members.
            if (!ds.ModuleDeclarations.IsDefaultOrEmpty)
                foreach (var m in ds.ModuleDeclarations)
                {
                    string? reason = ExplodeModule(m, out var hoisted, out var memberNames);
                    if (reason != null)
                        return $"module '{m.Name}': {reason}";
                    int bodyEndLine = LineAt(src, m.BodyEndOffset);
                    int closeLine = FindCloseBraceLine(lines, bodyEndLine);
                    replacements.Add((m.DeclarationLine, closeLine, hoisted));
                    explodedModuleNames.Add(m.Name);
                    hoistedMemberNames.AddRange(memberNames);
                }

            // Components — keyword surgery on the declaration line (params may wrap; only the
            // keyword token and, for the parameterless form, the missing `()` change).
            if (!ds.ComponentDeclarations.IsDefaultOrEmpty)
                foreach (var c in ds.ComponentDeclarations)
                {
                    int li = c.DeclarationLine - 1;
                    if (li < 0 || li >= lines.Count)
                        return $"component '{c.Name}': declaration line out of range";
                    string line = lines[li];
                    var mkw = Regex.Match(line, @"^(\s*)(export\s+)?component\b");
                    if (!mkw.Success)
                        return $"component '{c.Name}': wrapper keyword not found on its declaration line";
                    string rewrittenLine = Regex.Replace(line, @"^(\s*)(export\s+)?component\b", "$1$2VirtualNode");
                    if (c.FunctionParams.IsDefaultOrEmpty)
                    {
                        // Parameterless keeps empty parens: `component N {` → `VirtualNode N() {`.
                        rewrittenLine = Regex.Replace(
                            rewrittenLine,
                            $@"\b{Regex.Escape(c.Name)}\b(?!\s*\()",
                            c.Name + "()");
                    }
                    replacements.Add((c.DeclarationLine, c.DeclarationLine, new List<string> { rewrittenLine }));
                }

            // Apply bottom-up so earlier line numbers stay valid.
            foreach (var (start, end, newLines) in replacements.OrderByDescending(r => r.StartLine))
            {
                lines.RemoveRange(start - 1, end - start + 1);
                lines.InsertRange(start - 1, newLines);
            }

            newText = string.Join("\n", lines);
            if (!newText.EndsWith("\n", StringComparison.Ordinal))
                newText += "\n";
            return null;
        }

        /// <summary>
        /// Hoists a legacy module body's members to plain declarations. Null on success;
        /// else the reason the shape cannot be expressed in the plain dialect.
        /// </summary>
        private static string? ExplodeModule(
            ModuleDeclaration m, out List<string> hoisted, out List<string> memberNames)
        {
            hoisted = new List<string>();
            memberNames = new List<string>();

            var tree = CSharpSyntaxTree.ParseText("class __W {\n" + m.Body + "\n}");
            var errorsInBody = tree.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            if (errorsInBody.Count > 0)
                return $"body does not parse as C# members ({errorsInBody[0].GetMessage()})";
            var cls = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            if (cls == null)
                return "no members found";

            foreach (var member in cls.Members)
            {
                switch (member)
                {
                    case FieldDeclarationSyntax field:
                    {
                        bool isPublic = field.Modifiers.Any(SyntaxKind.PublicKeyword)
                            || !field.Modifiers.Any(mod =>
                                mod.IsKind(SyntaxKind.PrivateKeyword) || mod.IsKind(SyntaxKind.InternalKeyword)
                                || mod.IsKind(SyntaxKind.ProtectedKeyword));
                        string type = field.Declaration.Type.ToString();
                        foreach (var v in field.Declaration.Variables)
                        {
                            if (v.Initializer == null)
                                return $"field '{v.Identifier.Text}' has no initializer — plain values require '= …'";
                            string prefix = isPublic ? "export " : "";
                            hoisted.Add($"{prefix}{type} {v.Identifier.Text} = {v.Initializer.Value};");
                            memberNames.Add(v.Identifier.Text);
                        }
                        break;
                    }
                    case MethodDeclarationSyntax method:
                    {
                        if (method.TypeParameterList != null)
                            return $"generic method '{method.Identifier.Text}' — the plain dialect has no generic declaration heads";
                        bool isPublic = method.Modifiers.Any(SyntaxKind.PublicKeyword)
                            || !method.Modifiers.Any(mod =>
                                mod.IsKind(SyntaxKind.PrivateKeyword) || mod.IsKind(SyntaxKind.InternalKeyword)
                                || mod.IsKind(SyntaxKind.ProtectedKeyword));
                        string prefix = isPublic ? "export " : "";
                        string ret = method.ReturnType.ToString();
                        string plist = method.ParameterList.Parameters.ToFullString().Trim();
                        if (method.ExpressionBody != null)
                        {
                            hoisted.Add($"{prefix}{ret} {method.Identifier.Text}({plist}) => {method.ExpressionBody.Expression};");
                        }
                        else if (method.Body != null)
                        {
                            hoisted.Add($"{prefix}{ret} {method.Identifier.Text}({plist}) {{");
                            foreach (var bodyLine in DedentBlock(method.Body))
                                hoisted.Add(bodyLine);
                            hoisted.Add("}");
                        }
                        else
                        {
                            return $"method '{method.Identifier.Text}' has no body";
                        }
                        memberNames.Add(method.Identifier.Text);
                        break;
                    }
                    default:
                        return $"member kind '{member.Kind()}' cannot be hoisted to a plain declaration "
                            + "(properties, nested types, events, and constructors have no plain-dialect form)";
                }
                hoisted.Add(string.Empty);
            }
            if (hoisted.Count > 0 && hoisted[^1].Length == 0)
                hoisted.RemoveAt(hoisted.Count - 1);
            return null;
        }

        private static IEnumerable<string> DedentBlock(BlockSyntax body)
        {
            var inner = body.Statements.ToFullString().Replace("\r\n", "\n").TrimEnd('\n');
            foreach (var line in inner.Split('\n'))
                yield return line.TrimEnd();
        }

        // ── Importer rewrite (step 4) ────────────────────────────────────────────

        private static string RewriteImports(
            string absPath,
            string text,
            Dictionary<string, List<string>> moduleNamesByFile,
            Dictionary<string, List<string>> memberNamesByFile,
            HashSet<string> migratedFiles)
        {
            var ds = DirectiveParser.Parse(text, absPath, new List<ParseDiagnostic>());
            if (ds.Imports.IsDefaultOrEmpty && memberNamesByFile.Count == 0)
                return text;

            string importerDir = Norm(System.IO.Path.GetDirectoryName(absPath) ?? string.Empty);
            var lines = text.Replace("\r\n", "\n").Split('\n').ToList();

            // `import { M, x } from spec` → star import when M is an exploded module of the
            // (migrated) target; the other names keep a named import line.
            if (!ds.Imports.IsDefaultOrEmpty)
                foreach (var imp in ds.Imports.OrderByDescending(i => i.Line))
                {
                    if (imp.IsStar || imp.IsDefault)
                        continue;
                    string? target = ImportResolver.MapSpecifierToPath(importerDir, imp.Specifier, importerDir, out _);
                    if (target == null || !migratedFiles.Contains(Norm(target))
                        || !moduleNamesByFile.TryGetValue(Norm(target), out var modNames))
                        continue;

                    var starNames = imp.Names.Where(n => modNames.Contains(n)).ToList();
                    if (starNames.Count == 0)
                        continue;
                    var namedNames = imp.Names.Where(n => !modNames.Contains(n)).ToList();

                    var repl = new List<string>();
                    foreach (var sn in starNames)
                        repl.Add($"import * as {sn} from \"{imp.Specifier}\"");
                    if (namedNames.Count > 0)
                        repl.Add($"import {{ {string.Join(", ", namedNames)} }} from \"{imp.Specifier}\"");
                    lines.RemoveAt(imp.Line - 1);
                    lines.InsertRange(imp.Line - 1, repl);
                }

            // Companion member imports: bare references that used to resolve via companion
            // partial-merging (same folder, `{Stem}.*.uitkx` name-prefix — the exact merge
            // candidates). Only for migrated companions (member imports of a legacy module
            // target would be UITKX2301).
            string stem = StemOf(absPath);
            var neededBySpec = new SortedDictionary<string, SortedSet<string>>(StringComparer.Ordinal);
            string scrub = ScrubForScan(string.Join("\n", lines));
            foreach (var kv in memberNamesByFile)
            {
                if (!migratedFiles.Contains(kv.Key)) continue;
                if (string.Equals(kv.Key, Norm(absPath), StringComparison.OrdinalIgnoreCase)) continue;
                if (!string.Equals(Norm(System.IO.Path.GetDirectoryName(kv.Key) ?? ""), importerDir, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (!StemOf(kv.Key).StartsWith(stem + ".", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(StemOf(kv.Key), stem, StringComparison.OrdinalIgnoreCase))
                    continue;

                var already = new HashSet<string>(StringComparer.Ordinal);
                var dsNow = DirectiveParser.Parse(string.Join("\n", lines), absPath, new List<ParseDiagnostic>());
                if (!dsNow.Imports.IsDefaultOrEmpty)
                    foreach (var imp in dsNow.Imports)
                        foreach (var n in imp.Names)
                            already.Add(n);

                foreach (var name in kv.Value)
                {
                    if (already.Contains(name)) continue;
                    if (!Regex.IsMatch(scrub, $@"\b{Regex.Escape(name)}\b")) continue;
                    string spec = UitkxMigrator.RelativeSpecifier(absPath, kv.Key);
                    if (!neededBySpec.TryGetValue(spec, out var set))
                        neededBySpec[spec] = set = new SortedSet<string>(StringComparer.Ordinal);
                    set.Add(name);
                }
            }
            if (neededBySpec.Count > 0)
            {
                int insertAt = 0;
                var dsNow = DirectiveParser.Parse(string.Join("\n", lines), absPath, new List<ParseDiagnostic>());
                if (!dsNow.Imports.IsDefaultOrEmpty)
                    insertAt = dsNow.Imports.Max(i => i.Line); // after the last import line (1-based → insert index)
                var newLines = neededBySpec
                    .Select(kv2 => $"import {{ {string.Join(", ", kv2.Value)} }} from \"{kv2.Key}\"")
                    .ToList();
                lines.InsertRange(insertAt, newLines);
            }

            string result = string.Join("\n", lines);
            if (!result.EndsWith("\n", StringComparison.Ordinal))
                result += "\n";
            return result;
        }

        // ── Utilities ────────────────────────────────────────────────────────────

        private static string SetKey(string absPath)
        {
            string dir = Norm(System.IO.Path.GetDirectoryName(absPath) ?? string.Empty);
            string stem = StemOf(absPath);
            int dot = stem.IndexOf('.');
            if (dot > 0) stem = stem.Substring(0, dot);
            return dir + "/" + stem;
        }

        private static string StemOf(string absPath)
            => System.IO.Path.GetFileNameWithoutExtension(absPath);

        private static string Norm(string p) => p.Replace('\\', '/').TrimEnd('/');

        private static int LineAt(string src, int offset)
        {
            int line = 1;
            int end = Math.Min(offset, src.Length);
            for (int i = 0; i < end; i++)
                if (src[i] == '\n') line++;
            return line;
        }

        /// <summary>The line of the declaration's closing <c>}</c>: the first line at or after
        /// <paramref name="bodyEndLine"/> whose first non-space char is <c>}</c> (the parser's
        /// BodyEndOffset points just before it, possibly on the same line).</summary>
        private static int FindCloseBraceLine(List<string> lines, int bodyEndLine)
        {
            for (int i = bodyEndLine - 1; i < lines.Count; i++)
                if (lines[i].TrimStart().StartsWith("}", StringComparison.Ordinal))
                    return i + 1;
            return bodyEndLine;
        }

        private static List<string> SliceLines(string src, int startOffset, int endOffset)
        {
            string slice = src.Substring(startOffset, Math.Max(0, endOffset - startOffset))
                .TrimEnd('\n', ' ', '\t');
            return slice.Split('\n').Select(l => l.TrimEnd()).ToList();
        }

        private static string ScrubForScan(string text)
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
    }
}
