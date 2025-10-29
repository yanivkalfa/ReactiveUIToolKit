using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using ReactiveUITK.Core;
using ReactiveUITK.Elements.Pools;
using ReactiveUITK.Props;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class TreeViewElementAdapter : BaseElementAdapter
    {
        private const string LogPrefix = "[ReactiveUITK][TreeViewAdapter] ";
        private static bool DebugLogs = true; // toggle for debugging

        private static void Log(string msg)
        {
            if (DebugLogs)
                UnityEngine.Debug.Log(LogPrefix + msg);
        }

        private static void LogWarn(string msg)
        {
            if (DebugLogs)
                UnityEngine.Debug.LogWarning(LogPrefix + msg);
        }

        private static void LogErr(string msg)
        {
            if (DebugLogs)
                UnityEngine.Debug.LogError(LogPrefix + msg);
        }

        private static int? TryGetItemId(object item)
        {
            if (item == null)
                return null;
            try
            {
                var p = item.GetType()
                    .GetProperty(
                        "id",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                if (p != null)
                {
                    var v = p.GetValue(item);
                    if (v is int i)
                        return i;
                    if (v != null && int.TryParse(v.ToString(), out var parsed))
                        return parsed;
                }
            }
            catch { }
            return null;
        }

        private static bool ItemHasChildren(object node)
        {
            if (node == null)
                return false;
            try
            {
                var p = node.GetType()
                    .GetProperty(
                        "children",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                if (p != null)
                {
                    var v = p.GetValue(node);
                    if (v is IList il)
                        return il.Count > 0;
                    if (v is IEnumerable en)
                    {
                        var e = en.GetEnumerator();
                        return e != null && e.MoveNext();
                    }
                }
            }
            catch { }
            return false;
        }

        private static bool RootsStructurallyEqual(IList prev, IList next)
        {
            if (ReferenceEquals(prev, next))
                return true;
            if (prev == null || next == null)
                return false;
            if (prev.Count != next.Count)
                return false;
            for (int i = 0; i < prev.Count; i++)
            {
                var pNode = prev[i];
                var nNode = next[i];
                var a = TryGetItemId(pNode);
                var b = TryGetItemId(nNode);
                if (a != b)
                    return false;
                if (ItemHasChildren(pNode) != ItemHasChildren(nNode))
                    return false;
            }
            return true;
        }

        private static string ComputeDataSignature(object data)
        {
            if (data == null)
                return string.Empty;
            try
            {
                var t = data.GetType();
                var parts = new System.Text.StringBuilder();
                var props = t.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => p.CanRead)
                    .OrderBy(p => p.Name);
                foreach (var p in props)
                {
                    try
                    {
                        var pt = p.PropertyType;
                        if (
                            pt.IsPrimitive
                            || pt.IsEnum
                            || pt == typeof(string)
                            || pt == typeof(decimal)
                            || pt == typeof(DateTime)
                        )
                        {
                            var v = p.GetValue(data);
                            parts
                                .Append('|')
                                .Append(p.Name)
                                .Append('=')
                                .Append(v?.ToString() ?? "null");
                        }
                    }
                    catch { }
                }
                var fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public)
                    .OrderBy(f => f.Name);
                foreach (var f in fields)
                {
                    try
                    {
                        var ft = f.FieldType;
                        if (
                            ft.IsPrimitive
                            || ft.IsEnum
                            || ft == typeof(string)
                            || ft == typeof(decimal)
                            || ft == typeof(DateTime)
                        )
                        {
                            var v = f.GetValue(data);
                            parts
                                .Append('|')
                                .Append(f.Name)
                                .Append('=')
                                .Append(v?.ToString() ?? "null");
                        }
                    }
                    catch { }
                }
                if (parts.Length == 0)
                    return data.ToString() ?? string.Empty;
                return parts.ToString();
            }
            catch
            {
                return data.ToString() ?? string.Empty;
            }
        }

        private static IList TryGetChildren(object node)
        {
            try
            {
                if (node == null)
                    return null;
                var p = node.GetType()
                    .GetProperty(
                        "children",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                if (p != null)
                {
                    var v = p.GetValue(node);
                    if (v is IList il)
                        return il;
                    if (v is IEnumerable en)
                    {
                        var list = new List<object>();
                        foreach (var it in en)
                            list.Add(it);
                        return list;
                    }
                }
            }
            catch { }
            return null;
        }

        private static string ComputeShallowSignature(IList roots)
        {
            if (roots == null)
                return string.Empty;
            var sb = new System.Text.StringBuilder();
            sb.Append('#').Append(roots.Count);
            for (int i = 0; i < roots.Count; i++)
            {
                var node = roots[i];
                var id = TryGetItemId(node) ?? -1;
                sb.Append('|').Append(id);
                var dataProp = node
                    ?.GetType()
                    .GetProperty(
                        "data",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                var text = ComputeDataSignature(dataProp?.GetValue(node));
                sb.Append('~').Append(text ?? string.Empty);
                var children = TryGetChildren(node);
                int childCount = children?.Count ?? 0;
                sb.Append('[').Append(childCount).Append(']');
                if (childCount > 0)
                {
                    int take = Math.Min(childCount, 16);
                    for (int c = 0; c < take; c++)
                    {
                        var ch = children[c];
                        var cid = TryGetItemId(ch) ?? -1;
                        sb.Append('{').Append(cid).Append('}');
                        var ctext = ComputeDataSignature(
                            ch?.GetType()
                                .GetProperty(
                                    "data",
                                    BindingFlags.Instance
                                        | BindingFlags.Public
                                        | BindingFlags.NonPublic
                                )
                                ?.GetValue(ch)
                        );
                        sb.Append('^').Append(ctext ?? string.Empty);
                    }
                }
            }
            return sb.ToString();
        }

        private static int TryGetVirtualizedIndex(VisualElement container)
        {
            if (container == null)
                return -1;

            Func<VisualElement, int> probe = (ve) =>
            {
                if (ve == null)
                    return -1;
                var t = ve.GetType();
                var name = t.Name;
                var full = t.FullName;
                if (
                    name == "ReusableCollectionItem"
                    || (full != null && full.EndsWith(".ReusableCollectionItem"))
                )
                {
                    var ip = t.GetProperty(
                        "index",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                    if (ip != null)
                    {
                        object v = null;
                        try
                        {
                            v = ip.GetValue(ve);
                        }
                        catch { }
                        if (v is int iv)
                            return iv;
                        try
                        {
                            return Convert.ToInt32(v);
                        }
                        catch { }
                    }
                }
                return -1;
            };

            int idx = probe(container);
            if (idx >= 0)
                return idx;
            if (container.childCount > 0)
            {
                idx = probe(container.ElementAt(0) as VisualElement);
                if (idx >= 0)
                    return idx;
            }
            if (container.parent is VisualElement p2)
            {
                idx = probe(p2);
                if (idx >= 0)
                    return idx;
            }
            return -1;
        }

        private static void RerenderVisibleRows(TreeView tv, Cached parts)
        {
            if (tv == null || parts == null || parts.RowFn == null)
                return;
            try
            {
                var scroll = GetScrollView(tv);
                if (scroll == null)
                {
                    try
                    {
                        tv.RefreshItems();
                    }
                    catch { }
                    return;
                }
                var content = scroll.contentContainer ?? scroll;
                int childCount = content.childCount;
                Log($"RerenderVisibleRows: visible={childCount}");
                for (int i = 0; i < childCount; i++)
                {
                    var container = content.ElementAt(i) as VisualElement;
                    if (container == null)
                        continue;

                    // Resolve row host robustly (search a few levels)
                    var row =
                        container.childCount > 0
                            ? container.ElementAt(0) as VisualElement
                            : container;
                    var host = FindRowHost(container) ?? FindRowHost(row);

                    // Resolve index via virtualization metadata or RowHost fallback
                    int index = TryGetVirtualizedIndex(container);
                    if (index < 0)
                    {
                        var ri = TryGetReusableIndex(container);
                        if (ri >= 0)
                            index = ri;
                    }
                    if (index < 0 && host != null && host.RowIndex >= 0)
                        index = host.RowIndex;

                    Log(
                        $"Row i={i} virtIndex={index} hostIndex={(host != null ? host.RowIndex : -1)} hostId={(host != null ? host.ItemId : 0)}"
                    );

                    bool refreshed = false;
                    // Prefer refreshing by stable item id when available
                    if (host != null && host.ItemId > 0)
                    {
                        refreshed = TryRefreshById(tv, host.ItemId);
                        Log($"TryRefreshById({host.ItemId}) => {refreshed}");
                    }
                    // Fallback: refresh by current index
                    if (!refreshed && index >= 0)
                    {
                        refreshed = TryRefreshByIndex(tv, index);
                        Log($"TryRefreshByIndex({index}) => {refreshed}");
                    }

                    if (!refreshed)
                    {
                        Log("Fallback manual Render vnode");
                        if (host == null)
                            continue;
                        var rr =
                            host.Renderer
                            ?? (
                                host.Content != null
                                    ? host.Content.userData as IVNodeHostRenderer
                                    : null
                            );
                        if (rr == null)
                            continue;
                        object item = index >= 0 ? GetItemForIndex(tv, index) : null;
                        try
                        {
                            var vnode = parts.RowFn(index, item);
                            rr.Render(vnode);
                        }
                        catch { }
                    }
                }
                // expansion state might have changed via user interaction
                MaybeNotifyExpandedChanged(tv, parts);
            }
            catch
            {
                try
                {
                    tv.Rebuild();
                }
                catch { }
            }
        }

        private sealed class Cached
        {
            public IList LastRoot;
            public bool RowWired;
            public Func<int, object, VirtualNode> RowFn;
            public string LastSig;
            public List<int> LastExpanded;
            public Action<List<int>> OnExpandedChanged;
        }

        private sealed class RowHost
        {
            public IVNodeHostRenderer Renderer;
            public int RowIndex;
            public VisualElement Content;
            public int ItemId;
        }

        private static readonly ConditionalWeakTable<TreeView, Cached> cache = new();
        private static HostContext sharedHost;
        private static HostContext Host =>
            sharedHost ??= new HostContext(ElementRegistryProvider.GetDefaultRegistry());

        public override VisualElement Create()
        {
            return GlobalVisualElementPool.Get<TreeView>();
        }

        private static void SetRootItems(TreeView tv, object rootItems)
        {
            if (tv == null || rootItems == null)
                return;
            var methods = typeof(TreeView).GetMethods(BindingFlags.Instance | BindingFlags.Public);
            var mi = methods.FirstOrDefault(m =>
                m.Name == "SetRootItems" && m.IsGenericMethodDefinition
            );
            if (mi != null)
            {
                try
                {
                    mi.MakeGenericMethod(typeof(object)).Invoke(tv, new object[] { rootItems });
                    return;
                }
                catch { }
            }
            var any = methods.FirstOrDefault(m =>
                m.Name == "SetRootItems" && m.GetParameters().Length == 1
            );
            try
            {
                any?.Invoke(tv, new object[] { rootItems });
            }
            catch { }
        }

        private static ScrollView GetScrollView(TreeView tv)
        {
            if (tv == null)
                return null;
            try
            {
                var p = tv.GetType()
                    .GetProperty("scrollView", BindingFlags.Instance | BindingFlags.Public);
                if (p != null)
                {
                    var v = p.GetValue(tv) as ScrollView;
                    if (v != null)
                        return v;
                }
            }
            catch { }
            try
            {
                var p = tv.GetType()
                    .GetProperty("scrollView", BindingFlags.Instance | BindingFlags.NonPublic);
                if (p != null)
                {
                    var v = p.GetValue(tv) as ScrollView;
                    if (v != null)
                        return v;
                }
            }
            catch { }
            try
            {
                var f = tv.GetType()
                    .GetField("m_ScrollView", BindingFlags.Instance | BindingFlags.NonPublic);
                if (f != null)
                {
                    var v = f.GetValue(tv) as ScrollView;
                    if (v != null)
                        return v;
                }
            }
            catch { }
            try
            {
                return tv.Q<ScrollView>();
            }
            catch
            {
                return null;
            }
        }

        private static List<int> GetExpandedIds(TreeView tv)
        {
            try
            {
                var prop = tv.GetType()
                    .GetProperty("expandedItemIds", BindingFlags.Instance | BindingFlags.Public);
                if (prop != null)
                {
                    var v = prop.GetValue(tv) as System.Collections.IEnumerable;
                    if (v != null)
                    {
                        var list = new List<int>();
                        foreach (var o in v)
                        {
                            if (o is int i)
                                list.Add(i);
                            else if (o != null && int.TryParse(o.ToString(), out var pi))
                                list.Add(pi);
                        }
                        return list;
                    }
                }
                var vc = GetViewController(tv);
                if (vc != null)
                {
                    var p = vc.GetType()
                        .GetProperty(
                            "expandedItemIds",
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                        );
                    if (p != null)
                    {
                        var v = p.GetValue(vc) as System.Collections.IEnumerable;
                        if (v != null)
                        {
                            var list = new List<int>();
                            foreach (var o in v)
                            {
                                if (o is int i)
                                    list.Add(i);
                                else if (o != null && int.TryParse(o.ToString(), out var pi))
                                    list.Add(pi);
                            }
                            return list;
                        }
                    }
                }
            }
            catch { }
            return new List<int>();
        }

        private static void RestoreExpandedIds(TreeView tv, IEnumerable<int> ids)
        {
            if (ids == null)
                return;
            try
            {
                // Prefer public ExpandItem API
                foreach (var id in ids)
                {
                    try
                    {
                        tv.ExpandItem(id, true);
                    }
                    catch { }
                }
            }
            catch { }
            try
            {
                var prop = tv.GetType()
                    .GetProperty("expandedItemIds", BindingFlags.Instance | BindingFlags.Public);
                if (prop != null && prop.CanWrite)
                {
                    var list = ids.ToList();
                    prop.SetValue(tv, list);
                }
            }
            catch { }
        }

        private static void MaybeNotifyExpandedChanged(TreeView tv, Cached parts)
        {
            try
            {
                var now = GetExpandedIds(tv);
                bool changed =
                    parts.LastExpanded == null || !SequenceEqual(parts.LastExpanded, now);
                if (changed)
                {
                    parts.LastExpanded = now.ToList();
                    parts.OnExpandedChanged?.Invoke(now);
                }
            }
            catch { }
        }

        private static bool SequenceEqual(List<int> a, List<int> b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a == null || b == null)
                return false;
            if (a.Count != b.Count)
                return false;
            for (int i = 0; i < a.Count; i++)
                if (a[i] != b[i])
                    return false;
            return true;
        }

        private static RowHost FindRowHost(VisualElement root)
        {
            if (root == null)
                return null;
            var q = new Queue<(VisualElement, int)>();
            q.Enqueue((root, 0));
            while (q.Count > 0)
            {
                var (ve, d) = q.Dequeue();
                if (ve == null)
                    continue;
                if (ve.userData is RowHost rh)
                    return rh;
                if (d >= 4)
                    continue;
                int cc = ve.childCount;
                for (int i = 0; i < cc; i++)
                    q.Enqueue((ve.ElementAt(i) as VisualElement, d + 1));
            }
            return null;
        }

        private static int TryGetReusableIndex(VisualElement container)
        {
            try
            {
                var t = container?.GetType();
                if (t == null)
                    return -1;
                var p = t.GetProperty(
                    "index",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );
                if (p != null)
                {
                    var v = p.GetValue(container);
                    if (v is int i)
                        return i;
                    if (v != null && int.TryParse(v.ToString(), out var pi))
                        return pi;
                }
            }
            catch { }
            return -1;
        }

        private static object GetItemForIndex(TreeView tv, int index)
        {
            try
            {
                var mi = typeof(TreeView)
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(m =>
                        m.Name == "GetItemDataForIndex" && m.IsGenericMethodDefinition
                    );
                if (mi != null)
                {
                    var generic = mi.MakeGenericMethod(typeof(object));
                    return generic.Invoke(tv, new object[] { index });
                }
            }
            catch { }
            return null;
        }

        private static object GetViewController(TreeView tv)
        {
            try
            {
                var p = tv.GetType()
                    .GetProperty(
                        "viewController",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                return p?.GetValue(tv);
            }
            catch { }
            return null;
        }

        private static int? GetIdForIndex(TreeView tv, int index)
        {
            try
            {
                var vc = GetViewController(tv);
                if (vc == null)
                    return null;
                var mi = vc.GetType()
                    .GetMethod(
                        "GetIdForIndex",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        new Type[] { typeof(int) },
                        null
                    );
                if (mi != null)
                {
                    var v = mi.Invoke(vc, new object[] { index });
                    if (v is int i)
                        return i;
                    if (v != null && int.TryParse(v.ToString(), out var pi))
                        return pi;
                }
            }
            catch { }
            return null;
        }

        private static int? GetIndexForId(TreeView tv, int id)
        {
            try
            {
                var vc = GetViewController(tv);
                if (vc == null)
                    return null;
                var mi = vc.GetType()
                    .GetMethod(
                        "GetIndexForId",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        new Type[] { typeof(int) },
                        null
                    );
                if (mi != null)
                {
                    var v = mi.Invoke(vc, new object[] { id });
                    if (v is int i)
                        return i;
                    if (v != null && int.TryParse(v.ToString(), out var pi))
                        return pi;
                }
            }
            catch { }
            return null;
        }

        private static bool TryRefreshByIndex(TreeView tv, int index)
        {
            try
            {
                var m = tv.GetType()
                    .GetMethod(
                        "RefreshItem",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        new Type[] { typeof(int) },
                        null
                    );
                if (m != null)
                {
                    m.Invoke(tv, new object[] { index });
                    Log($"RefreshItem(index:{index}) via TreeView");
                    return true;
                }
                var baseM = tv.GetType()
                    .BaseType?.GetMethod(
                        "RefreshItem",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        new Type[] { typeof(int) },
                        null
                    );
                if (baseM != null)
                {
                    baseM.Invoke(tv, new object[] { index });
                    Log($"RefreshItem(index:{index}) via BaseType");
                    return true;
                }
                var vc = GetViewController(tv);
                if (vc != null)
                {
                    var vm = vc.GetType()
                        .GetMethod(
                            "RefreshItem",
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                            null,
                            new Type[] { typeof(int) },
                            null
                        );
                    if (vm != null)
                    {
                        vm.Invoke(vc, new object[] { index });
                        Log($"RefreshItem(index:{index}) via ViewController");
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        private static bool TryRefreshById(TreeView tv, int id)
        {
            try
            {
                var vc = GetViewController(tv);
                if (vc != null)
                {
                    var im = vc.GetType()
                        .GetMethod(
                            "RefreshItem",
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                            null,
                            new Type[] { typeof(int) },
                            null
                        );
                    if (im != null)
                    {
                        im.Invoke(vc, new object[] { id });
                        Log($"RefreshItem(id:{id}) via ViewController");
                        return true;
                    }
                    var idx = GetIndexForId(tv, id);
                    Log($"Map id->{id} to index {idx}");
                    if (idx.HasValue && idx.Value >= 0)
                    {
                        return TryRefreshByIndex(tv, idx.Value);
                    }
                }
            }
            catch { }
            return false;
        }

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (element is not TreeView tv)
            {
                PropsApplier.Apply(element, properties);
                return;
            }
            Log("ApplyProperties");
            var parts = cache.GetValue(tv, _ => new Cached());
            if (properties != null)
            {
                if (properties.TryGetValue("rootItems", out var r))
                {
                    var nextList = r as IList;
                    string nextSig = ComputeShallowSignature(nextList);
                    string prevSig = parts.LastSig ?? string.Empty;
                    var prevList = parts.LastRoot;
                    bool structureChanged = !RootsStructurallyEqual(prevList, nextList);
                    bool anyChanged = structureChanged || !string.Equals(prevSig, nextSig);
                    Log(
                        $"rootItems anyChanged={anyChanged} structureChanged={structureChanged} count={(nextList != null ? nextList.Count : -1)}"
                    );
                    if (anyChanged)
                    {
                        if (structureChanged)
                        {
                            var expanded = GetExpandedIds(tv);
                            SetRootItems(tv, nextList);
                            try
                            {
                                tv.Rebuild();
                            }
                            catch { }
                            RestoreExpandedIds(tv, expanded);
                        }
                        try
                        {
                            tv.RefreshItems();
                            RerenderVisibleRows(tv, parts);
                        }
                        catch { }
                        parts.LastRoot = nextList;
                        parts.LastSig = nextSig;
                    }
                }
                if (
                    properties.TryGetValue("onExpandedIdsChanged", out var onexp)
                    && onexp is Action<List<int>> cb
                )
                {
                    parts.OnExpandedChanged = cb;
                }
                TryApplyProp<float>(properties, "fixedItemHeight", f => tv.fixedItemHeight = f);
                if (properties.TryGetValue("selectionType", out var sel) && sel is SelectionType st)
                    tv.selectionType = st;
                TryApplyProp<int>(properties, "selectedIndex", i => tv.SetSelection(i));
                if (
                    properties.TryGetValue("row", out var row)
                    && row is Func<int, object, VirtualNode> rf
                )
                {
                    parts.RowFn = rf;
                    if (!parts.RowWired)
                    {
                        parts.RowWired = true;
                        tv.makeItem = () =>
                        {
                            var row = new VisualElement();
                            var content = new VisualElement();
                            try
                            {
                                content.pickingMode = PickingMode.Ignore;
                            }
                            catch { }
                            row.Add(content);
                            row.userData = new RowHost
                            {
                                Renderer = new VNodeHostRenderer(Host, content),
                                RowIndex = -1,
                                Content = content,
                                ItemId = 0,
                            };
                            return row;
                        };
                        tv.bindItem = (ve, index) =>
                        {
                            var host = ve.userData as RowHost;
                            var rr = host != null ? host.Renderer : null;
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
                                var newHost = new RowHost
                                {
                                    Renderer = new VNodeHostRenderer(Host, content),
                                    RowIndex = -1,
                                    Content = content,
                                    ItemId = 0,
                                };
                                ve.userData = newHost;
                                rr = newHost.Renderer;
                                host = newHost;
                            }
                            host.RowIndex = index;
                            var curId = GetIdForIndex(tv, index);
                            if (curId.HasValue)
                                host.ItemId = curId.Value;
                            var item = GetItemForIndex(tv, index);
                            var f = parts.RowFn;
                            if (f != null)
                            {
                                var vnode = f(index, item);
                                vnode = EnsureVisualElementRoot(vnode, "TreeViewRow");
                                rr.Render(vnode);
                            }
                            MaybeNotifyExpandedChanged(tv, parts);
                        };
                        tv.unbindItem = (ve, i) =>
                        {
                            var host = ve.userData as RowHost;
                            try
                            {
                                host?.Renderer?.Unmount();
                            }
                            catch { }
                        };
                    }
                }
            }
            if (
                properties != null
                && properties.TryGetValue("contentContainer", out var cc)
                && cc is Dictionary<string, object> ccMap
            )
            {
                PropsApplier.Apply(tv.contentContainer, ccMap);
            }
            if (
                properties != null
                && properties.TryGetValue("scrollView", out var sv)
                && sv is Dictionary<string, object> svMap
            )
            {
                var scroll = GetScrollView(tv);
                if (scroll != null)
                    PropsApplier.Apply(scroll, svMap);
            }
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is not TreeView tv)
            {
                PropsApplier.ApplyDiff(element, previous, next);
                return;
            }
            Log("ApplyPropertiesDiff");
            previous ??= new Dictionary<string, object>();
            next ??= new Dictionary<string, object>();
            var parts = cache.GetValue(tv, _ => new Cached());

            previous.TryGetValue("rootItems", out var prevRoot);
            next.TryGetValue("rootItems", out var nextRoot);
            if (!ReferenceEquals(prevRoot, nextRoot))
            {
                var nextList = nextRoot as IList;
                var prevList = prevRoot as IList;
                string nextSig = ComputeShallowSignature(nextList);
                string prevSig = parts.LastSig ?? ComputeShallowSignature(prevList);
                bool structureChanged = !RootsStructurallyEqual(prevList, nextList);
                bool anyChanged = structureChanged || !string.Equals(prevSig, nextSig);
                Log($"Diff rootItems anyChanged={anyChanged} structureChanged={structureChanged}");
                if (anyChanged)
                {
                    if (structureChanged)
                    {
                        var expanded = GetExpandedIds(tv);
                        SetRootItems(tv, nextList);
                        try
                        {
                            tv.Rebuild();
                        }
                        catch { }
                        RestoreExpandedIds(tv, expanded);
                    }
                    try
                    {
                        tv.RefreshItems();
                        RerenderVisibleRows(tv, parts);
                    }
                    catch { }
                    parts.LastRoot = nextList;
                    parts.LastSig = nextSig;
                }
            }
            else
            {
                if (next.TryGetValue("rootItems", out var riStable))
                {
                    try
                    {
                        var stableList = riStable as IList;
                        string sig = ComputeShallowSignature(stableList);
                        if (!string.Equals(parts.LastSig ?? string.Empty, sig))
                        {
                            parts.LastSig = sig;
                            Log(
                                "Stable root ref: shallow signature changed; refreshing visible rows"
                            );
                            try
                            {
                                tv.RefreshItems();
                                RerenderVisibleRows(tv, parts);
                            }
                            catch { }
                        }
                    }
                    catch { }
                }
            }
            TryDiffProp<float>(previous, next, "fixedItemHeight", f => tv.fixedItemHeight = f);
            if (next.TryGetValue("selectionType", out var sel) && sel is SelectionType st)
                tv.selectionType = st;
            TryDiffProp<int>(previous, next, "selectedIndex", i => tv.SetSelection(i));

            previous.TryGetValue("row", out var prow);
            next.TryGetValue("row", out var nrow);
            if (!ReferenceEquals(prow, nrow) && nrow is Func<int, object, VirtualNode> rf)
            {
                parts.RowFn = rf;
                try
                {
                    tv.RefreshItems();
                    RerenderVisibleRows(tv, parts);
                }
                catch { }
            }

            previous.TryGetValue("contentContainer", out var pcc);
            next.TryGetValue("contentContainer", out var ncc);
            if (!ReferenceEquals(pcc, ncc) && ncc is Dictionary<string, object> ccMap)
                PropsApplier.Apply(tv.contentContainer, ccMap);
            previous.TryGetValue("scrollView", out var psv);
            next.TryGetValue("scrollView", out var nsv);
            if (!ReferenceEquals(psv, nsv) && nsv is Dictionary<string, object> svMap)
            {
                var scroll = GetScrollView(tv);
                if (scroll != null)
                    PropsApplier.Apply(scroll, svMap);
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }
    }
}
