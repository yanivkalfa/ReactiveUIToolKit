using System;
using System.Collections.Generic;
using ReactiveUITK.Core.Fiber;
using ReactiveUITK.Props;
using UnityEngine;
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
        private IReadOnlyDictionary<string, object> lastHostProps;

#if UNITY_EDITOR
        /// <summary>HMR: exposes the FiberRenderer for tree walking.</summary>
        internal FiberRenderer FiberRendererInternal => fiberRenderer;
#endif

        public VNodeHostRenderer(HostContext hostContext, VisualElement host)
        {
            hostElement = host;
            fiberRenderer = new FiberRenderer(host, hostContext);
        }

        public void Render(VirtualNode vnode)
        {
            fiberRenderer.Render(NormalizeHostRoot(vnode));
        }

        public void Unmount()
        {
            ClearHostProps();
            fiberRenderer?.Clear();
        }

        private VirtualNode NormalizeHostRoot(VirtualNode vnode)
        {
            if (vnode == null)
            {
                ClearHostProps();
                return null;
            }

            if (vnode.NodeType != VirtualNodeType.Host)
            {
                ClearHostProps();
                return vnode;
            }

            ApplyHostProps(vnode.Properties ?? VirtualNode.EmptyProps);
            return WrapHostChildren(vnode);
        }

        private void ApplyHostProps(IReadOnlyDictionary<string, object> nextProps)
        {
            if (lastHostProps == null)
            {
                PropsApplier.Apply(hostElement, nextProps);
            }
            else
            {
                PropsApplier.ApplyDiff(hostElement, lastHostProps, nextProps);
            }

            lastHostProps = nextProps;
        }

        private void ClearHostProps()
        {
            if (lastHostProps == null)
            {
                return;
            }

            PropsApplier.ApplyDiff(hostElement, lastHostProps, VirtualNode.EmptyProps);
            lastHostProps = null;
        }

        private static VirtualNode WrapHostChildren(VirtualNode hostNode)
        {
            var children = hostNode.Children;
            int count = children?.Count ?? 0;
            if (count == 0)
            {
                return ReactiveUITK.V.Fragment(hostNode.Key);
            }

            var buffer = new VirtualNode[count];
            for (int i = 0; i < count; i++)
            {
                buffer[i] = children[i];
            }
            return ReactiveUITK.V.Fragment(hostNode.Key, buffer);
        }
    }
}
