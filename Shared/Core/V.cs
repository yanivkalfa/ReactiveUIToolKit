using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReactiveUITK.Core;
using ReactiveUITK.Core.AnimationComponents;
using ReactiveUITK.Core.Util;
using ReactiveUITK.Props.Typed;
using ReactiveUITK.Router;
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
                textContent: textContent ?? string.Empty,
                key: key,
                properties: EmptyProps(),
                children: EmptyChildren()
            );
        }

        public static VirtualNode VisualElement(
            VisualElementProps props = null,
            string key = null,
            params VirtualNode[] children
        )
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "VisualElement",
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: children ?? EmptyChildren()
            );
        }

        public static VirtualNode Button(ButtonProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "Button",
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: EmptyChildren()
            );
        }

        public static VirtualNode Tab(TabProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "Tab",
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: EmptyChildren()
            );
        }

        public static VirtualNode TabView(TabViewProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "TabView",
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: EmptyChildren()
            );
        }

        public static VirtualNode VisualElementSafe(
            object elementPropsOrStyle = null,
            string key = null,
            params VirtualNode[] children
        )
        {
            Dictionary<string, object> props;
            Style userStyle = null;

            switch (elementPropsOrStyle)
            {
                case null:
                    props = new Dictionary<string, object>();
                    break;

                case Style style:
                    props = new Dictionary<string, object>();
                    userStyle = style;
                    break;

                case BaseProps baseProps:
                    props = new Dictionary<string, object>(baseProps.ToDictionary());
                    if (
                        props.TryGetValue("style", out var basePropStyleObj)
                        && basePropStyleObj is Style basePropStyle
                    )
                    {
                        userStyle = basePropStyle;
                    }
                    break;

                case IReadOnlyDictionary<string, object> dictionary:
                    props = new Dictionary<string, object>(dictionary);
                    if (
                        props.TryGetValue("style", out var styleObj)
                        && styleObj is Style existingStyle
                    )
                    {
                        userStyle = existingStyle;
                    }
                    break;

                default:
                    throw new ArgumentException(
                        "VisualElementSafe expects either a Style, a BaseProps, or a props dictionary as the first argument.",
                        nameof(elementPropsOrStyle)
                    );
            }

            props["style"] = BuildSafeAreaStyle(userStyle);

            IReadOnlyDictionary<string, object> finalProps = CloneStyleDictionary(props);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "VisualElement",
                textContent: null,
                key: key,
                properties: finalProps ?? EmptyProps(),
                children: children ?? EmptyChildren()
            );
        }

        public static VirtualNode TextField(TextFieldProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "TextField",
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: EmptyChildren()
            );
        }

        public static VirtualNode EnumField(EnumFieldProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                "EnumField",
                null,
                key,
                map ?? EmptyProps(),
                EmptyChildren()
            );
        }

        private static Style BuildSafeAreaStyle(Style originalStyle)
        {
            var insets = SafeAreaUtility.GetInsets();

            float GetUserValue(string key)
            {
                if (
                    originalStyle != null
                    && originalStyle.TryGetValue(key, out var value)
                    && value is float f
                )
                {
                    return f;
                }
                return 0f;
            }

            var merged = new Style
            {
                (
                    Props.Typed.StyleKeys.PaddingLeft,
                    Mathf.Max(GetUserValue(Props.Typed.StyleKeys.PaddingLeft), insets.Left)
                ),
                (
                    Props.Typed.StyleKeys.PaddingRight,
                    Mathf.Max(GetUserValue(Props.Typed.StyleKeys.PaddingRight), insets.Right)
                ),
                (
                    Props.Typed.StyleKeys.PaddingTop,
                    Mathf.Max(GetUserValue(Props.Typed.StyleKeys.PaddingTop), insets.Top)
                ),
                (
                    Props.Typed.StyleKeys.PaddingBottom,
                    Mathf.Max(GetUserValue(Props.Typed.StyleKeys.PaddingBottom), insets.Bottom)
                ),
            };

            if (originalStyle != null)
            {
                foreach (var kv in originalStyle)
                {
                    if (
                        kv.Key == Props.Typed.StyleKeys.PaddingLeft
                        || kv.Key == Props.Typed.StyleKeys.PaddingRight
                        || kv.Key == Props.Typed.StyleKeys.PaddingTop
                        || kv.Key == Props.Typed.StyleKeys.PaddingBottom
                    )
                    {
                        continue;
                    }
                    merged[kv.Key] = kv.Value;
                }
            }

            return merged;
        }

        public static VirtualNode ObjectField(ObjectFieldProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                "ObjectField",
                null,
                key,
                map ?? EmptyProps(),
                EmptyChildren()
            );
        }

        public static VirtualNode Scroller(ScrollerProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                "Scroller",
                null,
                key,
                map ?? EmptyProps(),
                EmptyChildren()
            );
        }

        public static VirtualNode TextElement(TextElementProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                "TextElement",
                null,
                key,
                map ?? EmptyProps(),
                EmptyChildren()
            );
        }

        public static VirtualNode IMGUIContainer(IMGUIContainerProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                "IMGUIContainer",
                null,
                key,
                map ?? EmptyProps(),
                EmptyChildren()
            );
        }

        public static VirtualNode Vector2IntField(Vector2IntFieldProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                "Vector2IntField",
                null,
                key,
                map ?? EmptyProps(),
                EmptyChildren()
            );
        }

        public static VirtualNode Vector3IntField(Vector3IntFieldProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                "Vector3IntField",
                null,
                key,
                map ?? EmptyProps(),
                EmptyChildren()
            );
        }

        public static VirtualNode RectField(RectFieldProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                "RectField",
                null,
                key,
                map ?? EmptyProps(),
                EmptyChildren()
            );
        }

        public static VirtualNode RectIntField(RectIntFieldProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                "RectIntField",
                null,
                key,
                map ?? EmptyProps(),
                EmptyChildren()
            );
        }

        public static VirtualNode BoundsField(BoundsFieldProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                "BoundsField",
                null,
                key,
                map ?? EmptyProps(),
                EmptyChildren()
            );
        }

        public static VirtualNode MinMaxSlider(MinMaxSliderProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                "MinMaxSlider",
                null,
                key,
                map ?? EmptyProps(),
                EmptyChildren()
            );
        }

        public static VirtualNode TemplateContainer(
            TemplateContainerProps props,
            string key = null,
            params VirtualNode[] children
        )
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                "TemplateContainer",
                null,
                key,
                map ?? EmptyProps(),
                children ?? EmptyChildren()
            );
        }

        public static VirtualNode BoundsIntField(BoundsIntFieldProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                "BoundsIntField",
                null,
                key,
                map ?? EmptyProps(),
                EmptyChildren()
            );
        }

        public static VirtualNode EnumFlagsField(EnumFlagsFieldProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                "EnumFlagsField",
                null,
                key,
                map ?? EmptyProps(),
                EmptyChildren()
            );
        }

        public static VirtualNode ToggleButtonGroup(
            ToggleButtonGroupProps props,
            string key = null,
            params VirtualNode[] children
        )
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                "ToggleButtonGroup",
                null,
                key,
                map ?? EmptyProps(),
                children ?? EmptyChildren()
            );
        }

        public static VirtualNode Hash128Field(Hash128FieldProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                "Hash128Field",
                null,
                key,
                map ?? EmptyProps(),
                EmptyChildren()
            );
        }

        // Editor-only elements are registered behind UNITY_EDITOR; these helpers are safe to call.
        public static VirtualNode Toolbar(
            ToolbarProps props,
            string key = null,
            params VirtualNode[] children
        )
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                "Toolbar",
                null,
                key,
                map ?? EmptyProps(),
                children ?? EmptyChildren()
            );
        }

        public static VirtualNode ToolbarButton(ToolbarButtonProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                "ToolbarButton",
                null,
                key,
                map ?? EmptyProps(),
                EmptyChildren()
            );
        }

        public static VirtualNode ToolbarToggle(ToolbarToggleProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                "ToolbarToggle",
                null,
                key,
                map ?? EmptyProps(),
                EmptyChildren()
            );
        }

        public static VirtualNode ToolbarMenu(ToolbarMenuProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                "ToolbarMenu",
                null,
                key,
                map ?? EmptyProps(),
                EmptyChildren()
            );
        }

        public static VirtualNode ToolbarBreadcrumbs(
            ToolbarBreadcrumbsProps props,
            string key = null
        )
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                "ToolbarBreadcrumbs",
                null,
                key,
                map ?? EmptyProps(),
                EmptyChildren()
            );
        }

        public static VirtualNode ToolbarPopupSearchField(
            ToolbarPopupSearchFieldProps props,
            string key = null
        )
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                "ToolbarPopupSearchField",
                null,
                key,
                map ?? EmptyProps(),
                EmptyChildren()
            );
        }

        public static VirtualNode ToolbarSearchField(
            ToolbarSearchFieldProps props,
            string key = null
        )
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                "ToolbarSearchField",
                null,
                key,
                map ?? EmptyProps(),
                EmptyChildren()
            );
        }

        public static VirtualNode ToolbarSpacer(ToolbarSpacerProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                "ToolbarSpacer",
                null,
                key,
                map ?? EmptyProps(),
                EmptyChildren()
            );
        }

        public static VirtualNode PropertyField(PropertyFieldProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                "PropertyField",
                null,
                key,
                map ?? EmptyProps(),
                EmptyChildren()
            );
        }

        public static VirtualNode InspectorElement(InspectorElementProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                "InspectorElement",
                null,
                key,
                map ?? EmptyProps(),
                EmptyChildren()
            );
        }

