using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Core.Diagnostics;
using ReactiveUITK.Elements;
using ReactiveUITK.Signals;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Core
{
    public sealed class RootRenderer : MonoBehaviour
    {
        public static RootRenderer Instance { get; private set; }
        private HostContext sharedHostContext;
        private ElementRegistry elementRegistry;
        private VisualElement rootElement;
        private VNodeHostRenderer vnodeHostRenderer;

#if UNITY_EDITOR
        /// <summary>HMR: all active RootRenderer instances for multi-tree walking.</summary>
        private static readonly HashSet<RootRenderer> s_allInstances = new();
        internal static IEnumerable<RootRenderer> AllInstances => s_allInstances;

        /// <summary>HMR: exposes the VNodeHostRenderer for tree walking.</summary>
        internal VNodeHostRenderer VNodeHostRendererInternal => vnodeHostRenderer;
#endif

        private void EnsureSetup()
        {
            if (elementRegistry == null)
            {
                elementRegistry = ElementRegistryProvider.GetDefaultRegistry();
            }
            if (sharedHostContext == null)
            {
                if (RenderScheduler.Instance == null)
                {
                    var go = new GameObject("RenderScheduler");
                    go.hideFlags = HideFlags.DontSave;
                    go.AddComponent<RenderScheduler>();
                }
                SignalsRuntime.EnsureInitialized();
                sharedHostContext = new HostContext(elementRegistry);
                sharedHostContext.Environment["scheduler"] = RenderScheduler.Instance;
                sharedHostContext.Environment["isEditor"] = false;

                sharedHostContext.Environment["env"] = BuildDefinesConfig.ResolveEnvironment();

                // Initialize global diagnostics configuration from build defines.
                DiagnosticsConfig.CurrentTraceLevel = BuildDefinesConfig.ResolveTraceLevel();
                DiagnosticsConfig.EnableDiffTracing = BuildDefinesConfig.ResolveEnableDiffTracing();
                DiagnosticsConfig.UseExceptionBoundaryFlow =
                    BuildDefinesConfig.ResolveExceptionBoundaryFlow();

                // For now, drive internal logs off the verbose trace level.
                InternalLogOptions.EnableInternalLogs =
                    DiagnosticsConfig.CurrentTraceLevel == DiagnosticsConfig.TraceLevel.Verbose;
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
#if UNITY_EDITOR
            s_allInstances.Add(this);
#endif
            EnsureSetup();
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR
            s_allInstances.Remove(this);
#endif
            if (Instance == this)
            {
                Instance = null;
            }
            Unmount();
        }

        /// <summary>
        /// Configures the root VisualElement and optionally seeds named environment slots
        /// (portal targets, feature flags, etc.) into the shared <see cref="HostContext"/>.
        /// Must be called before the first <see cref="Render"/> call.
        /// </summary>
        /// <param name="uiRootElement">The VisualElement that acts as the React root.</param>
        /// <param name="env">
        /// Optional callback invoked with the <see cref="HostContext"/> after built-in keys
        /// (scheduler, env, etc.) are set.  Use this to seed named portal target slots:
        /// <code>
        /// rootRenderer.Initialize(uiDoc.rootVisualElement,
        ///     env: ctx => ctx.Environment[PortalContextKeys.ModalRoot] = overlayLayer);
        /// </code>
        /// </param>
        public void Initialize(VisualElement uiRootElement, Action<HostContext> env = null)
        {
            EnsureSetup();
            rootElement = uiRootElement;
            env?.Invoke(sharedHostContext);
        }

        public void Render(VirtualNode rootNode)
        {
            EnsureSetup();
            if (rootElement == null)
            {
                return;
            }
            if (vnodeHostRenderer == null)
            {
                vnodeHostRenderer = new VNodeHostRenderer(sharedHostContext, rootElement);
            }
            vnodeHostRenderer.Render(rootNode);
        }

        public void Unmount()
        {
            if (vnodeHostRenderer != null)
            {
                vnodeHostRenderer.Unmount();
                vnodeHostRenderer = null;
            }
        }
    }
}
