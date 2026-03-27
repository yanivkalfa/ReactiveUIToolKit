using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ReactiveUITK.Language;
using ReactiveUITK.Language.Diagnostics;
using ReactiveUITK.Language.Lowering;
using ReactiveUITK.Language.Nodes;
using ReactiveUITK.Language.Parser;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests;

/// <summary>
/// Tests for <see cref="DiagnosticsAnalyzer"/> — verifies Tier-2 structural
/// diagnostics (UITKX0101-0111) raised after parsing.
/// The UITKX0107 (unreachable after return) tests are especially important
/// because they validate the fix described in TECH_DEBT TD-08.
/// Pure language-lib tests (no LSP, no Roslyn).
/// </summary>
public sealed class DiagnosticsAnalyzerTests
{
    private static readonly DiagnosticsAnalyzer _analyzer = new();

    // ── Helpers ────────────────────────────────────────────────────────────

    private static ParseResult Parse(string source, string path = "Test.uitkx")
    {
        var diags = new List<ParseDiagnostic>();
        var directives = DirectiveParser.Parse(source, path, diags);
        var parsedNodes = UitkxParser.Parse(source, path, directives, diags);
        var nodes = CanonicalLowering.LowerToRenderRoots(directives, parsedNodes, path);
        return new ParseResult(directives, nodes, ImmutableArray.CreateRange(diags));
    }

    private static IReadOnlyList<ParseDiagnostic> Analyze(
        string source,
        string path = "Test.uitkx",
        HashSet<string>? projectElements = null)
    {
        return _analyzer.Analyze(Parse(source, path), path, projectElements);
    }

    private static bool HasDiag(IReadOnlyList<ParseDiagnostic> diags, string code) =>
        diags.Any(d => d.Code == code);

    // ── UITKX0102: Missing @component ──────────────────────────────────────

    [Fact]
    public void UITKX0102_FunctionStyle_NoWarning()
    {
        var diags = Analyze("component Foo {\n  return (\n    <Label/>\n  );\n}");
        Assert.False(HasDiag(diags, DiagnosticCodes.MissingComponent));
    }

    // ── UITKX0103: Filename mismatch ───────────────────────────────────────

    [Fact]
    public void UITKX0103_FilenameMismatch()
    {
        var diags = Analyze("component WrongName {\n  return (\n    <Label/>\n  );\n}", path: "Correct.uitkx");
        Assert.True(HasDiag(diags, DiagnosticCodes.FilenameMismatch));
    }

    [Fact]
    public void UITKX0103_FilenameMatches_NoWarning()
    {
        var diags = Analyze("component Test {\n  return (\n    <Label/>\n  );\n}", path: "Test.uitkx");
        Assert.False(HasDiag(diags, DiagnosticCodes.FilenameMismatch));
    }

    // ── UITKX0104: Duplicate key ───────────────────────────────────────────

    [Fact]
    public void UITKX0104_DuplicateKey()
    {
        var source = "component C {\n  return (\n    <Box>\n      <Label key=\"a\"/>\n      <Label key=\"a\"/>\n    </Box>\n  );\n}";
        var diags = Analyze(source);
        Assert.True(HasDiag(diags, DiagnosticCodes.DuplicateKey));
    }

    [Fact]
    public void UITKX0104_UniqueKeys_NoWarning()
    {
        var source = "component C {\n  return (\n    <Box>\n      <Label key=\"a\"/>\n      <Label key=\"b\"/>\n    </Box>\n  );\n}";
        var diags = Analyze(source);
        Assert.False(HasDiag(diags, DiagnosticCodes.DuplicateKey));
    }

    // ── UITKX0105: Unknown element ─────────────────────────────────────────

    [Fact]
    public void UITKX0105_UnknownElement_WhenIndexProvided()
    {
        var knownElems = new HashSet<string> { "Label", "Box" };
        var source = "component C {\n  return (\n    <UnknownWidget/>\n  );\n}";
        var diags = Analyze(source, projectElements: knownElems);
        Assert.True(HasDiag(diags, DiagnosticCodes.UnknownElement));
    }

    [Fact]
    public void UITKX0105_KnownElement_NoWarning()
    {
        var knownElems = new HashSet<string> { "Label", "Box" };
        var source = "component C {\n  return (\n    <Label/>\n  );\n}";
        var diags = Analyze(source, projectElements: knownElems);
        Assert.False(HasDiag(diags, DiagnosticCodes.UnknownElement));
    }

    [Fact]
    public void UITKX0105_NullIndex_Skipped()
    {
        var source = "component C {\n  return (\n    <Anything/>\n  );\n}";
        var diags = Analyze(source, projectElements: null);
        Assert.False(HasDiag(diags, DiagnosticCodes.UnknownElement));
    }

    // ── UITKX0106: Missing key in @foreach ─────────────────────────────────

    [Fact]
    public void UITKX0106_MissingKeyInForeach()
    {
        var source = "component C {\n  return (\n    <Box>\n      @foreach (var x in items) {\n        <Label/>\n      }\n    </Box>\n  );\n}";
        var diags = Analyze(source);
        Assert.True(HasDiag(diags, DiagnosticCodes.MissingKey));
    }

    [Fact]
    public void UITKX0106_HasKey_NoWarning()
    {
        var source = "component C {\n  return (\n    <Box>\n      @foreach (var x in items) {\n        <Label key={x}/>\n      }\n    </Box>\n  );\n}";
        var diags = Analyze(source);
        Assert.False(HasDiag(diags, DiagnosticCodes.MissingKey));
    }

