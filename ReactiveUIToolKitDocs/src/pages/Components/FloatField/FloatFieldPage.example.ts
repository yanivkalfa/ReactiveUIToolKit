export const FLOAT_FIELD_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;

public static class FloatFieldExamples
{
  public static VirtualNode Render(
    System.Collections.Generic.Dictionary<string, object> props,
    System.Collections.Generic.IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(1.23f);

    void OnChange(ChangeEvent<float> evt)
    {
      setValue.Set(evt.newValue);
    }

    return V.FloatField(
      new FloatFieldProps
      {
        Value = value,
        Label = new LabelProps { Text = "Float" }.ToDictionary(),
      }
    );
  }
}`

