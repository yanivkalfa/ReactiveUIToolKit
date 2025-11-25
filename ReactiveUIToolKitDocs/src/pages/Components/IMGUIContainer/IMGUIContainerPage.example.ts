export const IMGUI_CONTAINER_BASIC = `// Example namespace: ReactiveUITK.Samples.Editor

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public static class IMGUIContainerExamples
{
  // Function component – pass IMGUIContainerExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    void OnGUI()
    {
      EditorGUILayout.LabelField("IMGUI content inside UI Toolkit");
    }

    return V.IMGUIContainer(
      new IMGUIContainerProps
      {
        OnGUI = OnGUI,
      }
    );
  }
}`

