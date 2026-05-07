using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class ToolbarProps : BaseProps
    {
        public override bool ShallowEquals(BaseProps other)
        {
            return base.ShallowEquals(other) && other is ToolbarProps;
        }

        public override Dictionary<string, object> ToDictionary() => base.ToDictionary();

        internal override void __ResetFields() { }

        internal override void __ReturnToPool()
        {
            Pool<ToolbarProps>.Return(this);
        }
    }

    public sealed class ToolbarButtonProps : BaseProps
    {
        public string Text { get; set; }

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not ToolbarButtonProps o)
                return false;
            if (Text != o.Text)
                return false;
            return true;
        }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (!string.IsNullOrEmpty(Text))
                map["text"] = Text;
            return map;
        }

        internal override void __ResetFields()
        {
            Text = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<ToolbarButtonProps>.Return(this);
        }
    }

    public sealed class ToolbarToggleProps : BaseProps
    {
        public string Text { get; set; }
        public bool? Value { get; set; }
        public ChangeEventHandler<bool> OnChange { get; set; }
        public ChangeEventHandler<bool> OnChangeCapture { get; set; }

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not ToolbarToggleProps o)
                return false;
            if (Text != o.Text)
                return false;
            if (Value != o.Value)
                return false;
            if (OnChange != o.OnChange)
                return false;
            if (OnChangeCapture != o.OnChangeCapture)
                return false;
            return true;
        }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (!string.IsNullOrEmpty(Text))
                map["text"] = Text;
            if (Value.HasValue)
                map["value"] = Value.Value;
            if (OnChange != null)
                map["onChange"] = OnChange;
            if (OnChangeCapture != null)
                map["onChangeCapture"] = OnChangeCapture;
            return map;
        }

        internal override void __ResetFields()
        {
            Text = null;
            Value = null;
            OnChange = null;
            OnChangeCapture = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<ToolbarToggleProps>.Return(this);
        }
    }

    public sealed class ToolbarMenuProps : BaseProps
    {
        public string Text { get; set; }
        public MenuBuilderHandler PopulateMenu { get; set; }

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not ToolbarMenuProps o)
                return false;
            if (Text != o.Text)
                return false;
            if (PopulateMenu != o.PopulateMenu)
                return false;
            return true;
        }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (!string.IsNullOrEmpty(Text))
                map["text"] = Text;
            if (PopulateMenu != null)
                map["populateMenu"] = PopulateMenu;
            return map;
        }

        internal override void __ResetFields()
        {
            Text = null;
            PopulateMenu = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<ToolbarMenuProps>.Return(this);
        }
    }

    public sealed class ToolbarBreadcrumbsProps : BaseProps
    {
        public IEnumerable<string> Items { get; set; }
        public Action<int> OnItem { get; set; }

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not ToolbarBreadcrumbsProps o)
                return false;
            if (!ReferenceEquals(Items, o.Items))
                return false;
            if (OnItem != o.OnItem)
                return false;
            return true;
        }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (Items != null)
                map["items"] = Items;
            if (OnItem != null)
                map["onItem"] = OnItem;
            return map;
        }

        internal override void __ResetFields()
        {
            Items = null;
            OnItem = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<ToolbarBreadcrumbsProps>.Return(this);
        }
    }

    public sealed class ToolbarPopupSearchFieldProps : BaseProps
    {
        public string Value { get; set; }
        public ChangeEventHandler<string> OnChange { get; set; }
        public ChangeEventHandler<string> OnChangeCapture { get; set; }

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not ToolbarPopupSearchFieldProps o)
                return false;
            if (Value != o.Value)
                return false;
            if (OnChange != o.OnChange)
                return false;
            if (OnChangeCapture != o.OnChangeCapture)
                return false;
            return true;
        }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (Value != null)
                map["value"] = Value;
            if (OnChange != null)
                map["onChange"] = OnChange;
            if (OnChangeCapture != null)
                map["onChangeCapture"] = OnChangeCapture;
            return map;
        }

        internal override void __ResetFields()
        {
            Value = null;
            OnChange = null;
            OnChangeCapture = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<ToolbarPopupSearchFieldProps>.Return(this);
        }
    }

    public sealed class ToolbarSearchFieldProps : BaseProps
    {
        public string Value { get; set; }
        public ChangeEventHandler<string> OnChange { get; set; }
        public ChangeEventHandler<string> OnChangeCapture { get; set; }

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not ToolbarSearchFieldProps o)
                return false;
            if (Value != o.Value)
                return false;
            if (OnChange != o.OnChange)
                return false;
            if (OnChangeCapture != o.OnChangeCapture)
                return false;
            return true;
        }

        public override Dictionary<string, object> ToDictionary()
        {
            var map = base.ToDictionary();
            if (Value != null)
                map["value"] = Value;
            if (OnChange != null)
                map["onChange"] = OnChange;
            if (OnChangeCapture != null)
                map["onChangeCapture"] = OnChangeCapture;
            return map;
        }

        internal override void __ResetFields()
        {
            Value = null;
            OnChange = null;
            OnChangeCapture = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<ToolbarSearchFieldProps>.Return(this);
        }
    }

    public sealed class ToolbarSpacerProps : BaseProps
    {
        public override bool ShallowEquals(BaseProps other)
        {
            return base.ShallowEquals(other) && other is ToolbarSpacerProps;
        }

        public override Dictionary<string, object> ToDictionary() => base.ToDictionary();

        internal override void __ResetFields() { }

        internal override void __ReturnToPool()
        {
            Pool<ToolbarSpacerProps>.Return(this);
        }
    }
}
