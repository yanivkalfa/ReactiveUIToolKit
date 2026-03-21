export const SCROLLER_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

public static class ScrollerExamples
{
  private static readonly Style ScrollerStyle = new Style
  {
    (StyleKeys.Width, 12f),
    (StyleKeys.Height, 120f),
  };

  // Function component – pass ScrollerExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(0f);

    void OnChange(ChangeEvent<float> evt)
    {
      setValue(evt.newValue);
    }

    return V.Scroller(
      new ScrollerProps
      {
        LowValue = 0f,
        HighValue = 100f,
        Value = value,
        Style = ScrollerStyle,
      }
    );
  }
}`
