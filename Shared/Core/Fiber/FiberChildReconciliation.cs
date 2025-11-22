using System.Collections.Generic;

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
            IReadOnlyList<VirtualNode> newChildren)
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
            IReadOnlyList<VirtualNode> newChildren)
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
                    if (newFiber == null) continue;

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
            IReadOnlyList<VirtualNode> newChildren)
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
                    existingChildren.Remove(key);
                    newFiber = UpdateSlot(oldFiber, newChild);
                }
                
                // If can't reuse, create new
                if (newFiber == null)
                {
                    newFiber = CreateFiber(newChild, wipFiber, newIdx);
                }

                if (newFiber == null) continue;

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
                Alternate = fiber,
                Props = fiber.Props
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
        /// Extract props from vnode
        /// </summary>
        private static IReadOnlyDictionary<string, object> ExtractProps(VirtualNode vnode)
        {
            return vnode?.Properties ?? new Dictionary<string, object>();
        }
    }
}
