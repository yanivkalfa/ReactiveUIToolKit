export const RADIO_BUTTON_GROUP_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

public static class RadioButtonGroupExamples
{
  private static readonly Style ContentContainerStyle = new Style { (StyleKeys.FlexDirection, "row"), (StyleKeys.Gap, 8f) };

  // Function component – pass RadioButtonGroupExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (index, setIndex) = Hooks.UseState(0);

    void OnChange(ChangeEvent<int> evt)
    {
      setIndex(evt.newValue);
    }

    var contentContainerProps = new Dictionary<string, object>
    {
      { "style", ContentContainerStyle },
    };

    return V.RadioButtonGroup(
      new RadioButtonGroupProps
      {
        Choices = new[] { "Option A", "Option B", "Option C" },
        Index = index,
        ContentContainer = contentContainerProps,
      }
    );
  }
}`
