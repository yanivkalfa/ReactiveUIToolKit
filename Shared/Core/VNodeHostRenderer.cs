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

        public VNodeHostRenderer(HostContext hostContext, VisualElement host)
        {
            UnityEngine.Debug.Log("[DuplicationTest][VNodeHostRenderer] ctor");
            hostElement = host;
            fiberRenderer = new FiberRenderer(host, hostContext);

            if (FiberConfig.ShowReconcilerInfo)
            {
                UnityEngine.Debug.Log(
                    $"[DuplicationTest][VNodeHostRenderer] Using FIBER reconciler for {host.name}"
                );
            }
        }

        public void Render(VirtualNode vnode)
        {
            UnityEngine.Debug.Log("[DuplicationTest][VNodeHostRenderer] Render");
            fiberRenderer.Render(NormalizeHostRoot(vnode));
        }

        public void Unmount()
        {
            UnityEngine.Debug.Log("[DuplicationTest][VNodeHostRenderer] Unmount");
            ClearHostProps();
            fiberRenderer?.Clear();
        }

        private VirtualNode NormalizeHostRoot(VirtualNode vnode)
        {
            if (vnode == null)
            {
                UnityEngine.Debug.Log("[DuplicationTest][VNodeHostRenderer] Normalize null vnode");
                ClearHostProps();
                return null;
            }

            if (vnode.NodeType != VirtualNodeType.Host)
            {
                UnityEngine.Debug.Log("[DuplicationTest][VNodeHostRenderer] vnode not host");
                ClearHostProps();
                return vnode;
            }

            UnityEngine.Debug.Log("[DuplicationTest][VNodeHostRenderer] vnode host apply props");
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
