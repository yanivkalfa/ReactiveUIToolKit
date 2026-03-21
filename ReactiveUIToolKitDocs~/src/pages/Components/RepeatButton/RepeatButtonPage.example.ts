export const REPEAT_BUTTON_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;

public static class RepeatButtonExamples
{
  // Function component – pass RepeatButtonExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (count, setCount) = Hooks.UseState(0);

    void OnClick()
    {
      setCount(prev => prev + 1);
    }

    return V.RepeatButton(
      new RepeatButtonProps
      {
        Text = $"Hold to repeat ({count})",
        OnClick = OnClick,
      }
    );
  }
}`

