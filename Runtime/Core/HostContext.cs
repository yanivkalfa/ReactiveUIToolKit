using ReactiveUITK.Elements;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System.Linq;

namespace ReactiveUITK.Core
{
    public sealed class HostContext
    {
        public ElementRegistry ElementRegistry { get; }
        public Dictionary<string, object> Environment { get; } = new Dictionary<string, object>();

        private readonly Dictionary<string, HashSet<ReactiveComponent>> contextSubscribers = new Dictionary<string, HashSet<ReactiveComponent>>();
		private readonly Stack<Dictionary<string, object>> providerStack = new Stack<Dictionary<string, object>>();

        public HostContext(ElementRegistry elementRegistry)
        {
            ElementRegistry = elementRegistry;
        }

        internal void Subscribe(string key, ReactiveComponent component)
        {
            if (string.IsNullOrEmpty(key) || component == null) return;
            if (!contextSubscribers.TryGetValue(key, out var set))
            {
                set = new HashSet<ReactiveComponent>();
                contextSubscribers[key] = set;
            }
            set.Add(component);
        }

        internal void UnsubscribeAll(ReactiveComponent component)
        {
            if (component == null) return;
            foreach (var kv in contextSubscribers)
            {
                kv.Value.Remove(component);
            }
        }

        internal void SetContextValue(string key, object value)
        {
            if (string.IsNullOrEmpty(key)) return;
			if (providerStack.Count > 0)
			{
				providerStack.Peek()[key] = value;
			}
			Environment[key] = value; // global fallback
            if (contextSubscribers.TryGetValue(key, out var set))
            {
                // Copy to avoid modification during enumeration
                var list = set.ToList();
                foreach (var comp in list)
                {
					comp?.NotifyContextKeyChanged(key);
                }
            }
        }

		public void PushProvider(Dictionary<string, object> values)
		{
			providerStack.Push(values ?? new Dictionary<string, object>());
		}

		public void PopProvider()
		{
			if (providerStack.Count > 0) providerStack.Pop();
		}

		internal object ResolveContext(string key)
		{
			foreach (var dict in providerStack)
			{
				if (dict.TryGetValue(key, out var val)) return val;
			}
			Environment.TryGetValue(key, out var gval);
			return gval;
		}
    }
}
