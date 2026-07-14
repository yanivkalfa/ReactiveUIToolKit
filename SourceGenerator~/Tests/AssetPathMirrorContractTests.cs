using ReactiveUITK.Language;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests
{
    /// <summary>
    /// Parity CONTRACT for <c>~/</c> (root-alias) asset-path resolution (import/export grammar §9).
    ///
    /// The canonical rule lives in <see cref="AssetPathUtil.ResolveAssetPath"/> (language-lib). It is
    /// mirrored byte-for-byte by <c>UitkxHmrController.HmrAssetPathUtil.ResolveAssetPath</c>
    /// (Editor/HMR, which cannot reference language-lib). As with
    /// <see cref="AsmdefResolverParityTests"/>, this test can only call the language-lib canonical
    /// from here — the Editor copy is kept in lockstep by code review + the file-level "BYTE-FOR-BYTE
    /// MIRROR" comment above HmrAssetPathUtil. This pins the exact input→output pairs BOTH
    /// implementations must produce, so a drift in the pinned contract is a red test.
    /// </summary>
    public sealed class AssetPathMirrorContractTests
    {
        // NOTE: the pinned cases use the DEFAULT root (no config override) because the HMR mirror
        // resolves ~/ against "Assets" only — the uitkx.config.json "root" override is honored by the
        // language-lib canonical (see UitkxConfigTests) but not yet threaded into the HMR asset mirror
        // (a documented §9 partial). So the mirror contract pins only what BOTH sides produce.
        [Theory]
        // ~/ resolves against the engine-default root (Assets) and collapses . / .. segments.
        [InlineData("Assets/UI", "~/Shared/icon.png", "Assets/Shared/icon.png")]
        [InlineData("Assets/UI/Deep", "~/theme.uss", "Assets/theme.uss")]
        [InlineData("Assets/UI", "~/a/../b/icon.png", "Assets/b/icon.png")]
        // Non-~/ behavior is unchanged: already-rooted passthrough, bare → uitkx-dir-relative.
        [InlineData("Assets/UI", "Assets/x/icon.png", "Assets/x/icon.png")]
        [InlineData("Assets/UI", "icon.png", "Assets/UI/icon.png")]
        [InlineData("Assets/UI/Deep", "../shared/icon.png", "Assets/UI/shared/icon.png")]
        public void ResolveAssetPath_MirrorContract(string uitkxDir, string raw, string expected)
        {
            // The value HmrAssetPathUtil.ResolveAssetPath MUST also return for the same inputs.
            Assert.Equal(expected, AssetPathUtil.ResolveAssetPath(uitkxDir, raw));
        }
    }
}
