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
                RegisterIfAllowed(registry, "Label");
                RegisterIfAllowed(registry, "GroupBox");
                RegisterIfAllowed(registry, "Toggle");
                RegisterIfAllowed(registry, "RadioButton");
                RegisterIfAllowed(registry, "RadioButtonGroup");
                RegisterIfAllowed(registry, "ProgressBar");
                RegisterIfAllowed(registry, "RepeatButton");
                RegisterIfAllowed(registry, "Image");
                RegisterIfAllowed(registry, "ScrollView");
                RegisterIfAllowed(registry, "Slider");
                RegisterIfAllowed(registry, "DropdownField");
                RegisterIfAllowed(registry, "Foldout");
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
            RegisterIfAllowed(registry, "Label", allowedSet);
            RegisterIfAllowed(registry, "GroupBox", allowedSet);
            RegisterIfAllowed(registry, "Toggle", allowedSet);
            RegisterIfAllowed(registry, "RadioButton", allowedSet);
            RegisterIfAllowed(registry, "RadioButtonGroup", allowedSet);
            RegisterIfAllowed(registry, "ProgressBar", allowedSet);
            RegisterIfAllowed(registry, "RepeatButton", allowedSet);
            RegisterIfAllowed(registry, "Image", allowedSet);
            RegisterIfAllowed(registry, "ScrollView", allowedSet);
            RegisterIfAllowed(registry, "Slider", allowedSet);
            RegisterIfAllowed(registry, "DropdownField", allowedSet);
            RegisterIfAllowed(registry, "Foldout", allowedSet);
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
                    case "Label":
                        registry.Register("Label", new LabelElementAdapter());
                        break;
                    case "GroupBox":
                        registry.Register("GroupBox", new GroupBoxElementAdapter());
                        break;
                    case "Toggle":
                        registry.Register("Toggle", new ToggleElementAdapter());
                        break;
                    case "RadioButton":
                        registry.Register("RadioButton", new RadioButtonElementAdapter());
                        break;
                    case "RadioButtonGroup":
                        registry.Register("RadioButtonGroup", new RadioButtonGroupElementAdapter());
                        break;
                    case "ProgressBar":
                        registry.Register("ProgressBar", new ProgressBarElementAdapter());
                        break;
                    case "RepeatButton":
                        registry.Register("RepeatButton", new RepeatButtonElementAdapter());
                        break;
                    case "Image":
                        registry.Register("Image", new ImageElementAdapter());
                        break;
                    case "ScrollView":
                        registry.Register("ScrollView", new ScrollViewElementAdapter());
                        break;
                    case "Slider":
                        registry.Register("Slider", new SliderElementAdapter());
                        break;
                    case "DropdownField":
                        registry.Register("DropdownField", new DropdownFieldElementAdapter());
                        break;
                    case "Foldout":
                        registry.Register("Foldout", new FoldoutElementAdapter());
                        break;
                }
            }
        }
    }
}
