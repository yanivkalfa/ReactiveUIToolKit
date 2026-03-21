using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Core.Diagnostics;
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

        /// <summary>HMR: enumerates all active VNodeHostRenderers for fiber tree walking.</summary>
        internal static IEnumerable<VNodeHostRenderer> GetAllRenderers() => renderersByHost.Values;

        /// <summary>
        /// Mounts a component tree on <paramref name="hostElement"/>.
        /// </summary>
        /// <param name="hostElement">The VisualElement that acts as the React root.</param>
        /// <param name="root">The root VirtualNode to render.</param>
        /// <param name="env">
        /// Optional callback invoked with the freshly-created <see cref="HostContext"/> before
        /// the renderer is started.  Use this to seed named portal target slots:
        /// <code>
        /// env: ctx => ctx.Environment[PortalContextKeys.ModalRoot] = myOverlayPanel
        /// </code>
        /// The callback is only called when a <b>new</b> renderer is created for this host
        /// element; subsequent <c>Mount</c>/<c>Render</c> calls on the same host are no-ops
        /// for context setup (the context is shared for the renderer's lifetime).
        /// </param>
        public static void Mount(
            VisualElement hostElement,
            VirtualNode root,
            Action<HostContext> env = null
        )
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

                DiagnosticsConfig.CurrentTraceLevel = BuildDefinesConfig.ResolveTraceLevel();
                DiagnosticsConfig.EnableDiffTracing = BuildDefinesConfig.ResolveEnableDiffTracing();
                DiagnosticsConfig.UseExceptionBoundaryFlow =
                    BuildDefinesConfig.ResolveExceptionBoundaryFlow();

                InternalLogOptions.EnableInternalLogs =
                    DiagnosticsConfig.CurrentTraceLevel == DiagnosticsConfig.TraceLevel.Verbose;

                // Caller-supplied environment seeding (portal slots, feature flags, etc.)
                env?.Invoke(hostContext);

                renderer = new VNodeHostRenderer(hostContext, hostElement);
                renderersByHost[hostElement] = renderer;
            }
            renderer.Render(root);
        }

        /// <inheritdoc cref="Mount"/>
        public static void Render(
            VisualElement hostElement,
            VirtualNode root,
            Action<HostContext> env = null
        )
        {
            Mount(hostElement, root, env);
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
