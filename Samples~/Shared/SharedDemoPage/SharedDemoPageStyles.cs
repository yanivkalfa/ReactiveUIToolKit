using ReactiveUITK.Props.Typed;
using static ReactiveUITK.Props.Typed.StyleKeys;
using UColor = UnityEngine.Color;

namespace ReactiveUITK.Samples.Shared
{
    public static class SharedDemoPageStyles
    {
        public static readonly Style TopBarStyle = new()
        {
            (StyleKeys.FlexDirection, "row"),
            (JustifyContent, "space-between"),
            (AlignItems, "center"),
            (FlexGrow, 1f),
            (PaddingLeft, 12f),
            (PaddingRight, 12f),
            (PaddingTop, 8f),
            (PaddingBottom, 8f),
            (BorderBottomWidth, 1f),
            (BorderBottomColor, new UColor(0.85f, 0.85f, 0.85f, 1f)),
        };

        public static readonly Style LeftBoxStyle = new()
        {
            (BackgroundColor, new UColor(0.2f, 0.4f, 0.9f, 1f)),
            (TextColor, UColor.white),
            (PaddingLeft, 10f),
            (PaddingRight, 10f),
            (PaddingTop, 6f),
            (PaddingBottom, 6f),
            (BorderRadius, 4f),
            (FontSize, 14f),
        };

        public static readonly Style RightBoxStyle = new()
        {
            (BackgroundColor, new UColor(0.9f, 0.3f, 0.2f, 1f)),
            (TextColor, UColor.white),
            (PaddingLeft, 10f),
            (PaddingRight, 10f),
            (PaddingTop, 6f),
            (PaddingBottom, 6f),
            (BorderRadius, 4f),
            (FontSize, 14f),
        };

        public static readonly Style TextInputStyle = new()
        {
            (FlexGrow, 1f),
            (MarginLeft, 8f),
            (MarginRight, 8f),
            (PaddingLeft, 6f),
            (PaddingRight, 6f),
            (PaddingTop, 4f),
            (PaddingBottom, 4f),
            (BorderRadius, 4f),
            (BorderWidth, 1f),
            (TextColor, UColor.black),
            (BorderColor, new UColor(0.8f, 0.8f, 0.8f, 1f)),
            (BackgroundColor, new UColor(1f, 1f, 1f, 1f)),
        };

        public static readonly Style PageStyle = new()
        {
            (StyleKeys.FlexDirection, "column"),
            (FlexGrow, 1f),
            (JustifyContent, "space-between"),
            (BackgroundColor, new UColor(0.95f, 0.95f, 0.95f, 1f)),
        };

        public static readonly Style ExtrasContainerStyle = new()
        {
            (MarginTop, 12f),
            (PaddingLeft, 12f),
            (PaddingRight, 12f),
            (PaddingTop, 8f),
            (PaddingBottom, 8f),
            (BackgroundColor, UColor.white),
            (BorderTopWidth, 1f),
            (BorderTopColor, new UColor(0.85f, 0.85f, 0.85f, 1f)),
            (StyleKeys.FlexDirection, "column"),
        };

        public static readonly Style OuterWrapperStyle = new()
        {
            (BackgroundColor, new UColor(0.2f, 0.4f, 0.8f, 1f)),
            (FlexGrow, 1f),
        };

        public static readonly Style SafeWrapperStyle = new()
        {
            (BackgroundColor, new UColor(0.2f, 0.6f, 0.2f, 1f)),
            (FlexGrow, 1f),
            (StyleKeys.FlexDirection, "column"),
        };

        public static readonly Style BarSlotStyle = new()
        {
            (FlexShrink, 0f),
            (MinHeight, 110f),
        };

        public static readonly Style MainScrollStyle = new()
        {
            (FlexGrow, 1f),
            (PaddingBottom, 20f),
        };

        public static readonly Style NewCompsGroupContentStyle = new()
        {
            (PaddingLeft, 8f),
            (PaddingRight, 8f),
            (PaddingTop, 6f),
            (PaddingBottom, 6f),
        };

        public static readonly Style FoldoutHeaderStyle = new()
        {
            (BackgroundColor, new UColor(0.95f, 0.95f, 0.95f, 1f)),
            (PaddingLeft, 4f),
            (PaddingTop, 2f),
            (PaddingBottom, 2f),
        };

        public static readonly Style FoldoutContentStyle = new() { (PaddingLeft, 6f) };

        public static readonly Style ImageDemoStyle = new()
        {
            (Width, 96f),
            (Height, 96f),
            (BackgroundColor, new UColor(0.7f, 0.85f, 1f, 1f)),
            (BorderRadius, 6f),
        };

        public static readonly Style SliderWidthStyle = new() { (Width, 200f) };
    }
}
