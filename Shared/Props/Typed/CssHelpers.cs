using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    /// <summary>
    /// Short helpers for typed <see cref="Style"/> properties.
    /// Import via <c>using static ReactiveUITK.Props.Typed.CssHelpers;</c>
    /// <code>
    /// new Style { Width = Pct(50), Height = Px(100), MaxWidth = Auto }
    /// </code>
    /// </summary>
    public static class CssHelpers
    {
        // ── Length units ─────────────────────────────────────────────────
        public static Length Pct(float v) => new Length(v, LengthUnit.Percent);
        public static Length Pct(int v)   => new Length(v, LengthUnit.Percent);
        public static Length Px(float v)  => new Length(v, LengthUnit.Pixel);
        public static Length Px(int v)    => new Length(v, LengthUnit.Pixel);

        // ── Style keywords ──────────────────────────────────────────────
        public static StyleKeyword Auto    => StyleKeyword.Auto;
        public static StyleKeyword None    => StyleKeyword.None;
        public static StyleKeyword Initial => StyleKeyword.Initial;

        // ── Flex direction ──────────────────────────────────────────────
        public static FlexDirection Row           => FlexDirection.Row;
        public static FlexDirection Column        => FlexDirection.Column;
        public static FlexDirection RowReverse    => FlexDirection.RowReverse;
        public static FlexDirection ColumnReverse => FlexDirection.ColumnReverse;

        // ── Justify content ─────────────────────────────────────────────
        public static Justify JustifyStart   => Justify.FlexStart;
        public static Justify JustifyEnd     => Justify.FlexEnd;
        public static Justify JustifyCenter  => Justify.Center;
        public static Justify SpaceBetween   => Justify.SpaceBetween;
        public static Justify SpaceAround    => Justify.SpaceAround;

        // ── Align ───────────────────────────────────────────────────────
        public static Align AlignStart   => Align.FlexStart;
        public static Align AlignEnd     => Align.FlexEnd;
        public static Align AlignCenter  => Align.Center;
        public static Align Stretch      => Align.Stretch;
        public static Align AlignAuto    => Align.Auto;

        // ── Wrap ────────────────────────────────────────────────────────
        public static Wrap WrapOn  => Wrap.Wrap;
        public static Wrap NoWrap  => Wrap.NoWrap;
        public static Wrap WrapRev => Wrap.WrapReverse;

        // ── Position ────────────────────────────────────────────────────
        public static Position Relative => Position.Relative;
        public static Position Absolute => Position.Absolute;

        // ── Display ─────────────────────────────────────────────────────
        public static DisplayStyle Flex       => DisplayStyle.Flex;
        public static DisplayStyle DisplayNone => DisplayStyle.None;

        // ── Visibility ──────────────────────────────────────────────────
        public static Visibility Visible => Visibility.Visible;
        public static Visibility Hidden  => Visibility.Hidden;

        // ── Overflow ────────────────────────────────────────────────────
        public static Overflow OverflowVisible => Overflow.Visible;
        public static Overflow OverflowHidden  => Overflow.Hidden;

        // ── White-space ─────────────────────────────────────────────────
        public static WhiteSpace Normal  => WhiteSpace.Normal;
        public static WhiteSpace Nowrap  => WhiteSpace.NoWrap;

        // ── Text overflow ───────────────────────────────────────────────
        public static TextOverflow Clip     => TextOverflow.Clip;
        public static TextOverflow Ellipsis => TextOverflow.Ellipsis;

        // ── Text align ──────────────────────────────────────────────────
        public static TextAnchor UpperLeft    => TextAnchor.UpperLeft;
        public static TextAnchor UpperCenter  => TextAnchor.UpperCenter;
        public static TextAnchor UpperRight   => TextAnchor.UpperRight;
        public static TextAnchor MiddleLeft   => TextAnchor.MiddleLeft;
        public static TextAnchor MiddleCenter => TextAnchor.MiddleCenter;
        public static TextAnchor MiddleRight  => TextAnchor.MiddleRight;
        public static TextAnchor LowerLeft    => TextAnchor.LowerLeft;
        public static TextAnchor LowerCenter  => TextAnchor.LowerCenter;
        public static TextAnchor LowerRight   => TextAnchor.LowerRight;
        // ── Text overflow position ──────────────────────────────────
        public static TextOverflowPosition OverflowStart  => TextOverflowPosition.Start;
        public static TextOverflowPosition OverflowMiddle => TextOverflowPosition.Middle;
        public static TextOverflowPosition OverflowEnd    => TextOverflowPosition.End;
        // ── Font style ──────────────────────────────────────────────────
        public static FontStyle Bold       => FontStyle.Bold;
        public static FontStyle Italic     => FontStyle.Italic;
        public static FontStyle BoldItalic => FontStyle.BoldAndItalic;
        public static FontStyle FontNormal => FontStyle.Normal;

        // ── Colors (common presets) ─────────────────────────────────────
        public static Color Transparent => Color.clear;
        public static Color White       => Color.white;
        public static Color Black       => Color.black;
        public static Color Red         => Color.red;
        public static Color Green       => Color.green;
        public static Color Blue        => Color.blue;
        public static Color Yellow      => Color.yellow;
        public static Color Cyan        => Color.cyan;
        public static Color Magenta     => Color.magenta;
        public static Color Grey        => Color.grey;
        public static Color Gray        => Color.grey;

        /// <summary>Parse a hex color string like "#FF0000" or "rgba(255,0,0,1)".</summary>
        public static Color Hex(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out var c)) return c;
            return Color.white;
        }

        /// <summary>Create a color from 0-255 RGBA values.</summary>
        public static Color Rgba(byte r, byte g, byte b, byte a = 255)
            => new Color32(r, g, b, a);

        /// <summary>Create a color from 0-1 float RGBA values.</summary>
        public static Color Rgba(float r, float g, float b, float a = 1f)
            => new Color(r, g, b, a);

        // ── Filter functions (Unity 6.3+) ───────────────────────────────
#if UNITY_6000_3_OR_NEWER
        static FilterFunction MakeFilter(FilterFunctionType type, float value)
        {
            var ff = new FilterFunction(type);
            ff.AddParameter(new FilterParameter(value));
            return ff;
        }

        static FilterFunction MakeFilter(FilterFunctionType type, Color color)
        {
            var ff = new FilterFunction(type);
            ff.AddParameter(new FilterParameter(color));
            return ff;
        }

        public static FilterFunction Blur(float radiusPx)
            => MakeFilter(FilterFunctionType.Blur, radiusPx);
        public static FilterFunction Grayscale(float amount)
            => MakeFilter(FilterFunctionType.Grayscale, amount);
        public static FilterFunction Contrast(float amount)
            => MakeFilter(FilterFunctionType.Contrast, amount);
        public static FilterFunction HueRotate(float degrees)
            => MakeFilter(FilterFunctionType.HueRotate, degrees);
        public static FilterFunction Invert(float amount)
            => MakeFilter(FilterFunctionType.Invert, amount);
        public static FilterFunction Opacity(float amount)
            => MakeFilter(FilterFunctionType.Opacity, amount);
        public static FilterFunction Sepia(float amount)
            => MakeFilter(FilterFunctionType.Sepia, amount);
        public static FilterFunction Tint(Color color)
            => MakeFilter(FilterFunctionType.Tint, color);
#endif
    }
}

