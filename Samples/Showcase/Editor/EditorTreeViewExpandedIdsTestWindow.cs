using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Samples.Showcase.Editor
{
    public class EditorTreeViewExpandedIdsTestWindow : EditorWindow
    {
        [MenuItem("Window/ReactiveUITK/TV ExpandedIds Test")]
        public static void ShowWindow() =>
            GetWindow<EditorTreeViewExpandedIdsTestWindow>("TV ExpandedIds Test");

        private MultiColumnTreeView mctv;
        private readonly List<RowData> mctvRows = new List<RowData>();
        private int mctvNextPid = 1000;
        private MultiColumnTreeView mctvDefault;
        private readonly List<DefaultRowData> mctvDefaultRows = new List<DefaultRowData>();
        private int mctvDefaultNextPid = 3000;

        public void CreateGUI()
        {
            var header = new Label("MultiColumnTreeView Sorting Test")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginTop = 8,
                    marginBottom = 4,
                },
            };
            rootVisualElement.Add(header);

            mctv = new MultiColumnTreeView();
            mctv.style.flexGrow = 1f;
            try
            {
                mctv.sortingMode = ColumnSortingMode.Custom;
            }
            catch { }

            var colName = new Column
            {
                name = "name",
                title = "Name",
                sortable = true,
            };
            colName.makeCell = () => new Label();
            colName.bindCell = (ve, rowIndex) =>
            {
                var lbl = ve as Label;
                lbl.text = GetRowField(mctv, rowIndex, "Name");
            };
            var colId = new Column
            {
                name = "id",
                title = "ID",
                sortable = true,
            };
            colId.makeCell = () => new Label();
            colId.bindCell = (ve, rowIndex) =>
            {
                var lbl = ve as Label;
                lbl.text = GetRowField(mctv, rowIndex, "Id");
            };
            mctv.columns.Add(colName);
            mctv.columns.Add(colId);

            // No diagnostic logging; keep the example minimal

            if (mctvRows.Count == 0)
            {
                mctvRows.Add(new RowData { Name = "Banana", Id = "B001" });
                mctvRows.Add(new RowData { Name = "Apple", Id = "A100" });
                mctvRows.Add(new RowData { Name = "Cherry", Id = "C010" });
                int pid = mctvNextPid;
                foreach (var r in mctvRows)
                {
                    if (r.Pid == 0)
                    {
                        r.Pid = pid;
                        pid += 2;
                    }
                }
                mctvNextPid = pid;
            }
            mctv.SetRootItems(BuildMultiRoots());
            mctv.fixedItemHeight = 20f;
            mctv.selectionType = SelectionType.None;

            mctv.columnSortingChanged += () =>
            {
                var sorted = mctv.sortedColumns;
                var sortedList = (sorted as IList<SortColumnDescription>) ?? sorted?.ToList();
                if (sortedList != null && sortedList.Count > 0)
                {
                    var first = sortedList[0];
                    Comparison<RowData> cmp = null;
                    if (string.Equals(first.columnName, "name", StringComparison.Ordinal))
                        cmp = (a, b) =>
                            string.Compare(a?.Name, b?.Name, StringComparison.OrdinalIgnoreCase);
                    else if (string.Equals(first.columnName, "id", StringComparison.Ordinal))
                        cmp = (a, b) =>
                            string.Compare(a?.Id, b?.Id, StringComparison.OrdinalIgnoreCase);
                    if (cmp != null)
                    {
                        mctvRows.Sort(cmp);
                        if (first.direction == SortDirection.Descending)
                            mctvRows.Reverse();
                        mctv.SetRootItems(BuildMultiRoots());
                        mctv.RefreshItems();
                    }
                }
            };

            rootVisualElement.Add(mctv);
            var header2 = new Label("MultiColumnTreeView Default Sorting Test")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginTop = 12,
                    marginBottom = 4,
                },
            };
            rootVisualElement.Add(header2);

            mctvDefault = new MultiColumnTreeView();
            mctvDefault.style.flexGrow = 1f;
            try
            {
                mctvDefault.sortingMode = ColumnSortingMode.Default;
            }
            catch { }

            var defColName = new Column
            {
                name = "name",
                title = "Name",
                sortable = true,
            };
            defColName.makeCell = () => new Label();
            defColName.bindCell = (ve, rowIndex) =>
            {
                var lbl = ve as Label;
                lbl.text = GetRowField(mctvDefault, rowIndex, "RenderedName");
            };

            var defColId = new Column
            {
                name = "id",
                title = "ID",
                sortable = true,
            };
            defColId.makeCell = () => new Label();
            defColId.bindCell = (ve, rowIndex) =>
            {
                var lbl = ve as Label;
                lbl.text = GetRowField(mctvDefault, rowIndex, "Id");
            };
            mctvDefault.columns.Add(defColName);
            mctvDefault.columns.Add(defColId);

            // Hook column order logging for default-sorting view

            if (mctvDefaultRows.Count == 0)
            {
                mctvDefaultRows.Add(
                    new DefaultRowData
                    {
                        Name = "Alpha",
                        RenderedName = "Zeta",
                        Id = "ID-3",
                    }
                );
                mctvDefaultRows.Add(
                    new DefaultRowData
                    {
                        Name = "Beta",
                        RenderedName = "Alpha",
                        Id = "ID-2",
                    }
                );
                mctvDefaultRows.Add(
                    new DefaultRowData
                    {
                        Name = "Gamma",
                        RenderedName = "Mu",
                        Id = "ID-1",
                    }
                );
                int pid = mctvDefaultNextPid;
                foreach (var r in mctvDefaultRows)
                {
                    if (r.Pid == 0)
                    {
                        r.Pid = pid;
                        pid += 2;
                    }
                }
                mctvDefaultNextPid = pid;
            }
            mctvDefault.SetRootItems(BuildDefaultRoots());
            mctvDefault.fixedItemHeight = 20f;
            mctvDefault.selectionType = SelectionType.None;

            mctvDefault.columnSortingChanged += () =>
            {
                var sorted = mctvDefault.sortedColumns;
                var list = (sorted as IList<SortColumnDescription>) ?? sorted?.ToList();
                if (list != null && list.Count > 0)
                {
                    var first = list[0];
                    List<DefaultRowData> ordered = null;
                    if (string.Equals(first.columnName, "name", StringComparison.Ordinal))
                    {
                        ordered =
                            (first.direction == SortDirection.Descending)
                                ? mctvDefaultRows
                                    .OrderByDescending(
                                        r => r.Name,
                                        StringComparer.OrdinalIgnoreCase
                                    )
                                    .ToList()
                                : mctvDefaultRows
                                    .OrderBy(r => r.Name, StringComparer.OrdinalIgnoreCase)
                                    .ToList();
                    }
                    else if (string.Equals(first.columnName, "id", StringComparison.Ordinal))
                    {
                        ordered =
                            (first.direction == SortDirection.Descending)
                                ? mctvDefaultRows
                                    .OrderByDescending(r => r.Id, StringComparer.OrdinalIgnoreCase)
                                    .ToList()
                                : mctvDefaultRows
                                    .OrderBy(r => r.Id, StringComparer.OrdinalIgnoreCase)
                                    .ToList();
                    }
                    if (ordered != null)
                    {
                        mctvDefaultRows.Clear();
                        mctvDefaultRows.AddRange(ordered);
                        mctvDefault.SetRootItems(BuildDefaultRoots());
                        try
                        {
                            mctvDefault.RefreshItems();
                        }
                        catch { }
                    }
                }
            };

            rootVisualElement.Add(mctvDefault);

            // Add a manual dump button
            var dumpBtn = new Button(() => { }) { text = "Dump Column Orders" };
            rootVisualElement.Add(dumpBtn);
        }

        private sealed class RowData
        {
            public string Name;
            public string Id;
            public int Pid;
        }

        private sealed class DefaultRowData
        {
            public string Name;
            public string RenderedName;
            public string Id;
            public int Pid;
        }

        private List<TreeViewItemData<object>> BuildMultiRoots()
        {
            var list = new List<TreeViewItemData<object>>();
            foreach (var r in mctvRows)
            {
                list.Add(new TreeViewItemData<object>(r.Pid, r, null));
            }
            return list;
        }

        private List<TreeViewItemData<object>> BuildDefaultRoots()
        {
            var list = new List<TreeViewItemData<object>>();
            foreach (var r in mctvDefaultRows)
            {
                list.Add(new TreeViewItemData<object>(r.Pid, r, null));
            }
            return list;
        }

        private static string GetRowField(MultiColumnTreeView tv, int index, string field)
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
                    var obj = mi.MakeGenericMethod(typeof(object))
                        .Invoke(tv, new object[] { index });
                    if (obj != null)
                    {
                        var pi = obj.GetType()
                            .GetField(field, BindingFlags.Public | BindingFlags.Instance);
                        if (pi != null)
                            return pi.GetValue(obj)?.ToString();
                        var pp = obj.GetType()
                            .GetProperty(field, BindingFlags.Public | BindingFlags.Instance);
                        if (pp != null)
                            return pp.GetValue(obj)?.ToString();
                        return obj.ToString();
                    }
                }
            }
            catch { }
            return string.Empty;
        }

        // Diagnostics helpers removed
    }
}
