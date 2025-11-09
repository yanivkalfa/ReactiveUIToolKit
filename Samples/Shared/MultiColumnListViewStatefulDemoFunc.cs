using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

namespace ReactiveUITK.Samples.Shared
{
    public sealed class MultiColumnListViewRowState
    {
        public string Id;
        public string Text;
        public bool ShouldOverrideElement;
    }

    public static class MultiColumnListViewStatefulDemoFunc
    {
        public static VirtualNode Render(
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children
        )
        {
            static Dictionary<string, T> ExtractDict<T>(object source)
            {
                if (source is Dictionary<string, T> direct)
                    return direct;
                if (source is IDictionary<string, T> dict)
                    return new Dictionary<string, T>(dict);
                if (source is IReadOnlyDictionary<string, T> ro)
                    return new Dictionary<string, T>(ro);
                if (source is IDictionary<string, object> objMap)
                {
                    var result = new Dictionary<string, T>();
                    foreach (var kv in objMap)
                    {
                        try
                        {
                            if (kv.Value is T tv)
                            {
                                result[kv.Key] = tv;
                            }
                            else if (kv.Value != null)
                            {
                                var converted = (T)Convert.ChangeType(kv.Value, typeof(T));
                                result[kv.Key] = converted;
                            }
                        }
                        catch { }
                    }
                    return result;
                }
                return null;
            }

            var items =
                props != null
                && props.TryGetValue("items", out var itemsObj)
                && itemsObj is IReadOnlyList<MultiColumnListViewRowState> typedItems
                    ? typedItems
                    : Array.Empty<MultiColumnListViewRowState>();

            var sortDefs =
                props != null
                && props.TryGetValue("sortDefs", out var sortObj)
                && sortObj is List<MultiColumnListViewProps.SortedColumnDef> typedSort
                    ? typedSort
                    : null;

            var columnWidths =
                props != null
                && props.TryGetValue("columnWidths", out var widthsObj)
                    ? ExtractDict<float>(widthsObj)
                    : null;

            var columnVisibility =
                props != null
                && props.TryGetValue("columnVisibility", out var visibilityObj)
                    ? ExtractDict<bool>(visibilityObj)
                    : null;

            var columnDisplayIndex =
                props != null
                && props.TryGetValue("columnDisplayIndex", out var displayObj)
                    ? ExtractDict<int>(displayObj)
                    : null;

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

            var sortChanged =
                props != null
                && props.TryGetValue("onSortChanged", out var sortChangedObj)
                && sortChangedObj is Action<List<MultiColumnListViewProps.SortedColumnDef>> sortChangedAction
                    ? sortChangedAction
                    : null;

            Delegate columnLayoutChanged = null;
            if (props != null && props.TryGetValue("onLayoutChanged", out var layoutChangedObj))
            {
                if (layoutChangedObj is Delegate del)
                {
                    columnLayoutChanged = del;
                }
                else if (
                    layoutChangedObj
                    is Hooks.StateSetter<MultiColumnListViewProps.ColumnLayoutState> setter
                )
                {
                    columnLayoutChanged = setter.ToValueAction();
                }
            }

            var columns = Hooks.UseMemo(
                () =>
                    new List<MultiColumnListViewProps.ColumnDef>
                    {
                        new()
                        {
                            Name = "ID",
                            Title = "ID",
                            Width = 140f,
                            MinWidth = 100f,
                            Resizable = true,
                            Stretchable = true,
                            Sortable = true,
                            Cell = (i, obj) =>
                            {
                                var it = obj as MultiColumnListViewRowState;
                                var id = it?.Id ?? i.ToString();
                                var shortId = id.Length > 6 ? id.Substring(0, 6) : id;
                                return V.Label(
                                    new LabelProps { Text = shortId },
                                    key: $"id-cell-{id}"
                                );
                            },
                        },
                        new()
                        {
                            Name = "Text",
                            Title = "Text",
                            Width = 260f,
                            MinWidth = 140f,
                            Resizable = true,
                            Stretchable = true,
                            Sortable = true,
                            Cell = (i, obj) =>
                            {
                                var it = obj as MultiColumnListViewRowState;
                                if (it == null)
                                    return V.Label(
                                        new LabelProps { Text = "<invalid>" },
                                        key: $"invalid-{i}"
                                    );
                                var id = it.Id ?? i.ToString();
                                var funcKey = $"mclv-row-{id}";
                                var childrenNode = it.ShouldOverrideElement
                                    ? V.Label(
                                        new LabelProps { Text = it.Text ?? "<null>" },
                                        funcKey
                                    )
                                    : V.Func(IntroCounterFunc.Render, null, funcKey);
                                return V.VisualElement(null, key: $"row-wrap-{id}", childrenNode);
                            },
                        },
                    },
                items
            );

            var displayed = Hooks.UseMemo(
                () =>
                {
                    var defsTree =
                        sortDefs == null
                            ? null
                            : new List<MultiColumnTreeViewProps.SortedColumnDef>(sortDefs.Count);
                    if (sortDefs != null)
                    {
                        foreach (var d in sortDefs)
                        {
                            defsTree.Add(
                                new MultiColumnTreeViewProps.SortedColumnDef
                                {
                                    Name = d?.Name,
                                    Direction = d?.Direction,
                                    Index = d?.Index,
                                }
                            );
                        }
                    }
                    return ReactiveUITK.Shared.Util.SortUtils.MultiSort(
                        defsTree,
                        items ?? new List<MultiColumnListViewRowState>(),
                        (r, col) =>
                            string.Equals(col, "ID", StringComparison.OrdinalIgnoreCase) ? r?.Id
                            : string.Equals(col, "Text", StringComparison.OrdinalIgnoreCase)
                                ? r?.Text
                            : null
                    );
                },
                new object[] { items, sortDefs }
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

            var propsMap = new MultiColumnListViewProps
            {
                Items = displayed,
                Selection = SelectionType.None,
                FixedItemHeight = 20f,
                Columns = columns,
                SortingMode = ColumnSortingMode.Custom,
                SortedColumns = sortDefs,
                ColumnSortingChanged = sortChanged ?? (_ => { }),
                ColumnWidths = columnWidths,
                ColumnVisibility = columnVisibility,
                ColumnDisplayIndex = columnDisplayIndex,
                ColumnLayoutChanged = columnLayoutChanged,
                Style = new Style { (ReactiveUITK.Props.Typed.StyleKeys.MarginBottom, 30f) },
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
                            (ReactiveUITK.Props.Typed.StyleKeys.MarginBottom, 6f),
                            (ReactiveUITK.Props.Typed.StyleKeys.FlexShrink, 0f),
                        }
                    },
                },
                key: "controls",
                V.Button(new ButtonProps { Text = "Add Parent", OnClick = Safe(addItem) }, key: "btn-add"),
                V.Button(new ButtonProps { Text = "Set Value", OnClick = Safe(setTopItem) }, key: "btn-set"),
                V.Button(new ButtonProps { Text = "Delete Last", OnClick = Safe(deleteLast) }, key: "btn-delete")
            );

            return V.VisualElement(
                null,
                key: "mclv-root",
                controls,
                V.MultiColumnListView(propsMap, key: "mclv-list")
            );
        }
    }
}
