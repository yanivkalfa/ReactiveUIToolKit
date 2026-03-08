using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;
using static ReactiveUITK.Props.Typed.StyleKeys;

namespace ReactiveUITK.Samples.Shared
{
    public static class ShowcaseNewComponentsPanel
    {
        public sealed class Props : IProps
        {
            public bool FoldoutOpen { get; set; }
            public Action<ChangeEvent<bool>> OnFoldoutChange { get; set; }
            public float SliderValue { get; set; }
            public Action<ChangeEvent<float>> OnSliderChange { get; set; }
            public int SliderIntValue { get; set; }
            public Action<ChangeEvent<int>> OnSliderIntChange { get; set; }
            public List<string> DdChoices { get; set; }
            public string DdValue { get; set; }
            public Action<ChangeEvent<string>> OnDdChange { get; set; }
        }

        public static VirtualNode Render(IProps rawProps, IReadOnlyList<VirtualNode> children)
        {
            var p = rawProps as Props;
            var sliderValue = p?.SliderValue ?? 0.5f;
            var sliderIntValue = p?.SliderIntValue ?? 5;

            var newComponentsGroupProps = new GroupBoxProps
            {
                Text = "New Components",
                ContentContainer = new Dictionary<string, object>
                {
                    { "style", SharedDemoPageStyles.NewCompsGroupContentStyle },
                },
            };

            var foldoutHeaderStyle = SharedDemoPageStyles.FoldoutHeaderStyle;
            var foldoutContentStyle = SharedDemoPageStyles.FoldoutContentStyle;

            var foldoutProps = new FoldoutProps
            {
                Text = "More settings",
                Value = p?.FoldoutOpen ?? true,
                OnChange = p?.OnFoldoutChange,
                Header = new Dictionary<string, object> { { "style", foldoutHeaderStyle } },
                ContentContainer = new Dictionary<string, object>
                {
                    { "style", foldoutContentStyle },
                },
            };

            return V.GroupBox(
                newComponentsGroupProps,
                null,
                V.Foldout(
                    foldoutProps,
                    null,
                    V.Label(new LabelProps { Text = "Inside foldout" })
                ),
                V.Image(new ImageProps { Style = SharedDemoPageStyles.ImageDemoStyle }),
                V.Label(new LabelProps { Text = $"Slider: {sliderValue:F2}" }),
                V.Slider(new SliderProps
                {
                    LowValue = 0f,
                    HighValue = 1f,
                    Value = sliderValue,
                    Direction = "horizontal",
                    OnChange = p?.OnSliderChange,
                    Style = SharedDemoPageStyles.SliderWidthStyle,
                }),
                V.Label(new LabelProps { Text = $"SliderInt: {sliderIntValue}" }),
                V.SliderInt(new SliderIntProps
                {
                    LowValue = 0,
                    HighValue = 10,
                    Value = sliderIntValue,
                    OnChange = p?.OnSliderIntChange,
                }),
#if UNITY_EDITOR
                V.HelpBox(new HelpBoxProps
                {
                    MessageType = sliderIntValue % 3 == 0
                        ? "error"
                        : (sliderIntValue % 2 == 0 ? "warning" : "info"),
                    Text = "This is a HelpBox showing state-driven message type.",
                }),
#endif
                V.DropdownField(new DropdownFieldProps
                {
                    Choices = p?.DdChoices,
                    Value = p?.DdValue,
                    OnChange = p?.OnDdChange,
                })
            );
        }
    }
}
