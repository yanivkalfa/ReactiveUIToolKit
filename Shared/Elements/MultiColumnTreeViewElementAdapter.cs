using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ReactiveUITK.Core;
using ReactiveUITK.Props;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class MultiColumnTreeViewElementAdapter : BaseElementAdapter
    {
        private sealed class Cached
        {
            public IList LastRoot;
            public List<(string name, string title)> ColSig;
            public List<Func<int, object, VirtualNode>> CellFns;
        }

        private static readonly ConditionalWeakTable<MultiColumnTreeView, Cached> cache = new();
        private static HostContext host;
        private static HostContext Host =>
            host ??= new HostContext(ElementRegistryProvider.GetDefaultRegistry());

        public override VisualElement Create() => new MultiColumnTreeView();

        private static void SetRootItems(MultiColumnTreeView tv, object root)
        {
            if (tv == null || root == null)
                return;
            var mi = typeof(MultiColumnTreeView)
                .GetMethods()
                .FirstOrDefault(m => m.Name == "SetRootItems" && m.IsGenericMethodDefinition);
            if (mi != null)
            {
                try
                {
                    mi.MakeGenericMethod(typeof(object)).Invoke(tv, new object[] { root });
                    return;
                }
                catch { }
            }
            var any = typeof(MultiColumnTreeView)
                .GetMethods()
                .FirstOrDefault(m => m.Name == "SetRootItems" && m.GetParameters().Length == 1);
            try
            {
                any?.Invoke(tv, new object[] { root });
            }
            catch { }
        }

        private static object GetItemForRow(MultiColumnTreeView tv, int index)
        {
            try
            {
                var mi = typeof(MultiColumnTreeView)
                    .GetMethods()
                    .FirstOrDefault(m =>
                        m.Name == "GetItemDataForIndex" && m.IsGenericMethodDefinition
                    );
                if (mi != null)
                {
                    return mi.MakeGenericMethod(typeof(object)).Invoke(tv, new object[] { index });
                }
            }
            catch { }
            return null;
        }

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (element is not MultiColumnTreeView tv)
            {
                PropsApplier.Apply(element, properties);
                return;
            }
            var parts = cache.GetValue(tv, _ => new Cached());
            if (properties != null)
            {
                if (properties.TryGetValue("rootItems", out var r))
                {
                    var nextList = r as IList;
                    if (!ReferenceEquals(parts.LastRoot, nextList))
                    {
                        parts.LastRoot = nextList;
                        SetRootItems(tv, nextList);
                        try
                        {
                            tv.Rebuild();
                        }
                        catch { }
                    }
                }
                TryApplyProp<float>(properties, "fixedItemHeight", f => tv.fixedItemHeight = f);
                if (properties.TryGetValue("selectionType", out var sel) && sel is SelectionType st)
                    tv.selectionType = st;
                TryApplyProp<int>(properties, "selectedIndex", i => tv.selectedIndex = i);

                if (
                    properties.TryGetValue("columns", out var cols)
                    && cols is IEnumerable<Dictionary<string, object>> list
                )
                {
                    var sig = new List<(string, string)>();
                    var fns = new List<Func<int, object, VirtualNode>>();
                    foreach (var c in list)
                    {
                        c.TryGetValue("name", out var n);
                        c.TryGetValue("title", out var t);
                        c.TryGetValue("cell", out var cell);
                        sig.Add((n as string, t as string));
                        fns.Add(cell as Func<int, object, VirtualNode>);
                    }
                    bool same = parts.ColSig != null && parts.ColSig.Count == sig.Count;
                    if (same)
                    {
                        for (int i = 0; i < sig.Count; i++)
                            if (parts.ColSig[i] != sig[i])
                            {
                                same = false;
                                break;
                            }
                    }
                    parts.ColSig = sig;
                    parts.CellFns = fns;
                    if (!same)
                    {
                        RebuildColumns(tv, list, parts);
                    }
                }
            }
            ApplySlots(tv, properties);
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is not MultiColumnTreeView tv)
            {
                PropsApplier.ApplyDiff(element, previous, next);
                return;
            }
            previous ??= new Dictionary<string, object>();
            next ??= new Dictionary<string, object>();
            var parts = cache.GetValue(tv, _ => new Cached());

            previous.TryGetValue("rootItems", out var pr);
            next.TryGetValue("rootItems", out var nr);
            if (!ReferenceEquals(pr, nr))
            {
                parts.LastRoot = nr as IList;
                SetRootItems(tv, nr);
                try
                {
                    tv.Rebuild();
                }
                catch { }
            }
            TryDiffProp<float>(previous, next, "fixedItemHeight", f => tv.fixedItemHeight = f);
            if (next.TryGetValue("selectionType", out var sel) && sel is SelectionType st)
                tv.selectionType = st;
            TryDiffProp<int>(previous, next, "selectedIndex", i => tv.selectedIndex = i);

            previous.TryGetValue("columns", out var pc);
            next.TryGetValue("columns", out var nc);
            if (!ReferenceEquals(pc, nc) && nc is IEnumerable<Dictionary<string, object>> list)
            {
                var sig = new List<(string, string)>();
                var fns = new List<Func<int, object, VirtualNode>>();
                foreach (var c in list)
                {
                    c.TryGetValue("name", out var n);
                    c.TryGetValue("title", out var t);
                    c.TryGetValue("cell", out var cell);
                    sig.Add((n as string, t as string));
                    fns.Add(cell as Func<int, object, VirtualNode>);
                }
                parts.ColSig = sig;
                parts.CellFns = fns;
                RebuildColumns(tv, list, parts);
            }
            ApplySlotsDiff(tv, previous, next);
            PropsApplier.ApplyDiff(element, previous, next);
        }

        private static void RebuildColumns(
            MultiColumnTreeView tv,
            IEnumerable<Dictionary<string, object>> cols,
            Cached parts
        )
        {
            tv.columns.Clear();
            int idx = 0;
            foreach (var c in cols)
            {
                c.TryGetValue("title", out var t);
                c.TryGetValue("name", out var n);
                var col = new Column { title = t as string };
                if (n is string ns && !string.IsNullOrEmpty(ns))
                    col.name = ns;
                if (c.TryGetValue("width", out var w))
                {
                    try
                    {
                        col.width = Convert.ToSingle(w);
                    }
                    catch { }
                }
                if (c.TryGetValue("minWidth", out var mw))
                {
                    try
                    {
                        col.minWidth = Convert.ToSingle(mw);
                    }
                    catch { }
                }
                if (c.TryGetValue("maxWidth", out var xw))
                {
                    try
                    {
                        col.maxWidth = Convert.ToSingle(xw);
                    }
                    catch { }
                }
                if (c.TryGetValue("resizable", out var rz) && rz is bool rb)
                    col.resizable = rb;
                if (c.TryGetValue("stretchable", out var st) && st is bool sb)
                    col.stretchable = sb;
                col.makeCell = () =>
                {
                    var ve = new VisualElement();
                    var content = new VisualElement();
                    try
                    {
                        content.pickingMode = PickingMode.Ignore;
                    }
                    catch { }
                    ve.Add(content);
                    ve.userData = new VNodeHostRenderer(Host, content);
                    return ve;
                };
                int captured = idx;
                col.bindCell = (ve, rowIndex) =>
                {
                    var rr = ve.userData as IVNodeHostRenderer;
                    if (rr == null)
                    {
                        VisualElement content =
                            ve.childCount > 0 ? ve.ElementAt(0) as VisualElement : null;
                        if (content == null)
                        {
                            content = new VisualElement();
                            ve.Add(content);
                        }
                        try
                        {
                            content.pickingMode = PickingMode.Ignore;
                        }
                        catch { }
                        rr = new VNodeHostRenderer(Host, content);
                        ve.userData = rr;
                    }
                    object item = GetItemForRow(tv, rowIndex);
                    var fn =
                        parts.CellFns != null && captured < parts.CellFns.Count
                            ? parts.CellFns[captured]
                            : null;
                    if (fn != null)
                    {
                        var vnode = fn(rowIndex, item);
                        vnode = EnsureVisualElementRoot(vnode, "MultiColumnTreeViewCell");
                        rr.Render(vnode);
                    }
                };
                col.unbindCell = (ve, i) =>
                {
                    (ve.userData as IVNodeHostRenderer)?.Unmount();
                };
                tv.columns.Add(col);
                idx++;
            }
            try
            {
                tv.Rebuild();
            }
            catch { }
        }

        private static void ApplySlots(
            MultiColumnTreeView tv,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (properties == null)
                return;
            if (
                properties.TryGetValue("contentContainer", out var cc)
                && cc is Dictionary<string, object> ccMap
            )
                PropsApplier.Apply(tv.contentContainer, ccMap);
            if (
                properties.TryGetValue("scrollView", out var sv)
                && sv is Dictionary<string, object> svMap
            )
            {
                var scroll = tv.Q<ScrollView>();
                if (scroll != null)
                    PropsApplier.Apply(scroll, svMap);
            }
        }

        private static void ApplySlotsDiff(
            MultiColumnTreeView tv,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            previous ??= new Dictionary<string, object>();
            next ??= new Dictionary<string, object>();
            previous.TryGetValue("contentContainer", out var pcc);
            next.TryGetValue("contentContainer", out var ncc);
            if (!ReferenceEquals(pcc, ncc) && ncc is Dictionary<string, object> ccMap)
                PropsApplier.Apply(tv.contentContainer, ccMap);
            previous.TryGetValue("scrollView", out var psv);
            next.TryGetValue("scrollView", out var nsv);
            if (!ReferenceEquals(psv, nsv) && nsv is Dictionary<string, object> svMap)
            {
                var scroll = tv.Q<ScrollView>();
                if (scroll != null)
                    PropsApplier.Apply(scroll, svMap);
            }
        }
    }
}
