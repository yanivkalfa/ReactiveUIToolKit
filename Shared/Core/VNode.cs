using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ReactiveUITK.Core
{
    public enum VirtualNodeType
    {
        Element,
        Text,
        FunctionComponent,
        Fragment,
        Portal,
        Suspense,
        ErrorBoundary,
    }

    public sealed class VirtualNode
    {
        private static readonly IReadOnlyDictionary<string, object> EmptyPropsInstance =
            new ReadOnlyDictionary<string, object>(new Dictionary<string, object>(0));
        private static readonly IReadOnlyList<VirtualNode> EmptyChildrenInstance =
            new ReadOnlyCollection<VirtualNode>(Array.Empty<VirtualNode>());
        private static readonly IReadOnlyList<PropTypeDefinition> EmptyPropTypesInstance =
            new ReadOnlyCollection<PropTypeDefinition>(Array.Empty<PropTypeDefinition>());

        public VirtualNodeType NodeType { get; }
        public string ElementTypeName { get; }
        public System.Func<
            Dictionary<string, object>,
            IReadOnlyList<VirtualNode>,
            VirtualNode
        > FunctionRender { get; }
        public bool Memoize { get; }
        public System.Func<
            IReadOnlyDictionary<string, object>,
            IReadOnlyDictionary<string, object>,
            bool
        > MemoCompare { get; }
        public UnityEngine.UIElements.VisualElement PortalTarget { get; }
        public VirtualNode Fallback { get; }
        public System.Func<bool> SuspenseReady { get; }
        public System.Threading.Tasks.Task SuspenseReadyTask { get; }
        public string TextContent { get; }
        public string Key { get; }
        public IReadOnlyDictionary<string, object> Properties { get; }
        public IReadOnlyList<VirtualNode> Children { get; }
        public VirtualNode ErrorFallback { get; }
        public Action<Exception> ErrorHandler { get; }
        public string ErrorResetToken { get; }

        public VirtualNode(
            VirtualNodeType nodeType,
            string elementTypeName,
            System.Func<
                Dictionary<string, object>,
                IReadOnlyList<VirtualNode>,
                VirtualNode
            > functionRender,
            string textContent,
            string key,
            IReadOnlyDictionary<string, object> properties,
            IReadOnlyList<VirtualNode> children,
            bool memoize = false,
            System.Func<
                IReadOnlyDictionary<string, object>,
                IReadOnlyDictionary<string, object>,
                bool
            > memoCompare = null,
            UnityEngine.UIElements.VisualElement portalTarget = null,
            VirtualNode fallback = null,
            System.Func<bool> suspenseReady = null,
            System.Threading.Tasks.Task suspenseReadyTask = null,
            VirtualNode errorFallback = null,
            Action<Exception> errorHandler = null,
            string errorResetToken = null,
            IReadOnlyList<PropTypeDefinition> propTypes = null
        )
        {
            NodeType = nodeType;
            ElementTypeName = elementTypeName;
            FunctionRender = functionRender;
            TextContent = textContent;
            Key = key;
            Properties = CloneProps(properties);
            Children = CloneChildren(children);
            Memoize = memoize;
            MemoCompare = memoCompare;
            PortalTarget = portalTarget;
            Fallback = fallback;
            SuspenseReady = suspenseReady;
            SuspenseReadyTask = suspenseReadyTask;
            ErrorFallback = errorFallback;
            ErrorHandler = errorHandler;
            ErrorResetToken = errorResetToken;
            PropTypes = ClonePropTypes(propTypes);
        }

        private VirtualNode(VirtualNode template, IReadOnlyList<PropTypeDefinition> propTypes)
        {
            NodeType = template.NodeType;
            ElementTypeName = template.ElementTypeName;
            FunctionRender = template.FunctionRender;
            Memoize = template.Memoize;
            MemoCompare = template.MemoCompare;
            PortalTarget = template.PortalTarget;
            Fallback = template.Fallback;
            SuspenseReady = template.SuspenseReady;
            SuspenseReadyTask = template.SuspenseReadyTask;
            TextContent = template.TextContent;
            Key = template.Key;
            Properties = template.Properties;
            Children = template.Children;
            ErrorFallback = template.ErrorFallback;
            ErrorHandler = template.ErrorHandler;
            ErrorResetToken = template.ErrorResetToken;
            PropTypes = ClonePropTypes(propTypes);
        }

        public IReadOnlyList<PropTypeDefinition> PropTypes { get; }

        internal static IReadOnlyDictionary<string, object> EmptyProps => EmptyPropsInstance;

        internal static IReadOnlyList<VirtualNode> EmptyChildren => EmptyChildrenInstance;

        internal VirtualNode WithPropTypesImmutable(IReadOnlyList<PropTypeDefinition> propTypes)
        {
            if (propTypes == null || propTypes.Count == 0)
            {
                return this;
            }
            return new VirtualNode(this, propTypes);
        }

        private static IReadOnlyDictionary<string, object> CloneProps(
            IReadOnlyDictionary<string, object> props
        )
        {
            if (props == null || props.Count == 0)
            {
                return EmptyPropsInstance;
            }
            if (ReferenceEquals(props, EmptyPropsInstance))
            {
                return EmptyPropsInstance;
            }
            var dict = new Dictionary<string, object>(props.Count);
            foreach (var kvp in props)
            {
                dict[kvp.Key] = kvp.Value;
            }
            return new ReadOnlyDictionary<string, object>(dict);
        }

        private static IReadOnlyList<VirtualNode> CloneChildren(IReadOnlyList<VirtualNode> children)
        {
            if (children == null || children.Count == 0)
            {
                return EmptyChildrenInstance;
            }
            var buffer = new VirtualNode[children.Count];
            for (int i = 0; i < children.Count; i++)
            {
                buffer[i] = children[i];
            }
            return Array.AsReadOnly(buffer);
        }

        private static IReadOnlyList<PropTypeDefinition> ClonePropTypes(
            IReadOnlyList<PropTypeDefinition> propTypes
        )
        {
            if (propTypes == null || propTypes.Count == 0)
            {
                return EmptyPropTypesInstance;
            }
            var buffer = new PropTypeDefinition[propTypes.Count];
            for (int i = 0; i < propTypes.Count; i++)
            {
                buffer[i] = propTypes[i];
            }
            return Array.AsReadOnly(buffer);
        }

        public static implicit operator VirtualNode(
            System.Func<
                Dictionary<string, object>,
                IReadOnlyList<VirtualNode>,
                VirtualNode
            > renderFunction
        )
        {
            if (renderFunction == null)
            {
                return null;
            }
            return ReactiveUITK.V.Func(renderFunction);
        }
    }
}
