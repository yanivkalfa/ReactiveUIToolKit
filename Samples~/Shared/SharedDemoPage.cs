using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;
using static ReactiveUITK.Props.Typed.StyleKeys;
using UColor = UnityEngine.Color;

namespace ReactiveUITK.Samples.Shared
{
    public static class SharedDemoPage
    {
        private static readonly Style TopBarStyle = new()
        {
            (StyleKeys.FlexDirection, "row"),
            (JustifyContent, "space-between"),
            (AlignItems, "center"),
            (FlexGrow, 1f),
            (PaddingLeft, 12f),
            (PaddingRight, 12f),
            (PaddingTop, 8f),
            (PaddingBottom, 8f),
            (BorderBottomWidth, 1f),
            (BorderBottomColor, new Color(0.85f, 0.85f, 0.85f, 1f)),
        };

        private static readonly Style LeftBoxStyle = new()
        {
            (BackgroundColor, new Color(0.2f, 0.4f, 0.9f, 1f)),
            (TextColor, UColor.white),
            (PaddingLeft, 10f),
            (PaddingRight, 10f),
            (PaddingTop, 6f),
            (PaddingBottom, 6f),
            (BorderRadius, 4f),
            (FontSize, 14f),
        };

        private static readonly Style RightBoxStyle = new()
        {
            (BackgroundColor, new UColor(0.9f, 0.3f, 0.2f, 1f)),
            (TextColor, UColor.white),
            (PaddingLeft, 10f),
            (PaddingRight, 10f),
            (PaddingTop, 6f),
            (PaddingBottom, 6f),
            (BorderRadius, 4f),
            (FontSize, 14f),
        };

        private static readonly Style TextInputStyle = new()
        {
            (FlexGrow, 1f),
            (MarginLeft, 8f),
            (MarginRight, 8f),
            (PaddingLeft, 6f),
            (PaddingRight, 6f),
            (PaddingTop, 4f),
            (PaddingBottom, 4f),
            (BorderRadius, 4f),
            (BorderWidth, 1f),
            (TextColor, UColor.black),
            (BorderColor, new UColor(0.8f, 0.8f, 0.8f, 1f)),
            (BackgroundColor, new UColor(1f, 1f, 1f, 1f)),
        };

        private static readonly Style PageStyle = new()
        {
            (StyleKeys.FlexDirection, "column"),
            (FlexGrow, 1f),
            (JustifyContent, "space-between"),
            (BackgroundColor, new UColor(0.95f, 0.95f, 0.95f, 1f)),
        };

        private static readonly Style ExtrasContainerStyle = new()
        {
            (MarginTop, 12f),
            (PaddingLeft, 12f),
            (PaddingRight, 12f),
            (PaddingTop, 8f),
            (PaddingBottom, 8f),
            (BackgroundColor, UColor.white),
            (BorderTopWidth, 1f),
            (BorderTopColor, new UColor(0.85f, 0.85f, 0.85f, 1f)),
            (StyleKeys.FlexDirection, "column"),
        };

