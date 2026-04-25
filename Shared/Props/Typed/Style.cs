using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.UIElements;
using SStack = System.Collections.Generic.Stack<ReactiveUITK.Props.Typed.Style>;

namespace ReactiveUITK.Props.Typed
{
    /// <summary>
    /// Typed style container with bitmask tracking. No dictionary, no boxing.
    /// Implements <see cref="IDictionary{TKey,TValue}"/> for backward compatibility
    /// with existing consumers that iterate or cast Style to IDictionary.
    /// </summary>
    public class Style : IDictionary<string, object>
    {
        // ═══════════════════════════════════════════════════════════════════
        //  Bit index constants
        // ═══════════════════════════════════════════════════════════════════
        internal const int BIT_WIDTH = 0;
        internal const int BIT_HEIGHT = 1;
        internal const int BIT_MIN_WIDTH = 2;
        internal const int BIT_MIN_HEIGHT = 3;
        internal const int BIT_MAX_WIDTH = 4;
        internal const int BIT_MAX_HEIGHT = 5;
        internal const int BIT_FLEX_GROW = 6;
        internal const int BIT_FLEX_SHRINK = 7;
        internal const int BIT_FLEX_BASIS = 8;
        internal const int BIT_FLEX_DIRECTION = 9;
        internal const int BIT_FLEX_WRAP = 10;
        internal const int BIT_JUSTIFY_CONTENT = 11;
        internal const int BIT_ALIGN_ITEMS = 12;
        internal const int BIT_ALIGN_SELF = 13;
        internal const int BIT_ALIGN_CONTENT = 14;
        internal const int BIT_POSITION = 15;
        internal const int BIT_LEFT = 16;
        internal const int BIT_TOP = 17;
        internal const int BIT_RIGHT = 18;
        internal const int BIT_BOTTOM = 19;
        internal const int BIT_DISPLAY = 20;
        internal const int BIT_VISIBILITY = 21;
        internal const int BIT_OVERFLOW = 22;
        internal const int BIT_OPACITY = 23;
        internal const int BIT_MARGIN = 24;
        internal const int BIT_MARGIN_LEFT = 25;
        internal const int BIT_MARGIN_RIGHT = 26;
        internal const int BIT_MARGIN_TOP = 27;
        internal const int BIT_MARGIN_BOTTOM = 28;
        internal const int BIT_PADDING = 29;
        internal const int BIT_PADDING_LEFT = 30;
        internal const int BIT_PADDING_RIGHT = 31;
        internal const int BIT_PADDING_TOP = 32;
        internal const int BIT_PADDING_BOTTOM = 33;
        internal const int BIT_FONT_SIZE = 34;
        internal const int BIT_FONT_FAMILY = 35;
        internal const int BIT_TEXT_ALIGN = 36;
        internal const int BIT_COLOR = 37;
        internal const int BIT_BACKGROUND_COLOR = 38;
        internal const int BIT_BACKGROUND_IMAGE = 39;
        internal const int BIT_BACKGROUND_IMAGE_TINT = 40;
        internal const int BIT_WHITE_SPACE = 41;
        internal const int BIT_LETTER_SPACING = 42;
        internal const int BIT_TEXT_OVERFLOW = 43;
        internal const int BIT_UNITY_FONT_STYLE = 44;
        internal const int BIT_UNITY_TEXT_OUTLINE_COLOR = 45;
        internal const int BIT_UNITY_TEXT_OUTLINE_WIDTH = 46;
        internal const int BIT_UNITY_TEXT_OVERFLOW_POS = 47;
        internal const int BIT_UNITY_TEXT_AUTO_SIZE = 48;
        internal const int BIT_BORDER_WIDTH = 49;
        internal const int BIT_BORDER_COLOR = 50;
        internal const int BIT_BORDER_LEFT_WIDTH = 51;
        internal const int BIT_BORDER_RIGHT_WIDTH = 52;
        internal const int BIT_BORDER_TOP_WIDTH = 53;
        internal const int BIT_BORDER_BOTTOM_WIDTH = 54;
        internal const int BIT_BORDER_LEFT_COLOR = 55;
        internal const int BIT_BORDER_RIGHT_COLOR = 56;
        internal const int BIT_BORDER_TOP_COLOR = 57;
        internal const int BIT_BORDER_BOTTOM_COLOR = 58;
        internal const int BIT_BORDER_RADIUS = 59;
        internal const int BIT_BORDER_TOP_LEFT_RADIUS = 60;
        internal const int BIT_BORDER_TOP_RIGHT_RADIUS = 61;
        internal const int BIT_BORDER_BOTTOM_LEFT_RADIUS = 62;
        internal const int BIT_BORDER_BOTTOM_RIGHT_RADIUS = 63;

        // _setBits1:
        internal const int BIT_ROTATE = 64;
        internal const int BIT_SCALE = 65;
        internal const int BIT_TRANSLATE = 66;
        internal const int BIT_BACKGROUND_REPEAT = 67;
        internal const int BIT_BACKGROUND_POSITION_X = 68;
        internal const int BIT_BACKGROUND_POSITION_Y = 69;
        internal const int BIT_BACKGROUND_SIZE = 70;
        internal const int BIT_TRANSFORM_ORIGIN = 71;
        internal const int BIT_TRANSITION_DELAY = 72;
        internal const int BIT_TRANSITION_DURATION = 73;
        internal const int BIT_TRANSITION_PROPERTY = 74;
        internal const int BIT_TRANSITION_TIMING_FUNC = 75;
        internal const int BIT_ASPECT_RATIO = 76;
        internal const int BIT_FILTER = 77;
        internal const int BIT_UNITY_MATERIAL = 78;

        // ═══════════════════════════════════════════════════════════════════
        //  Bitmask fields — which properties are set
        // ═══════════════════════════════════════════════════════════════════
        internal ulong _setBits0; // bits 0-63
        internal ulong _setBits1; // bits 0-14+ (for indices 64+)

        // ═══════════════════════════════════════════════════════════════════
        //  Pool generation stamp
        //  0 = user-created via new Style() — never pooled
        //  >0 = rented from pool via __Rent()
        // ═══════════════════════════════════════════════════════════════════
        internal uint _generation;

        // ═══════════════════════════════════════════════════════════════════
        //  Typed backing fields
        // ═══════════════════════════════════════════════════════════════════

        // ── StyleLength (27 fields) ──────────────────────────────────────
        internal StyleLength _width,
            _height,
            _minWidth,
            _minHeight,
            _maxWidth,
            _maxHeight;
        internal StyleLength _flexBasis;
        internal StyleLength _left,
            _top,
            _right,
            _bottom;
        internal StyleLength _marginLeft,
            _marginRight,
            _marginTop,
            _marginBottom;
        internal StyleLength _paddingLeft,
            _paddingRight,
            _paddingTop,
            _paddingBottom;
        internal StyleLength _borderTopLeftRadius,
            _borderTopRightRadius;
        internal StyleLength _borderBottomLeftRadius,
            _borderBottomRightRadius;
        internal StyleLength _fontSize,
            _letterSpacing;

        // ── StyleFloat (9 fields) ────────────────────────────────────────
        internal StyleFloat _flexGrow,
            _flexShrink,
            _opacity;
        internal StyleFloat _borderLeftWidth,
            _borderRightWidth;
        internal StyleFloat _borderTopWidth,
            _borderBottomWidth;
        internal StyleFloat _unityTextOutlineWidth;

        // ── Color (9 fields) ─────────────────────────────────────────────
        internal Color _color,
            _backgroundColor,
            _backgroundImageTint;
        internal Color _borderLeftColor,
            _borderRightColor;
        internal Color _borderTopColor,
            _borderBottomColor;
        internal Color _unityTextOutlineColor;

        // ── Enums ────────────────────────────────────────────────────────
        internal FlexDirection _flexDirection;
        internal Wrap _flexWrap;
        internal Justify _justifyContent;
        internal Align _alignItems,
            _alignSelf,
            _alignContent;
        internal Position _position;
        internal DisplayStyle _display;
        internal Visibility _visibility;
        internal Overflow _overflow;
        internal WhiteSpace _whiteSpace;
        internal TextAnchor _textAlign;
        internal TextOverflow _textOverflow;
        internal FontStyle _unityFontStyle;
        internal TextOverflowPosition _unityTextOverflowPosition;
        internal TextAutoSizeMode _unityTextAutoSize;

        // ── float ────────────────────────────────────────────────────────
        internal float _rotate,
            _scale;

        // ── Compound structs ─────────────────────────────────────────────
        internal BackgroundRepeat _backgroundRepeat;
        internal BackgroundPosition _backgroundPositionX,
            _backgroundPositionY;
        internal BackgroundSize _backgroundSize;
        internal TransformOrigin _transformOrigin;
        internal Translate _translate;

        // ── Reference types ──────────────────────────────────────────────
        internal Texture2D _backgroundImage;
        internal Font _fontFamily;

        // ── Transitions ──────────────────────────────────────────────────
        internal StyleList<TimeValue> _transitionDelay;
        internal StyleList<TimeValue> _transitionDuration;
        internal StyleList<StylePropertyName> _transitionProperty;
        internal StyleList<EasingFunction> _transitionTimingFunction;

        // ── Unity 6.3+ ──────────────────────────────────────────────────
#if UNITY_6000_3_OR_NEWER
        internal StyleRatio _aspectRatio;
        internal StyleList<FilterFunction> _filter;
        internal StyleMaterialDefinition _unityMaterial;
#endif

        // ═══════════════════════════════════════════════════════════════════
        //  Constructors
        // ═══════════════════════════════════════════════════════════════════

        public Style() { }

        // ═══════════════════════════════════════════════════════════════════
        //  Object pool — rent/return lifecycle
        // ═══════════════════════════════════════════════════════════════════

        private const int PoolCapacity = 4096;

        private static readonly SStack s_pool = new SStack(256);
        private static readonly List<Style> s_pendingReturn = new List<Style>(2048);
        private static uint s_nextGeneration = 1;

