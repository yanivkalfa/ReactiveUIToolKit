using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Examples.ClassComponents
{
    // Basic class component that renders static text.
    public sealed class SimpleHelloComponent : ReactiveComponent
    {
        protected override VirtualNode Render()
        {
            return V.View(null, null,
                V.Text("Hello ReactiveUITK (Class Component)"));
        }
    }
}
