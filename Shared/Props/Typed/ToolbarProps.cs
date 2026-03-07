using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class ToolbarProps : global::ReactiveUITK.Core.IProps
    {
        public string Name { get; set; }
        public string ClassName { get; set; }
        public Style Style { get; set; }
        public object Ref { get; set; }

        public Dictionary<string, object> ToDictionary()
        {
            var map = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(Name))
                map["name"] = Name;
            if (!string.IsNullOrEmpty(ClassName))
                map["className"] = ClassName;
            if (Style != null)
                map["style"] = Style;
            if (Ref != null)
                map["ref"] = Ref;
            return map;
        }
    }

    public sealed class ToolbarButtonProps : global::ReactiveUITK.Core.IProps
    {
        public string Name { get; set; }
        public string ClassName { get; set; }
        public string Text { get; set; }
        public Action OnClick { get; set; }
        public Style Style { get; set; }
        public object Ref { get; set; }

        public Dictionary<string, object> ToDictionary()
        {
            var map = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(Name))
                map["name"] = Name;
            if (!string.IsNullOrEmpty(ClassName))
                map["className"] = ClassName;
            if (!string.IsNullOrEmpty(Text))
                map["text"] = Text;
            if (OnClick != null)
                map["onClick"] = OnClick;
            if (Style != null)
                map["style"] = Style;
            if (Ref != null)
                map["ref"] = Ref;
            return map;
        }
    }

    public sealed class ToolbarToggleProps : global::ReactiveUITK.Core.IProps
    {
        public string Name { get; set; }
        public string ClassName { get; set; }
        public string Text { get; set; }
        public bool? Value { get; set; }
        public Action<ChangeEvent<bool>> OnChange { get; set; }
        public Style Style { get; set; }
        public object Ref { get; set; }

        public Dictionary<string, object> ToDictionary()
        {
            var map = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(Name))
                map["name"] = Name;
            if (!string.IsNullOrEmpty(ClassName))
                map["className"] = ClassName;
            if (!string.IsNullOrEmpty(Text))
                map["text"] = Text;
            if (Value.HasValue)
                map["value"] = Value.Value;
            if (OnChange != null)
                map["onChange"] = OnChange;
            if (Style != null)
                map["style"] = Style;
            if (Ref != null)
                map["ref"] = Ref;
            return map;
        }
    }

    public sealed class ToolbarMenuProps : global::ReactiveUITK.Core.IProps
    {
        public string Name { get; set; }
        public string ClassName { get; set; }
        public string Text { get; set; }
        public Action<DropdownMenu> PopulateMenu { get; set; }
        public Style Style { get; set; }
        public object Ref { get; set; }

        public Dictionary<string, object> ToDictionary()
        {
            var map = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(Name))
                map["name"] = Name;
            if (!string.IsNullOrEmpty(ClassName))
                map["className"] = ClassName;
            if (!string.IsNullOrEmpty(Text))
                map["text"] = Text;
            if (PopulateMenu != null)
                map["populateMenu"] = PopulateMenu;
            if (Style != null)
                map["style"] = Style;
            if (Ref != null)
                map["ref"] = Ref;
            return map;
        }
    }

    public sealed class ToolbarBreadcrumbsProps : global::ReactiveUITK.Core.IProps
    {
        public string Name { get; set; }
        public string ClassName { get; set; }
        public IEnumerable<string> Items { get; set; }
        public Action<int> OnItem { get; set; }
        public Style Style { get; set; }
        public object Ref { get; set; }

        public Dictionary<string, object> ToDictionary()
        {
            var map = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(Name))
                map["name"] = Name;
            if (!string.IsNullOrEmpty(ClassName))
                map["className"] = ClassName;
            if (Items != null)
                map["items"] = Items;
            if (OnItem != null)
                map["onItem"] = OnItem;
            if (Style != null)
                map["style"] = Style;
            if (Ref != null)
                map["ref"] = Ref;
            return map;
        }
    }

    public sealed class ToolbarPopupSearchFieldProps : global::ReactiveUITK.Core.IProps
    {
        public string Name { get; set; }
        public string ClassName { get; set; }
        public string Value { get; set; }
        public Action<ChangeEvent<string>> OnChange { get; set; }
        public Style Style { get; set; }
        public object Ref { get; set; }

        public Dictionary<string, object> ToDictionary()
        {
            var map = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(Name))
                map["name"] = Name;
            if (!string.IsNullOrEmpty(ClassName))
                map["className"] = ClassName;
            if (Value != null)
                map["value"] = Value;
            if (OnChange != null)
                map["onChange"] = OnChange;
            if (Style != null)
                map["style"] = Style;
            if (Ref != null)
                map["ref"] = Ref;
            return map;
        }
    }

    public sealed class ToolbarSearchFieldProps : global::ReactiveUITK.Core.IProps
    {
        public string Name { get; set; }
        public string ClassName { get; set; }
        public string Value { get; set; }
        public Action<ChangeEvent<string>> OnChange { get; set; }
        public Style Style { get; set; }
        public object Ref { get; set; }

        public Dictionary<string, object> ToDictionary()
        {
            var map = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(Name))
                map["name"] = Name;
            if (!string.IsNullOrEmpty(ClassName))
                map["className"] = ClassName;
            if (Value != null)
                map["value"] = Value;
            if (OnChange != null)
                map["onChange"] = OnChange;
            if (Style != null)
                map["style"] = Style;
            if (Ref != null)
                map["ref"] = Ref;
            return map;
        }
    }

    public sealed class ToolbarSpacerProps : global::ReactiveUITK.Core.IProps
    {
        public string Name { get; set; }
        public string ClassName { get; set; }
        public Style Style { get; set; }
        public object Ref { get; set; }

        public Dictionary<string, object> ToDictionary()
        {
            var map = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(Name))
                map["name"] = Name;
            if (!string.IsNullOrEmpty(ClassName))
                map["className"] = ClassName;
            if (Style != null)
                map["style"] = Style;
            if (Ref != null)
                map["ref"] = Ref;
            return map;
        }
    }
}
