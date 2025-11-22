using ReactiveUITK.Core.Fiber;
using ReactiveUITK.Core; // For Hooks
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace ReactiveUITK.Tests
{
    /// <summary>
    /// Tests to validate Fiber core functionality
    /// </summary>
    public static class FiberTest
    {
        public static void RunBasicTest(VisualElement container)
        {
            Debug.Log("[FiberTest] Starting basic fiber test...");

            // Test 1: Simple host component
            var vnode = V.Button(new ButtonProps { Text = "Click Me!" });
            var renderer = new FiberRenderer(container);
            renderer.Render(vnode);
            Debug.Log("[FiberTest] ✓ Simple host component rendered");

            // Test 2: Nested elements
            var nested = V.VisualElement(
                elementProperties: null,
                key: null,
                children: new[] {
                    V.Label(new LabelProps { Text = "Hello" }),
                    V.Button(new ButtonProps { Text = "World" })
                }
            );
            renderer.Render(nested);
            Debug.Log("[FiberTest] ✓ Nested elements rendered");

            // Test 3: Function component
            var funcComp = V.Func((props, children) => {
                return V.Button(new ButtonProps { Text = "From Function Component" });
            });
            renderer.Render(funcComp);
            Debug.Log("[FiberTest] ✓ Function component rendered");

            Debug.Log("[FiberTest] All tests passed!");
        }

        public static void RunCounterTest(VisualElement container)
        {
            Debug.Log("[FiberTest] Starting counter test...");

            // Test with stateful function component
            var counterFunc = V.Func((props, children) =>
            {
                var (count, setCount) = Hooks.UseState(0);

                return V.VisualElement(
                    elementProperties: null,
                    key: null,
                    children: new[] {
                        V.Label(new LabelProps { Text = $"Count: {count}" }),
                        V.Button(new ButtonProps { 
                            Text = "Increment", 
                            OnClick = () => setCount(count + 1) 
                        })
                    }
                );
            });

            var renderer = new FiberRenderer(container);
            renderer.Render(counterFunc);
            
            Debug.Log("[FiberTest] ✓ Counter component rendered");
        }
    }
}
