using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements.Pools
{
    /// <summary>
    /// A simple global pool keyed by concrete VisualElement type.
    /// Reuses instances to reduce allocations/GC when reconciling churny UI.
    /// </summary>
    internal static class GlobalVisualElementPool
    {
        private static readonly Dictionary<Type, Stack<VisualElement>> _pools = new();

        public static T Get<T>()
            where T : VisualElement, new()
        {
            var t = typeof(T);
            if (_pools.TryGetValue(t, out var stack) && stack.Count > 0)
            {
                var ve = (T)stack.Pop();
                return ve;
            }
            return new T();
        }

        public static void Release(VisualElement ve)
        {
            if (ve == null)
                return;

            // Minimal hard reset to avoid state bleed
            try
            {
                ve.Clear();
            }
            catch { }
            try
            {
                ve.ClearClassList();
            }
            catch { }
            try
            {
                ve.tooltip = null;
            }
            catch { }
            try
            {
                ve.userData = null;
            }
            catch { }
            try
            {
                ve.pickingMode = PickingMode.Position;
            }
            catch { }
            try
            {
                ve.name = null;
            }
            catch { }

            var t = ve.GetType();
            if (!_pools.TryGetValue(t, out var stack))
            {
                stack = new Stack<VisualElement>(64);
                _pools[t] = stack;
            }
            stack.Push(ve);
        }
    }
}
