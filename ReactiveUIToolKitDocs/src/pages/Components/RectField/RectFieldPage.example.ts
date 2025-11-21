export const RECT_FIELD_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

public static class RectFieldExamples
{
  public static VirtualNode Render(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (rect, setRect) = Hooks.UseState(new Rect(0, 0, 128, 64));

    var visualInputStyle = new Style
    {
      (StyleKeys.PaddingLeft, 4f),
    };

    return V.RectField(
      new RectFieldProps
      {
        Value = rect,
        Label = new LabelProps { Text = "Rect" }.ToDictionary(),
        VisualInput = new Dictionary<string, object>
        {
          { "style", visualInputStyle },
        },
      }
    );
  }
}`

