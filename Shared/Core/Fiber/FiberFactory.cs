using System.Collections.Generic;
using System.Linq;

namespace ReactiveUITK.Core.Fiber
{
    /// <summary>
    /// Centralized factory for creating and cloning fibers with automatic flag propagation.
    /// This ensures consistent flag management across all fiber creation paths.
    /// </summary>
    public static class FiberFactory
    {
        /// <summary>
        /// Create a brand new fiber from a VirtualNode (no current fiber to clone from).
        /// New fibers start with clean flags.
        /// </summary>
        public static FiberNode CreateNew(VirtualNode vnode, FiberNode parent, int index)
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
                EffectTag = EffectFlags.Placement, // New fiber needs to be placed
                
                // Initialize flags - new fibers start clean
                HasPendingStateUpdate = false,
                SubtreeHasUpdates = false,
                ReadsContext = false,
            };

            // Set fiber type based on vnode
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
                    fiber.EffectTag = EffectFlags.None;
                    break;

                case VirtualNodeType.Portal:
                    fiber.Tag = FiberTag.HostPortal;
                    fiber.PortalTarget = vnode.PortalTarget;
                    fiber.HostElement = vnode.PortalTarget;
                    fiber.EffectTag = EffectFlags.None;
                    break;

                case VirtualNodeType.ErrorBoundary:
                    fiber.Tag = FiberTag.ErrorBoundary;
                    fiber.LastRenderedVNode = vnode;
                    fiber.ErrorBoundaryResetKey = vnode.ErrorResetToken;
                    fiber.ErrorBoundaryActive = false;
                    fiber.ErrorBoundaryShowingFallback = false;
                    fiber.ErrorBoundaryLastException = null;
                    fiber.EffectTag = EffectFlags.None;
                    break;

                case VirtualNodeType.Fragment:
                    fiber.Tag = FiberTag.Fragment;
                    fiber.EffectTag = EffectFlags.None;
                    break;

                default:
                    UnityEngine.Debug.LogError($"Unknown VirtualNodeType: {vnode.NodeType}");
                    break;
            }

            UnityEngine.Debug.Log($"[Full Tree Rerender][{fiber.ElementType ?? fiber.Render?.Method.DeclaringType?.Name ?? "Unknown"}][FiberFactory.CreateNew] Created new fiber with clean flags");
            
            return fiber;
        }

        /// <summary>
        /// Clone an existing fiber for reuse with a new VirtualNode.
        /// CRITICAL: Automatically propagates flags from current to clone.
        /// This is the single source of truth for flag propagation during reconciliation.
        /// </summary>
        public static FiberNode CloneForReuse(FiberNode current, VirtualNode newVNode)
        {
            if (current == null)
                return null;

            var clone = new FiberNode
            {
                Tag = current.Tag,
                ElementType = current.ElementType,
                Key = current.Key,
                Render = current.Render,
                HostElement = current.HostElement,
                ComponentState = current.ComponentState,
                Props = current.Props,
                ContextFrame = current.ContextFrame,
                ContextProviderId = current.ContextProviderId,
                ProvidedContext = current.ProvidedContext,
                PortalTarget = current.PortalTarget,
                ErrorBoundaryActive = current.ErrorBoundaryActive,
                ErrorBoundaryShowingFallback = current.ErrorBoundaryShowingFallback,
                ErrorBoundaryLastException = current.ErrorBoundaryLastException,
                ErrorBoundaryResetKey = current.ErrorBoundaryResetKey,
                
                // AUTOMATIC FLAG PROPAGATION - This is why the factory exists!
                HasPendingStateUpdate = current.HasPendingStateUpdate,
                SubtreeHasUpdates = current.SubtreeHasUpdates,
                ReadsContext = current.ReadsContext,
                
                // Set up alternate chain
                Alternate = current,
                
                // Update for new render
                PendingProps = ExtractProps(newVNode),
                Children = newVNode.Children,
                EffectTag = EffectFlags.Update, // Reused fiber = update
                LastRenderedVNode = newVNode,
            };

            // Link back to clone
            current.Alternate = clone;

            var name = clone.ElementType ?? clone.Render?.Method.DeclaringType?.Name ?? "Unknown";
            UnityEngine.Debug.Log($"[Full Tree Rerender][{name}][FiberFactory.CloneForReuse] Cloned with flags - HasPending:{clone.HasPendingStateUpdate}, SubtreeUpdates:{clone.SubtreeHasUpdates}, ReadsContext:{clone.ReadsContext}");

            return clone;
        }

        /// <summary>
        /// Clone all children from alternate tree without re-rendering parent.
        /// Used during bailout when parent doesn't need to render but children might.
        /// </summary>
        public static FiberNode CloneChildrenForBailout(FiberNode parent)
        {
            if (parent?.Alternate?.Child == null)
            {
                var parentName = parent?.ElementType ?? parent?.Render?.Method.DeclaringType?.Name ?? "Unknown";
                UnityEngine.Debug.Log($"[Full Tree Rerender][{parentName}][FiberFactory.CloneChildren] No children to clone");
                return null;
            }

            var parentName2 = parent.ElementType ?? parent.Render?.Method.DeclaringType?.Name ?? "Unknown";
            UnityEngine.Debug.Log($"[Full Tree Rerender][{parentName2}][FiberFactory.CloneChildren] Cloning children for bailout");

            var current = parent.Alternate.Child;
            
            // Clone first child
            var newChild = CloneForReuse(current, current.LastRenderedVNode);
            parent.Child = newChild;
            newChild.Parent = parent;

            // Clone siblings
            var previousNewFiber = newChild;
            current = current.Sibling;
            int siblingCount = 1;

            while (current != null)
            {
                var cloned = CloneForReuse(current, current.LastRenderedVNode);
                cloned.Parent = parent;
                previousNewFiber.Sibling = cloned;
                previousNewFiber = cloned;
                current = current.Sibling;
                siblingCount++;
            }

            previousNewFiber.Sibling = null;
            UnityEngine.Debug.Log($"[Full Tree Rerender][{parentName2}][FiberFactory.CloneChildren] Cloned {siblingCount} children");
            
            return newChild;
        }

        private static IReadOnlyDictionary<string, object> ExtractProps(VirtualNode vnode)
        {
            if (vnode == null)
                return new Dictionary<string, object>();

            // Match FiberFunctionComponent.ExtractProps implementation
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
