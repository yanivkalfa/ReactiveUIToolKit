using System.Collections.Generic;
using System.Linq;
using ReactiveUITK.SourceGenerator.Tools;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests
{
    /// <summary>
    /// Fixture tests for the leg-3 migration codemod (<see cref="UitkxMigrator"/>, plan §11 / §15).
    /// Exercises the pure core over in-memory file sets: export-everything, fresh reference-scan
    /// import insertion, @namespace stamping, idempotence (run twice = no diff), the ambiguity path,
    /// and relative-specifier computation.
    /// </summary>
    public sealed class CodemodTests
    {
        private const string Dir = "C:/proj/Assets/UI";

        private static MigratorFile F(string name, string text)
            => new($"{Dir}/{name}", "Game", text);

        private static Dictionary<string, string> Run(params MigratorFile[] files)
            => UitkxMigrator.Migrate(files, out _);

        // Apply a migration result back onto the input set → the file list after one pass.
        private static List<MigratorFile> Apply(IReadOnlyList<MigratorFile> input, Dictionary<string, string> changed)
            => input.Select(f => changed.TryGetValue(f.AbsPath, out var t) ? f with { Text = t } : f).ToList();

        [Fact]
        public void ComponentReference_InsertsImport_ExportsDecl_StampsNamespace()
        {
            var screen = F("Screen.uitkx",
                "component Screen {\n    return (\n        <StatusChip />\n    );\n}\n");
            var chip = F("StatusChip.uitkx",
                "component StatusChip {\n    return (\n        <Label text=\"x\" />\n    );\n}\n");

            var changed = Run(screen, chip);

            Assert.True(changed.ContainsKey(screen.AbsPath));
            string outText = changed[screen.AbsPath];
            Assert.Contains("import { StatusChip } from \"./StatusChip\"", outText);
            Assert.Contains("export component Screen", outText);
            Assert.Contains("@namespace ReactiveUITK.FunctionStyle", outText);
            // The peer that is only referenced (not referencing) still gets exported.
            Assert.Contains("export component StatusChip", changed[chip.AbsPath]);
        }

        [Fact]
        public void HookReference_InsertsImport_FromHooksFile()
        {
            var screen = F("Screen.uitkx",
                "component Screen {\n    var c = useCounter();\n    return (\n        <Box />\n    );\n}\n");
            var hooks = F("Counter.hooks.uitkx",
                "hook useCounter() {\n    return 0;\n}\n");

            var changed = Run(screen, hooks);

            Assert.Contains("import { useCounter } from \"./Counter.hooks\"", changed[screen.AbsPath]);
            Assert.Contains("export hook useCounter", changed[hooks.AbsPath]);
        }

        [Fact]
        public void Idempotent_SecondRun_NoChange()
        {
            var input = new List<MigratorFile>
            {
                F("Screen.uitkx", "component Screen {\n    var c = useCounter();\n    return (\n        <StatusChip />\n    );\n}\n"),
                F("StatusChip.uitkx", "component StatusChip {\n    return (\n        <Label text=\"x\" />\n    );\n}\n"),
                F("Counter.hooks.uitkx", "hook useCounter() {\n    return 0;\n}\n"),
            };

            var first = UitkxMigrator.Migrate(input, out _);
            Assert.NotEmpty(first);

            var afterFirst = Apply(input, first);
            var second = UitkxMigrator.Migrate(afterFirst, out _);

            Assert.Empty(second); // re-running the codemod on migrated sources is a no-op
        }

        [Fact]
        public void SelfReference_NotImported()
        {
            // A recursive component referencing itself must not import itself.
            var tree = F("Tree.uitkx",
                "component Tree {\n    return (\n        <Tree />\n    );\n}\n");

            var changed = Run(tree);

            string outText = changed[tree.AbsPath];
            Assert.DoesNotContain("import {", outText);
            Assert.Contains("export component Tree", outText);
        }

        [Fact]
        public void AlreadyExported_NotDoubled()
        {
            var chip = F("StatusChip.uitkx",
                "@namespace My.Ns\nexport component StatusChip {\n    return (\n        <Label text=\"x\" />\n    );\n}\n");

            var changed = Run(chip);

            // Already exported + explicit namespace + no refs → nothing to do.
            Assert.False(changed.ContainsKey(chip.AbsPath));
        }

        [Fact]
        public void AmbiguousReference_SurfacesError_NoImport()
        {
            // Two files in the same asmdef both declare component Widget.
            var a = F("A/Widget.uitkx", "component Widget {\n    return (\n        <Box />\n    );\n}\n");
            var b = F("B/Widget.uitkx", "component Widget {\n    return (\n        <Box />\n    );\n}\n");
            var user = F("User.uitkx", "component User {\n    return (\n        <Widget />\n    );\n}\n");

            var changed = UitkxMigrator.Migrate(new[] { a, b, user }, out var errors);

            Assert.Contains(errors, e => e.FilePath == user.AbsPath && e.Message.Contains("ambiguous"));
            Assert.DoesNotContain("import {", changed.TryGetValue(user.AbsPath, out var t) ? t : "");
        }

        [Theory]
        [InlineData("C:/p/Assets/UI/Screen.uitkx", "C:/p/Assets/UI/StatusChip.uitkx", "./StatusChip")]
        [InlineData("C:/p/Assets/UI/Screens/Home.uitkx", "C:/p/Assets/UI/Shared/Chip.uitkx", "../Shared/Chip")]
        [InlineData("C:/p/Assets/UI/Screen.uitkx", "C:/p/Assets/UI/Sub/Deep/T.uitkx", "./Sub/Deep/T")]
        public void RelativeSpecifier_IsRelativeAndExtensionless(string from, string to, string expected)
        {
            Assert.Equal(expected, UitkxMigrator.RelativeSpecifier(from, to));
        }
    }
}
