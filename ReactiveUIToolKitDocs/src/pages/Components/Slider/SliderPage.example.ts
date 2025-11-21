export const SLIDER_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

public static class SliderExamples
{
  public static VirtualNode Render(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(0.5f);

    void OnChange(ChangeEvent<float> evt)
    {
      setValue.Set(evt.newValue);
    }

    return V.Slider(
      new SliderProps
      {
        LowValue = 0f,
        HighValue = 1f,
        Value = value,
        Direction = "Horizontal",
      }
    );
  }
}`

