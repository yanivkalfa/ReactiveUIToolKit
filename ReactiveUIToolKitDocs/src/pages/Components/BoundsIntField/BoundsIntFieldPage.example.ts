export const BOUNDS_INT_FIELD_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

public static class BoundsIntFieldExamples
{
  private static readonly Style VisualInputStyle = new Style
  {
    (StyleKeys.PaddingLeft, 4f),
  };

  // Function component – pass BoundsIntFieldExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(new BoundsInt(1, 2, 3, 4, 5, 6));

    void OnChange(ChangeEvent<BoundsInt> evt)
    {
      setValue(evt.newValue);
    }

    return V.BoundsIntField(
      new BoundsIntFieldProps
      {
        Value = value,
        Label = new LabelProps { Text = "BoundsInt" }.ToDictionary(),
        VisualInput = new Dictionary<string, object>
        {
          { "style", VisualInputStyle },
        },
      }
    );
  }
}`

