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
    /// Wraps markup in a minimal function-style component for test convenience.
    private static string Wrap(string markup) =>
        "component MyComp {\n  return (\n" + markup + "\n  );\n}";

    // ΓöÇΓöÇ Element emission ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

    [Fact]
    public void SimpleElement_GeneratesVCall()
    {
        var result = GeneratorTestHelper.Run(Wrap("<label/>"));

        Assert.True(result.SourceWasProduced);
        Assert.True(result.SourceContains("V.Label("), "Expected a V.Label( call");
    }

    [Fact]
    public void StringAttribute_AppearsInVCall()
    {
        var result = GeneratorTestHelper.Run(Wrap("""<label text="Hello World"/>"""));

        Assert.True(
            result.SourceContains("Hello World"),
            "Expected string literal in generated source"
        );
    }

    [Fact]
    public void ExpressionAttribute_AppearsVerbatim()
    {
        var result = GeneratorTestHelper.Run(Wrap("<label text={count}/>"));

        Assert.True(
            result.SourceContains("count"),
            "Expected expression verbatim in generated source"
        );
    }

    [Fact]
    public void NestedElements_GeneratesNestedVCalls()
    {
        var result = GeneratorTestHelper.Run(Wrap("<box><label/></box>"));

        Assert.True(result.SourceContains("V.Box("), "Expected V.Box call");
        Assert.True(result.SourceContains("V.Label("), "Expected V.Label call inside box");
    }

    // ΓöÇΓöÇ Namespace / class structure ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

    [Fact]
    public void Namespace_EmittedInGeneratedSource()
    {
        var result = GeneratorTestHelper.Run(Wrap("<box/>"));

        Assert.True(
            result.SourceContains("namespace ReactiveUITK.FunctionStyle"),
            "Expected namespace declaration"
        );
    }

    [Fact]
    public void ClassName_MatchesComponentDirective()
    {
        var result = GeneratorTestHelper.Run(Wrap("<box/>"));

        Assert.True(
            result.SourceContains("partial class MyComp"),
            "Expected partial class named MyComp"
        );
    }

    [Fact]
    public void FunctionStyleComponent_GeneratesClassAndMarkup()
    {
        const string src = """
            component CounterPanel {
                var (count, setCount) = useState(0);
                return (
                    <Box>
                        <Label text={$"{count}"} />
                    </Box>
                );
            }
            """;

        var result = GeneratorTestHelper.Run(src, "CounterPanel.uitkx");

        Assert.True(result.SourceWasProduced);
        Assert.True(result.SourceContains("partial class CounterPanel"));
        Assert.True(result.SourceContains("V.Box("));
        Assert.True(result.SourceContains("V.Label("));
        Assert.True(result.SourceContains("Hooks.UseState(") || result.SourceContains("useState("));
    }

    // ΓöÇΓöÇ Control flow ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

    [Fact]
    public void IfDirective_GeneratesConditionalExpression()
    {
        var src = Wrap("@if (flag) { return (<label/>); }");
        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced);
        // @if ΓåÆ IIFE; the condition should appear in the generated source
        Assert.True(result.SourceContains("flag"), "Condition should appear verbatim");
        Assert.True(
            result.SourceContains("Func<VirtualNode>")
                || result.SourceContains("Func<")
                || result.SourceContains("? ")
                || result.SourceContains("?V.")
                || result.SourceContains("?("),
            "IIFE or ternary expected for @if.\nGenerated:\n" + (result.GeneratedSource ?? "<null>")
        );
    }

    [Fact]
    public void ForeachDirective_GeneratesSelectCall()
    {
        var src = Wrap("@foreach (var item in items) { return (<label key={item.Id}/>); }");
        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced);
        // @foreach ΓåÆ IIFE with list-building loop or .Select()
        Assert.True(
            result.SourceContains(".Select(") || result.SourceContains("foreach"),
            "Expected .Select( or foreach loop for @foreach");
    }

    [Fact]
    public void ForeachDirective_WithSetupCode_GeneratesIIFE()
    {
        var src = Wrap("""
            <box>
                @foreach (var entry in log) {
                    var a = "test";
                    return (
                        <label key={entry} text={entry} />
                    );
                }
                <label text="after" />
            </box>
            """);
        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced,
            "Expected generated source.\n" + string.Join("\n", result.Diagnostics.Select(d => $"  {d.Id}: {d.GetMessage()}")));
        Assert.True(result.SourceContains("var a = \"test\""),
            "Expected setup code in output. Generated:\n" + (result.GeneratedSource ?? "<null>"));
    }

    [Fact]
    public void ForeachDirective_WithSetupCode_InRealComponent_GeneratesValidCSharp()
    {
        // Mimics a real component: function-level hooks PLUS a foreach with setup code
        var src = """
            component TestComp {
                var (count, setCount) = useState(0);
                var (log, setLog) = useState(new System.Collections.Generic.List<string> { "Ready." });
                return (
                    <box>
                        <label text={count.ToString()} />
                        @foreach (var entry in log) {
                            var prefix = "[LOG] ";
                            return (
                                <label key={entry} text={prefix + entry} />
                            );
                        }
                        <label text="footer" />
                    </box>
                );
            }
            """;
        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced,
            "Expected generated source but got diagnostics:\n"
            + string.Join("\n", result.Diagnostics.Select(d => $"  {d.Id}: {d.GetMessage()}")));

        // Verify structure ΓÇö IIFE brace balance must be correct
        Assert.True(result.SourceContains("var prefix = \"[LOG] \""),
            "Expected foreach setup code in generated output.\nGenerated:\n" + (result.GeneratedSource ?? "<null>"));

        // Verify the closing pattern matches EmitForNode/EmitWhileNode: ); } return __r.ToArray(); }))()
        Assert.True(result.SourceContains("} return __r.ToArray(); }))()"),
            "IIFE must have exactly 2 closing braces (foreach body + IIFE lambda).\nGenerated:\n" + (result.GeneratedSource ?? "<null>"));
        Assert.False(result.SourceContains("}} return __r.ToArray();"),
            "Double '}}' in plain string = 4 braces (bug). Must use single '}' each.\nGenerated:\n" + (result.GeneratedSource ?? "<null>"));

        // Verify no false UITKX0014 (hook in loop)
        Assert.False(result.HasDiagnostic("UITKX0014"),
            "UITKX0014 should NOT fire ΓÇö hooks are at component top level, not inside @foreach.\n"
            + string.Join("\n", result.Diagnostics.Select(d => $"  {d.Id}: {d.GetMessage()}")));
    }

    [Fact]
    public void ForDirective_WithLoopFlow_EmitsBreakAndContinueStatements()
    {
        var src = Wrap(
            """
            @for (var i = 0; i < 10; i++) {
                if (i < 3) { continue; }
                if (i > 6) { break; }
                return (
                    <label text={i.ToString()} />
                );
            }
            """
        );

        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced);
        Assert.True(result.SourceContains("for (var i = 0; i < 10; i++)"), "Expected for loop");
        Assert.True(result.SourceContains("continue;"), "Expected continue statement");
        Assert.True(result.SourceContains("break;"), "Expected break statement");
    }

    [Fact]
    public void WhileDirective_WithLoopFlow_EmitsContinueStatement()
    {
        var src = Wrap(
            """
            @while (running) {
                if (skip) { continue; }
                return (
                    <label />
                );
            }
            """
        );

        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced);
        Assert.True(result.SourceContains("while (running)"), "Expected while loop");
        Assert.True(result.SourceContains("continue;"), "Expected continue statement");
    }

    [Fact]
    public void SwitchDirective_GeneratesSwitchExpression()
    {
        var src = Wrap(
            """
            @switch (mode) {
                @case 0: return (<label text="zero"/>);
                @case 1: return (<label text="one"/>);
            }
            """
        );
        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced);
        Assert.True(result.SourceContains("switch"), "Expected switch statement");
        // Emitter generates IIFE with switch statement
        Assert.True(
            result.SourceContains("Func<") || result.SourceContains("_ =>"),
            "Expected IIFE or switch expression pattern for @switch"
        );
    }

    [Fact]
    public void ElseBranch_GeneratesNullFallback()
    {
        var src = Wrap("@if (flag) { return (<label/>); }");
        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced);
        // @if without @else should produce `: null` in the ternary
        Assert.True(result.SourceContains(": null") || result.SourceContains("(VirtualNode)null"), "Missing @else should generate null fallback");
    }

    // ΓöÇΓöÇ Key attribute ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

    [Fact]
    public void KeyAttribute_PassedAsSecondArgument()
    {
        var result = GeneratorTestHelper.Run(Wrap("""<label key="my-key" text="Hi"/>"""));

        Assert.True(result.SourceContains("my-key"), "key value should appear in generated source");
    }

    // ΓöÇΓöÇ Line directives ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

    [Fact]
    public void GeneratedSource_ContainsLineDirectives()
    {
        var result = GeneratorTestHelper.Run(Wrap("<label/>"));

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

    // ΓöÇΓöÇ [UitkxElement] attribute ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

    [Fact]
    public void GeneratedClass_HasUitkxElementAttribute()
    {
        // The emitter should emit [global::ReactiveUITK.UitkxElement("MyComp")]
        // on the generated partial class so runtime tooling can discover it.
        var result = GeneratorTestHelper.Run(Wrap("<label/>"));

        Assert.True(result.SourceWasProduced);
        Assert.True(
            result.SourceContains("UitkxElement(\"MyComp\")"),
            "Expected [UitkxElement(\"MyComp\")] attribute on the generated partial class"
        );
    }

    [Fact]
    public void CommentInsideElement_DoesNotEmitExtraComma()
    {
        // Before the fix, CommentNode emitted nothing but the comma logic still ran,
        // resulting in invalid C# like V.Box(V.Label(...), , V.Label(...))
        var src = Wrap(
            """
<box>
    // this is a comment
    <label text="a"/>
    // another comment
    <label text="b"/>
</box>
"""
        );
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
    public void CommentAsRootNode_DoesNotBreakSingleRoot()
    {
        // A comment at the root level alongside a single element should not
        // force Fragment wrapping (which would cause a dangling empty argument).
        const string src = """
            component MyComp {
                return (
                    // root comment
                    <box />
                );
            }
            """;
        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced, "Source should be produced");
        Assert.True(
            result.SourceContains("V.Box("),
            "Box should be emitted\n--- GENERATED ---\n" + (result.GeneratedSource ?? "(null)")
        );
    }

    [Fact]
    public void AssignMarkupWithInterpolatedStrings_GeneratesVCall()
    {
        // Mirrors the real UitkxCounterFunc.uitkx pattern with interpolated strings and onClick lambdas
        const string src = """
            component MyComp {
                var (count, setCount) = useState(0);
                var component = (
                    <box>
                        <button text="-5" onClick={() => setCount(count - 5)} />
                        <label text={$"{count}"} />
                        <button text="+5" onClick={() => setCount(count + 5)} />
                    </box>
                );
                return (
                    <box>@(component)</box>
                );
            }
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
    public void FunctionStyleComponent_LeadingUsings_EmittedInGeneratedSource()
    {
        const string src = """
            using MyGame.Models;
            using System.Collections.Generic;
            component PlayerHUD {
                return (<Box />);
            }
            """;

        var result = GeneratorTestHelper.Run(src, "PlayerHUD.uitkx");

        Assert.True(result.SourceWasProduced);
        Assert.True(result.SourceContains("using MyGame.Models;"), "Expected using MyGame.Models;");
        Assert.True(
            result.SourceContains("using System.Collections.Generic;"),
            "Expected using System.Collections.Generic;"
        );
    }

    [Fact]
    public void HookAlias_NestedGenericTypeArg_IsExpanded()
    {
        const string src = """
            component Foo {
                var ctx = useContext<Dictionary<string, int>>("myKey");
                return (<Box />);
            }
            """;

        var result = GeneratorTestHelper.Run(src, "Foo.uitkx");

        Assert.True(result.SourceWasProduced);
        Assert.True(
            result.SourceContains("Hooks.UseContext<Dictionary<string, int>>("),
            "Nested generic hook alias should be expanded"
        );
    }

    // ΓöÇΓöÇ ErrorBoundary emission ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

    [Fact]
    public void ErrorBoundary_GeneratesVErrorBoundaryCall()
    {
        const string src = """
            component MyComp {
                return (
                    <ErrorBoundary>
                        <Label text="child" />
                    </ErrorBoundary>
                );
            }
            """;

        var result = GeneratorTestHelper.Run(src, "MyComp.uitkx");

        Assert.True(result.SourceWasProduced);
        Assert.True(
            result.SourceContains("V.ErrorBoundary("),
            $"Expected V.ErrorBoundary( call in generated source. Got:\n{result.GeneratedSource}"
        );
        Assert.True(
            result.SourceContains("ErrorBoundaryProps"),
            "Expected ErrorBoundaryProps in generated source"
        );
    }

    [Fact]
    public void ErrorBoundary_WithFallbackProp_GeneratesCorrectProps()
    {
        const string src = """
            component MyComp {
                var fallback = (<Label text="error!" />);
                return (
                    <ErrorBoundary fallback={fallback} resetKey="v1">
                        <Label text="child" />
                    </ErrorBoundary>
                );
            }
            """;

        var result = GeneratorTestHelper.Run(src, "MyComp.uitkx");

        Assert.True(result.SourceWasProduced);
        Assert.True(
            result.SourceContains("V.ErrorBoundary("),
            $"Expected V.ErrorBoundary( call. Got:\n{result.GeneratedSource}"
        );
        Assert.True(
            result.SourceContains("Fallback = fallback"),
            "Expected Fallback prop assignment"
        );
        Assert.True(
            result.SourceContains("ResetKey = \"v1\""),
            "Expected ResetKey prop assignment"
        );
    }

    [Fact]
    public void Suspense_WithIsReadyAndFallback_GeneratesVSuspenseCall()
    {
        const string src = """
            component MyComp {
                bool IsReady() => true;
                var fallback = (<Label text="loading..." />);
                return (
                    <Suspense isReady={IsReady} fallback={fallback}>
                        <Label text="content" />
                    </Suspense>
                );
            }
            """;

        var result = GeneratorTestHelper.Run(src, "MyComp.uitkx");

        Assert.True(result.SourceWasProduced);
        Assert.True(
            result.SourceContains("V.Suspense("),
            $"Expected V.Suspense( call. Got:\n{result.GeneratedSource}"
        );
        Assert.True(
            result.SourceContains("IsReady"),
            "Expected IsReady expression in V.Suspense call"
        );
        Assert.True(
            result.SourceContains("fallback"),
            "Expected fallback expression in V.Suspense call"
        );
    }

    [Fact]
    public void Portal_WithTargetAndChildren_GeneratesVPortalCall()
    {
        const string src = """
            component MyComp {
                var target = useContext<UnityEngine.UIElements.VisualElement>("slot");
                return (
                    <Portal target={target}>
                        <Label text="inside portal" />
                    </Portal>
                );
            }
            """;

        var result = GeneratorTestHelper.Run(src, "MyComp.uitkx");

        Assert.True(result.SourceWasProduced);
        Assert.True(
            result.SourceContains("V.Portal("),
            $"Expected V.Portal( call. Got:\n{result.GeneratedSource}"
        );
        Assert.True(result.SourceContains("target"), "Expected target expression in V.Portal call");
    }

    // ΓöÇΓöÇ Peer component props resolution ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

    [Fact]
    public void PeerComponent_WithProps_EmitsTypedVFuncAndPassesAttributes()
    {
        // ChildComp.uitkx ΓÇö sub-component with a bool prop
        const string childSrc = """
            component ChildComp(bool active = false) {
                return (<Label text="child" />);
            }
            """;

        // ParentComp.uitkx ΓÇö references ChildComp; its props type is peer-generated
        const string parentSrc = """
            component ParentComp {
                var flag = true;
                return (
                    <ChildComp active={flag} />
                );
            }
            """;

        var result = GeneratorTestHelper.RunMultiple(
            new[] { ("ChildComp.uitkx", childSrc), ("ParentComp.uitkx", parentSrc) },
            primaryFileName: "ParentComp.uitkx"
        );

        Assert.True(
            result.SourceWasProduced,
            $"No source produced. Diagnostics: {string.Join(", ", result.Diagnostics)}"
        );
        // Props are generated as nested types (ChildComp.ChildCompProps), so the
        // qualified form must be used to avoid CS0246 at the call site.
        Assert.True(
            result.SourceContains(
                "V.Func<global::ReactiveUITK.FunctionStyle.ChildComp.ChildCompProps>"
            ),
            $"Expected typed V.Func<global::ReactiveUITK.FunctionStyle.ChildComp.ChildCompProps> call. Got:\n{result.GeneratedSource}"
        );
        // The 'active' attribute must be passed through as 'Active = flag'
        Assert.True(
            result.SourceContains("Active = flag"),
            $"Expected 'Active = flag' prop in generated source. Got:\n{result.GeneratedSource}"
        );
    }

    // ΓöÇΓöÇ Nested Props class resolution (C# convention: TypeName.Props) ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

    [Fact]
    public void FuncComponent_WithNestedPropsClass_EmitsTypedVFuncAndPassesAttributes()
    {
        // A C# static class that follows the legacy ValuesBarFunc pattern:
        // nested Props class rather than the {TypeName}Props sibling convention.
        const string extraCSharp = """
            using ReactiveUITK.Core;
            using System.Collections.Generic;

            namespace MyApp
            {
                public static class ValuesBarFunc
                {
                    public sealed class Props : IProps
                    {
                        public IEnumerable<System.Collections.Generic.KeyValuePair<string,string>> Items { get; set; }
                    }

                    public static VirtualNode Render(IProps rawProps, IReadOnlyList<VirtualNode> children)
                        => null;
                }
            }
            """;

        const string uitkx = """
            @namespace MyApp.UI
            @using MyApp

            component MyPage {
                var items = new List<System.Collections.Generic.KeyValuePair<string,string>>();
                return (<ValuesBarFunc items={items} />);
            }
            """;

        var result = GeneratorTestHelper.RunWithExtraCSharp(uitkx, extraCSharp, "MyPage.uitkx");

        Assert.True(
            result.SourceWasProduced,
            $"No source produced. Diagnostics: {string.Join(", ", result.Diagnostics)}"
        );

        // Must use the typed V.Func<ValuesBarFunc.Props> overload, not the no-props fallback
        Assert.True(
            result.SourceContains("V.Func<global::MyApp.ValuesBarFunc.Props>"),
            $"Expected typed V.Func<global::MyApp.ValuesBarFunc.Props> call. Got:\n{result.GeneratedSource}"
        );

        // The 'items' attribute must be forwarded as 'Items = items'
        Assert.True(
            result.SourceContains("Items = items"),
            $"Expected 'Items = items' in generated source. Got:\n{result.GeneratedSource}"
        );
    }

    // ΓöÇΓöÇ ref={x} routing on user (UITKX) components ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

    [Fact]
    public void UserComponent_SingleRefParam_RefAttrRoutedToMutableRefProp()
    {
        // ChildComp.uitkx ΓÇö declares a single Hooks.MutableRef parameter.
        const string childSrc = """
            component ChildComp(
                Hooks.MutableRef<object>? inputRef = null
            ) {
                return (<Label text="child" />);
            }
            """;

        // ParentComp.uitkx ΓÇö uses ref={myRef} shorthand on ChildComp.
        const string parentSrc = """
            component ParentComp {
                return (
                    <ChildComp ref={myRef} />
                );
            }
            """;

        var result = GeneratorTestHelper.RunMultiple(
            new[] { ("ChildComp.uitkx", childSrc), ("ParentComp.uitkx", parentSrc) },
            primaryFileName: "ParentComp.uitkx"
        );

        Assert.True(
            result.SourceWasProduced,
            $"No source produced. Diagnostics: {string.Join(", ", result.Diagnostics)}"
        );

        // The ref={myRef} attribute must be routed to InputRef (PascalCase of inputRef).
        Assert.True(
            result.SourceContains("InputRef = myRef"),
            $"Expected 'InputRef = myRef' in generated source. Source:\n{result.GeneratedSource}"
        );

        // No ref-routing diagnostics should be emitted.
        Assert.False(
            result.HasDiagnostic("UITKX0020"),
            "Did not expect UITKX0020 (no ref param) when child has exactly one MutableRef param."
        );
        Assert.False(
            result.HasDiagnostic("UITKX0021"),
            "Did not expect UITKX0021 (ambiguous ref) when child has exactly one MutableRef param."
        );
    }

    [Fact]
    public void UserComponent_NoRefParam_RefAttrEmitsDiagnosticUITKX0020()
    {
        // ChildComp.uitkx ΓÇö no MutableRef parameter; ref={} cannot be routed.
        const string childSrc = """
            component ChildComp(string? label = null) {
                return (<Label text="child" />);
            }
            """;

        const string parentSrc = """
            component ParentComp {
                return (
                    <ChildComp ref={myRef} />
                );
            }
            """;

        var result = GeneratorTestHelper.RunMultiple(
            new[] { ("ChildComp.uitkx", childSrc), ("ParentComp.uitkx", parentSrc) },
            primaryFileName: "ParentComp.uitkx"
        );

        Assert.True(
            result.HasDiagnostic("UITKX0020"),
            $"Expected UITKX0020 when ref={{}} is used on a component with no MutableRef param. "
                + $"Diagnostics: {string.Join(", ", result.Diagnostics.Select(d => d.Id))}"
        );
    }

    [Fact]
    public void UserComponent_MultipleRefParams_RefAttrEmitsDiagnosticUITKX0021()
    {
        // ChildComp.uitkx ΓÇö two MutableRef parameters; ref={} is ambiguous.
        const string childSrc = """
            component ChildComp(
                Hooks.MutableRef<object>? inputRef  = null,
                Hooks.MutableRef<object>? secondRef = null
            ) {
                return (<Label text="child" />);
            }
            """;

        const string parentSrc = """
            component ParentComp {
                return (
                    <ChildComp ref={myRef} />
                );
            }
            """;

        var result = GeneratorTestHelper.RunMultiple(
            new[] { ("ChildComp.uitkx", childSrc), ("ParentComp.uitkx", parentSrc) },
            primaryFileName: "ParentComp.uitkx"
        );

        Assert.True(
            result.HasDiagnostic("UITKX0021"),
            $"Expected UITKX0021 when ref={{}} is used on a component with multiple MutableRef params. "
                + $"Diagnostics: {string.Join(", ", result.Diagnostics.Select(d => d.Id))}"
        );
    }

    [Fact]
    public void UserComponent_RefAttrAndOtherProps_CombineCorrectly()
    {
        // ChildComp.uitkx ΓÇö has a regular string prop + one MutableRef prop.
        const string childSrc = """
            component ChildComp(
                string? label = null,
                Hooks.MutableRef<object>? inputRef = null
            ) {
                return (<Label text="child" />);
            }
            """;

        // ParentComp.uitkx ΓÇö passes both a regular attribute and ref={}.
        const string parentSrc = """
            component ParentComp {
                return (
                    <ChildComp label="hello" ref={myRef} />
                );
            }
            """;

        var result = GeneratorTestHelper.RunMultiple(
            new[] { ("ChildComp.uitkx", childSrc), ("ParentComp.uitkx", parentSrc) },
            primaryFileName: "ParentComp.uitkx"
        );

        Assert.True(
            result.SourceWasProduced,
            $"No source produced. Diagnostics: {string.Join(", ", result.Diagnostics)}"
        );

        // Regular prop must appear in the Props initializer.
        Assert.True(
            result.SourceContains("Label = \"hello\""),
            $"Expected 'Label = \"hello\"' in generated source. Source:\n{result.GeneratedSource}"
        );

        // ref={} must be routed to InputRef.
        Assert.True(
            result.SourceContains("InputRef = myRef"),
            $"Expected 'InputRef = myRef' in generated source. Source:\n{result.GeneratedSource}"
        );

        // No routing diagnostics.
        Assert.False(result.HasDiagnostic("UITKX0020"), "Unexpected UITKX0020.");
        Assert.False(result.HasDiagnostic("UITKX0021"), "Unexpected UITKX0021.");
    }

    [Fact]
    public void CrossNamespacePeerComponent_WithProps_EmitsFullyQualifiedPeerReferences()
    {
        const string childSrc = """
            @namespace MyApp.Components.Buttons

            component JSOAppButton(bool disabled = false) {
                return (<Label text="child" />);
            }
            """;

        const string parentSrc = """
            @namespace MyApp.Components.Sidebar
            using MyApp.Components.Buttons;

            component Sidebar {
                var isDisabled = true;
                return (
                    <JSOAppButton disabled={isDisabled} />
                );
            }
            """;

        var result = GeneratorTestHelper.RunMultiple(
            new[] { ("JSOAppButton.uitkx", childSrc), ("Sidebar.uitkx", parentSrc) },
            primaryFileName: "Sidebar.uitkx"
        );

        Assert.True(
            result.SourceWasProduced,
            $"No source produced. Diagnostics: {string.Join(", ", result.Diagnostics)}"
        );
        Assert.True(
            result.SourceContains(
                "V.Func<global::MyApp.Components.Buttons.JSOAppButton.JSOAppButtonProps>"
            ),
            $"Expected fully qualified peer props type. Got:\n{result.GeneratedSource}"
        );
        Assert.True(
            result.SourceContains("global::MyApp.Components.Buttons.JSOAppButton.Render"),
            $"Expected fully qualified peer render target. Got:\n{result.GeneratedSource}"
        );
        Assert.True(
            result.SourceContains("Disabled = isDisabled"),
            $"Expected forwarded prop initializer. Got:\n{result.GeneratedSource}"
        );
    }

    [Fact]
    public void CrossNamespacePeerComponent_RefRouting_UsesQualifiedPeerIdentity()
    {
        const string childSrc = """
            @namespace MyApp.Components.Buttons

            component JSOAppButton(
                Hooks.MutableRef<object>? inputRef = null
            ) {
                return (<Label text="child" />);
            }
            """;

        const string parentSrc = """
            @namespace MyApp.Components.Sidebar
            using MyApp.Components.Buttons;

            component Sidebar {
                return (
                    <JSOAppButton ref={myRef} />
                );
            }
            """;

        var result = GeneratorTestHelper.RunMultiple(
            new[] { ("JSOAppButton.uitkx", childSrc), ("Sidebar.uitkx", parentSrc) },
            primaryFileName: "Sidebar.uitkx"
        );

        Assert.True(
            result.SourceWasProduced,
            $"No source produced. Diagnostics: {string.Join(", ", result.Diagnostics)}"
        );
        Assert.True(
            result.SourceContains("InputRef = myRef"),
            $"Expected routed ref prop. Got:\n{result.GeneratedSource}"
        );
        Assert.False(result.HasDiagnostic("UITKX0020"), "Unexpected UITKX0020.");
        Assert.False(result.HasDiagnostic("UITKX0021"), "Unexpected UITKX0021.");
    }

    // ΓöÇΓöÇ Asset path resolution ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

    [Fact]
    public void Asset_RelativePath_Resolved()
    {
        var src = """
            component Card {
                return (
                    <label text={Asset<Texture2D>("./avatar.png").name} />
                );
            }
            """;

        // Place the file inside an Assets/ sub-path so GetUitkxAssetDir finds it
        var result = GeneratorTestHelper.Run(src, "Assets/UI/Card.uitkx");

        Assert.True(result.SourceWasProduced, "No source produced");
        Assert.True(
            result.SourceContains("\"Assets/UI/avatar.png\""),
            $"Expected resolved path 'Assets/UI/avatar.png'. Got:\n{result.GeneratedSource}"
        );
        Assert.False(
            result.SourceContains("./avatar.png"),
            "Relative path should have been resolved"
        );
    }

    [Fact]
    public void Ast_RelativePath_Resolved()
    {
        var src = """
            component Card {
                return (
                    <label text={Ast<Sprite>("../shared/icon.png").name} />
                );
            }
            """;

        var result = GeneratorTestHelper.Run(src, "Assets/UI/Card.uitkx");

        Assert.True(result.SourceWasProduced, "No source produced");
        Assert.True(
            result.SourceContains("\"Assets/shared/icon.png\""),
            $"Expected resolved path 'Assets/shared/icon.png'. Got:\n{result.GeneratedSource}"
        );
    }

    [Fact]
    public void Asset_AbsolutePath_Unchanged()
    {
        var src = """
            component Card {
                return (
                    <label text={Asset<Font>("Assets/Fonts/custom.ttf").name} />
                );
            }
            """;

        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced, "No source produced");
        Assert.True(
            result.SourceContains("\"Assets/Fonts/custom.ttf\""),
            $"Expected absolute path unchanged with extension. Got:\n{result.GeneratedSource}"
        );
    }

    [Fact]
    public void Asset_AbsoluteNoExtension_Unchanged()
    {
        var src = """
            component Card {
                return (
                    <label text={Asset<Texture2D>("Assets/UI/avatar").name} />
                );
            }
            """;

        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced, "No source produced");
        Assert.True(
            result.SourceContains("\"Assets/UI/avatar\""),
            $"Expected path unchanged. Got:\n{result.GeneratedSource}"
        );
    }

    [Fact]
    public void Asset_InCodeBlock_PathResolved()
    {
        var src = """
            component Card {
                var tex = Asset<Texture2D>("./bg.png");
                return (
                    <label text={tex?.name ?? ""} />
                );
            }
            """;

        var result = GeneratorTestHelper.Run(src, "Assets/UI/Card.uitkx");

        Assert.True(result.SourceWasProduced, "No source produced");
        Assert.True(
            result.SourceContains("\"Assets/UI/bg.png\""),
            $"Expected resolved path in code block. Got:\n{result.GeneratedSource}"
        );
    }

    [Fact]
    public void AssetHelpers_UsingAutoInjected()
    {
        var src = """
            component Card {
                return (
                    <label text="hello" />
                );
            }
            """;

        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced, "No source produced");
        Assert.True(
            result.SourceContains("using static ReactiveUITK.AssetHelpers;"),
            "Expected auto-injected AssetHelpers using"
        );
    }

    [Fact]
    public void CssHelpers_UsingAutoInjected()
    {
        var src = """
            component Card {
                return (
                    <label text="hello" />
                );
            }
            """;

        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced, "No source produced");
        Assert.True(
            result.SourceContains("using static ReactiveUITK.Props.Typed.CssHelpers;"),
            "Expected auto-injected CssHelpers using"
        );
    }

    [Fact]
    public void BareAssignWithNestedDirective_InsideDirectiveBody_JsxIsEmitted()
    {
        // bare = <Container>...@if...</Container> in @foreach setup code
        // must not leave raw JSX in the generated source (range collision fix).
        var src = """
            component Comp {
              return (
                <Box>
                  @foreach (var item in items) {
                    var badge = <Box>
                      <Label text={item} />
                      @if (showX) {
                        return (<Label text="extra" />);
                      }
                    </Box>;
                    return (<Label text={item} />);
                  }
                </Box>
              );
            }
            """;

        var result = GeneratorTestHelper.Run(src, "Test.uitkx");
        Assert.True(result.SourceWasProduced, "No source produced");
        Assert.False(result.SourceContains("= <Box>"), "Raw bare JSX leaked into generated source");
        Assert.True(result.SourceContains("V.Box("), "Expected V.Box() call from the bare assignment");
        Assert.True(result.SourceContains("V.Label("), "Expected V.Label() inside the bare-assigned Box");
    }

}

