export const INTEGER_FIELD_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

public static class IntegerFieldExamples
{
  public static VirtualNode Render(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(42);

    void OnChange(ChangeEvent<int> evt)
    {
      setValue.Set(evt.newValue);
    }

    var inputStyle = new Style { (StyleKeys.PaddingLeft, 4f) };

    return V.IntegerField(
      new IntegerFieldProps
      {
        Value = value,
        Label = new LabelProps { Text = "Integer" }.ToDictionary(),
        VisualInput = new Dictionary<string, object>
        {
          { "style", inputStyle },
        },
      }
    );
  }
}`

