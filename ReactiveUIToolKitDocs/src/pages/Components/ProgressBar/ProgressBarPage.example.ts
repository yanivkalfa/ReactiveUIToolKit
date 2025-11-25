export const PROGRESS_BAR_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;

public static class ProgressBarExamples
{
  private static readonly Style ProgressStyle = new Style
  {
    (StyleKeys.BackgroundColor, new Color(0.2f, 0.6f, 0.9f, 1f)),
  };

  private static readonly Style TitleStyle = new Style
  {
    (StyleKeys.FontSize, 12f),
  };

  // Function component – pass ProgressBarExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(0.5f);

    var progressProps = new Dictionary<string, object>
    {
      { "style", ProgressStyle },
    };

    var titleElementProps = new Dictionary<string, object>
    {
      { "style", TitleStyle },
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
