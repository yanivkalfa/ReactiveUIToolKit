using System;
using System.Collections.Generic;
using UnityEngine;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using static ReactiveUITK.Props.Typed.StyleKeys;
using UColor = UnityEngine.Color;

namespace ReactiveUITK.Samples.Shared
{
    public static class SharedDemoPage
    {
        private static readonly Style TopBarStyle = new()
        {
            (FlexDirection, "row"), (JustifyContent, "space-between"), (AlignItems, "center"), (FlexGrow, 1f),
            (PaddingLeft, 12f), (PaddingRight, 12f), (PaddingTop, 8f), (PaddingBottom, 8f), (BorderBottomWidth, 1f),
            (BorderBottomColor, new Color(0.85f, 0.85f, 0.85f, 1f))
        };
        private static readonly Style LeftBoxStyle = new()
        {
            (BackgroundColor, new Color(0.2f, 0.4f, 0.9f, 1f)), (TextColor, UColor.white), (PaddingLeft, 10f), (PaddingRight, 10f),
            (PaddingTop, 6f), (PaddingBottom, 6f), (BorderRadius, 4f), (FontSize, 14f)
        };
        private static readonly Style RightBoxStyle = new()
        {
            (BackgroundColor, new UColor(0.9f, 0.3f, 0.2f, 1f)), (TextColor, UColor.white), (PaddingLeft, 10f), (PaddingRight, 10f),
            (PaddingTop, 6f), (PaddingBottom, 6f), (BorderRadius, 4f), (FontSize, 14f)
        };
        private static readonly Style TextInputStyle = new()
        {
            (FlexGrow, 1f), (MarginLeft, 8f), (MarginRight, 8f), (PaddingLeft, 6f), (PaddingRight, 6f), (PaddingTop, 4f), (PaddingBottom, 4f),
            (BorderRadius, 4f), (BorderWidth, 1f), (TextColor, UColor.black), (BorderColor, new UColor(0.8f, 0.8f, 0.8f, 1f)), (BackgroundColor, new UColor(1f, 1f, 1f, 1f))
        };
        private static readonly Style PageStyle = new()
        {
            (FlexDirection, "column"), (FlexGrow, 1f), (JustifyContent, "space-between"), (BackgroundColor, new UColor(0.95f, 0.95f, 0.95f, 1f))
        };
        private static readonly Style ListContainerStyle = new() { (FlexGrow, 1f), (MarginTop, 8f) };
        private static readonly Style ExtrasContainerStyle = new()
        {
            (MarginTop, 12f), (PaddingLeft, 12f), (PaddingRight, 12f), (PaddingTop, 8f), (PaddingBottom, 8f), (BackgroundColor, UColor.white),
            (BorderTopWidth, 1f), (BorderTopColor, new UColor(0.85f, 0.85f, 0.85f, 1f)), (FlexDirection, "column")
        };

