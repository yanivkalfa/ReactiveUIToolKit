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
                    fiber.TypedRender = vnode.TypedFunctionRender;
                    fiber.TypedPendingProps = vnode.TypedProps;
                    break;

                case VirtualNodeType.Suspense:
                    fiber.Tag = FiberTag.FunctionComponent;
                    fiber.TypedRender = FiberIntrinsicComponents.SuspenseRender;
                    fiber.TypedPendingProps = FiberIntrinsicComponents.CreateSuspenseProps(vnode);
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

            // Reuse the alternate fiber if available (React pattern: avoid allocation
            // on every re-render for stable tree positions).
            var clone = current.Alternate ?? new FiberNode();

            // === Identity and type ===
            clone.Tag = current.Tag;
            clone.ElementType = current.ElementType;
            clone.Key = current.Key;
            clone.TypedRender = current.TypedRender;

            // === Props ===
            clone.TypedProps = current.TypedProps;
            clone.TypedPendingProps = newVNode?.TypedProps ?? current.TypedPendingProps;
            clone.Props = current.Props;
            clone.PendingProps = newVNode != null ? ExtractProps(newVNode) : current.PendingProps;
            clone.Children = newVNode != null ? newVNode.Children : current.Children;

            // === Host/DOM ===
            clone.HostElement = current.HostElement;
            clone.PortalTarget = current.PortalTarget;

            // === State ===
            clone.ComponentState = current.ComponentState; // CRITICAL: Share, don't clone!
            clone.Index = current.Index;

            // === Context ===
            clone.ContextFrame = current.ContextFrame;
            clone.ContextProviderId = current.ContextProviderId;
            clone.ProvidedContext = current.ProvidedContext;

            // === Error boundary ===
            clone.ErrorBoundaryActive = current.ErrorBoundaryActive;
            clone.ErrorBoundaryShowingFallback = current.ErrorBoundaryShowingFallback;
            clone.ErrorBoundaryLastException = current.ErrorBoundaryLastException;
            clone.ErrorBoundaryResetKey = current.ErrorBoundaryResetKey;

            // === Flags (automatic propagation) ===
            clone.HasPendingStateUpdate = current.HasPendingStateUpdate;
            clone.SubtreeHasUpdates = current.SubtreeHasUpdates;
            clone.ReadsContext = current.ReadsContext;

            // === Reset tree structure (rebuilt during reconciliation) ===
            clone.Child = null;
            clone.Sibling = null;
            clone.Parent = null;

            // === Reset effect tracking ===
            clone.EffectTag = EffectFlags.None;
            clone.NextEffect = null;
            clone.Deletions = null;
            clone.LayoutEffects = null;
            clone.PassiveEffects = null;

            // === Rendered output ===
            clone.LastRenderedVNode = newVNode ?? current.LastRenderedVNode;

            // === Alternate chain ===
            clone.Alternate = current;
            current.Alternate = clone;

            // Fix: Suspense VNodes always have TypedProps=null because SuspenseProps is
            // internal infrastructure, not exposed as IProps on the VirtualNode.
            if (newVNode?.NodeType == VirtualNodeType.Suspense)
                clone.TypedPendingProps = FiberIntrinsicComponents.CreateSuspenseProps(newVNode);

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
                return null;
            }

            var current = parent.Alternate.Child;

            // Clone first child - pass null VNode to preserve prop identity
            // (CloneForReuse will use current.PendingProps when newVNode is null)
            var newChild = CloneForReuse(current, null);
            parent.Child = newChild;
            newChild.Parent = parent;

            // Clone siblings
            var previousNewFiber = newChild;
            current = current.Sibling;
            int siblingCount = 1;

            while (current != null)
            {
                var cloned = CloneForReuse(current, null);
                cloned.Parent = parent;
                previousNewFiber.Sibling = cloned;
                previousNewFiber = cloned;
                current = current.Sibling;
                siblingCount++;
            }

            previousNewFiber.Sibling = null;
            return newChild;
        }

        private static IReadOnlyDictionary<string, object> ExtractProps(VirtualNode vnode)
        {
            if (vnode == null)
                return VirtualNode.EmptyProps;

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
    }
}
