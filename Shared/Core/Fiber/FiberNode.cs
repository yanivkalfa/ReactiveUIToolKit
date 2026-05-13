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
        /// <summary>
        /// Typed render function — set for all function components (typed props or no-props).
        /// </summary>
        public Func<IProps, IReadOnlyList<VirtualNode>, VirtualNode> TypedRender;

        /// <summary>Pending typed props for the next render (typed path only).</summary>
        public IProps TypedPendingProps;

        /// <summary>Committed typed props from the last successful render (typed path only).</summary>
        public IProps TypedProps;

        /// <summary>Component state (hooks, effects)</summary>
        internal FunctionComponentState ComponentState;

        // ==== For Host Elements ====
        /// <summary>The actual VisualElement (only for host nodes)</summary>
        public VisualElement HostElement;

        /// <summary>Committed typed host props from last render (typed pipeline).</summary>
        public Props.Typed.BaseProps HostProps;

        /// <summary>Pending typed host props for next render (typed pipeline).</summary>
        public Props.Typed.BaseProps PendingHostProps;

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
        public Dictionary<string, object> ProvidedContext;

        // ==== Refs ====
        /// <summary>For Portal nodes</summary>
        public VisualElement PortalTarget;

        // ==== Error Boundary State ====
        public bool ErrorBoundaryActive;
        public bool ErrorBoundaryShowingFallback;
        public Exception ErrorBoundaryLastException;
        public string ErrorBoundaryResetKey;
        public VirtualNode ErrorBoundaryFallback;
        public ErrorEventHandler ErrorBoundaryHandler;
        public IReadOnlyList<VirtualNode> ErrorBoundaryChildren;

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

        // ==== Update Tracking (for bailout optimization) ====
        /// <summary>
        /// Flag indicating this fiber has a pending state update
        /// </summary>
        public bool HasPendingStateUpdate;

        /// <summary>
        /// Flag indicating this fiber's subtree has updates
        /// </summary>
        public bool SubtreeHasUpdates;

        /// <summary>
        /// Flag indicating this fiber reads from Context and cannot safely bail out based on props alone
        /// </summary>
        public bool ReadsContext;
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

        /// <summary>
        /// HostPortal whose <see cref="FiberNode.PortalTarget"/> changed between
        /// renders. The commit phase must physically reparent the portal's
        /// top-level host descendants from the previous target VisualElement to
        /// the new one. Set in <c>CompleteWork</c> by comparing the WIP fiber's
        /// PortalTarget to its alternate's; honored in <c>CommitWork</c> by
        /// <c>CommitPortalRetarget</c>.
        /// </summary>
        PortalRetarget = 1 << 6,
    }
}
