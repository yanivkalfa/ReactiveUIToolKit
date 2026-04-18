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

        // --- Pointer events ---
        public PointerEventHandler OnClick { get; set; }
        public PointerEventHandler OnClickCapture { get; set; }
        public PointerEventHandler OnPointerDown { get; set; }
        public PointerEventHandler OnPointerDownCapture { get; set; }
        public PointerEventHandler OnPointerUp { get; set; }
        public PointerEventHandler OnPointerUpCapture { get; set; }
        public PointerEventHandler OnPointerMove { get; set; }
        public PointerEventHandler OnPointerMoveCapture { get; set; }
        public PointerEventHandler OnPointerEnter { get; set; }
        public PointerEventHandler OnPointerEnterCapture { get; set; }
        public PointerEventHandler OnPointerLeave { get; set; }
        public PointerEventHandler OnPointerLeaveCapture { get; set; }
        public WheelEventHandler OnWheel { get; set; }
        public WheelEventHandler OnWheelCapture { get; set; }
        public WheelEventHandler OnScroll { get; set; }
        public WheelEventHandler OnScrollCapture { get; set; }

        // --- Drag events (editor-only) ---
#if UNITY_EDITOR
        public DragEventHandler OnDragEnter { get; set; }
        public DragEventHandler OnDragEnterCapture { get; set; }
        public DragEventHandler OnDragLeave { get; set; }
        public DragEventHandler OnDragLeaveCapture { get; set; }
        public DragEventHandler OnDragUpdated { get; set; }
        public DragEventHandler OnDragUpdatedCapture { get; set; }
        public DragEventHandler OnDragPerform { get; set; }
        public DragEventHandler OnDragPerformCapture { get; set; }
        public DragEventHandler OnDragExited { get; set; }
        public DragEventHandler OnDragExitedCapture { get; set; }
#endif

        // --- Focus events ---
        public FocusEventHandler OnFocus { get; set; }
        public FocusEventHandler OnFocusCapture { get; set; }
        public FocusEventHandler OnBlur { get; set; }
        public FocusEventHandler OnBlurCapture { get; set; }
        public FocusEventHandler OnFocusIn { get; set; }
        public FocusEventHandler OnFocusInCapture { get; set; }
        public FocusEventHandler OnFocusOut { get; set; }
        public FocusEventHandler OnFocusOutCapture { get; set; }

        // --- Keyboard events ---
        public KeyboardEventHandler OnKeyDown { get; set; }
        public KeyboardEventHandler OnKeyDownCapture { get; set; }
        public KeyboardEventHandler OnKeyUp { get; set; }
        public KeyboardEventHandler OnKeyUpCapture { get; set; }

        // --- Input event (fires on every keystroke) ---
        public InputEventHandler OnInput { get; set; }
        public InputEventHandler OnInputCapture { get; set; }

        // --- Lifecycle events (no capture — target-only) ---
        public GeometryChangedEventHandler OnGeometryChanged { get; set; }
        public PanelLifecycleEventHandler OnAttachToPanel { get; set; }
        public PanelLifecycleEventHandler OnDetachFromPanel { get; set; }

        // --- Escape hatch for non-standard / custom prop keys ---
        /// <summary>
        /// Optional dictionary of arbitrary extra props to be merged into the final
        /// serialized dictionary. Use this for custom event types or non-standard keys
        /// that are not covered by the typed properties above.
        /// </summary>
        public Dictionary<string, object> ExtraProps { get; set; }

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
