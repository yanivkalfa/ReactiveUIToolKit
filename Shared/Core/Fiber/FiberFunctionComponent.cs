using System;
using System.Collections.Generic;
using ReactiveUITK.Core;

namespace ReactiveUITK.Core.Fiber
{
    /// <summary>
    /// Function component support for Fiber reconciler
    /// Handles rendering, hooks, and effects
    /// </summary>
    public static class FiberFunctionComponent
    {
        /// <summary>
        /// Render a function component and return the child fiber
        /// </summary>
        public static FiberNode RenderFunctionComponent(
            FiberNode wipFiber,
            HostContext hostContext,
            FiberReconciler reconciler)
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

            // Wire up state updates to Fiber reconciler
            componentState.OnStateUpdated = () =>
            {
                reconciler.ScheduleUpdateOnFiber(wipFiber, null);
            };

            // Set hook context
            HookContext.Current = componentState;
            componentState.IsRendering = true;

            VirtualNode childVNode = null;

            try
            {
                // Call the render function
                var propsDict = wipFiber.PendingProps as Dictionary<string, object>
                    ?? new Dictionary<string, object>(wipFiber.PendingProps ?? new Dictionary<string, object>());

                childVNode = wipFiber.Render(propsDict, wipFiber.Children);

                // Store rendered vnode
                wipFiber.LastRenderedVNode = childVNode;
            }
            finally
            {
                componentState.IsRendering = false;
                HookContext.Current = null;
            }

            // Mark effect flags so the commit phase can run effects.
            if (componentState.FunctionLayoutEffects != null &&
                componentState.FunctionLayoutEffects.Count > 0)
            {
                wipFiber.EffectTag |= EffectFlags.LayoutEffect;
            }

            if (componentState.FunctionEffects != null &&
                componentState.FunctionEffects.Count > 0)
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

            return null;
        }

        /// <summary>
        /// Reconcile a single child (function components return single root)
        /// </summary>
        private static FiberNode ReconcileSingleChild(
            FiberNode parent,
            FiberNode currentChild,
            VirtualNode newVNode)
        {
            // Try to reuse existing fiber
            if (currentChild != null && CanReuseFiber(currentChild, newVNode))
            {
                // Clone and update
                var clone = new FiberNode
                {
                    Tag = currentChild.Tag,
                    ElementType = currentChild.ElementType,
                    Key = currentChild.Key,
                    Render = currentChild.Render,
                    HostElement = currentChild.HostElement,
                    ComponentState = currentChild.ComponentState,
                    Alternate = currentChild,
                    Props = currentChild.Props,
                    Parent = parent,
                    Index = 0
                };

                clone.PendingProps = ExtractProps(newVNode);
                clone.Children = newVNode.Children;
                clone.EffectTag = EffectFlags.Update;

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
                    return fiber.Tag == FiberTag.HostComponent &&
                           fiber.ElementType == vnode.ElementTypeName;

                case VirtualNodeType.FunctionComponent:
                    return fiber.Tag == FiberTag.FunctionComponent &&
                           fiber.Render == vnode.FunctionRender;

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
            if (vnode == null) return null;

            var fiber = new FiberNode
            {
                Key = vnode.Key,
                Parent = parent,
                Index = index,
                PendingProps = ExtractProps(vnode),
                Children = vnode.Children,
                EffectTag = EffectFlags.Placement
            };

            switch (vnode.NodeType)
            {
                case VirtualNodeType.Element:
                    fiber.Tag = FiberTag.HostComponent;
                    fiber.ElementType = vnode.ElementTypeName;
                    break;

                case VirtualNodeType.FunctionComponent:
                    fiber.Tag = FiberTag.FunctionComponent;
                    fiber.Render = vnode.FunctionRender;
                    break;

                case VirtualNodeType.Fragment:
                    fiber.Tag = FiberTag.Fragment;
                    break;
            }

            return fiber;
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
            return vnode?.Properties ?? new Dictionary<string, object>();
        }

        /// <summary>
        /// Commit layout effects for a function component
        /// </summary>
        public static void CommitLayoutEffects(FiberNode fiber)
        {
            var componentState = fiber.ComponentState;
            if (componentState?.FunctionLayoutEffects == null) return;

            for (int i = 0; i < componentState.FunctionLayoutEffects.Count; i++)
            {
                var effect = componentState.FunctionLayoutEffects[i];
                bool shouldRun = effect.lastDeps == null ||
                                DepsChanged(effect.lastDeps, effect.deps);

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
            if (componentState?.FunctionEffects == null) return;

            // For now, run effects immediately (React schedules these)
            for (int i = 0; i < componentState.FunctionEffects.Count; i++)
            {
                var effect = componentState.FunctionEffects[i];
                bool shouldRun = effect.lastDeps == null ||
                                DepsChanged(effect.lastDeps, effect.deps);

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
    }
}
