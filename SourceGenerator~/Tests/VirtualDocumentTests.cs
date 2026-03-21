using System.Collections.Generic;
using System.Collections.Immutable;
using ReactiveUITK.Language;
using ReactiveUITK.Language.Lowering;
using ReactiveUITK.Language.Nodes;
using ReactiveUITK.Language.Parser;
using ReactiveUITK.Language.Roslyn;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests;

/// <summary>
/// Tests for <see cref="VirtualDocumentGenerator"/> — verifies that the generated
/// virtual C# document:
///   1) Contains the expected C# scaffolding and user expressions.
///   2) Produces a <see cref="SourceMap"/> that bidirectionally maps positions
///      between .uitkx offsets and virtual C# offsets.
/// Pure language-lib tests (no LSP, no Roslyn compilation).
/// </summary>
public sealed class VirtualDocumentTests
{
    private static readonly VirtualDocumentGenerator _gen = new();

    // ── Helpers ────────────────────────────────────────────────────────────

    private static ParseResult Parse(string source, string path = "Test.uitkx")
    {
        var diags = new List<ParseDiagnostic>();
        var directives = DirectiveParser.Parse(source, path, diags);
        var parsedNodes = UitkxParser.Parse(source, path, directives, diags);
        var nodes = CanonicalLowering.LowerToRenderRoots(directives, parsedNodes, path);
        return new ParseResult(directives, nodes, ImmutableArray.CreateRange(diags));
    }

    private static VirtualDocument Generate(string source, string path = "Test.uitkx") =>
        _gen.Generate(Parse(source, path), source, path);

    // ── Basic scaffolding ──────────────────────────────────────────────────

    [Fact]
    public void BasicScaffolding_ContainsNamespace()
    {
        var source = "@namespace My.App\n@component Foo\n<Label text=\"hi\"/>";
        var doc = Generate(source);
        Assert.Contains("namespace My.App", doc.Text);
    }

    [Fact]
    public void BasicScaffolding_ContainsClassName()
    {
        var source = "@namespace My.App\n@component Foo\n<Label text=\"hi\"/>";
        var doc = Generate(source);
        Assert.Contains("Foo", doc.Text);
    }

    [Fact]
    public void BasicScaffolding_MapIsNotEmpty()
    {
        var source = "@namespace My.App\n@component Foo\n<Label text={myVar}/>";
        var doc = Generate(source);
        Assert.True(doc.Map.Entries.Length > 0,
            "Expected at least one mapped entry for an expression attribute");
    }

    // ── Expression embedding ───────────────────────────────────────────────

    [Fact]
    public void ExpressionAttribute_MappedInVirtualDoc()
    {
        var source = "@namespace T\n@component C\n<Label text={myVar}/>";
        var doc = Generate(source);
        Assert.Contains("myVar", doc.Text);
    }

    [Fact]
    public void InlineExpression_MappedInVirtualDoc()
    {
        var source = "@namespace T\n@component C\n<Box>\n  @(someExpr)\n</Box>";
        var doc = Generate(source);
        Assert.Contains("someExpr", doc.Text);
    }

    // ── @code block ────────────────────────────────────────────────────────

    [Fact]
    public void CodeBlock_VerbatimCopied()
    {
        var source = "@namespace T\n@component C\n@code {\n  int counter = 42;\n}\n<Label/>";
        var doc = Generate(source);
        Assert.Contains("counter = 42", doc.Text);
    }

    // ── Function-style component ───────────────────────────────────────────

    [Fact]
    public void FunctionStyle_SetupCodeCopied()
    {
        var source = "component Counter {\n  int x = 10;\n  return (\n    <Label/>\n  );\n}";
        var doc = Generate(source);
        Assert.Contains("int x = 10", doc.Text);
    }

    // ── Source map: bidirectional ───────────────────────────────────────────

