using System;
using System.Collections.Generic;
using ReactiveUITK.Core;

namespace ReactiveUITK.Core.Fiber
{
    /// <summary>
    /// Child reconciliation - the heart of the diffing algorithm
    /// Implements React's reconcileChildFibers
    /// </summary>
    public static class FiberChildReconciliation
    {
        /// <summary>
        /// Reconcile children - diff old and new children, create/update/delete fibers
        /// </summary>
        public static void ReconcileChildren(
            FiberNode wipFiber,
            FiberNode currentFirstChild,
            IReadOnlyList<VirtualNode> newChildren
        )
        {
            if (newChildren == null || newChildren.Count == 0)
            {
                // Delete all existing children
                DeleteRemainingChildren(wipFiber, currentFirstChild);
                return;
            }

            // Try keyed reconciliation first
            if (HasKeys(newChildren))
            {
                ReconcileChildrenWithKeys(wipFiber, currentFirstChild, newChildren);
            }
            else
            {
                ReconcileChildrenByIndex(wipFiber, currentFirstChild, newChildren);
            }
        }

        /// <summary>
        /// Reconcile children by index (no keys)
        /// </summary>
        private static void ReconcileChildrenByIndex(
            FiberNode wipFiber,
            FiberNode currentFirstChild,
            IReadOnlyList<VirtualNode> newChildren
        )
        {
            FiberNode oldFiber = currentFirstChild;
            FiberNode previousNewFiber = null;
            FiberNode newFiber = null;
            int newIdx = 0;

            // Update common children
            for (; oldFiber != null && newIdx < newChildren.Count; newIdx++)
            {
                var newChild = newChildren[newIdx];

                if (oldFiber.Index > newIdx)
                {
                    // Old fiber is ahead, insert new one
                    newFiber = CreateFiber(newChild, wipFiber, newIdx);
                }
                else
                {
                    // Try to update existing fiber
                    newFiber = UpdateSlot(oldFiber, newChild);
                    if (newFiber == null)
                    {
                        // Can't reuse, create new
                        newFiber = CreateFiber(newChild, wipFiber, newIdx);
                    }
                    else
                    {
                        // Reused old fiber
                        oldFiber = oldFiber.Sibling;
                    }
                }

                if (newFiber == null)
                {
                    continue;
                }

                // Ensure parent pointer is correct for reused fibers
                newFiber.Parent = wipFiber;

                PlaceChild(newFiber, newIdx);

                if (previousNewFiber == null)
                {
                    wipFiber.Child = newFiber;
                }
                else
                {
                    previousNewFiber.Sibling = newFiber;
                }

                previousNewFiber = newFiber;
            }

            if (newIdx == newChildren.Count)
            {
                // Finished all new children, delete remaining old
                DeleteRemainingChildren(wipFiber, oldFiber);
                return;
            }

            if (oldFiber == null)
            {
                // No more old children, create remaining new
                for (; newIdx < newChildren.Count; newIdx++)
                {
                    newFiber = CreateFiber(newChildren[newIdx], wipFiber, newIdx);
                    if (newFiber == null)
                        continue;

                    // Ensure parent pointer is correct
                    newFiber.Parent = wipFiber;

                    PlaceChild(newFiber, newIdx);

                    if (previousNewFiber == null)
                    {
                        wipFiber.Child = newFiber;
                    }
                    else
                    {
                        previousNewFiber.Sibling = newFiber;
                    }

                    previousNewFiber = newFiber;
                }
            }
        }

