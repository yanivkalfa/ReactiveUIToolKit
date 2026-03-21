using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace ReactiveUITK.SourceGenerator.Tests.Helpers;

/// <summary>
/// Creates a minimal Roslyn compilation, runs the <see cref="UitkxGenerator"/>
/// against a supplied .uitkx source string, and returns the resulting diagnostics
/// and generated C# text.
/// </summary>
internal static class GeneratorTestHelper
{
    // Both Guards in UitkxPipeline must pass:
    //
    //  Guard 1: compilation must resolve ReactiveUITK.Core.VirtualNode
    //  Guard 2: compilation.AssemblyName must match the nearest ancestor .asmdef
    //           "name" field, OR (when no .asmdef exists) must start with
    //           "Assembly-CSharp" (the Unity default assembly).
    //
    // We satisfy Guard 1 by including a stub VirtualNode declaration.
    // We satisfy Guard 2 by using "Assembly-CSharp" as the assembly name,
    // which matches the no-.asmdef fallback path used for all test .uitkx files
    // that live in a temp directory with no ancestor .asmdef.

    private const string StubSource = """
        namespace ReactiveUITK.Core
        {
            public abstract class VirtualNode { }
        }
        """;

    /// <summary>
    /// Runs the generator and returns all generator diagnostics plus the first
    /// generated source text (or <c>null</c> if no source was produced).
    /// </summary>
    public static GeneratorRunOutput Run(
        string uitkxContent,
        string fileName = "TestComponent.uitkx"
    )
    {
        // Use a stable temp directory so the path comparison in Guard 2 works
        string testDir = Path.Combine(Path.GetTempPath(), "uitkx_tests");
        string uitkxPath = Path.Combine(testDir, fileName);
        string stubPath = Path.Combine(testDir, "_Stubs.g.cs");

        var stubTree = CSharpSyntaxTree.ParseText(StubSource, path: stubPath);

        var compilation = CSharpCompilation.Create(
            assemblyName: "Assembly-CSharp",
            syntaxTrees: new[] { stubTree },
            references: new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            },
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        var generator = new UitkxGenerator();
        var addlText = new InMemoryAdditionalText(uitkxPath, uitkxContent);

        var driver = CSharpGeneratorDriver
            .Create(generator)
            .AddAdditionalTexts(ImmutableArray.Create<AdditionalText>(addlText));

        driver = (CSharpGeneratorDriver)
            driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);

        var runResult = driver.GetRunResult();

        var allDiags = ImmutableArray<Diagnostic>.Empty;
        foreach (var r in runResult.Results)
            allDiags = allDiags.AddRange(r.Diagnostics);

        string? generatedSource = null;
        if (runResult.Results.Length > 0)
        {
            // Skip UITKX_Loaded.g.cs (the always-present marker stub) and pick
            // the first source that actually contains generated component code.
            foreach (var src in runResult.Results[0].GeneratedSources)
            {
                string text = src.SourceText.ToString();
                if (text.Contains("partial class") || text.Contains("namespace "))
                {
                    generatedSource = text;
                    break;
                }
            }
        }

