export const TEXT_ELEMENT_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

public static class TextElementExamples
{
  private static readonly Style BoldTextStyle = new Style
  {
    (StyleKeys.UnityFontStyleAndWeight, FontStyle.Bold),
  };

  public static VirtualNode Render(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    return V.TextElement(
      new TextElementProps
      {
        Text = "Inline text element",
        Style = BoldTextStyle,
      }
    );
  }
}`
