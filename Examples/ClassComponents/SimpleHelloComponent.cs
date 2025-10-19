using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Examples.ClassComponents
{
    public sealed class SimpleHelloComponent : ReactiveComponent
    {
        protected override VirtualNode Render()
        {
            var containerStyle = new Dictionary<string, object>{{"padding",4f}};
            var containerProps = new Dictionary<string, object>{{"style", containerStyle}};
            return V.VisualElement(containerProps, null, V.Text("Hello ReactiveUITK (Class Component)"));
        }
    }
}
