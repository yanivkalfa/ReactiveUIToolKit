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
        public void NamespaceImport_NoDiagnosticsForValidNamespace()
        {
            const string src =
                "@namespace TestNs\nimport \"@System.Text\"\ncomponent MyComp {\n  return (<VisualElement />);\n}";

            var result = GeneratorTestHelper.Run(src);

            Assert.DoesNotContain(result.Diagnostics, d => d.Id.StartsWith("UITKX23"));
        }
    }
}
