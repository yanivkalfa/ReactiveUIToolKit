using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Elements;
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
                sharedHostContext = new HostContext(elementRegistry);
                sharedHostContext.Environment["scheduler"] = RenderScheduler.Instance;
                sharedHostContext.Environment["isEditor"] = false;
                // Apply build-define derived configuration
                sharedHostContext.Environment["env"] = BuildDefinesConfig.ResolveEnvironment();
                Reconciler.TraceLevel = BuildDefinesConfig.ResolveTraceLevel();
                Reconciler.EnableDiffTracing = BuildDefinesConfig.ResolveEnableDiffTracing();
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

        public void Initialize(VisualElement uiRootElement)
        {
            EnsureSetup();
            rootElement = uiRootElement;
        }

        // Consistent, explicit API: always pass a VirtualNode (from V.Func, V.Fragment, etc.)
        public void Render(VirtualNode rootNode)
        {
            EnsureSetup();
            if (rootElement == null)
            {
                Debug.LogError("RootRenderer: root not initialized");
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
