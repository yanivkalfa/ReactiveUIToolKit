using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests
{
    /// <summary>
    /// The floor-Unity store gate, run locally: executes the REAL generator over the repo's
    /// actual bundled Samples via the disk-scan path (assembly <c>ReactiveUITK.Samples</c>,
    /// project root walked up from a syntax-tree path) — exactly what a fresh Unity import
    /// does in the Asset-Store CI's <c>build-unitypackage</c> job. Asserts the strict-import
    /// pipeline produces ZERO errors over the whole corpus.
    ///
    /// This is the test that would have caught the UITKX2307 builtin-hook storm (camelCase
    /// aliases missing from the exemption set) and the GameScreen UITKX2305 that CI's real
    /// Unity compile surfaced — the tiny synthetic fixtures never combined "peer exports
    /// present" with "builtin hooks called".
    /// </summary>
    public sealed class SamplesCorpusGateTests
    {
        private const string StubSource = """
            namespace ReactiveUITK.Core
            {
                public abstract class VirtualNode { }
            }
            """;

        [Fact]
        public void BundledSamples_GenerateWithoutErrors()
        {
            string? packageRoot = FindPackageRoot();
            Assert.False(packageRoot is null,
                "Could not locate the package root (a directory containing Samples/ and SourceGenerator~/) above the test assembly.");

            // Synthesize a Unity-shaped project in temp: <root>/Assets/Samples/**. Locally the
            // package happens to live inside a real Unity project, but on CI the repo checkout
            // IS the package root — there is no Assets/ ancestor to discover — so the gate
            // builds its own layout instead of finding one. Copying only *.uitkx + *.asmdef is
            // faithful: the generator's disk scan reads exactly those (namespace derivation
            // anchors at the copied Samples asmdef, so relative segments are unchanged).
            string tempRoot = Path.Combine(
                Path.GetTempPath(), "uitkx-corpus-" + Guid.NewGuid().ToString("N"));
            try
            {
                string samplesSrc = Path.Combine(packageRoot!, "Samples");
                string samplesDst = Path.Combine(tempRoot, "Assets", "Samples");
                foreach (string src in Directory.EnumerateFiles(samplesSrc, "*.*", SearchOption.AllDirectories))
                {
                    if (!src.EndsWith(".uitkx", StringComparison.OrdinalIgnoreCase)
                        && !src.EndsWith(".asmdef", StringComparison.OrdinalIgnoreCase))
                        continue;
                    string rel = src.Substring(samplesSrc.Length).TrimStart('\\', '/');
                    string dst = Path.Combine(samplesDst, rel);
                    Directory.CreateDirectory(Path.GetDirectoryName(dst)!);
                    File.Copy(src, dst);
                }

                // A stub tree whose path sits in the temp project root drives the generator's
                // Strategy-2 root discovery (walk up until a dir containing "Assets" exists).
                var stubTree = CSharpSyntaxTree.ParseText(
                    StubSource, path: Path.Combine(tempRoot, "_CorpusGateStub.g.cs"));

                var compilation = CSharpCompilation.Create(
                    assemblyName: "ReactiveUITK.Samples", // matches the Samples .asmdef → IsOwnedByCompilation
                    syntaxTrees: new[] { stubTree },
                    references: new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
                    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                var driver = (CSharpGeneratorDriver)CSharpGeneratorDriver
                    .Create(new UitkxGenerator())
                    .RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
                AssertCleanRun(driver.GetRunResult());
            }
            finally
            {
                try { Directory.Delete(tempRoot, recursive: true); } catch { }
            }
        }

        private static void AssertCleanRun(GeneratorDriverRunResult run)
        {

            // Scope: the strict-import layer (UITKX2xxx) + parse errors (UITKX03xx) + any
            // #error stub. UITKX0109 (attribute-vs-props validation) is EXCLUDED here because
            // this stub compilation carries no UI Toolkit props types, so editor-only builtin
            // elements (ToolbarButton, MultiColumnListView, …) can't resolve their attributes —
            // an artifact of the stub, not a real failure; the Asset-Store CI's real Unity
            // compile covers that layer with the full assemblies.
            // Also excluded: UITKX0120/0121 (asset existence/type) — the temp layout copies
            // only .uitkx/.asmdef, so sprites/audio/uss binaries aren't present; the real
            // Unity compile in CI validates that layer against the actual project.
            var errors = run.Results
                .SelectMany(r => r.Diagnostics)
                .Where(d => d.Severity == DiagnosticSeverity.Error
                    && !d.Id.Equals("UITKX0109", StringComparison.Ordinal)
                    && !d.Id.Equals("UITKX0120", StringComparison.Ordinal)
                    && !d.Id.Equals("UITKX0121", StringComparison.Ordinal))
                .Select(d => d.ToString())
                .ToList();

            var errorStubs = run.Results
                .SelectMany(r => r.GeneratedSources)
                .Where(s => s.SourceText.ToString().Contains("#error"))
                .Select(s => s.HintName + "\n    " + string.Join("\n    ",
                    s.SourceText.ToString().Split('\n')
                        .Where(l => l.Contains("#error")).Take(5)))
                .ToList();

            Assert.True(errors.Count == 0 && errorStubs.Count == 0,
                "The bundled Samples must generate clean — this mirrors the Asset-Store CI's real Unity compile.\n\n"
                + "— Error diagnostics (" + errors.Count + ") —\n" + string.Join("\n", errors.Take(30))
                + "\n\n— #error stubs (" + errorStubs.Count + ") —\n" + string.Join("\n\n", errorStubs.Take(12)));
        }

        /// <summary>
        /// Walks up from the test assembly to the PACKAGE root — the directory containing
        /// both <c>Samples/</c> and <c>SourceGenerator~/</c>. Works in both environments:
        /// locally (package nested in a Unity project) and on CI (the repo checkout IS the
        /// package root, with no <c>Assets/</c> ancestor).
        /// </summary>
        private static string? FindPackageRoot()
        {
            string? dir = AppContext.BaseDirectory;
            while (!string.IsNullOrEmpty(dir))
            {
                if (Directory.Exists(Path.Combine(dir, "Samples"))
                    && Directory.Exists(Path.Combine(dir, "SourceGenerator~")))
                    return dir;
                dir = Path.GetDirectoryName(dir);
            }
            return null;
        }
    }
}
