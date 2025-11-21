export const ENUM_FLAGS_FIELD_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;

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
  public static VirtualNode Render(
    System.Collections.Generic.Dictionary<string, object> props,
    System.Collections.Generic.IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState(ExampleFlags.A | ExampleFlags.C);

    void OnChange(ChangeEvent<System.Enum> evt)
    {
      setValue.Set((ExampleFlags)evt.newValue);
    }

    return V.EnumFlagsField(
      new EnumFlagsFieldProps
      {
        EnumType = typeof(ExampleFlags).AssemblyQualifiedName,
        Value = value,
      }
    );
  }
}`

