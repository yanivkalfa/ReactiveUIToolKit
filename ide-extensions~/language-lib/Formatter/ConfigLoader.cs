using System.IO;

namespace ReactiveUITK.Language.Formatter
{
    /// <summary>
    /// Locates and loads <c>uitkx.config.json</c> using the same directory-walk
    /// algorithm as ESLint / Prettier: starting from <paramref name="fileDirectory"/>
    /// and walking upward until a config file is found or the filesystem root is
    /// reached.
    /// </summary>
    public static class ConfigLoader
    {
        private const string ConfigFileName = "uitkx.config.json";

        /// <summary>
        /// Walk directories from <paramref name="fileDirectory"/> up to the root.
        /// Returns the <see cref="FormatterOptions"/> parsed from the first
        /// <c>uitkx.config.json</c> found, or <see cref="FormatterOptions.Default"/>
        /// if none is found.
        /// </summary>
        /// <param name="fileDirectory">
        /// Absolute path to the directory containing the <c>.uitkx</c> file being
        /// formatted.  Pass <c>null</c> or empty to get defaults immediately.
        /// </param>
        public static FormatterOptions LoadFormatterOptions(string? fileDirectory)
        {
            if (string.IsNullOrEmpty(fileDirectory))
                return FormatterOptions.Default;

            var dir = fileDirectory;

            while (!string.IsNullOrEmpty(dir))
            {
                var candidate = Path.Combine(dir, ConfigFileName);
                if (File.Exists(candidate))
                {
                    try
                    {
                        var json = File.ReadAllText(candidate);
                        return FormatterOptions.FromJson(json);
                    }
                    catch
                    {
                        // Malformed config → fall through to default
                        return FormatterOptions.Default;
                    }
                }

                var parent = Path.GetDirectoryName(dir);

                // Stop when we've hit the root (GetDirectoryName returns null or
                // the same path for filesystem roots).
                if (parent == null || parent == dir)
                    break;

                dir = parent;
            }

            return FormatterOptions.Default;
        }
    }
}
