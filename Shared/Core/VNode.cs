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
        Host,
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
        public bool Memoize { get; }
        public UnityEngine.UIElements.VisualElement PortalTarget { get; }
        public VirtualNode Fallback { get; }
        public System.Func<bool> SuspenseReady { get; }
        public System.Threading.Tasks.Task SuspenseReadyTask { get; }
        public string TextContent { get; }
        public string Key { get; }
        public IReadOnlyDictionary<string, object> Properties { get; }
        public IReadOnlyList<VirtualNode> Children { get; }
        public VirtualNode ErrorFallback { get; }
        public ErrorEventHandler ErrorHandler { get; }
        public string ErrorResetToken { get; }

        // ── Typed-props path ─────────────────────────────────────────────────
        /// <summary>
        /// Typed render delegate. Set when <c>V.Func&lt;TProps&gt;</c> or <c>V.Func(IProps)</c> is used.
        /// </summary>
        public System.Func<
            IProps,
            IReadOnlyList<VirtualNode>,
            VirtualNode
        > TypedFunctionRender { get; }

        /// <summary>
        /// Typed props instance. Non-null when <c>V.Func&lt;TProps&gt;</c> was used.
        /// </summary>
        public IProps TypedProps { get; }

        /// <summary>
        /// Custom equality delegate for the typed-props path.
        /// Called only when <see cref="TypedFunctionRender"/> is set.
        /// </summary>
        public System.Func<IProps, IProps, bool> TypedMemoCompare { get; }

        public VirtualNode(
            VirtualNodeType nodeType,
            string elementTypeName,
            string textContent,
            string key,
            IReadOnlyDictionary<string, object> properties,
            IReadOnlyList<VirtualNode> children,
            bool memoize = false,
            UnityEngine.UIElements.VisualElement portalTarget = null,
            VirtualNode fallback = null,
            System.Func<bool> suspenseReady = null,
            System.Threading.Tasks.Task suspenseReadyTask = null,
            VirtualNode errorFallback = null,
            ErrorEventHandler errorHandler = null,
            string errorResetToken = null,
            IReadOnlyList<PropTypeDefinition> propTypes = null,
            System.Func<IProps, IReadOnlyList<VirtualNode>, VirtualNode> typedFunctionRender = null,
            IProps typedProps = null,
            System.Func<IProps, IProps, bool> typedMemoCompare = null
        )
        {
            NodeType = nodeType;
            ElementTypeName = elementTypeName;
            TextContent = textContent;
            Key = key;
            Properties = CloneProps(properties);
            Children = CloneChildren(children);
            Memoize = memoize;
            PortalTarget = portalTarget;
            Fallback = fallback;
            SuspenseReady = suspenseReady;
            SuspenseReadyTask = suspenseReadyTask;
            ErrorFallback = errorFallback;
            ErrorHandler = errorHandler;
            ErrorResetToken = errorResetToken;
            PropTypes = ClonePropTypes(propTypes);
            TypedFunctionRender = typedFunctionRender;
            TypedProps = typedProps;
            TypedMemoCompare = typedMemoCompare;
        }

        private VirtualNode(VirtualNode template, IReadOnlyList<PropTypeDefinition> propTypes)
        {
            NodeType = template.NodeType;
            ElementTypeName = template.ElementTypeName;
            Memoize = template.Memoize;
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
            TypedFunctionRender = template.TypedFunctionRender;
            TypedProps = template.TypedProps;
            TypedMemoCompare = template.TypedMemoCompare;
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

    }
}
