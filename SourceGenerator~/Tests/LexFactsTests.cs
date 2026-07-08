using ReactiveUITK.Language.Parser;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests;

/// <summary>
/// Torture tests for the canonical lexer facts (<see cref="CSharpLexFacts"/>) that replaced
/// six independently-drifted mini-lexers. See FINAL_AUDIT_UITKX_FINDINGS.md, finding U-20.
/// </summary>
public class LexFactsTests
{
    private static (string Expr, int After) Brace(string src, int open) =>
        ExpressionExtractor.FromBrace(src, open);

    [Fact]
    public void DollarAt_InterpolatedVerbatim_QuotedBraceInHole_DoesNotCloseEarly()
    {
        // U-20 exact repro: a quoted "}" inside the interpolation hole of a $@"..." string
        // must NOT be mistaken for the end of the (mis-lexed-as-plain-verbatim) string.
        var input = "{ $@\"{a(\"}\")}\" }";
        var (expr, after) = Brace(input, 0);
        Assert.Equal("$@\"{a(\"}\")}\"", expr);
        Assert.Equal(input.Length, after);
    }

    [Fact]
    public void AtDollar_InterpolatedVerbatim_QuotedBraceInHole_DoesNotCloseEarly()
    {
        // Mirror of the above with the @$" ordering.
        var input = "{ @$\"{a(\"}\")}\" }";
        var (expr, after) = Brace(input, 0);
        Assert.Equal("@$\"{a(\"}\")}\"", expr);
        Assert.Equal(input.Length, after);
    }

    [Fact]
    public void PlainInterpolated_QuotedBraceInHole_DoesNotCloseEarly()
    {
        var input = "{ $\"{a(\"}\")}\" }";
        var (expr, after) = Brace(input, 0);
        Assert.Equal("$\"{a(\"}\")}\"", expr);
        Assert.Equal(input.Length, after);
    }

    [Fact]
    public void FormatCall_WithParenAndQuoteInHole_Balances()
    {
        var input = "{ $@\"{x.ToString(\"D\")}\" + \"(\" }";
        var (expr, after) = Brace(input, 0);
        Assert.Equal("$@\"{x.ToString(\"D\")}\" + \"(\"", expr);
        Assert.Equal(input.Length, after);
    }

    [Fact]
    public void CharLiteral_QuoteChar_DoesNotConfuseStringScan()
    {
        var input = "{ x == '\"' }";
        var (expr, after) = Brace(input, 0);
        Assert.Equal("x == '\"'", expr);
        Assert.Equal(input.Length, after);
    }

    [Fact]
    public void CharLiteral_EscapedQuote_DoesNotConfuseStringScan()
    {
        var input = "{ x == '\\'' }";
        var (expr, after) = Brace(input, 0);
        Assert.Equal("x == '\\''", expr);
        Assert.Equal(input.Length, after);
    }

    [Fact]
    public void UnterminatedString_DoesNotThrow_AndReportsUnclosed()
    {
        var input = "{ \"unterminated ";
        int close = CSharpLexFacts.FindMatchingClose(input, 1, input.Length, '{', '}', out bool found);
        Assert.False(found);
        Assert.Equal(input.Length, close);
    }

    [Fact]
    public void NestedHoles_WithDifferentBraceStyles_Balance()
    {
        var input = "{ $\"{(cond ? \"{a}\" : \"{b}\")}\" }";
        var (expr, after) = Brace(input, 0);
        Assert.Equal("$\"{(cond ? \"{a}\" : \"{b}\")}\"", expr);
        Assert.Equal(input.Length, after);
    }

    [Fact]
    public void ComputeMultilineStringLineMask_MarksInteriorLinesOnly()
    {
        var code = "var s = @\"line1\n    }\n  indented content\n\";\nvar t = 1;\n";
        var mask = CSharpLexFacts.ComputeMultilineStringLineMask(code);
        // Line 1 (index 0): "var s = @"line1   -> opening line, NOT masked (starts outside the string)
        // Line 2 (index 1): "    }"            -> masked (inside the verbatim string)
        // Line 3 (index 2): "  indented content" -> masked
        // Line 4 (index 3): "\";"              -> masked (closing quote is on this line, but the
        //                                          line START is still inside the string)
        // Line 5 (index 4): "var t = 1;"       -> NOT masked
        Assert.False(mask[0]);
        Assert.True(mask[1]);
        Assert.True(mask[2]);
        Assert.True(mask[3]);
        Assert.False(mask[4]);
    }

    [Fact]
    public void BuildLineStarts_And_OffsetToLineCol_RoundTrip()
    {
        var code = "abc\ndef\nghi";
        var starts = CSharpLexFacts.BuildLineStarts(code);
        Assert.Equal(new[] { 0, 4, 8 }, starts);
        Assert.Equal((2, 2), CSharpLexFacts.OffsetToLineCol(starts, 5)); // 'e' in "def"
    }
}
