using ReactiveUITK.Language.Formatter;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests;

public class FormatterTests
{
    private const string Header = "@namespace Test.NS\n@component MyComp\n";

    [Fact]
    public void Format_InsertsMissingSemicolon_ForUseStateAssignment()
    {
        var formatter = new AstFormatter(FormatterOptions.Default);
        var source =
            Header
            + """
                @code {
                    var (mode, setMode) = useState("normal")
                }
                <box/>
                """;

        var formatted = formatter.Format(source, "Test.uitkx");

        Assert.Contains("var (mode, setMode) = useState(\"normal\");", formatted);
    }

    [Fact]
    public void Format_DoesNotInsertMissingSemicolon_WhenDisabled()
    {
        var opts = FormatterOptions.Default with { InsertMissingSemicolonsOnFormat = false };
        var formatter = new AstFormatter(opts);
        var source =
            Header
            + """
                @code {
                    var (mode, setMode) = useState("normal")
                }
                <box/>
                """;

        var formatted = formatter.Format(source, "Test.uitkx");

        Assert.DoesNotContain("var (mode, setMode) = useState(\"normal\");", formatted);
        Assert.Contains("var (mode, setMode) = useState(\"normal\")", formatted);
    }

    [Fact]
    public void Format_DoesNotAppendSemicolon_ToControlFlowHeader()
    {
        var formatter = new AstFormatter(FormatterOptions.Default);
        var source =
            Header
            + """
                @code {
                    if (mode == "normal")
                    {
                        setMode("active")
                    }
                }
                <box/>
                """;

        var formatted = formatter.Format(source, "Test.uitkx");

        Assert.Contains("if (mode == \"normal\")", formatted);
        Assert.DoesNotContain("if (mode == \"normal\");", formatted);
        Assert.Contains("setMode(\"active\");", formatted);
    }

    [Fact]
    public void Format_InsertsMissingSemicolon_ForMultilineParenthesizedJsxAssignment()
    {
        var formatter = new AstFormatter(FormatterOptions.Default);
        var source =
            Header
            + """
                @code {
                    var component = (
                        <Box>
                            <Label text={"ok"} />
                        </Box>
                    )
                }
                <box/>
                """;

        var formatted = formatter.Format(source, "Test.uitkx");

        Assert.Contains("var component = (", formatted);
        Assert.Contains(")", formatted);
        Assert.Contains(");", formatted);
        Assert.DoesNotContain(");}", formatted);
        Assert.Contains(");\n}", formatted.Replace("\r\n", "\n"));
    }

    [Fact]
    public void Format_FunctionStyle_PreservesFunctionDeclarationForm()
    {
        var formatter = new AstFormatter(FormatterOptions.Default);
        var source =
            """
            component CounterPanel {
                var (count, setCount) = useState(0)
                return (
                    <Box>
                        <Button text={count.ToString()} onClick={() => setCount(count + 1)} />
                    </Box>
                );
            }
            """;

        var formatted = formatter.Format(source, "Test.uitkx").Replace("\r\n", "\n");

        Assert.StartsWith("component CounterPanel {\n", formatted);
        Assert.Contains("var (count, setCount) = useState(0);", formatted);
        Assert.Contains("\n    return (\n", formatted);
        Assert.Contains("\n    );\n", formatted);
        Assert.EndsWith("}\n", formatted);
        Assert.DoesNotContain("@component", formatted);
        Assert.DoesNotContain("@namespace", formatted);
    }

    [Fact]
    public void Format_FunctionStyle_WithNoSetup_FormatsReturnMarkup()
    {
        var formatter = new AstFormatter(FormatterOptions.Default);
        var source = "component HelloWorld { return (<Label text=\"Hi\"/>); }";

        var formatted = formatter.Format(source, "Test.uitkx").Replace("\r\n", "\n");

        Assert.Contains("component HelloWorld {", formatted);
        Assert.Contains("return (", formatted);
        Assert.Contains("<Label text=\"Hi\" />", formatted);
        Assert.Contains(");", formatted);
    }
}