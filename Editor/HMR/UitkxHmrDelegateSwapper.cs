using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ReactiveUITK;
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
        public static int SwapAll(Assembly hmrAssembly, string componentName, string uitkxFilePath = null)
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
                total += WalkAndSwap(fiberRenderer.Root, componentName, newDelegate, uitkxFilePath);
            }

            // ── 3. Walk runtime renderers ─────────────────────────────────
            foreach (var rootRenderer in RootRenderer.AllInstances)
            {
                var vhr = rootRenderer.VNodeHostRendererInternal;
                if (vhr?.FiberRendererInternal?.Root?.Current != null)
                    total += WalkAndSwap(
                        vhr.FiberRendererInternal.Root,
                        componentName,
                        newDelegate,
                        uitkxFilePath
                    );
            }

            if (total > 0)
                Debug.Log($"[HMR] Swapped {total} instance(s) of {componentName}");
            else
                Debug.Log($"[HMR] Compiled {componentName} — no active instances to swap.");

            return total;
        }

        /// <summary>
        /// Swaps hook delegate fields in the project assembly to point at the
        /// new body methods from the HMR-compiled assembly, then triggers a
        /// global re-render on all active fiber trees.
        /// </summary>
        /// <returns>Number of hooks swapped.</returns>
        public static int SwapHooks(
            Assembly hmrAssembly,
            string containerClassName,
            string ns)
        {
            // ── 1. Find the container class in the project assemblies ────────
            string fullName = string.IsNullOrEmpty(ns)
                ? containerClassName
                : $"{ns}.{containerClassName}";

            Type projectContainer = null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm.IsDynamic)
                    continue;
                projectContainer = asm.GetType(fullName);
                if (projectContainer != null)
                    break;
            }

            if (projectContainer == null)
            {
                Debug.LogWarning(
                    $"[HMR] Could not find hook container '{fullName}' in loaded assemblies.");
                return 0;
            }

            // ── 2. Find the HMR container class with new body methods ────────
            Type hmrContainer = hmrAssembly.GetType(fullName);
            if (hmrContainer == null)
            {
                Debug.LogWarning(
                    $"[HMR] Could not find hook container '{fullName}' in HMR assembly.");
                return 0;
            }

            // ── 3. Swap each __hmr_* field ───────────────────────────────────
            int swapped = 0;
            var bindingFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

            foreach (var field in projectContainer.GetFields(bindingFlags))
            {
                if (!field.Name.StartsWith("__hmr_"))
                    continue;
                if (field.Name.EndsWith("_cache"))
                    continue;

                string hookName = field.Name.Substring("__hmr_".Length);
                string bodyMethodName = $"__{hookName}_body";

                if (field.FieldType == typeof(MethodInfo))
                {
                    // ── Generic hook: swap MethodInfo + clear cache ──────
                    var newMethod = hmrContainer.GetMethod(
                        bodyMethodName, bindingFlags);
                    if (newMethod != null)
                    {
                        field.SetValue(null, newMethod);
                        swapped++;

                        var cacheField = projectContainer.GetField(
                            field.Name + "_cache", bindingFlags);
                        if (cacheField?.GetValue(null) is System.Collections.IDictionary dict)
                            dict.Clear();
                    }
                }
                else if (typeof(Delegate).IsAssignableFrom(field.FieldType))
                {
                    // ── Non-generic hook: swap Func/Action delegate ──────
                    var newMethod = hmrContainer.GetMethod(
                        bodyMethodName, bindingFlags);
                    if (newMethod != null)
                    {
                        try
                        {
                            var newDel = Delegate.CreateDelegate(
                                field.FieldType, newMethod);
                            field.SetValue(null, newDel);
                            swapped++;
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning(
                                $"[HMR] Failed to swap hook '{hookName}': {ex.Message}");
                        }
                    }
                }
            }

            // ── 4. Trigger global re-render ──────────────────────────────────
            if (swapped > 0)
                TriggerGlobalReRender();

            return swapped;
        }

        /// <summary>
        /// Triggers a re-render on all active fiber trees so components pick up
        /// new hook implementations. Used by hook HMR since any component might
        /// call the changed hook.
        /// </summary>
        private static void TriggerGlobalReRender()
        {
            foreach (var renderer in EditorRootRendererUtility.GetAllRenderers())
            {
                var fiberRenderer = renderer.FiberRendererInternal;
                if (fiberRenderer?.Root?.Current == null)
                    continue;
                ScheduleFullTreeUpdate(fiberRenderer.Root.Current);
            }

            foreach (var rootRenderer in RootRenderer.AllInstances)
            {
                var vhr = rootRenderer.VNodeHostRendererInternal;
                if (vhr?.FiberRendererInternal?.Root?.Current == null)
                    continue;
                ScheduleFullTreeUpdate(vhr.FiberRendererInternal.Root.Current);
            }
        }

        private static void ScheduleFullTreeUpdate(FiberNode fiber)
        {
            if (fiber == null)
                return;
            if (fiber.Tag == FiberTag.FunctionComponent)
            {
                try { fiber.ComponentState?.OnStateUpdated?.Invoke(); }
                catch { /* best effort */ }
            }
            ScheduleFullTreeUpdate(fiber.Child);
            ScheduleFullTreeUpdate(fiber.Sibling);
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
            Func<IProps, IReadOnlyList<VirtualNode>, VirtualNode> newDelegate,
            string uitkxFilePath
        )
        {
            int count = 0;
            WalkFiber(root.Current, root.Reconciler, componentName, newDelegate, uitkxFilePath, ref count);
            return count;
        }

        private static void WalkFiber(
            FiberNode fiber,
            FiberReconciler reconciler,
            string componentName,
            Func<IProps, IReadOnlyList<VirtualNode>, VirtualNode> newDelegate,
            string uitkxFilePath,
            ref int count
        )
        {
            if (fiber == null)
                return;

            if (fiber.Tag == FiberTag.FunctionComponent && IsMatch(fiber, componentName, uitkxFilePath))
            {
                // ── Proactive hook signature check ───────────────────────
                // Compare [HookSignature] attributes from old and new types.
                // A mismatch means hooks were added/removed/reordered — reset
                // all state BEFORE render to avoid silent corruption.
                bool signatureChanged = HasHookSignatureChanged(
                    fiber.TypedRender?.Method?.DeclaringType,
                    newDelegate.Method.DeclaringType
                );

                if (signatureChanged)
                {
                    Debug.Log(
                        $"[HMR] Hook signature changed in {componentName} — resetting state"
                    );
                    FullResetComponentState(fiber);
                }

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
                    FullResetComponentState(fiber);
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
            WalkFiber(fiber.Child, reconciler, componentName, newDelegate, uitkxFilePath, ref count);
            WalkFiber(fiber.Sibling, reconciler, componentName, newDelegate, uitkxFilePath, ref count);
        }

        private static bool IsMatch(FiberNode fiber, string componentName, string uitkxFilePath)
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
            if (attr?.ComponentName == componentName)
                return true;

            // File-path fallback: after a rename, the class name has changed but
            // the source file path in [UitkxSource] stays stable.  Use it to
            // identify the component even when the name no longer matches.
            if (uitkxFilePath != null)
            {
                var sourceAttr = declaringType.GetCustomAttribute<UitkxSourceAttribute>();
                if (sourceAttr != null
                    && string.Equals(sourceAttr.SourcePath, uitkxFilePath,
                        StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static bool HasHookSignatureChanged(Type oldType, Type newType)
        {
            if (oldType == null || newType == null)
                return false;

            var oldAttr = oldType.GetCustomAttribute<HookSignatureAttribute>();
            var newAttr = newType.GetCustomAttribute<HookSignatureAttribute>();

            // If either type lacks the attribute (pre-upgrade code), skip check
            if (oldAttr == null || newAttr == null)
                return false;

            return !string.Equals(oldAttr.Signature, newAttr.Signature, StringComparison.Ordinal);
        }

        /// <summary>
        /// Comprehensive component state reset: runs effect cleanups, clears all
        /// hook state, queued updates, caches, and context dependencies.
        /// </summary>
        private static void FullResetComponentState(FiberNode fiber)
        {
            var state = fiber.ComponentState;
            if (state == null)
                return;

            // Run effect cleanups before clearing (mirrors unmount flow in FiberReconciler)
            if (state.FunctionEffects != null)
            {
                for (int i = 0; i < state.FunctionEffects.Count; i++)
                {
                    try
                    {
                        state.FunctionEffects[i].cleanup?.Invoke();
                    }
                    catch { }
                }
                state.FunctionEffects.Clear();
            }

            if (state.FunctionLayoutEffects != null)
            {
                for (int i = 0; i < state.FunctionLayoutEffects.Count; i++)
                {
                    try
                    {
                        state.FunctionLayoutEffects[i].cleanup?.Invoke();
                    }
                    catch { }
                }
                state.FunctionLayoutEffects.Clear();
            }

            // Dispose signal subscriptions before clearing hook states
            Hooks.DisposeSignalSubscriptions(state);

            // Clear hook values so hooks re-initialize on next render
            state.HookStates.Clear();

            // Reset hook validation state
            state.HookOrderSignatures?.Clear();
            state.HookOrderPrimed = false;

            // Clear queued state updates and caches
            state.HookStateQueues?.Clear();
            state.PendingHookStatePreviews?.Clear();
            state.StateSetterDelegateCache?.Clear();

            // Clear context dependency tracking
            state.ContextDependencies?.Clear();
        }
    }
}
