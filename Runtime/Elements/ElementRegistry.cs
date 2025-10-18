using System.Collections.Generic;

namespace ReactiveUITK.Elements
{
    public sealed class ElementRegistry
    {
        private readonly Dictionary<string, IElementAdapter> adaptersByType = new Dictionary<string, IElementAdapter>();

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

            if (adaptersByType.ContainsKey(elementTypeName) == false)
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
