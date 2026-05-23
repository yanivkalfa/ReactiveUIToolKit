using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using ReactiveUITK.SourceGenerator.Analyzers;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests;

public class UitkxHmrSwapWriteAnalyzerTests
{
    // Minimal stub of the attribute so the analyzer can resolve it against
    // the test compilation. (The real attribute lives in ReactiveUITK.Shared.
    // For analyzer-only unit tests we don't want to drag in Shared, so we
    // declare a same-namespace+same-name shadow.)
    private const string AttrShim = """
        namespace ReactiveUITK
        {
            [System.AttributeUsage(System.AttributeTargets.Field)]
            public sealed class UitkxHmrSwapAttribute : System.Attribute { }
        }
        """;

    private static async Task<ImmutableArray<Diagnostic>> RunAnalyzer(string userSource)
    {
        var compilation = CSharpCompilation.Create(
            "TestAsm",
            new[]
            {
                CSharpSyntaxTree.ParseText(AttrShim),
                CSharpSyntaxTree.ParseText(userSource),
            },
            new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Attribute).Assembly.Location),
                // System.Runtime reference (needed for netcore Object resolution).
                MetadataReference.CreateFromFile(
                    Path.Combine(
                        Path.GetDirectoryName(typeof(object).Assembly.Location)!,
                        "System.Runtime.dll")),
            },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(new UitkxHmrSwapWriteAnalyzer());
        var withAnalyzers = compilation.WithAnalyzers(analyzers);
        var diags = await withAnalyzers.GetAnalyzerDiagnosticsAsync();
        return diags;
    }

    [Fact]
    public async Task WriteFromNonCctor_IsFlagged()
    {
        const string src = """
            public class Mod
            {
                [global::ReactiveUITK.UitkxHmrSwap]
                public static int X = 1;

                public static void DoStuff()
                {
                    X = 5;
                }
            }
            """;
        var diags = await RunAnalyzer(src);
        Assert.Contains(diags, d => d.Id == "UITKX0210");
    }

    [Fact]
    public async Task WriteInsideStaticCtor_IsAllowed()
    {
        const string src = """
            public class Mod
            {
                [global::ReactiveUITK.UitkxHmrSwap]
                public static int X = 1;

                static Mod()
                {
                    X = 5;
                }
            }
            """;
        var diags = await RunAnalyzer(src);
        Assert.DoesNotContain(diags, d => d.Id == "UITKX0210");
    }

    [Fact]
    public async Task WriteToUnattributedField_NotFlagged()
    {
        const string src = """
            public class Mod
            {
                public static int X = 1;

                public static void DoStuff()
                {
                    X = 5;
                }
            }
            """;
        var diags = await RunAnalyzer(src);
        Assert.DoesNotContain(diags, d => d.Id == "UITKX0210");
    }

    [Fact]
    public async Task CompoundAssignment_IsFlagged()
    {
        const string src = """
            public class Mod
            {
                [global::ReactiveUITK.UitkxHmrSwap]
                public static int X = 1;

                public static void DoStuff()
                {
                    X += 5;
                }
            }
            """;
        var diags = await RunAnalyzer(src);
        Assert.Contains(diags, d => d.Id == "UITKX0210");
    }

    [Fact]
    public async Task Increment_IsFlagged()
    {
        const string src = """
            public class Mod
            {
                [global::ReactiveUITK.UitkxHmrSwap]
                public static int X = 1;

                public static void DoStuff()
                {
                    X++;
                }
            }
            """;
        var diags = await RunAnalyzer(src);
        Assert.Contains(diags, d => d.Id == "UITKX0210");
    }

    [Fact]
    public async Task FieldInitializerInOwnDeclaration_NotFlagged()
    {
        // Field initializer is lowered into the static ctor — must not flag.
        const string src = """
            public class Mod
            {
                [global::ReactiveUITK.UitkxHmrSwap]
                public static int X = 42;
            }
            """;
        var diags = await RunAnalyzer(src);
        Assert.DoesNotContain(diags, d => d.Id == "UITKX0210");
    }
}
