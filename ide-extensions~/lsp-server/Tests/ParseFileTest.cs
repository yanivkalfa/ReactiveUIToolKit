using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using ReactiveUITK.Language;
using ReactiveUITK.Language.Lowering;
using ReactiveUITK.Language.Parser;
using Xunit;
using Xunit.Abstractions;

namespace UitkxLanguageServer.Tests;

public sealed class ParseFileTest
{
    private readonly ITestOutputHelper _out;

    public ParseFileTest(ITestOutputHelper output) => _out = output;

    [Fact]
    public void ParseTestFile_NoDiagnosticErrors()
    {
        var filePath = Path.GetFullPath(
            Path.Combine(
                Directory.GetCurrentDirectory(),
                @"..\..\..\..\..\..\Samples\UITKX\Components\UitkxTestFileDoNotTouch\UitkxTestFileDoNotTouch.uitkx"));

        if (!File.Exists(filePath))
        {
            _out.WriteLine($"Skipping: file not found at {filePath}");
            return;
        }

        var source = File.ReadAllText(filePath);
        var diags = new List<ParseDiagnostic>();
        var directives = DirectiveParser.Parse(source, filePath, diags);

        _out.WriteLine($"MarkupStartIndex: {directives.MarkupStartIndex}");
        _out.WriteLine($"MarkupEndIndex: {directives.MarkupEndIndex}");
        _out.WriteLine($"MarkupStartLine: {directives.MarkupStartLine}");
        _out.WriteLine($"SetupCodeMarkupRanges: {directives.SetupCodeMarkupRanges.Length}");
        _out.WriteLine($"SetupCodeBareJsxRanges: {(directives.SetupCodeBareJsxRanges.IsDefaultOrEmpty ? 0 : directives.SetupCodeBareJsxRanges.Length)}");

        // Show the markup window
        if (directives.MarkupStartIndex < source.Length && directives.MarkupEndIndex <= source.Length)
        {
            var markupContent = source.Substring(
                directives.MarkupStartIndex,
                System.Math.Min(200, directives.MarkupEndIndex - directives.MarkupStartIndex));
            _out.WriteLine($"\nMarkup starts with: {markupContent.Substring(0, System.Math.Min(100, markupContent.Length))}...");
        }

        var parsedNodes = UitkxParser.Parse(source, filePath, directives, diags);
        _out.WriteLine($"\nParsed {parsedNodes.Length} root nodes");

        // Also parse setup code JSX blocks (mirrors DiagnosticsPublisher)
        var allSetupJsxRanges = directives.SetupCodeMarkupRanges;
        if (!directives.SetupCodeBareJsxRanges.IsDefaultOrEmpty)
        {
            allSetupJsxRanges = allSetupJsxRanges.IsDefaultOrEmpty
                ? directives.SetupCodeBareJsxRanges
                : allSetupJsxRanges.AddRange(directives.SetupCodeBareJsxRanges);
        }
        if (!allSetupJsxRanges.IsDefaultOrEmpty)
        {
            _out.WriteLine($"\nParsing {allSetupJsxRanges.Length} setup code JSX blocks:");
            foreach (var (jsxStart, jsxEnd, jsxLine) in allSetupJsxRanges)
            {
                var jsxDirectives = directives with
                {
                    MarkupStartIndex = jsxStart,
                    MarkupEndIndex = jsxEnd,
                    MarkupStartLine = jsxLine,
                };
                var preview = source.Substring(jsxStart, System.Math.Min(80, jsxEnd - jsxStart));
                _out.WriteLine($"  Block L{jsxLine} [{jsxStart}..{jsxEnd}]: {preview.Replace("\n", "\\n").Replace("\r", "")}...");
                var jsxNodes = UitkxParser.Parse(source, filePath, jsxDirectives, diags);
                _out.WriteLine($"    -> {jsxNodes.Length} nodes");
            }
        }

        _out.WriteLine($"\nTotal diagnostics: {diags.Count}");
        foreach (var d in diags)
        {
            _out.WriteLine($"  [{d.Severity}] {d.Code} L{d.SourceLine} C{d.SourceColumn}: {d.Message}");
        }

        // Check: no errors
        var errors = diags.FindAll(d => d.Severity == ParseSeverity.Error);
        if (errors.Count > 0)
        {
            _out.WriteLine($"\n*** {errors.Count} ERROR(S) FOUND ***");
            foreach (var e in errors)
                _out.WriteLine($"  ERROR: {e.Code} L{e.SourceLine}: {e.Message}");
        }

        Assert.Empty(errors);
    }
}
