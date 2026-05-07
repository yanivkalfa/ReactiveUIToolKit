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

#if UNITY_EDITOR
        /// <summary>HMR: exposes the root for fiber tree walking.</summary>
        internal FiberRoot Root => _root;
#endif

        public FiberRenderer(VisualElement container, HostContext context = null)
        {
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
        /// Clear and unmount the entire tree.
        ///
        /// <para>
        /// Runs every effect cleanup (UseEffect / UseLayoutEffect) and
        /// disposes every signal subscription before clearing the host
        /// container. This is critical for components that own external
        /// resources (pooled <c>VideoPlayer</c>, <c>AudioSource</c>,
        /// timers, native listeners, etc.) \u2014 without this they leak,
        /// e.g. an &lt;Audio&gt; element keeps playing forever after its
        /// owning EditorWindow is closed.
        /// </para>
        /// </summary>
        public void Clear()
        {
            _reconciler?.UnmountRoot();
            _container.Clear();
            _root = null;
        }
    }
}
