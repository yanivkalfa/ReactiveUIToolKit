using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveUITK.Core;
using ReactiveUITK.Core.Animation;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using static ReactiveUITK.Props.Typed.StyleKeys;
using UColor = UnityEngine.Color;

namespace ReactiveUITK.Samples.Shared
{
    public static class SharedDemoPage
    {
        private static readonly Style TopBarStyle = new()
        {
            (FlexDirection, "row"),
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
            (FlexDirection, "column"),
            (FlexGrow, 1f),
            (JustifyContent, "space-between"),
            (BackgroundColor, new UColor(0.95f, 0.95f, 0.95f, 1f)),
        };
        private static readonly Style ListContainerStyle = new()
        {
            (FlexGrow, 1f),
            (MarginTop, 8f),
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
            (FlexDirection, "column"),
        };

        public static VirtualNode Render(
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children
        )
        {
            var (isListVisible, setListVisible) = Hooks.UseState(true);
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
            // New element demo state
            var (sliderValue, setSliderValue) = Hooks.UseState(0.5f);
            var (sliderIntValue, setSliderIntValue) = Hooks.UseState(5);
            var ddChoices = Hooks.UseMemo(() => new List<string> { "Alpha", "Beta", "Gamma" }, 0);
            var (ddValue, setDdValue) = Hooks.UseState("Beta");
            var (foldoutOpen, setFoldoutOpen) = Hooks.UseState(true);
            var rootElement = Hooks.UseRef();
            Hooks.UseEffect(
                () =>
                {
                    if (rootElement == null)
                    {
                        return null;
                    }
                    var timerHandle = rootElement
                        .schedule.Execute(() => setCurrentTime(DateTime.Now))
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
            // Inline ListView demo removed; keep input and other controls
            TextFieldProps inputTextFieldProps = new()
            {
                Style = TextInputStyle,
                Placeholder = "Type here...",
                HidePlaceholderOnFocus = false,
                Value = inputText,
                LabelText = string.IsNullOrEmpty(inputText)
                    ? string.Empty
                    : ("Value: " + inputText),
                OnChange = e => setInputText(e.newValue),
            };
            // Removed inline ListView change/add buttons
            // removed inline table add button (handled in MultiColumnListViewStatefulDemoFunc)
            var listFadeTracks = Hooks.UseMemo(
                () =>
                    new List<AnimateTrack>
                    {
                        new AnimateTrack
                        {
                            Property = "opacity",
                            From = 0f,
                            To = 1f,
                            Duration = 1.5f,
                            Ease = Ease.EaseOutCubic,
                        },
                    },
                isListVisible
            );
            // Removed inline ListView content rendering

            // Extracted styles (avoid inline styles/props below)
            var outerWrapperStyle = new Style
            {
                (BackgroundColor, new UColor(0.2f, 0.4f, 0.8f, 1f)),
                (FlexGrow, 1f),
            };
            var safeWrapperStyle = new Style
            {
                (BackgroundColor, new UColor(0.2f, 0.6f, 0.2f, 1f)),
                (FlexGrow, 1f),
                (FlexDirection, "column"),
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

            // Extracted props
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

            var treeRootItems = Hooks.UseMemo(
                () =>
                {
                    var c1 = new List<UnityEngine.UIElements.TreeViewItemData<object>>
                    {
                        new UnityEngine.UIElements.TreeViewItemData<object>(
                            11,
                            new SharedRowItem
                            {
                                Id = System.Guid.NewGuid().ToString("N"),
                                Text = "Child 1",
                            },
                            null
                        ),
                        new UnityEngine.UIElements.TreeViewItemData<object>(
                            12,
                            new SharedRowItem
                            {
                                Id = System.Guid.NewGuid().ToString("N"),
                                Text = "Child 2",
                            },
                            null
                        ),
                    };
                    var c2 = new List<UnityEngine.UIElements.TreeViewItemData<object>>
                    {
                        new UnityEngine.UIElements.TreeViewItemData<object>(
                            21,
                            new SharedRowItem
                            {
                                Id = System.Guid.NewGuid().ToString("N"),
                                Text = "Child A",
                            },
                            null
                        ),
                        new UnityEngine.UIElements.TreeViewItemData<object>(
                            22,
                            new SharedRowItem
                            {
                                Id = System.Guid.NewGuid().ToString("N"),
                                Text = "Child B",
                            },
                            null
                        ),
                    };
                    var roots = new List<UnityEngine.UIElements.TreeViewItemData<object>>
                    {
                        new UnityEngine.UIElements.TreeViewItemData<object>(
                            1,
                            new SharedRowItem
                            {
                                Id = System.Guid.NewGuid().ToString("N"),
                                Text = "Root 1",
                            },
                            c1
                        ),
                        new UnityEngine.UIElements.TreeViewItemData<object>(
                            2,
                            new SharedRowItem
                            {
                                Id = System.Guid.NewGuid().ToString("N"),
                                Text = "Root 2",
                            },
                            c2
                        ),
                    };
                    return roots;
                },
                0
            );
            var treeRowRenderer = Hooks.UseMemo(
                () =>
                    (Func<int, object, VirtualNode>)(
                        (i, obj) =>
                        {
                            if (obj is VirtualNode vn)
                            {
                                return vn;
                            }
                            var it = obj as SharedRowItem;
                            return V.Label(new LabelProps { Text = it?.Text ?? "<null>" });
                        }
                    ),
                0
            );
            var treeViewProps = new TreeViewProps
            {
                RootItems = treeRootItems,
                Selection = UnityEngine.UIElements.SelectionType.None,
                FixedItemHeight = 20f,
                Row = treeRowRenderer,
            };
            var mctvColumns = Hooks.UseMemo(
                () =>
                    new List<MultiColumnTreeViewProps.ColumnDef>
                    {
                        new()
                        {
                            Title = "Name",
                            Width = 180f,
                            Cell = (i, obj) =>
                            {
                                var it = obj as SharedRowItem;
                                return V.Label(new LabelProps { Text = it?.Text ?? string.Empty });
                            },
                        },
                        new()
                        {
                            Title = "ID",
                            Width = 160f,
                            Cell = (i, obj) =>
                            {
                                var it = obj as SharedRowItem;
                                var id = it?.Id ?? string.Empty;
                                var s = id.Length > 6 ? id.Substring(0, 6) : id;
                                return V.VisualElement(
                                    new Dictionary<string, object>
                                    {
                                        {
                                            "style",
                                            new Style { (TextColor, "red") }
                                        },
                                    },
                                    null,
                                    V.Label(new LabelProps { Text = s })
                                );
                            },
                        },
                    },
                treeRootItems?.Count ?? 0
            );
            var mctvProps = new MultiColumnTreeViewProps
            {
                RootItems = treeRootItems,
                Selection = UnityEngine.UIElements.SelectionType.None,
                FixedItemHeight = 20f,
                Columns = mctvColumns,
            };
            // Dynamic Tree tab state for add/delete/set
            var (treePairs, setTreePairs) = Hooks.UseState(
                new List<(
                    SharedRowItem parent,
                    SharedRowItem childLabel,
                    bool hasChild,
                    bool childAsFunc
                )>()
            );
            var (treeNextIsParent, setTreeNextIsParent) = Hooks.UseState(true);
            var combinedTreeRoots = new List<UnityEngine.UIElements.TreeViewItemData<object>>(
                treeRootItems ?? new List<UnityEngine.UIElements.TreeViewItemData<object>>()
            );
            for (int i = 0; i < treePairs.Count; i++)
            {
                var pair = treePairs[i];
                int pid = 1000 + (i * 2);
                List<UnityEngine.UIElements.TreeViewItemData<object>> ch = null;
                if (pair.hasChild)
                {
                    object childData = pair.childAsFunc
                        ? (object)
                            ReactiveUITK.V.Func(
                                ReactiveUITK.Samples.Shared.IntroCounterFunc.Render,
                                null,
                                $"tv-child-{pid}"
                            )
                        : (object)(
                            pair.childLabel
                            ?? new SharedRowItem
                            {
                                Id = System.Guid.NewGuid().ToString("N"),
                                Text = "Child",
                            }
                        );
                    ch = new List<UnityEngine.UIElements.TreeViewItemData<object>>
                    {
                        new UnityEngine.UIElements.TreeViewItemData<object>(
                            pid + 1,
                            childData,
                            null
                        ),
                    };
                }
                combinedTreeRoots.Add(
                    new UnityEngine.UIElements.TreeViewItemData<object>(pid, pair.parent, ch)
                );
            }

            // Removed tab content animations
            // Track TreeView displayed row count for Values bar
            var (treeDisplayCount, setTreeDisplayCount) = Hooks.UseState(0);

            // Track MultiColumnTreeView displayed row count for Values bar
            var (mctvDisplayCount, setMctvDisplayCount) = Hooks.UseState(0);
            // Track MultiColumnListView displayed row count for Values bar
            var (mclvDisplayCount, setMclvDisplayCount) = Hooks.UseState(0);

            var tabViewProps = new TabViewProps
            {
                Tabs = new List<TabViewProps.TabDef>
                {
                    new()
                    {
                        Title = "Intro",
                        Content = () =>
                            ReactiveUITK.V.Func(
                                ReactiveUITK.Samples.Shared.IntroCounterFunc.Render
                            ),
                    },
                    new()
                    {
                        Title = "Tree",
                        Content = () =>
                        {
                            // Compose combined roots: initial + dynamic
                            var combined =
                                new List<UnityEngine.UIElements.TreeViewItemData<object>>(
                                    treeRootItems
                                        ?? new List<UnityEngine.UIElements.TreeViewItemData<object>>()
                                );
                            for (int i = 0; i < treePairs.Count; i++)
                            {
                                var pair = treePairs[i];
                                int pid = 1000 + (i * 2);
                                List<UnityEngine.UIElements.TreeViewItemData<object>> ch = null;
                                if (pair.hasChild)
                                {
                                    object childData = pair.childAsFunc
                                        ? (object)
                                            ReactiveUITK.V.Func(
                                                ReactiveUITK.Samples.Shared.IntroCounterFunc.Render,
                                                null,
                                                $"tv-child-{pid}"
                                            )
                                        : (object)(
                                            pair.childLabel
                                            ?? new SharedRowItem
                                            {
                                                Id = System.Guid.NewGuid().ToString("N"),
                                                Text = "Child",
                                            }
                                        );
                                    ch = new List<UnityEngine.UIElements.TreeViewItemData<object>>
                                    {
                                        new UnityEngine.UIElements.TreeViewItemData<object>(
                                            pid + 1,
                                            childData,
                                            null
                                        ),
                                    };
                                }
                                combined.Add(
                                    new UnityEngine.UIElements.TreeViewItemData<object>(
                                        pid,
                                        pair.parent,
                                        ch
                                    )
                                );
                            }

                            var addBtn = new ButtonProps
                            {
                                Text = "Add",
                                OnClick = () =>
                                {
                                    var copy = new List<(
                                        SharedRowItem parent,
                                        SharedRowItem childLabel,
                                        bool hasChild,
                                        bool childAsFunc
                                    )>(treePairs);
                                    if (treeNextIsParent)
                                    {
                                        copy.Add(
                                            (
                                                new SharedRowItem
                                                {
                                                    Id = System.Guid.NewGuid().ToString("N"),
                                                    Text = $"Parent {copy.Count + 1}",
                                                },
                                                null,
                                                false,
                                                false
                                            )
                                        );
                                        setTreePairs(copy);
                                        setTreeNextIsParent(false);
                                    }
                                    else if (copy.Count > 0)
                                    {
                                        var last = copy[copy.Count - 1];
                                        last.hasChild = true;
                                        last.childAsFunc = true;
                                        if (last.childLabel == null)
                                            last.childLabel = new SharedRowItem
                                            {
                                                Id = System.Guid.NewGuid().ToString("N"),
                                                Text = "Child",
                                            };
                                        copy[copy.Count - 1] = last;
                                        setTreePairs(copy);
                                        setTreeNextIsParent(true);
                                    }
                                },
                            };
                            var delBtn = new ButtonProps
                            {
                                Text = "Delete",
                                OnClick = () =>
                                {
                                    var copy = new List<(
                                        SharedRowItem parent,
                                        SharedRowItem childLabel,
                                        bool hasChild,
                                        bool childAsFunc
                                    )>(treePairs);
                                    if (copy.Count == 0)
                                        return;
                                    if (treeNextIsParent)
                                    {
                                        var last = copy[copy.Count - 1];
                                        if (last.hasChild)
                                        {
                                            last.hasChild = false;
                                            last.childAsFunc = false;
                                            copy[copy.Count - 1] = last;
                                            setTreePairs(copy);
                                            setTreeNextIsParent(false);
                                        }
                                    }
                                    else
                                    {
                                        copy.RemoveAt(copy.Count - 1);
                                        setTreePairs(copy);
                                        setTreeNextIsParent(true);
                                    }
                                },
                            };
                            var setBtn = new ButtonProps
                            {
                                Text = "SetValue",
                                OnClick = () =>
                                {
                                    var copy = new List<(
                                        SharedRowItem parent,
                                        SharedRowItem childLabel,
                                        bool hasChild,
                                        bool childAsFunc
                                    )>(treePairs);
                                    if (copy.Count == 0)
                                        return;
                                    var last = copy[copy.Count - 1];
                                    string stamp = System.DateTime.Now.ToString("HH:mm:ss");
                                    if (treeNextIsParent)
                                    {
                                        if (last.hasChild)
                                        {
                                            last.childAsFunc = false;
                                            if (last.childLabel == null)
                                                last.childLabel = new SharedRowItem
                                                {
                                                    Id = System.Guid.NewGuid().ToString("N"),
                                                };
                                            last.childLabel.Text = $"{last.childLabel.Id} {stamp}";
                                            copy[copy.Count - 1] = last;
                                            setTreePairs(copy);
                                        }
                                    }
                                    else
                                    {
                                        if (last.parent == null)
                                            last.parent = new SharedRowItem
                                            {
                                                Id = System.Guid.NewGuid().ToString("N"),
                                            };
                                        last.parent.Text = $"{last.parent.Id} {stamp}";
                                        copy[copy.Count - 1] = last;
                                        setTreePairs(copy);
                                    }
                                },
                            };

                            return V.Func(
                                TreeViewStatefulDemoFunc.Render,
                                new Dictionary<string, object>
                                {
                                    {
                                        "onCountChanged",
                                        (Action<int>)(
                                            c =>
                                            {
                                                if (c != treeDisplayCount)
                                                    setTreeDisplayCount(c);
                                            }
                                        )
                                    },
                                }
                            );
                        },
                    },
                    new()
                    {
                        Title = "Columns",
                        Content = () =>
                            V.Func(
                                MultiColumnTreeViewStatefulDemoFunc.Render,
                                new Dictionary<string, object>
                                {
                                    {
                                        "onCountChanged",
                                        (Action<int>)(
                                            c =>
                                            {
                                                if (c != mctvDisplayCount)
                                                    setMctvDisplayCount(c);
                                            }
                                        )
                                    },
                                }
                            ),
                    },
                },
                // Reserve space for the TabView body so content is visible
                Style = new Style { (Height, 240f) },
            };

            // List TabView
            // Receive simple ListView count from child component
            var (simpleListCount, setSimpleListCount) = Hooks.UseState(0);

            var listTabViewProps = new TabViewProps
            {
                Tabs = new List<TabViewProps.TabDef>
                {
                    new()
                    {
                        Title = "Intro",
                        Content = () =>
                            ReactiveUITK.V.Func(
                                ReactiveUITK.Samples.Shared.IntroCounterFunc.Render
                            ),
                    },
                    new()
                    {
                        Title = "List",
                        Content = () =>
                        {
                            return ReactiveUITK.V.Func(
                                ReactiveUITK.Samples.Shared.ListViewStatefulDemoFunc.Render,
                                new Dictionary<string, object>
                                {
                                    {
                                        "onCountChanged",
                                        (Action<int>)(
                                            c =>
                                            {
                                                if (c != simpleListCount)
                                                    setSimpleListCount(c);
                                            }
                                        )
                                    },
                                }
                            );
                        },
                    },
                    new()
                    {
                        Title = "Columns",
                        Content = () =>
                            ReactiveUITK.V.Func(
                                ReactiveUITK
                                    .Samples
                                    .Shared
                                    .MultiColumnListViewStatefulDemoFunc
                                    .Render,
                                new Dictionary<string, object>
                                {
                                    {
                                        "onCountChanged",
                                        (Action<int>)(
                                            c =>
                                            {
                                                if (c != mclvDisplayCount)
                                                    setMclvDisplayCount(c);
                                            }
                                        )
                                    },
                                }
                            ),
                    },
                },
                Style = new Style { (Height, 240f) },
            };

            // Toggle visibility of each TabView (state on main component)
            var (showTreeTabs, setShowTreeTabs) = Hooks.UseState(true);
            var (showListTabs, setShowListTabs) = Hooks.UseState(true);
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

            // Build Values bar items
            // ListCount shows sum of: TreeView + MultiColumnTreeView + ListView + MultiColumnListView
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
                new("ListCount", totalListCount.ToString()),
            };

            // Props for frequently used controls (avoid inline props)
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
            var repeatPulseTracks = Hooks.UseMemo(
                () =>
                    new List<AnimateTrack>
                    {
                        // Stronger visibility on the button
                        new AnimateTrack
                        {
                            Property = "opacity",
                            From = 1f,
                            To = 0.4f,
                            Duration = 0.8f,
                            Ease = Ease.EaseInOutSine,
                            Yoyo = true,
                            Loop = true,
                        },
                        // Gentle bob
                        new AnimateTrack
                        {
                            Property = "translateY",
                            From = 0f,
                            To = 6f,
                            Duration = 0.8f,
                            Ease = Ease.EaseInOutSine,
                            Yoyo = true,
                            Loop = true,
                        },
                        // Subtle size breathing via padding
                        new AnimateTrack
                        {
                            Property = "paddingLeft",
                            From = 0f,
                            To = 8f,
                            Duration = 0.8f,
                            Ease = Ease.EaseInOutSine,
                            Yoyo = true,
                            Loop = true,
                        },
                        new AnimateTrack
                        {
                            Property = "paddingRight",
                            From = 0f,
                            To = 8f,
                            Duration = 0.8f,
                            Ease = Ease.EaseInOutSine,
                            Yoyo = true,
                            Loop = true,
                        },
                    },
                0
            );
            // Multi-attribute animation demo controls
            var (animNonce, setAnimNonce) = Hooks.UseState(0);
            var multiTracks = Hooks.UseMemo(
                () =>
                    new List<AnimateTrack>
                    {
                        // Fade + slide in
                        new AnimateTrack
                        {
                            Property = "opacity",
                            From = 0f,
                            To = 1f,
                            Duration = 0.5f,
                            Ease = Ease.EaseOutCubic,
                        },
                        new AnimateTrack
                        {
                            Property = "translateY",
                            From = 12f,
                            To = 0f,
                            Duration = 0.5f,
                            Ease = Ease.EaseOutCubic,
                        },
                        // Size breathing
                        new AnimateTrack
                        {
                            Property = "width",
                            From = 120f,
                            To = 180f,
                            Duration = 0.6f,
                            Ease = Ease.EaseInOutSine,
                            Yoyo = true,
                            Loop = true,
                        },
                        new AnimateTrack
                        {
                            Property = "height",
                            From = 32f,
                            To = 44f,
                            Duration = 0.6f,
                            Ease = Ease.EaseInOutSine,
                            Yoyo = true,
                            Loop = true,
                        },
                        // Tint to white
                        new AnimateTrack
                        {
                            Property = "backgroundColor",
                            From = new UColor(0.75f, 0.85f, 1f, 1f),
                            To = new UColor(1f, 1f, 1f, 1f),
                            Duration = 0.5f,
                            Ease = Ease.EaseOutCubic,
                        },
                    },
                animNonce
            );
            var animCardStyle = new Style
            {
                (Width, 120f),
                (Height, 32f),
                (BackgroundColor, new UColor(0.9f, 0.95f, 1f, 1f)),
                (BorderRadius, 6f),
                (MarginTop, 6f),
                (AlignItems, "center"),
                (JustifyContent, "center"),
            };
            // Button near text field to change its value
            var setTextButtonProps = new ButtonProps
            {
                Text = "Set Text",
                OnClick = () => setInputText("Updated!"),
                Style = new Style { (MarginLeft, 6f), (Height, 28f) },
            };

            // Original page content grouped for ScrollView
            VirtualNode PageBody() =>
                V.VisualElement(
                    new Dictionary<string, object> { { "style", PageStyle } },
                    key: "shared-page-root",
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
                        key: "extras",
                        V.Label(new LabelProps { Text = "Extras" }),
                        V.GroupBox(
                            groupBox1Props,
                            null,
                            V.Label(new LabelProps { Text = "Inside group" }, key: "inner-one")
                        ),
                        V.Toggle(toggleProps),
                        V.RadioButton(radioSingleProps),
                        V.RadioButtonGroup(
                            radioGroupProps,
                            null,
                            V.Label(new LabelProps { Text = "Pick one" }, key: "radio-label")
                        ),
                        V.ProgressBar(progressBarProps),
                        V.Animate(
                            new AnimateProps { Tracks = repeatPulseTracks },
                            null,
                            V.RepeatButton(repeatButtonProps)
                        )
                    ),
                    // New components demo section
                    V.GroupBox(
                        newComponentsGroupProps,
                        null,
                        // Foldout first (ensure visibility)
                        V.Foldout(
                            foldoutProps,
                            null,
                            V.Label(new LabelProps { Text = "Inside foldout" })
                        ),
                        // Image demo (uses background color when no sprite/texture)
                        V.Image(imageProps, key: "img-demo"),
                        // Slider demo
                        V.Label(new LabelProps { Text = $"Slider: {sliderValue:F2}" }),
                        V.Slider(sliderProps),
                        // SliderInt demo
                        V.Label(new LabelProps { Text = $"SliderInt: {sliderIntValue}" }),
                        V.SliderInt(
                            new SliderIntProps
                            {
                                LowValue = 0,
                                HighValue = 10,
                                Value = sliderIntValue,
                                OnChange = e => setSliderIntValue(e.newValue),
                            }
                        ),
                        // HelpBox demo (Editor-only)
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
                        // Dropdown demo
                        V.DropdownField(dropdownProps)
                    ),
                    // Toggle buttons to show/hide each TabView (grouped separately)
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
                        (showTreeTabs ? V.TabView(tabViewProps) : V.Text("Tree tabs hidden"))
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
                        (showListTabs ? V.TabView(listTabViewProps) : V.Text("List tabs hidden"))
                    ),
                    // Animations section (moved to bottom)
                    V.Label(new LabelProps { Text = "Animations" }),
                    // Simple flashing box
                    V.Animate(
                        new AnimateProps
                        {
                            Tracks = Hooks.UseMemo(
                                () =>
                                    new List<AnimateTrack>
                                    {
                                        new AnimateTrack
                                        {
                                            Property = "opacity",
                                            From = 1f,
                                            To = 0.3f,
                                            Duration = 0.8f,
                                            Ease = Ease.EaseInOutSine,
                                            Yoyo = true,
                                            Loop = true,
                                        },
                                    },
                                0
                            ),
                        },
                        null,
                        V.VisualElement(
                            new Dictionary<string, object>
                            {
                                {
                                    "style",
                                    new Style
                                    {
                                        (Width, 160f),
                                        (Height, 60f),
                                        (BackgroundColor, new UColor(0.3f, 0.6f, 0.9f, 1f)),
                                        (BorderRadius, 6f),
                                        (JustifyContent, "center"),
                                        (AlignItems, "center"),
                                        (MarginTop, 6f),
                                    }
                                },
                            },
                            null,
                            V.Label(
                                new LabelProps
                                {
                                    Text = "Flashing Box",
                                    Style = new Style { (TextColor, UColor.white) },
                                }
                            )
                        )
                    ),
                    // Animated card using existing multiTracks
                    V.Animate(
                        new AnimateProps { Tracks = multiTracks },
                        null,
                        V.VisualElement(
                            new Dictionary<string, object>
                            {
                                {
                                    "style",
                                    new Style
                                    {
                                        (Width, 200f),
                                        (Height, 120f),
                                        (BackgroundColor, new UColor(0.95f, 0.95f, 0.95f, 1f)),
                                        (BorderRadius, 8f),
                                        (JustifyContent, "center"),
                                        (AlignItems, "center"),
                                        (MarginTop, 8f),
                                    }
                                },
                            },
                            null,
                            V.Label(new LabelProps { Text = "Animated Card" })
                        )
                    )
                );

            // Outer wrapper (blue full-screen) -> Safe area wrapper (green)
            // Inside safe area: Values bar (top, fixed in layout) + ScrollView (body)
            return V.VisualElement(
                outerWrapperProps,
                key: "outer-wrap",
                V.VisualElementSafe(
                    safeWrapperStyle,
                    key: "safe-wrap",
                    // Values bar docked at top; fixed height via minHeight
                    V.VisualElement(
                        new Dictionary<string, object> { { "style", barSlotStyle } },
                        null,
                        V.Func(
                            ValuesBarFunc.Render,
                            new Dictionary<string, object> { { "items", valuesItems } }
                        )
                    ),
                    // Main scrollable content
                    V.ScrollView(
                        new ScrollViewProps { Mode = "vertical", Style = mainScrollStyle },
                        null,
                        PageBody()
                    )
                )
            );
        }
    }
}
