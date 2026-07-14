using System;
using System.IO;
using ReactiveUITK.Language;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests
{
    /// <summary>
    /// The <c>uitkx.config.json "root"</c> key (import/export grammar §9): a directory walk-up
    /// (nearest-config-WINS, no merge) that a <c>~/</c> specifier resolves against, default
    /// <c>Assets</c>. Also covers the <see cref="AssetPathUtil.ResolveAssetPath"/> root override.
    /// </summary>
    public sealed class UitkxConfigTests : IDisposable
    {
        private readonly string _root;
        public UitkxConfigTests()
        {
            _root = Path.Combine(Path.GetTempPath(), "uitkx-config-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
        }
        public void Dispose() { try { Directory.Delete(_root, recursive: true); } catch { } }

        private string Write(string rel, string body)
        {
            string full = Path.Combine(_root, rel);
            Directory.CreateDirectory(Path.GetDirectoryName(full)!);
            File.WriteAllText(full, body);
            return full;
        }

        [Fact]
        public void LoadRoot_NoConfig_ReturnsDefaultAssets()
        {
            string dir = Path.Combine(_root, "UI");
            Directory.CreateDirectory(dir);
            Assert.Equal("Assets", UitkxConfig.LoadRoot(dir));
        }

        [Fact]
        public void LoadRoot_ReadsRootKey()
        {
            Write("uitkx.config.json", "{ \"root\": \"MyUI\" }");
            string dir = Path.Combine(_root, "UI");
            Directory.CreateDirectory(dir);
            Assert.Equal("MyUI", UitkxConfig.LoadRoot(dir));
        }

        [Fact]
        public void LoadRoot_NearestWins_SubdirShadowsAncestor()
        {
            Write("uitkx.config.json", "{ \"root\": \"Ancestor\" }");
            Write("UI/uitkx.config.json", "{ \"root\": \"Nearer\" }");
            string dir = Path.Combine(_root, "UI", "Deep");
            Directory.CreateDirectory(dir);
            Assert.Equal("Nearer", UitkxConfig.LoadRoot(dir));
        }

        [Fact]
        public void LoadRoot_ConfigWithoutRootKey_DefaultsAndDoesNotMergeUp()
        {
            // A nearer config without "root" wins outright (no merge) → default, not the ancestor's root.
            Write("uitkx.config.json", "{ \"root\": \"Ancestor\" }");
            Write("UI/uitkx.config.json", "{ \"printWidth\": 100 }");
            string dir = Path.Combine(_root, "UI");
            Assert.Equal("Assets", UitkxConfig.LoadRoot(dir));
        }

        [Theory]
        [InlineData("MyUI", "~/theme.uss", "MyUI/theme.uss")]
        [InlineData("Assets", "~/theme.uss", "Assets/theme.uss")]
        [InlineData(null, "~/theme.uss", "Assets/theme.uss")] // null root → default
        public void ResolveAssetPath_UsesRootOverride(string? root, string raw, string expected)
        {
            Assert.Equal(expected, AssetPathUtil.ResolveAssetPath("Assets/UI", raw, root));
        }
    }
}
