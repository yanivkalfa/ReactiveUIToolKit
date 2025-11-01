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

        private TreeView tv;
        private MultiColumnTreeView mctv;
        private readonly HashSet<int> desiredExpanded = new HashSet<int>();
        private readonly Dictionary<int, bool> desiredExpandAll = new Dictionary<int, bool>();

        public void CreateGUI()
        {
            tv = new TreeView();
            tv.style.flexGrow = 1f;
            tv.SetRootItems(BuildRoots());
            tv.fixedItemHeight = 20f;
            tv.selectionType = SelectionType.None;

            // Seed desired set with middle parent
            desiredExpanded.Add(200);
            desiredExpandAll[200] = false;

            // Toolbar with a rebuild button to simulate a diff cycle
            var toolbar = new VisualElement();
            toolbar.style.flexDirection = FlexDirection.Row;
            var rebuild = new Button(() => RebuildAndReapply()) { text = "Rebuild" };
            toolbar.Add(rebuild);
            rootVisualElement.Add(toolbar);

            tv.itemExpandedChanged += (a) =>
            {
                // Update desired set; store last expandAll flag seen for this id
                if (a.isExpanded)
                    desiredExpanded.Add(a.id);
                else
                    desiredExpanded.Remove(a.id);
                desiredExpandAll[a.id] = a.isAppliedToAllChildren;
            };

            rootVisualElement.Add(tv);

            // Native MultiColumnTreeView sorting test
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
                mctv.sortingMode = ColumnSortingMode.Default;
                //mctv.Sort((a, b) => {
                //    Debug.Log("aa");
                //    return -1;
                //});
            }
            catch { }

            // Columns: Name and ID
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

            // Data
            mctv.SetRootItems(BuildMultiRoots());
            mctv.fixedItemHeight = 20f;
            mctv.selectionType = SelectionType.None;

            // Seed sort descriptors to show priority badges (1/2) immediately
            try
            {
                var scd = mctv.sortColumnDescriptions;
                scd.Clear();
                scd.Add(new SortColumnDescription("name", SortDirection.Ascending));
                scd.Add(new SortColumnDescription("id", SortDirection.Ascending));
            }
            catch { }

            mctv.columnSortingChanged += () =>
            {
                try
                {
                    var sorted = mctv.sortedColumns;
                    var desc = string.Join(
                        ", ",
                        sorted.Select(s => $"{s.columnName}:{s.direction}")
                    );
                    Debug.Log($"[MCTV] columnSortingChanged: {desc}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[MCTV] columnSortingChanged error: {ex}");
                }
            };

            rootVisualElement.Add(mctv);
        }

        private List<TreeViewItemData<object>> BuildRoots()
        {
            return new List<TreeViewItemData<object>>
            {
                new TreeViewItemData<object>(
                    100,
                    "Parent A",
                    new List<TreeViewItemData<object>>
                    {
                        new TreeViewItemData<object>(101, "Child A1", null),
                    }
                ),
                new TreeViewItemData<object>(
                    200,
                    "Parent B",
                    new List<TreeViewItemData<object>>
                    {
                        new TreeViewItemData<object>(201, "Child B1", null),
                    }
                ),
                new TreeViewItemData<object>(
                    300,
                    "Parent C",
                    new List<TreeViewItemData<object>>
                    {
                        new TreeViewItemData<object>(301, "Child C1", null),
                    }
                ),
            };
        }

        private void RebuildAndReapply()
        {
            if (tv == null)
                return;
            tv.SetRootItems(BuildRoots());
            foreach (var id in desiredExpanded)
            {
                bool all = desiredExpandAll.TryGetValue(id, out var val) && val;
                // Do not refresh per call; we will refresh once after all operations
                tv.ExpandItem(id, all, false);
            }
            tv.RefreshItems();
        }

        private sealed class RowData
        {
            public string Name;
            public string Id;
        }

        private List<TreeViewItemData<object>> BuildMultiRoots()
        {
            var list = new List<TreeViewItemData<object>>();
            list.Add(
                new TreeViewItemData<object>(
                    1000,
                    new RowData { Name = "Banana", Id = "B001" },
                    null
                )
            );
            list.Add(
                new TreeViewItemData<object>(
                    1002,
                    new RowData { Name = "Apple", Id = "A100" },
                    null
                )
            );
            list.Add(
                new TreeViewItemData<object>(
                    1004,
                    new RowData { Name = "Cherry", Id = "C010" },
                    null
                )
            );
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

            // If enumerable (but not string), enumerate contents
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

            // Properties
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

            // Fields
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

            // Methods (declared on type)
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
