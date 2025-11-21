export const VECTOR2_INT_FIELD_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

public static class Vector2IntFieldExamples
{
  public static VirtualNode Render(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(new Vector2Int(1, 2));

    var inputStyle = new Style { (StyleKeys.PaddingLeft, 4f) };

    return V.Vector2IntField(
      new Vector2IntFieldProps
      {
        Value = value,
        Label = new LabelProps { Text = "Vector2Int" }.ToDictionary(),
        VisualInput = new Dictionary<string, object>
        {
          { "style", inputStyle },
        },
      }
    );
  }
}`

