using System.Collections.Generic;
using ReactiveUITK.Elements;
using UnityEngine.UIElements;

namespace ReactiveUITK.Core
{
    public sealed class HostContext
    {
        public ElementRegistry ElementRegistry { get; }
        public Dictionary<string, object> Environment { get; } = new();
        private readonly Stack<Dictionary<string, object>> contextProviderStack = new();
        private readonly Dictionary<string, int> contextVersions = new();
        private readonly Dictionary<string, HashSet<NodeMetadata>> contextSubscribers = new();

        public HostContext(ElementRegistry elementRegistry)
        {
            ElementRegistry = elementRegistry;
        }

        public void SetContextValue(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }
            object previous = ResolveContext(key);

            if (contextProviderStack.Count > 0)
            {
                contextProviderStack.Peek()[key] = value;
            }

            Environment[key] = value;

            if (!Equals(previous, value))
            {
                int version = IncrementContextVersion(key);
                NotifyContextSubscribers(key, version);
            }
        }

        public void PushProvider(Dictionary<string, object> values)
        {
            contextProviderStack.Push(values ?? new Dictionary<string, object>());
        }

        public void PopProvider()
        {
            if (contextProviderStack.Count > 0)
            {
                contextProviderStack.Pop();
            }
        }

        public object ResolveContext(string key)
        {
            return ResolveContext(key, out _);
        }

        public object ResolveContext(string key, out int version)
        {
            foreach (Dictionary<string, object> providerValues in contextProviderStack)
            {
                if (providerValues.TryGetValue(key, out object provided))
                {
                    version = GetContextVersion(key);
                    return provided;
                }
            }
            Environment.TryGetValue(key, out object globalValue);
            version = GetContextVersion(key);
            return globalValue;
        }

        internal void RegisterContextConsumer(NodeMetadata metadata, string key)
        {
            if (metadata == null || string.IsNullOrEmpty(key))
            {
                return;
            }

            metadata.SubscribedContextKeys ??= new HashSet<string>();
            metadata.SubscribedContextKeys.Add(key);
            metadata.ContextVersions ??= new Dictionary<string, int>();
            metadata.ContextVersions[key] = GetContextVersion(key);

            if (!contextSubscribers.TryGetValue(key, out HashSet<NodeMetadata> subscribers))
            {
                subscribers = new HashSet<NodeMetadata>();
                contextSubscribers[key] = subscribers;
            }
            subscribers.Add(metadata);
        }

        internal void UnregisterContextConsumer(NodeMetadata metadata)
        {
            if (metadata?.SubscribedContextKeys == null)
            {
                return;
            }

            foreach (string key in metadata.SubscribedContextKeys)
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

            metadata.SubscribedContextKeys.Clear();
        }

        private int IncrementContextVersion(string key)
        {
            if (!contextVersions.TryGetValue(key, out int version))
            {
                version = 0;
            }
            version++;
            contextVersions[key] = version;
            return version;
        }

        private int GetContextVersion(string key)
        {
            return contextVersions.TryGetValue(key, out int version) ? version : 0;
        }

        private void NotifyContextSubscribers(string key, int version)
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
                if (metadata.ContextVersions == null)
                {
                    metadata.ContextVersions = new Dictionary<string, int>();
                }

                if (
                    metadata.ContextVersions.TryGetValue(key, out int recordedVersion)
                    && recordedVersion == version
                )
                {
                    continue;
                }

                metadata.ContextVersions[key] = version;

                if (metadata.Container == null)
                {
                    continue;
                }

                try
                {
                    FrameBatcher.Enqueue(metadata);
                }
                catch { }
            }
        }
    }
}
