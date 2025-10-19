using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine;

namespace ReactiveUITK.Examples.ClassComponents
{
    public sealed class ContextConsumerComponent : ReactiveComponent
    {
        protected override VirtualNode Render()
        {
            var resolvedColor = ConsumeContext<Color>("themeColor");
            if (resolvedColor == default)
            {
                resolvedColor = Color.gray;
            }
            var style = new Dictionary<string, object>{{"padding",6f},{"backgroundColor", resolvedColor}};
            var props = new Dictionary<string, object>{{"style", style}};
            return V.VisualElement(props, null, V.Text("Consumer uses theme color"));
        }
    }
}
