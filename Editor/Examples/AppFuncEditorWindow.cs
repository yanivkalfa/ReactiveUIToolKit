using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.Core;
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Props.Typed;
using static ReactiveUITK.Props.Typed.StyleKeys;
using UColor = UnityEngine.Color;
using ReactiveUITK.Examples.ClassComponents;
using System.Collections.Generic;
using System;

namespace ReactiveUITK.EditorExamples
{
    public sealed class AppFuncEditorWindow : EditorWindow
    {
        [MenuItem("Window/ReactiveUITK/AppFunc Demo")]
        public static void ShowWindow()
        {
            AppFuncEditorWindow window = GetWindow<AppFuncEditorWindow>("ReactiveUITK AppFunc");
            window.minSize = new Vector2(420, 320);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement hostElement = rootVisualElement;
            hostElement.style.flexGrow = 1f;
            EditorRootRendererUtility.Mount(hostElement, V.Func(ReactiveUITK.Examples.SharedPage.SharedDemoPage.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }

    // Replaced inline component with shared page usage
    internal static class EditorAppFunc { }
}
