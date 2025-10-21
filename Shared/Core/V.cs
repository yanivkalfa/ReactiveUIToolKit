using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine.UIElements;
using ReactiveUITK.Props.Typed;

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

        // Raw dictionary overload removed to enforce typed props usage for Button

        public static VirtualNode Button(ButtonProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "Button",
                componentType: null,
                functionRender: null,
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: EmptyChildren()
            );
        }

        // Convenience typed overload for VisualElement styles
        public static VirtualNode VisualElement(
            Style style,
            string key = null,
            params VirtualNode[] children)
        {
            var props = new Dictionary<string, object>(1);
            if (style != null)
            {
                props["style"] = style;
            }
            return VisualElement(props, key, children);
        }

        // Raw dictionary overload removed to enforce typed props usage for TextField

        public static VirtualNode TextField(TextFieldProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "TextField",
                componentType: null,
                functionRender: null,
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: EmptyChildren()
            );
        }

        // Raw dictionary overload removed to enforce typed props usage for ListView

        public static VirtualNode ListView(ListViewProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "ListView",
                componentType: null,
                functionRender: null,
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: EmptyChildren()
            );
        }

        public static VirtualNode Label(LabelProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "Label",
                componentType: null,
                functionRender: null,
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: EmptyChildren()
            );
        }

        public static VirtualNode GroupBox(GroupBoxProps props, string key = null, params VirtualNode[] children)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "GroupBox",
                componentType: null,
                functionRender: null,
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: children ?? EmptyChildren()
            );
        }

        public static VirtualNode Toggle(ToggleProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "Toggle",
                componentType: null,
                functionRender: null,
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: EmptyChildren()
            );
        }

        public static VirtualNode RadioButton(RadioButtonProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "RadioButton",
                componentType: null,
                functionRender: null,
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: EmptyChildren()
            );
        }

        public static VirtualNode RadioButtonGroup(RadioButtonGroupProps props, string key = null, params VirtualNode[] children)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "RadioButtonGroup",
                componentType: null,
                functionRender: null,
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: children ?? EmptyChildren()
            );
        }

        public static VirtualNode ProgressBar(ProgressBarProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "ProgressBar",
                componentType: null,
                functionRender: null,
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: EmptyChildren()
            );
        }

        public static VirtualNode RepeatButton(RepeatButtonProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "RepeatButton",
                componentType: null,
                functionRender: null,
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: EmptyChildren()
            );
        }

        public static VirtualNode Component<TComponent>(
            IReadOnlyDictionary<string, object> componentProps = null,
            string key = null,
            bool memoize = false,
            System.Func<IReadOnlyDictionary<string, object>, IReadOnlyDictionary<string, object>, bool> memoCompare = null,
            params VirtualNode[] children) where TComponent : UnityEngine.MonoBehaviour, ReactiveUITK.Core.IReactiveComponent
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
