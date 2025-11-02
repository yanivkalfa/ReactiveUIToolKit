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

            // Notify parent only when count changes to avoid render loops
            Hooks.UseEffect(
                () =>
                {
                    try
                    {
                        if (props != null
                            && props.TryGetValue("onCountChanged", out var oc)
                            && oc is Action<int> cb)
                        {
                            cb(items?.Count ?? 0);
                        }
                    }
                    catch { }
                    return null;
                },
                new object[] { items?.Count ?? 0 }
            );

            var rowRenderer = Hooks.UseMemo(() =>
                (Func<int, object, VirtualNode>)((index, obj) =>
                {
                    var r = obj as Row;
                    if (r == null)
                        return V.Label(new LabelProps { Text = "<invalid>" });
                    var id = !string.IsNullOrEmpty(r.Id) ? r.Id : index.ToString();
                    var funcKey = $"lv-row-{id}";
                    var childrenNode = r.ShouldOverrideElement
                        ? V.Label(new LabelProps { Text = r.Text ?? "<null>" }, funcKey)
                        : V.Func(IntroCounterFunc.Render, null, funcKey);
                    // Always wrap in a VisualElement (matches TreeView usage and stabilizes row root)
                    return V.VisualElement(null, null, childrenNode);
                })
            , items);

            var listProps = new ListViewProps
            {
                Items = items,
                FixedItemHeight = 20f,
                Selection = SelectionType.None,
                Row = rowRenderer,
            };

            var btnAdd = new ButtonProps
            {
                Text = "Add",
                OnClick = () =>
                {
                    var copy = new List<Row>(items.Count + 1);
                    copy.Add(new Row { Id = System.Guid.NewGuid().ToString("N"), Text = "Parent" });
                    copy.AddRange(items);
                    setItems(copy);
                }
            };
            var btnSetLast = new ButtonProps
            {
                Text = "Set Value",
                OnClick = () =>
                {
                    if (items.Count == 0) return;
                    var copy = new List<Row>(items);
                    // New rows are added to the top; update the most recently added (index 0)
                    var top = copy[0];
                    top.Text = $"{top.Id} {System.DateTime.Now:HH:mm:ss}";
                    top.ShouldOverrideElement = true;
                    copy[0] = top;
                    setItems(copy);
                }
            };
            var btnDeleteLast = new ButtonProps
            {
                Text = "Delete Last",
                OnClick = () =>
                {
                    if (items.Count == 0) return;
                    var copy = new List<Row>(items);
                    copy.RemoveAt(copy.Count - 1);
                    setItems(copy);
                }
            };

            var controls = V.VisualElement(
                new Dictionary<string, object>
                {
                    { "style", new Style { (ReactiveUITK.Props.Typed.StyleKeys.FlexDirection, "row") } }
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
