using System.Collections.Generic;
using UnityEngine;
using ReactiveUITK.Core;
using ReactiveUITK;
using ReactiveUITK.Examples.ClassComponents; // for BottomBarComponent
using ReactiveUITK.Props.Typed;
using System.Collections;
using static ReactiveUITK.Props.Typed.StyleKeys;
using UColor = UnityEngine.Color;

namespace ReactiveUITK.Examples.FunctionalComponents
{
    public static class AppFunc
    {
        private static readonly Style TopBarStyle = new()
        {
            (BackgroundColor, UColor.white),
            (FlexDirection, "row"),
            (JustifyContent, "space-between"),
            (AlignItems, "center"),
            (FlexGrow, 1f),
            (PaddingLeft, 12f),
            (PaddingRight, 12f),
            (PaddingTop, 8f),
            (PaddingBottom, 8f),
            (BorderBottomWidth, 1f),
            (BorderBottomColor, new UColor(0.85f,0.85f,0.85f,1f))
        };

        private static readonly Style LeftBoxStyle = new()
        {
            (BackgroundColor, new UColor(0.2f,0.4f,0.9f,1f)),
            (TextColor, UColor.white),
            (PaddingLeft, 10f),
            (PaddingRight, 10f),
            (PaddingTop, 6f),
            (PaddingBottom, 6f),
            (BorderRadius, 4f),
            (FontSize, 14f)
        };

        private static readonly Style RightBoxStyle = new()
        {
            (BackgroundColor, new UColor(0.9f,0.3f,0.2f,1f)),
            (TextColor, UColor.white),
            (PaddingLeft, 10f),
            (PaddingRight, 10f),
            (PaddingTop, 6f),
            (PaddingBottom, 6f),
            (BorderRadius, 4f),
            (FontSize, 14f)
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
            (BorderColor, new UColor(0.8f,0.8f,0.8f,1f)),
            (BackgroundColor, new UColor(1f,1f,1f,1f))
        };

        private static readonly Style PageStyle = new()
        {
            (FlexDirection, "column"),
            (FlexGrow, 1f),
            (JustifyContent, "space-between"),
            (BackgroundColor, new UColor(0.95f,0.95f,0.95f,1f))
        };

        private static readonly Style ListContainerStyle = new()
        {
            (FlexGrow, 1f),
            (MarginTop, 8f)
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
            (BorderTopColor, new UColor(0.85f,0.85f,0.85f,1f)),
            (FlexDirection, "column")
        };

