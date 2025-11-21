export const DROPDOWN_FIELD_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;

public static class DropdownFieldExamples
{
  public static VirtualNode Render(
    System.Collections.Generic.Dictionary<string, object> props,
    System.Collections.Generic.IReadOnlyList<VirtualNode> children
  )
  {
    var (index, setIndex) = Hooks.UseState(0);

    IList choices = new[] { "Red", "Green", "Blue" };

    void OnChange(ChangeEvent<string> evt)
    {
      setIndex.Set(previous => choices.IndexOf(evt.newValue));
    }

    return V.DropdownField(
      new DropdownFieldProps
      {
        Choices = choices,
        Index = index,
        Label = new LabelProps { Text = "Color" }.ToDictionary(),
      }
    );
  }
}`

