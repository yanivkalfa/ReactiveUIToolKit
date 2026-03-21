using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ReactiveUITK.Language;
using ReactiveUITK.Language.Lowering;
using ReactiveUITK.Language.Nodes;
using ReactiveUITK.Language.Parser;
using ReactiveUITK.Language.SemanticTokens;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests;

/// <summary>
/// Tests for <see cref="SemanticTokensProvider"/> — verifies that the correct
/// semantic token types are produced for each UITKX syntax construct.
/// These are pure language-lib tests (no LSP server, no Roslyn).
/// </summary>
public sealed class SemanticTokenTests
{
    private static readonly SemanticTokensProvider _provider = new();

    // ── Helpers ────────────────────────────────────────────────────────────

    private static ParseResult Parse(string source)
    {
        var diags = new List<ParseDiagnostic>();
        var directives = DirectiveParser.Parse(source, "test.uitkx", diags);
        var parsedNodes = UitkxParser.Parse(source, "test.uitkx", directives, diags);
        var nodes = CanonicalLowering.LowerToRenderRoots(directives, parsedNodes, "test.uitkx");
        return new ParseResult(directives, nodes, ImmutableArray.CreateRange(diags));
    }

    private static SemanticTokenData[] GetTokens(string source) =>
        _provider.GetTokens(Parse(source), source);

    private static bool HasToken(SemanticTokenData[] tokens, string source, string tokenType, string text)
    {
        var lines = source.Split('\n');
        return tokens.Any(t =>
            t.TokenType == tokenType &&
            t.Line < lines.Length &&
            t.Column + t.Length <= lines[t.Line].Length &&
            lines[t.Line].Substring(t.Column, t.Length) == text);
    }

    // ── Directive-header tests ─────────────────────────────────────────────

    [Fact]
    public void DirectiveHeader_NamespaceKeyword_IsDirective()
    {
        var source = "@namespace My.App\n@component Foo\n<Label/>";
        var tokens = GetTokens(source);
        Assert.True(HasToken(tokens, source, SemanticTokenTypes.Directive, "@namespace"),
            "Expected @namespace to be a uitkxDirective token");
    }

    [Fact]
    public void DirectiveHeader_NamespaceValue_IsDirectiveName()
    {
        var source = "@namespace My.App\n@component Foo\n<Label/>";
        var tokens = GetTokens(source);
        Assert.True(HasToken(tokens, source, SemanticTokenTypes.DirectiveName, "My.App"),
            "Expected My.App to be a uitkxDirectiveName token");
    }

    [Fact]
    public void DirectiveHeader_ComponentKeyword_IsDirective()
    {
        var source = "@namespace My.App\n@component Foo\n<Label/>";
        var tokens = GetTokens(source);
        Assert.True(HasToken(tokens, source, SemanticTokenTypes.Directive, "@component"),
            "Expected @component to be a uitkxDirective token");
    }

    [Fact]
    public void DirectiveHeader_ComponentName_IsDirectiveName()
    {
        var source = "@namespace My.App\n@component Foo\n<Label/>";
        var tokens = GetTokens(source);
        Assert.True(HasToken(tokens, source, SemanticTokenTypes.DirectiveName, "Foo"),
            "Expected Foo to be a uitkxDirectiveName token");
    }

    // ── Element & attribute tests ──────────────────────────────────────────

    [Fact]
    public void ElementName_IsElement()
    {
        var source = "@namespace T\n@component C\n<Label text=\"hi\"/>";
        var tokens = GetTokens(source);
        Assert.True(HasToken(tokens, source, SemanticTokenTypes.Element, "Label"),
            "Expected Label to be a uitkxElement token");
    }

    [Fact]
    public void AttributeName_IsAttribute()
    {
        var source = "@namespace T\n@component C\n<Label text=\"hi\"/>";
        var tokens = GetTokens(source);
        Assert.True(HasToken(tokens, source, SemanticTokenTypes.Attribute, "text"),
            "Expected text to be a uitkxAttribute token");
    }

