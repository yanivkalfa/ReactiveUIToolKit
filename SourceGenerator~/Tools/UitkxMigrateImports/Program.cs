using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ReactiveUITK.SourceGenerator.Tools
{
    /// <summary>
    /// CLI wrapper around <see cref="UitkxMigrator"/> — the only layer that touches the filesystem.
    ///
    /// <code>
    ///   dotnet run --project SourceGenerator~/Tools/UitkxMigrateImports -- &lt;dir&gt; [--check]
    /// </code>
    ///
    /// Walks <c>&lt;dir&gt;</c> for <c>.uitkx</c> files (skipping <c>~</c>-suffixed tooling folders),
    /// groups them by owning asmdef (nearest <c>*.asmdef</c> "name"), runs the migration, and writes
    /// the changed files back. <c>--check</c> makes it a dry run that exits non-zero if anything would
    /// change — the idempotence gate (run once for real, then <c>--check</c> must be clean).
    /// </summary>
    public static class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("usage: UitkxMigrateImports <dir> [--check]");
                return 2;
            }

            string root = Path.GetFullPath(args[0]);
            bool check = args.Contains("--check");
            if (!Directory.Exists(root))
            {
                Console.Error.WriteLine($"error: directory not found: {root}");
                return 2;
            }

            var files = new List<MigratorFile>();
            foreach (string path in Directory.EnumerateFiles(root, "*.uitkx", SearchOption.AllDirectories))
            {
                if (IsInsideIgnoredFolder(path)) continue;
                string asmdef = FindOwningAsmdefName(path) ?? "<Assembly-CSharp>";
                files.Add(new MigratorFile(Path.GetFullPath(path), asmdef, File.ReadAllText(path)));
            }

            var changed = UitkxMigrator.Migrate(files, out var errors);

            foreach (var e in errors)
                Console.Error.WriteLine($"warn: {e.FilePath}: {e.Message}");

            if (check)
            {
                foreach (var kv in changed)
                    Console.Error.WriteLine($"would change: {kv.Key}");
                Console.WriteLine($"{files.Count} file(s) scanned; {changed.Count} would change; {errors.Count} warning(s).");
                return changed.Count == 0 ? 0 : 1;
            }

            foreach (var kv in changed)
                File.WriteAllText(kv.Key, kv.Value);

            Console.WriteLine($"{files.Count} file(s) scanned; {changed.Count} rewritten; {errors.Count} warning(s).");
            return 0;
        }

        private static bool IsInsideIgnoredFolder(string path)
        {
            foreach (string part in path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
                if (part.EndsWith("~", StringComparison.Ordinal))
                    return true;
            return false;
        }

        private static readonly Regex s_asmdefNameRe =
            new(@"""name""\s*:\s*""([^""]+)""", RegexOptions.CultureInvariant);

        private static string? FindOwningAsmdefName(string filePath)
        {
            try
            {
                string? dir = Path.GetDirectoryName(filePath);
                while (!string.IsNullOrEmpty(dir))
                {
                    foreach (string asmdef in Directory.GetFiles(dir, "*.asmdef"))
                    {
                        var m = s_asmdefNameRe.Match(File.ReadAllText(asmdef));
                        if (m.Success) return m.Groups[1].Value.Trim();
                    }
                    if (string.Equals(Path.GetFileName(dir), "Assets", StringComparison.OrdinalIgnoreCase))
                        break;
                    dir = Path.GetDirectoryName(dir);
                }
            }
            catch { /* fall through to default assembly key */ }
            return null;
        }
    }
}
