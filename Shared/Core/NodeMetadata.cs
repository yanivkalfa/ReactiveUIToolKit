using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReactiveUITK.Core.Fiber;
using UnityEngine.UIElements;

namespace ReactiveUITK.Core
{
    internal sealed class NodeMetadata
    {
        public string Key;

        public Dictionary<string, Delegate> EventHandlers = new();

        public Dictionary<string, Delegate> EventHandlerTargets = new();
        public Dictionary<string, string> EventHandlerSignatures = new();
        public object AttachedRef;

        public UnityEngine.UIElements.VisualElement Container;
        public HostContext HostContext;
        public FunctionComponentState ComponentState;

        internal FunctionComponentState EnsureComponentState()
        {
            if (ComponentState == null)
            {
                ComponentState = new FunctionComponentState(this);
                ComponentState.HostContext = HostContext;
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
        public HostContext HostContext;
        public FiberNode Fiber;
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
        public bool PendingUpdate;
        public bool UpdateQueued;
        public Action OnStateUpdated; // Bridge for Fiber reconciler
        public List<string> HookOrderSignatures;
        public bool HookOrderPrimed;
        public bool IsRendering;
        public HashSet<string> StrictDiagnosticsKeys;
        public Dictionary<(int slot, byte kind), Delegate> StateSetterDelegateCache;
        public Dictionary<int, HookStateUpdateQueue> HookStateQueues;
        public Dictionary<int, object> PendingHookStatePreviews;
        public SuspenseRenderState SuspenseState;
        public Task SuspensePendingTask;
        public object SuspenseTaskLock;
        public int SuspenseTaskVersion;
        
        // React-style context dependency tracking
        public List<ContextDependency> ContextDependencies;
    }

    internal readonly struct ContextDependency
    {
        public readonly string Key;
        public readonly object Value;

        public ContextDependency(string key, object value)
        {
            Key = key;
            Value = value;
        }
    }
}
