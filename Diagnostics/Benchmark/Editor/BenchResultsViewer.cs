#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ReactiveUITK.Bench.EditorTools
{
    public class BenchResultsViewer : EditorWindow
    {
        [MenuItem("ReactiveUITK/Diagnostics/Benchmark/Results Viewer")]
        public static void Open()
        {
            var w = GetWindow<BenchResultsViewer>();
            w.titleContent = new GUIContent("RUITK Bench Viewer");
            w.minSize = new Vector2(820, 420);
            w.Show();
        }

        private const string Pref_LastRunFolder = "RUITK_BenchViewer_LastRunFolder";

        private readonly List<RunEntry> _runs = new();
        private readonly List<Item> _items = new();
        private Vector2 _leftScroll,
            _summaryScroll;

        private bool _autoY = true;
        private float _yMax = 200f;
        private bool _showMinMaxBands = true;
        private bool _showP95 = true;
        private bool _normalizeXToDuration = true;
        private bool _averageRuns = false;

        private bool _onlyEditor = false;
        private bool _onlyRuntime = false;
        private string _search = "";

        private readonly List<Item> _averagedItems = new();
        private bool _averagedDirty = true;

        private class RunEntry
        {
            public string path;
            public string name;
            public bool selected = true;
            public int fileCount;
        }

        private class Item
        {
            public string path;
            public BenchScenarioFile file;
            public bool visible = true;
            public Color color;
            public string runName;
        }

        private static readonly Color[] kColors =
        {
            new(0.24f, 0.67f, 0.89f),
            new(0.93f, 0.49f, 0.19f),
            new(0.51f, 0.74f, 0.37f),
            new(0.80f, 0.40f, 0.80f),
            new(0.93f, 0.76f, 0.27f),
            new(0.35f, 0.82f, 0.84f),
            new(0.92f, 0.28f, 0.28f),
            new(0.56f, 0.56f, 0.56f),
        };

        private void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawLeftPanel();
                DrawRightPanel();
            }
        }

        private void DrawLeftPanel()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(340)))
            {
                EditorGUILayout.LabelField("Runs", EditorStyles.boldLabel);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Add Run…", GUILayout.Width(90)))
                    {
                        var start = GetDefaultStartFolder();
                        var folder = EditorUtility.OpenFolderPanel(
                            "Select run folder (contains JSON files)",
                            start,
                            ""
                        );
                        if (!string.IsNullOrEmpty(folder))
                        {
                            TryAddRun(folder);
                        }
                    }
                    if (GUILayout.Button("Add All…", GUILayout.Width(90)))
                    {
                        var start = GetDefaultStartFolder();
                        var parent = EditorUtility.OpenFolderPanel(
                            "Select parent folder (each subfolder = one run)",
                            start,
                            ""
                        );
                        if (!string.IsNullOrEmpty(parent))
                        {
                            TryAddAllRuns(parent);
                        }
                    }
                    if (GUILayout.Button("Clear", GUILayout.Width(60)))
                    {
                        _runs.Clear();
                        _items.Clear();
                        _averagedItems.Clear();
                        _averagedDirty = true;
                    }
                    GUILayout.FlexibleSpace();
                }

                _leftScroll = EditorGUILayout.BeginScrollView(_leftScroll, GUILayout.Height(140));
                foreach (var r in _runs)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        var sel = EditorGUILayout.Toggle(r.selected, GUILayout.Width(18));
                        if (sel != r.selected)
                        {
                            r.selected = sel;
                            RebuildItems();
                        }

                        if (
                            GUILayout.Button($"{r.name}  [{r.fileCount} files]", EditorStyles.label)
                        )
                        {
                            var rel = ToAssetPath(r.path);
                            if (!string.IsNullOrEmpty(rel))
                            {
                                var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(rel);
                                if (obj)
                                {
                                    EditorGUIUtility.PingObject(obj);
                                }
                            }
                        }
                    }
                }
                EditorGUILayout.EndScrollView();

                GUILayout.Space(6);
                EditorGUILayout.LabelField("Display", EditorStyles.boldLabel);
                _autoY = EditorGUILayout.ToggleLeft("Auto Y-Axis", _autoY);
                using (new EditorGUI.DisabledScope(_autoY))
                {
                    _yMax = EditorGUILayout.FloatField("Y Max (FPS)", Mathf.Max(10f, _yMax));
                }
                _showMinMaxBands = EditorGUILayout.ToggleLeft(
                    "Show min/max bands",
                    _showMinMaxBands
                );
                _showP95 = EditorGUILayout.ToggleLeft("Show P95 marker", _showP95);
                _normalizeXToDuration = EditorGUILayout.ToggleLeft(
                    "Normalize X by duration",
                    _normalizeXToDuration
                );
                var prevAvg = _averageRuns;
                _averageRuns = EditorGUILayout.ToggleLeft(
                    "Average across runs",
                    _averageRuns
                );
                if (prevAvg != _averageRuns)
                {
                    _averagedDirty = true;
                }

                GUILayout.Space(6);
                EditorGUILayout.LabelField("Filters", EditorStyles.boldLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    _onlyEditor = GUILayout.Toggle(_onlyEditor, "Only Editor");
                    _onlyRuntime = GUILayout.Toggle(_onlyRuntime, "Only Runtime");
                }
                _search = EditorGUILayout.TextField("Search (name)", _search);
            }
        }

        private void DrawRightPanel()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField("Chart", EditorStyles.boldLabel);
                var rect = GUILayoutUtility.GetRect(
                    10,
                    300,
                    GUILayout.ExpandWidth(true),
                    GUILayout.MinHeight(260)
                );
                EditorGUI.DrawRect(rect, new Color(0.10f, 0.10f, 0.10f));

                var displayItems = GetDisplayItems();

                float maxX = 1f,
                    globalYMax = 60f;
                foreach (var it in displayItems.Where(VisibleAndPasses))
                {
                    var s = it.file.scenario;
                    var secs = it.file.perSecond;
                    if (secs == null || secs.Count == 0)
                    {
                        continue;
                    }

                    var lastX = _normalizeXToDuration ? s.durationSec : (secs.Count - 1);
                    if (lastX > maxX)
                    {
                        maxX = lastX;
                    }

                    var localMax = Mathf.Max(
                        it.file.scenarioSummary.fpsMax,
                        secs.Max(ps => Mathf.Max(ps.fpsAvg, ps.fpsMax))
                    );
                    if (localMax > globalYMax)
                    {
                        globalYMax = localMax;
                    }
                }
                float yMax = _autoY ? NeatCeil(globalYMax) : _yMax;

                DrawAxes(rect, maxX, yMax);

                foreach (var it in displayItems.Where(VisibleAndPasses))
                {
                    Plot(rect, it, maxX, yMax);
                }

                GUILayout.Space(8);

                _summaryScroll = EditorGUILayout.BeginScrollView(_summaryScroll);
                foreach (var it in displayItems.Where(PassesFilters))
                {
                    DrawSummaryRow(it);
                    GUILayout.Space(4);
                }
                EditorGUILayout.EndScrollView();
            }
        }

        private List<Item> GetDisplayItems()
        {
            if (!_averageRuns)
            {
                return _items;
            }

            if (!_averagedDirty)
            {
                return _averagedItems;
            }

            _averagedItems.Clear();
            _averagedDirty = false;

            var groups = _items
                .Where(it => it.file.scenario != null)
                .GroupBy(it => it.file.scenario.name ?? "")
                .OrderBy(g =>
                    g.Min(it => it.file.scenario.index)
                );

            int colorIdx = 0;
            foreach (var group in groups)
            {
                var list = group.ToList();
                if (list.Count == 0)
                {
                    continue;
                }

                var first = list[0].file;
                int maxBins = list.Max(it => it.file.perSecond?.Count ?? 0);
                var avgBins = new List<BenchPerSecondBucket>(maxBins);

                for (int b = 0; b < maxBins; b++)
                {
                    float sumFpsAvg = 0, sumFpsMin = 0, sumFpsMax = 0;
                    long sumMonoMax = 0, sumAllocAvg = 0, sumAllocMax = 0;
                    int sumSamples = 0, sumGc = 0, sumWorkUnits = 0;
                    float sumRecMs = 0, sumRecMsMax = 0;
                    int contributors = 0;

                    foreach (var it in list)
                    {
                        var ps = it.file.perSecond;
                        if (ps == null || b >= ps.Count)
                        {
                            continue;
                        }

                        var bucket = ps[b];
                        sumFpsAvg += bucket.fpsAvg;
                        sumFpsMin += bucket.fpsMin;
                        sumFpsMax += bucket.fpsMax;
                        sumSamples += bucket.sampleCount;
                        sumMonoMax += bucket.monoUsedMax;
                        sumGc += bucket.gcCollections;
                        sumAllocAvg += bucket.renderAllocBytesAvg;
                        sumAllocMax += bucket.renderAllocBytesMax;
                        sumRecMs += bucket.reconcilerMsAvg;
                        sumRecMsMax += bucket.reconcilerMsMax;
                        sumWorkUnits += bucket.workUnitsTotal;
                        contributors++;
                    }

                    if (contributors == 0)
                    {
                        continue;
                    }

                    float n = contributors;
                    avgBins.Add(
                        new BenchPerSecondBucket
                        {
                            sec = b,
                            fpsAvg = sumFpsAvg / n,
                            fpsMin = sumFpsMin / n,
                            fpsMax = sumFpsMax / n,
                            sampleCount = sumSamples / contributors,
                            monoUsedMax = sumMonoMax / contributors,
                            gcCollections = sumGc / contributors,
                            renderAllocBytesAvg = sumAllocAvg / contributors,
                            renderAllocBytesMax = sumAllocMax / contributors,
                            reconcilerMsAvg = sumRecMs / n,
                            reconcilerMsMax = sumRecMsMax / n,
                            workUnitsTotal = sumWorkUnits / contributors,
                        }
                    );
                }

                float cnt = list.Count;
                var avgSummary = new BenchScenarioSummary
                {
                    fpsAvg = list.Sum(it => it.file.scenarioSummary.fpsAvg) / cnt,
                    fpsP95 = list.Sum(it => it.file.scenarioSummary.fpsP95) / cnt,
                    fpsMin = list.Sum(it => it.file.scenarioSummary.fpsMin) / cnt,
                    fpsMax = list.Sum(it => it.file.scenarioSummary.fpsMax) / cnt,
                    samplesTotal = (int)(list.Sum(it => it.file.scenarioSummary.samplesTotal) / cnt),
                    secondsTotal =
                        (int)(list.Sum(it => it.file.scenarioSummary.secondsTotal) / cnt),
                    monoUsedMax = (long)(list.Sum(it => it.file.scenarioSummary.monoUsedMax) / cnt),
                    gcCollectionsTotal =
                        (int)(list.Sum(it => it.file.scenarioSummary.gcCollectionsTotal) / cnt),
                    renderAllocBytesAvg =
                        (long)(list.Sum(it => it.file.scenarioSummary.renderAllocBytesAvg) / cnt),
                    renderAllocBytesMax =
                        (long)(list.Sum(it => it.file.scenarioSummary.renderAllocBytesMax) / cnt),
                    reconcilerMsAvg =
                        list.Sum(it => it.file.scenarioSummary.reconcilerMsAvg) / cnt,
                    reconcilerMsMax =
                        list.Sum(it => it.file.scenarioSummary.reconcilerMsMax) / cnt,
                    workUnitsTotal =
                        (int)(list.Sum(it => it.file.scenarioSummary.workUnitsTotal) / cnt),
                };

                var avgFile = new BenchScenarioFile
                {
                    runId = $"avg_{list.Count}_runs",
                    suite = first.suite,
                    env = first.env,
                    scenario = new BenchScenarioHeader
                    {
                        index = first.scenario.index,
                        name = first.scenario.name,
                        durationSec = list.Average(it => it.file.scenario.durationSec),
                        startedAt = first.scenario.startedAt,
                        endedAt = first.scenario.endedAt,
                    },
                    scenarioSummary = avgSummary,
                    perSecond = avgBins,
                };

                _averagedItems.Add(
                    new Item
                    {
                        path = "",
                        file = avgFile,
                        visible = true,
                        color = kColors[colorIdx++ % kColors.Length],
                        runName = $"avg ({list.Count} runs)",
                    }
                );
            }

            return _averagedItems;
        }

        private void DrawSummaryRow(Item it)
        {
            var s = it.file.scenario;
            var sum = it.file.scenarioSummary;

            var row = EditorGUILayout.GetControlRect(
                false,
                EditorGUIUtility.singleLineHeight * 1.6f
            );
            GUI.Box(row, GUIContent.none, EditorStyles.helpBox);

            var chk = new Rect(row.x + 6, row.y + 5, 16, 16);
            it.visible = GUI.Toggle(chk, it.visible, GUIContent.none);

            var dot = new Rect(chk.xMax + 4, row.y + 5, 16, 16);
            EditorGUI.DrawRect(dot, it.color);

            var nameRect = new Rect(dot.xMax + 6, row.y + 3, 260, row.height - 6);
            var style = new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = it.color } };
            GUI.Label(nameRect, $"#{s.index} {s.name}");

            var info =
                $"fps avg {sum.fpsAvg:F1}  p95 {sum.fpsP95:F1}  min {sum.fpsMin:F1}  max {sum.fpsMax:F1}";
            GUI.Label(new Rect(nameRect.xMax + 8, row.y + 3, 360, row.height - 6), info);

            var tail =
                $"samples {sum.samplesTotal}  secs {sum.secondsTotal}  GC {sum.gcCollectionsTotal}  renderAlloc {Fmt(sum.renderAllocBytesAvg)}/frame  reconciler {sum.reconcilerMsAvg:F1}ms  mem {Fmt(sum.monoUsedMax)}  — run: {it.runName}";
            GUI.Label(
                new Rect(row.xMax - 580, row.y + 3, 572, row.height - 6),
                tail,
                EditorStyles.miniLabel
            );

            var e = Event.current;
            if (e.type == EventType.MouseDown && e.clickCount == 2 && row.Contains(e.mousePosition))
            {
                bool alreadySolo = it.visible && _items.Count(VisibleAndPasses) == 1;
                if (alreadySolo)
                {
                    foreach (var x in _items.Where(PassesFilters))
                    {
                        x.visible = true;
                    }
                }
                else
                {
                    foreach (var x in _items)
                    {
                        x.visible = false;
                    }
                    it.visible = true;
                }
                e.Use();
                Repaint();
            }
        }

        private void Plot(Rect rect, Item it, float maxX, float yMax)
        {
            var secs = it.file.perSecond;
            if (secs == null || secs.Count == 0)
            {
                return;
            }

            Handles.BeginGUI();

            if (_showMinMaxBands)
            {
                for (int i = 0; i < secs.Count; i++)
                {
                    var ps = secs[i];
                    float x0 = MapX(rect, i, maxX);
                    float x1 = MapX(rect, i + 1, maxX);
                    var yMin = MapY(rect, ps.fpsMin, yMax);
                    var yMaxV = MapY(rect, ps.fpsMax, yMax);
                    EditorGUI.DrawRect(
                        new Rect(x0, yMaxV, x1 - x0, yMin - yMaxV),
                        new Color(it.color.r, it.color.g, it.color.b, 0.12f)
                    );
                }
            }

            Vector3? prev = null;
            Handles.color = it.color;
            for (int i = 0; i < secs.Count; i++)
            {
                var ps = secs[i];
                var p = new Vector3(MapX(rect, i + 0.5f, maxX), MapY(rect, ps.fpsAvg, yMax), 0);
                if (prev.HasValue)
                {
                    Handles.DrawAAPolyLine(2f, new[] { prev.Value, p });
                }
                prev = p;
            }

            if (_showP95)
            {
                var y = MapY(rect, it.file.scenarioSummary.fpsP95, yMax);
                Handles.color = new Color(it.color.r, it.color.g, it.color.b, 0.6f);
                Handles.DrawLine(new Vector3(rect.x, y), new Vector3(rect.xMax, y));
            }

            Handles.EndGUI();
        }

        private void DrawAxes(Rect r, float maxX, float yMax)
        {
            Handles.BeginGUI();
            Handles.color = new Color(1, 1, 1, 0.15f);
            Handles.DrawAAPolyLine(
                2f,
                new[]
                {
                    new Vector3(r.x, r.y),
                    new Vector3(r.xMax, r.y),
                    new Vector3(r.xMax, r.yMax),
                    new Vector3(r.x, r.yMax),
                    new Vector3(r.x, r.y),
                }
            );

            int yTicks = 5;
            for (int i = 0; i <= yTicks; i++)
            {
                float v = (yMax / yTicks) * i;
                float y = MapY(r, v, yMax);
                Handles.color = new Color(1, 1, 1, 0.06f);
                Handles.DrawLine(new Vector3(r.x, y), new Vector3(r.xMax, y));
                GUI.Label(new Rect(r.x + 4, y - 8, 80, 16), $"{v:F0}", EditorStyles.miniLabel);
            }

            Handles.color = new Color(1, 1, 1, 0.06f);
            int xTicks = 10;
            for (int i = 0; i <= xTicks; i++)
            {
                float t = (maxX / xTicks) * i;
                float x = r.x + (t / maxX) * r.width;
                Handles.DrawLine(new Vector3(x, r.y), new Vector3(x, r.yMax));
                GUI.Label(new Rect(x + 2, r.yMax - 18, 60, 16), $"{t:F0}s", EditorStyles.miniLabel);
            }
            Handles.EndGUI();
        }

        private void TryAddRun(string folder, bool silent = false)
        {
            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
            {
                return;
            }

            var jsons = Directory.GetFiles(folder, "*.json", SearchOption.TopDirectoryOnly);
            if (jsons.Length == 0)
            {
                if (!silent)
                {
                    EditorUtility.DisplayDialog(
                        "No JSON files",
                        "Pick a folder that contains per-scenario JSON files.",
                        "OK"
                    );
                }
                return;
            }

            if (_runs.Any(r => string.Equals(r.path, folder, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            _runs.Add(
                new RunEntry
                {
                    path = folder,
                    name = new DirectoryInfo(folder).Name,
                    selected = true,
                    fileCount = jsons.Length,
                }
            );

            EditorPrefs.SetString(Pref_LastRunFolder, folder);

            RebuildItems();
        }

        private void TryAddAllRuns(string parentFolder)
        {
            if (string.IsNullOrEmpty(parentFolder) || !Directory.Exists(parentFolder))
            {
                return;
            }

            // If the parent itself contains JSON files, treat it as a single run
            var parentJsons = Directory.GetFiles(
                parentFolder,
                "*.json",
                SearchOption.TopDirectoryOnly
            );
            if (parentJsons.Length > 0)
            {
                TryAddRun(parentFolder, silent: true);
            }

            // Add each subfolder that contains JSON files
            var subdirs = Directory.GetDirectories(parentFolder);
            Array.Sort(subdirs);
            int added = 0;
            foreach (var sub in subdirs)
            {
                var jsons = Directory.GetFiles(sub, "*.json", SearchOption.TopDirectoryOnly);
                if (jsons.Length > 0)
                {
                    TryAddRun(sub, silent: true);
                    added++;
                }
            }

            if (added == 0 && parentJsons.Length == 0)
            {
                EditorUtility.DisplayDialog(
                    "No runs found",
                    "No subfolders with JSON files were found in the selected folder.",
                    "OK"
                );
            }
        }

        private void InvalidateAveraged()
        {
            _averagedDirty = true;
        }

        private void RebuildItems()
        {
            _items.Clear();
            int colorIdx = 0;

            foreach (var run in _runs.Where(r => r.selected))
            {
                var files = Directory
                    .GetFiles(run.path, "*.json", SearchOption.TopDirectoryOnly)
                    .OrderBy(x => x);
                foreach (var f in files)
                {
                    try
                    {
                        var json = File.ReadAllText(f);
                        var data = JsonUtility.FromJson<BenchScenarioFile>(json);
                        if (data?.scenario == null || data.perSecond == null)
                        {
                            continue;
                        }

                        _items.Add(
                            new Item
                            {
                                path = f,
                                file = data,
                                visible = true,
                                color = kColors[colorIdx++ % kColors.Length],
                                runName = run.name,
                            }
                        );
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[BenchViewer] Failed to read {f}: {ex.Message}");
                    }
                }
            }
            InvalidateAveraged();
            Repaint();
        }

        private bool VisibleAndPasses(Item it) => it.visible && PassesFilters(it);

        private bool PassesFilters(Item it)
        {
            if (_onlyEditor && !it.file.env.isEditor)
            {
                return false;
            }
            if (_onlyRuntime && it.file.env.isEditor)
            {
                return false;
            }
            if (!string.IsNullOrEmpty(_search))
            {
                var n = it.file.scenario.name ?? "";
                if (n.IndexOf(_search, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    return false;
                }
            }
            return true;
        }

        private static float MapX(Rect r, float t, float maxX) =>
            r.x + Mathf.Clamp01(t / Mathf.Max(1e-4f, maxX)) * r.width;

        private static float MapY(Rect r, float v, float yMax)
        {
            var ny = Mathf.Clamp01(v / Mathf.Max(1e-3f, yMax));
            return Mathf.Lerp(r.yMax - 8, r.y + 8, ny);
        }

        private static float NeatCeil(float v)
        {
            var pow = Mathf.Pow(10, Mathf.Floor(Mathf.Log10(Mathf.Max(1f, v))));
            var n = Mathf.Ceil(v / (float)pow);
            if (n > 5)
            {
                n = 10;
            }
            else if (n > 2)
            {
                n = 5;
            }
            else
            {
                n = 2;
            }
            return (float)(n * pow);
        }

        private static string ToAssetPath(string abs)
        {
            abs = abs.Replace('\\', '/');
            var root = Application.dataPath.Replace('\\', '/');
            if (abs.StartsWith(root))
            {
                return "Assets" + abs.Substring(root.Length);
            }
            return null;
        }

        private static string Fmt(long bytes)
        {
            if (bytes <= 0)
            {
                return "0B";
            }
            string[] units = { "B", "KB", "MB", "GB" };
            int unitIndex = (int)Math.Min(units.Length - 1, Math.Log(bytes, 1024));
            double val = bytes / Math.Pow(1024, unitIndex);
            return $"{val:F1}{units[unitIndex]}";
        }

        private string GetDefaultStartFolder()
        {
            // If we have a known last run output folder, go one level up to show all runs
            if (!string.IsNullOrEmpty(BenchPerSecondLogger.LastRunFolder)
                && Directory.Exists(BenchPerSecondLogger.LastRunFolder))
            {
                var parent = Directory.GetParent(BenchPerSecondLogger.LastRunFolder);
                if (parent != null && parent.Exists)
                {
                    return parent.FullName;
                }
                return BenchPerSecondLogger.LastRunFolder;
            }

            var last = EditorPrefs.GetString(Pref_LastRunFolder, null);
            if (!string.IsNullOrEmpty(last) && Directory.Exists(last))
            {
                return last;
            }

            var sel = _runs.FirstOrDefault(r => r.selected);
            if (sel != null && Directory.Exists(sel.path))
            {
                return sel.path;
            }

            var guess = Path.Combine(
                Application.dataPath,
                "ReactiveUIToolKit",
                "Diagnostics",
                "Benchmark",
                "Results",
                "Editor"
            );
            if (Directory.Exists(guess))
            {
                return guess;
            }

            var guessRoot = Path.Combine(
                Application.dataPath,
                "ReactiveUIToolKit",
                "Diagnostics",
                "Benchmark",
                "Results"
            );
            if (Directory.Exists(guessRoot))
            {
                return guessRoot;
            }

            return Application.dataPath;
        }
    }
}
#endif
