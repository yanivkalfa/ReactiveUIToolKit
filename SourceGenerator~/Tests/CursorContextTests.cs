using System.Collections.Generic;
using System.Collections.Immutable;
using ReactiveUITK.Language;
using ReactiveUITK.Language.IntelliSense;
using ReactiveUITK.Language.Lowering;
using ReactiveUITK.Language.Nodes;
using ReactiveUITK.Language.Parser;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests;

/// <summary>
/// Tests for <see cref="AstCursorContext"/> — verifies that cursor positions
/// are correctly classified for IntelliSense (completions, hover, go-to-def).
/// Pure language-lib tests (no LSP, no Roslyn).
/// </summary>
public sealed class CursorContextTests
{
    // ── Helpers ────────────────────────────────────────────────────────────

    private static ParseResult Parse(string source)
    {
        var diags = new List<ParseDiagnostic>();
        var directives = DirectiveParser.Parse(source, "test.uitkx", diags);
        var parsedNodes = UitkxParser.Parse(source, "test.uitkx", directives, diags);
        var nodes = CanonicalLowering.LowerToRenderRoots(directives, parsedNodes, "test.uitkx");
        return new ParseResult(directives, nodes, ImmutableArray.CreateRange(diags));
    }

    /// <summary>
    /// Locate the cursor from a pipe character <c>|</c> embedded in the source.
    /// Returns the source with <c>|</c> removed, plus the 1-based line and 0-based column.
    /// </summary>
    private static (string source, int line1, int col0) ExtractCursor(string raw)
    {
        int idx = raw.IndexOf('|');
        Assert.True(idx >= 0, "Test source must contain a | cursor marker");
        string source = raw.Remove(idx, 1);

        int line1 = 1;
        int col0 = 0;
        for (int i = 0; i < idx; i++)
        {
            if (raw[i] == '\n')
            {
                line1++;
                col0 = 0;
            }
            else
            {
                col0++;
            }
        }
        return (source, line1, col0);
    }

    private static CursorContext FindAtPipe(string rawSource)
    {
        var (source, line1, col0) = ExtractCursor(rawSource);
        return AstCursorContext.Find(Parse(source), source, line1, col0);
    }

    // ── Tag names ──────────────────────────────────────────────────────────

    [Fact]
    public void TagName_AfterOpenAngle()
    {
        var ctx = FindAtPipe("component C {\n  return (\n    <|Label/>\n  );\n}");
        Assert.Equal(CursorKind.TagName, ctx.Kind);
    }

    [Fact]
    public void TagName_MidElement()
    {
        var ctx = FindAtPipe("component C {\n  return (\n    <Lab|el/>\n  );\n}");
        Assert.Equal(CursorKind.TagName, ctx.Kind);
    }

    [Fact]
    public void TagName_AtEnd()
    {
        var ctx = FindAtPipe("component C {\n  return (\n    <Label| />\n  );\n}");
        Assert.Equal(CursorKind.TagName, ctx.Kind);
    }

    // ── Attribute names ────────────────────────────────────────────────────

    [Fact]
    public void AttributeName_AfterSpace()
    {
        var ctx = FindAtPipe("component C {\n  return (\n    <Label |text=\"hi\"/>\n  );\n}");
        Assert.Equal(CursorKind.AttributeName, ctx.Kind);
    }

    [Fact]
    public void AttributeName_MidWord()
    {
        var ctx = FindAtPipe("component C {\n  return (\n    <Label te|xt=\"hi\"/>\n  );\n}");
        Assert.Equal(CursorKind.AttributeName, ctx.Kind);
    }

    [Fact]
    public void AttributeName_TagNamePreserved()
    {
        var ctx = FindAtPipe("component C {\n  return (\n    <Label te|xt=\"hi\"/>\n  );\n}");
        Assert.Equal("Label", ctx.TagName);
    }

    // ── Attribute values ───────────────────────────────────────────────────

    [Fact]
    public void AttributeValue_InsideQuotes()
    {
        var ctx = FindAtPipe("component C {\n  return (\n    <Label text=\"h|i\"/>\n  );\n}");
        Assert.Equal(CursorKind.AttributeValue, ctx.Kind);
    }

    [Fact]
    public void AttributeValue_InsideBraces()
    {
        var ctx = FindAtPipe("component C {\n  return (\n    <Label text={|val}/>\n  );\n}");
        Assert.Equal(CursorKind.AttributeValue, ctx.Kind);
    }

    // ── Control flow ───────────────────────────────────────────────────────

    [Fact]
    public void ControlFlowName_AfterAtInMarkup()
    {
        var ctx = FindAtPipe("component C {\n  return (\n    <Box>\n    @|if (true) {\n      <Label/>\n    }\n    </Box>\n  );\n}");
        Assert.Equal(CursorKind.ControlFlowName, ctx.Kind);
    }

    [Fact]
    public void ControlFlowName_MidKeyword()
    {
        var ctx = FindAtPipe("component C {\n  return (\n    <Box>\n    @fore|ach (var x in items) {\n      <Label key={x}/>\n    }\n    </Box>\n  );\n}");
        Assert.Equal(CursorKind.ControlFlowName, ctx.Kind);
    }

    // ── CSharp expression ──────────────────────────────────────────────────

    [Fact]
    public void CSharpExpression_InlineAtExpr()
    {
        // @(expr) in markup body — an inline C# expression
        var ctx = FindAtPipe("component C {\n  return (\n    <Box>\n      @(my|Var)\n    </Box>\n  );\n}");
        Assert.Equal(CursorKind.CSharpExpression, ctx.Kind);
    }

    // ── Function-style component ───────────────────────────────────────────

    [Fact]
    public void CSharpCodeBlock_FunctionSetup()
    {
        var ctx = FindAtPipe("component Foo {\n  int x| = 5;\n  return (\n    <Label/>\n  );\n}");
        Assert.Equal(CursorKind.CSharpCodeBlock, ctx.Kind);
    }

    [Fact]
    public void TagName_FunctionReturnMarkup()
    {
        var ctx = FindAtPipe("component Foo {\n  return (\n    <Lab|el/>\n  );\n}");
        Assert.Equal(CursorKind.TagName, ctx.Kind);
    }
}
