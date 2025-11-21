using System;
using System.Collections.Generic;
using System.Reflection;
using ReactiveUITK.Props;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class ToggleButtonGroupElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create() => new ToggleButtonGroup();

        public override VisualElement ResolveChildHost(VisualElement element)
        {
            return element;
        }

        public override void ApplyProperties(VisualElement element, IReadOnlyDictionary<string, object> properties)
        {
            if (element is not ToggleButtonGroup g || properties == null)
            {
                PropsApplier.Apply(element, properties);
                return;
            }
            TryApplyProp<int>(properties, "value", v => SetGroupValue(g, v));
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(VisualElement element, IReadOnlyDictionary<string, object> previous, IReadOnlyDictionary<string, object> next)
        {
            if (element is not ToggleButtonGroup g)
            {
                PropsApplier.ApplyDiff(element, previous, next);
                return;
            }
            previous ??= new Dictionary<string, object>();
            next ??= new Dictionary<string, object>();
            TryDiffProp<int>(previous, next, "value", v => SetGroupValue(g, v));
            PropsApplier.ApplyDiff(element, previous, next);
        }

        private static void SetGroupValue(ToggleButtonGroup g, int index)
        {
            if (g == null)
            {
                return;
            }
            try
            {
                var prop = g.GetType().GetProperty("value", BindingFlags.Public | BindingFlags.Instance);
                if (prop == null)
                {
                    return;
                }
                // If legacy API used int, set directly
                if (prop.PropertyType == typeof(int))
                {
                    prop.SetValue(g, index);
                    return;
                }
                // Try to construct UnityEngine.UIElements.ToggleButtonGroupState(int)
                var stateType = g.GetType().Assembly.GetType("UnityEngine.UIElements.ToggleButtonGroupState");
                if (stateType != null)
                {
                    object state = null;
                    // Prefer ctor(int)
                    var ctor = stateType.GetConstructor(new[] { typeof(int) });
                    if (ctor != null)
                    {
                        state = ctor.Invoke(new object[] { index });
                    }
                    else
                    {
                        // Fallback: parameterless then try set SelectedIndex property
                        state = Activator.CreateInstance(stateType);
                        var si = stateType.GetProperty("selectedIndex", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                        si?.SetValue(state, index);
                    }
                    if (state != null && prop.PropertyType.IsAssignableFrom(stateType))
                    {
                        prop.SetValue(g, state);
                        return;
                    }
                }
            }
            catch { }
        }
    }
}
