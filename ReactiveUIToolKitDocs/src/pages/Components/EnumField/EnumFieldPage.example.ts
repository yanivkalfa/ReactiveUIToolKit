export const ENUM_FIELD_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

public enum ExampleEnum
{
  A,
  B,
  C,
}

public static class EnumFieldExamples
{
  private static readonly Style InputStyle = new Style { (StyleKeys.PaddingLeft, 4f) };

  public static VirtualNode Render(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
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
        VisualInput = new Dictionary<string, object>
        {
          { "style", InputStyle },
        },
      }
    );
  }
}`

