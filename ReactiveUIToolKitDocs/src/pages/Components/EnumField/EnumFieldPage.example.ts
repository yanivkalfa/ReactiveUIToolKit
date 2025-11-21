export const ENUM_FIELD_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;

public enum ExampleEnum
{
  A,
  B,
  C,
}

public static class EnumFieldExamples
{
  public static VirtualNode Render(
    System.Collections.Generic.Dictionary<string, object> props,
    System.Collections.Generic.IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(ExampleEnum.B);

    void OnChange(ChangeEvent<System.Enum> evt)
    {
      setValue.Set((ExampleEnum)evt.newValue);
    }

    return V.EnumField(
      new EnumFieldProps
      {
        EnumType = typeof(ExampleEnum).AssemblyQualifiedName,
        Value = value,
        Label = new LabelProps { Text = "Example enum" }.ToDictionary(),
      }
    );
  }
}`

