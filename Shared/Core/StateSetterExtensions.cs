using System;

namespace ReactiveUITK.Core
{
    /// <summary>
    /// Extension surface for <see cref="Hooks.StateSetter{T}"/> enabling ergonomic functional updates despite C# lambda conversion limits.
    /// Keep BOTH value and functional Set overloads: value form allows <c>setX.Set(newVal)</c> parity with existing usages; functional form allows <c>setX.Set(prev => next)</c>.
    /// Direct lambda invocation <c>setX(prev => next)</c> is impossible while the delegate parameter type is a struct (StateUpdate{T}). This is the finalized design.
    /// </summary>
    public static class StateSetterExtensions
    {
        // Unified Set: accepts either direct value or functional updater via implicit conversion to StateUpdate<T>.
        public static T Set<T>(this Hooks.StateSetter<T> setter, Hooks.StateUpdate<T> update)
        {
            if (setter == null)
            {
                return update.UsesUpdater ? default : update.Value;
            }
            return setter(update);
        }

        // Functional updater overload. Lambdas only convert to delegate or expression tree types, not arbitrary structs;
        // this preserves setX.Set(x => x + 1).
        public static T Set<T>(this Hooks.StateSetter<T> setter, Func<T, T> updater)
        {
            if (updater == null)
            {
                return default;
            }
            return setter == null ? default : setter(updater); // implicit operator from Func<T,T> to StateUpdate<T> applies here
        }

        // Convenience wrapper for producing an Action<T> (e.g. passing to UI events expecting a direct value setter)
        public static Action<T> ToValueAction<T>(this Hooks.StateSetter<T> setter) =>
            setter == null ? null : (v => setter(v));
    }
}
