using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace ReactiveUITK.Core
{
    internal sealed class NodeMetadata
    {
        public string Key;

        // Registered wrapper callbacks per event (UI Toolkit EventCallback wrappers)
        public Dictionary<string, Delegate> EventHandlers = new();

        // Latest user-provided handlers per event; wrappers read from here at invoke time
        public Dictionary<string, Delegate> EventHandlerTargets = new();
        public Dictionary<string, string> EventHandlerSignatures = new();
        public object AttachedRef;

        // Restored original System.Func signature for compatibility
        public System.Func<
            Dictionary<string, object>,
            IReadOnlyList<VirtualNode>,
            VirtualNode
        > FuncRender;
        public Dictionary<string, object> FuncProps;
        public IReadOnlyList<VirtualNode> FuncChildren;
        public IReadOnlyList<PropTypeDefinition> FuncPropTypes;
        public VirtualNode LastRenderedSubtree;
        public UnityEngine.UIElements.VisualElement Container;
        public HostContext HostContext;
        public Reconciler Reconciler;
        public FunctionComponentState ComponentState;
        public List<VirtualNode> PortalPreviousChildren;
        public UnityEngine.UIElements.VisualElement PortalTarget;
        public bool PortalDetachWired;
        public EventCallback<DetachFromPanelEvent> PortalDetachHandler;
        public bool IsFlattened; // true when function component root element is directly mounted without wrapper
        public bool ErrorBoundaryActive;
        public bool ErrorBoundaryShowingFallback;
        public Exception ErrorBoundaryLastException;
        public string ErrorBoundaryResetKey;
        public SuspenseRenderState SuspenseState;
        public Task SuspensePendingTask;
        public object SuspenseTaskLock;
        public int SuspenseTaskVersion;
        public HostContext.ContextFrameHandle InheritedContextFrame;
        public Dictionary<string, object> PendingProvidedContext;
        public IReadOnlyDictionary<string, object> LastProvidedContextSnapshot;
        public int ContextProviderId;

        internal FunctionComponentState EnsureComponentState()
        {
            if (ComponentState == null)
            {
                ComponentState = new FunctionComponentState(this);
            }
            return ComponentState;
        }

        internal void SyncComponentState(FunctionComponentState state)
        {
            if (state == null)
            {
                return;
            }
            ComponentState = state;
        }
    }

    internal sealed class HookStateUpdateQueue
    {
        private HookStateUpdateNode head;
        private HookStateUpdateNode tail;

        public bool HasPending => head != null;

        public void Enqueue(Hooks.PendingStateUpdate update)
        {
            var node = new HookStateUpdateNode(update);
            if (head == null)
            {
                head = tail = node;
            }
            else
            {
                tail.Next = node;
                tail = node;
            }
        }

        public HookStateUpdateNode ConsumeAll()
        {
            var current = head;
            head = null;
            tail = null;
            return current;
        }
    }

    internal sealed class HookStateUpdateNode
    {
        internal HookStateUpdateNode(Hooks.PendingStateUpdate update)
        {
            Update = update;
        }

        public Hooks.PendingStateUpdate Update { get; }
        public HookStateUpdateNode Next { get; set; }
    }

    internal sealed class SuspenseRenderState
    {
        public IReadOnlyList<VirtualNode> LastRenderedChildren;
        public bool ShowingFallback;
    }

    internal sealed class FunctionComponentState
    {
        public FunctionComponentState(NodeMetadata owner)
        {
            Owner = owner;
        }

        public NodeMetadata Owner { get; }
        public List<object> HookStates = new();
        public int HookIndex;
        public List<(
            Func<Action> factory,
            object[] deps,
            object[] lastDeps,
            Action cleanup
        )> FunctionEffects;
        public List<(
            Func<Action> factory,
            object[] deps,
            object[] lastDeps,
            Action cleanup
        )> FunctionLayoutEffects;
        public int EffectIndex;
        public int LayoutEffectIndex;
        public HashSet<ContextKey> SubscribedContextKeys;
        public bool PendingUpdate;
        public bool UpdateQueued;
        public List<string> HookOrderSignatures;
        public bool HookOrderPrimed;
        public bool IsRendering;
        public HashSet<string> StrictDiagnosticsKeys;
        public Dictionary<(int slot, byte kind), Delegate> StateSetterDelegateCache;
        public Dictionary<ContextKey, int> ContextVersions;
        public Dictionary<int, HookStateUpdateQueue> HookStateQueues;
        public Dictionary<int, object> PendingHookStatePreviews;
    }
}
