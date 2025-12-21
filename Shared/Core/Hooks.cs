using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReactiveUITK.Core.Diagnostics;
using ReactiveUITK.Core.Fiber;
using ReactiveUITK.Core.Util;
using ReactiveUITK.Signals;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Core
{
    internal static class HookContext
    {
        [ThreadStatic]
        public static FunctionComponentState Current;
    }

    public static class Hooks
    {
        public static bool EnableHookValidation { get; set; } = true;
        public static bool EnableStrictDiagnostics { get; set; } = false;

        public static bool EnableHookAutoRealign { get; set; } = true;

        public sealed class MutableRef<T>
        {
            public T Value;

            public T Current
            {
                get => Value;
                set => Value = value;
            }
        }

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

            public static implicit operator StateUpdate<T>(T value) =>
                new StateUpdate<T>(value, null, false);

            public static implicit operator StateUpdate<T>(Func<T, T> updater) =>
                new StateUpdate<T>(default, updater, true);
        }

        internal readonly struct PendingStateUpdate
        {
            private readonly bool usesUpdater;
            private readonly object directValue;
            private readonly Func<object, object> updater;

            private PendingStateUpdate(
                bool usesUpdater,
                object directValue,
                Func<object, object> updater
            )
            {
                this.usesUpdater = usesUpdater;
                this.directValue = directValue;
                this.updater = updater;
            }

            public static PendingStateUpdate From<T>(StateUpdate<T> update)
            {
                if (update.UsesUpdater)
                {
                    Func<object, object> bridge = previous =>
                    {
                        T typedPrev = previous is T cast ? cast : default;
                        return update.Apply(typedPrev);
                    };
                    return new PendingStateUpdate(true, null, bridge);
                }
                return new PendingStateUpdate(false, update.Value, null);
            }

            public object Apply(object previous)
            {
                if (usesUpdater)
                {
                    return updater != null ? updater(previous) : previous;
                }
                return directValue;
            }
        }

        internal readonly struct StateSetterHandle<T>
        {
            private readonly FunctionComponentState state;
            private readonly NodeMetadata metadata;
            private readonly int index;
            private const byte ValueDelegateKind = 0;
            private const byte UpdaterDelegateKind = 1;
            private const byte CombinedDelegateKind = 2;
            private static readonly Action<T> NoopValueDelegate = _ => { };
            private static readonly Action<Func<T, T>> NoopUpdaterDelegate = _ => { };
            private static readonly StateSetter<T> NoopCombinedDelegate = _ => default;

            internal StateSetterHandle(FunctionComponentState state, int index)
            {
                this.state = state;
                metadata = state?.Owner;
                this.index = index;
            }

            public void Invoke(T value) => GetValueDelegate()(value);

            public void Invoke(Func<T, T> updater) => GetUpdaterDelegate()(updater);

            public T Invoke(StateUpdate<T> update) => GetCombinedDelegate()(update);

            public T Set(StateUpdate<T> update) => ApplyAndQueue(update);

            public T Set(Func<T, T> updater) => ApplyAndQueue(updater);

            public T Set(T value)
            {
                UnityEngine.Debug.Log($"[Hooks] Set(T value) called. Value={value}");
                return ApplyAndQueue(value);
            }

            private T ApplyAndQueue(StateUpdate<T> update)
            {
                UnityEngine.Debug.Log($"[Hooks] ApplyAndQueue called. State={state?.GetHashCode()}");
                if (state == null)
                {
                    return update.Apply(default);
                }
                if (state.IsRendering)
                {
                    WarnStrict(
                        state,
                        metadata,
                        "state-update-during-render",
                        $"[Hooks][StrictMode] State update scheduled during render of '{DescribeComponent(metadata)}'. Move this set call to an effect or event handler."
                    );
                }
                var previous = GetProjectedState();
                var computed = update.Apply(previous);

                // Avoid scheduling if the state value did not actually change.
                if (EqualityComparer<T>.Default.Equals(previous, computed))
                {
                    return previous;
                }

                EnqueuePendingUpdate(update, computed);
                metadata?.SyncComponentState(state);
                return computed;
            }

            private T GetProjectedState()
            {
                if (state == null)
                {
                    return default;
                }

                if (
                    state.PendingHookStatePreviews != null
                    && state.PendingHookStatePreviews.TryGetValue(index, out var pending)
                    && pending is T projected
                )
                {
                    return projected;
                }

                if (state.HookStates == null || index >= state.HookStates.Count)
                {
                    return default;
                }

                return state.HookStates[index] is T current ? current : default;
            }

            private void EnqueuePendingUpdate(StateUpdate<T> update, T computed)
            {
                // Fiber path: if the component state has an update callback,
                // update the hook state immediately and trigger Fiber to
                // schedule a re-render. This avoids relying on the legacy
                // queued-update mechanism, which is primarily for the
                // old reconciler.
                UnityEngine.Debug.Log($"[Hooks] EnqueuePendingUpdate. State={state?.GetHashCode()}, HasOnStateUpdated={state?.OnStateUpdated != null}");

                if (state.OnStateUpdated != null)
                {
                    state.HookStates ??= new List<object>();
                    while (state.HookStates.Count <= index)
                    {
                        state.HookStates.Add(null);
                    }
                    state.HookStates[index] = computed;
                    state.PendingHookStatePreviews?.Remove(index);
                    metadata?.SyncComponentState(state);

                    if (InternalLogOptions.EnableInternalLogs)
                    {
                        try
                        {
                            UnityEngine.Debug.Log(
                                $"[Hooks] EnqueuePendingUpdate (Fiber immediate) state={state?.GetHashCode() ?? 0} slot={index} computed={computed}"
                            );
                        }
                        catch { }
                    }

                    state.OnStateUpdated.Invoke();
                    return;
                }

                // Legacy reconciler path: enqueue into a pending queue that
                // will be flushed before the next render.
                state.HookStateQueues ??= new Dictionary<int, HookStateUpdateQueue>();
                if (!state.HookStateQueues.TryGetValue(index, out var queue))
                {
                    queue = new HookStateUpdateQueue();
                    state.HookStateQueues[index] = queue;
                }
                queue.Enqueue(PendingStateUpdate.From(update));
                state.PendingHookStatePreviews ??= new Dictionary<int, object>();
                state.PendingHookStatePreviews[index] = computed;
                metadata?.SyncComponentState(state);

                if (InternalLogOptions.EnableInternalLogs)
                {
                    try
                    {
                        UnityEngine.Debug.Log(
                            $"[Hooks] EnqueuePendingUpdate state={state?.GetHashCode() ?? 0} slot={index} computed={computed} hasUpdateCb={(state?.OnStateUpdated != null ? "true" : "false")}"
                        );
                    }
                    catch { }
                }

                // Schedule a rerender via either Fiber (preferred) or legacy metadata.
                Hooks.RequestComponentRerender(metadata);
            }

            private Action<T> GetValueDelegate()
            {
                if (state == null)
                {
                    return NoopValueDelegate;
                }
                state.StateSetterDelegateCache ??= new Dictionary<(int, byte), Delegate>();
                var key = (index, ValueDelegateKind);
                if (
                    state.StateSetterDelegateCache.TryGetValue(key, out var existing)
                    && existing is Action<T> typed
                )
                {
                    return typed;
                }
                var setter = this;
                Action<T> action = value => setter.Set(value);
                state.StateSetterDelegateCache[key] = action;
                return action;
            }

            private Action<Func<T, T>> GetUpdaterDelegate()
            {
                if (state == null)
                {
                    return NoopUpdaterDelegate;
                }
                state.StateSetterDelegateCache ??= new Dictionary<(int, byte), Delegate>();
                var key = (index, UpdaterDelegateKind);
                if (
                    state.StateSetterDelegateCache.TryGetValue(key, out var existing)
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
                state.StateSetterDelegateCache[key] = action;
                return action;
            }

            internal StateSetter<T> GetCombinedDelegate()
            {
                if (state == null)
                {
                    return NoopCombinedDelegate;
                }
                state.StateSetterDelegateCache ??= new Dictionary<(int, byte), Delegate>();
                var key = (index, CombinedDelegateKind);
                if (
                    state.StateSetterDelegateCache.TryGetValue(key, out var existing)
                    && existing is StateSetter<T> typed
                )
                {
                    return typed;
                }
                var setter = this;
                StateSetter<T> combined = update =>
                {
                    UnityEngine.Debug.Log($"[Hooks] CombinedDelegate invoked via {key}. UpdateValue={update.Value}");
                    if (!update.UsesUpdater)
                    {
                        return setter.Set(update.Value);
                    }
                    if (update.Updater == null)
                    {
                        return setter.GetProjectedState();
                    }
                    return setter.Set(update.Updater);
                };
                state.StateSetterDelegateCache[key] = combined;
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
        private const string HookIdSignal = "UseSignal";

        private static FunctionComponentState EnsureState(NodeMetadata metadata)
        {
            // Fiber path: rely on HookContext.Current when no metadata is available.
            if (metadata == null)
            {
                return HookContext.Current;
            }
            var state = HookContext.Current ?? metadata.ComponentState;
            if (state == null)
            {
                state = metadata.EnsureComponentState();
            }
            metadata?.SyncComponentState(state);
            return state;
        }

        private static void SyncState(NodeMetadata metadata, FunctionComponentState state)
        {
            metadata?.SyncComponentState(state);
        }

        private static UnityEngine.UIElements.VisualElement ResolveAnimationTarget(
            NodeMetadata metadata,
            FunctionComponentState state
        )
        {
            // Prefer the legacy container when available.
            if (metadata?.Container != null)
            {
                return metadata.Container;
            }

            // Fiber path: first try to find a host element in the current
            // function component's subtree (for headless wrappers like
            // Animate), then fall back to walking ancestor fibers.
            var fiber = state?.Fiber;
            if (fiber != null)
            {
                var descendantHost = FindFirstHostElement(fiber.Child);
                if (descendantHost != null)
                {
                    return descendantHost;
                }
            }

            while (fiber != null)
            {
                if (fiber.HostElement != null)
                {
                    return fiber.HostElement;
                }
                fiber = fiber.Parent;
            }
            return null;
        }

        private static UnityEngine.UIElements.VisualElement FindFirstHostElement(
            Fiber.FiberNode node
        )
        {
            var current = node;
            while (current != null)
            {
                if (current.HostElement != null)
                {
                    return current.HostElement;
                }
                var childResult = FindFirstHostElement(current.Child);
                if (childResult != null)
                {
                    return childResult;
                }
                current = current.Sibling;
            }
            return null;
        }

        private static void RecordHook(
            NodeMetadata metadata,
            FunctionComponentState state,
            string hookId
        )
        {
            if (!EnableHookValidation || metadata == null || state == null)
            {
                return;
            }
            state.HookOrderSignatures ??= new List<string>();
            int index = state.HookIndex;
            if (!state.HookOrderPrimed)
            {
                if (state.HookOrderSignatures.Count > index)
                {
                    state.HookOrderSignatures[index] = hookId;
                }
                else
                {
                    state.HookOrderSignatures.Add(hookId);
                }
                return;
            }
            if (index >= state.HookOrderSignatures.Count)
            {
                Debug.LogError(
                    $"[Hooks] Hook count changed for component {DescribeComponent(metadata)}"
                );
                return;
            }
            if (!string.Equals(state.HookOrderSignatures[index], hookId, StringComparison.Ordinal))
            {
                Debug.LogError(
                    $"[Hooks] Hook order mismatch: expected {state.HookOrderSignatures[index]} but saw {hookId} for component {DescribeComponent(metadata)}"
                );
                if (EnableHookAutoRealign)
                {
                    try
                    {
                        state.HookOrderSignatures.Clear();
                    }
                    catch { }
                    state.HookOrderSignatures.Add(hookId);
                    state.HookOrderPrimed = false;
                }
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

        private static void WarnStrict(
            FunctionComponentState state,
            NodeMetadata metadata,
            string key,
            string message
        )
        {
            if (!EnableStrictDiagnostics || string.IsNullOrEmpty(message))
            {
                return;
            }
            state ??= metadata?.ComponentState ?? HookContext.Current;
            if (state == null)
            {
                return;
            }
            state.StrictDiagnosticsKeys ??= new HashSet<string>();
            if (!state.StrictDiagnosticsKeys.Add(key))
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
            FunctionComponentState state,
            string hookId,
            int logicalIndex,
            object[] dependencies,
            bool treatEmptyAsMissing = false
        )
        {
            bool missing =
                dependencies == null || (treatEmptyAsMissing && (dependencies?.Length ?? 0) == 0);
            if (!missing)
            {
                return;
            }
            string component = DescribeComponent(metadata);
            string hookName = hookId ?? "Hook";
            string key = $"missing-deps:{hookName}:{logicalIndex}";
            WarnStrict(
                state,
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
            // Resolve state from either the current hook context (Fiber) or metadata (legacy).
            FunctionComponentState state =
                HookContext.Current ?? metadata?.ComponentState ?? metadata?.EnsureComponentState();
            if (state?.IsRendering == true)
            {
                state.PendingUpdate = true;
                if (metadata != null)
                {
                    WarnStrict(
                        state,
                        metadata,
                        "state-update-during-render",
                        $"[Hooks][StrictMode] State update scheduled during render of '{DescribeComponent(metadata)}'. Move this set call to an effect or event handler."
                    );
                    metadata?.SyncComponentState(state);
                }
                return;
            }
            if (metadata != null)
            {
                metadata?.SyncComponentState(state);
            }

            // Fiber integration: If we have a callback, use it
            if (state.OnStateUpdated != null)
            {
                state.OnStateUpdated.Invoke();
                return;
            }
        }

        internal static void FlushQueuedStateUpdates(FunctionComponentState state)
        {
            if (state == null)
            {
                return;
            }
            if (state.HookStateQueues == null || state.HookStateQueues.Count == 0)
            {
                state.PendingHookStatePreviews?.Clear();
                return;
            }

            if (InternalLogOptions.EnableInternalLogs)
            {
                try
                {
                    Debug.Log(
                        $"[Hooks] FlushQueuedStateUpdates state={state.GetHashCode()} queues={state.HookStateQueues.Count}"
                    );
                }
                catch { }
            }

            state.HookStates ??= new List<object>();
            foreach (var kvp in state.HookStateQueues)
            {
                var queue = kvp.Value;
                if (queue == null || !queue.HasPending)
                {
                    continue;
                }
                int slot = kvp.Key;
                object current = slot < state.HookStates.Count ? state.HookStates[slot] : null;
                var node = queue.ConsumeAll();
                while (node != null)
                {
                    var before = current;
                    current = node.Update.Apply(current);
                    node = node.Next;
                    if (InternalLogOptions.EnableInternalLogs)
                    {
                        try
                        {
                            Debug.Log($"[Hooks] Flush slot={slot} before={before} after={current}");
                        }
                        catch { }
                    }
                }
                while (state.HookStates.Count <= slot)
                {
                    state.HookStates.Add(null);
                }
                state.HookStates[slot] = current;
            }
            state.PendingHookStatePreviews?.Clear();
        }

        internal static void FlushQueuedStateUpdates(NodeMetadata metadata)
        {
            if (metadata == null)
            {
                return;
            }
            var state = metadata.EnsureComponentState();
            if (state.HookStateQueues == null || state.HookStateQueues.Count == 0)
            {
                state.PendingHookStatePreviews?.Clear();
                metadata?.SyncComponentState(state);
                return;
            }
            state.HookStates ??= new List<object>();
            foreach (var kvp in state.HookStateQueues)
            {
                var queue = kvp.Value;
                if (queue == null || !queue.HasPending)
                {
                    continue;
                }
                int slot = kvp.Key;
                object current = slot < state.HookStates.Count ? state.HookStates[slot] : null;
                var node = queue.ConsumeAll();
                while (node != null)
                {
                    current = node.Update.Apply(current);
                    node = node.Next;
                }
                while (state.HookStates.Count <= slot)
                {
                    state.HookStates.Add(null);
                }
                state.HookStates[slot] = current;
            }
            state.PendingHookStatePreviews?.Clear();
            metadata?.SyncComponentState(state);
        }

        public static SafeAreaInsets UseSafeArea(float tolerance = 0.5f)
        {
            var current = SafeAreaUtility.GetInsets();
            FunctionComponentState state = HookContext.Current;
            NodeMetadata metadata = state?.Owner;
            if (metadata == null || state == null)
            {
                return current;
            }
            RecordHook(metadata, state, HookIdUseSafeArea);
            state.HookStates ??= new System.Collections.Generic.List<object>();
            if (state.HookIndex >= state.HookStates.Count)
            {
                state.HookStates.Add(current);
            }

            state.HookStates[state.HookIndex] = current;
            state.HookIndex++;
            metadata?.SyncComponentState(state);
            return current;
        }

        public static Func<T> UseStableFunc<T>(Func<T> function)
        {
            NodeMetadata metadata = HookContext.Current?.Owner;
            var state = EnsureState(metadata);
            if (metadata == null || state == null)
            {
                return function;
            }
            RecordHook(metadata, state, HookIdStableFunc);
            state.HookStates ??= new List<object>();
            if (state.HookIndex >= state.HookStates.Count)
            {
                state.HookStates.Add(function);
            }
            Func<T> stored = (Func<T>)state.HookStates[state.HookIndex];
            state.HookIndex++;
            state.HookStates[state.HookIndex - 1] = function;
            metadata?.SyncComponentState(state);
            return stored;
        }

        public static Action<T> UseStableAction<T>(Action<T> action)
        {
            NodeMetadata metadata = HookContext.Current?.Owner;
            var state = EnsureState(metadata);
            if (metadata == null || state == null)
            {
                return action;
            }
            RecordHook(metadata, state, HookIdStableAction);
            state.HookStates ??= new List<object>();
            if (state.HookIndex >= state.HookStates.Count)
            {
                state.HookStates.Add(action);
            }
            Action<T> stored = (Action<T>)state.HookStates[state.HookIndex];
            state.HookIndex++;
            state.HookStates[state.HookIndex - 1] = action;
            metadata?.SyncComponentState(state);
            return stored;
        }

        public static Action UseStableCallback(Action callback)
        {
            NodeMetadata metadata = HookContext.Current?.Owner;
            var state = EnsureState(metadata);
            if (metadata == null || state == null)
            {
                return callback;
            }
            RecordHook(metadata, state, HookIdStableCallback);
            state.HookStates ??= new List<object>();
            if (state.HookIndex >= state.HookStates.Count)
            {
                state.HookStates.Add(callback);
            }
            Action stored = (Action)state.HookStates[state.HookIndex];
            state.HookIndex++;
            state.HookStates[state.HookIndex - 1] = callback;
            metadata?.SyncComponentState(state);
            return stored;
        }

        public static void UseLayoutEffect(Func<Action> effectFactory, params object[] dependencies)
        {
            NodeMetadata metadata = HookContext.Current?.Owner;
            var state = EnsureState(metadata);
            if (state == null)
            {
                return;
            }
            RecordHook(metadata, state, HookIdLayoutEffect);
            if (metadata != null)
            {
                WarnMissingDependencies(
                    metadata,
                    state,
                    HookIdLayoutEffect,
                    state.LayoutEffectIndex,
                    dependencies
                );
            }
            state.FunctionLayoutEffects ??= new List<(Func<Action>, object[], object[], Action)>();
            int index = state.LayoutEffectIndex;
            if (index >= state.FunctionLayoutEffects.Count)
            {
                state.FunctionLayoutEffects.Add((effectFactory, dependencies, null, null));
            }
            else
            {
                var entry = state.FunctionLayoutEffects[index];
                entry.factory = effectFactory;
                entry.deps = dependencies;
                state.FunctionLayoutEffects[index] = entry;
            }
            state.LayoutEffectIndex++;
            SyncState(metadata, state);
        }

        public static (T value, StateSetter<T> set) UseState<T>(T initial = default)
        {
            NodeMetadata metadata = HookContext.Current?.Owner;
            FunctionComponentState state = HookContext.Current ?? metadata?.EnsureComponentState();
            if (state == null)
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
            RecordHook(metadata, state, HookIdState);
            state.HookStates ??= new List<object>();
            if (state.HookIndex >= state.HookStates.Count)
            {
                state.HookStates.Add(initial);
            }
            T currentValue = (T)state.HookStates[state.HookIndex];
            int capturedIndex = state.HookIndex;
            state.HookIndex++;
            if (metadata != null)
            {
                metadata?.SyncComponentState(state);
            }
            var setterHandle = new StateSetterHandle<T>(state, capturedIndex);
            return (currentValue, setterHandle.GetCombinedDelegate());
        }

        public static (TState state, Action<TAction> dispatch) UseReducer<TState, TAction>(
            Func<TState, TAction, TState> reducer,
            TState initialState
        )
        {
            NodeMetadata metadata = HookContext.Current?.Owner;
            FunctionComponentState state = HookContext.Current ?? metadata?.EnsureComponentState();
            if (state == null)
            {
                return (initialState, _ => { });
            }
            RecordHook(metadata, state, HookIdReducer);
            state.HookStates ??= new List<object>();
            if (state.HookIndex >= state.HookStates.Count)
            {
                state.HookStates.Add(initialState);
            }
            int index = state.HookIndex;
            TState currentState = (TState)state.HookStates[index];
            state.HookIndex++;
            if (metadata != null)
            {
                metadata?.SyncComponentState(state);
            }
            void Dispatch(TAction action)
            {
                TState previous = (TState)state.HookStates[index];
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
                    state.HookStates[index] = next;
                    RequestComponentRerender(metadata);
                }
            }
            if (metadata != null)
            {
                metadata?.SyncComponentState(state);
            }
            return (currentState, Dispatch);
        }

        public static T UseMemo<T>(Func<T> factory, params object[] dependencies)
        {
            NodeMetadata metadata = HookContext.Current?.Owner;
            var state = EnsureState(metadata);
            if (state == null)
            {
                return factory();
            }
            RecordHook(metadata, state, HookIdMemo);
            if (metadata != null)
            {
                WarnMissingDependencies(
                    metadata,
                    state,
                    HookIdMemo,
                    state.HookIndex,
                    dependencies,
                    treatEmptyAsMissing: true
                );
            }
            state.HookStates ??= new List<object>();
            if (state.HookIndex >= state.HookStates.Count)
            {
                state.HookStates.Add((factory(), dependencies));
            }

            if (state.HookIndex >= state.HookStates.Count)
            {
                state.HookStates.Add((factory(), dependencies));
            }
            var tuple = ((T value, object[] d))state.HookStates[state.HookIndex];
            bool changed = DepsChanged(tuple.d, dependencies);
            if (changed)
            {
                tuple = (factory(), dependencies);
                state.HookStates[state.HookIndex] = tuple;
            }
            state.HookIndex++;
            SyncState(metadata, state);
            return tuple.value;
        }

        public static T UseDeferredValue<T>(T value, params object[] dependencies)
        {
            NodeMetadata metadata = HookContext.Current?.Owner;
            var state = EnsureState(metadata);
            if (state == null)
            {
                return value;
            }
            RecordHook(metadata, state, HookIdDeferred);
            if (metadata != null)
            {
                WarnMissingDependencies(
                    metadata,
                    state,
                    HookIdDeferred,
                    state.HookIndex,
                    dependencies
                );
            }
            state.HookStates ??= new List<object>();
            if (state.HookIndex >= state.HookStates.Count)
            {
                state.HookStates.Add((value, dependencies));
            }
            var tuple = ((T val, object[] d))state.HookStates[state.HookIndex];
            bool changed = DepsChanged(tuple.d, dependencies);
            if (changed && !Equals(tuple.val, value))
            {
                T newVal = value;
                int index = state.HookIndex;
                var scheduler = ResolveScheduler(metadata);
                if (scheduler != null)
                {
                    scheduler.EnqueueBatchedEffect(() =>
                    {
                        state.HookStates[index] = (newVal, dependencies);
                        SyncState(metadata, state);
                    });
                }
                else
                {
                    state.HookStates[index] = (newVal, dependencies);
                }
            }
            state.HookIndex++;
            SyncState(metadata, state);
            var latest = ((T val, object[] d))state.HookStates[state.HookIndex - 1];
            return latest.val;
        }

        public static Func<T> UseCallback<T>(Func<T> callback, params object[] dependencies)
        {
            NodeMetadata metadata = HookContext.Current?.Owner;
            var state = EnsureState(metadata);
            if (state == null)
            {
                return callback;
            }
            RecordHook(metadata, state, HookIdCallback);
            if (metadata != null)
            {
                WarnMissingDependencies(
                    metadata,
                    state,
                    HookIdCallback,
                    state.HookIndex,
                    dependencies,
                    treatEmptyAsMissing: true
                );
            }
            state.HookStates ??= new List<object>();
            if (state.HookIndex >= state.HookStates.Count)
            {
                state.HookStates.Add((callback, dependencies));
            }
            var tuple = ((Func<T> cb, object[] d))state.HookStates[state.HookIndex];
            bool changed = DepsChanged(tuple.d, dependencies);
            if (changed)
            {
                tuple = (callback, dependencies);
                state.HookStates[state.HookIndex] = tuple;
            }
            state.HookIndex++;
            SyncState(metadata, state);
            return tuple.cb;
        }

        public static THandle UseImperativeHandle<THandle>(
            Func<THandle> factory,
            params object[] dependencies
        )
            where THandle : class
        {
            NodeMetadata metadata = HookContext.Current?.Owner;
            var state = EnsureState(metadata);
            if (state == null)
            {
                return factory();
            }
            RecordHook(metadata, state, HookIdImperative);
            if (metadata != null)
            {
                WarnMissingDependencies(
                    metadata,
                    state,
                    HookIdImperative,
                    state.HookIndex,
                    dependencies,
                    treatEmptyAsMissing: true
                );
            }
            state.HookStates ??= new List<object>();
            if (state.HookIndex >= state.HookStates.Count)
            {
                state.HookStates.Add((factory(), dependencies));
            }
            var tuple = ((THandle handle, object[] d))state.HookStates[state.HookIndex];
            if (DepsChanged(tuple.d, dependencies))
            {
                tuple = (factory(), dependencies);
                state.HookStates[state.HookIndex] = tuple;
            }
            state.HookIndex++;
            SyncState(metadata, state);
            return tuple.handle;
        }

        public static MutableRef<T> UseRef<T>(T initial = default)
        {
            NodeMetadata metadata = HookContext.Current?.Owner;
            var state = EnsureState(metadata);
            if (state == null)
            {
                return new MutableRef<T> { Value = initial };
            }
            RecordHook(metadata, state, HookIdMutableRef);
            state.HookStates ??= new List<object>();
            if (state.HookIndex >= state.HookStates.Count)
            {
                state.HookStates.Add(new MutableRef<T> { Value = initial });
            }
            var stored = (MutableRef<T>)state.HookStates[state.HookIndex];
            state.HookIndex++;
            SyncState(metadata, state);
            return stored;
        }

        public static VisualElement UseRef()
        {
            NodeMetadata metadata = HookContext.Current?.Owner;
            var state = EnsureState(metadata);
            if (state == null)
            {
                return null;
            }

            // Fiber path: prefer the current fiber's host element or
            // its first host-descendant as the ref target.
            VisualElement target = null;
            var fiber = state.Fiber;
            if (fiber != null)
            {
                target = ResolveAnimationTarget(metadata, state);
            }

            // Legacy path: fall back to metadata.Container when available.
            target ??= metadata?.Container;

            if (target == null)
            {
                return null;
            }

            RecordHook(metadata, state, HookIdElementRef);
            state.HookStates ??= new List<object>();
            if (state.HookIndex >= state.HookStates.Count)
            {
                state.HookStates.Add(target);
            }
            else
            {
                state.HookStates[state.HookIndex] = target;
            }
            state.HookIndex++;
            SyncState(metadata, state);
            return target;
        }

        public static void UseEffect(Func<Action> effectFactory, params object[] dependencies)
        {
            NodeMetadata metadata = HookContext.Current?.Owner;
            var state = EnsureState(metadata);
            if (state == null)
            {
                return;
            }
            RecordHook(metadata, state, HookIdEffect);
            if (metadata != null)
            {
                WarnMissingDependencies(
                    metadata,
                    state,
                    HookIdEffect,
                    state.EffectIndex,
                    dependencies
                );
            }
            state.FunctionEffects ??= new List<(Func<Action>, object[], object[], Action)>();
            int index = state.EffectIndex;
            if (index >= state.FunctionEffects.Count)
            {
                state.FunctionEffects.Add((effectFactory, dependencies, null, null));
            }
            else
            {
                var entry = state.FunctionEffects[index];
                entry.factory = effectFactory;
                entry.deps = dependencies;
                state.FunctionEffects[index] = entry;
            }
            if (DiagnosticsConfig.CurrentTraceLevel == DiagnosticsConfig.TraceLevel.Verbose)
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
            state.EffectIndex++;
            SyncState(metadata, state);
        }

        public static void UseAnimate(
            System.Collections.Generic.IReadOnlyList<ReactiveUITK.Core.Animation.AnimateTrack> tracks,
            bool autoplay = true,
            params object[] dependencies
        )
        {
            NodeMetadata metadata = HookContext.Current?.Owner;
            var state = EnsureState(metadata);
            if (state == null)
            {
                return;
            }
            RecordHook(metadata, state, HookIdAnimate);

            state.HookStates ??= new System.Collections.Generic.List<object>();
            if (state.HookIndex >= state.HookStates.Count)
            {
                state.HookStates.Add(null);
            }
            int index = state.HookIndex;
            state.HookIndex++;
            SyncState(metadata, state);

            UseEffect(
                () =>
                {
                    var prev =
                        state.HookStates[index]
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

                    var target = ResolveAnimationTarget(metadata, state);
                    System.Collections.Generic.List<ReactiveUITK.Core.Animation.AnimationHandle> handles =
                        null;
                    if (autoplay && tracks != null && tracks.Count > 0 && target != null)
                    {
                        handles = ReactiveUITK.Core.Animation.Animator.PlayTracks(target, tracks);
                    }
                    state.HookStates[index] = handles;
                    SyncState(metadata, state);
                    return () =>
                    {
                        var hs =
                            state.HookStates[index]
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
                            state.HookStates[index] = null;
                            SyncState(metadata, state);
                        }
                    };
                },
                dependencies
            );
        }

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
            NodeMetadata metadata = HookContext.Current?.Owner;
            var state = EnsureState(metadata);
            if (state == null)
            {
                return;
            }
            RecordHook(metadata, state, HookIdTween);
            state.HookStates ??= new System.Collections.Generic.List<object>();
            if (state.HookIndex >= state.HookStates.Count)
            {
                state.HookStates.Add(null);
            }
            int index = state.HookIndex;
            state.HookIndex++;
            SyncState(metadata, state);

            UseEffect(
                () =>
                {
                    UnityEngine.UIElements.IVisualElementScheduledItem item = null;
                    double start = 0;
                    bool started = false;
                    var target = ResolveAnimationTarget(metadata, state);
                    if (target == null)
                    {
                        return null;
                    }
                    item = target
                        .schedule.Execute(() =>
                        {
                            if (target.panel == null)
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
            NodeMetadata metadata = HookContext.Current?.Owner;
            var state = EnsureState(metadata);
            if (state == null || string.IsNullOrEmpty(key))
            {
                return default;
            }

            RecordHook(metadata, state, HookIdContext);

            // Context usage makes bailout unsafe
            if (state.Fiber != null)
            {
                state.Fiber.ReadsContext = true;
            }

            // Resolve context value
            object resolved = default;

            // Fiber path: walk the Fiber tree's provided context
            var fiber = state.Fiber;
            while (fiber != null)
            {
                if (
                    fiber.ProvidedContext != null
                    && fiber.ProvidedContext.TryGetValue(key, out var fiberVal)
                )
                {
                    resolved = fiberVal;
                    break;
                }
                fiber = fiber.Parent;
            }

            if (resolved == null || Equals(resolved, default(object)))
            {
                // Fallback to HostContext environment
                if (
                    metadata?.HostContext?.Environment != null
                    && metadata.HostContext.Environment.TryGetValue(key, out var envVal)
                )
                {
                    resolved = envVal;
                }
            }

            state.HookStates ??= new List<object>();
            if (state.HookIndex >= state.HookStates.Count)
            {
                state.HookStates.Add(default(T));
            }
            if (resolved is T typed)
            {
                state.HookStates[state.HookIndex] = typed;
                state.HookIndex++;
                metadata?.SyncComponentState(state);
                return typed;
            }
            state.HookStates[state.HookIndex] = default(T);
            state.HookIndex++;
            metadata?.SyncComponentState(state);
            return default;
        }



        public static T UseSignal<T>(Signal<T> signal)
        {
            return UseSignal(signal, static value => value, EqualityComparer<T>.Default);
        }

        public static TSlice UseSignal<T, TSlice>(
            Signal<T> signal,
            Func<T, TSlice> selector,
            IEqualityComparer<TSlice> comparer = null
        )
        {
            if (signal == null)
            {
                return default;
            }
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }
            var boxedSelector = new Func<object, object>(raw => selector((T)raw));
            var boxedComparer = new BoxedEqualityComparer<TSlice>(
                comparer ?? EqualityComparer<TSlice>.Default
            );
            return (TSlice)UseSignalInternal(signal, boxedSelector, boxedComparer);
        }

        public static T UseSignal<T>(string key, T initialValue = default)
        {
            return UseSignal(ReactiveUITK.Signals.Signals.Get<T>(key, initialValue));
        }

        public static TSlice UseSignal<T, TSlice>(
            string key,
            Func<T, TSlice> selector,
            IEqualityComparer<TSlice> comparer = null,
            T initialValue = default
        )
        {
            return UseSignal(
                ReactiveUITK.Signals.Signals.Get<T>(key, initialValue),
                selector,
                comparer
            );
        }

        public static void ProvideContext<T>(string key, T value) =>
            ProvideContext(key, (object)value);

        public static void ProvideContext(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            // Fiber path: attach provided context to the current function component state / fiber
            var state = HookContext.Current;
            var fiber = state?.Fiber;
            if (fiber == null)
            {
                return;
            }

            fiber.ProvidedContext ??= new Dictionary<string, object>();
            fiber.ProvidedContext[key] = value;
        }

        /// <summary>
        /// Suspend the current Fiber function component until the given task
        /// completes, using Suspense. If the task is already completed, this
        /// is a no-op. Otherwise it schedules a re-render and throws a
        /// FiberSuspenseSuspendException to abort the current render pass.
        /// </summary>
        public static void SuspendUntil(Task task)
        {
            if (task == null || task.IsCompleted)
            {
                return;
            }

            var state = HookContext.Current;
            if (state != null)
            {
                state.SuspensePendingTask = task;
                state.OnStateUpdated?.Invoke();
            }

            throw new FiberSuspenseSuspendException(task);
        }

        public static void FlushSync(Action action)
        {
            if (action == null)
            {
                return;
            }

            // Prefer the current hook context's scheduler when available.
            var state = HookContext.Current;
            IScheduler scheduler = ResolveScheduler(state?.Owner);

            if (scheduler != null)
            {
                scheduler.BeginBatch();
                try
                {
                    action();
                    scheduler.PumpNow();
                }
                finally
                {
                    scheduler.EndBatch();
                }
            }
            else
            {
                action();
            }
        }

        public static void FlushSync()
        {
            var state = HookContext.Current;
            IScheduler scheduler = ResolveScheduler(state?.Owner);
            scheduler?.PumpNow();
        }

        private static readonly Func<object, object> IdentitySelector = value => value;

        private static object UseSignalInternal(
            SignalBase signal,
            Func<object, object> selector,
            IEqualityComparer<object> comparer
        )
        {
            selector ??= IdentitySelector;
            comparer ??= EqualityComparer<object>.Default;

            NodeMetadata metadata = HookContext.Current?.Owner;
            FunctionComponentState state = HookContext.Current ?? metadata?.EnsureComponentState();
            if (state == null)
            {
                return selector(signal?.UntypedValue);
            }
            RecordHook(metadata, state, HookIdSignal);
            state.HookStates ??= new List<object>();
            SignalSubscriptionState slot;
            if (
                state.HookIndex < state.HookStates.Count
                && state.HookStates[state.HookIndex] is SignalSubscriptionState existing
            )
            {
                slot = existing;
            }
            else
            {
                slot = new SignalSubscriptionState(metadata, state);
                if (state.HookIndex < state.HookStates.Count)
                {
                    state.HookStates[state.HookIndex] = slot;
                }
                else
                {
                    state.HookStates.Add(slot);
                }
            }
            object latest = slot.Bind(signal, selector, comparer);
            state.HookIndex++;
            SyncState(metadata, state);
            return latest;
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

        private sealed class SignalSubscriptionState : IDisposable
        {
            private readonly NodeMetadata owner;
            private readonly FunctionComponentState state;
            private SignalBase signal;
            private IDisposable subscription;
            private Func<object, object> selector = IdentitySelector;
            private IEqualityComparer<object> comparer = EqualityComparer<object>.Default;
            private object lastValue;
            private bool initialized;

            internal SignalSubscriptionState(NodeMetadata owner, FunctionComponentState state)
            {
                this.owner = owner;
                this.state = state;
            }

            public object Bind(
                SignalBase nextSignal,
                Func<object, object> nextSelector,
                IEqualityComparer<object> nextComparer
            )
            {
                bool signalChanged = !ReferenceEquals(signal, nextSignal);
                Func<object, object> resolvedSelector = nextSelector ?? IdentitySelector;
                IEqualityComparer<object> resolvedComparer =
                    nextComparer ?? EqualityComparer<object>.Default;
                bool selectorChanged =
                    !ReferenceEquals(selector, resolvedSelector)
                    || !ReferenceEquals(comparer, resolvedComparer);

                selector = resolvedSelector;
                comparer = resolvedComparer;

                if (signalChanged)
                {
                    DisposeSubscription();
                    signal = nextSignal;
                    if (signal != null)
                    {
                        subscription = signal.SubscribeRaw(OnSignalChanged);
                    }
                    initialized = false;
                }

                if (!initialized || selectorChanged)
                {
                    lastValue = selector(signal?.UntypedValue);
                    initialized = true;
                }

                return lastValue;
            }

            private void OnSignalChanged(object rawValue)
            {
                var next = selector(rawValue);
                if (comparer.Equals(next, lastValue))
                {
                    return;
                }
                lastValue = next;

                // Fiber path: prefer FunctionComponentState.OnStateUpdated when available.
                if (state?.OnStateUpdated != null)
                {
                    try
                    {
                        state.OnStateUpdated.Invoke();
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            Debug.LogException(ex);
                        }
                        catch { }
                    }
                    return;
                }

                // Legacy path: fall back to metadata-based rerender.
                RequestComponentRerender(owner);
            }

            private void DisposeSubscription()
            {
                subscription?.Dispose();
                subscription = null;
            }

            public void Dispose()
            {
                DisposeSubscription();
                signal = null;
                initialized = false;
            }
        }

        private sealed class BoxedEqualityComparer<T> : IEqualityComparer<object>
        {
            private readonly IEqualityComparer<T> comparer;

            public BoxedEqualityComparer(IEqualityComparer<T> comparer)
            {
                this.comparer = comparer ?? EqualityComparer<T>.Default;
            }

            public new bool Equals(object x, object y)
            {
                if (x is T tx && y is T ty)
                {
                    return comparer.Equals(tx, ty);
                }
                return ReferenceEquals(x, y);
            }

            public int GetHashCode(object obj)
            {
                if (obj is T t)
                {
                    return comparer.GetHashCode(t);
                }
                return obj?.GetHashCode() ?? 0;
            }
        }

        internal static void DisposeSignalSubscriptions(NodeMetadata metadata)
        {
            if (metadata == null)
            {
                return;
            }
            DisposeSignalSubscriptions(metadata.ComponentState);
        }

        internal static void DisposeSignalSubscriptions(FunctionComponentState state)
        {
            if (state?.HookStates == null)
            {
                return;
            }
            foreach (var hookState in state.HookStates)
            {
                if (hookState is SignalSubscriptionState subscription)
                {
                    subscription.Dispose();
                }
            }
        }
    }
}
