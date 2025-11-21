// Editor-only Toolbar family adapters
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using ReactiveUITK.Props;
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

        public override void ApplyProperties(VisualElement element, IReadOnlyDictionary<string, object> properties)
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

        public override void ApplyPropertiesDiff(VisualElement element, IReadOnlyDictionary<string, object> previous, IReadOnlyDictionary<string, object> next)
        {
            if (element is ToolbarButton btn)
            {
                string prev = previous != null && previous.TryGetValue("text", out var p) ? p as string : null;
                string nxt = next != null && next.TryGetValue("text", out var n) ? n as string : null;
                if (!string.Equals(prev, nxt, StringComparison.Ordinal))
                {
                    btn.text = nxt ?? string.Empty;
                }
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }
    }

    public sealed class ToolbarToggleElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create() => new ToolbarToggle();

        public override void ApplyProperties(VisualElement element, IReadOnlyDictionary<string, object> properties)
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

        public override void ApplyPropertiesDiff(VisualElement element, IReadOnlyDictionary<string, object> previous, IReadOnlyDictionary<string, object> next)
        {
            if (element is ToolbarToggle t)
            {
                string prevText = previous != null && previous.TryGetValue("text", out var p) ? p as string : null;
                string nextText = next != null && next.TryGetValue("text", out var n) ? n as string : null;
                if (!string.Equals(prevText, nextText, StringComparison.Ordinal))
                {
                    t.text = nextText ?? string.Empty;
                }
                bool prevVal = previous != null && previous.TryGetValue("value", out var pv) && pv is bool pb && pb;
                bool nextVal = next != null && next.TryGetValue("value", out var nv) && nv is bool nb && nb;
                if (prevVal != nextVal)
                {
                    t.value = nextVal;
                }
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }
    }

    public sealed class ToolbarMenuElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create() => new ToolbarMenu();

        public override void ApplyProperties(VisualElement element, IReadOnlyDictionary<string, object> properties)
        {
            if (element is ToolbarMenu m && properties != null)
            {
                if (properties.TryGetValue("text", out var t) && t is string ts)
                {
                    m.text = ts ?? string.Empty;
                }
                if (properties.TryGetValue("populateMenu", out var pm) && pm is Action<DropdownMenu> action)
                {
                    // ToolbarMenu.menu is read-only; populate the existing instance.
                    // Avoid repeated appends for the same callback instance.
                    if (!_lastPopulate.TryGetValue(m, out var existing) || !ReferenceEquals(existing, action))
                    {
                        _lastPopulate.Remove(m);
                        _lastPopulate.Add(m, action);
                        action?.Invoke(m.menu);
                    }
                }
            }
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(VisualElement element, IReadOnlyDictionary<string, object> previous, IReadOnlyDictionary<string, object> next)
        {
            if (element is ToolbarMenu m)
            {
                string prev = previous != null && previous.TryGetValue("text", out var p) ? p as string : null;
                string nxt = next != null && next.TryGetValue("text", out var n) ? n as string : null;
                if (!string.Equals(prev, nxt, StringComparison.Ordinal))
                {
                    m.text = nxt ?? string.Empty;
                }
                Action<DropdownMenu> prevAct = previous != null && previous.TryGetValue("populateMenu", out var pa) ? pa as Action<DropdownMenu> : null;
                Action<DropdownMenu> nextAct = next != null && next.TryGetValue("populateMenu", out var na) ? na as Action<DropdownMenu> : null;
                if (!ReferenceEquals(prevAct, nextAct) && nextAct != null)
                {
                    _lastPopulate.Remove(m);
                    _lastPopulate.Add(m, nextAct);
                    nextAct(m.menu);
                }
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }

        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<ToolbarMenu, Action<DropdownMenu>> _lastPopulate = new();
    }

    public sealed class ToolbarBreadcrumbsElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create() => new ToolbarBreadcrumbs();

        public override VisualElement ResolveChildHost(VisualElement element)
        {
            return element; // no child mounting; managed via items
        }

        public override void ApplyProperties(VisualElement element, IReadOnlyDictionary<string, object> properties)
        {
            if (element is ToolbarBreadcrumbs bc && properties != null)
            {
                if (properties.TryGetValue("items", out var itemsObj) && itemsObj is System.Collections.IEnumerable items)
                {
                    bc.Clear();
                    int idx = 0;
                    foreach (var it in items)
                    {
                        string label = it?.ToString() ?? string.Empty;
                        int captured = idx;
                        bc.PushItem(label, () =>
                        {
                            if (properties.TryGetValue("onItem", out var cb) && cb is Action<int> onItem)
                            {
                                onItem(captured);
                            }
                        });
                        idx++;
                    }
                }
            }
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(VisualElement element, IReadOnlyDictionary<string, object> previous, IReadOnlyDictionary<string, object> next)
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
    }

    public sealed class ToolbarPopupSearchFieldElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create() => new ToolbarPopupSearchField();

        public override void ApplyProperties(VisualElement element, IReadOnlyDictionary<string, object> properties)
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

        public override void ApplyPropertiesDiff(VisualElement element, IReadOnlyDictionary<string, object> previous, IReadOnlyDictionary<string, object> next)
        {
            if (element is ToolbarPopupSearchField sf)
            {
                string pv = previous != null && previous.TryGetValue("value", out var p) ? p as string : null;
                string nv = next != null && next.TryGetValue("value", out var n) ? n as string : null;
                if (!string.Equals(pv, nv, StringComparison.Ordinal))
                {
                    sf.value = nv ?? string.Empty;
                }
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }
    }

    public sealed class ToolbarSearchFieldElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create() => new ToolbarSearchField();

        public override void ApplyProperties(VisualElement element, IReadOnlyDictionary<string, object> properties)
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

        public override void ApplyPropertiesDiff(VisualElement element, IReadOnlyDictionary<string, object> previous, IReadOnlyDictionary<string, object> next)
        {
            if (element is ToolbarSearchField sf)
            {
                string pv = previous != null && previous.TryGetValue("value", out var p) ? p as string : null;
                string nv = next != null && next.TryGetValue("value", out var n) ? n as string : null;
                if (!string.Equals(pv, nv, StringComparison.Ordinal))
                {
                    sf.value = nv ?? string.Empty;
                }
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }
    }

    public sealed class ToolbarSpacerElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create() => new ToolbarSpacer();

        public override void ApplyProperties(VisualElement element, IReadOnlyDictionary<string, object> properties)
        {
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(VisualElement element, IReadOnlyDictionary<string, object> previous, IReadOnlyDictionary<string, object> next)
        {
            PropsApplier.ApplyDiff(element, previous, next);
        }
    }
}
#endif
