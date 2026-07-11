using ReactiveUITK.Language.Formatter;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests
{
    /// <summary>
    /// Plan §10 / A7f — the formatter re-prints the preamble from the model and DROPS unmodeled
    /// lines. These guard that import lines AND export prefixes survive a format pass (else
    /// formatting a codemodded file would silently delete them and break strict mode).
    /// </summary>
    public sealed class FormatterImportTests
    {
        private static string Fmt(string src) =>
            new AstFormatter(FormatterOptions.Default).Format(src, "Test.uitkx").Replace("\r\n", "\n");

        [Fact]
        public void ImportLines_SurviveFormat()
        {
            string outp = Fmt(
                "import { A, B } from \"./X\"\n\ncomponent Foo {\n  return ( <Spacer /> );\n}\n");
            Assert.Contains("import { A, B } from \"./X\"", outp);
        }

        [Fact]
        public void MultipleImports_AllSurvive_InOrder()
        {
            string outp = Fmt(
                "import { A } from \"./A\"\nimport { B } from \"~/B\"\n\n" +
                "component Foo {\n  return ( <Spacer /> );\n}\n");
            int a = outp.IndexOf("import { A } from \"./A\"", System.StringComparison.Ordinal);
            int b = outp.IndexOf("import { B } from \"~/B\"", System.StringComparison.Ordinal);
            Assert.True(a >= 0 && b > a, "both imports must survive, in source order");
        }

        [Fact]
        public void ExportComponent_PrefixSurvives()
        {
            Assert.Contains("export component Foo", Fmt("export component Foo {\n  return ( <Spacer /> );\n}\n"));
        }

        [Fact]
        public void ExportHook_PrefixSurvives()
        {
            Assert.Contains("export hook useThing", Fmt("export hook useThing() { }\n"));
        }

        [Fact]
        public void ExportModule_PrefixSurvives()
        {
            Assert.Contains("export module Styles", Fmt("export module Styles { }\n"));
        }

        [Fact]
        public void FormatIsIdempotent_WithImportsAndExport()
        {
            string src = "import { A } from \"./X\"\n\nexport component Foo {\n  return ( <Spacer /> );\n}\n";
            string once = Fmt(src);
            string twice = Fmt(once);
            Assert.Equal(once, twice);
        }
    }
}
