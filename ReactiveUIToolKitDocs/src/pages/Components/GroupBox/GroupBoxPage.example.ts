export const GROUP_BOX_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;

public static class GroupBoxExamples
{
  private static readonly Style OuterStyle = new Style
  {
    (StyleKeys.MarginTop, 8f),
    (StyleKeys.Padding, 6f),
  };

  private static readonly Style ContentContainerStyle = new Style
  {
    (StyleKeys.PaddingTop, 4f),
  };

  private static readonly Style LabelStyle = new Style
  {
    (StyleKeys.FontSize, 14f),
  };

  // Function component – pass GroupBoxExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var contentContainerProps = new Dictionary<string, object>
    {
      { "style", ContentContainerStyle },
    };

    var labelProps = new Dictionary<string, object>
    {
      { "style", LabelStyle },
    };

    return V.GroupBox(
      new GroupBoxProps
      {
        Text = "Group title",
        Style = OuterStyle,
        ContentContainer = contentContainerProps,
        Label = labelProps,
      },
      key: null,
      V.Label(new LabelProps { Text = "Content item 1" }),
      V.Label(new LabelProps { Text = "Content item 2" })
    );
  }
}`
