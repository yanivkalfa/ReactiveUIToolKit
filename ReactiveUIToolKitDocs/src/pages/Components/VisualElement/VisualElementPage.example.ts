export const VISUAL_ELEMENT_SIGNATURE = `public static VirtualNode VisualElement(
  Style style,
  string key = null,
  params VirtualNode[] children
);

public static VirtualNode VisualElement(
  IReadOnlyDictionary<string, object> elementProperties = null,
  string key = null,
  params VirtualNode[] children
);`

export const VISUAL_ELEMENT_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;
using UnityEngine;

public static class VisualElementExamples
{
  private static readonly Style ContainerStyle = new Style
  {
    (StyleKeys.FlexDirection, FlexDirection.Column),
    (StyleKeys.PaddingLeft, 8f),
    (StyleKeys.PaddingTop, 4f),
    (StyleKeys.Gap, 4f),
  };

  // Function component – pass VisualElementExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    return V.VisualElement(
      ContainerStyle,
      null,
      V.Label(new LabelProps { Text = "VisualElement container" }),
      V.Button(new ButtonProps { Text = "Click me" })
    );
  }
}`
