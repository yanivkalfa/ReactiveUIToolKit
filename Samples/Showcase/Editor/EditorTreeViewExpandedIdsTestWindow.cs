using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
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

        private static string DumpObject(object obj)
        {
            var sb = new StringBuilder();
            if (obj == null)
            {
                sb.AppendLine("<null>");
                return sb.ToString();
            }

            var t = obj.GetType();
            sb.AppendLine($"Object Type: {t.FullName}");

            if (obj is System.Collections.IEnumerable en && !(obj is string))
            {
                sb.AppendLine("Enumerable contents:");
                int idx = 0;
                foreach (var it in en)
                {
                    sb.AppendLine($" [{idx}] {FormatValue(it)}");
                    idx++;
                    if (idx >= 200)
                    {
                        sb.AppendLine(" ...truncated...");
                        break;
                    }
                }
                if (idx == 0)
                    sb.AppendLine(" (empty)");
            }

            try
            {
                var props = t.GetProperties(
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic
                );
                if (props.Length > 0)
                {
                    sb.AppendLine("Properties:");
                    foreach (var p in props.OrderBy(p => p.Name))
                    {
                        object val = null;
                        try
                        {
                            val = p.GetValue(obj);
                        }
                        catch (Exception ex)
                        {
                            val = $"<err: {ex.Message}>";
                        }
                        sb.AppendLine($" {p.PropertyType.Name} {p.Name} = {FormatValue(val)}");
                    }
                }
            }
            catch { }

            try
            {
                var fields = t.GetFields(
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic
                );
                if (fields.Length > 0)
                {
                    sb.AppendLine("Fields:");
                    foreach (var f in fields.OrderBy(f => f.Name))
                    {
                        object val = null;
                        try
                        {
                            val = f.GetValue(obj);
                        }
                        catch (Exception ex)
                        {
                            val = $"<err: {ex.Message}>";
                        }
                        sb.AppendLine($" {f.FieldType.Name} {f.Name} = {FormatValue(val)}");
                    }
                }
            }
            catch { }

            try
            {
                var methods = t.GetMethods(
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly
                );
                if (methods.Length > 0)
                {
                    sb.AppendLine("Methods:");
                    foreach (var m in methods.OrderBy(m => m.Name))
                    {
                        var parameters = string.Join(
                            ", ",
                            m.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name)
                        );
                        sb.AppendLine($" {m.ReturnType.Name} {m.Name}({parameters})");
                    }
                }
            }
            catch { }

            return sb.ToString();
        }

        private static string FormatValue(object v)
        {
            if (v == null)
                return "<null>";
            try
            {
                if (v is string)
                    return '"' + v.ToString() + '"';
                return v.ToString();
            }
            catch
            {
                return "<err>";
            }
        }
    }
}
