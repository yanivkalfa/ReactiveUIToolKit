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

        public static VirtualNode Tab(TabProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "Tab",
                functionRender: null,
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
                functionRender: null,
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: EmptyChildren()
            );
        }

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

        public static VirtualNode VisualElementSafe(
            Style style = null,
            string key = null,
            params VirtualNode[] children
        )
        {
            var insets = SafeAreaUtility.GetInsets();

            float GetUser(string k)
            {
                if (style == null)
                {
                    return 0f;
                }
                if (style.TryGetValue(k, out var v) && v is float f)
                {
                    return f;
                }
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
                        continue;
                    }
                    merged[kv.Key] = kv.Value;
                }
            }
            return VisualElement(merged, key, children);
        }

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

        public static VirtualNode EnumField(EnumFieldProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(VirtualNodeType.Element, "EnumField", null, null, key, map ?? EmptyProps(), EmptyChildren());
        }

        public static VirtualNode ObjectField(ObjectFieldProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(VirtualNodeType.Element, "ObjectField", null, null, key, map ?? EmptyProps(), EmptyChildren());
        }

        public static VirtualNode Scroller(ScrollerProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(VirtualNodeType.Element, "Scroller", null, null, key, map ?? EmptyProps(), EmptyChildren());
        }

        public static VirtualNode TextElement(TextElementProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(VirtualNodeType.Element, "TextElement", null, null, key, map ?? EmptyProps(), EmptyChildren());
        }

        public static VirtualNode IMGUIContainer(IMGUIContainerProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(VirtualNodeType.Element, "IMGUIContainer", null, null, key, map ?? EmptyProps(), EmptyChildren());
        }

        public static VirtualNode Vector2IntField(Vector2IntFieldProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(VirtualNodeType.Element, "Vector2IntField", null, null, key, map ?? EmptyProps(), EmptyChildren());
        }

        public static VirtualNode Vector3IntField(Vector3IntFieldProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(VirtualNodeType.Element, "Vector3IntField", null, null, key, map ?? EmptyProps(), EmptyChildren());
        }

        public static VirtualNode RectField(RectFieldProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(VirtualNodeType.Element, "RectField", null, null, key, map ?? EmptyProps(), EmptyChildren());
        }

        public static VirtualNode RectIntField(RectIntFieldProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(VirtualNodeType.Element, "RectIntField", null, null, key, map ?? EmptyProps(), EmptyChildren());
        }

        public static VirtualNode BoundsField(BoundsFieldProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(VirtualNodeType.Element, "BoundsField", null, null, key, map ?? EmptyProps(), EmptyChildren());
        }

        public static VirtualNode MinMaxSlider(MinMaxSliderProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(VirtualNodeType.Element, "MinMaxSlider", null, null, key, map ?? EmptyProps(), EmptyChildren());
        }

        public static VirtualNode TemplateContainer(TemplateContainerProps props, string key = null, params VirtualNode[] children)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(VirtualNodeType.Element, "TemplateContainer", null, null, key, map ?? EmptyProps(), children ?? EmptyChildren());
        }

        public static VirtualNode BoundsIntField(BoundsIntFieldProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(VirtualNodeType.Element, "BoundsIntField", null, null, key, map ?? EmptyProps(), EmptyChildren());
        }

        public static VirtualNode EnumFlagsField(EnumFlagsFieldProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(VirtualNodeType.Element, "EnumFlagsField", null, null, key, map ?? EmptyProps(), EmptyChildren());
        }

        public static VirtualNode ToggleButtonGroup(ToggleButtonGroupProps props, string key = null, params VirtualNode[] children)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(VirtualNodeType.Element, "ToggleButtonGroup", null, null, key, map ?? EmptyProps(), children ?? EmptyChildren());
        }

        public static VirtualNode Hash128Field(Hash128FieldProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(VirtualNodeType.Element, "Hash128Field", null, null, key, map ?? EmptyProps(), EmptyChildren());
        }

        // Editor-only elements are registered behind UNITY_EDITOR; these helpers are safe to call.
        public static VirtualNode Toolbar(ToolbarProps props, string key = null, params VirtualNode[] children)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(VirtualNodeType.Element, "Toolbar", null, null, key, map ?? EmptyProps(), children ?? EmptyChildren());
        }

        public static VirtualNode ToolbarButton(ToolbarButtonProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(VirtualNodeType.Element, "ToolbarButton", null, null, key, map ?? EmptyProps(), EmptyChildren());
        }

        public static VirtualNode ToolbarToggle(ToolbarToggleProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(VirtualNodeType.Element, "ToolbarToggle", null, null, key, map ?? EmptyProps(), EmptyChildren());
        }

        public static VirtualNode ToolbarMenu(ToolbarMenuProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(VirtualNodeType.Element, "ToolbarMenu", null, null, key, map ?? EmptyProps(), EmptyChildren());
        }

        public static VirtualNode ToolbarBreadcrumbs(ToolbarBreadcrumbsProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(VirtualNodeType.Element, "ToolbarBreadcrumbs", null, null, key, map ?? EmptyProps(), EmptyChildren());
        }

        public static VirtualNode ToolbarPopupSearchField(ToolbarPopupSearchFieldProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(VirtualNodeType.Element, "ToolbarPopupSearchField", null, null, key, map ?? EmptyProps(), EmptyChildren());
        }

        public static VirtualNode ToolbarSearchField(ToolbarSearchFieldProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(VirtualNodeType.Element, "ToolbarSearchField", null, null, key, map ?? EmptyProps(), EmptyChildren());
        }

        public static VirtualNode ToolbarSpacer(ToolbarSpacerProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(VirtualNodeType.Element, "ToolbarSpacer", null, null, key, map ?? EmptyProps(), EmptyChildren());
        }

        public static VirtualNode PropertyField(PropertyFieldProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(VirtualNodeType.Element, "PropertyField", null, null, key, map ?? EmptyProps(), EmptyChildren());
        }

        public static VirtualNode InspectorElement(InspectorElementProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(VirtualNodeType.Element, "InspectorElement", null, null, key, map ?? EmptyProps(), EmptyChildren());
        }

