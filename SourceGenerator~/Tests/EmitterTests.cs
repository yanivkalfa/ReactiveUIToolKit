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

    // ── Element emission ──────────────────────────────────────────────────────

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

    // ── Namespace / class structure ──────────────────────────────────────────

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

    // ── Control flow ─────────────────────────────────────────────────────────

    [Fact]
    public void IfDirective_GeneratesConditionalExpression()
    {
        var src = Wrap("@if (flag) { return (<label/>); }");
        var result = GeneratorTestHelper.Run(src);

        Assert.True(result.SourceWasProduced);
        // @if → IIFE; the condition should appear in the generated source
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
        // @foreach → IIFE with list-building loop or .Select()
        Assert.True(
            result.SourceContains(".Select(") || result.SourceContains("foreach"),
            "Expected .Select( or foreach loop for @foreach"
        );
    }

    [Fact]
    public void ForeachDirective_WithSetupCode_GeneratesIIFE()
    {
        var src = Wrap(
            """
            <box>
                @foreach (var entry in log) {
                    var a = "test";
                    return (
                        <label key={entry} text={entry} />
                    );
                }
                <label text="after" />
            </box>
            """
        );
        var result = GeneratorTestHelper.Run(src);

        Assert.True(
            result.SourceWasProduced,
            "Expected generated source.\n"
                + string.Join("\n", result.Diagnostics.Select(d => $"  {d.Id}: {d.GetMessage()}"))
        );
        Assert.True(
            result.SourceContains("var a = \"test\""),
            "Expected setup code in output. Generated:\n" + (result.GeneratedSource ?? "<null>")
        );
        // Inner per-iteration IIFE should NOT exist (OPT-10)
        Assert.False(
            result.SourceContains("Func<global::ReactiveUITK.Core.VirtualNode>>)"),
            "Inner per-iteration IIFE should be eliminated (OPT-10).\nGenerated:\n"
                + (result.GeneratedSource ?? "<null>")
        );
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

        Assert.True(
            result.SourceWasProduced,
            "Expected generated source but got diagnostics:\n"
                + string.Join("\n", result.Diagnostics.Select(d => $"  {d.Id}: {d.GetMessage()}"))
        );

        // Verify structure — outer IIFE brace balance must be correct
        Assert.True(
            result.SourceContains("var prefix = \"[LOG] \""),
            "Expected foreach setup code in generated output.\nGenerated:\n"
                + (result.GeneratedSource ?? "<null>")
        );

        // Verify body code is inlined directly (no inner per-iteration IIFE)
        Assert.True(
            result.SourceContains("__r.Add("),
            "Expected __r.Add() for inlined return.\nGenerated:\n"
                + (result.GeneratedSource ?? "<null>")
        );

        // Verify the closing pattern: foreach body brace + outer IIFE brace
        Assert.True(
            result.SourceContains("} return __r.ToArray(); }))()"),
            "Outer IIFE must have exactly 2 closing braces (foreach body + IIFE lambda).\nGenerated:\n"
                + (result.GeneratedSource ?? "<null>")
        );
        Assert.False(
            result.SourceContains("}} return __r.ToArray();"),
            "Double '}}' in plain string = 4 braces (bug). Must use single '}' each.\nGenerated:\n"
                + (result.GeneratedSource ?? "<null>")
        );

        // Verify no false UITKX0014 (hook in loop)
        Assert.False(
            result.HasDiagnostic("UITKX0014"),
            "UITKX0014 should NOT fire — hooks are at component top level, not inside @foreach.\n"
                + string.Join("\n", result.Diagnostics.Select(d => $"  {d.Id}: {d.GetMessage()}"))
        );
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
        Assert.True(
            result.SourceContains(": null") || result.SourceContains("(VirtualNode)null"),
            "Missing @else should generate null fallback"
        );
    }

    // ── Key attribute ─────────────────────────────────────────────────────────

    [Fact]
    public void KeyAttribute_PassedAsSecondArgument()
    {
        var result = GeneratorTestHelper.Run(Wrap("""<label key="my-key" text="Hi"/>"""));

        Assert.True(result.SourceContains("my-key"), "key value should appear in generated source");
    }

    // ── Line directives ───────────────────────────────────────────────────────

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

    // ── [UitkxElement] attribute ─────────────────────────────────────────────

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

    // ── ErrorBoundary emission ───────────────────────────────────────────────

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

    // ── Peer component props resolution ──────────────────────────────────────

    [Fact]
    public void PeerComponent_WithProps_EmitsTypedVFuncAndPassesAttributes()
    {
        // ChildComp.uitkx — sub-component with a bool prop
        const string childSrc = """
            component ChildComp(bool active = false) {
                return (<Label text="child" />);
            }
            """;

        // ParentComp.uitkx — references ChildComp; its props type is peer-generated
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

    // ── Nested Props class resolution (C# convention: TypeName.Props) ─────────

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

    // ── ref={x} routing on user (UITKX) components ────────────────────────────

    [Fact]
    public void UserComponent_SingleRefParam_RefAttrRoutedToMutableRefProp()
    {
        // ChildComp.uitkx — declares a single Hooks.MutableRef parameter.
        const string childSrc = """
            component ChildComp(
                Hooks.MutableRef<object>? inputRef = null
            ) {
                return (<Label text="child" />);
            }
            """;

        // ParentComp.uitkx — uses ref={myRef} shorthand on ChildComp.
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
        // ChildComp.uitkx — no MutableRef parameter; ref={} cannot be routed.
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
        // ChildComp.uitkx — two MutableRef parameters; ref={} is ambiguous.
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
        // ChildComp.uitkx — has a regular string prop + one MutableRef prop.
        const string childSrc = """
            component ChildComp(
                string? label = null,
                Hooks.MutableRef<object>? inputRef = null
            ) {
                return (<Label text="child" />);
            }
            """;

        // ParentComp.uitkx — passes both a regular attribute and ref={}.
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

    // ── Asset path resolution ─────────────────────────────────────────────────

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

    // ── Asset path resolution inside `module` blocks ──────────────────────────
    // Regression coverage for the bug where ModuleEmitter emitted module bodies
    // verbatim, causing Asset<T>("./x") / Asset<T>("../x") literals to reach the
    // runtime UitkxAssetRegistry as raw relative strings (which the registry —
    // keyed by resolved Unity asset paths — would never find).

    [Fact]
    public void Module_AssetCall_RelativePath_IsRewritten()
    {
        var src = """
            @namespace Test
            module AppRoot {
              public static readonly string Bg = Asset<Texture2D>("./bg.png")?.name;
            }
            """;

        var result = GeneratorTestHelper.Run(src, "Assets/UI/AppRoot.style.uitkx");

        Assert.True(result.SourceWasProduced, "No source produced");
        Assert.True(
            result.SourceContains("\"Assets/UI/bg.png\""),
            $"Expected './bg.png' rewritten to 'Assets/UI/bg.png'.\n{result.GeneratedSource}"
        );
        Assert.False(
            result.SourceContains("\"./bg.png\""),
            "Relative module-body asset path leaked through unrewritten."
        );
    }

    [Fact]
    public void Module_AssetCall_DotDotPath_IsRewritten()
    {
        // The original failing case: `../Resources/background-01.png` inside a
        // `module AppRoot { ... }` block in an .style.uitkx file.
        var src = """
            @namespace Test
            module AppRoot {
              public static readonly string Bg = Asset<Texture2D>("../Resources/bg.png")?.name;
            }
            """;

        var result = GeneratorTestHelper.Run(src, "Assets/UI/AppRoot.style.uitkx");

        Assert.True(result.SourceWasProduced, "No source produced");
        Assert.True(
            result.SourceContains("\"Assets/Resources/bg.png\""),
            $"Expected '../Resources/bg.png' rewritten to 'Assets/Resources/bg.png'.\n{result.GeneratedSource}"
        );
        Assert.False(
            result.SourceContains("\"../Resources/bg.png\""),
            "Relative module-body asset path leaked through unrewritten."
        );
    }

    [Fact]
    public void Module_AssetCall_AbsolutePath_Unchanged()
    {
        // Absolute Assets/... paths must pass through untouched — no double-prefix.
        var src = """
            @namespace Test
            module AppRoot {
              public static readonly string Bg = Asset<Texture2D>("Assets/Fonts/custom.ttf")?.name;
            }
            """;

        var result = GeneratorTestHelper.Run(src, "Assets/UI/AppRoot.style.uitkx");

        Assert.True(result.SourceWasProduced, "No source produced");
        Assert.True(
            result.SourceContains("\"Assets/Fonts/custom.ttf\""),
            $"Expected absolute path unchanged. Got:\n{result.GeneratedSource}"
        );
        Assert.False(
            result.SourceContains("\"Assets/UI/Assets/Fonts/custom.ttf\""),
            "Absolute path was incorrectly re-prefixed with the .uitkx directory."
        );
    }

    // ── Asset path resolution inside `hook` bodies ────────────────────────────
    // Parallel coverage for HookEmitter, which previously applied ApplyHookAliases
    // but skipped ResolveAssetPaths.

    [Fact]
    public void Hook_AssetCall_RelativePath_IsRewritten()
    {
        var src = """
            @namespace Test
            hook useBg() -> string {
              var tex = Asset<Texture2D>("./bg.png");
              return tex?.name ?? "";
            }
            """;

        var result = GeneratorTestHelper.Run(src, "Assets/UI/useBg.hooks.uitkx");

        Assert.True(result.SourceWasProduced, "No source produced");
        Assert.True(
            result.SourceContains("\"Assets/UI/bg.png\""),
            $"Expected './bg.png' rewritten to 'Assets/UI/bg.png' inside hook body.\n{result.GeneratedSource}"
        );
        Assert.False(
            result.SourceContains("\"./bg.png\""),
            "Relative hook-body asset path leaked through unrewritten."
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
        Assert.True(
            result.SourceContains("V.Box("),
            "Expected V.Box() call from the bare assignment"
        );
        Assert.True(
            result.SourceContains("V.Label("),
            "Expected V.Label() inside the bare-assigned Box"
        );
    }

    // ── RewriteReturnsForInline unit tests ──────────────────────────────

    [Theory]
    [InlineData("return null;", "continue;")]
    [InlineData("  return null;", "  continue;")]
    [InlineData("return null ;", "continue;")]
    public void RewriteReturns_ReturnNull_BecomesContinue(string input, string expected)
    {
        var result = ReactiveUITK.SourceGenerator.Emitter.EmitContext.RewriteReturnsForInline(
            input,
            "__r"
        );
        Assert.Equal(expected, result);
    }

    [Fact]
    public void RewriteReturns_ReturnExpr_BecomesAddContinue()
    {
        var result = ReactiveUITK.SourceGenerator.Emitter.EmitContext.RewriteReturnsForInline(
            "return V.Label();",
            "__r"
        );
        Assert.Equal("__r.Add(V.Label()); continue;", result);
    }

    [Fact]
    public void RewriteReturns_ReturnParenExpr_UnwrapsParens()
    {
        var result = ReactiveUITK.SourceGenerator.Emitter.EmitContext.RewriteReturnsForInline(
            "return (V.Label());",
            "__r"
        );
        Assert.Equal("__r.Add(V.Label()); continue;", result);
    }

    [Fact]
    public void RewriteReturns_SetupCodeThenReturn_PreservesSetup()
    {
        var input = "var x = 1; return (V.Label());";
        var result = ReactiveUITK.SourceGenerator.Emitter.EmitContext.RewriteReturnsForInline(
            input,
            "__r"
        );
        Assert.Equal("var x = 1; __r.Add(V.Label()); continue;", result);
    }

    [Fact]
    public void RewriteReturns_GuardReturnNull_ThenReturnExpr()
    {
        var input = "if (x) { return null; } return (V.Label());";
        var result = ReactiveUITK.SourceGenerator.Emitter.EmitContext.RewriteReturnsForInline(
            input,
            "__r"
        );
        Assert.Equal("if (x) { continue; } __r.Add(V.Label()); continue;", result);
    }

    [Fact]
    public void RewriteReturns_LambdaReturnNotRewritten()
    {
        var input = "Func<int> f = () => { return 42; }; return (V.Label());";
        var result = ReactiveUITK.SourceGenerator.Emitter.EmitContext.RewriteReturnsForInline(
            input,
            "__r"
        );
        Assert.Contains("return 42;", result);
        Assert.Contains("__r.Add(V.Label()); continue;", result);
    }

    [Fact]
    public void RewriteReturns_StringWithReturnNotRewritten()
    {
        var input = "var s = \"return null;\"; return (V.Label());";
        var result = ReactiveUITK.SourceGenerator.Emitter.EmitContext.RewriteReturnsForInline(
            input,
            "__r"
        );
        Assert.Contains("\"return null;\"", result);
        Assert.Contains("__r.Add(V.Label()); continue;", result);
    }

    [Fact]
    public void RewriteReturns_BreakAndContinuePassThrough()
    {
        var input = "if (i < 3) { continue; } if (i > 6) { break; } return (V.Label());";
        var result = ReactiveUITK.SourceGenerator.Emitter.EmitContext.RewriteReturnsForInline(
            input,
            "__r"
        );
        Assert.Contains("continue;", result);
        Assert.Contains("break;", result);
        Assert.Contains("__r.Add(V.Label()); continue;", result);
    }

    [Fact]
    public void RewriteReturns_CustomListVar()
    {
        var result = ReactiveUITK.SourceGenerator.Emitter.EmitContext.RewriteReturnsForInline(
            "return V.Box();",
            "__items"
        );
        Assert.Equal("__items.Add(V.Box()); continue;", result);
    }

    // ── OPT-V2-2 Phase A: static-style hoisting ────────────────────────────
    //
    // These tests verify the SG hoists `style={new Style { ... }}` initializers
    // to class-level `static readonly Style __sty_N = ...;` fields when every
    // initializer value is a compile-time constant, and falls back to per-render
    // pool rent (or raw allocation for tuple form) otherwise.

    [Fact]
    public void StyleHoist_TupleSyntaxAllLiterals_HoistedToStaticField()
    {
        const string src = """
            component HoistTuple {
                return (
                    <Box style={new Style { (StyleKeys.Width, 100f), (StyleKeys.Height, 50f) }} />
                );
            }
            """;
        var result = GeneratorTestHelper.Run(src, "HoistTuple.uitkx");

        Assert.True(
            result.SourceWasProduced,
            $"No source produced. Diagnostics: {string.Join(", ", result.Diagnostics.Select(d => d.GetMessage()))}"
        );
        Assert.True(
            result.SourceContains(
                "private static readonly global::ReactiveUITK.Props.Typed.Style __sty_0"
            ),
            $"Expected hoisted static field __sty_0. Got:\n{result.GeneratedSource}"
        );
        Assert.True(
            result.SourceContains("__sty_0"),
            "Expected the hoisted style to be referenced at the call site"
        );
        Assert.False(
            result.SourceContains("Style.__Rent()"),
            "Hoisted styles must not call Style.__Rent()"
        );
    }

    [Fact]
    public void StyleHoist_SetterSyntaxAllLiterals_HoistedToStaticField()
    {
        const string src = """
            component HoistSetter {
                return (
                    <Box style={new Style { Width = 100f, Height = 50f }} />
                );
            }
            """;
        var result = GeneratorTestHelper.Run(src, "HoistSetter.uitkx");

        Assert.True(result.SourceWasProduced);
        Assert.True(
            result.SourceContains(
                "private static readonly global::ReactiveUITK.Props.Typed.Style __sty_0"
            ),
            $"Expected hoisted static field. Got:\n{result.GeneratedSource}"
        );
        Assert.False(
            result.SourceContains("Style.__Rent()"),
            "Setter-form all-literal style must not call __Rent()"
        );
    }

    [Fact]
    public void StyleHoist_DynamicValue_StaysOnRentPath()
    {
        const string src = """
            component DynamicStyle {
                var w = 100f;
                return (
                    <Box style={new Style { Width = w }} />
                );
            }
            """;
        var result = GeneratorTestHelper.Run(src, "DynamicStyle.uitkx");

        Assert.True(result.SourceWasProduced);
        Assert.False(
            result.SourceContains("__sty_"),
            $"Style with non-literal value must NOT be hoisted. Got:\n{result.GeneratedSource}"
        );
        Assert.True(
            result.SourceContains("Style.__Rent()"),
            "Dynamic style must still rent from the pool"
        );
    }

    [Fact]
    public void StyleHoist_NewColorWithLiteralArgs_Hoists()
    {
        const string src = """
            component ColorLiteral {
                return (
                    <Box style={new Style { (StyleKeys.BackgroundColor, new Color(0.15f, 0.15f, 0.15f, 0.9f)) }} />
                );
            }
            """;
        var result = GeneratorTestHelper.Run(src, "ColorLiteral.uitkx");

        Assert.True(result.SourceWasProduced);
        Assert.True(
            result.SourceContains(
                "private static readonly global::ReactiveUITK.Props.Typed.Style __sty_0"
            ),
            $"new Color(literal-args) must be hoist-safe. Got:\n{result.GeneratedSource}"
        );
    }

    [Fact]
    public void StyleHoist_DottedEnumRef_Hoists()
    {
        const string src = """
            component EnumRef {
                return (
                    <Box style={new Style { (StyleKeys.FlexDirection, "column") }} />
                );
            }
            """;
        var result = GeneratorTestHelper.Run(src, "EnumRef.uitkx");

        Assert.True(result.SourceWasProduced);
        Assert.True(
            result.SourceContains("__sty_0"),
            $"Tuple with string-literal value must hoist. Got:\n{result.GeneratedSource}"
        );
    }

    [Fact]
    public void StyleHoist_MultipleSiblings_GetUniqueIds()
    {
        const string src = """
            component TwoStatics {
                return (
                    <Box>
                        <Label text="a" style={new Style { (StyleKeys.Width, 10f) }} />
                        <Label text="b" style={new Style { (StyleKeys.Width, 20f) }} />
                    </Box>
                );
            }
            """;
        var result = GeneratorTestHelper.Run(src, "TwoStatics.uitkx");

        Assert.True(result.SourceWasProduced);
        Assert.True(result.SourceContains("__sty_0"), "Expected __sty_0");
        Assert.True(result.SourceContains("__sty_1"), "Expected __sty_1 for second hoist");
    }

    [Fact]
    public void StyleHoist_EmptyStyleBody_NotHoisted()
    {
        const string src = """
            component EmptyStyle {
                return (
                    <Box style={new Style { }} />
                );
            }
            """;
        var result = GeneratorTestHelper.Run(src, "EmptyStyle.uitkx");

        Assert.True(result.SourceWasProduced);
        Assert.False(
            result.SourceContains("__sty_"),
            "Empty-body Style is deferred to a future EmptyStyle singleton; must not hoist"
        );
    }

    [Fact]
    public void StyleHoist_MixedStaticAndDynamic_HoistsOnlyStatic()
    {
        const string src = """
            component Mixed {
                var sz = 50f;
                return (
                    <Box>
                        <Label text="a" style={new Style { (StyleKeys.Width, 10f) }} />
                        <Label text="b" style={new Style { Width = sz }} />
                    </Box>
                );
            }
            """;
        var result = GeneratorTestHelper.Run(src, "Mixed.uitkx");

        Assert.True(result.SourceWasProduced);
        Assert.True(result.SourceContains("__sty_0"), "Static style must hoist");
        Assert.True(result.SourceContains("Style.__Rent()"), "Dynamic style must still rent");
        Assert.False(result.SourceContains("__sty_1"), "Only one hoist expected");
    }

    [Fact]
    public void StyleHoist_TupleWithVariableValue_NotHoisted()
    {
        const string src = """
            component TupleDynamic {
                var w = 100f;
                return (
                    <Box style={new Style { (StyleKeys.Width, w) }} />
                );
            }
            """;
        var result = GeneratorTestHelper.Run(src, "TupleDynamic.uitkx");

        Assert.True(result.SourceWasProduced);
        Assert.False(
            result.SourceContains("__sty_"),
            $"Tuple with bare-identifier value must NOT hoist. Got:\n{result.GeneratedSource}"
        );
    }

    [Fact]
    public void StyleHoist_NonStyleAttributeUnaffected()
    {
        const string src = """
            component PlainBox {
                return (
                    <Box>
                        <Label text="hi" />
                    </Box>
                );
            }
            """;
        var result = GeneratorTestHelper.Run(src, "PlainBox.uitkx");

        Assert.True(result.SourceWasProduced);
        Assert.False(result.SourceContains("__sty_"), "No style attribute → no hoist");
    }

    [Fact]
    public void StyleHoist_InstanceMemberOnLocal_NotHoisted()
    {
        // Regression: `areaSize.x` is a local-variable member access, NOT a static
        // reference. Must not be hoisted to a class-level field (where the local
        // would be out of scope, causing CS0103). The classifier requires the
        // first dotted segment to start uppercase (PascalCase = type/enum/static
        // class convention) — locals are camelCase and are correctly rejected.
        const string src = """
            component AreaBox {
                var areaSize = new UnityEngine.Vector2(100f, 50f);
                return (
                    <Box style={new Style { Width = areaSize.x, Height = areaSize.y }} />
                );
            }
            """;
        var result = GeneratorTestHelper.Run(src, "AreaBox.uitkx");

        Assert.True(result.SourceWasProduced);
        Assert.False(
            result.SourceContains("__sty_"),
            $"Instance-member access on local must NOT hoist (would cause CS0103). Got:\n{result.GeneratedSource}"
        );
        Assert.True(
            result.SourceContains("Style.__Rent()"),
            "Local-derived style must still rent from the pool"
        );
    }

    [Fact]
    public void StyleHoist_InstanceMemberInTuple_NotHoisted()
    {
        // Same regression in tuple form: (StyleKeys.Width, box.size) — `box.size`
        // is a local foreach-variable member access; must not hoist.
        const string src = """
            component Boxes {
                var boxes = new[] { new { size = 5f } };
                return (
                    <Box>
                        @foreach (var box in boxes) {
                            return (
                                <Label text="x" style={new Style { (StyleKeys.Width, box.size) }} />
                            );
                        }
                    </Box>
                );
            }
            """;
        var result = GeneratorTestHelper.Run(src, "Boxes.uitkx");

        Assert.True(result.SourceWasProduced);
        Assert.False(
            result.SourceContains("__sty_"),
            $"Instance-member on loop local must NOT hoist. Got:\n{result.GeneratedSource}"
        );
    }

    // ── Router primitives — alias-map resolution ────────────────────────────
    // Verifies the source generator wires every <RouterTagAliases.Map> entry
    // through to a V.Func(<TypeName>.Render, ...) emission, exercising the
    // shared dictionary at Shared/Core/Router/RouterTagAliases.cs.

    [Fact]
    public void RouterTag_EmitsRouterFunc()
    {
        var result = GeneratorTestHelper.Run(Wrap("<Router/>"));
        Assert.True(result.SourceWasProduced);
        Assert.True(
            result.SourceContains("RouterFunc.Render"),
            $"Expected V.Func(RouterFunc.Render, ...). Got:\n{result.GeneratedSource}"
        );
    }

    [Fact]
    public void RouteTag_EmitsRouteFunc()
    {
        var result = GeneratorTestHelper.Run(Wrap("<Route path=\"/\"/>"));
        Assert.True(result.SourceContains("RouteFunc.Render"));
    }

    [Fact]
    public void OutletTag_EmitsOutletFunc()
    {
        var result = GeneratorTestHelper.Run(Wrap("<Outlet/>"));
        Assert.True(
            result.SourceContains("OutletFunc.Render"),
            $"Expected OutletFunc.Render call. Got:\n{result.GeneratedSource}"
        );
    }

    [Fact]
    public void RoutesTag_EmitsRoutesFunc()
    {
        var result = GeneratorTestHelper.Run(Wrap("<Routes><Route path=\"/\"/></Routes>"));
        Assert.True(
            result.SourceContains("RoutesFunc.Render"),
            $"Expected RoutesFunc.Render call. Got:\n{result.GeneratedSource}"
        );
    }

    [Fact]
    public void NavLinkTag_EmitsNavLinkFunc()
    {
        var result = GeneratorTestHelper.Run(Wrap("<NavLink to=\"/\" label=\"Home\"/>"));
        Assert.True(
            result.SourceContains("NavLinkFunc.Render"),
            $"Expected NavLinkFunc.Render call. Got:\n{result.GeneratedSource}"
        );
    }

    [Fact]
    public void NavigateTag_EmitsNavigateFunc()
    {
        var result = GeneratorTestHelper.Run(Wrap("<Navigate to=\"/home\"/>"));
        Assert.True(
            result.SourceContains("NavigateFunc.Render"),
            $"Expected NavigateFunc.Render call. Got:\n{result.GeneratedSource}"
        );
    }

    // NOTE: Per-attribute pass-through coverage for the new Index/End/CaseSensitive
    // properties cannot be validated in this isolated test compilation because the
    // generator skips its strongly-typed props block when the *FuncProps type is
    // not resolvable (no Unity reference to ReactiveUITK.Shared.dll here).  The
    // Index/End/CaseSensitive attributes follow the exact same emission path as
    // the existing Router/Route/Link properties, which are exercised end-to-end
    // by the FormatterSnapshotTests against real .uitkx samples
    // (Samples/Components/RouterDemoFunc/RouterOutletDemo.uitkx now uses these
    // attributes in production-shaped markup).
}
