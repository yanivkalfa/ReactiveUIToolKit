using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Core
{
    public interface IReactiveComponent
    {
        void SetProps(Dictionary<string, object> nextProps);
        void Mount(VisualElement parentElement, HostContext hostContext);
        void Unmount();
        void NotifyContextKeyChanged(string key);
    }
}

