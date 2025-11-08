using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;
using static ReactiveUITK.Props.Typed.StyleKeys;

namespace ReactiveUITK.Samples.Shared
{
    public sealed class TreeViewRowState
    {
        public SharedTreeRowItem Parent;
        public SharedTreeRowItem Child;
        public bool HasChild;
    }

    public static class TreeViewStatefulDemoFunc
    {
        public static VirtualNode Render(
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children
        )
        {
            var rows =
                props != null
                && props.TryGetValue("rows", out var rowsObj)
                && rowsObj is IReadOnlyList<TreeViewRowState> typedRows
                    ? typedRows
                    : Array.Empty<TreeViewRowState>();

            var addParent =
                props != null
                && props.TryGetValue("addParent", out var addParentObj)
                && addParentObj is Action addParentAction
                    ? addParentAction
                    : null;

            var addChild =
                props != null
                && props.TryGetValue("addChild", out var addChildObj)
                && addChildObj is Action addChildAction
                    ? addChildAction
                    : null;

            var setParent =
                props != null
                && props.TryGetValue("setParent", out var setParentObj)
                && setParentObj is Action setParentAction
                    ? setParentAction
                    : null;

            var setChild =
                props != null
                && props.TryGetValue("setChild", out var setChildObj)
                && setChildObj is Action setChildAction
                    ? setChildAction
                    : null;

            var deleteLast =
                props != null
                && props.TryGetValue("deleteLast", out var deleteObj)
                && deleteObj is Action deleteAction
                    ? deleteAction
                    : null;

            Func<List<TreeViewItemData<object>>> buildRoots = () =>
            {
                var combined = new List<TreeViewItemData<object>>();
                for (int i = 0; i < rows.Count; i++)
                {
                    var row = rows[i];
                    if (row == null)
                    {
                        continue;
                    }

                    int pid = 1000 + (i * 2);
                    List<TreeViewItemData<object>> ch = null;
                    if (row.HasChild)
                    {
                        ch = new List<TreeViewItemData<object>>
                        {
                            new TreeViewItemData<object>(
                                pid + 1,
                                row.Child
                                    ?? new SharedTreeRowItem
                                    {
                                        Id = Guid.NewGuid().ToString("N"),
                                        Text = "Child",
                                        IsChild = true,
                                    },
                                null
                            ),
                        };
                    }
                    combined.Add(
                        new TreeViewItemData<object>(
                            pid,
                            row.Parent
                                ?? new SharedTreeRowItem
                                {
                                    Id = Guid.NewGuid().ToString("N"),
                                    Text = "Parent",
                                },
                            ch
                        )
                    );
                }
                return combined;
            };

            var rowRenderer = Hooks.UseMemo(
                () =>
                    (Func<int, object, VirtualNode>)(
                        (i, obj) =>
                        {
                            var row = obj as SharedTreeRowItem;
                            if (row == null)
                                return V.Label(
                                    new LabelProps { Text = "<invalid row payload>" },
                                    $"tv-invalid-{i}"
                                );

                            var id = !string.IsNullOrEmpty(row.Id) ? row.Id : i.ToString();
                            var prefix = (row.IsChild == true) ? "child" : "parent";
                            var funcKey = $"tv-{prefix}-{id}";

                            var childNode = row.ShouldOverrideElement
                                ? V.Label(new LabelProps { Text = row.Text ?? "<null>" }, funcKey)
                                : V.Func(IntroCounterFunc.Render, null, funcKey);

                            return V.VisualElement(null, key: $"tv-wrap-{prefix}-{id}", childNode);
                        }
                    ),
                rows
            );

            try
            {
                int countValue = 0;
                for (int i = 0; i < rows.Count; i++)
                {
                    var row = rows[i];
                    if (row == null)
                        continue;
                    countValue += 1;
                    if (row.HasChild)
                    {
                        countValue += 1;
                    }
                }
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
                                cb(countValue);
                            }
                        }
                        catch { }
                        return null;
                    },
                    new object[] { countValue }
                );
            }
            catch { }

            Action Safe(Action candidate) => candidate ?? (() => { });

            var btnRow = V.VisualElement(
                new Style { (StyleKeys.FlexDirection, "row"), (MarginBottom, 6f) },
                null,
                V.Button(new ButtonProps { Text = "Add Parent", OnClick = Safe(addParent) }),
                V.Button(new ButtonProps { Text = "Add Child", OnClick = Safe(addChild) }),
                V.Button(new ButtonProps { Text = "Set Parent", OnClick = Safe(setParent) }),
                V.Button(new ButtonProps { Text = "Set Child", OnClick = Safe(setChild) }),
                V.Button(new ButtonProps { Text = "Delete Last", OnClick = Safe(deleteLast) })
            );

            var tvProps = new TreeViewProps
            {
                RootItems = buildRoots(),
                Selection = SelectionType.None,
                FixedItemHeight = 20f,
                Row = rowRenderer,
                Style = new Style { (MarginBottom, 30f) },
            };
            return V.VisualElement(null, null, btnRow, V.TreeView(tvProps));
        }
    }
}