    [Fact]
    public void NestedElement_BothTagged()
    {
        var source = "@namespace T\n@component C\n<Box>\n  <Label text=\"hi\"/>\n</Box>";
        var tokens = GetTokens(source);
        Assert.True(HasToken(tokens, source, SemanticTokenTypes.Element, "Box"),
            "Expected Box to be a uitkxElement token");
        Assert.True(HasToken(tokens, source, SemanticTokenTypes.Element, "Label"),
            "Expected Label to be a uitkxElement token");
    }

    // ── Control flow tests ─────────────────────────────────────────────────

    [Fact]
    public void IfDirective_IsDirective()
    {
        var source = "@namespace T\n@component C\n@if (true) {\n  <Label/>\n}";
        var tokens = GetTokens(source);
        Assert.True(HasToken(tokens, source, SemanticTokenTypes.Directive, "@if"),
            "Expected @if to be a uitkxDirective token");
    }

    [Fact]
    public void ElseDirective_IsDirective()
    {
        var source = "@namespace T\n@component C\n@if (true) {\n  <Label/>\n} @else {\n  <Box/>\n}";
        var tokens = GetTokens(source);
        Assert.True(HasToken(tokens, source, SemanticTokenTypes.Directive, "@else"),
            "Expected @else to be a uitkxDirective token");
    }

    [Fact]
    public void ForeachDirective_IsDirective()
    {
        var source = "@namespace T\n@component C\n@foreach (var x in items) {\n  <Label key={x}/>\n}";
        var tokens = GetTokens(source);
        Assert.True(HasToken(tokens, source, SemanticTokenTypes.Directive, "@foreach"),
            "Expected @foreach to be a uitkxDirective token");
    }

    [Fact]
    public void ForDirective_IsDirective()
    {
        var source = "@namespace T\n@component C\n@for (int i = 0; i < 5; i++) {\n  <Label key={i}/>\n}";
        var tokens = GetTokens(source);
        Assert.True(HasToken(tokens, source, SemanticTokenTypes.Directive, "@for"),
            "Expected @for to be a uitkxDirective token");
    }

    [Fact]
    public void SwitchDirective_IsDirective()
    {
        var source = "@namespace T\n@component C\n@switch (mode) {\n  @case \"a\":\n    <Label/>\n}";
        var tokens = GetTokens(source);
        Assert.True(HasToken(tokens, source, SemanticTokenTypes.Directive, "@switch"),
            "Expected @switch to be a uitkxDirective token");
    }

    // ── Function-style component tests ─────────────────────────────────────

    [Fact]
    public void FunctionStyle_ComponentKeyword_IsDirective()
    {
        var source = "component Counter {\n  return (\n    <Label text=\"0\"/>\n  );\n}";
        var tokens = GetTokens(source);
        Assert.True(HasToken(tokens, source, SemanticTokenTypes.Directive, "component"),
            "Expected component to be a uitkxDirective token");
    }

    [Fact]
    public void FunctionStyle_ElementInReturn_IsElement()
    {
        var source = "component Counter {\n  return (\n    <Label text=\"0\"/>\n  );\n}";
        var tokens = GetTokens(source);
        Assert.True(HasToken(tokens, source, SemanticTokenTypes.Element, "Label"),
            "Expected Label inside return to be a uitkxElement token");
    }

    // ── Comment test ───────────────────────────────────────────────────────

    [Fact]
    public void JsxComment_IsComment()
    {
        var source = "@namespace T\n@component C\n<Box>\n  {/* hello */}\n</Box>";
        var tokens = GetTokens(source);
        Assert.True(tokens.Any(t => t.TokenType == SemanticTokenTypes.Comment),
            "Expected a comment token for {/* */}");
    }

    [Fact]
    public void JsxComment_SetterInsideComment_IsSuppressed()
    {
        // function-style component with useState and a commented-out setter call
        var source =
            "component Counter {\n" +
            "  var (count, setCount) = useState(0);\n" +
            "  return (\n" +
            "    <Box>\n" +
            "      {/* <Button onClick={_ => setCount(count + 1)} /> */}\n" +
            "    </Box>\n" +
            "  );\n" +
            "}";
        var tokens = GetTokens(source);
        // The comment line (line 4, 0-indexed) must NOT contain a Function token
        var line4Fn = tokens.Where(t => t.Line == 4 && t.TokenType == SemanticTokenTypes.Function).ToArray();
        Assert.Empty(line4Fn);
        // But the comment token itself must exist on that line
        Assert.True(tokens.Any(t => t.Line == 4 && t.TokenType == SemanticTokenTypes.Comment),
            "Expected a comment token on the JSX comment line");
    }

