using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class ToolbarProps : BaseProps
    {
        public override Dictionary<string, object> ToDictionary() => base.ToDictionary();
    }

    public sealed class ToolbarButtonProps : BaseProps
    {
        public string Text { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (!string.IsNullOrEmpty(Text))
                map["text"] = Text;
            return map;
        }
    }

    public sealed class ToolbarToggleProps : BaseProps
    {
        public string Text { get; set; }
        public bool? Value { get; set; }
        public Action<ChangeEvent<bool>> OnChange { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (!string.IsNullOrEmpty(Text))
                map["text"] = Text;
            if (Value.HasValue)
                map["value"] = Value.Value;
            if (OnChange != null)
                map["onChange"] = OnChange;
            return map;
        }
    }

    public sealed class ToolbarMenuProps : BaseProps
    {
        public string Text { get; set; }
        public Action<DropdownMenu> PopulateMenu { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (!string.IsNullOrEmpty(Text))
                map["text"] = Text;
            if (PopulateMenu != null)
                map["populateMenu"] = PopulateMenu;
            return map;
        }
    }

    public sealed class ToolbarBreadcrumbsProps : BaseProps
    {
        public IEnumerable<string> Items { get; set; }
        public Action<int> OnItem { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (Items != null)
                map["items"] = Items;
            if (OnItem != null)
                map["onItem"] = OnItem;
            return map;
        }
    }

    public sealed class ToolbarPopupSearchFieldProps : BaseProps
    {
        public string Value { get; set; }
        public Action<ChangeEvent<string>> OnChange { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (Value != null)
                map["value"] = Value;
            if (OnChange != null)
                map["onChange"] = OnChange;
            return map;
        }
    }

    public sealed class ToolbarSearchFieldProps : BaseProps
    {
        public string Value { get; set; }
        public Action<ChangeEvent<string>> OnChange { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (Value != null)
                map["value"] = Value;
            if (OnChange != null)
                map["onChange"] = OnChange;
            return map;
        }
    }

    public sealed class ToolbarSpacerProps : BaseProps
    {
        public override Dictionary<string, object> ToDictionary() => base.ToDictionary();
    }
}
