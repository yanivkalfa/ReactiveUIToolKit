using System;
using System.Collections.Generic;
using System.Linq;
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

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (element is not ToggleButtonGroup g || properties == null)
            {
                PropsApplier.Apply(element, properties);
                return;
            }
            TryApplyProp<int>(properties, "value", v => ScheduleSetGroupValue(g, v));
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is not ToggleButtonGroup g)
            {
                PropsApplier.ApplyDiff(element, previous, next);
                return;
            }
            previous ??= s_emptyProps;
            next ??= s_emptyProps;
            TryDiffProp<int>(previous, next, "value", v => ScheduleSetGroupValue(g, v));
            PropsApplier.ApplyDiff(element, previous, next);
        }

        private static readonly Type StateType = typeof(ToggleButtonGroup).Assembly.GetType(
            "UnityEngine.UIElements.ToggleButtonGroupState"
        );
        private static readonly MethodInfo CreateFromOptionsMethod = StateType?.GetMethod(
            "CreateFromOptions",
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: new[] { typeof(IEnumerable<bool>) },
            modifiers: null
        );

        private static void ScheduleSetGroupValue(ToggleButtonGroup g, int index)
        {
            if (g == null)
            {
                return;
            }
            if (TrySetGroupValue(g, index))
            {
                return;
            }
            g.schedule.Execute(() => TrySetGroupValue(g, index)).StartingIn(0);
        }

        private static bool TrySetGroupValue(ToggleButtonGroup g, int index)
        {
            if (g == null)
            {
                return false;
            }
            var buttons = g.Query<Button>().ToList();
            if (buttons.Count == 0)
            {
                return false;
            }
            try
            {
                var prop = g.GetType()
                    .GetProperty("value", BindingFlags.Public | BindingFlags.Instance);
                if (prop == null)
                {
                    return false;
                }
                if (prop.PropertyType == typeof(int))
                {
                    prop.SetValue(g, index);
                    return true;
                }
                if (StateType != null && CreateFromOptionsMethod != null)
                {
                    var selected = new bool[buttons.Count];
                    if (index >= 0 && index < buttons.Count)
                    {
                        selected[index] = true;
                    }
                    var state = CreateFromOptionsMethod.Invoke(null, new object[] { selected });
                    if (state != null && prop.PropertyType.IsAssignableFrom(StateType))
                    {
                        prop.SetValue(g, state);
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }
    }
}
