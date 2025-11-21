export const UNSIGNED_LONG_FIELD_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

public static class UnsignedLongFieldExamples
{
  private static readonly Style InputStyle = new Style { (StyleKeys.PaddingLeft, 4f) };

  public static VirtualNode Render(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState<ulong>(0ul);

    void OnChange(ChangeEvent<ulong> evt)
    {
      setValue.Set(evt.newValue);
    }

    return V.UnsignedLongField(
      new UnsignedLongFieldProps
      {
        Value = value,
        OnChange = OnChange,
        Label = new LabelProps { Text = "Unsigned Long" }.ToDictionary(),
        VisualInput = new Dictionary<string, object>
        {
          { "style", InputStyle },
        },
      }
    );
  }
}`