        public static VirtualNode Render(Dictionary<string, object> props, IReadOnlyList<VirtualNode> children)
        {
            var (showList, setShowList) = Hooks.UseState(true);
            var (textValue, setTextValue) = Hooks.UseState("");
            var (toggleValue, setToggleValue) = Hooks.UseState(false);
            var (radioChecked, setRadioChecked) = Hooks.UseState(false);
            List<string> radioChoices = Hooks.UseMemo(() => new List<string>{"One","Two","Three"}, 0);
            var (radioIndex, setRadioIndex) = Hooks.UseState(0);
            var (repeatClicks, setRepeatClicks) = Hooks.UseState(0);


            ButtonProps toggleButtonProps = new()
            {
                Text = showList ? "Hide List" : "Show List",
                OnClick = () => setShowList(!showList),
                Style = new Style
                {
                    (MarginTop, 8f),
                    (Width, 120f),
                    (Height, 28f)
                }
            };

            TextFieldProps TextFieldProps = new()
            {
                Style = TextInputStyle,
                Placeholder = "Type here...",
                HidePlaceholderOnFocus = false,
                Value = textValue,
                LabelText = string.IsNullOrEmpty(textValue) ? "" : ("Value: " + textValue),
                OnChange = (System.Action<UnityEngine.UIElements.ChangeEvent<string>>)(e => setTextValue(e.newValue))
            };

            // Immutable items list managed via state
            List<string> initialItems = Hooks.UseMemo(() =>
            {
                var list = new List<string>();
                for (int i = 1; i <= 20; i++)
                {
                    list.Add($"Item {i}");
                }
                return list;
            });
            var (items, setItems) = Hooks.UseState(initialItems);

            ListViewProps listViewProps = new()
            {
                Items = items,
                FixedItemHeight = 18f,
                Row = (i, item) =>
                {
                    string text = item?.ToString() ?? "<null>";
                    return V.VisualElement(new Style { (FlexDirection, "row") }, null,
                        V.Text(text)
                    );
                }
            };

            // Button to change the first item immutably to demonstrate row re-render
            ButtonProps changeFirstProps = new()
            {
                Text = "Change First Item",
                OnClick = () =>
                {
                    var copy = new List<string>(items);
                    if (copy.Count > 0)
                    {
                        copy[0] = "UPDATED " + System.DateTime.Now.ToLongTimeString();
                        setItems(copy);
                    }
                },
                Style = new Style { (MarginTop, 8f), (Width, 160f), (Height, 28f) }
            };


            VirtualNode conditionalList = V.Fragment("list-slot",
                showList
                    ? V.VisualElement(new Dictionary<string, object> { { "style", ListContainerStyle } }, key: "list-on",
                        V.ListView(listViewProps)
                      )
                    : V.Text("List hidden", key: "list-off")
            );

            return V.VisualElement(new Dictionary<string, object> { { "style", PageStyle } }, null,
                V.VisualElement(new Dictionary<string, object> { { "style", TopBarStyle } }, key: "topbar",
                    V.VisualElement(new Dictionary<string, object> { { "style", LeftBoxStyle } }, null, V.Text("Left")),
                    V.TextField(TextFieldProps),
                    
                    V.VisualElement(new Dictionary<string, object> { { "style", RightBoxStyle } }, null, V.Text("Right"))
                ),
                V.Button(toggleButtonProps, key: "btn-toggle"),
                V.Button(changeFirstProps, key: "btn-change-first"),
                conditionalList,
                V.VisualElement(new Dictionary<string, object> { { "style", ExtrasContainerStyle } }, key: "extras",
                    V.Label(new LabelProps { Text = "Extras" }, key: "extras-label"),
                    V.GroupBox(new GroupBoxProps { Text = "GroupBox", ContentContainer = new Dictionary<string, object> { { "style", new Style { (PaddingLeft, 6f), (PaddingTop, 4f) } } } }, key: "group-box",
                        V.Label(new LabelProps { Text = "Inside group" }, key: "group-box-inner-label")
                    ),
                    V.Toggle(new ToggleProps
                    {
                        Text = "Enable option",
                        Value = toggleValue,
                        OnChange = (System.Action<UnityEngine.UIElements.ChangeEvent<bool>>)(e => setToggleValue(e.newValue))
                    }, key: "toggle"),
                    V.RadioButton(new RadioButtonProps
                    {
                        Text = "Single radio",
                        Value = radioChecked,
                        OnChange = (System.Action<UnityEngine.UIElements.ChangeEvent<bool>>)(e => setRadioChecked(e.newValue))
                    }, key: "single-radio"),
                    V.RadioButtonGroup(new RadioButtonGroupProps
                    {
                        Choices = radioChoices,
                        Index = radioIndex,
                        OnChange = (System.Action<UnityEngine.UIElements.ChangeEvent<int>>)(e => setRadioIndex(e.newValue))
                    }, key: "radio-group",
                        V.Label(new LabelProps { Text = "Pick one" }, key: "radio-label")
                    ),
                    V.ProgressBar(new ProgressBarProps { Value = repeatClicks % 100, Title = "Progress" }, key: "progress"),
                    V.RepeatButton(new RepeatButtonProps { Text = $"Repeat ({repeatClicks})", OnClick = () => setRepeatClicks(repeatClicks + 1) }, key: "repeat-btn")
                ),
                V.Component<BottomBarComponent>(new Dictionary<string, object> 
                {
                    { "inputValue", textValue },
                    { "setTextValue", (System.Action<string>)setTextValue }
                }, key: "bottom-bar")
            );
        }
    }

    public sealed class AppFuncRoot : ReactiveComponent
    {
        private static readonly Style wrapperStyle = new()
        {
            (FlexDirection, "column"),
            (FlexGrow, 1f),
            (BackgroundColor, new Color(0.95f,0.95f,0.95f,1f))
        };

        protected override VirtualNode Render()
        {
            return V.VisualElement(new Dictionary<string, object>{{"style", wrapperStyle}}, null,
                V.Func(AppFunc.Render)
            );
        }
    }
}
