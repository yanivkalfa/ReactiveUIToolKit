using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using ReactiveUITK.Core;
using ReactiveUITK.Core.Fiber;
using ReactiveUITK.Props.Typed;

namespace ReactiveUITK.Samples.Showcase.Editor
{
    public class FiberMultiComponentDemo : EditorWindow
    {
        [MenuItem("ReactiveUITK/Fiber/Multi-Component Demo")]
        public static void ShowWindow()
        {
            GetWindow<FiberMultiComponentDemo>("Fiber Multi Demo");
        }

        private VNodeHostRenderer renderer;

        private void CreateGUI()
        {
            // Ensure Fiber is enabled
            FiberConfig.UseFiberReconciler = true;
            
            var registry = ReactiveUITK.Elements.ElementRegistryProvider.GetDefaultRegistry();
            renderer = new VNodeHostRenderer(new HostContext(registry), rootVisualElement);
            renderer.Render(V.Func(MultiComponentApp));
        }

        private void OnDestroy()
        {
            renderer?.Unmount();
        }

        private static VirtualNode MultiComponentApp(Dictionary<string, object> props, IReadOnlyList<VirtualNode> children)
        {
            return V.VisualElement(null, null,
                V.Label(new LabelProps { Text = "Multi-Component Demo (Fiber)" }),
                V.VisualElement(new Dictionary<string, object> { { "style", "flex-direction: row; margin-top: 10px;" } }, null,
                    V.Func(Counter, new Dictionary<string, object> { { "id", 1 } }),
                    V.Func(Counter, new Dictionary<string, object> { { "id", 2 } })
                ),
                V.Func(StatusPanel)
            );
        }

        private static VirtualNode Counter(Dictionary<string, object> props, IReadOnlyList<VirtualNode> children)
        {
            var id = props["id"];
            var (count, setCount) = Hooks.UseState(0);

            var containerStyle = new Dictionary<string, object> 
            { 
                { "style", "border-width: 1px; border-color: #666; margin: 5px; padding: 5px;" } 
            };

            return V.VisualElement(containerStyle, null,
                V.Label(new LabelProps { Text = $"Counter {id}: {count}" }),
                V.Button(new ButtonProps 
                { 
                    Text = "Increment", 
                    OnClick = () => 
                    {
                        UnityEngine.Debug.Log($"[DEMO] Button clicked for Counter {id}!");
                        setCount(15);
                    }
                })
            );
        }

        private static VirtualNode StatusPanel(Dictionary<string, object> props, IReadOnlyList<VirtualNode> children)
        {
             return V.VisualElement(new Dictionary<string, object> { { "style", "margin-top: 20px; color: #888;" } }, null,
                 V.Label(new LabelProps { Text = "Status: Active (Fiber Reconciler)" })
             );
        }
    }
}
