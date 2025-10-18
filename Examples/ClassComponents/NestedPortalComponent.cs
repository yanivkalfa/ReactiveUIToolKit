using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Examples.ClassComponents
{
    // Demonstrates portal usage: renders a label into an external VisualElement.
    public sealed class NestedPortalComponent : ReactiveComponent
    {
        public VisualElement ExternalTarget;
        public string Message = "Portal Content";
        protected override VirtualNode Render()
        {
            return V.View(null, null,
                V.Text("Regular content"),
                V.Portal(ExternalTarget, null, V.Text(Message))
            );
        }
    }
}
