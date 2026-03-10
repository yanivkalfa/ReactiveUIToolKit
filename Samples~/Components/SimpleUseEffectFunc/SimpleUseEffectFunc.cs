using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;

namespace ReactiveUITK.Samples.FunctionalComponents
{
    public static class SimpleUseEffectFunc
    {
        public static VirtualNode Render(
            IProps rawProps,
            IReadOnlyList<VirtualNode> children
        )
        {
            var (message, setMessage) = Hooks.UseState("Waiting...");
            Hooks.UseEffect(
                () =>
                {
                    setMessage.Set("Effect ran!");
                    return null;
                },
                System.Array.Empty<object>()
            );
            return V.VisualElement(null, null, V.Text($"Message: {message}"));
        }
    }
}
