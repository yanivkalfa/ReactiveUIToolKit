using ReactiveUITK.Core.Fiber;
using UnityEngine.UIElements;

namespace ReactiveUITK.Core
{
    public interface IVNodeHostRenderer
    {
        void Render(VirtualNode vnode);
        void Unmount();
    }

    public sealed class VNodeHostRenderer : IVNodeHostRenderer
    {
        private readonly FiberRenderer fiberRenderer;
        private readonly VisualElement hostElement;

        public VNodeHostRenderer(HostContext hostContext, VisualElement host)
        {
            hostElement = host;
            fiberRenderer = new FiberRenderer(host, hostContext);

            if (FiberConfig.ShowReconcilerInfo)
            {
                UnityEngine.Debug.Log($"[VNodeHostRenderer] Using FIBER reconciler for {host.name}");
            }
        }

        public void Render(VirtualNode vnode)
        {
            fiberRenderer.Render(vnode);
        }

        public void Unmount()
        {
            fiberRenderer?.Clear();
        }
    }
}
