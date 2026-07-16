using System.Collections.Generic;
using System.IO;
using System.Linq;
using ReactiveUITK.Language;
using ReactiveUITK.Language.Parser;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests
{
    /// <summary>
    /// ImportScopeFacts — the single source of truth for "what using lines a file's imports imply",
    /// shared by the LSP virtual doc (typed) and the Unity HMR compiler (reflection). Exercised over
    /// a real temp project layout (asmdef anchor) so the EFFECTIVE-namespace path — the whole point
    /// (the old VDG used the raw parsed namespace, wrong for every stamp-less file) — is covered.
    /// </summary>
    public sealed class ImportScopeFactsTests : System.IDisposable
    {
        private readonly string _root;   // <tmp>/proj
        private readonly string _assets; // <tmp>/proj/Assets

        public ImportScopeFactsTests()
        {
            _root = Path.Combine(Path.GetTempPath(), "uitkx_scope_" + System.Guid.NewGuid().ToString("N"));
            _assets = Path.Combine(_root, "Assets");
            Directory.CreateDirectory(_assets);
            File.WriteAllText(Path.Combine(_assets, "Asm.asmdef"), "{ \"name\": \"Game\" }");
        }

        public void Dispose()
        {
            try { Directory.Delete(_root, recursive: true); } catch { }
        }

        private string F(string rel, string text)
        {
            string p = Path.Combine(_assets, rel.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(p)!);
            File.WriteAllText(p, text);
            return p;
        }

        private static IReadOnlyList<string> Payloads(string importerPath)
        {
            var ds = DirectiveParser.Parse(File.ReadAllText(importerPath), importerPath, new List<ParseDiagnostic>());
            return ImportScopeFacts.ComputeInjectedUsingPayloads(ds, importerPath);
        }

        [Fact]
        public void CrossFolderModuleImport_AliasUsesDerivedEffectiveNamespace()
        {
            // NO @namespace anywhere — the alias must use the PATH-DERIVED namespace
            // (ReactiveUITK.Uitkx.shared), not the raw parsed default.
            F("shared/Tokens.types.uitkx", "export module Tokens {\n  public const int Gap = 8;\n}\n");
            string screen = F("screens/Home.uitkx",
                "import { Tokens } from \"../shared/Tokens.types\"\ncomponent Home {\n  return (<Box />);\n}\n");

            var payloads = Payloads(screen);

            Assert.Contains("Tokens = ReactiveUITK.Uitkx.shared.Tokens", payloads);
        }

        [Fact]
        public void CrossFolderComponentImport_GetsAlias()
        {
            F("widgets/Card.uitkx", "export component Card(string title = \"\") {\n  return (<Box />);\n}\n");
            string screen = F("screens/Home.uitkx",
                "import { Card } from \"../widgets/Card\"\ncomponent Home {\n  return (<Card title=\"x\" />);\n}\n");

            var payloads = Payloads(screen);

            Assert.Contains("Card = ReactiveUITK.Uitkx.widgets.Card", payloads);
        }

        [Fact]
        public void SameFolderImport_SameNamespace_NoAlias()
        {
            // Same folder → same derived namespace → alias would be CS0576; must be skipped.
            F("screens/Home.style.uitkx", "export module HomeStyles {\n  public const int Gap = 8;\n}\n");
            string screen = F("screens/Home.uitkx",
                "import { HomeStyles } from \"./Home.style\"\ncomponent Home {\n  return (<Box />);\n}\n");

            Assert.Empty(Payloads(screen));
        }

        [Fact]
        public void CrossFolderHookImport_GetsUsingStaticContainer()
        {
            F("shared/Counter.hooks.uitkx", "export hook useCounter() {\n  return 0;\n}\n");
            string screen = F("screens/Home.uitkx",
                "import { useCounter } from \"../shared/Counter.hooks\"\ncomponent Home {\n  return (<Box />);\n}\n");

            var payloads = Payloads(screen);

            Assert.Contains("static ReactiveUITK.Uitkx.shared.CounterHooks", payloads);
        }

        [Fact]
        public void ExplicitNamespaceOnTarget_WinsOverDerivation()
        {
            F("shared/Tokens.types.uitkx",
                "@namespace My.Stamped.Ns\nexport module Tokens {\n  public const int Gap = 8;\n}\n");
            string screen = F("screens/Home.uitkx",
                "import { Tokens } from \"../shared/Tokens.types\"\ncomponent Home {\n  return (<Box />);\n}\n");

            Assert.Contains("Tokens = My.Stamped.Ns.Tokens", Payloads(screen));
        }

        [Fact]
        public void ReservedAliasName_Skipped()
        {
            // A module deliberately named after a built-in style alias must not inject (CS1537).
            F("shared/Color.types.uitkx", "export module Color {\n  public const int X = 1;\n}\n");
            string screen = F("screens/Home.uitkx",
                "import { Color } from \"../shared/Color.types\"\ncomponent Home {\n  return (<Box />);\n}\n");

            Assert.DoesNotContain(Payloads(screen), p => p.StartsWith("Color ="));
        }

        [Fact]
        public void PipelineReservedAliases_AreTheSameSet()
        {
            // Single-source guard: the SG-side view must BE the language-lib set, not a copy.
            Assert.Same(ImportScopeFacts.ReservedTypeAliases, UitkxPipeline.ReservedTypeAliases);
        }
    }

    /// <summary>
    /// Contract: the HMR compiler consumes ImportScopeFacts via reflection (it cannot reference
    /// language-lib). Pins the type/method names on BOTH sides so a rename can't silently sever the
    /// hot-reload injection — the same text-level idiom as the other Hmr*ContractTests.
    /// </summary>
    public sealed class HmrImportScopeContractTests
    {
        [Fact]
        public void HmrCompiler_WiresImportScopeFacts_ByExactNames()
        {
            string hmr = File.ReadAllText(HmrCompilerPath());
            Assert.Contains("ReactiveUITK.Language.ImportScopeFacts", hmr);
            Assert.Contains("ComputeInjectedUsingPayloads", hmr);
        }

        [Fact]
        public void HmrGetItems_GuardsDefaultImmutableArray()
        {
            // RUNTIME-V regression: a DEFAULT ImmutableArray (e.g. DirectiveSet.HookDeclarations on
            // every component-only file) throws from GetEnumerator(); HMR's reflective GetItems must
            // check IsDefault before enumerating or the first hot-reload of a plain component
            // crashes in BuildHookFamilyKeyMap. Pins the guard textually (Editor code cannot be
            // referenced from this test project).
            string hmr = File.ReadAllText(HmrCompilerPath());
            int getItems = hmr.IndexOf("internal static IList GetItems", System.StringComparison.Ordinal);
            Assert.True(getItems >= 0, "GetItems not found in UitkxHmrCompiler");
            string body = hmr.Substring(getItems, Math.Min(1200, hmr.Length - getItems));
            Assert.Contains("IsDefault", body);

            // And the reflected-to method really exists with the expected shape.
            var mi = typeof(ImportScopeFacts).GetMethod("ComputeInjectedUsingPayloads");
            Assert.NotNull(mi);
            var ps = mi!.GetParameters();
            Assert.Equal(2, ps.Length);
            Assert.Equal(typeof(ReactiveUITK.Language.Parser.DirectiveSet), ps[0].ParameterType);
            Assert.Equal(typeof(string), ps[1].ParameterType);
        }

        private static string HmrCompilerPath()
        {
            string dir = Path.GetDirectoryName(
                new System.Uri(typeof(HmrImportScopeContractTests).Assembly.Location).LocalPath)!;
            for (int i = 0; i < 8 && dir != null; i++)
            {
                string candidate = Path.Combine(dir, "Editor", "HMR", "UitkxHmrCompiler.cs");
                if (File.Exists(candidate)) return candidate;
                dir = Path.GetDirectoryName(dir)!;
            }
            throw new FileNotFoundException("Could not locate Editor/HMR/UitkxHmrCompiler.cs");
        }
    }
}
