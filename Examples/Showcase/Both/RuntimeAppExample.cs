using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Examples.Shared;

namespace ReactiveUITK.Examples.FunctionalComponents
{
    public static class RuntimeAppExample
    {
        public static VirtualNode Render(Dictionary<string, object> props, IReadOnlyList<VirtualNode> children)
        {
            return V.Func(SharedDemoPage.Render);
        }
    }

    public sealed class RuntimeAppExampleRoot : ReactiveComponent
    {
        protected override VirtualNode Render()
        {
            return V.Func(RuntimeAppExample.Render);
        }
    }
}
