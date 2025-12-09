using ReactiveUITK.Core;
using ReactiveUITK.Elements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Core.Fiber
{
    /// <summary>
    /// Simple renderer using Fiber reconciler
    /// Drop-in replacement for VNodeHostRenderer
    /// </summary>
    public class FiberRenderer
    {
        private FiberRoot _root;
        private FiberReconciler _reconciler;
        private VisualElement _container;

        public FiberRenderer(VisualElement container, HostContext context = null)
        {
            UnityEngine.Debug.Log("[DuplicationTest][FiberRenderer] ctor");
            _container = container;

            if (context == null)
            {
                var registry = ElementRegistryProvider.GetDefaultRegistry();
                context = new HostContext(registry);
            }

            _reconciler = new FiberReconciler(context);
        }

        /// <summary>
        /// Render a virtual node tree (initial mount)
        /// </summary>
        public void Render(VirtualNode vnode)
        {
            UnityEngine.Debug.Log("[DuplicationTest][FiberRenderer] Render");
            if (_root == null)
            {
                // Initial mount - ensure container is clean
                _container.Clear();
                _root = _reconciler.CreateRoot(_container, vnode);
            }
            else
            {
                // Update
                _reconciler.ScheduleUpdateOnFiber(_root.Current, vnode);
            }
        }

        /// <summary>
        /// Clear and unmount
        /// </summary>
        public void Clear()
        {
            UnityEngine.Debug.Log("[DuplicationTest][FiberRenderer] Clear");
            _container.Clear();
            _root = null;
        }
    }
}
