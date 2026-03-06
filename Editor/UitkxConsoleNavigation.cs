using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditorInternal;
using UnityEngine;

namespace ReactiveUITK.Editor
{
    /// <summary>
    /// Intercepts Unity's "open asset" event for <c>.uitkx</c> files so that
    /// clicking a Console hyperlink (or double-clicking in the Project window)
    /// opens the correct <c>.uitkx</c> line in VS Code instead of the generated
    /// <c>.g.cs</c> file.
    ///
    /// Works because the source generator already emits <c>#line N "File.uitkx"</c>
    /// directives, so Unity's console already knows the original file and line.
    /// </summary>
    public static class UitkxConsoleNavigation
    {
        private static readonly bool s_verboseLogs = EditorPrefs.GetBool("ReactiveUITK.UitkxNavVerbose", false);
        private static bool s_isProgrammaticOpenInProgress;

        [InitializeOnLoadMethod]
        private static void LogNavigationLoaded()
        {
            EnsureProjectGenerationSupportsUitkx();
            LogVerbose("[UITKX Nav] UitkxConsoleNavigation loaded");
        }

        private static void EnsureProjectGenerationSupportsUitkx()
        {
            try
            {
                var existing = EditorSettings.projectGenerationUserExtensions;
                if (existing == null)
                {
                    EditorSettings.projectGenerationUserExtensions = new[] { "uitkx" };
                    return;
                }

                for (int i = 0; i < existing.Length; i++)
                {
                    if (string.Equals(existing[i], "uitkx", StringComparison.OrdinalIgnoreCase))
                        return;
                }

                var updated = new string[existing.Length + 1];
                Array.Copy(existing, updated, existing.Length);
                updated[existing.Length] = "uitkx";
                EditorSettings.projectGenerationUserExtensions = updated;
            }
            catch (Exception ex)
            {
                LogVerbose($"[UITKX Nav] EnsureProjectGenerationSupportsUitkx exception: {ex.Message}");
            }
        }

        private static readonly Regex s_lineDirectiveRegex = new Regex(
            "^\\s*#line\\s+(?<line>\\d+)\\s+\"(?<path>[^\"]+)\"",
            RegexOptions.Compiled
        );

        private static readonly Regex s_uitkxLocationRegex = new Regex(
            @"(?<path>[A-Za-z]:[^\r\n()]*?\.uitkx|Assets[/\\][^\r\n()]*?\.uitkx|[^\r\n():]*[\\/][^\r\n()]*?\.uitkx)\((?<line>\d+)(?:,(?<col>\d+))?\)",
            RegexOptions.Compiled
        );

        [OnOpenAsset(-10000)]
        private static bool OnOpenAssetPriority(int instanceId, int line, int column)
        {
            return HandleOnOpenAsset(instanceId, line, column);
        }

        [OnOpenAsset(-10000)]
        private static bool OnOpenAssetPriorityCompat(int instanceId, int line)
        {
            return HandleOnOpenAsset(instanceId, line, 1);
        }

        [OnOpenAsset]
        private static bool OnOpenAssetCompat(int instanceId, int line, int column)
        {
            return HandleOnOpenAsset(instanceId, line, column);
        }

        [OnOpenAsset]
        private static bool OnOpenAssetCompat2(int instanceId, int line)
        {
            return HandleOnOpenAsset(instanceId, line, 1);
        }

