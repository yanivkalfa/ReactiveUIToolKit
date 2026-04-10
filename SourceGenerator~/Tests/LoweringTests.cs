using ReactiveUITK.Language;
using ReactiveUITK.Language.Lowering;
using ReactiveUITK.Language.Nodes;
using ReactiveUITK.Language.Parser;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests;

public class LoweringTests
{
    [Fact]
    public void CanonicalLowering_PassThrough_ReturnsParsedRootsUnchanged()
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

        // CanonicalLowering is now a pass-through — roots are unchanged
        Assert.Equal(parsed.Length, lowered.Length);
        for (int i = 0; i < parsed.Length; i++)
            Assert.Same(parsed[i], lowered[i]);
    }
}
