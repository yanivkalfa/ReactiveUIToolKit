#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ReactiveUITK.Elements;
using ReactiveUITK.Core;

public static class RegistryDebug
{
    [MenuItem("ReactiveUITK/Debug/Check Registry")]
    public static void CheckRegistry()
    {
        var registry = ElementRegistryProvider.GetDefaultRegistry();
        Debug.Log($"[RegistryDebug] Registry instance: {registry}");

        string[] typesToCheck = new[] { "Button", "Label", "VisualElement" };
        foreach (var type in typesToCheck)
        {
            var adapter = registry.Resolve(type);
            Debug.Log($"[RegistryDebug] Resolve('{type}') -> {adapter?.GetType().Name ?? "NULL"}");
        }
    }
}
#endif
