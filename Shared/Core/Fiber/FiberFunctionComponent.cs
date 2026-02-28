using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Core.Diagnostics;

namespace ReactiveUITK.Core.Fiber
{
    /// <summary>
    /// Function component support for Fiber reconciler
    /// Handles rendering, hooks, and effects
    /// </summary>
    public static class FiberFunctionComponent
    {
        // Guards against infinite render loops (e.g. setState called unconditionally during render).
        // ThreadStatic ensures each thread keeps its own independent depth counter.
        [ThreadStatic]
        private static int s_renderDepth;
        private const int MaxRenderDepth = 25;

        /// <summary>
        /// Render a function component and return the child fiber
        /// </summary>
        public static FiberNode RenderFunctionComponent(
            FiberNode wipFiber,
            HostContext hostContext,
            FiberReconciler reconciler
        )
        {
            if (wipFiber.Render == null)
            {
                return null;
            }

            // Get or create component state
            var componentState = wipFiber.ComponentState ?? new FunctionComponentState(null);
            componentState.HostContext = hostContext;
            componentState.Fiber = wipFiber;
            wipFiber.ComponentState = componentState;

            // Apply any queued state updates before rendering
            Hooks.FlushQueuedStateUpdates(componentState);

            // Reset hook indices
            componentState.HookIndex = 0;
            componentState.EffectIndex = 0;
            componentState.LayoutEffectIndex = 0;

            var componentName =
                wipFiber.ElementType ?? wipFiber.Render?.Method.DeclaringType?.Name ?? "Unknown";

            // Wire up state updates to Fiber reconciler
            // CRITICAL FIX: Use componentState.Fiber (kept current by UpdateComponentStateReferences)
            // instead of capturing wipFiber, which becomes stale after tree swap in CommitRoot.
            componentState.OnStateUpdated = () =>
                reconciler.ScheduleUpdateOnFiber(componentState.Fiber, null);

            // Log render attempt
            var propsEqual = ArePropsEqual(wipFiber.PendingProps, wipFiber.Props);
            bool contextUnchanged = !wipFiber.ReadsContext || !Hooks.HasContextChanged(wipFiber);

            // Bailout check: if no state update and props match AND context unchanged, we can skip rendering
            if (!wipFiber.HasPendingStateUpdate && contextUnchanged && propsEqual)
            {
                // If the subtree has updates, we still need to clone the children but skip *this* component's render logic
                if (wipFiber.SubtreeHasUpdates)
                {
                    FiberNode newChild = FiberChildReconciliation.CloneChildFibers(wipFiber);
                    return newChild;
                }
                // Commit props so the next render cycle sees matching props for ArePropsEqual
                wipFiber.Props = wipFiber.PendingProps;

                // CRITICAL FIX: We must carry over the existing child pointer to the WIP tree
                // even if we don't visit it. Otherwise, this branch is severed in the new tree.
                if (wipFiber.Alternate != null)
                {
                    wipFiber.Child = wipFiber.Alternate.Child;

                    // CRITICAL FIX: We must update the child's Parent pointer to point to this new WIP fiber!
                    // Otherwise, the child remains attached to the OLD parent (Alternate), breaking the update chain.
                    if (wipFiber.Child != null)
                    {
                        var child = wipFiber.Child;
                        while (child != null)
                        {
                            child.Parent = wipFiber;
                            child = child.Sibling;
                        }
                    }
                }

                componentState.IsRendering = false;
                HookContext.Current = null;
                return null;
            }
            else
            {
                // Clear HasPendingStateUpdate now that we've read it
                // (SubtreeHasUpdates stays for reconciliation, cleared after commit)
                if (wipFiber.HasPendingStateUpdate)
                {
                    wipFiber.HasPendingStateUpdate = false;
                }
            }

            // Set hook context
            HookContext.Current = componentState;
            componentState.IsRendering = true;

            // NOW clear context deps right before render — UseContext will rebuild them
            if (componentState.ContextDependencies != null)
            {
                componentState.ContextDependencies.Clear();
            }

            VirtualNode childVNode = null;

            s_renderDepth++;
            try
            {
                // Guard against infinite render loops caused by unconditional setState during render
                if (s_renderDepth > MaxRenderDepth)
                {
                    UnityEngine.Debug.LogError(
                        $"[Fiber] Maximum render depth ({MaxRenderDepth}) exceeded in '{componentName}'. "
                        + "A component may be calling setState unconditionally during render."
                    );
                    return null;
                }

                // Call the render function
                var propsDict =
                    wipFiber.PendingProps as Dictionary<string, object>
                    ?? new Dictionary<string, object>(
                        wipFiber.PendingProps ?? new Dictionary<string, object>()
                    );

                childVNode = wipFiber.Render(propsDict, wipFiber.Children);

                // Store rendered vnode
                wipFiber.LastRenderedVNode = childVNode;
            }
            finally
            {
                s_renderDepth--;
                componentState.IsRendering = false;
                HookContext.Current = null;
            }

            // Mark effect flags so the commit phase can run effects.
            if (
                componentState.FunctionLayoutEffects != null
                && componentState.FunctionLayoutEffects.Count > 0
            )
            {
                wipFiber.EffectTag |= EffectFlags.LayoutEffect;
            }

            if (componentState.FunctionEffects != null && componentState.FunctionEffects.Count > 0)
            {
                wipFiber.EffectTag |= EffectFlags.PassiveEffect;
            }

            // Reconcile single child
            if (childVNode != null)
            {
                var currentChild = wipFiber.Alternate?.Child;
                var newChild = ReconcileSingleChild(wipFiber, currentChild, childVNode);
                wipFiber.Child = newChild;
                return newChild;
            }

            // If the function component now returns null, ensure any
            // previously rendered child fibers are marked for deletion.
            var existingChild = wipFiber.Alternate?.Child;
            if (existingChild != null)
            {
                var child = existingChild;
                while (child != null)
                {
                    DeleteChild(wipFiber, child);
                    child = child.Sibling;
                }
            }

            wipFiber.Child = null;
            return null;
        }

