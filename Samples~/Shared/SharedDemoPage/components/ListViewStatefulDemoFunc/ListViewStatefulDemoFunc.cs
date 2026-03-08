using System;
using System.Collections;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;
using static ReactiveUITK.Props.Typed.StyleKeys;

namespace ReactiveUITK.Samples.Shared
{
    public static class ListViewStatefulDemoFunc
    {
        public sealed class Props : IProps
        {
            public IReadOnlyList<ListViewRowState> Items { get; set; }
            public Action AddItem { get; set; }
            public Action SetTopItem { get; set; }
            public Action DeleteLast { get; set; }
            public Action<int> OnCountChanged { get; set; }
        }

        public static VirtualNode Render(
            IProps rawProps,
            IReadOnlyList<VirtualNode> children
        )
        {
            var p = rawProps as Props;
            var items = p?.Items ?? Array.Empty<ListViewRowState>();
            var addItem = p?.AddItem;
            var setTopItem = p?.SetTopItem;
            var deleteLast = p?.DeleteLast;

            var rowRenderer = Hooks.UseMemo<RowRenderer>(
                () =>
                    (index, obj) =>
                        {
                            var r = obj as ListViewRowState;
                            if (r == null)
                            {
                                return V.Label(
                                    new LabelProps { Text = "<invalid>" },
                                    key: $"lv-invalid-{index}"
                                );
                            }
                            var id = !string.IsNullOrEmpty(r.Id) ? r.Id : index.ToString();
                            var funcKey = $"lv-row-{id}";
                            var childrenNode = r.ShouldOverrideElement
                                ? V.Label(new LabelProps { Text = r.Text ?? "<null>" }, funcKey)
                                : V.Func(IntroCounterFunc.Render, null, funcKey);
                            return V.VisualElement(null, key: $"lv-wrap-{id}", childrenNode);
                        },
                items
            );

            Hooks.UseEffect(
                () =>
                {
                    try
                    {
                        p?.OnCountChanged?.Invoke(items?.Count ?? 0);
                    }
                    catch { }
                    return null;
                },
                new object[] { items?.Count ?? 0 }
            );

            IList listItems =
                items as IList
                ?? (
                    items != null ? new List<ListViewRowState>(items) : new List<ListViewRowState>()
                );

            var listProps = new ListViewProps
            {
                Items = listItems,
                FixedItemHeight = 20f,
                Selection = SelectionType.None,
                Row = rowRenderer,
                Style = new Style
                {
                    (FlexGrow, 1f),
                    (
                        BackgroundColor,
                        new Color(0.15f, 0.15f, 0.15f, 1f)
                    ),
                },
            };

            PointerEventHandler Safe(Action candidate) => _ => candidate?.Invoke();

            var controls = V.VisualElement(
                new VisualElementProps
                {
                    Style = new Style
                    {
                        (StyleKeys.FlexDirection, "row"),
                        (FlexShrink, 0f),
                        (MarginBottom, 6f),
                    },
                },
                null,
                V.Button(new ButtonProps { Text = "Add", OnClick = Safe(addItem) }),
                V.Button(new ButtonProps { Text = "Set Value", OnClick = Safe(setTopItem) }),
                V.Button(new ButtonProps { Text = "Delete Last", OnClick = Safe(deleteLast) })
            );

            return V.VisualElement(null, null, controls, V.ListView(listProps));
        }
    }
}
