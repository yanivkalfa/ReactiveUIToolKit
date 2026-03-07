using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Security.Cryptography;
using System.Text;
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
    ///   UITKX0005   — missing required @namespace / @component directive (error)
    ///   UITKX0006   — @component name ≠ filename (warning)
    ///   UITKX0008   — unknown PascalCase function component (warning)
    ///   UITKX03xx   — parse errors
    /// </summary>
    public static class UitkxPipeline
    {
        public static UitkxPipelineResult Run(
            string source,
            string filePath,
            Compilation compilation,
            CancellationToken ct
        )
        {
            ct.ThrowIfCancellationRequested();

            string fileName = Path.GetFileName(filePath);
            string hintName = BuildHintName(filePath);

            var parseDiags = new List<ParseDiagnostic>();

            // ── Stage 1: Directive parsing ────────────────────────────────────
            DirectiveSet directives = DirectiveParser.Parse(source, filePath, parseDiags);

            ct.ThrowIfCancellationRequested();

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
            // into both causes CS0436 "conflicts with imported type".
            //
            // Ownership is determined by two checks in priority order:
            //
            //  (A) Companion-file check — the compilation has a SyntaxTree whose
            //      filename stem matches the .uitkx stem (e.g. "UitkxCounterFunc").
            //      This is the canonical Unity convention: every .uitkx has a partner
            //      .cs file in the same folder, and the partner belongs to exactly one
            //      assembly.
            //
            //  (B) Directory check — as a fallback, the compilation has a SyntaxTree
            //      whose resolved directory path matches the .uitkx file's directory.
            //      Uses suffix-based matching to handle both absolute and relative
            //      SyntaxTree.FilePath values (Unity can emit either).
            //
            // NOTE: Unity uses forward-slash paths in SyntaxTree.FilePath while the
            // AdditionalText paths (from UitkxCsprojPostprocessor) use backslashes.
            // We normalise to the OS separator before comparing.
            string uitkxStem = Path.GetFileNameWithoutExtension(filePath);
            string uitkxDir  = NormalizeDir(Path.GetDirectoryName(filePath));
            bool ownedByThisCompilation = false;

            foreach (var tree in compilation.SyntaxTrees)
            {
                if (string.IsNullOrEmpty(tree.FilePath)) continue;

                string treeStem = Path.GetFileNameWithoutExtension(tree.FilePath);
                string treeDir  = NormalizeDir(Path.GetDirectoryName(tree.FilePath));

                // (A) Companion-file match — stem AND same directory.
                // Stem-only matching is intentionally avoided: two .uitkx files with
                // the same component name but in different folders (e.g. Samples/Legacy
                // and Samples/UITKX) both have companion .cs files in the same
                // assembly. Without the directory check both would claim ownership and
                // their generated sources would be bound to the same hint name,
                // causing the second AddSource call to be silently dropped and one
                // component's Render method to never be emitted.
                if (string.Equals(treeStem, uitkxStem, StringComparison.OrdinalIgnoreCase)
                    && DirsMatch(uitkxDir, treeDir))
                {
                    ownedByThisCompilation = true;
                    break;
                }

                // (B) Directory suffix / exact match — fallback when no companion
                // .cs file exists yet (new component, not yet created).
                if (DirsMatch(uitkxDir, treeDir))
                {
                    ownedByThisCompilation = true;
                    break;
                }
            }
            if (!ownedByThisCompilation)
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

            bool missingRequiredDirectives = !directives.IsFunctionStyle
                && (directives.ComponentName == null || directives.Namespace == null);

            if (errorCount > 0 || missingRequiredDirectives)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("// <auto-generated/>");
                sb.AppendLine(
                    $"// UITKX parse errors in {fileName} — fix the .uitkx file to regenerate."
                );
                foreach (var d in diagnostics)
                    if (d.Severity == DiagnosticSeverity.Error)
                        sb.AppendLine(
                            $"#error {d.GetMessage().Replace('\n', ' ').Replace('\r', ' ')}"
                        );
                if (!directives.IsFunctionStyle && directives.ComponentName == null)
                    sb.AppendLine(
                        $"#error UITKX: '{fileName}' is missing a required '@component' directive."
                    );
                if (!directives.IsFunctionStyle && directives.Namespace == null)
                    sb.AppendLine(
                        $"#error UITKX: '{fileName}' is missing a required '@namespace' directive."
                    );

                return new UitkxPipelineResult(
                    HintName: hintName,
                    Source: sb.ToString(),
                    Diagnostics: diagnostics.ToImmutableArray()
                );
            }

            ct.ThrowIfCancellationRequested();

            // ── Stage 3: PropsResolver ────────────────────────────────────────
            var resolver = new PropsResolver(compilation);

            ct.ThrowIfCancellationRequested();

            // ── Stage 3b: Rules-of-Hooks validation ───────────────────────────
            HooksValidator.Validate(rootNodes, filePath, diagnostics);

            // ── Stage 3c: Structural validation ───────────────────────────────
            StructureValidator.Validate(rootNodes, filePath, diagnostics);

            ct.ThrowIfCancellationRequested();

            // ── Stage 4: CSharpEmitter ────────────────────────────────────────
            string generatedSource = CSharpEmitter.Emit(
                filePath,
                directives,
                rootNodes,
                resolver,
                diagnostics
            );

            ct.ThrowIfCancellationRequested();

            return new UitkxPipelineResult(
                HintName: hintName,
                Source: generatedSource,
                Diagnostics: diagnostics.ToImmutableArray()
            );
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static Diagnostic ParseDiagToRoslyn(ParseDiagnostic pd)
        {
            var severity = pd.Severity == ParseSeverity.Error
                ? DiagnosticSeverity.Error
                : pd.Severity == ParseSeverity.Warning
                    ? DiagnosticSeverity.Warning
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
        /// Normalises a directory path for cross-platform comparison.
        /// Converts forward slashes to backslashes (Windows canonical form) and
        /// strips any trailing separator so that two equivalent paths always compare
        /// equal regardless of how Unity or the OS produced them.
        /// </summary>
        private static string NormalizeDir(string? dir)
        {
            if (dir == null) return string.Empty;
            // Use the OS separator everywhere, then strip trailing separators.
            return dir
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar)
                .TrimEnd(Path.DirectorySeparatorChar)
                .TrimEnd(Path.AltDirectorySeparatorChar);
        }

        /// <summary>
        /// Returns true when two normalised directory paths refer to the same
        /// location, handling the case where one path is an absolute form of the
        /// other (Unity can emit relative SyntaxTree.FilePath values while
        /// AdditionalText paths are absolute).
        ///
        /// The suffix rule: "X:\absolute\path\Samples\Components" matches the
        /// relative path "Assets\ReactiveUIToolKit\Samples\Components" because
        /// the longer absolute path ends with the separator-prefixed shorter path.
        /// </summary>
        private static bool DirsMatch(string dir1, string dir2)
        {
            if (string.IsNullOrEmpty(dir1) || string.IsNullOrEmpty(dir2))
                return false;

            // Fast path: exact equality (both absolute on same machine).
            if (string.Equals(dir1, dir2, StringComparison.OrdinalIgnoreCase))
                return true;

            // One path is a relative suffix of the other.
            // Require a path separator immediately before the shorter string
            // starts inside the longer one, to avoid false prefix matches.
            string longer  = dir1.Length >= dir2.Length ? dir1 : dir2;
            string shorter = dir1.Length <  dir2.Length ? dir1 : dir2;

            if (longer.Length > shorter.Length
                && longer.EndsWith(shorter, StringComparison.OrdinalIgnoreCase))
            {
                char sep = longer[longer.Length - shorter.Length - 1];
                if (sep == Path.DirectorySeparatorChar || sep == Path.AltDirectorySeparatorChar)
                    return true;
            }

            return false;
        }

        private static string BuildHintName(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            string normalizedPath = (filePath ?? string.Empty).Replace('\\', '/').ToLowerInvariant();

            using var md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(normalizedPath));
            string suffix = BitConverter.ToString(hash, 0, 6).Replace("-", string.Empty).ToLowerInvariant();

            return $"{fileName}.{suffix}.g.cs";
        }
    }
}
