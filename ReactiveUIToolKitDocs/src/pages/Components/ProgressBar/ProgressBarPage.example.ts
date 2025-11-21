export const PROGRESS_BAR_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;

public static class ProgressBarExamples
{
  public static VirtualNode Render(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(0.5f);

    var progressStyle = new Style
    {
      (StyleKeys.BackgroundColor, new Color(0.2f, 0.6f, 0.9f, 1f)),
    };

    var titleStyle = new Style
    {
      (StyleKeys.FontSize, 12f),
    };

    var progressProps = new Dictionary<string, object>
    {
      { "style", progressStyle },
    };

    var titleElementProps = new Dictionary<string, object>
    {
      { "style", titleStyle },
    };

    return V.ProgressBar(
      new ProgressBarProps
      {
        Title = "Loading...",
        Value = value,
        Progress = progressProps,
        TitleElement = titleElementProps,
      }
    );
  }
}`

