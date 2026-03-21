namespace ReactiveUITK.Core
{
    /// <summary>
    /// Well-known <see cref="HostContext"/> environment keys for named portal target slots.
    ///
    /// The host (EditorWindow / RootRenderer / test harness) seeds these values into the
    /// <see cref="HostContext.Environment"/> before mounting the component tree.
    /// Any component in the tree retrieves them via <c>Hooks.UseContext&lt;VisualElement&gt;</c>
    /// and passes the result to <c>V.Portal(target, ...)</c>.
    ///
    /// Usage (host side):
    /// <code>
    /// EditorRootRendererUtility.Render(
    ///     contentRegion,
    ///     V.Func(MyComponent.Render),
    ///     env: ctx => ctx.Environment[PortalContextKeys.ModalRoot] = myOverlayPanel
    /// );
    /// </code>
    ///
    /// Usage (component side — C#):
    /// <code>
    /// var overlayRoot = Hooks.UseContext&lt;VisualElement&gt;(PortalContextKeys.ModalRoot);
    /// var portal = overlayRoot != null ? V.Portal(overlayRoot, null, ...) : null;
    /// </code>
    ///
    /// Usage (component side — UITKX):
    /// <code>
    /// var overlayRoot = useContext&lt;VisualElement&gt;(PortalContextKeys.ModalRoot);
    /// </code>
    /// </summary>
    public static class PortalContextKeys
    {
        /// <summary>
        /// The primary overlay / modal layer.
        /// Equivalent to a &lt;div id="modal-root"&gt; in a browser React app.
        /// </summary>
        public const string ModalRoot = "__portal:modal-root";

        /// <summary>
        /// A floating tooltip layer rendered above all other content.
        /// </summary>
        public const string TooltipRoot = "__portal:tooltip-root";

        /// <summary>
        /// A full-screen overlay layer (e.g. for drawers, side panels).
        /// </summary>
        public const string OverlayRoot = "__portal:overlay-root";
    }
}
