using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public class Style : Dictionary<string, object>
    {
        public Style() { }

        public Style(int capacity)
            : base(capacity) { }

        public Style(IDictionary<string, object> dictionary)
            : base(dictionary) { }

        public void Add((string key, object value) entry)
        {
            this[entry.key] = entry.value;
        }

        public static Style Of(params (string key, object value)[] entries)
        {
            var style = new Style(entries?.Length ?? 0);
            if (entries != null)
            {
                for (int i = 0; i < entries.Length; i++)
                {
                    style[entries[i].key] = entries[i].value;
                }
            }
            return style;
        }

        // ── Layout (StyleLength) ─────────────────────────────────────────
        public StyleLength Width                  { set => this["width"] = value; }
        public StyleLength Height                 { set => this["height"] = value; }
        public StyleLength MinWidth               { set => this["minWidth"] = value; }
        public StyleLength MinHeight              { set => this["minHeight"] = value; }
        public StyleLength MaxWidth               { set => this["maxWidth"] = value; }
        public StyleLength MaxHeight              { set => this["maxHeight"] = value; }
        public StyleLength FlexBasis              { set => this["flexBasis"] = value; }

        // ── Positioning (StyleLength) ────────────────────────────────────
        public StyleLength Left                   { set => this["left"] = value; }
        public StyleLength Top                    { set => this["top"] = value; }
        public StyleLength Right                  { set => this["right"] = value; }
        public StyleLength Bottom                 { set => this["bottom"] = value; }

        // ── Spacing (StyleLength) ────────────────────────────────────────
        public StyleLength Margin                 { set => this["margin"] = value; }
        public StyleLength MarginLeft             { set => this["marginLeft"] = value; }
        public StyleLength MarginRight            { set => this["marginRight"] = value; }
        public StyleLength MarginTop              { set => this["marginTop"] = value; }
        public StyleLength MarginBottom           { set => this["marginBottom"] = value; }
        public StyleLength Padding                { set => this["padding"] = value; }
        public StyleLength PaddingLeft            { set => this["paddingLeft"] = value; }
        public StyleLength PaddingRight           { set => this["paddingRight"] = value; }
        public StyleLength PaddingTop             { set => this["paddingTop"] = value; }
        public StyleLength PaddingBottom          { set => this["paddingBottom"] = value; }

        // ── Border radius (StyleLength) ──────────────────────────────────
        public StyleLength BorderRadius           { set => this["borderRadius"] = value; }
        public StyleLength BorderTopLeftRadius     { set => this["borderTopLeftRadius"] = value; }
        public StyleLength BorderTopRightRadius    { set => this["borderTopRightRadius"] = value; }
        public StyleLength BorderBottomLeftRadius  { set => this["borderBottomLeftRadius"] = value; }
        public StyleLength BorderBottomRightRadius { set => this["borderBottomRightRadius"] = value; }

        // ── Text (StyleLength) ───────────────────────────────────────────
        public StyleLength FontSize               { set => this["fontSize"] = value; }
        public StyleLength LetterSpacing          { set => this["letterSpacing"] = value; }

        // ── Flexbox float (StyleFloat) ───────────────────────────────────
        public StyleFloat FlexGrow                { set => this["flexGrow"] = value; }
        public StyleFloat FlexShrink              { set => this["flexShrink"] = value; }
        public StyleFloat Opacity                 { set => this["opacity"] = value; }

        // ── Border width (StyleFloat) ────────────────────────────────────
        public StyleFloat BorderWidth             { set => this["borderWidth"] = value; }
        public StyleFloat BorderLeftWidth         { set => this["borderLeftWidth"] = value; }
        public StyleFloat BorderRightWidth        { set => this["borderRightWidth"] = value; }
        public StyleFloat BorderTopWidth          { set => this["borderTopWidth"] = value; }
        public StyleFloat BorderBottomWidth       { set => this["borderBottomWidth"] = value; }
        public StyleFloat UnityTextOutlineWidth   { set => this["unityTextOutlineWidth"] = value; }

        // ── Colors ───────────────────────────────────────────────────────
        public Color Color                        { set => this["color"] = value; }
        public Color BackgroundColor              { set => this["backgroundColor"] = value; }
        public Color BackgroundImageTint          { set => this["backgroundImageTint"] = value; }
        public Color BorderColor                  { set => this["borderColor"] = value; }
        public Color BorderLeftColor              { set => this["borderLeftColor"] = value; }
        public Color BorderRightColor             { set => this["borderRightColor"] = value; }
        public Color BorderTopColor               { set => this["borderTopColor"] = value; }
        public Color BorderBottomColor            { set => this["borderBottomColor"] = value; }
        public Color UnityTextOutlineColor        { set => this["unityTextOutlineColor"] = value; }

        // ── Enum styles ──────────────────────────────────────────────────
        public FlexDirection FlexDirection         { set => this["flexDirection"] = value; }
        public Wrap          FlexWrap              { set => this["flexWrap"] = value; }
        public Justify       JustifyContent        { set => this["justifyContent"] = value; }
        public Align         AlignItems            { set => this["alignItems"] = value; }
        public Align         AlignSelf             { set => this["alignSelf"] = value; }
        public Align         AlignContent          { set => this["alignContent"] = value; }
        public Position      Position              { set => this["position"] = value; }
        public DisplayStyle  Display               { set => this["display"] = value; }
        public Visibility    Visibility             { set => this["visibility"] = value; }
        public Overflow      Overflow              { set => this["overflow"] = value; }
        public WhiteSpace    WhiteSpace            { set => this["whiteSpace"] = value; }
        public TextAnchor    TextAlign             { set => this["textAlign"] = value; }
        public TextOverflow  TextOverflow          { set => this["textOverflow"] = value; }
        public FontStyle     UnityFontStyle        { set => this["unityFontStyle"] = value; }

        // ── Assets ───────────────────────────────────────────────────────
        public Texture2D     BackgroundImage       { set => this["backgroundImage"] = value; }
        public Font          FontFamily            { set => this["fontFamily"] = value; }

        // ── Background (compound structs) ────────────────────────────────
        public BackgroundRepeat   BackgroundRepeat    { set => this["backgroundRepeat"] = value; }
        public BackgroundPosition BackgroundPositionX { set => this["backgroundPositionX"] = value; }
        public BackgroundPosition BackgroundPositionY { set => this["backgroundPositionY"] = value; }
        public BackgroundSize     BackgroundSize      { set => this["backgroundSize"] = value; }

        // ── Transform origin ─────────────────────────────────────────────
        public TransformOrigin    TransformOrigin     { set => this["transformOrigin"] = value; }

        // ── Enum – text overflow position ───────────────────────────────
        public TextOverflowPosition UnityTextOverflowPosition { set => this["unityTextOverflowPosition"] = value; }

        // ── Transforms ───────────────────────────────────────────────
        public float         Rotate                { set => this["rotate"] = value; }
        public float         Scale                 { set => this["scale"] = value; }
        public Translate     Translate              { set => this["translate"] = value; }

        // ── Unity 6.3+ ──────────────────────────────────────────────────
#if UNITY_6000_3_OR_NEWER
        public StyleRatio    AspectRatio            { set => this["aspectRatio"] = value; }
        public StyleList<FilterFunction> Filter     { set => this["filter"] = value; }
        public StyleMaterialDefinition UnityMaterial { set => this["unityMaterial"] = value; }
#endif

        // ── Transitions ──────────────────────────────────────────────────
        public StyleList<TimeValue> TransitionDelay              { set => this["transitionDelay"] = value; }
        public StyleList<TimeValue> TransitionDuration           { set => this["transitionDuration"] = value; }
        public StyleList<StylePropertyName> TransitionProperty   { set => this["transitionProperty"] = value; }
        public StyleList<EasingFunction> TransitionTimingFunction { set => this["transitionTimingFunction"] = value; }
    }
}