        return new GeneratorRunOutput(allDiags, generatedSource);
    }

    /// <summary>
    /// Runs the generator with <b>multiple</b> .uitkx files so that peer-component
    /// name and props-type resolution can be exercised across files.
    /// Returns the generated output for <paramref name="primaryFileName"/>.
    /// </summary>
    public static GeneratorRunOutput RunMultiple(
        (string fileName, string content)[] files,
        string primaryFileName
    )
    {
        string testDir = Path.Combine(Path.GetTempPath(), "uitkx_tests");
        string stubPath = Path.Combine(testDir, "_Stubs.g.cs");

        var stubTree = CSharpSyntaxTree.ParseText(StubSource, path: stubPath);

        var compilation = CSharpCompilation.Create(
            assemblyName: "Assembly-CSharp",
            syntaxTrees: new[] { stubTree },
            references: new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            },
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        var generator = new UitkxGenerator();
        var addlTexts = ImmutableArray.CreateRange(
            files.Select(f =>
                (AdditionalText)new InMemoryAdditionalText(
                    Path.Combine(testDir, f.fileName),
                    f.content
                )
            )
        );

        var driver = CSharpGeneratorDriver
            .Create(generator)
            .AddAdditionalTexts(addlTexts);

        driver = (CSharpGeneratorDriver)
            driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);

        var runResult = driver.GetRunResult();

        var allDiags = ImmutableArray<Diagnostic>.Empty;
        foreach (var r in runResult.Results)
            allDiags = allDiags.AddRange(r.Diagnostics);

        // Find the generated source for the primary file
        string? generatedSource = null;
        string primaryBase = Path.GetFileNameWithoutExtension(primaryFileName); // e.g. "Parent"
        foreach (var r in runResult.Results)
        {
            foreach (var src in r.GeneratedSources)
            {
                string text = src.SourceText.ToString();
                // Match by hint name containing the primary component name
                if (src.HintName.Contains(primaryBase) &&
                    (text.Contains("partial class") || text.Contains("namespace ")))
                {
                    generatedSource = text;
                    break;
                }
            }
            if (generatedSource != null) break;
        }

        // Fallback: first source with component code
        if (generatedSource == null)
        {
            foreach (var r in runResult.Results)
            {
                foreach (var src in r.GeneratedSources)
                {
                    string text = src.SourceText.ToString();
                    if (text.Contains("partial class") || text.Contains("namespace "))
                    {
                        generatedSource = text;
                        break;
                    }
                }
                if (generatedSource != null) break;
            }
        }

        return new GeneratorRunOutput(allDiags, generatedSource);
    }

    /// <summary>
    /// Like <see cref="Run"/> but also compiles additional C# source into the
    /// Roslyn snapshot so that external types (e.g. a C# class with a nested
    /// <c>Props</c> class) are visible to <see cref="PropsResolver"/>.
    /// </summary>
    public static GeneratorRunOutput RunWithExtraCSharp(
        string uitkxContent,
        string extraCSharpSource,
        string fileName = "TestComponent.uitkx"
    )
    {
        string testDir = Path.Combine(Path.GetTempPath(), "uitkx_tests");
        string uitkxPath = Path.Combine(testDir, fileName);
        string stubPath = Path.Combine(testDir, "_Stubs.g.cs");
        string extraPath = Path.Combine(testDir, "_Extra.g.cs");

        var stubTree = CSharpSyntaxTree.ParseText(StubSource, path: stubPath);
        var extraTree = CSharpSyntaxTree.ParseText(extraCSharpSource, path: extraPath);

        var compilation = CSharpCompilation.Create(
            assemblyName: "Assembly-CSharp",
            syntaxTrees: new[] { stubTree, extraTree },
            references: new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            },
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        var generator = new UitkxGenerator();
        var addlText = new InMemoryAdditionalText(uitkxPath, uitkxContent);

        var driver = CSharpGeneratorDriver
            .Create(generator)
            .AddAdditionalTexts(ImmutableArray.Create<AdditionalText>(addlText));

        driver = (CSharpGeneratorDriver)
            driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);

        var runResult = driver.GetRunResult();

        var allDiags = ImmutableArray<Diagnostic>.Empty;
        foreach (var r in runResult.Results)
            allDiags = allDiags.AddRange(r.Diagnostics);

        string? generatedSource = null;
        if (runResult.Results.Length > 0)
        {
            foreach (var src in runResult.Results[0].GeneratedSources)
            {
                string text = src.SourceText.ToString();
                if (text.Contains("partial class") || text.Contains("namespace "))
                {
                    generatedSource = text;
                    break;
                }
            }
        }

        return new GeneratorRunOutput(allDiags, generatedSource);
    }
}

/// <param name="Diagnostics">All diagnostics emitted by the generator run.</param>
/// <param name="GeneratedSource">The first generated C# source text, or <c>null</c>.</param>
internal sealed record GeneratorRunOutput(
    ImmutableArray<Diagnostic> Diagnostics,
    string? GeneratedSource
)
{
    public bool HasDiagnostic(string id) => Diagnostics.Any(d => d.Id == id);

    public bool HasNoDiagnostics => Diagnostics.IsEmpty;

    /// <summary>Returns true if the generated source contains <paramref name="text"/>.</summary>
    public bool SourceContains(string text) =>
        GeneratedSource?.Contains(text, StringComparison.Ordinal) == true;

    public bool SourceWasProduced => GeneratedSource != null;
}

/// <summary>An in-memory <see cref="AdditionalText"/> backed by a plain string.</summary>
internal sealed class InMemoryAdditionalText : AdditionalText
{
    private readonly string _content;

    public override string Path { get; }

    public InMemoryAdditionalText(string path, string content)
    {
        Path = path;
        _content = content;
    }

    public override SourceText? GetText(CancellationToken ct = default) =>
        SourceText.From(_content, Encoding.UTF8);
}
