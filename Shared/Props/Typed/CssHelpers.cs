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

        public static Length Pct(int v) => new Length(v, LengthUnit.Percent);

        public static Length Px(float v) => new Length(v, LengthUnit.Pixel);

        public static Length Px(int v) => new Length(v, LengthUnit.Pixel);

        // ── Style keywords ──────────────────────────────────────────────
        public static StyleKeyword StyleAuto => StyleKeyword.Auto;
        public static StyleKeyword StyleNone => StyleKeyword.None;
        public static StyleKeyword StyleInitial => StyleKeyword.Initial;

        // ── Flex direction ──────────────────────────────────────────────
        public static FlexDirection FlexRow => FlexDirection.Row;
        public static FlexDirection FlexColumn => FlexDirection.Column;
        public static FlexDirection FlexRowReverse => FlexDirection.RowReverse;
        public static FlexDirection FlexColumnReverse => FlexDirection.ColumnReverse;

        // ── Justify content ─────────────────────────────────────────────
        public static Justify JustifyStart => Justify.FlexStart;
        public static Justify JustifyEnd => Justify.FlexEnd;
        public static Justify JustifyCenter => Justify.Center;
        public static Justify JustifySpaceBetween => Justify.SpaceBetween;
        public static Justify JustifySpaceAround => Justify.SpaceAround;
        public static Justify JustifySpaceEvenly => Justify.SpaceEvenly;

        // ── Align ───────────────────────────────────────────────────────
        public static Align AlignStart => Align.FlexStart;
        public static Align AlignEnd => Align.FlexEnd;
        public static Align AlignCenter => Align.Center;
        public static Align AlignStretch => Align.Stretch;
        public static Align AlignAuto => Align.Auto;

        // ── Wrap ────────────────────────────────────────────────────────
        public static Wrap WrapOn => Wrap.Wrap;
        public static Wrap WrapOff => Wrap.NoWrap;
        public static Wrap WrapReverse => Wrap.WrapReverse;

        // ── Position ────────────────────────────────────────────────────
        public static Position PosRelative => Position.Relative;
        public static Position PosAbsolute => Position.Absolute;

        // ── Display ─────────────────────────────────────────────────────
        public static DisplayStyle DisplayFlex => DisplayStyle.Flex;
        public static DisplayStyle DisplayNone => DisplayStyle.None;

        // ── Visibility ──────────────────────────────────────────────────
        public static Visibility VisVisible => Visibility.Visible;
        public static Visibility VisHidden => Visibility.Hidden;

        // ── Overflow ────────────────────────────────────────────────────
        public static Overflow OverflowVisible => Overflow.Visible;
        public static Overflow OverflowHidden => Overflow.Hidden;

        // ── White-space ─────────────────────────────────────────────────
        public static WhiteSpace WsNormal => WhiteSpace.Normal;
        public static WhiteSpace WsNowrap => WhiteSpace.NoWrap;
        public static WhiteSpace WsPre => WhiteSpace.Pre;
        public static WhiteSpace WsPreWrap => WhiteSpace.PreWrap;

        // ── Text overflow ───────────────────────────────────────────────
        public static TextOverflow TextClip => TextOverflow.Clip;
        public static TextOverflow TextEllipsis => TextOverflow.Ellipsis;

        // ── Text align ──────────────────────────────────────────────────
        public static TextAnchor TextUpperLeft => TextAnchor.UpperLeft;
        public static TextAnchor TextUpperCenter => TextAnchor.UpperCenter;
        public static TextAnchor TextUpperRight => TextAnchor.UpperRight;
        public static TextAnchor TextMiddleLeft => TextAnchor.MiddleLeft;
        public static TextAnchor TextMiddleCenter => TextAnchor.MiddleCenter;
        public static TextAnchor TextMiddleRight => TextAnchor.MiddleRight;
        public static TextAnchor TextLowerLeft => TextAnchor.LowerLeft;
        public static TextAnchor TextLowerCenter => TextAnchor.LowerCenter;
        public static TextAnchor TextLowerRight => TextAnchor.LowerRight;

        // ── Text overflow position ──────────────────────────────────
        public static TextOverflowPosition TextOverflowStart => TextOverflowPosition.Start;
        public static TextOverflowPosition TextOverflowMiddle => TextOverflowPosition.Middle;
        public static TextOverflowPosition TextOverflowEnd => TextOverflowPosition.End;

        // ── Text auto size ──────────────────────────────────────────────
        public static TextAutoSizeMode AutoSizeNone => TextAutoSizeMode.None;
        public static TextAutoSizeMode AutoSizeBestFit => TextAutoSizeMode.BestFit;

        // ── Font style ──────────────────────────────────────────────────
        public static FontStyle FontBold => FontStyle.Bold;
        public static FontStyle FontItalic => FontStyle.Italic;
        public static FontStyle FontBoldItalic => FontStyle.BoldAndItalic;
        public static FontStyle FontNormal => FontStyle.Normal;

        // ── Picking mode ────────────────────────────────────────────────
        public static PickingMode PickPosition => PickingMode.Position;
        public static PickingMode PickIgnore => PickingMode.Ignore;

        // ── Selection type (ListView, TreeView) ─────────────────────────
        public static SelectionType SelectNone => SelectionType.None;
        public static SelectionType SelectSingle => SelectionType.Single;
        public static SelectionType SelectMultiple => SelectionType.Multiple;

        // ── Scroller visibility (ScrollView) ────────────────────────────
        public static ScrollerVisibility ScrollerAuto => ScrollerVisibility.Auto;
        public static ScrollerVisibility ScrollerVisible => ScrollerVisibility.AlwaysVisible;
        public static ScrollerVisibility ScrollerHidden => ScrollerVisibility.Hidden;

        // ── Language direction ──────────────────────────────────────────
        public static LanguageDirection DirInherit => LanguageDirection.Inherit;
        public static LanguageDirection DirLTR => LanguageDirection.LTR;
        public static LanguageDirection DirRTL => LanguageDirection.RTL;

        // ── Slider direction (string-based) ─────────────────────────────
        public static string SliderHorizontal => "horizontal";
        public static string SliderVertical => "vertical";

        // ── ScrollView mode (string-based) ──────────────────────────────
        public static string ScrollVertical => "vertical";
        public static string ScrollHorizontal => "horizontal";
        public static string ScrollBoth => "verticalandhorizontal";

        // ── Image scale mode (string-based) ─────────────────────────────
        public static string ScaleStretch => "stretchfill";
        public static string ScaleFit => "scaletofit";
        public static string ScaleCrop => "scalefill";

        // ── TwoPaneSplitView orientation (string-based) ─────────────────
        public static string OrientHorizontal => "horizontal";
        public static string OrientVertical => "vertical";

        // ── Column sorting mode (string-based) ─────────────────────────
        public static string SortNone => "None";
        public static string SortDefault => "Default";
        public static string SortCustom => "Custom";

        // ── Colors (common presets) ─────────────────────────────────────
        public static Color ColorTransparent => Color.clear;
        public static Color ColorWhite => Color.white;
        public static Color ColorBlack => Color.black;
        public static Color ColorRed => Color.red;
        public static Color ColorGreen => Color.green;
        public static Color ColorBlue => Color.blue;
        public static Color ColorYellow => Color.yellow;
        public static Color ColorCyan => Color.cyan;
        public static Color ColorMagenta => Color.magenta;
        public static Color ColorGrey => Color.grey;
        public static Color ColorGray => Color.grey;

        /// <summary>Parse a hex color string like "#FF0000" or "rgba(255,0,0,1)".</summary>
        public static Color Hex(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out var c))
                return c;
            return Color.white;
        }

        /// <summary>Create a color from 0-255 RGBA values.</summary>
        public static Color Rgba(byte r, byte g, byte b, byte a = 255) => new Color32(r, g, b, a);

        /// <summary>Create a color from 0-1 float RGBA values.</summary>
        public static Color Rgba(float r, float g, float b, float a = 1f) => new Color(r, g, b, a);

        // ── Background repeat ───────────────────────────────────────────
        public static BackgroundRepeat BgRepeat(Repeat x, Repeat y) => new BackgroundRepeat(x, y);

        public static BackgroundRepeat BgRepeatNone =>
            new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat);
        public static BackgroundRepeat BgRepeatBoth =>
            new BackgroundRepeat(Repeat.Repeat, Repeat.Repeat);
        public static BackgroundRepeat BgRepeatX =>
            new BackgroundRepeat(Repeat.Repeat, Repeat.NoRepeat);
        public static BackgroundRepeat BgRepeatY =>
            new BackgroundRepeat(Repeat.NoRepeat, Repeat.Repeat);
        public static BackgroundRepeat BgRepeatSpace =>
            new BackgroundRepeat(Repeat.Space, Repeat.Space);
        public static BackgroundRepeat BgRepeatRound =>
            new BackgroundRepeat(Repeat.Round, Repeat.Round);

        // ── Background position ─────────────────────────────────────────
        public static BackgroundPosition BgPos(BackgroundPositionKeyword keyword) =>
            new BackgroundPosition(keyword);

        public static BackgroundPosition BgPos(BackgroundPositionKeyword keyword, Length offset) =>
            new BackgroundPosition(keyword, offset);

        public static BackgroundPosition BgPosCenter =>
            new BackgroundPosition(BackgroundPositionKeyword.Center);
        public static BackgroundPosition BgPosTop =>
            new BackgroundPosition(BackgroundPositionKeyword.Top);
        public static BackgroundPosition BgPosBottom =>
            new BackgroundPosition(BackgroundPositionKeyword.Bottom);
        public static BackgroundPosition BgPosLeft =>
            new BackgroundPosition(BackgroundPositionKeyword.Left);
        public static BackgroundPosition BgPosRight =>
            new BackgroundPosition(BackgroundPositionKeyword.Right);

        // ── Background size ─────────────────────────────────────────────
        public static BackgroundSize BgSize(Length x, Length y) => new BackgroundSize(x, y);

        public static BackgroundSize BgSizeCover => new BackgroundSize(BackgroundSizeType.Cover);
        public static BackgroundSize BgSizeContain =>
            new BackgroundSize(BackgroundSizeType.Contain);

        // ── Transform origin ────────────────────────────────────────────
        public static TransformOrigin Origin(Length x, Length y) => new TransformOrigin(x, y);

        public static TransformOrigin OriginCenter => Origin(Pct(50), Pct(50));

        // ── Translate ───────────────────────────────────────────────────
        public static Translate Xlate(Length x, Length y) => new Translate(x, y, 0);

        // ── Easing functions ────────────────────────────────────────────
        public static EasingFunction Easing(EasingMode mode) => new EasingFunction(mode);

        public static EasingFunction EaseDefault => new EasingFunction(EasingMode.Ease);
        public static EasingFunction EaseLinear => new EasingFunction(EasingMode.Linear);
        public static EasingFunction EaseIn => new EasingFunction(EasingMode.EaseIn);
        public static EasingFunction EaseOut => new EasingFunction(EasingMode.EaseOut);
        public static EasingFunction EaseInOut => new EasingFunction(EasingMode.EaseInOut);
        public static EasingFunction EaseInSine => new EasingFunction(EasingMode.EaseInSine);
        public static EasingFunction EaseOutSine => new EasingFunction(EasingMode.EaseOutSine);
        public static EasingFunction EaseInOutSine => new EasingFunction(EasingMode.EaseInOutSine);
        public static EasingFunction EaseInCubic => new EasingFunction(EasingMode.EaseInCubic);
        public static EasingFunction EaseOutCubic => new EasingFunction(EasingMode.EaseOutCubic);
        public static EasingFunction EaseInOutCubic =>
            new EasingFunction(EasingMode.EaseInOutCubic);
        public static EasingFunction EaseInCirc => new EasingFunction(EasingMode.EaseInCirc);
        public static EasingFunction EaseOutCirc => new EasingFunction(EasingMode.EaseOutCirc);
        public static EasingFunction EaseInOutCirc => new EasingFunction(EasingMode.EaseInOutCirc);
        public static EasingFunction EaseInElastic => new EasingFunction(EasingMode.EaseInElastic);
        public static EasingFunction EaseOutElastic =>
            new EasingFunction(EasingMode.EaseOutElastic);
        public static EasingFunction EaseInOutElastic =>
            new EasingFunction(EasingMode.EaseInOutElastic);
        public static EasingFunction EaseInBack => new EasingFunction(EasingMode.EaseInBack);
        public static EasingFunction EaseOutBack => new EasingFunction(EasingMode.EaseOutBack);
        public static EasingFunction EaseInOutBack => new EasingFunction(EasingMode.EaseInOutBack);
        public static EasingFunction EaseInBounce => new EasingFunction(EasingMode.EaseInBounce);
        public static EasingFunction EaseOutBounce => new EasingFunction(EasingMode.EaseOutBounce);
        public static EasingFunction EaseInOutBounce =>
            new EasingFunction(EasingMode.EaseInOutBounce);

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

        public static FilterFunction FilterBlur(float radiusPx) =>
            MakeFilter(FilterFunctionType.Blur, radiusPx);

        public static FilterFunction FilterGrayscale(float amount) =>
            MakeFilter(FilterFunctionType.Grayscale, amount);

        public static FilterFunction FilterContrast(float amount) =>
            MakeFilter(FilterFunctionType.Contrast, amount);

        public static FilterFunction FilterHueRotate(float degrees) =>
            MakeFilter(FilterFunctionType.HueRotate, degrees);

        public static FilterFunction FilterInvert(float amount) =>
            MakeFilter(FilterFunctionType.Invert, amount);

        public static FilterFunction FilterOpacity(float amount) =>
            MakeFilter(FilterFunctionType.Opacity, amount);

        public static FilterFunction FilterSepia(float amount) =>
            MakeFilter(FilterFunctionType.Sepia, amount);

        public static FilterFunction FilterTint(Color color) =>
            MakeFilter(FilterFunctionType.Tint, color);

        // ── Material (Style.UnityMaterial) ──────────────────────────────
        /// <summary>
        /// Wraps a <see cref="Material"/> into the <see cref="StyleMaterialDefinition"/>
        /// value expected by <c>Style.UnityMaterial</c>. Mirrors the existing
        /// <see cref="FontDef"/> precedent for asset-backed style wrappers.
        /// <code>new Style { UnityMaterial = MaterialDef(myMat) }</code>
        /// </summary>
        public static StyleMaterialDefinition MaterialDef(Material material) =>
            new StyleMaterialDefinition(new MaterialDefinition(material));

        // ── Aspect ratio (Style.AspectRatio) ────────────────────────────
        /// <summary>
        /// Builds a <see cref="StyleRatio"/> from a single float ratio
        /// (width / height) for <c>Style.AspectRatio</c>.
        /// <code>new Style { AspectRatio = Ratio(16f / 9f) }</code>
        /// </summary>
        public static StyleRatio Ratio(float value) => new StyleRatio(new Ratio(value));
