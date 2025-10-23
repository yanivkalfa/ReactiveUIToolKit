using System.Collections.Generic;
using ReactiveUITK.Core;

namespace ReactiveUITK.Samples.FunctionalComponents
{
    public static class HelloWorldFunc
    {
        public static VirtualNode Render(Dictionary<string, object> props, IReadOnlyList<VirtualNode> children)
        {
            return V.Text("Hello, world! (Functional Component)");
        }
    }
}
