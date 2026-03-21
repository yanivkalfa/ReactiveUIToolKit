export const PROPERTY_INSPECTOR_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

// Editor-only usage
public static class PropertyInspectorExamples
{
  private static readonly Style InspectorBoxStyle = new Style
  {
    (StyleKeys.FlexDirection, FlexDirection.Row),
    (StyleKeys.Gap, 4f),
  };

  // Function component – pass PropertyInspectorExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (target, setTarget) = Hooks.UseState<Object>(null);

    return V.Box(
      new BoxProps
      {
        Style = InspectorBoxStyle,
      },
      V.PropertyField(
        new PropertyFieldProps
        {
          Target = target,
          BindingPath = "m_Name",
          Label = "Name",
        }
      ),
      V.InspectorElement(
        new InspectorElementProps
        {
          Target = target,
        }
      )
    );
  }
}`
