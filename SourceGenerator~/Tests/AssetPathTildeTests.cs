using ReactiveUITK.Language;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests
{
    /// <summary>
    /// Plan §9 — <c>~/</c> (root alias) in asset references resolves against the UI source root
    /// (engine default "Assets"). Existing bare/relative/Assets-rooted behavior is unchanged. The
    /// Editor/HMR HmrAssetPathUtil mirror is updated in lockstep (byte-for-byte; Unity-compiled).
    /// </summary>
    public sealed class AssetPathTildeTests
    {
        [Theory]
        [InlineData("Assets/UI", "~/Shared/icon.png", "Assets/Shared/icon.png")]
        [InlineData("Assets/UI/Deep/Nested", "~/theme.uss", "Assets/theme.uss")]
        [InlineData("Assets/UI", "~/a/../b/icon.png", "Assets/b/icon.png")]  // collapses
        public void Tilde_ResolvesAgainstRoot(string dir, string raw, string expected)
        {
            Assert.Equal(expected, AssetPathUtil.ResolveAssetPath(dir, raw));
        }

        [Theory]
        // existing behavior preserved
        [InlineData("Assets/UI", "Assets/x/icon.png", "Assets/x/icon.png")]   // already-rooted passthrough
        [InlineData("Assets/UI", "Packages/p/icon.png", "Packages/p/icon.png")]
        [InlineData("Assets/UI", "icon.png", "Assets/UI/icon.png")]           // bare → uitkx-dir-relative
        [InlineData("Assets/UI", "./icon.png", "Assets/UI/icon.png")]
        [InlineData("Assets/UI/Deep", "../shared/icon.png", "Assets/UI/shared/icon.png")]
        public void NonTilde_BehaviorUnchanged(string dir, string raw, string expected)
        {
            Assert.Equal(expected, AssetPathUtil.ResolveAssetPath(dir, raw));
        }
    }
}
