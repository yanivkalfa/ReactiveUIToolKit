using System;
using System.Collections.Generic;
using UnityEngine;

namespace ReactiveUITK.Signals
{
    public sealed class SignalRegistry
    {
        private readonly Dictionary<string, SignalBase> signals = new(StringComparer.Ordinal);
        private readonly object gate = new();

        public Signal<T> GetOrCreate<T>(string key, T initialValue = default)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Signal key must be non-empty", nameof(key));
            }
            lock (gate)
            {
                if (signals.TryGetValue(key, out SignalBase existing))
                {
                    if (existing is Signal<T> typed)
                    {
                        return typed;
                    }
                    throw new InvalidOperationException(
                        $"Signal '{key}' already exists with type {existing.ValueType.Name}."
                    );
                }
                var created = new Signal<T>(key, initialValue);
                signals[key] = created;
                return created;
            }
        }

        internal bool TryGet<T>(string key, out Signal<T> signal)
        {
            lock (gate)
            {
                if (
                    signals.TryGetValue(key, out SignalBase existing) && existing is Signal<T> typed
                )
                {
                    signal = typed;
                    return true;
                }
            }
            signal = null;
            return false;
        }
    }

    public static class SignalFactory
    {
        public static Signal<T> Get<T>(string key, T initialValue = default)
        {
            SignalsRuntime.EnsureInitialized();
            return SignalsRuntime.Registry.GetOrCreate(key, initialValue);
        }

        public static bool TryGet<T>(string key, out Signal<T> signal)
        {
            SignalsRuntime.EnsureInitialized();
            return SignalsRuntime.Registry.TryGet(key, out signal);
        }
    }

    public static class SignalsRuntime
    {
        private static SignalRegistry registry;
        private static bool hostCreated;

        public static SignalRegistry Registry => registry ??= InitializeRegistry();

        public static void EnsureInitialized()
        {
            _ = Registry;
        }

        private static SignalRegistry InitializeRegistry()
        {
            var reg = new SignalRegistry();
            CreateRuntimeHost();
            return reg;
        }

        private static void CreateRuntimeHost()
        {
            if (hostCreated)
            {
                return;
            }
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                hostCreated = true;
                return;
            }
#endif
            var go = new GameObject("__ReactiveSignalsRuntime");
            go.hideFlags = HideFlags.HideAndDontSave;
            UnityEngine.Object.DontDestroyOnLoad(go);
            go.AddComponent<SignalsRuntimeHost>();
            hostCreated = true;
        }
    }

    internal sealed class SignalsRuntimeHost : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