    [Fact]
    public void SourceMap_RoundTripsExpressionOffset()
    {
        var source = "@namespace T\n@component C\n<Label text={myVar}/>";
        var doc = Generate(source);

        // Find "myVar" in the uitkx source
        int uitkxIdx = source.IndexOf("myVar");
        Assert.True(uitkxIdx >= 0, "myVar must appear in source");

        // Map to virtual
        var toVirtual = doc.Map.ToVirtualOffset(uitkxIdx);
        Assert.NotNull(toVirtual);

        // The virtual offset should point at "myVar" in virtual doc
        Assert.True(doc.Text.Length > toVirtual.Value.VirtualOffset);
        var virtualSub = doc.Text.Substring(toVirtual.Value.VirtualOffset, 5);
        Assert.Equal("myVar", virtualSub);

        // Round-trip back
        var backToUitkx = doc.Map.ToUitkxOffset(toVirtual.Value.VirtualOffset);
        Assert.NotNull(backToUitkx);
        Assert.Equal(uitkxIdx, backToUitkx.Value.UitkxOffset);
    }

    [Fact]
    public void SourceMap_NonCSharpRegion_ReturnsNull()
    {
        var source = "@namespace T\n@component C\n<Label text=\"plain\"/>";
        var doc = Generate(source);

        // "@namespace" is not C#; should not map
        var result = doc.Map.ToVirtualOffset(0);
        Assert.Null(result);
    }

    [Fact]
    public void SourceMap_CodeBlockRegion_RoundTrips()
    {
        var source = "@namespace T\n@component C\n@code {\n  int val = 99;\n}\n<Label/>";
        var doc = Generate(source);

        int uitkxIdx = source.IndexOf("val = 99");
        Assert.True(uitkxIdx >= 0);

        var toVirtual = doc.Map.ToVirtualOffset(uitkxIdx);
        Assert.NotNull(toVirtual);
        Assert.Equal(SourceRegionKind.CodeBlock, toVirtual.Value.Entry.Kind);

        var virtualSub = doc.Text.Substring(toVirtual.Value.VirtualOffset, 3);
        Assert.Equal("val", virtualSub);
    }

    [Fact]
    public void SourceMap_FunctionSetup_RegionKind()
    {
        var source = "component Foo {\n  int z = 7;\n  return (\n    <Label/>\n  );\n}";
        var doc = Generate(source);

        int uitkxIdx = source.IndexOf("int z = 7");
        Assert.True(uitkxIdx >= 0);

        var toVirtual = doc.Map.ToVirtualOffset(uitkxIdx);
        Assert.NotNull(toVirtual);
        Assert.Equal(SourceRegionKind.FunctionSetup, toVirtual.Value.Entry.Kind);
    }

    // ── IsInCSharpRegion ───────────────────────────────────────────────────

    [Fact]
    public void IsInCSharpRegion_TrueForExpression()
    {
        var source = "@namespace T\n@component C\n<Label text={myExpr}/>";
        var doc = Generate(source);

        int idx = source.IndexOf("myExpr");
        Assert.True(doc.Map.IsInCSharpRegion(idx));
    }

    [Fact]
    public void IsInCSharpRegion_FalseForMarkup()
    {
        var source = "@namespace T\n@component C\n<Label text=\"plain\"/>";
        var doc = Generate(source);

        int idx = source.IndexOf("<Label");
        Assert.False(doc.Map.IsInCSharpRegion(idx));
    }

    // ── Multiple expressions ───────────────────────────────────────────────

    [Fact]
    public void MultipleExpressions_AllMapped()
    {
        var source = "@namespace T\n@component C\n<Box>\n  <Label text={aaa}/>\n  <Label text={bbb}/>\n</Box>";
        var doc = Generate(source);

        Assert.Contains("aaa", doc.Text);
        Assert.Contains("bbb", doc.Text);

        Assert.True(doc.Map.IsInCSharpRegion(source.IndexOf("aaa")));
        Assert.True(doc.Map.IsInCSharpRegion(source.IndexOf("bbb")));
    }
}
