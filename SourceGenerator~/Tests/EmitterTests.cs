using System.Linq;
using ReactiveUITK.SourceGenerator.Tests.Helpers;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests;

/// <summary>
/// Integration tests that run the full <see cref="UitkxGenerator"/> pipeline
/// and assert properties of the generated C# source text.
/// </summary>
public class EmitterTests
{
    private const string Header = "@namespace Test.NS\n@component MyComp\n";

    // ── Element emission ──────────────────────────────────────────────────────

    [Fact]
    public void SimpleElement_GeneratesVCall()
    {
        var result = GeneratorTestHelper.Run(Header + "<label/>");

        Assert.True(result.SourceWasProduced);
        Assert.True(result.SourceContains("V.Label("), "Expected a V.Label( call");
    }

    [Fact]
    public void StringAttribute_AppearsInVCall()
    {
        var result = GeneratorTestHelper.Run(Header + """<label text="Hello World"/>""");

        Assert.True(
            result.SourceContains("Hello World"),
            "Expected string literal in generated source"
        );
    }

    [Fact]
    public void ExpressionAttribute_AppearsVerbatim()
    {
        var result = GeneratorTestHelper.Run(Header + "<label text={count}/>");

        Assert.True(
            result.SourceContains("count"),
            "Expected expression verbatim in generated source"
        );
    }

    [Fact]
    public void NestedElements_GeneratesNestedVCalls()
    {
        var result = GeneratorTestHelper.Run(Header + "<box><label/></box>");

        Assert.True(result.SourceContains("V.Box("), "Expected V.Box call");
        Assert.True(result.SourceContains("V.Label("), "Expected V.Label call inside box");
    }

    // ── Namespace / class structure ──────────────────────────────────────────

    [Fact]
    public void Namespace_EmittedInGeneratedSource()
    {
        var result = GeneratorTestHelper.Run(Header + "<box/>");

        Assert.True(result.SourceContains("namespace Test.NS"), "Expected namespace declaration");
    }

    [Fact]
    public void ClassName_MatchesComponentDirective()
    {
        var result = GeneratorTestHelper.Run(Header + "<box/>");

        Assert.True(
            result.SourceContains("partial class MyComp"),
            "Expected partial class named MyComp"
        );
    }

    // ── @code hoisting ───────────────────────────────────────────────────────

    [Fact]
    public void CodeBlock_AppearsBeforeReturn()
    {
        const string src = Header + "@code { var x = 42; }\n<box/>";
        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced);
        var generated = result.GeneratedSource!;

        int codePos = generated.IndexOf("var x = 42;", System.StringComparison.Ordinal);
        // Single-root: return <element>; — search for the V. call return in Render
        // (avoids matching the 'return' inside the __C helper generated above Render)
        int returnPos = generated.IndexOf("return V.", System.StringComparison.Ordinal);

        Assert.True(codePos >= 0, "@code content not found in generated source");
        Assert.True(returnPos >= 0, "return statement not found in generated source");
        Assert.True(codePos < returnPos, "@code block must appear before the return statement");
    }

    // ── Control flow ─────────────────────────────────────────────────────────

    [Fact]
    public void IfDirective_GeneratesConditionalExpression()
    {
        const string src = Header + "@if (flag) { <label/> }";
        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced);
        // @if → ternary; the condition should appear in the generated source
        Assert.True(result.SourceContains("flag"), "Condition should appear verbatim");
        Assert.True(
            result.SourceContains("? ")
                || result.SourceContains("?V.")
                || result.SourceContains("?("),
            "Ternary operator expected for @if"
        );
    }

    [Fact]
    public void ForeachDirective_GeneratesSelectCall()
    {
        const string src = Header + "@foreach (var item in items) { <label key={item.Id}/> }";
        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced);
        Assert.True(result.SourceContains(".Select("), "Expected .Select( for @foreach");
        Assert.True(result.SourceContains(".ToArray()"), "Expected .ToArray() after .Select(");
    }

    [Fact]
    public void SwitchDirective_GeneratesSwitchExpression()
    {
        const string src =
            Header
            + """
                @switch (mode) {
                    @case 0: <label text="zero"/>
                    @case 1: <label text="one"/>
                }
                """;
        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced);
        Assert.True(result.SourceContains("switch"), "Expected switch expression");
        // Emitter generates: _ => (global::ReactiveUITK.Core.VirtualNode)null,
        Assert.True(
            result.SourceContains("_ =>"),
            "Expected _ => fallback arm in switch expression"
        );
    }

    [Fact]
    public void ElseBranch_GeneratesNullFallback()
    {
        const string src = Header + "@if (flag) { <label/> }";
        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced);
        // @if without @else should produce `: null` in the ternary
        Assert.True(result.SourceContains(": null"), "Missing @else should generate : null");
    }

    // ── Key attribute ─────────────────────────────────────────────────────────

    [Fact]
    public void KeyAttribute_PassedAsSecondArgument()
    {
        var result = GeneratorTestHelper.Run(Header + """<label key="my-key" text="Hi"/>""");

        Assert.True(result.SourceContains("my-key"), "key value should appear in generated source");
    }

    // ── Line directives ───────────────────────────────────────────────────────

    [Fact]
    public void GeneratedSource_ContainsLineDirectives()
    {
        var result = GeneratorTestHelper.Run(Header + "<label/>");

        // #line directives must reference the original .uitkx file
        Assert.True(
            result.SourceContains("#line"),
            "Expected #line directives in generated source"
        );
    }
}
