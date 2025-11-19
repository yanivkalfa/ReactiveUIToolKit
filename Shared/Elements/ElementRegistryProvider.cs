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
                RegisterIfAllowed(registry, "MultiColumnListView");
                RegisterIfAllowed(registry, "TreeView");
                RegisterIfAllowed(registry, "MultiColumnTreeView");
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
                RegisterIfAllowed(registry, "SliderInt");
                RegisterIfAllowed(registry, "DropdownField");
                RegisterIfAllowed(registry, "Foldout");
                RegisterIfAllowed(registry, "Tab");
                RegisterIfAllowed(registry, "TabView");
                RegisterIfAllowed(registry, "FloatField");
                RegisterIfAllowed(registry, "IntegerField");
                RegisterIfAllowed(registry, "LongField");
                RegisterIfAllowed(registry, "DoubleField");
                RegisterIfAllowed(registry, "UnsignedIntegerField");
                RegisterIfAllowed(registry, "UnsignedLongField");
                RegisterIfAllowed(registry, "Vector2Field");
                RegisterIfAllowed(registry, "Vector3Field");
                RegisterIfAllowed(registry, "Vector4Field");
                
#if UNITY_EDITOR
                RegisterIfAllowed(registry, "ColorField");
#endif
                RegisterIfAllowed(registry, "Box");
#if UNITY_EDITOR
                RegisterIfAllowed(registry, "HelpBox");
#endif
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
            RegisterIfAllowed(registry, "MultiColumnListView", allowedSet);
            RegisterIfAllowed(registry, "TreeView", allowedSet);
            RegisterIfAllowed(registry, "MultiColumnTreeView", allowedSet);
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
            RegisterIfAllowed(registry, "SliderInt", allowedSet);
            RegisterIfAllowed(registry, "DropdownField", allowedSet);
            RegisterIfAllowed(registry, "Foldout", allowedSet);
            RegisterIfAllowed(registry, "Tab", allowedSet);
            RegisterIfAllowed(registry, "TabView", allowedSet);
            RegisterIfAllowed(registry, "FloatField", allowedSet);
            RegisterIfAllowed(registry, "IntegerField", allowedSet);
            RegisterIfAllowed(registry, "LongField", allowedSet);
            RegisterIfAllowed(registry, "DoubleField", allowedSet);
            RegisterIfAllowed(registry, "UnsignedIntegerField", allowedSet);
            RegisterIfAllowed(registry, "UnsignedLongField", allowedSet);
            RegisterIfAllowed(registry, "Vector2Field", allowedSet);
            RegisterIfAllowed(registry, "Vector3Field", allowedSet);
            RegisterIfAllowed(registry, "Vector4Field", allowedSet);
            
#if UNITY_EDITOR
            RegisterIfAllowed(registry, "ColorField", allowedSet);
#endif
            RegisterIfAllowed(registry, "Box", allowedSet);
#if UNITY_EDITOR
            RegisterIfAllowed(registry, "HelpBox", allowedSet);
#endif
            return registry;
        }

        private static void RegisterIfAllowed(
            ElementRegistry registry,
            string elementTypeName,
            HashSet<string> allowedSet = null
        )
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
                    case "MultiColumnListView":
                        registry.Register(
                            "MultiColumnListView",
                            new MultiColumnListViewElementAdapter()
                        );
                        break;
                    case "TreeView":
                        registry.Register("TreeView", new TreeViewElementAdapter());
                        break;
                    case "MultiColumnTreeView":
                        registry.Register(
                            "MultiColumnTreeView",
                            new MultiColumnTreeViewElementAdapter()
                        );
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
                    case "SliderInt":
                        registry.Register("SliderInt", new SliderIntElementAdapter());
                        break;
                    case "DropdownField":
                        registry.Register("DropdownField", new DropdownFieldElementAdapter());
                        break;
                    case "Foldout":
                        registry.Register("Foldout", new FoldoutElementAdapter());
                        break;
                    case "Tab":
                        registry.Register("Tab", new TabElementAdapter());
                        break;
                    case "TabView":
                        registry.Register("TabView", new TabViewElementAdapter());
                        break;
                    case "FloatField":
                        registry.Register("FloatField", new FloatFieldElementAdapter());
                        break;
                    case "IntegerField":
                        registry.Register("IntegerField", new IntegerFieldElementAdapter());
                        break;
                    case "LongField":
                        registry.Register("LongField", new LongFieldElementAdapter());
                        break;
                    case "DoubleField":
                        registry.Register("DoubleField", new DoubleFieldElementAdapter());
                        break;
                    case "UnsignedIntegerField":
                        registry.Register(
                            "UnsignedIntegerField",
                            new UnsignedIntegerFieldElementAdapter()
                        );
                        break;
                    case "UnsignedLongField":
                        registry.Register(
                            "UnsignedLongField",
                            new UnsignedLongFieldElementAdapter()
                        );
                        break;
                    case "Vector2Field":
                        registry.Register("Vector2Field", new Vector2FieldElementAdapter());
                        break;
                    case "Vector3Field":
                        registry.Register("Vector3Field", new Vector3FieldElementAdapter());
                        break;
                    case "Vector4Field":
                        registry.Register("Vector4Field", new Vector4FieldElementAdapter());
                        break;
                    
#if UNITY_EDITOR
                    case "ColorField":
                        registry.Register("ColorField", new ColorFieldElementAdapter());
                        break;
#endif
                    case "Box":
                        registry.Register("Box", new BoxElementAdapter());
                        break;
#if UNITY_EDITOR
                    case "HelpBox":
                        registry.Register("HelpBox", new HelpBoxElementAdapter());
                        break;
#endif
                }
            }
        }
    }
}
