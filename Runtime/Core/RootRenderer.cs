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
        private HostContext hostContext;
        private ElementRegistry registry;
        private VisualElement root;
        private ReactiveComponent mountedComponent;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            registry = new ElementRegistry();
            registry.Register("VisualElement", new VisualElementAdapter());
            hostContext = new HostContext(registry);
        }

        public void Initialize(VisualElement rootElement)
        {
            root = rootElement;
        }

        public TComponent Render<TComponent>(Dictionary<string, object> props = null) where TComponent : ReactiveComponent
        {
            if (root == null)
            {
                Debug.LogError("RootRenderer: root not initialized");
                return null;
            }

            if (mountedComponent != null)
            {
                mountedComponent.Unmount();
                Destroy(mountedComponent.gameObject);
                mountedComponent = null;
            }

            GameObject go = new GameObject(typeof(TComponent).Name);
            var comp = go.AddComponent<TComponent>();
            if (props != null) comp.SetProps(props);
            comp.Mount(root, hostContext);
            mountedComponent = comp;
            return comp;
        }

        public void Unmount()
        {
            if (mountedComponent == null) return;
            mountedComponent.Unmount();
            Destroy(mountedComponent.gameObject);
            mountedComponent = null;
        }
    }
}
