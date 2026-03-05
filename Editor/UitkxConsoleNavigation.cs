using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;
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
        private static readonly Regex s_lineDirectiveRegex = new Regex(
            "^\\s*#line\\s+(?<line>\\d+)\\s+\"(?<path>[^\"]+)\"",
            RegexOptions.Compiled
        );

        private static readonly Regex s_uitkxLocationRegex = new Regex(
            @"(?<path>[A-Za-z]:[^\r\n()]*?\.uitkx|Assets[/\\][^\r\n()]*?\.uitkx|[^\r\n():]*[\\/][^\r\n()]*?\.uitkx)\((?<line>\d+)(?:,(?<col>\d+))?\)",
            RegexOptions.Compiled
        );

        [OnOpenAsset]
        private static bool OnOpenAsset(int instanceId, int line)
        {
            string assetPath = AssetDatabase.GetAssetPath(instanceId);
            if (string.IsNullOrEmpty(assetPath))
            {
                var obj = EditorUtility.InstanceIDToObject(instanceId);
                if (obj != null)
                    assetPath = AssetDatabase.GetAssetPath(obj);
            }

            Debug.Log($"[UITKX Nav] OnOpenAsset instanceId={instanceId} line={line} assetPath='{assetPath}'");

            if (string.IsNullOrEmpty(assetPath)
                && TryResolveFromConsoleActiveText(out string consolePath, out int consoleLine))
            {
                assetPath = consolePath;
                if (line <= 0)
                    line = consoleLine;
            }

            if (!TryResolveUitkxTarget(assetPath, line, out string fullPath, out int targetLine))
            {
                // Second chance: Console entries sometimes carry generator virtual paths
                // that don't round-trip through AssetDatabase instance ids.
                if (!TryResolveFromConsoleActiveText(out string consolePath2, out int consoleLine2))
                {
                    Debug.Log("[UITKX Nav] Could not resolve from console active text.");
                    return false; // not ours — let Unity handle it
                }

                if (!TryResolveUitkxTarget(consolePath2, consoleLine2, out fullPath, out targetLine))
                {
                    Debug.Log($"[UITKX Nav] Console fallback path unresolved: '{consolePath2}' line={consoleLine2}");
                    return false;
                }
            }

            Debug.Log($"[UITKX Nav] Resolved target: '{fullPath}:{targetLine}'");

            try
            {
                // On Windows, 'code' is a .cmd script so we need cmd.exe as the host.
                // On macOS/Linux, 'code' is a binary and can be launched directly.
                string gotoArg = targetLine > 0 ? $"--goto \"{fullPath}:{targetLine}\"" : $"\"{fullPath}\"";

#if UNITY_EDITOR_WIN
                Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c code {gotoArg}",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                );
#else
                Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = "code",
                        Arguments = gotoArg,
                        UseShellExecute = true,
                    }
                );
#endif
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning(
                    $"[UITKX] Could not open VS Code for '{assetPath}': {ex.Message}\n"
                        + $"Make sure the 'code' command is on your PATH."
                );
            }

            return true; // handled — suppress Unity's default open behaviour
        }

        private static bool TryResolveUitkxTarget(string assetPath, int clickedLine, out string fullPath, out int line)
        {
            fullPath = string.Empty;
            line = clickedLine > 0 ? clickedLine : 1;

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

        private static bool TryResolveFromConsoleActiveText(out string assetPath, out int line)
        {
            assetPath = string.Empty;
            line = 1;

            try
            {
                if (TryResolveFromSelectedConsoleEntry(out assetPath, out line))
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
                    Debug.Log($"[UITKX Nav] Parsed from active text: '{assetPath}:{line}'");
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryResolveFromSelectedConsoleEntry(out string assetPath, out int line)
        {
            assetPath = string.Empty;
            line = 1;

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

                string file = fileField != null ? (fileField.GetValue(entry) as string ?? string.Empty) : string.Empty;
                int ln = lineField != null ? (int)lineField.GetValue(entry) : 1;

                if (!string.IsNullOrEmpty(file))
                {
                    assetPath = file.Replace('\\', '/');
                    line = ln > 0 ? ln : 1;
                    Debug.Log($"[UITKX Nav] Parsed from selected console entry: '{assetPath}:{line}'");
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
