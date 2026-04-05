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
            CancellationToken ct,
            ImmutableArray<PeerComponentInfo>? peerComponents = null
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

            // ── Stage 3: PropsResolver ────────────────────────────────────────
            var resolver = new PropsResolver(compilation, peerComponents);

            ct.ThrowIfCancellationRequested();

            // ── Stage 3b: Rules-of-Hooks validation ───────────────────────────
            HooksValidator.Validate(rootNodes, filePath, diagnostics);

            // ── Stage 3c: Structural validation ───────────────────────────────
            StructureValidator.Validate(rootNodes, filePath, diagnostics, directives);

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
