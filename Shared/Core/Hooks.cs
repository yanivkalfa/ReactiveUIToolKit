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
        public static Func<T> UseStableFunc<T>(Func<T> function)
        {
            NodeMetadata metadata = HookContext.Current;
            if (metadata == null)
            {
                return function;
            }
            metadata.HookStates ??= new List<object>();
            if (metadata.HookIndex >= metadata.HookStates.Count)
            {
                metadata.HookStates.Add(function);
            }
            Func<T> stored = (Func<T>)metadata.HookStates[metadata.HookIndex];
            metadata.HookIndex++;
            metadata.HookStates[metadata.HookIndex - 1] = function;
            return stored;
        }
        public static Action<T> UseStableAction<T>(Action<T> action)
        {
            NodeMetadata metadata = HookContext.Current;
            if (metadata == null)
            {
                return action;
            }
            metadata.HookStates ??= new List<object>();
            if (metadata.HookIndex >= metadata.HookStates.Count)
            {
                metadata.HookStates.Add(action);
            }
            Action<T> stored = (Action<T>)metadata.HookStates[metadata.HookIndex];
            metadata.HookIndex++;
            metadata.HookStates[metadata.HookIndex - 1] = action;
            return stored;
        }
        public static Action UseStableCallback(Action callback)
        {
            NodeMetadata metadata = HookContext.Current;
            if (metadata == null)
            {
                return callback;
            }
            metadata.HookStates ??= new List<object>();
            if (metadata.HookIndex >= metadata.HookStates.Count)
            {
                metadata.HookStates.Add(callback);
            }
            Action stored = (Action)metadata.HookStates[metadata.HookIndex];
            metadata.HookIndex++;
            metadata.HookStates[metadata.HookIndex - 1] = callback;
            return stored;
        }

        public static void UseLayoutEffect(Func<Action> effectFactory, params object[] dependencies)
        {
            NodeMetadata metadata = HookContext.Current;
            if (metadata == null)
            {
                return;
            }
            metadata.FunctionLayoutEffects ??= new List<(Func<Action>, object[], object[], Action)>();
            if (metadata.HookIndex >= metadata.FunctionLayoutEffects.Count)
            {
                metadata.FunctionLayoutEffects.Add((effectFactory, dependencies, null, null));
            }
            else
            {
                var entry = metadata.FunctionLayoutEffects[metadata.HookIndex];
                entry.factory = effectFactory;
                entry.deps = dependencies;
                metadata.FunctionLayoutEffects[metadata.HookIndex] = entry;
            }
            metadata.HookIndex++;
        }
        public static (T value, Action<T> set) UseState<T>(T initial = default)
        {
            NodeMetadata metadata = HookContext.Current;
            if (metadata == null)
            {
                return (initial, _ => { });
            }
            metadata.HookStates ??= new List<object>();
            if (metadata.HookIndex >= metadata.HookStates.Count)
            {
                metadata.HookStates.Add(initial);
            }
            T currentValue = (T)metadata.HookStates[metadata.HookIndex];
            int capturedIndex = metadata.HookIndex;
            metadata.HookIndex++;
            void Setter(T newValue)
            {
                metadata.HookStates[capturedIndex] = newValue;
                if (metadata.Reconciler != null)
                {
                    // Avoid re-entrancy: if rendering, defer one update
                    if (metadata.IsRendering)
                    {
                        metadata.PendingUpdate = true;
                        return;
                    }
                    // Prefer scheduling via host scheduler to avoid re-entrancy during render loop
                    ReactiveUITK.Core.IScheduler scheduler = null;
                    if (metadata.HostContext != null && metadata.HostContext.Environment != null && metadata.HostContext.Environment.TryGetValue("scheduler", out var sObj))
                    {
                        scheduler = sObj as ReactiveUITK.Core.IScheduler;
                    }
                    if (scheduler != null)
                    {
                        if (ReactiveUITK.Core.Reconciler.TraceLevel == ReactiveUITK.Core.Reconciler.DiffTraceLevel.Verbose)
                        {
                            try { UnityEngine.Debug.Log("[HookSet:state] key=" + metadata.Key + ", renderPending=" + metadata.IsRendering); } catch { }
                        }
                        scheduler.Enqueue(() => { metadata.HookIndex = 0; metadata.Reconciler.ForceFunctionComponentUpdate(metadata); });
                    }
                    else
                    {
                        metadata.HookIndex = 0;
                        metadata.Reconciler.ForceFunctionComponentUpdate(metadata);
                    }
                }
            }
            return (currentValue, Setter);
        }

        public static (TState state, Action<TAction> dispatch) UseReducer<TState, TAction>(Func<TState, TAction, TState> reducer, TState initialState)
        {
            NodeMetadata metadata = HookContext.Current;
            if (metadata == null)
            {
                return (initialState, _ => { });
            }
            metadata.HookStates ??= new List<object>();
            if (metadata.HookIndex >= metadata.HookStates.Count)
            {
                metadata.HookStates.Add(initialState);
            }
            int index = metadata.HookIndex;
            TState currentState = (TState)metadata.HookStates[index];
            metadata.HookIndex++;
            void Dispatch(TAction action)
            {
                TState previous = (TState)metadata.HookStates[index];
                TState next;
                try
                {
                    next = reducer(previous, action);
                }
                catch
                {
                    next = previous;
                }
                if (!Equals(previous, next))
                {
                    metadata.HookStates[index] = next;
                    if (metadata.Reconciler != null)
                    {
                        if (metadata.IsRendering)
                        {
                            metadata.PendingUpdate = true;
                            return;
                        }
                        ReactiveUITK.Core.IScheduler scheduler = null;
                        if (metadata.HostContext != null && metadata.HostContext.Environment != null && metadata.HostContext.Environment.TryGetValue("scheduler", out var sObj))
                        {
                            scheduler = sObj as ReactiveUITK.Core.IScheduler;
                        }
                        if (scheduler != null)
                        {
                            if (ReactiveUITK.Core.Reconciler.TraceLevel == ReactiveUITK.Core.Reconciler.DiffTraceLevel.Verbose)
                            {
                                try { UnityEngine.Debug.Log("[HookSet:reducer] key=" + metadata.Key + ", renderPending=" + metadata.IsRendering); } catch { }
                            }
                            scheduler.Enqueue(() => { metadata.HookIndex = 0; metadata.Reconciler.ForceFunctionComponentUpdate(metadata); });
                        }
                        else
                        {
                            metadata.HookIndex = 0;
                            metadata.Reconciler.ForceFunctionComponentUpdate(metadata);
                        }
                    }
                }
            }
            return (currentState, Dispatch);
        }

        public static T UseMemo<T>(Func<T> factory, params object[] dependencies)
        {
            NodeMetadata metadata = HookContext.Current;
            if (metadata == null)
            {
                return factory();
            }
            metadata.HookStates ??= new List<object>();
            if (metadata.HookIndex >= metadata.HookStates.Count)
            {
                metadata.HookStates.Add((factory(), dependencies));
            }
            // Guard against transient mismatch
            if (metadata.HookIndex >= metadata.HookStates.Count)
            {
                metadata.HookStates.Add((factory(), dependencies));
            }
            var tuple = ((T value, object[] d))metadata.HookStates[metadata.HookIndex];
            bool changed = DepsChanged(tuple.d, dependencies);
            if (changed)
            {
                tuple = (factory(), dependencies);
                metadata.HookStates[metadata.HookIndex] = tuple;
            }
            metadata.HookIndex++;
            return tuple.value;
        }

        public static T UseDeferredValue<T>(T value, params object[] dependencies)
        {
            NodeMetadata metadata = HookContext.Current;
            if (metadata == null)
            {
                return value;
            }
            metadata.HookStates ??= new List<object>();
            if (metadata.HookIndex >= metadata.HookStates.Count)
            {
                metadata.HookStates.Add((value, dependencies));
            }
            var tuple = ((T val, object[] d))metadata.HookStates[metadata.HookIndex];
            bool changed = DepsChanged(tuple.d, dependencies);
            if (changed && !Equals(tuple.val, value))
            {
                T newVal = value;
                int index = metadata.HookIndex;
                ReactiveUITK.Core.IScheduler scheduler = null;
                if (metadata.HostContext != null && metadata.HostContext.Environment != null && metadata.HostContext.Environment.TryGetValue("scheduler", out var sObj))
                {
                    scheduler = sObj as ReactiveUITK.Core.IScheduler;
                }
                if (scheduler != null)
                {
                    scheduler.EnqueueBatchedEffect(() => { metadata.HookStates[index] = (newVal, dependencies); });
                }
                else
                {
                    metadata.HookStates[index] = (newVal, dependencies);
                }
            }
            metadata.HookIndex++;
            var latest = ((T val, object[] d))metadata.HookStates[metadata.HookIndex - 1];
            return latest.val;
        }

        public static Func<T> UseCallback<T>(Func<T> callback, params object[] dependencies)
        {
            NodeMetadata metadata = HookContext.Current;
            if (metadata == null)
            {
                return callback;
            }
            metadata.HookStates ??= new List<object>();
            if (metadata.HookIndex >= metadata.HookStates.Count)
            {
                metadata.HookStates.Add((callback, dependencies));
            }
            var tuple = ((Func<T> cb, object[] d))metadata.HookStates[metadata.HookIndex];
            bool changed = DepsChanged(tuple.d, dependencies);
            if (changed)
            {
                tuple = (callback, dependencies);
                metadata.HookStates[metadata.HookIndex] = tuple;
            }
            metadata.HookIndex++;
            return tuple.cb;
        }

        public static THandle UseImperativeHandle<THandle>(Func<THandle> factory, params object[] dependencies) where THandle : class
        {
            NodeMetadata metadata = HookContext.Current;
            if (metadata == null)
            {
                return factory();
            }
            metadata.HookStates ??= new List<object>();
            if (metadata.HookIndex >= metadata.HookStates.Count)
            {
                metadata.HookStates.Add((factory(), dependencies));
            }
            var tuple = ((THandle handle, object[] d))metadata.HookStates[metadata.HookIndex];
            if (DepsChanged(tuple.d, dependencies))
            {
                tuple = (factory(), dependencies);
                metadata.HookStates[metadata.HookIndex] = tuple;
            }
            metadata.HookIndex++;
            return tuple.handle;
        }

        public static VisualElement UseRef()
        {
            NodeMetadata metadata = HookContext.Current;
            return metadata?.Container;
        }

        public static void UseEffect(Func<Action> effectFactory, params object[] dependencies)
        {
            NodeMetadata metadata = HookContext.Current;
            if (metadata == null)
            {
                return;
            }
            metadata.FunctionEffects ??= new List<(Func<Action>, object[], object[], Action)>();
            if (metadata.HookIndex >= metadata.FunctionEffects.Count)
            {
                metadata.FunctionEffects.Add((effectFactory, dependencies, null, null));
            }
            else
            {
                var entry = metadata.FunctionEffects[metadata.HookIndex];
                entry.factory = effectFactory;
                entry.deps = dependencies;
                metadata.FunctionEffects[metadata.HookIndex] = entry;
            }
            metadata.HookIndex++;
        }

        public static T UseContext<T>(string key)
        {
            NodeMetadata metadata = HookContext.Current;
            if (metadata == null || metadata.HostContext == null)
            {
                return default;
            }
            metadata.SubscribedContextKeys ??= new HashSet<string>();
            metadata.SubscribedContextKeys.Add(key);
            object resolved = metadata.HostContext.ResolveContext(key);
            if (resolved is T typed)
            {
                return typed;
            }
            return default;
        }

        private static bool DepsChanged(object[] previousDependencies, object[] nextDependencies)
        {
            if (previousDependencies == null || nextDependencies == null)
            {
                return true;
            }
            if (previousDependencies.Length != nextDependencies.Length)
            {
                return true;
            }
            for (int i = 0; i < previousDependencies.Length; i++)
            {
                if (!Equals(previousDependencies[i], nextDependencies[i]))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
