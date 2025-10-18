using System;
using System.Collections.Generic;

namespace ReactiveUITK.Core
{
    internal sealed class NodeMetadata
    {
        public string Key;
        public Dictionary<string, Delegate> EventHandlers = new Dictionary<string, Delegate>();
        public ReactiveComponent ComponentInstance; // only for component virtual nodes
        // Function component support
        public System.Func<Dictionary<string, object>, System.Collections.Generic.IReadOnlyList<Core.VirtualNode>, Core.VirtualNode> FuncRender;
        public Dictionary<string, object> FuncProps;
        public System.Collections.Generic.IReadOnlyList<Core.VirtualNode> FuncChildren;
        public System.Collections.Generic.List<object> HookStates;
        public int HookIndex;
        public Core.VirtualNode LastRenderedSubtree;
        public UnityEngine.UIElements.VisualElement Container; // host container for function component
        public HostContext HostContext; // context access
        public Reconciler Reconciler; // for scheduling re-renders
		// Effects registered by function components (UseEffect hook)
		public System.Collections.Generic.List<(System.Func<System.Action> factory, object[] deps, object[] lastDeps, System.Action cleanup)> FunctionEffects;
		// Layout effects (run immediately after render diff, before paint)
		public System.Collections.Generic.List<(System.Func<System.Action> factory, object[] deps, object[] lastDeps, System.Action cleanup)> FunctionLayoutEffects;
		// Context keys this function component subscribed to
		public System.Collections.Generic.HashSet<string> SubscribedContextKeys;
		// Portal previous children snapshot for incremental diff
		public System.Collections.Generic.List<Core.VirtualNode> PortalPreviousChildren;
    }
}
