using System.Collections.Generic;
using ReactiveUITK.Core.Util;

namespace ReactiveUITK
{
    public abstract class PureReactiveComponent : ReactiveComponent
    {
        protected override bool ShouldUpdate(Dictionary<string, object> nextProps)
        {
            return !ShallowCompare.PropsEqual(Props, nextProps);
        }
    }
}
