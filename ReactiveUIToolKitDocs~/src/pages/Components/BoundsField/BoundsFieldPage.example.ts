export const BOUNDS_FIELD_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

public static class BoundsFieldExamples
{
  private static readonly Style VisualInputStyle = new Style
  {
    (StyleKeys.PaddingLeft, 4f),
  };

  // Function component – pass BoundsFieldExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (bounds, setBounds) = Hooks.UseState(new Bounds(Vector3.zero, new Vector3(1, 1, 1)));

    void OnChange(ChangeEvent<Bounds> evt)
    {
      setBounds(evt.newValue);
    }

    return V.BoundsField(
      new BoundsFieldProps
      {
        Value = bounds,
        Label = new LabelProps { Text = "Bounds" }.ToDictionary(),
        VisualInput = new Dictionary<string, object>
        {
          { "style", VisualInputStyle },
        },
      }
    );
  }
}`

