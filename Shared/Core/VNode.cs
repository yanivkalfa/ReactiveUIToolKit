using System.Collections.Generic;

namespace ReactiveUITK.Core
{
    public enum VirtualNodeType
    {
        Element,
        Text,
        Component,
        FunctionComponent,
        Fragment,
        Portal,
        Suspense
    }

    public sealed class VirtualNode
    {
        public VirtualNodeType NodeType { get; set; }
        public string ElementTypeName { get; set; }
        public System.Type ComponentType { get; set; }
        public System.Func<Dictionary<string, object>, IReadOnlyList<VirtualNode>, VirtualNode> FunctionRender { get; set; }
        public bool Memoize { get; set; }
        public System.Func<IReadOnlyDictionary<string, object>, IReadOnlyDictionary<string, object>, bool> MemoCompare { get; set; }
        public UnityEngine.UIElements.VisualElement PortalTarget { get; set; }
        public VirtualNode Fallback { get; set; }
        public System.Func<bool> SuspenseReady { get; set; }
        public System.Threading.Tasks.Task<bool> SuspenseReadyTask { get; set; }
        public string TextContent { get; set; }
        public string Key { get; set; }
        public IReadOnlyDictionary<string, object> Properties { get; set; }
        public IReadOnlyList<VirtualNode> Children { get; set; }

        public VirtualNode(
            VirtualNodeType nodeType,
            string elementTypeName,
            System.Type componentType,
            System.Func<Dictionary<string, object>, IReadOnlyList<VirtualNode>, VirtualNode> functionRender,
            string textContent,
            string key,
            IReadOnlyDictionary<string, object> properties,
            IReadOnlyList<VirtualNode> children,
            bool memoize = false,
            System.Func<IReadOnlyDictionary<string, object>, IReadOnlyDictionary<string, object>, bool> memoCompare = null,
            UnityEngine.UIElements.VisualElement portalTarget = null,
            VirtualNode fallback = null,
            System.Func<bool> suspenseReady = null,
            System.Threading.Tasks.Task<bool> suspenseReadyTask = null)
        {
            NodeType = nodeType;
            ElementTypeName = elementTypeName;
            ComponentType = componentType;
            FunctionRender = functionRender;
            TextContent = textContent;
            Key = key;
            Properties = properties;
            Children = children;
            Memoize = memoize;
            MemoCompare = memoCompare;
            PortalTarget = portalTarget;
            Fallback = fallback;
            SuspenseReady = suspenseReady;
            SuspenseReadyTask = suspenseReadyTask;
        }
    }
}
