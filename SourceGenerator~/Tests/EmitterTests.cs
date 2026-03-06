using System.Linq;
using ReactiveUITK.SourceGenerator.Tests.Helpers;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests;

/// <summary>
/// Integration tests that run the full <see cref="UitkxGenerator"/> pipeline
/// and assert properties of the generated C# source text.
/// </summary>
public class EmitterTests
{
    private const string Header = "@namespace Test.NS\n@component MyComp\n";

    // ── Element emission ──────────────────────────────────────────────────────

    [Fact]
    public void SimpleElement_GeneratesVCall()
    {
        var result = GeneratorTestHelper.Run(Header + "<label/>");

        Assert.True(result.SourceWasProduced);
        Assert.True(result.SourceContains("V.Label("), "Expected a V.Label( call");
    }

    [Fact]
    public void StringAttribute_AppearsInVCall()
    {
        var result = GeneratorTestHelper.Run(Header + """<label text="Hello World"/>""");

        Assert.True(
            result.SourceContains("Hello World"),
            "Expected string literal in generated source"
        );
    }

    [Fact]
    public void ExpressionAttribute_AppearsVerbatim()
    {
        var result = GeneratorTestHelper.Run(Header + "<label text={count}/>");

        Assert.True(
            result.SourceContains("count"),
            "Expected expression verbatim in generated source"
        );
    }

    [Fact]
    public void NestedElements_GeneratesNestedVCalls()
    {
        var result = GeneratorTestHelper.Run(Header + "<box><label/></box>");

        Assert.True(result.SourceContains("V.Box("), "Expected V.Box call");
        Assert.True(result.SourceContains("V.Label("), "Expected V.Label call inside box");
    }

    // ── Namespace / class structure ──────────────────────────────────────────

    [Fact]
    public void Namespace_EmittedInGeneratedSource()
    {
        var result = GeneratorTestHelper.Run(Header + "<box/>");

        Assert.True(result.SourceContains("namespace Test.NS"), "Expected namespace declaration");
    }

    [Fact]
    public void ClassName_MatchesComponentDirective()
    {
        var result = GeneratorTestHelper.Run(Header + "<box/>");

        Assert.True(
            result.SourceContains("partial class MyComp"),
            "Expected partial class named MyComp"
        );
    }

    // ── @code hoisting ───────────────────────────────────────────────────────

    [Fact]
    public void CodeBlock_AppearsBeforeReturn()
    {
        const string src = Header + "@code { var x = 42; }\n<box/>";
        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced);
        var generated = result.GeneratedSource!;

        int codePos = generated.IndexOf("var x = 42;", System.StringComparison.Ordinal);
        // Single-root: return <element>; — search for the V. call return in Render
        // (avoids matching the 'return' inside the __C helper generated above Render)
        int returnPos = generated.IndexOf("return V.", System.StringComparison.Ordinal);

