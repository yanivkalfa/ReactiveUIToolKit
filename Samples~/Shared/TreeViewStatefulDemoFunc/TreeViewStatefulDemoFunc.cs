using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;
using static ReactiveUITK.Props.Typed.StyleKeys;

namespace ReactiveUITK.Samples.Shared
{
    public static class TreeViewStatefulDemoFunc
    {
        public sealed class Props : IProps
        {
            public IReadOnlyList<TreeViewRowState> Rows { get; set; }
            public Action AddParent { get; set; }
            public Action AddChild { get; set; }
            public Action SetParent { get; set; }
            public Action SetChild { get; set; }
            public Action DeleteLast { get; set; }
            public IList<int> ExpandedItemIds { get; set; }
            public Delegate OnExpandedChanged { get; set; }
            public Action<int> OnCountChanged { get; set; }
        }

        public static VirtualNode Render(
            IProps rawProps,
            IReadOnlyList<VirtualNode> children
        )
        {
            var p = rawProps as Props;
            var rows = p?.Rows ?? Array.Empty<TreeViewRowState>();
            var addParent = p?.AddParent;
            var addChild = p?.AddChild;
            var setParent = p?.SetParent;
            var setChild = p?.SetChild;
            var deleteLast = p?.DeleteLast;
            var expandedItemIds = p?.ExpandedItemIds != null ? new List<int>(p.ExpandedItemIds) : null;
            Delegate expandedChanged = p?.OnExpandedChanged;

            var rootItems = Hooks.UseMemo(
                () =>
                {
                    var combined = new List<TreeViewItemData<object>>();
                    if (rows == null)
                    {
                        return combined;
                    }
                    for (int i = 0; i < rows.Count; i++)
                    {
                        var row = rows[i];
                        if (row == null)
                        {
                            continue;
                        }
                        var baseId = row.Pid != 0 ? row.Pid : 1000 + (i * 2);
                        List<TreeViewItemData<object>> ch = null;
                        if (row.HasChild)
                        {
                            ch = new List<TreeViewItemData<object>>
                            {
                                new TreeViewItemData<object>(
                                    baseId + 1,
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
                                baseId,
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
                },
                new object[] { rows }
            );

            var rowRenderer = Hooks.UseMemo(
                () =>
                    (Func<int, object, VirtualNode>)(
                        (i, obj) =>
                        {
                            var row = obj as SharedTreeRowItem;
                            if (row == null)
                            {
                                return V.Label(
                                    new LabelProps { Text = "<invalid row payload>" },
                                    $"tv-invalid-{i}"
                                );
                            }

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
                    {
                        continue;
                    }
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
                            p?.OnCountChanged?.Invoke(countValue);
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
                new VisualElementProps { Style = new Style { (StyleKeys.FlexDirection, "row"), (MarginBottom, 6f) } },
                null,
                V.Button(new ButtonProps { Text = "Add Parent", OnClick = Safe(addParent) }),
                V.Button(new ButtonProps { Text = "Add Child", OnClick = Safe(addChild) }),
                V.Button(new ButtonProps { Text = "Set Parent", OnClick = Safe(setParent) }),
                V.Button(new ButtonProps { Text = "Set Child", OnClick = Safe(setChild) }),
                V.Button(new ButtonProps { Text = "Delete Last", OnClick = Safe(deleteLast) })
            );

            var tvProps = new TreeViewProps
            {
                RootItems = rootItems,
                Selection = SelectionType.None,
                FixedItemHeight = 20f,
                Row = rowRenderer,
                ExpandedItemIds = expandedItemIds,
                ItemExpandedChanged = expandedChanged,
                Style = new Style { (MarginBottom, 30f) },
            };
            return V.VisualElement(null, null, btnRow, V.TreeView(tvProps));
        }
    }
}
