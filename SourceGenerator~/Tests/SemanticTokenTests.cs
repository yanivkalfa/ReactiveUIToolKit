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
