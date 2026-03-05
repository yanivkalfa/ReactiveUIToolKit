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

            if (string.IsNullOrEmpty(assetPath)
                && TryResolveFromConsoleActiveText(out string consolePath, out int consoleLine))
            {
                assetPath = consolePath;
                if (line <= 0)
                    line = consoleLine;
            }

            if (!TryResolveUitkxTarget(assetPath, line, out string fullPath, out int targetLine))
                return false; // not ours — let Unity handle it

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
                fullPath = Path.GetFullPath(assetPath);
                return true;
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
            string? activeDirectiveTargetPath = null;

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
            string mappedPath = ResolvePathToAbsolute(activeDirectiveTargetPath);
            if (!mappedPath.EndsWith(".uitkx", StringComparison.OrdinalIgnoreCase))
                return false;
            if (!File.Exists(mappedPath))
                return false;

            fullPath = mappedPath;
            line = targetLine;
            return true;
        }

        private static string ResolvePathToAbsolute(string mappedPath)
        {
            if (Path.IsPathRooted(mappedPath))
                return Path.GetFullPath(mappedPath);

            // Unity project root = parent of Assets/
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
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
                if (activeTextField == null)
                    return false;

                string activeText = activeTextField.GetValue(consoleWindow) as string ?? string.Empty;
                if (string.IsNullOrEmpty(activeText))
                    return false;

                var match = s_uitkxLocationRegex.Match(activeText);
                if (!match.Success)
                    return false;

                assetPath = match.Groups["path"].Value.Replace('\\', '/');
                if (int.TryParse(match.Groups["line"].Value, out int parsedLine) && parsedLine > 0)
                    line = parsedLine;

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
