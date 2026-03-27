export const TOGGLE_BUTTON_GROUP_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using static ReactiveUITK.Props.Typed.StyleKeys;

public static class ToggleButtonGroupExamples
{
  private static readonly Style ContainerStyle = new()
  {
    (StyleKeys.FlexDirection, "column"),
    (StyleKeys.Padding, 16f),
    (StyleKeys.BackgroundColor, new Color(0.11f, 0.11f, 0.11f, 0.85f)),
  };

  private static readonly Style StatusStyle = new()
  {
    (StyleKeys.FontSize, 13f),
    (StyleKeys.Color, new Color(0.85f, 0.95f, 1f, 1f)),
  };

  private static readonly string[] Options = new[] { "Alpha", "Beta", "Gamma" };

  // Function component: pass ToggleButtonGroupExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (selected, setSelected) = Hooks.UseState(0);

    return V.VisualElement(
      ContainerStyle,
      null,
      V.Label(new LabelProps { Text = "ToggleButtonGroup (Buttons inside)" }),
      V.ToggleButtonGroup(
        new ToggleButtonGroupProps { Value = selected },
        null,
        V.Button(new ButtonProps { Text = "0", OnClick = () => setSelected.Set(0) }),
        V.Button(new ButtonProps { Text = "1", OnClick = () => setSelected.Set(1) }),
        V.Button(new ButtonProps { Text = "2", OnClick = () => setSelected.Set(2) })
      ),
      V.Label(
        new LabelProps
        {
          Text = $"Selected option: {Options[selected]}",
          Style = StatusStyle,
        }
      )
    );
  }
}`
