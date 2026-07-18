using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using ReactiveUITK.Language;
using ReactiveUITK.Language.Formatter;
using ReactiveUITK.Language.Parser;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests
{
    /// <summary>
    /// ES-modules audit wave (Plans~/ES_MODULES_AUDIT_FINDINGS.md): regression pins for the
    /// execution-verified parser/detector/formatter defects (F1-F11, P1/P2), the shared-parser
    /// dotted-closing-tag fix (H2), the payload/bridge/tag-map additions (F7b, H1/H3/H4), and
    /// the 2108 mixed-mode mirror direction (PC-6).
    /// </summary>
    public sealed class EsModulesAuditWaveTests
    {
        private static (DirectiveSet ds, List<ParseDiagnostic> diags) Parse(
            string source, string path = "C:/p/Assets/UI/File.uitkx")
        {
            var diags = new List<ParseDiagnostic>();
            var ds = DirectiveParser.Parse(source, path, diags);
            return (ds, diags);
        }

        private static readonly AstFormatter s_fmt = new AstFormatter(FormatterOptions.Default);
        private static string N(string s) => s.Replace("\r\n", "\n").Replace("\r", "\n");

        // ── F1: line counter resyncs after a component body ─────────────────

        [Fact]
        public void F1_DeclarationAfterComponentBody_HasCorrectLine()
        {
            var (ds, _) = Parse(
                "export VirtualNode Foo() {\n" +      // 1
                "    return (<Label/>);\n" +           // 2
                "}\n" +                                // 3
                "export int useCount() { return 1; }\n"); // 4
            Assert.Single(ds.MemberDeclarations);
            Assert.Equal(4, ds.MemberDeclarations[0].DeclarationLine);
        }

        [Fact]
        public void F1_SecondComponentAfterFirst_HasCorrectLine()
        {
            var (ds, _) = Parse(
                "export VirtualNode A() {\n" +        // 1
                "    return (<Box/>);\n" +             // 2
                "}\n" +                                // 3
                "\n" +                                 // 4
                "export VirtualNode B() {\n" +         // 5
                "    return (<Box/>);\n" +
                "}\n");
            Assert.Equal(2, ds.ComponentDeclarations.Length);
            Assert.Equal(5, ds.ComponentDeclarations[1].DeclarationLine);
        }

        // ── F9: generic declarations get a targeted diagnostic ───────────────

        [Fact]
        public void F9_GenericDeclaration_GetsTargetedError_NotWholeFileFallback()
        {
            var (_, diags) = Parse("export T Identity<T>(T v) { return v; }\n");
            Assert.Contains(diags, d =>
                d.Code == "UITKX2105" && d.Message.Contains("generic declarations are not supported"));
            Assert.DoesNotContain(diags, d =>
                d.Message.Contains("does not contain a valid function-style component"));
        }

        // ── F10: multi-line heads keep the line counter in sync ──────────────

        [Fact]
        public void F10_MultiLineTupleHead_NextDeclarationLineIsCorrect()
        {
            var (ds, _) = Parse(
                "export (int value,\n" +               // 1
                "  Action reset) useCountdown(int s) {\n" + // 2
                "  return (s, () => {});\n" +          // 3
                "}\n" +                                // 4
                "export int Gap = 8;\n");              // 5
            Assert.Equal(2, ds.MemberDeclarations.Length);
            Assert.Equal(5, ds.MemberDeclarations[1].DeclarationLine);
        }

        // ── P1: malformed hook header must not crash the parser ─────────────

        [Fact]
        public void P1_MalformedHookHeader_ReportsInsteadOfThrowing()
        {
            var diags = new List<ParseDiagnostic>();
            var ex = Record.Exception(() =>
                DirectiveParser.Parse("hook 123 {}\n", "C:/p/Assets/UI/Bad.uitkx", diags));
            Assert.Null(ex);
            Assert.Contains(diags, d => d.Severity == ParseSeverity.Error);
        }

        // ── PC-6: 2108 fires in BOTH mixed-mode directions ──────────────────

        [Fact]
        public void PC6_PlainDeclarationAfterLegacyComponent_Emits2108()
        {
            var (_, diags) = Parse(
                "component Foo {\n" +
                "  return (<Box/>);\n" +
                "}\n" +
                "export int Gap = 8;\n");
            Assert.Contains(diags, d => d.Code == "UITKX2108");
        }

        [Fact]
        public void PC6_PlainDeclarationAfterLegacyHook_Emits2108()
        {
            var (_, diags) = Parse(
                "export hook useThing() -> (int) {\n" +
                "  return (1);\n" +
                "}\n" +
                "export int Gap = 8;\n");
            Assert.Contains(diags, d => d.Code == "UITKX2108");
        }

        [Fact]
        public void PC6_BareStatementAfterLegacyComponent_Stays2105()
        {
            var (_, diags) = Parse(
                "component Foo {\n" +
                "  return (<Box/>);\n" +
                "}\n" +
                "DoThing();\n");
            Assert.DoesNotContain(diags, d => d.Code == "UITKX2108");
            Assert.Contains(diags, d => d.Code == "UITKX2105");
        }

        // ── H2: dotted tags with children parse (shared markup parser) ──────

        [Fact]
        public void H2_DottedTagWithChildren_Parses()
        {
            var diags = new List<ParseDiagnostic>();
            var nodes = UitkxParser.ParseFragment(
                "<X.Comp><Label text=\"hi\" /></X.Comp>", "t.uitkx", 1, diags);
            Assert.DoesNotContain(diags, d => d.Severity == ParseSeverity.Error);
            Assert.Single(nodes);
        }

        [Fact]
        public void H2_MismatchedDottedClosingTag_StillErrors()
        {
            var diags = new List<ParseDiagnostic>();
            UitkxParser.ParseFragment("<X.Comp></X.Wrong>", "t.uitkx", 1, diags);
            Assert.Contains(diags, d => d.Severity == ParseSeverity.Error);
        }

        // ── F4/F5/F6: StrictImportDetector reference-scan fixes ─────────────

        private static DirectiveSet DetectorDs() => new DirectiveSet(
            Namespace: "N", ComponentName: "Screen", PropsTypeName: null, DefaultKey: null,
            Usings: ImmutableArray<string>.Empty, UssFiles: ImmutableArray<string>.Empty,
            Injects: ImmutableArray<(string, string)>.Empty, MarkupStartLine: 1, MarkupStartIndex: 0);

        [Fact]
        public void F4_OwnNewModeHook_IsNotFlagged()
        {
            var ds = DetectorDs() with
            {
                MemberDeclarations = ImmutableArray.Create(new MemberDeclaration(
                    "useCounter", DeclKind.Hook, true, "(int, Action)", "int seed", "return (seed, () => {});",
                    false, 2, 0, 2, 0, 0)),
            };
            var peers = new List<StrictImportDetector.PeerExport>
            {
                new("useCounter", "C:/p/Assets/UI/Other.uitkx", StrictImportDetector.ExportKind.Hook),
            };
            var findings = StrictImportDetector.Detect(
                ds, "C:/p/Assets/UI/File.uitkx", "var (c, r) = useCounter(0);", peers, _ => false);
            Assert.Empty(findings);
        }

        [Fact]
        public void F5_DefaultImportedComponentTag_IsNotFlagged()
        {
            var ds = DetectorDs() with
            {
                Imports = ImmutableArray.Create(new ImportDeclaration(
                    ImmutableArray<string>.Empty, "./ButtonX", 1, 0, ImmutableArray<int>.Empty,
                    IsDefault: true, DefaultAlias: "ButtonX")),
            };
            var peers = new List<StrictImportDetector.PeerExport>
            {
                new("ButtonX", "C:/p/Assets/UI/ButtonX.uitkx", StrictImportDetector.ExportKind.Component),
            };
            var findings = StrictImportDetector.Detect(
                ds, "C:/p/Assets/UI/File.uitkx", "<ButtonX />", peers, _ => false);
            Assert.DoesNotContain(findings, f => f.Code == "UITKX2305");
        }

        [Fact]
        public void F6_RenamedImportThatIsUsed_IsNotUnused()
        {
            var ds = DetectorDs() with
            {
                Imports = ImmutableArray.Create(new ImportDeclaration(
                    ImmutableArray.Create("Widget"), "./Widget", 1, 0, ImmutableArray<int>.Empty,
                    Aliases: ImmutableArray.Create<string?>("W"))),
            };
            var findings = StrictImportDetector.DetectUnusedImports(ds, "<W />");
            Assert.Empty(findings);
        }

        [Fact]
        public void F6_RenamedImport_OriginalNameNoLongerSatisfies()
        {
            var ds = DetectorDs() with
            {
                Imports = ImmutableArray.Create(new ImportDeclaration(
                    ImmutableArray.Create("Widget"), "./Widget", 1, 0, ImmutableArray<int>.Empty,
                    Aliases: ImmutableArray.Create<string?>("W"))),
            };
            var peers = new List<StrictImportDetector.PeerExport>
            {
                new("Widget", "C:/p/Assets/UI/Widget.uitkx", StrictImportDetector.ExportKind.Component),
            };
            var findings = StrictImportDetector.Detect(
                ds, "C:/p/Assets/UI/File.uitkx", "<Widget />", peers, _ => false);
            Assert.Contains(findings, f => f.Code == "UITKX2305");
        }

        // ── F7/F11/PC-2: ValidateImports rename gate + double-report + wording ──

        private static List<StrictImportDetector.Finding> Validate(
            DirectiveSet ds, Func<string, DirectiveSet?> parseTargetFile)
            => StrictImportDetector.ValidateImports(
                ds, "C:/p/Assets/UI", "C:/p/Assets", "Game",
                _ => true, _ => "Game", (_, _) => true, parseTargetFile);

        [Fact]
        public void F7_RenameImport_AgainstLegacyTarget_Emits2109()
        {
            var ds = DetectorDs() with
            {
                Imports = ImmutableArray.Create(new ImportDeclaration(
                    ImmutableArray.Create("useThing"), "./LegacyHooks", 1, 0, ImmutableArray<int>.Empty,
                    Aliases: ImmutableArray.Create<string?>("useOther"))),
            };
            var target = DetectorDs() with { UsesLegacySyntax = true };
            var findings = Validate(ds, _ => target);
            Assert.Contains(findings, f => f.Code == "UITKX2109");
        }

        [Fact]
        public void F11_DefaultImport_AgainstLegacyTarget_Only2109_No2326()
        {
            var ds = DetectorDs() with
            {
                Imports = ImmutableArray.Create(new ImportDeclaration(
                    ImmutableArray<string>.Empty, "./Legacy", 1, 0, ImmutableArray<int>.Empty,
                    IsDefault: true, DefaultAlias: "Thing")),
            };
            var target = DetectorDs() with { UsesLegacySyntax = true };
            var findings = Validate(ds, _ => target);
            Assert.Contains(findings, f => f.Code == "UITKX2109");
            Assert.DoesNotContain(findings, f => f.Code == "UITKX2326");
        }

        [Fact]
        public void PC2_FamilyMessages_UseSingleQuotes()
        {
            var ds = DetectorDs() with
            {
                Imports = ImmutableArray.Create(new ImportDeclaration(
                    ImmutableArray<string>.Empty, "./Shapes", 1, 0, ImmutableArray<int>.Empty,
                    IsStar: true, StarAlias: "Shapes")),
            };
            var target = DetectorDs() with { UsesLegacySyntax = true };
            var findings = Validate(ds, _ => target);
            var f2109 = findings.Find(f => f.Code == "UITKX2109");
            Assert.NotNull(f2109);
            Assert.Contains("'Shapes.uitkx'", f2109!.Message);
            Assert.DoesNotContain("`", f2109.Message);
        }

        // ── F7b/star-gap/bridges/tag-maps: ImportScopeFacts against real files ──

        private sealed class TempUitkxDir : IDisposable
        {
            public string Dir { get; }
            public TempUitkxDir()
            {
                Dir = Path.Combine(Path.GetTempPath(), "uitkx-audit-" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(Dir);
            }
            public string Write(string name, string content)
            {
                string p = Path.Combine(Dir, name).Replace('\\', '/');
                File.WriteAllText(p, content);
                return p;
            }
            public void Dispose()
            {
                try { Directory.Delete(Dir, recursive: true); } catch { }
            }
        }

        [Fact]
        public void F7b_RenamedImportFromLegacyTarget_GetsNoPayload()
        {
            using var tmp = new TempUitkxDir();
            tmp.Write("LegacyHooks.uitkx",
                "export hook useThing() -> (int) {\n  return (1);\n}\n");
            string importer = tmp.Write("Screen.uitkx",
                "import { useThing as useOther } from \"./LegacyHooks\"\n" +
                "export VirtualNode Screen() {\n  return (<Box/>);\n}\n");

            var (ds, _) = Parse(File.ReadAllText(importer), importer);
            var payloads = ImportScopeFacts.ComputeInjectedUsingPayloads(ds, importer);
            Assert.DoesNotContain(payloads, p => p.Contains("LegacyHooks"));
        }

        [Fact]
        public void F7b_UnaliasedImportFromLegacyTarget_KeepsPayload()
        {
            using var tmp = new TempUitkxDir();
            tmp.Write("LegacyHooks.uitkx",
                "export hook useThing() -> (int) {\n  return (1);\n}\n");
            string importer = tmp.Write("Screen.uitkx",
                "import { useThing } from \"./LegacyHooks\"\n" +
                "export VirtualNode Screen() {\n  return (<Box/>);\n}\n");

            var (ds, _) = Parse(File.ReadAllText(importer), importer);
            var payloads = ImportScopeFacts.ComputeInjectedUsingPayloads(ds, importer);
            Assert.Contains(payloads, p => p.StartsWith("static ") && p.Contains("LegacyHooksHooks"));
        }

        [Fact]
        public void StarImport_OfComponentOnlyTarget_GetsNoExportsAlias()
        {
            using var tmp = new TempUitkxDir();
            tmp.Write("Card.uitkx",
                "export VirtualNode Card() {\n  return (<Box/>);\n}\n");
            string importer = tmp.Write("Screen.uitkx",
                "import * as X from \"./Card\"\n" +
                "export VirtualNode Screen() {\n  return (<X.Card/>);\n}\n");

            var (ds, _) = Parse(File.ReadAllText(importer), importer);
            var payloads = ImportScopeFacts.ComputeInjectedUsingPayloads(ds, importer);
            Assert.DoesNotContain(payloads, p => p.Contains("__Exports"));
        }

        [Fact]
        public void StarImport_OfMemberTarget_KeepsExportsAlias()
        {
            using var tmp = new TempUitkxDir();
            tmp.Write("tokens.uitkx", "export int Gap = 4;\n");
            string importer = tmp.Write("Screen.uitkx",
                "import * as T from \"./tokens\"\n" +
                "export VirtualNode Screen() {\n  return (<Box/>);\n}\n");

            var (ds, _) = Parse(File.ReadAllText(importer), importer);
            var payloads = ImportScopeFacts.ComputeInjectedUsingPayloads(ds, importer);
            Assert.Contains(payloads, p => p.StartsWith("T = ") && p.EndsWith(".__Exports"));
        }

        [Fact]
        public void BridgeLines_RenamedValueImport_RendersSgShapedForwarder()
        {
            using var tmp = new TempUitkxDir();
            tmp.Write("tokens.uitkx", "export int Gap = 4;\n");
            string importer = tmp.Write("Screen.uitkx",
                "import { Gap as G } from \"./tokens\"\n" +
                "export VirtualNode Screen() {\n  return (<Box/>);\n}\n");

            var (ds, _) = Parse(File.ReadAllText(importer), importer);
            var lines = ImportScopeFacts.ComputeImportedMemberBridgeLines(ds, importer);
            Assert.Single(lines);
            Assert.StartsWith("        internal static int G => global::", lines[0]);
            Assert.EndsWith(".__Exports.Gap;", lines[0]);
        }

        [Fact]
        public void BridgeLines_RenamedUtilImport_ForwardsArgs()
        {
            using var tmp = new TempUitkxDir();
            tmp.Write("utils.uitkx", "export string FormatScore(int score) {\n  return $\"{score}\";\n}\n");
            string importer = tmp.Write("Screen.uitkx",
                "import { FormatScore as Fmt } from \"./utils\"\n" +
                "export VirtualNode Screen() {\n  return (<Box/>);\n}\n");

            var (ds, _) = Parse(File.ReadAllText(importer), importer);
            var lines = ImportScopeFacts.ComputeImportedMemberBridgeLines(ds, importer);
            Assert.Single(lines);
            Assert.Contains("internal static string Fmt(int score) => global::", lines[0]);
            Assert.EndsWith(".__Exports.FormatScore(score);", lines[0]);
        }

        [Fact]
        public void TagMaps_StarAndAlias_ResolveAgainstNewModeTargets()
        {
            using var tmp = new TempUitkxDir();
            tmp.Write("Card.uitkx", "export VirtualNode Card() {\n  return (<Box/>);\n}\n");
            string importer = tmp.Write("Screen.uitkx",
                "import { Card as Tile } from \"./Card\"\n" +
                "import * as X from \"./Card\"\n" +
                "export VirtualNode Screen() {\n  return (<Box/>);\n}\n");

            var (ds, _) = Parse(File.ReadAllText(importer), importer);
            var stars = ImportScopeFacts.ComputeStarImportNamespaces(ds, importer);
            var aliases = ImportScopeFacts.ComputeImportAliasTypeMap(ds, importer);
            Assert.True(stars.ContainsKey("X"));
            Assert.True(aliases.ContainsKey("Tile"));
            Assert.EndsWith(".Card", aliases["Tile"]);
            Assert.Equal(stars["X"] + ".Card", aliases["Tile"]);
        }

        // ── F12: parameter modifiers parse into the type text ───────────────

        [Fact]
        public void F12_ParamsModifier_ParsesIntoTypeText()
        {
            var (ds, _) = Parse(
                "export string Join(params object[] xs) { return string.Concat(xs); }\n");
            Assert.Single(ds.MemberDeclarations);
            var p = ds.MemberDeclarations[0].Params;
            Assert.Single(p);
            Assert.Equal("params object[]", p[0].Type);
            Assert.Equal("xs", p[0].Name);
        }

        [Fact]
        public void F12_RefModifier_ParsesAndBridgesForwardIt()
        {
            using var tmp = new TempUitkxDir();
            tmp.Write("utils.uitkx", "export void Bump(ref int counter) {\n  counter++;\n}\n");
            string importer = tmp.Write("Screen.uitkx",
                "import { Bump as Inc } from \"./utils\"\n" +
                "export VirtualNode Screen() {\n  return (<Box/>);\n}\n");

            var (ds, _) = Parse(File.ReadAllText(importer), importer);
            var lines = ImportScopeFacts.ComputeImportedMemberBridgeLines(ds, importer);
            Assert.Single(lines);
            Assert.Contains("Inc(ref int counter)", lines[0]);
            Assert.EndsWith(".__Exports.Bump(ref counter);", lines[0]);
        }

        // ── F2/F3/F8/P2: formatter guarantees ───────────────────────────────

        [Fact]
        public void F3_EmptyFile_FormatsToItself_NoFabricatedComponent()
        {
            string formatted = N(s_fmt.Format(""));
            Assert.DoesNotContain("component Component", formatted);
        }

        [Fact]
        public void F3_ImportsOnlyFile_FormatUnchanged()
        {
            string src = "import { a } from \"./x\"\n";
            string formatted = N(s_fmt.Format(src));
            Assert.DoesNotContain("component Component", formatted);
            Assert.Equal(N(src), formatted);
        }

        [Fact]
        public void F2_SameLineDeclarations_KeepSourceOrder()
        {
            string src = "export int A = 1; export int B = 2;\n";
            string formatted = N(s_fmt.Format(src));
            int a = formatted.IndexOf("export int A", StringComparison.Ordinal);
            int b = formatted.IndexOf("export int B", StringComparison.Ordinal);
            Assert.True(a >= 0 && b >= 0 && a < b);
        }

        [Fact]
        public void F8_DefaultOnlyExport_DoesNotGainInlinePrefix()
        {
            string src =
                "VirtualNode App() {\n" +
                "  return (\n" +
                "    <Box />\n" +
                "  );\n" +
                "}\n" +
                "\n" +
                "export default App;\n";
            string formatted = N(s_fmt.Format(src));
            Assert.Contains("VirtualNode App()", formatted);
            Assert.DoesNotContain("export VirtualNode App()", formatted);
            Assert.Contains("export default App;", formatted);
            Assert.Equal(formatted, N(s_fmt.Format(formatted)));
        }

        [Fact]
        public void P2_LegacyMixedFile_IsLeftUntouched()
        {
            string src =
                "component Foo {\n" +
                "  return (<Box/>);\n" +
                "}\n" +
                "export hook useThing() -> (int) {\n" +
                "  return (1);\n" +
                "}\n";
            Assert.Equal(N(src), N(s_fmt.Format(src)));
        }

        [Fact]
        public void PC8_Imports_GroupNamedThenStarThenDefault()
        {
            string src =
                "import * as X from \"./x\"\n" +
                "import D from \"./z\"\n" +
                "import { a } from \"./y\"\n" +
                "export int V = 1;\n";
            string formatted = N(s_fmt.Format(src));
            int named = formatted.IndexOf("import { a }", StringComparison.Ordinal);
            int star = formatted.IndexOf("import * as X", StringComparison.Ordinal);
            int deflt = formatted.IndexOf("import D from", StringComparison.Ordinal);
            Assert.True(named >= 0 && star >= 0 && deflt >= 0);
            Assert.True(named < star && star < deflt);
            Assert.Equal(formatted, N(s_fmt.Format(formatted)));
        }
    }
}
