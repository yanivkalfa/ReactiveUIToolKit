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
                {"style.width", 160f},
                {"style.height", 30f},
                {"style.marginTop", 8f},
                {"onClick", (System.Action)(() => SetState(ref count, count + 1)) }
            };
            return V.View(new Dictionary<string, object>
            {
                {"style.padding", 10f},
                {"style.backgroundColor", new Color(0.15f,0.15f,0.15f,1f)}
            }, null,
                V.Text($"Count: {count}"),
                V.View(propsButton, null, V.Text("Increment"))
            );
        }
    }
}
