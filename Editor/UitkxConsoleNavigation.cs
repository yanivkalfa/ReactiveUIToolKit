using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;

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
        [OnOpenAsset]
        private static bool OnOpenAsset(int instanceId, int line)
        {
            string assetPath = AssetDatabase.GetAssetPath(instanceId);

            if (!assetPath.EndsWith(".uitkx", StringComparison.OrdinalIgnoreCase))
                return false; // not our file — let Unity handle it

            string fullPath = Path.GetFullPath(assetPath);

            try
            {
                // On Windows, 'code' is a .cmd script so we need cmd.exe as the host.
                // On macOS/Linux, 'code' is a binary and can be launched directly.
                string gotoArg = line > 0 ? $"--goto \"{fullPath}:{line}\"" : $"\"{fullPath}\"";

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
    }
}
