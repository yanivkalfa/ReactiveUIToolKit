export const RADIO_BUTTON_GROUP_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

public static class RadioButtonGroupExamples
{
  public static VirtualNode Render(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (index, setIndex) = Hooks.UseState(0);

    void OnChange(ChangeEvent<int> evt)
    {
      setIndex.Set(evt.newValue);
    }

    var contentContainerProps = new Dictionary<string, object>
    {
      { "style", new Style { (StyleKeys.FlexDirection, "row"), (StyleKeys.Gap, 8f) } },
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

