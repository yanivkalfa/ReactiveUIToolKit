using ReactiveUITK.Elements;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System.Linq;

namespace ReactiveUITK.Core
{
    public sealed class HostContext
    {
        public ElementRegistry ElementRegistry { get; }
        public Dictionary<string, object> Environment { get; } = new();
        private readonly Dictionary<string, HashSet<IReactiveComponent>> contextSubscribersByKey = new();
        private readonly Stack<Dictionary<string, object>> contextProviderStack = new();

        public HostContext(ElementRegistry elementRegistry)
        {
            ElementRegistry = elementRegistry;
        }

        public void Subscribe(string key, IReactiveComponent reactiveComponent)
        {
            if (string.IsNullOrEmpty(key) || reactiveComponent == null)
            {
                return;
            }
            if (!contextSubscribersByKey.TryGetValue(key, out HashSet<IReactiveComponent> subscribers))
            {
                subscribers = new HashSet<IReactiveComponent>();
                contextSubscribersByKey[key] = subscribers;
            }
            subscribers.Add(reactiveComponent);
        }

        public void UnsubscribeAll(IReactiveComponent reactiveComponent)
        {
            if (reactiveComponent == null)
            {
                return;
            }
            foreach (var entry in contextSubscribersByKey)
            {
                entry.Value.Remove(reactiveComponent);
            }
        }

        public void SetContextValue(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }
            if (contextProviderStack.Count > 0)
            {
                contextProviderStack.Peek()[key] = value;
            }
            Environment[key] = value;
            if (contextSubscribersByKey.TryGetValue(key, out HashSet<IReactiveComponent> subscribers))
            {
                List<IReactiveComponent> snapshot = subscribers.ToList();
                foreach (IReactiveComponent component in snapshot)
                {
                    component?.NotifyContextKeyChanged(key);
                }
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
            foreach (Dictionary<string, object> providerValues in contextProviderStack)
            {
                if (providerValues.TryGetValue(key, out object provided))
                {
                    return provided;
                }
            }
            Environment.TryGetValue(key, out object globalValue);
            return globalValue;
        }
    }
}
