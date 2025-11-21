export const MIN_MAX_SLIDER_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;

public static class MinMaxSliderExamples
{
  public static VirtualNode Render(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (range, setRange) = Hooks.UseState((min: 20f, max: 80f));

    void Update(float min, float max)
    {
      setRange.Set(_ => (min, max));
    }

    return V.MinMaxSlider(
      new MinMaxSliderProps
      {
        MinValue = range.min,
        MaxValue = range.max,
        LowLimit = 0f,
        HighLimit = 100f,
        Style = new Style { (StyleKeys.Width, 200f) },
      }
    );
  }
}`