#endif

        // ── 9-slice ─────────────────────────────────────────────────────
        public static SliceType SliceFill => SliceType.Sliced;
        public static SliceType SliceTile => SliceType.Tiled;

        // ── Overflow clip box ───────────────────────────────────────────
        public static OverflowClipBox ClipPaddingBox => OverflowClipBox.PaddingBox;
        public static OverflowClipBox ClipContentBox => OverflowClipBox.ContentBox;

        // ── Text generator / editor text rendering ──────────────────────
        public static UnityEngine.TextGeneratorType TextGenStandard =>
            UnityEngine.TextGeneratorType.Standard;
        public static UnityEngine.TextGeneratorType TextGenAdvanced =>
            UnityEngine.TextGeneratorType.Advanced;

        public static EditorTextRenderingMode EditorTextSDF => EditorTextRenderingMode.SDF;
        public static EditorTextRenderingMode EditorTextBitmap => EditorTextRenderingMode.Bitmap;

        // ── Text shadow ─────────────────────────────────────────────────
        /// <summary>Construct a <see cref="TextShadow"/> from offset (px), blur (px), and color.</summary>
        public static TextShadow Shadow(float offsetX, float offsetY, float blur, Color color) =>
            new TextShadow
            {
                offset = new Vector2(offsetX, offsetY),
                blurRadius = blur,
                color = color,
            };

        // ── Font definition ─────────────────────────────────────────────
        /// <summary>Wrap a legacy <see cref="Font"/> into a <see cref="FontDefinition"/>.</summary>
        public static FontDefinition FontDef(Font font) => FontDefinition.FromFont(font);
    }
}
