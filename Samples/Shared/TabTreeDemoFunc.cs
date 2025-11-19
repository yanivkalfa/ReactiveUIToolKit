using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Samples.Shared
{
    public static class TabTreeDemoFunc
    {
        public static VirtualNode Render(
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children
        )
        {
            var (tabIndex, setTabIndex) = Hooks.UseState(0);

            var tabViewProps = new TabViewProps
            {
                SelectedIndex = tabIndex,
                Tabs = new List<TabViewProps.TabDef>
                {
                    new()
                    {
                        Title = "Intro",
                        Content = () =>
                            V.Label(new LabelProps { Text = "This is a TabView + TreeView demo." }),
                    },
                    new()
                    {
                        Title = "Tree",
                        Content = () => V.Func(TreeViewStatefulDemoFunc.Render),
                    },
                    new()
                    {
                        Title = "Tree (Columns)",
                        Content = () => V.Func(MultiColumnTreeViewStatefulDemoFunc.Render),
                    },
                },

                Style = new Style { (Props.Typed.StyleKeys.Height, 240f) },
            };

            var btnRow = V.VisualElement(
                new Dictionary<string, object>
                {
                    {
                        "style",
                        new Style
                        {
                            (Props.Typed.StyleKeys.FlexDirection, "row"),
                            (Props.Typed.StyleKeys.MarginTop, 6f),
                        }
                    },
                },
                null,
                V.Button(
                    new ButtonProps
                    {
                        Text = "Intro",
                        OnClick = () => setTabIndex(0),
                        Style = new Style { (Props.Typed.StyleKeys.Width, 80f) },
                    },
                    key: "tabs-btn-intro"
                ),
                V.Button(
                    new ButtonProps
                    {
                        Text = "Tree",
                        OnClick = () => setTabIndex(1),
                        Style = new Style
                        {
                            (Props.Typed.StyleKeys.MarginLeft, 6f),
                            (Props.Typed.StyleKeys.Width, 80f),
                        },
                    },
                    key: "tabs-btn-tree"
                ),
                V.Button(
                    new ButtonProps
                    {
                        Text = "Columns",
                        OnClick = () => setTabIndex(2),
                        Style = new Style
                        {
                            (Props.Typed.StyleKeys.MarginLeft, 6f),
                            (Props.Typed.StyleKeys.Width, 90f),
                        },
                    },
                    key: "tabs-btn-columns"
                )
            );

            return V.GroupBox(
                new GroupBoxProps
                {
                    Text = "Tabs + TreeView",
                    ContentContainer = new Dictionary<string, object>
                    {
                        {
                            "style",
                            new Style
                            {
                                (Props.Typed.StyleKeys.PaddingLeft, 6f),
                                (Props.Typed.StyleKeys.PaddingTop, 4f),
                            }
                        },
                    },
                },
                null,
                btnRow,
                V.TabView(tabViewProps)
            );
        }
    }
}
