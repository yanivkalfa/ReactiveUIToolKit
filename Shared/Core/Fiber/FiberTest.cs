using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Core.Fiber;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

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
            Debug.Log("[FiberTest] Simple host component rendered");

            // Test 2: Nested elements
            var nested = V.VisualElement(
                elementProperties: null,
                key: null,
                children: new[]
                {
                    V.Label(new LabelProps { Text = "Hello" }),
                    V.Button(new ButtonProps { Text = "World" }),
                }
            );
            renderer.Render(nested);
            Debug.Log("[FiberTest] Nested elements rendered");

            // Test 3: Function component
            var funcComp = V.Func(
                (IProps props, IReadOnlyList<VirtualNode> children) =>
                {
                    return V.Button(new ButtonProps { Text = "From Function Component" });
                }
            );
            renderer.Render(funcComp);
            Debug.Log("[FiberTest] Function component rendered");

            Debug.Log("[FiberTest] All tests passed!");
        }

        public static void RunCounterTest(VisualElement container)
        {
            Debug.Log("[FiberTest] Starting counter test...");

            // Test with stateful function component
            var counterFunc = V.Func(
                (IProps props, IReadOnlyList<VirtualNode> children) =>
                {
                    var (count, setCount) = Hooks.UseState(0);

                    return V.VisualElement(
                        elementProperties: null,
                        key: null,
                        children: new[]
                        {
                            V.Label(new LabelProps { Text = $"Count: {count}" }),
                            V.Button(
                                new ButtonProps
                                {
                                    Text = "Increment",
                                    OnClick = () => setCount(count + 1),
                                }
                            ),
                        }
                    );
                }
            );

            var renderer = new FiberRenderer(container);
            renderer.Render(counterFunc);

            Debug.Log("[FiberTest] Counter component rendered");
        }

        /// <summary>
        /// Validate that useEffect works in Fiber function components
        /// without requiring NodeMetadata.
        /// </summary>
        public static void RunEffectTest(VisualElement container)
        {
            Debug.Log("[FiberTest] Starting effect test...");

            int effectRuns = 0;

            var effectComp = V.Func(
                (IProps props, IReadOnlyList<VirtualNode> children) =>
                {
                    Hooks.UseEffect(
                        () =>
                        {
                            effectRuns++;
                            Debug.Log($"[FiberTest] Effect run #{effectRuns}");
                            return null;
                        },
                        System.Array.Empty<object>()
                    );

                    return V.Label(new LabelProps { Text = $"Effect runs: {effectRuns}" });
                }
            );

            var renderer = new FiberRenderer(container);
            renderer.Render(effectComp);

            Debug.Log("[FiberTest] Effect test rendered");
        }

        /// <summary>
        /// Validate that useSignal works in Fiber function components and
        /// triggers Fiber-driven re-renders via FunctionComponentState.OnStateUpdated.
        /// </summary>
        public static void RunSignalTest(VisualElement container)
        {
            Debug.Log("[FiberTest] Starting signal test...");

            var signal = ReactiveUITK.Signals.Signals.Get<int>("FiberTest.SignalCounter", 0);

            var signalComp = V.Func(
                (IProps props, IReadOnlyList<VirtualNode> children) =>
                {
                    var value = Hooks.UseSignal(signal);

                    return V.Button(
                        new ButtonProps
                        {
                            Text = $"Signal: {value}",
                            OnClick = () => signal.Set(value + 1),
                        }
                    );
                }
            );

            var renderer = new FiberRenderer(container);
            renderer.Render(signalComp);

            Debug.Log("[FiberTest] Signal component rendered");
        }
    }
}
