using System.Collections.Generic;
using System.Linq;
using ReactiveUITK.Language;
using ReactiveUITK.Language.Parser;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests
{
    /// <summary>
    /// Plan §5 — the parser populates the import/export model (DirectiveSet.Imports,
    /// ComponentDeclarations[].IsExported, HookDeclarations/ModuleDeclarations[].IsExported) and
    /// emits the frozen scan diagnostics 2303/2309. Additive: single-decl behavior is unchanged.
    /// </summary>
    public sealed class ImportExportParsingTests
    {
        private static (DirectiveSet ds, List<ParseDiagnostic> diags) Parse(string src)
        {
            var diags = new List<ParseDiagnostic>();
            var ds = DirectiveParser.Parse(src, "C:/proj/Assets/UI/Foo.uitkx", diags);
            return (ds, diags);
        }

        [Fact]
        public void ImportLine_PopulatesImports()
        {
            var (ds, _) = Parse("import { A, B } from \"./X\"\ncomponent Foo {\n  return ( <Spacer /> );\n}\n");
            Assert.Single(ds.Imports);
            Assert.Equal(new[] { "A", "B" }, ds.Imports[0].Names.ToArray());
            Assert.Equal("./X", ds.Imports[0].Specifier);
        }

        [Fact]
        public void ImportLine_CapturesSpecifierColumn()
        {
            // import { A, B } from "./X"
            // 0123456789...        ^ col 21 = opening quote
            var (ds, _) = Parse("import { A, B } from \"./X\"\ncomponent Foo {\n  return ( <Spacer /> );\n}\n");
            var imp = ds.Imports[0];
            Assert.Equal(21, imp.SpecifierColumn);
            Assert.Equal(new[] { 9, 12 }, imp.NameColumns.ToArray());
            // The specifier span (quotes included) closes exactly at the line's last char.
            Assert.Equal("import { A, B } from \"./X\"".Length, imp.SpecifierColumn + imp.Specifier.Length + 2);
        }

        [Fact]
        public void MultipleImports_MixedSpecifierForms()
        {
            var (ds, _) = Parse(
                "import { A } from \"./A\"\nimport { B } from \"../B\"\nimport { C } from \"~/C\"\n" +
                "component Foo {\n  return ( <Spacer /> );\n}\n");
            Assert.Equal(3, ds.Imports.Length);
            Assert.Equal("./A", ds.Imports[0].Specifier);
            Assert.Equal("../B", ds.Imports[1].Specifier);
            Assert.Equal("~/C", ds.Imports[2].Specifier);
        }

        [Fact]
        public void ExportComponent_SetsIsExported()
        {
            var (ds, _) = Parse("export component Foo {\n  return ( <Spacer /> );\n}\n");
            Assert.Equal("Foo", ds.ComponentName);
            Assert.Single(ds.ComponentDeclarations);
            Assert.True(ds.ComponentDeclarations[0].IsExported);
            Assert.Equal("Foo", ds.ComponentDeclarations[0].Name);
        }

        [Fact]
        public void PrivateComponent_IsNotExported()
        {
            var (ds, _) = Parse("component Foo {\n  return ( <Spacer /> );\n}\n");
            Assert.Single(ds.ComponentDeclarations);
            Assert.False(ds.ComponentDeclarations[0].IsExported);
        }

        [Fact]
        public void ExportHook_SetsIsExported()
        {
            var (ds, _) = Parse("export hook useThing() { }\n");
            Assert.Single(ds.HookDeclarations);
            Assert.True(ds.HookDeclarations[0].IsExported);
        }

        [Fact]
        public void ExportModule_SetsIsExported()
        {
            var (ds, _) = Parse("export module Styles { }\n");
            Assert.Single(ds.ModuleDeclarations);
            Assert.True(ds.ModuleDeclarations[0].IsExported);
        }

        [Fact]
        public void DuplicateImport_Emits2303()
        {
            var (_, diags) = Parse(
                "import { A } from \"./X\"\nimport { A } from \"./Y\"\n" +
                "component Foo {\n  return ( <Spacer /> );\n}\n");
            Assert.Contains(diags, d => d.Code == "UITKX2303");
        }

        [Fact]
        public void ImportAfterDeclaration_Emits2309()
        {
            var (_, diags) = Parse(
                "component Foo {\n  return ( <Spacer /> );\n}\nimport { A } from \"./X\"\n");
            Assert.Contains(diags, d => d.Code == "UITKX2309");
        }
    }
}
