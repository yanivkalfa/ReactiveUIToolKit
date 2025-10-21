using ReactiveUITK.Core;
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
        private readonly Reconciler reconciler;
        private readonly VisualElement hostElement;
        private VirtualNode lastVNode;

        public VNodeHostRenderer(HostContext hostContext, VisualElement host)
        {
            hostElement = host;
            reconciler = new Reconciler(hostContext);
        }

        public void Render(VirtualNode vnode)
        {
            if (lastVNode == null)
            {
                reconciler.BuildSubtree(hostElement, vnode);
            }
            else
            {
                reconciler.DiffSubtree(hostElement, lastVNode, vnode);
            }
            lastVNode = vnode;
        }

        public void Unmount()
        {
            hostElement.Clear();
            lastVNode = null;
        }
    }
}

