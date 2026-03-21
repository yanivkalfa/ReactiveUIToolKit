using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ReactiveUITK.Core;
using ReactiveUITK.Core.Fiber;
using UnityEngine;

namespace ReactiveUITK.EditorSupport.HMR
{
    /// <summary>
    /// Walks all active Fiber trees, swaps render delegates for a given component,
    /// and triggers re-renders. State (hooks) is preserved through the swap.
    /// </summary>
    internal static class UitkxHmrDelegateSwapper
    {
        /// <summary>
        /// Extract the Render delegate from a loaded HMR assembly and swap it into
        /// all matching fibers across all active renderers.
        /// </summary>
        /// <returns>Number of fiber instances swapped.</returns>
        public static int SwapAll(Assembly hmrAssembly, string componentName)
        {
            // ── 1. Find the generated type via [UitkxElement] attribute ──────
            var newDelegate = ExtractRenderDelegate(hmrAssembly, componentName);
            if (newDelegate == null)
            {
                Debug.LogWarning(
                    $"[HMR] Could not find Render delegate for '{componentName}' "
                        + "in loaded assembly."
                );
                return 0;
            }

            int total = 0;

            // ── 2. Walk editor renderers ─────────────────────────────────────
            foreach (var renderer in EditorRootRendererUtility.GetAllRenderers())
            {
                var fiberRenderer = renderer.FiberRendererInternal;
                if (fiberRenderer?.Root?.Current == null)
                    continue;
                total += WalkAndSwap(fiberRenderer.Root, componentName, newDelegate);
            }

            // ── 3. Walk runtime renderer ─────────────────────────────────────
            if (RootRenderer.Instance != null)
            {
                var vhr = RootRenderer.Instance.VNodeHostRendererInternal;
                if (vhr?.FiberRendererInternal?.Root?.Current != null)
                    total += WalkAndSwap(
                        vhr.FiberRendererInternal.Root,
                        componentName,
                        newDelegate
                    );
            }

            if (total > 0)
                Debug.Log($"[HMR] Swapped {total} instance(s) of {componentName}");
            else
                Debug.Log($"[HMR] Compiled {componentName} — no active instances to swap.");

            return total;
        }

        // ── Delegate extraction ───────────────────────────────────────────────

        private static Func<IProps, IReadOnlyList<VirtualNode>, VirtualNode> ExtractRenderDelegate(
            Assembly asm,
            string componentName
        )
        {
            Type targetType = null;

            foreach (var type in asm.GetTypes())
            {
                var attr = type.GetCustomAttribute<UitkxElementAttribute>();
                if (attr != null && attr.ComponentName == componentName)
                {
                    targetType = type;
                    break;
                }
            }

            if (targetType == null)
            {
                // Fallback: match by type name
                targetType = asm.GetTypes().FirstOrDefault(t => t.Name == componentName);
            }

            if (targetType == null)
                return null;

            var renderMethod = targetType.GetMethod(
                "Render",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(IProps), typeof(IReadOnlyList<VirtualNode>) },
                null
            );

            if (renderMethod == null)
                return null;

            return (Func<IProps, IReadOnlyList<VirtualNode>, VirtualNode>)
                Delegate.CreateDelegate(
                    typeof(Func<IProps, IReadOnlyList<VirtualNode>, VirtualNode>),
                    renderMethod
                );
        }

        // ── Tree walking ──────────────────────────────────────────────────────

        private static int WalkAndSwap(
            FiberRoot root,
            string componentName,
            Func<IProps, IReadOnlyList<VirtualNode>, VirtualNode> newDelegate
        )
        {
            int count = 0;
            WalkFiber(root.Current, root.Reconciler, componentName, newDelegate, ref count);
            return count;
        }

        private static void WalkFiber(
            FiberNode fiber,
            FiberReconciler reconciler,
            string componentName,
            Func<IProps, IReadOnlyList<VirtualNode>, VirtualNode> newDelegate,
            ref int count
        )
        {
            if (fiber == null)
                return;

            if (fiber.Tag == FiberTag.FunctionComponent && IsMatch(fiber, componentName))
            {
                // Swap the render delegate
                fiber.TypedRender = newDelegate;

                // Trigger re-render via the same mechanism as setState.
                // ComponentState.OnStateUpdated is wired to
                // reconciler.ScheduleUpdateOnFiber(fiber, null).
                try
                {
                    fiber.ComponentState?.OnStateUpdated?.Invoke();
                }
                catch (Exception ex)
                {
                    // Hook count/order mismatch → state is invalid.
                    // Log warning and let the next render cycle re-mount.
                    Debug.LogWarning(
                        $"[HMR] Hook mismatch in {componentName}, state was reset: {ex.Message}"
                    );
                    ResetComponentState(fiber);
                    try
                    {
                        fiber.ComponentState?.OnStateUpdated?.Invoke();
                    }
                    catch
                    { /* best effort */
                    }
                }

                count++;
            }

            // Recurse: first child, then sibling
            WalkFiber(fiber.Child, reconciler, componentName, newDelegate, ref count);
            WalkFiber(fiber.Sibling, reconciler, componentName, newDelegate, ref count);
        }

        private static bool IsMatch(FiberNode fiber, string componentName)
        {
            if (fiber.TypedRender == null)
                return false;

            var declaringType = fiber.TypedRender.Method.DeclaringType;
            if (declaringType == null)
                return false;

            // Direct name match (generated class name == component name)
            if (declaringType.Name == componentName)
                return true;

            // Check [UitkxElement] attribute
            var attr = declaringType.GetCustomAttribute<UitkxElementAttribute>();
            return attr?.ComponentName == componentName;
        }

        private static void ResetComponentState(FiberNode fiber)
        {
            if (fiber.ComponentState == null)
                return;

            // Clear hook states so hooks re-initialize on next render
            fiber.ComponentState.HookStates.Clear();
        }
    }
}
