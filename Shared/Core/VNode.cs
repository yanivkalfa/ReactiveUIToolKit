using System;
using System.Collections.Generic;

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
        public VirtualNodeType NodeType { get; set; }
        public string ElementTypeName { get; set; }
        public System.Func<
            Dictionary<string, object>,
            IReadOnlyList<VirtualNode>,
            VirtualNode
        > FunctionRender { get; set; }
        public bool Memoize { get; set; }
        public System.Func<
            IReadOnlyDictionary<string, object>,
            IReadOnlyDictionary<string, object>,
            bool
        > MemoCompare { get; set; }
        public UnityEngine.UIElements.VisualElement PortalTarget { get; set; }
        public VirtualNode Fallback { get; set; }
        public System.Func<bool> SuspenseReady { get; set; }
    public System.Threading.Tasks.Task SuspenseReadyTask { get; set; }
        public string TextContent { get; set; }
        public string Key { get; set; }
        public IReadOnlyDictionary<string, object> Properties { get; set; }
        public IReadOnlyList<VirtualNode> Children { get; set; }
        public VirtualNode ErrorFallback { get; set; }
        public Action<Exception> ErrorHandler { get; set; }
        public string ErrorResetToken { get; set; }

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
            Properties = properties;
            Children = children;
            Memoize = memoize;
            MemoCompare = memoCompare;
            PortalTarget = portalTarget;
            Fallback = fallback;
            SuspenseReady = suspenseReady;
            SuspenseReadyTask = suspenseReadyTask;
            ErrorFallback = errorFallback;
            ErrorHandler = errorHandler;
            ErrorResetToken = errorResetToken;
            PropTypes = propTypes;
        }

        public IReadOnlyList<PropTypeDefinition> PropTypes { get; set; }

        // Implicit conversion: function component -> VirtualNode
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
