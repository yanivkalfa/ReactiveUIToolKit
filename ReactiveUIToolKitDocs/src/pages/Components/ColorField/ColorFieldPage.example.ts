export const COLOR_FIELD_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;

public static class ColorFieldExamples
{
  public static VirtualNode Render(
    System.Collections.Generic.Dictionary<string, object> props,
    System.Collections.Generic.IReadOnlyList<VirtualNode> children
  )
  {
    var (color, setColor) = Hooks.UseState(new Color(0.2f, 0.6f, 0.9f, 1f));

    void OnChange(ChangeEvent<Color> evt)
    {
      setColor.Set(evt.newValue);
    }

    return V.ColorField(
      new ColorFieldProps
      {
        Value = color,
        Label = new LabelProps { Text = "Tint" }.ToDictionary(),
      }
    );
  }
}`

