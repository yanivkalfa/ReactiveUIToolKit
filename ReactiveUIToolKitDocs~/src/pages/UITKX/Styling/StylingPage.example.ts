export const EXAMPLE_OLD_STYLE = `// Old untyped tuple syntax (still works as escape hatch)
var panelStyle = new Style {
    (StyleKeys.Height, 80f),
    (StyleKeys.BorderRadius, 6f),
    (StyleKeys.BackgroundColor, new Color(0.2f, 0.2f, 0.25f, 0.9f)),
    (StyleKeys.JustifyContent, "center"),
    (StyleKeys.AlignItems, "center"),
    (StyleKeys.MarginBottom, 10f),
};`

export const EXAMPLE_NEW_STYLE = `// New typed property syntax — compile-time checked
var panelStyle = new Style {
    Height = 80f,
    BorderRadius = 6f,
    BackgroundColor = new Color(0.2f, 0.2f, 0.25f, 0.9f),
    JustifyContent = JustifyCenter,
    AlignItems = AlignCenter,
    MarginBottom = 10f,
};`

export const EXAMPLE_CSS_HELPERS = `// CssHelpers shortcuts are auto-imported in .uitkx files
var panelStyle = new Style {
    Height = 80f,
    BorderRadius = Px(6),
    BackgroundColor = Rgba(0.2f, 0.2f, 0.25f, 0.9f),
    JustifyContent = JustifyCenter,
    AlignItems = AlignCenter,
    MarginBottom = 10f,
};`

export const EXAMPLE_IMPORT = `@using ReactiveUITK.Props.Typed      // Style (only needed in .cs files)
@using UnityEngine                    // Color, Vector2, etc.
// StyleKeys + CssHelpers are auto-imported — no @using needed`

export const EXAMPLE_LAYOUT = `var cardStyle = new Style {
    Width = Pct(100),
    Height = Px(200),
    MinWidth = 120f,
    MaxHeight = Pct(50),
    FlexDirection = FlexColumn,
    JustifyContent = JustifySpaceBetween,
    AlignItems = AlignCenter,
    FlexGrow = 1f,
    FlexWrap = WrapOn,
    Padding = 16f,
    Margin = 8f,
};`

export const EXAMPLE_POSITIONING = `var overlayStyle = new Style {
    Position = PosAbsolute,
    Left = 0f,
    Top = 0f,
    Right = 0f,
    Bottom = 0f,
};`

export const EXAMPLE_COLORS = `var headerStyle = new Style {
    Color = ColorWhite,
    BackgroundColor = Hex("#1a1a2e"),
    BorderColor = Rgba(255, 200, 0),
    UnityTextOutlineColor = ColorBlack,
};`

export const EXAMPLE_BORDERS = `var cardStyle = new Style {
    BorderRadius = 8f,
    BorderTopLeftRadius = Px(12),
    BorderWidth = 2f,
    BorderColor = Rgba(255, 255, 255, 128),
    BorderLeftColor = Red,
};`

export const EXAMPLE_TEXT = `var titleStyle = new Style {
    FontSize = 24f,
    LetterSpacing = 1.5f,
    TextAlign = TextMiddleCenter,
    UnityFontStyle = FontBold,
    TextOverflow = TextEllipsis,
    WhiteSpace = WsNowrap,
};`

export const EXAMPLE_BACKGROUND = `var bgStyle = new Style {
    BackgroundRepeat = BgRepeatNone,
    BackgroundSize = BgSizeCover,
    TransformOrigin = OriginCenter,
};`

export const EXAMPLE_TRANSFORMS = `var animatedStyle = new Style {
    Rotate = 45f,
    Scale = 1.2f,
    Translate = Xlate(Px(10), Px(-5)),
};`

export const EXAMPLE_CONDITIONAL = `var buttonStyle = new Style {
    BackgroundColor = isHovered
        ? Rgba(0.3f, 0.85f, 0.45f)
        : Rgba(0.2f, 0.2f, 0.25f, 0.9f),
    Opacity = isEnabled ? 1f : 0.5f,
};`