#if UNITY_EDITOR
        public static VirtualNode TwoPaneSplitView(TwoPaneSplitViewProps props, string key = null, params VirtualNode[] children)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(VirtualNodeType.Element, "TwoPaneSplitView", null, null, key, map ?? EmptyProps(), children ?? EmptyChildren());
        }
#endif

        public static VirtualNode FloatField(FloatFieldProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "FloatField",
                functionRender: null,
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
                functionRender: null,
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
                functionRender: null,
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
                functionRender: null,
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: EmptyChildren()
            );
        }

        public static VirtualNode UnsignedIntegerField(UnsignedIntegerFieldProps props, string key = null)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "UnsignedIntegerField",
                functionRender: null,
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
                functionRender: null,
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
                functionRender: null,
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
                functionRender: null,
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
                functionRender: null,
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
                functionRender: null,
                textContent: null,
                key: key,
                properties: map ?? EmptyProps(),
                children: EmptyChildren()
            );
        }

        public static VirtualNode Box(BoxProps props, string key = null, params VirtualNode[] children)
        {
            IReadOnlyDictionary<string, object> map = props?.ToDictionary();
            map = CloneStyleDictionary(map);
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "Box",
                functionRender: null,
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
                functionRender: null,
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
                functionRender: null,
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
                functionRender: null,
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

        public static VirtualNode ForwardRef(
            Func<
                Dictionary<string, object>,
                object,
                IReadOnlyList<VirtualNode>,
                VirtualNode
            > renderFunction,
            IReadOnlyDictionary<string, object> functionProps = null,
            string key = null,
            bool memoize = false,
            Func<
                IReadOnlyDictionary<string, object>,
                IReadOnlyDictionary<string, object>,
                bool
            > memoCompare = null,
            params VirtualNode[] children
        )
        {
            if (renderFunction == null)
            {
                throw new ArgumentNullException(nameof(renderFunction));
            }

            VirtualNode Wrapper(
                Dictionary<string, object> incomingProps,
                IReadOnlyList<VirtualNode> childNodes
            )
            {
                object forwardedRef = null;
                Dictionary<string, object> sanitizedProps;

                if (incomingProps == null || incomingProps.Count == 0)
                {
                    sanitizedProps = new Dictionary<string, object>();
                }
                else
                {
                    sanitizedProps = new Dictionary<string, object>(incomingProps);
                    if (sanitizedProps.TryGetValue("ref", out var refCandidate))
                    {
                        forwardedRef = refCandidate;
                        sanitizedProps.Remove("ref");
                    }
                }

                return renderFunction(sanitizedProps, forwardedRef, childNodes);
            }

            return Func(Wrapper, functionProps, key, memoize, memoCompare, children);
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

        public static VirtualNode Router(
            IRouterHistory history = null,
            string initialPath = null,
            string key = null,
            params VirtualNode[] children
        )
        {
            Dictionary<string, object> props = null;
            if (history != null || !string.IsNullOrEmpty(initialPath))
            {
                props = new Dictionary<string, object>();
                if (history != null)
                {
                    props["history"] = history;
                }
                if (!string.IsNullOrEmpty(initialPath))
                {
                    props["initialPath"] = initialPath;
                }
            }
            return Func(RouterFunc.Render, props, key, false, null, children);
        }

        public static VirtualNode Route(
            string path = null,
            bool exact = false,
            VirtualNode element = null,
            string key = null,
            params VirtualNode[] children
        )
        {
            Dictionary<string, object> props = null;
            if (!string.IsNullOrEmpty(path) || exact || element != null)
            {
                props = new Dictionary<string, object>();
                if (!string.IsNullOrEmpty(path))
                {
                    props["path"] = path;
                }
                if (exact)
                {
                    props["exact"] = true;
                }
                if (element != null)
                {
                    props["element"] = element;
                }
            }
            return Func(RouteFunc.Render, props, key, false, null, children);
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
            var props = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(to))
            {
                props["to"] = to;
            }
            if (!string.IsNullOrEmpty(label))
            {
                props["label"] = label;
            }
            if (replace)
            {
                props["replace"] = true;
            }
            if (style != null)
            {
                props["style"] = style;
            }
            if (state != null)
            {
                props["state"] = state;
            }
            if (props.Count == 0)
            {
                props = null;
            }
            return Func(LinkFunc.Render, props, key);
        }

        public static VirtualNode Animate(
            AnimateProps props,
            string key = null,
            params VirtualNode[] children
        )
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

            enriched["__childRef"] = children;
            return Func(AnimateFunc.Render, enriched, key, false, null, children);
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
                functionRender: null,
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
                functionRender: null,
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
            System.Func<
                Dictionary<string, object>,
                IReadOnlyList<VirtualNode>,
                VirtualNode
            > renderFunction,
            IReadOnlyDictionary<string, object> functionProps = null,
            string key = null,
            System.Func<
                IReadOnlyDictionary<string, object>,
                IReadOnlyDictionary<string, object>,
                bool
            > memoCompare = null,
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
