using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUITK.SourceGenerator.Emitter;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests;

public class StaticReadonlyStripperTests
{
    private static FieldDeclarationSyntax ParseField(string source)
    {
        var tree = CSharpSyntaxTree.ParseText("class C { " + source + " }");
        var root = tree.GetRoot();
        FieldDeclarationSyntax field = null!;
        foreach (var node in root.DescendantNodes())
        {
            if (node is FieldDeclarationSyntax f) { field = f; break; }
        }
        Assert.NotNull(field);
        return field;
    }

    [Fact]
    public void IsStripCandidate_StaticReadonly_True()
    {
        Assert.True(StaticReadonlyStripper.IsStripCandidate(
            ParseField("public static readonly int X = 1;")));
    }

    [Fact]
    public void IsStripCandidate_MutableStatic_False()
    {
        Assert.False(StaticReadonlyStripper.IsStripCandidate(
            ParseField("public static int X = 1;")));
    }

    [Fact]
    public void IsStripCandidate_Const_False()
    {
        // const fields cannot be readonly in C# — defensive coverage.
        Assert.False(StaticReadonlyStripper.IsStripCandidate(
            ParseField("public const int X = 1;")));
    }

    [Fact]
    public void IsStripCandidate_InstanceReadonly_False()
    {
        Assert.False(StaticReadonlyStripper.IsStripCandidate(
            ParseField("public readonly int X = 1;")));
    }

    [Fact]
    public void Strip_RemovesReadonly_AddsAttribute()
    {
        var field = ParseField("public static readonly int X = 1;");
        var result = StaticReadonlyStripper.Strip(field).ToFullString();

        Assert.DoesNotContain("readonly", result);
        Assert.Contains("[global::ReactiveUITK.UitkxHmrSwap]", result);
        Assert.Contains("public static int X = 1", result);
    }

    [Fact]
    public void Strip_PreservesExistingAttributes()
    {
        var field = ParseField("[System.Obsolete] public static readonly int X = 1;");
        var result = StaticReadonlyStripper.Strip(field).ToFullString();

        Assert.Contains("[System.Obsolete]", result);
        Assert.Contains("[global::ReactiveUITK.UitkxHmrSwap]", result);
        Assert.DoesNotContain("readonly", result);
    }

    [Fact]
    public void Strip_MultiDeclarator_SingleAttributeCoversAll()
    {
        var field = ParseField("public static readonly int A = 1, B = 2;");
        var result = StaticReadonlyStripper.Strip(field).ToFullString();

        Assert.DoesNotContain("readonly", result);
        Assert.Contains("[global::ReactiveUITK.UitkxHmrSwap]", result);
        Assert.Contains("A = 1", result);
        Assert.Contains("B = 2", result);
    }

    [Fact]
    public void Strip_GenericFieldType_Preserved()
    {
        var field = ParseField("public static readonly System.Collections.Generic.Dictionary<string, int> Map = new();");
        var result = StaticReadonlyStripper.Strip(field).ToFullString();

        Assert.DoesNotContain("readonly", result);
        Assert.Contains("Dictionary<string, int>", result);
        Assert.Contains("[global::ReactiveUITK.UitkxHmrSwap]", result);
    }

    [Fact]
    public void Strip_XmlDocComment_Preserved()
    {
        var src = """
            /// <summary>The thing.</summary>
            public static readonly int X = 1;
            """;
        var field = ParseField(src);
        var result = StaticReadonlyStripper.Strip(field).ToFullString();

        Assert.Contains("<summary>The thing.</summary>", result);
        Assert.Contains("[global::ReactiveUITK.UitkxHmrSwap]", result);
        Assert.DoesNotContain("readonly", result);
    }
}
