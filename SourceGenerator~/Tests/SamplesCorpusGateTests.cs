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
            string? unityRoot = FindUnityProjectRoot();
            Assert.False(unityRoot is null,
                "Could not locate the Unity project root (a directory containing Assets/ReactiveUIToolKit/Samples) above the test assembly.");

            // A stub tree whose path sits in the Unity project root drives the generator's
            // Strategy-2 root discovery (walk up until a dir containing "Assets" exists).
            var stubTree = CSharpSyntaxTree.ParseText(
                StubSource, path: Path.Combine(unityRoot!, "_CorpusGateStub.g.cs"));

            var compilation = CSharpCompilation.Create(
                assemblyName: "ReactiveUITK.Samples", // matches the Samples .asmdef → IsOwnedByCompilation
                syntaxTrees: new[] { stubTree },
                references: new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var driver = (CSharpGeneratorDriver)CSharpGeneratorDriver
                .Create(new UitkxGenerator())
                .RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
            var run = driver.GetRunResult();

            // Scope: the strict-import layer (UITKX2xxx) + parse errors (UITKX03xx) + any
            // #error stub. UITKX0109 (attribute-vs-props validation) is EXCLUDED here because
            // this stub compilation carries no UI Toolkit props types, so editor-only builtin
            // elements (ToolbarButton, MultiColumnListView, …) can't resolve their attributes —
            // an artifact of the stub, not a real failure; the Asset-Store CI's real Unity
            // compile covers that layer with the full assemblies.
            var errors = run.Results
                .SelectMany(r => r.Diagnostics)
                .Where(d => d.Severity == DiagnosticSeverity.Error
                    && !d.Id.Equals("UITKX0109", StringComparison.Ordinal))
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

        private static string? FindUnityProjectRoot()
        {
            string? dir = AppContext.BaseDirectory;
            while (!string.IsNullOrEmpty(dir))
            {
                if (Directory.Exists(Path.Combine(dir, "Assets", "ReactiveUIToolKit", "Samples")))
                    return dir;
                dir = Path.GetDirectoryName(dir);
            }
            return null;
        }
    }
}
