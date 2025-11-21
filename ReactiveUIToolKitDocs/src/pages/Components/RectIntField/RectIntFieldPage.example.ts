export const RECT_INT_FIELD_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

public static class RectIntFieldExamples
{
  public static VirtualNode Render(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (rect, setRect) = Hooks.UseState(new RectInt(0, 0, 16, 16));

    var visualInputStyle = new Style
    {
      (StyleKeys.PaddingLeft, 4f),
    };

    return V.RectIntField(
      new RectIntFieldProps
      {
        Value = rect,
        Label = new LabelProps { Text = "RectInt" }.ToDictionary(),
        VisualInput = new Dictionary<string, object>
        {
          { "style", visualInputStyle },
        },
      }
    );
  }
}`

