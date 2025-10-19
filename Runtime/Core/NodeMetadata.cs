using System;
using System.Collections.Generic;

namespace ReactiveUITK.Core
{
    internal sealed class NodeMetadata
    {
        public string Key;
        public Dictionary<string, Delegate> EventHandlers = new();
        public ReactiveComponent ComponentInstance;
        public System.Func<Dictionary<string, object>, IReadOnlyList<VirtualNode>, VirtualNode> FuncRender;
        public Dictionary<string, object> FuncProps;
        public IReadOnlyList<VirtualNode> FuncChildren;
        public List<object> HookStates;
        public int HookIndex;
        public VirtualNode LastRenderedSubtree;
        public UnityEngine.UIElements.VisualElement Container;
        public HostContext HostContext;
        public Reconciler Reconciler;
        public List<(System.Func<System.Action> factory, object[] deps, object[] lastDeps, System.Action cleanup)> FunctionEffects;
        public List<(System.Func<System.Action> factory, object[] deps, object[] lastDeps, System.Action cleanup)> FunctionLayoutEffects;
        public HashSet<string> SubscribedContextKeys;
        public List<VirtualNode> PortalPreviousChildren;
		public bool IsFlattened; // true when function component root element is directly mounted without wrapper
    }
}
