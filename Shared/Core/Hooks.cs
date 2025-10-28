using System;
using System.Collections.Generic;
using ReactiveUITK.Core.Util;
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
        public static SafeAreaInsets UseSafeArea(float tolerance = 0.5f)
        {
            // Lightweight read; no re-render trigger. Caller can combine with other state to refresh on orientation/resize.
            var current = SafeAreaUtility.GetInsets();
            NodeMetadata metadata = HookContext.Current;
            if (metadata == null)
            {
                return current;
            }
            metadata.HookStates ??= new System.Collections.Generic.List<object>();
            if (metadata.HookIndex >= metadata.HookStates.Count)
            {
                metadata.HookStates.Add(current);
            }
            // Keep last value for potential future change detection
            metadata.HookStates[metadata.HookIndex] = current;
            metadata.HookIndex++;
            return current;
        }

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
            metadata.FunctionLayoutEffects ??=
                new List<(Func<Action>, object[], object[], Action)>();
            int index = metadata.LayoutEffectIndex;
            if (index >= metadata.FunctionLayoutEffects.Count)
            {
                metadata.FunctionLayoutEffects.Add((effectFactory, dependencies, null, null));
            }
            else
            {
                var entry = metadata.FunctionLayoutEffects[index];
                entry.factory = effectFactory;
                entry.deps = dependencies;
                metadata.FunctionLayoutEffects[index] = entry;
            }
            metadata.LayoutEffectIndex++;
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
                    if (
                        metadata.HostContext != null
                        && metadata.HostContext.Environment != null
                        && metadata.HostContext.Environment.TryGetValue("scheduler", out var sObj)
                    )
                    {
                        scheduler = sObj as ReactiveUITK.Core.IScheduler;
                    }
                    if (scheduler != null)
                    {
                        if (
                            ReactiveUITK.Core.Reconciler.TraceLevel
                            == ReactiveUITK.Core.Reconciler.DiffTraceLevel.Verbose
                        )
                        {
                            try
                            {
                                UnityEngine.Debug.Log(
                                    "[HookSet:state] key="
                                        + metadata.Key
                                        + ", renderPending="
                                        + metadata.IsRendering
                                );
                            }
                            catch { }
                        }
                        scheduler.Enqueue(() =>
                        {
                            metadata.HookIndex = 0;
                            metadata.Reconciler.ForceFunctionComponentUpdate(metadata);
                        });
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

        public static (TState state, Action<TAction> dispatch) UseReducer<TState, TAction>(
            Func<TState, TAction, TState> reducer,
            TState initialState
        )
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
                        if (
                            metadata.HostContext != null
                            && metadata.HostContext.Environment != null
                            && metadata.HostContext.Environment.TryGetValue(
                                "scheduler",
                                out var sObj
                            )
                        )
                        {
                            scheduler = sObj as ReactiveUITK.Core.IScheduler;
                        }
                        if (scheduler != null)
                        {
                            if (
                                ReactiveUITK.Core.Reconciler.TraceLevel
                                == ReactiveUITK.Core.Reconciler.DiffTraceLevel.Verbose
                            )
                            {
                                try
                                {
                                    UnityEngine.Debug.Log(
                                        "[HookSet:reducer] key="
                                            + metadata.Key
                                            + ", renderPending="
                                            + metadata.IsRendering
                                    );
                                }
                                catch { }
                            }
                            scheduler.Enqueue(() =>
                            {
                                metadata.HookIndex = 0;
                                metadata.Reconciler.ForceFunctionComponentUpdate(metadata);
                            });
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
                if (
                    metadata.HostContext != null
                    && metadata.HostContext.Environment != null
                    && metadata.HostContext.Environment.TryGetValue("scheduler", out var sObj)
                )
                {
                    scheduler = sObj as ReactiveUITK.Core.IScheduler;
                }
                if (scheduler != null)
                {
                    scheduler.EnqueueBatchedEffect(() =>
                    {
                        metadata.HookStates[index] = (newVal, dependencies);
                    });
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

        public static THandle UseImperativeHandle<THandle>(
            Func<THandle> factory,
            params object[] dependencies
        )
            where THandle : class
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
            int index = metadata.EffectIndex;
            if (index >= metadata.FunctionEffects.Count)
            {
                metadata.FunctionEffects.Add((effectFactory, dependencies, null, null));
            }
            else
            {
                var entry = metadata.FunctionEffects[index];
                entry.factory = effectFactory;
                entry.deps = dependencies;
                metadata.FunctionEffects[index] = entry;
            }
            if (
                ReactiveUITK.Core.Reconciler.TraceLevel
                == ReactiveUITK.Core.Reconciler.DiffTraceLevel.Verbose
            )
            {
                try
                {
                    Debug.Log(
                        "[Hooks] UseEffect captured index="
                            + index
                            + ", depsLen="
                            + (dependencies?.Length ?? 0)
                    );
                }
                catch { }
            }
            metadata.EffectIndex++;
        }

        // Animate the current function component container using tracks.
        // Imperative per-frame updates; avoids re-rendering for animation frames.
        public static void UseAnimate(System.Collections.Generic.IReadOnlyList<ReactiveUITK.Core.Animation.AnimateTrack> tracks, bool autoplay = true, params object[] dependencies)
        {
            NodeMetadata metadata = HookContext.Current;
            if (metadata == null)
            {
                return;
            }
            // Store last handles in hook state slot
            metadata.HookStates ??= new System.Collections.Generic.List<object>();
            if (metadata.HookIndex >= metadata.HookStates.Count)
            {
                metadata.HookStates.Add(null);
            }
            int index = metadata.HookIndex;
            metadata.HookIndex++;
            // Run effect when deps change
            UseEffect(() =>
            {
                // stop previous if any
                var prev = metadata.HookStates[index] as System.Collections.Generic.List<ReactiveUITK.Core.Animation.AnimationHandle>;
                if (prev != null)
                {
                    foreach (var h in prev)
                    {
                        try { h?.Stop(); } catch { }
                    }
                }
                System.Collections.Generic.List<ReactiveUITK.Core.Animation.AnimationHandle> handles = null;
                if (autoplay && tracks != null && tracks.Count > 0)
                {
                    handles = ReactiveUITK.Core.Animation.Animator.PlayTracks(metadata.Container, tracks);
                }
                metadata.HookStates[index] = handles;
                return () =>
                {
                    var hs = metadata.HookStates[index] as System.Collections.Generic.List<ReactiveUITK.Core.Animation.AnimationHandle>;
                    if (hs != null)
                    {
                        foreach (var h in hs)
                        {
                            try { h?.Stop(); } catch { }
                        }
                        metadata.HookStates[index] = null;
                    }
                };
            }, dependencies);
        }

        // Tween a float value without re-rendering; invokes onUpdate each frame.
        public static void UseTweenFloat(
            float from,
            float to,
            float duration,
            ReactiveUITK.Core.Animation.Ease ease,
            float delay,
            System.Action<float> onUpdate,
            System.Action onComplete,
            params object[] dependencies
        )
        {
            NodeMetadata metadata = HookContext.Current;
            if (metadata == null)
            {
                return;
            }
            metadata.HookStates ??= new System.Collections.Generic.List<object>();
            if (metadata.HookIndex >= metadata.HookStates.Count)
            {
                metadata.HookStates.Add(null);
            }
            int index = metadata.HookIndex;
            metadata.HookIndex++;

            UseEffect(() =>
            {
                UnityEngine.UIElements.IVisualElementScheduledItem item = null;
                double start = 0;
                bool started = false;
                item = metadata.Container.schedule.Execute(() =>
                {
                    if (metadata.Container.panel == null)
                    {
                        try { item?.Pause(); } catch { }
                        item = null;
                        return;
                    }
                    double now;
                    try { now = UnityEngine.Time.realtimeSinceStartupAsDouble; } catch { now = (double)UnityEngine.Time.realtimeSinceStartup; }
                    if (!started)
                    {
                        start = now + delay;
                        started = true;
                    }
                    if (now < start)
                    {
                        return;
                    }
                    float t = duration <= 0f ? 1f : UnityEngine.Mathf.Clamp01((float)((now - start) / duration));
                    float eased = ReactiveUITK.Core.Animation.Easing.Evaluate(ease, t);
                    float v = UnityEngine.Mathf.Lerp(from, to, eased);
                    try { onUpdate?.Invoke(v); } catch { }
                    if (t >= 1f)
                    {
                        try { onComplete?.Invoke(); } catch { }
                        try { item?.Pause(); } catch { }
                        item = null;
                    }
                }).Every(16);

                return () =>
                {
                    try { item?.Pause(); } catch { }
                };
            }, dependencies);
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
