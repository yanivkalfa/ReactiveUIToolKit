export const SLIDER_INT_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

public static class SliderIntExamples
{
  public static VirtualNode Render(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(5);

    void OnChange(ChangeEvent<int> evt)
    {
      setValue.Set(evt.newValue);
    }

    return V.SliderInt(
      new SliderIntProps
      {
        LowValue = 0,
        HighValue = 10,
        Value = value,
        Direction = "Horizontal",
      }
    );
  }
}`

