using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine.UIElements;

namespace ReactiveUITK.Core.Fiber
{
    /// <summary>
    /// Fiber node - represents a unit of work in the component tree.
    /// Closely mirrors React's Fiber structure.
    /// </summary>
    public class FiberNode
    {
        // ==== Fiber Tree Structure ====
        /// <summary>Parent fiber</summary>
        public FiberNode Parent;

        /// <summary>First child fiber</summary>
        public FiberNode Child;

        /// <summary>Next sibling fiber</summary>
        public FiberNode Sibling;

        /// <summary>Index in parent's children</summary>
        public int Index;

        // ==== Component Identity ====
        /// <summary>User-provided key for reconciliation</summary>
        public string Key;

        /// <summary>Type of fiber node</summary>
        public FiberTag Tag;

        /// <summary>Element type (for host components like "Button", "Label")</summary>
        public string ElementType;

        // ==== For Function Components ====
        /// <summary>The render function</summary>
        /// <summary>The render function</summary>
        public Func<Dictionary<string, object>, IReadOnlyList<VirtualNode>, VirtualNode> Render;

        /// <summary>Component state (hooks, effects)</summary>
        internal FunctionComponentState ComponentState;

        // ==== For Host Elements ====
        /// <summary>The actual VisualElement (only for host nodes)</summary>
        public VisualElement HostElement;

        // ==== Props and State ====
        /// <summary>Current props</summary>
        public IReadOnlyDictionary<string, object> Props;

        /// <summary>Pending props (next render)</summary>
        public IReadOnlyDictionary<string, object> PendingProps;

        /// <summary>Children vnodes</summary>
        public IReadOnlyList<VirtualNode> Children;

        // ==== Reconciliation ====
        /// <summary>Alternate fiber (current ↔ work-in-progress)</summary>
        public FiberNode Alternate;

        /// <summary>Effect tags for commit phase</summary>
        public EffectFlags EffectTag;

        /// <summary>Next fiber in effect list</summary>
        public FiberNode NextEffect;

        /// <summary>Deletions to perform</summary>
        public List<FiberNode> Deletions;

        // ==== Context ====
        internal HostContext.ContextFrameHandle ContextFrame;
        public int ContextProviderId;
        public Dictionary<string, object> ProvidedContext;

        // ==== Refs ====
        /// <summary>For Portal nodes</summary>
        public VisualElement PortalTarget;

        /// <summary>For tracking rendered output</summary>
        public VirtualNode LastRenderedVNode;

        // ==== Error Boundary State ====
        public bool ErrorBoundaryActive;
        public bool ErrorBoundaryShowingFallback;
        public Exception ErrorBoundaryLastException;
        public string ErrorBoundaryResetKey;

        // ==== Lifecycle ====
        /// <summary>List of layout effects to run</summary>
        public List<(
            Func<Action> factory,
            object[] deps,
            object[] lastDeps,
            Action cleanup
        )> LayoutEffects;

        /// <summary>List of passive effects to run</summary>
        public List<(
            Func<Action> factory,
            object[] deps,
            object[] lastDeps,
            Action cleanup
        )> PassiveEffects;

        /// <summary>
        /// Flag indicating this fiber has a pending state update
        /// </summary>
        public bool HasPendingStateUpdate;

        /// <summary>
        /// Flag indicating a descendant has a pending update
        /// </summary>
        public bool SubtreeHasUpdates;
    }

    /// <summary>
    /// Fiber node type tags - matches React's fiber tags
    /// </summary>
    public enum FiberTag
    {
        /// <summary>Function component</summary>
        FunctionComponent = 0,

        /// <summary>Host element (VisualElement)</summary>
        HostComponent = 5,

        /// <summary>Portal</summary>
        HostPortal = 4,

        /// <summary>Fragment (multiple children, no wrapper)</summary>
        Fragment = 7,

        /// <summary>Error boundary</summary>
        ErrorBoundary = 16,
    }

    /// <summary>
    /// Effect flags - what changed that needs to be committed
    /// Matches React's effect tags
    /// </summary>
    [Flags]
    public enum EffectFlags
    {
        None = 0,

        /// <summary>Fiber was just created</summary>
        Placement = 1 << 0,

        /// <summary>Props or state changed</summary>
        Update = 1 << 1,

        /// <summary>Fiber should be removed</summary>
        Deletion = 1 << 2,

        /// <summary>Has layout effects</summary>
        LayoutEffect = 1 << 3,

        /// <summary>Has passive effects</summary>
        PassiveEffect = 1 << 4,

        /// <summary>Ref needs update</summary>
        Ref = 1 << 5,


    }
}
