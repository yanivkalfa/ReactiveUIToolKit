export const OBJECT_FIELD_BASIC = `// Example namespace: ReactiveUITK.Samples.Editor

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;

public static class ObjectFieldExamples
{
  public static VirtualNode Render(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (value, setValue) = Hooks.UseState<Object>(null);

    void OnChange(ChangeEvent<Object> evt)
    {
      setValue.Set(evt.newValue);
    }

    return V.ObjectField(
      new ObjectFieldProps
      {
        ObjectType = typeof(Texture2D).AssemblyQualifiedName,
        AllowSceneObjects = false,
        Value = value,
        Label = new LabelProps { Text = "Texture" }.ToDictionary(),
      }
    );
  }
}`

