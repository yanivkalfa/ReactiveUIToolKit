using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    /// <summary>
    /// Base class for all Props that wrap a VisualElement.
    /// Provides the common set of properties that every VisualElement supports.
    /// </summary>
    public abstract class BaseProps : global::ReactiveUITK.Core.IProps
    {
        // ═══════════════════════════════════════════════════════════════════
        //  Pool generation stamp
        //  0 = user-created via new — never pooled
        //  >0 = rented from pool via __Rent<T>()
        // ═══════════════════════════════════════════════════════════════════
        internal uint _generation;

        // Idempotent return guard: true when this instance is currently in the
        // s_pendingReturn list waiting to be moved to the pool. Prevents the
        // same instance from being scheduled twice in one flush window
        // (which would push it into the pool twice and let two future Rents
        // hand out the same instance — the cross-wired "disco" style bug).
        internal bool _isPendingReturn;

        // --- Identity / structure ---
        public string Name { get; set; }
        public string ClassName { get; set; }
        public Style Style { get; set; }
        public object Ref { get; set; }
        public Dictionary<string, object> ContentContainer { get; set; }

        // --- Visibility / enabled ---
        public bool? Visible { get; set; }
        public bool? Enabled { get; set; }

        // --- Tooltip / persistence ---
        public string Tooltip { get; set; }
        public string ViewDataKey { get; set; }

        // --- Focus / interaction ---
        public PickingMode? PickingMode { get; set; }
        public bool? Focusable { get; set; }
        public int? TabIndex { get; set; }
        public bool? DelegatesFocus { get; set; }

        // --- Locale ---
        public LanguageDirection? LanguageDirection { get; set; }

        // ═══════════════════════════════════════════════════════════════════
        //  Event handler fields + fast "has any event?" flag
        //
        //  _hasEvents is set to true by any event property setter when a
        //  non-null handler is assigned. It is monotonic: once true, it stays
        //  true until pool reset. This allows TypedPropsApplier.ApplyDiff to
        //  skip the entire 43-call DiffEvent block when both prev and next
        //  have _hasEvents == false (meaning all event fields are null).
        // ═══════════════════════════════════════════════════════════════════
        internal bool _hasEvents;

        // --- Pointer event backing fields ---
        private PointerEventHandler _onClick;
        private PointerEventHandler _onClickCapture;
        private PointerEventHandler _onPointerDown;
        private PointerEventHandler _onPointerDownCapture;
        private PointerEventHandler _onPointerUp;
        private PointerEventHandler _onPointerUpCapture;
        private PointerEventHandler _onPointerMove;
        private PointerEventHandler _onPointerMoveCapture;
        private PointerEventHandler _onPointerEnter;
        private PointerEventHandler _onPointerEnterCapture;
        private PointerEventHandler _onPointerLeave;
        private PointerEventHandler _onPointerLeaveCapture;
        private WheelEventHandler _onWheel;
        private WheelEventHandler _onWheelCapture;
        private WheelEventHandler _onScroll;
        private WheelEventHandler _onScrollCapture;

        // --- Drag event backing fields (editor-only) ---
#if UNITY_EDITOR
        private DragEventHandler _onDragEnter;
        private DragEventHandler _onDragEnterCapture;
        private DragEventHandler _onDragLeave;
        private DragEventHandler _onDragLeaveCapture;
        private DragEventHandler _onDragUpdated;
        private DragEventHandler _onDragUpdatedCapture;
        private DragEventHandler _onDragPerform;
        private DragEventHandler _onDragPerformCapture;
        private DragEventHandler _onDragExited;
        private DragEventHandler _onDragExitedCapture;
#endif

        // --- Focus event backing fields ---
        private FocusEventHandler _onFocus;
        private FocusEventHandler _onFocusCapture;
        private FocusEventHandler _onBlur;
        private FocusEventHandler _onBlurCapture;
        private FocusEventHandler _onFocusIn;
        private FocusEventHandler _onFocusInCapture;
        private FocusEventHandler _onFocusOut;
        private FocusEventHandler _onFocusOutCapture;

        // --- Keyboard event backing fields ---
        private KeyboardEventHandler _onKeyDown;
        private KeyboardEventHandler _onKeyDownCapture;
        private KeyboardEventHandler _onKeyUp;
        private KeyboardEventHandler _onKeyUpCapture;

        // --- Input event backing fields ---
        private InputEventHandler _onInput;
        private InputEventHandler _onInputCapture;

        // --- Lifecycle event backing fields ---
        private GeometryChangedEventHandler _onGeometryChanged;
        private PanelLifecycleEventHandler _onAttachToPanel;
        private PanelLifecycleEventHandler _onDetachFromPanel;

        // --- Pointer events ---
        public PointerEventHandler OnClick
        {
            get => _onClick;
            set
            {
                _onClick = value;
                if (value != null)
                    _hasEvents = true;
            }
        }
        public PointerEventHandler OnClickCapture
        {
            get => _onClickCapture;
            set
            {
                _onClickCapture = value;
                if (value != null)
                    _hasEvents = true;
            }
        }
        public PointerEventHandler OnPointerDown
        {
            get => _onPointerDown;
            set
            {
                _onPointerDown = value;
                if (value != null)
                    _hasEvents = true;
            }
        }
        public PointerEventHandler OnPointerDownCapture
        {
            get => _onPointerDownCapture;
            set
            {
                _onPointerDownCapture = value;
                if (value != null)
                    _hasEvents = true;
            }
        }
        public PointerEventHandler OnPointerUp
        {
            get => _onPointerUp;
            set
            {
                _onPointerUp = value;
                if (value != null)
                    _hasEvents = true;
            }
        }
        public PointerEventHandler OnPointerUpCapture
        {
            get => _onPointerUpCapture;
            set
            {
                _onPointerUpCapture = value;
                if (value != null)
                    _hasEvents = true;
            }
        }
        public PointerEventHandler OnPointerMove
        {
            get => _onPointerMove;
            set
            {
                _onPointerMove = value;
                if (value != null)
                    _hasEvents = true;
            }
        }
        public PointerEventHandler OnPointerMoveCapture
        {
            get => _onPointerMoveCapture;
            set
            {
                _onPointerMoveCapture = value;
                if (value != null)
                    _hasEvents = true;
            }
        }
        public PointerEventHandler OnPointerEnter
        {
            get => _onPointerEnter;
            set
            {
                _onPointerEnter = value;
                if (value != null)
                    _hasEvents = true;
            }
        }
        public PointerEventHandler OnPointerEnterCapture
        {
            get => _onPointerEnterCapture;
            set
            {
                _onPointerEnterCapture = value;
                if (value != null)
                    _hasEvents = true;
            }
        }
        public PointerEventHandler OnPointerLeave
        {
            get => _onPointerLeave;
            set
            {
                _onPointerLeave = value;
                if (value != null)
                    _hasEvents = true;
            }
        }
        public PointerEventHandler OnPointerLeaveCapture
        {
            get => _onPointerLeaveCapture;
            set
            {
                _onPointerLeaveCapture = value;
                if (value != null)
                    _hasEvents = true;
            }
        }
        public WheelEventHandler OnWheel
        {
            get => _onWheel;
            set
            {
                _onWheel = value;
                if (value != null)
                    _hasEvents = true;
            }
        }
        public WheelEventHandler OnWheelCapture
        {
            get => _onWheelCapture;
            set
            {
                _onWheelCapture = value;
                if (value != null)
                    _hasEvents = true;
            }
        }
        public WheelEventHandler OnScroll
        {
            get => _onScroll;
            set
            {
                _onScroll = value;
                if (value != null)
                    _hasEvents = true;
            }
        }
        public WheelEventHandler OnScrollCapture
        {
            get => _onScrollCapture;
            set
            {
                _onScrollCapture = value;
                if (value != null)
                    _hasEvents = true;
            }
        }

        // --- Drag events (editor-only) ---
#if UNITY_EDITOR
        public DragEventHandler OnDragEnter
        {
            get => _onDragEnter;
            set
            {
                _onDragEnter = value;
                if (value != null)
                    _hasEvents = true;
            }
        }
        public DragEventHandler OnDragEnterCapture
        {
            get => _onDragEnterCapture;
            set
            {
                _onDragEnterCapture = value;
                if (value != null)
                    _hasEvents = true;
            }
        }
        public DragEventHandler OnDragLeave
        {
            get => _onDragLeave;
            set
            {
                _onDragLeave = value;
                if (value != null)
                    _hasEvents = true;
            }
        }
        public DragEventHandler OnDragLeaveCapture
        {
            get => _onDragLeaveCapture;
            set
            {
                _onDragLeaveCapture = value;
                if (value != null)
                    _hasEvents = true;
            }
        }
        public DragEventHandler OnDragUpdated
        {
            get => _onDragUpdated;
            set
            {
                _onDragUpdated = value;
                if (value != null)
                    _hasEvents = true;
            }
        }
        public DragEventHandler OnDragUpdatedCapture
        {
            get => _onDragUpdatedCapture;
            set
            {
                _onDragUpdatedCapture = value;
                if (value != null)
                    _hasEvents = true;
            }
        }
        public DragEventHandler OnDragPerform
        {
            get => _onDragPerform;
            set
            {
                _onDragPerform = value;
                if (value != null)
                    _hasEvents = true;
            }
        }
        public DragEventHandler OnDragPerformCapture
        {
            get => _onDragPerformCapture;
            set
            {
                _onDragPerformCapture = value;
                if (value != null)
                    _hasEvents = true;
            }
        }
        public DragEventHandler OnDragExited
        {
            get => _onDragExited;
            set
            {
                _onDragExited = value;
                if (value != null)
                    _hasEvents = true;
            }
        }
        public DragEventHandler OnDragExitedCapture
        {
            get => _onDragExitedCapture;
            set
            {
                _onDragExitedCapture = value;
                if (value != null)
                    _hasEvents = true;
            }
        }
#endif

        // --- Focus events ---
        public FocusEventHandler OnFocus
        {
            get => _onFocus;
            set
            {
                _onFocus = value;
                if (value != null)
                    _hasEvents = true;
            }
        }
        public FocusEventHandler OnFocusCapture
        {
            get => _onFocusCapture;
            set
            {
                _onFocusCapture = value;
                if (value != null)
                    _hasEvents = true;
            }
        }
        public FocusEventHandler OnBlur
        {
            get => _onBlur;
            set
            {
                _onBlur = value;
                if (value != null)
                    _hasEvents = true;
            }
        }
        public FocusEventHandler OnBlurCapture
        {
            get => _onBlurCapture;
            set
            {
                _onBlurCapture = value;
                if (value != null)
                    _hasEvents = true;
            }
        }
        public FocusEventHandler OnFocusIn
        {
            get => _onFocusIn;
            set
            {
                _onFocusIn = value;
                if (value != null)
                    _hasEvents = true;
            }
        }
        public FocusEventHandler OnFocusInCapture
        {
            get => _onFocusInCapture;
            set
            {
                _onFocusInCapture = value;
                if (value != null)
                    _hasEvents = true;
            }
        }
        public FocusEventHandler OnFocusOut
        {
            get => _onFocusOut;
            set
            {
                _onFocusOut = value;
                if (value != null)
                    _hasEvents = true;
            }
        }
        public FocusEventHandler OnFocusOutCapture
        {
            get => _onFocusOutCapture;
            set
            {
                _onFocusOutCapture = value;
                if (value != null)
                    _hasEvents = true;
            }
        }

        // --- Keyboard events ---
        public KeyboardEventHandler OnKeyDown
        {
            get => _onKeyDown;
            set
            {
                _onKeyDown = value;
                if (value != null)
                    _hasEvents = true;
            }
        }
        public KeyboardEventHandler OnKeyDownCapture
        {
            get => _onKeyDownCapture;
            set
            {
                _onKeyDownCapture = value;
                if (value != null)
                    _hasEvents = true;
            }
        }
        public KeyboardEventHandler OnKeyUp
        {
            get => _onKeyUp;
            set
            {
                _onKeyUp = value;
                if (value != null)
                    _hasEvents = true;
            }
        }
        public KeyboardEventHandler OnKeyUpCapture
        {
            get => _onKeyUpCapture;
            set
            {
                _onKeyUpCapture = value;
                if (value != null)
                    _hasEvents = true;
            }
        }

        // --- Input event (fires on every keystroke) ---
        public InputEventHandler OnInput
        {
            get => _onInput;
            set
            {
                _onInput = value;
                if (value != null)
                    _hasEvents = true;
            }
        }
        public InputEventHandler OnInputCapture
        {
            get => _onInputCapture;
            set
            {
                _onInputCapture = value;
                if (value != null)
                    _hasEvents = true;
            }
        }

        // --- Lifecycle events (no capture — target-only) ---
        public GeometryChangedEventHandler OnGeometryChanged
        {
            get => _onGeometryChanged;
            set
            {
                _onGeometryChanged = value;
                if (value != null)
                    _hasEvents = true;
            }
        }
        public PanelLifecycleEventHandler OnAttachToPanel
        {
            get => _onAttachToPanel;
            set
            {
                _onAttachToPanel = value;
                if (value != null)
                    _hasEvents = true;
            }
        }
        public PanelLifecycleEventHandler OnDetachFromPanel
        {
            get => _onDetachFromPanel;
            set
            {
                _onDetachFromPanel = value;
                if (value != null)
                    _hasEvents = true;
            }
        }

        // --- Escape hatch for non-standard / custom prop keys ---
        /// <summary>
        /// Optional dictionary of arbitrary extra props to be merged into the final
        /// serialized dictionary. Use this for custom event types or non-standard keys
        /// that are not covered by the typed properties above.
        /// </summary>
        public Dictionary<string, object> ExtraProps { get; set; }

        /// <summary>
        /// Field-by-field equality check for host element bailout.
        /// Compares all BaseProps fields. Subclasses MUST override to compare
        /// their own fields and call <c>base.ShallowEquals(other)</c>.
        /// </summary>
        public virtual bool ShallowEquals(BaseProps other)
        {
            if (other == null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            if (GetType() != other.GetType())
                return false;

            // --- Identity / structure ---
            if (Name != other.Name)
                return false;
            if (ClassName != other.ClassName)
                return false;
            if (!Style.SameInstance(Style, other.Style) && !StyleEquals(Style, other.Style))
                return false;
            if (!ReferenceEquals(Ref, other.Ref))
                return false;
            if (!ReferenceEquals(ContentContainer, other.ContentContainer))
                return false;

            // --- Visibility / enabled ---
            if (Visible != other.Visible)
                return false;
            if (Enabled != other.Enabled)
                return false;

            // --- Tooltip / persistence ---
            if (Tooltip != other.Tooltip)
                return false;
            if (ViewDataKey != other.ViewDataKey)
                return false;

            // --- Focus / interaction ---
            if (PickingMode != other.PickingMode)
                return false;
            if (Focusable != other.Focusable)
                return false;
            if (TabIndex != other.TabIndex)
                return false;
            if (DelegatesFocus != other.DelegatesFocus)
                return false;

            // --- Locale ---
            if (LanguageDirection != other.LanguageDirection)
                return false;

            // --- Event handlers (skip all 43 comparisons when neither side has events) ---
            if (_hasEvents || other._hasEvents)
            {
                // --- Pointer events (reference equality) ---
                if (OnClick != other.OnClick)
                    return false;
                if (OnClickCapture != other.OnClickCapture)
                    return false;
                if (OnPointerDown != other.OnPointerDown)
                    return false;
                if (OnPointerDownCapture != other.OnPointerDownCapture)
                    return false;
                if (OnPointerUp != other.OnPointerUp)
                    return false;
                if (OnPointerUpCapture != other.OnPointerUpCapture)
                    return false;
                if (OnPointerMove != other.OnPointerMove)
                    return false;
                if (OnPointerMoveCapture != other.OnPointerMoveCapture)
                    return false;
                if (OnPointerEnter != other.OnPointerEnter)
                    return false;
                if (OnPointerEnterCapture != other.OnPointerEnterCapture)
                    return false;
                if (OnPointerLeave != other.OnPointerLeave)
                    return false;
                if (OnPointerLeaveCapture != other.OnPointerLeaveCapture)
                    return false;
                if (OnWheel != other.OnWheel)
                    return false;
                if (OnWheelCapture != other.OnWheelCapture)
                    return false;
                if (OnScroll != other.OnScroll)
                    return false;
                if (OnScrollCapture != other.OnScrollCapture)
                    return false;

                // --- Drag events (editor-only) ---
#if UNITY_EDITOR
                if (OnDragEnter != other.OnDragEnter)
                    return false;
                if (OnDragEnterCapture != other.OnDragEnterCapture)
                    return false;
                if (OnDragLeave != other.OnDragLeave)
                    return false;
                if (OnDragLeaveCapture != other.OnDragLeaveCapture)
                    return false;
                if (OnDragUpdated != other.OnDragUpdated)
                    return false;
                if (OnDragUpdatedCapture != other.OnDragUpdatedCapture)
                    return false;
                if (OnDragPerform != other.OnDragPerform)
                    return false;
                if (OnDragPerformCapture != other.OnDragPerformCapture)
                    return false;
                if (OnDragExited != other.OnDragExited)
                    return false;
                if (OnDragExitedCapture != other.OnDragExitedCapture)
                    return false;
#endif

                // --- Focus events ---
                if (OnFocus != other.OnFocus)
                    return false;
                if (OnFocusCapture != other.OnFocusCapture)
                    return false;
                if (OnBlur != other.OnBlur)
                    return false;
                if (OnBlurCapture != other.OnBlurCapture)
                    return false;
                if (OnFocusIn != other.OnFocusIn)
                    return false;
                if (OnFocusInCapture != other.OnFocusInCapture)
                    return false;
                if (OnFocusOut != other.OnFocusOut)
                    return false;
                if (OnFocusOutCapture != other.OnFocusOutCapture)
                    return false;

                // --- Keyboard events ---
                if (OnKeyDown != other.OnKeyDown)
                    return false;
                if (OnKeyDownCapture != other.OnKeyDownCapture)
                    return false;
                if (OnKeyUp != other.OnKeyUp)
                    return false;
                if (OnKeyUpCapture != other.OnKeyUpCapture)
                    return false;

                // --- Input event ---
                if (OnInput != other.OnInput)
                    return false;
                if (OnInputCapture != other.OnInputCapture)
                    return false;

                // --- Lifecycle events ---
                if (OnGeometryChanged != other.OnGeometryChanged)
                    return false;
                if (OnAttachToPanel != other.OnAttachToPanel)
                    return false;
                if (OnDetachFromPanel != other.OnDetachFromPanel)
                    return false;
            }

            // --- ExtraProps ---
            if (!ReferenceEquals(ExtraProps, other.ExtraProps))
                return false;

            return true;
        }

        private static bool StyleEquals(Style a, Style b)
        {
            if (a == null && b == null)
                return true;
            if (a == null || b == null)
                return false;
            return a.TypedEquals(b);
        }

        // ═══════════════════════════════════════════════════════════════════
        //  Object pool — rent/return lifecycle
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Called by pool to clear all derived-class fields to default.
        /// Subclasses MUST override and clear their own fields, then call
        /// <c>base.__ResetFields()</c>.
        /// </summary>
        internal virtual void __ResetFields() { }

        /// <summary>
        /// Clears all BaseProps fields to default values.
        /// Called internally by the pool before handing out a rented instance.
        /// </summary>
        private void __ResetBase()
        {
            Name = null;
            ClassName = null;
            Style = null;
            Ref = null;
            ContentContainer = null;
            Visible = null;
            Enabled = null;
            Tooltip = null;
            ViewDataKey = null;
            PickingMode = null;
            Focusable = null;
            TabIndex = null;
            DelegatesFocus = null;
            LanguageDirection = null;
            _hasEvents = false;
            _onClick = null;
            _onClickCapture = null;
            _onPointerDown = null;
            _onPointerDownCapture = null;
            _onPointerUp = null;
            _onPointerUpCapture = null;
            _onPointerMove = null;
            _onPointerMoveCapture = null;
            _onPointerEnter = null;
            _onPointerEnterCapture = null;
            _onPointerLeave = null;
            _onPointerLeaveCapture = null;
            _onWheel = null;
            _onWheelCapture = null;
            _onScroll = null;
            _onScrollCapture = null;
#if UNITY_EDITOR
            _onDragEnter = null;
            _onDragEnterCapture = null;
            _onDragLeave = null;
            _onDragLeaveCapture = null;
            _onDragUpdated = null;
            _onDragUpdatedCapture = null;
            _onDragPerform = null;
            _onDragPerformCapture = null;
            _onDragExited = null;
            _onDragExitedCapture = null;
#endif
            _onFocus = null;
            _onFocusCapture = null;
            _onBlur = null;
            _onBlurCapture = null;
            _onFocusIn = null;
            _onFocusInCapture = null;
            _onFocusOut = null;
            _onFocusOutCapture = null;
            _onKeyDown = null;
            _onKeyDownCapture = null;
            _onKeyUp = null;
            _onKeyUpCapture = null;
            _onInput = null;
            _onInputCapture = null;
            _onGeometryChanged = null;
            _onAttachToPanel = null;
            _onDetachFromPanel = null;
            ExtraProps = null;
        }

        /// <summary>
        /// Per-type pool with a fixed cap. Each concrete BaseProps subclass
        /// (e.g. LabelProps, ButtonProps) gets its own pool via the generic parameter.
        /// </summary>
        internal static class Pool<T>
            where T : BaseProps, new()
        {
            private const int Capacity = 4096;

            private static readonly Stack<T> s_pool = new Stack<T>(256);
            private static uint s_nextGeneration = 1;

            internal static T Rent()
            {
                T p;
                if (s_pool.Count > 0)
                {
                    p = s_pool.Pop();
                    // Only reset fields when reusing from pool — new objects
                    // are already zero-initialized by the CLR.
                    p.__ResetBase();
                    p.__ResetFields();
                }
                else
                {
                    p = new T();
                }
                uint gen = s_nextGeneration++;
                if (gen == 0)
                    gen = s_nextGeneration++; // skip 0 on overflow
                p._generation = gen;
                p._isPendingReturn = false; // safety: ensure rented instances are not flagged as pending
                return p;
            }

            internal static void Return(T p)
            {
                if (s_pool.Count < Capacity)
                    s_pool.Push(p);
            }
        }

        /// <summary>
        /// Convenience wrapper to rent a pooled Props instance.
        /// Only called by generated code.
        /// </summary>
        public static T __Rent<T>()
            where T : BaseProps, new()
        {
            return Pool<T>.Rent();
        }

        private static readonly List<BaseProps> s_pendingReturn = new List<BaseProps>(2048);

        /// <summary>
        /// Schedule a BaseProps for return to pool on next flush.
        /// Props with generation 0 (user-created) are ignored.
        /// Idempotent: a single instance can only sit in the pending-return
        /// list once per flush window. Subsequent calls are no-ops.
        /// </summary>
        internal static void __ScheduleReturn(BaseProps p)
        {
            if (p == null || p._generation == 0)
                return;
            if (p._isPendingReturn)
                return;
            p._isPendingReturn = true;
            s_pendingReturn.Add(p);
        }

        /// <summary>
        /// Move all pending returns into their type-specific pools.
        /// Called once per frame after the full commit tree walk.
        /// </summary>
        internal static void __FlushReturns()
        {
            for (int i = 0; i < s_pendingReturn.Count; i++)
            {
                var p = s_pendingReturn[i];
                p._isPendingReturn = false;
                __ReturnToTypedPool(p);
            }
            s_pendingReturn.Clear();
        }

        /// <summary>
        /// Dispatches a BaseProps instance to its concrete Pool&lt;T&gt;.Return().
        /// Subclasses override this to call Pool&lt;ConcreteType&gt;.Return(this).
        /// </summary>
        internal virtual void __ReturnToPool() { }

        private static void __ReturnToTypedPool(BaseProps p)
        {
            p.__ReturnToPool();
        }

        /// <summary>
        /// Identity check safe across pool cycles.
        /// Returns true only when both point to the same object with the same generation.
        /// </summary>
        internal static bool SameInstance(BaseProps a, BaseProps b)
        {
            if (a == null && b == null)
                return true;
            if (a == null || b == null)
                return false;
            return ReferenceEquals(a, b) && a._generation == b._generation;
        }

        public virtual Dictionary<string, object> ToDictionary()
        {
            var dict = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(Name))
                dict["name"] = Name;
            if (!string.IsNullOrEmpty(ClassName))
                dict["className"] = ClassName;
            if (Style != null)
                dict["style"] = Style;
            if (Ref != null)
                dict["ref"] = Ref;
            if (ContentContainer != null)
                dict["contentContainer"] = ContentContainer;
            if (Visible.HasValue)
                dict["visible"] = Visible.Value;
            if (Enabled.HasValue)
                dict["enabled"] = Enabled.Value;
            if (!string.IsNullOrEmpty(Tooltip))
                dict["tooltip"] = Tooltip;
            if (!string.IsNullOrEmpty(ViewDataKey))
                dict["viewDataKey"] = ViewDataKey;
            if (PickingMode.HasValue)
                dict["pickingMode"] = PickingMode.Value;
            if (Focusable.HasValue)
                dict["focusable"] = Focusable.Value;
            if (TabIndex.HasValue)
                dict["tabIndex"] = TabIndex.Value;
            if (DelegatesFocus.HasValue)
                dict["delegatesFocus"] = DelegatesFocus.Value;
            if (LanguageDirection.HasValue)
                dict["languageDirection"] = LanguageDirection.Value;
            if (OnClick != null)
                dict["onClick"] = OnClick;
            if (OnClickCapture != null)
                dict["onClickCapture"] = OnClickCapture;
            if (OnPointerDown != null)
                dict["onPointerDown"] = OnPointerDown;
            if (OnPointerDownCapture != null)
                dict["onPointerDownCapture"] = OnPointerDownCapture;
            if (OnPointerUp != null)
                dict["onPointerUp"] = OnPointerUp;
            if (OnPointerUpCapture != null)
                dict["onPointerUpCapture"] = OnPointerUpCapture;
            if (OnPointerMove != null)
                dict["onPointerMove"] = OnPointerMove;
            if (OnPointerMoveCapture != null)
                dict["onPointerMoveCapture"] = OnPointerMoveCapture;
            if (OnPointerEnter != null)
                dict["onPointerEnter"] = OnPointerEnter;
            if (OnPointerEnterCapture != null)
                dict["onPointerEnterCapture"] = OnPointerEnterCapture;
            if (OnPointerLeave != null)
                dict["onPointerLeave"] = OnPointerLeave;
            if (OnPointerLeaveCapture != null)
                dict["onPointerLeaveCapture"] = OnPointerLeaveCapture;
            if (OnWheel != null)
                dict["onWheel"] = OnWheel;
            if (OnWheelCapture != null)
                dict["onWheelCapture"] = OnWheelCapture;
            if (OnScroll != null)
                dict["onScroll"] = OnScroll;
            if (OnScrollCapture != null)
                dict["onScrollCapture"] = OnScrollCapture;
#if UNITY_EDITOR
            if (OnDragEnter != null)
                dict["onDragEnter"] = OnDragEnter;
            if (OnDragEnterCapture != null)
                dict["onDragEnterCapture"] = OnDragEnterCapture;
            if (OnDragLeave != null)
                dict["onDragLeave"] = OnDragLeave;
            if (OnDragLeaveCapture != null)
                dict["onDragLeaveCapture"] = OnDragLeaveCapture;
            if (OnDragUpdated != null)
                dict["onDragUpdated"] = OnDragUpdated;
            if (OnDragUpdatedCapture != null)
                dict["onDragUpdatedCapture"] = OnDragUpdatedCapture;
            if (OnDragPerform != null)
                dict["onDragPerform"] = OnDragPerform;
            if (OnDragPerformCapture != null)
                dict["onDragPerformCapture"] = OnDragPerformCapture;
            if (OnDragExited != null)
                dict["onDragExited"] = OnDragExited;
            if (OnDragExitedCapture != null)
                dict["onDragExitedCapture"] = OnDragExitedCapture;
#endif
            if (OnFocus != null)
                dict["onFocus"] = OnFocus;
            if (OnFocusCapture != null)
                dict["onFocusCapture"] = OnFocusCapture;
            if (OnBlur != null)
                dict["onBlur"] = OnBlur;
            if (OnBlurCapture != null)
                dict["onBlurCapture"] = OnBlurCapture;
            if (OnFocusIn != null)
                dict["onFocusIn"] = OnFocusIn;
            if (OnFocusInCapture != null)
                dict["onFocusInCapture"] = OnFocusInCapture;
            if (OnFocusOut != null)
                dict["onFocusOut"] = OnFocusOut;
            if (OnFocusOutCapture != null)
                dict["onFocusOutCapture"] = OnFocusOutCapture;
            if (OnKeyDown != null)
                dict["onKeyDown"] = OnKeyDown;
            if (OnKeyDownCapture != null)
                dict["onKeyDownCapture"] = OnKeyDownCapture;
            if (OnKeyUp != null)
                dict["onKeyUp"] = OnKeyUp;
            if (OnKeyUpCapture != null)
                dict["onKeyUpCapture"] = OnKeyUpCapture;
            if (OnInput != null)
                dict["onInput"] = OnInput;
            if (OnInputCapture != null)
                dict["onInputCapture"] = OnInputCapture;
            if (OnGeometryChanged != null)
                dict["onGeometryChanged"] = OnGeometryChanged;
            if (OnAttachToPanel != null)
                dict["onAttachToPanel"] = OnAttachToPanel;
            if (OnDetachFromPanel != null)
                dict["onDetachFromPanel"] = OnDetachFromPanel;
            if (ExtraProps != null)
            {
                foreach (var kv in ExtraProps)
                    dict[kv.Key] = kv.Value;
            }
            return dict;
        }
    }
}
