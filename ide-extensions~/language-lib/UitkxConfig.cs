using System.IO;
using System.Text.RegularExpressions;

namespace ReactiveUITK.Language
{
    /// <summary>
    /// Reads project-level settings from <c>uitkx.config.json</c> for the import/export grammar
    /// (leg 3, plan §9). The only key today is <c>"root"</c> — the UI source root that a <c>~/</c>
    /// specifier (in an <c>import</c> or an <c>Asset&lt;T&gt;</c>/<c>@uss</c> path) resolves against,
    /// project-relative, engine default <c>Assets</c>.
    ///
    /// Discovery is a directory walk-up (same as the formatter's <c>ConfigLoader</c>) with
    /// <b>nearest-config-WINS, NO merge</b>: a config in a subdirectory SHADOWS an ancestor's
    /// <c>"root"</c> entirely. Shared by the source generator, the analyzer/LSP, and (via a mirror)
    /// HMR so <c>~/</c> resolves identically everywhere.
    /// </summary>
    public static class UitkxConfig
    {
        private const string ConfigFileName = "uitkx.config.json";

        /// <summary>The engine-default UI source root when no config <c>"root"</c> is set.</summary>
        public const string DefaultRoot = "Assets";

        private static readonly Regex s_rootRe =
            new Regex("\"root\"\\s*:\\s*\"([^\"]*)\"", RegexOptions.CultureInvariant);

        /// <summary>
        /// The project-relative UI source root for a file whose directory is
        /// <paramref name="fileDirectory"/> (absolute). Walks up to the nearest
        /// <c>uitkx.config.json</c>; its <c>"root"</c> wins outright. Returns <see cref="DefaultRoot"/>
        /// when there is no config, no <c>"root"</c> key, or the file is malformed/unreadable.
        /// </summary>
        public static string LoadRoot(string? fileDirectory)
        {
            if (string.IsNullOrEmpty(fileDirectory))
                return DefaultRoot;

            string? dir = fileDirectory;
            while (!string.IsNullOrEmpty(dir))
            {
                string candidate = Path.Combine(dir, ConfigFileName);
                if (File.Exists(candidate))
                {
                    try
                    {
                        var m = s_rootRe.Match(File.ReadAllText(candidate));
                        if (m.Success)
                        {
                            string root = m.Groups[1].Value.Trim().Replace('\\', '/').TrimEnd('/');
                            return string.IsNullOrEmpty(root) ? DefaultRoot : root;
                        }
                    }
                    catch { /* malformed/unreadable → default */ }
                    return DefaultRoot; // nearest config wins even if it has no "root" (no merge upward)
                }

                string? parent = Path.GetDirectoryName(dir);
                if (parent == null || parent == dir)
                    break;
                dir = parent;
            }
            return DefaultRoot;
        }
    }
}
