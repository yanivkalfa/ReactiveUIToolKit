using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.Elements;
using ReactiveUITK.Core;

namespace ReactiveUITK.Core
{
    public sealed class RootRenderer : MonoBehaviour
    {
        public static RootRenderer Instance { get; private set; }
        private HostContext sharedHostContext;
        private ElementRegistry elementRegistry;
        private VisualElement rootElement;
        private ReactiveComponent currentMountedComponent;

        private void EnsureSetup()
        {
            if (elementRegistry == null)
            {
                elementRegistry = ElementRegistryProvider.GetDefaultRegistry();
            }
            if (sharedHostContext == null)
            {
                // Ensure a scheduler exists so passive effects and queued updates flush immediately in runtime
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

        public TComponent Render<TComponent>(Dictionary<string, object> componentProps = null) where TComponent : ReactiveComponent
        {
            EnsureSetup();
            if (rootElement == null)
            {
                Debug.LogError("RootRenderer: root not initialized");
                return null;
            }
            if (currentMountedComponent != null)
            {
                currentMountedComponent.Unmount();
                Destroy(currentMountedComponent.gameObject);
                currentMountedComponent = null;
            }
            GameObject componentGameObject = new(typeof(TComponent).Name);
            TComponent componentInstance = componentGameObject.AddComponent<TComponent>();
            if (componentProps != null)
            {
                componentInstance.SetProps(componentProps);
            }
            componentInstance.Mount(rootElement, sharedHostContext);
            currentMountedComponent = componentInstance;
            return componentInstance;
        }

        public void Unmount()
        {
            if (currentMountedComponent == null)
            {
                return;
            }
            currentMountedComponent.Unmount();
            Destroy(currentMountedComponent.gameObject);
            currentMountedComponent = null;
        }
    }
}
