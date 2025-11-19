using System;
using System.Collections.Generic;
using ReactiveUITK.Elements;
using UnityEngine.UIElements;

namespace ReactiveUITK.Core
{
    internal readonly struct ContextKey : IEquatable<ContextKey>
    {
        public ContextKey(string name, int providerId)
        {
            Name = name ?? string.Empty;
            ProviderId = providerId;
        }

        public string Name { get; }
        public int ProviderId { get; }

        public bool Equals(ContextKey other) =>
            ProviderId == other.ProviderId
            && string.Equals(Name, other.Name, StringComparison.Ordinal);

        public override bool Equals(object obj) => obj is ContextKey other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return ((ProviderId * 397) ^ (Name?.GetHashCode() ?? 0));
            }
        }

        public override string ToString() => $"{Name}@{ProviderId}";
    }

    public sealed class HostContext
    {
        internal sealed class ContextFrame
        {
            public ContextFrame Parent;
            public IReadOnlyDictionary<string, object> Values;
            public int ProviderId;
        }

        internal readonly struct ContextFrameHandle : IEquatable<ContextFrameHandle>
        {
            internal ContextFrameHandle(ContextFrame frame)
            {
                Frame = frame;
            }

            internal ContextFrame Frame { get; }
            public bool IsValid => Frame != null;

            public bool Equals(ContextFrameHandle other) => ReferenceEquals(Frame, other.Frame);

            public override bool Equals(object obj) =>
                obj is ContextFrameHandle handle && Equals(handle);

            public override int GetHashCode() => Frame != null ? Frame.GetHashCode() : 0;
        }

        internal ContextFrameHandle CaptureFrame() => new ContextFrameHandle(currentFrame);

        internal void RestoreFrame(ContextFrameHandle handle)
        {
            currentFrame = handle.Frame;
        }

        public ElementRegistry ElementRegistry { get; }
        public Dictionary<string, object> Environment { get; } = new();

        private ContextFrame currentFrame;
        private int nextProviderId = 1;
        private readonly Dictionary<ContextKey, int> contextVersions = new();
        private readonly Dictionary<ContextKey, HashSet<NodeMetadata>> contextSubscribers = new();

        public HostContext(ElementRegistry elementRegistry)
        {
            ElementRegistry = elementRegistry;
            currentFrame = null;
        }

        public void SetContextValue(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }
            Environment[key] = value;
            var contextKey = new ContextKey(key, 0);
            int version = IncrementContextVersion(contextKey);
            NotifyContextSubscribers(contextKey, version);
        }

        internal ContextFrameHandle PushProvider(
            IReadOnlyDictionary<string, object> values,
            ref int providerId
        )
        {
            if (values == null || values.Count == 0)
            {
                return default;
            }
            if (providerId <= 0)
            {
                providerId = nextProviderId++;
            }
            var frame = new ContextFrame
            {
                Parent = currentFrame,
                Values = values,
                ProviderId = providerId,
            };
            currentFrame = frame;
            return new ContextFrameHandle(frame);
        }

        internal void PopProvider(ContextFrameHandle handle)
        {
            if (!handle.IsValid)
            {
                return;
            }
            if (currentFrame == handle.Frame)
            {
                currentFrame = handle.Frame.Parent;
                return;
            }
            
            var cursor = currentFrame;
            while (cursor != null && cursor != handle.Frame)
            {
                cursor = cursor.Parent;
            }
            if (cursor == handle.Frame)
            {
                currentFrame = handle.Frame.Parent;
            }
        }

        public object ResolveContext(string key)
        {
            return ResolveContext(key, out _, out _);
        }

        public object ResolveContext(string key, out int version, out int providerId)
        {
            var frame = currentFrame;
            while (frame != null)
            {
                if (frame.Values != null && frame.Values.TryGetValue(key, out object provided))
                {
                    providerId = frame.ProviderId;
                    version = GetContextVersion(new ContextKey(key, providerId));
                    return provided;
                }
                frame = frame.Parent;
            }
            providerId = 0;
            Environment.TryGetValue(key, out object globalValue);
            version = GetContextVersion(new ContextKey(key, providerId));
            return globalValue;
        }

        internal void RegisterContextConsumer(NodeMetadata metadata, string key, int providerId)
        {
            if (metadata == null || string.IsNullOrEmpty(key))
            {
                return;
            }

            var contextKey = new ContextKey(key, providerId);
            var state = metadata.ComponentState ?? metadata.EnsureComponentState();
            if (state == null)
            {
                return;
            }
            RemoveStaleContextSubscriptions(metadata, state, key, contextKey);
            state.SubscribedContextKeys ??= new HashSet<ContextKey>();
            state.SubscribedContextKeys.Add(contextKey);
            state.ContextVersions ??= new Dictionary<ContextKey, int>();
            state.ContextVersions[contextKey] = GetContextVersion(contextKey);
            metadata.SyncComponentState(state);

            if (!contextSubscribers.TryGetValue(contextKey, out HashSet<NodeMetadata> subscribers))
            {
                subscribers = new HashSet<NodeMetadata>();
                contextSubscribers[contextKey] = subscribers;
            }
            subscribers.Add(metadata);
        }

        internal void UnregisterContextConsumer(NodeMetadata metadata)
        {
            var state = metadata?.ComponentState ?? metadata?.EnsureComponentState();
            if (state?.SubscribedContextKeys == null)
            {
                return;
            }

            foreach (ContextKey key in state.SubscribedContextKeys)
            {
                if (contextSubscribers.TryGetValue(key, out HashSet<NodeMetadata> subscribers))
                {
                    subscribers.Remove(metadata);
                    if (subscribers.Count == 0)
                    {
                        contextSubscribers.Remove(key);
                    }
                }
            }

            state.SubscribedContextKeys.Clear();
            state.ContextVersions?.Clear();
            metadata?.SyncComponentState(state);
        }

        internal void NotifyContextChanged(
            int providerId,
            IReadOnlyDictionary<string, object> values
        )
        {
            if (providerId <= 0 || values == null || values.Count == 0)
            {
                return;
            }
            foreach (var kv in values)
            {
                var key = new ContextKey(kv.Key, providerId);
                int version = IncrementContextVersion(key);
                NotifyContextSubscribers(key, version);
            }
        }

        private void RemoveStaleContextSubscriptions(
            NodeMetadata metadata,
            FunctionComponentState state,
            string keyName,
            ContextKey incomingKey
        )
        {
            if (
                metadata == null
                || state?.SubscribedContextKeys == null
                || state.SubscribedContextKeys.Count == 0
            )
            {
                return;
            }
            List<ContextKey> removals = null;
            foreach (var existing in state.SubscribedContextKeys)
            {
                if (
                    string.Equals(existing.Name, keyName, StringComparison.Ordinal)
                    && existing.ProviderId != incomingKey.ProviderId
                )
                {
                    removals ??= new List<ContextKey>();
                    removals.Add(existing);
                }
            }
            if (removals == null)
            {
                return;
            }
            foreach (var removeKey in removals)
            {
                if (contextSubscribers.TryGetValue(removeKey, out var subscribers))
                {
                    subscribers.Remove(metadata);
                    if (subscribers.Count == 0)
                    {
                        contextSubscribers.Remove(removeKey);
                    }
                }
                state.SubscribedContextKeys.Remove(removeKey);
                state.ContextVersions?.Remove(removeKey);
            }
            metadata.SyncComponentState(state);
        }

        private int IncrementContextVersion(ContextKey key)
        {
            if (!contextVersions.TryGetValue(key, out int version))
            {
                version = 0;
            }
            version++;
            contextVersions[key] = version;
            return version;
        }

        private int GetContextVersion(ContextKey key)
        {
            return contextVersions.TryGetValue(key, out int version) ? version : 0;
        }

        private void NotifyContextSubscribers(ContextKey key, int version)
        {
            if (!contextSubscribers.TryGetValue(key, out HashSet<NodeMetadata> subscribers))
            {
                return;
            }

            if (subscribers.Count == 0)
            {
                return;
            }

            var snapshot = new List<NodeMetadata>(subscribers);
            foreach (NodeMetadata metadata in snapshot)
            {
                if (metadata == null)
                {
                    continue;
                }
                var state = metadata.ComponentState ?? metadata.EnsureComponentState();
                if (state == null)
                {
                    continue;
                }
                state.ContextVersions ??= new Dictionary<ContextKey, int>();

                if (
                    state.ContextVersions.TryGetValue(key, out int recordedVersion)
                    && recordedVersion == version
                )
                {
                    continue;
                }

                state.ContextVersions[key] = version;
                metadata.SyncComponentState(state);

                if (metadata.Container == null)
                {
                    continue;
                }

                try
                {
                    metadata.Reconciler?.ForceFunctionComponentUpdate(metadata);
                    FrameBatcher.Enqueue(metadata);
                }
                catch
                {
                }
            }
        }
    }
}
