export const VISUAL_ELEMENT_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

public static class VisualElementExamples
{
  private static readonly Style ContainerStyle = new Style
  {
    (StyleKeys.FlexDirection, FlexDirection.Column),
    (StyleKeys.PaddingLeft, 8f),
    (StyleKeys.PaddingTop, 4f),
    (StyleKeys.Gap, 4f),
  };

  public static VirtualNode Render(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    return V.VisualElement(
      ContainerStyle,
      null,
      V.Label(new LabelProps { Text = "VisualElement container" }),
      V.Button(new ButtonProps { Text = "Click me" })
    );
  }
}`

export const VISUAL_ELEMENT_SAFE = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

public static class VisualElementSafeExamples
{
  private static readonly Style SafeStyle = new Style
  {
    (StyleKeys.BackgroundColor, new Color(0.15f, 0.15f, 0.15f, 1f)),
  };

  public static VirtualNode Render(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    // VisualElementSafe merges user padding with safe-area insets.
    return V.VisualElementSafe(
      SafeStyle,
      null,
      V.Label(new LabelProps { Text = "Safe-area aware root" })
    );
  }
}`
