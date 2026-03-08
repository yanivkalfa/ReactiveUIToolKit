using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;
using SK = ReactiveUITK.Props.Typed.StyleKeys;

namespace ReactiveUITK.Samples.FunctionalComponents
{
    /// <summary>
    /// Demonstrates the ref-as-prop pattern: <see cref="Ref{T}"/> instances
    /// are passed to child components as plain typed props — no <c>V.ForwardRef</c>
    /// wrapper is needed.  The child receives fully-typed refs without any runtime cast.
    /// </summary>
    public static class RefForwardingDemoFunc
    {
        public static VirtualNode Render(
            IProps rawProps,
            IReadOnlyList<VirtualNode> children
        )
        {
            Ref<TextField> inputRef = Hooks.UseRef<TextField>();
            Ref<Label>     labelRef  = Hooks.UseRef<Label>();
            var (snapshot, setSnapshot) = Hooks.UseState(
                "Click \"Read refs\" to inspect ref assignments."
            );

            void UpdateSnapshot()
            {
                TextField textField = inputRef?.Current;
                Label     label     = labelRef?.Current;
                bool isFocused = textField?.focusController?.focusedElement == textField;
                string textSummary =
                    textField == null
                        ? "input ref is null"
                        : $"input ref: value=\"{textField.value}\" focused={isFocused}";
                string labelSummary =
                    label == null
                        ? "label ref is null"
                        : $"label ref: text=\"{label.text}\"";
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
                "ref-demo-controls",
                V.Button(
                    new ButtonProps { Text = "Read refs (parent)", OnClick = _ => UpdateSnapshot() },
                    key: "read-refs-btn"
                ),
                V.Button(
                    new ButtonProps
                    {
                        Text    = "Focus input (parent)",
                        OnClick = _ => inputRef?.Current?.Focus(),
                        Style   = new Style { (SK.MarginLeft, 6f) },
                    },
                    key: "focus-parent-btn"
                )
            );

            // Pass refs to the child as plain typed props — identical to any other prop.
            VirtualNode childNode = V.Func<RefChild.Props>(
                RefChild.Render,
                new RefChild.Props
                {
                    InputRef   = inputRef,
                    LabelRef   = labelRef,
                    OnSnapshot = UpdateSnapshot,
                },
                key: "ref-child"
            );

            return V.VisualElement(
                new VisualElementProps
                {
                    Style = new Style
                    {
                        (SK.FlexDirection, "column"),
                        (SK.Padding, 12f),
                        (SK.FlexGrow, 1f),
                    }
                },
                "ref-demo-root",
                V.Text("useRef + ref-as-prop demo", key: "title"),
                V.Text(
                    "Parent holds Ref<TextField> and Ref<Label>, passes them to the child as plain typed props.",
                    key: "subtitle"
                ),
                controlsRow,
                childNode,
                V.Label(
                    new LabelProps
                    {
                        Text  = snapshot,
                        Style = new Style { (SK.MarginTop, 8f), (SK.FontSize, 13f) },
                    },
                    key: "snapshot-label"
                ),
                V.Label(
                    new LabelProps
                    {
                        Text =
                            inputRef?.Current != null
                                ? $"Parent sees inputRef.current: TextField name='{inputRef.Current.name}'"
                                : "Parent sees inputRef.current == null",
                        Style = new Style { (SK.MarginTop, 4f) },
                    },
                    key: "parent-input-ref-line"
                ),
                V.Label(
                    new LabelProps
                    {
                        Text =
                            labelRef?.Current != null
                                ? $"Parent sees labelRef.current.text = '{labelRef.Current.text}'"
                                : "Parent sees labelRef.current == null",
                        Style = new Style { (SK.MarginTop, 2f) },
                    },
                    key: "parent-label-ref-line"
                )
            );
        }

        /// <summary>
        /// Child component that receives refs as plain typed props.
        /// Uses the standard two-argument <c>Render</c> signature — identical to every
        /// other component in the library.
        /// </summary>
        private static class RefChild
        {
            public sealed class Props : IProps
            {
                public Ref<TextField> InputRef   { get; set; }
                public Ref<Label>     LabelRef   { get; set; }
                public Action                      OnSnapshot { get; set; }

                public override bool Equals(object obj)
                {
                    if (obj is not Props other) return false;
                    return ReferenceEquals(InputRef,   other.InputRef)
                        && ReferenceEquals(LabelRef,   other.LabelRef)
                        && ReferenceEquals(OnSnapshot, other.OnSnapshot);
                }

                public override int GetHashCode()
                {
                    unchecked
                    {
                        int h = 17;
                        h = h * 31 + (InputRef   != null ? InputRef.GetHashCode()   : 0);
                        h = h * 31 + (LabelRef   != null ? LabelRef.GetHashCode()   : 0);
                        h = h * 31 + (OnSnapshot != null ? OnSnapshot.GetHashCode() : 0);
                        return h;
                    }
                }
            }

            public static VirtualNode Render(
                IProps rawProps,
                IReadOnlyList<VirtualNode> children
            )
            {
                var p     = (rawProps as Props) ?? new Props();
                string value = p.InputRef?.Current?.value ?? "Hello ReactiveUITK refs!";

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
                    "ref-child-root",
                    V.Label(
                        new LabelProps
                        {
                            Text = $"Child sees value: {value}",
                            Ref  = p.LabelRef,
                        },
                        key: "child-label"
                    ),
                    V.TextField(
                        new TextFieldProps
                        {
                            Value = value,
                            Ref   = p.InputRef,
                            Style = new Style { (SK.MarginTop, 6f) },
                        },
                        key: "child-input"
                    ),
                    V.Button(
                        new ButtonProps
                        {
                            Text    = "Focus input (child via ref)",
                            OnClick = _ =>
                            {
                                p.InputRef?.Current?.Focus();
                                p.OnSnapshot?.Invoke();
                            },
                            Style = new Style { (SK.MarginTop, 8f) },
                        },
                        key: "child-focus-btn"
                    )
                );
            }
        }
    }
}
