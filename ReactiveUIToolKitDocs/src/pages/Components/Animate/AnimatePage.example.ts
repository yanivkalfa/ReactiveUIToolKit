export const ANIMATE_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Core.Animation;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

public static class AnimateExamples
{
  public static VirtualNode Render(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var tracks = new List<AnimateTrack>
    {
      AnimateTrack.Property(
        property: StyleKeys.BackgroundColor,
        from: Color.gray,
        to: Color.cyan,
        durationSeconds: 0.75f,
        easing: Easing.InOutQuad
      ),
    };

    return V.Animate(
      new AnimateProps
      {
        Tracks = tracks,
      },
      null,
      V.Box(
        new BoxProps
        {
          Style = new Style
          {
            (StyleKeys.Width, 120f),
            (StyleKeys.Height, 32f),
            (StyleKeys.AlignItems, Align.Center),
            (StyleKeys.JustifyContent, Justify.Center),
          },
        },
        V.Label(new LabelProps { Text = "Animated box" })
      )
    );
  }
}`

