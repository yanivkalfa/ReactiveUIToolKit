using System.Collections.Generic;
using System.Linq;
using Ruitk.Language;
using Ruitk.Language.Parser;
using Xunit;

namespace Ruitk.SourceGenerator.Tests
{
    /// <summary>
    /// UITKX2114: an export name is emitted VERBATIM as a C# class or member name, so a reserved
    /// keyword produced uncompilable generated code with no UITKX diagnostic anywhere. Components
    /// were already shielded by the PascalCase rule (every C# keyword is lowercase); the reachable
    /// hole was the camelCase kinds - style, utils and value exports.
    /// </summary>
    public sealed class ReservedIdentifierNameTests
    {
        private static List<ParseDiagnostic> Diags(string source)
        {
            var diags = new List<ParseDiagnostic>();
            DirectiveParser.Parse(source, "C:/p/Assets/UI/File.uitkx", diags);
            return diags;
        }

        private static bool Has2114(string source) =>
            Diags(source).Any(d => d.Code == "UITKX2114");

        [Theory]
        [InlineData("default")]
        [InlineData("class")]
        [InlineData("int")]
        [InlineData("new")]
        [InlineData("object")]
        [InlineData("string")]
        public void ValueExport_NamedAKeyword_IsUITKX2114(string keyword)
        {
            Assert.True(Has2114("export int " + keyword + " = 0;\n"));
        }

        [Fact]
        public void UtilExport_NamedAKeyword_IsUITKX2114()
        {
            Assert.True(Has2114("export int lock() {\n  return 1;\n}\n"));
        }

        [Theory]
        [InlineData("New")]
        [InlineData("Default")]
        [InlineData("Class")]
        [InlineData("Card")]
        public void PascalCaseComponent_IsNeverAKeyword(string name)
        {
            // C# keywords are case-sensitive and all lowercase: 'new' is reserved, 'New' is not.
            // This pins that the guard never refuses an ordinary component name.
            Assert.False(Has2114(
                "export VirtualNode " + name + "() {\n  return (<Box />);\n}\n"));
        }

        [Fact]
        public void ContextualKeyword_IsALegalName()
        {
            // 'value', 'var' and 'record' are contextual - legal identifiers, deliberately absent
            // from the reserved set.
            Assert.False(Has2114("export int value = 0;\n"));
            Assert.False(Has2114("export int record = 0;\n"));
        }

        [Fact]
        public void ReservedKeyword_ComparisonIsOrdinal()
        {
            Assert.True(CSharpIdentifiers.IsReservedKeyword("new"));
            Assert.False(CSharpIdentifiers.IsReservedKeyword("New"));
            Assert.False(CSharpIdentifiers.IsReservedKeyword("NEW"));
            Assert.False(CSharpIdentifiers.IsReservedKeyword(""));
            Assert.False(CSharpIdentifiers.IsReservedKeyword(null));
        }

        [Fact]
        public void IsValidIdentifier_RejectsKeywordsAndIllegalShapes()
        {
            Assert.True(CSharpIdentifiers.IsValidIdentifier("Card"));
            Assert.True(CSharpIdentifiers.IsValidIdentifier("_leading"));
            Assert.True(CSharpIdentifiers.IsValidIdentifier("a1_b2"));
            Assert.False(CSharpIdentifiers.IsValidIdentifier("class"));
            Assert.False(CSharpIdentifiers.IsValidIdentifier("3DViewer"));
            Assert.False(CSharpIdentifiers.IsValidIdentifier("my-panel"));
            Assert.False(CSharpIdentifiers.IsValidIdentifier("has space"));
            Assert.False(CSharpIdentifiers.IsValidIdentifier(""));
        }

        [Theory]
        [InlineData("my-panel", "MyPanel")]
        [InlineData("player_card", "PlayerCard")]
        [InlineData("hud", "Hud")]
        [InlineData("3d-viewer", "_3dViewer")]
        [InlineData("Already", "Already")]
        public void ToPascalIdentifier_FoldsAFileStem(string stem, string expected)
        {
            Assert.Equal(expected, CSharpIdentifiers.ToPascalIdentifier(stem));
        }

        [Theory]
        [InlineData("")]
        [InlineData("---")]
        [InlineData(null)]
        public void ToPascalIdentifier_FallsBackWhenNothingSurvives(string? stem)
        {
            Assert.Equal("Component", CSharpIdentifiers.ToPascalIdentifier(stem));
        }

        [Fact]
        public void NamespaceSanitize_StillUnderscorePrefixesAKeywordStem()
        {
            // The keyword list moved to CSharpIdentifiers; Sanitize must behave byte-identically.
            Assert.Equal("_int", NamespaceDerivation.Sanitize("int"));
            Assert.Equal("Int", NamespaceDerivation.Sanitize("Int"));
            Assert.Equal("_3D", NamespaceDerivation.Sanitize("3D"));
        }
    }
}
