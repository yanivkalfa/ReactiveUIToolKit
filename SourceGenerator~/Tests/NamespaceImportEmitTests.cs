using System.Linq;
using ReactiveUITK.SourceGenerator.Tests.Helpers;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests
{
    /// <summary>
    /// Namespace-import unification plan, step 3 — emit equivalence. `import "@Ns"` desugars in the
    /// parser to the same <c>DirectiveSet.Usings</c> entry a <c>@using Ns</c> line produces, so the
    /// generated C# must be byte-identical between the two spellings. This is the load-bearing
    /// guarantee that lets the new syntax ship without touching any emitter or the HMR path.
    /// </summary>
    public sealed class NamespaceImportEmitTests
    {
        // NOTE: the test compilation references only the core assembly, so namespaces used here
        // must exist there (System.Text, System.Collections.Generic) — see UITKX2316 (unknown
        // namespace) which is now live and would flag e.g. UnityEngine.* in this minimal setup.

        [Fact]
        public void NamespaceImport_And_AtUsing_ProduceIdenticalComponentSource()
        {
            const string withAtUsing =
                "@namespace TestNs\n@using System.Text\ncomponent MyComp {\n  return (<VisualElement />);\n}";
            const string withNsImport =
                "@namespace TestNs\nimport \"@System.Text\"\ncomponent MyComp {\n  return (<VisualElement />);\n}";

            var a = GeneratorTestHelper.Run(withAtUsing);
            var b = GeneratorTestHelper.Run(withNsImport);

            Assert.True(a.SourceWasProduced);
            Assert.True(b.SourceWasProduced);
            Assert.Equal(a.GeneratedSource, b.GeneratedSource);
            // And the namespace actually made it into the emitted usings.
            Assert.Contains("using System.Text;", b.GeneratedSource!);
        }

        [Fact]
        public void NamespaceImport_And_AtUsing_ProduceIdenticalHookSource()
        {
            const string withAtUsing =
                "@namespace TestNs\n@using System.Text\nhook UseFoo(int initial = 0) -> int {\n  return initial;\n}";
            const string withNsImport =
                "@namespace TestNs\nimport \"@System.Text\"\nhook UseFoo(int initial = 0) -> int {\n  return initial;\n}";

            var a = GeneratorTestHelper.Run(withAtUsing, "UseFoo.uitkx");
            var b = GeneratorTestHelper.Run(withNsImport, "UseFoo.uitkx");

            Assert.True(a.SourceWasProduced);
            Assert.True(b.SourceWasProduced);
            Assert.Equal(a.GeneratedSource, b.GeneratedSource);
        }

        [Fact]
        public void NamespaceImport_StaticPayload_EmitsUsingStatic()
        {
            const string src =
                "@namespace TestNs\nimport \"@static System.Math\"\ncomponent MyComp {\n  return (<VisualElement />);\n}";

            var result = GeneratorTestHelper.Run(src);

            Assert.True(result.SourceWasProduced);
            Assert.Contains("using static System.Math;", result.GeneratedSource!);
        }

        [Fact]
        public void RouterNamespace_AutoInjected_NoImportNeeded()
        {
            // Feature 2: ReactiveUITK.Router is in the baseline, so RouterHooks resolves with no
            // @using/import at all — and the generated file carries the using.
            const string src =
                "@namespace TestNs\ncomponent Screen {\n  var nav = RouterHooks.UseNavigate();\n  return (<VisualElement />);\n}";

            var result = GeneratorTestHelper.Run(src);

            Assert.True(result.SourceWasProduced);
            Assert.Contains("using ReactiveUITK.Router;", result.GeneratedSource!);
            // No unknown-namespace / unresolved-hook diagnostics from the bare RouterHooks call.
            Assert.DoesNotContain(result.Diagnostics, d => d.Id == "UITKX2316" || d.Id == "UITKX2307");
        }

        [Fact]
        public void RouterImport_NowRedundant_Emits2317Hint_ViaTidyRule()
        {
            // Since Router is auto-injected, an explicit import "@ReactiveUITK.Router" is redundant —
            // AutoInjectedUsings.IsRedundant reports it (drives the editor 2317 hint + --tidy strip).
            Assert.True(ReactiveUITK.Language.AutoInjectedUsings.IsRedundant("ReactiveUITK.Router"));
        }

        [Fact]
        public void RouterComponentTags_ResolveProps_WithNoImport()
        {
            // Regression (Publish #105 floor-Unity failure): the resolver's search-namespace list was
            // usings-only, so when the samples dropped `import "@ReactiveUITK.Router"` (now baseline),
            // <Route path element> stopped resolving RouteFuncProps → UITKX0008 warnings + false
            // UITKX0109 errors, even though the EMITTED C# resolved fine via the baseline using.
            // BuildSearchNamespaces must include the auto-injected baseline. This mirrors the exact
            // failing shape: Router types in the real ReactiveUITK.Router namespace, a <RouteFunc>
            // tag with attributes, and ZERO usings/imports in the .uitkx.
            const string extraCSharp = """
                using ReactiveUITK.Core;
                using System.Collections.Generic;

                namespace ReactiveUITK.Router
                {
                    public static class RouteFunc
                    {
                        public static VirtualNode Render(IProps rawProps, IReadOnlyList<VirtualNode> children)
                            => null;
                    }

                    public sealed class RouteFuncProps : IProps
                    {
                        public string Path { get; set; }
                        public object Element { get; set; }
                    }
                }
                """;

            const string uitkx = """
                @namespace MyApp.UI

                component MyPage {
                    return (<RouteFunc path="/home" element={null} />);
                }
                """;

            var result = GeneratorTestHelper.RunWithExtraCSharp(uitkx, extraCSharp, "MyPage.uitkx");

            Assert.True(result.SourceWasProduced,
                $"No source produced. Diagnostics: {string.Join(", ", result.Diagnostics)}");
            // No false UITKX0109 (unknown attribute) — Path/Element resolve via the baseline using.
            Assert.DoesNotContain(result.Diagnostics, d => d.Id == "UITKX0109");
            Assert.DoesNotContain(result.Diagnostics, d => d.Id == "UITKX0008");
            Assert.True(result.SourceContains("V.Func<global::ReactiveUITK.Router.RouteFuncProps>"),
                $"Expected typed V.Func via the baseline namespace. Got:\n{result.GeneratedSource}");
            Assert.True(result.SourceContains("Path = \"/home\""),
                $"Expected Path prop forwarded. Got:\n{result.GeneratedSource}");
        }

        [Fact]
        public void RouterAliasTags_ResolveProps_WithNoImport()
        {
            // Same regression as above but through the ALIAS path the real samples use
            // (<Router>/<Routes>/<Route> → RouterTagAliases → *Func) — mirrors AppRoot.uitkx.
            const string extraCSharp = """
                using ReactiveUITK.Core;
                using System.Collections.Generic;

                namespace ReactiveUITK.Router
                {
                    public static class RouterFunc
                    {
                        public static VirtualNode Render(IProps rawProps, IReadOnlyList<VirtualNode> children) => null;
                    }
                    public sealed class RouterFuncProps : IProps
                    {
                        public string InitialPath { get; set; }
                    }
                    public static class RoutesFunc
                    {
                        public static VirtualNode Render(IProps rawProps, IReadOnlyList<VirtualNode> children) => null;
                    }
                    public static class RouteFunc
                    {
                        public static VirtualNode Render(IProps rawProps, IReadOnlyList<VirtualNode> children) => null;
                    }
                    public sealed class RouteFuncProps : IProps
                    {
                        public string Path { get; set; }
                        public object Element { get; set; }
                    }
                }
                """;

            const string uitkx = """
                @namespace MyApp.UI

                component AppShell {
                    return (
                        <Router initialPath="/home">
                            <Routes>
                                <Route path="/home" element={null} />
                            </Routes>
                        </Router>
                    );
                }
                """;

            var result = GeneratorTestHelper.RunWithExtraCSharp(uitkx, extraCSharp, "AppShell.uitkx");

            Assert.True(result.SourceWasProduced,
                $"No source produced. Diagnostics: {string.Join(", ", result.Diagnostics)}");
            Assert.DoesNotContain(result.Diagnostics, d => d.Id == "UITKX0109");
            Assert.DoesNotContain(result.Diagnostics, d => d.Id == "UITKX0008");
            Assert.True(result.SourceContains("Path = \"/home\""),
                $"Expected Path prop forwarded through the <Route> alias. Got:\n{result.GeneratedSource}");
        }

        [Fact]
        public void NamespaceImport_NoDiagnosticsForValidNamespace()
        {
            const string src =
                "@namespace TestNs\nimport \"@System.Text\"\ncomponent MyComp {\n  return (<VisualElement />);\n}";

            var result = GeneratorTestHelper.Run(src);

            Assert.DoesNotContain(result.Diagnostics, d => d.Id.StartsWith("UITKX23"));
        }
    }
}
