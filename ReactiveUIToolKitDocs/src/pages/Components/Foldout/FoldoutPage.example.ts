export const FOLDOUT_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

public static class FoldoutExamples
{
  public static VirtualNode Render(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (isOpen, setIsOpen) = Hooks.UseState(true);

    void OnChange(ChangeEvent<bool> evt)
    {
      setIsOpen.Set(evt.newValue);
    }

    var headerProps = new Dictionary<string, object>
    {
      { "style", new Style { (StyleKeys.FontSize, 14f) } },
    };

    var contentContainerProps = new Dictionary<string, object>
    {
      { "style", new Style { (StyleKeys.PaddingLeft, 12f) } },
    };

    return V.Foldout(
      new FoldoutProps
      {
        Text = "Foldout title",
        Value = isOpen,
        OnChange = OnChange,
        Header = headerProps,
        ContentContainer = contentContainerProps,
      },
      key: null,
      V.Label(new LabelProps { Text = "Child 1" }),
      V.Label(new LabelProps { Text = "Child 2" })
    );
  }
}`

