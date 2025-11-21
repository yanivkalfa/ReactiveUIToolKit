export const DOUBLE_FIELD_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;

public static class DoubleFieldExamples
{
  public static VirtualNode Render(
    System.Collections.Generic.Dictionary<string, object> props,
    System.Collections.Generic.IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(3.14159);

    void OnChange(ChangeEvent<double> evt)
    {
      setValue.Set(evt.newValue);
    }

    return V.DoubleField(
      new DoubleFieldProps
      {
        Value = value,
        Label = new LabelProps { Text = "Double" }.ToDictionary(),
      }
    );
  }
}`

