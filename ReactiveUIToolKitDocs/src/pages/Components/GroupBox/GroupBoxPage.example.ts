export const GROUP_BOX_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;

public static class GroupBoxExamples
{
  public static VirtualNode Render(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var outerStyle = new Style
    {
      (StyleKeys.MarginTop, 8f),
      (StyleKeys.Padding, 6f),
    };

    var contentContainerProps = new Dictionary<string, object>
    {
      { "style", new Style { (StyleKeys.PaddingTop, 4f) } },
    };

    var labelProps = new Dictionary<string, object>
    {
      { "style", new Style { (StyleKeys.FontSize, 14f) } },
    };

    return V.GroupBox(
      new GroupBoxProps
      {
        Text = "Group title",
        Style = outerStyle,
        ContentContainer = contentContainerProps,
        Label = labelProps,
      },
      key: null,
      V.Label(new LabelProps { Text = "Content item 1" }),
      V.Label(new LabelProps { Text = "Content item 2" })
    );
  }
}`

