export const VECTOR4_FIELD_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

public static class Vector4FieldExamples
{
  private static readonly Style InputStyle = new Style { (StyleKeys.PaddingLeft, 4f) };

  // Function component – pass Vector4FieldExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(new Vector4(1f, 2f, 3f, 4f));

    void OnChange(ChangeEvent<Vector4> evt)
    {
      setValue(evt.newValue);
    }

    return V.Vector4Field(
      new Vector4FieldProps
      {
        Value = value,
        OnChange = OnChange,
        Label = new LabelProps { Text = "Vector4" }.ToDictionary(),
        VisualInput = new Dictionary<string, object>
        {
          { "style", InputStyle },
        },
      }
    );
  }
}`
