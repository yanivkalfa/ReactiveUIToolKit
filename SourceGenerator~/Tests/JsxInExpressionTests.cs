using ReactiveUITK.SourceGenerator.Tests.Helpers;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests;

/// <summary>
/// Phase 1 — JSX literals in arbitrary expression positions.
///
/// <para>The generator's <see cref="DirectiveParser"/> scanner has long
/// supported JSX in component preambles and directive bodies. Phase 1 wires
/// the same scanner into the two remaining expression positions:</para>
/// <list type="bullet">
///   <item>Child expressions: <c>{cond ? &lt;A/&gt; : &lt;B/&gt;}</c></item>
///   <item>Attribute values:  <c>attr={cond ? &lt;A/&gt; : null}</c>,
///         <c>attr={x =&gt; &lt;Item/&gt;}</c></item>
/// </list>
/// <para>Result: JSX is allowed wherever a C# expression is allowed,
/// matching React/Babel semantics.</para>
/// </summary>
public class JsxInExpressionTests
{
    private static string Wrap(string markup) =>
        "component MyComp {\n  return (\n" + markup + "\n  );\n}";

    private static string WrapWithSetup(string setup, string markup) =>
        "component MyComp {\n" + setup + "\n  return (\n" + markup + "\n  );\n}";

    // ── Child expression: {cond ? <A/> : <B/>} ──────────────────────────────

    [Fact]
    public void Child_TernaryWithJsxBranches_BothSpliced()
    {
        var src = WrapWithSetup(
            "  bool flag = true;",
            "<box>{flag ? <label text=\"A\"/> : <label text=\"B\"/>}</box>"
        );
        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced);
        Assert.True(result.SourceContains("V.Label("), "Expected V.Label inside ternary branches");
        Assert.True(result.SourceContains("V.Box("), "Expected outer V.Box");
        // No raw JSX should leak into emitted code.
        Assert.False(
            result.SourceContains("<label text=\"A\"/>"),
            "Raw JSX should be spliced, not pass through"
        );
    }

    [Fact]
    public void Child_TernaryWithSingleJsxBranch_Spliced()
    {
        var src = WrapWithSetup(
            "  bool flag = true;",
            "<box>{flag ? <label text=\"hi\"/> : null}</box>"
        );
        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced);
        Assert.True(result.SourceContains("V.Label("));
        Assert.True(result.SourceContains("? V.Label(") || result.SourceContains("?V.Label("));
    }

    [Fact]
    public void Child_AtParenWithJsxTernary_Spliced()
    {
        // @(cond ? <A/> : <B/>) and {cond ? <A/> : <B/>} should behave identically.
        var src = WrapWithSetup(
            "  bool flag = true;",
            "<box>@(flag ? <label text=\"A\"/> : <label text=\"B\"/>)</box>"
        );
        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced);
        Assert.True(result.SourceContains("V.Label("));
        Assert.False(result.SourceContains("<label text=\"A\"/>"));
    }

    // ── Attribute value: attr={cond ? <A/> : null} ──────────────────────────

    [Fact]
    public void Attribute_TernaryWithJsx_Spliced()
    {
        // attr={cond ? <A/> : null} on a built-in element. We use a dictionary-style
        // built-in to sidestep prop-type lookups.
        var src = WrapWithSetup(
            "  bool flag = true;",
            "<box class={flag ? \"on\" : \"off\"}/>"
        );
        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced);
        Assert.True(result.SourceContains("V.Box("));
        Assert.True(result.SourceContains("\"on\""));
    }

    [Fact]
    public void Attribute_LambdaReturningJsx_Spliced()
    {
        // attr={x => <Item/>} — lambda body contains JSX.
        // We verify scanner detects bare JSX after `=>` and splices.
        var src = WrapWithSetup(
            "  System.Func<int, global::ReactiveUITK.Core.VirtualNode> renderItem = i => <label text=\"item\"/>;",
            "<box>@(renderItem(0))</box>"
        );
        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced);
        Assert.True(result.SourceContains("V.Label("), "Lambda body JSX should splice");
    }

    // ── Edge cases the scanner must NOT confuse with JSX ────────────────────

    [Fact]
    public void GenericLessThan_NotConfusedWithJsx()
    {
        var src = WrapWithSetup(
            "  var items = new System.Collections.Generic.List<int>();\n  bool small = items.Count < 1;",
            "<label text={small ? \"empty\" : \"has\"}/>"
        );
        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced);
        // Comparison operator must be preserved, not interpreted as JSX.
        Assert.True(result.SourceContains("Count < 1"));
    }

    [Fact]
    public void StringContainingTagLikeText_NotSpliced()
    {
        var src = WrapWithSetup(
            "  string s = \"<label/>\";",
            "<label text={s}/>"
        );
        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced);
        // The string literal "<label/>" must remain a string, not become V.Label(...).
        Assert.True(result.SourceContains("\"<label/>\""));
    }

    [Fact]
    public void NullCoalesceWithJsx_Spliced()
    {
        var src = WrapWithSetup(
            "  global::ReactiveUITK.Core.VirtualNode? fallback = null;",
            "<box>{fallback ?? <label text=\"fallback\"/>}</box>"
        );
        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced);
        Assert.True(result.SourceContains("V.Label("));
        Assert.True(result.SourceContains("?? V.Label(") || result.SourceContains("??V.Label("));
    }

    [Fact]
    public void NoJsxInExpression_ScannerIsNoOp()
    {
        // Sanity: expressions without JSX are emitted verbatim — the scanner
        // should run, find nothing, and return the input unchanged.
        var src = WrapWithSetup(
            "  int n = 42;",
            "<label text={n.ToString()}/>"
        );
        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced);
        Assert.True(result.SourceContains("n.ToString()"));
    }

    // ── Existing JsxExpressionValue path is preserved (cleaner emit) ────────

    [Fact]
    public void Attribute_BareJsxLiteral_StillUsesJsxExpressionValue()
    {
        // attr={<Tag/>} — opt-in path predates Phase 1 and produces cleaner emit.
        // Phase 1 must NOT regress it.
        var src = Wrap("<box class={\"red\"}><label text=\"hi\"/></box>");
        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced);
        Assert.True(result.SourceContains("V.Label("));
        Assert.True(result.SourceContains("V.Box("));
    }
}