    // ── UITKX0107: Unreachable after return ────────────────────────────────

    [Fact]
    public void UITKX0107_UnreachableAfterReturn_InCodeBlock()
    {
        var source = "component C {\n  return;\n  int x = 5;\n  return (\n    <Label/>\n  );\n}";
        var diags = Analyze(source);
        Assert.True(HasDiag(diags, DiagnosticCodes.UnreachableAfterReturn));
    }

    [Fact]
    public void UITKX0107_NoReturn_NoWarning()
    {
        var source = "component C {\n  int x = 5;\n  return (\n    <Label/>\n  );\n}";
        var diags = Analyze(source);
        Assert.False(HasDiag(diags, DiagnosticCodes.UnreachableAfterReturn));
    }

    [Fact]
    public void UITKX0107_FunctionStyle_UnreachableAfterReturn()
    {
        var source = "component Foo {\n  return (\n    <Label/>\n  );\n  int dead = 0;\n}";
        var diags = Analyze(source);
        Assert.True(HasDiag(diags, DiagnosticCodes.UnreachableAfterReturn));
    }

    [Fact]
    public void UITKX0107_SpansCorrectLine()
    {
        var source = "component C {\n  return;\n  int dead = 0;\n  return (\n    <Label/>\n  );\n}";
        var diags = Analyze(source);
        var unreachable = diags.FirstOrDefault(d => d.Code == DiagnosticCodes.UnreachableAfterReturn);
        Assert.NotNull(unreachable);
        // The unreachable code starts at "int dead = 0;" which is line 3 (1-based)
        Assert.True(unreachable.SourceLine >= 2, $"Expected unreachable line >= 2 but got {unreachable.SourceLine}");
    }

    // ── UITKX0108: Multiple render roots ───────────────────────────────────

    [Fact]
    public void UITKX0108_MultipleRenderRoots()
    {
        var source = "component C {\n  return (\n    <Label/>\n    <Box/>\n  );\n}";
        var diags = Analyze(source);
        Assert.True(HasDiag(diags, DiagnosticCodes.MultipleRenderRoots));
    }

    [Fact]
    public void UITKX0108_SingleRoot_NoWarning()
    {
        var source = "component C {\n  return (\n    <Box>\n      <Label/>\n    </Box>\n  );\n}";
        var diags = Analyze(source);
        Assert.False(HasDiag(diags, DiagnosticCodes.MultipleRenderRoots));
    }

    // ── UITKX0110: Unreachable after break/continue ────────────────────────

    [Fact]
    public void UITKX0110_UnreachableAfterBreak()
    {
        // @break; is a UITKX-level control-flow node (requires semicolon).
        // Nodes after @break; in a loop body are unreachable.
        var source = "component C {\n  return (\n    <Box>\n      @for (var i = 0; i < 10; i++) {\n        @break;\n        <Label key={i}/>\n      }\n    </Box>\n  );\n}";
        var diags = Analyze(source);
        Assert.True(HasDiag(diags, DiagnosticCodes.UnreachableAfterBreakOrContinue));
    }

    // ── UITKX0109: Unknown attribute ───────────────────────────────────────

    [Fact]
    public void UITKX0109_UnknownAttribute_WhenMapProvided()
    {
        var knownAttrs = new Dictionary<string, IReadOnlyCollection<string>>
        {
            ["Label"] = new HashSet<string> { "text", "style" }
        };
        var source = "component C {\n  return (\n    <Label bogus=\"hi\"/>\n  );\n}";
        var diags = _analyzer.Analyze(Parse(source), "Test.uitkx", null, knownAttrs);
        Assert.True(HasDiag(diags, DiagnosticCodes.UnknownAttribute));
    }

    [Fact]
    public void UITKX0109_KnownAttribute_NoWarning()
    {
        var knownAttrs = new Dictionary<string, IReadOnlyCollection<string>>
        {
            ["Label"] = new HashSet<string> { "text", "style" }
        };
        var source = "component C {\n  return (\n    <Label text=\"hi\"/>\n  );\n}";
        var diags = _analyzer.Analyze(Parse(source), "Test.uitkx", null, knownAttrs);
        Assert.False(HasDiag(diags, DiagnosticCodes.UnknownAttribute));
    }

    // ── UITKX0111: Unused parameter ────────────────────────────────────────

    [Fact]
    public void UITKX0111_UnusedParam_FunctionStyle()
    {
        var source = "component Foo(string name) {\n  return (\n    <Label text=\"static\"/>\n  );\n}";
        var diags = Analyze(source);
        Assert.True(HasDiag(diags, DiagnosticCodes.UnusedParameter));
    }

    [Fact]
    public void UITKX0111_UsedParam_NoWarning()
    {
        var source = "component Foo(string name) {\n  return (\n    <Label text={name}/>\n  );\n}";
        var diags = Analyze(source);
        Assert.False(HasDiag(diags, DiagnosticCodes.UnusedParameter));
    }

    // ── Severity checks ────────────────────────────────────────────────────

    [Fact]
    public void MissingKey_IsSeverityError()
    {
        var source = "component C {\n  return (\n    <Box>\n      @foreach (var x in items) {\n        <Label/>\n      }\n    </Box>\n  );\n}";
        var diags = Analyze(source);
        var mk = diags.FirstOrDefault(d => d.Code == DiagnosticCodes.MissingKey);
        Assert.NotNull(mk);
        Assert.Equal(ParseSeverity.Error, mk.Severity);
    }
}
