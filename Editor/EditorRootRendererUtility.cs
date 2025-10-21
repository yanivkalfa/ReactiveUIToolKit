using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using ReactiveUITK.Core;
using ReactiveUITK.Elements;

namespace ReactiveUITK.EditorSupport
{
    public static class EditorRootRendererUtility
    {
        private static readonly Dictionary<VisualElement, VNodeHostRenderer> renderersByHost = new();

        public static void Mount(VisualElement hostElement, VirtualNode root)
        {
            if (hostElement == null || root == null)
            {
                return;
            }
            if (!renderersByHost.TryGetValue(hostElement, out VNodeHostRenderer renderer))
            {
                ElementRegistry registry = ElementRegistryProvider.GetDefaultRegistry();
                HostContext hostContext = new(registry);
                hostContext.Environment["scheduler"] = EditorRenderScheduler.Instance;
                hostContext.Environment["isEditor"] = true;
                renderer = new VNodeHostRenderer(hostContext, hostElement);
                renderersByHost[hostElement] = renderer;
            }
            renderer.Render(root);
        }

        public static void Unmount(VisualElement hostElement)
        {
            if (hostElement == null)
            {
                return;
            }
            if (renderersByHost.TryGetValue(hostElement, out VNodeHostRenderer renderer))
            {
                renderer.Unmount();
                renderersByHost.Remove(hostElement);
            }
        }
    }
}

