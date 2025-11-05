using System;

namespace ReactiveUITK.Core
{
    public static class StateSetterExtensions
    {
        public static T Set<T>(this Hooks.StateSetter<T> setter, T value)
        {
            if (setter == null)
            {
                return value;
            }
            return setter(value);
        }

        public static T Set<T>(this Hooks.StateSetter<T> setter, Func<T, T> updater)
        {
            if (setter == null)
            {
                return default;
            }
            if (updater == null)
            {
                return default;
            }
            return setter(updater);
        }

        public static Action<T> ToValueAction<T>(this Hooks.StateSetter<T> setter)
        {
            if (setter == null)
            {
                return null;
            }
            return value => setter(value);
        }
    }
}
