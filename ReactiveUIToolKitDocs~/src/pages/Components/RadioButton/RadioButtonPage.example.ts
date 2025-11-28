export const RADIO_BUTTON_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

public static class RadioButtonExamples
{
  // Function component – pass RadioButtonExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(false);

    void OnChange(ChangeEvent<bool> evt)
    {
      setValue(evt.newValue);
    }

    return V.RadioButton(
      new RadioButtonProps
      {
        Text = "Option",
        Value = value,
        Label = new LabelProps { Text = "Option" }.ToDictionary(),
      }
    );
  }
}`

