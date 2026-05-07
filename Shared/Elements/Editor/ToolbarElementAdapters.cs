// Editor-only Toolbar family adapters
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props;
using ReactiveUITK.Props.Typed;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class ToolbarElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create()
        {
            return new Toolbar();
        }

        public override VisualElement ResolveChildHost(VisualElement element)
        {
            var tb = element as Toolbar;
            var cc = tb != null ? tb.contentContainer : element;
            return EnsureMount(cc);
        }

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            PropsApplier.ApplyDiff(element, previous, next);
        }
    }

    public sealed class ToolbarButtonElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create() => new ToolbarButton();

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (element is ToolbarButton btn && properties != null)
            {
                if (properties.TryGetValue("text", out var t) && t is string s)
                {
                    btn.text = s ?? string.Empty;
                }
            }
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is ToolbarButton btn)
            {
                string prev =
                    previous != null && previous.TryGetValue("text", out var p)
                        ? p as string
                        : null;
                string nxt =
                    next != null && next.TryGetValue("text", out var n) ? n as string : null;
                if (!string.Equals(prev, nxt, StringComparison.Ordinal))
                {
                    btn.text = nxt ?? string.Empty;
                }
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }

        public override void ApplyTypedFull(VisualElement element, BaseProps props)
        {
            if (element is ToolbarButton btn && props is ToolbarButtonProps fp)
            {
                if (fp.Text != null)
                    btn.text = fp.Text;
            }
            base.ApplyTypedFull(element, props);
        }

        public override void ApplyTypedDiff(VisualElement element, BaseProps prev, BaseProps next)
        {
            if (
                element is ToolbarButton btn
                && prev is ToolbarButtonProps fp
                && next is ToolbarButtonProps fn
            )
            {
                if (fp.Text != fn.Text)
                    btn.text = fn.Text ?? string.Empty;
            }
            base.ApplyTypedDiff(element, prev, next);
        }
    }

    public sealed class ToolbarToggleElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create() => new ToolbarToggle();

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (element is ToolbarToggle t && properties != null)
            {
                if (properties.TryGetValue("text", out var to) && to is string ts)
                {
                    t.text = ts ?? string.Empty;
                }
                if (properties.TryGetValue("value", out var v) && v is bool b)
                {
                    t.value = b;
                }
            }
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is ToolbarToggle t)
            {
                string prevText =
                    previous != null && previous.TryGetValue("text", out var p)
                        ? p as string
                        : null;
                string nextText =
                    next != null && next.TryGetValue("text", out var n) ? n as string : null;
                if (!string.Equals(prevText, nextText, StringComparison.Ordinal))
                {
                    t.text = nextText ?? string.Empty;
                }
                bool prevVal =
                    previous != null
                    && previous.TryGetValue("value", out var pv)
                    && pv is bool pb
                    && pb;
                bool nextVal =
                    next != null && next.TryGetValue("value", out var nv) && nv is bool nb && nb;
                if (prevVal != nextVal)
                {
                    t.value = nextVal;
                }
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }

        public override void ApplyTypedFull(VisualElement element, BaseProps props)
        {
            if (element is ToolbarToggle t && props is ToolbarToggleProps fp)
            {
                if (fp.Text != null)
                    t.text = fp.Text;
                if (fp.Value.HasValue)
                    t.value = fp.Value.Value;
                if (fp.OnChange != null)
                    PropsApplier.ApplySingle(element, null, "onChange", fp.OnChange);
                if (fp.OnChangeCapture != null)
                    PropsApplier.ApplySingle(element, null, "onChangeCapture", fp.OnChangeCapture);
            }
            base.ApplyTypedFull(element, props);
        }

        public override void ApplyTypedDiff(VisualElement element, BaseProps prev, BaseProps next)
        {
            if (
                element is ToolbarToggle t
                && prev is ToolbarToggleProps fp
                && next is ToolbarToggleProps fn
            )
            {
                if (fp.Text != fn.Text)
                    t.text = fn.Text ?? string.Empty;
                if (fp.Value != fn.Value && fn.Value.HasValue)
                    t.value = fn.Value.Value;
                if (fp.OnChange != fn.OnChange)
                {
                    if (fn.OnChange != null)
                        PropsApplier.ApplySingle(element, fp.OnChange, "onChange", fn.OnChange);
                    else if (fp.OnChange != null)
                        PropsApplier.RemoveProp(element, "onChange", fp.OnChange);
                }
                if (fp.OnChangeCapture != fn.OnChangeCapture)
                {
                    if (fn.OnChangeCapture != null)
                        PropsApplier.ApplySingle(
                            element,
                            fp.OnChangeCapture,
                            "onChangeCapture",
                            fn.OnChangeCapture
                        );
                    else if (fp.OnChangeCapture != null)
                        PropsApplier.RemoveProp(element, "onChangeCapture", fp.OnChangeCapture);
                }
            }
            base.ApplyTypedDiff(element, prev, next);
        }
    }

    public sealed class ToolbarMenuElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create() => new ToolbarMenu();

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (element is ToolbarMenu m && properties != null)
            {
                if (properties.TryGetValue("text", out var t) && t is string ts)
                {
                    m.text = ts ?? string.Empty;
                }
                if (
                    properties.TryGetValue("populateMenu", out var pm)
                    && pm is MenuBuilderHandler action
                )
                {
                    // ToolbarMenu.menu is read-only; populate the existing instance.
                    // Avoid repeated appends for the same callback instance.
                    if (
                        !_lastPopulate.TryGetValue(m, out var existing)
                        || !ReferenceEquals(existing, action)
                    )
                    {
                        _lastPopulate.Remove(m);
                        _lastPopulate.Add(m, action);
                        action.Invoke(m.menu);
                    }
                }
            }
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is ToolbarMenu m)
            {
                string prev =
                    previous != null && previous.TryGetValue("text", out var p)
                        ? p as string
                        : null;
                string nxt =
                    next != null && next.TryGetValue("text", out var n) ? n as string : null;
                if (!string.Equals(prev, nxt, StringComparison.Ordinal))
                {
                    m.text = nxt ?? string.Empty;
                }
                MenuBuilderHandler prevAct =
                    previous != null && previous.TryGetValue("populateMenu", out var pa)
                        ? pa as MenuBuilderHandler
                        : null;
                MenuBuilderHandler nextAct =
                    next != null && next.TryGetValue("populateMenu", out var na)
                        ? na as MenuBuilderHandler
                        : null;
                if (!ReferenceEquals(prevAct, nextAct) && nextAct != null)
                {
                    _lastPopulate.Remove(m);
                    _lastPopulate.Add(m, nextAct);
                    nextAct(m.menu);
                }
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }

        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<
            ToolbarMenu,
            MenuBuilderHandler
        > _lastPopulate = new();

        public override void ApplyTypedFull(VisualElement element, BaseProps props)
        {
            if (element is ToolbarMenu m && props is ToolbarMenuProps fp)
            {
                if (fp.Text != null)
                    m.text = fp.Text;
                if (fp.PopulateMenu != null)
                {
                    _lastPopulate.Remove(m);
                    _lastPopulate.Add(m, fp.PopulateMenu);
                    fp.PopulateMenu(m.menu);
                }
            }
            base.ApplyTypedFull(element, props);
        }

        public override void ApplyTypedDiff(VisualElement element, BaseProps prev, BaseProps next)
        {
            if (
                element is ToolbarMenu m
                && prev is ToolbarMenuProps fp
                && next is ToolbarMenuProps fn
            )
            {
                if (fp.Text != fn.Text)
                    m.text = fn.Text ?? string.Empty;
                if (!ReferenceEquals(fp.PopulateMenu, fn.PopulateMenu) && fn.PopulateMenu != null)
                {
                    _lastPopulate.Remove(m);
                    _lastPopulate.Add(m, fn.PopulateMenu);
                    fn.PopulateMenu(m.menu);
                }
            }
            base.ApplyTypedDiff(element, prev, next);
        }
    }

    public sealed class ToolbarBreadcrumbsElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create() => new ToolbarBreadcrumbs();

        public override VisualElement ResolveChildHost(VisualElement element)
        {
            return element; // no child mounting; managed via items
        }

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (element is ToolbarBreadcrumbs bc && properties != null)
            {
                if (
                    properties.TryGetValue("items", out var itemsObj)
                    && itemsObj is System.Collections.IEnumerable items
                )
                {
                    bc.Clear();
                    int idx = 0;
                    foreach (var it in items)
                    {
                        string label = it?.ToString() ?? string.Empty;
                        int captured = idx;
                        bc.PushItem(
                            label,
                            () =>
                            {
                                if (
                                    properties.TryGetValue("onItem", out var cb)
                                    && cb is Action<int> onItem
                                )
                                {
                                    onItem(captured);
                                }
                            }
                        );
                        idx++;
                    }
                }
            }
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is ToolbarBreadcrumbs)
            {
                // Rebuild when items reference changes
                object pi = previous != null && previous.TryGetValue("items", out var p) ? p : null;
                object ni = next != null && next.TryGetValue("items", out var n) ? n : null;
                if (!ReferenceEquals(pi, ni))
                {
                    ApplyProperties(element, next);
                }
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }

        public override void ApplyTypedFull(VisualElement element, BaseProps props)
        {
            if (element is ToolbarBreadcrumbs bc && props is ToolbarBreadcrumbsProps fp)
            {
                if (fp.Items != null)
                {
                    bc.Clear();
                    int idx = 0;
                    foreach (var item in fp.Items)
                    {
                        string label = item ?? string.Empty;
                        int captured = idx;
                        bc.PushItem(label, () => fp.OnItem?.Invoke(captured));
                        idx++;
                    }
                }
            }
            base.ApplyTypedFull(element, props);
        }

        public override void ApplyTypedDiff(VisualElement element, BaseProps prev, BaseProps next)
        {
            if (
                element is ToolbarBreadcrumbs bc
                && prev is ToolbarBreadcrumbsProps fp
                && next is ToolbarBreadcrumbsProps fn
            )
            {
                if (!ReferenceEquals(fp.Items, fn.Items) || fp.OnItem != fn.OnItem)
                {
                    if (fn.Items != null)
                    {
                        bc.Clear();
                        int idx = 0;
                        foreach (var item in fn.Items)
                        {
                            string label = item ?? string.Empty;
                            int captured = idx;
                            bc.PushItem(label, () => fn.OnItem?.Invoke(captured));
                            idx++;
                        }
                    }
                }
            }
            base.ApplyTypedDiff(element, prev, next);
        }
    }

    public sealed class ToolbarPopupSearchFieldElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create() => new ToolbarPopupSearchField();

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (element is ToolbarPopupSearchField sf && properties != null)
            {
                if (properties.TryGetValue("value", out var v) && v is string s)
                {
                    sf.value = s ?? string.Empty;
                }
            }
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is ToolbarPopupSearchField sf)
            {
                string pv =
                    previous != null && previous.TryGetValue("value", out var p)
                        ? p as string
                        : null;
                string nv =
                    next != null && next.TryGetValue("value", out var n) ? n as string : null;
                if (!string.Equals(pv, nv, StringComparison.Ordinal))
                {
                    sf.value = nv ?? string.Empty;
                }
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }

        public override void ApplyTypedFull(VisualElement element, BaseProps props)
        {
            if (element is ToolbarPopupSearchField sf && props is ToolbarPopupSearchFieldProps fp)
            {
                if (fp.Value != null)
                    sf.value = fp.Value;
                if (fp.OnChange != null)
                    PropsApplier.ApplySingle(element, null, "onChange", fp.OnChange);
                if (fp.OnChangeCapture != null)
                    PropsApplier.ApplySingle(element, null, "onChangeCapture", fp.OnChangeCapture);
            }
            base.ApplyTypedFull(element, props);
        }

        public override void ApplyTypedDiff(VisualElement element, BaseProps prev, BaseProps next)
        {
            if (
                element is ToolbarPopupSearchField sf
                && prev is ToolbarPopupSearchFieldProps fp
                && next is ToolbarPopupSearchFieldProps fn
            )
            {
                if (fp.Value != fn.Value)
                    sf.value = fn.Value ?? string.Empty;
                if (fp.OnChange != fn.OnChange)
                {
                    if (fn.OnChange != null)
                        PropsApplier.ApplySingle(element, fp.OnChange, "onChange", fn.OnChange);
                    else if (fp.OnChange != null)
                        PropsApplier.RemoveProp(element, "onChange", fp.OnChange);
                }
                if (fp.OnChangeCapture != fn.OnChangeCapture)
                {
                    if (fn.OnChangeCapture != null)
                        PropsApplier.ApplySingle(
                            element,
                            fp.OnChangeCapture,
                            "onChangeCapture",
                            fn.OnChangeCapture
                        );
                    else if (fp.OnChangeCapture != null)
                        PropsApplier.RemoveProp(element, "onChangeCapture", fp.OnChangeCapture);
                }
            }
            base.ApplyTypedDiff(element, prev, next);
        }
    }

    public sealed class ToolbarSearchFieldElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create() => new ToolbarSearchField();

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (element is ToolbarSearchField sf && properties != null)
            {
                if (properties.TryGetValue("value", out var v) && v is string s)
                {
                    sf.value = s ?? string.Empty;
                }
            }
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is ToolbarSearchField sf)
            {
                string pv =
                    previous != null && previous.TryGetValue("value", out var p)
                        ? p as string
                        : null;
                string nv =
                    next != null && next.TryGetValue("value", out var n) ? n as string : null;
                if (!string.Equals(pv, nv, StringComparison.Ordinal))
                {
                    sf.value = nv ?? string.Empty;
                }
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }

        public override void ApplyTypedFull(VisualElement element, BaseProps props)
        {
            if (element is ToolbarSearchField sf && props is ToolbarSearchFieldProps fp)
            {
                if (fp.Value != null)
                    sf.value = fp.Value;
                if (fp.OnChange != null)
                    PropsApplier.ApplySingle(element, null, "onChange", fp.OnChange);
                if (fp.OnChangeCapture != null)
                    PropsApplier.ApplySingle(element, null, "onChangeCapture", fp.OnChangeCapture);
            }
            base.ApplyTypedFull(element, props);
        }

        public override void ApplyTypedDiff(VisualElement element, BaseProps prev, BaseProps next)
        {
            if (
                element is ToolbarSearchField sf
                && prev is ToolbarSearchFieldProps fp
                && next is ToolbarSearchFieldProps fn
            )
            {
                if (fp.Value != fn.Value)
                    sf.value = fn.Value ?? string.Empty;
                if (fp.OnChange != fn.OnChange)
                {
                    if (fn.OnChange != null)
                        PropsApplier.ApplySingle(element, fp.OnChange, "onChange", fn.OnChange);
                    else if (fp.OnChange != null)
                        PropsApplier.RemoveProp(element, "onChange", fp.OnChange);
                }
                if (fp.OnChangeCapture != fn.OnChangeCapture)
                {
                    if (fn.OnChangeCapture != null)
                        PropsApplier.ApplySingle(
                            element,
                            fp.OnChangeCapture,
                            "onChangeCapture",
                            fn.OnChangeCapture
                        );
                    else if (fp.OnChangeCapture != null)
                        PropsApplier.RemoveProp(element, "onChangeCapture", fp.OnChangeCapture);
                }
            }
            base.ApplyTypedDiff(element, prev, next);
        }
    }

    public sealed class ToolbarSpacerElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create() => new ToolbarSpacer();

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            PropsApplier.ApplyDiff(element, previous, next);
        }
    }
}
#endif
