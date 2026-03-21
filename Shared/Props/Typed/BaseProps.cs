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
        public PointerEventHandler OnPointerDown { get; set; }
        public PointerEventHandler OnPointerUp { get; set; }
        public PointerEventHandler OnPointerMove { get; set; }
        public PointerEventHandler OnPointerEnter { get; set; }
        public PointerEventHandler OnPointerLeave { get; set; }
        public WheelEventHandler OnWheel { get; set; }
        public WheelEventHandler OnScroll { get; set; }

        // --- Drag events (editor-only) ---
#if UNITY_EDITOR
        public DragEventHandler OnDragEnter { get; set; }
        public DragEventHandler OnDragLeave { get; set; }
        public DragEventHandler OnDragUpdated { get; set; }
        public DragEventHandler OnDragPerform { get; set; }
        public DragEventHandler OnDragExited { get; set; }
#endif

        // --- Focus events ---
        public FocusEventHandler OnFocus { get; set; }
        public FocusEventHandler OnBlur { get; set; }
        public FocusEventHandler OnFocusIn { get; set; }
        public FocusEventHandler OnFocusOut { get; set; }

        // --- Keyboard events ---
        public KeyboardEventHandler OnKeyDown { get; set; }
        public KeyboardEventHandler OnKeyUp { get; set; }

        // --- Input event (fires on every keystroke) ---
        public InputEventHandler OnInput { get; set; }

        // --- Lifecycle events ---
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
            if (OnPointerDown != null)
                dict["onPointerDown"] = OnPointerDown;
            if (OnPointerUp != null)
                dict["onPointerUp"] = OnPointerUp;
            if (OnPointerMove != null)
                dict["onPointerMove"] = OnPointerMove;
            if (OnPointerEnter != null)
                dict["onPointerEnter"] = OnPointerEnter;
            if (OnPointerLeave != null)
                dict["onPointerLeave"] = OnPointerLeave;
            if (OnWheel != null)
                dict["onWheel"] = OnWheel;
            if (OnScroll != null)
                dict["onScroll"] = OnScroll;
#if UNITY_EDITOR
            if (OnDragEnter != null)
                dict["onDragEnter"] = OnDragEnter;
            if (OnDragLeave != null)
                dict["onDragLeave"] = OnDragLeave;
            if (OnDragUpdated != null)
                dict["onDragUpdated"] = OnDragUpdated;
            if (OnDragPerform != null)
                dict["onDragPerform"] = OnDragPerform;
            if (OnDragExited != null)
                dict["onDragExited"] = OnDragExited;
#endif
            if (OnFocus != null)
                dict["onFocus"] = OnFocus;
            if (OnBlur != null)
                dict["onBlur"] = OnBlur;
            if (OnFocusIn != null)
                dict["onFocusIn"] = OnFocusIn;
            if (OnFocusOut != null)
                dict["onFocusOut"] = OnFocusOut;
            if (OnKeyDown != null)
                dict["onKeyDown"] = OnKeyDown;
            if (OnKeyUp != null)
                dict["onKeyUp"] = OnKeyUp;
            if (OnInput != null)
                dict["onInput"] = OnInput;
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
