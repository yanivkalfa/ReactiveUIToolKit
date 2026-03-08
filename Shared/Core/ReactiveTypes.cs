// ReactiveTypes.cs
// ─────────────────────────────────────────────────────────────────────────────
// Single source-of-truth for all public delegate, event-data and utility types
// in ReactiveUITK.Core.
//
// Design rationale
// ────────────────
//  • Follows the React / DefinitelyTyped convention:
//      1. One generic base utility (UIEventHandler<E>) for library extensibility.
//      2. Named concrete delegates per EVENT CATEGORY, not per prop.
//         e.g. PointerEventHandler covers onClick, onPointerDown, onPointerUp …
//         exactly as React uses MouseEventHandler for all mouse-category props.
//  • Event data classes carry a "Reactive" prefix to avoid name collisions with
//    UnityEngine.UIElements types (WheelEvent, FocusEvent, etc.) that already
//    exist in that namespace.
//  • Ref<T> replaces Hooks.MutableRef<T> — matches React 19 ref-as-prop model.
//    .Current is the primary accessor (React parity); .Value is kept as an
//    [Obsolete] alias so existing code still compiles with a warning during
//    migration.
//  • Render-function delegates unify every "return a VirtualNode" slot regardless
//    of whether the call site is a list row, tree row, or column cell.
//
// Origins (unified from)
// ──────────────────────
//  Ref<T>                   ← Hooks.MutableRef<T>  (Shared/Core/Hooks.cs:26)
//  ReactiveEvent            ← SyntheticEvent        (Shared/Core/SyntheticEvents.cs:7)
//  ReactivePointerEvent     ← SyntheticPointerEvent (SyntheticEvents.cs:78)
//  ReactiveWheelEvent       ← SyntheticWheelEvent   (SyntheticEvents.cs:151)
//  ReactiveKeyboardEvent    ← SyntheticKeyboardEvent(SyntheticEvents.cs:163)
//  ReactiveFocusEvent       ← NEW (focus/blur carry no extra UIElements data)
//  ReactiveDragEvent        ← NEW (drag events carry no extra UIElements data)
//  ReactiveGeometryEvent    ← NEW (wraps oldRect/newRect from GeometryChangedEvent)
//  ReactivePanelEvent       ← NEW (wraps panel from Attach/DetachToPanelEvent)
//  PointerEventHandler      ← Action (onClick) + EventCallback<Pointer*Event>
//  WheelEventHandler        ← EventCallback<WheelEvent>
//  KeyboardEventHandler     ← EventCallback<KeyDownEvent/KeyUpEvent>
//  FocusEventHandler        ← EventCallback<FocusEvent/BlurEvent/FocusIn/FocusOut>
//  DragEventHandler         ← EventCallback<DragEnterEvent/LeaveEvent/…>
//  GeometryChangedEventHandler ← EventCallback<GeometryChangedEvent>
//  PanelLifecycleEventHandler ← EventCallback<AttachToPanelEvent/DetachFromPanelEvent>
//  ChangeEventHandler<T>    ← Action<ChangeEvent<T>> (all typed input OnChange props)
//  InputEventHandler        ← (was missing from BaseProps; PropsApplier had it)
//  RowRenderer              ← Func<int,object,VirtualNode> (Row/Cell in all list/tree/column)
//  ContentRenderer          ← Func<VirtualNode>            (TabDef.Content)
//  ItemFactory              ← Func<VisualElement>          (ListView.MakeItem)
//  ItemBinder               ← Action<VisualElement,int>    (ListView.BindItem/UnbindItem)
//  TreeExpansionEventHandler← Action<TreeViewExpansionChangedArgs>
//  TabIndexEventHandler     ← Action<int>                  (TabView.SelectedIndexChanged)
//  TabChangeEventHandler    ← Action<Tab>                  (TabView.ActiveTabChanged single)
//  TabChangedEventHandler   ← Action<Tab,Tab>              (TabView.ActiveTabChanged from+to)
//  ColumnSortEventHandler   ← Action<List<SortedColumnDef>>(MultiColumn.ColumnSortingChanged)
//  ColumnLayoutEventHandler ← Action<ColumnLayoutState>    (MultiColumn.ColumnLayoutChanged)
//  MenuBuilderHandler       ← Action<DropdownMenu>         (ToolbarMenu.PopulateMenu)
//  ErrorEventHandler        ← Action<Exception>            (ErrorBoundary.OnError)
// ─────────────────────────────────────────────────────────────────────────────

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Core
{
    // =========================================================================
    // Ref<T>
    // =========================================================================

    /// <summary>
    /// A mutable container whose <see cref="Current"/> value persists across
    /// renders without triggering a re-render when mutated.
    /// Replaces <c>Hooks.MutableRef&lt;T&gt;</c>.
    /// Obtain one with <c>Hooks.UseRef&lt;T&gt;()</c>.
    /// </summary>
    public sealed class Ref<T>
    {
        /// <summary>The current held value (React parity: <c>ref.current</c>).</summary>
        public T Current { get; set; }

        /// <summary>Alias for <see cref="Current"/>. Prefer <c>Current</c>.</summary>
        [Obsolete("Use Current instead. Value is kept only for migration compatibility.")]
        public T Value
        {
            get => Current;
            set => Current = value;
        }
    }

    // =========================================================================
    // Event data — base
    // =========================================================================

    /// <summary>
    /// Base class for all ReactiveUITK event data objects passed to event handlers.
    /// Wraps the underlying <see cref="UnityEngine.UIElements.EventBase"/> with a
    /// predictable, UIToolkit-agnostic surface.
    /// Origin: <c>SyntheticEvent</c>.
    /// </summary>
    public class ReactiveEvent
    {
        private readonly EventBase _nativeEvent;

        public ReactiveEvent(EventBase evt)
        {
            _nativeEvent = evt;
            Type = evt?.GetType().Name ?? string.Empty;
            Target = evt?.target as VisualElement;
            CurrentTarget = evt?.currentTarget as VisualElement;
            Timestamp = evt != null ? evt.timestamp : 0L;
        }

        /// <summary>UIElements event type name (e.g. "PointerDownEvent").</summary>
        public string Type { get; }

        /// <summary>The element that originally dispatched the event.</summary>
        public VisualElement Target { get; }

        /// <summary>The element whose handler is currently being invoked.</summary>
        public VisualElement CurrentTarget { get; internal set; }

        /// <summary>UIElements timestamp of the event.</summary>
        public long Timestamp { get; }

        public bool IsPropagationStopped { get; private set; }
        public bool IsDefaultPrevented { get; private set; }

        /// <summary>The underlying UIElements event (use only when you need UIElements-specfic API).</summary>
        public EventBase NativeEvent => _nativeEvent;

        public virtual void StopPropagation()
        {
            try
            {
                IsPropagationStopped = true;
                _nativeEvent?.StopPropagation();
            }
            catch { }
        }

        public virtual void PreventDefault()
        {
            IsDefaultPrevented = true;
        }

        /// <summary>
        /// Factory: creates the most-specific <see cref="ReactiveEvent"/> subclass
        /// for a given UIElements event.
        /// </summary>
        public static ReactiveEvent Create(EventBase evt)
        {
            if (evt == null)
                return null;

            if (evt is WheelEvent wheel)
                return new ReactiveWheelEvent(evt, wheel);

            if (evt is IPointerEvent pointer)
                return new ReactivePointerEvent(evt, pointer);

            if (evt is IMouseEvent mouse)
                return new ReactivePointerEvent(evt, mouse);

            if (evt is IKeyboardEvent keyboard)
                return new ReactiveKeyboardEvent(evt, keyboard);

            if (
                evt is FocusEvent
                || evt is BlurEvent
                || evt is FocusInEvent
                || evt is FocusOutEvent
            )
                return new ReactiveFocusEvent(evt);

            if (
                evt is DragEnterEvent
                || evt is DragLeaveEvent
                || evt is DragUpdatedEvent
                || evt is DragPerformEvent
                || evt is DragExitedEvent
            )
                return new ReactiveDragEvent(evt);

            if (evt is GeometryChangedEvent geo)
                return new ReactiveGeometryEvent(evt, geo);

            if (evt is AttachToPanelEvent || evt is DetachFromPanelEvent)
                return new ReactivePanelEvent(evt);

            return new ReactiveEvent(evt);
        }
    }

    // =========================================================================
    // Event data — pointer / mouse / wheel
    // =========================================================================

    /// <summary>
    /// Event data for pointer and mouse events: onClick, onPointerDown, onPointerUp,
    /// onPointerMove, onPointerEnter, onPointerLeave.
    /// Origin: <c>SyntheticPointerEvent</c>.
    /// </summary>
    public class ReactivePointerEvent : ReactiveEvent
    {
        internal ReactivePointerEvent(EventBase evt, IPointerEvent pointer)
            : base(evt)
        {
            if (pointer == null)
                return;
            PointerId = pointer.pointerId;
            Position = pointer.position;
            DeltaPosition = pointer.deltaPosition;
            Button = pointer.button;
            ClickCount = pointer.clickCount;
            AltKey = pointer.altKey;
            CtrlKey = pointer.ctrlKey;
            ShiftKey = pointer.shiftKey;
            CommandKey = pointer.commandKey;
            Pressure = pointer.pressure;
            TangentialPressure = pointer.tangentialPressure;
            AltitudeAngle = pointer.altitudeAngle;
            AzimuthAngle = pointer.azimuthAngle;
            Twist = pointer.twist;
            Radius = pointer.radius;
            RadiusVariance = pointer.radiusVariance;
        }

        internal ReactivePointerEvent(EventBase evt, IMouseEvent mouse)
            : base(evt)
        {
            if (mouse == null)
                return;
            Position = mouse.mousePosition;
            DeltaPosition = mouse.mouseDelta;
            Button = mouse.button;
            ClickCount = mouse.clickCount;
            AltKey = mouse.altKey;
            CtrlKey = mouse.ctrlKey;
            ShiftKey = mouse.shiftKey;
            CommandKey = mouse.commandKey;
        }

        public int PointerId { get; }
        public Vector2 Position { get; }
        public Vector2 DeltaPosition { get; }
        public int Button { get; }
        public int ClickCount { get; }
        public bool AltKey { get; }
        public bool CtrlKey { get; }
        public bool ShiftKey { get; }
        public bool CommandKey { get; }
        public float Pressure { get; }
        public float TangentialPressure { get; }
        public float AltitudeAngle { get; }
        public float AzimuthAngle { get; }
        public float Twist { get; }
        public Vector2 Radius { get; }
        public Vector2 RadiusVariance { get; }
    }

    /// <summary>
    /// Event data for scroll-wheel events: onWheel, onScroll.
    /// Origin: <c>SyntheticWheelEvent</c>.
    /// </summary>
    public sealed class ReactiveWheelEvent : ReactivePointerEvent
    {
        internal ReactiveWheelEvent(EventBase evt, WheelEvent wheel)
            : base(evt, wheel as IMouseEvent)
        {
            if (wheel != null)
                Delta = wheel.delta;
        }

        /// <summary>Scroll delta (x = horizontal, y = vertical, z = depth).</summary>
        public Vector3 Delta { get; }
    }

    // =========================================================================
    // Event data — keyboard
    // =========================================================================

    /// <summary>
    /// Event data for keyboard events: onKeyDown, onKeyUp.
    /// Origin: <c>SyntheticKeyboardEvent</c>.
    /// </summary>
    public sealed class ReactiveKeyboardEvent : ReactiveEvent
    {
        internal ReactiveKeyboardEvent(EventBase evt, IKeyboardEvent keyboard)
            : base(evt)
        {
            if (keyboard == null)
                return;
            KeyCode = keyboard.keyCode;
            Character = keyboard.character;
            AltKey = keyboard.altKey;
            CtrlKey = keyboard.ctrlKey;
            ShiftKey = keyboard.shiftKey;
            CommandKey = keyboard.commandKey;
        }

        public KeyCode KeyCode { get; }
        public char Character { get; }
        public bool AltKey { get; }
        public bool CtrlKey { get; }
        public bool ShiftKey { get; }
        public bool CommandKey { get; }
    }

    // =========================================================================
    // Event data — focus
    // =========================================================================

    /// <summary>
    /// Event data for focus events: onFocus, onBlur, onFocusIn, onFocusOut.
    /// Origin: NEW (UIElements FocusEvent / BlurEvent carry no extra fields
    /// beyond the relatedTarget, which is exposed here).
    /// </summary>
    public sealed class ReactiveFocusEvent : ReactiveEvent
    {
        internal ReactiveFocusEvent(EventBase evt)
            : base(evt)
        {
            // FocusEvent and BlurEvent share IFocusEvent interface
            if (evt is IFocusEvent focus)
                RelatedTarget = focus.relatedTarget as VisualElement;
        }

        /// <summary>
        /// For onFocus/onFocusIn: the element losing focus.
        /// For onBlur/onFocusOut: the element gaining focus.
        /// </summary>
        public VisualElement RelatedTarget { get; }
    }

    // =========================================================================
    // Event data — drag
    // =========================================================================

    /// <summary>
    /// Event data for drag events: onDragEnter, onDragLeave, onDragUpdated,
    /// onDragPerform, onDragExited.
    /// Origin: NEW (UIElements drag events share IDragEvent; no extra typed
    /// fields exist beyond the base event).
    /// Only available in Editor builds (#if UNITY_EDITOR).
    /// </summary>
    public sealed class ReactiveDragEvent : ReactiveEvent
    {
        internal ReactiveDragEvent(EventBase evt)
            : base(evt) { }
    }

    // =========================================================================
    // Event data — geometry
    // =========================================================================

    /// <summary>
    /// Event data for onGeometryChanged. Exposes the element's old and new rects.
    /// Origin: NEW (wraps <c>GeometryChangedEvent.oldRect</c> / <c>newRect</c>).
    /// </summary>
    public sealed class ReactiveGeometryEvent : ReactiveEvent
    {
        internal ReactiveGeometryEvent(EventBase evt, GeometryChangedEvent geo)
            : base(evt)
        {
            if (geo == null)
                return;
            OldRect = geo.oldRect;
            NewRect = geo.newRect;
        }

        /// <summary>The element's layout rect before the change.</summary>
        public Rect OldRect { get; }

        /// <summary>The element's layout rect after the change.</summary>
        public Rect NewRect { get; }
    }

    // =========================================================================
    // Event data — panel attach / detach
    // =========================================================================

    /// <summary>
    /// Event data for onAttachToPanel and onDetachFromPanel.
    /// Origin: NEW (wraps <c>AttachToPanelEvent</c> / <c>DetachFromPanelEvent</c>).
    /// </summary>
    public sealed class ReactivePanelEvent : ReactiveEvent
    {
        internal ReactivePanelEvent(EventBase evt)
            : base(evt)
        {
            if (evt is AttachToPanelEvent attach)
                Panel = attach.destinationPanel as VisualElement;
            else if (evt is DetachFromPanelEvent detach)
                Panel = detach.originPanel as VisualElement;
        }

        /// <summary>
        /// For onAttachToPanel: the panel being attached to.
        /// For onDetachFromPanel: the panel being detached from.
        /// May be null if the panel root is not a VisualElement.
        /// </summary>
        public VisualElement Panel { get; }
    }

    // =========================================================================
    // Event-handler delegates
    // =========================================================================

    /// <summary>
    /// Generic event handler — base utility for library authors extending the
    /// event system. Prefer concrete aliases (PointerEventHandler etc.) in
    /// application code.
    /// React parity: <c>EventHandler&lt;E&gt;</c>.
    /// </summary>
    public delegate void UIEventHandler<E>(E e)
        where E : ReactiveEvent;

    /// <summary>
    /// Handler for pointer / mouse / click events.
    /// Covers props: onClick, onPointerDown, onPointerUp, onPointerMove,
    ///               onPointerEnter, onPointerLeave.
    /// React parity: <c>PointerEventHandler&lt;T&gt;</c> / <c>MouseEventHandler&lt;T&gt;</c>.
    /// Origin: Action (onClick) + EventCallback&lt;Pointer*Event&gt; (all others).
    /// </summary>
    public delegate void PointerEventHandler(ReactivePointerEvent e);

    /// <summary>
    /// Handler for scroll-wheel events.
    /// Covers props: onWheel, onScroll.
    /// React parity: <c>WheelEventHandler&lt;T&gt;</c>.
    /// Origin: EventCallback&lt;WheelEvent&gt;.
    /// </summary>
    public delegate void WheelEventHandler(ReactiveWheelEvent e);

    /// <summary>
    /// Handler for keyboard events.
    /// Covers props: onKeyDown, onKeyUp.
    /// React parity: <c>KeyboardEventHandler&lt;T&gt;</c>.
    /// Origin: EventCallback&lt;KeyDownEvent&gt; / EventCallback&lt;KeyUpEvent&gt;.
    /// </summary>
    public delegate void KeyboardEventHandler(ReactiveKeyboardEvent e);

    /// <summary>
    /// Handler for focus/blur events.
    /// Covers props: onFocus, onBlur, onFocusIn, onFocusOut.
    /// React parity: <c>FocusEventHandler&lt;T&gt;</c>.
    /// Origin: EventCallback&lt;FocusEvent&gt; / EventCallback&lt;BlurEvent&gt; / etc.
    /// </summary>
    public delegate void FocusEventHandler(ReactiveFocusEvent e);

    /// <summary>
    /// Handler for drag events (Editor only).
    /// Covers props: onDragEnter, onDragLeave, onDragUpdated, onDragPerform,
    ///               onDragExited.
    /// React parity: <c>DragEventHandler&lt;T&gt;</c>.
    /// Origin: EventCallback&lt;DragEnterEvent&gt; / etc.
    /// </summary>
    public delegate void DragEventHandler(ReactiveDragEvent e);

    /// <summary>
    /// Handler for geometry-changed events.
    /// Covers props: onGeometryChanged.
    /// React parity: (no direct equivalent — UIToolkit-specific).
    /// Origin: EventCallback&lt;GeometryChangedEvent&gt;.
    /// </summary>
    public delegate void GeometryChangedEventHandler(ReactiveGeometryEvent e);

    /// <summary>
    /// Handler for panel attach/detach events.
    /// Covers props: onAttachToPanel, onDetachFromPanel.
    /// React parity: (closest: componentDidMount / componentWillUnmount).
    /// Origin: EventCallback&lt;AttachToPanelEvent&gt; / EventCallback&lt;DetachFromPanelEvent&gt;.
    /// </summary>
    public delegate void PanelLifecycleEventHandler(ReactivePanelEvent e);

    /// <summary>
    /// Handler for value-change events on typed input elements.
    /// Covers props: onChange on TextField, Slider, Toggle, Dropdown, ColorField, etc.
    /// React parity: <c>ChangeEventHandler&lt;CurrentTarget, Target&gt;</c>.
    /// Origin: Action&lt;ChangeEvent&lt;T&gt;&gt; across 15+ Props files.
    /// </summary>
    public delegate void ChangeEventHandler<T>(ChangeEvent<T> e);

    /// <summary>
    /// Handler for raw text-input events (fires on every keystroke).
    /// Covers props: onInput.
    /// React parity: <c>InputEventHandler&lt;T&gt;</c>.
    /// Origin: was missing from BaseProps; only existed in PropsApplier (line 1877).
    /// </summary>
    public delegate void InputEventHandler(string newValue);

    // =========================================================================
    // Virtual-DOM render-function delegates
    // =========================================================================

    /// <summary>
    /// Renders a single row/cell for a given index and data item.
    /// Covers props: Row (ListView, TreeView), Cell (MultiColumn ColumnDef).
    /// Unifies: Func&lt;int, object, VirtualNode&gt; from ListView, TreeView,
    ///          MultiColumnListView, MultiColumnTreeView.
    /// React parity: render prop / children-as-function pattern.
    /// </summary>
    public delegate VirtualNode RowRenderer(int index, object item);

    /// <summary>
    /// Renders deferred/lazy content with no parameters.
    /// Covers props: Content (TabDef in TabViewProps).
    /// Origin: Func&lt;VirtualNode&gt; in TabViewProps.TabDef.
    /// React parity: children-as-function / render prop.
    /// </summary>
    public delegate VirtualNode ContentRenderer();

    /// <summary>
    /// Creates a reusable native VisualElement for ListView's virtual pool.
    /// Covers props: MakeItem (ListView).
    /// Origin: Func&lt;VisualElement&gt;.
    /// Note: Use <see cref="RowRenderer"/> (Row=) instead when possible — it is
    ///       simpler and React-idiomatic. MakeItem+BindItem is the native
    ///       UIToolkit path for maximum performance on large lists.
    /// </summary>
    public delegate VisualElement ItemFactory();

    /// <summary>
    /// Binds or unbinds data to a pooled VisualElement in ListView.
    /// Covers props: BindItem, UnbindItem (ListView).
    /// Origin: Action&lt;VisualElement, int&gt;.
    /// </summary>
    public delegate void ItemBinder(VisualElement element, int index);

    // =========================================================================
    // Widget-event delegates
    // =========================================================================

    /// <summary>
    /// Fired when a TreeView item is expanded or collapsed.
    /// Covers props: ItemExpandedChanged (TreeViewProps).
    /// Origin: Delegate (runtime-checked) → concrete Action&lt;TreeViewExpansionChangedArgs&gt;.
    /// </summary>
    public delegate void TreeExpansionEventHandler(TreeViewExpansionChangedArgs args);

    /// <summary>
    /// Fired when the selected tab index changes.
    /// Covers props: SelectedIndexChanged (TabViewProps).
    /// Origin: Delegate (runtime-checked) → Action&lt;int&gt;.
    /// </summary>
    public delegate void TabIndexEventHandler(int index);

    /// <summary>
    /// Fired when the active tab changes (single-arg form — new active tab only).
    /// Covers props: ActiveTabChanged (TabViewProps) — single argument form.
    /// Origin: Delegate (runtime-checked) → Action&lt;Tab&gt;.
    /// </summary>
    public delegate void TabChangeEventHandler(Tab tab);

    /// <summary>
    /// Fired when the active tab changes (two-arg form — previous + next tab).
    /// Covers props: ActiveTabChanged (TabViewProps) — two argument form.
    /// Origin: Delegate (runtime-checked) → Action&lt;Tab, Tab&gt;.
    /// </summary>
    public delegate void TabChangedEventHandler(Tab previous, Tab next);

    /// <summary>
    /// Fired when column sort order changes in MultiColumnListView / MultiColumnTreeView.
    /// Covers props: ColumnSortingChanged.
    /// </summary>
    public delegate void ColumnSortEventHandler(List<SortedColumnDef> columns);

    /// <summary>
    /// Fired when column widths, visibility, or display order changes.
    /// Covers props: ColumnLayoutChanged.
    /// </summary>
    public delegate void ColumnLayoutEventHandler(ColumnLayoutState state);

    // =========================================================================
    // Misc delegates
    // =========================================================================

    /// <summary>
    /// Populates a DropdownMenu with items.
    /// Covers props: PopulateMenu (ToolbarMenuProps).
    /// Origin: Action&lt;DropdownMenu&gt;.
    /// </summary>
    public delegate void MenuBuilderHandler(DropdownMenu menu);

    /// <summary>
    /// Called when an ErrorBoundary catches an unhandled exception.
    /// Covers props: OnError (ErrorBoundaryProps).
    /// Origin: Action&lt;Exception&gt;.
    /// </summary>
    public delegate void ErrorEventHandler(Exception error);
}
