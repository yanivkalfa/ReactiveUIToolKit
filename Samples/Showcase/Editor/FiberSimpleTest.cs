using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using ReactiveUITK.Core;
using ReactiveUITK.Core.Fiber;

namespace ReactiveUITK.Samples.Showcase.Editor
{
    public class FiberSimpleTest : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/Simple Test")]
        public static void ShowWindow()
        {
            GetWindow<FiberSimpleTest>("Fiber Simple");
        }

        private VNodeHostRenderer renderer;

        private void CreateGUI()
        {
            var registry = ReactiveUITK.Elements.ElementRegistryProvider.GetDefaultRegistry();
            renderer = new VNodeHostRenderer(new HostContext(registry), rootVisualElement);
            
            // Directly create a label with simple text
            var label = new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "Label",
                functionRender: null,
                textContent: null,
                key: null,
                properties: new Dictionary<string, object> { { "text", "Hello from Fiber!" } },
                children: System.Array.Empty<VirtualNode>()
            );
            
            renderer.Render(label);
        }

        private void OnDestroy()
        {
            renderer?.Unmount();
        }
    }
}
