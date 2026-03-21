using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using static ReactiveUITK.Props.Typed.StyleKeys;

namespace ReactiveUITK.Samples.Shared
{
    /// <summary>
    /// Static display panel showing all field types: numeric/vector/color,
    /// misc controls, and edge-case fields. No interactive state.
    /// </summary>
    public static class ShowcaseFieldsPanel
    {
        public static VirtualNode Render(IProps rawProps, IReadOnlyList<VirtualNode> children)
        {
            return V.VisualElement(
                null,
                null,
                V.Label(new LabelProps { Text = "New Fields" }),
                RenderNumericFields(),
                RenderMoreFields(),
                RenderEvenMoreFields()
            );
        }

        private static VirtualNode RenderNumericFields()
        {
            return V.GroupBox(
                new GroupBoxProps
                {
                    Text = "Numeric, Vector and Color Fields",
                    Style = new Style { (MarginTop, 8f) },
                },
                null,
                V.Label(new LabelProps { Text = "FloatField" }),
                V.FloatField(new FloatFieldProps { Value = 1.23f }),
                V.Label(new LabelProps { Text = "IntegerField" }),
                V.IntegerField(new IntegerFieldProps { Value = 42 }),
                V.Label(new LabelProps { Text = "LongField" }),
                V.LongField(new LongFieldProps { Value = 123456789 }),
                V.Label(new LabelProps { Text = "DoubleField" }),
                V.DoubleField(new DoubleFieldProps { Value = 3.14159 }),
                V.Label(new LabelProps { Text = "UnsignedIntegerField" }),
                V.UnsignedIntegerField(new UnsignedIntegerFieldProps { Value = 77 }),
                V.Label(new LabelProps { Text = "UnsignedLongField" }),
                V.UnsignedLongField(new UnsignedLongFieldProps { Value = 9876543210 }),
                V.Label(new LabelProps { Text = "Vector2Field" }),
                V.Vector2Field(new Vector2FieldProps { Value = new Vector2(1, 2) }),
                V.Label(new LabelProps { Text = "Vector3Field" }),
                V.Vector3Field(new Vector3FieldProps { Value = new Vector3(1, 2, 3) }),
                V.Label(new LabelProps { Text = "Vector4Field" }),
                V.Vector4Field(new Vector4FieldProps { Value = new Vector4(1, 2, 3, 4) }),
                V.Label(new LabelProps { Text = "ColorField" }),
                V.ColorField(new ColorFieldProps
                {
                    Value = new Color(0.2f, 0.6f, 0.9f, 1f),
                })
            );
        }

        private static VirtualNode RenderMoreFields()
        {
            return V.GroupBox(
                new GroupBoxProps
                {
                    Text = "More Fields",
                    Style = new Style { (MarginTop, 8f) },
                },
                null,
                V.Label(new LabelProps { Text = "EnumField (TextAnchor)" }),
                V.EnumField(new EnumFieldProps
                {
                    EnumType = typeof(TextAnchor).AssemblyQualifiedName,
                    Value = TextAnchor.MiddleCenter,
                }),
                V.Label(new LabelProps { Text = "Scroller" }),
                V.Scroller(new ScrollerProps
                {
                    LowValue = 0f,
                    HighValue = 100f,
                    Value = 25f,
                    Style = new Style { (Height, 18f), (Width, 160f) },
                }),
                V.Label(new LabelProps { Text = "TextElement" }),
                V.TextElement(new TextElementProps
                {
                    Text = "This is a TextElement",
                    Style = new Style { (FontSize, 13f) },
                }),
                V.Label(new LabelProps { Text = "IMGUIContainer" }),
                V.IMGUIContainer(new IMGUIContainerProps
                {
                    OnGUI = () => UnityEngine.GUILayout.Label("IMGUI says hello"),
                    Style = new Style { (Height, 22f) },
                }),
                V.Label(new LabelProps { Text = "Vector2IntField" }),
                V.Vector2IntField(new Vector2IntFieldProps { Value = new Vector2Int(3, 7) }),
                V.Label(new LabelProps { Text = "Vector3IntField" }),
                V.Vector3IntField(new Vector3IntFieldProps { Value = new Vector3Int(1, 2, 3) }),
                V.Label(new LabelProps { Text = "RectField" }),
                V.RectField(new RectFieldProps { Value = new Rect(10f, 20f, 80f, 40f) }),
                V.Label(new LabelProps { Text = "RectIntField" }),
                V.RectIntField(new RectIntFieldProps { Value = new RectInt(2, 4, 11, 9) }),
                V.Label(new LabelProps { Text = "BoundsField" }),
                V.BoundsField(new BoundsFieldProps
                {
                    Value = new Bounds(new Vector3(0, 0, 0), new Vector3(1, 2, 3)),
                })
#if UNITY_EDITOR
                ,
                V.Label(new LabelProps { Text = "ObjectField (Texture2D)" }),
                V.ObjectField(new ObjectFieldProps
                {
                    ObjectType = typeof(Texture2D).AssemblyQualifiedName,
                    AllowSceneObjects = false,
                })
#endif
            );
        }

        private static VirtualNode RenderEvenMoreFields()
        {
            return V.GroupBox(
                new GroupBoxProps
                {
                    Text = "Even More Fields",
                    Style = new Style { (MarginTop, 8f) },
                },
                null,
                V.Label(new LabelProps { Text = "MinMaxSlider" }),
                V.MinMaxSlider(new MinMaxSliderProps
                {
                    MinValue = 20f,
                    MaxValue = 80f,
                    LowLimit = 0f,
                    HighLimit = 100f,
                    Style = new Style { (Width, 200f) },
                }),
                V.Label(new LabelProps { Text = "TemplateContainer" }),
                V.TemplateContainer(
                    new TemplateContainerProps
                    {
                        ContentContainer = new Style
                        {
                            (Padding, 6f),
                            (BackgroundColor, new Color(1f, 1f, 1f, 1f)),
                        },
                        Style = new Style
                        {
                            (BorderWidth, 1f),
                            (BorderColor, new Color(0.85f, 0.85f, 0.85f, 1f)),
                        },
                    },
                    null,
                    V.Label(new LabelProps { Text = "Inside TemplateContainer" })
                ),
                V.Label(new LabelProps { Text = "BoundsIntField" }),
                V.BoundsIntField(new BoundsIntFieldProps
                {
                    Value = new BoundsInt(1, 2, 3, 4, 5, 6),
                }),
                V.Label(new LabelProps { Text = "Hash128Field" }),
                V.Hash128Field(new Hash128FieldProps
                {
                    Value = new Hash128(1, 2, 3, 4),
                }),
                V.Label(new LabelProps { Text = "ToggleButtonGroup" }),
                V.ToggleButtonGroup(
                    new ToggleButtonGroupProps { Value = 1 },
                    null,
                    V.Button(new ButtonProps { Text = "One" }),
                    V.Button(new ButtonProps { Text = "Two" }),
                    V.Button(new ButtonProps { Text = "Three" })
                )
            );
        }
    }
}
