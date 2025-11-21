export const VECTOR3_INT_FIELD_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

public static class Vector3IntFieldExamples
{
  public static VirtualNode Render(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(new Vector3Int(1, 2, 3));

    var inputStyle = new Style { (StyleKeys.PaddingLeft, 4f) };

    return V.Vector3IntField(
      new Vector3IntFieldProps
      {
        Value = value,
        Label = new LabelProps { Text = "Vector3Int" }.ToDictionary(),
        VisualInput = new Dictionary<string, object>
        {
          { "style", inputStyle },
        },
      }
    );
  }
}`

