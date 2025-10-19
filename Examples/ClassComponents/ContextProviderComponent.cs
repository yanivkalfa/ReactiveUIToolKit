using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine;

namespace ReactiveUITK.Examples.ClassComponents
{
    public sealed class ContextProviderComponent : ReactiveComponent
    {
        public Color themeColor = Color.cyan;
        protected override VirtualNode Render()
        {
            ProvideContext("themeColor", themeColor);
            var style = new Dictionary<string, object>{{"padding",6f},{"backgroundColor", themeColor}};
            var props = new Dictionary<string, object>{{"style", style}};
            return V.VisualElement(props, null, V.Text("Provided theme context"));
        }
    }
}
