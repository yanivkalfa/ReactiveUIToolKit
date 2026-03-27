using ReactiveUITK.Core;

namespace ReactiveUITK
{
    /// <summary>
    /// Short helpers for loading Unity assets by path from the UITKX asset registry.
    /// Import via <c>using static ReactiveUITK.AssetHelpers;</c>
    /// <code>
    /// Asset&lt;Texture2D&gt;("Assets/UI/avatar")
    /// Ast&lt;Sprite&gt;("Assets/UI/icon")
    /// </code>
    /// </summary>
    public static class AssetHelpers
    {
        /// <summary>
        /// Load a Unity asset by its registry key (resolved asset path without extension).
        /// </summary>
        /// <typeparam name="T">The Unity asset type (Texture2D, Sprite, Font, etc.)</typeparam>
        /// <param name="path">
        /// The asset path as registered. In .uitkx files, relative paths like
        /// <c>"./avatar.png"</c> are resolved by the source generator to absolute
        /// registry keys like <c>"Assets/UI/avatar"</c>.
        /// </param>
        public static T Asset<T>(string path) where T : UnityEngine.Object
        {
            return UitkxAssetRegistry.Get<T>(path);
        }

        /// <summary>Shorthand alias for <see cref="Asset{T}"/>.</summary>
        public static T Ast<T>(string path) where T : UnityEngine.Object
        {
            return UitkxAssetRegistry.Get<T>(path);
        }
    }
}
