export const BOUNDS_FIELD_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

public static class BoundsFieldExamples
{
  public static VirtualNode Render(
    System.Collections.Generic.Dictionary<string, object> props,
    System.Collections.Generic.IReadOnlyList<VirtualNode> children
  )
  {
    var (bounds, setBounds) = Hooks.UseState(new Bounds(Vector3.zero, new Vector3(1, 1, 1)));

    void OnChange(ChangeEvent<Bounds> evt)
    {
      setBounds.Set(evt.newValue);
    }

    return V.BoundsField(
      new BoundsFieldProps
      {
        Value = bounds,
        Label = new LabelProps { Text = "Bounds" }.ToDictionary(),
        VisualInput = new Style
        {
          (StyleKeys.PaddingLeft, 4f),
        },
      }
    );
  }
}`