export const EXAMPLE_INLINE = `<Label text="Hello"
       style={new Style { Color = ColorGreen, FontSize = 18f }} />`

export const EXAMPLE_BOTH_APIs = `// Typed properties — type-safe, IDE completion
var safe = new Style {
    Width = 100f,
    FlexDirection = FlexRow,
};

// Tuple syntax — escape hatch for edge cases
var escape = new Style {
    (StyleKeys.Width, 100f),
    (StyleKeys.FlexDirection, "row"),
};

// Mix both in one style (not recommended but works)
var mixed = new Style {
    Width = 100f,
    (StyleKeys.CustomProperty, someValue),
};`

export const EXAMPLE_ENUM_TABLE = `// Flexbox enums
FlexDirection:  FlexRow, FlexColumn, FlexRowReverse, FlexColumnReverse
Justify:        JustifyStart, JustifyEnd, JustifyCenter, JustifySpaceBetween, JustifySpaceAround, JustifySpaceEvenly
Align:          AlignStart, AlignEnd, AlignCenter, AlignStretch, AlignAuto
Wrap:           WrapOn, WrapOff, WrapReverse

// Layout enums
Position:       PosRelative, PosAbsolute
DisplayStyle:   DisplayFlex, DisplayNone
Visibility:     VisVisible, VisHidden
Overflow:       OverflowVisible, OverflowHidden
WhiteSpace:     WsNormal, WsNowrap, WsPre, WsPreWrap

// Text enums
TextOverflow:   TextClip, TextEllipsis
TextAnchor:     TextUpperLeft, TextUpperCenter, TextUpperRight,
                TextMiddleLeft, TextMiddleCenter, TextMiddleRight,
                TextLowerLeft, TextLowerCenter, TextLowerRight
FontStyle:      FontNormal, FontBold, FontItalic, FontBoldItalic
TextOverflowPosition: TextOverflowStart, TextOverflowMiddle, TextOverflowEnd
TextAutoSizeMode: AutoSizeNone, AutoSizeBestFit`

export const EXAMPLE_LENGTH_HELPERS = `// Length helpers
Pct(50)     → 50% (Length.Percent)
Px(100)     → 100px (Length.Pixel)

// Style keywords  
StyleAuto        → StyleKeyword.Auto
StyleNone        → StyleKeyword.None
StyleInitial     → StyleKeyword.Initial

// Color helpers
ColorWhite, ColorBlack, ColorRed, ColorGreen, ColorBlue, ColorYellow, ColorCyan, ColorMagenta, ColorGrey, ColorTransparent
Hex("#FF0000")          → Color from hex string
Rgba(255, 0, 0)         → Color from 0-255 byte values
Rgba(1f, 0f, 0f, 0.5f)  → Color from 0-1 float values`

export const EXAMPLE_USS_BASIC = `@uss "./Card.uss"

component Card {
  return (
    <VisualElement>
      <Label text="Styled with USS" className="card-title" />
    </VisualElement>
  );
}`

export const EXAMPLE_USS_FILE = `/* Card.uss — standard Unity Style Sheet */
VisualElement {
    padding: 12px;
    background-color: rgba(30, 30, 35, 0.95);
    border-radius: 8px;
}

.card-title {
    font-size: 18px;
    color: white;
    -unity-font-style: bold;
}`

export const EXAMPLE_USS_MULTIPLE = `@uss "./base.uss"
@uss "./theme-dark.uss"

component ThemedPanel {
  return (
    <VisualElement className="panel">
      <Label text="Multiple stylesheets applied in order" />
    </VisualElement>
  );
}`

export const EXAMPLE_USS_COMBINED = `@uss "./Card.uss"
@using static ReactiveUITK.Props.Typed.CssHelpers

component Card {
  // USS handles static layout, typed Style handles dynamic values
  var highlight = new Style {
      BorderColor = isSelected ? Hex("#00AAFF") : ColorTransparent,
      BorderWidth = 2f,
  };

  return (
    <VisualElement className="card" style={highlight}>
      <Label text="Best of both worlds" />
    </VisualElement>
  );
}`
