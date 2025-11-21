export const TEXT_FIELD_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

public static class TextFieldExamples
{
  public static VirtualNode Render(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState("Hello");

    void OnChange(ChangeEvent<string> evt)
    {
      setValue.Set(evt.newValue);
    }

    var inputProps = new Dictionary<string, object>
    {
      { "style", new Style { (StyleKeys.PaddingLeft, 4f) } },
    };

    return V.TextField(
      new TextFieldProps
      {
        Value = value,
        Placeholder = "Type here...",
        Input = inputProps,
      }
    );
  }
}`

