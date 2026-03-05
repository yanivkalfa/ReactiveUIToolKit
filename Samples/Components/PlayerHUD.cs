// Companion file for PlayerHUD.uitkx
//
// The source generator emits a partial class in this namespace:
//
//   [UitkxElement("PlayerHUD")]
//   public partial class PlayerHUD
//   {
//       public static VirtualNode Render(
//           Dictionary<string, object> __rawProps,
//           IReadOnlyList<VirtualNode> __children) { ... }
//   }
//
// ── Presenter / MonoBehaviour integration ─────────────────────────────────────
//
//   using ReactiveUITK.Props;
//   using System.Collections.Generic;
//
//   // Build the props object once; mutate it on each state change.
//   var props = new PlayerHUDProps
//   {
//       Health       = 100,
//       MaxHealth    = 100,
//       ActiveAbility = "fireball",
//       Inventory    = new[] { new InventoryItem { Id = "sword", Name = "Iron Sword", Count = 1, IsEquipped = true } },
//   };
//
//   // Render the component by passing props under the "data" key:
//   var node = V.Func(PlayerHUD.Render,
//       new Dictionary<string, object> { { "data", props } });
//
//   // Reactively update Health from a Signal<int> — re-renders automatically.
//   _disposables.Add(PropsHelper.Bind(
//       propsInstance : props,
//       selector      : (PlayerHUDProps p) => p.Health,
//       signal        : _healthSignal,
//       onChanged     : RequestRender));
//
// ─────────────────────────────────────────────────────────────────────────────

namespace ReactiveUITK.Samples.FunctionalComponents
{
    public partial class PlayerHUD { }
}
