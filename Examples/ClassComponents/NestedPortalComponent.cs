using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Examples.ClassComponents
{
    public sealed class NestedPortalComponent : ReactiveComponent
    {
        public VisualElement externalTarget;
        public string message = "Portal Content";
        protected override VirtualNode Render()
        {
            return V.VisualElement(null, null,
                V.Text("Regular content"),
                V.Portal(externalTarget, null, V.Text(message))
            );
        }
    }
}
