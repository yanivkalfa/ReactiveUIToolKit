using System.Linq;
using ReactiveUITK.SourceGenerator.Tests.Helpers;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests;

/// <summary>
/// HMR ↔ Source-Generator emitter parity contract tests.
///
/// <para>
/// HMR's <c>HmrCSharpEmitter</c> (under <c>Editor/HMR/</c>, in the Unity
/// <c>ReactiveUITK.Editor</c> assembly) is a hand-written transpiler that
/// must emit the same C# shape as the Roslyn-based source generator
/// (<c>SourceGenerator~/Emitter/CSharpEmitter.cs</c>) for any given
/// <c>.uitkx</c> input. When the SG learns a new emit shape, HMR must be
/// updated in lockstep — there is no shared emitter assembly.
/// </para>
///
/// <para>
/// These tests pin the SG-side emit invariants HMR currently mirrors. Each
/// test runs the SG against a tiny <c>.uitkx</c> source and asserts that the
/// generated C# contains the marker substring(s) HMR's emitter is expected
/// to produce too. If SG ever stops emitting one of these markers (because
/// the emit shape changed), the test fails and the corresponding HMR site
/// must be reviewed for parity.
/// </para>
///
/// <para>
/// This file does NOT (and cannot) load the HMR emitter directly — the
/// Editor asmdef pulls in <c>UnityEditor</c> which is not loadable from a
/// standalone .NET test runner. Instead, this is a one-way drift tripwire:
/// SG is the ground truth; HMR's parity is reviewed manually (and, where
/// the algorithm is small enough, mirrored verbatim — see
/// <see cref="HmrFindPropsTypeContractTests"/> for the existing example).
/// </para>
/// </summary>
public class HmrEmitterParityContractTests
{
    // ── Issue 5 — ref={x} routing on function components ────────────────────────

    /// <summary>
    /// SG resolves <c>ref={x}</c> on a function component to the matching
    /// <c>Ref&lt;T&gt;</c> property on the synthesized props object. HMR
    /// mirrors this at <c>HmrCSharpEmitter.EmitFuncComponent</c> by extracting
    /// <c>ref={x}</c> before the attribute filter loop and routing it through
    /// <c>FindPropsTypeAndRefSlot</c>. If SG ever stops emitting the routed
    /// assignment, the post-fix HMR code is wrong too.
    /// </summary>
    [Fact]
    public void Sg_RefOnFuncComponent_RoutesIntoPropsSlot()
    {
        // Two files: a peer component declaring a Ref<T> param + the
        // consumer that passes ref={x} into it. SG resolves the ref slot
        // via PropsResolver.TryGetRefParamPropName.
        var output = GeneratorTestHelper.RunMultiple(
            new[]
            {
                (
                    "MyTextField.uitkx",
                    """
                    @namespace ReactiveUITK.HmrParity
                    @using ReactiveUITK.Core

                    component MyTextField(Ref<object> inputRef = null) {
                        return (<TextField />);
                    }
                    """
                ),
                (
                    "App.uitkx",
                    """
                    @namespace ReactiveUITK.HmrParity
                    @using ReactiveUITK.Core
                    @using ReactiveUITK.HmrParity

                    component App {
                        var myRef = Hooks.UseRef<object>();
                        return (
                            <VisualElement>
                                <MyTextField ref={myRef} />
                            </VisualElement>
                        );
                    }
                    """
                ),
            },
            primaryFileName: "App.uitkx"
        );

        Assert.NotNull(output.GeneratedSource);
        // SG must emit the ref-routing assignment with the resolved prop name
        // (here: InputRef from `Ref<object> inputRef`). HMR mirrors this with
        // FindRefSlotName scanning Props for Ref<>/MutableRef<> properties.
        Assert.Contains("InputRef = myRef", output.GeneratedSource);
        // And no literal `Ref = ` should leak (that would indicate the resolver
        // fell through to "treat ref as a literal prop" — the pre-fix shape).
        Assert.DoesNotContain(" Ref = myRef", output.GeneratedSource);
    }

    // ── Issue 6 — JSX-valued attributes (the React-Router element={<X/>} case) ──

