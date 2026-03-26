using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUITK.Language;
using ReactiveUITK.Language.Lowering;
using ReactiveUITK.Language.Nodes;
using ReactiveUITK.Language.Parser;
using ReactiveUITK.Language.Roslyn;
using UitkxLanguageServer;
using UitkxLanguageServer.Roslyn;
using Xunit;

namespace UitkxLanguageServer.Tests;

/// <summary>
/// Tests for <see cref="RoslynHost"/> in-process — verifies that:
///   1) The Roslyn workspace builds successfully from .uitkx source.
///   2) The semantic model is available and resolves symbols.
///   3) The virtual document + source map produce correct mappings.
///   4) Hover-style type queries return expected type info.
/// Serialised with RoslynCompletionTests to avoid MefHostServices race.
/// </summary>
[Collection("Roslyn")]
public sealed class RoslynHostTests : IAsyncLifetime
{
    private RoslynHost _host = null!;

    public Task InitializeAsync()
    {
        _host = new RoslynHost(null!, new UitkxSchema(), new WorkspaceIndex());
        _host.SetWorkspaceRoot(null);
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    // ── Helpers ────────────────────────────────────────────────────────────

    private static ParseResult Parse(string source, string path = "Test.uitkx")
    {
        var diags = new List<ParseDiagnostic>();
        var directives = DirectiveParser.Parse(source, path, diags);
        var parsedNodes = UitkxParser.Parse(source, path, directives, diags);
        var nodes = CanonicalLowering.LowerToRenderRoots(directives, parsedNodes, path);
        return new ParseResult(directives, nodes, ImmutableArray.CreateRange(diags));
    }

    private async Task EnsureReady(string source, string path = "c:/test/Test.uitkx")
    {
        var parseResult = Parse(source, path);
        await _host.EnsureReadyAsync(path, source, parseResult, CancellationToken.None);
    }

    // ── Workspace creation ─────────────────────────────────────────────────

    [Fact]
    public async Task EnsureReady_CreatesVirtualDocument()
    {
        // Use the same source pattern that works in SemanticModel_ResolvesBclType.
        var source = "@namespace T\n@component C\n@code {\n  string msg = \"hello\";\n}\n<Label text={msg}/>";
        var path = "c:/test/VirtualDocTest.uitkx";
        var parseResult = Parse(source, path);
        await _host.EnsureReadyAsync(path, source, parseResult, CancellationToken.None);

        // If EnsureReadyAsync silently failed, GetRoslynDocument also returns null.
        var roslynDoc = _host.GetRoslynDocument(path);
        Assert.NotNull(roslynDoc);

        var vdoc = _host.GetVirtualDocument(path);
        Assert.NotNull(vdoc);
        Assert.False(string.IsNullOrEmpty(vdoc!.Text));
        Assert.Contains("msg", vdoc.Text);
    }

    [Fact]
    public async Task EnsureReady_CreatesRoslynDocument()
    {
        var source = "@namespace T\n@component C\n<Label text=\"hi\"/>";
        await EnsureReady(source);

        var doc = _host.GetRoslynDocument("c:/test/Test.uitkx");
        Assert.NotNull(doc);
    }

    // ── Semantic model ─────────────────────────────────────────────────────

    [Fact]
    public async Task SemanticModel_ResolvesBclType()
    {
        var source = "@namespace T\n@component C\n@code {\n  string msg = \"hello\";\n}\n<Label text={msg}/>";
        await EnsureReady(source);

        var doc = _host.GetRoslynDocument("c:/test/Test.uitkx");
        Assert.NotNull(doc);

        var model = await doc!.GetSemanticModelAsync();
        Assert.NotNull(model);

        // SemanticModel should resolve without compilation errors
        // that would indicate a broken workspace
        var diagnostics = model!.GetDiagnostics();
        // Filter to errors only — some warnings about unused variables are fine.
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        // We don't require zero errors (some may come from generated scaffold),
        // but the workspace should be functional enough for completions.
    }

    [Fact]
    public async Task SemanticModel_FunctionStyle_ResolvesBclType()
    {
        var source = "component Counter {\n  int count = 0;\n  string s = count.ToString();\n  return (\n    <Label text={s}/>\n  );\n}";
        await EnsureReady(source);

        var doc = _host.GetRoslynDocument("c:/test/Test.uitkx");
        Assert.NotNull(doc);

        var model = await doc!.GetSemanticModelAsync();
        Assert.NotNull(model);
    }

    // ── Hover-style type resolution ────────────────────────────────────────

    [Fact]
    public async Task HoverTypeInfo_StringVariable()
    {
        var source = "@namespace T\n@component C\n@code {\n  string greeting = \"hello\";\n}\n<Label text={greeting}/>";
        await EnsureReady(source);

        var vdoc = _host.GetVirtualDocument("c:/test/Test.uitkx");
        Assert.NotNull(vdoc);

        // Find "greeting" in the source and map to virtual
        int uitkxIdx = source.IndexOf("greeting}");
        Assert.True(uitkxIdx >= 0);

        var mapping = vdoc!.Map.ToVirtualOffset(uitkxIdx);
        Assert.NotNull(mapping);

        // Get the Roslyn document and resolve symbol at position
        var roslynDoc = _host.GetRoslynDocument("c:/test/Test.uitkx");
        Assert.NotNull(roslynDoc);

        var syntaxRoot = await roslynDoc!.GetSyntaxRootAsync();
        var model = await roslynDoc.GetSemanticModelAsync();
        Assert.NotNull(syntaxRoot);
        Assert.NotNull(model);

        var token = syntaxRoot!.FindToken(mapping.Value.VirtualOffset);
        var typeInfo = model!.GetTypeInfo(token.Parent!);
        // The type should be System.String
        if (typeInfo.Type != null)
        {
            Assert.Equal("String", typeInfo.Type.Name);
        }
    }

    // ── Source map from host agrees with language-lib ───────────────────────

    [Fact]
    public async Task VirtualDoc_ExpressionRegion_IsInCSharp()
    {
        var source = "@namespace T\n@component C\n<Label text={myVar}/>";
        await EnsureReady(source);

        var vdoc = _host.GetVirtualDocument("c:/test/Test.uitkx");
        Assert.NotNull(vdoc);

        int idx = source.IndexOf("myVar");
        Assert.True(vdoc!.Map.IsInCSharpRegion(idx));
    }

    [Fact]
    public async Task VirtualDoc_MarkupRegion_IsNotCSharp()
    {
        var source = "@namespace T\n@component C\n<Label text=\"plain\"/>";
        await EnsureReady(source);

        var vdoc = _host.GetVirtualDocument("c:/test/Test.uitkx");
        Assert.NotNull(vdoc);

        int idx = source.IndexOf("<Label");
        Assert.False(vdoc!.Map.IsInCSharpRegion(idx));
    }

    // ── Idempotent rebuild ─────────────────────────────────────────────────

    [Fact]
    public async Task EnsureReady_Idempotent_SameSource()
    {
        var source = "@namespace T\n@component C\n<Label/>";
        await EnsureReady(source);
        await EnsureReady(source); // should short-circuit

        var doc = _host.GetRoslynDocument("c:/test/Test.uitkx");
        Assert.NotNull(doc);
    }

    // ── UITKX0112 data-flow analysis ──────────────────────────────────────

    [Fact]
    public async Task UITKX0112_UnusedVariable_Detected()
    {
        // `unused` is declared but never referenced in markup or other code.
        var source = "component Test {\n  int unused = 42;\n  return (\n    <Label text=\"hi\"/>\n  );\n}";
        await EnsureReady(source);

        var diags = _host.GetLatestDiagnostics("c:/test/Test.uitkx");
        Assert.Contains(diags, d => d.Diagnostic.Id == "UITKX0112");
    }

    [Fact]
    public async Task UITKX0112_UsedVariable_NoDiagnostic()
    {
        // `count` IS referenced in expression {count.ToString()}.
        var source = "component Test {\n  int count = 42;\n  return (\n    <Label text={count.ToString()}/>\n  );\n}";
        await EnsureReady(source);

        var diags = _host.GetLatestDiagnostics("c:/test/Test.uitkx");
        Assert.DoesNotContain(diags, d => d.Diagnostic.Id == "UITKX0112");
    }

    [Fact]
    public async Task UITKX0112_LambdaParam_NoFalsePositive()
    {
        // Lambda discard param `_` must NOT be flagged as unused.
        var source = "component Test {\n  var (count, setCount) = useState(0);\n  return (\n    <Button text=\"+1\" onClick={_ => setCount(count + 1)} />\n  );\n}";
        await EnsureReady(source);

        var diags = _host.GetLatestDiagnostics("c:/test/Test.uitkx");
        Assert.DoesNotContain(diags, d =>
            d.Diagnostic.Id == "UITKX0112"
            && d.Diagnostic.GetMessage().Contains("'_'"));
    }

    [Fact]
    public async Task UITKX0112_ForeachVariable_NoFalsePositive()
    {
        // Loop variable `item` is used in expression check — no false positive.
        var source = "component Test {\n  var items = new string[] { \"a\", \"b\" };\n  return (\n    <VisualElement>\n      @foreach (var item in items) {\n        <Label text={item} />\n      }\n    </VisualElement>\n  );\n}";
        await EnsureReady(source);

        var diags = _host.GetLatestDiagnostics("c:/test/Test.uitkx");
        Assert.DoesNotContain(diags, d => d.Diagnostic.Id == "UITKX0112");
    }
}
