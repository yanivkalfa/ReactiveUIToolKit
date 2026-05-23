#if UNITY_EDITOR
#nullable enable annotations
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ReactiveUITK.EditorSupport.HMR
{
    // ─────────────────────────────────────────────────────────────────────────
    //  NewCsFileDiscovery — Rank 2 of the TECH_DEBT_20_21_22 plan
    //
    //  Closes failure mode #22A: when the user adds a brand-new `.cs` helper
    //  inside an asmdef AND references it from a `.uitkx` in the same asmdef
    //  before Unity has had a chance to recompile the project DLL, the HMR
    //  pipeline used to loop on `CS0246` because UitkxHmrCompiler.Compile
    //  only sees types already loaded in AppDomain.
    //
    //  This helper enumerates every `.cs` file owned by the asmdef of the
    //  changed `.uitkx` and returns the ones whose primary type-name is NOT
    //  already present in any loaded assembly. Those files are added as
    //  additional Roslyn syntax trees in the HMR compile pipeline.
    //
    //  Deliberately uses AppDomain reflection (not Roslyn symbol queries) for
    //  the dedupe check so we don't have to round-trip the project DLL through
    //  a Roslyn Compilation just to look up type names — that would more than
    //  double HMR compile times on cold cache.
    //
    //  Cross-asmdef boundary is enforced: Unity treats every asmdef as its own
    //  compilation unit and never lets one asmdef recompile against another's
    //  in-flight HMR DLL. Picking up a `.cs` from a different asmdef would
    //  pollute the union with types that have no business in this HMR assembly.
    //
    //  Tilde folders are skipped (same `~`-suffix rule used everywhere else).
    // ─────────────────────────────────────────────────────────────────────────
    internal static class NewCsFileDiscovery
    {
        // Top-level `class|struct|interface|record|enum Foo` declaration. We
        // capture the first such declaration in each file; that's the usual
        // primary type and is what consumers reference by name. Nested types
        // are not tracked — they would only matter for outer-class refs which
        // already fall through the AppDomain dedupe via the outer name.
        private static readonly Regex s_primaryTypeRegex = new Regex(
            @"^\s*(?:public|internal|private|protected|sealed|static|abstract|partial|new|unsafe|readonly|\s)*\b" +
            @"(?:class|struct|interface|record|enum)\s+(?<name>[A-Za-z_]\w*)",
            RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// Enumerates `.cs` files owned by <paramref name="asmdef"/> that
        /// declare a top-level type-name not yet present in any loaded
        /// assembly. Returns absolute file paths.
        /// </summary>
        /// <param name="asmdef">
        /// Asmdef name (e.g. <c>Assembly-CSharp</c>) of the changed `.uitkx`.
        /// Cross-asmdef `.cs` files are intentionally skipped.
        /// </param>
        /// <param name="rootDir">
        /// Project root (typically <c>Application.dataPath</c>'s parent) used
        /// as the file-tree scan root. Empty or missing returns an empty list.
        /// </param>
        /// <param name="newerThanUtc">
        /// Only consider `.cs` files modified after this UTC timestamp. The
        /// caller usually passes the project DLL's last-write time; older
        /// files are already in AppDomain and will be deduped out anyway.
        /// </param>
        /// <param name="alreadyIncluded">
        /// Set of `.cs` paths already added to the compile via the watcher's
        /// same-folder companion logic — skipped to avoid duplicate trees.
        /// </param>
        public static List<string> FindForAsmdef(
            string asmdef,
            string rootDir,
            DateTime newerThanUtc,
            ICollection<string>? alreadyIncluded)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(asmdef) || string.IsNullOrEmpty(rootDir))
                return result;
            if (!Directory.Exists(rootDir))
                return result;

            // Build the set of type-names already present in AppDomain. This
            // is the canonical dedupe source — any name in here would CS0101
            // if we added a fresh declaration of it.
            HashSet<string> loadedTypeNames;
            try
            {
                loadedTypeNames = CollectLoadedTypeNames();
            }
            catch
            {
                // If reflection blows up (rare; ReflectionTypeLoadException)
                // bail out conservatively — better to not pick up new files
                // than to risk redeclaring a loaded type.
                return result;
            }

            var skip = alreadyIncluded ?? Array.Empty<string>();
            var skipSet = skip as HashSet<string> ?? new HashSet<string>(
                skip.Select(p => { try { return Path.GetFullPath(p); } catch { return p; } }),
                StringComparer.OrdinalIgnoreCase);

            IEnumerable<string> candidates;
            try
            {
                candidates = Directory.EnumerateFiles(rootDir, "*.cs", SearchOption.AllDirectories);
            }
            catch
            {
                return result;
            }

            foreach (var path in candidates)
            {
                if (IsInsideTildeFolder(path))
                    continue;
                // Skip generator output — these are emitted at build time and
                // would CS0101 against the project DLL.
                if (path.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase))
                    continue;

                string full;
                try { full = Path.GetFullPath(path); } catch { continue; }
                if (skipSet.Contains(full))
                    continue;

                // mtime gate — avoid even reading files older than the project
                // DLL (those are guaranteed already in AppDomain).
                try
                {
                    if (File.GetLastWriteTimeUtc(full) <= newerThanUtc)
                        continue;
                }
                catch { continue; }

                // Asmdef gate — must own the file. AsmdefResolver caches.
                string ownAsmdef;
                try { ownAsmdef = AsmdefResolver.OwningAsmdefName(full); }
                catch { continue; }
                if (!string.Equals(ownAsmdef, asmdef, StringComparison.Ordinal))
                    continue;

                // Type-name dedupe — refuse if any primary type-name already
                // exists in AppDomain.
                string text;
                try { text = File.ReadAllText(full); }
                catch { continue; }

                bool conflict = false;
                foreach (Match m in s_primaryTypeRegex.Matches(text))
                {
                    string name = m.Groups["name"].Value;
                    if (loadedTypeNames.Contains(name))
                    {
                        conflict = true;
                        break;
                    }
                }
                if (conflict)
                    continue;

                result.Add(full);
            }

            return result;
        }

        private static HashSet<string> CollectLoadedTypeNames()
        {
            var set = new HashSet<string>(StringComparer.Ordinal);
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[]? types;
                try { types = asm.GetTypes(); }
                catch (ReflectionTypeLoadException rtle) { types = rtle.Types; }
                catch { continue; }
                if (types == null) continue;

                foreach (var t in types)
                {
                    if (t == null) continue;
                    // Use the simple name; the regex above also captures the
                    // simple name, so comparison is apples-to-apples.
                    if (!string.IsNullOrEmpty(t.Name))
                        set.Add(t.Name);
                }
            }
            return set;
        }

        private static bool IsInsideTildeFolder(string filePath)
        {
            foreach (var seg in (filePath ?? string.Empty).Split(
                         new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                         StringSplitOptions.RemoveEmptyEntries))
            {
                if (seg.Length > 1 && seg[seg.Length - 1] == '~')
                    return true;
            }
            return false;
        }
    }
}
#endif