    [Fact]
    public void JsxComment_MultiLine_SettersSuppressed()
    {
        var source =
            "component Counter {\n" +
            "  var (count, setCount) = useState(0);\n" +
            "  return (\n" +
            "    <Box>\n" +
            "      {/* multi-line comment\n" +
            "        setCount(count + 1)\n" +
            "        setCount(count - 1)\n" +
            "      */}\n" +
            "    </Box>\n" +
            "  );\n" +
            "}";
        var tokens = GetTokens(source);
        // Lines 5 and 6 (0-indexed) are inside the comment — no Function tokens allowed
        var fnInComment = tokens.Where(t =>
            (t.Line == 5 || t.Line == 6) && t.TokenType == SemanticTokenTypes.Function).ToArray();
        Assert.Empty(fnInComment);
        // Comment tokens must exist on lines 4-7
        for (int l = 4; l <= 7; l++)
            Assert.True(tokens.Any(t => t.Line == l && t.TokenType == SemanticTokenTypes.Comment),
                $"Expected a comment token on line {l}");
    }

    [Fact]
    public void CStyleComment_SettersSuppressed()
    {
        // In code context, {/* */} is a C# block + comment — no JsxCommentNode
        var source =
            "component Counter {\n" +
            "  var (count, setCount) = useState(0);\n" +
            "  /* setCount(count + 1) */\n" +
            "  return (\n" +
            "    <Box />\n" +
            "  );\n" +
            "}";
        var tokens = GetTokens(source);
        // Line 2 (0-indexed) is inside a C-style comment — no Function token
        var line2Fn = tokens.Where(t => t.Line == 2 && t.TokenType == SemanticTokenTypes.Function).ToArray();
        Assert.Empty(line2Fn);
    }

    [Fact]
    public void LineComment_SetterSuppressed()
    {
        var source =
            "component Counter {\n" +
            "  var (count, setCount) = useState(0);\n" +
            "  // setCount(count + 1)\n" +
            "  return (\n" +
            "    <Box />\n" +
            "  );\n" +
            "}";
        var tokens = GetTokens(source);
        // Line 2 inside a // comment — no Function token
        var line2Fn = tokens.Where(t => t.Line == 2 && t.TokenType == SemanticTokenTypes.Function).ToArray();
        Assert.Empty(line2Fn);
    }

    [Fact]
    public void SetterOutsideComment_StillColored()
    {
        // Setter calls NOT in comments should still get Function tokens
        var source =
            "component Counter {\n" +
            "  var (count, setCount) = useState(0);\n" +
            "  return (\n" +
            "    <Box>\n" +
            "      <Button onClick={_ => setCount(count + 1)} />\n" +
            "    </Box>\n" +
            "  );\n" +
            "}";
        var tokens = GetTokens(source);
        // setCount on line 4 (the onClick handler) should still be a Function
        var line4Fn = tokens.Where(t => t.Line == 4 && t.TokenType == SemanticTokenTypes.Function).ToArray();
        Assert.NotEmpty(line4Fn);
    }

    // ── No tokens in wrong places ──────────────────────────────────────────

    [Fact]
    public void PlainStringAttribute_NoExpressionToken()
    {
        var source = "@namespace T\n@component C\n<Label text=\"hello\"/>";
        var tokens = GetTokens(source);
        Assert.False(tokens.Any(t => t.TokenType == SemanticTokenTypes.Expression),
            "Plain string attribute should not produce expression tokens");
    }

    // ── Using directive ────────────────────────────────────────────────────

    [Fact]
    public void UsingDirective_IsDirective()
    {
        var source = "@using System.Linq\n@namespace T\n@component C\n<Label/>";
        var tokens = GetTokens(source);
        Assert.True(HasToken(tokens, source, SemanticTokenTypes.Directive, "@using"),
            "Expected @using to be a uitkxDirective token");
    }
}
