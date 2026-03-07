using System.Linq;
using ReactiveUITK.Language;
using ReactiveUITK.Language.Lowering;
using ReactiveUITK.Language.Nodes;
using ReactiveUITK.Language.Parser;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests;

public class LoweringTests
{
    [Fact]
    public void CanonicalLowering_FunctionStyle_HoistsSetupCodeAsFirstCodeBlock()
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

        var diags = new System.Collections.Generic.List<ParseDiagnostic>();
        var directives = DirectiveParser.Parse(src, "CounterPanel.uitkx", diags);
        var parsed = UitkxParser.Parse(src, "CounterPanel.uitkx", directives, diags);

        var lowered = CanonicalLowering.LowerToRenderRoots(directives, parsed, "CounterPanel.uitkx");

        var code = Assert.IsType<CodeBlockNode>(lowered[0]);
        Assert.Contains("useState", code.Code);
        Assert.Contains(lowered, n => n is ElementNode);
    }

    [Fact]
    public void CanonicalLowering_FunctionStyle_SetupMarkupAssignment_ProducesReturnMarkupNodes()
    {
        const string src =
            """
            component CounterPanel {
                var fragment = (
                    <Box>
                        <Label text="head" />
                    </Box>
                );

                return (
                    <Box>
                        @(fragment)
                    </Box>
                );
            }
            """;

        var diags = new System.Collections.Generic.List<ParseDiagnostic>();
        var directives = DirectiveParser.Parse(src, "CounterPanel.uitkx", diags);
        var parsed = UitkxParser.Parse(src, "CounterPanel.uitkx", directives, diags);

        var lowered = CanonicalLowering.LowerToRenderRoots(directives, parsed, "CounterPanel.uitkx");

        var code = Assert.IsType<CodeBlockNode>(lowered[0]);
        Assert.Contains("var fragment", code.Code);
        Assert.NotEmpty(code.ReturnMarkups);
        Assert.Contains(code.ReturnMarkups, rm => rm.Element.TagName == "Box");
    }

    [Fact]
    public void CanonicalLowering_DirectiveStyle_DoesNotInjectCodeBlock()
    {
        const string src = "@namespace Test.NS\n@component MyComp\n<Box />";

        var diags = new System.Collections.Generic.List<ParseDiagnostic>();
        var directives = DirectiveParser.Parse(src, "MyComp.uitkx", diags);
        var parsed = UitkxParser.Parse(src, "MyComp.uitkx", directives, diags);

        var lowered = CanonicalLowering.LowerToRenderRoots(directives, parsed, "MyComp.uitkx");

        Assert.DoesNotContain(lowered, n => n is CodeBlockNode);
        Assert.Single(lowered.OfType<ElementNode>());
    }

    [Fact]
    public void CanonicalLowering_FunctionAndDirectiveStyles_ProduceEquivalentSemanticShape()
    {
        const string functionStyle =
            """
            component CounterPanel {
                var (count, setCount) = useState(0);
                return (
                    <Box>
                        @if (count > 0) {
                            <Label text={$"{count}"} />
                        }
                    </Box>
                );
            }
            """;

        const string directiveStyle =
            """
            @namespace ReactiveUITK.FunctionStyle
            @component CounterPanel
            @code {
                var (count, setCount) = useState(0);
            }
            <Box>
                @if (count > 0) {
                    <Label text={$"{count}"} />
                }
            </Box>
            """;

        var functionDiags = new System.Collections.Generic.List<ParseDiagnostic>();
        var functionDirectives = DirectiveParser.Parse(
            functionStyle,
            "CounterPanel.uitkx",
            functionDiags
        );
        var functionParsed = UitkxParser.Parse(
            functionStyle,
            "CounterPanel.uitkx",
            functionDirectives,
            functionDiags
        );
        var functionLowered = CanonicalLowering.LowerToRenderRoots(
            functionDirectives,
            functionParsed,
            "CounterPanel.uitkx"
        );

        var directiveDiags = new System.Collections.Generic.List<ParseDiagnostic>();
        var directiveDirectives = DirectiveParser.Parse(
            directiveStyle,
            "CounterPanel.uitkx",
            directiveDiags
        );
        var directiveParsed = UitkxParser.Parse(
            directiveStyle,
            "CounterPanel.uitkx",
            directiveDirectives,
            directiveDiags
        );
        var directiveLowered = CanonicalLowering.LowerToRenderRoots(
            directiveDirectives,
            directiveParsed,
            "CounterPanel.uitkx"
        );

        string functionShape = BuildSemanticShape(functionLowered);
        string directiveShape = BuildSemanticShape(directiveLowered);

        Assert.Equal(directiveShape, functionShape);
    }

    private static string BuildSemanticShape(System.Collections.Immutable.ImmutableArray<AstNode> roots)
    {
        var parts = new System.Collections.Generic.List<string>();
        foreach (var node in roots)
            AppendNode(parts, node);
        return string.Join("|", parts);
    }

    private static void AppendNode(System.Collections.Generic.List<string> parts, AstNode node)
    {
        switch (node)
        {
            case CodeBlockNode code:
                parts.Add($"Code:{NormalizeWhitespace(code.Code)}");
                break;
            case ElementNode element:
                parts.Add($"Element:{element.TagName}");
                foreach (var attr in element.Attributes)
                    parts.Add($"Attr:{attr.Name}={FormatValue(attr.Value)}");
                foreach (var child in element.Children)
                    AppendNode(parts, child);
                parts.Add($"EndElement:{element.TagName}");
                break;
            case IfNode ifNode:
                parts.Add("If");
                foreach (var branch in ifNode.Branches)
                {
                    parts.Add($"Branch:{NormalizeWhitespace(branch.Condition ?? "else")}");
                    foreach (var child in branch.Body)
                        AppendNode(parts, child);
                }
                parts.Add("EndIf");
                break;
            case TextNode text:
                parts.Add($"Text:{NormalizeWhitespace(text.Content)}");
                break;
            case ExpressionNode expr:
                parts.Add($"Expr:{NormalizeWhitespace(expr.Expression)}");
                break;
            default:
                parts.Add($"Node:{node.GetType().Name}");
                break;
        }
    }

    private static string FormatValue(AttributeValue value) =>
        value switch
        {
            StringLiteralValue s => $"S:{s.Value}",
            CSharpExpressionValue e => $"E:{NormalizeWhitespace(e.Expression)}",
            BooleanShorthandValue => "B:true",
            _ => value.GetType().Name,
        };

    private static string NormalizeWhitespace(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return string.Join(" ", value.Split((char[])null!, System.StringSplitOptions.RemoveEmptyEntries));
    }
}