#if UNITY_EDITOR
        public static VirtualNode TwoPaneSplitView(
            TwoPaneSplitViewProps props,
            string key = null,
            params VirtualNode[] children
        )
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                "TwoPaneSplitView",
                null,
                key,
                map ?? EmptyProps(),
                children ?? EmptyChildren()
            );
        }
#endif

        public static VirtualNode FloatField(FloatFieldProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "FloatField",
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: EmptyChildren()
            );
        }

        public static VirtualNode IntegerField(IntegerFieldProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "IntegerField",
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: EmptyChildren()
            );
        }

        public static VirtualNode LongField(LongFieldProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "LongField",
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: EmptyChildren()
            );
        }

        public static VirtualNode DoubleField(DoubleFieldProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "DoubleField",
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: EmptyChildren()
            );
        }

        public static VirtualNode UnsignedIntegerField(
            UnsignedIntegerFieldProps props,
            string key = null
        )
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "UnsignedIntegerField",
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: EmptyChildren()
            );
        }

        public static VirtualNode UnsignedLongField(UnsignedLongFieldProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "UnsignedLongField",
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: EmptyChildren()
            );
        }

        public static VirtualNode Vector2Field(Vector2FieldProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "Vector2Field",
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: EmptyChildren()
            );
        }

        public static VirtualNode Vector3Field(Vector3FieldProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "Vector3Field",
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: EmptyChildren()
            );
        }

        public static VirtualNode Vector4Field(Vector4FieldProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "Vector4Field",
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: EmptyChildren()
            );
        }

        public static VirtualNode ColorField(ColorFieldProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "ColorField",
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: EmptyChildren()
            );
        }

        public static VirtualNode Box(
            BoxProps props,
            string key = null,
            params VirtualNode[] children
        )
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "Box",
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: children ?? EmptyChildren()
            );
        }

        public static VirtualNode ListView(ListViewProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "ListView",
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: EmptyChildren()
            );
        }

        public static VirtualNode TreeView(TreeViewProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "TreeView",
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: EmptyChildren()
            );
        }

        public static VirtualNode MultiColumnTreeView(
            MultiColumnTreeViewProps props,
            string key = null
        )
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "MultiColumnTreeView",
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: EmptyChildren()
            );
        }

        public static VirtualNode MultiColumnListView(
            MultiColumnListViewProps props,
            string key = null
        )
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "MultiColumnListView",
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
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: children ?? EmptyChildren()
            );
        }

        public static VirtualNode Host(
            VisualElementProps hostProps = null,
            string key = null,
            params VirtualNode[] children
        )
        {
            var propsDict =
                hostProps != null
                    ? CloneStyleDictionary(hostProps.ToDictionary())
                    : (IReadOnlyDictionary<string, object>)EmptyProps();
            return new VirtualNode(
                VirtualNodeType.Host,
                elementTypeName: null,
                textContent: null,
                key: key,
                properties: propsDict,
                children: children ?? EmptyChildren()
            );
        }

        /// <summary>
        /// Creates a typed-props function component VirtualNode.
        ///
        /// The <paramref name="renderFunction"/> receives an <see cref="Core.IProps"/>
        /// which it should cast to <typeparamref name="TProps"/> at the top of the method:
        /// <code>
        ///   var props = rawProps as MyProps;
        /// </code>
        /// The cast succeeds because <c>V.Func&lt;TProps&gt;</c> always stores the concrete
        /// instance passed here, wrapped only in the <see cref="Core.IProps"/> interface.
        ///
        /// Equality for bailout is determined by <typeparamref name="TProps"/>'s
        /// <see cref="object.Equals(object)"/> implementation — generated props classes
        /// get structural equality automatically.
        /// </summary>
        public static VirtualNode Func<TProps>(
            System.Func<Core.IProps, IReadOnlyList<VirtualNode>, VirtualNode> renderFunction,
            TProps typedProps,
            string key = null,
            bool memoize = false,
            System.Func<TProps, TProps, bool> memoCompare = null,
            params VirtualNode[] children
        )
            where TProps : class, Core.IProps
        {
            System.Func<Core.IProps, Core.IProps, bool> wrappedCompare =
                memoCompare != null
                    ? (Core.IProps a, Core.IProps b) => memoCompare(a as TProps, b as TProps)
                    : null;
            return new VirtualNode(
                VirtualNodeType.FunctionComponent,
                elementTypeName: null,
                textContent: null,
                key: key,
                properties: EmptyProps(),
                children: children ?? EmptyChildren(),
                memoize: memoize,
                typedFunctionRender: renderFunction,
                typedProps: (Core.IProps)typedProps ?? Core.EmptyProps.Instance,
                typedMemoCompare: wrappedCompare
            );
        }

        /// <summary>
        /// Creates an untyped IProps function component VirtualNode.
        /// Use this when no strongly-typed props class is needed (no-props components or
        /// when you only need reference equality for bailout via <see cref="Core.EmptyProps.Instance"/>).
        /// </summary>
        public static VirtualNode Func(
            System.Func<Core.IProps, IReadOnlyList<VirtualNode>, VirtualNode> render,
            Core.IProps props = null,
            string key = null,
            bool memoize = false,
            System.Func<Core.IProps, Core.IProps, bool> memoCompare = null,
            params VirtualNode[] children
        )
        {
            return new VirtualNode(
                VirtualNodeType.FunctionComponent,
                elementTypeName: null,
                textContent: null,
                key: key,
                properties: EmptyProps(),
                children: children ?? EmptyChildren(),
                memoize: memoize,
                typedFunctionRender: render,
                typedProps: props ?? Core.EmptyProps.Instance,
                typedMemoCompare: memoCompare
            );
        }

        public static VirtualNode Fragment(string key = null, params VirtualNode[] children)
        {
            return new VirtualNode(
                VirtualNodeType.Fragment,
                elementTypeName: null,
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
                textContent: null,
                key: key,
                properties: EmptyProps(),
                children: children ?? EmptyChildren(),
                portalTarget: portalTargetElement
            );
        }

        public static VirtualNode Router(
            IRouterHistory history = null,
            string initialPath = null,
            string key = null,
            params VirtualNode[] children
        )
        {
            return Func<RouterFuncProps>(
                RouterFunc.Render,
                new RouterFuncProps { History = history, InitialPath = initialPath },
                key,
                false,
                null,
                children
            );
        }

        public static VirtualNode Route(
            string path = null,
            bool exact = false,
            VirtualNode element = null,
            string key = null,
            params VirtualNode[] children
        )
        {
            return Func<RouteFuncProps>(
                RouteFunc.Render,
                new RouteFuncProps
                {
                    Path = path,
                    Exact = exact,
                    Element = element,
                },
                key,
                false,
                null,
                children
            );
        }

        public static VirtualNode Link(
            string to,
            string label = null,
            bool replace = false,
            Style style = null,
            string key = null,
            object state = null
        )
        {
            return Func<LinkFuncProps>(
                LinkFunc.Render,
                new LinkFuncProps
                {
                    To = to,
                    Label = label,
                    Replace = replace,
                    Style = style,
                    State = state,
                },
                key
            );
        }

        public static VirtualNode Animate(
            AnimateProps props,
            string key = null,
            params VirtualNode[] children
        )
        {
            return Func<AnimateProps>(AnimateFunc.Render, props, key, false, null, children);
        }

        public static VirtualNode Suspense(
            System.Func<bool> isReady,
            VirtualNode fallbackNode,
            string key = null,
            params VirtualNode[] children
        ) => Suspense(isReady, null, fallbackNode, key, children);

        public static VirtualNode Suspense(
            System.Func<bool> isReady,
            Task readyTask,
            VirtualNode fallbackNode,
            string key = null,
            params VirtualNode[] children
        )
        {
            return new VirtualNode(
                VirtualNodeType.Suspense,
                elementTypeName: null,
                textContent: null,
                key: key,
                properties: EmptyProps(),
                children: children ?? EmptyChildren(),
                suspenseReady: isReady,
                suspenseReadyTask: readyTask,
                fallback: fallbackNode
            );
        }

        public static VirtualNode Suspense(
            Task readyTask,
            VirtualNode fallbackNode,
            string key = null,
            params VirtualNode[] children
        ) => Suspense(null, readyTask, fallbackNode, key, children);

        public static VirtualNode ErrorBoundary(
            ErrorBoundaryProps props,
            string key = null,
            params VirtualNode[] children
        )
        {
            return new VirtualNode(
                VirtualNodeType.ErrorBoundary,
                elementTypeName: null,
                textContent: null,
                key: key,
                properties: EmptyProps(),
                children: children ?? EmptyChildren(),
                errorFallback: props?.Fallback,
                errorHandler: props?.OnError,
                errorResetToken: props?.ResetKey
            );
        }

        public static VirtualNode Memo(
            System.Func<Core.IProps, IReadOnlyList<VirtualNode>, VirtualNode> renderFunction,
            Core.IProps functionProps = null,
            string key = null,
            System.Func<Core.IProps, Core.IProps, bool> memoCompare = null,
            params VirtualNode[] children
        )
        {
            return Func(renderFunction, functionProps, key, true, memoCompare, children);
        }

        private static IReadOnlyDictionary<string, object> EmptyProps() => VirtualNode.EmptyProps;

        private static IReadOnlyList<VirtualNode> EmptyChildren() => VirtualNode.EmptyChildren;

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
