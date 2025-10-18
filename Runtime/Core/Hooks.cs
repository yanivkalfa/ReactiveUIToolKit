using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Core
{
    internal static class HookContext
    {
        [ThreadStatic]
        public static NodeMetadata Current;
    }
    public static class Hooks
    {
        public static Func<T> UseStableFunc<T>(Func<T> fn)
        {
            var meta = HookContext.Current;
            if (meta == null) return fn;
            if (meta.HookStates == null) meta.HookStates = new List<object>();
            if (meta.HookIndex >= meta.HookStates.Count)
            {
                meta.HookStates.Add(fn);
            }
            var stored = (Func<T>)meta.HookStates[meta.HookIndex];
            meta.HookIndex++;
            meta.HookStates[meta.HookIndex - 1] = fn; // update with latest closure
            return stored;
        }
        // Stable Action<T> (captures latest closure but preserves identity)
        public static Action<T> UseStableAction<T>(Action<T> action)
        {
            var meta = HookContext.Current;
            if (meta == null) return action;
            meta.HookStates ??= new List<object>();
            if (meta.HookIndex >= meta.HookStates.Count) meta.HookStates.Add(action);
            var stored = (Action<T>)meta.HookStates[meta.HookIndex];
            meta.HookIndex++;
            meta.HookStates[meta.HookIndex - 1] = action; // refresh closure
            return stored;
        }
        public static Action UseStableCallback(Action callback)
        {
            var meta = HookContext.Current;
            if (meta == null) return callback;
            if (meta.HookStates == null) meta.HookStates = new List<object>();
            if (meta.HookIndex >= meta.HookStates.Count) meta.HookStates.Add(callback);
            var stored = (Action)meta.HookStates[meta.HookIndex];
            meta.HookIndex++;
            // Update reference each render to latest closure
            meta.HookStates[meta.HookIndex - 1] = callback;
            return stored;
        }

        public static void UseLayoutEffect(Func<Action> effectFactory, params object[] deps)
        {
            var meta = HookContext.Current;
            if (meta == null) return;
            if (meta.FunctionLayoutEffects == null) meta.FunctionLayoutEffects = new List<(Func<Action>, object[], object[], Action)>();
            if (meta.HookIndex >= meta.FunctionLayoutEffects.Count)
            {
                meta.FunctionLayoutEffects.Add((effectFactory, deps, null, null));
            }
            else
            {
                var entry = meta.FunctionLayoutEffects[meta.HookIndex];
                entry.factory = effectFactory;
                entry.deps = deps;
                meta.FunctionLayoutEffects[meta.HookIndex] = entry;
            }
            meta.HookIndex++;
        }
        public static (T value, Action<T> set) UseState<T>(T initial = default)
        {
            var meta = HookContext.Current;
            if (meta == null) return (initial, _ => { });
            if (meta.HookStates == null) meta.HookStates = new List<object>();
            if (meta.HookIndex >= meta.HookStates.Count)
            {
                meta.HookStates.Add(initial);
            }
            T current = (T)meta.HookStates[meta.HookIndex];
            int capturedIndex = meta.HookIndex;
            meta.HookIndex++;
            void Setter(T newValue)
            {
                meta.HookStates[capturedIndex] = newValue;
                // Re-render function component
                if (meta.Reconciler != null)
                {
                    meta.HookIndex = 0; // reset for next render
                    meta.Reconciler.ForceFunctionComponentUpdate(meta);
                }
            }
            return (current, Setter);
        }

        // Reducer hook similar to React's useReducer
        public static (TState state, Action<TAction> dispatch) UseReducer<TState, TAction>(Func<TState, TAction, TState> reducer, TState initial)
        {
            var meta = HookContext.Current;
            if (meta == null) return (initial, _ => { });
            meta.HookStates ??= new List<object>();
            if (meta.HookIndex >= meta.HookStates.Count) meta.HookStates.Add(initial);
            int index = meta.HookIndex;
            TState current = (TState)meta.HookStates[index];
            meta.HookIndex++;
            void Dispatch(TAction action)
            {
                var prev = (TState)meta.HookStates[index];
                TState next;
                try { next = reducer(prev, action); } catch { next = prev; }
                if (!Equals(prev, next))
                {
                    meta.HookStates[index] = next;
                    if (meta.Reconciler != null)
                    {
                        meta.HookIndex = 0;
                        meta.Reconciler.ForceFunctionComponentUpdate(meta);
                    }
                }
            }
            return (current, Dispatch);
        }

        public static T UseMemo<T>(Func<T> factory, params object[] deps)
        {
            var meta = HookContext.Current;
            if (meta == null) return factory();
            if (meta.HookStates == null) meta.HookStates = new List<object>();
            if (meta.HookIndex >= meta.HookStates.Count)
            {
                meta.HookStates.Add((factory(), deps));
            }
            var tuple = ((T value, object[] d))meta.HookStates[meta.HookIndex];
            bool changed = DepsChanged(tuple.d, deps);
            if (changed)
            {
                tuple = (factory(), deps);
                meta.HookStates[meta.HookIndex] = tuple;
            }
            meta.HookIndex++;
            return tuple.value;
        }

        // Deferred value: updates only when deps change and after one passive effect tick to avoid jank
        public static T UseDeferredValue<T>(T value, params object[] deps)
        {
            var meta = HookContext.Current;
            if (meta == null) return value;
            meta.HookStates ??= new List<object>();
            if (meta.HookIndex >= meta.HookStates.Count)
            {
                meta.HookStates.Add((value, deps));
            }
            var tuple = ((T val, object[] d))meta.HookStates[meta.HookIndex];
            bool changed = DepsChanged(tuple.d, deps);
            if (changed && !Equals(tuple.val, value))
            {
                // schedule update after passive effects batch
                T newVal = value;
                int index = meta.HookIndex;
                RenderScheduler.Instance.EnqueueBatchedEffect(() =>
                {
                    meta.HookStates[index] = (newVal, deps);
                });
            }
            meta.HookIndex++;
            var latest = ((T val, object[] d))meta.HookStates[meta.HookIndex - 1];
            return latest.val;
        }

        public static Func<T> UseCallback<T>(Func<T> callback, params object[] deps)
        {
            var meta = HookContext.Current;
            if (meta == null) return callback;
            if (meta.HookStates == null) meta.HookStates = new List<object>();
            if (meta.HookIndex >= meta.HookStates.Count)
            {
                meta.HookStates.Add((callback, deps));
            }
            var tuple = ((Func<T> cb, object[] d))meta.HookStates[meta.HookIndex];
            bool changed = DepsChanged(tuple.d, deps);
            if (changed)
            {
                tuple = (callback, deps);
                meta.HookStates[meta.HookIndex] = tuple;
            }
            meta.HookIndex++;
            return tuple.cb;
        }

        // Imperative handle: store a handle object (e.g., API) and return stable reference
        public static THandle UseImperativeHandle<THandle>(Func<THandle> factory, params object[] deps) where THandle : class
        {
            var meta = HookContext.Current;
            if (meta == null) return factory();
            meta.HookStates ??= new List<object>();
            if (meta.HookIndex >= meta.HookStates.Count)
            {
                meta.HookStates.Add((factory(), deps));
            }
            var tuple = ((THandle handle, object[] d))meta.HookStates[meta.HookIndex];
            if (DepsChanged(tuple.d, deps))
            {
                tuple = (factory(), deps);
                meta.HookStates[meta.HookIndex] = tuple;
            }
            meta.HookIndex++;
            return tuple.handle;
        }

        public static VisualElement UseRef()
        {
            var meta = HookContext.Current;
            return meta?.Container;
        }

        public static void UseEffect(Func<Action> effectFactory, params object[] deps)
        {
            var meta = HookContext.Current;
            if (meta == null) return;
            if (meta.FunctionEffects == null) meta.FunctionEffects = new List<(Func<Action>, object[], object[], Action)>();
            // Match existing slot by HookIndex to preserve call order semantics
            if (meta.HookIndex >= meta.FunctionEffects.Count)
            {
                meta.FunctionEffects.Add((effectFactory, deps, null, null));
            }
            else
            {
                var entry = meta.FunctionEffects[meta.HookIndex];
                entry.factory = effectFactory;
                entry.deps = deps;
                meta.FunctionEffects[meta.HookIndex] = entry;
            }
            meta.HookIndex++;
        }

        public static T UseContext<T>(string key)
        {
            var meta = HookContext.Current;
            if (meta == null || meta.HostContext == null) return default;
            meta.SubscribedContextKeys ??= new HashSet<string>();
            meta.SubscribedContextKeys.Add(key);
            var resolved = meta.HostContext.ResolveContext(key);
            if (resolved is T typed) return typed;
            return default;
        }

        private static bool DepsChanged(object[] oldDeps, object[] newDeps)
        {
            if (oldDeps == null || newDeps == null) return true;
            if (oldDeps.Length != newDeps.Length) return true;
            for (int i = 0; i < oldDeps.Length; i++)
            {
                if (!Equals(oldDeps[i], newDeps[i])) return true;
            }
            return false;
        }
    }
}
