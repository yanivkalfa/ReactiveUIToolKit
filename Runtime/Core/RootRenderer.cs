using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
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
            UnityEngine.Debug.Log("[RootRenderer] EnsureSetup start");
            if (elementRegistry == null)
            {
                UnityEngine.Debug.Log("[RootRenderer] Creating ElementRegistry");
                elementRegistry = ElementRegistryProvider.GetDefaultRegistry();
            }
            if (sharedHostContext == null)
            {
                UnityEngine.Debug.Log("[RootRenderer] Creating HostContext");
                if (RenderScheduler.Instance == null)
                {
                    UnityEngine.Debug.Log("[RootRenderer] Creating RenderScheduler");
                    var go = new GameObject("RenderScheduler");
                    go.hideFlags = HideFlags.DontSave;
                    go.AddComponent<RenderScheduler>();
                }
                SignalsRuntime.EnsureInitialized();
                sharedHostContext = new HostContext(elementRegistry);
                sharedHostContext.Environment["scheduler"] = RenderScheduler.Instance;
                sharedHostContext.Environment["isEditor"] = false;

                sharedHostContext.Environment["env"] = BuildDefinesConfig.ResolveEnvironment();
                Reconciler.TraceLevel = BuildDefinesConfig.ResolveTraceLevel();
                Reconciler.EnableDiffTracing = BuildDefinesConfig.ResolveEnableDiffTracing();
                Reconciler.UseExceptionBoundaryFlow =
                    BuildDefinesConfig.ResolveExceptionBoundaryFlow();
            }
            UnityEngine.Debug.Log("[RootRenderer] EnsureSetup end");
        }

        private void Awake()
        {
            UnityEngine.Debug.Log("[RootRenderer] Awake");
            if (Instance != null && Instance != this)
            {
                UnityEngine.Debug.Log("[RootRenderer] Duplicate instance destroyed");
                Destroy(gameObject);
                return;
            }
            Instance = this;
            EnsureSetup();
        }

        private void OnDestroy()
        {
            UnityEngine.Debug.Log("[RootRenderer] OnDestroy");
            if (Instance == this)
            {
                Instance = null;
            }
            Unmount();
        }

        public void Initialize(VisualElement uiRootElement)
        {
            UnityEngine.Debug.Log("[RootRenderer] Initialize");
            EnsureSetup();
            rootElement = uiRootElement;
        }

        public void Render(VirtualNode rootNode)
        {
            UnityEngine.Debug.Log("[RootRenderer] Render invoked");
            EnsureSetup();
            if (rootElement == null)
            {
                UnityEngine.Debug.LogError("RootRenderer: root not initialized");
                return;
            }
            if (vnodeHostRenderer == null)
            {
                UnityEngine.Debug.Log("[RootRenderer] Creating VNodeHostRenderer");
                vnodeHostRenderer = new VNodeHostRenderer(sharedHostContext, rootElement);
            }
            vnodeHostRenderer.Render(rootNode);
        }

        public void Unmount()
        {
            UnityEngine.Debug.Log("[RootRenderer] Unmount");
            if (vnodeHostRenderer != null)
            {
                vnodeHostRenderer.Unmount();
                vnodeHostRenderer = null;
            }
        }
    }
}
