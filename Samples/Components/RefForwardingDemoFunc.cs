using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;
using SK = ReactiveUITK.Props.Typed.StyleKeys;

namespace ReactiveUITK.Samples.FunctionalComponents
{
    public static class RefForwardingDemoFunc
    {
        public static VirtualNode Render(
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children
        )
        {
            Hooks.MutableRef<TextField> inputRef = Hooks.UseRef<TextField>();
            Hooks.MutableRef<Label> labelRef = Hooks.UseRef<Label>();
            var (inputValue, setInputValue) = Hooks.UseState("Hello ReactiveUITK refs!");
            var (snapshot, setSnapshot) = Hooks.UseState(
                "Click \"Read refs\" to inspect ref.current assignments."
            );

            void UpdateSnapshot()
            {
                TextField textField = inputRef?.Value;
                Label label = labelRef?.Value;
                bool isFocused = textField?.focusController?.focusedElement == textField;
                string textSummary = textField == null
                    ? "input ref is null"
                    : $"input ref current: value=\"{textField.value}\" focused={isFocused}";
                string labelSummary = label == null
                    ? "label ref is null"
                    : $"label ref current: text=\"{label.text}\"";
                setSnapshot($"{textSummary}; {labelSummary}");
            }

            VirtualNode controlsRow = V.VisualElement(
                new Style
                {
                    (SK.FlexDirection, "row"),
                    (SK.AlignItems, "center"),
                    (SK.MarginTop, 6f)
                },
                "ref-forward-controls",
                V.Button(
                    new ButtonProps
                    {
                        Text = "Read refs (parent)",
                        OnClick = UpdateSnapshot
                    },
                    key: "read-refs-btn"
                ),
                V.Button(
                    new ButtonProps
                    {
                        Text = "Focus input (parent)",
                        OnClick = () => inputRef?.Value?.Focus(),
                        Style = new Style { (SK.MarginLeft, 6f) }
                    },
                    key: "focus-parent-btn"
                )
            );

            var childProps = new Dictionary<string, object>
            {
                { "ref", inputRef },
                { "labelRef", labelRef },
                { "value", inputValue },
                { "onChange", (Action<string>)(newValue => setInputValue(newValue)) },
                { "onChildSnapshot", (Action)UpdateSnapshot }
            };

            VirtualNode childDemo = V.ForwardRef(
                ForwardedChild.RenderWithForwardedRef,
                childProps,
                key: "forwarded-child"
            );

            return V.VisualElement(
                new Style
                {
                    (SK.FlexDirection, "column"),
                    (SK.Padding, 12f),
                    (SK.FlexGrow, 1f)
                },
                "ref-forward-root",
                V.Text("Forward refs + useRef demo", key: "title"),
                V.Text(
                    "Parent holds MutableRef<TextField> and forwards it into a child element.",
                    key: "subtitle"
                ),
                controlsRow,
                childDemo,
                V.Label(
                    new LabelProps
                    {
                        Text = snapshot,
                        Style = new Style { (SK.MarginTop, 8f), (SK.FontSize, 13f) }
                    },
                    key: "snapshot-label"
                ),
                V.Label(
                    new LabelProps
                    {
                        Text = inputRef?.Value != null
                            ? $"Parent sees ref.current: TextField name='{inputRef.Value.name}'"
                            : "Parent sees ref.current == null",
                        Style = new Style { (SK.MarginTop, 4f) }
                    },
                    key: "parent-ref-line"
                ),
                V.Label(
                    new LabelProps
                    {
                        Text = labelRef?.Value != null
                            ? $"Child label ref.current.text = '{labelRef.Value.text}'"
                            : "Child label ref.current is null",
                        Style = new Style { (SK.MarginTop, 2f) }
                    },
                    key: "label-ref-line"
                )
            );
        }

        private static class ForwardedChild
        {
            public static VirtualNode RenderWithForwardedRef(
                Dictionary<string, object> props,
                object forwardedRef,
                IReadOnlyList<VirtualNode> children
            )
            {
                Hooks.MutableRef<Label> labelRef = null;
                string value = string.Empty;
                Action<string> onChange = null;
                Action onChildSnapshot = null;
                Hooks.MutableRef<TextField> typedForwardedRef = forwardedRef as Hooks.MutableRef<TextField>;

                if (props != null)
                {
                    if (props.TryGetValue("labelRef", out object labelObj))
                    {
                        labelRef = labelObj as Hooks.MutableRef<Label>;
                    }
                    if (props.TryGetValue("value", out object rawValue) && rawValue is string s)
                    {
                        value = s;
                    }
                    if (props.TryGetValue("onChange", out object changeObj) && changeObj is Action<string> change)
                    {
                        onChange = change;
                    }
                    if (props.TryGetValue("onChildSnapshot", out object snapshotObj) && snapshotObj is Action snap)
                    {
                        onChildSnapshot = snap;
                    }
                }

                return V.VisualElement(
                    new Style
                    {
                        (SK.FlexDirection, "column"),
                        (SK.MarginTop, 10f),
                        (SK.Padding, 8f),
                        (SK.BorderWidth, 1f),
                        (SK.BorderColor, new Color(0.82f, 0.82f, 0.82f, 1f))
                    },
                    "ref-forward-child-root",
                    V.Label(
                        new LabelProps
                        {
                            Text = $"Child sees value: {value}",
                            Ref = labelRef
                        },
                        key: "child-label"
                    ),
                    V.TextField(
                        new TextFieldProps
                        {
                            Value = value,
                            OnChange = evt => onChange?.Invoke(evt.newValue),
                            Ref = forwardedRef,
                            Style = new Style { (SK.MarginTop, 6f) }
                        },
                        key: "child-input"
                    ),
                    V.Button(
                        new ButtonProps
                        {
                            Text = "Focus input (child via ref)",
                            OnClick = () =>
                            {
                                typedForwardedRef?.Value?.Focus();
                                onChildSnapshot?.Invoke();
                            },
                            Style = new Style { (SK.MarginTop, 8f) }
                        },
                        key: "child-focus-button"
                    )
                );
            }
        }
    }
}
