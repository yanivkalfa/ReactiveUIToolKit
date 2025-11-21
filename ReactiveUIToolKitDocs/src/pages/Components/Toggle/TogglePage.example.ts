export const TOGGLE_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

public static class ToggleExamples
{
  private static readonly Style InputStyle = new Style { (StyleKeys.MarginRight, 4f) };

  public static VirtualNode Render(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(true);

    void OnChange(ChangeEvent<bool> evt)
    {
      setValue.Set(evt.newValue);
    }

    var inputProps = new Dictionary<string, object>
    {
      { "style", InputStyle },
    };

    return V.Toggle(
      new ToggleProps
      {
        Text = "Enabled",
        Value = value,
        Input = inputProps,
      }
    );
  }
}`
