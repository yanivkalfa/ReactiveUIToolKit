using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine;

namespace ReactiveUITK.Examples.ClassComponents
{
    public sealed class SimpleUseEffectComponent : ReactiveComponent
    {
        private string message = "Waiting...";
        protected override VirtualNode Render()
        {
            UseEffect(() =>
            {
                message = "Effect ran!";
                SetState(() => { });
                return null;
            }, true);
            return V.VisualElement(null, null,
                V.Text($"Message: {message}")
            );
        }
    }
}
