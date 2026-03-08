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
            IProps rawProps,
            IReadOnlyList<VirtualNode> children
        )
        {
            Hooks.MutableRef<TextField> inputRef = Hooks.UseRef<TextField>();
            Hooks.MutableRef<Label> labelRef = Hooks.UseRef<Label>();
            var (snapshot, setSnapshot) = Hooks.UseState(
                "Click \"Read refs\" to inspect ref.current assignments."
            );

            void UpdateSnapshot()
            {
                TextField textField = inputRef?.Value;
                Label label = labelRef?.Value;
                bool isFocused = textField?.focusController?.focusedElement == textField;
                string textSummary =
                    textField == null
                        ? "input ref is null"
                        : $"input ref current: value=\"{textField.value}\" focused={isFocused}";
                string labelSummary =
                    label == null
                        ? "label ref is null"
                        : $"label ref current: text=\"{label.text}\"";
                setSnapshot($"{textSummary}; {labelSummary}");
            }

            VirtualNode controlsRow = V.VisualElement(
                new VisualElementProps
                {
                    Style = new Style
                    {
                        (SK.FlexDirection, "row"),
                        (SK.AlignItems, "center"),
                        (SK.MarginTop, 6f),
                    }
                },
                "ref-forward-controls",
                V.Button(
                    new ButtonProps { Text = "Read refs (parent)", OnClick = UpdateSnapshot },
                    key: "read-refs-btn"
                ),
                V.Button(
                    new ButtonProps
                    {
                        Text = "Focus input (parent)",
                        OnClick = () => inputRef?.Value?.Focus(),
                        Style = new Style { (SK.MarginLeft, 6f) },
                    },
                    key: "focus-parent-btn"
                )
            );

            VirtualNode childDemo = V.ForwardRef<ForwardedChild.Props>(
                ForwardedChild.RenderWithForwardedRef,
                new ForwardedChild.Props { LabelRef = labelRef, OnChildSnapshot = UpdateSnapshot },
                forwardedRef: inputRef,
                key: "forwarded-child"
            );

            return V.VisualElement(
                new VisualElementProps { Style = new Style { (SK.FlexDirection, "column"), (SK.Padding, 12f), (SK.FlexGrow, 1f) } },
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
                        Style = new Style { (SK.MarginTop, 8f), (SK.FontSize, 13f) },
                    },
                    key: "snapshot-label"
                ),
                V.Label(
                    new LabelProps
                    {
                        Text =
                            inputRef?.Value != null
                                ? $"Parent sees ref.current: TextField name='{inputRef.Value.name}'"
                                : "Parent sees ref.current == null",
                        Style = new Style { (SK.MarginTop, 4f) },
                    },
                    key: "parent-ref-line"
                ),
                V.Label(
                    new LabelProps
                    {
                        Text =
                            labelRef?.Value != null
                                ? $"Child label ref.current.text = '{labelRef.Value.text}'"
                                : "Child label ref.current is null",
                        Style = new Style { (SK.MarginTop, 2f) },
                    },
                    key: "label-ref-line"
                )
            );
        }

        private static class ForwardedChild
        {
            public sealed class Props : IProps
            {
                public Hooks.MutableRef<Label> LabelRef { get; set; }
                public Action OnChildSnapshot { get; set; }
            }

            public static VirtualNode RenderWithForwardedRef(
                IProps rawProps,
                object forwardedRef,
                IReadOnlyList<VirtualNode> children
            )
            {
                var p = rawProps as Props;
                Hooks.MutableRef<Label> labelRef = p?.LabelRef;
                Action onChildSnapshot = p?.OnChildSnapshot;
                Hooks.MutableRef<TextField> typedForwardedRef =
                    forwardedRef as Hooks.MutableRef<TextField>;

                // Read the latest input value via the forwarded ref when available.
                string currentValue = typedForwardedRef?.Value?.value ?? "Hello ReactiveUITK refs!";

                return V.VisualElement(
                    new VisualElementProps
                    {
                        Style = new Style
                        {
                            (SK.FlexDirection, "column"),
                            (SK.MarginTop, 10f),
                            (SK.Padding, 8f),
                            (SK.BorderWidth, 1f),
                            (SK.BorderColor, new Color(0.82f, 0.82f, 0.82f, 1f)),
                        }
                    },
                    "ref-forward-child-root",
                    V.Label(
                        new LabelProps
                        {
                            Text = $"Child sees value: {currentValue}",
                            Ref = labelRef,
                        },
                        key: "child-label"
                    ),
                    V.TextField(
                        new TextFieldProps
                        {
                            Value = currentValue,
                            Ref = forwardedRef,
                            Style = new Style { (SK.MarginTop, 6f) },
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
                            Style = new Style { (SK.MarginTop, 8f) },
                        },
                        key: "child-focus-button"
                    )
                );
            }
        }
    }
}
