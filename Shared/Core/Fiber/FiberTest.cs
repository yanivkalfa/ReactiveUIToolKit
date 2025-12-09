using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Core.Fiber;
using ReactiveUITK.Elements;

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
                (props, children) =>
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
                (props, children) =>
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
                (props, children) =>
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
                (props, children) =>
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



        private class MockScheduler : IScheduler
        {
            private readonly Queue<Action> _queue = new Queue<Action>();

            public void Enqueue(Action action, IScheduler.Priority priority = IScheduler.Priority.Normal)
            {
                _queue.Enqueue(action);
            }

            public void EnqueueBatchedEffect(Action effect)
            {
                _queue.Enqueue(effect);
            }

            public void BeginBatch() { }
            public void EndBatch() { }

            public void RunAll()
            {
                while (_queue.Count > 0)
                {
                    _queue.Dequeue().Invoke();
                }
            }
        }

        public static void RunDuplicationTest(VisualElement container)
        {
            Debug.Log("[FiberTest] Starting duplication test with MockScheduler...");

            var scheduler = new MockScheduler();
            var hostContext = new HostContext(ElementRegistryProvider.GetDefaultRegistry());
            hostContext.Environment["scheduler"] = scheduler;

            var renderer = new FiberRenderer(container, hostContext);

            Action<int> setValue = null;

            var duplicationComp = V.Func(
                (props, children) =>
                {
                    var (count, setCount) = Hooks.UseState(0);
                    setValue = v => setCount(v);

                    return V.VisualElement(
                        elementProperties: null,
                        key: null,
                        children: new[]
                        {
                            V.Label(new LabelProps { Text = $"Count: {count}" }),
                            V.VisualElement(
                                elementProperties: null,
                                key: null,
                                children: new[]
                                {
                                    V.Label(new LabelProps { Text = "Inner 1" }),
                                    V.Label(new LabelProps { Text = "Inner 2" }),
                                    V.Label(new LabelProps { Text = "Inner 3" })
                                }
                            )
                        }
                    );
                }
            );

            // Initial render
            renderer.Render(duplicationComp);
            scheduler.RunAll();

            // Simulate rapid updates
            for (int i = 0; i < 50; i++)
            {
                setValue?.Invoke(i);
                // In a real scenario, the scheduler might run partially or fully between updates
                // Here we simulate the scheduler running after each update to mimic high frequency
                scheduler.RunAll();
            }

            // Check for duplication
            var root = container;
            int childCount = root.childCount;
            Debug.Log($"[FiberTest] Root child count: {childCount}");

            if (childCount > 0)
            {
                var innerContainer = root[0][1]; // Accessing the inner VisualElement
                int innerChildCount = innerContainer.childCount;
                Debug.Log($"[FiberTest] Inner child count: {innerChildCount}");

                if (innerChildCount > 3)
                {
                    Debug.LogError("[FiberTest] DUPLICATION DETECTED! Inner child count is " + innerChildCount);
                }
                else
                {
                    Debug.Log("[FiberTest] No duplication detected.");
                }
            }
        }

        public static void RunDetachedUpdateTest(VisualElement container)
        {
            Debug.Log("[FiberTest] Starting detached update test...");

            var scheduler = new MockScheduler();
            var hostContext = new HostContext(ElementRegistryProvider.GetDefaultRegistry());
            hostContext.Environment["scheduler"] = scheduler;

            var renderer = new FiberRenderer(container, hostContext);

            Action<int> setDetachedState = null;

            var childComp = V.Func(
                (props, children) =>
                {
                    var (count, setCount) = Hooks.UseState(0);
                    setDetachedState = v => setCount(v);
                    return V.Label(new LabelProps { Text = $"Child {count}" });
                }
            );

            // 1. Render Child
            Debug.Log("[FiberTest] Rendering Child...");
            renderer.Render(childComp);
            scheduler.RunAll();

            Debug.Log($"[FiberTest] Child count after render 1: {container.childCount}"); // Should be 1

            // 2. Replace with Empty
            Debug.Log("[FiberTest] Replacing with Empty...");
            renderer.Render(V.VisualElement(elementProperties: null, key: null, children: Array.Empty<VirtualNode>()));
            scheduler.RunAll();

            Debug.Log($"[FiberTest] Child count after render 2: {container.childCount}"); // Should be 1 (the empty VE) or 0 if we rendered null? 
            // We rendered V.VisualElement, so it should be 1 VE with 0 children.
            // The previous Label should be gone.

            // 3. Trigger update on detached Child
            Debug.Log("[FiberTest] Triggering update on detached Child...");
            setDetachedState?.Invoke(1);
            scheduler.RunAll();

            Debug.Log($"[FiberTest] Child count after detached update: {container.childCount}");

            if (container.childCount > 1)
            {
                Debug.LogError("[FiberTest] DUPLICATION DETECTED! Detached component was re-mounted.");
            }
            else
            {
                Debug.Log("[FiberTest] No duplication detected.");
            }
        }
    }
}
