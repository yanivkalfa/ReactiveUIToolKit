using ReactiveUITK.Elements;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Core
{
    public sealed class HostContext
    {
        public ElementRegistry ElementRegistry { get; }
        public Dictionary<string, object> Environment { get; } = new();
        private readonly Stack<Dictionary<string, object>> contextProviderStack = new();

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
            if (contextProviderStack.Count > 0)
            {
                contextProviderStack.Peek()[key] = value;
            }
            Environment[key] = value;
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
