export const PROGRESS_BAR_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;

public static class ProgressBarExamples
{
  private static readonly Style TrackStyle = new Style
  {
    (StyleKeys.BackgroundColor, new Color(0.02f, 0.2f, 0.02f, 0.7f)),
    (StyleKeys.BorderColor, new Color(0.07f, 0.9f, 0.22f, 1f)),
    (StyleKeys.BorderWidth, 2f),
    (StyleKeys.BorderRadius, 6f),
    (StyleKeys.Height, 30f),
  };

  private static readonly Style ProgressFillStyle = new Style
  {
    (StyleKeys.BackgroundColor, new Color(0.4f, 0.95f, 0.4f, 0.7f)),
    (StyleKeys.BorderRadius, 4f),
    (StyleKeys.MarginLeft, 2f),
    (StyleKeys.MarginRight, 2f),
    (StyleKeys.MarginTop, 2f),
    (StyleKeys.MarginBottom, 2f),
  };

  private static readonly Style TitleStyle = new Style
  {
    (StyleKeys.FontSize, 13f),
    (StyleKeys.TextAlign, "center"),
  };

  // Function component – pass ProgressBarExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(0.65f);

    var progressProps = new Dictionary<string, object>
    {
      { "style", ProgressFillStyle },
    };

    var titleElementProps = new Dictionary<string, object>
    {
      { "style", TitleStyle },
    };

    return V.ProgressBar(
      new ProgressBarProps
      {
        Title = $"Downloading - {(value * 100f):0}%",
        Value = value,
        Style = TrackStyle,
        Progress = progressProps,
        TitleElement = titleElementProps,
      }
    );
  }
}`
