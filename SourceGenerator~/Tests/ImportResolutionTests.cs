using System;
using System.Collections.Generic;
using ReactiveUITK.Language;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests
{
    /// <summary>
    /// Plan §6 — import specifier resolution + the frozen 2300/2308/2314 codes (mirrors the
    /// Unreal resolve suite's case list). Filesystem + asmdef lookups are injected.
    /// </summary>
    public sealed class ImportResolutionTests
    {
        // ── Pure path mapping ────────────────────────────────────────────────

        [Theory]
        [InlineData("Assets/UI/Screens", "./StatusChip", "Assets", "Assets/UI/Screens/StatusChip.uitkx")]
        [InlineData("Assets/UI/Screens", "../lib/X", "Assets", "Assets/UI/lib/X.uitkx")]
        [InlineData("Assets/UI/Screens", "~/Shared/Types", "Assets", "Assets/Shared/Types.uitkx")]
        [InlineData("Assets/UI", "./Chip.uitkx", "Assets", "Assets/UI/Chip.uitkx")] // already-extensioned not doubled
        [InlineData("Assets/UI", "~/Counter.hooks", "Assets", "Assets/Counter.hooks.uitkx")]
        public void MapSpecifier_RelativeAndRoot(string importerDir, string spec, string root, string expected)
        {
            string? path = ImportResolver.MapSpecifierToPath(importerDir, spec, root, out bool escaped);
            Assert.False(escaped);
            Assert.Equal(expected, path);
        }

        [Theory]
        [InlineData("Assets/Foo")]      // engine-native
        [InlineData("Packages/x/Bar")]  // engine-native
        [InlineData("/abs/path")]       // absolute
        [InlineData("BareName")]        // bare
        public void MapSpecifier_EngineNativeOrBare_ReturnsNull(string spec)
        {
            Assert.Null(ImportResolver.MapSpecifierToPath("Assets/UI", spec, "Assets", out bool escaped));
            Assert.False(escaped);
        }

        [Fact]
        public void MapSpecifier_TildeEscapesRoot_SetsEscaped()
        {
            Assert.Null(ImportResolver.MapSpecifierToPath("Assets/UI", "~/../../evil", "Assets", out bool escaped));
            Assert.True(escaped);
        }

        // ── Full resolve (injected FS + asmdef) → frozen codes ───────────────

        private static ImportResolveResult Resolve(
            string spec, string importerDir = "Assets/UI", string root = "Assets",
            string? importerAsmdef = "MyGame", IReadOnlyDictionary<string, string?>? files = null)
        {
            files ??= new Dictionary<string, string?>
            {
                ["Assets/UI/StatusChip.uitkx"] = "MyGame",
                ["Assets/Shared/Types.uitkx"] = "MyGame",
                ["Assets/Other/Far.uitkx"] = "OtherAsm",
            };
            return ImportResolver.Resolve(
                importerDir, spec, root,
                p => files.ContainsKey(p),
                p => files.TryGetValue(p, out var a) ? a : null,
                importerAsmdef);
        }

        [Fact]
        public void Resolve_ExistingSameAsmdef_IsOk()
        {
            var r = Resolve("./StatusChip");
            Assert.Equal(ImportResolveStatus.Ok, r.Status);
            Assert.Equal("Assets/UI/StatusChip.uitkx", r.ProjectRelativePath);
        }

        [Fact]
        public void Resolve_MissingFile_Is2300()
        {
            var r = Resolve("./DoesNotExist");
            Assert.Equal(ImportResolveStatus.UnknownSpecifier, r.Status);
            Assert.Equal("Assets/UI/DoesNotExist.uitkx", r.ProjectRelativePath); // candidate recorded
        }

        [Fact]
        public void Resolve_EngineNativeSpecifier_Is2300()
        {
            Assert.Equal(ImportResolveStatus.UnknownSpecifier, Resolve("Assets/UI/StatusChip").Status);
        }

        [Fact]
        public void Resolve_CrossAsmdef_Is2308()
        {
            var r = Resolve("../Other/Far");
            Assert.Equal(ImportResolveStatus.CrossesBoundary, r.Status);
            Assert.Equal("Assets/Other/Far.uitkx", r.ProjectRelativePath);
        }

        [Fact]
        public void Resolve_TildeEscape_Is2314()
        {
            Assert.Equal(ImportResolveStatus.RootEscape, Resolve("~/../../evil").Status);
        }
    }
}
