using System;
using System.Collections.Generic;
using System.Reflection;
using ReactiveUITK;
using ReactiveUITK.Core;

namespace ReactiveUITK.Core.Fiber
{
    /// <summary>
    /// Child reconciliation - the heart of the diffing algorithm
    /// Implements React's reconcileChildFibers
    /// </summary>
    public static class FiberChildReconciliation
    {
        // Pre-computed index → string cache for MapRemainingChildren
        private static readonly string[] s_indexStrings = InitIndexStrings(256);

        private static string[] InitIndexStrings(int count)
        {
            var arr = new string[count];
            for (int i = 0; i < count; i++)
                arr[i] = i.ToString();
            return arr;
        }

        private static string IndexToString(int index)
        {
            return (uint)index < (uint)s_indexStrings.Length
                ? s_indexStrings[index]
                : index.ToString();
        }

        /// <summary>
        /// Reconcile children - diff old and new children, create/update/delete fibers
        /// </summary>
        public static void ReconcileChildren(
            FiberNode returnFiber,
            FiberNode currentFirstChild,
            IReadOnlyList<VirtualNode> newChildren
        )
        {
            // Optimization for likely case: the list of children is empty
            if (newChildren == null || newChildren.Count == 0)
            {
                // Delete all existing children
                DeleteRemainingChildren(returnFiber, currentFirstChild);
                return;
            }

            // Check first child for keys (React convention: all-or-nothing keyed within a sibling set)
            if (newChildren.Count > 0 && !string.IsNullOrEmpty(newChildren[0]?.Key))
            {
                ReconcileChildrenWithKeys(returnFiber, currentFirstChild, newChildren);
            }
            else
            {
                ReconcileChildrenByIndex(returnFiber, currentFirstChild, newChildren);
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
                var key = newChild.Key ?? IndexToString(newIdx);

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

            // Use centralized factory for consistent flag propagation
            var reused = FiberFactory.CloneForReuse(oldFiber, newVNode);
            return reused;
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
                    if (fiber.Tag != FiberTag.FunctionComponent)
                        return false;

                    // All function components use TypedRender.
                    if (fiber.TypedRender == null || vnode.TypedFunctionRender == null)
                        return false;
                    if (ReferenceEquals(fiber.TypedRender, vnode.TypedFunctionRender))
                        return true;
                    if (
                        fiber.TypedRender.Method == vnode.TypedFunctionRender.Method
                        && fiber.TypedRender.Target == vnode.TypedFunctionRender.Target
                    )
                        return true;

#if UNITY_EDITOR
                    // HMR fallback: when hot-reloading, the fiber's delegate points to a
                    // new assembly while the parent's VNode still references the old one.
                    // Match by declaring type name + method name to preserve state.
                    if (HmrState.IsActive)
                    {
                        var fiberType = fiber.TypedRender.Method.DeclaringType;
                        var vnodeType = vnode.TypedFunctionRender.Method.DeclaringType;
                        if (
                            fiberType != null
                            && vnodeType != null
                            && fiber.TypedRender.Method.Name
                                == vnode.TypedFunctionRender.Method.Name
                        )
                        {
                            // Primary: class name match
                            if (fiberType.Name == vnodeType.Name)
                                return true;

                            // Fallback: after a component rename, class names differ
                            // but [UitkxSource] file paths remain stable.
                            var fiberSource = fiberType.GetCustomAttribute<UitkxSourceAttribute>();
                            var vnodeSource = vnodeType.GetCustomAttribute<UitkxSourceAttribute>();
                            if (
                                fiberSource != null
                                && vnodeSource != null
                                && string.Equals(
                                    fiberSource.SourcePath,
                                    vnodeSource.SourcePath,
                                    System.StringComparison.OrdinalIgnoreCase
                                )
                            )
                                return true;
                        }
                    }
#endif
                    return false;

                case VirtualNodeType.Suspense:
                    return fiber.Tag == FiberTag.FunctionComponent
                        && fiber.TypedRender == FiberIntrinsicComponents.SuspenseRender;

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
                    fiber.TypedRender = vnode.TypedFunctionRender;
                    fiber.TypedPendingProps = vnode.TypedProps;
                    break;

                case VirtualNodeType.Suspense:
                    fiber.Tag = FiberTag.FunctionComponent;
                    fiber.TypedRender = FiberIntrinsicComponents.SuspenseRender;
                    fiber.TypedPendingProps = FiberIntrinsicComponents.CreateSuspenseProps(vnode);
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
        /// Mark fiber for placement at index
        /// </summary>
        private static FiberNode PlaceExistingChild(FiberNode fiber, int newIndex)
        {
            fiber.Index = newIndex;
            fiber.EffectTag = EffectFlags.None; // Mark as reused (no placement needed)

            // PHASE 2: Propagate flags from alternate when reusing
            if (fiber.Alternate != null)
            {
                fiber.SubtreeHasUpdates = fiber.Alternate.SubtreeHasUpdates;
                fiber.ReadsContext = fiber.Alternate.ReadsContext;
            }

            return fiber;
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
                var key = child.Key ?? IndexToString(index);
                map[key] = child;
                child = child.Sibling;
                index++;
            }

            return map;
        }

        /// <summary>
        /// Extract props from vnode
        /// </summary>
        private static IReadOnlyDictionary<string, object> ExtractProps(VirtualNode vnode)
        {
            if (vnode == null)
            {
                return VirtualNode.EmptyProps;
            }

            switch (vnode.NodeType)
            {
                case VirtualNodeType.Suspense:
                    return VirtualNode.EmptyProps;

                case VirtualNodeType.Text:
                    return new Dictionary<string, object>
                    {
                        { "text", vnode.TextContent ?? string.Empty },
                    };

                default:
                    return vnode.Properties ?? VirtualNode.EmptyProps;
            }
        }

        /// <summary>
        /// Clone child fibers from alternate without re-rendering parent.
        /// Used for bailout optimization when parent props haven't changed
        /// but subtree has updates.
        /// </summary>
        public static FiberNode CloneChildFibers(FiberNode parent)
        {
            // Use centralized factory for consistent flag propagation
            return FiberFactory.CloneChildrenForBailout(parent);
        }
    }
}
