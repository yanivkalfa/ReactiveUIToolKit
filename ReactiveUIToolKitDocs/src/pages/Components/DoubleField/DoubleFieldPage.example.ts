export const DOUBLE_FIELD_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

public static class DoubleFieldExamples
{
  private static readonly Style InputStyle = new Style { (StyleKeys.PaddingLeft, 4f) };

  // Function component – pass DoubleFieldExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(3.14159);

    void OnChange(ChangeEvent<double> evt)
  {
      setValue(evt.newValue);
    }

    return V.DoubleField(
      new DoubleFieldProps
      {
        Value = value,
        Label = new LabelProps { Text = "Double" }.ToDictionary(),
        VisualInput = new Dictionary<string, object>
        {
          { "style", InputStyle },
        },
      }
    );
  }
}`