    /// <summary>
    /// SG recursively emits a nested <c>ElementNode</c> when it appears as the
    /// value of a JSX attribute, via <c>EmitJsxToString</c>. HMR mirrors this
    /// in <c>HmrCSharpEmitter.AttrToExpr</c>'s <c>JsxExpressionValue</c> case
    /// using the same <c>_sb</c>-capture pattern. If SG ever changes the
    /// nested-element representation (e.g. a different <c>V.*</c> entry-point),
    /// HMR's mirror must change too.
    /// </summary>
    [Fact]
    public void Sg_JsxAsAttributeValue_EmitsNestedElement()
    {
        var output = GeneratorTestHelper.RunMultiple(
            new[]
            {
                (
                    "Wrapper.uitkx",
                    """
                    @namespace ReactiveUITK.HmrParity
                    @using ReactiveUITK.Core

                    component Wrapper(VirtualNode header = null) {
                        return (<VisualElement />);
                    }
                    """
                ),
                (
                    "Page.uitkx",
                    """
                    @namespace ReactiveUITK.HmrParity
                    @using ReactiveUITK.HmrParity

                    component Page {
                        return (
                            <Wrapper header={<Label text="Hi" />} />
                        );
                    }
                    """
                ),
            },
            primaryFileName: "Page.uitkx"
        );

        Assert.NotNull(output.GeneratedSource);
        // The inner <Label> must actually be emitted as a V.Label call inline
        // inside the props initializer — not collapsed to null (which is what
        // HMR's pre-fix stub did, and which would make any element-typed prop
        // silently disappear after a hot reload).
        Assert.Contains("V.Label(", output.GeneratedSource);
        Assert.Contains("Header = V.Label(", output.GeneratedSource);
        Assert.DoesNotContain("Header = null", output.GeneratedSource);
    }

    // ── Issue 10 — UITKX0104 duplicate-key warning is emitted by SG ─────────────

    /// <summary>
    /// SG raises <c>UITKX0104</c> at compile time when sibling elements
    /// share a static-string <c>key={...}</c> value. HMR mirrors this with
    /// a <c>Debug.LogWarning</c> from <c>CheckDuplicateKeys</c> in its
    /// <c>EmitChildArgs</c>. The diagnostic id is the contract: if SG ever
    /// renumbers / suppresses the diagnostic, HMR must follow.
    /// </summary>
    [Fact]
    public void Sg_DuplicateSiblingKey_RaisesUitkx0104()
    {
        var output = GeneratorTestHelper.Run(
            """
            @namespace ReactiveUITK.HmrParity

            component Dup {
                return (
                    <VisualElement>
                        <Label key="a" text="one" />
                        <Label key="a" text="two" />
                    </VisualElement>
                );
            }
            """
        );

        Assert.Contains(
            output.Diagnostics,
            d => d.Id == "UITKX0104"
        );
    }

    /// <summary>
    /// Negative for <see cref="Sg_DuplicateSiblingKey_RaisesUitkx0104"/>:
    /// distinct keys must NOT trip the diagnostic. Pins the warning to the
    /// duplicate case alone so HMR's mirror does not over-fire.
    /// </summary>
    [Fact]
    public void Sg_DistinctSiblingKeys_NoUitkx0104()
    {
        var output = GeneratorTestHelper.Run(
            """
            @namespace ReactiveUITK.HmrParity

            component Distinct {
                return (
                    <VisualElement>
                        <Label key="a" text="one" />
                        <Label key="b" text="two" />
                    </VisualElement>
                );
            }
            """
        );

        Assert.DoesNotContain(output.Diagnostics, d => d.Id == "UITKX0104");
    }

    // ── Issue 9 (retracted) — function-component props are NOT pooled ───────────

    /// <summary>
    /// Pinned regression: HMR's <c>EmitFuncComponent</c> uses
    /// <c>new {PropsName} { ... }</c> for function-component invocations,
    /// not <c>BaseProps.__Rent</c>. This test confirms SG does the same —
    /// any future SG change toward pooling here would require HMR to follow
    /// (and would invalidate the original Issue 9 retraction).
    /// </summary>
    [Fact]
    public void Sg_FuncComponentInvocation_UsesNewNotRent()
    {
        var output = GeneratorTestHelper.RunMultiple(
            new[]
            {
                (
                    "Greeter.uitkx",
                    """
                    @namespace ReactiveUITK.HmrParity

                    component Greeter(string name = "world") {
                        return (<Label text={"hi " + name} />);
                    }
                    """
                ),
                (
                    "Host.uitkx",
                    """
                    @namespace ReactiveUITK.HmrParity
                    @using ReactiveUITK.HmrParity

                    component Host {
                        return (<Greeter name="copilot" />);
                    }
                    """
                ),
            },
            primaryFileName: "Host.uitkx"
        );

        Assert.NotNull(output.GeneratedSource);
        // Greeter is a function component — its props must be constructed with
        // `new ...GreeterProps { ... }`, NOT `BaseProps.__Rent<GreeterProps>()`.
        // The synthesized {Name}Props class derives from IProps (not BaseProps)
        // and cannot be pooled. SG emits the FQN form.
        Assert.Contains("new global::ReactiveUITK.HmrParity.Greeter.GreeterProps", output.GeneratedSource);
        Assert.DoesNotContain("__Rent", output.GeneratedSource);
    }
}
