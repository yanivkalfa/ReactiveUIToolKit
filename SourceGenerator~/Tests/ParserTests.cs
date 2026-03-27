using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
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

    private static ImmutableArray<AstNode> ParseMarkup(
        string source,
        out List<ParseDiagnostic> diags
    )
    {
        diags = new List<ParseDiagnostic>();
        var directives = DirectiveParser.Parse(source, "test.uitkx", diags);
        return UitkxParser.Parse(source, "test.uitkx", directives, diags);
    }

    /// Wraps markup in a minimal function-style component for test convenience.
    private static string Wrap(string markup) =>
        "component MyComp {\n  return (\n" + markup + "\n  );\n}";

    /// Wraps code + markup in a function-style component (code runs before return).
    private static string WrapWithCode(string code, string markup) =>
        "component MyComp {\n  " + code + "\n  return (\n" + markup + "\n  );\n}";

    // ── Directive parsing ─────────────────────────────────────────────────────

    [Fact]
    public void Directives_FunctionStyle_ParsesComponentAndReturnRange()
    {
        const string src =
            """
            component CounterPanel {
                var (count, setCount) = useState(0);
                return (
                    <Box><Label text={$"{count}"} /></Box>
                );
            }
            """;

        var set = ParseDirectives(src, out var diags);

        Assert.DoesNotContain(diags, d => d.Severity == ParseSeverity.Error);
        Assert.True(set.IsFunctionStyle);
        Assert.Equal("CounterPanel", set.ComponentName);
        Assert.Equal("ReactiveUITK.FunctionStyle", set.Namespace);
        Assert.True(set.MarkupStartIndex > 0);
        Assert.True(set.MarkupEndIndex > set.MarkupStartIndex);
        Assert.Contains("useState", set.FunctionSetupCode ?? string.Empty);
    }

    [Fact]
    public void Directives_FunctionStyle_InfersNamespace_FromCompanionPartialClass()
    {
        string dir = Path.Combine(
            Path.GetTempPath(),
            "uitkx-func-ns-" + System.Guid.NewGuid().ToString("N")
        );
        Directory.CreateDirectory(dir);

        try
        {
            string uitkxPath = Path.Combine(dir, "CounterPanel.uitkx");
            string companionPath = Path.Combine(dir, "CounterPanel.cs");

            File.WriteAllText(
                companionPath,
                "namespace MyGame.Sample.UI { public partial class CounterPanel { } }"
            );

            const string src =
                """
                component CounterPanel {
                    return (<Box />);
                }
                """;

            var diags = new List<ParseDiagnostic>();
            var set = DirectiveParser.Parse(src, uitkxPath, diags);

            Assert.True(set.IsFunctionStyle);
            Assert.Equal("CounterPanel", set.ComponentName);
            Assert.Equal("MyGame.Sample.UI", set.Namespace);
        }
        finally
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void Directives_FunctionStyle_WithLeadingComments_IsDetected()
    {
        const string src =
            """
            // file banner
            /* parser should skip this block comment */
            <!-- and this UITKX comment -->
            component CounterPanel {
                return (<Box />);
            }
            """;

        var set = ParseDirectives(src, out var diags);

        Assert.True(set.IsFunctionStyle);
        Assert.Equal("CounterPanel", set.ComponentName);
        Assert.Equal("ReactiveUITK.FunctionStyle", set.Namespace);
        Assert.DoesNotContain(diags, d => d.Code == "UITKX0005");
    }

    [Fact]
    public void Directives_FunctionStyle_MissingReturn_EmitsUITKX2101()
    {
        const string src =
            """
            component CounterPanel {
                var (count, setCount) = useState(0);
                <Box />
            }
            """;

        ParseDirectives(src, out var diags);
        Assert.Contains(diags, d => d.Code == "UITKX2101");
    }

    [Fact]
    public void Directives_FunctionStyle_NonPascalName_EmitsUITKX2100()
    {
        const string src = "component counterPanel { return (<Box />); }";

        ParseDirectives(src, out var diags);
        Assert.Contains(diags, d => d.Code == "UITKX2100");
    }

    [Fact]
    public void Directives_FunctionStyle_ReturnNonMarkup_EmitsUITKX2102()
    {
        const string src =
            """
            component CounterPanel {
                var count = 1;
                return (count);
            }
            """;

        ParseDirectives(src, out var diags);
        Assert.Contains(diags, d => d.Code == "UITKX2102");
    }

    [Fact]
    public void Directives_FunctionStyle_MalformedReturn_EmitsUITKX2102()
    {
        const string src =
            """
            component CounterPanel {
                return count;
            }
            """;

        ParseDirectives(src, out var diags);
        Assert.Contains(diags, d => d.Code == "UITKX2102");
    }

    [Fact]
    public void Directives_FunctionStyle_WithTrailingDirective_EmitsUITKX2104()
    {
        const string src =
            """
            component CounterPanel {
                return (<Box />);
            }
            @namespace Test.NS
            """;

        ParseDirectives(src, out var diags);
        Assert.Contains(diags, d => d.Code == "UITKX2104");
    }

    [Fact]
    public void Directives_FunctionStyle_LeadingUsingLines_AreParsedIntoDirectiveSet()
    {
        const string src =
            """
            using MyGame.Models;
            using System.Collections.Generic;
            component PlayerHUD {
                return (<Box />);
            }
            """;

        var set = ParseDirectives(src, out var diags);

        Assert.True(set.IsFunctionStyle);
        Assert.DoesNotContain(diags, d => d.Severity == ParseSeverity.Error);
        Assert.Contains(set.Usings, u => u == "MyGame.Models");
        Assert.Contains(set.Usings, u => u == "System.Collections.Generic");
    }

    [Fact]
    public void Directives_FunctionStyle_InlineNamespaceDirective_UsedOverDefault()
    {
        const string src =
            """
            @namespace MyGame.UI
            component PlayerHUD {
                return (<Box />);
            }
            """;

        var set = ParseDirectives(src, out var diags);

        Assert.True(set.IsFunctionStyle);
        Assert.DoesNotContain(diags, d => d.Severity == ParseSeverity.Error);
        Assert.Equal("PlayerHUD", set.ComponentName);
        Assert.Equal("MyGame.UI", set.Namespace);
    }

    [Fact]
    public void Directives_FunctionStyle_InlineNamespaceDirective_AfterUsings()
    {
        const string src =
            """
            using System.Collections.Generic;
            @namespace MyGame.Screens
            component PlayerHUD {
                return (<Box />);
            }
            """;

        var set = ParseDirectives(src, out var diags);

        Assert.True(set.IsFunctionStyle);
        Assert.DoesNotContain(diags, d => d.Severity == ParseSeverity.Error);
        Assert.Equal("MyGame.Screens", set.Namespace);
        Assert.Contains(set.Usings, u => u == "System.Collections.Generic");
    }

    [Fact]
    public void Directives_FunctionStyle_InlineNamespaceDirective_TakesPriorityOverCompanion()
    {
        string dir = Path.Combine(
            Path.GetTempPath(),
            "uitkx-ns-priority-" + System.Guid.NewGuid().ToString("N")
        );
        Directory.CreateDirectory(dir);

        try
        {
            string uitkxPath = Path.Combine(dir, "PlayerHUD.uitkx");
            string companionPath = Path.Combine(dir, "PlayerHUD.cs");

            File.WriteAllText(
                companionPath,
                "namespace CompanionNamespace { public partial class PlayerHUD { } }"
            );

            const string src =
                """
                @namespace InlineNamespace
                component PlayerHUD {
                    return (<Box />);
                }
                """;

            var diags = new List<ParseDiagnostic>();
            var set = DirectiveParser.Parse(src, uitkxPath, diags);

            Assert.True(set.IsFunctionStyle);
            Assert.DoesNotContain(diags, d => d.Severity == ParseSeverity.Error);
            // Inline @namespace must win over the companion .cs
            Assert.Equal("InlineNamespace", set.Namespace);
        }
        finally
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, recursive: true);
        }
    }

    // ── Markup: elements ─────────────────────────────────────────────────────

    [Fact]
    public void Markup_SelfClosingElement_ProducesElementNode()
    {
        var nodes = ParseMarkup(Wrap("<label/>"), out _);

        var el = Assert.Single(nodes.OfType<ElementNode>());
        Assert.Equal("label", el.TagName);
        Assert.Empty(el.Children);
    }

    [Fact]
    public void Markup_FunctionStyle_ReturnMarkup_ProducesElementNode()
    {
        const string src =
            """
            component CounterPanel {
                var (count, setCount) = useState(0);
                return (
                    <Box><Label text={$"{count}"} /></Box>
                );
            }
            """;

        var nodes = ParseMarkup(src, out _);

        var box = Assert.Single(nodes.OfType<ElementNode>());
        Assert.Equal("Box", box.TagName);
    }

    [Fact]
    public void Markup_NestedElement_ProducesChildren()
    {
        var nodes = ParseMarkup(Wrap("<box><label/></box>"), out _);

        var root = Assert.Single(nodes.OfType<ElementNode>());
        Assert.Equal("box", root.TagName);

        var child = Assert.Single(root.Children.OfType<ElementNode>());
        Assert.Equal("label", child.TagName);
    }

    [Fact]
    public void Markup_StringAttribute_ProducesStringLiteralValue()
    {
        var nodes = ParseMarkup(Wrap("""<label text="Hello"/>"""), out _);

        var el = Assert.Single(nodes.OfType<ElementNode>());
        var attr = Assert.Single(el.Attributes, a => a.Name == "text");

        var value = Assert.IsType<StringLiteralValue>(attr.Value);
        Assert.Equal("Hello", value.Value);
    }

    [Fact]
    public void Markup_ExpressionAttribute_ProducesCSharpExpressionValue()
    {
        var nodes = ParseMarkup(Wrap("<label text={count}/>"), out _);

        var el = Assert.Single(nodes.OfType<ElementNode>());
        var attr = Assert.Single(el.Attributes, a => a.Name == "text");

        var value = Assert.IsType<CSharpExpressionValue>(attr.Value);
        Assert.Equal("count", value.Expression.Trim());
    }

    [Fact]
    public void Markup_BooleanShorthandAttribute_ProducesBooleanShorthandValue()
    {
        var nodes = ParseMarkup(Wrap("<toggle disabled/>"), out _);

        var el = Assert.Single(nodes.OfType<ElementNode>());
        var attr = Assert.Single(el.Attributes, a => a.Name == "disabled");

        Assert.IsType<BooleanShorthandValue>(attr.Value);
    }

    [Fact]
    public void Markup_MultipleAttributes_AllParsed()
    {
        var nodes = ParseMarkup(
            Wrap("""<button text="Click" onClick={handler}/>"""),
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
        var src = Wrap("@if (x > 0) { <label/> }");
        var nodes = ParseMarkup(src, out _);

        var ifNode = Assert.Single(nodes.OfType<IfNode>());
        Assert.Single(ifNode.Branches);
        Assert.Equal("x > 0", ifNode.Branches[0].Condition?.Trim());
    }

    [Fact]
    public void Markup_IfElse_ProducesTwoBranches()
    {
        var src = Wrap("@if (flag) { <label/> } @else { <button/> }");
        var nodes = ParseMarkup(src, out _);

        var ifNode = Assert.Single(nodes.OfType<IfNode>());
        Assert.Equal(2, ifNode.Branches.Length);
        Assert.Null(ifNode.Branches[1].Condition); // @else has no condition
    }

    [Fact]
    public void Markup_ForeachDirective_ProducesForeachNode()
    {
        var src = Wrap("@foreach (var item in items) { <label/> }");
        var nodes = ParseMarkup(src, out _);

        var forNode = Assert.Single(nodes.OfType<ForeachNode>());
        Assert.Contains("item", forNode.IteratorDeclaration);
        Assert.Contains("items", forNode.CollectionExpression);
    }

    [Fact]
    public void Markup_SwitchDirective_ProducesSwitchNode()
    {
        var src = Wrap(
            """
            @switch (mode) {
                @case 0: <label text="zero"/>
                @case 1: <label text="one"/>
            }
            """);
        var nodes = ParseMarkup(src, out _);

        var sw = Assert.Single(nodes.OfType<SwitchNode>());
        Assert.Contains("mode", sw.SwitchExpression);
        Assert.Equal(2, sw.Cases.Length);
    }

    [Fact]
    public void Markup_ForDirective_ParsesBreakAndContinueNodes()
    {
        var src = Wrap(
            """
            @for (var i = 0; i < 10; i++) {
                @continue;
                @break;
            }
            """);

        var nodes = ParseMarkup(src, out _);
        var forNode = Assert.Single(nodes.OfType<ForNode>());

        Assert.Contains(forNode.Body, n => n is ContinueNode);
        Assert.Contains(forNode.Body, n => n is BreakNode);
    }

    [Fact]
    public void Markup_BreakOutsideLoop_EmitsUnexpectedToken()
    {
        var src = Wrap("@break;\n<label/>");
        ParseMarkup(src, out var diags);

        Assert.Contains(diags, d => d.Code == "UITKX0300" && d.Message.Contains("@break"));
    }

    [Fact]
    public void Markup_ContinueInsideLoopIf_ParsesNestedContinueNode()
    {
        var src = Wrap(
            """
            @while (isRunning) {
                @if (skip) {
                    @continue;
                }
                <label />
            }
            """);

        var nodes = ParseMarkup(src, out _);
        var whileNode = Assert.Single(nodes.OfType<WhileNode>());
        var ifNode = Assert.Single(whileNode.Body.OfType<IfNode>());

        Assert.Contains(ifNode.Branches[0].Body, n => n is ContinueNode);
    }

    [Fact]
    public void Markup_CodeBlock_ProducesCodeBlockNode()
    {
        var src = Wrap("@code { var x = 1; }\n<box/>");
        var nodes = ParseMarkup(src, out _);

        var cb = Assert.Single(nodes.OfType<CodeBlockNode>());
        Assert.Contains("var x = 1;", cb.Code);
    }

    [Fact]
    public void Markup_CodeBlock_LineCommentedMarkup_IsIgnored()
    {
        var src = Wrap("""
            @code {
                // var node = <Box>
                //   <Label text="hi"/>
                // </Box>;
                var x = 1;
            }
            <box/>
            """);

        var nodes = ParseMarkup(src, out _);
        var cb = Assert.Single(nodes.OfType<CodeBlockNode>());

        Assert.Empty(cb.ReturnMarkups);
    }

    [Fact]
    public void Markup_CodeBlock_BlockCommentedMarkup_IsIgnored_ButLiveMarkupStillParsed()
    {
        var src = Wrap("""
            @code {
                /*
                var node = <Box>
                    <Label text="hi"/>
                </Box>;
                */
                var live = <Label text="ok"/>;
            }
            <box/>
            """);

        var nodes = ParseMarkup(src, out _);
        var cb = Assert.Single(nodes.OfType<CodeBlockNode>());

        var only = Assert.Single(cb.ReturnMarkups);
        Assert.Equal("Label", only.Element.TagName);
    }

    [Fact]
    public void Markup_InlineExpression_ProducesExpressionNode()
    {
        var src = Wrap("<box>@(someCall())</box>");
        var nodes = ParseMarkup(src, out _);

        var box = Assert.Single(nodes.OfType<ElementNode>());
        var expr = Assert.Single(box.Children.OfType<ExpressionNode>());
        Assert.Contains("someCall()", expr.Expression);
    }

    // ── Markup: line tracking ─────────────────────────────────────────────────

    [Fact]
    public void Markup_SourceLineIsOneBasedAndApproximate()
    {
        var src = Wrap("<label/>");
        var nodes = ParseMarkup(src, out _);

        var el = Assert.Single(nodes.OfType<ElementNode>());
        Assert.True(el.SourceLine >= 1, "SourceLine should be 1-based");
    }

    // ── Missing semicolon after paren-wrapped JSX in setup code ───────────────

    [Fact]
    public void SetupJsx_MissingSemicolon_EmitsCS1002()
    {
        const string src =
            """
            component Counter(int count = 0) {
              var inlineNode = (
                <VisualElement>
                  <Button text="-5" onClick={_ => setCount(count - 5)} />
                  <Button text="+5" onClick={_ => setCount(count + 5)} />
                </VisualElement>
              )

              return (<Box />);
            }
            """;

        var diags = new List<ParseDiagnostic>();
        var set = DirectiveParser.Parse(src, "test.uitkx", diags);

        // Debug output
        System.Console.WriteLine($"IsFunctionStyle: {set.IsFunctionStyle}");
        System.Console.WriteLine($"SetupCodeMarkupRanges: {(set.SetupCodeMarkupRanges.IsDefault ? 0 : set.SetupCodeMarkupRanges.Length)}");
        if (!set.SetupCodeMarkupRanges.IsDefaultOrEmpty)
        {
            foreach (var (s, e, l) in set.SetupCodeMarkupRanges)
                System.Console.WriteLine($"  Range: Start={s} End={e} Line={l}  src[Start-1]='{src[s-1]}' src[End]='{src[e]}'");
        }
        foreach (var d in diags)
            System.Console.WriteLine($"  [{d.Code}] L{d.SourceLine}:C{d.SourceColumn} {d.Message}");

        var cs1002 = diags.Where(d => d.Code == "CS1002").ToList();
        Assert.True(cs1002.Count > 0, "Expected CS1002 for missing ';' after paren-wrapped JSX block");
    }

    [Fact]
    public void SetupJsx_WithSemicolon_NoCS1002()
    {
        const string src =
            """
            component Counter(int count = 0) {
              var inlineNode = (
                <VisualElement>
                  <Button text="-5" onClick={_ => setCount(count - 5)} />
                  <Button text="+5" onClick={_ => setCount(count + 5)} />
                </VisualElement>
              );

              return (<Box />);
            }
            """;

        var diags = new List<ParseDiagnostic>();
        DirectiveParser.Parse(src, "test.uitkx", diags);

        var cs1002 = diags.Where(d => d.Code == "CS1002").ToList();
        Assert.Empty(cs1002);
    }
}
