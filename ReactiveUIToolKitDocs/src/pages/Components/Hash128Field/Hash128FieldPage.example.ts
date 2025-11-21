export const HASH128_FIELD_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

public static class Hash128FieldExamples
{
  public static VirtualNode Render(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(new Hash128(1, 2, 3, 4));

    void OnChange(ChangeEvent<Hash128> evt)
    {
      setValue.Set(evt.newValue);
    }

    var inputStyle = new Style { (StyleKeys.PaddingLeft, 4f) };

    return V.Hash128Field(
      new Hash128FieldProps
      {
        Value = value,
        Label = new LabelProps { Text = "Hash128" }.ToDictionary(),
        VisualInput = new Dictionary<string, object>
        {
          { "style", inputStyle },
        },
      }
    );
  }
}`

