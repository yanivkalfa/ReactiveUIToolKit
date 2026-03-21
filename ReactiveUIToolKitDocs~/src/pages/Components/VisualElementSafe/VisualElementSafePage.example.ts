export const VISUAL_ELEMENT_SAFE_SIGNATURE = `public static VirtualNode VisualElementSafe(
  object elementPropsOrStyle = null,
  string key = null,
  params VirtualNode[] children
);`

export const VISUAL_ELEMENT_SAFE = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;
using UnityEngine;

public static class VisualElementSafeExamples
{
  private static readonly Style SafeStyle = new Style
  {
    (StyleKeys.BackgroundColor, new Color(0.15f, 0.15f, 0.15f, 1f)),
  };

  // Function component – pass VisualElementSafeExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    // VisualElementSafe merges user padding with safe-area insets.
    // You can pass either a Style or a props dictionary.
    return V.VisualElementSafe(new Dictionary<string, object>
      {
        { "pickingMode", PickingMode.Ignore },
        { "style", SafeStyle },
      },
      null,
      V.Label(new LabelProps { Text = "Safe-area aware root" })
    );
  }
}`
