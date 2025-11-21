export const LABEL_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;

public static class LabelExamples
{
  private static readonly Style LabelStyle = new Style { (StyleKeys.FontSize, 16f) };

  public static VirtualNode Render(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    return V.Label(
      new LabelProps
      {
        Text = "Hello label",
        Style = LabelStyle,
      }
    );
  }
}`