        /// <summary>
        /// Reconcile children with keys (optimized reconciliation)
        /// </summary>
        private static void ReconcileChildrenWithKeys(
            FiberNode wipFiber,
            FiberNode currentFirstChild,
            IReadOnlyList<VirtualNode> newChildren
        )
        {
            // Build map of keyed old children
            var existingChildren = MapRemainingChildren(currentFirstChild);

            FiberNode previousNewFiber = null;

            for (int newIdx = 0; newIdx < newChildren.Count; newIdx++)
            {
                var newChild = newChildren[newIdx];
                var key = newChild.Key ?? newIdx.ToString();

                FiberNode newFiber = null;

                // Try to find existing fiber with same key
                if (existingChildren.TryGetValue(key, out var oldFiber))
                {
                    // Attempt to reuse the old fiber for this key.
                    // Only remove from the lookup map if reuse succeeds;
                    // otherwise keep it so that it can be deleted later.
                    newFiber = UpdateSlot(oldFiber, newChild);
                    if (newFiber != null)
                    {
                        existingChildren.Remove(key);
                    }
                }

                // If can't reuse, create new
                if (newFiber == null)
                {
                    newFiber = CreateFiber(newChild, wipFiber, newIdx);
                }

                if (newFiber == null)
                    continue;

                // Ensure parent pointer is correct for reused fibers
                newFiber.Parent = wipFiber;

                PlaceChild(newFiber, newIdx);

                if (previousNewFiber == null)
                {
                    wipFiber.Child = newFiber;
                }
                else
                {
                    previousNewFiber.Sibling = newFiber;
                }

                previousNewFiber = newFiber;
            }

            // Delete remaining old children
            foreach (var oldFiber in existingChildren.Values)
            {
                DeleteChild(wipFiber, oldFiber);
            }
        }

        /// <summary>
        /// Try to update existing fiber with new vnode
        /// Returns null if can't reuse
        /// </summary>
        private static FiberNode UpdateSlot(FiberNode oldFiber, VirtualNode newVNode)
        {
            // Check if we can reuse this fiber
            if (!CanReuseFiber(oldFiber, newVNode))
            {
                return null;
            }

            // Clone and update
            var newFiber = CloneFiber(oldFiber);
            newFiber.PendingProps = ExtractProps(newVNode);
            newFiber.Children = newVNode.Children;
            newFiber.EffectTag = EffectFlags.Update;
            newFiber.LastRenderedVNode = newVNode;

            return newFiber;
        }

