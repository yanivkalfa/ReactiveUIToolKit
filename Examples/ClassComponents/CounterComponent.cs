using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Examples.ClassComponents
{
    // Interactive counter demonstrating state updates.
    public sealed class CounterComponent : ReactiveComponent
    {
        private int count;

        protected override VirtualNode Render()
        {
            var propsButton = new Dictionary<string, object>
            {
                {"style", new Dictionary<string, object>{{"width",160f},{"height",30f},{"marginTop",8f}}},
                {"onClick", (System.Action)(() => SetState(ref count, count + 1)) }
            };
            return V.VisualElement(new Dictionary<string, object>
            {
                {"style", new Dictionary<string, object>{{"padding",10f},{"backgroundColor", new Color(0.15f,0.15f,0.15f,1f)}}}
            }, null,
                V.Text($"Count: {count}"),
                V.VisualElement(propsButton, null, V.Text("Increment"))
            );
        }
    }
}
