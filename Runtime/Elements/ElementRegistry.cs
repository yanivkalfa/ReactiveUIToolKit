using System.Collections.Generic;

namespace ReactiveUITK.Elements
{
    public sealed class ElementRegistry
    {
        private readonly Dictionary<string, IElementAdapter> adaptersByType = new();

        public void Register(string elementTypeName, IElementAdapter adapter)
        {
            if (string.IsNullOrWhiteSpace(elementTypeName))
            {
                return;
            }
            if (adapter == null)
            {
                return;
            }
            if (!adaptersByType.ContainsKey(elementTypeName))
            {
                adaptersByType.Add(elementTypeName, adapter);
                return;
            }
            adaptersByType[elementTypeName] = adapter;
        }

        public IElementAdapter Resolve(string elementTypeName)
        {
            if (adaptersByType.TryGetValue(elementTypeName, out IElementAdapter adapter))
            {
                return adapter;
            }

            return null;
        }
    }
}
