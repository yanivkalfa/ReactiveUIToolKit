using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine.UIElements;

namespace ReactiveUITK
{
    public static class V
    {
        public static VirtualNode Text(string text, string key = null)
        {
            return new VirtualNode(
                VirtualNodeType.Text,
                elementTypeName: null,
                componentType: null,
                functionRender: null,
                textContent: text ?? string.Empty,
                key: key,
                properties: EmptyProps(),
                children: EmptyChildren()
            );
        }

        public static VirtualNode View(
            IReadOnlyDictionary<string, object> properties = null,
            string key = null,
            params VirtualNode[] children)
        {
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "VisualElement",
                componentType: null,
                functionRender: null,
                textContent: null,
                key: key,
                properties: properties ?? EmptyProps(),
                children: children ?? EmptyChildren()
            );
        }

        public static VirtualNode Component<TComponent>(
            IReadOnlyDictionary<string, object> props = null,
            string key = null,
            bool memo = false,
            System.Func<IReadOnlyDictionary<string, object>, IReadOnlyDictionary<string, object>, bool> memoCompare = null,
            params VirtualNode[] children) where TComponent : ReactiveComponent
        {
            return new VirtualNode(
                VirtualNodeType.Component,
                elementTypeName: null,
                componentType: typeof(TComponent),
                functionRender: null,
                textContent: null,
                key: key,
                properties: props ?? EmptyProps(),
                children: children ?? EmptyChildren(),
                memoize: memo,
                memoCompare: memoCompare
            );
        }

        public static VirtualNode Func(
            System.Func<Dictionary<string, object>, IReadOnlyList<VirtualNode>, VirtualNode> renderer,
            IReadOnlyDictionary<string, object> props = null,
            string key = null,
            bool memo = false,
            System.Func<IReadOnlyDictionary<string, object>, IReadOnlyDictionary<string, object>, bool> memoCompare = null,
            params VirtualNode[] children)
        {
            return new VirtualNode(
                VirtualNodeType.FunctionComponent,
                elementTypeName: null,
                componentType: null,
                functionRender: renderer,
                textContent: null,
                key: key,
                properties: props ?? EmptyProps(),
                children: children ?? EmptyChildren(),
                memoize: memo,
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

        public static VirtualNode Portal(VisualElement target, string key = null, params VirtualNode[] children)
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
                portalTarget: target
            );
        }

        public static VirtualNode Suspense(System.Func<bool> ready, VirtualNode fallback, string key = null, params VirtualNode[] children)
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
                suspenseReady: ready,
                fallback: fallback
            );
        }

        private static IReadOnlyDictionary<string, object> EmptyProps()
        {
            return new Dictionary<string, object>(0);
        }

        private static IReadOnlyList<VirtualNode> EmptyChildren()
        {
            return new List<VirtualNode>(0);
        }
    }
}
