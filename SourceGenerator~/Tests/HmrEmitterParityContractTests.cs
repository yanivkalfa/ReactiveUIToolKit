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

        Assert.Contains(output.Diagnostics, d => d.Id == "UITKX0104");
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
        Assert.Contains(
            "new global::ReactiveUITK.HmrParity.Greeter.GreeterProps",
            output.GeneratedSource
        );
        Assert.DoesNotContain("__Rent", output.GeneratedSource);
    }

    // ── Module static-method HMR rewrite (v0.4.20, Issue (a)) ───────────────────

    /// <summary>
    /// Every <c>public static</c> method declared inside a <c>module {…}</c>
    /// body must compile into the HMR trampoline triplet:
    /// <c>__hmr_&lt;Name&gt;_h&lt;sig&gt;</c> delegate field,
    /// <c>__&lt;Name&gt;_body_h&lt;sig&gt;</c> private body method,
    /// and a public trampoline that checks <c>HmrState.IsActive</c>.
    /// </summary>
    [Fact]
    public void Sg_ModuleStaticMethod_GeneratesTrampoline()
    {
        var output = GeneratorTestHelper.Run(
            """
            @namespace ReactiveUITK.HmrParity

            module Calculator {
                public static int Add(int a, int b) {
                    return a + b;
                }
            }
            """
        );

        Assert.NotNull(output.GeneratedSource);
        // HMR delegate slot — initialised to body method.
        Assert.Contains("__hmr_Add_h", output.GeneratedSource);
        // Body method synthesized with __<Name>_body_h<sig> shape.
        Assert.Contains("__Add_body_h", output.GeneratedSource);
        // Trampoline guards on HmrState.IsActive — exact pattern match.
        Assert.Contains("global::ReactiveUITK.Core.HmrState.IsActive", output.GeneratedSource);
        // Public surface is preserved (caller signature unchanged).
        Assert.Contains("public static int Add(int a, int b)", output.GeneratedSource);
    }

    /// <summary>
    /// <c>ref</c>/<c>out</c>/<c>in</c>/<c>params</c> parameters cannot be
    /// expressed by <c>Func&lt;&gt;</c>/<c>Action&lt;&gt;</c>. The rewriter
    /// MUST emit a custom delegate type so by-ref methods (e.g. DoomGame's
    /// <c>Tick(ref GameState st, …)</c>) hot-reload correctly.
    /// </summary>
    [Fact]
    public void Sg_ModuleStaticMethod_RefParam_UsesCustomDelegate()
    {
        var output = GeneratorTestHelper.Run(
            """
            @namespace ReactiveUITK.HmrParity

            module RefHolder {
                public static void Bump(ref int value) {
                    value++;
                }
            }
            """
        );

        Assert.NotNull(output.GeneratedSource);
        // Custom delegate declaration carrying the `ref` modifier — the whole
        // point of NOT using Action<int>. Visibility tracks the original method
        // (here `public`) so the synthesized declarations never tighten access.
        Assert.Contains("public delegate void __Bump_h", output.GeneratedSource);
        Assert.Contains("ref int value", output.GeneratedSource);
        // Argument forwarding must keep the `ref` keyword.
        Assert.Contains("(ref value)", output.GeneratedSource);
        // Sanity: framework Action<int> must NOT be used as the field type.
        Assert.DoesNotContain("global::System.Action<int> __hmr_Bump", output.GeneratedSource);
    }

    /// <summary>
    /// Two static methods sharing the same name but different signatures
    /// must produce two distinct <c>__hmr_*</c> field names. The signature
    /// hash is the disambiguator; without it overloads would collide on
    /// the same field slot and only the first emitted would HMR.
    /// </summary>
    [Fact]
    public void Sg_ModuleStaticMethod_Overloads_GetDistinctHashes()
    {
        var output = GeneratorTestHelper.Run(
            """
            @namespace ReactiveUITK.HmrParity

            module Overloaded {
                public static int Foo(int x) => x;
                public static string Foo(string x) => x;
            }
            """
        );

        Assert.NotNull(output.GeneratedSource);
        // Both overloads must appear with their own __hmr_Foo_h<hash> field.
        var matches = System.Text.RegularExpressions.Regex.Matches(
            output.GeneratedSource,
            @"__hmr_Foo_h[0-9a-f]{8}"
        );
        // Each overload produces exactly one field declaration AND one
        // call inside the trampoline body — so we expect at least 2
        // distinct hashes across all matches.
        var distinctHashes = matches
            .Cast<System.Text.RegularExpressions.Match>()
            .Select(m => m.Value)
            .Distinct()
            .ToList();
        Assert.True(
            distinctHashes.Count >= 2,
            $"Expected at least 2 distinct overload hashes, got {distinctHashes.Count}: "
                + string.Join(", ", distinctHashes)
        );
    }

    /// <summary>
    /// Generic methods use the <c>MethodInfo</c> + <c>ConcurrentDictionary</c>
    /// cache pattern (mirrors HookEmitter generic case). The cache keys per
    /// closed type, the <c>MethodInfo</c> field is rebound by the swapper,
    /// and the value is a delegate built via <c>MakeGenericMethod</c>.
    /// </summary>
    [Fact]
    public void Sg_ModuleGenericMethod_UsesMethodInfoCache()
    {
        var output = GeneratorTestHelper.Run(
            """
            @namespace ReactiveUITK.HmrParity

            module Box {
                public static T Identity<T>(T x) {
                    return x;
                }
            }
            """
        );

        Assert.NotNull(output.GeneratedSource);
        // MethodInfo backing field for the generic case. Visibility tracks the
        // original method (here `public`).
        Assert.Contains(
            "public static global::System.Reflection.MethodInfo __hmr_Identity_h",
            output.GeneratedSource
        );
        // Per-closed-type delegate cache.
        Assert.Contains(
            "global::System.Collections.Concurrent.ConcurrentDictionary<global::System.Type, global::System.Delegate>",
            output.GeneratedSource
        );
        // MakeGenericMethod call that closes the open MethodInfo per call site.
        Assert.Contains("MakeGenericMethod", output.GeneratedSource);
        // Trampoline preserves the generic signature.
        Assert.Contains("public static T Identity<T>(T x)", output.GeneratedSource);
    }

    /// <summary>
    /// Regression for v0.5.1: generic module methods generated invalid C#
    /// in v0.4.19 / v0.5.0 due to two emit bugs in <c>ModuleBodyRewriter</c>:
    /// <list type="number">
    ///   <item><description>
    ///     <c>AppendTypeArgs</c> emitted bare type-parameter names into the
    ///     <c>MakeGenericMethod(...)</c> argument list, producing
    ///     <c>MakeGenericMethod(TProps, TResult)</c> — CS0119
    ///     ('TProps' is a type, which is not valid in the given context).
    ///     The fix wraps each name in <c>typeof(...)</c>.
    ///   </description></item>
    ///   <item><description>
    ///     The synthesized <c>MethodInfo</c> HMR field was initialized as
    ///     <c>= null;</c>. The field MUST start null (the trampoline checks
    ///     <c>!= null</c> to fall through to the body method until the HMR
    ///     swapper fills it via reflection), but consumer projects with
    ///     <c>&lt;Nullable&gt;enable&lt;/Nullable&gt;</c> fired CS8625. The
    ///     fix emits <c>= null!;</c> — same runtime value, no warning.
    ///   </description></item>
    /// </list>
    /// This test compiles the generated C# end-to-end with
    /// <c>UNITY_EDITOR</c> defined and nullable-context enabled. Substring
    /// checks alone (as in <see cref="Sg_ModuleGenericMethod_UsesMethodInfoCache"/>)
    /// missed both bugs because the broken output still contained
    /// <c>MakeGenericMethod</c> and the field declaration.
    /// </summary>
    [Fact]
    public void Sg_ModuleGenericMethod_GeneratedCodeCompiles_NoCS0119_NoCS8625()
    {
        // Multi-type-parameter generic with constraints — exercises the
        // exact shape that broke in JustStayOn (Dialogs.utils.uitkx Register,
        // RegisterComponent, Open, TryResolve, TryClose, IsRegistered).
        var output = GeneratorTestHelper.Run(
            """
            @namespace ReactiveUITK.HmrParity

            module Dialogs {
                public static int Register<TProps, TResult>(TProps props, TResult result)
                    where TProps : class
                {
                    return 1;
                }

                public static TResult Open<TResult>(string id)
                {
                    return default(TResult);
                }
            }
            """
        );

        Assert.NotNull(output.GeneratedSource);

        // Stub for ReactiveUITK.Core.HmrState (referenced by the trampoline).
        const string Stubs =
            """
            namespace ReactiveUITK.Core
            {
                public static class HmrState
                {
                    public static bool IsActive => false;
                }
            }
            """;

        var parseOptions = new Microsoft.CodeAnalysis.CSharp.CSharpParseOptions(
            languageVersion: Microsoft.CodeAnalysis.CSharp.LanguageVersion.Latest,
            preprocessorSymbols: new[] { "UNITY_EDITOR" }
        );

        var trees = new[]
        {
            Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(
                output.GeneratedSource!,
                parseOptions
            ),
            Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(Stubs, parseOptions),
        };

        // Reference every assembly the host runtime trusts. Simpler &
        // more reliable than hand-picking System.Reflection / System.Collections.Concurrent
        // / System.ComponentModel under .NET 10 where types are split across
        // many ref assemblies.
        var trustedAssemblies =
            (string)System.AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;
        var refs = trustedAssemblies
            .Split(System.IO.Path.PathSeparator)
            .Where(p => !string.IsNullOrEmpty(p) && System.IO.File.Exists(p))
            .Select(p => (Microsoft.CodeAnalysis.MetadataReference)
                Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(p))
            .ToArray();

        var compilation = Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create(
            "ModuleGenericCompileCheck",
            trees,
            refs,
            new Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions(
                Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: Microsoft.CodeAnalysis.NullableContextOptions.Enable
            )
        );

        // Bug-targeted assertion. We don't try to compile the *whole*
        // generated file (it `using`s UnityEngine, ReactiveUITK.Props,
        // AssetHelpers, etc. — providing all those stubs would balloon
        // this test). Instead we filter to the two diagnostic IDs that
        // pinpoint the v0.4.19 / v0.5.0 regression:
        //   • CS0119 — `MakeGenericMethod(TProps, TResult)` (bare type
        //     parameter names instead of typeof(...)).
        //   • CS8625 — `MethodInfo … = null;` under nullable-enabled.
        // Either appearing means the bug is back; neither appearing in a
        // file that exercises the generic-module-method path means the
        // emitter is producing valid C# for that shape.
        var bugDiags = compilation
            .GetDiagnostics()
            .Where(d => d.Id == "CS0119" || d.Id == "CS8625")
            .ToList();

        Assert.True(
            bugDiags.Count == 0,
            "Generic module method emit regressed (CS0119 or CS8625):\n"
                + string.Join("\n", bugDiags.Select(d => d.ToString()))
                + "\n\n--- Generated Source ---\n"
                + output.GeneratedSource
        );
    }

    /// <summary>
    /// <c>const</c> fields, <c>static readonly</c> fields, mutable static
    /// fields, instance methods, properties, and nested types must all be
    /// emitted verbatim — never wrapped in trampolines. Anything else would
    /// be a regression on existing module behaviour.
    /// </summary>
    [Fact]
    public void Sg_ModuleNonMethodMembers_StayVerbatim()
    {
        var output = GeneratorTestHelper.Run(
            """
            @namespace ReactiveUITK.HmrParity
            @using System

            module Mixed {
                public const int VERSION = 7;
                public static readonly string Tag = "abc";
                private static int _counter;
                public enum Color { Red, Green, Blue }
                public struct Pair { public int A; public int B; }
            }
            """
        );

        Assert.NotNull(output.GeneratedSource);
        // No __hmr_ prefix should appear at all — there are no static methods.
        Assert.DoesNotContain("__hmr_", output.GeneratedSource);
        // Original member declarations preserved (substring matches the source).
        Assert.Contains("public const int VERSION = 7", output.GeneratedSource);
        Assert.Contains("public static readonly string Tag", output.GeneratedSource);
        Assert.Contains("private static int _counter", output.GeneratedSource);
        Assert.Contains("public enum Color", output.GeneratedSource);
        Assert.Contains("public struct Pair", output.GeneratedSource);
    }

    /// <summary>
    /// Instance methods inside a module are not hot-swappable (modules are
    /// static-utility containers; instance dispatch needs a <c>this</c>-bound
    /// delegate per receiver). They must emit verbatim.
    /// </summary>
    [Fact]
    public void Sg_ModuleInstanceMethod_NotRewritten()
    {
        var output = GeneratorTestHelper.Run(
            """
            @namespace ReactiveUITK.HmrParity

            module HasInstanceMethod {
                public void Foo() { }
                public static void Bar() { }
            }
            """
        );

        Assert.NotNull(output.GeneratedSource);
        // Only `Bar` is rewritten — `Foo` must stay verbatim.
        Assert.Contains("__hmr_Bar_h", output.GeneratedSource);
        Assert.DoesNotContain("__hmr_Foo_h", output.GeneratedSource);
        Assert.Contains("public void Foo()", output.GeneratedSource);
    }

    /// <summary>
    /// Default parameter values must survive on the public trampoline
    /// (callers rely on them) but be elided from the body method (which
    /// receives explicit arguments from the trampoline).
    /// </summary>
    [Fact]
    public void Sg_ModuleStaticMethod_DefaultParams_PreservedOnTrampoline()
    {
        var output = GeneratorTestHelper.Run(
            """
            @namespace ReactiveUITK.HmrParity

            module Defaults {
                public static int Add(int a, int b = 5) {
                    return a + b;
                }
            }
            """
        );

        Assert.NotNull(output.GeneratedSource);
        // Trampoline: defaults present.
        Assert.Contains("public static int Add(int a, int b = 5)", output.GeneratedSource);
        // Body method: defaults stripped (explicit values forwarded).
        Assert.Contains("private static int __Add_body_h", output.GeneratedSource);
    }

    // ── Media (v0.4.20) — <Audio>, <Video>, useSfx ─────────────────────────────

    /// <summary>
    /// <c>&lt;Audio&gt;</c> is a Func-Component but, because
    /// <c>V.Audio(AudioProps, key, params children)</c> matches the
    /// auto-discovery shape (first param ends in <c>Props</c>, last param
    /// is <c>params VirtualNode[]</c>), SG and HMR both emit it through the
    /// Typed-with-children path: <c>V.Audio(BaseProps.__Rent&lt;AudioProps&gt;()…)</c>.
    /// HMR auto-discovery (Phase 1) ensures parity.
    /// </summary>
    [Fact]
    public void Sg_AudioElement_EmitsThroughTypedPath()
    {
        var output = GeneratorTestHelper.Run(
            """
            @namespace ReactiveUITK.HmrParity

            component MusicPlayer {
                return (<Audio Autoplay={true} Loop={true} Volume={0.5f} />);
            }
            """
        );

        Assert.NotNull(output.GeneratedSource);
        // Typed-path emission: V.Audio(...) with rented AudioProps.
        Assert.Contains("V.Audio(", output.GeneratedSource);
        Assert.Contains("AudioProps", output.GeneratedSource);
        // Specific prop assignments survive.
        Assert.Contains("Autoplay = true", output.GeneratedSource);
        Assert.Contains("Loop = true", output.GeneratedSource);
        // Pooled (BaseProps.__Rent), NOT new — Audio is a typed built-in
        // whose props extend BaseProps.
        Assert.Contains("__Rent<", output.GeneratedSource);
    }

    /// <summary>
    /// <c>&lt;Video&gt;</c> mirrors <c>&lt;Audio&gt;</c>'s emit shape via
    /// auto-discovery. Verifies the URL streaming case (<c>Url</c> string
    /// attribute) round-trips through the typed-prop pipeline.
    /// </summary>
    [Fact]
    public void Sg_VideoElement_EmitsThroughTypedPath()
    {
        var output = GeneratorTestHelper.Run(
            """
            @namespace ReactiveUITK.HmrParity

            component VideoPanel {
                return (<Video Url="https://example.com/clip.mp4" Autoplay={true} Muted={true} />);
            }
            """
        );

        Assert.NotNull(output.GeneratedSource);
        Assert.Contains("V.Video(", output.GeneratedSource);
        Assert.Contains("VideoProps", output.GeneratedSource);
        Assert.Contains("Url = ", output.GeneratedSource);
        Assert.Contains("Autoplay = true", output.GeneratedSource);
        Assert.Contains("Muted = true", output.GeneratedSource);
    }

    /// <summary>
    /// <c>&lt;Video&gt;</c> accepts overlay children (e.g. play/pause buttons
    /// rendered on top of the video). Confirms SG emits the children-array
    /// argument so HMR's TypedC dispatch produces the same shape.
    /// </summary>
    [Fact]
    public void Sg_VideoElement_AcceptsOverlayChildren()
    {
        var output = GeneratorTestHelper.Run(
            """
            @namespace ReactiveUITK.HmrParity

            component VideoWithOverlay {
                return (
                    <Video Loop={true}>
                        <Button text="Play" />
                    </Video>
                );
            }
            """
        );

        Assert.NotNull(output.GeneratedSource);
        // Both V.Video and the inner V.Button must appear in the same
        // generated unit — confirms TypedC dispatch flattened the child.
        Assert.Contains("V.Video(", output.GeneratedSource);
        Assert.Contains("V.Button(", output.GeneratedSource);
    }

    /// <summary>
    /// <c>useSfx()</c> hook reports as <c>UseSfx</c> in the generated
    /// <c>HookSignatureAttribute</c>. The signature is what HMR uses to
    /// detect rude hook-order edits; if the hook is not in the regex
    /// whitelist on either side, it's invisible to that mechanism.
    /// </summary>
    [Fact]
    public void Sg_UseSfxHook_AppearsInHookSignature()
    {
        var output = GeneratorTestHelper.Run(
            """
            @namespace ReactiveUITK.HmrParity

            component SfxButton {
                var playSfx = useSfx();
                return (<Button text="Click" />);
            }
            """
        );

        Assert.NotNull(output.GeneratedSource);
        // Lower-case alias rewritten to canonical Hooks.UseSfx call.
        Assert.Contains("Hooks.UseSfx(", output.GeneratedSource);
        // Hook-signature attribute carries the UseSfx token.
        Assert.Contains("HookSignature", output.GeneratedSource);
        Assert.Contains("UseSfx", output.GeneratedSource);
    }

    /// <summary>
    /// PascalCase form (<c>Hooks.UseSfx</c>) is also matched by the hook
    /// signature regex. Tests parity with the camelCase alias above.
    /// </summary>
    [Fact]
    public void Sg_UseSfxPascalCase_AlsoDetectedBySignatureRegex()
    {
        var output = GeneratorTestHelper.Run(
            """
            @namespace ReactiveUITK.HmrParity

            component SfxButtonPascal {
                var playSfx = Hooks.UseSfx();
                return (<Button text="Click" />);
            }
            """
        );

        Assert.NotNull(output.GeneratedSource);
        Assert.Contains("Hooks.UseSfx(", output.GeneratedSource);
        // Even though no alias-rewrite was needed (already PascalCase), the
        // signature regex must still detect it.
        Assert.Contains("UseSfx", output.GeneratedSource);
    }
}
