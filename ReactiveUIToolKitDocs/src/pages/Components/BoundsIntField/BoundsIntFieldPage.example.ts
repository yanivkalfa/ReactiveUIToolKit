export const BOUNDS_INT_FIELD_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;

public static class BoundsIntFieldExamples
{
  public static VirtualNode Render(
    System.Collections.Generic.Dictionary<string, object> props,
    System.Collections.Generic.IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(new BoundsInt(1, 2, 3, 4, 5, 6));

    void OnChange(ChangeEvent<BoundsInt> evt)
    {
      setValue.Set(evt.newValue);
    }

    return V.BoundsIntField(
      new BoundsIntFieldProps
      {
        Value = value,
        Label = new LabelProps { Text = "BoundsInt" }.ToDictionary(),
      }
    );
  }
}`

