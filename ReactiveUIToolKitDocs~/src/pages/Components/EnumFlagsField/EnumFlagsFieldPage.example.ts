export const ENUM_FLAGS_FIELD_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System;
using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

[Flags]
public enum ExampleFlags
{
  None = 0,
  A = 1 << 0,
  B = 1 << 1,
  C = 1 << 2,
}

public static class EnumFlagsFieldExamples
{
  private static readonly Style InputStyle = new Style { (StyleKeys.PaddingLeft, 4f) };

  // Function component – pass EnumFlagsFieldExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(ExampleFlags.A | ExampleFlags.C);

    void OnChange(ChangeEvent<System.Enum> evt)
    {
      setValue((ExampleFlags)evt.newValue);
    }

    return V.EnumFlagsField(
      new EnumFlagsFieldProps
      {
        EnumType = typeof(ExampleFlags).AssemblyQualifiedName,
        Value = value,
        VisualInput = new Dictionary<string, object>
        {
          { "style", InputStyle },
        },
      }
    );
  }
}`

