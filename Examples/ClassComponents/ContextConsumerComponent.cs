using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine;

namespace ReactiveUITK.Examples.ClassComponents
{
    // Consumes theme color from context, falling back if absent.
    public sealed class ContextConsumerComponent : ReactiveComponent
    {
        protected override VirtualNode Render()
        {
            var col = ConsumeContext<Color>("themeColor");
            if (col == default) col = Color.gray;
            return V.VisualElement(new Dictionary<string, object>
            {
                {"style", new Dictionary<string, object>{{"padding",6f},{"backgroundColor", col}}}
            }, null, V.Text("Consumer uses theme color"));
        }
    }
}
