using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine.UIElements;

namespace ReactiveUITK.Examples.ClassComponents
{
    public sealed class HelloWorldComponent : ReactiveComponent
    {
        protected override VirtualNode Render()
        {
            return V.Text("Hello, world! (Class Component)");
        }
    }
}
