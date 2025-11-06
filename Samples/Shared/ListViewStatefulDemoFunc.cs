using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

namespace ReactiveUITK.Samples.Shared
{
    public static class ListViewStatefulDemoFunc
    {
        private sealed class Row
        {
            public string Id;
            public string Text;
            public bool ShouldOverrideElement;
        }

        public static VirtualNode Render(
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children
        )
        {
            var initial = Hooks.UseMemo(() =>
            {
                // Start with 0 rows
                return new List<Row>();
            });

            var (items, setItems) = Hooks.UseState(initial);

            var rowRenderer = Hooks.UseMemo(
                () =>
                    (Func<int, object, VirtualNode>)(
                        (index, obj) =>
                        {
                            var r = obj as Row;
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

            // Notify parent only when count changes to avoid render loops (moved below memo to preserve original hook order signature)
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

            var listProps = new ListViewProps
            {
                Items = items,
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

            var btnAdd = new ButtonProps
            {
                Text = "Add",
                OnClick = () =>
                {
                    var copy = new List<Row>(items.Count + 1);
                    copy.Add(new Row { Id = System.Guid.NewGuid().ToString("N"), Text = "Parent" });
                    copy.AddRange(items);
                    setItems.Set(copy);
                },
            };
            var btnSetLast = new ButtonProps
            {
                Text = "Set Value",
                OnClick = () =>
                {
                    if (items.Count == 0)
                        return;
                    var copy = new List<Row>(items);
                    // New rows are added to the top; update the most recently added (index 0)
                    var top = copy[0];
                    top.Text = $"{top.Id} {System.DateTime.Now:HH:mm:ss}";
                    top.ShouldOverrideElement = true;
                    copy[0] = top;
                    setItems.Set(copy);
                },
            };
            var btnDeleteLast = new ButtonProps
            {
                Text = "Delete Last",
                OnClick = () =>
                {
                    if (items.Count == 0)
                        return;
                    var copy = new List<Row>(items);
                    copy.RemoveAt(copy.Count - 1);
                    setItems.Set(copy);
                },
            };

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
                V.Button(btnAdd),
                V.Button(btnSetLast),
                V.Button(btnDeleteLast)
            );

            return V.VisualElement(null, null, controls, V.ListView(listProps));
        }
    }
}
