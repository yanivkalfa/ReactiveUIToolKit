using System;
using System.Collections;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

namespace ReactiveUITK.Samples.Shared
{
    public sealed class ListViewRowState
    {
        public string Id;
        public string Text;
        public bool ShouldOverrideElement;
    }

    public static class ListViewStatefulDemoFunc
    {
        public static VirtualNode Render(
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children
        )
        {
            var items =
                props != null
                && props.TryGetValue("items", out var itemsObj)
                && itemsObj is IReadOnlyList<ListViewRowState> typedItems
                    ? typedItems
                    : Array.Empty<ListViewRowState>();

            var addItem =
                props != null
                && props.TryGetValue("addItem", out var addItemObj)
                && addItemObj is Action addAction
                    ? addAction
                    : null;

            var setTopItem =
                props != null
                && props.TryGetValue("setTopItem", out var setTopObj)
                && setTopObj is Action setTopAction
                    ? setTopAction
                    : null;

            var deleteLast =
                props != null
                && props.TryGetValue("deleteLast", out var deleteObj)
                && deleteObj is Action deleteAction
                    ? deleteAction
                    : null;

            var rowRenderer = Hooks.UseMemo(
                () =>
                    (Func<int, object, VirtualNode>)(
                        (index, obj) =>
                        {
                            var r = obj as ListViewRowState;
                            if (r == null)
                                return V.Label(
                                    new LabelProps { Text = "<invalid>" },
                                    key: $"lv-invalid-{index}"
                                );
                            var id = !string.IsNullOrEmpty(r.Id) ? r.Id : index.ToString();
                            var funcKey = $"lv-row-{id}";
                            var childrenNode = r.ShouldOverrideElement
                                ? V.Label(new LabelProps { Text = r.Text ?? "<null>" }, funcKey)
                                : V.Func(IntroCounterFunc.Render, null, funcKey);
                            return V.VisualElement(null, key: $"lv-wrap-{id}", childrenNode);
                        }
                    ),
                items
            );

            Hooks.UseEffect(
                () =>
                {
                    try
                    {
                        if (
                            props != null
                            && props.TryGetValue("onCountChanged", out var oc)
                            && oc is Action<int> cb
                        )
                        {
                            cb(items?.Count ?? 0);
                        }
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
                    (ReactiveUITK.Props.Typed.StyleKeys.FlexGrow, 1f),
                    (
                        ReactiveUITK.Props.Typed.StyleKeys.BackgroundColor,
                        new UnityEngine.Color(0.15f, 0.15f, 0.15f, 1f)
                    ),
                },
            };

            Action Safe(Action candidate) => candidate ?? (() => { });

            var controls = V.VisualElement(
                new Dictionary<string, object>
                {
                    {
                        "style",
                        new Style
                        {
                            (ReactiveUITK.Props.Typed.StyleKeys.FlexDirection, "row"),
                            (ReactiveUITK.Props.Typed.StyleKeys.FlexShrink, 0f),
                            (ReactiveUITK.Props.Typed.StyleKeys.MarginBottom, 6f),
                        }
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
