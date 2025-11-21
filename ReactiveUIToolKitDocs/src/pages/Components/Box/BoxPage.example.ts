export const BOX_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;

public static class BoxExamples
{
  public static VirtualNode Render(
    System.Collections.Generic.Dictionary<string, object> props,
    System.Collections.Generic.IReadOnlyList<VirtualNode> children
  )
  {
    var style = new Style
    {
      (StyleKeys.Padding, 8f),
      (StyleKeys.BackgroundColor, new Color(0.15f, 0.15f, 0.2f, 1f)),
      (StyleKeys.BorderRadius, 4f),
    };

    return V.Box(
      new BoxProps { Style = style },
      key: null,
      V.Label(new LabelProps { Text = "Inside Box" })
    );
  }
}`