        /// <summary>
        /// Check if fiber can be reused for vnode
        /// </summary>
        private static bool CanReuseFiber(FiberNode fiber, VirtualNode vnode)
        {
            if (fiber == null || vnode == null)
                return false;

            // Check type matches
            switch (vnode.NodeType)
            {
                case VirtualNodeType.Element:
                    return fiber.Tag == FiberTag.HostComponent
                        && fiber.ElementType == vnode.ElementTypeName;

                case VirtualNodeType.Text:
                    // Text nodes are represented as host "Label" elements.
                    // Treat them as reusable when the host element type matches.
                    return fiber.Tag == FiberTag.HostComponent && fiber.ElementType == "Label";

                case VirtualNodeType.FunctionComponent:
                    if (fiber.Tag != FiberTag.FunctionComponent) return false;
                    
                    // Check delegate equality
                    if (fiber.Render == vnode.FunctionRender) return true;
                    
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
            if (vnode == null)
                return null;

            var fiber = new FiberNode
            {
                Key = vnode.Key,
                Parent = parent,
                Index = index,
                PendingProps = ExtractProps(vnode),
                Children = vnode.Children,
                EffectTag = EffectFlags.Placement,
            };

            switch (vnode.NodeType)
            {
                case VirtualNodeType.Element:
                    fiber.Tag = FiberTag.HostComponent;
                    fiber.ElementType = vnode.ElementTypeName;
                    break;

                case VirtualNodeType.Text:
                    fiber.Tag = FiberTag.HostComponent;
                    fiber.ElementType = "Label";
                    break;

                case VirtualNodeType.FunctionComponent:
                    fiber.Tag = FiberTag.FunctionComponent;
                    fiber.Render = vnode.FunctionRender;
                    break;

                case VirtualNodeType.Suspense:
                    fiber.Tag = FiberTag.FunctionComponent;
                    fiber.Render = FiberIntrinsicComponents.SuspenseRender;
                    fiber.PendingProps = FiberIntrinsicComponents.CreateSuspenseProps(vnode);
                    break;

                case VirtualNodeType.Portal:
                    fiber.Tag = FiberTag.HostPortal;
                    fiber.PortalTarget = vnode.PortalTarget;
                    fiber.HostElement = vnode.PortalTarget;
                    break;

                case VirtualNodeType.ErrorBoundary:
                    fiber.Tag = FiberTag.ErrorBoundary;
                    fiber.LastRenderedVNode = vnode;
                    fiber.ErrorBoundaryResetKey = vnode.ErrorResetToken;
                    fiber.ErrorBoundaryActive = false;
                    fiber.ErrorBoundaryShowingFallback = false;
                    fiber.ErrorBoundaryLastException = null;
                    break;

                case VirtualNodeType.Fragment:
                    fiber.Tag = FiberTag.Fragment;
                    break;
            }

            return fiber;
        }

        /// <summary>
        /// Clone existing fiber for reuse
        /// </summary>
        private static FiberNode CloneFiber(FiberNode fiber)
        {
            return new FiberNode
            {
                Tag = fiber.Tag,
                ElementType = fiber.ElementType,
                Key = fiber.Key,
                Render = fiber.Render,
                HostElement = fiber.HostElement,
                ComponentState = fiber.ComponentState,
                Alternate = fiber,
                Props = fiber.Props,
                ContextFrame = fiber.ContextFrame,
                ContextProviderId = fiber.ContextProviderId,
                ProvidedContext = fiber.ProvidedContext,
                PortalTarget = fiber.PortalTarget,
                LastRenderedVNode = fiber.LastRenderedVNode,
                ErrorBoundaryActive = fiber.ErrorBoundaryActive,
                ErrorBoundaryShowingFallback = fiber.ErrorBoundaryShowingFallback,
                ErrorBoundaryLastException = fiber.ErrorBoundaryLastException,
                ErrorBoundaryResetKey = fiber.ErrorBoundaryResetKey,
                HasPendingStateUpdate = fiber.HasPendingStateUpdate,
                SubtreeHasUpdates = fiber.SubtreeHasUpdates,
                EffectTag = fiber.EffectTag,
            };
        }

        /// <summary>
        /// Mark fiber for placement at index
        /// </summary>
        private static void PlaceChild(FiberNode fiber, int index)
        {
            fiber.Index = index;
            if (fiber.Alternate == null)
            {
                // New fiber, needs placement
                fiber.EffectTag |= EffectFlags.Placement;
            }
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
        /// Delete all remaining children
        /// </summary>
        private static void DeleteRemainingChildren(FiberNode parentFiber, FiberNode firstChild)
        {
            var child = firstChild;
            while (child != null)
            {
                DeleteChild(parentFiber, child);
                child = child.Sibling;
            }
        }

        /// <summary>
        /// Map remaining children by key for efficient lookup
        /// </summary>
        private static Dictionary<string, FiberNode> MapRemainingChildren(FiberNode firstChild)
        {
            var map = new Dictionary<string, FiberNode>();
            var child = firstChild;
            int index = 0;

            while (child != null)
            {
                var key = child.Key ?? index.ToString();
                map[key] = child;
                child = child.Sibling;
                index++;
            }

            return map;
        }

        /// <summary>
        /// Check if any children have keys
        /// </summary>
        private static bool HasKeys(IReadOnlyList<VirtualNode> children)
        {
            foreach (var child in children)
            {
                if (!string.IsNullOrEmpty(child?.Key))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Clone child fibers when skipping render (Bailout)
        /// </summary>
        public static FiberNode CloneChildFibers(FiberNode wipFiber)
        {
            if (wipFiber.Alternate == null || wipFiber.Alternate.Child == null)
            {
                return null;
            }

            var currentChild = wipFiber.Alternate.Child;
            var newChild = CloneFiber(currentChild);
            newChild.Parent = wipFiber;
            wipFiber.Child = newChild;

            // Log details about the cloned child to debug flag propagation
            if (newChild.Render != null && (newChild.Render.Method.Name.Contains("Router") || newChild.ElementType == "Router"))
            {
                UnityEngine.Debug.Log($"[CloneChildFibers] Cloned Router - Pending: {newChild.HasPendingStateUpdate}, Subtree: {newChild.SubtreeHasUpdates}, EffectTag: {newChild.EffectTag}");
            }

            var currentSibling = currentChild.Sibling;
            var newSibling = newChild;

            while (currentSibling != null)
            {
                var cloneSibling = CloneFiber(currentSibling);
                cloneSibling.Parent = wipFiber;
                newSibling.Sibling = cloneSibling;
                
                newSibling = cloneSibling;
                currentSibling = currentSibling.Sibling;
            }

            return newChild;
        }



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
    }
}
