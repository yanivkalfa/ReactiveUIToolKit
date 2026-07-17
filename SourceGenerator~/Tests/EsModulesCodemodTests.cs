using System.Collections.Generic;
using System.Linq;
using ReactiveUITK.SourceGenerator.Tools;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests
{
    /// <summary>
    /// ES-modules campaign (Plans~/ES_MODULES_EXECUTION_PLAN.md M7, U-09/§7.1): the
    /// <c>--es-modules</c> codemod pass over in-memory file sets — each wrapper rewrite form,
    /// module→<c>* as</c> importer preservation, companion member-import insertion, companion-set
    /// atomicity, unmigratable-shape skip, and idempotence.
    /// </summary>
    public sealed class EsModulesCodemodTests
    {
        private const string Dir = "C:/proj/Assets/UI";

        private static MigratorFile F(string name, string text)
            => new($"{Dir}/{name}", "Game", text);

        private static Dictionary<string, string> Run(out List<UitkxMigrator.MigrationError> errors, params MigratorFile[] files)
            => EsModulesMigrator.Migrate(files, out errors);

        private static List<MigratorFile> Apply(IReadOnlyList<MigratorFile> input, Dictionary<string, string> changed)
            => input.Select(f => changed.TryGetValue(f.AbsPath, out var t) ? f with { Text = t } : f).ToList();

        // ── Wrapper rewrites ─────────────────────────────────────────────────

        [Fact]
        public void Component_RewritesToVirtualNode_ParameterlessGetsParens()
        {
            var file = F("Foo.uitkx", "export component Foo {\n    return (\n        <Box />\n    );\n}\n");
            var changed = Run(out var errors, file);

            Assert.Empty(errors);
            string outText = changed[file.AbsPath];
            Assert.Contains("export VirtualNode Foo() {", outText);
            Assert.DoesNotContain("component Foo", outText);
        }

        [Fact]
        public void Component_WithParams_KeepsParamList()
        {
            var file = F("Bar.uitkx",
                "export component Bar(string title = \"x\") {\n    return (\n        <Label text={title} />\n    );\n}\n");
            var changed = Run(out var errors, file);

            Assert.Empty(errors);
            Assert.Contains("export VirtualNode Bar(string title = \"x\") {", changed[file.AbsPath]);
        }

        [Fact]
        public void Hook_TupleReturn_MovesBeforeName()
        {
            var file = F("Counter.hooks.uitkx",
                "export hook useCounter(int initial = 0) -> (int count, Action increment) {\n" +
                "    var (count, setCount) = useState(initial);\n" +
                "    Action increment = () => setCount(c => c + 1);\n" +
                "    return (count, increment);\n" +
                "}\n");
            var changed = Run(out var errors, file);

            Assert.Empty(errors);
            string outText = changed[file.AbsPath];
            Assert.Contains("export (int count, Action increment) useCounter(int initial = 0) {", outText);
            Assert.DoesNotContain("hook useCounter", outText);
            Assert.Contains("var (count, setCount) = useState(initial);", outText);
        }

        [Fact]
        public void Hook_SingleTypeReturn_LosesNothing()
        {
            var file = F("Thing.hooks.uitkx",
                "export hook useThing() -> int {\n    return 42;\n}\n");
            var changed = Run(out var errors, file);

            Assert.Empty(errors);
            Assert.Contains("export int useThing() {", changed[file.AbsPath]);
        }

        [Fact]
        public void Module_ExplodesToPlainDeclarations()
        {
            var file = F("Tokens.uitkx",
                "export module Tokens {\n" +
                "    public static readonly int Gap = 8;\n" +
                "    public static string Fmt(int s) { return $\"{s}\"; }\n" +
                "    private static int hidden = 1;\n" +
                "}\n");
            var changed = Run(out var errors, file);

            Assert.Empty(errors);
            string outText = changed[file.AbsPath];
            Assert.Contains("export int Gap = 8;", outText);
            Assert.Contains("export string Fmt(int s) {", outText);
            Assert.Contains("int hidden = 1;", outText);
            Assert.DoesNotContain("module Tokens", outText);
        }

        // ── Importer rewrite (module → * as) ─────────────────────────────────

        [Fact]
        public void ModuleImporter_BecomesStarImport_CallSitesUntouched()
        {
            var tokens = F("Tokens.uitkx",
                "export module Tokens {\n    public static readonly int Gap = 8;\n}\n");
            var screen = F("Screen.uitkx",
                "import { Tokens } from \"./Tokens\"\ncomponent Screen {\n    var g = Tokens.Gap;\n    return (\n        <Box />\n    );\n}\n");
            var changed = Run(out var errors, tokens, screen);

            Assert.Empty(errors);
            string outText = changed[screen.AbsPath];
            Assert.Contains("import * as Tokens from \"./Tokens\"", outText);
            Assert.Contains("Tokens.Gap", outText);
            Assert.DoesNotContain("import { Tokens }", outText);
        }

        // ── Companion member imports ─────────────────────────────────────────

        [Fact]
        public void CompanionSet_MemberRefsGetImports()
        {
            // Legacy companion merge: Foo.style.uitkx's `module Foo` merged into component Foo,
            // so `container` resolved bare. Post-migration Foo.uitkx must import it.
            var style = F("Foo.style.uitkx",
                "export module Foo {\n    public static readonly int container = 1;\n}\n");
            var comp = F("Foo.uitkx",
                "component Foo {\n    var c = container;\n    return (\n        <Box />\n    );\n}\n");
            var changed = Run(out var errors, style, comp);

            Assert.Empty(errors);
            string outText = changed[comp.AbsPath];
            Assert.Contains("import { container } from \"./Foo.style\"", outText);
        }

        // ── Atomicity + unmigratable shapes ──────────────────────────────────

        [Fact]
        public void ModuleWithNestedType_WholeSetStaysLegacy_WithError()
        {
            var style = F("Baz.style.uitkx",
                "export module Baz {\n    public enum Kind { A, B }\n}\n");
            var comp = F("Baz.uitkx",
                "component Baz {\n    return (\n        <Box />\n    );\n}\n");
            var changed = Run(out var errors, style, comp);

            Assert.Contains(errors, e => e.Message.Contains("left legacy"));
            // Neither file switched grammar: the component keeps its wrapper keyword.
            string compText = changed.TryGetValue(comp.AbsPath, out var t) ? t : comp.Text;
            Assert.Contains("component Baz", compText);
            string styleText = changed.TryGetValue(style.AbsPath, out var s) ? s : style.Text;
            Assert.Contains("module Baz", styleText);
        }

        [Fact]
        public void GenericHook_FileStaysLegacy_WithError()
        {
            var file = F("Gen.hooks.uitkx",
                "export hook useGen<T>(T seed) -> T {\n    return seed;\n}\n");
            var changed = Run(out var errors, file);

            Assert.Contains(errors, e => e.Message.Contains("generic hook"));
            string outText = changed.TryGetValue(file.AbsPath, out var t) ? t : file.Text;
            Assert.Contains("hook useGen", outText);
        }

        // ── Idempotence (the §7.1 acceptance) ────────────────────────────────

        [Fact]
        public void Migration_IsIdempotent()
        {
            var input = new List<MigratorFile>
            {
                F("Tokens.uitkx", "export module Tokens {\n    public static readonly int Gap = 8;\n}\n"),
                F("Screen.uitkx",
                    "import { Tokens } from \"./Tokens\"\ncomponent Screen {\n    var g = Tokens.Gap;\n    return (\n        <Box />\n    );\n}\n"),
                F("Counter.hooks.uitkx", "export hook useCounter() -> int {\n    return 1;\n}\n"),
            };

            var first = Run(out var errs1, input.ToArray());
            Assert.Empty(errs1);
            var afterFirst = Apply(input, first);

            var second = EsModulesMigrator.Migrate(afterFirst, out var errs2);
            Assert.Empty(errs2);
            Assert.Empty(second);
        }

        [Fact]
        public void MigratedFile_NeverGetsNamespaceStamp()
        {
            // The imports-leg identity-freezing @namespace stamp is WRONG here: it would pin every
            // migrated file to its pre-migration namespace (with the raw parsed fallback, no
            // less) and defeat G-01's file-keyed derivation. Found live: the first samples run
            // stamped `@namespace ReactiveUITK.FunctionStyle` into all 165 files.
            var comp = F("Stamp.uitkx", "component Stamp {\n    return (\n        <Box />\n    );\n}\n");
            var changed = Run(out var errors, comp);

            Assert.Empty(errors);
            Assert.DoesNotContain("@namespace", changed[comp.AbsPath]);
        }

        [Fact]
        public void ExplicitNamespaceStamp_IsPreserved()
        {
            // An AUTHORED @namespace is the escape hatch (G-07/U-01) — it must survive migration.
            var comp = F("Stamped.uitkx",
                "@namespace My.Stamp\ncomponent Stamped {\n    return (\n        <Box />\n    );\n}\n");
            var changed = Run(out var errors, comp);

            Assert.Empty(errors);
            Assert.Contains("@namespace My.Stamp", changed[comp.AbsPath]);
        }

        [Fact]
        public void AlreadyMigratedFile_Untouched()
        {
            var file = F("Done.uitkx",
                "export VirtualNode Done() {\n  return (\n    <Box />\n  );\n}\n");
            var changed = Run(out var errors, file);

            Assert.Empty(errors);
            Assert.False(changed.ContainsKey(file.AbsPath));
        }
    }
}
