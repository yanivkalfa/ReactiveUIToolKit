export const SCROLL_VIEW_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;

public static class ScrollViewExamples
{
  public static VirtualNode Render(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var contentProps = new Dictionary<string, object>
    {
      { "style", new Style { (StyleKeys.Padding, 6f), (StyleKeys.RowGap, 4f) } },
    };

    var scrollViewProps = new ScrollViewProps
    {
      Mode = "Vertical",
      ContentContainer = contentProps,
      Style = new Style { (StyleKeys.Height, 200f) },
    };

    return V.ScrollView(
      scrollViewProps,
      key: null,
      V.Label(new LabelProps { Text = "Row 1" }),
      V.Label(new LabelProps { Text = "Row 2" }),
      V.Label(new LabelProps { Text = "Row 3" })
    );
  }
}`

