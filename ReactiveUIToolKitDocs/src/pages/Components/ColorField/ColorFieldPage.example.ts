export const COLOR_FIELD_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

public static class ColorFieldExamples
{
  public static VirtualNode Render(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (color, setColor) = Hooks.UseState(new Color(0.2f, 0.6f, 0.9f, 1f));

    void OnChange(ChangeEvent<Color> evt)
    {
      setColor.Set(evt.newValue);
    }

    var inputStyle = new Style
    {
      (StyleKeys.PaddingLeft, 4f),
    };

    return V.ColorField(
      new ColorFieldProps
      {
        Value = color,
        Label = new LabelProps { Text = "Tint" }.ToDictionary(),
        VisualInput = new Dictionary<string, object>
        {
          { "style", inputStyle },
        },
      }
    );
  }
}`

