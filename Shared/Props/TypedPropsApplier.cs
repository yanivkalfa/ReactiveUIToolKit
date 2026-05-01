using System;
using System.Collections.Generic;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props
{
    /// <summary>
    /// Applies typed <see cref="BaseProps"/> directly to a <see cref="VisualElement"/>
    /// without going through <c>Dictionary&lt;string, object&gt;</c>.
    /// Handles all properties defined on <see cref="BaseProps"/> (name, className,
    /// style, events, ref, visibility, etc.). Element-specific properties
    /// (e.g. <c>Button.text</c>) are NOT handled here — each adapter handles those.
    /// </summary>
    public static class TypedPropsApplier
    {
        /// <summary>
        /// Apply all properties from <paramref name="props"/> to <paramref name="element"/>.
        /// Used on initial placement (no previous state).
        /// </summary>
        public static void ApplyFull(VisualElement element, BaseProps props)
        {
            if (props == null)
                return;

            // --- Identity / structure ---
            if (props.Name != null)
                PropsApplier.ApplySingle(element, null, "name", props.Name);
            if (props.ClassName != null)
                PropsApplier.ApplySingle(element, null, "className", props.ClassName);
            if (props.Style != null)
                DiffStyle(element, null, props.Style);
            if (props.Ref != null)
                PropsApplier.ApplySingle(element, null, "ref", props.Ref);
            if (props.ContentContainer != null)
                PropsApplier.ApplySingle(element, null, "contentContainer", props.ContentContainer);

            // --- Visibility / enabled ---
            if (props.Visible.HasValue)
                PropsApplier.ApplySingle(element, null, "visible", props.Visible.Value);
            if (props.Enabled.HasValue)
                PropsApplier.ApplySingle(element, null, "enabled", props.Enabled.Value);

            // --- Tooltip / persistence ---
            if (props.Tooltip != null)
                PropsApplier.ApplySingle(element, null, "tooltip", props.Tooltip);
            if (props.ViewDataKey != null)
                PropsApplier.ApplySingle(element, null, "viewDataKey", props.ViewDataKey);

            // --- Focus / interaction ---
            if (props.PickingMode.HasValue)
                PropsApplier.ApplySingle(element, null, "pickingMode", props.PickingMode.Value);
            if (props.Focusable.HasValue)
                PropsApplier.ApplySingle(element, null, "focusable", props.Focusable.Value);
            if (props.TabIndex.HasValue)
                PropsApplier.ApplySingle(element, null, "tabIndex", props.TabIndex.Value);
            if (props.DelegatesFocus.HasValue)
                PropsApplier.ApplySingle(
                    element,
                    null,
                    "delegatesFocus",
                    props.DelegatesFocus.Value
                );

            // --- Locale ---
            if (props.LanguageDirection.HasValue)
                PropsApplier.ApplySingle(
                    element,
                    null,
                    "languageDirection",
                    props.LanguageDirection.Value
                );

            // --- Pointer events ---
            ApplyEventIfSet(element, "onClick", props.OnClick);
            ApplyEventIfSet(element, "onClickCapture", props.OnClickCapture);
            ApplyEventIfSet(element, "onPointerDown", props.OnPointerDown);
            ApplyEventIfSet(element, "onPointerDownCapture", props.OnPointerDownCapture);
            ApplyEventIfSet(element, "onPointerUp", props.OnPointerUp);
            ApplyEventIfSet(element, "onPointerUpCapture", props.OnPointerUpCapture);
            ApplyEventIfSet(element, "onPointerMove", props.OnPointerMove);
            ApplyEventIfSet(element, "onPointerMoveCapture", props.OnPointerMoveCapture);
            ApplyEventIfSet(element, "onPointerEnter", props.OnPointerEnter);
            ApplyEventIfSet(element, "onPointerEnterCapture", props.OnPointerEnterCapture);
            ApplyEventIfSet(element, "onPointerLeave", props.OnPointerLeave);
            ApplyEventIfSet(element, "onPointerLeaveCapture", props.OnPointerLeaveCapture);
            ApplyEventIfSet(element, "onWheel", props.OnWheel);
            ApplyEventIfSet(element, "onWheelCapture", props.OnWheelCapture);
            ApplyEventIfSet(element, "onScroll", props.OnScroll);
            ApplyEventIfSet(element, "onScrollCapture", props.OnScrollCapture);

#if UNITY_EDITOR
            // --- Drag events ---
            ApplyEventIfSet(element, "onDragEnter", props.OnDragEnter);
            ApplyEventIfSet(element, "onDragEnterCapture", props.OnDragEnterCapture);
            ApplyEventIfSet(element, "onDragLeave", props.OnDragLeave);
            ApplyEventIfSet(element, "onDragLeaveCapture", props.OnDragLeaveCapture);
            ApplyEventIfSet(element, "onDragUpdated", props.OnDragUpdated);
            ApplyEventIfSet(element, "onDragUpdatedCapture", props.OnDragUpdatedCapture);
            ApplyEventIfSet(element, "onDragPerform", props.OnDragPerform);
            ApplyEventIfSet(element, "onDragPerformCapture", props.OnDragPerformCapture);
            ApplyEventIfSet(element, "onDragExited", props.OnDragExited);
            ApplyEventIfSet(element, "onDragExitedCapture", props.OnDragExitedCapture);
#endif

            // --- Focus events ---
            ApplyEventIfSet(element, "onFocus", props.OnFocus);
            ApplyEventIfSet(element, "onFocusCapture", props.OnFocusCapture);
            ApplyEventIfSet(element, "onBlur", props.OnBlur);
            ApplyEventIfSet(element, "onBlurCapture", props.OnBlurCapture);
            ApplyEventIfSet(element, "onFocusIn", props.OnFocusIn);
            ApplyEventIfSet(element, "onFocusInCapture", props.OnFocusInCapture);
            ApplyEventIfSet(element, "onFocusOut", props.OnFocusOut);
            ApplyEventIfSet(element, "onFocusOutCapture", props.OnFocusOutCapture);

            // --- Keyboard events ---
            ApplyEventIfSet(element, "onKeyDown", props.OnKeyDown);
            ApplyEventIfSet(element, "onKeyDownCapture", props.OnKeyDownCapture);
            ApplyEventIfSet(element, "onKeyUp", props.OnKeyUp);
            ApplyEventIfSet(element, "onKeyUpCapture", props.OnKeyUpCapture);

            // --- Input event ---
            ApplyEventIfSet(element, "onInput", props.OnInput);
            ApplyEventIfSet(element, "onInputCapture", props.OnInputCapture);

            // --- Lifecycle events ---
            ApplyEventIfSet(element, "onGeometryChanged", props.OnGeometryChanged);
            ApplyEventIfSet(element, "onAttachToPanel", props.OnAttachToPanel);
            ApplyEventIfSet(element, "onDetachFromPanel", props.OnDetachFromPanel);

            // --- ExtraProps (escape hatch) ---
            if (props.ExtraProps != null)
            {
                foreach (var kv in props.ExtraProps)
                    PropsApplier.ApplySingle(element, null, kv.Key, kv.Value);
            }
        }

        /// <summary>
        /// Apply only the changed base properties from <paramref name="prev"/> to
        /// <paramref name="next"/> on the given <paramref name="element"/>.
        /// </summary>
        public static void ApplyDiff(VisualElement element, BaseProps prev, BaseProps next)
        {
            prev ??= s_empty;
            next ??= s_empty;

            // --- Identity / structure ---
            if (prev.Name != next.Name)
                DiffField(element, "name", prev.Name, next.Name);
            if (prev.ClassName != next.ClassName)
                DiffField(element, "className", prev.ClassName, next.ClassName);
            if (!Style.SameInstance(prev.Style, next.Style))
                DiffStyle(element, prev.Style, next.Style);
            if (!ReferenceEquals(prev.Ref, next.Ref))
                DiffField(element, "ref", prev.Ref, next.Ref);
            if (!ReferenceEquals(prev.ContentContainer, next.ContentContainer))
                DiffField(
                    element,
                    "contentContainer",
                    prev.ContentContainer,
                    next.ContentContainer
                );

            // --- Visibility / enabled ---
            if (prev.Visible != next.Visible)
                DiffNullableField(element, "visible", prev.Visible, next.Visible);
            if (prev.Enabled != next.Enabled)
                DiffNullableField(element, "enabled", prev.Enabled, next.Enabled);

            // --- Tooltip / persistence ---
            if (prev.Tooltip != next.Tooltip)
                DiffField(element, "tooltip", prev.Tooltip, next.Tooltip);
            if (prev.ViewDataKey != next.ViewDataKey)
                DiffField(element, "viewDataKey", prev.ViewDataKey, next.ViewDataKey);

            // --- Focus / interaction ---
            if (prev.PickingMode != next.PickingMode)
                DiffNullableField(element, "pickingMode", prev.PickingMode, next.PickingMode);
            if (prev.Focusable != next.Focusable)
                DiffNullableField(element, "focusable", prev.Focusable, next.Focusable);
            if (prev.TabIndex != next.TabIndex)
                DiffNullableField(element, "tabIndex", prev.TabIndex, next.TabIndex);
            if (prev.DelegatesFocus != next.DelegatesFocus)
                DiffNullableField(
                    element,
                    "delegatesFocus",
                    prev.DelegatesFocus,
                    next.DelegatesFocus
                );

            // --- Locale ---
            if (prev.LanguageDirection != next.LanguageDirection)
                DiffNullableField(
                    element,
                    "languageDirection",
                    prev.LanguageDirection,
                    next.LanguageDirection
                );

            // --- Events: skip the entire block when neither side has any event handler ---
            if (prev._hasEvents || next._hasEvents)
            {
                // --- Pointer events ---
                DiffEvent(element, "onClick", prev.OnClick, next.OnClick);
                DiffEvent(element, "onClickCapture", prev.OnClickCapture, next.OnClickCapture);
                DiffEvent(element, "onPointerDown", prev.OnPointerDown, next.OnPointerDown);
                DiffEvent(
                    element,
                    "onPointerDownCapture",
                    prev.OnPointerDownCapture,
                    next.OnPointerDownCapture
                );
                DiffEvent(element, "onPointerUp", prev.OnPointerUp, next.OnPointerUp);
                DiffEvent(
                    element,
                    "onPointerUpCapture",
                    prev.OnPointerUpCapture,
                    next.OnPointerUpCapture
                );
                DiffEvent(element, "onPointerMove", prev.OnPointerMove, next.OnPointerMove);
                DiffEvent(
                    element,
                    "onPointerMoveCapture",
                    prev.OnPointerMoveCapture,
                    next.OnPointerMoveCapture
                );
                DiffEvent(element, "onPointerEnter", prev.OnPointerEnter, next.OnPointerEnter);
                DiffEvent(
                    element,
                    "onPointerEnterCapture",
                    prev.OnPointerEnterCapture,
                    next.OnPointerEnterCapture
                );
                DiffEvent(element, "onPointerLeave", prev.OnPointerLeave, next.OnPointerLeave);
                DiffEvent(
                    element,
                    "onPointerLeaveCapture",
                    prev.OnPointerLeaveCapture,
                    next.OnPointerLeaveCapture
                );
                DiffEvent(element, "onWheel", prev.OnWheel, next.OnWheel);
                DiffEvent(element, "onWheelCapture", prev.OnWheelCapture, next.OnWheelCapture);
                DiffEvent(element, "onScroll", prev.OnScroll, next.OnScroll);
                DiffEvent(element, "onScrollCapture", prev.OnScrollCapture, next.OnScrollCapture);

#if UNITY_EDITOR
                // --- Drag events ---
                DiffEvent(element, "onDragEnter", prev.OnDragEnter, next.OnDragEnter);
                DiffEvent(
                    element,
                    "onDragEnterCapture",
                    prev.OnDragEnterCapture,
                    next.OnDragEnterCapture
                );
                DiffEvent(element, "onDragLeave", prev.OnDragLeave, next.OnDragLeave);
                DiffEvent(
                    element,
                    "onDragLeaveCapture",
                    prev.OnDragLeaveCapture,
                    next.OnDragLeaveCapture
                );
                DiffEvent(element, "onDragUpdated", prev.OnDragUpdated, next.OnDragUpdated);
                DiffEvent(
                    element,
                    "onDragUpdatedCapture",
                    prev.OnDragUpdatedCapture,
                    next.OnDragUpdatedCapture
                );
                DiffEvent(element, "onDragPerform", prev.OnDragPerform, next.OnDragPerform);
                DiffEvent(
                    element,
                    "onDragPerformCapture",
                    prev.OnDragPerformCapture,
                    next.OnDragPerformCapture
                );
                DiffEvent(element, "onDragExited", prev.OnDragExited, next.OnDragExited);
                DiffEvent(
                    element,
                    "onDragExitedCapture",
                    prev.OnDragExitedCapture,
                    next.OnDragExitedCapture
                );
#endif

                // --- Focus events ---
                DiffEvent(element, "onFocus", prev.OnFocus, next.OnFocus);
                DiffEvent(element, "onFocusCapture", prev.OnFocusCapture, next.OnFocusCapture);
                DiffEvent(element, "onBlur", prev.OnBlur, next.OnBlur);
                DiffEvent(element, "onBlurCapture", prev.OnBlurCapture, next.OnBlurCapture);
                DiffEvent(element, "onFocusIn", prev.OnFocusIn, next.OnFocusIn);
                DiffEvent(
                    element,
                    "onFocusInCapture",
                    prev.OnFocusInCapture,
                    next.OnFocusInCapture
                );
                DiffEvent(element, "onFocusOut", prev.OnFocusOut, next.OnFocusOut);
                DiffEvent(
                    element,
                    "onFocusOutCapture",
                    prev.OnFocusOutCapture,
                    next.OnFocusOutCapture
                );

                // --- Keyboard events ---
                DiffEvent(element, "onKeyDown", prev.OnKeyDown, next.OnKeyDown);
                DiffEvent(
                    element,
                    "onKeyDownCapture",
                    prev.OnKeyDownCapture,
                    next.OnKeyDownCapture
                );
                DiffEvent(element, "onKeyUp", prev.OnKeyUp, next.OnKeyUp);
                DiffEvent(element, "onKeyUpCapture", prev.OnKeyUpCapture, next.OnKeyUpCapture);

                // --- Input event ---
                DiffEvent(element, "onInput", prev.OnInput, next.OnInput);
                DiffEvent(element, "onInputCapture", prev.OnInputCapture, next.OnInputCapture);

                // --- Lifecycle events ---
                DiffEvent(
                    element,
                    "onGeometryChanged",
                    prev.OnGeometryChanged,
                    next.OnGeometryChanged
                );
                DiffEvent(element, "onAttachToPanel", prev.OnAttachToPanel, next.OnAttachToPanel);
                DiffEvent(
                    element,
                    "onDetachFromPanel",
                    prev.OnDetachFromPanel,
                    next.OnDetachFromPanel
                );
            }

            // --- ExtraProps (escape hatch) ---
            if (!ReferenceEquals(prev.ExtraProps, next.ExtraProps))
                DiffExtraProps(element, prev.ExtraProps, next.ExtraProps);
        }

        /// <summary>
        /// Diff two typed <see cref="Style"/> objects using bitmask comparison.
        /// Compares only fields that are set in either prev or next — no dictionary
        /// iteration, no boxing, no string key lookups.
        /// </summary>
        public static void DiffStyle(VisualElement element, Style prev, Style next)
        {
            if (Style.SameInstance(prev, next))
                return;

            ulong prevBits0 = prev?._setBits0 ?? 0UL;
            ulong prevBits1 = prev?._setBits1 ?? 0UL;
            ulong nextBits0 = next?._setBits0 ?? 0UL;
            ulong nextBits1 = next?._setBits1 ?? 0UL;

            // Removed: set in prev but not in next → reset
            ulong removed0 = prevBits0 & ~nextBits0;
            ulong removed1 = prevBits1 & ~nextBits1;
            ResetBits(element, removed0, 0);
            ResetBits(element, removed1, 64);

            // Added or potentially changed: set in next → apply if value differs
            ApplyChangedBits(element, prev, next, nextBits0, prevBits0, 0);
            ApplyChangedBits(element, prev, next, nextBits1, prevBits1, 64);
        }

        private static void ResetBits(VisualElement el, ulong bits, int offset)
        {
            while (bits != 0)
            {
                int localBit = Style.BitOps.TrailingZeroCount(bits);
                int bit = offset + localBit;
                // Skip shorthand-only tracking bits
                if (
                    bit != Style.BIT_MARGIN
                    && bit != Style.BIT_PADDING
                    && bit != Style.BIT_BORDER_RADIUS
                    && bit != Style.BIT_BORDER_WIDTH
                    && bit != Style.BIT_BORDER_COLOR
                )
                {
                    ResetTypedStyleField(el, bit);
                }
                bits &= bits - 1;
            }
        }

        private static void ApplyChangedBits(
            VisualElement el,
            Style prev,
            Style next,
            ulong bits,
            ulong prevBits,
            int offset
        )
        {
            while (bits != 0)
            {
                int localBit = Style.BitOps.TrailingZeroCount(bits);
                int bit = offset + localBit;
                // Skip shorthand-only tracking bits
                if (
                    bit != Style.BIT_MARGIN
                    && bit != Style.BIT_PADDING
                    && bit != Style.BIT_BORDER_RADIUS
                    && bit != Style.BIT_BORDER_WIDTH
                    && bit != Style.BIT_BORDER_COLOR
                )
                {
                    bool wasSet = (prevBits & (1UL << localBit)) != 0;
                    if (!wasSet || prev == null || !next.FieldEquals(bit, prev))
                    {
                        ApplyTypedStyleField(el, next, bit);
                    }
                }
                bits &= bits - 1;
            }
        }

        internal static void ApplyTypedStyleField(VisualElement el, Style style, int bit)
        {
            switch (bit)
            {
                // StyleLength
                case Style.BIT_WIDTH:
                    el.style.width = style._width;
                    break;
                case Style.BIT_HEIGHT:
                    el.style.height = style._height;
                    break;
                case Style.BIT_MIN_WIDTH:
                    el.style.minWidth = style._minWidth;
                    break;
                case Style.BIT_MIN_HEIGHT:
                    el.style.minHeight = style._minHeight;
                    break;
                case Style.BIT_MAX_WIDTH:
                    el.style.maxWidth = style._maxWidth;
                    break;
                case Style.BIT_MAX_HEIGHT:
                    el.style.maxHeight = style._maxHeight;
                    break;
                case Style.BIT_FLEX_BASIS:
                    el.style.flexBasis = style._flexBasis;
                    break;
                case Style.BIT_LEFT:
                    el.style.left = style._left;
                    break;
                case Style.BIT_TOP:
                    el.style.top = style._top;
                    break;
                case Style.BIT_RIGHT:
                    el.style.right = style._right;
                    break;
                case Style.BIT_BOTTOM:
                    el.style.bottom = style._bottom;
                    break;
                case Style.BIT_MARGIN_LEFT:
                    el.style.marginLeft = style._marginLeft;
                    break;
                case Style.BIT_MARGIN_RIGHT:
                    el.style.marginRight = style._marginRight;
                    break;
                case Style.BIT_MARGIN_TOP:
                    el.style.marginTop = style._marginTop;
                    break;
                case Style.BIT_MARGIN_BOTTOM:
                    el.style.marginBottom = style._marginBottom;
                    break;
                case Style.BIT_PADDING_LEFT:
                    el.style.paddingLeft = style._paddingLeft;
                    break;
                case Style.BIT_PADDING_RIGHT:
                    el.style.paddingRight = style._paddingRight;
                    break;
                case Style.BIT_PADDING_TOP:
                    el.style.paddingTop = style._paddingTop;
                    break;
                case Style.BIT_PADDING_BOTTOM:
                    el.style.paddingBottom = style._paddingBottom;
                    break;
                case Style.BIT_BORDER_TOP_LEFT_RADIUS:
                    el.style.borderTopLeftRadius = style._borderTopLeftRadius;
                    break;
                case Style.BIT_BORDER_TOP_RIGHT_RADIUS:
                    el.style.borderTopRightRadius = style._borderTopRightRadius;
                    break;
                case Style.BIT_BORDER_BOTTOM_LEFT_RADIUS:
                    el.style.borderBottomLeftRadius = style._borderBottomLeftRadius;
                    break;
                case Style.BIT_BORDER_BOTTOM_RIGHT_RADIUS:
                    el.style.borderBottomRightRadius = style._borderBottomRightRadius;
                    break;
                case Style.BIT_FONT_SIZE:
                    el.style.fontSize = style._fontSize;
                    break;
                case Style.BIT_LETTER_SPACING:
                    try
                    {
                        el.style.letterSpacing = style._letterSpacing;
                    }
                    catch { }
                    break;
                // StyleFloat
                case Style.BIT_FLEX_GROW:
                    el.style.flexGrow = style._flexGrow;
                    break;
                case Style.BIT_FLEX_SHRINK:
                    el.style.flexShrink = style._flexShrink;
                    break;
                case Style.BIT_OPACITY:
                    el.style.opacity = style._opacity;
                    break;
                case Style.BIT_BORDER_LEFT_WIDTH:
                    el.style.borderLeftWidth = style._borderLeftWidth;
                    break;
                case Style.BIT_BORDER_RIGHT_WIDTH:
                    el.style.borderRightWidth = style._borderRightWidth;
                    break;
                case Style.BIT_BORDER_TOP_WIDTH:
                    el.style.borderTopWidth = style._borderTopWidth;
                    break;
                case Style.BIT_BORDER_BOTTOM_WIDTH:
                    el.style.borderBottomWidth = style._borderBottomWidth;
                    break;
                case Style.BIT_UNITY_TEXT_OUTLINE_WIDTH:
                    try
                    {
                        el.style.unityTextOutlineWidth = style._unityTextOutlineWidth;
                    }
                    catch { }
                    break;
                // Color
                case Style.BIT_COLOR:
                    el.style.color = style._color;
                    break;
                case Style.BIT_BACKGROUND_COLOR:
                    el.style.backgroundColor = style._backgroundColor;
                    break;
                case Style.BIT_BACKGROUND_IMAGE_TINT:
                    el.style.unityBackgroundImageTintColor = style._backgroundImageTint;
                    break;
                case Style.BIT_BORDER_LEFT_COLOR:
                    el.style.borderLeftColor = style._borderLeftColor;
                    break;
                case Style.BIT_BORDER_RIGHT_COLOR:
                    el.style.borderRightColor = style._borderRightColor;
                    break;
                case Style.BIT_BORDER_TOP_COLOR:
                    el.style.borderTopColor = style._borderTopColor;
                    break;
                case Style.BIT_BORDER_BOTTOM_COLOR:
                    el.style.borderBottomColor = style._borderBottomColor;
                    break;
                case Style.BIT_UNITY_TEXT_OUTLINE_COLOR:
                    try
                    {
                        el.style.unityTextOutlineColor = style._unityTextOutlineColor;
                    }
                    catch { }
                    break;
                // Enums
                case Style.BIT_FLEX_DIRECTION:
                    el.style.flexDirection = style._flexDirection;
                    break;
                case Style.BIT_FLEX_WRAP:
                    el.style.flexWrap = style._flexWrap;
                    break;
                case Style.BIT_JUSTIFY_CONTENT:
                    el.style.justifyContent = style._justifyContent;
                    break;
                case Style.BIT_ALIGN_ITEMS:
                    el.style.alignItems = style._alignItems;
                    break;
                case Style.BIT_ALIGN_SELF:
                    el.style.alignSelf = style._alignSelf;
                    break;
                case Style.BIT_ALIGN_CONTENT:
                    el.style.alignContent = style._alignContent;
                    break;
                case Style.BIT_POSITION:
                    el.style.position = style._position;
                    break;
                case Style.BIT_DISPLAY:
                    el.style.display = style._display;
                    break;
                case Style.BIT_VISIBILITY:
                    el.style.visibility = style._visibility;
                    break;
                case Style.BIT_OVERFLOW:
                    el.style.overflow = style._overflow;
                    break;
                case Style.BIT_WHITE_SPACE:
                    el.style.whiteSpace = style._whiteSpace;
                    break;
                case Style.BIT_TEXT_ALIGN:
                    el.style.unityTextAlign = style._textAlign;
                    break;
                case Style.BIT_TEXT_OVERFLOW:
                    try
                    {
                        el.style.textOverflow = style._textOverflow;
                    }
                    catch { }
                    break;
                case Style.BIT_UNITY_FONT_STYLE:
                    el.style.unityFontStyleAndWeight = style._unityFontStyle;
                    break;
                case Style.BIT_UNITY_TEXT_OVERFLOW_POS:
                    try
                    {
                        el.style.unityTextOverflowPosition = style._unityTextOverflowPosition;
                    }
                    catch { }
                    break;
                case Style.BIT_UNITY_TEXT_AUTO_SIZE:
                    try
                    {
                        el.style.unityTextAutoSize = new StyleTextAutoSize(
                            new TextAutoSize(style._unityTextAutoSize, default, default)
                        );
                    }
                    catch { }
                    break;
                // Assets
                case Style.BIT_BACKGROUND_IMAGE:
                    el.style.backgroundImage =
                        style._backgroundImage != null
                            ? new StyleBackground(style._backgroundImage)
                            : StyleKeyword.Null;
                    break;
                case Style.BIT_FONT_FAMILY:
                    if (style._fontFamily != null)
                        el.style.unityFont = style._fontFamily;
                    break;
                // float
                case Style.BIT_ROTATE:
                    try
                    {
                        el.style.rotate = new Rotate(style._rotate);
                    }
                    catch { }
                    break;
                case Style.BIT_SCALE:
                    try
                    {
                        el.style.scale = style._scale;
                    }
                    catch { }
                    break;
                // Compound structs
                case Style.BIT_TRANSLATE:
                    try
                    {
                        el.style.translate = style._translate;
                    }
                    catch { }
                    break;
                case Style.BIT_BACKGROUND_REPEAT:
                    try
                    {
                        el.style.backgroundRepeat = style._backgroundRepeat;
                    }
                    catch { }
                    break;
                case Style.BIT_BACKGROUND_POSITION_X:
                    try
                    {
                        el.style.backgroundPositionX = style._backgroundPositionX;
                    }
                    catch { }
                    break;
                case Style.BIT_BACKGROUND_POSITION_Y:
                    try
                    {
                        el.style.backgroundPositionY = style._backgroundPositionY;
                    }
                    catch { }
                    break;
                case Style.BIT_BACKGROUND_SIZE:
                    try
                    {
                        el.style.backgroundSize = style._backgroundSize;
                    }
                    catch { }
                    break;
                case Style.BIT_TRANSFORM_ORIGIN:
                    try
                    {
                        el.style.transformOrigin = style._transformOrigin;
                    }
                    catch { }
                    break;
                // Transitions
                case Style.BIT_TRANSITION_DELAY:
                    el.style.transitionDelay = style._transitionDelay;
                    break;
                case Style.BIT_TRANSITION_DURATION:
                    el.style.transitionDuration = style._transitionDuration;
                    break;
                case Style.BIT_TRANSITION_PROPERTY:
                    el.style.transitionProperty = style._transitionProperty;
                    break;
                case Style.BIT_TRANSITION_TIMING_FUNC:
                    el.style.transitionTimingFunction = style._transitionTimingFunction;
                    break;
#if UNITY_6000_3_OR_NEWER
                case Style.BIT_ASPECT_RATIO:
                    el.style.aspectRatio = style._aspectRatio;
                    break;
                case Style.BIT_FILTER:
                    el.style.filter = style._filter;
                    break;
                case Style.BIT_UNITY_MATERIAL:
                    el.style.unityMaterial = style._unityMaterial;
                    break;
#endif
                // 9-slice background
                case Style.BIT_UNITY_SLICE_LEFT:
                    try { el.style.unitySliceLeft = style._unitySliceLeft; }
                    catch { }
                    break;
                case Style.BIT_UNITY_SLICE_RIGHT:
                    try { el.style.unitySliceRight = style._unitySliceRight; }
                    catch { }
                    break;
                case Style.BIT_UNITY_SLICE_TOP:
                    try { el.style.unitySliceTop = style._unitySliceTop; }
                    catch { }
                    break;
                case Style.BIT_UNITY_SLICE_BOTTOM:
                    try { el.style.unitySliceBottom = style._unitySliceBottom; }
                    catch { }
                    break;
                case Style.BIT_UNITY_SLICE_SCALE:
                    try { el.style.unitySliceScale = style._unitySliceScale; }
                    catch { }
                    break;
                case Style.BIT_UNITY_SLICE_TYPE:
                    try { el.style.unitySliceType = style._unitySliceType; }
                    catch { }
                    break;
                // Clipping
                case Style.BIT_UNITY_OVERFLOW_CLIP_BOX:
                    try { el.style.unityOverflowClipBox = style._unityOverflowClipBox; }
                    catch { }
                    break;
                // Text spacing / shadow / advanced font
                case Style.BIT_UNITY_PARAGRAPH_SPACING:
                    try { el.style.unityParagraphSpacing = style._unityParagraphSpacing; }
                    catch { }
                    break;
                case Style.BIT_WORD_SPACING:
                    try { el.style.wordSpacing = style._wordSpacing; }
                    catch { }
                    break;
                case Style.BIT_TEXT_SHADOW:
                    try { el.style.textShadow = style._textShadow; }
                    catch { }
                    break;
                case Style.BIT_UNITY_FONT_DEFINITION:
                    try { el.style.unityFontDefinition = style._unityFontDefinition; }
                    catch { }
                    break;
                case Style.BIT_UNITY_TEXT_GENERATOR:
                    try { el.style.unityTextGenerator = style._unityTextGenerator; }
                    catch { }
                    break;
                case Style.BIT_UNITY_EDITOR_TEXT_RENDERING_MODE:
                    try { el.style.unityEditorTextRenderingMode = style._unityEditorTextRenderingMode; }
                    catch { }
                    break;
            }
        }

        internal static void ResetTypedStyleField(VisualElement el, int bit)
        {
            switch (bit)
            {
                case Style.BIT_WIDTH:
                    el.style.width = StyleKeyword.Null;
                    break;
                case Style.BIT_HEIGHT:
                    el.style.height = StyleKeyword.Null;
                    break;
                case Style.BIT_MIN_WIDTH:
                    el.style.minWidth = StyleKeyword.Null;
                    break;
                case Style.BIT_MIN_HEIGHT:
                    el.style.minHeight = StyleKeyword.Null;
                    break;
                case Style.BIT_MAX_WIDTH:
                    el.style.maxWidth = StyleKeyword.Null;
                    break;
                case Style.BIT_MAX_HEIGHT:
                    el.style.maxHeight = StyleKeyword.Null;
                    break;
                case Style.BIT_FLEX_BASIS:
                    el.style.flexBasis = StyleKeyword.Null;
                    break;
                case Style.BIT_LEFT:
                    el.style.left = StyleKeyword.Null;
                    break;
                case Style.BIT_TOP:
                    el.style.top = StyleKeyword.Null;
                    break;
                case Style.BIT_RIGHT:
                    el.style.right = StyleKeyword.Null;
                    break;
                case Style.BIT_BOTTOM:
                    el.style.bottom = StyleKeyword.Null;
                    break;
                case Style.BIT_MARGIN_LEFT:
                    el.style.marginLeft = StyleKeyword.Null;
                    break;
                case Style.BIT_MARGIN_RIGHT:
                    el.style.marginRight = StyleKeyword.Null;
                    break;
                case Style.BIT_MARGIN_TOP:
                    el.style.marginTop = StyleKeyword.Null;
                    break;
                case Style.BIT_MARGIN_BOTTOM:
                    el.style.marginBottom = StyleKeyword.Null;
                    break;
                case Style.BIT_PADDING_LEFT:
                    el.style.paddingLeft = StyleKeyword.Null;
                    break;
                case Style.BIT_PADDING_RIGHT:
                    el.style.paddingRight = StyleKeyword.Null;
                    break;
                case Style.BIT_PADDING_TOP:
                    el.style.paddingTop = StyleKeyword.Null;
                    break;
                case Style.BIT_PADDING_BOTTOM:
                    el.style.paddingBottom = StyleKeyword.Null;
                    break;
                case Style.BIT_BORDER_TOP_LEFT_RADIUS:
                    el.style.borderTopLeftRadius = StyleKeyword.Null;
                    break;
                case Style.BIT_BORDER_TOP_RIGHT_RADIUS:
                    el.style.borderTopRightRadius = StyleKeyword.Null;
                    break;
                case Style.BIT_BORDER_BOTTOM_LEFT_RADIUS:
                    el.style.borderBottomLeftRadius = StyleKeyword.Null;
                    break;
                case Style.BIT_BORDER_BOTTOM_RIGHT_RADIUS:
                    el.style.borderBottomRightRadius = StyleKeyword.Null;
                    break;
                case Style.BIT_FONT_SIZE:
                    el.style.fontSize = StyleKeyword.Null;
                    break;
                case Style.BIT_LETTER_SPACING:
                    try
                    {
                        el.style.letterSpacing = StyleKeyword.Null;
                    }
                    catch { }
                    break;
                case Style.BIT_FLEX_GROW:
                    el.style.flexGrow = StyleKeyword.Null;
                    break;
                case Style.BIT_FLEX_SHRINK:
                    el.style.flexShrink = StyleKeyword.Null;
                    break;
                case Style.BIT_OPACITY:
                    el.style.opacity = StyleKeyword.Null;
                    break;
                case Style.BIT_BORDER_LEFT_WIDTH:
                    el.style.borderLeftWidth = StyleKeyword.Null;
                    break;
                case Style.BIT_BORDER_RIGHT_WIDTH:
                    el.style.borderRightWidth = StyleKeyword.Null;
                    break;
                case Style.BIT_BORDER_TOP_WIDTH:
                    el.style.borderTopWidth = StyleKeyword.Null;
                    break;
                case Style.BIT_BORDER_BOTTOM_WIDTH:
                    el.style.borderBottomWidth = StyleKeyword.Null;
                    break;
                case Style.BIT_UNITY_TEXT_OUTLINE_WIDTH:
                    try
                    {
                        el.style.unityTextOutlineWidth = StyleKeyword.Null;
                    }
                    catch { }
                    break;
                case Style.BIT_COLOR:
                    el.style.color = StyleKeyword.Null;
                    break;
                case Style.BIT_BACKGROUND_COLOR:
                    el.style.backgroundColor = StyleKeyword.Null;
                    break;
                case Style.BIT_BACKGROUND_IMAGE:
                    el.style.backgroundImage = StyleKeyword.Null;
                    break;
                case Style.BIT_BACKGROUND_IMAGE_TINT:
                    el.style.unityBackgroundImageTintColor = StyleKeyword.Null;
                    break;
                case Style.BIT_BORDER_LEFT_COLOR:
                    el.style.borderLeftColor = StyleKeyword.Null;
                    break;
                case Style.BIT_BORDER_RIGHT_COLOR:
                    el.style.borderRightColor = StyleKeyword.Null;
                    break;
                case Style.BIT_BORDER_TOP_COLOR:
                    el.style.borderTopColor = StyleKeyword.Null;
                    break;
                case Style.BIT_BORDER_BOTTOM_COLOR:
                    el.style.borderBottomColor = StyleKeyword.Null;
                    break;
                case Style.BIT_UNITY_TEXT_OUTLINE_COLOR:
                    try
                    {
                        el.style.unityTextOutlineColor = StyleKeyword.Null;
                    }
                    catch { }
                    break;
                case Style.BIT_FLEX_DIRECTION:
                    el.style.flexDirection = StyleKeyword.Null;
                    break;
                case Style.BIT_FLEX_WRAP:
                    el.style.flexWrap = StyleKeyword.Null;
                    break;
                case Style.BIT_JUSTIFY_CONTENT:
                    el.style.justifyContent = StyleKeyword.Null;
                    break;
                case Style.BIT_ALIGN_ITEMS:
                    el.style.alignItems = StyleKeyword.Null;
                    break;
                case Style.BIT_ALIGN_SELF:
                    el.style.alignSelf = StyleKeyword.Null;
                    break;
                case Style.BIT_ALIGN_CONTENT:
                    el.style.alignContent = StyleKeyword.Null;
                    break;
                case Style.BIT_POSITION:
                    el.style.position = StyleKeyword.Null;
                    break;
                case Style.BIT_DISPLAY:
                    el.style.display = StyleKeyword.Null;
                    break;
                case Style.BIT_VISIBILITY:
                    el.style.visibility = StyleKeyword.Null;
                    break;
                case Style.BIT_OVERFLOW:
                    el.style.overflow = StyleKeyword.Null;
                    break;
                case Style.BIT_WHITE_SPACE:
                    el.style.whiteSpace = StyleKeyword.Null;
                    break;
                case Style.BIT_TEXT_ALIGN:
                    el.style.unityTextAlign = StyleKeyword.Null;
                    break;
                case Style.BIT_TEXT_OVERFLOW:
                    try
                    {
                        el.style.textOverflow = StyleKeyword.Null;
                    }
                    catch { }
                    break;
                case Style.BIT_UNITY_FONT_STYLE:
                    el.style.unityFontStyleAndWeight = StyleKeyword.Null;
                    break;
                case Style.BIT_UNITY_TEXT_OVERFLOW_POS:
                    try
                    {
                        el.style.unityTextOverflowPosition = StyleKeyword.Null;
                    }
                    catch { }
                    break;
                case Style.BIT_UNITY_TEXT_AUTO_SIZE:
                    try
                    {
                        el.style.unityTextAutoSize = StyleKeyword.Null;
                    }
                    catch { }
                    break;
                case Style.BIT_FONT_FAMILY:
                    el.style.unityFont = null;
                    break;
                case Style.BIT_ROTATE:
                    try
                    {
                        el.style.rotate = StyleKeyword.Null;
                    }
                    catch { }
                    break;
                case Style.BIT_SCALE:
                    try
                    {
                        el.style.scale = StyleKeyword.Null;
                    }
                    catch { }
                    break;
                case Style.BIT_TRANSLATE:
                    try
                    {
                        el.style.translate = StyleKeyword.Null;
                    }
                    catch { }
                    break;
                case Style.BIT_BACKGROUND_REPEAT:
                    try
                    {
                        el.style.backgroundRepeat = StyleKeyword.Null;
                    }
                    catch { }
                    break;
                case Style.BIT_BACKGROUND_POSITION_X:
                    try
                    {
                        el.style.backgroundPositionX = StyleKeyword.Null;
                    }
                    catch { }
                    break;
                case Style.BIT_BACKGROUND_POSITION_Y:
                    try
                    {
                        el.style.backgroundPositionY = StyleKeyword.Null;
                    }
                    catch { }
                    break;
                case Style.BIT_BACKGROUND_SIZE:
                    try
                    {
                        el.style.backgroundSize = StyleKeyword.Null;
                    }
                    catch { }
                    break;
                case Style.BIT_TRANSFORM_ORIGIN:
                    try
                    {
                        el.style.transformOrigin = StyleKeyword.Null;
                    }
                    catch { }
                    break;
                case Style.BIT_TRANSITION_DELAY:
                    el.style.transitionDelay = StyleKeyword.Null;
                    break;
                case Style.BIT_TRANSITION_DURATION:
                    el.style.transitionDuration = StyleKeyword.Null;
                    break;
                case Style.BIT_TRANSITION_PROPERTY:
                    el.style.transitionProperty = StyleKeyword.Null;
                    break;
                case Style.BIT_TRANSITION_TIMING_FUNC:
                    el.style.transitionTimingFunction = StyleKeyword.Null;
                    break;
#if UNITY_6000_3_OR_NEWER
                case Style.BIT_ASPECT_RATIO:
                    el.style.aspectRatio = StyleKeyword.Null;
                    break;
                case Style.BIT_FILTER:
                    el.style.filter = StyleKeyword.Null;
                    break;
                case Style.BIT_UNITY_MATERIAL:
                    el.style.unityMaterial = StyleKeyword.Null;
                    break;
#endif
                // 9-slice background
                case Style.BIT_UNITY_SLICE_LEFT:
                    try { el.style.unitySliceLeft = StyleKeyword.Null; }
                    catch { }
                    break;
                case Style.BIT_UNITY_SLICE_RIGHT:
                    try { el.style.unitySliceRight = StyleKeyword.Null; }
                    catch { }
                    break;
                case Style.BIT_UNITY_SLICE_TOP:
                    try { el.style.unitySliceTop = StyleKeyword.Null; }
                    catch { }
                    break;
                case Style.BIT_UNITY_SLICE_BOTTOM:
                    try { el.style.unitySliceBottom = StyleKeyword.Null; }
                    catch { }
                    break;
                case Style.BIT_UNITY_SLICE_SCALE:
                    try { el.style.unitySliceScale = StyleKeyword.Null; }
                    catch { }
                    break;
                case Style.BIT_UNITY_SLICE_TYPE:
                    try { el.style.unitySliceType = StyleKeyword.Null; }
                    catch { }
                    break;
                // Clipping
                case Style.BIT_UNITY_OVERFLOW_CLIP_BOX:
                    try { el.style.unityOverflowClipBox = StyleKeyword.Null; }
                    catch { }
                    break;
                // Text spacing / shadow / advanced font
                case Style.BIT_UNITY_PARAGRAPH_SPACING:
                    try { el.style.unityParagraphSpacing = StyleKeyword.Null; }
                    catch { }
                    break;
                case Style.BIT_WORD_SPACING:
                    try { el.style.wordSpacing = StyleKeyword.Null; }
                    catch { }
                    break;
                case Style.BIT_TEXT_SHADOW:
                    try { el.style.textShadow = StyleKeyword.Null; }
                    catch { }
                    break;
                case Style.BIT_UNITY_FONT_DEFINITION:
                    try { el.style.unityFontDefinition = StyleKeyword.Null; }
                    catch { }
                    break;
                case Style.BIT_UNITY_TEXT_GENERATOR:
                    try { el.style.unityTextGenerator = StyleKeyword.Null; }
                    catch { }
                    break;
                case Style.BIT_UNITY_EDITOR_TEXT_RENDERING_MODE:
                    try { el.style.unityEditorTextRenderingMode = StyleKeyword.Null; }
                    catch { }
                    break;
            }
        }

        /// <summary>
        /// Diff a slot sub-element's props dictionary. Slots (Label, Input, VisualInput etc.)
        /// remain dict-based. This compares two slot dicts and applies changes to the
        /// resolved sub-element.
        /// </summary>
        public static void DiffSlot(
            VisualElement parent,
            Dictionary<string, object> prevSlot,
            Dictionary<string, object> nextSlot,
            string slotName
        )
        {
            if (ReferenceEquals(prevSlot, nextSlot))
                return;

            var slotElement = ResolveSlotElement(parent, slotName);
            if (slotElement == null)
                return;

            var prevDict = (IReadOnlyDictionary<string, object>)prevSlot;
            var nextDict = (IReadOnlyDictionary<string, object>)nextSlot;

            if (prevDict != null && nextDict != null)
                PropsApplier.ApplyDiff(slotElement, prevDict, nextDict);
            else if (nextDict != null)
                PropsApplier.Apply(slotElement, nextDict);
        }

        /// <summary>
        /// Apply a slot sub-element's props dictionary on initial placement.
        /// </summary>
        public static void ApplySlot(
            VisualElement parent,
            Dictionary<string, object> slotProps,
            string slotName
        )
        {
            if (slotProps == null)
                return;

            var slotElement = ResolveSlotElement(parent, slotName);
            if (slotElement == null)
                return;

            PropsApplier.Apply(slotElement, slotProps);
        }

        // ─── Internals ──────────────────────────────────────────────────────

        private static readonly EmptyBaseProps s_empty = new();

        private static void DiffField(VisualElement element, string key, object prev, object next)
        {
            if (next != null)
                PropsApplier.ApplySingle(element, prev, key, next);
            else if (prev != null)
                PropsApplier.RemoveProp(element, key, prev);
        }

        private static void DiffNullableField<T>(
            VisualElement element,
            string key,
            T? prev,
            T? next
        )
            where T : struct
        {
            if (next.HasValue)
                PropsApplier.ApplySingle(
                    element,
                    prev.HasValue ? (object)prev.Value : null,
                    key,
                    next.Value
                );
            else if (prev.HasValue)
                PropsApplier.RemoveProp(element, key, prev.Value);
        }

        private static void DiffEvent(
            VisualElement element,
            string key,
            Delegate prev,
            Delegate next
        )
        {
            if (prev == next)
                return;
            if (next != null)
                PropsApplier.ApplySingle(element, prev, key, next);
            else if (prev != null)
                PropsApplier.RemoveProp(element, key, prev);
        }

        private static void ApplyEventIfSet(VisualElement element, string key, Delegate handler)
        {
            if (handler != null)
                PropsApplier.ApplySingle(element, null, key, handler);
        }

        private static void DiffExtraProps(
            VisualElement element,
            Dictionary<string, object> prev,
            Dictionary<string, object> next
        )
        {
            // Remove keys in prev not in next
            if (prev != null)
            {
                foreach (var kv in prev)
                {
                    if (next == null || !next.ContainsKey(kv.Key))
                        PropsApplier.RemoveProp(element, kv.Key, kv.Value);
                }
            }

            // Apply changed/new keys from next
            if (next != null)
            {
                foreach (var kv in next)
                {
                    object oldVal = null;
                    if (prev != null)
                        prev.TryGetValue(kv.Key, out oldVal);
                    if (oldVal != null && ReferenceEquals(oldVal, kv.Value))
                        continue;
                    if (oldVal != null && oldVal.Equals(kv.Value))
                        continue;
                    PropsApplier.ApplySingle(element, oldVal, kv.Key, kv.Value);
                }
            }
        }

        private static VisualElement ResolveSlotElement(VisualElement parent, string slotName)
        {
            if (parent == null)
                return null;
            // Slots are resolved by querying the parent for a child with a matching name
            // or by known UI Toolkit sub-element patterns
            return parent.Q(slotName) ?? parent.Q(className: $"unity-{slotName}");
        }

        /// <summary>
        /// Empty placeholder used when prev/next is null.
        /// All fields are default (null/false/0).
        /// </summary>
        private sealed class EmptyBaseProps : BaseProps
        {
            public override Dictionary<string, object> ToDictionary() => new(0);

            public override bool ShallowEquals(BaseProps other) => other is EmptyBaseProps;
        }
    }
}
