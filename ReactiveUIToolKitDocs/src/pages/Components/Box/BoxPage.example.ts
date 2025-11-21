export const BOX_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;

public static class BoxExamples
{
  public static VirtualNode Render(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var outerStyle = new Style
    {
      (StyleKeys.Padding, 8f),
      (StyleKeys.BackgroundColor, new Color(0.15f, 0.15f, 0.2f, 1f)),
      (StyleKeys.BorderRadius, 4f),
    };

    var contentContainerProps = new Dictionary<string, object>
    {
      { "style", new Style { (StyleKeys.MarginTop, 4f) } },
    };

    return V.Box(
      new BoxProps
      {
        Style = outerStyle,
        ContentContainer = contentContainerProps,
      },
      key: null,
      V.Label(new LabelProps { Text = "Inside Box" })
    );
  }
}`

