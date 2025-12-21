using System;

namespace ReactiveUITK.Core
{
    public static class StateSetterExtensions
    {
        public static T Set<T>(this Hooks.StateSetter<T> setter, Hooks.StateUpdate<T> update)
        {
            UnityEngine.Debug.Log($"[StateSetterExtensions] Set<{typeof(T).Name}> called. Value={update.Value}");
            if (setter == null)
            {
                UnityEngine.Debug.LogWarning("[StateSetterExtensions] Setter is NULL!");
                return update.UsesUpdater ? default : update.Value;
            }
            return setter(update);
        }

        public static T Set<T>(this Hooks.StateSetter<T> setter, Func<T, T> updater)
        {
            if (updater == null)
            {
                return default;
            }
            return setter == null ? default : setter(updater);
        }

        public static Action<T> ToValueAction<T>(this Hooks.StateSetter<T> setter) =>
            setter == null ? null : (v => setter(v));
    }
}
