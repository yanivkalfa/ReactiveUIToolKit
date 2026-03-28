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

export const EXAMPLE_CSS_HELPERS = `// Even more concise with CssHelpers
using static ReactiveUITK.Props.Typed.CssHelpers;

var panelStyle = new Style {
    Height = 80f,
    BorderRadius = Px(6),
    BackgroundColor = Rgba(0.2f, 0.2f, 0.25f, 0.9f),
    JustifyContent = JustifyCenter,
    AlignItems = AlignCenter,
    MarginBottom = 10f,
};`

export const EXAMPLE_IMPORT = `@using ReactiveUITK.Props.Typed      // Style, StyleKeys
@using UnityEngine
@using UnityEngine.UIElements
@using static ReactiveUITK.Props.Typed.CssHelpers`

export const EXAMPLE_LAYOUT = `var cardStyle = new Style {
    Width = Pct(100),
    Height = Px(200),
    MinWidth = 120f,
    MaxHeight = Pct(50),
    FlexDirection = Column,
    JustifyContent = SpaceBetween,
    AlignItems = AlignCenter,
    FlexGrow = 1f,
    FlexWrap = WrapOn,
    Padding = 16f,
    Margin = 8f,
};`

export const EXAMPLE_POSITIONING = `var overlayStyle = new Style {
    Position = Absolute,
    Left = 0f,
    Top = 0f,
    Right = 0f,
    Bottom = 0f,
};`

export const EXAMPLE_COLORS = `var headerStyle = new Style {
    Color = White,
    BackgroundColor = Hex("#1a1a2e"),
    BorderColor = Rgba(255, 200, 0),
    UnityTextOutlineColor = Black,
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
    TextAlign = MiddleCenter,
    UnityFontStyle = Bold,
    TextOverflow = Ellipsis,
    WhiteSpace = Nowrap,
};`

export const EXAMPLE_BACKGROUND = `var bgStyle = new Style {
    BackgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
    BackgroundSize = new BackgroundSize(BackgroundSizeType.Cover),
    TransformOrigin = new TransformOrigin(Length.Percent(50), Length.Percent(50), 0),
};`

export const EXAMPLE_TRANSFORMS = `var animatedStyle = new Style {
    Rotate = 45f,
    Scale = 1.2f,
    Translate = new Translate(Px(10), Px(-5), 0),
};`

export const EXAMPLE_CONDITIONAL = `var buttonStyle = new Style {
    BackgroundColor = isHovered
        ? Rgba(0.3f, 0.85f, 0.45f)
        : Rgba(0.2f, 0.2f, 0.25f, 0.9f),
    Opacity = isEnabled ? 1f : 0.5f,
};`

export const EXAMPLE_INLINE = `<Label text="Hello"
       style={new Style { Color = Green, FontSize = 18f }} />`

export const EXAMPLE_BOTH_APIs = `// Typed properties — type-safe, IDE completion
var safe = new Style {
    Width = 100f,
    FlexDirection = Row,
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
FlexDirection:  Row, Column, RowReverse, ColumnReverse
Justify:        JustifyStart, JustifyEnd, JustifyCenter, SpaceBetween, SpaceAround
Align:          AlignStart, AlignEnd, AlignCenter, Stretch, AlignAuto
Wrap:           WrapOn, NoWrap, WrapRev

// Layout enums
Position:       Relative, Absolute
DisplayStyle:   Flex, DisplayNone
Visibility:     Visible, Hidden
Overflow:       OverflowVisible, OverflowHidden
WhiteSpace:     Normal, Nowrap

// Text enums
TextOverflow:   Clip, Ellipsis
TextAnchor:     UpperLeft, UpperCenter, UpperRight,
                MiddleLeft, MiddleCenter, MiddleRight,
                LowerLeft, LowerCenter, LowerRight
FontStyle:      FontNormal, Bold, Italic, BoldItalic
TextOverflowPosition: OverflowStart, OverflowMiddle, OverflowEnd`

export const EXAMPLE_LENGTH_HELPERS = `// Length helpers
Pct(50)     → 50% (Length.Percent)
Px(100)     → 100px (Length.Pixel)

// Style keywords  
Auto        → StyleKeyword.Auto
None        → StyleKeyword.None
Initial     → StyleKeyword.Initial

// Color helpers
White, Black, Red, Green, Blue, Yellow, Cyan, Magenta, Grey, Transparent
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
      BorderColor = isSelected ? Hex("#00AAFF") : Transparent,
      BorderWidth = 2f,
  };

  return (
    <VisualElement className="card" style={highlight}>
      <Label text="Best of both worlds" />
    </VisualElement>
  );
}`
