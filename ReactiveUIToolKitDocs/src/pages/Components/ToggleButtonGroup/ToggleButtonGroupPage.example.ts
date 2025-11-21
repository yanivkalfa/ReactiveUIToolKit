export const TOGGLE_BUTTON_GROUP_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;

public static class ToggleButtonGroupExamples
{
  public static VirtualNode Render(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(1);

    return V.ToggleButtonGroup(
      new ToggleButtonGroupProps { Value = value },
      key: null,
      V.Button(new ButtonProps { Text = "One" }),
      V.Button(new ButtonProps { Text = "Two" }),
      V.Button(new ButtonProps { Text = "Three" })
    );
  }
}`

