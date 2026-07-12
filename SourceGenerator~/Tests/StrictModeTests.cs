using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ReactiveUITK.Language.Parser;
using ReactiveUITK.SourceGenerator;
using Xunit;
using static ReactiveUITK.SourceGenerator.StrictImportDetector;

namespace ReactiveUITK.SourceGenerator.Tests
{
    /// <summary>
    /// The strict reference detector (plan §6 / §15 StrictModeTests): 2305 (peer-exported but not
    /// imported, with the exact fix-it line), 2307 (used-like-a-hook but nothing exports it),
    /// ambient/builtin exemption, and the import/self-declaration satisfaction paths.
    /// </summary>
    public sealed class StrictModeTests
    {
        private const string Importer = "C:/proj/Assets/UI/Screen.uitkx";

        private static bool NoBuiltins(string _) => false;
        private static bool BuiltinUseState(string n) => n == "useState" || n == "useEffect";

        private static DirectiveSet DsWithImports(params (string[] names, string spec)[] imports)
        {
            var ds = new DirectiveSet(
                Namespace: "N", ComponentName: "Screen", PropsTypeName: null, DefaultKey: null,
                Usings: ImmutableArray<string>.Empty, UssFiles: ImmutableArray<string>.Empty,
                Injects: ImmutableArray<(string, string)>.Empty, MarkupStartLine: 1, MarkupStartIndex: 0);
            if (imports.Length == 0) return ds;
            var builder = ImmutableArray.CreateBuilder<ImportDeclaration>();
            foreach (var (names, spec) in imports)
                builder.Add(new ImportDeclaration(names.ToImmutableArray(), spec, 1, 0, ImmutableArray<int>.Empty));
            return ds with { Imports = builder.ToImmutable() };
        }

        private static readonly IReadOnlyList<PeerExport> Peers = new[]
        {
            new PeerExport("StatusChip", "C:/proj/Assets/UI/StatusChip.uitkx", ExportKind.Component),
            new PeerExport("useCounter", "C:/proj/Assets/UI/Counter.hooks.uitkx", ExportKind.Hook),
            new PeerExport("Palette", "C:/proj/Assets/UI/Palette.uitkx", ExportKind.Module),
        };

        [Fact]
        public void ComponentTag_NotImported_Emits2305_WithFixItLine()
        {
            var findings = Detect(DsWithImports(), Importer, "<StatusChip />", Peers, NoBuiltins);
            var f = Assert.Single(findings, x => x.Code == "UITKX2305");
            Assert.Contains("import { StatusChip } from \"./StatusChip\"", f.Message);
        }

        [Fact]
        public void ComponentTag_Imported_NoFinding()
        {
            var ds = DsWithImports((new[] { "StatusChip" }, "./StatusChip"));
            var findings = Detect(ds, Importer, "<StatusChip />", Peers, NoBuiltins);
            Assert.Empty(findings);
        }

        [Fact]
        public void HookCall_ExportedButNotImported_Emits2305()
        {
            var findings = Detect(DsWithImports(), Importer, "var c = useCounter();", Peers, BuiltinUseState);
            Assert.Single(findings, x => x.Code == "UITKX2305" && x.Message.Contains("useCounter"));
        }

        [Fact]
        public void HookCall_NoExporter_NotBuiltin_Emits2307()
        {
            var findings = Detect(DsWithImports(), Importer, "var x = useMystery();", Peers, BuiltinUseState);
            Assert.Single(findings, x => x.Code == "UITKX2307" && x.Message.Contains("useMystery"));
        }

        [Fact]
        public void BuiltinHook_Exempt_NoFinding()
        {
            var findings = Detect(DsWithImports(), Importer, "var (s, set) = useState(0);", Peers, BuiltinUseState);
            Assert.Empty(findings);
        }

        [Fact]
        public void ModuleAccess_NotImported_Emits2305()
        {
            var findings = Detect(DsWithImports(), Importer, "var g = Palette.Gap;", Peers, BuiltinUseState);
            Assert.Single(findings, x => x.Code == "UITKX2305" && x.Message.Contains("Palette"));
        }

        [Fact]
        public void SelfDeclaredName_NeverFlagged()
        {
            // Screen references a hook it declares itself → no import needed.
            var ds = new DirectiveSet(
                Namespace: "N", ComponentName: "Screen", PropsTypeName: null, DefaultKey: null,
                Usings: ImmutableArray<string>.Empty, UssFiles: ImmutableArray<string>.Empty,
                Injects: ImmutableArray<(string, string)>.Empty, MarkupStartLine: 1, MarkupStartIndex: 0)
            {
                HookDeclarations = ImmutableArray.Create(new HookDeclaration(
                    "useLocal", null, ImmutableArray<FunctionParam>.Empty, null, "return 0;", 1, 1, 0, 0)),
            };
            var peers = new[] { new PeerExport("useLocal", "C:/other.uitkx", ExportKind.Hook) };
            var findings = Detect(ds, Importer, "var x = useLocal();", peers, NoBuiltins);
            Assert.Empty(findings);
        }

        [Fact]
        public void StringLiteralAndComment_Scrubbed_NotScanned()
        {
            string code = "// <StatusChip />\nvar s = \"<StatusChip />\";";
            var findings = Detect(DsWithImports(), Importer, ScrubNonCode(code), Peers, NoBuiltins);
            Assert.Empty(findings); // the only StatusChip occurrences are in a comment and a string
        }
    }
}