        public static VirtualNode Render(
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children
        )
        {
            static Dictionary<string, T> CloneDict<T>(IReadOnlyDictionary<string, T> source)
            {
                if (source == null)
                {
                    return null;
                }
                if (source.Count == 0)
                {
                    return new Dictionary<string, T>();
                }
                return new Dictionary<string, T>(source);
            }

            static bool DictEqual<T>(
                IReadOnlyDictionary<string, T> left,
                IReadOnlyDictionary<string, T> right
            )
            {
                if (ReferenceEquals(left, right))
                {
                    return true;
                }
                if (left == null || right == null)
                {
                    return false;
                }
                if (left.Count != right.Count)
                {
                    return false;
                }
                foreach (var kv in left)
                {
                    if (!right.TryGetValue(kv.Key, out var rv))
                    {
                        return false;
                    }
                    if (!EqualityComparer<T>.Default.Equals(kv.Value, rv))
                    {
                        return false;
                    }
                }
                return true;
            }

            static MultiColumnListViewProps.ColumnLayoutState CloneListLayout(
                MultiColumnListViewProps.ColumnLayoutState layout
            )
            {
                if (layout == null)
                {
                    return null;
                }
                return new MultiColumnListViewProps.ColumnLayoutState
                {
                    ColumnWidths = CloneDict(layout.ColumnWidths),
                    ColumnVisibility = CloneDict(layout.ColumnVisibility),
                    ColumnDisplayIndex = CloneDict(layout.ColumnDisplayIndex),
                };
            }

            static bool ListLayoutEqual(
                MultiColumnListViewProps.ColumnLayoutState a,
                MultiColumnListViewProps.ColumnLayoutState b
            )
            {
                return DictEqual(a?.ColumnWidths, b?.ColumnWidths)
                    && DictEqual(a?.ColumnVisibility, b?.ColumnVisibility)
                    && DictEqual(a?.ColumnDisplayIndex, b?.ColumnDisplayIndex);
            }

            static MultiColumnTreeViewProps.ColumnLayoutState CloneTreeLayout(
                MultiColumnTreeViewProps.ColumnLayoutState layout
            )
            {
                if (layout == null)
                {
                    return null;
                }
                return new MultiColumnTreeViewProps.ColumnLayoutState
                {
                    ColumnWidths = CloneDict(layout.ColumnWidths),
                    ColumnVisibility = CloneDict(layout.ColumnVisibility),
                    ColumnDisplayIndex = CloneDict(layout.ColumnDisplayIndex),
                };
            }

            static bool TreeLayoutEqual(
                MultiColumnTreeViewProps.ColumnLayoutState a,
                MultiColumnTreeViewProps.ColumnLayoutState b
            )
            {
                return DictEqual(a?.ColumnWidths, b?.ColumnWidths)
                    && DictEqual(a?.ColumnVisibility, b?.ColumnVisibility)
                    && DictEqual(a?.ColumnDisplayIndex, b?.ColumnDisplayIndex);
            }

            static HashSet<int> BuildTreeValidIds(IReadOnlyList<TreeViewRowState> rows)
            {
                var set = new HashSet<int>();
                if (rows == null)
                {
                    return set;
                }
                for (int i = 0; i < rows.Count; i++)
                {
                    var row = rows[i];
                    int baseId = row != null && row.Pid != 0 ? row.Pid : 1000 + (i * 2);
                    if (baseId != 0)
                    {
                        set.Add(baseId);
                    }
                    if (row?.HasChild == true)
                    {
                        set.Add(baseId + 1);
                    }
                }
                return set;
            }

            static List<int> PruneTreeExpandedIds(
                IReadOnlyList<TreeViewRowState> rows,
                IList<int> expanded
            )
            {
                if (expanded == null)
                {
                    return null;
                }
                var valid = BuildTreeValidIds(rows);
                if (expanded.Count == 0)
                {
                    return expanded as List<int> ?? new List<int>();
                }
                var nextSet = new HashSet<int>();
                bool changed = false;
                for (int i = 0; i < expanded.Count; i++)
                {
                    var id = expanded[i];
                    if (valid.Contains(id))
                    {
                        nextSet.Add(id);
                    }
                    else
                    {
                        changed = true;
                    }
                }
                if (
                    !changed
                    && expanded is List<int> existingList
                    && existingList.Count == nextSet.Count
                )
                {
                    return existingList;
                }
                var nextList = new List<int>(nextSet);
                nextList.Sort();
                return nextList;
            }

            var (inputText, setInputText) = Hooks.UseState(string.Empty);
            var (isOptionEnabled, setOptionEnabled) = Hooks.UseState(false);
            var (isRadioSingleSelected, setRadioSingleSelected) = Hooks.UseState(false);
            var selectionChoices = Hooks.UseMemo(
                () => new List<string> { "One", "Two", "Three" },
                0
            );
            var (selectionIndex, setSelectionIndex) = Hooks.UseState(0);
            var (repeatClickCount, setRepeatClickCount) = Hooks.UseState(0);
            var (currentTime, setCurrentTime) = Hooks.UseState(DateTime.Now);
            var (sliderValue, setSliderValue) = Hooks.UseState(0.5f);
            var (sliderIntValue, setSliderIntValue) = Hooks.UseState(5);
            var (treeTabIndex, setTreeTabIndex) = Hooks.UseState(0);
            var (listTabIndex, setListTabIndex) = Hooks.UseState(0);
            var ddChoices = Hooks.UseMemo(() => new List<string> { "Alpha", "Beta", "Gamma" }, 0);
            var (ddValue, setDdValue) = Hooks.UseState("Beta");
            var (foldoutOpen, setFoldoutOpen) = Hooks.UseState(true);
            var rootElement = Hooks.UseRef();
            var (treeDisplayCount, setTreeDisplayCount) = Hooks.UseState(0);
            var (mctvDisplayCount, setMctvDisplayCount) = Hooks.UseState(0);
            var (mclvDisplayCount, setMclvDisplayCount) = Hooks.UseState(0);
            var (simpleListCount, setSimpleListCount) = Hooks.UseState(0);
            var (showTreeTabs, setShowTreeTabs) = Hooks.UseState(true);
            var (showListTabs, setShowListTabs) = Hooks.UseState(true);
            var (batchClicks, setBatchClicks) = Hooks.UseState(0);
            var (treeRows, setTreeRows) = Hooks.UseState(new List<TreeViewRowState>());
            var (treeExpandedIds, setTreeExpandedIds) = Hooks.UseState(new List<int>());
            var (_, setTreeNextPid) = Hooks.UseState(1000);
            var (mctvRows, setMctvRows) = Hooks.UseState(new List<MultiColumnTreeViewRowState>());
            var (mctvNextPid, setMctvNextPid) = Hooks.UseState(2000);
            var (mctvSortDefs, setMctvSortDefs) = Hooks.UseState<
                List<MultiColumnTreeViewProps.SortedColumnDef>
            >(null);
            var (listRows, setListRows) = Hooks.UseState(new List<ListViewRowState>());
            var (mclvRows, setMclvRows) = Hooks.UseState(new List<MultiColumnListViewRowState>());
            var (mclvSortDefs, setMclvSortDefs) = Hooks.UseState<
                List<MultiColumnListViewProps.SortedColumnDef>
            >(null);
            var (mctvLayout, setMctvLayout) =
                Hooks.UseState<MultiColumnTreeViewProps.ColumnLayoutState>(null);
            var (mclvLayout, setMclvLayout) =
                Hooks.UseState<MultiColumnListViewProps.ColumnLayoutState>(null);

            TextFieldProps inputTextFieldProps = new()
            {
                Style = TextInputStyle,
                Placeholder = "Type here...",
                HidePlaceholderOnFocus = false,
                Value = inputText,
                LabelText = string.IsNullOrEmpty(inputText) ? string.Empty : "Value: " + inputText,
                OnChange = e => setInputText(e.newValue),
            };

            var outerWrapperStyle = new Style
            {
                (BackgroundColor, new UColor(0.2f, 0.4f, 0.8f, 1f)),
                (FlexGrow, 1f),
            };
            var safeWrapperStyle = new Style
            {
                (BackgroundColor, new UColor(0.2f, 0.6f, 0.2f, 1f)),
                (FlexGrow, 1f),
                (StyleKeys.FlexDirection, "column"),
            };
            var barSlotStyle = new Style { (FlexShrink, 0f), (MinHeight, 110f) };
            var mainScrollStyle = new Style { (FlexGrow, 1f), (PaddingBottom, 20f) };
            var groupBox1ContentStyle = new Style { (PaddingLeft, 6f), (PaddingTop, 4f) };
            var newCompsGroupContentStyle = new Style
            {
                (PaddingLeft, 8f),
                (PaddingRight, 8f),
                (PaddingTop, 6f),
                (PaddingBottom, 6f),
            };
            var foldoutHeaderStyle = new Style
            {
                (BackgroundColor, new UColor(0.95f, 0.95f, 0.95f, 1f)),
                (PaddingLeft, 4f),
                (PaddingTop, 2f),
                (PaddingBottom, 2f),
            };
            var foldoutContentStyle = new Style { (PaddingLeft, 6f) };
            var imageDemoStyle = new Style
            {
                (Width, 96f),
                (Height, 96f),
                (BackgroundColor, new UColor(0.7f, 0.85f, 1f, 1f)),
                (BorderRadius, 6f),
            };
            var sliderWidthStyle = new Style { (Width, 200f) };

            var outerWrapperProps = new Dictionary<string, object>
            {
                { "style", outerWrapperStyle },
            };

            var groupBox1Props = new GroupBoxProps
            {
                Text = "GroupBox",
                ContentContainer = new Dictionary<string, object>
                {
                    { "style", groupBox1ContentStyle },
                },
            };

            var newComponentsGroupProps = new GroupBoxProps
            {
                Text = "New Components",
                ContentContainer = new Dictionary<string, object>
                {
                    { "style", newCompsGroupContentStyle },
                },
            };

            var foldoutProps = new FoldoutProps
            {
                Text = "More settings",
                Value = foldoutOpen,
                OnChange = e => setFoldoutOpen(e.newValue),
                Header = new Dictionary<string, object> { { "style", foldoutHeaderStyle } },
                ContentContainer = new Dictionary<string, object>
                {
                    { "style", foldoutContentStyle },
                },
            };

            var imageProps = new ImageProps { Style = imageDemoStyle };

            var sliderProps = new SliderProps
            {
                LowValue = 0f,
                HighValue = 1f,
                Value = sliderValue,
                Direction = "horizontal",
                OnChange = e => setSliderValue(e.newValue),
                Style = sliderWidthStyle,
            };

            var dropdownProps = new DropdownFieldProps
            {
                Choices = ddChoices,
                Value = ddValue,
                OnChange = e => setDdValue(e.newValue),
            };

            var toggleProps = new ToggleProps
            {
                Text = "Enable option",
                Value = isOptionEnabled,
                OnChange = e => setOptionEnabled(e.newValue),
            };

            var radioSingleProps = new RadioButtonProps
            {
                Text = "Single radio",
                Value = isRadioSingleSelected,
                OnChange = e => setRadioSingleSelected(e.newValue),
            };

            var radioGroupProps = new RadioButtonGroupProps
            {
                Choices = selectionChoices,
                Index = selectionIndex,
                OnChange = e => setSelectionIndex(e.newValue),
            };

            var progressBarProps = new ProgressBarProps
            {
                Value = repeatClickCount % 100,
                Title = "Progress",
            };

            var repeatButtonProps = new RepeatButtonProps
            {
                Text = $"Repeat ({repeatClickCount})",
                OnClick = () => setRepeatClickCount(repeatClickCount + 1),
            };

            var batchTestButtonProps = new ButtonProps
            {
                Text = $"Batch Test ({batchClicks})",
                OnClick = () =>
                {
                    setBatchClicks(batchClicks + 1);
                    setRepeatClickCount.Set(value => value + 1);
                    setRepeatClickCount.Set(value => value + 1);
                    setSliderValue.Set(value => Mathf.Clamp01(value + 0.05f));
                    setSliderIntValue.Set(value => (value + 1) % 11);
                    setOptionEnabled(!isOptionEnabled);
                },
                Style = new Style { (MarginLeft, 6f) },
            };

            Hooks.UseEffect(
                () =>
                {
                    if (rootElement == null)
                    {
                        return null;
                    }

                    var timerHandle = rootElement
                        .schedule.Execute(() => setCurrentTime.Set(DateTime.Now))
                        .Every(1000);
                    return () =>
                    {
                        try
                        {
                            timerHandle?.Pause();
                        }
                        catch { }
                    };
                },
                Array.Empty<object>()
            );

            var setTextButtonProps = new ButtonProps
            {
                Text = "Set Text",
                OnClick = () => setInputText.Set("Updated!"),
                Style = new Style { (MarginLeft, 6f), (Height, 28f) },
            };

            Action treeAddParent = () =>
            {
                int assignedPid = 0;
                setTreeNextPid.Set(prev =>
                {
                    assignedPid = prev;
                    return prev + 2;
                });
                List<TreeViewRowState> latestRows = null;
                setTreeRows.Set(prev =>
                {
                    var next =
                        prev != null
                            ? new List<TreeViewRowState>(prev)
                            : new List<TreeViewRowState>();
                    next.Add(
                        new TreeViewRowState
                        {
                            Pid = assignedPid,
                            Parent = new SharedTreeRowItem
                            {
                                Id = Guid.NewGuid().ToString("N"),
                                Text = "Parent",
                            },
                            HasChild = false,
                        }
                    );
                    latestRows = next;
                    return next;
                });
                if (latestRows != null)
                {
                    setTreeExpandedIds.Set(prev => PruneTreeExpandedIds(latestRows, prev));
                }
            };

            Action treeAddChild = () =>
            {
                List<TreeViewRowState> latestRows = null;
                setTreeRows.Set(prev =>
                {
                    if (prev == null || prev.Count == 0)
                    {
                        return prev;
                    }
                    var source = prev[prev.Count - 1];
                    if (source == null)
                    {
                        return prev;
                    }
                    var next = new List<TreeViewRowState>(prev);
                    next[next.Count - 1] = new TreeViewRowState
                    {
                        Pid = source.Pid,
                        Parent = source.Parent,
                        Child =
                            source.Child
                            ?? new SharedTreeRowItem
                            {
                                Id = Guid.NewGuid().ToString("N"),
                                Text = "Child",
                                IsChild = true,
                            },
                        HasChild = true,
                    };
                    latestRows = next;
                    return next;
                });
                if (latestRows != null)
                {
                    setTreeExpandedIds.Set(prev => PruneTreeExpandedIds(latestRows, prev));
                }
            };

            Action treeSetParentValue = () =>
            {
                List<TreeViewRowState> latestRows = null;
                setTreeRows.Set(prev =>
                {
                    if (prev == null || prev.Count == 0)
                    {
                        return prev;
                    }
                    var source = prev[prev.Count - 1];
                    if (source == null)
                    {
                        return prev;
                    }
                    var parentItem =
                        source.Parent
                        ?? new SharedTreeRowItem { Id = Guid.NewGuid().ToString("N") };
                    parentItem.Text = $"{parentItem.Id} {DateTime.Now:HH:mm:ss}";
                    parentItem.ShouldOverrideElement = true;
                    var next = new List<TreeViewRowState>(prev);
                    next[next.Count - 1] = new TreeViewRowState
                    {
                        Pid = source.Pid,
                        Parent = parentItem,
                        Child = source.Child,
                        HasChild = source.HasChild,
                    };
                    latestRows = next;
                    return next;
                });
                if (latestRows != null)
                {
                    setTreeExpandedIds.Set(prev => PruneTreeExpandedIds(latestRows, prev));
                }
            };

            Action treeSetChildValue = () =>
            {
                List<TreeViewRowState> latestRows = null;
                setTreeRows.Set(prev =>
                {
                    if (prev == null || prev.Count == 0)
                    {
                        return prev;
                    }
                    var source = prev[prev.Count - 1];
                    if (source == null || !source.HasChild)
                    {
                        return prev;
                    }
                    var childItem =
                        source.Child
                        ?? new SharedTreeRowItem
                        {
                            Id = Guid.NewGuid().ToString("N"),
                            IsChild = true,
                        };
                    childItem.Text = $"{childItem.Id} {DateTime.Now:HH:mm:ss}";
                    childItem.ShouldOverrideElement = true;
                    var next = new List<TreeViewRowState>(prev);
                    next[next.Count - 1] = new TreeViewRowState
                    {
                        Pid = source.Pid,
                        Parent = source.Parent,
                        Child = childItem,
                        HasChild = true,
                    };
                    latestRows = next;
                    return next;
                });
                if (latestRows != null)
                {
                    setTreeExpandedIds.Set(prev => PruneTreeExpandedIds(latestRows, prev));
                }
            };

            Action treeDeleteLast = () =>
            {
                List<TreeViewRowState> latestRows = null;
                setTreeRows.Set(prev =>
                {
                    if (prev == null || prev.Count == 0)
                    {
                        return prev;
                    }
                    var next = new List<TreeViewRowState>(prev);
                    next.RemoveAt(next.Count - 1);
                    latestRows = next;
                    return next;
                });
                if (latestRows != null)
                {
                    setTreeExpandedIds.Set(prev => PruneTreeExpandedIds(latestRows, prev));
                }
            };
            Action<TreeViewExpansionChangedArgs> treeExpandedChanged = args =>
            {
                setTreeExpandedIds.Set(prev =>
                {
                    var nextSet = prev != null ? new HashSet<int>(prev) : new HashSet<int>();
                    if (args != null)
                    {
                        if (args.isExpanded)
                        {
                            nextSet.Add(args.id);
                        }
                        else
                        {
                            nextSet.Remove(args.id);
                        }
                    }
                    var valid = BuildTreeValidIds(treeRows);
                    if (valid.Count > 0)
                    {
                        var removals = new List<int>();
                        foreach (var id in nextSet)
                        {
                            if (!valid.Contains(id))
                            {
                                removals.Add(id);
                            }
                        }
                        for (int i = 0; i < removals.Count; i++)
                        {
                            nextSet.Remove(removals[i]);
                        }
                    }
                    var nextList = new List<int>(nextSet);
                    nextList.Sort();
                    if (prev != null && prev.Count == nextList.Count)
                    {
                        var prevSet = new HashSet<int>(prev);
                        if (prevSet.SetEquals(nextSet))
                        {
                            return prev;
                        }
                    }
                    return nextList;
                });
            };

            Action mctvAddParent = () =>
            {
                var pidBase = mctvNextPid;
                setMctvRows.Set(prev =>
                {
                    var next =
                        prev != null
                            ? new List<MultiColumnTreeViewRowState>(prev)
                            : new List<MultiColumnTreeViewRowState>();
                    next.Add(
                        new MultiColumnTreeViewRowState
                        {
                            Pid = pidBase,
                            Parent = new SharedTreeRowItem
                            {
                                Id = Guid.NewGuid().ToString("N"),
                                Text = "Parent",
                            },
                            HasChild = false,
                        }
                    );
                    return next;
                });
                setMctvNextPid(pidBase + 2);
            };

            Action mctvAddChild = () =>
            {
                setMctvRows.Set(prev =>
                {
                    if (prev == null || prev.Count == 0)
                    {
                        return prev;
                    }
                    var source = prev[prev.Count - 1];
                    if (source == null)
                    {
                        return prev;
                    }
                    var next = new List<MultiColumnTreeViewRowState>(prev);
                    next[next.Count - 1] = new MultiColumnTreeViewRowState
                    {
                        Pid = source.Pid,
                        Parent = source.Parent,
                        Child =
                            source.Child
                            ?? new SharedTreeRowItem
                            {
                                Id = Guid.NewGuid().ToString("N"),
                                Text = "Child",
                                IsChild = true,
                            },
                        HasChild = true,
                    };
                    return next;
                });
            };

            Action mctvSetParentValue = () =>
            {
                setMctvRows.Set(prev =>
                {
                    if (prev == null || prev.Count == 0)
                    {
                        return prev;
                    }
                    var source = prev[prev.Count - 1];
                    if (source == null)
                    {
                        return prev;
                    }
                    var parentItem =
                        source.Parent
                        ?? new SharedTreeRowItem { Id = Guid.NewGuid().ToString("N") };
                    parentItem.Text = $"{parentItem.Id} {DateTime.Now:HH:mm:ss}";
                    parentItem.ShouldOverrideElement = true;
                    var next = new List<MultiColumnTreeViewRowState>(prev);
                    next[next.Count - 1] = new MultiColumnTreeViewRowState
                    {
                        Pid = source.Pid,
                        Parent = parentItem,
                        Child = source.Child,
                        HasChild = source.HasChild,
                    };
                    return next;
                });
            };

            Action mctvSetChildValue = () =>
            {
                setMctvRows.Set(prev =>
                {
                    if (prev == null || prev.Count == 0)
                    {
                        return prev;
                    }
                    var source = prev[prev.Count - 1];
                    if (source == null || !source.HasChild)
                    {
                        return prev;
                    }
                    var childItem =
                        source.Child
                        ?? new SharedTreeRowItem
                        {
                            Id = Guid.NewGuid().ToString("N"),
                            IsChild = true,
                        };
                    childItem.Text = $"{childItem.Id} {DateTime.Now:HH:mm:ss}";
                    childItem.ShouldOverrideElement = true;
                    var next = new List<MultiColumnTreeViewRowState>(prev);
                    next[next.Count - 1] = new MultiColumnTreeViewRowState
                    {
                        Pid = source.Pid,
                        Parent = source.Parent,
                        Child = childItem,
                        HasChild = true,
                    };
                    return next;
                });
            };

            Action mctvDeleteLast = () =>
            {
                setMctvRows.Set(prev =>
                {
                    if (prev == null || prev.Count == 0)
                    {
                        return prev;
                    }
                    var next = new List<MultiColumnTreeViewRowState>(prev);
                    next.RemoveAt(next.Count - 1);
                    return next;
                });
            };

            Action<MultiColumnTreeViewProps.ColumnLayoutState> mctvLayoutChanged = layout =>
            {
                var clone = CloneTreeLayout(layout);
                if (TreeLayoutEqual(clone, mctvLayout))
                {
                    return;
                }
                setMctvLayout.Set(_ => clone);
            };

            Action<List<MultiColumnTreeViewProps.SortedColumnDef>> mctvSortChanged = defs =>
            {
                setMctvSortDefs(
                    defs != null ? new List<MultiColumnTreeViewProps.SortedColumnDef>(defs) : null
                );
            };

            Action listAddItem = () =>
            {
                setListRows.Set(prev =>
                {
                    var next = new List<ListViewRowState>
                    {
                        new ListViewRowState { Id = Guid.NewGuid().ToString("N"), Text = "Parent" },
                    };
                    if (prev != null)
                    {
                        for (int i = 0; i < prev.Count; i++)
                        {
                            next.Add(prev[i]);
                        }
                    }
                    return next;
                });
            };

            Action listSetTopItem = () =>
            {
                setListRows.Set(prev =>
                {
                    if (prev == null || prev.Count == 0)
                    {
                        return prev;
                    }
                    var source = prev[0];
                    if (source == null)
                    {
                        return prev;
                    }
                    var id = !string.IsNullOrEmpty(source.Id)
                        ? source.Id
                        : Guid.NewGuid().ToString("N");
                    var next = new List<ListViewRowState>(prev);
                    next[0] = new ListViewRowState
                    {
                        Id = id,
                        Text = $"{id} {DateTime.Now:HH:mm:ss}",
                        ShouldOverrideElement = true,
                    };
                    return next;
                });
            };

            Action listDeleteLast = () =>
            {
                setListRows.Set(prev =>
                {
                    if (prev == null || prev.Count == 0)
                    {
                        return prev;
                    }
                    var next = new List<ListViewRowState>(prev);
                    next.RemoveAt(next.Count - 1);
                    return next;
                });
            };

            Action mclvAddItem = () =>
            {
                setMclvRows.Set(prev =>
                {
                    var next = new List<MultiColumnListViewRowState>
                    {
                        new MultiColumnListViewRowState
                        {
                            Id = Guid.NewGuid().ToString("N"),
                            Text = "NEW " + DateTime.Now.ToLongTimeString(),
                        },
                    };
                    if (prev != null)
                    {
                        for (int i = 0; i < prev.Count; i++)
                        {
                            next.Add(prev[i]);
                        }
                    }
                    return next;
                });
            };

            Action mclvSetTopItem = () =>
            {
                setMclvRows.Set(prev =>
                {
                    if (prev == null || prev.Count == 0)
                    {
                        return prev;
                    }
                    var source = prev[0];
                    if (source == null)
                    {
                        return prev;
                    }
                    var id = !string.IsNullOrEmpty(source.Id)
                        ? source.Id
                        : Guid.NewGuid().ToString("N");
                    var next = new List<MultiColumnListViewRowState>(prev);
                    next[0] = new MultiColumnListViewRowState
                    {
                        Id = id,
                        Text = $"{id} {DateTime.Now:HH:mm:ss}",
                        ShouldOverrideElement = true,
                    };
                    return next;
                });
            };

            Action mclvDeleteLast = () =>
            {
                setMclvRows.Set(prev =>
                {
                    if (prev == null || prev.Count == 0)
                    {
                        return prev;
                    }
                    var next = new List<MultiColumnListViewRowState>(prev);
                    next.RemoveAt(next.Count - 1);
                    return next;
                });
            };

            Action<MultiColumnListViewProps.ColumnLayoutState> mclvLayoutChanged = layout =>
            {
                var clone = CloneListLayout(layout);
                if (ListLayoutEqual(clone, mclvLayout))
                {
                    return;
                }
                setMclvLayout.Set(_ => clone);
            };

            Action<List<MultiColumnListViewProps.SortedColumnDef>> mclvSortChanged = defs =>
            {
                setMclvSortDefs(
                    defs != null ? new List<MultiColumnListViewProps.SortedColumnDef>(defs) : null
                );
            };

            var tabViewProps = new TabViewProps
            {
                SelectedTabIndex = treeTabIndex,
                SelectedIndexChanged = (Action<int>)(index => setTreeTabIndex(index)),
                Tabs = new List<TabViewProps.TabDef>
                {
                    new()
                    {
                        Title = "Intro",
                        Content = () => ReactiveUITK.V.Func(Shared.IntroCounterFunc.Render),
                    },
                    new()
                    {
                        Title = "Tree",
                        Content = () =>
                            V.Func(
                                TreeViewStatefulDemoFunc.Render,
                                new Dictionary<string, object>
                                {
                                    { "rows", treeRows },
                                    { "addParent", treeAddParent },
                                    { "addChild", treeAddChild },
                                    { "setParent", treeSetParentValue },
                                    { "setChild", treeSetChildValue },
                                    { "deleteLast", treeDeleteLast },
                                    { "expandedItemIds", treeExpandedIds },
                                    { "onExpandedChanged", treeExpandedChanged },
                                    {
                                        "onCountChanged",
                                        (Action<int>)(
                                            count =>
                                            {
                                                if (count != treeDisplayCount)
                                                {
                                                    setTreeDisplayCount(count);
                                                }
                                            }
                                        )
                                    },
                                }
                            ),
                    },
                    new()
                    {
                        Title = "Columns",
                        Content = () =>
                            V.Func(
                                MultiColumnTreeViewStatefulDemoFunc.Render,
                                new Dictionary<string, object>
                                {
                                    { "rows", mctvRows },
                                    { "sortDefs", mctvSortDefs },
                                    { "columnWidths", mctvLayout?.ColumnWidths },
                                    { "columnVisibility", mctvLayout?.ColumnVisibility },
                                    { "columnDisplayIndex", mctvLayout?.ColumnDisplayIndex },
                                    { "addParent", mctvAddParent },
                                    { "addChild", mctvAddChild },
                                    { "setParent", mctvSetParentValue },
                                    { "setChild", mctvSetChildValue },
                                    { "deleteLast", mctvDeleteLast },
                                    { "onSortChanged", mctvSortChanged },
                                    { "onLayoutChanged", mctvLayoutChanged },
                                    {
                                        "onCountChanged",
                                        (Action<int>)(
                                            count =>
                                            {
                                                if (count != mctvDisplayCount)
                                                {
                                                    setMctvDisplayCount(count);
                                                }
                                            }
                                        )
                                    },
                                }
                            ),
                    },
                },
                Style = new Style { (Height, 240f) },
            };

            var listTabViewProps = new TabViewProps
            {
                SelectedTabIndex = listTabIndex,
                SelectedIndexChanged = (Action<int>)(index => setListTabIndex(index)),
                Tabs = new List<TabViewProps.TabDef>
                {
                    new()
                    {
                        Title = "Intro",
                        Content = () => ReactiveUITK.V.Func(Shared.IntroCounterFunc.Render),
                    },
                    new()
                    {
                        Title = "List",
                        Content = () =>
                            ReactiveUITK.V.Func(
                                ListViewStatefulDemoFunc.Render,
                                new Dictionary<string, object>
                                {
                                    { "items", listRows },
                                    { "addItem", listAddItem },
                                    { "setTopItem", listSetTopItem },
                                    { "deleteLast", listDeleteLast },
                                    {
                                        "onCountChanged",
                                        (Action<int>)(
                                            count =>
                                            {
                                                if (count != simpleListCount)
                                                {
                                                    setSimpleListCount(count);
                                                }
                                            }
                                        )
                                    },
                                }
                            ),
                    },
                    new()
                    {
                        Title = "Columns",
                        Content = () =>
                            ReactiveUITK.V.Func(
                                MultiColumnListViewStatefulDemoFunc.Render,
                                new Dictionary<string, object>
                                {
                                    { "items", mclvRows },
                                    { "sortDefs", mclvSortDefs },
                                    { "columnWidths", mclvLayout?.ColumnWidths },
                                    { "columnVisibility", mclvLayout?.ColumnVisibility },
                                    { "columnDisplayIndex", mclvLayout?.ColumnDisplayIndex },
                                    { "addItem", mclvAddItem },
                                    { "setTopItem", mclvSetTopItem },
                                    { "deleteLast", mclvDeleteLast },
                                    { "onSortChanged", mclvSortChanged },
                                    { "onLayoutChanged", mclvLayoutChanged },
                                    {
                                        "onCountChanged",
                                        (Action<int>)(
                                            count =>
                                            {
                                                if (count != mclvDisplayCount)
                                                {
                                                    setMclvDisplayCount(count);
                                                }
                                            }
                                        )
                                    },
                                }
                            ),
                    },
                },
                Style = new Style { (Height, 240f) },
            };

            int totalListCount =
                treeDisplayCount + mctvDisplayCount + simpleListCount + mclvDisplayCount;

            var valuesItems = new List<KeyValuePair<string, string>>
            {
                new("TextField", inputText ?? string.Empty),
                new("Toggle", isOptionEnabled.ToString()),
                new("RadioSingle", isRadioSingleSelected.ToString()),
                new("RadioIndex", selectionIndex.ToString()),
                new("Slider", sliderValue.ToString("F2")),
                new("SliderInt", sliderIntValue.ToString()),
                new("Dropdown", ddValue ?? string.Empty),
                new("Repeat", repeatClickCount.ToString()),
                new("Time", currentTime.ToLongTimeString()),
                new("TreeTab", treeTabIndex.ToString()),
                new("ListTab", listTabIndex.ToString()),
                new("ListCount", totalListCount.ToString()),
            };

            var toggleTreeTabsBtn = new ButtonProps
            {
                Text = showTreeTabs ? "Hide Tree Tabs" : "Show Tree Tabs",
                OnClick = () => setShowTreeTabs(!showTreeTabs),
            };

            var toggleListTabsBtn = new ButtonProps
            {
                Text = showListTabs ? "Hide List Tabs" : "Show List Tabs",
                OnClick = () => setShowListTabs(!showListTabs),
            };

            VirtualNode PageBody() =>
                V.VisualElement(
                    new Dictionary<string, object> { { "style", PageStyle } },
                    null,
                    V.VisualElement(
                        new Dictionary<string, object> { { "style", TopBarStyle } },
                        null,
                        V.VisualElement(
                            new Dictionary<string, object> { { "style", LeftBoxStyle } },
                            null,
                            V.Text("Left")
                        ),
                        V.TextField(inputTextFieldProps),
                        V.Button(setTextButtonProps),
                        V.VisualElement(
                            new Dictionary<string, object> { { "style", RightBoxStyle } },
                            null,
                            V.Text("Right")
                        )
                    ),
                    V.Label(new LabelProps { Text = "Now: " + currentTime.ToLongTimeString() }),
                    V.VisualElement(
                        new Dictionary<string, object> { { "style", ExtrasContainerStyle } },
                        null,
                        V.Label(new LabelProps { Text = "Extras" }),
                        V.GroupBox(
                            groupBox1Props,
                            null,
                            V.Label(new LabelProps { Text = "Inside group" })
                        ),
                        V.Toggle(toggleProps),
                        V.RadioButton(radioSingleProps),
                        V.RadioButtonGroup(
                            radioGroupProps,
                            null,
                            V.Label(new LabelProps { Text = "Pick one" })
                        ),
                        V.ProgressBar(progressBarProps),
                        V.Button(batchTestButtonProps)
                    ),
                    V.GroupBox(
                        newComponentsGroupProps,
                        null,
                        V.Foldout(
                            foldoutProps,
                            null,
                            V.Label(new LabelProps { Text = "Inside foldout" })
                        ),
                        V.Image(imageProps),
                        V.Label(new LabelProps { Text = $"Slider: {sliderValue:F2}" }),
                        V.Slider(sliderProps),
                        V.Label(new LabelProps { Text = $"SliderInt: {sliderIntValue}" }),
                        V.SliderInt(
                            new SliderIntProps
                            {
                                LowValue = 0,
                                HighValue = 10,
                                Value = sliderIntValue,
                                OnChange = e => setSliderIntValue.Set(e.newValue),
                            }
                        ),
#if UNITY_EDITOR
                        V.HelpBox(
                            new HelpBoxProps
                            {
                                MessageType =
                                    sliderIntValue % 3 == 0
                                        ? "error"
                                        : (sliderIntValue % 2 == 0 ? "warning" : "info"),
                                Text = "This is a HelpBox showing state-driven message type.",
                            }
                        ),
#endif
                        V.DropdownField(dropdownProps)
                    ),
                    V.VisualElement(
                        null,
                        null,
                        V.Button(toggleTreeTabsBtn),
                        V.Label(
                            new LabelProps
                            {
                                Text = "TabView + TreeView",
                                Style = new Style
                                {
                                    (FontSize, 16f),
                                    (TextColor, new UColor(0.1f, 0.1f, 0.1f, 1f)),
                                },
                            }
                        ),
                        V.VisualElement(
                            new Dictionary<string, object>
                            {
                                {
                                    "style",
                                    new Style
                                    {
                                        (MaxHeight, 500f),
                                        (FlexGrow, 0f),
                                        (StyleKeys.Display, showTreeTabs ? "flex" : "none"),
                                        (StyleKeys.FlexDirection, "column"),
                                        (StyleKeys.Overflow, "visible"),
                                    }
                                },
                            },
                            null,
                            V.TabView(tabViewProps)
                        ),
                        showTreeTabs ? V.Fragment() : V.Text("Tree tabs hidden")
                    ),
                    V.VisualElement(
                        null,
                        null,
                        V.Button(toggleListTabsBtn),
                        V.Label(
                            new LabelProps
                            {
                                Text = "TabView + ListViews",
                                Style = new Style
                                {
                                    (FontSize, 16f),
                                    (TextColor, new UColor(0.1f, 0.1f, 0.1f, 1f)),
                                },
                            }
                        ),
                        V.VisualElement(
                            new Dictionary<string, object>
                            {
                                {
                                    "style",
                                    new Style
                                    {
                                        (MaxHeight, 500f),
                                        (FlexGrow, 0f),
                                        (StyleKeys.Display, showListTabs ? "flex" : "none"),
                                        (StyleKeys.FlexDirection, "column"),
                                        (StyleKeys.Overflow, "visible"),
                                    }
                                },
                            },
                            null,
                            V.TabView(listTabViewProps)
                        ),
                        showListTabs ? V.Fragment() : V.Text("List tabs hidden")
                    ),
                    V.Label(new LabelProps { Text = "New Fields" }),
                    V.GroupBox(
                        new GroupBoxProps
                        {
                            Text = "Numeric, Vector and Color Fields",
                            Style = new Style { (MarginTop, 8f) },
                        },
                        null,
                        V.Label(new LabelProps { Text = "FloatField" }),
                        V.FloatField(new FloatFieldProps { Value = 1.23f }),
                        V.Label(new LabelProps { Text = "IntegerField" }),
                        V.IntegerField(new IntegerFieldProps { Value = 42 }),
                        V.Label(new LabelProps { Text = "LongField" }),
                        V.LongField(new LongFieldProps { Value = 123456789 }),
                        V.Label(new LabelProps { Text = "DoubleField" }),
                        V.DoubleField(new DoubleFieldProps { Value = 3.14159 }),
                        V.Label(new LabelProps { Text = "UnsignedIntegerField" }),
                        V.UnsignedIntegerField(new UnsignedIntegerFieldProps { Value = 77 }),
                        V.Label(new LabelProps { Text = "UnsignedLongField" }),
                        V.UnsignedLongField(new UnsignedLongFieldProps { Value = 9876543210 }),
                        V.Label(new LabelProps { Text = "Vector2Field" }),
                        V.Vector2Field(
                            new Vector2FieldProps { Value = new UnityEngine.Vector2(1, 2) }
                        ),
                        V.Label(new LabelProps { Text = "Vector3Field" }),
                        V.Vector3Field(
                            new Vector3FieldProps { Value = new UnityEngine.Vector3(1, 2, 3) }
                        ),
                        V.Label(new LabelProps { Text = "Vector4Field" }),
                        V.Vector4Field(
                            new Vector4FieldProps { Value = new UnityEngine.Vector4(1, 2, 3, 4) }
                        ),
                        V.Label(new LabelProps { Text = "ColorField" }),
                        V.ColorField(
                            new ColorFieldProps
                            {
                                Value = new UnityEngine.Color(0.2f, 0.6f, 0.9f, 1f),
                            }
                        )
                    ),
                    V.GroupBox(
                        new GroupBoxProps
                        {
                            Text = "More Fields",
                            Style = new Style { (MarginTop, 8f) },
                        },
                        null,
                        // EnumField
                        V.Label(new LabelProps { Text = "EnumField (TextAnchor)" }),
                        V.EnumField(
                            new EnumFieldProps
                            {
                                EnumType = typeof(UnityEngine.TextAnchor).AssemblyQualifiedName,
                                Value = UnityEngine.TextAnchor.MiddleCenter,
                            }
                        ),
                        // Scroller
                        V.Label(new LabelProps { Text = "Scroller" }),
                        V.Scroller(
                            new ScrollerProps
                            {
                                LowValue = 0f,
                                HighValue = 100f,
                                Value = 25f,
                                Style = new Style { (Height, 18f), (Width, 160f) },
                            }
                        ),
                        // TextElement
                        V.Label(new LabelProps { Text = "TextElement" }),
                        V.TextElement(
                            new TextElementProps
                            {
                                Text = "This is a TextElement",
                                Style = new Style { (FontSize, 13f) },
                            }
                        ),
                        // IMGUIContainer
                        V.Label(new LabelProps { Text = "IMGUIContainer" }),
                        V.IMGUIContainer(
                            new IMGUIContainerProps
                            {
                                OnGUI = () =>
                                {
                                    UnityEngine.GUILayout.Label("IMGUI says hello");
                                },
                                Style = new Style { (Height, 22f) },
                            }
                        ),
                        // Vector2Int/Vector3Int
                        V.Label(new LabelProps { Text = "Vector2IntField" }),
                        V.Vector2IntField(
                            new Vector2IntFieldProps { Value = new UnityEngine.Vector2Int(3, 7) }
                        ),
                        V.Label(new LabelProps { Text = "Vector3IntField" }),
                        V.Vector3IntField(
                            new Vector3IntFieldProps { Value = new UnityEngine.Vector3Int(1, 2, 3) }
                        ),
                        // Rect / RectInt / Bounds
                        V.Label(new LabelProps { Text = "RectField" }),
                        V.RectField(
                            new RectFieldProps { Value = new UnityEngine.Rect(10f, 20f, 80f, 40f) }
                        ),
                        V.Label(new LabelProps { Text = "RectIntField" }),
                        V.RectIntField(
                            new RectIntFieldProps { Value = new UnityEngine.RectInt(2, 4, 11, 9) }
                        ),
                        V.Label(new LabelProps { Text = "BoundsField" }),
                        V.BoundsField(
                            new BoundsFieldProps
                            {
                                Value = new UnityEngine.Bounds(
                                    new UnityEngine.Vector3(0, 0, 0),
                                    new UnityEngine.Vector3(1, 2, 3)
                                ),
                            }
                        )
#if UNITY_EDITOR
                        ,
                        // ObjectField (Editor only)
                        V.Label(new LabelProps { Text = "ObjectField (Texture2D)" }),
                        V.ObjectField(
                            new ObjectFieldProps
                            {
                                ObjectType = typeof(UnityEngine.Texture2D).AssemblyQualifiedName,
                                AllowSceneObjects = false,
                            }
                        )
#endif
                    ),
                    V.GroupBox(
                        new GroupBoxProps
                        {
                            Text = "Even More Fields",
                            Style = new Style { (MarginTop, 8f) },
                        },
                        null,
                        // MinMaxSlider
                        V.Label(new LabelProps { Text = "MinMaxSlider" }),
                        V.MinMaxSlider(
                            new MinMaxSliderProps
                            {
                                MinValue = 20f,
                                MaxValue = 80f,
                                LowLimit = 0f,
                                HighLimit = 100f,
                                Style = new Style { (Width, 200f) },
                            }
                        ),
                        // TemplateContainer
                        V.Label(new LabelProps { Text = "TemplateContainer" }),
                        V.TemplateContainer(
                            new TemplateContainerProps
                            {
                                ContentContainer = new Style
                                {
                                    (Padding, 6f),
                                    (BackgroundColor, new Color(1f, 1f, 1f, 1f)),
                                },
                                Style = new Style
                                {
                                    (BorderWidth, 1f),
                                    (BorderColor, new Color(0.85f, 0.85f, 0.85f, 1f)),
                                },
                            },
                            null,
                            V.Label(new LabelProps { Text = "Inside TemplateContainer" })
                        ),
                        // BoundsIntField
                        V.Label(new LabelProps { Text = "BoundsIntField" }),
                        V.BoundsIntField(
                            new BoundsIntFieldProps
                            {
                                Value = new UnityEngine.BoundsInt(1, 2, 3, 4, 5, 6),
                            }
                        ),
                        // Hash128Field
                        V.Label(new LabelProps { Text = "Hash128Field" }),
                        V.Hash128Field(
                            new Hash128FieldProps { Value = new UnityEngine.Hash128(1, 2, 3, 4) }
                        ),
                        // ToggleButtonGroup
                        V.Label(new LabelProps { Text = "ToggleButtonGroup" }),
                        V.ToggleButtonGroup(
                            new ToggleButtonGroupProps { Value = 1 },
                            null,
                            V.Button(new ButtonProps { Text = "One" }),
                            V.Button(new ButtonProps { Text = "Two" }),
                            V.Button(new ButtonProps { Text = "Three" })
                        )
                    )
                );

            return V.VisualElement(
                outerWrapperProps,
                null,
                V.VisualElementSafe(
                    safeWrapperStyle,
                    null,
                    V.VisualElement(
                        new Dictionary<string, object> { { "style", barSlotStyle } },
                        null,
                        V.Func(
                            ValuesBarFunc.Render,
                            new Dictionary<string, object> { { "items", valuesItems } }
                        )
                    ),
                    V.ScrollView(
                        new ScrollViewProps
                        {
                            Mode = "vertical",
                            Style = mainScrollStyle,
                            Name = "main-scroll",
                        },
                        null,
                        PageBody()
                    )
                )
            );
        }
    }
}
