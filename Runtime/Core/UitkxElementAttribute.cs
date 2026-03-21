using System;

namespace ReactiveUITK
{
    /// <summary>
    /// Marks a partial class as a UITKX-managed component whose <c>Render()</c>
    /// method was generated from a <c>.uitkx</c> source file.
    ///
    /// The source generator emits this attribute automatically on every generated
    /// partial class so that runtime tooling and reflection-based helpers
    /// (e.g. <see cref="ReactiveUITK.Props.PropsHelper"/>) can discover UITKX
    /// components without scanning assembly names.
    /// </summary>
    /// <example>
    /// The generator turns:
    /// <code>
    ///   @component PlayerHUD
    ///   @namespace MyGame.UI
    /// </code>
    /// into:
    /// <code>
    ///   [UitkxElement("PlayerHUD")]
    ///   public partial class PlayerHUD { ... }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class UitkxElementAttribute : Attribute
    {
        /// <summary>The UITKX component name (matches the <c>@component</c> directive).</summary>
        public string ComponentName { get; }

        /// <param name="componentName">
        /// The name declared via <c>@component</c> in the <c>.uitkx</c> file.
        /// </param>
        public UitkxElementAttribute(string componentName)
        {
            ComponentName = componentName ?? throw new ArgumentNullException(nameof(componentName));
        }
    }
}
