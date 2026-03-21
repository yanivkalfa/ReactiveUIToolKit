export const FOLDOUT_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

public static class FoldoutExamples
{
  private static readonly Style HeaderStyle = new Style { (StyleKeys.FontSize, 14f) };

  private static readonly Style ContentContainerStyle = new Style { (StyleKeys.PaddingLeft, 12f) };

  // Function component – pass FoldoutExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (isOpen, setIsOpen) = Hooks.UseState(true);

    void OnChange(ChangeEvent<bool> evt)
    {
      setIsOpen(evt.newValue);
    }

    var headerProps = new Dictionary<string, object>
    {
      { "style", HeaderStyle },
    };

    var contentContainerProps = new Dictionary<string, object>
    {
      { "style", ContentContainerStyle },
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
