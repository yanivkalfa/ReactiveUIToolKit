using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Elements;
using ReactiveUITK.Signals;
using UnityEditor;
using UnityEngine.UIElements;

namespace ReactiveUITK.EditorSupport
{
    public static class EditorRootRendererUtility
    {
        private static readonly Dictionary<VisualElement, VNodeHostRenderer> renderersByHost =
            new();
        private static readonly Dictionary<VisualElement, Action> commitHandlers = new();

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
                SignalsRuntime.EnsureInitialized();

                hostContext.Environment["env"] = BuildDefinesConfig.ResolveEnvironment();
                Reconciler.TraceLevel = BuildDefinesConfig.ResolveTraceLevel();
                Reconciler.EnableDiffTracing = BuildDefinesConfig.ResolveEnableDiffTracing();
                Reconciler.UseExceptionBoundaryFlow =
                    BuildDefinesConfig.ResolveExceptionBoundaryFlow();
                renderer = new VNodeHostRenderer(hostContext, hostElement);
                if (commitHandlers.TryGetValue(hostElement, out var handlers) && handlers != null)
                {
                    renderer.OnCommit += handlers;
                }
                renderersByHost[hostElement] = renderer;
            }
            renderer.Render(root);
        }

        public static void Render(VisualElement hostElement, VirtualNode root)
        {
            Mount(hostElement, root);
        }

        public static void Unmount(VisualElement hostElement)
        {
            if (hostElement == null)
            {
                return;
            }
            if (renderersByHost.TryGetValue(hostElement, out VNodeHostRenderer renderer))
            {
                if (commitHandlers.TryGetValue(hostElement, out var handlers) && handlers != null)
                {
                    renderer.OnCommit -= handlers;
                }
                renderer.Unmount();
                renderersByHost.Remove(hostElement);
                commitHandlers.Remove(hostElement);
            }
        }

        public static void RegisterOnCommit(VisualElement hostElement, Action callback)
        {
            if (hostElement == null || callback == null)
            {
                return;
            }

            if (commitHandlers.TryGetValue(hostElement, out var handlers))
            {
                handlers += callback;
                commitHandlers[hostElement] = handlers;
            }
            else
            {
                commitHandlers[hostElement] = callback;
            }

            if (renderersByHost.TryGetValue(hostElement, out var renderer))
            {
                renderer.OnCommit += callback;
            }
        }

        public static void UnregisterOnCommit(VisualElement hostElement, Action callback)
        {
            if (hostElement == null || callback == null)
            {
                return;
            }

            if (!commitHandlers.TryGetValue(hostElement, out var handlers) || handlers == null)
            {
                return;
            }

            handlers -= callback;

            if (handlers == null)
            {
                commitHandlers.Remove(hostElement);
            }
            else
            {
                commitHandlers[hostElement] = handlers;
            }

            if (renderersByHost.TryGetValue(hostElement, out var renderer))
            {
                renderer.OnCommit -= callback;
            }
        }
    }
}
