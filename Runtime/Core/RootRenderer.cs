using System;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.Elements;
using ReactiveUITK.Core;
using System.Collections.Generic;

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

        // Convenience overload: accept a function component directly
        public void Render(System.Func<Dictionary<string, object>, IReadOnlyList<VirtualNode>, VirtualNode> renderFunction)
        {
            if (renderFunction == null)
            {
                Debug.LogError("RootRenderer: render function is null");
                return;
            }
            Render((VirtualNode)renderFunction);
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
