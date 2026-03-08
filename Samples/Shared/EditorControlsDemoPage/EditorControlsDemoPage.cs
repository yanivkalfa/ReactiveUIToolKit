#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Samples.Shared
{
    public static class EditorControlsDemoPage
    {
        public sealed class Props : IProps
        {
            public string Search { get; set; }
            public UnityEngine.Object Sel { get; set; }
        }

        public static VirtualNode Render(
            IProps rawProps,
            IReadOnlyList<VirtualNode> children
        )
        {
            var p = rawProps as Props;
            string search = p?.Search ?? string.Empty;
            UnityEngine.Object sel = p?.Sel;
            // Provide a safe default inspector/serialized target to avoid empty visuals
            if (sel == null)
            {
                var so = ScriptableObject.CreateInstance<ScriptableObject>();
                so.name = "Demo ScriptableObject";
                sel = so;
            }

            return V.VisualElement(
                new VisualElementProps
                {
                    Style = new Style
                    {
                        { StyleKeys.FlexGrow, 1f },
                        { StyleKeys.FlexDirection, "column" },
                        { StyleKeys.Padding, 8f },
                    }
                },
                key: "editor-controls-root",
                V.Toolbar(
                    new ToolbarProps
                    {
                        Style = new Style
                        {
                            { StyleKeys.FlexShrink, 0f },
                            { StyleKeys.MarginBottom, 6f },
                        },
                    },
                    key: "tb",
                    V.ToolbarButton(
                        new ToolbarButtonProps
                        {
                            Text = "Ping",
                            OnClick = () => Debug.Log("Ping clicked"),
                        }
                    ),
                    V.ToolbarToggle(new ToolbarToggleProps { Text = "Toggle", Value = false }),
                    V.ToolbarMenu(
                        new ToolbarMenuProps
                        {
                            Text = "Menu",
                            PopulateMenu = dm =>
                            {
                                dm.AppendAction("Action A", _ => Debug.Log("Menu A"));
                                dm.AppendAction("Action B", _ => Debug.Log("Menu B"));
                            },
                        }
                    ),
                    V.ToolbarSpacer(new ToolbarSpacerProps { }),
                    V.ToolbarSearchField(
                        new ToolbarSearchFieldProps
                        {
                            Value = search,
                            Style = new Style { { StyleKeys.Width, 180f } },
                        }
                    )
                ),
                V.ScrollView(
                    new ScrollViewProps { Mode = "vertical" },
                    key: "scroll",
                    V.VisualElement(
                        new VisualElementProps { Style = new Style { { StyleKeys.FlexDirection, "column" } } },
                        key: "content",
                        V.VisualElement(
                            new VisualElementProps
                            {
                                Style = new Style
                                {
                                    { StyleKeys.FlexDirection, FlexDirection.Row },
                                    { StyleKeys.AlignItems, "flex-start" },
                                    { StyleKeys.MarginTop, 8f },
                                }
                            },
                            key: "row",
                            V.VisualElement(
                                new VisualElementProps
                                {
                                    Style = new Style
                                    {
                                        { StyleKeys.FlexGrow, 1f },
                                        { StyleKeys.MarginRight, 8f },
                                    }
                                },
                                key: "lhs",
                                V.Label(new LabelProps { Text = "PropertyField Example" }),
                                V.PropertyField(
                                    new PropertyFieldProps
                                    {
                                        Target = sel,
                                        BindingPath = "m_Name",
                                        Label = "Name",
                                    }
                                )
                            ),
                            V.VisualElement(
                                new VisualElementProps { Style = new Style { { StyleKeys.FlexGrow, 1f } } },
                                key: "rhs",
                                V.Label(new LabelProps { Text = "InspectorElement Example" }),
                                V.InspectorElement(new InspectorElementProps { Target = sel })
                            )
                        ),
                        V.Label(new LabelProps { Text = "TwoPaneSplitView" }),
                        V.TwoPaneSplitView(
                            new TwoPaneSplitViewProps
                            {
                                Orientation = "horizontal",
                                FixedPaneIndex = 0,
                                FixedPaneInitialDimension = 220f,
                                Style = new Style
                                {
                                    { StyleKeys.Height, 260f },
                                    { StyleKeys.MarginTop, 6f },
                                },
                            },
                            key: "split",
                            V.VisualElement(
                                new VisualElementProps
                                {
                                    Style = new Style
                                    {
                                        { StyleKeys.FlexGrow, 1f },
                                        { StyleKeys.Padding, 6f },
                                        {
                                            StyleKeys.BackgroundColor,
                                            new Color(0.93f, 0.93f, 0.93f, 1f)
                                        },
                                    }
                                },
                                key: "pane1",
                                V.Label(new LabelProps { Text = "Pane 1" })
                            ),
                            V.VisualElement(
                                new VisualElementProps { Style = new Style { { StyleKeys.FlexGrow, 1f }, { StyleKeys.Padding, 6f } } },
                                key: "pane2",
                                V.Label(new LabelProps { Text = "Pane 2" })
                            )
                        )
                    )
                )
            );
        }
    }
}
#endif
