using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;
using static ReactiveUITK.Props.Typed.StyleKeys;

namespace ReactiveUITK.Samples.Shared
{
    public static class ShowcaseExtrasPanel
    {
        public sealed class Props : IProps
        {
            public bool IsOptionEnabled { get; set; }
            public ChangeEventHandler<bool> OnOptionEnabledChange { get; set; }
            public bool IsRadioSingleSelected { get; set; }
            public ChangeEventHandler<bool> OnRadioSingleChange { get; set; }
            public List<string> SelectionChoices { get; set; }
            public int SelectionIndex { get; set; }
            public ChangeEventHandler<int> OnSelectionChange { get; set; }
            /// <summary>Used to fill the ProgressBar (repeatClickCount % 100).</summary>
            public int ProgressValue { get; set; }
            public int BatchClicks { get; set; }
            public Action OnBatchClick { get; set; }
        }

        public static VirtualNode Render(IProps rawProps, IReadOnlyList<VirtualNode> children)
        {
            var p = rawProps as Props;

            var groupBox1Props = new GroupBoxProps
            {
                Text = "GroupBox",
                ContentContainer = new Dictionary<string, object>
                {
                    { "style", new Style { (PaddingLeft, 6f), (PaddingTop, 4f) } },
                },
            };

            return V.VisualElement(
                new VisualElementProps { Style = SharedDemoPageStyles.ExtrasContainerStyle },
                null,
                V.Label(new LabelProps { Text = "Extras" }),
                V.GroupBox(
                    groupBox1Props,
                    null,
                    V.Label(new LabelProps { Text = "Inside group" })
                ),
                V.Toggle(new ToggleProps
                {
                    Text = "Enable option",
                    Value = p?.IsOptionEnabled ?? false,
                    OnChange = p?.OnOptionEnabledChange,
                }),
                V.RadioButton(new RadioButtonProps
                {
                    Text = "Single radio",
                    Value = p?.IsRadioSingleSelected ?? false,
                    OnChange = p?.OnRadioSingleChange,
                }),
                V.RadioButtonGroup(
                    new RadioButtonGroupProps
                    {
                        Choices = p?.SelectionChoices,
                        Index = p?.SelectionIndex ?? 0,
                        OnChange = p?.OnSelectionChange,
                    },
                    null,
                    V.Label(new LabelProps { Text = "Pick one" })
                ),
                V.ProgressBar(new ProgressBarProps
                {
                    Value = p?.ProgressValue ?? 0,
                    Title = "Progress",
                }),
                V.Button(new ButtonProps
                {
                    Text = $"Batch Test ({p?.BatchClicks ?? 0})",
                    OnClick = _ => p?.OnBatchClick?.Invoke(),
                    Style = new Style { (MarginLeft, 6f) },
                })
            );
        }
    }
}
