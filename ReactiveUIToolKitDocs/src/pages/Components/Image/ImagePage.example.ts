export const IMAGE_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;

public static class ImageExamples
{
  public static VirtualNode Render(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var texture = props != null && props.TryGetValue("texture", out var t) ? t as Texture2D : null;

    return V.Image(
      new ImageProps
      {
        Texture = texture,
        ScaleMode = "ScaleToFit",
        Style = new Style { (StyleKeys.Width, 128f), (StyleKeys.Height, 128f) },
      }
    );
  }
}`

