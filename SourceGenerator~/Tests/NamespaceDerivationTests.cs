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
