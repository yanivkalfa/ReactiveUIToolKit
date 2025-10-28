using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ReactiveUITK.Core;
using ReactiveUITK.Elements.Pools;
using ReactiveUITK.Props;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class MultiColumnListViewElementAdapter : BaseElementAdapter
    {
        private sealed class CachedParts
        {
            public IList LastItems;
            public List<ColSig> LastColSignature;
            public List<Func<int, object, VirtualNode>> CellFns;
        }

        private static readonly ConditionalWeakTable<MultiColumnListView, CachedParts> cache = new();

        private static HostContext sharedHostContext;
        private static HostContext GetCellHostContext()
        {
            if (sharedHostContext == null)
            {
                var registry = ElementRegistryProvider.GetDefaultRegistry();
                sharedHostContext = new HostContext(registry);
            }
            return sharedHostContext;
        }

        private static VirtualNode EnsureVisualRoot(VirtualNode node)
        {
            if (node == null)
                return null;
            if (
                node.NodeType == VirtualNodeType.Element
                && string.Equals(node.ElementTypeName, "VisualElement", StringComparison.Ordinal)
            )
                return node;
            return new VirtualNode(
                VirtualNodeType.Element,
                elementTypeName: "VisualElement",
                functionRender: null,
                textContent: null,
                key: node.Key,
                properties: new Dictionary<string, object>(0),
                children: new List<VirtualNode> { node }
            );
        }

        private static IList NormalizeItems(object itemsObj)
        {
            if (itemsObj == null)
                return null;
            if (itemsObj is IList il)
                return il;
            if (itemsObj is IEnumerable en)
            {
                var list = new List<object>();
                foreach (var it in en)
                    list.Add(it);
                return list;
            }
            return null;
        }

        public override VisualElement Create()
        {
            return GlobalVisualElementPool.Get<MultiColumnListView>();
        }

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (element is not MultiColumnListView view || properties == null)
            {
                PropsApplier.Apply(element, properties);
                return;
            }

            var parts = cache.GetValue(view, _ => new CachedParts());

            if (properties.TryGetValue("items", out var itemsObj))
            {
                var incoming = NormalizeItems(itemsObj);
                if (!ReferenceEquals(parts.LastItems, incoming))
                {
                    parts.LastItems = incoming;
                    view.itemsSource = incoming as IList;
                    try { view.Rebuild(); } catch { }
                }
            }
            else if (parts.LastItems != null)
            {
                parts.LastItems = null;
                view.itemsSource = null;
                try { view.Rebuild(); } catch { }
            }

            TryApplyProp<int>(properties, "selectedIndex", i => view.selectedIndex = i);
            TryApplyProp<float>(properties, "fixedItemHeight", h => view.fixedItemHeight = h);
            if (
                properties.TryGetValue("selectionType", out var selObj)
                && selObj is SelectionType sel
            )
            {
                view.selectionType = sel;
            }

            // Columns: rebuild only if semantic signature changes
            if (properties.TryGetValue("columns", out var colsObj) && colsObj is IEnumerable cols)
            {
                var (newSig, newFns) = ExtractSignatureAndFns(cols);
                if (!SignaturesEqual(parts.LastColSignature, newSig))
                {
                    parts.CellFns = newFns;
                    RebuildColumnsPreservingState(view, cols, parts);
                    parts.LastColSignature = newSig;
                }
                else
                {
                    // Update cell delegates without forcing a refresh; avoid unbind/remount
                    parts.CellFns = newFns;
                }
            }

            ApplySlots(view, properties);
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is not MultiColumnListView view)
            {
                PropsApplier.ApplyDiff(element, previous, next);
                return;
            }
            previous ??= new Dictionary<string, object>();
            next ??= new Dictionary<string, object>();

            var parts = cache.GetValue(view, _ => new CachedParts());

            previous.TryGetValue("items", out var prevItemsObj);
            next.TryGetValue("items", out var nextItemsObj);
            var prevItems = NormalizeItems(prevItemsObj);
            var nextItems = NormalizeItems(nextItemsObj);
            if (!ReferenceEquals(prevItems, nextItems))
            {
                parts.LastItems = nextItems;
                view.itemsSource = nextItems;
                try { view.Rebuild(); } catch { }
            }

            TryDiffProp<int>(previous, next, "selectedIndex", i => view.selectedIndex = i);
            TryDiffProp<float>(previous, next, "fixedItemHeight", h => view.fixedItemHeight = h);
            if (
                next.TryGetValue("selectionType", out var selNext)
                && selNext is SelectionType selType
            )
            {
                if (view.selectionType != selType)
                {
                    view.selectionType = selType;
                }
            }

            previous.TryGetValue("columns", out var prevCols);
            next.TryGetValue("columns", out var nextCols);
            if (nextCols is IEnumerable ncols)
            {
                var (newSig, newFns) = ExtractSignatureAndFns(ncols);
                if (!SignaturesEqual(parts.LastColSignature, newSig))
                {
                    parts.CellFns = newFns;
                    RebuildColumnsPreservingState(view, ncols, parts);
                    parts.LastColSignature = newSig;
                }
                else
                {
                    // Only update delegate references; do not refresh realized rows
                    parts.CellFns = newFns;
                }
            }

            ApplySlotsDiff(view, previous, next);
            PropsApplier.ApplyDiff(element, previous, next);
        }

        private static void RebuildColumnsPreservingState(MultiColumnListView view, IEnumerable newCols, CachedParts parts)
        {
            // Capture existing columns by name and index
            var existingByName = new Dictionary<string, Column>();
            var existingByIndex = new List<Column>();
            for (int ci = 0; ci < view.columns.Count; ci++)
            {
                var col = view.columns[ci];
                existingByIndex.Add(col);
                if (!string.IsNullOrEmpty(col.name) && !existingByName.ContainsKey(col.name))
                {
                    existingByName[col.name] = col;
                }
            }

            view.columns.Clear();
            int index = 0;
            int colIndex = 0;
            foreach (var co in newCols)
            {
                if (co is not IDictionary<string, object> colMap) { index++; continue; }
                string name = colMap.TryGetValue("name", out var n) ? n as string : null;
                string title = colMap.TryGetValue("title", out var t) ? t as string : null;
                Func<int, object, VirtualNode> cellFn = null;
                if (colMap.TryGetValue("cell", out var c) && c is Func<int, object, VirtualNode> cf)
                {
                    cellFn = cf;
                }
                float width = 0f;
                bool widthProvided = false;
                if (colMap.TryGetValue("width", out var w))
                {
                    try { width = Convert.ToSingle(w); widthProvided = true; } catch { width = 0f; }
                }
                float minWidth = 0f;
                bool minWidthProvided = false;
                if (colMap.TryGetValue("minWidth", out var mw))
                {
                    try { minWidth = Convert.ToSingle(mw); minWidthProvided = true; } catch { }
                }
                float maxWidth = 0f;
                bool maxWidthProvided = false;
                if (colMap.TryGetValue("maxWidth", out var xw))
                {
                    try { maxWidth = Convert.ToSingle(xw); maxWidthProvided = true; } catch { maxWidth = 0f; }
                }
                bool resizable = true;
                bool resizableProvided = false;
                if (colMap.TryGetValue("resizable", out var rz) && rz is bool rb)
                {
                    resizable = rb; resizableProvided = true;
                }
                bool stretchable = true;
                bool stretchableProvided = false;
                if (colMap.TryGetValue("stretchable", out var st) && st is bool sb)
                {
                    stretchable = sb; stretchableProvided = true;
                }

                var column = new Column { title = title };
                if (!string.IsNullOrEmpty(name)) column.name = name;

                Column prev = null;
                if (!string.IsNullOrEmpty(name)) existingByName.TryGetValue(name, out prev);
                if (prev == null && index < existingByIndex.Count) prev = existingByIndex[index];

                if (widthProvided) column.width = width; else if (prev != null) column.width = prev.width;
                if (minWidthProvided) column.minWidth = minWidth; else if (prev != null) column.minWidth = prev.minWidth;
                if (maxWidthProvided) column.maxWidth = maxWidth; else if (prev != null) column.maxWidth = prev.maxWidth;
                if (resizableProvided) column.resizable = resizable; else if (prev != null) column.resizable = prev.resizable;
                if (stretchableProvided) column.stretchable = stretchable; else if (prev != null) column.stretchable = prev.stretchable;

                column.makeCell = () =>
                {
                    var ve = new VisualElement();
                    ve.style.flexGrow = 1;
                    ve.userData = new VNodeHostRenderer(GetCellHostContext(), ve);
                    return ve;
                };
                int capturedIndex = colIndex;
                column.bindCell = (ve, rowIndex) =>
                {
                    var rr = ve.userData as IVNodeHostRenderer;
                    if (rr == null)
                    {
                        rr = new VNodeHostRenderer(GetCellHostContext(), ve);
                        ve.userData = rr;
                    }
                    object item = null;
                    if (view.itemsSource is IList il && rowIndex >= 0 && rowIndex < il.Count)
                    {
                        item = il[rowIndex];
                    }
                    var fnList = parts.CellFns;
                    Func<int, object, VirtualNode> activeFn = null;
                    if (fnList != null && capturedIndex >= 0 && capturedIndex < fnList.Count)
                        activeFn = fnList[capturedIndex];
                    if (activeFn != null)
                    {
                        var vnode = activeFn(rowIndex, item);
                        rr.Render(EnsureVisualRoot(vnode));
                    }
                };
                column.unbindCell = (ve, i) => { (ve.userData as IVNodeHostRenderer)?.Unmount(); };
                view.columns.Add(column);
                index++;
                colIndex++;
            }

            try { view.Rebuild(); } catch { }
        }

        private sealed class ColSig
        {
            public string Name;
            public string Title;
        }

        private static (List<ColSig> sig, List<Func<int, object, VirtualNode>> fns) ExtractSignatureAndFns(IEnumerable cols)
        {
            var list = new List<ColSig>();
            var fns = new List<Func<int, object, VirtualNode>>();
            foreach (var co in cols)
            {
                if (co is not IDictionary<string, object> colMap)
                    continue;
                colMap.TryGetValue("name", out var n);
                colMap.TryGetValue("title", out var t);
                Func<int, object, VirtualNode> fn = null;
                if (colMap.TryGetValue("cell", out var c) && c is Func<int, object, VirtualNode> cf)
                    fn = cf;
                list.Add(new ColSig
                {
                    Name = n as string,
                    Title = t as string,
                });
                fns.Add(fn);
            }
            return (list, fns);
        }

        private static bool SignaturesEqual(List<ColSig> a, List<ColSig> b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null) return false;
            if (a.Count != b.Count) return false;
            for (int i = 0; i < a.Count; i++)
            {
                var x = a[i];
                var y = b[i];
                if (!string.Equals(x?.Name, y?.Name, StringComparison.Ordinal)) return false;
                if (!string.Equals(x?.Title, y?.Title, StringComparison.Ordinal)) return false;
            }
            return true;
        }

        private static void ApplySlots(
            MultiColumnListView view,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (properties == null)
                return;
            if (
                properties.TryGetValue("contentContainer", out var cc)
                && cc is Dictionary<string, object> ccMap
            )
            {
                PropsApplier.Apply(view.contentContainer, ccMap);
            }
            if (
                properties.TryGetValue("scrollView", out var sv)
                && sv is Dictionary<string, object> svMap
            )
            {
                var scroll = view.Q<ScrollView>();
                if (scroll != null)
                    PropsApplier.Apply(scroll, svMap);
            }
        }

        private static void ApplySlotsDiff(
            MultiColumnListView view,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            previous ??= new Dictionary<string, object>();
            next ??= new Dictionary<string, object>();
            previous.TryGetValue("contentContainer", out var prevCC);
            next.TryGetValue("contentContainer", out var nextCC);
            if (!ReferenceEquals(prevCC, nextCC) && nextCC is Dictionary<string, object> ccMap)
            {
                PropsApplier.Apply(view.contentContainer, ccMap);
            }
            previous.TryGetValue("scrollView", out var prevSV);
            next.TryGetValue("scrollView", out var nextSV);
            if (!ReferenceEquals(prevSV, nextSV) && nextSV is Dictionary<string, object> svMap)
            {
                var scroll = view.Q<ScrollView>();
                if (scroll != null)
                    PropsApplier.Apply(scroll, svMap);
            }
        }
    }
}
