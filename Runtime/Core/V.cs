using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine.UIElements;

namespace ReactiveUITK
{
    public static class V
    {
        public static VirtualNode Text(string textContent, string key = null)
        {
            return new VirtualNode(
                VirtualNodeType.Text,
                elementTypeName: null,
                componentType: null,
                functionRender: null,
                textContent: textContent ?? string.Empty,
                key: key,
                properties: EmptyProps(),
                children: EmptyChildren()
            );
        }

        public static VirtualNode VisualElement(
            IReadOnlyDictionary<string, object> elementProperties = null,
            string key = null,
            params VirtualNode[] children)
        {
            elementProperties = CloneStyleDictionary(elementProperties);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "VisualElement",
                componentType: null,
                functionRender: null,
                textContent: null,
                key: key,
                properties: elementProperties ?? EmptyProps(),
                children: children ?? EmptyChildren()
            );
        }

        public static VirtualNode Button(
            IReadOnlyDictionary<string, object> buttonProperties = null,
            string key = null)
        {
            buttonProperties = CloneStyleDictionary(buttonProperties);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "Button",
                componentType: null,
                functionRender: null,
                textContent: null,
                key: key,
                properties: buttonProperties ?? EmptyProps(),
                children: EmptyChildren()
            );
        }

        public static VirtualNode TextField(
            IReadOnlyDictionary<string, object> textFieldProperties = null,
            string key = null)
        {
            textFieldProperties = CloneStyleDictionary(textFieldProperties);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "TextField",
                componentType: null,
                functionRender: null,
                textContent: null,
                key: key,
                properties: textFieldProperties ?? EmptyProps(),
                children: EmptyChildren()
            );
        }

        public static VirtualNode Component<TComponent>(
            IReadOnlyDictionary<string, object> componentProps = null,
            string key = null,
            bool memoize = false,
            System.Func<IReadOnlyDictionary<string, object>, IReadOnlyDictionary<string, object>, bool> memoCompare = null,
            params VirtualNode[] children) where TComponent : ReactiveComponent
        {
            componentProps = CloneStyleDictionary(componentProps);
            return new VirtualNode(
                VirtualNodeType.Component,
                elementTypeName: null,
                componentType: typeof(TComponent),
                functionRender: null,
                textContent: null,
                key: key,
                properties: componentProps ?? EmptyProps(),
                children: children ?? EmptyChildren(),
                memoize: memoize,
                memoCompare: memoCompare
            );
        }

        public static VirtualNode Func(
            System.Func<Dictionary<string, object>, IReadOnlyList<VirtualNode>, VirtualNode> renderFunction,
            IReadOnlyDictionary<string, object> functionProps = null,
            string key = null,
            bool memoize = false,
            System.Func<IReadOnlyDictionary<string, object>, IReadOnlyDictionary<string, object>, bool> memoCompare = null,
            params VirtualNode[] children)
        {
            functionProps = CloneStyleDictionary(functionProps);
            return new VirtualNode(
                VirtualNodeType.FunctionComponent,
                elementTypeName: null,
                componentType: null,
                functionRender: renderFunction,
                textContent: null,
                key: key,
                properties: functionProps ?? EmptyProps(),
                children: children ?? EmptyChildren(),
                memoize: memoize,
                memoCompare: memoCompare
            );
        }

        public static VirtualNode Fragment(string key = null, params VirtualNode[] children)
        {
            return new VirtualNode(
                VirtualNodeType.Fragment,
                elementTypeName: null,
                componentType: null,
                functionRender: null,
                textContent: null,
                key: key,
                properties: EmptyProps(),
                children: children ?? EmptyChildren()
            );
        }

        public static VirtualNode Portal(VisualElement portalTargetElement, string key = null, params VirtualNode[] children)
        {
            return new VirtualNode(
                VirtualNodeType.Portal,
                elementTypeName: null,
                componentType: null,
                functionRender: null,
                textContent: null,
                key: key,
                properties: EmptyProps(),
                children: children ?? EmptyChildren(),
                portalTarget: portalTargetElement
            );
        }

        public static VirtualNode Suspense(System.Func<bool> isReady, VirtualNode fallbackNode, string key = null, params VirtualNode[] children)
        {
            return new VirtualNode(
                VirtualNodeType.Suspense,
                elementTypeName: null,
                componentType: null,
                functionRender: null,
                textContent: null,
                key: key,
                properties: EmptyProps(),
                children: children ?? EmptyChildren(),
                suspenseReady: isReady,
                fallback: fallbackNode
            );
        }

        private static IReadOnlyDictionary<string, object> EmptyProps() => new Dictionary<string, object>(0);

        private static IReadOnlyList<VirtualNode> EmptyChildren() => new List<VirtualNode>(0);

        private static IReadOnlyDictionary<string, object> CloneStyleDictionary(IReadOnlyDictionary<string, object> source)
        {
            if (source == null)
            {
                return null;
            }
            if (!source.ContainsKey("style"))
            {
                return source;
            }
            if (source["style"] is not IDictionary<string, object> styleMap)
            {
                return source;
            }
            var outer = new Dictionary<string, object>(source.Count);
            foreach (var kv in source)
            {
                outer[kv.Key] = kv.Value;
            }
            var styleClone = new Dictionary<string, object>(styleMap.Count);
            foreach (var kv in styleMap)
            {
                styleClone[kv.Key] = kv.Value;
            }
            outer["style"] = styleClone;
            return outer;
        }
    }
}
