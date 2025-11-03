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
    public sealed class MultiColumnListViewElementAdapter
        : StatefulElementAdapter<MultiColumnListView, MultiColumnListViewElementAdapter.Cached>
    {
        public sealed class Cached : ISortState, IColumnLayoutState, IAdjustmentSuspendState
        {
            public IList LastItems;
            internal List<ColumnSignature> LastColSignature;
            public List<Func<int, object, VirtualNode>> CellFns;

            public List<(
                string name,
                SortDirection direction,
                int index
            )> SortedColumns { get; set; } = new();
            public Delegate UserSortNotify { get; set; }
            public Action InternalSortHandler { get; set; }
            public IElementStateTracker<MultiColumnListView, Cached> SortTracker =
                new MultiColumnSortTracker<MultiColumnListView, Cached>();

            public Dictionary<string, float> ColumnWidths { get; set; } = new();
            public Dictionary<string, bool> ColumnVisibility { get; set; } = new();
            public Dictionary<string, int> ColumnDisplayIndex { get; set; } = new();
            public IElementStateTracker<MultiColumnListView, Cached> LayoutTracker =
                new MultiColumnLayoutTracker<MultiColumnListView, Cached>();

            // Suspend heavy updates during header interactions
            public bool IsAdjusting { get; set; }
            public bool HeaderWired { get; set; }
            public IReadOnlyDictionary<string, object> PendingPrev { get; set; }
            public IReadOnlyDictionary<string, object> PendingNext { get; set; }
            public IElementStateTracker<MultiColumnListView, Cached> AdjustmentTracker =
                new MultiColumnAdjustmentTracker<MultiColumnListView, Cached>(
                    new MultiColumnHeaderOps<MultiColumnListView>(),
                    ApplyAdjustmentFlush
                );
        }

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

        public override VisualElement Create() =>
            GlobalVisualElementPool.Get<MultiColumnListView>();

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
            var parts = GetState(view);
            parts.AdjustmentTracker.Attach(view, parts, properties);
            if (parts.IsAdjusting)
            {
                parts.AdjustmentTracker.Reapply(view, parts, null, properties);
                return;
            }

            if (properties.TryGetValue("items", out var itemsObj))
            {
                var incoming = NormalizeItems(itemsObj);
                if (!ReferenceEquals(parts.LastItems, incoming))
                {
                    parts.LastItems = incoming;
                    view.itemsSource = incoming as IList;
                    try
                    {
                        view.Rebuild();
                    }
                    catch { }
                }
            }
            else if (parts.LastItems != null)
            {
                parts.LastItems = null;
                view.itemsSource = null;
                try
                {
                    view.Rebuild();
                }
                catch { }
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

            // sortingMode (optional)
            TryApplyProp<object>(properties, "sortingMode", m => ApplySortingMode(view, m));

            // Columns: rebuild only if semantic signature changes
            if (properties.TryGetValue("columns", out var colsObj) && colsObj is IEnumerable cols)
            {
                var (newSig, newFns) = ColumnSignatureUtil.Extract(cols);
                if (!SignaturesEqual(parts.LastColSignature, newSig))
                {
                    parts.CellFns = newFns;
                    RebuildColumnsPreservingState(view, cols, parts);
                    parts.LastColSignature = newSig;
                }
                else
                {
                    // Update cell delegates and refresh realized rows to pick up new closures
                    // Preserve state by not unmounting during unbindCell
                    parts.CellFns = newFns;
                    try
                    {
                        view.RefreshItems();
                    }
                    catch { }
                }
            }

            ApplySlots(view, properties);

            // trackers
            parts.LayoutTracker.Attach(view, parts, properties);
            parts.LayoutTracker.Reapply(view, parts, null, properties);
            parts.SortTracker.Attach(view, parts, properties);
            parts.SortTracker.Reapply(view, parts, null, properties);
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

            var parts = GetState(view);
            parts.AdjustmentTracker.Attach(view, parts, next);
            if (parts.IsAdjusting)
            {
                parts.AdjustmentTracker.Reapply(view, parts, previous, next);
                return;
            }

            previous.TryGetValue("items", out var prevItemsObj);
            next.TryGetValue("items", out var nextItemsObj);
            var prevItems = NormalizeItems(prevItemsObj);
            var nextItems = NormalizeItems(nextItemsObj);
            if (!ReferenceEquals(prevItems, nextItems))
            {
                parts.LastItems = nextItems;
                view.itemsSource = nextItems;
                try
                {
                    view.Rebuild();
                }
                catch { }
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

            TryDiffProp<object>(previous, next, "sortingMode", m => ApplySortingMode(view, m));

            previous.TryGetValue("columns", out var prevCols);
            next.TryGetValue("columns", out var nextCols);
            if (nextCols is IEnumerable ncols)
            {
                var (newSig, newFns) = ColumnSignatureUtil.Extract(ncols);
                if (!SignaturesEqual(parts.LastColSignature, newSig))
                {
                    parts.CellFns = newFns;
                    RebuildColumnsPreservingState(view, ncols, parts);
                    parts.LastColSignature = newSig;
                }
                else
                {
                    // Update delegate references and refresh realized rows so closures update
                    parts.CellFns = newFns;
                    try
                    {
                        view.RefreshItems();
                    }
                    catch { }
                }
            }

            ApplySlotsDiff(view, previous, next);

            // trackers
            parts.LayoutTracker.Reapply(view, parts, previous, next);
            parts.SortTracker.Reapply(view, parts, previous, next);
            PropsApplier.ApplyDiff(element, previous, next);
        }

        private static void ApplySortingMode(MultiColumnListView view, object mode)
        {
            if (view == null || mode == null)
                return;
            try
            {
                var pi = typeof(MultiColumnListView).GetProperty(
                    "sortingMode",
                    System.Reflection.BindingFlags.Instance
                        | System.Reflection.BindingFlags.Public
                        | System.Reflection.BindingFlags.NonPublic
                );
                if (pi == null)
                    return;
                var enumType = pi.PropertyType;
                object val = null;
                if (mode.GetType().IsEnum && mode.GetType().Name == enumType.Name)
                    val = mode;
                else if (mode is string s)
                {
                    try
                    {
                        val = Enum.Parse(enumType, s, true);
                    }
                    catch { }
                }
                else if (mode is int i)
                {
                    try
                    {
                        val = Enum.ToObject(enumType, i);
                    }
                    catch { }
                }
                if (val != null)
                    pi.SetValue(view, val);
            }
            catch { }
        }

        private static void RebuildColumnsPreservingState(
            MultiColumnListView view,
            IEnumerable newCols,
            Cached parts
        )
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
                if (co is not IDictionary<string, object> colMap)
                {
                    index++;
                    continue;
                }
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
                    try
                    {
                        width = Convert.ToSingle(w);
                        widthProvided = true;
                    }
                    catch
                    {
                        width = 0f;
                    }
                }
                float minWidth = 0f;
                bool minWidthProvided = false;
                if (colMap.TryGetValue("minWidth", out var mw))
                {
                    try
                    {
                        minWidth = Convert.ToSingle(mw);
                        minWidthProvided = true;
                    }
                    catch { }
                }
                float maxWidth = 0f;
                bool maxWidthProvided = false;
                if (colMap.TryGetValue("maxWidth", out var xw))
                {
                    try
                    {
                        maxWidth = Convert.ToSingle(xw);
                        maxWidthProvided = true;
                    }
                    catch
                    {
                        maxWidth = 0f;
                    }
                }
                bool resizable = true;
                bool resizableProvided = false;
                if (colMap.TryGetValue("resizable", out var rz) && rz is bool rb)
                {
                    resizable = rb;
                    resizableProvided = true;
                }
                bool stretchable = true;
                bool stretchableProvided = false;
                if (colMap.TryGetValue("stretchable", out var st) && st is bool sb)
                {
                    stretchable = sb;
                    stretchableProvided = true;
                }

                var column = new Column { title = title };
                if (!string.IsNullOrEmpty(name))
                    column.name = name;

                Column prev = null;
                if (!string.IsNullOrEmpty(name))
                    existingByName.TryGetValue(name, out prev);
                if (prev == null && index < existingByIndex.Count)
                    prev = existingByIndex[index];

                if (widthProvided)
                    column.width = width;
                else if (prev != null)
                    column.width = prev.width;
                if (minWidthProvided)
                    column.minWidth = minWidth;
                else if (prev != null)
                    column.minWidth = prev.minWidth;
                if (maxWidthProvided)
                    column.maxWidth = maxWidth;
                else if (prev != null)
                    column.maxWidth = prev.maxWidth;
                if (resizableProvided)
                    column.resizable = resizable;
                else if (prev != null)
                    column.resizable = prev.resizable;
                if (stretchableProvided)
                    column.stretchable = stretchable;
                else if (prev != null)
                    column.stretchable = prev.stretchable;

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
                // Preserve existing VNodeHostRenderer across unbind to avoid remounting stateful subtrees
                column.unbindCell = (ve, i) => { };
                view.columns.Add(column);
                index++;
                colIndex++;
            }

            try
            {
                view.Rebuild();
            }
            catch { }
        }

        private static bool SignaturesEqual(List<ColumnSignature> a, List<ColumnSignature> b)
        {
            return ColumnSignatureUtil.Equal(a, b);
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

        private static void ApplyAdjustmentFlush(
            MultiColumnListView view,
            Cached parts,
            IReadOnlyDictionary<string, object> prev,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (view == null || parts == null)
                return;
            try
            {
                view.schedule?.Execute(() =>
                    {
                        try
                        {
                            var p = prev ?? new Dictionary<string, object>();
                            var n = next ?? new Dictionary<string, object>();

                            // items diff
                            p.TryGetValue("items", out var prevItemsObj);
                            n.TryGetValue("items", out var nextItemsObj);
                            var prevItems = NormalizeItems(prevItemsObj);
                            var nextItems = NormalizeItems(nextItemsObj);
                            if (!ReferenceEquals(prevItems, nextItems))
                            {
                                parts.LastItems = nextItems;
                                view.itemsSource = nextItems;
                                try
                                {
                                    view.Rebuild();
                                }
                                catch { }
                            }

                            // columns diff
                            p.TryGetValue("columns", out var prevCols);
                            n.TryGetValue("columns", out var nextCols);
                            if (nextCols is IEnumerable ncols)
                            {
                                var (newSig, newFns) = ColumnSignatureUtil.Extract(ncols);
                                if (!SignaturesEqual(parts.LastColSignature, newSig))
                                {
                                    parts.CellFns = newFns;
                                    RebuildColumnsPreservingState(view, ncols, parts);
                                    parts.LastColSignature = newSig;
                                }
                                else
                                {
                                    parts.CellFns = newFns;
                                    try
                                    {
                                        view.RefreshItems();
                                    }
                                    catch { }
                                }
                            }

                            // trackers + slots + props
                            parts.LayoutTracker.Reapply(view, parts, p, n);
                            parts.SortTracker.Reapply(view, parts, p, n);
                            ApplySlotsDiff(view, p, n);
                            try
                            {
                                PropsApplier.ApplyDiff(view, p, n);
                            }
                            catch { }
                        }
                        catch { }
                    })
                    ?.ExecuteLater(0);
            }
            catch { }
        }
    }
}
