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

        public static VirtualNode Render(Dictionary<string, object> props, IReadOnlyList<VirtualNode> children)
        {
            var (showList, setShowList) = Hooks.UseState(true);
            var (textValue, setTextValue) = Hooks.UseState("");


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

            // (Removed test cases that intentionally failed to compile)

            IList listItems = Hooks.UseMemo(() =>
            {
                var list = new List<string>();
                for (int i = 1; i <= 20; i++)
                {
                    list.Add($"Item {i}");
                }
                return (IList)list;
            });

            ListViewProps listViewProps = new()
            {
                Items = listItems,
                FixedItemHeight = 10f,
                MakeItem = () => new UnityEngine.UIElements.Label(),
                BindItem = (ve, i) => ((UnityEngine.UIElements.Label)ve).text = listItems[i]?.ToString()
            };


            VirtualNode conditionalList = showList
                ? V.VisualElement(new Dictionary<string, object> { { "style", ListContainerStyle } }, null,
                    V.ListView(listViewProps)
                  )
                : V.Text("List hidden");

            return V.VisualElement(new Dictionary<string, object> { { "style", PageStyle } }, null,
                V.VisualElement(new Dictionary<string, object> { { "style", TopBarStyle } }, null,
                    V.VisualElement(new Dictionary<string, object> { { "style", LeftBoxStyle } }, null, V.Text("Left")),
                    V.TextField(TextFieldProps),
                    
                    V.VisualElement(new Dictionary<string, object> { { "style", RightBoxStyle } }, null, V.Text("Right"))
                ),
                V.Button(toggleButtonProps),
                conditionalList,
                V.Component<BottomBarComponent>(new Dictionary<string, object>
                {
                    { "inputValue", textValue },
                    { "setTextValue", (System.Action<string>)setTextValue }
                })
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
