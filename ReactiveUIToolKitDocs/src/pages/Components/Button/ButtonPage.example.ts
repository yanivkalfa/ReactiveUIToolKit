export const BUTTON_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System;
using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;

public static class ButtonExamples
{
  private static readonly Style ButtonStyle = new Style { (StyleKeys.MarginTop, 4f) };

  public static VirtualNode Render(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
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
        Style = ButtonStyle,
      }
    );
  }
}`