        private static bool HandleOnOpenAsset(int instanceId, int line, int column)
        {
            if (s_isProgrammaticOpenInProgress)
                return false;

            string assetPath = AssetDatabase.GetAssetPath(instanceId);
            if (string.IsNullOrEmpty(assetPath))
            {
                var obj = EditorUtility.InstanceIDToObject(instanceId);
                if (obj != null)
                    assetPath = AssetDatabase.GetAssetPath(obj);
            }

            LogVerbose($"[UITKX Nav] OnOpenAsset instanceId={instanceId} line={line} column={column} assetPath='{assetPath}'");

            if (string.IsNullOrEmpty(assetPath)
                && TryResolveFromConsoleActiveText(out string consolePath, out int consoleLine, out int consoleColumn))
            {
                assetPath = consolePath;
                if (line <= 0)
                    line = consoleLine;
                if (column <= 0)
                    column = consoleColumn;
            }

            if (!TryResolveUitkxTarget(assetPath, line, column, out string fullPath, out int targetLine, out int targetColumn))
            {
                // Second chance: Console entries sometimes carry generator virtual paths
                // that don't round-trip through AssetDatabase instance ids.
                if (!TryResolveFromConsoleActiveText(out string consolePath2, out int consoleLine2, out int consoleColumn2))
                {
                    LogVerbose("[UITKX Nav] Could not resolve from console active text.");
                    return false; // not ours — let Unity handle it
                }

                if (!TryResolveUitkxTarget(consolePath2, consoleLine2, consoleColumn2, out fullPath, out targetLine, out targetColumn))
                {
                    LogVerbose($"[UITKX Nav] Console fallback path unresolved: '{consolePath2}' line={consoleLine2}");
                    return false;
                }
            }

            LogVerbose($"[UITKX Nav] Resolved target: '{fullPath}:{targetLine}:{targetColumn}'");

            try
            {
                s_isProgrammaticOpenInProgress = true;

                EnsureProjectGenerationSupportsUitkx();

                if (TryOpenViaConfiguredCodeEditor(fullPath, targetLine, targetColumn))
                    return true;

                if (TryOpenViaConfiguredEditorExecutable(fullPath, targetLine, targetColumn))
                    return true;

                // Prefer opening via AssetDatabase so Unity uses the configured
                // external script editor (VS / Rider / VS Code in Preferences).
                if (TryOpenViaUnityDefaultEditor(fullPath, targetLine, targetColumn))
                    return true;
            }
            catch (Exception ex)
            {
                LogVerbose($"[UITKX Nav] Open handling exception: {ex.Message}");
            }
            finally
            {
                s_isProgrammaticOpenInProgress = false;
            }

            return false;
        }

        private static bool TryOpenViaConfiguredEditorExecutable(string fullPath, int line, int column)
        {
            try
            {
                string editorPath = GetConfiguredEditorPath();
                if (string.IsNullOrEmpty(editorPath) || !File.Exists(editorPath))
                    return false;

                int targetLine = line > 0 ? line : 1;
                int targetColumn = column > 0 ? column : 1;
                string lowerName = Path.GetFileNameWithoutExtension(editorPath).ToLowerInvariant();

                if (lowerName.Contains("devenv") || lowerName.Contains("visualstudio"))
                {
                    if (TryOpenViaVisualStudioComIntegration(editorPath, fullPath, targetLine))
                        return true;
                }

                string args;
                if (lowerName.Contains("code"))
                {
                    args = $"--goto \"{fullPath}:{targetLine}:{targetColumn}\"";
                }
                else if (lowerName.Contains("rider"))
                {
                    args = $"--line {targetLine} \"{fullPath}\"";
                }
                else
                {
                    args = $"\"{fullPath}\"";
                }

                Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = editorPath,
                        Arguments = args,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                );

                return true;
            }
            catch (Exception ex)
            {
                LogVerbose($"[UITKX Nav] TryOpenViaConfiguredEditorExecutable exception: {ex.Message}");
                return false;
            }
        }