        /// <summary>
        /// Rent a Style from the pool. Only called by generated code.
        /// The returned Style has all fields cleared and a generation &gt; 0.
        /// </summary>
        public static Style __Rent()
        {
            Style s;
            if (s_pool.Count > 0)
            {
                s = s_pool.Pop();
                // Only clear when reusing from pool — new objects
                // are already zero-initialized by the CLR.
                s._setBits0 = 0;
                s._setBits1 = 0;
                s._backgroundImage = null;
                s._fontFamily = null;
                s._transitionDelay = default;
                s._transitionDuration = default;
                s._transitionProperty = default;
                s._transitionTimingFunction = default;
#if UNITY_6000_3_OR_NEWER
                s._filter = default;
                s._unityMaterial = null;
#endif
            }
            else
            {
                s = new Style();
            }

            // Stamp with new generation (never 0)
            uint gen = s_nextGeneration++;
            if (gen == 0)
                gen = s_nextGeneration++; // skip 0 on overflow
            s._generation = gen;
            return s;
        }

        /// <summary>
        /// Schedule a Style for return to pool on next flush.
        /// Styles with generation 0 (user-created) are ignored.
        /// </summary>
        internal static void __ScheduleReturn(Style s)
        {
            if (s == null || s._generation == 0)
                return;
            s_pendingReturn.Add(s);
        }

        /// <summary>
        /// Move all pending returns into the pool. Called once per frame
        /// after the full commit tree walk has completed.
        /// </summary>
        internal static void __FlushReturns()
        {
            for (int i = 0; i < s_pendingReturn.Count; i++)
            {
                if (s_pool.Count < PoolCapacity)
                    s_pool.Push(s_pendingReturn[i]);
                // else: drop — pool is full, let GC collect
            }
            s_pendingReturn.Clear();
        }

        /// <summary>
        /// Identity check that is safe across pool rent/return cycles.
        /// Returns true only when both references point to the same object
        /// AND that object has not been recycled since it was assigned.
        /// </summary>
        internal static bool SameInstance(Style a, Style b)
        {
            if (a == null && b == null)
                return true;
            if (a == null || b == null)
                return false;
            return ReferenceEquals(a, b) && a._generation == b._generation;
        }

        // ═══════════════════════════════════════════════════════════════════
        //  Collection initializer support: new Style { (key, val), ... }
        // ═══════════════════════════════════════════════════════════════════

        public void Add((string key, object value) entry)
        {
            SetByKey(entry.key, entry.value);
        }

        public static Style Of(params (string key, object value)[] entries)
        {
            var style = new Style();
            if (entries != null)
            {
                for (int i = 0; i < entries.Length; i++)
                    style.SetByKey(entries[i].key, entries[i].value);
            }
            return style;
        }

        // ═══════════════════════════════════════════════════════════════════
        //  Typed property setters + getters (zero boxing)
        // ═══════════════════════════════════════════════════════════════════

        // ── Layout (StyleLength) ─────────────────────────────────────────
        public StyleLength Width
        {
            get => _width;
            set
            {
                _width = value;
                _setBits0 |= (1UL << BIT_WIDTH);
            }
        }
        public StyleLength Height
        {
            get => _height;
            set
            {
                _height = value;
                _setBits0 |= (1UL << BIT_HEIGHT);
            }
        }
        public StyleLength MinWidth
        {
            get => _minWidth;
            set
            {
                _minWidth = value;
                _setBits0 |= (1UL << BIT_MIN_WIDTH);
            }
        }
        public StyleLength MinHeight
        {
            get => _minHeight;
            set
            {
                _minHeight = value;
                _setBits0 |= (1UL << BIT_MIN_HEIGHT);
            }
        }
        public StyleLength MaxWidth
        {
            get => _maxWidth;
            set
            {
                _maxWidth = value;
                _setBits0 |= (1UL << BIT_MAX_WIDTH);
            }
        }
        public StyleLength MaxHeight
        {
            get => _maxHeight;
            set
            {
                _maxHeight = value;
                _setBits0 |= (1UL << BIT_MAX_HEIGHT);
            }
        }
        public StyleLength FlexBasis
        {
            get => _flexBasis;
            set
            {
                _flexBasis = value;
                _setBits0 |= (1UL << BIT_FLEX_BASIS);
            }
        }

        // ── Positioning (StyleLength) ────────────────────────────────────
        public StyleLength Left
        {
            get => _left;
            set
            {
                _left = value;
                _setBits0 |= (1UL << BIT_LEFT);
            }
        }
        public StyleLength Top
        {
            get => _top;
            set
            {
                _top = value;
                _setBits0 |= (1UL << BIT_TOP);
            }
        }
        public StyleLength Right
        {
            get => _right;
            set
            {
                _right = value;
                _setBits0 |= (1UL << BIT_RIGHT);
            }
        }
        public StyleLength Bottom
        {
            get => _bottom;
            set
            {
                _bottom = value;
                _setBits0 |= (1UL << BIT_BOTTOM);
            }
        }

        // ── Spacing (StyleLength) — shorthands expand to all sides ───────
        public StyleLength Margin
        {
            set
            {
                _marginLeft = value;
                _setBits0 |= (1UL << BIT_MARGIN_LEFT);
                _marginRight = value;
                _setBits0 |= (1UL << BIT_MARGIN_RIGHT);
                _marginTop = value;
                _setBits0 |= (1UL << BIT_MARGIN_TOP);
                _marginBottom = value;
                _setBits0 |= (1UL << BIT_MARGIN_BOTTOM);
                _setBits0 |= (1UL << BIT_MARGIN);
            }
        }
        public StyleLength MarginLeft
        {
            get => _marginLeft;
            set
            {
                _marginLeft = value;
                _setBits0 |= (1UL << BIT_MARGIN_LEFT);
            }
        }
        public StyleLength MarginRight
        {
            get => _marginRight;
            set
            {
                _marginRight = value;
                _setBits0 |= (1UL << BIT_MARGIN_RIGHT);
            }
        }
        public StyleLength MarginTop
        {
            get => _marginTop;
            set
            {
                _marginTop = value;
                _setBits0 |= (1UL << BIT_MARGIN_TOP);
            }
        }
        public StyleLength MarginBottom
        {
            get => _marginBottom;
            set
            {
                _marginBottom = value;
                _setBits0 |= (1UL << BIT_MARGIN_BOTTOM);
            }
        }
        public StyleLength Padding
        {
            set
            {
                _paddingLeft = value;
                _setBits0 |= (1UL << BIT_PADDING_LEFT);
                _paddingRight = value;
                _setBits0 |= (1UL << BIT_PADDING_RIGHT);
                _paddingTop = value;
                _setBits0 |= (1UL << BIT_PADDING_TOP);
                _paddingBottom = value;
                _setBits0 |= (1UL << BIT_PADDING_BOTTOM);
                _setBits0 |= (1UL << BIT_PADDING);
            }
        }
        public StyleLength PaddingLeft
        {
            get => _paddingLeft;
            set
            {
                _paddingLeft = value;
                _setBits0 |= (1UL << BIT_PADDING_LEFT);
            }
        }
        public StyleLength PaddingRight
        {
            get => _paddingRight;
            set
            {
                _paddingRight = value;
                _setBits0 |= (1UL << BIT_PADDING_RIGHT);
            }
        }
        public StyleLength PaddingTop
        {
            get => _paddingTop;
            set
            {
                _paddingTop = value;
                _setBits0 |= (1UL << BIT_PADDING_TOP);
            }
        }
        public StyleLength PaddingBottom
        {
            get => _paddingBottom;
            set
            {
                _paddingBottom = value;
                _setBits0 |= (1UL << BIT_PADDING_BOTTOM);
            }
        }

        // ── Border radius (StyleLength) ──────────────────────────────────
        public StyleLength BorderRadius
        {
            set
            {
                _borderTopLeftRadius = value;
                _setBits0 |= (1UL << BIT_BORDER_TOP_LEFT_RADIUS);
                _borderTopRightRadius = value;
                _setBits0 |= (1UL << BIT_BORDER_TOP_RIGHT_RADIUS);
                _borderBottomLeftRadius = value;
                _setBits0 |= (1UL << BIT_BORDER_BOTTOM_LEFT_RADIUS);
                _borderBottomRightRadius = value;
                _setBits0 |= (1UL << BIT_BORDER_BOTTOM_RIGHT_RADIUS);
                _setBits0 |= (1UL << BIT_BORDER_RADIUS);
            }
        }
        public StyleLength BorderTopLeftRadius
        {
            get => _borderTopLeftRadius;
            set
            {
                _borderTopLeftRadius = value;
                _setBits0 |= (1UL << BIT_BORDER_TOP_LEFT_RADIUS);
            }
        }
        public StyleLength BorderTopRightRadius
        {
            get => _borderTopRightRadius;
            set
            {
                _borderTopRightRadius = value;
                _setBits0 |= (1UL << BIT_BORDER_TOP_RIGHT_RADIUS);
            }
        }
        public StyleLength BorderBottomLeftRadius
        {
            get => _borderBottomLeftRadius;
            set
            {
                _borderBottomLeftRadius = value;
                _setBits0 |= (1UL << BIT_BORDER_BOTTOM_LEFT_RADIUS);
            }
        }
        public StyleLength BorderBottomRightRadius
        {
            get => _borderBottomRightRadius;
            set
            {
                _borderBottomRightRadius = value;
                _setBits0 |= (1UL << BIT_BORDER_BOTTOM_RIGHT_RADIUS);
            }
        }

