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
        private static int totalStyleSets;
        private static int totalStyleResets;
        private static int totalEventsRegistered;
        private static int totalEventsRemoved;

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
                e.style.flexWrap = v is string fw && fw == "wrap" ? Wrap.Wrap : Wrap.NoWrap;
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
                e.style.position =
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
                e.style.display =
                    v is string ds && ds == "none" ? DisplayStyle.None : DisplayStyle.Flex;
            };
            styleSetters["visibility"] = (e, v) =>
            {
                e.style.visibility =
                    v is string vis && vis == "hidden" ? Visibility.Hidden : Visibility.Visible;
            };
            styleSetters["overflow"] = (e, v) =>
            {
                e.style.overflow =
                    v is string ov && ov == "hidden" ? Overflow.Hidden : Overflow.Visible;
            };
            styleSetters["opacity"] = (e, v) =>
            {
                e.style.opacity = ConvertToStyleFloat(v);
            };
            styleSetters["whiteSpace"] = (e, v) =>
            {
                e.style.whiteSpace =
                    v is string ws && ws == "nowrap" ? WhiteSpace.NoWrap : WhiteSpace.Normal;
            };

            styleSetters["fontSize"] = (e, v) =>
            {
                e.style.fontSize = ConvertToStyleLength(v);
            };
            styleSetters["textAlign"] = (e, v) =>
            {
                if (v is string ta && ta == "center")
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
                if (v is string fs && fs == "bold")
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
                if (v is string ovf)
                {
                    try
                    {
                        e.style.textOverflow =
                            ovf == "ellipsis" ? TextOverflow.Ellipsis : TextOverflow.Clip;
                    }
                    catch { }
                }
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
                if (v is string pos)
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
                    if (v is IEnumerable seq)
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
            styleSetters["cursor"] = (e, v) =>
            {
                try
                {
                    var uiCursor = new UnityEngine.UIElements.Cursor();
                    e.style.cursor = new StyleCursor(uiCursor);
                }
                catch { }
            };
            styleSetters["backgroundRepeat"] = (e, v) => { };
            styleSetters["backgroundPosition"] = (e, v) => { };
            styleSetters["backgroundSize"] = (e, v) => { };
            styleSetters["filter"] = (e, v) => { };
            styleSetters["transformOrigin"] = (e, v) => { };
            styleSetters["transition"] = (e, v) => { };
            styleSetters["transitionDelay"] = (e, v) => { };
            styleSetters["transitionDuration"] = (e, v) => { };
            styleSetters["transitionProperty"] = (e, v) => { };
            styleSetters["transitionTimingFunction"] = (e, v) => { };

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
        }

        private static string Canonicalize(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return key;
            }
            if (key.StartsWith("-unity-", StringComparison.OrdinalIgnoreCase))
            {
                return ToCamelCase(key.Substring(7));
            }
            if (key.IndexOf('-') >= 0)
            {
                return ToCamelCase(key);
            }
            return key;
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
            previous ??= new Dictionary<string, object>();
            next ??= new Dictionary<string, object>();

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
                    ?? (IDictionary<string, object>)new Dictionary<string, object>();
                var nextMap =
                    nextStyleObj as IDictionary<string, object>
                    ?? (IDictionary<string, object>)new Dictionary<string, object>();
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
                    var oldSet = new HashSet<string>(
                        (oldClasses ?? string.Empty).Split(
                            new[] { ' ', '\t', '\n', '\r' },
                            StringSplitOptions.RemoveEmptyEntries
                        )
                    );
                    var newSet = new HashSet<string>(
                        (newClasses ?? string.Empty).Split(
                            new[] { ' ', '\t', '\n', '\r' },
                            StringSplitOptions.RemoveEmptyEntries
                        )
                    );
                    foreach (var cls in oldSet)
                    {
                        if (!newSet.Contains(cls))
                        {
                            element.RemoveFromClassList(cls);
                        }
                    }
                    foreach (var cls in newSet)
                    {
                        if (!oldSet.Contains(cls))
                        {
                            element.AddToClassList(cls);
                        }
                    }
                }
                else
                {
                    element.ClearClassList();
                    if (!string.IsNullOrEmpty(newClasses))
                    {
                        var tokens = newClasses.Split(
                            new[] { ' ', '\t', '\n', '\r' },
                            StringSplitOptions.RemoveEmptyEntries
                        );
                        foreach (var cls in tokens)
                        {
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
                    new[] { ' ', '\t', '\n', '\r' },
                    StringSplitOptions.RemoveEmptyEntries
                );
                foreach (var cls in tokens)
                {
                    element.RemoveFromClassList(cls);
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
                    Debug.Log(
                        "[ApplyEvent] unregister (newHandler null) eventPropName="
                            + eventPropName
                            + ", element="
                            + element.name
                            + ", parent="
                            + (element.parent != null ? element.parent.name : "<null>")
                    );
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
                    ReactiveUITK.Core.Reconciler.TraceLevel
                    == ReactiveUITK.Core.Reconciler.DiffTraceLevel.Verbose
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
            Debug.Log(
                "[RemoveEvent] begin eventPropName="
                    + eventPropName
                    + ", element="
                    + element.name
                    + ", parent="
                    + (element.parent != null ? element.parent.name : "<null>")
                    + ", handlerType="
                    + handler.GetType().Name
            );

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
#endif
            if (eventPropName == "onScroll" && handler is EventCallback<WheelEvent> se)
            {
                element.UnregisterCallback(se);
                meta.EventHandlers.Remove(eventPropName);
                totalEventsRemoved++;
                return;
            }

            if (meta.EventHandlers.TryGetValue(eventPropName, out var stored))
            {
                try
                {
                    Debug.Log(
                        "[RemoveEvent] fallback stored wrapper eventPropName="
                            + eventPropName
                            + ", element="
                            + element.name
                            + ", parent="
                            + (element.parent != null ? element.parent.name : "<null>")
                            + ", storedType="
                            + stored?.GetType().Name
                    );

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
                var ve = evt?.target as VisualElement;
                if (
                    Core.Reconciler.EnableDiffTracing
                    && Core.Reconciler.TraceLevel == Core.Reconciler.DiffTraceLevel.Verbose
                )
                {
                    Debug.Log(
                        "[InvokeHandler] eventType="
                            + (evt != null ? evt.GetType().Name : "<null>")
                            + ", target="
                            + (ve != null ? ve.name : "<null>")
                    );
                }
            }
            catch { }
            SyntheticEvent syntheticEvent = SyntheticEvent.Create(evt);
            if (syntheticEvent != null)
            {
                syntheticEvent.CurrentTarget = evt?.currentTarget as VisualElement;
            }
            try
            {
                if (del is Action action)
                {
                    action();
                    return;
                }

                var method = del.Method;
                var parameters = method.GetParameters();

                if (parameters.Length == 0)
                {
                    del.DynamicInvoke();
                    return;
                }

                object newValue = null;
                var evtObj = evt as object;
                var evtType = evtObj?.GetType();
                if (
                    evtType != null
                    && evtType.IsGenericType
                    && evtType.Name.StartsWith("ChangeEvent`", StringComparison.Ordinal)
                )
                {
                    try
                    {
                        newValue = evtType.GetProperty("newValue")?.GetValue(evtObj);
                    }
                    catch { }
                }

                if (parameters.Length == 1)
                {
                    var p0 = parameters[0].ParameterType;

                    if (evt != null && p0.IsAssignableFrom(evt.GetType()))
                    {
                        del.DynamicInvoke(evt);
                        return;
                    }

                    if (syntheticEvent != null && p0.IsInstanceOfType(syntheticEvent))
                    {
                        del.DynamicInvoke(syntheticEvent);
                        return;
                    }

                    if (newValue != null)
                    {
                        if (p0.IsInstanceOfType(newValue))
                        {
                            del.DynamicInvoke(newValue);
                            return;
                        }
                        try
                        {
                            var converted = System.Convert.ChangeType(newValue, p0);
                            del.DynamicInvoke(converted);
                            return;
                        }
                        catch { }
                    }

                    if (p0 == typeof(object))
                    {
                        del.DynamicInvoke(newValue ?? (object)evt);
                        return;
                    }

                    return;
                }

                {
                    object[] args = new object[parameters.Length];
                    var p0 = parameters[0].ParameterType;
                    if (evt != null && p0.IsAssignableFrom(evt.GetType()))
                    {
                        args[0] = evt;
                    }
                    else if (syntheticEvent != null && p0.IsInstanceOfType(syntheticEvent))
                    {
                        args[0] = syntheticEvent;
                    }
                    else if (newValue != null && p0.IsInstanceOfType(newValue))
                    {
                        args[0] = newValue;
                    }
                    else
                    {
                        try
                        {
                            if (newValue != null)
                            {
                                args[0] = System.Convert.ChangeType(newValue, p0);
                            }
                        }
                        catch
                        {
                            args[0] = null;
                        }
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