        private static bool TryOpenViaVisualStudioComIntegration(string editorPath, string fullPath, int line)
        {
            try
            {
                string comIntegrationPath = FindVisualStudioComIntegrationExe();
                if (string.IsNullOrEmpty(comIntegrationPath) || !File.Exists(comIntegrationPath))
                    return false;

                string solutionPath = TryGetFirstSolutionPath();
                string quotedSolution = string.IsNullOrEmpty(solutionPath) ? "\"\"" : $"\"{solutionPath}\"";
                string absolutePath = Path.GetFullPath(fullPath);

                var psi = new ProcessStartInfo
                {
                    FileName = comIntegrationPath,
                    Arguments = $"\"{editorPath}\" {quotedSolution} \"{absolutePath}\" {line}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                Process.Start(psi);
                return true;
            }
            catch (Exception ex)
            {
                LogVerbose($"[UITKX Nav] TryOpenViaVisualStudioComIntegration exception: {ex.Message}");
                return false;
            }
        }

        private static string FindVisualStudioComIntegrationExe()
        {
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            if (string.IsNullOrEmpty(projectRoot))
                return string.Empty;

            string packageCache = Path.Combine(projectRoot, "Library", "PackageCache");
            if (!Directory.Exists(packageCache))
                return string.Empty;

            var candidates = Directory.GetDirectories(packageCache, "com.unity.ide.visualstudio@*");
            for (int i = 0; i < candidates.Length; i++)
            {
                string path = Path.Combine(candidates[i], "Editor", "COMIntegration", "Release", "COMIntegration.exe");
                if (File.Exists(path))
                    return path;
            }

            return string.Empty;
        }

        private static string TryGetFirstSolutionPath()
        {
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            if (string.IsNullOrEmpty(projectRoot) || !Directory.Exists(projectRoot))
                return string.Empty;

            var solutions = Directory.GetFiles(projectRoot, "*.sln", SearchOption.TopDirectoryOnly);
            if (solutions.Length == 0)
                return string.Empty;

            return solutions[0];
        }

        private static string GetConfiguredEditorPath()
        {
            try
            {
                var codeEditorType = Type.GetType("Unity.CodeEditor.CodeEditor, Unity.CodeEditor");
                if (codeEditorType != null)
                {
                    var currentEditorPathProperty = codeEditorType.GetProperty(
                        "CurrentEditorPath",
                        BindingFlags.Static | BindingFlags.Public
                    );
                    if (currentEditorPathProperty != null)
                    {
                        string fromCodeEditor = currentEditorPathProperty.GetValue(null, null) as string;
                        if (!string.IsNullOrEmpty(fromCodeEditor))
                            return fromCodeEditor;
                    }
                }
            }
            catch (Exception ex)
            {
                LogVerbose($"[UITKX Nav] GetConfiguredEditorPath CodeEditor exception: {ex.Message}");
            }

            return EditorPrefs.GetString("kScriptsDefaultApp", string.Empty);
        }

        private static bool TryOpenViaConfiguredCodeEditor(string fullPath, int line, int column)
        {
            try
            {
                var codeEditorType = Type.GetType("Unity.CodeEditor.CodeEditor, Unity.CodeEditor");
                if (codeEditorType == null)
                    return false;

                object editorInstance = null;

                var editorProperty = codeEditorType.GetProperty("Editor", BindingFlags.Static | BindingFlags.Public);
                if (editorProperty != null)
                    editorInstance = editorProperty.GetValue(null, null);

                if (editorInstance == null)
                    return false;

                object currentCodeEditor = null;
                var currentCodeEditorProp = editorInstance.GetType().GetProperty(
                    "CurrentCodeEditor",
                    BindingFlags.Instance | BindingFlags.Public
                );
                if (currentCodeEditorProp != null)
                    currentCodeEditor = currentCodeEditorProp.GetValue(editorInstance, null);

                if (currentCodeEditor == null)
                    return false;

                return TryInvokeOpenProject(currentCodeEditor, fullPath, line > 0 ? line : 1, column > 0 ? column : 1);
            }
            catch (Exception ex)
            {
                LogVerbose($"[UITKX Nav] TryOpenViaConfiguredCodeEditor exception: {ex.Message}");
                return false;
            }
        }

        private static bool TryInvokeOpenProject(object codeEditor, string fullPath, int line, int column)
        {
            var editorType = codeEditor.GetType();

            // First try public instance overloads.
            var openProject3 = editorType.GetMethod(
                "OpenProject",
                BindingFlags.Instance | BindingFlags.Public,
                null,
                new[] { typeof(string), typeof(int), typeof(int) },
                null
            );
            if (openProject3 != null)
            {
                object result3 = openProject3.Invoke(codeEditor, new object[] { fullPath, line, column });
                if (result3 is bool opened3)
                    return opened3;
                return true;
            }

            var openProject2 = editorType.GetMethod(
                "OpenProject",
                BindingFlags.Instance | BindingFlags.Public,
                null,
                new[] { typeof(string), typeof(int) },
                null
            );
            if (openProject2 != null)
            {
                object result2 = openProject2.Invoke(codeEditor, new object[] { fullPath, line });
                if (result2 is bool opened2)
                    return opened2;
                return true;
            }

            // Then try explicit interface implementations/non-public methods.
            var allMethods = editorType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            for (int i = 0; i < allMethods.Length; i++)
            {
                var method = allMethods[i];
                if (!method.Name.EndsWith("OpenProject", StringComparison.Ordinal))
                    continue;

                var parameters = method.GetParameters();
                if (parameters.Length == 3
                    && parameters[0].ParameterType == typeof(string)
                    && parameters[1].ParameterType == typeof(int)
                    && parameters[2].ParameterType == typeof(int))
                {
                    object result = method.Invoke(codeEditor, new object[] { fullPath, line, column });
                    if (result is bool opened)
                        return opened;
                    return true;
                }

                if (parameters.Length == 2
                    && parameters[0].ParameterType == typeof(string)
                    && parameters[1].ParameterType == typeof(int))
                {
                    object result = method.Invoke(codeEditor, new object[] { fullPath, line });
                    if (result is bool opened)
                        return opened;
                    return true;
                }
            }

            return false;
        }

        private static bool TryOpenViaUnityDefaultEditor(string fullPath, int line, int column)
        {
            string assetPath = TryToAssetPath(fullPath);
            if (string.IsNullOrEmpty(assetPath))
                return false;

            var asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
            if (asset == null)
                return false;

            return AssetDatabase.OpenAsset(asset, line > 0 ? line : 1, column > 0 ? column : 1);
        }

        private static string TryToAssetPath(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
                return string.Empty;

            string normalizedFull = Path.GetFullPath(fullPath).Replace('\\', '/');
            string normalizedData = Path.GetFullPath(Application.dataPath).Replace('\\', '/');

            if (!normalizedFull.StartsWith(normalizedData, StringComparison.OrdinalIgnoreCase))
                return string.Empty;

            string relativeFromAssets = normalizedFull.Substring(normalizedData.Length).TrimStart('/');
            return string.IsNullOrEmpty(relativeFromAssets)
                ? "Assets"
                : "Assets/" + relativeFromAssets;
        }

        private static bool TryResolveUitkxTarget(string assetPath, int clickedLine, int clickedColumn, out string fullPath, out int line, out int column)
        {
            fullPath = string.Empty;
            line = clickedLine > 0 ? clickedLine : 1;
            column = clickedColumn > 0 ? clickedColumn : 1;

            if (string.IsNullOrEmpty(assetPath))
                return false;

            // Direct .uitkx click (project window or console where #line is already respected)
            if (assetPath.EndsWith(".uitkx", StringComparison.OrdinalIgnoreCase))
            {
                return TryResolveExistingUitkxPath(assetPath, out fullPath);
            }

            // Console may open the generated .g.cs file. Recover original .uitkx via #line pragmas.
            if (!assetPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                return false;

            string generatedFullPath = Path.GetFullPath(assetPath);
            if (!File.Exists(generatedFullPath))
                return false;

            string[] lines = File.ReadAllLines(generatedFullPath);
            if (lines.Length == 0)
                return false;

            int clicked = Mathf.Clamp(clickedLine, 1, lines.Length);

            int activeDirectiveSourceLine = -1; // 1-based line of '#line ... "file"'
            int activeDirectiveTargetLine = -1; // target line number from directive
            string activeDirectiveTargetPath = null;

            for (int sourceLine = 1; sourceLine <= clicked; sourceLine++)
            {
                string text = lines[sourceLine - 1];
                var match = s_lineDirectiveRegex.Match(text);
                if (!match.Success)
                    continue;

                if (!int.TryParse(match.Groups["line"].Value, out int mappedStart))
                    continue;

                activeDirectiveSourceLine = sourceLine;
                activeDirectiveTargetLine = mappedStart;
                activeDirectiveTargetPath = match.Groups["path"].Value;
            }

            if (string.IsNullOrEmpty(activeDirectiveTargetPath))
                return false;

            // Line mapping: source line right after '#line' maps to mappedStart.
            int targetLine = activeDirectiveTargetLine + Math.Max(0, clicked - (activeDirectiveSourceLine + 1));
            string mappedPath;
            if (!TryResolveExistingUitkxPath(activeDirectiveTargetPath, out mappedPath))
                return false;

            fullPath = mappedPath;
            line = targetLine;
            column = 1;
            return true;
        }

        private static bool TryResolveExistingUitkxPath(string pathHint, out string fullPath)
        {
            fullPath = string.Empty;
            if (string.IsNullOrEmpty(pathHint))
                return false;

            string normalizedHint = pathHint.Replace('\\', '/');

            if (Path.IsPathRooted(normalizedHint))
            {
                string rooted = Path.GetFullPath(normalizedHint);
                if (File.Exists(rooted) && rooted.EndsWith(".uitkx", StringComparison.OrdinalIgnoreCase))
                {
                    fullPath = rooted;
                    return true;
                }
            }

            // Asset-style relative path (Assets/...) or project-relative path.
            string candidate = ResolvePathToAbsolute(normalizedHint);
            if (File.Exists(candidate) && candidate.EndsWith(".uitkx", StringComparison.OrdinalIgnoreCase))
            {
                fullPath = candidate;
                return true;
            }

            // Fallback for generator virtual paths (e.g. ReactiveUITK.SourceGenerator\...\Foo.uitkx)
            string fileName = Path.GetFileName(normalizedHint);
            string hintNoExt = Path.GetFileNameWithoutExtension(normalizedHint);
            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(hintNoExt))
                return false;

            string suffix = normalizedHint.TrimStart('/');
            string[] guids = AssetDatabase.FindAssets(hintNoExt);

            // 1) Prefer suffix match when possible.
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]).Replace('\\', '/');
                if (!assetPath.EndsWith(".uitkx", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (assetPath.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    fullPath = Path.GetFullPath(assetPath);
                    return true;
                }
            }

            // 2) Fallback to exact filename match.
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (!assetPath.EndsWith(".uitkx", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (string.Equals(Path.GetFileName(assetPath), fileName, StringComparison.OrdinalIgnoreCase))
                {
                    fullPath = Path.GetFullPath(assetPath);
                    return true;
                }
            }

            return false;
        }

        private static string ResolvePathToAbsolute(string mappedPath)
        {
            if (Path.IsPathRooted(mappedPath))
                return Path.GetFullPath(mappedPath);

            // Unity project root = parent of Assets/
            string projectRoot = string.Empty;
            var parent = Directory.GetParent(Application.dataPath);
            if (parent != null)
                projectRoot = parent.FullName;
            if (!string.IsNullOrEmpty(projectRoot))
            {
                string combined = Path.GetFullPath(Path.Combine(projectRoot, mappedPath));
                return combined;
            }

            return Path.GetFullPath(mappedPath);
        }

        private static bool TryResolveFromConsoleActiveText(out string assetPath, out int line, out int column)
        {
            assetPath = string.Empty;
            line = 1;
            column = 1;

            try
            {
                if (TryResolveFromSelectedConsoleEntry(out assetPath, out line, out column))
                    return true;

                var consoleWindowType = Type.GetType("UnityEditor.ConsoleWindow, UnityEditor");
                if (consoleWindowType == null)
                    return false;

                var consoleField = consoleWindowType.GetField(
                    "ms_ConsoleWindow",
                    BindingFlags.Static | BindingFlags.NonPublic
                );
                if (consoleField == null)
                    return false;

                var consoleWindow = consoleField.GetValue(null);
                if (consoleWindow == null)
                    return false;

                var activeTextField = consoleWindowType.GetField(
                    "m_ActiveText",
                    BindingFlags.Instance | BindingFlags.NonPublic
                );
                string[] activeTextCandidates;
                if (activeTextField != null)
                {
                    string t1 = activeTextField.GetValue(consoleWindow) as string ?? string.Empty;
                    var altField = consoleWindowType.GetField(
                        "m_ActiveTextWithHyperlinks",
                        BindingFlags.Instance | BindingFlags.NonPublic
                    );
                    string t2 = altField != null ? (altField.GetValue(consoleWindow) as string ?? string.Empty) : string.Empty;
                    activeTextCandidates = new[] { t1, t2 };
                }
                else
                {
                    activeTextCandidates = Array.Empty<string>();
                }

                for (int i = 0; i < activeTextCandidates.Length; i++)
                {
                    string candidate = activeTextCandidates[i];
                    if (string.IsNullOrEmpty(candidate))
                        continue;

                    var match = s_uitkxLocationRegex.Match(candidate);
                    if (!match.Success)
                        continue;

                    assetPath = match.Groups["path"].Value.Replace('\\', '/');
                    if (int.TryParse(match.Groups["line"].Value, out int parsedLine) && parsedLine > 0)
                        line = parsedLine;
                    if (int.TryParse(match.Groups["col"].Value, out int parsedCol) && parsedCol > 0)
                        column = parsedCol;
                    LogVerbose($"[UITKX Nav] Parsed from active text: '{assetPath}:{line}:{column}'");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                LogVerbose($"[UITKX Nav] TryResolveFromConsoleActiveText exception: {ex.Message}");
                return false;
            }
        }

        private static bool TryResolveFromSelectedConsoleEntry(out string assetPath, out int line, out int column)
        {
            assetPath = string.Empty;
            line = 1;
            column = 1;

            try
            {
                var consoleWindowType = Type.GetType("UnityEditor.ConsoleWindow, UnityEditor");
                var logEntriesType = Type.GetType("UnityEditorInternal.LogEntries, UnityEditor")
                    ?? Type.GetType("UnityEditor.LogEntries, UnityEditor");
                var logEntryType = Type.GetType("UnityEditorInternal.LogEntry, UnityEditor")
                    ?? Type.GetType("UnityEditor.LogEntry, UnityEditor");
                if (consoleWindowType == null || logEntriesType == null || logEntryType == null)
                    return false;

                var consoleField = consoleWindowType.GetField(
                    "ms_ConsoleWindow",
                    BindingFlags.Static | BindingFlags.NonPublic
                );
                if (consoleField == null)
                    return false;

                var consoleWindow = consoleField.GetValue(null);
                if (consoleWindow == null)
                    return false;

                var listViewField = consoleWindowType.GetField(
                    "m_ListView",
                    BindingFlags.Instance | BindingFlags.NonPublic
                );
                if (listViewField == null)
                    return false;

                var listView = listViewField.GetValue(consoleWindow);
                if (listView == null)
                    return false;

                var rowField = listView.GetType().GetField("row", BindingFlags.Instance | BindingFlags.Public);
                if (rowField == null)
                    return false;

                int row = (int)rowField.GetValue(listView);
                if (row < 0)
                    return false;

                var getEntryMethod = logEntriesType.GetMethod(
                    "GetEntryInternal",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(int), logEntryType },
                    null
                );
                if (getEntryMethod == null)
                    return false;

                var entry = Activator.CreateInstance(logEntryType);
                if (entry == null)
                    return false;

                bool ok = (bool)getEntryMethod.Invoke(null, new[] { (object)row, entry });
                if (!ok)
                    return false;

                var fileField = logEntryType.GetField("file", BindingFlags.Instance | BindingFlags.Public)
                    ?? logEntryType.GetField("file", BindingFlags.Instance | BindingFlags.NonPublic);
                var lineField = logEntryType.GetField("line", BindingFlags.Instance | BindingFlags.Public)
                    ?? logEntryType.GetField("line", BindingFlags.Instance | BindingFlags.NonPublic);
                var columnField = logEntryType.GetField("column", BindingFlags.Instance | BindingFlags.Public)
                    ?? logEntryType.GetField("column", BindingFlags.Instance | BindingFlags.NonPublic);

                string file = fileField != null ? (fileField.GetValue(entry) as string ?? string.Empty) : string.Empty;
                int ln = lineField != null ? (int)lineField.GetValue(entry) : 1;
                int col = columnField != null ? (int)columnField.GetValue(entry) : 1;

                if (!string.IsNullOrEmpty(file))
                {
                    assetPath = file.Replace('\\', '/');
                    line = ln > 0 ? ln : 1;
                    column = col > 0 ? col : 1;
                    LogVerbose($"[UITKX Nav] Parsed from selected console entry: '{assetPath}:{line}:{column}'");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                LogVerbose($"[UITKX Nav] TryResolveFromSelectedConsoleEntry exception: {ex.Message}");
                return false;
            }
        }

        private static void LogVerbose(string message)
        {
            if (s_verboseLogs)
                UnityEngine.Debug.LogWarning(message);
        }
    }
}