        Assert.True(codePos >= 0, "@code content not found in generated source");
        Assert.True(returnPos >= 0, "return statement not found in generated source");
        Assert.True(codePos < returnPos, "@code block must appear before the return statement");
    }

    // ── Control flow ─────────────────────────────────────────────────────────

    [Fact]
    public void IfDirective_GeneratesConditionalExpression()
    {
        const string src = Header + "@if (flag) { <label/> }";
        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced);
        // @if → ternary; the condition should appear in the generated source
        Assert.True(result.SourceContains("flag"), "Condition should appear verbatim");
        Assert.True(
            result.SourceContains("? ")
                || result.SourceContains("?V.")
                || result.SourceContains("?("),
            "Ternary operator expected for @if"
        );
    }

    [Fact]
    public void ForeachDirective_GeneratesSelectCall()
    {
        const string src = Header + "@foreach (var item in items) { <label key={item.Id}/> }";
        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced);
        Assert.True(result.SourceContains(".Select("), "Expected .Select( for @foreach");
        Assert.True(result.SourceContains(".ToArray()"), "Expected .ToArray() after .Select(");
    }

    [Fact]
    public void ForDirective_WithLoopFlow_EmitsBreakAndContinueStatements()
    {
        const string src =
            Header
            + """
                @for (var i = 0; i < 10; i++) {
                    @if (i < 3) { @continue; }
                    <label text={i.ToString()} />
                    @if (i > 6) { @break; }
                }
                """;

        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced);
        Assert.True(result.SourceContains("for (var i = 0; i < 10; i++)"), "Expected for loop");
        Assert.True(result.SourceContains("continue;"), "Expected continue statement");
        Assert.True(result.SourceContains("break;"), "Expected break statement");
    }

    [Fact]
    public void WhileDirective_WithLoopFlow_EmitsContinueStatement()
    {
        const string src =
            Header
            + """
                @while (running) {
                    @if (skip) { @continue; }
                    <label />
                }
                """;

        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced);
        Assert.True(result.SourceContains("while (running)"), "Expected while loop");
        Assert.True(result.SourceContains("continue;"), "Expected continue statement");
    }

    [Fact]
    public void SwitchDirective_GeneratesSwitchExpression()
    {
        const string src =
            Header
            + """
                @switch (mode) {
                    @case 0: <label text="zero"/>
                    @case 1: <label text="one"/>
                }
                """;
        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced);
        Assert.True(result.SourceContains("switch"), "Expected switch expression");
        // Emitter generates: _ => (global::ReactiveUITK.Core.VirtualNode)null,
        Assert.True(
            result.SourceContains("_ =>"),
            "Expected _ => fallback arm in switch expression"
        );
    }

    [Fact]
    public void ElseBranch_GeneratesNullFallback()
    {
        const string src = Header + "@if (flag) { <label/> }";
        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced);
        // @if without @else should produce `: null` in the ternary
        Assert.True(result.SourceContains(": null"), "Missing @else should generate : null");
    }

    // ── Key attribute ─────────────────────────────────────────────────────────

    [Fact]
    public void KeyAttribute_PassedAsSecondArgument()
    {
        var result = GeneratorTestHelper.Run(Header + """<label key="my-key" text="Hi"/>""");

        Assert.True(result.SourceContains("my-key"), "key value should appear in generated source");
    }

    // ── Line directives ───────────────────────────────────────────────────────

    [Fact]
    public void GeneratedSource_ContainsLineDirectives()
    {
        var result = GeneratorTestHelper.Run(Header + "<label/>");

        // #line directives must reference the original .uitkx file
        Assert.True(
            result.SourceContains("#line"),
            "Expected #line directives in generated source"
        );

        // Must carry a concrete path to avoid Unity virtual generator hyperlinks
        // like ReactiveUITK.SourceGenerator\...\Foo.uitkx(...).
        Assert.True(
            result.SourceContains("/TestComponent.uitkx\""),
            "Expected #line to include normalized source file path"
        );
        Assert.False(
            result.SourceContains("ReactiveUITK.SourceGenerator\\"),
            "#line should not point to virtual generator paths"
        );
    }

    // ── [UitkxElement] attribute ─────────────────────────────────────────────

    [Fact]
    public void GeneratedClass_HasUitkxElementAttribute()
    {
        // The emitter should emit [global::ReactiveUITK.UitkxElement("MyComp")]
        // on the generated partial class so runtime tooling can discover it.
        var result = GeneratorTestHelper.Run(Header + "<label/>");

        Assert.True(result.SourceWasProduced);
        Assert.True(
            result.SourceContains("UitkxElement(\"MyComp\")"),
            "Expected [UitkxElement(\"MyComp\")] attribute on the generated partial class"
        );
    }

    [Fact]
    public void JsxCommentInsideElement_DoesNotEmitExtraComma()
    {
        // Before the fix, JsxCommentNode emitted nothing but the comma logic still ran,
        // resulting in invalid C# like V.Box(V.Label(...), , V.Label(...))
        const string src =
            Header
            + """
<box>
    {/* this is a comment */}
    <label text="a"/>
    {/* another comment */}
    <label text="b"/>
</box>
""";
        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced, "Source should be produced");
        Assert.False(
            result.SourceContains(", ,"),
            "No double-comma should appear (dangling comma from comment)\n--- GENERATED ---\n"
                + (result.GeneratedSource ?? "(null)")
        );
        Assert.True(result.SourceContains("V.Label("), "Labels should still be emitted");
    }

    [Fact]
    public void JsxCommentAsRootNode_DoesNotBreakSingleRoot()
    {
        // A JSX comment at the root level alongside a single element should not
        // force Fragment wrapping (which would cause a dangling empty argument).
        const string src =
            Header
            + """
{/* root comment */}
<box />
""";
        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced, "Source should be produced");
        Assert.True(
            result.SourceContains("V.Box("),
            "Box should be emitted\n--- GENERATED ---\n" + (result.GeneratedSource ?? "(null)")
        );
    }

    // ── Embedded markup in @code ─────────────────────────────────────────────

    [Fact]
    public void AssignMarkupInCodeBlock_GeneratesVCall()
    {
        const string src =
            Header
            + "@code {\n    var (count, setCount) = useState(0);\n    var component = (\n        <box>\n            <label text=\"hi\"/>\n        </box>\n    );\n}\n<box>@(component)</box>";
        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced, "Source should be produced");
        Assert.True(
            result.SourceContains("V.Box("),
            "Assigned <box> should produce V.Box( call.\n--- GENERATED ---\n"
                + (result.GeneratedSource ?? "(null)")
        );
    }

    [Fact]
    public void AssignMarkupWithInterpolatedStrings_GeneratesVCall()
    {
        // Mirrors the real UitkxCounterFunc.uitkx pattern with interpolated strings and onClick lambdas
        const string src =
            Header
            + """
@code {
    var (count, setCount) = useState(0);
    var component = (
        <box>
            <button text="-5" onClick={() => setCount(count - 5)} />
            <label text={$"{count}"} />
            <button text="+5" onClick={() => setCount(count + 5)} />
        </box>
    );
}
<box>@(component)</box>
""";
        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced, "Source should be produced");
        Assert.True(
            result.SourceContains("V.Box("),
            "Assigned <box> with onClick lambdas should produce V.Box( call.\n--- GENERATED ---\n"
                + (result.GeneratedSource ?? "(null)")
        );
    }

    [Fact]
    public void ReturnMarkupInCodeBlock_GeneratesVCall()
    {
        const string src =
            Header
            + "@code {\n    private static VirtualNode Btn() {\n        return <label text=\"hi\" />;\n    }\n}";
        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced, "Source should be produced");
        Assert.True(
            result.SourceContains("V.Label("),
            "Embedded <label> should produce V.Label( call"
        );
    }

    [Fact]
    public void ReturnMarkupInCodeBlock_ExpressionAttribute_IsVerbatim()
    {
        const string src =
            Header
            + "@code {\n    private static VirtualNode Btn(string msg) {\n        return <label text={msg} />;\n    }\n}";
        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced, "Source should be produced");
        Assert.True(
            result.SourceContains("msg"),
            "Expression attribute should appear verbatim in generated source"
        );
    }
}
