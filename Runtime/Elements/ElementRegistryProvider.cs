using System.Collections.Generic;

namespace ReactiveUITK.Elements
{
    public static class ElementRegistryProvider
    {
        private static ElementRegistry defaultRegistry;

        public static ElementRegistry GetDefaultRegistry()
        {
            if (defaultRegistry == null)
            {
                ElementRegistry registry = new();
                RegisterIfAllowed(registry, "VisualElement");
                RegisterIfAllowed(registry, "Button");
                RegisterIfAllowed(registry, "TextField");
                RegisterIfAllowed(registry, "ListView");
                defaultRegistry = registry;
            }
            return defaultRegistry;
        }

        public static ElementRegistry CreateFilteredRegistry(IEnumerable<string> allowed)
        {
            HashSet<string> allowedSet = new(allowed ?? System.Array.Empty<string>());
            ElementRegistry registry = new();
            RegisterIfAllowed(registry, "VisualElement", allowedSet);
            RegisterIfAllowed(registry, "Button", allowedSet);
            RegisterIfAllowed(registry, "TextField", allowedSet);
            RegisterIfAllowed(registry, "ListView", allowedSet);
            return registry;
        }

        private static void RegisterIfAllowed(ElementRegistry registry, string elementTypeName, HashSet<string> allowedSet = null)
        {
            bool isAllowed = allowedSet == null || allowedSet.Contains(elementTypeName);
            if (isAllowed)
            {
                switch (elementTypeName)
                {
                    case "VisualElement":
                        registry.Register("VisualElement", new VisualElementAdapter());
                        break;
                    case "Button":
                        registry.Register("Button", new ButtonElementAdapter());
                        break;
                    case "TextField":
                        registry.Register("TextField", new TextFieldElementAdapter());
                        break;
                    case "ListView":
                        registry.Register("ListView", new ListViewElementAdapter());
                        break;
                }
            }
        }
    }
}
