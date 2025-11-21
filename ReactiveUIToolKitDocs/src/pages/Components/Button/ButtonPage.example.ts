export const BUTTON_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;

public static class ButtonExamples
{
  public static VirtualNode Render(
    System.Collections.Generic.Dictionary<string, object> props,
    System.Collections.Generic.IReadOnlyList<VirtualNode> children
  )
  {
    var (count, setCount) = Hooks.UseState(0);

    void OnClick()
    {
      setCount.Set(previous => previous + 1);
      Debug.Log($"Clicked {count + 1} times");
    }

    return V.Button(
      new ButtonProps
      {
        Text = $"Click me ({count})",
        OnClick = OnClick,
        Style = new Style { (StyleKeys.MarginTop, 4f) },
      }
    );
  }
}`

