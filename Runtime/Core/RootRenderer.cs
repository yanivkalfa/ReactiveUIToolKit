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
            EnsureSetup();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            Unmount();
        }

        public void Initialize(VisualElement uiRootElement)
        {
            EnsureSetup();
            rootElement = uiRootElement;
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
