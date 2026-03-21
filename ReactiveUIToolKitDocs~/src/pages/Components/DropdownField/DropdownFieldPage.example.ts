export const DROPDOWN_FIELD_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections;
using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

public static class DropdownFieldExamples
{
  private static readonly Style InputStyle = new Style { (StyleKeys.PaddingLeft, 4f) };

  // Function component – pass DropdownFieldExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (index, setIndex) = Hooks.UseState(0);

    IList choices = new[] { "Red", "Green", "Blue" };

    void OnChange(ChangeEvent<string> evt)
    {
      setIndex(previous => choices.IndexOf(evt.newValue));
    }

    return V.DropdownField(
      new DropdownFieldProps
      {
        Choices = choices,
        SelectedIndex = index,
        Label = new LabelProps { Text = "Color" }.ToDictionary(),
        VisualInput = new Dictionary<string, object>
        {
          { "style", InputStyle },
        },
      }
    );
  }
}`

