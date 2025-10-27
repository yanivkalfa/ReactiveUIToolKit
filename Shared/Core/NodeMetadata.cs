using System;
using System.Collections.Generic;

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

        // Restored original System.Func signature for compatibility
        public System.Func<
            Dictionary<string, object>,
            IReadOnlyList<VirtualNode>,
            VirtualNode
        > FuncRender;
        public Dictionary<string, object> FuncProps;
        public IReadOnlyList<VirtualNode> FuncChildren;
        public List<object> HookStates;
        public int HookIndex;
        public VirtualNode LastRenderedSubtree;
        public UnityEngine.UIElements.VisualElement Container;
        public HostContext HostContext;
        public Reconciler Reconciler;
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

        // Independent indices for effect lists to avoid interference with state/reducer hook index
        public int EffectIndex;
        public int LayoutEffectIndex;
        public HashSet<string> SubscribedContextKeys;
        public List<VirtualNode> PortalPreviousChildren;
        public bool IsFlattened; // true when function component root element is directly mounted without wrapper
        public bool IsRendering; // re-entrancy guard for function components
        public bool PendingUpdate; // schedule one update after commit
        public bool UpdateQueued; // legacy field retained for compatibility with prior code paths
    }
}