        // ── Text (StyleLength) ───────────────────────────────────────────
        public StyleLength FontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;
                _setBits0 |= (1UL << BIT_FONT_SIZE);
            }
        }
        public StyleLength LetterSpacing
        {
            get => _letterSpacing;
            set
            {
                _letterSpacing = value;
                _setBits0 |= (1UL << BIT_LETTER_SPACING);
            }
        }

        // ── Flexbox float (StyleFloat) ───────────────────────────────────
        public StyleFloat FlexGrow
        {
            get => _flexGrow;
            set
            {
                _flexGrow = value;
                _setBits0 |= (1UL << BIT_FLEX_GROW);
            }
        }
        public StyleFloat FlexShrink
        {
            get => _flexShrink;
            set
            {
                _flexShrink = value;
                _setBits0 |= (1UL << BIT_FLEX_SHRINK);
            }
        }
        public StyleFloat Opacity
        {
            get => _opacity;
            set
            {
                _opacity = value;
                _setBits0 |= (1UL << BIT_OPACITY);
            }
        }

        // ── Border width (StyleFloat) — shorthand expands to all sides ───
        public StyleFloat BorderWidth
        {
            set
            {
                _borderLeftWidth = value;
                _setBits0 |= (1UL << BIT_BORDER_LEFT_WIDTH);
                _borderRightWidth = value;
                _setBits0 |= (1UL << BIT_BORDER_RIGHT_WIDTH);
                _borderTopWidth = value;
                _setBits0 |= (1UL << BIT_BORDER_TOP_WIDTH);
                _borderBottomWidth = value;
                _setBits0 |= (1UL << BIT_BORDER_BOTTOM_WIDTH);
                _setBits0 |= (1UL << BIT_BORDER_WIDTH);
            }
        }
        public StyleFloat BorderLeftWidth
        {
            get => _borderLeftWidth;
            set
            {
                _borderLeftWidth = value;
                _setBits0 |= (1UL << BIT_BORDER_LEFT_WIDTH);
            }
        }
        public StyleFloat BorderRightWidth
        {
            get => _borderRightWidth;
            set
            {
                _borderRightWidth = value;
                _setBits0 |= (1UL << BIT_BORDER_RIGHT_WIDTH);
            }
        }
        public StyleFloat BorderTopWidth
        {
            get => _borderTopWidth;
            set
            {
                _borderTopWidth = value;
                _setBits0 |= (1UL << BIT_BORDER_TOP_WIDTH);
            }
        }
        public StyleFloat BorderBottomWidth
        {
            get => _borderBottomWidth;
            set
            {
                _borderBottomWidth = value;
                _setBits0 |= (1UL << BIT_BORDER_BOTTOM_WIDTH);
            }
        }
        public StyleFloat UnityTextOutlineWidth
        {
            get => _unityTextOutlineWidth;
            set
            {
                _unityTextOutlineWidth = value;
                _setBits0 |= (1UL << BIT_UNITY_TEXT_OUTLINE_WIDTH);
            }
        }

        // ── Colors ───────────────────────────────────────────────────────
        public Color Color
        {
            get => _color;
            set
            {
                _color = value;
                _setBits0 |= (1UL << BIT_COLOR);
            }
        }
        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                _setBits0 |= (1UL << BIT_BACKGROUND_COLOR);
            }
        }
        public Color BackgroundImageTint
        {
            get => _backgroundImageTint;
            set
            {
                _backgroundImageTint = value;
                _setBits0 |= (1UL << BIT_BACKGROUND_IMAGE_TINT);
            }
        }
        public Color BorderColor
        {
            set
            {
                _borderLeftColor = value;
                _setBits0 |= (1UL << BIT_BORDER_LEFT_COLOR);
                _borderRightColor = value;
                _setBits0 |= (1UL << BIT_BORDER_RIGHT_COLOR);
                _borderTopColor = value;
                _setBits0 |= (1UL << BIT_BORDER_TOP_COLOR);
                _borderBottomColor = value;
                _setBits0 |= (1UL << BIT_BORDER_BOTTOM_COLOR);
                _setBits0 |= (1UL << BIT_BORDER_COLOR);
            }
        }
        public Color BorderLeftColor
        {
            get => _borderLeftColor;
            set
            {
                _borderLeftColor = value;
                _setBits0 |= (1UL << BIT_BORDER_LEFT_COLOR);
            }
        }
        public Color BorderRightColor
        {
            get => _borderRightColor;
            set
            {
                _borderRightColor = value;
                _setBits0 |= (1UL << BIT_BORDER_RIGHT_COLOR);
            }
        }
        public Color BorderTopColor
        {
            get => _borderTopColor;
            set
            {
                _borderTopColor = value;
                _setBits0 |= (1UL << BIT_BORDER_TOP_COLOR);
            }
        }
        public Color BorderBottomColor
        {
            get => _borderBottomColor;
            set
            {
                _borderBottomColor = value;
                _setBits0 |= (1UL << BIT_BORDER_BOTTOM_COLOR);
            }
        }
        public Color UnityTextOutlineColor
        {
            get => _unityTextOutlineColor;
            set
            {
                _unityTextOutlineColor = value;
                _setBits0 |= (1UL << BIT_UNITY_TEXT_OUTLINE_COLOR);
            }
        }

        // ── Enum styles ──────────────────────────────────────────────────
        public FlexDirection FlexDirection
        {
            get => _flexDirection;
            set
            {
                _flexDirection = value;
                _setBits0 |= (1UL << BIT_FLEX_DIRECTION);
            }
        }
        public Wrap FlexWrap
        {
            get => _flexWrap;
            set
            {
                _flexWrap = value;
                _setBits0 |= (1UL << BIT_FLEX_WRAP);
            }
        }
        public Justify JustifyContent
        {
            get => _justifyContent;
            set
            {
                _justifyContent = value;
                _setBits0 |= (1UL << BIT_JUSTIFY_CONTENT);
            }
        }
        public Align AlignItems
        {
            get => _alignItems;
            set
            {
                _alignItems = value;
                _setBits0 |= (1UL << BIT_ALIGN_ITEMS);
            }
        }
        public Align AlignSelf
        {
            get => _alignSelf;
            set
            {
                _alignSelf = value;
                _setBits0 |= (1UL << BIT_ALIGN_SELF);
            }
        }
        public Align AlignContent
        {
            get => _alignContent;
            set
            {
                _alignContent = value;
                _setBits0 |= (1UL << BIT_ALIGN_CONTENT);
            }
        }
        public Position Position
        {
            get => _position;
            set
            {
                _position = value;
                _setBits0 |= (1UL << BIT_POSITION);
            }
        }
        public DisplayStyle Display
        {
            get => _display;
            set
            {
                _display = value;
                _setBits0 |= (1UL << BIT_DISPLAY);
            }
        }
        public Visibility Visibility
        {
            get => _visibility;
            set
            {
                _visibility = value;
                _setBits0 |= (1UL << BIT_VISIBILITY);
            }
        }
        public Overflow Overflow
        {
            get => _overflow;
            set
            {
                _overflow = value;
                _setBits0 |= (1UL << BIT_OVERFLOW);
            }
        }
        public WhiteSpace WhiteSpace
        {
            get => _whiteSpace;
            set
            {
                _whiteSpace = value;
                _setBits0 |= (1UL << BIT_WHITE_SPACE);
            }
        }
        public TextAnchor TextAlign
        {
            get => _textAlign;
            set
            {
                _textAlign = value;
                _setBits0 |= (1UL << BIT_TEXT_ALIGN);
            }
        }
        public TextOverflow TextOverflow
        {
            get => _textOverflow;
            set
            {
                _textOverflow = value;
                _setBits0 |= (1UL << BIT_TEXT_OVERFLOW);
            }
        }
        public FontStyle UnityFontStyle
        {
            get => _unityFontStyle;
            set
            {
                _unityFontStyle = value;
                _setBits0 |= (1UL << BIT_UNITY_FONT_STYLE);
            }
        }
        public TextOverflowPosition UnityTextOverflowPosition
        {
            get => _unityTextOverflowPosition;
            set
            {
                _unityTextOverflowPosition = value;
                _setBits0 |= (1UL << BIT_UNITY_TEXT_OVERFLOW_POS);
            }
        }
        public TextAutoSizeMode UnityTextAutoSize
        {
            get => _unityTextAutoSize;
            set
            {
                _unityTextAutoSize = value;
                _setBits0 |= (1UL << BIT_UNITY_TEXT_AUTO_SIZE);
            }
        }

        // ── Assets ───────────────────────────────────────────────────────
        public Texture2D BackgroundImage
        {
            get => _backgroundImage;
            set
            {
                _backgroundImage = value;
                _setBits0 |= (1UL << BIT_BACKGROUND_IMAGE);
            }
        }
        public Font FontFamily
        {
            get => _fontFamily;
            set
            {
                _fontFamily = value;
                _setBits0 |= (1UL << BIT_FONT_FAMILY);
            }
        }

        // ── Transforms ──────────────────────────────────────────────────
        public float Rotate
        {
            get => _rotate;
            set
            {
                _rotate = value;
                _setBits1 |= (1UL << (BIT_ROTATE - 64));
            }
        }
        public float Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                _setBits1 |= (1UL << (BIT_SCALE - 64));
            }
        }
        public Translate Translate
        {
            get => _translate;
            set
            {
                _translate = value;
                _setBits1 |= (1UL << (BIT_TRANSLATE - 64));
            }
        }

        // ── Background (compound structs) ────────────────────────────────
        public BackgroundRepeat BackgroundRepeat
        {
            get => _backgroundRepeat;
            set
            {
                _backgroundRepeat = value;
                _setBits1 |= (1UL << (BIT_BACKGROUND_REPEAT - 64));
            }
        }
        public BackgroundPosition BackgroundPositionX
        {
            get => _backgroundPositionX;
            set
            {
                _backgroundPositionX = value;
                _setBits1 |= (1UL << (BIT_BACKGROUND_POSITION_X - 64));
            }
        }
        public BackgroundPosition BackgroundPositionY
        {
            get => _backgroundPositionY;
            set
            {
                _backgroundPositionY = value;
                _setBits1 |= (1UL << (BIT_BACKGROUND_POSITION_Y - 64));
            }
        }
        public BackgroundSize BackgroundSize
        {
            get => _backgroundSize;
            set
            {
                _backgroundSize = value;
                _setBits1 |= (1UL << (BIT_BACKGROUND_SIZE - 64));
            }
        }

        // ── Transform origin ─────────────────────────────────────────────
        public TransformOrigin TransformOrigin
        {
            get => _transformOrigin;
            set
            {
                _transformOrigin = value;
                _setBits1 |= (1UL << (BIT_TRANSFORM_ORIGIN - 64));
            }
        }

        // ── Transitions ──────────────────────────────────────────────────
        public StyleList<TimeValue> TransitionDelay
        {
            get => _transitionDelay;
            set
            {
                _transitionDelay = value;
                _setBits1 |= (1UL << (BIT_TRANSITION_DELAY - 64));
            }
        }
        public StyleList<TimeValue> TransitionDuration
        {
            get => _transitionDuration;
            set
            {
                _transitionDuration = value;
                _setBits1 |= (1UL << (BIT_TRANSITION_DURATION - 64));
            }
        }
        public StyleList<StylePropertyName> TransitionProperty
        {
            get => _transitionProperty;
            set
            {
                _transitionProperty = value;
                _setBits1 |= (1UL << (BIT_TRANSITION_PROPERTY - 64));
            }
        }
        public StyleList<EasingFunction> TransitionTimingFunction
        {
            get => _transitionTimingFunction;
            set
            {
                _transitionTimingFunction = value;
                _setBits1 |= (1UL << (BIT_TRANSITION_TIMING_FUNC - 64));
            }
        }

        // ── Unity 6.3+ ──────────────────────────────────────────────────