        public static VirtualNode Render(Dictionary<string, object> props, IReadOnlyList<VirtualNode> children)
        {
            var (isListVisible, setListVisible) = Hooks.UseState(true);
            var (inputText, setInputText) = Hooks.UseState(string.Empty);
            var (isOptionEnabled, setOptionEnabled) = Hooks.UseState(false);
            var (isRadioSingleSelected, setRadioSingleSelected) = Hooks.UseState(false);
            var selectionChoices = Hooks.UseMemo(() => new List<string> { "One", "Two", "Three" }, 0);
            var (selectionIndex, setSelectionIndex) = Hooks.UseState(0);
            var (repeatClickCount, setRepeatClickCount) = Hooks.UseState(0);
            var (currentTime, setCurrentTime) = Hooks.UseState(DateTime.Now);
            // New element demo state
            var (sliderValue, setSliderValue) = Hooks.UseState(0.5f);
            var ddChoices = Hooks.UseMemo(() => new List<string> { "Alpha", "Beta", "Gamma" }, 0);
            var (ddValue, setDdValue) = Hooks.UseState("Beta");
            var (foldoutOpen, setFoldoutOpen) = Hooks.UseState(true);
            var rootElement = Hooks.UseRef();
            Hooks.UseEffect(() =>
            {
                if (rootElement == null) { return null; }
                var timerHandle = rootElement.schedule.Execute(() => setCurrentTime(DateTime.Now)).Every(1000);
                return () => { try { timerHandle?.Pause(); } catch { } };
            }, Array.Empty<object>());
            var initialItems = Hooks.UseMemo(() =>
            {
                var seededItems = new List<SharedRowItem>();
                for (int i = 1; i <= 5; i++) seededItems.Add(new SharedRowItem { Id = Guid.NewGuid().ToString("N"), Text = $"Item {i}" });
                return seededItems;
            });
            var (listItems, setListItems) = Hooks.UseState(initialItems);
            var listRowRenderer = Hooks.UseMemo(() => (Func<int, object, VirtualNode>)((index, itemObj) =>
            {
                var typedItem = itemObj as SharedRowItem;
                return V.Func(SharedListViewRow.Render, new Dictionary<string, object>
                {
                    { "item", typedItem },
                    { "index", index },
                    { "onRemove", (Action<SharedRowItem>)(removeItem =>
                        {
                            if (removeItem == null) { return; }
                            var copy = new List<SharedRowItem>(listItems);
                            int foundIndex = copy.FindIndex(r => r.Id == removeItem.Id);
                            if (foundIndex >= 0) { copy.RemoveAt(foundIndex); setListItems(copy); }
                        }) }
                }, key: typedItem != null ? $"{typedItem.Id}-{index}" : $"row-missing-{index}");
            }), listItems);
            ListViewProps listViewProps = new() { Items = listItems, FixedItemHeight = 20f, Selection = UnityEngine.UIElements.SelectionType.None, Row = listRowRenderer };
            ButtonProps toggleListButtonProps = new() { Text = isListVisible ? "Hide List" : "Show List", OnClick = () => setListVisible(!isListVisible), Style = new Style { (MarginTop, 8f), (Width, 120f), (Height, 28f) } };
            TextFieldProps inputTextFieldProps = new()
            {
                Style = TextInputStyle, Placeholder = "Type here...", HidePlaceholderOnFocus = false, Value = inputText,
                LabelText = string.IsNullOrEmpty(inputText) ? string.Empty : ("Value: " + inputText),
                OnChange = e => setInputText(e.newValue)
            };
            ButtonProps updateFirstItemButtonProps = new()
            {
                Text = "Change First Item",
                OnClick = () =>
                {
                    if (listItems.Count > 0)
                    {
                        var copy = new List<SharedRowItem>(listItems);
                        copy[0] = new SharedRowItem { Id = copy[0].Id, Text = "UPDATED " + DateTime.Now.ToLongTimeString() };
                        setListItems(copy);
                    }
                },
                Style = new Style { (MarginTop, 8f), (Width, 160f), (Height, 28f) }
            };
            var listContent = isListVisible ? V.VisualElement(new Dictionary<string, object> { { "style", ListContainerStyle } }, null, V.ListView(listViewProps)) : V.Text("List hidden");

            Debug.Log($"blaaaa: {sliderValue}");
            // Original page content grouped for ScrollView
            VirtualNode PageBody() => V.VisualElement(new Dictionary<string, object> { { "style", PageStyle } }, key: "shared-page-root",
                V.VisualElement(new Dictionary<string, object> { { "style", TopBarStyle } }, null,
                    V.VisualElement(new Dictionary<string, object> { { "style", LeftBoxStyle } }, null, V.Text("Left")),
                    V.TextField(inputTextFieldProps),
                    V.VisualElement(new Dictionary<string, object> { { "style", RightBoxStyle } }, null, V.Text("Right"))
                ),
                V.Label(new LabelProps { Text = "Now: " + currentTime.ToLongTimeString() }),
                V.Button(toggleListButtonProps),
                V.Button(updateFirstItemButtonProps),
                listContent,
                V.VisualElement(new Dictionary<string, object> { { "style", ExtrasContainerStyle } }, key: "extras",
                    V.Label(new LabelProps { Text = "Extras" }),
                    V.GroupBox(new GroupBoxProps
                    {
                        Text = "GroupBox",
                        ContentContainer = new Dictionary<string, object> { { "style", new Style { (PaddingLeft, 6f), (PaddingTop, 4f) } } }
                    }, null,
                        V.Label(new LabelProps { Text = "Inside group" }, key: "inner-one")
                    ),
                    V.Toggle(new ToggleProps
                    {
                        Text = "Enable option",
                        Value = isOptionEnabled,
                        OnChange = e => setOptionEnabled(e.newValue)
                    }),
                    V.RadioButton(new RadioButtonProps
                    {
                        Text = "Single radio",
                        Value = isRadioSingleSelected,
                        OnChange = e => setRadioSingleSelected(e.newValue)
                    }),
                    V.RadioButtonGroup(new RadioButtonGroupProps
                    {
                        Choices = selectionChoices,
                        Index = selectionIndex,
                        OnChange = e => setSelectionIndex(e.newValue)
                    }, null,
                        V.Label(new LabelProps { Text = "Pick one" }, key: "radio-label")
                    ),
                    V.ProgressBar(new ProgressBarProps { Value = repeatClickCount % 100, Title = "Progress" }),
                    V.RepeatButton(new RepeatButtonProps { Text = $"Repeat ({repeatClickCount})", OnClick = () => setRepeatClickCount(repeatClickCount + 1) })
                ),
                V.Func(BottomBarFunc.Render, new Dictionary<string, object>
                {
                    { "inputValue", inputText },
                    { "setTextValue", setInputText }
                }),
                // New components demo section
                V.GroupBox(new GroupBoxProps
                {
                    Text = "New Components",
                    ContentContainer = new Dictionary<string, object> { { "style", new Style { (PaddingLeft, 8f), (PaddingRight, 8f), (PaddingTop, 6f), (PaddingBottom, 6f) } } }
                }, null,
                    // Foldout first (ensure visibility)
                    V.Foldout(new FoldoutProps
                    {
                        Text = "More settings",
                        Value = foldoutOpen,
                        OnChange = e => setFoldoutOpen(e.newValue),
                        Header = new Dictionary<string, object> { { "style", new Style { (BackgroundColor, new UColor(0.95f, 0.95f, 0.95f, 1f) ), (PaddingLeft, 4f), (PaddingTop, 2f), (PaddingBottom, 2f) } } },
                        ContentContainer = new Dictionary<string, object> { { "style", new Style { (PaddingLeft, 6f) } } }
                    }, null,
                        V.Label(new LabelProps { Text = "Inside foldout" })
                    ),
                    // Image demo (uses background color when no sprite/texture)
                    V.Image(new ImageProps
                    {
                        Style = new Style { (Width, 96f), (Height, 96f), (BackgroundColor, new UColor(0.7f, 0.85f, 1f, 1f)), (BorderRadius, 6f) }
                    }, key: "img-demo"),
                    // Slider demo
                    V.Label(new LabelProps { Text = $"Slider: {sliderValue:F2}" }),
                    V.Slider(new SliderProps
                    {
                        LowValue = 0f,
                        HighValue = 1f,
                        Value = sliderValue,
                        Direction = "horizontal",
                        OnChange = e => setSliderValue(e.newValue),
                        Style = new Style { (Width, 200f) }
                    }),
                    // Dropdown demo
                    V.DropdownField(new DropdownFieldProps
                    {
                        Choices = ddChoices,
                        Value = ddValue,
                        OnChange = e => setDdValue(e.newValue)
                    })
                )
            );

            // Outer wrapper (blue full-screen) -> Safe area wrapper (green) -> ScrollView
            return V.VisualElement(new Dictionary<string, object>
            {
                { "style", new Style { (BackgroundColor, new UColor(0.2f, 0.4f, 0.8f, 1f)), (FlexGrow, 1f) } }
            }, key: "outer-wrap",
                V.VisualElementSafe(new Style { (BackgroundColor, new UColor(0.2f, 0.6f, 0.2f, 1f)), (FlexGrow, 1f) }, key: "safe-wrap",
                    V.ScrollView(new ScrollViewProps
                    {
                        Mode = "vertical",
                        Style = new Style { (FlexGrow, 1f) }
                    }, null,
                        PageBody()
                    )
                )
            );
        }
    }
}