        /// <summary>
        /// Reconcile a single child (function components return single root)
        /// </summary>
        private static FiberNode ReconcileSingleChild(
            FiberNode parent,
            FiberNode currentChild,
            VirtualNode newVNode
        )
        {
            // Try to reuse existing fiber
            if (currentChild != null && CanReuseFiber(currentChild, newVNode))
            {
                // Use centralized factory for consistent flag propagation
                var clone = FiberFactory.CloneForReuse(currentChild, newVNode);
                clone.Parent = parent;
                clone.Index = 0;

                // Delete remaining siblings
                var sibling = currentChild.Sibling;
                while (sibling != null)
                {
                    DeleteChild(parent, sibling);
                    sibling = sibling.Sibling;
                }

                return clone;
            }

            // Can't reuse, create new fiber
            var newFiber = CreateFiber(newVNode, parent, 0);

            // Delete all old children
            if (currentChild != null)
            {
                var child = currentChild;
                while (child != null)
                {
                    DeleteChild(parent, child);
                    child = child.Sibling;
                }
            }

            return newFiber;
        }

        /// <summary>
        /// Check if fiber can be reused for vnode
        /// </summary>
        private static bool CanReuseFiber(FiberNode fiber, VirtualNode vnode)
        {
            if (fiber == null || vnode == null)
                return false;

            switch (vnode.NodeType)
            {
                case VirtualNodeType.Element:
                    return fiber.Tag == FiberTag.HostComponent
                        && fiber.ElementType == vnode.ElementTypeName;

                case VirtualNodeType.Text:
                    // Text nodes are modeled as host "Label" elements.
                    // Reuse when the host element type matches.
                    return fiber.Tag == FiberTag.HostComponent && fiber.ElementType == "Label";

                case VirtualNodeType.FunctionComponent:
                    if (fiber.Tag != FiberTag.FunctionComponent)
                        return false;

                    // Check delegate equality
                    if (fiber.Render == vnode.FunctionRender)
                        return true;

                    // Handle method group conversion (creates new delegate instance)
                    if (fiber.Render != null && vnode.FunctionRender != null)
                    {
                        return fiber.Render.Method == vnode.FunctionRender.Method
                            && fiber.Render.Target == vnode.FunctionRender.Target;
                    }
                    return false;

                case VirtualNodeType.Suspense:
                    return fiber.Tag == FiberTag.FunctionComponent
                        && fiber.Render == FiberIntrinsicComponents.SuspenseRender;

                case VirtualNodeType.Portal:
                    return fiber.Tag == FiberTag.HostPortal;

                case VirtualNodeType.ErrorBoundary:
                    return fiber.Tag == FiberTag.ErrorBoundary;

                case VirtualNodeType.Fragment:
                    return fiber.Tag == FiberTag.Fragment;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Create new fiber from vnode
        /// </summary>
        private static FiberNode CreateFiber(VirtualNode vnode, FiberNode parent, int index)
        {
            // Use centralized factory for consistent flag management
            return FiberFactory.CreateNew(vnode, parent, index);
        }

        /// <summary>
        /// Mark fiber for deletion
        /// </summary>
        private static void DeleteChild(FiberNode parentFiber, FiberNode childFiber)
        {
            if (parentFiber.Deletions == null)
            {
                parentFiber.Deletions = new List<FiberNode>();
            }
            childFiber.EffectTag |= EffectFlags.Deletion;
            parentFiber.Deletions.Add(childFiber);
        }

        /// <summary>
        /// Extract props from vnode
        /// </summary>
        private static IReadOnlyDictionary<string, object> ExtractProps(VirtualNode vnode)
        {
            if (vnode == null)
            {
                return new Dictionary<string, object>();
            }

            switch (vnode.NodeType)
            {
                case VirtualNodeType.Suspense:
                    return FiberIntrinsicComponents.CreateSuspenseProps(vnode);

                case VirtualNodeType.Text:
                    return new Dictionary<string, object>
                    {
                        { "text", vnode.TextContent ?? string.Empty },
                    };

                default:
                    return vnode.Properties ?? new Dictionary<string, object>();
            }
        }

        /// <summary>
        /// Commit layout effects for a function component
        /// </summary>
        public static void CommitLayoutEffects(FiberNode fiber)
        {
            var componentState = fiber.ComponentState;
            if (componentState?.FunctionLayoutEffects == null)
                return;

            for (int i = 0; i < componentState.FunctionLayoutEffects.Count; i++)
            {
                var effect = componentState.FunctionLayoutEffects[i];
                bool shouldRun =
                    effect.lastDeps == null || DepsChanged(effect.lastDeps, effect.deps);

                if (shouldRun)
                {
                    // Cleanup previous
                    try
                    {
                        effect.cleanup?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError($"Layout effect cleanup error: {ex}");
                    }

                    // Run new effect
                    Action newCleanup = null;
                    try
                    {
                        newCleanup = effect.factory?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError($"Layout effect error: {ex}");
                    }

                    // Update effect entry
                    if (i < componentState.FunctionLayoutEffects.Count)
                    {
                        componentState.FunctionLayoutEffects[i] = (
                            effect.factory,
                            effect.deps,
                            (object[])effect.deps?.Clone(),
                            newCleanup
                        );
                    }
                }
            }
        }

        /// <summary>
        /// Schedule passive effects for a function component
        /// </summary>
        public static void SchedulePassiveEffects(FiberNode fiber)
        {
            var componentState = fiber.ComponentState;
            if (componentState?.FunctionEffects == null)
                return;

            // For now, run effects immediately (React schedules these)
            for (int i = 0; i < componentState.FunctionEffects.Count; i++)
            {
                var effect = componentState.FunctionEffects[i];
                bool shouldRun =
                    effect.lastDeps == null || DepsChanged(effect.lastDeps, effect.deps);

                if (shouldRun)
                {
                    int capturedIndex = i;
                    var capturedEffect = effect;

                    // Schedule effect run
                    ScheduleEffect(() =>
                    {
                        // Cleanup previous
                        try
                        {
                            capturedEffect.cleanup?.Invoke();
                        }
                        catch (Exception ex)
                        {
                            UnityEngine.Debug.LogError($"Effect cleanup error: {ex}");
                        }

                        // Run new effect
                        Action newCleanup = null;
                        try
                        {
                            newCleanup = capturedEffect.factory?.Invoke();
                        }
                        catch (Exception ex)
                        {
                            UnityEngine.Debug.LogError($"Effect error: {ex}");
                        }

                        // Update effect entry
                        if (capturedIndex < componentState.FunctionEffects.Count)
                        {
                            componentState.FunctionEffects[capturedIndex] = (
                                capturedEffect.factory,
                                capturedEffect.deps,
                                (object[])capturedEffect.deps?.Clone(),
                                newCleanup
                            );
                        }
                    });
                }
            }
        }

        /// <summary>
        /// Pass 1 of 2 for passive-effect flushing: run only the cleanup of each dirty effect.
        /// Must be called for ALL committed fibers before RunPassiveEffectSetups is called for any.
        /// </summary>
        public static void RunPassiveEffectCleanups(FiberNode fiber)
        {
            var componentState = fiber.ComponentState;
            if (componentState?.FunctionEffects == null)
                return;

            for (int i = 0; i < componentState.FunctionEffects.Count; i++)
            {
                var effect = componentState.FunctionEffects[i];
                bool shouldRun = effect.lastDeps == null || DepsChanged(effect.lastDeps, effect.deps);
                if (!shouldRun || effect.cleanup == null)
                    continue;

                try
                {
                    effect.cleanup.Invoke();
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"Effect cleanup error: {ex}");
                }

                // Clear the cleanup reference so it cannot fire twice, but preserve lastDeps
                // so RunPassiveEffectSetups can re-evaluate shouldRun identically.
                componentState.FunctionEffects[i] = (
                    effect.factory,
                    effect.deps,
                    effect.lastDeps,
                    null
                );
            }
        }

        /// <summary>
        /// Pass 2 of 2 for passive-effect flushing: run the setup of each dirty effect and store the new cleanup.
        /// Must be called after RunPassiveEffectCleanups has been called for ALL committed fibers.
        /// </summary>
        public static void RunPassiveEffectSetups(FiberNode fiber)
        {
            var componentState = fiber.ComponentState;
            if (componentState?.FunctionEffects == null)
                return;

            for (int i = 0; i < componentState.FunctionEffects.Count; i++)
            {
                var effect = componentState.FunctionEffects[i];
                bool shouldRun = effect.lastDeps == null || DepsChanged(effect.lastDeps, effect.deps);
                if (!shouldRun)
                    continue;

                Action newCleanup = null;
                try
                {
                    newCleanup = effect.factory?.Invoke();
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"Effect error: {ex}");
                }

                componentState.FunctionEffects[i] = (
                    effect.factory,
                    effect.deps,
                    (object[])effect.deps?.Clone(),
                    newCleanup
                );
            }
        }

        /// <summary>
        /// Check if dependencies changed
        /// </summary>
        private static bool DepsChanged(object[] oldDeps, object[] newDeps)
        {
            if (oldDeps == null || newDeps == null)
                return true;

            if (oldDeps.Length != newDeps.Length)
                return true;

            for (int i = 0; i < oldDeps.Length; i++)
            {
                if (!object.Equals(oldDeps[i], newDeps[i]))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Schedule an effect to run (for now, runs immediately)
        /// </summary>
        private static void ScheduleEffect(Action effect)
        {
            // TODO: Use proper scheduler
            effect?.Invoke();
        }

        /// <summary>
        /// Check if props are equal (shallow comparison)
        /// </summary>
        private static bool ArePropsEqual(
            IReadOnlyDictionary<string, object> props1,
            IReadOnlyDictionary<string, object> props2
        )
        {
            if (props1 == props2)
                return true;
            if (props1 == null || props2 == null)
                return false;
            if (props1.Count != props2.Count)
                return false;

            foreach (var kvp in props1)
            {
                if (!props2.TryGetValue(kvp.Key, out var value2))
                    return false;
                if (!object.Equals(kvp.Value, value2))
                    return false;
            }

            return true;
        }
    }
}
