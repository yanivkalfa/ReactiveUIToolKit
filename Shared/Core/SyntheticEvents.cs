using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Core
{
    /// <summary>Use <see cref="ReactiveEvent"/> instead.</summary>
    [Obsolete(
        "SyntheticEvent is deprecated. Use ReactiveEvent and the typed Reactive*Event subclasses instead."
    )]
    public class SyntheticEvent
    {
        private readonly EventBase nativeEvent;

        public SyntheticEvent(EventBase evt)
        {
            nativeEvent = evt;
            Type = evt?.GetType().Name ?? string.Empty;
            Target = evt?.target as VisualElement;
            CurrentTarget = evt?.currentTarget as VisualElement;
            Timestamp = evt != null ? evt.timestamp : 0L;
        }

        public string Type { get; }
        public VisualElement Target { get; }
        public VisualElement CurrentTarget { get; internal set; }
        public long Timestamp { get; }
        public bool IsPropagationStopped { get; private set; }
        public bool IsDefaultPrevented { get; private set; }
        public EventBase NativeEvent => nativeEvent;

        public virtual void StopPropagation()
        {
            try
            {
                IsPropagationStopped = true;
                nativeEvent?.StopPropagation();
            }
            catch { }
        }

        public virtual void PreventDefault()
        {
            IsDefaultPrevented = true;
        }

        public static SyntheticEvent Create(EventBase evt)
        {
            if (evt == null)
            {
                return null;
            }

            if (evt is WheelEvent wheel)
            {
                return new SyntheticWheelEvent(evt, wheel);
            }

            if (evt is IPointerEvent pointer)
            {
                return new SyntheticPointerEvent(evt, pointer);
            }
            if (evt is IMouseEvent mouse)
            {
                return new SyntheticPointerEvent(evt, mouse);
            }

            if (evt is IKeyboardEvent keyboard)
            {
                return new SyntheticKeyboardEvent(evt, keyboard);
            }

            if (evt is IChangeEvent change)
            {
                return new SyntheticChangeEvent(evt, change);
            }

            return new SyntheticEvent(evt);
        }
    }

    /// <summary>Use <see cref="ReactivePointerEvent"/> instead.</summary>
    [Obsolete("SyntheticPointerEvent is deprecated. Use ReactivePointerEvent instead.")]
    public class SyntheticPointerEvent : SyntheticEvent
    {
        internal SyntheticPointerEvent(EventBase evt, IPointerEvent pointer)
            : base(evt)
        {
            if (pointer != null)
            {
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
        }

        internal SyntheticPointerEvent(EventBase evt, IMouseEvent mouse)
            : base(evt)
        {
            if (mouse != null)
            {
                Position = mouse.mousePosition;
                DeltaPosition = mouse.mouseDelta;
                Button = mouse.button;
                ClickCount = mouse.clickCount;
                AltKey = mouse.altKey;
                CtrlKey = mouse.ctrlKey;
                ShiftKey = mouse.shiftKey;
                CommandKey = mouse.commandKey;
            }
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

    /// <summary>Use <see cref="ReactiveWheelEvent"/> instead.</summary>
    [Obsolete("SyntheticWheelEvent is deprecated. Use ReactiveWheelEvent instead.")]
    public sealed class SyntheticWheelEvent : SyntheticPointerEvent
    {
        internal SyntheticWheelEvent(EventBase evt, WheelEvent wheel)
            : base(evt, wheel as IMouseEvent)
        {
            if (wheel != null)
            {
                Delta = wheel.delta;
            }
        }

        public Vector3 Delta { get; }
    }

    /// <summary>Use <see cref="ReactiveKeyboardEvent"/> instead.</summary>
    [Obsolete("SyntheticKeyboardEvent is deprecated. Use ReactiveKeyboardEvent instead.")]
    public sealed class SyntheticKeyboardEvent : SyntheticEvent
    {
        internal SyntheticKeyboardEvent(EventBase evt, IKeyboardEvent keyboard)
            : base(evt)
        {
            if (keyboard != null)
            {
                KeyCode = keyboard.keyCode;
                Character = keyboard.character;
                AltKey = keyboard.altKey;
                CtrlKey = keyboard.ctrlKey;
                ShiftKey = keyboard.shiftKey;
                CommandKey = keyboard.commandKey;
            }
        }

        public KeyCode KeyCode { get; }
        public char Character { get; }
        public bool AltKey { get; }
        public bool CtrlKey { get; }
        public bool ShiftKey { get; }
        public bool CommandKey { get; }
    }

    /// <summary>Use <see cref="ChangeEventHandler{T}"/> instead.</summary>
    [Obsolete("SyntheticChangeEvent is deprecated. Use ChangeEventHandler<T> instead.")]
    public sealed class SyntheticChangeEvent : SyntheticEvent
    {
        internal SyntheticChangeEvent(EventBase evt, IChangeEvent change)
            : base(evt)
        {
            if (change != null)
            {
                try
                {
                    var type = change.GetType();
                    var prevProp = type.GetProperty("previousValue");
                    var newProp = type.GetProperty("newValue");
                    PreviousValue = prevProp?.GetValue(change);
                    NewValue = newProp?.GetValue(change);
                }
                catch { }
            }
        }

        public object PreviousValue { get; }
        public object NewValue { get; }
    }
}
