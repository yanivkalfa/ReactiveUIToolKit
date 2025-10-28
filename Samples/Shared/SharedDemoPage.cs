using System;
using System.Collections.Generic;
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
            var initialItems = Hooks.UseMemo(() =>
            {
                var seededItems = new List<SharedRowItem>();
                for (int i = 1; i <= 5; i++)
                    seededItems.Add(
                        new SharedRowItem { Id = Guid.NewGuid().ToString("N"), Text = $"Item {i}" }
                    );
                return seededItems;
            });
            var (listItems, setListItems) = Hooks.UseState(initialItems);
            // Separate state for table view (independent from ListView)
            var (tableItems, setTableItems) = Hooks.UseState(initialItems);
            var listRowRenderer = Hooks.UseMemo(
                () =>
                    (Func<int, object, VirtualNode>)(
                        (index, itemObj) =>
                        {
                            var typedItem = itemObj as SharedRowItem;
                            return V.Func(
                                SharedListViewRow.Render,
                                new Dictionary<string, object>
                                {
                                    { "item", typedItem },
                                    { "index", index },
                                    {
                                        "onRemove",
                                        (Action<SharedRowItem>)(
                                            removeItem =>
                                            {
                                                if (removeItem == null)
                                                {
                                                    return;
                                                }
                                                var copy = new List<SharedRowItem>(listItems);
                                                int foundIndex = copy.FindIndex(r =>
                                                    r.Id == removeItem.Id
                                                );
                                                if (foundIndex >= 0)
                                                {
                                                    copy.RemoveAt(foundIndex);
                                                    setListItems(copy);
                                                }
                                            }
                                        )
                                    },
                                },
                                key: typedItem != null
                                    ? $"{typedItem.Id}-{index}"
                                    : $"row-missing-{index}"
                            );
                        }
                    ),
                listItems
            );
            ListViewProps listViewProps = new()
            {
                Items = listItems,
                FixedItemHeight = 20f,
                Selection = UnityEngine.UIElements.SelectionType.None,
                Row = listRowRenderer,
            };
            ButtonProps toggleListButtonProps = new()
            {
                Text = isListVisible ? "Hide List" : "Show List",
                OnClick = () => setListVisible(!isListVisible),
                Style = new Style { (MarginTop, 8f), (Width, 120f), (Height, 28f) },
            };
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
            ButtonProps updateFirstItemButtonProps = new()
            {
                Text = "Change First Item",
                OnClick = () =>
                {
                    if (listItems.Count > 0)
                    {
                        var copy = new List<SharedRowItem>(listItems);
                        copy[0] = new SharedRowItem
                        {
                            Id = copy[0].Id,
                            Text = "UPDATED " + DateTime.Now.ToLongTimeString(),
                        };
                        setListItems(copy);
                    }
                },
                Style = new Style { (MarginTop, 8f), (Width, 160f), (Height, 28f) },
            };
            ButtonProps addListItemButtonProps = new()
            {
                Text = "Add Item (ListView)",
                OnClick = () =>
                {
                    var copy = new List<SharedRowItem>(listItems.Count + 1);
                    copy.Add(
                        new SharedRowItem
                        {
                            Id = Guid.NewGuid().ToString("N"),
                            Text = "NEW " + DateTime.Now.ToLongTimeString(),
                        }
                    );
                    copy.AddRange(listItems);
                    setListItems(copy);
                },
                Style = new Style { (MarginTop, 8f), (Width, 180f), (Height, 28f) },
            };
            ButtonProps addTableItemButtonProps = new()
            {
                Text = "Add Item (Table)",
                OnClick = () =>
                {
                    var copy = new List<SharedRowItem>((tableItems?.Count ?? 0) + 1);
                    copy.Add(
                        new SharedRowItem
                        {
                            Id = Guid.NewGuid().ToString("N"),
                            Text = "NEW " + DateTime.Now.ToLongTimeString(),
                        }
                    );
                    if (tableItems != null)
                        copy.AddRange(tableItems);
                    setTableItems(copy);
                },
                Style = new Style { (MarginTop, 8f), (Width, 180f), (Height, 28f) },
            };
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
            var listContent = isListVisible
                ? V.Animate(
                    new AnimateProps { Tracks = listFadeTracks },
                    null,
                    V.VisualElement(
                        new Dictionary<string, object> { { "style", ListContainerStyle } },
                        null,
                        V.ListView(listViewProps)
                    )
                )
                : V.Text("List hidden");

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
            var mainScrollStyle = new Style { (FlexGrow, 1f) };

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

            // Build Values bar items
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
                new("ListCount", listItems?.Count.ToString() ?? "0"),
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
                    V.Button(toggleListButtonProps),
                    V.Button(updateFirstItemButtonProps),
                    V.Button(addListItemButtonProps),
                    listContent,
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
                        V.DropdownField(dropdownProps),
                        // MultiColumnListView demo
                        V.Label(new LabelProps { Text = "MultiColumnListView" }),
                        V.Button(addTableItemButtonProps),
                        V.MultiColumnListView(
                            new MultiColumnListViewProps
                            {
                                Items = tableItems,
                                Selection = UnityEngine.UIElements.SelectionType.None,
                                FixedItemHeight = 20f,
                                Columns = Hooks.UseMemo(
                                    () =>
                                        new List<MultiColumnListViewProps.ColumnDef>
                                        {
                                            new()
                                            {
                                                Title = "ID",
                                                Width = 140f,
                                                MinWidth = 100f,
                                                Resizable = true,
                                                Stretchable = true,
                                                Cell = (i, obj) =>
                                                {
                                                    var it = obj as SharedRowItem;
                                                    var id = it?.Id ?? string.Empty;
                                                    var shortId =
                                                        id.Length > 6 ? id.Substring(0, 6) : id;
                                                    return V.VisualElement(
                                                        null,
                                                        null,
                                                        V.Label(new LabelProps { Text = shortId })
                                                    );
                                                },
                                            },
                                            new()
                                            {
                                                Title = "Text",
                                                Width = 260f,
                                                MinWidth = 140f,
                                                Resizable = true,
                                                Stretchable = true,
                                                Cell = (i, obj) =>
                                                {
                                                    var it = obj as SharedRowItem;
                                                    // Render a full row-like UI to verify nested components bind per cell
                                                    return V.Func(
                                                        SharedListViewRow.Render,
                                                        new Dictionary<string, object>
                                                        {
                                                            { "item", it },
                                                            { "index", i },
                                                            {
                                                                "onRemove",
                                                                (Action<SharedRowItem>)(
                                                                    removeItem =>
                                                                    {
                                                                        if (removeItem == null)
                                                                            return;
                                                                        var source =
                                                                            tableItems
                                                                            ?? new List<SharedRowItem>();
                                                                        var copy =
                                                                            new List<SharedRowItem>(
                                                                                source
                                                                            );
                                                                        int foundIndex =
                                                                            copy.FindIndex(r =>
                                                                                r != null
                                                                                && r.Id
                                                                                    == removeItem.Id
                                                                            );
                                                                        if (foundIndex < 0)
                                                                        {
                                                                            foundIndex =
                                                                                copy.FindIndex(r =>
                                                                                    ReferenceEquals(
                                                                                        r,
                                                                                        removeItem
                                                                                    )
                                                                                );
                                                                        }
                                                                        if (foundIndex >= 0)
                                                                        {
                                                                            copy.RemoveAt(
                                                                                foundIndex
                                                                            );
                                                                            setTableItems(copy);
                                                                        }
                                                                    }
                                                                )
                                                            },
                                                        }
                                                    );
                                                },
                                            },
                                        },
                                    tableItems
                                ),
                            }
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
