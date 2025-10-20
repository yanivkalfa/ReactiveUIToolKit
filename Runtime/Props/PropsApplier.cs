using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props
{
    public static class PropsApplier
    {
        private static readonly Dictionary<VisualElement, Dictionary<string, object>> previousStyles = new();
        private static int totalStyleSets;
        private static int totalStyleResets;
        private static int totalEventsRegistered;
        private static int totalEventsRemoved;

        // Algorithmic canonicalization helpers (USS dashed -> camelCase) so user can supply pure USS names.
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
            return parts[0].ToLowerInvariant() + string.Join(string.Empty, parts, 1, parts.Length - 1);
        }

        public static void Apply(VisualElement element, IReadOnlyDictionary<string, object> properties)
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

        public static void ApplyDiff(VisualElement element, IReadOnlyDictionary<string, object> previous, IReadOnlyDictionary<string, object> next)
        {
            previous ??= new Dictionary<string, object>();
            next ??= new Dictionary<string, object>();

            // Removed non-style props
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

            // Added / changed non-style props
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

            // Nested style diff if present
            previous.TryGetValue("style", out var prevStyleObj);
            next.TryGetValue("style", out var nextStyleObj);
            if (prevStyleObj != null || nextStyleObj != null)
            {
                var prevMap = prevStyleObj as IDictionary<string, object> ?? (IDictionary<string, object>)new Dictionary<string, object>();
                var nextMap = nextStyleObj as IDictionary<string, object> ?? (IDictionary<string, object>)new Dictionary<string, object>();
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

        private static void ApplySingle(VisualElement element, object oldValue, string propertyName, object propertyValue)
        {
            if (propertyName == "name")
            {
                element.name = propertyValue as string;
                return;
            }
            if (propertyName == "className" || propertyName == "class")
            {
                string newClasses = propertyValue as string;
                string oldClasses = oldValue as string;
                if (oldClasses != null)
                {
                    var oldSet = new HashSet<string>((oldClasses ?? string.Empty).Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries));
                    var newSet = new HashSet<string>((newClasses ?? string.Empty).Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries));
                    foreach (var cls in oldSet)
                    {
                        if (!newSet.Contains(cls)) element.RemoveFromClassList(cls);
                    }
                    foreach (var cls in newSet)
                    {
                        if (!oldSet.Contains(cls)) element.AddToClassList(cls);
                    }
                }
                else
                {
                    element.ClearClassList();
                    if (!string.IsNullOrEmpty(newClasses))
                    {
                        var tokens = newClasses.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
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
            if (propertyName.StartsWith("on") && oldValue is Delegate oldHandler)
            {
                var meta = element.userData as Core.NodeMetadata;
                if (meta != null) RemoveEvent(element, propertyName, oldHandler, meta);
                return;
            }
            if ((propertyName == "className" || propertyName == "class") && oldValue is string oldClasses)
            {
                var tokens = oldClasses.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var cls in tokens)
                {
                    element.RemoveFromClassList(cls);
                }
                return;
            }
            if (propertyName == "style" && oldValue is IDictionary<string, object> oldMap)
            {
                foreach (var kv in oldMap) ResetStyle(element, kv.Key);
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
            if (styleKey == "width")
            {
                element.style.width = ConvertToLength(value);
                totalStyleSets++;
                return;
            }
            if (styleKey == "height")
            {
                element.style.height = ConvertToLength(value);
                totalStyleSets++;
                return;
            }
            if (styleKey == "opacity")
            {
                element.style.opacity = ConvertToFloat(value);
                return;
            }
            if (styleKey == "flexGrow")
            {
                element.style.flexGrow = ConvertToFloat(value);
                return;
            }
            if (styleKey == "flexShrink")
            {
                element.style.flexShrink = ConvertToFloat(value);
                return;
            }
            if (styleKey == "flexDirection")
            {
                element.style.flexDirection = ConvertToFlexDirection(value);
                return;
            }
            if (styleKey == "justifyContent")
            {
                element.style.justifyContent = ConvertToJustify(value);
                return;
            }
            if (styleKey == "alignItems")
            {
                element.style.alignItems = ConvertToAlign(value);
                return;
            }
            if (styleKey == "alignSelf") { if (value is string s) { element.style.alignSelf = ConvertToAlign(value); } return; }
            if (styleKey == "alignContent") { if (value is string s2) { element.style.alignContent = ConvertToAlign(value); } return; }
            if (styleKey == "fontSize") { element.style.fontSize = ConvertToFloat(value); return; }
            if (styleKey == "unityFontStyle") { if (value is string fs && fs == "bold") element.style.unityFontStyleAndWeight = FontStyle.Bold; else element.style.unityFontStyleAndWeight = FontStyle.Normal; return; }
            if (styleKey == "textAlign") { if (value is string ta && ta == "center") element.style.unityTextAlign = TextAnchor.MiddleCenter; else element.style.unityTextAlign = TextAnchor.UpperLeft; return; }
            if (styleKey == "position") { if (value is string p && p == "absolute") element.style.position = Position.Absolute; else element.style.position = Position.Relative; return; }
            if (styleKey == "left") { element.style.left = ConvertToLength(value); return; }
            if (styleKey == "top") { element.style.top = ConvertToLength(value); return; }
            if (styleKey == "right") { element.style.right = ConvertToLength(value); return; }
            if (styleKey == "bottom") { element.style.bottom = ConvertToLength(value); return; }
            if (styleKey == "flexWrap") { if (value is string fw && fw == "wrap") element.style.flexWrap = Wrap.Wrap; else element.style.flexWrap = Wrap.NoWrap; return; }
            if (styleKey == "flexBasis") { element.style.flexBasis = ConvertToLength(value); return; }
            if (styleKey == "minWidth") { element.style.minWidth = ConvertToLength(value); return; }
            if (styleKey == "minHeight") { element.style.minHeight = ConvertToLength(value); return; }
            if (styleKey == "maxWidth") { element.style.maxWidth = ConvertToLength(value); return; }
            if (styleKey == "maxHeight") { element.style.maxHeight = ConvertToLength(value); return; }
            if (styleKey == "display") { if (value is string dv && dv == "none") element.style.display = DisplayStyle.None; else element.style.display = DisplayStyle.Flex; return; }
            if (styleKey == "visibility") { if (value is string vis && vis == "hidden") element.style.visibility = Visibility.Hidden; else element.style.visibility = Visibility.Visible; return; }
            if (styleKey == "overflow") { if (value is string ov && ov == "hidden") element.style.overflow = Overflow.Hidden; else element.style.overflow = Overflow.Visible; return; }
            if (styleKey == "whiteSpace") { if (value is string ws && ws == "nowrap") element.style.whiteSpace = WhiteSpace.NoWrap; else element.style.whiteSpace = WhiteSpace.Normal; return; }
            if (styleKey == "backgroundImage") { if (value is Texture2D tex) element.style.backgroundImage = new StyleBackground(tex); else if (value is Sprite sp) element.style.backgroundImage = new StyleBackground(sp); return; }
            if (styleKey == "backgroundImageTint" ) { element.style.unityBackgroundImageTintColor = ConvertToColor(value); return; }
            if (styleKey == "borderWidth") { var bw = ConvertToFloat(value); element.style.borderLeftWidth = bw; element.style.borderRightWidth = bw; element.style.borderTopWidth = bw; element.style.borderBottomWidth = bw; return; }
            if (styleKey == "borderColor") { var col = ConvertToColor(value); element.style.borderLeftColor = col; element.style.borderRightColor = col; element.style.borderTopColor = col; element.style.borderBottomColor = col; return; }
            if (styleKey == "borderLeftWidth") { element.style.borderLeftWidth = ConvertToFloat(value); return; }
            if (styleKey == "borderRightWidth") { element.style.borderRightWidth = ConvertToFloat(value); return; }
            if (styleKey == "borderTopWidth") { element.style.borderTopWidth = ConvertToFloat(value); return; }
            if (styleKey == "borderBottomWidth") { element.style.borderBottomWidth = ConvertToFloat(value); return; }
            if (styleKey == "borderLeftColor") { element.style.borderLeftColor = ConvertToColor(value); return; }
            if (styleKey == "borderRightColor") { element.style.borderRightColor = ConvertToColor(value); return; }
            if (styleKey == "borderTopColor") { element.style.borderTopColor = ConvertToColor(value); return; }
            if (styleKey == "borderBottomColor") { element.style.borderBottomColor = ConvertToColor(value); return; }
            if (styleKey == "color")
            {
                element.style.color = ConvertToColor(value);
                return;
            }
            if (styleKey == "backgroundColor")
            {
                element.style.backgroundColor = ConvertToColor(value);
                return;
            }
            // Modern transform style fields (no obsolete VisualElement.transform usage)
            if (styleKey == "rotate")
            {
                // Accept number as degrees
                if (value != null) {
                    float deg = ConvertToFloat(value);
                    try { element.style.rotate = new Rotate(deg); } catch { }
                }
                return;
            }
            if (styleKey == "fontFamily") { if (value is Font font) { element.style.unityFont = font; } return; }
            if (styleKey == "borderRadius")
            {
                element.style.borderTopLeftRadius = ConvertToLength(value);
                element.style.borderTopRightRadius = ConvertToLength(value);
                element.style.borderBottomLeftRadius = ConvertToLength(value);
                element.style.borderBottomRightRadius = ConvertToLength(value);
                return;
            }
            if (styleKey == "borderTopLeftRadius") { element.style.borderTopLeftRadius = ConvertToLength(value); return; }
            if (styleKey == "borderTopRightRadius") { element.style.borderTopRightRadius = ConvertToLength(value); return; }
            if (styleKey == "borderBottomLeftRadius") { element.style.borderBottomLeftRadius = ConvertToLength(value); return; }
            if (styleKey == "borderBottomRightRadius") { element.style.borderBottomRightRadius = ConvertToLength(value); return; }
            if (styleKey == "margin")
            {
                var len = ConvertToLength(value);
                element.style.marginLeft = len;
                element.style.marginRight = len;
                element.style.marginTop = len;
                element.style.marginBottom = len;
                return;
            }
            if (styleKey == "padding")
            {
                var len = ConvertToLength(value);
                element.style.paddingLeft = len;
                element.style.paddingRight = len;
                element.style.paddingTop = len;
                element.style.paddingBottom = len;
                return;
            }
            if (styleKey == "marginLeft") { element.style.marginLeft = ConvertToLength(value); return; }
            if (styleKey == "marginRight") { element.style.marginRight = ConvertToLength(value); return; }
            if (styleKey == "marginTop") { element.style.marginTop = ConvertToLength(value); return; }
            if (styleKey == "marginBottom") { element.style.marginBottom = ConvertToLength(value); return; }
            if (styleKey == "paddingLeft") { element.style.paddingLeft = ConvertToLength(value); return; }
            if (styleKey == "paddingRight") { element.style.paddingRight = ConvertToLength(value); return; }
            if (styleKey == "paddingTop") { element.style.paddingTop = ConvertToLength(value); return; }
            if (styleKey == "paddingBottom") { element.style.paddingBottom = ConvertToLength(value); return; }
            if (styleKey == "scale")
            {
                try {
                    if (value is float uni) element.style.scale = new Scale(new Vector3(uni, uni, 1));
                    else if (value is IEnumerable<object> seq)
                    {
                        float sx=1, sy=1; int i=0; foreach(var v in seq){ if(i==0) sx=ConvertToFloat(v); else if(i==1) sy=ConvertToFloat(v); i++; if(i>1) break; }
                        element.style.scale = new Scale(new Vector3(sx, sy, 1));
                    }
                } catch { }
                return;
            }
            if (styleKey == "translate")
            {
                try {
                    if (value is IEnumerable<object> seqT)
                    { float tx=0, ty=0; int i=0; foreach(var v in seqT){ if(i==0) tx=ConvertToFloat(v); else if(i==1) ty=ConvertToFloat(v); i++; if(i>1) break; }
                      element.style.translate = new Translate(new Length(tx, LengthUnit.Pixel), new Length(ty, LengthUnit.Pixel), 0);
                    }
                    else { float tx = ConvertToFloat(value); element.style.translate = new Translate(new Length(tx, LengthUnit.Pixel), new Length(0, LengthUnit.Pixel), 0); }
                } catch { }
                return;
            }
            if (styleKey == "flex")
            {
                // Accept "grow shrink basis" tokens
                if (value is string flexStr)
                {
                    var parts = flexStr.Split(new[]{' ','\t'}, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 0) { element.style.flexGrow = ConvertToFloat(parts[0]); }
                    if (parts.Length > 1) { element.style.flexShrink = ConvertToFloat(parts[1]); }
                    if (parts.Length > 2) { element.style.flexBasis = ConvertToLength(parts[2]); }
                }
                return;
            }
            if (styleKey == "cursor") { if (value is string cur) { try { var uiCursor = new UnityEngine.UIElements.Cursor(); element.style.cursor = new StyleCursor(uiCursor); } catch { } } return; }
            if (styleKey == "backgroundRepeat") { /* not directly supported pre-2022; ignore safely */ return; }
            if (styleKey == "backgroundPosition") { /* not directly supported; potential future */ return; }
            if (styleKey == "backgroundSize") { /* handled via unityBackgroundScaleMode; ignore */ return; }
            if (styleKey == "filter") { /* UI Toolkit filter chain not exposed; ignore */ return; }
            if (styleKey == "transformOrigin") { /* Not exposed; ignore */ return; }
            if (styleKey == "transition" || styleKey == "transitionDelay" || styleKey == "transitionDuration" || styleKey == "transitionProperty" || styleKey == "transitionTimingFunction") { /* animations not applied via runtime API here */ return; }
            if (styleKey == "wordSpacing") { try { element.style.wordSpacing = ConvertToFloat(value); } catch { } return; }
            if (styleKey == "unityOverflowClipBox") { if (value is string ocb) { try { element.style.unityOverflowClipBox = ocb == "padding-box" ? OverflowClipBox.PaddingBox : OverflowClipBox.ContentBox; } catch { } } return; }
            if (styleKey == "unityParagraphSpacing") { try { element.style.unityParagraphSpacing = ConvertToFloat(value); } catch { } return; }
            // unityBackgroundScaleMode deprecated in Unity 6.2+; ignored.
            if (styleKey == "unitySliceBottom") { try { element.style.unitySliceBottom = (int)ConvertToFloat(value); } catch { } return; }
            if (styleKey == "unitySliceTop") { try { element.style.unitySliceTop = (int)ConvertToFloat(value); } catch { } return; }
            if (styleKey == "unitySliceLeft") { try { element.style.unitySliceLeft = (int)ConvertToFloat(value); } catch { } return; }
            if (styleKey == "unitySliceRight") { try { element.style.unitySliceRight = (int)ConvertToFloat(value); } catch { } return; }
            if (styleKey == "unitySliceScale") { try { element.style.unitySliceScale = ConvertToFloat(value); } catch { } return; }
            if (styleKey == "unitySliceType") { if (value is string st) { try { element.style.unitySliceType = st == "tiled" ? SliceType.Tiled : SliceType.Sliced; } catch { } } return; }
            // Extended (optional) text-related styles
            if (styleKey == "letterSpacing") { try { element.style.letterSpacing = ConvertToFloat(value); } catch { } return; }
            if (styleKey == "textOverflow") { if (value is string ovf) { try { element.style.textOverflow = ovf == "ellipsis" ? TextOverflow.Ellipsis : TextOverflow.Clip; } catch { } } return; }
            if (styleKey == "unityTextOutlineColor") { try { element.style.unityTextOutlineColor = ConvertToColor(value); } catch { } return; }
            if (styleKey == "unityTextOutlineWidth") { try { element.style.unityTextOutlineWidth = ConvertToFloat(value); } catch { } return; }
            if (styleKey == "unityTextOverflowPosition") { if (value is string pos) { try { element.style.unityTextOverflowPosition = pos switch { "middle" => TextOverflowPosition.Middle, "end" => TextOverflowPosition.End, _ => TextOverflowPosition.Start }; } catch { } } return; }
            if (styleKey == "unityTextAutoSize") { /* unsupported in current version */ return; }
            // aspectRatio not supported in current target version
        }

        private static void ResetStyle(VisualElement element, string styleKey)
        {
            styleKey = Canonicalize(styleKey);
            if (previousStyles.TryGetValue(element, out var prevMap))
            {
                prevMap.Remove(styleKey);
            }
            if (styleKey == "width") { element.style.width = StyleKeyword.Null; totalStyleResets++; return; }
            if (styleKey == "height") { element.style.height = StyleKeyword.Null; totalStyleResets++; return; }
            if (styleKey == "opacity") { element.style.opacity = StyleKeyword.Null; totalStyleResets++; return; }
            if (styleKey == "flexGrow") { element.style.flexGrow = StyleKeyword.Null; return; }
            if (styleKey == "flexShrink") { element.style.flexShrink = StyleKeyword.Null; return; }
            if (styleKey == "flexDirection") { element.style.flexDirection = StyleKeyword.Null; return; }
            if (styleKey == "justifyContent") { element.style.justifyContent = StyleKeyword.Null; return; }
            if (styleKey == "alignItems") { element.style.alignItems = StyleKeyword.Null; return; }
            if (styleKey == "alignSelf") { element.style.alignSelf = StyleKeyword.Null; return; }
            if (styleKey == "alignContent") { element.style.alignContent = StyleKeyword.Null; return; }
            if (styleKey == "fontSize") { element.style.fontSize = StyleKeyword.Null; return; }
            if (styleKey == "unityFontStyle") { element.style.unityFontStyleAndWeight = StyleKeyword.Null; return; }
            if (styleKey == "textAlign") { element.style.unityTextAlign = StyleKeyword.Null; return; }
            if (styleKey == "position") { element.style.position = StyleKeyword.Null; return; }
            if (styleKey == "left") { element.style.left = StyleKeyword.Null; return; }
            if (styleKey == "top") { element.style.top = StyleKeyword.Null; return; }
            if (styleKey == "right") { element.style.right = StyleKeyword.Null; return; }
            if (styleKey == "bottom") { element.style.bottom = StyleKeyword.Null; return; }
            if (styleKey == "flexWrap") { element.style.flexWrap = StyleKeyword.Null; return; }
            if (styleKey == "flexBasis") { element.style.flexBasis = StyleKeyword.Null; return; }
            if (styleKey == "minWidth") { element.style.minWidth = StyleKeyword.Null; return; }
            if (styleKey == "minHeight") { element.style.minHeight = StyleKeyword.Null; return; }
            if (styleKey == "maxWidth") { element.style.maxWidth = StyleKeyword.Null; return; }
            if (styleKey == "maxHeight") { element.style.maxHeight = StyleKeyword.Null; return; }
            if (styleKey == "display") { element.style.display = StyleKeyword.Null; return; }
            if (styleKey == "visibility") { element.style.visibility = StyleKeyword.Null; return; }
            if (styleKey == "overflow") { element.style.overflow = StyleKeyword.Null; return; }
            if (styleKey == "whiteSpace") { element.style.whiteSpace = StyleKeyword.Null; return; }
            if (styleKey == "backgroundImage") { element.style.backgroundImage = StyleKeyword.Null; return; }
            if (styleKey == "backgroundImageTint") { element.style.unityBackgroundImageTintColor = StyleKeyword.Null; return; }
            // gap not supported in current Unity version
            if (styleKey == "borderWidth") { element.style.borderLeftWidth = StyleKeyword.Null; element.style.borderRightWidth = StyleKeyword.Null; element.style.borderTopWidth = StyleKeyword.Null; element.style.borderBottomWidth = StyleKeyword.Null; return; }
            if (styleKey == "borderColor") { element.style.borderLeftColor = StyleKeyword.Null; element.style.borderRightColor = StyleKeyword.Null; element.style.borderTopColor = StyleKeyword.Null; element.style.borderBottomColor = StyleKeyword.Null; return; }
            if (styleKey == "color") { element.style.color = StyleKeyword.Null; return; }
            if (styleKey == "backgroundColor") { element.style.backgroundColor = StyleKeyword.Null; return; }
            if (styleKey == "borderRadius") {
                element.style.borderTopLeftRadius = StyleKeyword.Null;
                element.style.borderTopRightRadius = StyleKeyword.Null;
                element.style.borderBottomLeftRadius = StyleKeyword.Null;
                element.style.borderBottomRightRadius = StyleKeyword.Null;
                return;
            }
            if (styleKey == "borderTopLeftRadius") { element.style.borderTopLeftRadius = StyleKeyword.Null; return; }
            if (styleKey == "borderTopRightRadius") { element.style.borderTopRightRadius = StyleKeyword.Null; return; }
            if (styleKey == "borderBottomLeftRadius") { element.style.borderBottomLeftRadius = StyleKeyword.Null; return; }
            if (styleKey == "borderBottomRightRadius") { element.style.borderBottomRightRadius = StyleKeyword.Null; return; }
            if (styleKey == "margin") {
                element.style.marginLeft = StyleKeyword.Null;
                element.style.marginRight = StyleKeyword.Null;
                element.style.marginTop = StyleKeyword.Null;
                element.style.marginBottom = StyleKeyword.Null;
                return;
            }
            if (styleKey == "padding") {
                element.style.paddingLeft = StyleKeyword.Null;
                element.style.paddingRight = StyleKeyword.Null;
                element.style.paddingTop = StyleKeyword.Null;
                element.style.paddingBottom = StyleKeyword.Null;
                return;
            }
            if (styleKey == "marginLeft") { element.style.marginLeft = StyleKeyword.Null; return; }
            if (styleKey == "marginRight") { element.style.marginRight = StyleKeyword.Null; return; }
            if (styleKey == "marginTop") { element.style.marginTop = StyleKeyword.Null; return; }
            if (styleKey == "marginBottom") { element.style.marginBottom = StyleKeyword.Null; return; }
            if (styleKey == "paddingLeft") { element.style.paddingLeft = StyleKeyword.Null; return; }
            if (styleKey == "paddingRight") { element.style.paddingRight = StyleKeyword.Null; return; }
            if (styleKey == "paddingTop") { element.style.paddingTop = StyleKeyword.Null; return; }
            if (styleKey == "paddingBottom") { element.style.paddingBottom = StyleKeyword.Null; return; }
            // Extended resets
            if (styleKey == "letterSpacing") { try { element.style.letterSpacing = StyleKeyword.Null; } catch { } return; }
            if (styleKey == "textOverflow") { try { element.style.textOverflow = StyleKeyword.Null; } catch { } return; }
            if (styleKey == "unityTextOutlineColor") { try { element.style.unityTextOutlineColor = StyleKeyword.Null; } catch { } return; }
            if (styleKey == "unityTextOutlineWidth") { try { element.style.unityTextOutlineWidth = StyleKeyword.Null; } catch { } return; }
            if (styleKey == "unityTextOverflowPosition") { try { element.style.unityTextOverflowPosition = StyleKeyword.Null; } catch { } return; }
            if (styleKey == "unityTextAutoSize") { /* unsupported; nothing to reset */ return; }
            // aspectRatio reset skipped (not supported)
        }

        private static Color ConvertToColor(object value)
        {
            if (value is Color c)
            {
                return c;
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

        // Note: no alias map needed; Canonicalize handles USS dashed conversion.

        private static void ApplyEvent(VisualElement element, string eventPropName, Delegate newHandler, Delegate oldHandler)
        {
            Core.NodeMetadata meta = element.userData as Core.NodeMetadata;
            if (meta == null)
            {
                meta = new Core.NodeMetadata();
                element.userData = meta;
            }

            if (oldHandler != null && !Equals(oldHandler, newHandler))
            {
                RemoveEvent(element, eventPropName, oldHandler, meta);
            }

            if (newHandler == null)
            {
                if (meta.EventHandlers.ContainsKey(eventPropName))
                {
                    RemoveEvent(element, eventPropName, meta.EventHandlers[eventPropName], meta);
                }
                return;
            }

            // Map events and pass event object to handler via dynamic invoke
            if (eventPropName == "onClick")
            {
                EventCallback<ClickEvent> wrapper = e => InvokeHandler(newHandler, e);
                element.RegisterCallback(wrapper);
                meta.EventHandlers[eventPropName] = wrapper;
                totalEventsRegistered++;
                return;
            }
            if (eventPropName == "onPointerDown") { EventCallback<PointerDownEvent> w = e => InvokeHandler(newHandler, e); element.RegisterCallback(w); meta.EventHandlers[eventPropName] = w; return; }
            if (eventPropName == "onPointerUp") { EventCallback<PointerUpEvent> w = e => InvokeHandler(newHandler, e); element.RegisterCallback(w); meta.EventHandlers[eventPropName] = w; return; }
            if (eventPropName == "onPointerMove") { EventCallback<PointerMoveEvent> w = e => InvokeHandler(newHandler, e); element.RegisterCallback(w); meta.EventHandlers[eventPropName] = w; return; }
            if (eventPropName == "onPointerEnter") { EventCallback<PointerEnterEvent> w = e => InvokeHandler(newHandler, e); element.RegisterCallback(w); meta.EventHandlers[eventPropName] = w; return; }
            if (eventPropName == "onPointerLeave") { EventCallback<PointerLeaveEvent> w = e => InvokeHandler(newHandler, e); element.RegisterCallback(w); meta.EventHandlers[eventPropName] = w; return; }
            if (eventPropName == "onWheel") { EventCallback<WheelEvent> w = e => InvokeHandler(newHandler, e); element.RegisterCallback(w); meta.EventHandlers[eventPropName] = w; return; }
            if (eventPropName == "onFocus") { EventCallback<FocusEvent> w = e => InvokeHandler(newHandler, e); element.RegisterCallback(w); meta.EventHandlers[eventPropName] = w; return; }
            if (eventPropName == "onBlur") { EventCallback<BlurEvent> w = e => InvokeHandler(newHandler, e); element.RegisterCallback(w); meta.EventHandlers[eventPropName] = w; return; }
            if (eventPropName == "onKeyDown") { EventCallback<KeyDownEvent> w = e => InvokeHandler(newHandler, e); element.RegisterCallback(w); meta.EventHandlers[eventPropName] = w; return; }
            if (eventPropName == "onKeyUp") { EventCallback<KeyUpEvent> w = e => InvokeHandler(newHandler, e); element.RegisterCallback(w); meta.EventHandlers[eventPropName] = w; return; }
            if (eventPropName == "onChange") { EventCallback<ChangeEvent<string>> w = e => InvokeHandler(newHandler, e); element.RegisterCallback(w); meta.EventHandlers[eventPropName] = w; return; }
            if (eventPropName == "onInput") { EventCallback<InputEvent> w = e => InvokeHandler(newHandler, e); element.RegisterCallback(w); meta.EventHandlers[eventPropName] = w; return; }
            if (eventPropName == "onDragEnter") { EventCallback<DragEnterEvent> w = e => InvokeHandler(newHandler, e); element.RegisterCallback(w); meta.EventHandlers[eventPropName] = w; return; }
            if (eventPropName == "onDragLeave") { EventCallback<DragLeaveEvent> w = e => InvokeHandler(newHandler, e); element.RegisterCallback(w); meta.EventHandlers[eventPropName] = w; return; }
            if (eventPropName == "onScroll") { EventCallback<WheelEvent> w = e => InvokeHandler(newHandler, e); element.RegisterCallback(w); meta.EventHandlers[eventPropName] = w; return; }
        }

        private static void RemoveEvent(VisualElement element, string eventPropName, Delegate handler, Core.NodeMetadata meta)
        {
            if (handler == null)
            {
                return;
            }
            // Attempt to unregister based on event type
            if (eventPropName == "onClick" && handler is EventCallback<ClickEvent> clickCb) { element.UnregisterCallback(clickCb); meta.EventHandlers.Remove(eventPropName); totalEventsRemoved++; return; }
            if (eventPropName == "onPointerDown" && handler is EventCallback<PointerDownEvent> pd) { element.UnregisterCallback(pd); meta.EventHandlers.Remove(eventPropName); totalEventsRemoved++; return; }
            if (eventPropName == "onPointerUp" && handler is EventCallback<PointerUpEvent> pu) { element.UnregisterCallback(pu); meta.EventHandlers.Remove(eventPropName); totalEventsRemoved++; return; }
            if (eventPropName == "onPointerMove" && handler is EventCallback<PointerMoveEvent> pm) { element.UnregisterCallback(pm); meta.EventHandlers.Remove(eventPropName); totalEventsRemoved++; return; }
            if (eventPropName == "onPointerEnter" && handler is EventCallback<PointerEnterEvent> pe) { element.UnregisterCallback(pe); meta.EventHandlers.Remove(eventPropName); totalEventsRemoved++; return; }
            if (eventPropName == "onPointerLeave" && handler is EventCallback<PointerLeaveEvent> pl) { element.UnregisterCallback(pl); meta.EventHandlers.Remove(eventPropName); totalEventsRemoved++; return; }
            if (eventPropName == "onWheel" && handler is EventCallback<WheelEvent> we) { element.UnregisterCallback(we); meta.EventHandlers.Remove(eventPropName); totalEventsRemoved++; return; }
            if (eventPropName == "onFocus" && handler is EventCallback<FocusEvent> fe) { element.UnregisterCallback(fe); meta.EventHandlers.Remove(eventPropName); totalEventsRemoved++; return; }
            if (eventPropName == "onBlur" && handler is EventCallback<BlurEvent> be) { element.UnregisterCallback(be); meta.EventHandlers.Remove(eventPropName); totalEventsRemoved++; return; }
            if (eventPropName == "onKeyDown" && handler is EventCallback<KeyDownEvent> kd) { element.UnregisterCallback(kd); meta.EventHandlers.Remove(eventPropName); totalEventsRemoved++; return; }
            if (eventPropName == "onKeyUp" && handler is EventCallback<KeyUpEvent> ku) { element.UnregisterCallback(ku); meta.EventHandlers.Remove(eventPropName); totalEventsRemoved++; return; }
            if (eventPropName == "onChange" && handler is EventCallback<ChangeEvent<string>> ch) { element.UnregisterCallback(ch); meta.EventHandlers.Remove(eventPropName); totalEventsRemoved++; return; }
            if (eventPropName == "onInput" && handler is EventCallback<InputEvent> ie) { element.UnregisterCallback(ie); meta.EventHandlers.Remove(eventPropName); totalEventsRemoved++; return; }
            if (eventPropName == "onDragEnter" && handler is EventCallback<DragEnterEvent> de) { element.UnregisterCallback(de); meta.EventHandlers.Remove(eventPropName); totalEventsRemoved++; return; }
            if (eventPropName == "onDragLeave" && handler is EventCallback<DragLeaveEvent> dle) { element.UnregisterCallback(dle); meta.EventHandlers.Remove(eventPropName); totalEventsRemoved++; return; }
            if (eventPropName == "onScroll" && handler is EventCallback<WheelEvent> se) { element.UnregisterCallback(se); meta.EventHandlers.Remove(eventPropName); totalEventsRemoved++; return; }
            meta.EventHandlers.Remove(eventPropName);
            totalEventsRemoved++;
        }

        private static void InvokeHandler(Delegate del, EventBase evt)
        {
            if (del == null)
            {
                return;
            }
            try
            {
                if (del is Action action)
                {
                    action();
                    return;
                }
                var parameters = del.Method.GetParameters();
                if (parameters.Length == 0)
                {
                    del.DynamicInvoke();
                }
                else if (parameters.Length == 1)
                {
                    del.DynamicInvoke(evt);
                }
                else
                {
                    // Attempt best effort: pass event as first arg, nulls for rest
                    object[] args = new object[parameters.Length];
                    args[0] = evt;
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

        private static Length ConvertToLength(object value)
        {
            if (value is float f)
            {
                return new Length(f, LengthUnit.Pixel);
            }

            if (value is int i)
            {
                return new Length(i, LengthUnit.Pixel);
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

        // Called externally when element is removed to prune cache
        public static void NotifyElementRemoved(VisualElement element)
        {
            if (element == null)
            {
                return;
            }
            previousStyles.Remove(element);
        }

        public static (int styleSets, int styleResets, int eventsAdded, int eventsRemoved) GetStyleMetrics() => (totalStyleSets, totalStyleResets, totalEventsRegistered, totalEventsRemoved);

        private static void TrySetStyleField(VisualElement element, string fieldName, object value)
        {
            // Removed due to limited runtime reflection safety; kept signature for compatibility.
        }
    }
}
