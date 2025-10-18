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
            return V.View(new Dictionary<string, object>
            {
                {"style.padding", 6f},
                {"style.backgroundColor", ThemeColor}
            }, null, V.Text("Provided theme context"));
        }
    }
}
