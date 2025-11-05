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
        public static bool EnableHookValidation { get; set; } = true;
    public static bool EnableStrictDiagnostics { get; set; } = false;

        public sealed class MutableRef<T>
        {
            public T Value;

            public T Current
            {
                get => Value;
                set => Value = value;
            }
        }

    // React-style setter delegate: accepts either a direct value or functional updater and returns the committed value.
    public delegate T StateSetter<T>(StateUpdate<T> update);

        public readonly struct StateUpdate<T>
        {
            internal readonly T Value;
            internal readonly Func<T, T> Updater;
            internal readonly bool UsesUpdater;

            private StateUpdate(T value, Func<T, T> updater, bool usesUpdater)
            {
                Value = value;
                Updater = updater;
                UsesUpdater = usesUpdater;
            }

            internal T Apply(T previous)
            {
                if (!UsesUpdater)
                {
                    return Value;
                }
                if (Updater == null)
                {
                    return previous;
                }
                return Updater(previous);
            }

            public static implicit operator StateUpdate<T>(T value) => new StateUpdate<T>(value, null, false);

            public static implicit operator StateUpdate<T>(Func<T, T> updater) =>
                new StateUpdate<T>(default, updater, true);
        }

        internal readonly struct StateSetterHandle<T>
        {
            private readonly NodeMetadata metadata;
            private readonly int index;
            private const byte ValueDelegateKind = 0;
            private const byte UpdaterDelegateKind = 1;
            private const byte CombinedDelegateKind = 2;
            private static readonly Action<T> NoopValueDelegate = _ => { };
            private static readonly Action<Func<T, T>> NoopUpdaterDelegate = _ => { };
            private static readonly StateSetter<T> NoopCombinedDelegate = _ => default;

            internal StateSetterHandle(NodeMetadata metadata, int index)
            {
                this.metadata = metadata;
                this.index = index;
            }

            public void Invoke(T value) => GetValueDelegate()(value);

            public void Invoke(Func<T, T> updater) => GetUpdaterDelegate()(updater);

            public T Invoke(StateUpdate<T> update) => GetCombinedDelegate()(update);

            public T Set(StateUpdate<T> update)
            {
                if (!update.UsesUpdater)
                {
                    return Set(update.Value);
                }
                if (update.Updater == null)
                {
                    return GetCurrent();
                }
                return Set(update.Updater);
            }

            public T Set(Func<T, T> updater)
            {
                if (metadata == null || updater == null)
                {
                    return default;
                }
                var previous = GetCurrent();
                return Set(updater(previous));
            }

            public T Set(T value)
            {
                if (metadata == null)
                {
                    return value;
                }
                if (metadata.IsRendering)
                {
                    WarnStrict(
                        metadata,
                        "state-update-during-render",
                        $"[Hooks][StrictMode] State update scheduled during render of '{DescribeComponent(metadata)}'. Move this set call to an effect or event handler."
                    );
                }
                if (metadata.HookStates == null || index >= metadata.HookStates.Count)
                {
                    return value;
                }
                var previous = metadata.HookStates[index];
                if (previous is T prevTyped && Equals(prevTyped, value))
                {
                    return prevTyped;
                }
                metadata.HookStates[index] = value;
                Hooks.RequestComponentRerender(metadata);
                return value;
            }

            private T GetCurrent()
            {
                if (metadata == null || metadata.HookStates == null || index >= metadata.HookStates.Count)
                {
                    return default;
                }
                return metadata.HookStates[index] is T current ? current : default;
            }

            private Action<T> GetValueDelegate()
            {
                if (metadata == null)
                {
                    return NoopValueDelegate;
                }
                metadata.StateSetterDelegateCache ??= new Dictionary<(int, byte), Delegate>();
                var key = (index, ValueDelegateKind);
                if (
                    metadata.StateSetterDelegateCache.TryGetValue(key, out var existing)
                    && existing is Action<T> typed
                )
                {
                    return typed;
                }
                var setter = this;
                Action<T> action = value => setter.Set(value);
                metadata.StateSetterDelegateCache[key] = action;
                return action;
            }

            private Action<Func<T, T>> GetUpdaterDelegate()
            {
                if (metadata == null)
                {
                    return NoopUpdaterDelegate;
                }
                metadata.StateSetterDelegateCache ??= new Dictionary<(int, byte), Delegate>();
                var key = (index, UpdaterDelegateKind);
                if (
                    metadata.StateSetterDelegateCache.TryGetValue(key, out var existing)
                    && existing is Action<Func<T, T>> typed
                )
                {
                    return typed;
                }
                var setter = this;
                Action<Func<T, T>> action = update =>
                {
                    if (update == null)
                    {
                        return;
                    }
                    setter.Set(update);
                };
                metadata.StateSetterDelegateCache[key] = action;
                return action;
            }

            internal StateSetter<T> GetCombinedDelegate()
            {
                if (metadata == null)
                {
                    return NoopCombinedDelegate;
                }
                metadata.StateSetterDelegateCache ??= new Dictionary<(int, byte), Delegate>();
                var key = (index, CombinedDelegateKind);
                if (
                    metadata.StateSetterDelegateCache.TryGetValue(key, out var existing)
                    && existing is StateSetter<T> typed
                )
                {
                    return typed;
                }
                var setter = this;
                StateSetter<T> combined = update =>
                {
                    if (!update.UsesUpdater)
                    {
                        return setter.Set(update.Value);
                    }
                    if (update.Updater == null)
                    {
                        return setter.GetCurrent();
                    }
                    return setter.Set(update.Updater);
                };
                metadata.StateSetterDelegateCache[key] = combined;
                return combined;
            }
        }


        private const string HookIdUseSafeArea = "UseSafeArea";
        private const string HookIdStableFunc = "UseStableFunc";
        private const string HookIdStableAction = "UseStableAction";
        private const string HookIdStableCallback = "UseStableCallback";
        private const string HookIdLayoutEffect = "UseLayoutEffect";
        private const string HookIdState = "UseState";
        private const string HookIdReducer = "UseReducer";
        private const string HookIdMemo = "UseMemo";
        private const string HookIdDeferred = "UseDeferredValue";
        private const string HookIdCallback = "UseCallback";
        private const string HookIdImperative = "UseImperativeHandle";
        private const string HookIdElementRef = "UseRefElement";
        private const string HookIdEffect = "UseEffect";
        private const string HookIdAnimate = "UseAnimate";
        private const string HookIdTween = "UseTween";
        private const string HookIdContext = "UseContext";
        private const string HookIdMutableRef = "UseMutableRef";

        private static void RecordHook(NodeMetadata metadata, string hookId)
        {
            if (!EnableHookValidation || metadata == null)
            {
                return;
            }
            metadata.HookOrderSignatures ??= new List<string>();
            int index = metadata.HookIndex;
            if (!metadata.HookOrderPrimed)
            {
                if (metadata.HookOrderSignatures.Count > index)
                {
                    metadata.HookOrderSignatures[index] = hookId;
                }
                else
                {
                    metadata.HookOrderSignatures.Add(hookId);
                }
                return;
            }
            if (index >= metadata.HookOrderSignatures.Count)
            {
                Debug.LogError(
                    $"[Hooks] Hook count changed for component {DescribeComponent(metadata)}"
                );
                return;
            }
            if (!string.Equals(metadata.HookOrderSignatures[index], hookId, StringComparison.Ordinal))
            {
                Debug.LogError(
                    $"[Hooks] Hook order mismatch: expected {metadata.HookOrderSignatures[index]} but saw {hookId} for component {DescribeComponent(metadata)}"
                );
            }
        }

        private static string DescribeComponent(NodeMetadata metadata)
        {
            if (metadata == null)
            {
                return "<null>";
            }
            if (!string.IsNullOrEmpty(metadata.Key))
            {
                return metadata.Key;
            }
            try
            {
                return metadata.Container != null ? metadata.Container.name : "<container-null>";
            }
            catch
            {
                return "<unknown>";
            }
        }

        private static void WarnStrict(NodeMetadata metadata, string key, string message)
        {
            if (!EnableStrictDiagnostics || metadata == null || string.IsNullOrEmpty(message))
            {
                return;
            }
            metadata.StrictDiagnosticsKeys ??= new HashSet<string>();
            if (!metadata.StrictDiagnosticsKeys.Add(key))
            {
                return;
            }
            try
            {
                Debug.LogWarning(message);
            }
            catch { }
        }

        private static void WarnMissingDependencies(
            NodeMetadata metadata,
            string hookId,
            int logicalIndex,
            object[] dependencies,
            bool treatEmptyAsMissing = false
        )
        {
            bool missing = dependencies == null
                || (treatEmptyAsMissing && (dependencies?.Length ?? 0) == 0);
            if (!missing)
            {
                return;
            }
            string component = DescribeComponent(metadata);
            string hookName = hookId ?? "Hook";
            string key = $"missing-deps:{hookName}:{logicalIndex}";
            WarnStrict(
                metadata,
                key,
                $"[Hooks][StrictMode] {hookName} in component '{component}' was invoked without a dependency array; it will re-run every render. Provide explicit dependencies or refactor the logic."
            );
        }

        private static IScheduler ResolveScheduler(NodeMetadata metadata)
        {
            if (metadata?.HostContext?.Environment == null)
            {
                return null;
            }
            if (
                metadata.HostContext.Environment.TryGetValue("scheduler", out var schedulerObj)
                && schedulerObj is IScheduler scheduler
            )
            {
                return scheduler;
            }
            return null;
        }

        private static void RequestComponentRerender(NodeMetadata metadata)
        {
            if (metadata?.Reconciler == null)
            {
                return;
            }
            if (metadata.IsRendering)
            {
                metadata.PendingUpdate = true;
                WarnStrict(
                    metadata,
                    "state-update-during-render",
                    $"[Hooks][StrictMode] State update scheduled during render of '{DescribeComponent(metadata)}'. Move this set call to an effect or event handler."
                );
                return;
            }
            if (metadata.UpdateQueued)
            {
                return;
            }
            metadata.UpdateQueued = true;
            void Flush()
            {
                try
                {
                    metadata.HookIndex = 0;
                    metadata.Reconciler.ForceFunctionComponentUpdate(metadata);
                }
                finally
                {
                    metadata.UpdateQueued = false;
                }
            }

            var scheduler = ResolveScheduler(metadata);
            if (scheduler != null)
            {
                scheduler.Enqueue(Flush);
            }
            else
            {
                Flush();
            }
        }

        public static SafeAreaInsets UseSafeArea(float tolerance = 0.5f)
        {
            // Lightweight read; no re-render trigger. Caller can combine with other state to refresh on orientation/resize.
            var current = SafeAreaUtility.GetInsets();
            NodeMetadata metadata = HookContext.Current;
            if (metadata == null)
            {
                return current;
            }
            RecordHook(metadata, HookIdUseSafeArea);
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
            RecordHook(metadata, HookIdStableFunc);
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
            RecordHook(metadata, HookIdStableAction);
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
            RecordHook(metadata, HookIdStableCallback);
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
            RecordHook(metadata, HookIdLayoutEffect);
            WarnMissingDependencies(metadata, HookIdLayoutEffect, metadata.LayoutEffectIndex, dependencies);
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

        public static (T value, StateSetter<T> set) UseState<T>(T initial = default)
        {
            NodeMetadata metadata = HookContext.Current;
            if (metadata == null)
            {
                StateSetter<T> noop = update =>
                {
                    if (!update.UsesUpdater)
                    {
                        return update.Value;
                    }
                    if (update.Updater == null)
                    {
                        return initial;
                    }
                    try
                    {
                        return update.Updater(initial);
                    }
                    catch
                    {
                        return initial;
                    }
                };
                return (initial, noop);
            }
            RecordHook(metadata, HookIdState);
            metadata.HookStates ??= new List<object>();
            if (metadata.HookIndex >= metadata.HookStates.Count)
            {
                metadata.HookStates.Add(initial);
            }
            T currentValue = (T)metadata.HookStates[metadata.HookIndex];
            int capturedIndex = metadata.HookIndex;
            metadata.HookIndex++;
            var setterHandle = new StateSetterHandle<T>(metadata, capturedIndex);
            return (currentValue, setterHandle.GetCombinedDelegate());
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
            RecordHook(metadata, HookIdReducer);
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
                    RequestComponentRerender(metadata);
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
            RecordHook(metadata, HookIdMemo);
            WarnMissingDependencies(
                metadata,
                HookIdMemo,
                metadata.HookIndex,
                dependencies,
                treatEmptyAsMissing: true
            );
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
            RecordHook(metadata, HookIdDeferred);
            WarnMissingDependencies(metadata, HookIdDeferred, metadata.HookIndex, dependencies);
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
                var scheduler = ResolveScheduler(metadata);
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
            RecordHook(metadata, HookIdCallback);
            WarnMissingDependencies(
                metadata,
                HookIdCallback,
                metadata.HookIndex,
                dependencies,
                treatEmptyAsMissing: true
            );
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
            RecordHook(metadata, HookIdImperative);
            WarnMissingDependencies(
                metadata,
                HookIdImperative,
                metadata.HookIndex,
                dependencies,
                treatEmptyAsMissing: true
            );
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

        public static MutableRef<T> UseRef<T>(T initial = default)
        {
            NodeMetadata metadata = HookContext.Current;
            if (metadata == null)
            {
                return new MutableRef<T> { Value = initial };
            }
            RecordHook(metadata, HookIdMutableRef);
            metadata.HookStates ??= new List<object>();
            if (metadata.HookIndex >= metadata.HookStates.Count)
            {
                metadata.HookStates.Add(new MutableRef<T> { Value = initial });
            }
            var stored = (MutableRef<T>)metadata.HookStates[metadata.HookIndex];
            metadata.HookIndex++;
            return stored;
        }

        public static VisualElement UseRef()
        {
            NodeMetadata metadata = HookContext.Current;
            if (metadata == null)
            {
                return null;
            }
            RecordHook(metadata, HookIdElementRef);
            metadata.HookStates ??= new List<object>();
            if (metadata.HookIndex >= metadata.HookStates.Count)
            {
                metadata.HookStates.Add(metadata.Container);
            }
            else
            {
                metadata.HookStates[metadata.HookIndex] = metadata.Container;
            }
            metadata.HookIndex++;
            return metadata.Container;
        }

        public static void UseEffect(Func<Action> effectFactory, params object[] dependencies)
        {
            NodeMetadata metadata = HookContext.Current;
            if (metadata == null)
            {
                return;
            }
            RecordHook(metadata, HookIdEffect);
            WarnMissingDependencies(metadata, HookIdEffect, metadata.EffectIndex, dependencies);
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
        public static void UseAnimate(
            System.Collections.Generic.IReadOnlyList<ReactiveUITK.Core.Animation.AnimateTrack> tracks,
            bool autoplay = true,
            params object[] dependencies
        )
        {
            NodeMetadata metadata = HookContext.Current;
            if (metadata == null)
            {
                return;
            }
            RecordHook(metadata, HookIdAnimate);
            // Store last handles in hook state slot
            metadata.HookStates ??= new System.Collections.Generic.List<object>();
            if (metadata.HookIndex >= metadata.HookStates.Count)
            {
                metadata.HookStates.Add(null);
            }
            int index = metadata.HookIndex;
            metadata.HookIndex++;
            // Run effect when deps change
            UseEffect(
                () =>
                {
                    // stop previous if any
                    var prev =
                        metadata.HookStates[index]
                        as System.Collections.Generic.List<ReactiveUITK.Core.Animation.AnimationHandle>;
                    if (prev != null)
                    {
                        foreach (var h in prev)
                        {
                            try
                            {
                                h?.Stop();
                            }
                            catch { }
                        }
                    }
                    System.Collections.Generic.List<ReactiveUITK.Core.Animation.AnimationHandle> handles =
                        null;
                    if (autoplay && tracks != null && tracks.Count > 0)
                    {
                        handles = ReactiveUITK.Core.Animation.Animator.PlayTracks(
                            metadata.Container,
                            tracks
                        );
                    }
                    metadata.HookStates[index] = handles;
                    return () =>
                    {
                        var hs =
                            metadata.HookStates[index]
                            as System.Collections.Generic.List<ReactiveUITK.Core.Animation.AnimationHandle>;
                        if (hs != null)
                        {
                            foreach (var h in hs)
                            {
                                try
                                {
                                    h?.Stop();
                                }
                                catch { }
                            }
                            metadata.HookStates[index] = null;
                        }
                    };
                },
                dependencies
            );
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
            RecordHook(metadata, HookIdTween);
            metadata.HookStates ??= new System.Collections.Generic.List<object>();
            if (metadata.HookIndex >= metadata.HookStates.Count)
            {
                metadata.HookStates.Add(null);
            }
            int index = metadata.HookIndex;
            metadata.HookIndex++;

            UseEffect(
                () =>
                {
                    UnityEngine.UIElements.IVisualElementScheduledItem item = null;
                    double start = 0;
                    bool started = false;
                    item = metadata
                        .Container.schedule.Execute(() =>
                        {
                            if (metadata.Container.panel == null)
                            {
                                try
                                {
                                    item?.Pause();
                                }
                                catch { }
                                item = null;
                                return;
                            }
                            double now;
                            try
                            {
                                now = UnityEngine.Time.realtimeSinceStartupAsDouble;
                            }
                            catch
                            {
                                now = (double)UnityEngine.Time.realtimeSinceStartup;
                            }
                            if (!started)
                            {
                                start = now + delay;
                                started = true;
                            }
                            if (now < start)
                            {
                                return;
                            }
                            float t =
                                duration <= 0f
                                    ? 1f
                                    : UnityEngine.Mathf.Clamp01((float)((now - start) / duration));
                            float eased = ReactiveUITK.Core.Animation.Easing.Evaluate(ease, t);
                            float v = UnityEngine.Mathf.Lerp(from, to, eased);
                            try
                            {
                                onUpdate?.Invoke(v);
                            }
                            catch { }
                            if (t >= 1f)
                            {
                                try
                                {
                                    onComplete?.Invoke();
                                }
                                catch { }
                                try
                                {
                                    item?.Pause();
                                }
                                catch { }
                                item = null;
                            }
                        })
                        .Every(16);

                    return () =>
                    {
                        try
                        {
                            item?.Pause();
                        }
                        catch { }
                    };
                },
                dependencies
            );
        }

        public static T UseContext<T>(string key)
        {
            NodeMetadata metadata = HookContext.Current;
            if (metadata == null || metadata.HostContext == null)
            {
                return default;
            }
            RecordHook(metadata, HookIdContext);
            metadata.SubscribedContextKeys ??= new HashSet<string>();
            metadata.SubscribedContextKeys.Add(key);
            object resolved = metadata.HostContext.ResolveContext(key);
            metadata.HookStates ??= new List<object>();
            if (metadata.HookIndex >= metadata.HookStates.Count)
            {
                metadata.HookStates.Add(default(T));
            }
            if (resolved is T typed)
            {
                metadata.HookStates[metadata.HookIndex] = typed;
                metadata.HookIndex++;
                return typed;
            }
            metadata.HookStates[metadata.HookIndex] = default(T);
            metadata.HookIndex++;
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
