export const SLIDER_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

public static class SliderExamples
{
  // Function component – pass SliderExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(0.5f);

    void OnChange(ChangeEvent<float> evt)
    {
      setValue(evt.newValue);
    }

    var trackStyle = new Style
    {
      { StyleKeys.BackgroundColor, new Color(0.2f, 0.2f, 0.2f, 1f) },
      { StyleKeys.Height, 3f },
    };

    var handleStyle = new Style
    {
      { StyleKeys.Width, 12f },
      { StyleKeys.Height, 12f },
      { StyleKeys.BorderRadius, 6f },
      { StyleKeys.BackgroundColor, Color.white },
    };

    return V.Slider(
      new SliderProps
      {
        LowValue = 0f,
        HighValue = 1f,
        Value = value,
        Direction = "horizontal",
        Style = new Style { { StyleKeys.Width, 220f } },
        Track = new Dictionary<string, object>
        {
          { "style", trackStyle },
        },
        Handle = new Dictionary<string, object>
        {
          { "style", handleStyle },
        },
        OnChange = OnChange,
      }
    );
  }
}`

