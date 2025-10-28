using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Core.Util;
using ReactiveUITK.Core.AnimationComponents;
using ReactiveUITK.Props.Typed;
using UnityEngine;
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
            params VirtualNode[] children
        )
        {
            elementProperties = CloneStyleDictionary(elementProperties);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "VisualElement",
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
            params VirtualNode[] children
        )
        {
            var props = new Dictionary<string, object>(1);
            if (style != null)
            {
                props["style"] = style;
            }
            return VisualElement(props, key, children);
        }

        // VisualElementSafe: applies safe-area padding as a minimum, without overriding larger paddings the user specifies.
        public static VirtualNode VisualElementSafe(
            Style style = null,
            string key = null,
            params VirtualNode[] children
        )
        {
            var insets = SafeAreaUtility.GetInsets();
            // Build a style where padding = Max(userPadding, safeInset)
            float GetUser(string k)
            {
                if (style == null)
                    return 0f;
                if (style.TryGetValue(k, out var v) && v is float f)
                    return f;
                return 0f;
            }

            var merged = new Style
            {
                (
                    Props.Typed.StyleKeys.PaddingLeft,
                    Mathf.Max(GetUser(Props.Typed.StyleKeys.PaddingLeft), insets.Left)
                ),
                (
                    Props.Typed.StyleKeys.PaddingRight,
                    Mathf.Max(GetUser(Props.Typed.StyleKeys.PaddingRight), insets.Right)
                ),
                (
                    Props.Typed.StyleKeys.PaddingTop,
                    Mathf.Max(GetUser(Props.Typed.StyleKeys.PaddingTop), insets.Top)
                ),
                (
                    Props.Typed.StyleKeys.PaddingBottom,
                    Mathf.Max(GetUser(Props.Typed.StyleKeys.PaddingBottom), insets.Bottom)
                ),
            };
            // Merge any remaining user styles on top (they can override unrelated keys)
            if (style != null)
            {
                foreach (var kv in style)
                {
                    if (
                        kv.Key == Props.Typed.StyleKeys.PaddingLeft
                        || kv.Key == Props.Typed.StyleKeys.PaddingRight
                        || kv.Key == Props.Typed.StyleKeys.PaddingTop
                        || kv.Key == Props.Typed.StyleKeys.PaddingBottom
                    )
                    {
                        // Keep the max already set
                        continue;
                    }
                    merged[kv.Key] = kv.Value;
                }
            }
            return VisualElement(merged, key, children);
        }

        // Raw dictionary overload removed to enforce typed props usage for TextField

        public static VirtualNode TextField(TextFieldProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "TextField",
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
                functionRender: null,
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: EmptyChildren()
            );
        }

        public static VirtualNode MultiColumnListView(MultiColumnListViewProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "MultiColumnListView",
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
                functionRender: null,
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: EmptyChildren()
            );
        }

        public static VirtualNode GroupBox(
            GroupBoxProps props,
            string key = null,
            params VirtualNode[] children
        )
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "GroupBox",
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
                functionRender: null,
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: EmptyChildren()
            );
        }

        public static VirtualNode RadioButtonGroup(
            RadioButtonGroupProps props,
            string key = null,
            params VirtualNode[] children
        )
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "RadioButtonGroup",
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
                functionRender: null,
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: EmptyChildren()
            );
        }

        public static VirtualNode Image(ImageProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "Image",
                functionRender: null,
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: EmptyChildren()
            );
        }

        public static VirtualNode HelpBox(HelpBoxProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "HelpBox",
                functionRender: null,
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: EmptyChildren()
            );
        }

        public static VirtualNode ScrollView(
            ScrollViewProps props,
            string key = null,
            params VirtualNode[] children
        )
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "ScrollView",
                functionRender: null,
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: children ?? EmptyChildren()
            );
        }

        public static VirtualNode Slider(SliderProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "Slider",
                functionRender: null,
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: EmptyChildren()
            );
        }

        public static VirtualNode SliderInt(SliderIntProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "SliderInt",
                functionRender: null,
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: EmptyChildren()
            );
        }

        public static VirtualNode DropdownField(DropdownFieldProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "DropdownField",
                functionRender: null,
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: EmptyChildren()
            );
        }

        public static VirtualNode Foldout(
            FoldoutProps props,
            string key = null,
            params VirtualNode[] children
        )
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "Foldout",
                functionRender: null,
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: children ?? EmptyChildren()
            );
        }

        public static VirtualNode Func(
            System.Func<
                Dictionary<string, object>,
                IReadOnlyList<VirtualNode>,
                VirtualNode
            > renderFunction,
            IReadOnlyDictionary<string, object> functionProps = null,
            string key = null,
            bool memoize = false,
            System.Func<
                IReadOnlyDictionary<string, object>,
                IReadOnlyDictionary<string, object>,
                bool
            > memoCompare = null,
            params VirtualNode[] children
        )
        {
            functionProps = CloneStyleDictionary(functionProps);
            return new VirtualNode(
                VirtualNodeType.FunctionComponent,
                elementTypeName: null,
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
                functionRender: null,
                textContent: null,
                key: key,
                properties: EmptyProps(),
                children: children ?? EmptyChildren()
            );
        }

        public static VirtualNode Portal(
            VisualElement portalTargetElement,
            string key = null,
            params VirtualNode[] children
        )
        {
            return new VirtualNode(
                VirtualNodeType.Portal,
                elementTypeName: null,
                functionRender: null,
                textContent: null,
                key: key,
                properties: EmptyProps(),
                children: children ?? EmptyChildren(),
                portalTarget: portalTargetElement
            );
        }

        // Animate wrapper component: applies style animations to a wrapper element and renders children inside.
        public static VirtualNode Animate(AnimateProps props, string key = null, params VirtualNode[] children)
        {
            IReadOnlyDictionary<string, object> map = CloneStyleDictionary(props?.ToDictionary());
            var enriched = new Dictionary<string, object>();
            if (map != null)
            {
                foreach (var kv in map)
                {
                    enriched[kv.Key] = kv.Value;
                }
            }
            // Feed children array by reference into props so function component never shallow-skips
            // when inner content changes (Reconciler memoizes function components on props+children shape).
            enriched["__childRef"] = children;
            return Func(AnimateFunc.Render, enriched, key, false, null, children);
        }

        public static VirtualNode Suspense(
            System.Func<bool> isReady,
            VirtualNode fallbackNode,
            string key = null,
            params VirtualNode[] children
        )
        {
            return new VirtualNode(
                VirtualNodeType.Suspense,
                elementTypeName: null,
                functionRender: null,
                textContent: null,
                key: key,
                properties: EmptyProps(),
                children: children ?? EmptyChildren(),
                suspenseReady: isReady,
                fallback: fallbackNode
            );
        }

        private static IReadOnlyDictionary<string, object> EmptyProps() =>
            new Dictionary<string, object>(0);

        private static IReadOnlyList<VirtualNode> EmptyChildren() => new List<VirtualNode>(0);

        private static IReadOnlyDictionary<string, object> CloneStyleDictionary(
            IReadOnlyDictionary<string, object> source
        )
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
