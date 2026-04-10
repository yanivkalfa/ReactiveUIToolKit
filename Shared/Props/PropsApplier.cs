using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using ReactiveUITK.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props
{
    public static class PropsApplier
    {
        private static readonly Dictionary<
            VisualElement,
            Dictionary<string, object>
        > previousStyles = new();

        private static readonly IReadOnlyDictionary<string, object> s_emptyReadOnlyProps =
            new System.Collections.ObjectModel.ReadOnlyDictionary<string, object>(
                new Dictionary<string, object>(0));
        private static readonly IDictionary<string, object> s_emptyMutableProps =
            new Dictionary<string, object>(0);

        private static int totalStyleSets;
        private static int totalStyleResets;
        private static int totalEventsRegistered;
        private static int totalEventsRemoved;

        private static readonly char[] s_classNameSeparators = { ' ', '\t', '\n', '\r' };
        private static readonly HashSet<string> s_oldClassSet = new();
        private static readonly HashSet<string> s_newClassSet = new();

        private static readonly Dictionary<string, Action<VisualElement, object>> styleSetters =
            new(StringComparer.Ordinal);
        private static readonly Dictionary<string, Action<VisualElement>> styleResetters = new(
            StringComparer.Ordinal
        );

        static PropsApplier()
        {
            styleSetters["width"] = (e, v) =>
            {
                e.style.width = ConvertToStyleLength(v);
            };
            styleSetters["height"] = (e, v) =>
            {
                e.style.height = ConvertToStyleLength(v);
            };

            styleSetters["flexGrow"] = (e, v) =>
            {
                e.style.flexGrow = ConvertToStyleFloat(v);
            };
            styleSetters["flexShrink"] = (e, v) =>
            {
                e.style.flexShrink = ConvertToStyleFloat(v);
            };
            styleSetters["flexDirection"] = (e, v) =>
            {
                e.style.flexDirection = ConvertToFlexDirection(v);
            };
            styleSetters["justifyContent"] = (e, v) =>
            {
                e.style.justifyContent = ConvertToJustify(v);
            };
            styleSetters["alignItems"] = (e, v) =>
            {
                e.style.alignItems = ConvertToAlign(v);
            };
            styleSetters["alignSelf"] = (e, v) =>
            {
                e.style.alignSelf = ConvertToAlign(v);
            };
            styleSetters["alignContent"] = (e, v) =>
            {
                e.style.alignContent = ConvertToAlign(v);
            };
            styleSetters["flexWrap"] = (e, v) =>
            {
                if (v is Wrap w) e.style.flexWrap = w;
                else e.style.flexWrap = v is string fw && fw == "wrap" ? Wrap.Wrap : Wrap.NoWrap;
            };
            styleSetters["flexBasis"] = (e, v) =>
            {
                e.style.flexBasis = ConvertToStyleLength(v);
            };
            styleSetters["minWidth"] = (e, v) =>
            {
                e.style.minWidth = ConvertToStyleLength(v);
            };
            styleSetters["minHeight"] = (e, v) =>
            {
                e.style.minHeight = ConvertToStyleLength(v);
            };
            styleSetters["maxWidth"] = (e, v) =>
            {
                e.style.maxWidth = ConvertToStyleLength(v);
            };
            styleSetters["maxHeight"] = (e, v) =>
            {
                e.style.maxHeight = ConvertToStyleLength(v);
            };

            styleSetters["position"] = (e, v) =>
            {
                if (v is Position p) e.style.position = p;
                else e.style.position =
                    v is string s && s == "absolute" ? Position.Absolute : Position.Relative;
            };
            styleSetters["left"] = (e, v) =>
            {
                e.style.left = ConvertToStyleLength(v);
            };
            styleSetters["top"] = (e, v) =>
            {
                e.style.top = ConvertToStyleLength(v);
            };
            styleSetters["right"] = (e, v) =>
            {
                e.style.right = ConvertToStyleLength(v);
            };
            styleSetters["bottom"] = (e, v) =>
            {
                e.style.bottom = ConvertToStyleLength(v);
            };

            styleSetters["display"] = (e, v) =>
            {
                if (v is DisplayStyle d) e.style.display = d;
                else e.style.display =
                    v is string ds && ds == "none" ? DisplayStyle.None : DisplayStyle.Flex;
            };
            styleSetters["visibility"] = (e, v) =>
            {
                if (v is Visibility vis2) e.style.visibility = vis2;
                else e.style.visibility =
                    v is string vis && vis == "hidden" ? Visibility.Hidden : Visibility.Visible;
            };
            styleSetters["overflow"] = (e, v) =>
            {
                if (v is Overflow ov2) e.style.overflow = ov2;
                else e.style.overflow =
                    v is string ov && ov == "hidden" ? Overflow.Hidden : Overflow.Visible;
            };
            styleSetters["opacity"] = (e, v) =>
            {
                e.style.opacity = ConvertToStyleFloat(v);
            };
            styleSetters["whiteSpace"] = (e, v) =>
            {
                if (v is WhiteSpace ws2) e.style.whiteSpace = ws2;
                else e.style.whiteSpace =
                    v is string ws && ws == "nowrap" ? WhiteSpace.NoWrap : WhiteSpace.Normal;
            };

            styleSetters["fontSize"] = (e, v) =>
            {
                e.style.fontSize = ConvertToStyleLength(v);
            };
            styleSetters["textAlign"] = (e, v) =>
            {
                if (v is TextAnchor ta2) { e.style.unityTextAlign = ta2; }
                else if (v is string ta && ta == "center")
                {
                    e.style.unityTextAlign = TextAnchor.MiddleCenter;
                }
                else
                {
                    e.style.unityTextAlign = TextAnchor.UpperLeft;
                }
            };
            styleSetters["fontFamily"] = (e, v) =>
            {
                if (v is Font f)
                {
                    e.style.unityFont = f;
                }
            };
            styleSetters["unityFontStyle"] = (e, v) =>
            {
                if (v is FontStyle fs2) { e.style.unityFontStyleAndWeight = fs2; }
                else if (v is string fs && fs == "bold")
                {
                    e.style.unityFontStyleAndWeight = FontStyle.Bold;
                }
                else
                {
                    e.style.unityFontStyleAndWeight = FontStyle.Normal;
                }
            };
            styleSetters["color"] = (e, v) =>
            {
                e.style.color = ConvertToColor(v);
            };
            styleSetters["backgroundColor"] = (e, v) =>
            {
                e.style.backgroundColor = ConvertToColor(v);
            };
            styleSetters["letterSpacing"] = (e, v) =>
            {
                try
                {
                    e.style.letterSpacing = ConvertToStyleLength(v);
                }
                catch { }
            };
            styleSetters["textOverflow"] = (e, v) =>
            {
                try
                {
                    if (v is TextOverflow to2) { e.style.textOverflow = to2; }
                    else if (v is string ovf)
                    {
                        e.style.textOverflow =
                            ovf == "ellipsis" ? TextOverflow.Ellipsis : TextOverflow.Clip;
                    }
                }
                catch { }
            };
            styleSetters["unityTextOutlineColor"] = (e, v) =>
            {
                try
                {
                    e.style.unityTextOutlineColor = ConvertToColor(v);
                }
                catch { }
            };
            styleSetters["unityTextOutlineWidth"] = (e, v) =>
            {
                try
                {
                    e.style.unityTextOutlineWidth = ConvertToStyleFloat(v);
                }
                catch { }
            };
            styleSetters["unityTextOverflowPosition"] = (e, v) =>
            {
                if (v is TextOverflowPosition tp)
                {
                    try { e.style.unityTextOverflowPosition = tp; } catch { }
                }
                else if (v is string pos)
                {
                    try
                    {
                        e.style.unityTextOverflowPosition = pos switch
                        {
                            "middle" => TextOverflowPosition.Middle,
                            "end" => TextOverflowPosition.End,
                            _ => TextOverflowPosition.Start,
                        };
                    }
                    catch { }
                }
            };
            styleSetters["unityTextAutoSize"] = (e, v) =>
            {
                if (v is TextAutoSizeMode mode)
                {
                    try { e.style.unityTextAutoSize = new StyleTextAutoSize(new TextAutoSize(mode, default, default)); } catch { }
                }
                else if (v is string s)
                {
                    try
                    {
                        var m = s switch
                        {
                            "best-fit" => TextAutoSizeMode.BestFit,
                            _ => TextAutoSizeMode.None,
                        };
                        e.style.unityTextAutoSize = new StyleTextAutoSize(new TextAutoSize(m, default, default));
                    }
                    catch { }
                }
            };

            styleSetters["backgroundImage"] = (e, v) =>
            {
                if (v is Texture2D tex)
                {
                    e.style.backgroundImage = new StyleBackground(tex);
                }
                else if (v is Sprite sp)
                {
                    e.style.backgroundImage = new StyleBackground(sp);
                }
            };
            styleSetters["backgroundImageTint"] = (e, v) =>
            {
                e.style.unityBackgroundImageTintColor = ConvertToColor(v);
            };

            styleSetters["borderWidth"] = (e, v) =>
            {
                var bw = ConvertToStyleFloat(v);
                e.style.borderLeftWidth = bw;
                e.style.borderRightWidth = bw;
                e.style.borderTopWidth = bw;
                e.style.borderBottomWidth = bw;
            };
            styleSetters["borderColor"] = (e, v) =>
            {
                var col = ConvertToColor(v);
                e.style.borderLeftColor = col;
                e.style.borderRightColor = col;
                e.style.borderTopColor = col;
                e.style.borderBottomColor = col;
            };
            styleSetters["borderLeftWidth"] = (e, v) =>
            {
                e.style.borderLeftWidth = ConvertToStyleFloat(v);
            };
            styleSetters["borderRightWidth"] = (e, v) =>
            {
                e.style.borderRightWidth = ConvertToStyleFloat(v);
            };
            styleSetters["borderTopWidth"] = (e, v) =>
            {
                e.style.borderTopWidth = ConvertToStyleFloat(v);
            };
            styleSetters["borderBottomWidth"] = (e, v) =>
            {
                e.style.borderBottomWidth = ConvertToStyleFloat(v);
            };
            styleSetters["borderLeftColor"] = (e, v) =>
            {
                e.style.borderLeftColor = ConvertToColor(v);
            };
            styleSetters["borderRightColor"] = (e, v) =>
            {
                e.style.borderRightColor = ConvertToColor(v);
            };
            styleSetters["borderTopColor"] = (e, v) =>
            {
                e.style.borderTopColor = ConvertToColor(v);
            };
            styleSetters["borderBottomColor"] = (e, v) =>
            {
                e.style.borderBottomColor = ConvertToColor(v);
            };
            styleSetters["borderRadius"] = (e, v) =>
            {
                var len = ConvertToStyleLength(v);
                e.style.borderTopLeftRadius = len;
                e.style.borderTopRightRadius = len;
                e.style.borderBottomLeftRadius = len;
                e.style.borderBottomRightRadius = len;
            };
            styleSetters["borderTopLeftRadius"] = (e, v) =>
            {
                e.style.borderTopLeftRadius = ConvertToStyleLength(v);
            };
            styleSetters["borderTopRightRadius"] = (e, v) =>
            {
                e.style.borderTopRightRadius = ConvertToStyleLength(v);
            };
            styleSetters["borderBottomLeftRadius"] = (e, v) =>
            {
                e.style.borderBottomLeftRadius = ConvertToStyleLength(v);
            };
            styleSetters["borderBottomRightRadius"] = (e, v) =>
            {
                e.style.borderBottomRightRadius = ConvertToStyleLength(v);
            };

            styleSetters["margin"] = (e, v) =>
            {
                var len = ConvertToStyleLength(v);
                e.style.marginLeft = len;
                e.style.marginRight = len;
                e.style.marginTop = len;
                e.style.marginBottom = len;
            };
            styleSetters["padding"] = (e, v) =>
            {
                var len = ConvertToStyleLength(v);
                e.style.paddingLeft = len;
                e.style.paddingRight = len;
                e.style.paddingTop = len;
                e.style.paddingBottom = len;
            };
            styleSetters["marginLeft"] = (e, v) =>
            {
                e.style.marginLeft = ConvertToStyleLength(v);
            };
            styleSetters["marginRight"] = (e, v) =>
            {
                e.style.marginRight = ConvertToStyleLength(v);
            };
            styleSetters["marginTop"] = (e, v) =>
            {
                e.style.marginTop = ConvertToStyleLength(v);
            };
            styleSetters["marginBottom"] = (e, v) =>
            {
                e.style.marginBottom = ConvertToStyleLength(v);
            };
            styleSetters["paddingLeft"] = (e, v) =>
            {
                e.style.paddingLeft = ConvertToStyleLength(v);
            };
            styleSetters["paddingRight"] = (e, v) =>
            {
                e.style.paddingRight = ConvertToStyleLength(v);
            };
            styleSetters["paddingTop"] = (e, v) =>
            {
                e.style.paddingTop = ConvertToStyleLength(v);
            };
            styleSetters["paddingBottom"] = (e, v) =>
            {
                e.style.paddingBottom = ConvertToStyleLength(v);
            };

            styleSetters["rotate"] = (e, v) =>
            {
                try
                {
                    e.style.rotate = new Rotate(ConvertToFloat(v));
                }
                catch { }
            };
            styleSetters["scale"] = (e, v) =>
            {
                try
                {
                    if (v is float uni)
                    {
                        e.style.scale = new Scale(new Vector3(uni, uni, 1));
                        return;
                    }
                    if (v is IEnumerable seq)
                    {
                        float sx = 1,
                            sy = 1;
                        int i = 0;
                        foreach (var o in seq)
                        {
                            if (i == 0)
                            {
                                sx = ConvertToFloat(o);
                            }
                            else if (i == 1)
                            {
                                sy = ConvertToFloat(o);
                            }
                            i++;
                            if (i > 1)
                            {
                                break;
                            }
                        }
                        e.style.scale = new Scale(new Vector3(sx, sy, 1));
                    }
                }
                catch { }
            };
            styleSetters["translate"] = (e, v) =>
            {
                try
                {
                    if (v is Translate t)
                    {
                        e.style.translate = t;
                    }
                    else if (v is IEnumerable seq)
                    {
                        float tx = 0,
                            ty = 0;
                        int i = 0;
                        foreach (var o in seq)
                        {
                            if (i == 0)
                            {
                                tx = ConvertToFloat(o);
                            }
                            else if (i == 1)
                            {
                                ty = ConvertToFloat(o);
                            }
                            i++;
                            if (i > 1)
                            {
                                break;
                            }
                        }
                        e.style.translate = new Translate(
                            new Length(tx, LengthUnit.Pixel),
                            new Length(ty, LengthUnit.Pixel),
                            0
                        );
                    }
                }
                catch { }
            };

            styleSetters["flex"] = (e, v) =>
            {
                if (v is string flexStr)
                {
                    var parts = flexStr.Split(
                        new[] { ' ', '\t' },
                        StringSplitOptions.RemoveEmptyEntries
                    );
                    if (parts.Length > 0)
                    {
                        e.style.flexGrow = ConvertToFloat(parts[0]);
                    }
                    if (parts.Length > 1)
                    {
                        e.style.flexShrink = ConvertToFloat(parts[1]);
                    }
                    if (parts.Length > 2)
                    {
                        e.style.flexBasis = ConvertToStyleLength(parts[2]);
                    }
                }
            };
            styleSetters["backgroundRepeat"] = (e, v) =>
            {
                try
                {
                    if (v is BackgroundRepeat br)
                        e.style.backgroundRepeat = br;
                    else if (v is string s)
                    {
                        var r = s switch
                        {
                            "no-repeat" => Repeat.NoRepeat,
                            "space"     => Repeat.Space,
                            "round"     => Repeat.Round,
                            _           => Repeat.Repeat,
                        };
                        e.style.backgroundRepeat = new BackgroundRepeat(r, r);
                    }
                }
                catch { }
            };
            styleSetters["backgroundPositionX"] = (e, v) =>
            {
                try
                {
                    if (v is BackgroundPosition bp)
                        e.style.backgroundPositionX = bp;
                    else if (v is string s)
                    {
                        e.style.backgroundPositionX = s switch
                        {
                            "left"  => new BackgroundPosition(BackgroundPositionKeyword.Left),
                            "right" => new BackgroundPosition(BackgroundPositionKeyword.Right),
                            _       => new BackgroundPosition(BackgroundPositionKeyword.Center),
                        };
                    }
                }
                catch { }
            };
            styleSetters["backgroundPositionY"] = (e, v) =>
            {
                try
                {
                    if (v is BackgroundPosition bp)
                        e.style.backgroundPositionY = bp;
                    else if (v is string s)
                    {
                        e.style.backgroundPositionY = s switch
                        {
                            "top"    => new BackgroundPosition(BackgroundPositionKeyword.Top),
                            "bottom" => new BackgroundPosition(BackgroundPositionKeyword.Bottom),
                            _        => new BackgroundPosition(BackgroundPositionKeyword.Center),
                        };
                    }
                }
                catch { }
            };
            styleSetters["backgroundSize"] = (e, v) =>
            {
                try
                {
                    if (v is BackgroundSize bs)
                        e.style.backgroundSize = bs;
                    else if (v is string s)
                    {
                        e.style.backgroundSize = s switch
                        {
                            "cover"   => new BackgroundSize(BackgroundSizeType.Cover),
                            "contain" => new BackgroundSize(BackgroundSizeType.Contain),
                            _         => new BackgroundSize(BackgroundSizeType.Length),
                        };
                    }
                }
                catch { }
            };
            styleSetters["transformOrigin"] = (e, v) =>
            {
                try
                {
                    if (v is TransformOrigin to)
                        e.style.transformOrigin = to;
                }
                catch { }
            };
            // ── Unity 6.3+ (UNITY_6000_3_OR_NEWER) ─────────────────────────
#if UNITY_6000_3_OR_NEWER
            styleSetters["aspectRatio"] = (e, v) =>
            {
                if (v is StyleRatio sr)
                    e.style.aspectRatio = sr;
                else if (v is Ratio r)
                    e.style.aspectRatio = new StyleRatio(r);
                else if (v is float f)
                    e.style.aspectRatio = new StyleRatio(new Ratio(f));
                else if (v is int i)
                    e.style.aspectRatio = new StyleRatio(new Ratio(i));
            };
            styleSetters["filter"] = (e, v) =>
            {
                if (v is StyleList<FilterFunction> sl)
                    e.style.filter = sl;
                else if (v is System.Collections.Generic.List<FilterFunction> list)
                    e.style.filter = new StyleList<FilterFunction>(list);
            };
            styleSetters["unityMaterial"] = (e, v) =>
            {
                if (v is StyleMaterialDefinition smd)
                    e.style.unityMaterial = smd;
                else if (v is MaterialDefinition md)
                    e.style.unityMaterial = new StyleMaterialDefinition(md);
            };
#else
            styleSetters["aspectRatio"] = (e, v) => { };
            styleSetters["filter"] = (e, v) => { };
            styleSetters["unityMaterial"] = (e, v) => { };
#endif
            // TODO: cursor — StyleCursor wraps Cursor struct (Texture2D + hotspot). Unity has no built-in cursor constants.
            styleSetters["cursor"] = (e, v) => { };
            // transition — CSS shorthand only, no IStyle.transition in Unity
            styleSetters["transition"] = (e, v) => { };
            styleSetters["transitionDelay"] = (e, v) =>
            {
                if (v is StyleList<TimeValue> sl)
                    e.style.transitionDelay = sl;
                else if (v is System.Collections.Generic.List<TimeValue> list)
                    e.style.transitionDelay = new StyleList<TimeValue>(list);
            };
            styleSetters["transitionDuration"] = (e, v) =>
            {
                if (v is StyleList<TimeValue> sl)
                    e.style.transitionDuration = sl;
                else if (v is System.Collections.Generic.List<TimeValue> list)
                    e.style.transitionDuration = new StyleList<TimeValue>(list);
            };
            styleSetters["transitionProperty"] = (e, v) =>
            {
                if (v is StyleList<StylePropertyName> sl)
                    e.style.transitionProperty = sl;
                else if (v is System.Collections.Generic.List<StylePropertyName> list)
                    e.style.transitionProperty = new StyleList<StylePropertyName>(list);
            };
            styleSetters["transitionTimingFunction"] = (e, v) =>
            {
                if (v is StyleList<EasingFunction> sl)
                    e.style.transitionTimingFunction = sl;
                else if (v is System.Collections.Generic.List<EasingFunction> list)
                    e.style.transitionTimingFunction = new StyleList<EasingFunction>(list);
                else if (v is string s)
                {
                    var mode = ConvertToEasingMode(s);
                    e.style.transitionTimingFunction = new StyleList<EasingFunction>(
                        new System.Collections.Generic.List<EasingFunction> { new EasingFunction(mode) }
                    );
                }
            };
            // backgroundPosition kept as no-op for backward compat (IStyle splits to backgroundPositionX/Y)
            styleSetters["backgroundPosition"] = (e, v) => { };

            styleResetters["width"] = e =>
            {
                e.style.width = StyleKeyword.Null;
            };
            styleResetters["height"] = e =>
            {
                e.style.height = StyleKeyword.Null;
            };
            styleResetters["left"] = e =>
            {
                e.style.left = StyleKeyword.Null;
            };
            styleResetters["top"] = e =>
            {
                e.style.top = StyleKeyword.Null;
            };
            styleResetters["right"] = e =>
            {
                e.style.right = StyleKeyword.Null;
            };
            styleResetters["bottom"] = e =>
            {
                e.style.bottom = StyleKeyword.Null;
            };
            styleResetters["display"] = e =>
            {
                e.style.display = StyleKeyword.Null;
            };
            styleResetters["visibility"] = e =>
            {
                e.style.visibility = StyleKeyword.Null;
            };
            styleResetters["overflow"] = e =>
            {
                e.style.overflow = StyleKeyword.Null;
            };
            styleResetters["opacity"] = e =>
            {
                e.style.opacity = StyleKeyword.Null;
            };
            styleResetters["color"] = e =>
            {
                e.style.color = StyleKeyword.Null;
            };
            styleResetters["backgroundColor"] = e =>
            {
                e.style.backgroundColor = StyleKeyword.Null;
            };
            styleResetters["backgroundImage"] = e =>
            {
                e.style.backgroundImage = StyleKeyword.Null;
            };
            styleResetters["backgroundImageTint"] = e =>
            {
                e.style.unityBackgroundImageTintColor = StyleKeyword.Null;
            };
            styleResetters["borderLeftWidth"] = e =>
            {
                e.style.borderLeftWidth = StyleKeyword.Null;
            };
            styleResetters["borderRightWidth"] = e =>
            {
                e.style.borderRightWidth = StyleKeyword.Null;
            };
            styleResetters["borderTopWidth"] = e =>
            {
                e.style.borderTopWidth = StyleKeyword.Null;
            };
            styleResetters["borderBottomWidth"] = e =>
            {
                e.style.borderBottomWidth = StyleKeyword.Null;
            };
            styleResetters["borderLeftColor"] = e =>
            {
                e.style.borderLeftColor = StyleKeyword.Null;
            };
            styleResetters["borderRightColor"] = e =>
            {
                e.style.borderRightColor = StyleKeyword.Null;
            };
            styleResetters["borderTopColor"] = e =>
            {
                e.style.borderTopColor = StyleKeyword.Null;
            };
            styleResetters["borderBottomColor"] = e =>
            {
                e.style.borderBottomColor = StyleKeyword.Null;
            };
            styleResetters["borderTopLeftRadius"] = e =>
            {
                e.style.borderTopLeftRadius = StyleKeyword.Null;
            };
            styleResetters["borderTopRightRadius"] = e =>
            {
                e.style.borderTopRightRadius = StyleKeyword.Null;
            };
            styleResetters["borderBottomLeftRadius"] = e =>
            {
                e.style.borderBottomLeftRadius = StyleKeyword.Null;
            };
            styleResetters["borderBottomRightRadius"] = e =>
            {
                e.style.borderBottomRightRadius = StyleKeyword.Null;
            };
            styleResetters["marginLeft"] = e =>
            {
                e.style.marginLeft = StyleKeyword.Null;
            };
            styleResetters["marginRight"] = e =>
            {
                e.style.marginRight = StyleKeyword.Null;
            };
            styleResetters["marginTop"] = e =>
            {
                e.style.marginTop = StyleKeyword.Null;
            };
            styleResetters["marginBottom"] = e =>
            {
                e.style.marginBottom = StyleKeyword.Null;
            };
            styleResetters["paddingLeft"] = e =>
            {
                e.style.paddingLeft = StyleKeyword.Null;
            };
            styleResetters["paddingRight"] = e =>
            {
                e.style.paddingRight = StyleKeyword.Null;
            };
            styleResetters["paddingTop"] = e =>
            {
                e.style.paddingTop = StyleKeyword.Null;
            };
            styleResetters["paddingBottom"] = e =>
            {
                e.style.paddingBottom = StyleKeyword.Null;
            };
            styleResetters["flexGrow"] = e =>
            {
                e.style.flexGrow = StyleKeyword.Null;
            };
            styleResetters["flexShrink"] = e =>
            {
                e.style.flexShrink = StyleKeyword.Null;
            };
            styleResetters["flexBasis"] = e =>
            {
                e.style.flexBasis = StyleKeyword.Null;
            };
            styleResetters["minWidth"] = e =>
            {
                e.style.minWidth = StyleKeyword.Null;
            };
            styleResetters["minHeight"] = e =>
            {
                e.style.minHeight = StyleKeyword.Null;
            };
            styleResetters["maxWidth"] = e =>
            {
                e.style.maxWidth = StyleKeyword.Null;
            };
            styleResetters["maxHeight"] = e =>
            {
                e.style.maxHeight = StyleKeyword.Null;
            };
            styleResetters["whiteSpace"] = e =>
            {
                e.style.whiteSpace = StyleKeyword.Null;
            };
            styleResetters["letterSpacing"] = e =>
            {
                try
                {
                    e.style.letterSpacing = StyleKeyword.Null;
                }
                catch { }
            };
            styleResetters["textOverflow"] = e =>
            {
                try
                {
                    e.style.textOverflow = StyleKeyword.Null;
                }
                catch { }
            };
            styleResetters["unityTextOutlineColor"] = e =>
            {
                try
                {
                    e.style.unityTextOutlineColor = StyleKeyword.Null;
                }
                catch { }
            };
            styleResetters["unityTextOutlineWidth"] = e =>
            {
                try
                {
                    e.style.unityTextOutlineWidth = StyleKeyword.Null;
                }
                catch { }
            };
            styleResetters["unityTextOverflowPosition"] = e =>
            {
                try
                {
                    e.style.unityTextOverflowPosition = StyleKeyword.Null;
                }
                catch { }
            };
            styleResetters["unityTextAutoSize"] = e =>
            {
                try
                {
                    e.style.unityTextAutoSize = StyleKeyword.Null;
                }
                catch { }
            };
            styleResetters["unityOverflowClipBox"] = e =>
            {
                try
                {
                    e.style.unityOverflowClipBox = StyleKeyword.Null;
                }
                catch { }
            };
            styleResetters["unityParagraphSpacing"] = e =>
            {
                try
                {
                    e.style.unityParagraphSpacing = StyleKeyword.Null;
                }
                catch { }
            };
            styleResetters["unitySliceBottom"] = e =>
            {
                try
                {
                    e.style.unitySliceBottom = StyleKeyword.Null;
                }
                catch { }
            };
            styleResetters["unitySliceTop"] = e =>
            {
                try
                {
                    e.style.unitySliceTop = StyleKeyword.Null;
                }
                catch { }
            };
            styleResetters["unitySliceLeft"] = e =>
            {
                try
                {
                    e.style.unitySliceLeft = StyleKeyword.Null;
                }
                catch { }
            };
            styleResetters["unitySliceRight"] = e =>
            {
                try
                {
                    e.style.unitySliceRight = StyleKeyword.Null;
                }
                catch { }
            };
            styleResetters["unitySliceScale"] = e =>
            {
                try
                {
                    e.style.unitySliceScale = StyleKeyword.Null;
                }
                catch { }
            };
            styleResetters["unitySliceType"] = e =>
            {
                try
                {
                    e.style.unitySliceType = StyleKeyword.Null;
                }
                catch { }
            };
#if UNITY_6000_3_OR_NEWER
            styleResetters["aspectRatio"] = e =>
            {
                e.style.aspectRatio = StyleKeyword.Null;
            };
            styleResetters["filter"] = e =>
            {
                e.style.filter = StyleKeyword.Null;
            };
            styleResetters["unityMaterial"] = e =>
            {
                e.style.unityMaterial = StyleKeyword.Null;
            };
#endif
            styleResetters["transitionDelay"] = e =>
            {
                e.style.transitionDelay = StyleKeyword.Null;
            };
            styleResetters["transitionDuration"] = e =>
            {
                e.style.transitionDuration = StyleKeyword.Null;
            };
            styleResetters["transitionProperty"] = e =>
            {
                e.style.transitionProperty = StyleKeyword.Null;
            };
            styleResetters["transitionTimingFunction"] = e =>
            {
                e.style.transitionTimingFunction = StyleKeyword.Null;
            };
        }

        private static readonly Dictionary<string, string> s_canonicalizeCache = new(64);

        private static string Canonicalize(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return key;
            }
            if (s_canonicalizeCache.TryGetValue(key, out var cached))
            {
                return cached;
            }
            string result;
            if (key.StartsWith("-unity-", StringComparison.OrdinalIgnoreCase))
            {
                result = ToCamelCase(key.Substring(7));
            }
            else if (key.IndexOf('-') >= 0)
            {
                result = ToCamelCase(key);
            }
            else
            {
                result = key;
            }
            s_canonicalizeCache[key] = result;
            return result;
        }

        private static string ToCamelCase(string dashed)
        {
            var parts = dashed.Split('-', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return dashed;
            }
            for (int i = 1; i < parts.Length; i++)
            {
                var p = parts[i];
                if (p.Length == 0)
                {
                    continue;
                }
                parts[i] = char.ToUpperInvariant(p[0]) + p.Substring(1);
            }
            return parts[0].ToLowerInvariant()
                + string.Join(string.Empty, parts, 1, parts.Length - 1);
        }

        public static void Apply(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (properties == null)
            {
                return;
            }
            foreach (KeyValuePair<string, object> entry in properties)
            {
                ApplySingle(element, null, entry.Key, entry.Value);
            }
        }

        public static void ApplyDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            previous ??= s_emptyReadOnlyProps;
            next ??= s_emptyReadOnlyProps;

            foreach (KeyValuePair<string, object> prevEntry in previous)
            {
                if (prevEntry.Key == "style")
                {
                    continue;
                }
                if (next.ContainsKey(prevEntry.Key))
                {
                    continue;
                }
                RemoveProp(element, prevEntry.Key, prevEntry.Value);
            }

            foreach (KeyValuePair<string, object> currentEntry in next)
            {
                if (currentEntry.Key == "style")
                {
                    continue;
                }
                previous.TryGetValue(currentEntry.Key, out object oldValue);
                if (oldValue != null && ReferenceEquals(oldValue, currentEntry.Value))
                {
                    continue;
                }
                if (oldValue != null && oldValue.Equals(currentEntry.Value))
                {
                    continue;
                }
                ApplySingle(element, oldValue, currentEntry.Key, currentEntry.Value);
            }

            previous.TryGetValue("style", out var prevStyleObj);
            next.TryGetValue("style", out var nextStyleObj);
            if (prevStyleObj != null || nextStyleObj != null)
            {
                var prevMap =
                    prevStyleObj as IDictionary<string, object>
                    ?? s_emptyMutableProps;
                var nextMap =
                    nextStyleObj as IDictionary<string, object>
                    ?? s_emptyMutableProps;
                foreach (KeyValuePair<string, object> previousStyle in prevMap)
                {
                    if (!nextMap.ContainsKey(previousStyle.Key))
                    {
                        ResetStyle(element, previousStyle.Key);
                    }
                }
                foreach (KeyValuePair<string, object> nextStyle in nextMap)
                {
                    prevMap.TryGetValue(nextStyle.Key, out object previousValue);
                    if (previousValue != null && ReferenceEquals(previousValue, nextStyle.Value))
                    {
                        continue;
                    }
                    if (previousValue != null && previousValue.Equals(nextStyle.Value))
                    {
                        continue;
                    }
                    ApplyStyle(element, nextStyle.Key, nextStyle.Value);
                }
            }
        }

        private static void ApplySingle(
            VisualElement element,
            object oldValue,
            string propertyName,
            object propertyValue
        )
        {
            if (propertyName == "name")
            {
                element.name = propertyValue as string;
                return;
            }
            if (propertyName == "focusable")
            {
                if (propertyValue is bool fb)
                {
                    element.focusable = fb;
                }
                return;
            }
            if (propertyName == "tabIndex")
            {
                if (propertyValue is int ti)
                {
                    element.tabIndex = ti;
                }
                else if (
                    propertyValue != null
                    && int.TryParse(propertyValue.ToString(), out int parsed)
                )
                {
                    element.tabIndex = parsed;
                }
                return;
            }
            if (propertyName == "pickingMode")
            {
                if (propertyValue is string pm)
                {
                    pm = pm.ToLowerInvariant();
                    element.pickingMode =
                        pm == "ignore" ? PickingMode.Ignore : PickingMode.Position;
                }
                else if (propertyValue is PickingMode picking)
                {
                    element.pickingMode = picking;
                }
                return;
            }
            if (propertyName == "visible")
            {
                if (propertyValue is bool vis)
                {
                    element.visible = vis;
                }
                return;
            }
            if (propertyName == "enabled")
            {
                if (propertyValue is bool en)
                {
                    element.SetEnabled(en);
                }
                return;
            }
            if (propertyName == "tooltip")
            {
                element.tooltip = propertyValue as string;
                return;
            }
            if (propertyName == "viewDataKey")
            {
                element.viewDataKey = propertyValue as string;
                return;
            }
            if (propertyName == "delegatesFocus")
            {
                if (propertyValue is bool df)
                {
                    element.delegatesFocus = df;
                }
                return;
            }
            if (propertyName == "languageDirection")
            {
                if (propertyValue is LanguageDirection ld)
                {
                    element.languageDirection = ld;
                }
                return;
            }
            if (propertyName == "ref")
            {
                var meta = element.userData as NodeMetadata;
                if (meta == null && element.userData == null)
                {
                    meta = new NodeMetadata();
                    element.userData = meta;
                }
                if (meta != null)
                {
                    if (!ReferenceEquals(meta.AttachedRef, propertyValue))
                    {
                        if (meta.AttachedRef != null)
                        {
                            RefUtility.Assign(meta.AttachedRef, null);
                        }
                        meta.AttachedRef = propertyValue;
                    }
                    RefUtility.Assign(meta.AttachedRef, element);
                }
                else if (propertyValue != null)
                {
                    RefUtility.Assign(propertyValue, element);
                }
                return;
            }
            if (propertyName == "className" || propertyName == "class")
            {
                string newClasses = propertyValue as string;
                string oldClasses = oldValue as string;
                if (oldClasses != null)
                {
                    // Fast-path: both are single-token class names (no whitespace)
                    bool oldIsSingle = oldClasses.IndexOfAny(s_classNameSeparators) < 0;
                    bool newIsSingle = newClasses != null && newClasses.IndexOfAny(s_classNameSeparators) < 0;
                    if (oldIsSingle && newIsSingle)
                    {
                        if (!string.Equals(oldClasses, newClasses, StringComparison.Ordinal))
                        {
                            element.RemoveFromClassList(oldClasses);
                            element.AddToClassList(newClasses);
                        }
                    }
                    else if (oldIsSingle && string.IsNullOrEmpty(newClasses))
                    {
                        element.RemoveFromClassList(oldClasses);
                    }
                    else
                    {
                        // General diff with reusable static HashSets
                        s_oldClassSet.Clear();
                        s_newClassSet.Clear();
                        foreach (var token in (oldClasses ?? string.Empty).Split(s_classNameSeparators, StringSplitOptions.RemoveEmptyEntries))
                            s_oldClassSet.Add(token);
                        foreach (var token in (newClasses ?? string.Empty).Split(s_classNameSeparators, StringSplitOptions.RemoveEmptyEntries))
                            s_newClassSet.Add(token);

                        foreach (var cls in s_oldClassSet)
                        {
                            if (!s_newClassSet.Contains(cls))
                                element.RemoveFromClassList(cls);
                        }
                        foreach (var cls in s_newClassSet)
                        {
                            if (!s_oldClassSet.Contains(cls))
                                element.AddToClassList(cls);
                        }
                    }
                }
                else
                {
                    element.ClearClassList();
                    if (!string.IsNullOrEmpty(newClasses))
                    {
                        if (newClasses.IndexOfAny(s_classNameSeparators) < 0)
                        {
                            element.AddToClassList(newClasses);
                        }
                        else
                        {
                            foreach (var cls in newClasses.Split(s_classNameSeparators, StringSplitOptions.RemoveEmptyEntries))
                                element.AddToClassList(cls);
                        }
                    }
                }
                return;
            }
            if (propertyName == "classes")
            {
                element.ClearClassList();
                if (propertyValue is IEnumerable<string> list)
                {
                    foreach (var className in list)
                    {
                        if (!string.IsNullOrWhiteSpace(className))
                        {
                            element.AddToClassList(className);
                        }
                    }
                }
                return;
            }
            if (propertyName == "__ussKeys")
            {
                if (propertyValue is string[] keys)
                {
                    for (int i = 0; i < keys.Length; i++)
                    {
                        var sheet = UitkxAssetRegistry.Get<StyleSheet>(keys[i]);
                        if (sheet != null && !element.styleSheets.Contains(sheet))
                            element.styleSheets.Add(sheet);
                    }
                }
                return;
            }
            if (propertyName == "style" && propertyValue is IDictionary<string, object> styleMap)
            {
                foreach (KeyValuePair<string, object> styleEntry in styleMap)
                {
                    ApplyStyle(element, styleEntry.Key, styleEntry.Value);
                }
                return;
            }
            if (propertyName.StartsWith("on") && propertyValue is Delegate d)
            {
                ApplyEvent(element, propertyName, d, oldValue as Delegate);
                return;
            }
        }

        private static void RemoveProp(VisualElement element, string propertyName, object oldValue)
        {
            if (propertyName == "ref")
            {
                var meta = element.userData as NodeMetadata;
                object target = oldValue;
                if (meta != null)
                {
                    if (target == null)
                    {
                        target = meta.AttachedRef;
                    }
                    if (ReferenceEquals(meta.AttachedRef, target))
                    {
                        meta.AttachedRef = null;
                    }
                }
                if (target != null)
                {
                    RefUtility.Assign(target, null);
                }
                return;
            }
            if (propertyName.StartsWith("on") && oldValue is Delegate oldHandler)
            {
                var meta = element.userData as Core.NodeMetadata;
                if (meta != null)
                {
                    RemoveEvent(element, propertyName, oldHandler, meta);
                }
                return;
            }
            if (
                (propertyName == "className" || propertyName == "class")
                && oldValue is string oldClasses
            )
            {
                var tokens = oldClasses.Split(
                    s_classNameSeparators,
                    StringSplitOptions.RemoveEmptyEntries
                );
                foreach (var cls in tokens)
                {
                    element.RemoveFromClassList(cls);
                }
                return;
            }
            if (propertyName == "__ussKeys" && oldValue is string[] oldKeys)
            {
                for (int i = 0; i < oldKeys.Length; i++)
                {
                    var sheet = UitkxAssetRegistry.Get<StyleSheet>(oldKeys[i]);
                    if (sheet != null && element.styleSheets.Contains(sheet))
                        element.styleSheets.Remove(sheet);
                }
                return;
            }
            if (propertyName == "focusable")
            {
                element.focusable = false;
                return;
            }
            if (propertyName == "tabIndex")
            {
                element.tabIndex = 0;
                return;
            }
            if (propertyName == "pickingMode")
            {
                element.pickingMode = PickingMode.Position;
                return;
            }
            if (propertyName == "visible")
            {
                element.visible = true;
                return;
            }
            if (propertyName == "enabled")
            {
                element.SetEnabled(true);
                return;
            }
            if (propertyName == "tooltip")
            {
                element.tooltip = string.Empty;
                return;
            }
            if (propertyName == "viewDataKey")
            {
                element.viewDataKey = null;
                return;
            }
            if (propertyName == "delegatesFocus")
            {
                element.delegatesFocus = false;
                return;
            }
            if (propertyName == "style" && oldValue is IDictionary<string, object> oldMap)
            {
                foreach (var kv in oldMap)
                {
                    ResetStyle(element, kv.Key);
                }
                return;
            }
        }

        private static void ApplyStyle(VisualElement element, string styleKey, object value)
        {
            styleKey = Canonicalize(styleKey);
            if (!previousStyles.TryGetValue(element, out var prevMap))
            {
                prevMap = new Dictionary<string, object>();
                previousStyles[element] = prevMap;
            }
            prevMap[styleKey] = value;
            if (styleSetters.TryGetValue(styleKey, out var setter))
            {
                setter(element, value);
                totalStyleSets++;
                return;
            }

            return;
        }

        private static void ResetStyle(VisualElement element, string styleKey)
        {
            styleKey = Canonicalize(styleKey);
            if (styleResetters.TryGetValue(styleKey, out var reset))
            {
                reset(element);
                totalStyleResets++;
                return;
            }
            if (previousStyles.TryGetValue(element, out var prevMap))
            {
                prevMap.Remove(styleKey);
            }
            if (styleKey == "width")
            {
                element.style.width = StyleKeyword.Null;
                totalStyleResets++;
                return;
            }
            if (styleKey == "height")
            {
                element.style.height = StyleKeyword.Null;
                totalStyleResets++;
                return;
            }
            if (styleKey == "opacity")
            {
                element.style.opacity = StyleKeyword.Null;
                totalStyleResets++;
                return;
            }
            if (styleKey == "flexGrow")
            {
                element.style.flexGrow = StyleKeyword.Null;
                return;
            }
            if (styleKey == "flexShrink")
            {
                element.style.flexShrink = StyleKeyword.Null;
                return;
            }
            if (styleKey == "flexDirection")
            {
                element.style.flexDirection = StyleKeyword.Null;
                return;
            }
            if (styleKey == "justifyContent")
            {
                element.style.justifyContent = StyleKeyword.Null;
                return;
            }
            if (styleKey == "alignItems")
            {
                element.style.alignItems = StyleKeyword.Null;
                return;
            }
            if (styleKey == "alignSelf")
            {
                element.style.alignSelf = StyleKeyword.Null;
                return;
            }
            if (styleKey == "alignContent")
            {
                element.style.alignContent = StyleKeyword.Null;
                return;
            }
            if (styleKey == "fontSize")
            {
                element.style.fontSize = StyleKeyword.Null;
                return;
            }
            if (styleKey == "unityFontStyle")
            {
                element.style.unityFontStyleAndWeight = StyleKeyword.Null;
                return;
            }
            if (styleKey == "textAlign")
            {
                element.style.unityTextAlign = StyleKeyword.Null;
                return;
            }
            if (styleKey == "position")
            {
                element.style.position = StyleKeyword.Null;
                return;
            }
            if (styleKey == "left")
            {
                element.style.left = StyleKeyword.Null;
                return;
            }
            if (styleKey == "top")
            {
                element.style.top = StyleKeyword.Null;
                return;
            }
            if (styleKey == "right")
            {
                element.style.right = StyleKeyword.Null;
                return;
            }
            if (styleKey == "bottom")
            {
                element.style.bottom = StyleKeyword.Null;
                return;
            }
            if (styleKey == "flexWrap")
            {
                element.style.flexWrap = StyleKeyword.Null;
                return;
            }
            if (styleKey == "flexBasis")
            {
                element.style.flexBasis = StyleKeyword.Null;
                return;
            }
            if (styleKey == "minWidth")
            {
                element.style.minWidth = StyleKeyword.Null;
                return;
            }
            if (styleKey == "minHeight")
            {
                element.style.minHeight = StyleKeyword.Null;
                return;
            }
            if (styleKey == "maxWidth")
            {
                element.style.maxWidth = StyleKeyword.Null;
                return;
            }
            if (styleKey == "maxHeight")
            {
                element.style.maxHeight = StyleKeyword.Null;
                return;
            }
            if (styleKey == "display")
            {
                element.style.display = StyleKeyword.Null;
                return;
            }
            if (styleKey == "visibility")
            {
                element.style.visibility = StyleKeyword.Null;
                return;
            }
            if (styleKey == "overflow")
            {
                element.style.overflow = StyleKeyword.Null;
                return;
            }
            if (styleKey == "whiteSpace")
            {
                element.style.whiteSpace = StyleKeyword.Null;
                return;
            }
            if (styleKey == "backgroundImage")
            {
                element.style.backgroundImage = StyleKeyword.Null;
                return;
            }
            if (styleKey == "backgroundImageTint")
            {
                element.style.unityBackgroundImageTintColor = StyleKeyword.Null;
                return;
            }

            if (styleKey == "borderWidth")
            {
                element.style.borderLeftWidth = StyleKeyword.Null;
                element.style.borderRightWidth = StyleKeyword.Null;
                element.style.borderTopWidth = StyleKeyword.Null;
                element.style.borderBottomWidth = StyleKeyword.Null;
                return;
            }
            if (styleKey == "borderColor")
            {
                element.style.borderLeftColor = StyleKeyword.Null;
                element.style.borderRightColor = StyleKeyword.Null;
                element.style.borderTopColor = StyleKeyword.Null;
                element.style.borderBottomColor = StyleKeyword.Null;
                return;
            }
            if (styleKey == "color")
            {
                element.style.color = StyleKeyword.Null;
                return;
            }
            if (styleKey == "backgroundColor")
            {
                element.style.backgroundColor = StyleKeyword.Null;
                return;
            }
            if (styleKey == "borderRadius")
            {
                element.style.borderTopLeftRadius = StyleKeyword.Null;
                element.style.borderTopRightRadius = StyleKeyword.Null;
                element.style.borderBottomLeftRadius = StyleKeyword.Null;
                element.style.borderBottomRightRadius = StyleKeyword.Null;
                return;
            }
            if (styleKey == "borderTopLeftRadius")
            {
                element.style.borderTopLeftRadius = StyleKeyword.Null;
                return;
            }
            if (styleKey == "borderTopRightRadius")
            {
                element.style.borderTopRightRadius = StyleKeyword.Null;
                return;
            }
            if (styleKey == "borderBottomLeftRadius")
            {
                element.style.borderBottomLeftRadius = StyleKeyword.Null;
                return;
            }
            if (styleKey == "borderBottomRightRadius")
            {
                element.style.borderBottomRightRadius = StyleKeyword.Null;
                return;
            }
            if (styleKey == "margin")
            {
                element.style.marginLeft = StyleKeyword.Null;
                element.style.marginRight = StyleKeyword.Null;
                element.style.marginTop = StyleKeyword.Null;
                element.style.marginBottom = StyleKeyword.Null;
                return;
            }
            if (styleKey == "padding")
            {
                element.style.paddingLeft = StyleKeyword.Null;
                element.style.paddingRight = StyleKeyword.Null;
                element.style.paddingTop = StyleKeyword.Null;
                element.style.paddingBottom = StyleKeyword.Null;
                return;
            }
            if (styleKey == "marginLeft")
            {
                element.style.marginLeft = StyleKeyword.Null;
                return;
            }
            if (styleKey == "marginRight")
            {
                element.style.marginRight = StyleKeyword.Null;
                return;
            }
            if (styleKey == "marginTop")
            {
                element.style.marginTop = StyleKeyword.Null;
                return;
            }
            if (styleKey == "marginBottom")
            {
                element.style.marginBottom = StyleKeyword.Null;
                return;
            }
            if (styleKey == "paddingLeft")
            {
                element.style.paddingLeft = StyleKeyword.Null;
                return;
            }
            if (styleKey == "paddingRight")
            {
                element.style.paddingRight = StyleKeyword.Null;
                return;
            }
            if (styleKey == "paddingTop")
            {
                element.style.paddingTop = StyleKeyword.Null;
                return;
            }
            if (styleKey == "paddingBottom")
            {
                element.style.paddingBottom = StyleKeyword.Null;
                return;
            }

            if (styleKey == "letterSpacing")
            {
                try
                {
                    element.style.letterSpacing = StyleKeyword.Null;
                }
                catch { }
                return;
            }
            if (styleKey == "textOverflow")
            {
                try
                {
                    element.style.textOverflow = StyleKeyword.Null;
                }
                catch { }
                return;
            }
            if (styleKey == "unityTextOutlineColor")
            {
                try
                {
                    element.style.unityTextOutlineColor = StyleKeyword.Null;
                }
                catch { }
                return;
            }
            if (styleKey == "unityTextOutlineWidth")
            {
                try
                {
                    element.style.unityTextOutlineWidth = StyleKeyword.Null;
                }
                catch { }
                return;
            }
            if (styleKey == "unityTextOverflowPosition")
            {
                try
                {
                    element.style.unityTextOverflowPosition = StyleKeyword.Null;
                }
                catch { }
                return;
            }
            if (styleKey == "unityTextAutoSize")
            {
                try
                {
                    element.style.unityTextAutoSize = StyleKeyword.Null;
                }
                catch { }
                return;
            }
        }

        private static Color ConvertToColor(object value)
        {
            if (value is Color c)
            {
                return c;
            }
            if (value is Color32 c32)
            {
                return c32;
            }
            if (value is string s)
            {
                if (ColorUtility.TryParseHtmlString(s, out Color parsed))
                {
                    return parsed;
                }
            }
            return Color.white;
        }

        private static string ComputeHandlerSignature(Delegate del)
        {
            if (del == null)
            {
                return null;
            }
            try
            {
                var m = del.Method;
                if (m == null)
                {
                    return null;
                }
                string owner = m.DeclaringType != null ? m.DeclaringType.FullName : "<null>";
                string name = m.Name ?? "<noname>";

                return owner + "::" + name;
            }
            catch
            {
                return null;
            }
        }

        private static void ApplyEvent(
            VisualElement element,
            string eventPropName,
            Delegate newHandler,
            Delegate oldHandler
        )
        {
            Core.NodeMetadata meta = element.userData as Core.NodeMetadata;
            if (meta == null)
            {
                meta = new Core.NodeMetadata();
                element.userData = meta;
            }
            if (meta.EventHandlerSignatures == null)
            {
                meta.EventHandlerSignatures = new System.Collections.Generic.Dictionary<
                    string,
                    string
                >();
            }
            if (meta.EventHandlerTargets == null)
            {
                meta.EventHandlerTargets = new System.Collections.Generic.Dictionary<
                    string,
                    Delegate
                >();
            }

            meta.EventHandlerTargets[eventPropName] = newHandler;
            string newSig = ComputeHandlerSignature(newHandler);

            if (newHandler == null)
            {
                if (meta.EventHandlers.ContainsKey(eventPropName))
                {
                    RemoveEvent(element, eventPropName, meta.EventHandlers[eventPropName], meta);
                    meta.EventHandlerTargets.Remove(eventPropName);
                    meta.EventHandlerSignatures.Remove(eventPropName);
                }
                return;
            }

            if (eventPropName == "onClick")
            {
                if (!meta.EventHandlers.ContainsKey(eventPropName))
                {
                    EventCallback<ClickEvent> wrapper = e => InvokeEvent(meta, eventPropName, e);
                    element.RegisterCallback(wrapper);
                    meta.EventHandlers[eventPropName] = wrapper;
                }
                if (meta.EventHandlerSignatures != null)
                {
                    meta.EventHandlerSignatures[eventPropName] = newSig;
                }

                if (
                    ReactiveUITK.Core.Diagnostics.DiagnosticsConfig.CurrentTraceLevel
                    == ReactiveUITK.Core.Diagnostics.DiagnosticsConfig.TraceLevel.Verbose
                )
                {
                    try
                    {
                        UnityEngine.Debug.Log(
                            "[ApplyEvent] register onClick element="
                                + element.name
                                + ", parent="
                                + (element.parent != null ? element.parent.name : "<null>")
                        );
                    }
                    catch { }
                }
                totalEventsRegistered++;
                return;
            }
            if (eventPropName == "onPointerDown")
            {
                if (!meta.EventHandlers.ContainsKey(eventPropName))
                {
                    EventCallback<PointerDownEvent> w = e => InvokeEvent(meta, eventPropName, e);
                    element.RegisterCallback(w);
                    meta.EventHandlers[eventPropName] = w;
                }
                meta.EventHandlerSignatures[eventPropName] = newSig;
                return;
            }
            if (eventPropName == "onPointerUp")
            {
                if (!meta.EventHandlers.ContainsKey(eventPropName))
                {
                    EventCallback<PointerUpEvent> w = e => InvokeEvent(meta, eventPropName, e);
                    element.RegisterCallback(w);
                    meta.EventHandlers[eventPropName] = w;
                }
                meta.EventHandlerSignatures[eventPropName] = newSig;
                return;
            }
            if (eventPropName == "onPointerMove")
            {
                if (!meta.EventHandlers.ContainsKey(eventPropName))
                {
                    EventCallback<PointerMoveEvent> w = e => InvokeEvent(meta, eventPropName, e);
                    element.RegisterCallback(w);
                    meta.EventHandlers[eventPropName] = w;
                }
                meta.EventHandlerSignatures[eventPropName] = newSig;
                return;
            }
            if (eventPropName == "onPointerEnter")
            {
                if (!meta.EventHandlers.ContainsKey(eventPropName))
                {
                    EventCallback<PointerEnterEvent> w = e => InvokeEvent(meta, eventPropName, e);
                    element.RegisterCallback(w);
                    meta.EventHandlers[eventPropName] = w;
                }
                meta.EventHandlerSignatures[eventPropName] = newSig;
                return;
            }
            if (eventPropName == "onPointerLeave")
            {
                if (!meta.EventHandlers.ContainsKey(eventPropName))
                {
                    EventCallback<PointerLeaveEvent> w = e => InvokeEvent(meta, eventPropName, e);
                    element.RegisterCallback(w);
                    meta.EventHandlers[eventPropName] = w;
                }
                meta.EventHandlerSignatures[eventPropName] = newSig;
                return;
            }
            if (eventPropName == "onWheel")
            {
                if (!meta.EventHandlers.ContainsKey(eventPropName))
                {
                    EventCallback<WheelEvent> w = e => InvokeEvent(meta, eventPropName, e);
                    element.RegisterCallback(w);
                    meta.EventHandlers[eventPropName] = w;
                }
                meta.EventHandlerSignatures[eventPropName] = newSig;
                return;
            }
            if (eventPropName == "onFocus")
            {
                if (!meta.EventHandlers.ContainsKey(eventPropName))
                {
                    EventCallback<FocusEvent> w = e => InvokeEvent(meta, eventPropName, e);
                    element.RegisterCallback(w);
                    meta.EventHandlers[eventPropName] = w;
                }
                meta.EventHandlerSignatures[eventPropName] = newSig;
                return;
            }
            if (eventPropName == "onBlur")
            {
                if (!meta.EventHandlers.ContainsKey(eventPropName))
                {
                    EventCallback<BlurEvent> w = e => InvokeEvent(meta, eventPropName, e);
                    element.RegisterCallback(w);
                    meta.EventHandlers[eventPropName] = w;
                }
                meta.EventHandlerSignatures[eventPropName] = newSig;
                return;
            }
            if (eventPropName == "onFocusIn")
            {
                if (!meta.EventHandlers.ContainsKey(eventPropName))
                {
                    EventCallback<FocusInEvent> w = e => InvokeEvent(meta, eventPropName, e);
                    element.RegisterCallback(w);
                    meta.EventHandlers[eventPropName] = w;
                }
                meta.EventHandlerSignatures[eventPropName] = newSig;
                return;
            }
            if (eventPropName == "onFocusOut")
            {
                if (!meta.EventHandlers.ContainsKey(eventPropName))
                {
                    EventCallback<FocusOutEvent> w = e => InvokeEvent(meta, eventPropName, e);
                    element.RegisterCallback(w);
                    meta.EventHandlers[eventPropName] = w;
                }
                meta.EventHandlerSignatures[eventPropName] = newSig;
                return;
            }
            if (eventPropName == "onKeyDown")
            {
                if (!meta.EventHandlers.ContainsKey(eventPropName))
                {
                    EventCallback<KeyDownEvent> w = e => InvokeEvent(meta, eventPropName, e);
                    element.RegisterCallback(w);
                    meta.EventHandlers[eventPropName] = w;
                }
                meta.EventHandlerSignatures[eventPropName] = newSig;
                return;
            }
            if (eventPropName == "onKeyUp")
            {
                if (!meta.EventHandlers.ContainsKey(eventPropName))
                {
                    EventCallback<KeyUpEvent> w = e => InvokeEvent(meta, eventPropName, e);
                    element.RegisterCallback(w);
                    meta.EventHandlers[eventPropName] = w;
                }
                meta.EventHandlerSignatures[eventPropName] = newSig;
                return;
            }
            if (eventPropName == "onChange")
            {
                if (element is UnityEngine.UIElements.Toggle)
                {
                    if (!meta.EventHandlers.ContainsKey(eventPropName))
                    {
                        EventCallback<ChangeEvent<bool>> w = e =>
                            InvokeEvent(meta, eventPropName, e);
                        element.RegisterCallback(w);
                        meta.EventHandlers[eventPropName] = w;
                    }
                    meta.EventHandlerSignatures[eventPropName] = newSig;
                    return;
                }
                if (element is UnityEngine.UIElements.SliderInt)
                {
                    if (!meta.EventHandlers.ContainsKey(eventPropName))
                    {
                        EventCallback<ChangeEvent<int>> w = e =>
                            InvokeEvent(meta, eventPropName, e);
                        element.RegisterCallback(w);
                        meta.EventHandlers[eventPropName] = w;
                    }
                    meta.EventHandlerSignatures[eventPropName] = newSig;
                    return;
                }
                if (element is UnityEngine.UIElements.RadioButton)
                {
                    if (!meta.EventHandlers.ContainsKey(eventPropName))
                    {
                        EventCallback<ChangeEvent<bool>> w = e =>
                            InvokeEvent(meta, eventPropName, e);
                        element.RegisterCallback(w);
                        meta.EventHandlers[eventPropName] = w;
                    }
                    meta.EventHandlerSignatures[eventPropName] = newSig;
                    return;
                }
                if (element is UnityEngine.UIElements.RadioButtonGroup)
                {
                    if (!meta.EventHandlers.ContainsKey(eventPropName))
                    {
                        EventCallback<ChangeEvent<int>> w = e =>
                            InvokeEvent(meta, eventPropName, e);
                        element.RegisterCallback(w);
                        meta.EventHandlers[eventPropName] = w;
                    }
                    meta.EventHandlerSignatures[eventPropName] = newSig;
                    return;
                }
                if (element is UnityEngine.UIElements.Slider)
                {
                    if (!meta.EventHandlers.ContainsKey(eventPropName))
                    {
                        EventCallback<ChangeEvent<float>> w = e =>
                            InvokeEvent(meta, eventPropName, e);
                        element.RegisterCallback(w);
                        meta.EventHandlers[eventPropName] = w;
                    }
                    meta.EventHandlerSignatures[eventPropName] = newSig;
                    return;
                }
                if (element is UnityEngine.UIElements.Foldout)
                {
                    if (!meta.EventHandlers.ContainsKey(eventPropName))
                    {
                        EventCallback<ChangeEvent<bool>> w = e =>
                            InvokeEvent(meta, eventPropName, e);
                        element.RegisterCallback(w);
                        meta.EventHandlers[eventPropName] = w;
                    }
                    meta.EventHandlerSignatures[eventPropName] = newSig;
                    return;
                }

                {
                    if (!meta.EventHandlers.ContainsKey(eventPropName))
                    {
                        EventCallback<ChangeEvent<string>> w = e =>
                            InvokeEvent(meta, eventPropName, e);
                        element.RegisterCallback(w);
                        meta.EventHandlers[eventPropName] = w;
                    }
                    meta.EventHandlerSignatures[eventPropName] = newSig;
                    return;
                }
            }
            if (eventPropName == "onInput")
            {
                if (!meta.EventHandlers.ContainsKey(eventPropName))
                {
                    EventCallback<InputEvent> w = e => InvokeEvent(meta, eventPropName, e);
                    element.RegisterCallback(w);
                    meta.EventHandlers[eventPropName] = w;
                }
                meta.EventHandlerSignatures[eventPropName] = newSig;
                return;
            }
#if UNITY_EDITOR
            if (eventPropName == "onDragEnter")
            {
                if (!meta.EventHandlers.ContainsKey(eventPropName))
                {
                    EventCallback<DragEnterEvent> w = e => InvokeEvent(meta, eventPropName, e);
                    element.RegisterCallback(w);
                    meta.EventHandlers[eventPropName] = w;
                }
                meta.EventHandlerSignatures[eventPropName] = newSig;
                return;
            }
            if (eventPropName == "onDragLeave")
            {
                if (!meta.EventHandlers.ContainsKey(eventPropName))
                {
                    EventCallback<DragLeaveEvent> w = e => InvokeEvent(meta, eventPropName, e);
                    element.RegisterCallback(w);
                    meta.EventHandlers[eventPropName] = w;
                }
                meta.EventHandlerSignatures[eventPropName] = newSig;
                return;
            }
            if (eventPropName == "onDragUpdated")
            {
                if (!meta.EventHandlers.ContainsKey(eventPropName))
                {
                    EventCallback<DragUpdatedEvent> w = e => InvokeEvent(meta, eventPropName, e);
                    element.RegisterCallback(w);
                    meta.EventHandlers[eventPropName] = w;
                }
                meta.EventHandlerSignatures[eventPropName] = newSig;
                return;
            }
            if (eventPropName == "onDragPerform")
            {
                if (!meta.EventHandlers.ContainsKey(eventPropName))
                {
                    EventCallback<DragPerformEvent> w = e => InvokeEvent(meta, eventPropName, e);
                    element.RegisterCallback(w);
                    meta.EventHandlers[eventPropName] = w;
                }
                meta.EventHandlerSignatures[eventPropName] = newSig;
                return;
            }
            if (eventPropName == "onDragExited")
            {
                if (!meta.EventHandlers.ContainsKey(eventPropName))
                {
                    EventCallback<DragExitedEvent> w = e => InvokeEvent(meta, eventPropName, e);
                    element.RegisterCallback(w);
                    meta.EventHandlers[eventPropName] = w;
                }
                meta.EventHandlerSignatures[eventPropName] = newSig;
                return;
            }
#endif
            if (eventPropName == "onScroll")
            {
                if (!meta.EventHandlers.ContainsKey(eventPropName))
                {
                    EventCallback<WheelEvent> w = e => InvokeEvent(meta, eventPropName, e);
                    element.RegisterCallback(w);
                    meta.EventHandlers[eventPropName] = w;
                }
                meta.EventHandlerSignatures[eventPropName] = newSig;
                return;
            }
            if (eventPropName == "onGeometryChanged")
            {
                if (!meta.EventHandlers.ContainsKey(eventPropName))
                {
                    EventCallback<GeometryChangedEvent> w = e =>
                        InvokeEvent(meta, eventPropName, e);
                    element.RegisterCallback(w);
                    meta.EventHandlers[eventPropName] = w;
                }
                meta.EventHandlerSignatures[eventPropName] = newSig;
                return;
            }
            if (eventPropName == "onAttachToPanel")
            {
                if (!meta.EventHandlers.ContainsKey(eventPropName))
                {
                    EventCallback<AttachToPanelEvent> w = e => InvokeEvent(meta, eventPropName, e);
                    element.RegisterCallback(w);
                    meta.EventHandlers[eventPropName] = w;
                }
                meta.EventHandlerSignatures[eventPropName] = newSig;
                return;
            }
            if (eventPropName == "onDetachFromPanel")
            {
                if (!meta.EventHandlers.ContainsKey(eventPropName))
                {
                    EventCallback<DetachFromPanelEvent> w = e =>
                        InvokeEvent(meta, eventPropName, e);
                    element.RegisterCallback(w);
                    meta.EventHandlers[eventPropName] = w;
                }
                meta.EventHandlerSignatures[eventPropName] = newSig;
                return;
            }
        }

        private static void RemoveEvent(
            VisualElement element,
            string eventPropName,
            Delegate handler,
            Core.NodeMetadata meta
        )
        {
            if (handler == null)
            {
                return;
            }
            if (eventPropName == "onClick" && handler is EventCallback<ClickEvent> clickCb)
            {
                element.UnregisterCallback(clickCb);
                meta.EventHandlers.Remove(eventPropName);
                totalEventsRemoved++;
                return;
            }
            if (eventPropName == "onChange" && handler is EventCallback<ChangeEvent<int>> chi)
            {
                element.UnregisterCallback(chi);
                meta.EventHandlers.Remove(eventPropName);
                if (meta.EventHandlerSignatures != null)
                {
                    meta.EventHandlerSignatures.Remove(eventPropName);
                }
                totalEventsRemoved++;
                return;
            }
            if (eventPropName == "onPointerDown" && handler is EventCallback<PointerDownEvent> pd)
            {
                element.UnregisterCallback(pd);
                meta.EventHandlers.Remove(eventPropName);
                totalEventsRemoved++;
                return;
            }
            if (eventPropName == "onPointerUp" && handler is EventCallback<PointerUpEvent> pu)
            {
                element.UnregisterCallback(pu);
                meta.EventHandlers.Remove(eventPropName);
                totalEventsRemoved++;
                return;
            }
            if (eventPropName == "onPointerMove" && handler is EventCallback<PointerMoveEvent> pm)
            {
                element.UnregisterCallback(pm);
                meta.EventHandlers.Remove(eventPropName);
                totalEventsRemoved++;
                return;
            }
            if (eventPropName == "onPointerEnter" && handler is EventCallback<PointerEnterEvent> pe)
            {
                element.UnregisterCallback(pe);
                meta.EventHandlers.Remove(eventPropName);
                totalEventsRemoved++;
                return;
            }
            if (eventPropName == "onPointerLeave" && handler is EventCallback<PointerLeaveEvent> pl)
            {
                element.UnregisterCallback(pl);
                meta.EventHandlers.Remove(eventPropName);
                totalEventsRemoved++;
                return;
            }
            if (eventPropName == "onWheel" && handler is EventCallback<WheelEvent> we)
            {
                element.UnregisterCallback(we);
                meta.EventHandlers.Remove(eventPropName);
                totalEventsRemoved++;
                return;
            }
            if (eventPropName == "onFocus" && handler is EventCallback<FocusEvent> fe)
            {
                element.UnregisterCallback(fe);
                meta.EventHandlers.Remove(eventPropName);
                totalEventsRemoved++;
                return;
            }
            if (eventPropName == "onBlur" && handler is EventCallback<BlurEvent> be)
            {
                element.UnregisterCallback(be);
                meta.EventHandlers.Remove(eventPropName);
                totalEventsRemoved++;
                return;
            }
            if (eventPropName == "onFocusIn" && handler is EventCallback<FocusInEvent> fi)
            {
                element.UnregisterCallback(fi);
                meta.EventHandlers.Remove(eventPropName);
                totalEventsRemoved++;
                return;
            }
            if (eventPropName == "onFocusOut" && handler is EventCallback<FocusOutEvent> fo)
            {
                element.UnregisterCallback(fo);
                meta.EventHandlers.Remove(eventPropName);
                totalEventsRemoved++;
                return;
            }
            if (eventPropName == "onKeyDown" && handler is EventCallback<KeyDownEvent> kd)
            {
                element.UnregisterCallback(kd);
                meta.EventHandlers.Remove(eventPropName);
                totalEventsRemoved++;
                return;
            }
            if (eventPropName == "onKeyUp" && handler is EventCallback<KeyUpEvent> ku)
            {
                element.UnregisterCallback(ku);
                meta.EventHandlers.Remove(eventPropName);
                totalEventsRemoved++;
                return;
            }
            if (eventPropName == "onChange" && handler is EventCallback<ChangeEvent<string>> ch)
            {
                element.UnregisterCallback(ch);
                meta.EventHandlers.Remove(eventPropName);
                if (meta.EventHandlerSignatures != null)
                {
                    meta.EventHandlerSignatures.Remove(eventPropName);
                }
                totalEventsRemoved++;
                return;
            }
            if (eventPropName == "onChange" && handler is EventCallback<ChangeEvent<bool>> chb)
            {
                element.UnregisterCallback(chb);
                meta.EventHandlers.Remove(eventPropName);
                if (meta.EventHandlerSignatures != null)
                {
                    meta.EventHandlerSignatures.Remove(eventPropName);
                }
                totalEventsRemoved++;
                return;
            }
            if (eventPropName == "onChange" && handler is EventCallback<ChangeEvent<float>> chf)
            {
                element.UnregisterCallback(chf);
                meta.EventHandlers.Remove(eventPropName);
                if (meta.EventHandlerSignatures != null)
                {
                    meta.EventHandlerSignatures.Remove(eventPropName);
                }
                totalEventsRemoved++;
                return;
            }
            if (eventPropName == "onInput" && handler is EventCallback<InputEvent> ie)
            {
                element.UnregisterCallback(ie);
                meta.EventHandlers.Remove(eventPropName);
                totalEventsRemoved++;
                return;
            }
#if UNITY_EDITOR
            if (eventPropName == "onDragEnter" && handler is EventCallback<DragEnterEvent> de)
            {
                element.UnregisterCallback(de);
                meta.EventHandlers.Remove(eventPropName);
                totalEventsRemoved++;
                return;
            }
            if (eventPropName == "onDragLeave" && handler is EventCallback<DragLeaveEvent> dle)
            {
                element.UnregisterCallback(dle);
                meta.EventHandlers.Remove(eventPropName);
                totalEventsRemoved++;
                return;
            }
            if (eventPropName == "onDragUpdated" && handler is EventCallback<DragUpdatedEvent> du)
            {
                element.UnregisterCallback(du);
                meta.EventHandlers.Remove(eventPropName);
                totalEventsRemoved++;
                return;
            }
            if (eventPropName == "onDragPerform" && handler is EventCallback<DragPerformEvent> dp)
            {
                element.UnregisterCallback(dp);
                meta.EventHandlers.Remove(eventPropName);
                totalEventsRemoved++;
                return;
            }
            if (eventPropName == "onDragExited" && handler is EventCallback<DragExitedEvent> dx)
            {
                element.UnregisterCallback(dx);
                meta.EventHandlers.Remove(eventPropName);
                totalEventsRemoved++;
                return;
            }
#endif
            if (eventPropName == "onScroll" && handler is EventCallback<WheelEvent> se)
            {
                element.UnregisterCallback(se);
                meta.EventHandlers.Remove(eventPropName);
                totalEventsRemoved++;
                return;
            }
            if (
                eventPropName == "onGeometryChanged"
                && handler is EventCallback<GeometryChangedEvent> gc
            )
            {
                element.UnregisterCallback(gc);
                meta.EventHandlers.Remove(eventPropName);
                totalEventsRemoved++;
                return;
            }
            if (
                eventPropName == "onAttachToPanel"
                && handler is EventCallback<AttachToPanelEvent> atp
            )
            {
                element.UnregisterCallback(atp);
                meta.EventHandlers.Remove(eventPropName);
                totalEventsRemoved++;
                return;
            }
            if (
                eventPropName == "onDetachFromPanel"
                && handler is EventCallback<DetachFromPanelEvent> dfp
            )
            {
                element.UnregisterCallback(dfp);
                meta.EventHandlers.Remove(eventPropName);
                totalEventsRemoved++;
                return;
            }

            if (meta.EventHandlers.TryGetValue(eventPropName, out var stored))
            {
                try
                {
                    RemoveEvent(element, eventPropName, stored, meta);
                    return;
                }
                catch { }
            }
            meta.EventHandlers.Remove(eventPropName);
            totalEventsRemoved++;
        }

        private static void InvokeEvent(
            Core.NodeMetadata metadata,
            string eventPropName,
            EventBase evt
        )
        {
            if (metadata == null || string.IsNullOrEmpty(eventPropName))
            {
                return;
            }
            if (
                metadata.EventHandlerTargets == null
                || !metadata.EventHandlerTargets.TryGetValue(eventPropName, out var handler)
                || handler == null
            )
            {
                return;
            }
            var scheduler = ResolveScheduler(metadata);
            if (scheduler == null)
            {
                InvokeHandler(handler, evt);
                return;
            }
            scheduler.BeginBatch();
            try
            {
                InvokeHandler(handler, evt);
            }
            finally
            {
                scheduler.EndBatch();
            }
        }

        private static IScheduler ResolveScheduler(Core.NodeMetadata metadata)
        {
            if (metadata?.HostContext?.Environment == null)
            {
                return null;
            }
            if (
                metadata.HostContext.Environment.TryGetValue("scheduler", out var schedulerObj)
                && schedulerObj is IScheduler scheduler
            )
            {
                return scheduler;
            }
            return null;
        }

        private static void InvokeHandler(Delegate del, EventBase evt)
        {
            if (del == null)
            {
                return;
            }
            try
            {
                // ── Fast typed dispatch — covers 95%+ of real-world handlers ──
                // Action (zero-arg) — already the #1 path
                if (del is Action action) { action(); return; }

                // Common single-event-type handlers
                if (del is Action<ClickEvent> clickH) { clickH(evt as ClickEvent); return; }
                if (del is Action<PointerDownEvent> ptrDown) { ptrDown(evt as PointerDownEvent); return; }
                if (del is Action<PointerUpEvent> ptrUp) { ptrUp(evt as PointerUpEvent); return; }
                if (del is Action<PointerMoveEvent> ptrMove) { ptrMove(evt as PointerMoveEvent); return; }
                if (del is Action<PointerEnterEvent> ptrEnter) { ptrEnter(evt as PointerEnterEvent); return; }
                if (del is Action<PointerLeaveEvent> ptrLeave) { ptrLeave(evt as PointerLeaveEvent); return; }
                if (del is Action<PointerOverEvent> ptrOver) { ptrOver(evt as PointerOverEvent); return; }
                if (del is Action<PointerOutEvent> ptrOut) { ptrOut(evt as PointerOutEvent); return; }
                if (del is Action<MouseDownEvent> mouseDown) { mouseDown(evt as MouseDownEvent); return; }
                if (del is Action<MouseUpEvent> mouseUp) { mouseUp(evt as MouseUpEvent); return; }
                if (del is Action<MouseMoveEvent> mouseMove) { mouseMove(evt as MouseMoveEvent); return; }
                if (del is Action<MouseEnterEvent> mouseEnter) { mouseEnter(evt as MouseEnterEvent); return; }
                if (del is Action<MouseLeaveEvent> mouseLeave) { mouseLeave(evt as MouseLeaveEvent); return; }
                if (del is Action<MouseOverEvent> mouseOver) { mouseOver(evt as MouseOverEvent); return; }
                if (del is Action<MouseOutEvent> mouseOut) { mouseOut(evt as MouseOutEvent); return; }
                if (del is Action<WheelEvent> wheel) { wheel(evt as WheelEvent); return; }
                if (del is Action<FocusEvent> focus) { focus(evt as FocusEvent); return; }
                if (del is Action<BlurEvent> blur) { blur(evt as BlurEvent); return; }
                if (del is Action<FocusInEvent> focusIn) { focusIn(evt as FocusInEvent); return; }
                if (del is Action<FocusOutEvent> focusOut) { focusOut(evt as FocusOutEvent); return; }
                if (del is Action<KeyDownEvent> keyDown) { keyDown(evt as KeyDownEvent); return; }
                if (del is Action<KeyUpEvent> keyUp) { keyUp(evt as KeyUpEvent); return; }
                if (del is Action<NavigationSubmitEvent> navSubmit) { navSubmit(evt as NavigationSubmitEvent); return; }
                if (del is Action<NavigationCancelEvent> navCancel) { navCancel(evt as NavigationCancelEvent); return; }
                if (del is Action<NavigationMoveEvent> navMove) { navMove(evt as NavigationMoveEvent); return; }
                if (del is Action<GeometryChangedEvent> geom) { geom(evt as GeometryChangedEvent); return; }
                if (del is Action<AttachToPanelEvent> attach) { attach(evt as AttachToPanelEvent); return; }
                if (del is Action<DetachFromPanelEvent> detach) { detach(evt as DetachFromPanelEvent); return; }
                if (del is Action<TooltipEvent> tooltip) { tooltip(evt as TooltipEvent); return; }
                if (del is Action<TransitionEndEvent> transEnd) { transEnd(evt as TransitionEndEvent); return; }

                // ChangeEvent<T> typed handlers
                if (del is Action<ChangeEvent<string>> ceStr) { ceStr(evt as ChangeEvent<string>); return; }
                if (del is Action<ChangeEvent<bool>> ceBool) { ceBool(evt as ChangeEvent<bool>); return; }
                if (del is Action<ChangeEvent<int>> ceInt) { ceInt(evt as ChangeEvent<int>); return; }
                if (del is Action<ChangeEvent<float>> ceFloat) { ceFloat(evt as ChangeEvent<float>); return; }
                if (del is Action<ChangeEvent<double>> ceDouble) { ceDouble(evt as ChangeEvent<double>); return; }
                if (del is Action<ChangeEvent<long>> ceLong) { ceLong(evt as ChangeEvent<long>); return; }
                if (del is Action<ChangeEvent<Enum>> ceEnum) { ceEnum(evt as ChangeEvent<Enum>); return; }
                if (del is Action<ChangeEvent<UnityEngine.Object>> ceObj) { ceObj(evt as ChangeEvent<UnityEngine.Object>); return; }

                // ReactiveEvent handler
                if (del is Action<ReactiveEvent> reactiveH)
                {
                    var synth = ReactiveEvent.Create(evt);
                    if (synth != null) synth.CurrentTarget = evt?.currentTarget as VisualElement;
                    reactiveH(synth);
                    return;
                }

                // ── Extract newValue from ChangeEvent<T> for shorthand value handlers ──
                object newValue = null;
                if (evt != null)
                {
                    var evtType = evt.GetType();
                    if (evtType.IsGenericType
                        && evtType.Name.StartsWith("ChangeEvent`", StringComparison.Ordinal))
                    {
                        try { newValue = evtType.GetProperty("newValue")?.GetValue(evt); }
                        catch { }
                    }
                }

                // Value-shorthand handlers: Action<string>, Action<bool>, etc.
                if (del is Action<string> strH) { strH(newValue as string); return; }
                if (del is Action<bool> boolH && newValue is bool bv) { boolH(bv); return; }
                if (del is Action<int> intH && newValue is int iv) { intH(iv); return; }
                if (del is Action<float> floatH && newValue is float fv) { floatH(fv); return; }
                if (del is Action<double> doubleH && newValue is double dv) { doubleH(dv); return; }
                if (del is Action<long> longH && newValue is long lv) { longH(lv); return; }
                if (del is Action<object> objH) { objH(newValue ?? (object)evt); return; }

                // EventBase base type
                if (del is Action<EventBase> evtH) { evtH(evt); return; }

                // ── Slow fallback — DynamicInvoke for unknown signatures ──
                var method = del.Method;
                var parameters = method.GetParameters();

                if (parameters.Length == 0)
                {
                    del.DynamicInvoke();
                    return;
                }

                if (parameters.Length == 1)
                {
                    var p0 = parameters[0].ParameterType;
                    if (evt != null && p0.IsAssignableFrom(evt.GetType()))
                    {
                        del.DynamicInvoke(evt);
                        return;
                    }
                    if (newValue != null && p0.IsInstanceOfType(newValue))
                    {
                        del.DynamicInvoke(newValue);
                        return;
                    }
                    // Lazy synthetic event creation for fallback path only
                    var syntheticEvent = ReactiveEvent.Create(evt);
                    if (syntheticEvent != null)
                    {
                        syntheticEvent.CurrentTarget = evt?.currentTarget as VisualElement;
                        if (p0.IsInstanceOfType(syntheticEvent))
                        {
                            del.DynamicInvoke(syntheticEvent);
                            return;
                        }
                    }
                    if (p0 == typeof(object))
                    {
                        del.DynamicInvoke(newValue ?? (object)evt);
                        return;
                    }
                    return;
                }

                // Multi-param fallback
                {
                    object[] args = new object[parameters.Length];
                    var p0 = parameters[0].ParameterType;
                    if (evt != null && p0.IsAssignableFrom(evt.GetType()))
                        args[0] = evt;
                    else if (newValue != null && p0.IsInstanceOfType(newValue))
                        args[0] = newValue;
                    else
                    {
                        try { if (newValue != null) args[0] = System.Convert.ChangeType(newValue, p0); }
                        catch { args[0] = null; }
                    }
                    del.DynamicInvoke(args);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Event handler exception: " + ex);
            }
        }

        private static float ConvertToFloat(object value)
        {
            if (value is float f)
            {
                return f;
            }

            if (value is int i)
            {
                return i;
            }

            if (float.TryParse(value?.ToString(), out float parsed))
            {
                return parsed;
            }

            return 0f;
        }

        private static StyleFloat ConvertToStyleFloat(object value)
        {
            if (TryConvertToStyleKeyword(value, out var keyword))
            {
                return new StyleFloat(keyword);
            }

            return new StyleFloat(ConvertToFloat(value));
        }

        private static StyleLength ConvertToStyleLength(object value)
        {
            if (value is StyleLength styleLength)
            {
                return styleLength;
            }

            if (TryConvertToStyleKeyword(value, out var keyword))
            {
                return new StyleLength(keyword);
            }

            return new StyleLength(ConvertToLength(value));
        }

        private static bool TryConvertToStyleKeyword(object value, out StyleKeyword keyword)
        {
            if (value is StyleKeyword direct && direct != StyleKeyword.Undefined)
            {
                keyword = direct;
                return true;
            }

            if (value is string s && !string.IsNullOrWhiteSpace(s))
            {
                switch (s.Trim().ToLowerInvariant())
                {
                    case "auto":
                        keyword = StyleKeyword.Auto;
                        return true;
                    case "none":
                        keyword = StyleKeyword.None;
                        return true;
                    case "initial":
                        keyword = StyleKeyword.Initial;
                        return true;
                    case "null":
                    case "unset":
                    case "default":
                        keyword = StyleKeyword.Null;
                        return true;
                }
            }

            if (value == null)
            {
                keyword = StyleKeyword.Null;
                return true;
            }

            keyword = StyleKeyword.Undefined;
            return false;
        }

        private static Length ConvertToLength(object value)
        {
            if (value is Length existingLength)
            {
                return existingLength;
            }

            if (value is StyleLength styleLength)
            {
                return styleLength.value;
            }

            if (value is float f)
            {
                return new Length(f, LengthUnit.Pixel);
            }

            if (value is double d)
            {
                return new Length((float)d, LengthUnit.Pixel);
            }

            if (value is int i)
            {
                return new Length(i, LengthUnit.Pixel);
            }

            if (value is string s)
            {
                string trimmed = s.Trim();
                if (trimmed.EndsWith("%", StringComparison.Ordinal))
                {
                    string number = trimmed.Substring(0, trimmed.Length - 1);
                    if (
                        float.TryParse(
                            number,
                            NumberStyles.Float,
                            CultureInfo.InvariantCulture,
                            out float percent
                        )
                    )
                    {
                        return new Length(percent, LengthUnit.Percent);
                    }
                }
                else if (
                    trimmed.EndsWith("px", StringComparison.OrdinalIgnoreCase)
                    && float.TryParse(
                        trimmed[..^2],
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out float pixelsWithUnit
                    )
                )
                {
                    return new Length(pixelsWithUnit, LengthUnit.Pixel);
                }
                else if (
                    float.TryParse(
                        trimmed,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out float pixels
                    )
                )
                {
                    return new Length(pixels, LengthUnit.Pixel);
                }
            }

            return new Length(0f, LengthUnit.Pixel);
        }

        private static FlexDirection ConvertToFlexDirection(object value)
        {
            if (value is FlexDirection fd) return fd;
            if (value is string s)
            {
                if (s == "row")
                {
                    return FlexDirection.Row;
                }

                if (s == "column")
                {
                    return FlexDirection.Column;
                }
            }

            return FlexDirection.Row;
        }

        private static Justify ConvertToJustify(object value)
        {
            if (value is Justify j) return j;
            if (value is string s)
            {
                if (s == "center")
                {
                    return Justify.Center;
                }

                if (s == "flex-start")
                {
                    return Justify.FlexStart;
                }

                if (s == "flex-end")
                {
                    return Justify.FlexEnd;
                }

                if (s == "space-between")
                {
                    return Justify.SpaceBetween;
                }

                if (s == "space-around")
                {
                    return Justify.SpaceAround;
                }
            }

            return Justify.FlexStart;
        }

        private static Align ConvertToAlign(object value)
        {
            if (value is Align a) return a;
            if (value is string s)
            {
                if (s == "center")
                {
                    return Align.Center;
                }

                if (s == "flex-start")
                {
                    return Align.FlexStart;
                }

                if (s == "flex-end")
                {
                    return Align.FlexEnd;
                }

                if (s == "stretch")
                {
                    return Align.Stretch;
                }
            }

            return Align.Stretch;
        }

        private static EasingMode ConvertToEasingMode(string s) => s switch
        {
            "linear"              => EasingMode.Linear,
            "ease-in"             => EasingMode.EaseIn,
            "ease-out"            => EasingMode.EaseOut,
            "ease-in-out"         => EasingMode.EaseInOut,
            "ease-in-sine"        => EasingMode.EaseInSine,
            "ease-out-sine"       => EasingMode.EaseOutSine,
            "ease-in-out-sine"    => EasingMode.EaseInOutSine,
            "ease-in-cubic"       => EasingMode.EaseInCubic,
            "ease-out-cubic"      => EasingMode.EaseOutCubic,
            "ease-in-out-cubic"   => EasingMode.EaseInOutCubic,
            "ease-in-circ"        => EasingMode.EaseInCirc,
            "ease-out-circ"       => EasingMode.EaseOutCirc,
            "ease-in-out-circ"    => EasingMode.EaseInOutCirc,
            "ease-in-elastic"     => EasingMode.EaseInElastic,
            "ease-out-elastic"    => EasingMode.EaseOutElastic,
            "ease-in-out-elastic" => EasingMode.EaseInOutElastic,
            "ease-in-back"        => EasingMode.EaseInBack,
            "ease-out-back"       => EasingMode.EaseOutBack,
            "ease-in-out-back"    => EasingMode.EaseInOutBack,
            "ease-in-bounce"      => EasingMode.EaseInBounce,
            "ease-out-bounce"     => EasingMode.EaseOutBounce,
            "ease-in-out-bounce"  => EasingMode.EaseInOutBounce,
            _                     => EasingMode.Ease,
        };

        public static void NotifyElementRemoved(VisualElement element)
        {
            if (element == null)
            {
                return;
            }
            previousStyles.Remove(element);
        }

        public static (
            int styleSets,
            int styleResets,
            int eventsAdded,
            int eventsRemoved
        ) GetStyleMetrics() =>
            (totalStyleSets, totalStyleResets, totalEventsRegistered, totalEventsRemoved);

        private static void TrySetStyleField(
            VisualElement element,
            string fieldName,
            object value
        ) { }
    }
}
