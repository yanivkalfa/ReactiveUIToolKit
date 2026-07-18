using ReactiveUITK.Language;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests
{
    /// <summary>
    /// Plan §4 — path-derived default namespace + segment sanitization. The algorithm is pinned
    /// EXACTLY here (the HMR mirror + LSP consume the same rule; parity is enforced separately).
    /// </summary>
    public sealed class NamespaceDerivationTests
    {
        [Fact]
        public void FileBesideAsmdef_DerivesRootOnly()
        {
            string ns = NamespaceDerivation.Derive(
                "C:/proj/Assets/Samples/Board.uitkx", "C:/proj/Assets/Samples");
            Assert.Equal("ReactiveUITK.Uitkx", ns);
        }

        [Fact]
        public void NestedDirs_JoinSanitizedSegments_FileNameExcluded()
        {
            // Plan §4 canonical example.
            string ns = NamespaceDerivation.Derive(
                "C:/proj/Assets/Samples/Components/Tic Tac-Toe/Board.uitkx",
                "C:/proj/Assets/Samples");
            Assert.Equal("ReactiveUITK.Uitkx.Components.Tic_Tac_Toe", ns);
        }

        [Fact]
        public void BackslashPaths_NormalizeIdentically()
        {
            string ns = NamespaceDerivation.Derive(
                @"C:\proj\Assets\Samples\Components\Widget\Widget.uitkx",
                @"C:\proj\Assets\Samples");
            Assert.Equal("ReactiveUITK.Uitkx.Components.Widget", ns);
        }

        [Fact]
        public void NoOwningAsmdef_ReturnsNull_ForUITKX2310()
        {
            Assert.Null(NamespaceDerivation.Derive("C:/proj/Assets/x/Board.uitkx", null));
            Assert.Null(NamespaceDerivation.Derive("C:/proj/Assets/x/Board.uitkx", ""));
        }

        // ── Configurable prefix (namespace-import unification plan, feature 3) ──

        [Fact]
        public void Derive_NullPrefix_UsesDefaultRoot()
        {
            Assert.Equal("ReactiveUITK.Uitkx.Widget",
                NamespaceDerivation.Derive("C:/p/Assets/S/Widget/W.uitkx", "C:/p/Assets/S"));
        }

        [Fact]
        public void Derive_WithPrefix_ReplacesRoot()
        {
            Assert.Equal("UI.App.Components.Widget",
                NamespaceDerivation.Derive(
                    "C:/p/Assets/UI/App/Components/Widget/W.uitkx", "C:/p/Assets/UI/App", "UI.App"));
        }

        [Fact]
        public void Derive_PrefixBesideAsmdef_PrefixOnly()
        {
            Assert.Equal("UI.App",
                NamespaceDerivation.Derive("C:/p/Assets/UI/App/Root.uitkx", "C:/p/Assets/UI/App", "UI.App"));
        }

        [Fact]
        public void Derive_SanitizesBadPrefix()
        {
            // A mis-typed prefix can never emit invalid C#.
            Assert.Equal("My_Game.Widget",
                NamespaceDerivation.Derive("C:/p/Assets/S/Widget/W.uitkx", "C:/p/Assets/S", "My-Game"));
        }

        // ── File-keyed derivation (ES-modules campaign, U-01) ────────────────

        [Fact]
        public void DeriveFileModule_AppendsSanitizedFileStem()
        {
            Assert.Equal("ReactiveUITK.Uitkx.Components.Widget.Board",
                NamespaceDerivation.DeriveFileModule(
                    "C:/proj/Assets/Samples/Components/Widget/Board.uitkx", "C:/proj/Assets/Samples"));
        }

        [Fact]
        public void DeriveFileModule_FileBesideAsmdef_RootPlusStem()
        {
            Assert.Equal("ReactiveUITK.Uitkx.Board",
                NamespaceDerivation.DeriveFileModule(
                    "C:/proj/Assets/Samples/Board.uitkx", "C:/proj/Assets/Samples"));
        }

        [Fact]
        public void DeriveFileModule_DottedStem_SanitizedAsOneToken()
        {
            // A companion-style filename: `AppRoot.style.uitkx` → stem `AppRoot.style` → the
            // interior dot is a literal char, sanitized to '_' (NOT a namespace separator).
            Assert.Equal("ReactiveUITK.Uitkx.Widget.AppRoot_style",
                NamespaceDerivation.DeriveFileModule(
                    "C:/proj/Assets/S/Widget/AppRoot.style.uitkx", "C:/proj/Assets/S"));
        }

        [Fact]
        public void DeriveFileModule_DigitLeadingStem_UnderscorePrefixed()
        {
            Assert.Equal("ReactiveUITK.Uitkx._3DViewer",
                NamespaceDerivation.DeriveFileModule(
                    "C:/proj/Assets/S/3DViewer.uitkx", "C:/proj/Assets/S"));
        }

        [Fact]
        public void DeriveFileModule_ReservedKeywordStem_UnderscorePrefixed()
        {
            Assert.Equal("ReactiveUITK.Uitkx._int",
                NamespaceDerivation.DeriveFileModule(
                    "C:/proj/Assets/S/int.uitkx", "C:/proj/Assets/S"));
        }

        [Fact]
        public void DeriveFileModule_WithPrefix_ReplacesRoot()
        {
            Assert.Equal("UI.App.Widget.Board",
                NamespaceDerivation.DeriveFileModule(
                    "C:/p/Assets/UI/App/Widget/Board.uitkx", "C:/p/Assets/UI/App", "UI.App"));
        }

        [Fact]
        public void DeriveFileModule_NoOwningAsmdef_ReturnsNull()
        {
            Assert.Null(NamespaceDerivation.DeriveFileModule("C:/proj/Assets/x/Board.uitkx", null));
            Assert.Null(NamespaceDerivation.DeriveFileModule("C:/proj/Assets/x/Board.uitkx", ""));
        }

        [Fact]
        public void DeriveFileModule_TwoSameFolderFiles_GetDifferentNamespaces()
        {
            // G-01: a file IS a module — same-folder files must never share a namespace.
            string a = NamespaceDerivation.DeriveFileModule(
                "C:/proj/Assets/S/Widget/Board.uitkx", "C:/proj/Assets/S");
            string b = NamespaceDerivation.DeriveFileModule(
                "C:/proj/Assets/S/Widget/Panel.uitkx", "C:/proj/Assets/S");
            Assert.NotEqual(a, b);
            // While legacy folder-keyed derivation still shares (deprecation window).
            Assert.Equal(
                NamespaceDerivation.Derive("C:/proj/Assets/S/Widget/Board.uitkx", "C:/proj/Assets/S"),
                NamespaceDerivation.Derive("C:/proj/Assets/S/Widget/Panel.uitkx", "C:/proj/Assets/S"));
        }

        [Theory]
        // keep [A-Za-z0-9_]; case preserved verbatim
        [InlineData("Components", "Components")]
        [InlineData("widget_2", "widget_2")]
        // every other char -> '_'
        [InlineData("Tic Tac-Toe", "Tic_Tac_Toe")]
        [InlineData("foo-bar", "foo_bar")]
        [InlineData("a.b", "a_b")]
        [InlineData("café☕", "caf__")] // é (U+00E9) and ☕ (U+2615) are each 1 BMP char → 2 underscores
        // leading digit -> '_' prefix
        [InlineData("3D", "_3D")]
        [InlineData("2players", "_2players")]
        // C# reserved keyword (case-sensitive) -> '_' prefix; non-keyword casing untouched
        [InlineData("int", "_int")]
        [InlineData("class", "_class")]
        [InlineData("Int", "Int")]
        [InlineData("Class", "Class")]
        // empty -> '_'
        [InlineData("", "_")]
        public void Sanitize_MatchesPinnedTable(string input, string expected)
        {
            Assert.Equal(expected, NamespaceDerivation.Sanitize(input));
        }
    }
}
