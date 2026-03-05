using System.Collections.Generic;

namespace ReactiveUITK.Samples.FunctionalComponents
{
    /// <summary>
    /// Typed props for the <see cref="PlayerHUD"/> UITKX component.
    /// </summary>
    /// <remarks>
    /// Pass an instance under the <c>"data"</c> key when calling <c>V.Func</c>:
    /// <code>
    ///   var node = V.Func(PlayerHUD.Render,
    ///       new Dictionary&lt;string, object&gt; { { "data", myProps } });
    /// </code>
    /// Use <see cref="ReactiveUITK.Props.PropsHelper.Bind{TProps,TValue}"/> to keep
    /// props in sync with reactive data sources (Signals or INotifyPropertyChanged).
    /// </remarks>
    public sealed class PlayerHUDProps
    {
        public int Health    { get; set; } = 100;
        public int MaxHealth { get; set; } = 100;

        /// <summary>
        /// Token identifying the currently-selected ability.
        /// Recognised values: <c>"fireball"</c>, <c>"shield"</c>.
        /// Any other value falls through to the "No ability" default.
        /// </summary>
        public string ActiveAbility { get; set; } = "";

        public IReadOnlyList<InventoryItem> Inventory { get; set; } = new List<InventoryItem>();

        /// <summary>Fallback used when no props are forwarded to the component.</summary>
        public static readonly PlayerHUDProps Default = new PlayerHUDProps();
    }

    /// <summary>A single item slot rendered by <see cref="PlayerHUD"/>.</summary>
    public sealed class InventoryItem
    {
        /// <summary>
        /// Stable unique identifier. Used as the UITKX <c>key</c> attribute so the
        /// reconciler can track items across list changes without unmounting them.
        /// </summary>
        public string Id         { get; set; }
        public string Name       { get; set; }
        public int    Count      { get; set; }
        public bool   IsEquipped { get; set; }
    }
}
