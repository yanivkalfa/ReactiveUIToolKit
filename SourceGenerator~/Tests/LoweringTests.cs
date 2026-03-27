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
    public void CanonicalLowering_FunctionStyle_TernaryJsxInSetup_ProducesReturnMarkupNodes()
    {
        const string src =
            """
            component PortalDemo {
                var target = useRef<VisualElement>();
                var portalNode = target != null
                    ? ( <Portal target={target}><Label text="inside" /></Portal> )
                    : null;

                return (
                    <Box>
                        @(portalNode)
                    </Box>
                );
            }
            """;

        var diags = new System.Collections.Generic.List<ParseDiagnostic>();
        var directives = DirectiveParser.Parse(src, "PortalDemo.uitkx", diags);
        var parsed = UitkxParser.Parse(src, "PortalDemo.uitkx", directives, diags);

        var lowered = CanonicalLowering.LowerToRenderRoots(directives, parsed, "PortalDemo.uitkx");

        var code = Assert.IsType<CodeBlockNode>(lowered[0]);
        Assert.Contains("var portalNode", code.Code);
        Assert.NotEmpty(code.ReturnMarkups);
        Assert.Contains(code.ReturnMarkups, rm => rm.Element.TagName == "Portal");
    }

}
