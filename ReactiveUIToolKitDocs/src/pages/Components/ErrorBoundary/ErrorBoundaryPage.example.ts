export const ERROR_BOUNDARY_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System;
using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;

public static class ErrorBoundaryExamples
{
  private static readonly Style FallbackBoxStyle = new Style
  {
    (StyleKeys.PaddingLeft, 8f),
    (StyleKeys.PaddingTop, 4f),
  };

  public static VirtualNode Render(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var fallback = V.Box(
      new BoxProps
      {
        Style = FallbackBoxStyle,
      },
      V.Label(new LabelProps { Text = "Something went wrong." })
    );

    void OnError(Exception ex)
    {
      UnityEngine.Debug.LogException(ex);
    }

    return V.ErrorBoundary(
      new ErrorBoundaryProps
      {
        Fallback = fallback,
        OnError = OnError,
      },
      V.Func(
        (p, c) =>
        {
          var (value, setValue) = Hooks.UseState(0);
          if (value > 3)
          {
            throw new InvalidOperationException("Demo error");
          }
          return V.Button(
            new ButtonProps
            {
              Text = $"Clicks: {value}",
              OnClick = _ => setValue.Set(prev => prev + 1),
            }
          );
        }
      )
    );
  }
}`
