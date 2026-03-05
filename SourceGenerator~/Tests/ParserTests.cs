using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ReactiveUITK.Language;
using ReactiveUITK.Language.Nodes;
using ReactiveUITK.Language.Parser;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests;

/// <summary>
/// Direct unit tests for <see cref="DirectiveParser"/> and <see cref="UitkxParser"/>.
/// These tests exercise the internal parser types independently of the full
/// Roslyn generator pipeline.
/// </summary>
public class ParserTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static DirectiveSet ParseDirectives(string source, out List<ParseDiagnostic> diags)
    {
        diags = new List<ParseDiagnostic>();
        return DirectiveParser.Parse(source, "test.uitkx", diags);
    }

    private static ImmutableArray<AstNode> ParseMarkup(string source, out List<ParseDiagnostic> diags)
    {
        diags = new List<ParseDiagnostic>();
        var directives = DirectiveParser.Parse(source, "test.uitkx", diags);
        return UitkxParser.Parse(source, "test.uitkx", directives, diags);
    }

    private const string ValidHeader = "@namespace Test.NS\n@component MyComp\n";

    // ── Directive parsing ─────────────────────────────────────────────────────

    [Fact]
    public void Directives_ReadsNamespaceAndComponent()
    {
        var set = ParseDirectives(ValidHeader + "<box/>", out _);

        Assert.Equal("Test.NS", set.Namespace);
        Assert.Equal("MyComp", set.ComponentName);
    }

    [Fact]
    public void Directives_ReadsUsings()
    {
        const string src = """
            @namespace Test.NS
            @component MyComp
            @using System.Collections.Generic
            @using UnityEngine
            <box/>
            """;

        var set = ParseDirectives(src, out _);

        Assert.Contains("System.Collections.Generic", set.Usings);
        Assert.Contains("UnityEngine", set.Usings);
    }

    [Fact]
    public void Directives_ReadsPropsType()
    {
        const string src = "@namespace Test.NS\n@component MyComp\n@props MyProps\n<box/>";
        var set = ParseDirectives(src, out _);
        Assert.Equal("MyProps", set.PropsTypeName);
    }

    [Fact]
    public void Directives_MissingNamespace_EmitsUITKX0005()
    {
        ParseDirectives("@component MyComp\n<box/>", out var diags);
        Assert.Contains(diags, d => d.Code == "UITKX0005");
    }

    [Fact]
    public void Directives_MissingComponent_EmitsUITKX0005()
    {
        ParseDirectives("@namespace Test.NS\n<box/>", out var diags);
        Assert.Contains(diags, d => d.Code == "UITKX0005");
    }

    [Fact]
    public void Directives_WrongOrder_EmitsUITKX0012()
    {
        // @component declared before @namespace
        ParseDirectives("@component MyComp\n@namespace Test.NS\n<box/>", out var diags);
        Assert.Contains(diags, d => d.Code == "UITKX0012");
    }

    // ── Markup: elements ─────────────────────────────────────────────────────

    [Fact]
    public void Markup_SelfClosingElement_ProducesElementNode()
    {
        var nodes = ParseMarkup(ValidHeader + "<label/>", out _);

        var el = Assert.Single(nodes.OfType<ElementNode>());
        Assert.Equal("label", el.TagName);
        Assert.Empty(el.Children);
    }

    [Fact]
    public void Markup_NestedElement_ProducesChildren()
    {
        var nodes = ParseMarkup(ValidHeader + "<box><label/></box>", out _);

        var root = Assert.Single(nodes.OfType<ElementNode>());
        Assert.Equal("box", root.TagName);

        var child = Assert.Single(root.Children.OfType<ElementNode>());
        Assert.Equal("label", child.TagName);
    }

    [Fact]
    public void Markup_StringAttribute_ProducesStringLiteralValue()
    {
        var nodes = ParseMarkup(ValidHeader + """<label text="Hello"/>""", out _);

        var el = Assert.Single(nodes.OfType<ElementNode>());
        var attr = Assert.Single(el.Attributes, a => a.Name == "text");

        var value = Assert.IsType<StringLiteralValue>(attr.Value);
        Assert.Equal("Hello", value.Value);
    }

    [Fact]
    public void Markup_ExpressionAttribute_ProducesCSharpExpressionValue()
    {
        var nodes = ParseMarkup(ValidHeader + "<label text={count}/>", out _);

        var el = Assert.Single(nodes.OfType<ElementNode>());
        var attr = Assert.Single(el.Attributes, a => a.Name == "text");

        var value = Assert.IsType<CSharpExpressionValue>(attr.Value);
        Assert.Equal("count", value.Expression.Trim());
    }

    [Fact]
    public void Markup_BooleanShorthandAttribute_ProducesBooleanShorthandValue()
    {
        var nodes = ParseMarkup(ValidHeader + "<toggle disabled/>", out _);

        var el = Assert.Single(nodes.OfType<ElementNode>());
        var attr = Assert.Single(el.Attributes, a => a.Name == "disabled");

        Assert.IsType<BooleanShorthandValue>(attr.Value);
    }

    [Fact]
    public void Markup_MultipleAttributes_AllParsed()
    {
        var nodes = ParseMarkup(
            ValidHeader + """<button text="Click" onClick={handler}/>""",
            out _
        );

        var el = Assert.Single(nodes.OfType<ElementNode>());
        Assert.Equal(2, el.Attributes.Length);
        Assert.Contains(el.Attributes, a => a.Name == "text");
        Assert.Contains(el.Attributes, a => a.Name == "onClick");
    }

    // ── Markup: control flow ─────────────────────────────────────────────────

    [Fact]
    public void Markup_IfDirective_ProducesIfNode()
    {
        const string src = ValidHeader + "@if (x > 0) { <label/> }";
        var nodes = ParseMarkup(src, out _);

        var ifNode = Assert.Single(nodes.OfType<IfNode>());
        Assert.Single(ifNode.Branches);
        Assert.Equal("x > 0", ifNode.Branches[0].Condition?.Trim());
    }

    [Fact]
    public void Markup_IfElse_ProducesTwoBranches()
    {
        const string src = ValidHeader + "@if (flag) { <label/> } @else { <button/> }";
        var nodes = ParseMarkup(src, out _);

        var ifNode = Assert.Single(nodes.OfType<IfNode>());
        Assert.Equal(2, ifNode.Branches.Length);
        Assert.Null(ifNode.Branches[1].Condition); // @else has no condition
    }

    [Fact]
    public void Markup_ForeachDirective_ProducesForeachNode()
    {
        const string src = ValidHeader + "@foreach (var item in items) { <label/> }";
        var nodes = ParseMarkup(src, out _);

        var forNode = Assert.Single(nodes.OfType<ForeachNode>());
        Assert.Contains("item", forNode.IteratorDeclaration);
        Assert.Contains("items", forNode.CollectionExpression);
    }

    [Fact]
    public void Markup_SwitchDirective_ProducesSwitchNode()
    {
        const string src =
            ValidHeader
            + """
                @switch (mode) {
                    @case 0: <label text="zero"/>
                    @case 1: <label text="one"/>
                }
                """;
        var nodes = ParseMarkup(src, out _);

        var sw = Assert.Single(nodes.OfType<SwitchNode>());
        Assert.Contains("mode", sw.SwitchExpression);
        Assert.Equal(2, sw.Cases.Length);
    }

    [Fact]
    public void Markup_CodeBlock_ProducesCodeBlockNode()
    {
        const string src = ValidHeader + "@code { var x = 1; }\n<box/>";
        var nodes = ParseMarkup(src, out _);

        var cb = Assert.Single(nodes.OfType<CodeBlockNode>());
        Assert.Contains("var x = 1;", cb.Code);
    }

    [Fact]
    public void Markup_CodeBlock_LineCommentedMarkup_IsIgnored()
    {
        const string src =
            ValidHeader
            + """
              @code {
                  // var node = <Box>
                  //   <Label text="hi"/>
                  // </Box>;
                  var x = 1;
              }
              <box/>
              """;

        var nodes = ParseMarkup(src, out _);
        var cb = Assert.Single(nodes.OfType<CodeBlockNode>());

        Assert.Empty(cb.ReturnMarkups);
    }

    [Fact]
    public void Markup_CodeBlock_BlockCommentedMarkup_IsIgnored_ButLiveMarkupStillParsed()
    {
        const string src =
            ValidHeader
            + """
              @code {
                  /*
                  var node = <Box>
                      <Label text="hi"/>
                  </Box>;
                  */
                  var live = <Label text="ok"/>;
              }
              <box/>
              """;

        var nodes = ParseMarkup(src, out _);
        var cb = Assert.Single(nodes.OfType<CodeBlockNode>());

        var only = Assert.Single(cb.ReturnMarkups);
        Assert.Equal("Label", only.Element.TagName);
    }

    [Fact]
    public void Markup_InlineExpression_ProducesExpressionNode()
    {
        const string src = ValidHeader + "<box>@(someCall())</box>";
        var nodes = ParseMarkup(src, out _);

        var box = Assert.Single(nodes.OfType<ElementNode>());
        var expr = Assert.Single(box.Children.OfType<ExpressionNode>());
        Assert.Contains("someCall()", expr.Expression);
    }

    // ── Markup: line tracking ─────────────────────────────────────────────────

    [Fact]
    public void Markup_SourceLineIsOneBasedAndApproximate()
    {
        // The element is on line 3 (line 1 = @namespace, line 2 = @component)
        const string src = "@namespace Test.NS\n@component MyComp\n<label/>";
        var nodes = ParseMarkup(src, out _);

        var el = Assert.Single(nodes.OfType<ElementNode>());
        Assert.True(el.SourceLine >= 1, "SourceLine should be 1-based");
    }
}
