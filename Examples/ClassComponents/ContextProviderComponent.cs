using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine;

namespace ReactiveUITK.Examples.ClassComponents
{
    // Provides a theme color via context.
    public sealed class ContextProviderComponent : ReactiveComponent
    {
        public Color ThemeColor = Color.cyan;
        protected override VirtualNode Render()
        {
            ProvideContext("themeColor", ThemeColor);
            return V.VisualElement(new Dictionary<string, object>
            {
                {"style", new Dictionary<string, object>{{"padding",6f},{"backgroundColor", ThemeColor}}}
            }, null, V.Text("Provided theme context"));
        }
    }
}
