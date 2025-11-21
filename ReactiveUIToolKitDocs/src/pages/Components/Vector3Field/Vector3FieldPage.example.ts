export const VECTOR3_FIELD_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

public static class Vector3FieldExamples
{
  private static readonly Style InputStyle = new Style { (StyleKeys.PaddingLeft, 4f) };

  public static VirtualNode Render(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(new Vector3(1f, 2f, 3f));

    void OnChange(ChangeEvent<Vector3> evt)
    {
      setValue.Set(evt.newValue);
    }

    return V.Vector3Field(
      new Vector3FieldProps
      {
        Value = value,
        OnChange = OnChange,
        Label = new LabelProps { Text = "Vector3" }.ToDictionary(),
        VisualInput = new Dictionary<string, object>
        {
          { "style", InputStyle },
        },
      }
    );
  }
}`
