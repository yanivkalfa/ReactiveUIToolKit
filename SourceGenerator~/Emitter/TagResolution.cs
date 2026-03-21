namespace ReactiveUITK.SourceGenerator.Emitter
{
    /// <summary>
    /// Describes how a markup tag maps to a V.* call in generated code.
    /// </summary>
    public enum TagResolutionKind
    {
        /// <summary>
        /// A typed built-in element: V.Label(new LabelProps { ... }, key: key)
        /// </summary>
        BuiltinTyped,

        /// <summary>
        /// A dictionary-based built-in element: V.VisualElement(dict, key: key, children...)
        /// </summary>
        BuiltinDictionary,

        /// <summary>
        /// The V.Text(string, key) primitive.
        /// </summary>
        BuiltinText,

        /// <summary>
        /// A user-defined function component: V.Func&lt;TypeName.Props&gt;(TypeName.Render, ...) or V.Func(TypeName.Render, ...)
        /// </summary>
        FuncComponent,

        /// <summary>
        /// A fragment element (&lt;&gt;...&lt;/&gt; or explicit &lt;Fragment&gt;): V.Fragment(key, children...)
        /// </summary>
        Fragment,

        /// <summary>
        /// The V.Suspense(...) primitive.  Emitted via a dedicated code path that maps the
        /// well-known <c>isReady</c>, <c>pendingTask</c>, and <c>fallback</c> attributes
        /// directly to V.Suspense overload arguments.
        /// </summary>
        BuiltinSuspense,

        /// <summary>
        /// The V.Portal(...) primitive.  Maps the <c>target</c> attribute to the
        /// <c>portalTargetElement</c> parameter of <c>V.Portal</c>.
        /// </summary>
        BuiltinPortal,

        /// <summary>
        /// The tag could not be resolved. A UITKX0001 / UITKX0008 diagnostic is emitted.
        /// The emitter still produces a best-effort call.
        /// </summary>
        Unknown,
    }

    /// <summary>
    /// Immutable result record produced by <see cref="PropsResolver.Resolve"/>.
    /// </summary>
    /// <param name="Kind">Resolution strategy to use when emitting.</param>
    /// <param name="MethodName">
    ///   The V.* method name (e.g. "Label", "Box", "Func", "Fragment").
    /// </param>
    /// <param name="PropsTypeName">
    ///   Short name of the Props type (e.g. "LabelProps"), or <c>null</c> for
    ///   dictionary-based / text / func / fragment elements.
    /// </param>
    /// <param name="AcceptsChildren">
    ///   <c>true</c> when the resolved V.* overload has a
    ///   <c>params VirtualNode[] children</c> parameter.
    /// </param>
    /// <param name="FuncTypeName">
    ///   For <see cref="TagResolutionKind.FuncComponent"/>, the type whose <c>Render</c>
    ///   static method is called. May be fully qualified for cross-namespace peer
    ///   component emission (e.g. <c>global::App.UI.PlayerHUD</c>).
    /// </param>
    /// <param name="FuncPropsTypeName">
    ///   For <see cref="TagResolutionKind.FuncComponent"/>, the companion props class if one
    ///   was found or inferred. May be fully qualified (e.g.
    ///   <c>global::App.UI.PlayerHUD.PlayerHUDProps</c>), or <c>null</c> when no typed-props
    ///   class exists (falls back to the no-props <c>V.Func(TypeName.Render)</c> call).
    /// </param>
    public sealed record TagResolution(
        TagResolutionKind Kind,
        string MethodName,
        string? PropsTypeName,
        bool AcceptsChildren,
        string? FuncTypeName = null,
        string? FuncPropsTypeName = null
    );
}