#if UNITY_6000_3_OR_NEWER
        public StyleRatio AspectRatio
        {
            get => _aspectRatio;
            set
            {
                _aspectRatio = value;
                _setBits1 |= (1UL << (BIT_ASPECT_RATIO - 64));
            }
        }
        public StyleList<FilterFunction> Filter
        {
            get => _filter;
            set
            {
                _filter = value;
                _setBits1 |= (1UL << (BIT_FILTER - 64));
            }
        }
        public StyleMaterialDefinition UnityMaterial
        {
            get => _unityMaterial;
            set
            {
                _unityMaterial = value;
                _setBits1 |= (1UL << (BIT_UNITY_MATERIAL - 64));
            }
        }
#endif

        // ═══════════════════════════════════════════════════════════════════
        //  Typed equality — no boxing, no dict iteration
        // ═══════════════════════════════════════════════════════════════════

        public bool TypedEquals(Style other)
        {
            if (other == null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            if (_setBits0 != other._setBits0 || _setBits1 != other._setBits1)
                return false;

            ulong bits = _setBits0;
            while (bits != 0)
            {
                int bit = BitOps.TrailingZeroCount(bits);
                if (!FieldEquals(bit, other))
                    return false;
                bits &= bits - 1;
            }
            bits = _setBits1;
            while (bits != 0)
            {
                int bit = BitOps.TrailingZeroCount(bits);
                if (!FieldEquals(64 + bit, other))
                    return false;
                bits &= bits - 1;
            }
            return true;
        }

        internal bool FieldEquals(int bit, Style other)
        {
            switch (bit)
            {
                // StyleLength
                case BIT_WIDTH:
                    return _width == other._width;
                case BIT_HEIGHT:
                    return _height == other._height;
                case BIT_MIN_WIDTH:
                    return _minWidth == other._minWidth;
                case BIT_MIN_HEIGHT:
                    return _minHeight == other._minHeight;
                case BIT_MAX_WIDTH:
                    return _maxWidth == other._maxWidth;
                case BIT_MAX_HEIGHT:
                    return _maxHeight == other._maxHeight;
                case BIT_FLEX_BASIS:
                    return _flexBasis == other._flexBasis;
                case BIT_LEFT:
                    return _left == other._left;
                case BIT_TOP:
                    return _top == other._top;
                case BIT_RIGHT:
                    return _right == other._right;
                case BIT_BOTTOM:
                    return _bottom == other._bottom;
                case BIT_MARGIN_LEFT:
                    return _marginLeft == other._marginLeft;
                case BIT_MARGIN_RIGHT:
                    return _marginRight == other._marginRight;
                case BIT_MARGIN_TOP:
                    return _marginTop == other._marginTop;
                case BIT_MARGIN_BOTTOM:
                    return _marginBottom == other._marginBottom;
                case BIT_PADDING_LEFT:
                    return _paddingLeft == other._paddingLeft;
                case BIT_PADDING_RIGHT:
                    return _paddingRight == other._paddingRight;
                case BIT_PADDING_TOP:
                    return _paddingTop == other._paddingTop;
                case BIT_PADDING_BOTTOM:
                    return _paddingBottom == other._paddingBottom;
                case BIT_BORDER_TOP_LEFT_RADIUS:
                    return _borderTopLeftRadius == other._borderTopLeftRadius;
                case BIT_BORDER_TOP_RIGHT_RADIUS:
                    return _borderTopRightRadius == other._borderTopRightRadius;
                case BIT_BORDER_BOTTOM_LEFT_RADIUS:
                    return _borderBottomLeftRadius == other._borderBottomLeftRadius;
                case BIT_BORDER_BOTTOM_RIGHT_RADIUS:
                    return _borderBottomRightRadius == other._borderBottomRightRadius;
                case BIT_FONT_SIZE:
                    return _fontSize == other._fontSize;
                case BIT_LETTER_SPACING:
                    return _letterSpacing == other._letterSpacing;
                // Shorthand tracking bits — no backing field comparison needed
                case BIT_MARGIN:
                    return true;
                case BIT_PADDING:
                    return true;
                case BIT_BORDER_RADIUS:
                    return true;
                case BIT_BORDER_WIDTH:
                    return true;
                case BIT_BORDER_COLOR:
                    return true;
                // StyleFloat
                case BIT_FLEX_GROW:
                    return _flexGrow == other._flexGrow;
                case BIT_FLEX_SHRINK:
                    return _flexShrink == other._flexShrink;
                case BIT_OPACITY:
                    return _opacity == other._opacity;
                case BIT_BORDER_LEFT_WIDTH:
                    return _borderLeftWidth == other._borderLeftWidth;
                case BIT_BORDER_RIGHT_WIDTH:
                    return _borderRightWidth == other._borderRightWidth;
                case BIT_BORDER_TOP_WIDTH:
                    return _borderTopWidth == other._borderTopWidth;
                case BIT_BORDER_BOTTOM_WIDTH:
                    return _borderBottomWidth == other._borderBottomWidth;
                case BIT_UNITY_TEXT_OUTLINE_WIDTH:
                    return _unityTextOutlineWidth == other._unityTextOutlineWidth;
                // Color
                case BIT_COLOR:
                    return _color == other._color;
                case BIT_BACKGROUND_COLOR:
                    return _backgroundColor == other._backgroundColor;
                case BIT_BACKGROUND_IMAGE_TINT:
                    return _backgroundImageTint == other._backgroundImageTint;
                case BIT_BORDER_LEFT_COLOR:
                    return _borderLeftColor == other._borderLeftColor;
                case BIT_BORDER_RIGHT_COLOR:
                    return _borderRightColor == other._borderRightColor;
                case BIT_BORDER_TOP_COLOR:
                    return _borderTopColor == other._borderTopColor;
                case BIT_BORDER_BOTTOM_COLOR:
                    return _borderBottomColor == other._borderBottomColor;
                case BIT_UNITY_TEXT_OUTLINE_COLOR:
                    return _unityTextOutlineColor == other._unityTextOutlineColor;
                // Enums
                case BIT_FLEX_DIRECTION:
                    return _flexDirection == other._flexDirection;
                case BIT_FLEX_WRAP:
                    return _flexWrap == other._flexWrap;
                case BIT_JUSTIFY_CONTENT:
                    return _justifyContent == other._justifyContent;
                case BIT_ALIGN_ITEMS:
                    return _alignItems == other._alignItems;
                case BIT_ALIGN_SELF:
                    return _alignSelf == other._alignSelf;
                case BIT_ALIGN_CONTENT:
                    return _alignContent == other._alignContent;
                case BIT_POSITION:
                    return _position == other._position;
                case BIT_DISPLAY:
                    return _display == other._display;
                case BIT_VISIBILITY:
                    return _visibility == other._visibility;
                case BIT_OVERFLOW:
                    return _overflow == other._overflow;
                case BIT_WHITE_SPACE:
                    return _whiteSpace == other._whiteSpace;
                case BIT_TEXT_ALIGN:
                    return _textAlign == other._textAlign;
                case BIT_TEXT_OVERFLOW:
                    return _textOverflow == other._textOverflow;
                case BIT_UNITY_FONT_STYLE:
                    return _unityFontStyle == other._unityFontStyle;
                case BIT_UNITY_TEXT_OVERFLOW_POS:
                    return _unityTextOverflowPosition == other._unityTextOverflowPosition;
                case BIT_UNITY_TEXT_AUTO_SIZE:
                    return _unityTextAutoSize == other._unityTextAutoSize;
                // Assets
                case BIT_BACKGROUND_IMAGE:
                    return ReferenceEquals(_backgroundImage, other._backgroundImage);
                case BIT_FONT_FAMILY:
                    return ReferenceEquals(_fontFamily, other._fontFamily);
                // float
                case BIT_ROTATE:
                    return _rotate == other._rotate;
                case BIT_SCALE:
                    return _scale == other._scale;
                // Compound structs
                case BIT_TRANSLATE:
                    return _translate.Equals(other._translate);
                case BIT_BACKGROUND_REPEAT:
                    return _backgroundRepeat.Equals(other._backgroundRepeat);
                case BIT_BACKGROUND_POSITION_X:
                    return _backgroundPositionX.Equals(other._backgroundPositionX);
                case BIT_BACKGROUND_POSITION_Y:
                    return _backgroundPositionY.Equals(other._backgroundPositionY);
                case BIT_BACKGROUND_SIZE:
                    return _backgroundSize.Equals(other._backgroundSize);
                case BIT_TRANSFORM_ORIGIN:
                    return _transformOrigin.Equals(other._transformOrigin);
                // Transitions
                case BIT_TRANSITION_DELAY:
                    return _transitionDelay.Equals(other._transitionDelay);
                case BIT_TRANSITION_DURATION:
                    return _transitionDuration.Equals(other._transitionDuration);
                case BIT_TRANSITION_PROPERTY:
                    return _transitionProperty.Equals(other._transitionProperty);
                case BIT_TRANSITION_TIMING_FUNC:
                    return _transitionTimingFunction.Equals(other._transitionTimingFunction);
                // Unity 6.3+
                case BIT_ASPECT_RATIO:
                    return true; // fallback — conditional compile handled in DiffStyle
                case BIT_FILTER:
                    return true;
                case BIT_UNITY_MATERIAL:
                    return true;
                default:
                    return true;
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        //  Copy all set properties to another Style
        // ═══════════════════════════════════════════════════════════════════

        public void CopyTo(Style target)
        {
            ulong bits = _setBits0;
            while (bits != 0)
            {
                int bit = BitOps.TrailingZeroCount(bits);
                CopyField(bit, target);
                bits &= bits - 1;
            }
            target._setBits0 |= _setBits0;

            bits = _setBits1;
            while (bits != 0)
            {
                int bit = BitOps.TrailingZeroCount(bits);
                CopyField(64 + bit, target);
                bits &= bits - 1;
            }
            target._setBits1 |= _setBits1;
        }

        private void CopyField(int bit, Style target)
        {
            switch (bit)
            {
                case BIT_WIDTH:
                    target._width = _width;
                    break;
                case BIT_HEIGHT:
                    target._height = _height;
                    break;
                case BIT_MIN_WIDTH:
                    target._minWidth = _minWidth;
                    break;
                case BIT_MIN_HEIGHT:
                    target._minHeight = _minHeight;
                    break;
                case BIT_MAX_WIDTH:
                    target._maxWidth = _maxWidth;
                    break;
                case BIT_MAX_HEIGHT:
                    target._maxHeight = _maxHeight;
                    break;
                case BIT_FLEX_BASIS:
                    target._flexBasis = _flexBasis;
                    break;
                case BIT_LEFT:
                    target._left = _left;
                    break;
                case BIT_TOP:
                    target._top = _top;
                    break;
                case BIT_RIGHT:
                    target._right = _right;
                    break;
                case BIT_BOTTOM:
                    target._bottom = _bottom;
                    break;
                case BIT_MARGIN_LEFT:
                    target._marginLeft = _marginLeft;
                    break;
                case BIT_MARGIN_RIGHT:
                    target._marginRight = _marginRight;
                    break;
                case BIT_MARGIN_TOP:
                    target._marginTop = _marginTop;
                    break;
                case BIT_MARGIN_BOTTOM:
                    target._marginBottom = _marginBottom;
                    break;
                case BIT_PADDING_LEFT:
                    target._paddingLeft = _paddingLeft;
                    break;
                case BIT_PADDING_RIGHT:
                    target._paddingRight = _paddingRight;
                    break;
                case BIT_PADDING_TOP:
                    target._paddingTop = _paddingTop;
                    break;
                case BIT_PADDING_BOTTOM:
                    target._paddingBottom = _paddingBottom;
                    break;
                case BIT_BORDER_TOP_LEFT_RADIUS:
                    target._borderTopLeftRadius = _borderTopLeftRadius;
                    break;
                case BIT_BORDER_TOP_RIGHT_RADIUS:
                    target._borderTopRightRadius = _borderTopRightRadius;
                    break;
                case BIT_BORDER_BOTTOM_LEFT_RADIUS:
                    target._borderBottomLeftRadius = _borderBottomLeftRadius;
                    break;
                case BIT_BORDER_BOTTOM_RIGHT_RADIUS:
                    target._borderBottomRightRadius = _borderBottomRightRadius;
                    break;
                case BIT_FONT_SIZE:
                    target._fontSize = _fontSize;
                    break;
                case BIT_LETTER_SPACING:
                    target._letterSpacing = _letterSpacing;
                    break;
                case BIT_FLEX_GROW:
                    target._flexGrow = _flexGrow;
                    break;
                case BIT_FLEX_SHRINK:
                    target._flexShrink = _flexShrink;
                    break;
                case BIT_OPACITY:
                    target._opacity = _opacity;
                    break;
                case BIT_BORDER_LEFT_WIDTH:
                    target._borderLeftWidth = _borderLeftWidth;
                    break;
                case BIT_BORDER_RIGHT_WIDTH:
                    target._borderRightWidth = _borderRightWidth;
                    break;
                case BIT_BORDER_TOP_WIDTH:
                    target._borderTopWidth = _borderTopWidth;
                    break;
                case BIT_BORDER_BOTTOM_WIDTH:
                    target._borderBottomWidth = _borderBottomWidth;
                    break;
                case BIT_UNITY_TEXT_OUTLINE_WIDTH:
                    target._unityTextOutlineWidth = _unityTextOutlineWidth;
                    break;
                case BIT_COLOR:
                    target._color = _color;
                    break;
                case BIT_BACKGROUND_COLOR:
                    target._backgroundColor = _backgroundColor;
                    break;
                case BIT_BACKGROUND_IMAGE_TINT:
                    target._backgroundImageTint = _backgroundImageTint;
                    break;
                case BIT_BORDER_LEFT_COLOR:
                    target._borderLeftColor = _borderLeftColor;
                    break;
                case BIT_BORDER_RIGHT_COLOR:
                    target._borderRightColor = _borderRightColor;
                    break;
                case BIT_BORDER_TOP_COLOR:
                    target._borderTopColor = _borderTopColor;
                    break;
                case BIT_BORDER_BOTTOM_COLOR:
                    target._borderBottomColor = _borderBottomColor;
                    break;
                case BIT_UNITY_TEXT_OUTLINE_COLOR:
                    target._unityTextOutlineColor = _unityTextOutlineColor;
                    break;
                case BIT_FLEX_DIRECTION:
                    target._flexDirection = _flexDirection;
                    break;
                case BIT_FLEX_WRAP:
                    target._flexWrap = _flexWrap;
                    break;
                case BIT_JUSTIFY_CONTENT:
                    target._justifyContent = _justifyContent;
                    break;
                case BIT_ALIGN_ITEMS:
                    target._alignItems = _alignItems;
                    break;
                case BIT_ALIGN_SELF:
                    target._alignSelf = _alignSelf;
                    break;
                case BIT_ALIGN_CONTENT:
                    target._alignContent = _alignContent;
                    break;
                case BIT_POSITION:
                    target._position = _position;
                    break;
                case BIT_DISPLAY:
                    target._display = _display;
                    break;
                case BIT_VISIBILITY:
                    target._visibility = _visibility;
                    break;
                case BIT_OVERFLOW:
                    target._overflow = _overflow;
                    break;
                case BIT_WHITE_SPACE:
                    target._whiteSpace = _whiteSpace;
                    break;
                case BIT_TEXT_ALIGN:
                    target._textAlign = _textAlign;
                    break;
                case BIT_TEXT_OVERFLOW:
                    target._textOverflow = _textOverflow;
                    break;
                case BIT_UNITY_FONT_STYLE:
                    target._unityFontStyle = _unityFontStyle;
                    break;
                case BIT_UNITY_TEXT_OVERFLOW_POS:
                    target._unityTextOverflowPosition = _unityTextOverflowPosition;
                    break;
                case BIT_UNITY_TEXT_AUTO_SIZE:
                    target._unityTextAutoSize = _unityTextAutoSize;
                    break;
                case BIT_BACKGROUND_IMAGE:
                    target._backgroundImage = _backgroundImage;
                    break;
                case BIT_FONT_FAMILY:
                    target._fontFamily = _fontFamily;
                    break;
                case BIT_ROTATE:
                    target._rotate = _rotate;
                    break;
                case BIT_SCALE:
                    target._scale = _scale;
                    break;
                case BIT_TRANSLATE:
                    target._translate = _translate;
                    break;
                case BIT_BACKGROUND_REPEAT:
                    target._backgroundRepeat = _backgroundRepeat;
                    break;
                case BIT_BACKGROUND_POSITION_X:
                    target._backgroundPositionX = _backgroundPositionX;
                    break;
                case BIT_BACKGROUND_POSITION_Y:
                    target._backgroundPositionY = _backgroundPositionY;
                    break;
                case BIT_BACKGROUND_SIZE:
                    target._backgroundSize = _backgroundSize;
                    break;
                case BIT_TRANSFORM_ORIGIN:
                    target._transformOrigin = _transformOrigin;
                    break;
                case BIT_TRANSITION_DELAY:
                    target._transitionDelay = _transitionDelay;
                    break;
                case BIT_TRANSITION_DURATION:
                    target._transitionDuration = _transitionDuration;
                    break;
                case BIT_TRANSITION_PROPERTY:
                    target._transitionProperty = _transitionProperty;
                    break;
                case BIT_TRANSITION_TIMING_FUNC:
                    target._transitionTimingFunction = _transitionTimingFunction;
                    break;
                // Shorthand bits — no separate field
                case BIT_MARGIN:
                    break;
                case BIT_PADDING:
                    break;
                case BIT_BORDER_RADIUS:
                    break;
                case BIT_BORDER_WIDTH:
                    break;
                case BIT_BORDER_COLOR:
                    break;
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        //  SetByKey / GetByKey — string-keyed access for backward compat
        //  (used by tuple initializer path and IDictionary interface)
        // ═══════════════════════════════════════════════════════════════════

        internal void SetByKey(string key, object value)
        {
            switch (key)
            {
                // StyleLength
                case "width":
                    Width = PropsApplier.ConvertToStyleLength(value);
                    break;
                case "height":
                    Height = PropsApplier.ConvertToStyleLength(value);
                    break;
                case "minWidth":
                    MinWidth = PropsApplier.ConvertToStyleLength(value);
                    break;
                case "minHeight":
                    MinHeight = PropsApplier.ConvertToStyleLength(value);
                    break;
                case "maxWidth":
                    MaxWidth = PropsApplier.ConvertToStyleLength(value);
                    break;
                case "maxHeight":
                    MaxHeight = PropsApplier.ConvertToStyleLength(value);
                    break;
                case "flexBasis":
                    FlexBasis = PropsApplier.ConvertToStyleLength(value);
                    break;
                case "left":
                    Left = PropsApplier.ConvertToStyleLength(value);
                    break;
                case "top":
                    Top = PropsApplier.ConvertToStyleLength(value);
                    break;
                case "right":
                    Right = PropsApplier.ConvertToStyleLength(value);
                    break;
                case "bottom":
                    Bottom = PropsApplier.ConvertToStyleLength(value);
                    break;
                case "margin":
                    Margin = PropsApplier.ConvertToStyleLength(value);
                    break;
                case "marginLeft":
                    MarginLeft = PropsApplier.ConvertToStyleLength(value);
                    break;
                case "marginRight":
                    MarginRight = PropsApplier.ConvertToStyleLength(value);
                    break;
                case "marginTop":
                    MarginTop = PropsApplier.ConvertToStyleLength(value);
                    break;
                case "marginBottom":
                    MarginBottom = PropsApplier.ConvertToStyleLength(value);
                    break;
                case "padding":
                    Padding = PropsApplier.ConvertToStyleLength(value);
                    break;
                case "paddingLeft":
                    PaddingLeft = PropsApplier.ConvertToStyleLength(value);
                    break;
                case "paddingRight":
                    PaddingRight = PropsApplier.ConvertToStyleLength(value);
                    break;
                case "paddingTop":
                    PaddingTop = PropsApplier.ConvertToStyleLength(value);
                    break;
                case "paddingBottom":
                    PaddingBottom = PropsApplier.ConvertToStyleLength(value);
                    break;
                case "borderRadius":
                    BorderRadius = PropsApplier.ConvertToStyleLength(value);
                    break;
                case "borderTopLeftRadius":
                    BorderTopLeftRadius = PropsApplier.ConvertToStyleLength(value);
                    break;
                case "borderTopRightRadius":
                    BorderTopRightRadius = PropsApplier.ConvertToStyleLength(value);
                    break;
                case "borderBottomLeftRadius":
                    BorderBottomLeftRadius = PropsApplier.ConvertToStyleLength(value);
                    break;
                case "borderBottomRightRadius":
                    BorderBottomRightRadius = PropsApplier.ConvertToStyleLength(value);
                    break;
                case "fontSize":
                    FontSize = PropsApplier.ConvertToStyleLength(value);
                    break;
                case "letterSpacing":
                    LetterSpacing = PropsApplier.ConvertToStyleLength(value);
                    break;
                // StyleFloat
                case "flexGrow":
                    FlexGrow = PropsApplier.ConvertToStyleFloat(value);
                    break;
                case "flexShrink":
                    FlexShrink = PropsApplier.ConvertToStyleFloat(value);
                    break;
                case "opacity":
                    Opacity = PropsApplier.ConvertToStyleFloat(value);
                    break;
                case "borderWidth":
                    BorderWidth = PropsApplier.ConvertToStyleFloat(value);
                    break;
                case "borderLeftWidth":
                    BorderLeftWidth = PropsApplier.ConvertToStyleFloat(value);
                    break;
                case "borderRightWidth":
                    BorderRightWidth = PropsApplier.ConvertToStyleFloat(value);
                    break;
                case "borderTopWidth":
                    BorderTopWidth = PropsApplier.ConvertToStyleFloat(value);
                    break;
                case "borderBottomWidth":
                    BorderBottomWidth = PropsApplier.ConvertToStyleFloat(value);
                    break;
                case "unityTextOutlineWidth":
                    UnityTextOutlineWidth = PropsApplier.ConvertToStyleFloat(value);
                    break;
                // Color
                case "color":
                    Color = PropsApplier.ConvertToColor(value);
                    break;
                case "backgroundColor":
                    BackgroundColor = PropsApplier.ConvertToColor(value);
                    break;
                case "backgroundImageTint":
                    BackgroundImageTint = PropsApplier.ConvertToColor(value);
                    break;
                case "borderColor":
                    BorderColor = PropsApplier.ConvertToColor(value);
                    break;
                case "borderLeftColor":
                    BorderLeftColor = PropsApplier.ConvertToColor(value);
                    break;
                case "borderRightColor":
                    BorderRightColor = PropsApplier.ConvertToColor(value);
                    break;
                case "borderTopColor":
                    BorderTopColor = PropsApplier.ConvertToColor(value);
                    break;
                case "borderBottomColor":
                    BorderBottomColor = PropsApplier.ConvertToColor(value);
                    break;
                case "unityTextOutlineColor":
                    UnityTextOutlineColor = PropsApplier.ConvertToColor(value);
                    break;
                // Enums
                case "flexDirection":
                    FlexDirection = PropsApplier.ConvertToFlexDirection(value);
                    break;
                case "flexWrap":
                    FlexWrap = PropsApplier.ConvertToWrap(value);
                    break;
                case "justifyContent":
                    JustifyContent = PropsApplier.ConvertToJustify(value);
                    break;
                case "alignItems":
                    AlignItems = PropsApplier.ConvertToAlign(value);
                    break;
                case "alignSelf":
                    AlignSelf = PropsApplier.ConvertToAlign(value);
                    break;
                case "alignContent":
                    AlignContent = PropsApplier.ConvertToAlign(value);
                    break;
                case "position":
                    Position = PropsApplier.ConvertToPosition(value);
                    break;
                case "display":
                    Display = PropsApplier.ConvertToDisplayStyle(value);
                    break;
                case "visibility":
                    Visibility = PropsApplier.ConvertToVisibility(value);
                    break;
                case "overflow":
                    Overflow = PropsApplier.ConvertToOverflow(value);
                    break;
                case "whiteSpace":
                    WhiteSpace = PropsApplier.ConvertToWhiteSpace(value);
                    break;
                case "textAlign":
                    TextAlign = PropsApplier.ConvertToTextAnchor(value);
                    break;
                case "textOverflow":
                    TextOverflow = PropsApplier.ConvertToTextOverflow(value);
                    break;
                case "unityFontStyle":
                    UnityFontStyle = PropsApplier.ConvertToFontStyle(value);
                    break;
                case "unityTextOverflowPosition":
                    UnityTextOverflowPosition = PropsApplier.ConvertToTextOverflowPosition(value);
                    break;
                case "unityTextAutoSize":
                    UnityTextAutoSize = PropsApplier.ConvertToTextAutoSizeMode(value);
                    break;
                // Assets
                case "backgroundImage":
                    if (value is Texture2D tex)
                        BackgroundImage = tex;
                    break;
                case "fontFamily":
                    if (value is Font font)
                        FontFamily = font;
                    break;
                // float
                case "rotate":
                    Rotate = PropsApplier.ConvertToFloat(value);
                    break;
                case "scale":
                    Scale = PropsApplier.ConvertToFloat(value);
                    break;
                // Compound structs
                case "translate":
                    if (value is Translate t)
                        Translate = t;
                    break;
                case "backgroundRepeat":
                    if (value is BackgroundRepeat br)
                        BackgroundRepeat = br;
                    break;
                case "backgroundPositionX":
                    if (value is BackgroundPosition bpx)
                        BackgroundPositionX = bpx;
                    break;
                case "backgroundPositionY":
                    if (value is BackgroundPosition bpy)
                        BackgroundPositionY = bpy;
                    break;
                case "backgroundSize":
                    if (value is BackgroundSize bs)
                        BackgroundSize = bs;
                    break;
                case "transformOrigin":
                    if (value is TransformOrigin to)
                        TransformOrigin = to;
                    break;
                // Transitions
                case "transitionDelay":
                    if (value is StyleList<TimeValue> td)
                        TransitionDelay = td;
                    break;
                case "transitionDuration":
                    if (value is StyleList<TimeValue> tdu)
                        TransitionDuration = tdu;
                    break;
                case "transitionProperty":
                    if (value is StyleList<StylePropertyName> tp)
                        TransitionProperty = tp;
                    break;
                case "transitionTimingFunction":
                    if (value is StyleList<EasingFunction> ttf)
                        TransitionTimingFunction = ttf;
                    break;
#if UNITY_6000_3_OR_NEWER
                case "aspectRatio":
                    if (value is StyleRatio ar)
                        AspectRatio = ar;
                    break;
                case "filter":
                    if (value is StyleList<FilterFunction> fl)
                        Filter = fl;
                    break;
                case "unityMaterial":
                    if (value is StyleMaterialDefinition smd)
                        UnityMaterial = smd;
                    break;
#endif
                // Unknown keys silently ignored (matches current behavior)
            }
        }

        internal object GetByKey(string key)
        {
            switch (key)
            {
                case "width":
                    return HasBit(BIT_WIDTH) ? (object)_width : null;
                case "height":
                    return HasBit(BIT_HEIGHT) ? (object)_height : null;
                case "minWidth":
                    return HasBit(BIT_MIN_WIDTH) ? (object)_minWidth : null;
                case "minHeight":
                    return HasBit(BIT_MIN_HEIGHT) ? (object)_minHeight : null;
                case "maxWidth":
                    return HasBit(BIT_MAX_WIDTH) ? (object)_maxWidth : null;
                case "maxHeight":
                    return HasBit(BIT_MAX_HEIGHT) ? (object)_maxHeight : null;
                case "flexBasis":
                    return HasBit(BIT_FLEX_BASIS) ? (object)_flexBasis : null;
                case "left":
                    return HasBit(BIT_LEFT) ? (object)_left : null;
                case "top":
                    return HasBit(BIT_TOP) ? (object)_top : null;
                case "right":
                    return HasBit(BIT_RIGHT) ? (object)_right : null;
                case "bottom":
                    return HasBit(BIT_BOTTOM) ? (object)_bottom : null;
                case "marginLeft":
                    return HasBit(BIT_MARGIN_LEFT) ? (object)_marginLeft : null;
                case "marginRight":
                    return HasBit(BIT_MARGIN_RIGHT) ? (object)_marginRight : null;
                case "marginTop":
                    return HasBit(BIT_MARGIN_TOP) ? (object)_marginTop : null;
                case "marginBottom":
                    return HasBit(BIT_MARGIN_BOTTOM) ? (object)_marginBottom : null;
                case "paddingLeft":
                    return HasBit(BIT_PADDING_LEFT) ? (object)_paddingLeft : null;
                case "paddingRight":
                    return HasBit(BIT_PADDING_RIGHT) ? (object)_paddingRight : null;
                case "paddingTop":
                    return HasBit(BIT_PADDING_TOP) ? (object)_paddingTop : null;
                case "paddingBottom":
                    return HasBit(BIT_PADDING_BOTTOM) ? (object)_paddingBottom : null;
                case "borderTopLeftRadius":
                    return HasBit(BIT_BORDER_TOP_LEFT_RADIUS) ? (object)_borderTopLeftRadius : null;
                case "borderTopRightRadius":
                    return HasBit(BIT_BORDER_TOP_RIGHT_RADIUS)
                        ? (object)_borderTopRightRadius
                        : null;
                case "borderBottomLeftRadius":
                    return HasBit(BIT_BORDER_BOTTOM_LEFT_RADIUS)
                        ? (object)_borderBottomLeftRadius
                        : null;
                case "borderBottomRightRadius":
                    return HasBit(BIT_BORDER_BOTTOM_RIGHT_RADIUS)
                        ? (object)_borderBottomRightRadius
                        : null;
                case "fontSize":
                    return HasBit(BIT_FONT_SIZE) ? (object)_fontSize : null;
                case "letterSpacing":
                    return HasBit(BIT_LETTER_SPACING) ? (object)_letterSpacing : null;
                case "flexGrow":
                    return HasBit(BIT_FLEX_GROW) ? (object)_flexGrow : null;
                case "flexShrink":
                    return HasBit(BIT_FLEX_SHRINK) ? (object)_flexShrink : null;
                case "opacity":
                    return HasBit(BIT_OPACITY) ? (object)_opacity : null;
                case "borderLeftWidth":
                    return HasBit(BIT_BORDER_LEFT_WIDTH) ? (object)_borderLeftWidth : null;
                case "borderRightWidth":
                    return HasBit(BIT_BORDER_RIGHT_WIDTH) ? (object)_borderRightWidth : null;
                case "borderTopWidth":
                    return HasBit(BIT_BORDER_TOP_WIDTH) ? (object)_borderTopWidth : null;
                case "borderBottomWidth":
                    return HasBit(BIT_BORDER_BOTTOM_WIDTH) ? (object)_borderBottomWidth : null;
                case "unityTextOutlineWidth":
                    return HasBit(BIT_UNITY_TEXT_OUTLINE_WIDTH)
                        ? (object)_unityTextOutlineWidth
                        : null;
                case "color":
                    return HasBit(BIT_COLOR) ? (object)_color : null;
                case "backgroundColor":
                    return HasBit(BIT_BACKGROUND_COLOR) ? (object)_backgroundColor : null;
                case "backgroundImageTint":
                    return HasBit(BIT_BACKGROUND_IMAGE_TINT) ? (object)_backgroundImageTint : null;
                case "borderLeftColor":
                    return HasBit(BIT_BORDER_LEFT_COLOR) ? (object)_borderLeftColor : null;
                case "borderRightColor":
                    return HasBit(BIT_BORDER_RIGHT_COLOR) ? (object)_borderRightColor : null;
                case "borderTopColor":
                    return HasBit(BIT_BORDER_TOP_COLOR) ? (object)_borderTopColor : null;
                case "borderBottomColor":
                    return HasBit(BIT_BORDER_BOTTOM_COLOR) ? (object)_borderBottomColor : null;
                case "unityTextOutlineColor":
                    return HasBit(BIT_UNITY_TEXT_OUTLINE_COLOR)
                        ? (object)_unityTextOutlineColor
                        : null;
                case "flexDirection":
                    return HasBit(BIT_FLEX_DIRECTION) ? (object)_flexDirection : null;
                case "flexWrap":
                    return HasBit(BIT_FLEX_WRAP) ? (object)_flexWrap : null;
                case "justifyContent":
                    return HasBit(BIT_JUSTIFY_CONTENT) ? (object)_justifyContent : null;
                case "alignItems":
                    return HasBit(BIT_ALIGN_ITEMS) ? (object)_alignItems : null;
                case "alignSelf":
                    return HasBit(BIT_ALIGN_SELF) ? (object)_alignSelf : null;
                case "alignContent":
                    return HasBit(BIT_ALIGN_CONTENT) ? (object)_alignContent : null;
                case "position":
                    return HasBit(BIT_POSITION) ? (object)_position : null;
                case "display":
                    return HasBit(BIT_DISPLAY) ? (object)_display : null;
                case "visibility":
                    return HasBit(BIT_VISIBILITY) ? (object)_visibility : null;
                case "overflow":
                    return HasBit(BIT_OVERFLOW) ? (object)_overflow : null;
                case "whiteSpace":
                    return HasBit(BIT_WHITE_SPACE) ? (object)_whiteSpace : null;
                case "textAlign":
                    return HasBit(BIT_TEXT_ALIGN) ? (object)_textAlign : null;
                case "textOverflow":
                    return HasBit(BIT_TEXT_OVERFLOW) ? (object)_textOverflow : null;
                case "unityFontStyle":
                    return HasBit(BIT_UNITY_FONT_STYLE) ? (object)_unityFontStyle : null;
                case "unityTextOverflowPosition":
                    return HasBit(BIT_UNITY_TEXT_OVERFLOW_POS)
                        ? (object)_unityTextOverflowPosition
                        : null;
                case "unityTextAutoSize":
                    return HasBit(BIT_UNITY_TEXT_AUTO_SIZE) ? (object)_unityTextAutoSize : null;
                case "backgroundImage":
                    return HasBit(BIT_BACKGROUND_IMAGE) ? _backgroundImage : null;
                case "fontFamily":
                    return HasBit(BIT_FONT_FAMILY) ? _fontFamily : null;
                case "rotate":
                    return HasBit(BIT_ROTATE) ? (object)_rotate : null;
                case "scale":
                    return HasBit(BIT_SCALE) ? (object)_scale : null;
                case "translate":
                    return HasBit(BIT_TRANSLATE) ? (object)_translate : null;
                case "backgroundRepeat":
                    return HasBit(BIT_BACKGROUND_REPEAT) ? (object)_backgroundRepeat : null;
                case "backgroundPositionX":
                    return HasBit(BIT_BACKGROUND_POSITION_X) ? (object)_backgroundPositionX : null;
                case "backgroundPositionY":
                    return HasBit(BIT_BACKGROUND_POSITION_Y) ? (object)_backgroundPositionY : null;
                case "backgroundSize":
                    return HasBit(BIT_BACKGROUND_SIZE) ? (object)_backgroundSize : null;
                case "transformOrigin":
                    return HasBit(BIT_TRANSFORM_ORIGIN) ? (object)_transformOrigin : null;
                case "transitionDelay":
                    return HasBit(BIT_TRANSITION_DELAY) ? (object)_transitionDelay : null;
                case "transitionDuration":
                    return HasBit(BIT_TRANSITION_DURATION) ? (object)_transitionDuration : null;
                case "transitionProperty":
                    return HasBit(BIT_TRANSITION_PROPERTY) ? (object)_transitionProperty : null;
                case "transitionTimingFunction":
                    return HasBit(BIT_TRANSITION_TIMING_FUNC)
                        ? (object)_transitionTimingFunction
                        : null;
                default:
                    return null;
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        //  Bitmask helpers
        // ═══════════════════════════════════════════════════════════════════

        internal bool HasBit(int bit)
        {
            if (bit < 64)
                return (_setBits0 & (1UL << bit)) != 0;
            return (_setBits1 & (1UL << (bit - 64))) != 0;
        }

        private bool HasBitByKey(string key)
        {
            int bit = KeyToBit(key);
            return bit >= 0 && HasBit(bit);
        }

        private void ClearBit(int bit)
        {
            if (bit < 64)
                _setBits0 &= ~(1UL << bit);
            else
                _setBits1 &= ~(1UL << (bit - 64));
        }

        internal static int KeyToBit(string key)
        {
            switch (key)
            {
                case "width":
                    return BIT_WIDTH;
                case "height":
                    return BIT_HEIGHT;
                case "minWidth":
                    return BIT_MIN_WIDTH;
                case "minHeight":
                    return BIT_MIN_HEIGHT;
                case "maxWidth":
                    return BIT_MAX_WIDTH;
                case "maxHeight":
                    return BIT_MAX_HEIGHT;
                case "flexGrow":
                    return BIT_FLEX_GROW;
                case "flexShrink":
                    return BIT_FLEX_SHRINK;
                case "flexBasis":
                    return BIT_FLEX_BASIS;
                case "flexDirection":
                    return BIT_FLEX_DIRECTION;
                case "flexWrap":
                    return BIT_FLEX_WRAP;
                case "justifyContent":
                    return BIT_JUSTIFY_CONTENT;
                case "alignItems":
                    return BIT_ALIGN_ITEMS;
                case "alignSelf":
                    return BIT_ALIGN_SELF;
                case "alignContent":
                    return BIT_ALIGN_CONTENT;
                case "position":
                    return BIT_POSITION;
                case "left":
                    return BIT_LEFT;
                case "top":
                    return BIT_TOP;
                case "right":
                    return BIT_RIGHT;
                case "bottom":
                    return BIT_BOTTOM;
                case "display":
                    return BIT_DISPLAY;
                case "visibility":
                    return BIT_VISIBILITY;
                case "overflow":
                    return BIT_OVERFLOW;
                case "opacity":
                    return BIT_OPACITY;
                case "margin":
                    return BIT_MARGIN;
                case "marginLeft":
                    return BIT_MARGIN_LEFT;
                case "marginRight":
                    return BIT_MARGIN_RIGHT;
                case "marginTop":
                    return BIT_MARGIN_TOP;
                case "marginBottom":
                    return BIT_MARGIN_BOTTOM;
                case "padding":
                    return BIT_PADDING;
                case "paddingLeft":
                    return BIT_PADDING_LEFT;
                case "paddingRight":
                    return BIT_PADDING_RIGHT;
                case "paddingTop":
                    return BIT_PADDING_TOP;
                case "paddingBottom":
                    return BIT_PADDING_BOTTOM;
                case "fontSize":
                    return BIT_FONT_SIZE;
                case "fontFamily":
                    return BIT_FONT_FAMILY;
                case "textAlign":
                    return BIT_TEXT_ALIGN;
                case "color":
                    return BIT_COLOR;
                case "backgroundColor":
                    return BIT_BACKGROUND_COLOR;
                case "backgroundImage":
                    return BIT_BACKGROUND_IMAGE;
                case "backgroundImageTint":
                    return BIT_BACKGROUND_IMAGE_TINT;
                case "whiteSpace":
                    return BIT_WHITE_SPACE;
                case "letterSpacing":
                    return BIT_LETTER_SPACING;
                case "textOverflow":
                    return BIT_TEXT_OVERFLOW;
                case "unityFontStyle":
                    return BIT_UNITY_FONT_STYLE;
                case "unityTextOutlineColor":
                    return BIT_UNITY_TEXT_OUTLINE_COLOR;
                case "unityTextOutlineWidth":
                    return BIT_UNITY_TEXT_OUTLINE_WIDTH;
                case "unityTextOverflowPosition":
                    return BIT_UNITY_TEXT_OVERFLOW_POS;
                case "unityTextAutoSize":
                    return BIT_UNITY_TEXT_AUTO_SIZE;
                case "borderWidth":
                    return BIT_BORDER_WIDTH;
                case "borderColor":
                    return BIT_BORDER_COLOR;
                case "borderLeftWidth":
                    return BIT_BORDER_LEFT_WIDTH;
                case "borderRightWidth":
                    return BIT_BORDER_RIGHT_WIDTH;
                case "borderTopWidth":
                    return BIT_BORDER_TOP_WIDTH;
                case "borderBottomWidth":
                    return BIT_BORDER_BOTTOM_WIDTH;
                case "borderLeftColor":
                    return BIT_BORDER_LEFT_COLOR;
                case "borderRightColor":
                    return BIT_BORDER_RIGHT_COLOR;
                case "borderTopColor":
                    return BIT_BORDER_TOP_COLOR;
                case "borderBottomColor":
                    return BIT_BORDER_BOTTOM_COLOR;
                case "borderRadius":
                    return BIT_BORDER_RADIUS;
                case "borderTopLeftRadius":
                    return BIT_BORDER_TOP_LEFT_RADIUS;
                case "borderTopRightRadius":
                    return BIT_BORDER_TOP_RIGHT_RADIUS;
                case "borderBottomLeftRadius":
                    return BIT_BORDER_BOTTOM_LEFT_RADIUS;
                case "borderBottomRightRadius":
                    return BIT_BORDER_BOTTOM_RIGHT_RADIUS;
                case "rotate":
                    return BIT_ROTATE;
                case "scale":
                    return BIT_SCALE;
                case "translate":
                    return BIT_TRANSLATE;
                case "backgroundRepeat":
                    return BIT_BACKGROUND_REPEAT;
                case "backgroundPositionX":
                    return BIT_BACKGROUND_POSITION_X;
                case "backgroundPositionY":
                    return BIT_BACKGROUND_POSITION_Y;
                case "backgroundSize":
                    return BIT_BACKGROUND_SIZE;
                case "transformOrigin":
                    return BIT_TRANSFORM_ORIGIN;
                case "transitionDelay":
                    return BIT_TRANSITION_DELAY;
                case "transitionDuration":
                    return BIT_TRANSITION_DURATION;
                case "transitionProperty":
                    return BIT_TRANSITION_PROPERTY;
                case "transitionTimingFunction":
                    return BIT_TRANSITION_TIMING_FUNC;
                case "aspectRatio":
                    return BIT_ASPECT_RATIO;
                case "filter":
                    return BIT_FILTER;
                case "unityMaterial":
                    return BIT_UNITY_MATERIAL;
                default:
                    return -1;
            }
        }

        // Key → string mapping for enumeration
        private static readonly string[] s_bitToKey = new string[]
        {
            "width",
            "height",
            "minWidth",
            "minHeight",
            "maxWidth",
            "maxHeight",
            "flexGrow",
            "flexShrink",
            "flexBasis",
            "flexDirection",
            "flexWrap",
            "justifyContent",
            "alignItems",
            "alignSelf",
            "alignContent",
            "position",
            "left",
            "top",
            "right",
            "bottom",
            "display",
            "visibility",
            "overflow",
            "opacity",
            "margin",
            "marginLeft",
            "marginRight",
            "marginTop",
            "marginBottom",
            "padding",
            "paddingLeft",
            "paddingRight",
            "paddingTop",
            "paddingBottom",
            "fontSize",
            "fontFamily",
            "textAlign",
            "color",
            "backgroundColor",
            "backgroundImage",
            "backgroundImageTint",
            "whiteSpace",
            "letterSpacing",
            "textOverflow",
            "unityFontStyle",
            "unityTextOutlineColor",
            "unityTextOutlineWidth",
            "unityTextOverflowPosition",
            "unityTextAutoSize",
            "borderWidth",
            "borderColor",
            "borderLeftWidth",
            "borderRightWidth",
            "borderTopWidth",
            "borderBottomWidth",
            "borderLeftColor",
            "borderRightColor",
            "borderTopColor",
            "borderBottomColor",
            "borderRadius",
            "borderTopLeftRadius",
            "borderTopRightRadius",
            "borderBottomLeftRadius",
            "borderBottomRightRadius",
            // _setBits1 offset 64
            "rotate",
            "scale",
            "translate",
            "backgroundRepeat",
            "backgroundPositionX",
            "backgroundPositionY",
            "backgroundSize",
            "transformOrigin",
            "transitionDelay",
            "transitionDuration",
            "transitionProperty",
            "transitionTimingFunction",
            "aspectRatio",
            "filter",
            "unityMaterial",
        };

        private static string BitToKey(int bit)
        {
            if (bit >= 0 && bit < s_bitToKey.Length)
                return s_bitToKey[bit];
            return null;
        }

        // ═══════════════════════════════════════════════════════════════════
        //  IDictionary<string, object> implementation (backward compat)
        // ═══════════════════════════════════════════════════════════════════

        public object this[string key]
        {
            get => GetByKey(key);
            set => SetByKey(key, value);
        }

        public ICollection<string> Keys
        {
            get
            {
                var list = new List<string>();
                EnumerateSetBits(
                    (bit) =>
                    {
                        var k = BitToKey(bit);
                        if (k != null)
                            list.Add(k);
                    }
                );
                return list;
            }
        }

        public ICollection<object> Values
        {
            get
            {
                var list = new List<object>();
                EnumerateSetBits(
                    (bit) =>
                    {
                        var k = BitToKey(bit);
                        if (k != null)
                            list.Add(GetByKey(k));
                    }
                );
                return list;
            }
        }

        public int Count
        {
            get
            {
                // Exclude shorthand-only tracking bits from count — they don't represent
                // separate properties, their individual sides are already counted.
                ulong mask0 =
                    _setBits0
                    & ~(
                        (1UL << BIT_MARGIN)
                        | (1UL << BIT_PADDING)
                        | (1UL << BIT_BORDER_RADIUS)
                        | (1UL << BIT_BORDER_WIDTH)
                        | (1UL << BIT_BORDER_COLOR)
                    );
                return BitOps.PopCount(mask0) + BitOps.PopCount(_setBits1);
            }
        }

        public bool IsReadOnly => false;

        public void Add(string key, object value) => SetByKey(key, value);

        public void Add(KeyValuePair<string, object> item) => SetByKey(item.Key, item.Value);

        public bool ContainsKey(string key) => HasBitByKey(key);

        public bool Contains(KeyValuePair<string, object> item) => HasBitByKey(item.Key);

        public bool Remove(string key)
        {
            int bit = KeyToBit(key);
            if (bit < 0 || !HasBit(bit))
                return false;
            ClearBit(bit);
            return true;
        }

        public bool Remove(KeyValuePair<string, object> item) => Remove(item.Key);

        public bool TryGetValue(string key, out object value)
        {
            if (HasBitByKey(key))
            {
                value = GetByKey(key);
                return true;
            }
            value = null;
            return false;
        }

        public void Clear()
        {
            _setBits0 = 0;
            _setBits1 = 0;
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            foreach (var kv in this)
                array[arrayIndex++] = kv;
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            // Yield only set properties — skips shorthand tracking bits
            ulong bits = _setBits0;
            while (bits != 0)
            {
                int bit = BitOps.TrailingZeroCount(bits);
                // Skip shorthand-only tracking bits
                if (
                    bit != BIT_MARGIN
                    && bit != BIT_PADDING
                    && bit != BIT_BORDER_RADIUS
                    && bit != BIT_BORDER_WIDTH
                    && bit != BIT_BORDER_COLOR
                )
                {
                    var key = BitToKey(bit);
                    if (key != null)
                        yield return new KeyValuePair<string, object>(key, GetByKey(key));
                }
                bits &= bits - 1;
            }
            bits = _setBits1;
            while (bits != 0)
            {
                int bit = BitOps.TrailingZeroCount(bits);
                var key = BitToKey(64 + bit);
                if (key != null)
                    yield return new KeyValuePair<string, object>(key, GetByKey(key));
                bits &= bits - 1;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private void EnumerateSetBits(Action<int> action)
        {
            ulong bits = _setBits0;
            while (bits != 0)
            {
                int bit = BitOps.TrailingZeroCount(bits);
                if (
                    bit != BIT_MARGIN
                    && bit != BIT_PADDING
                    && bit != BIT_BORDER_RADIUS
                    && bit != BIT_BORDER_WIDTH
                    && bit != BIT_BORDER_COLOR
                )
                    action(bit);
                bits &= bits - 1;
            }
            bits = _setBits1;
            while (bits != 0)
            {
                int bit = BitOps.TrailingZeroCount(bits);
                action(64 + bit);
                bits &= bits - 1;
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        //  Portable bit operations (no System.Numerics.BitOperations)
        // ═══════════════════════════════════════════════════════════════════

        internal static class BitOps
        {
            public static int TrailingZeroCount(ulong value)
            {
                if (value == 0)
                    return 64;
                int count = 0;
                while ((value & 1UL) == 0)
                {
                    value >>= 1;
                    count++;
                }
                return count;
            }

            public static int PopCount(ulong value)
            {
                int count = 0;
                while (value != 0)
                {
                    value &= value - 1;
                    count++;
                }
                return count;
            }
        }
    }
}
