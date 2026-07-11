using System;
using System.Collections.Generic;

namespace ReactiveUITK.Language
{
    /// <summary>Outcome of resolving an import specifier (import/export grammar, leg 3, plan §6).</summary>
    public enum ImportResolveStatus
    {
        /// <summary>Resolved to an existing file in the same asmdef.</summary>
        Ok,
        /// <summary>UITKX2300 — the specifier does not map to an existing <c>.uitkx</c> (incl. engine-native forms).</summary>
        UnknownSpecifier,
        /// <summary>UITKX2308 — the target is owned by a different asmdef (imports are module-scoped in v1).</summary>
        CrossesBoundary,
        /// <summary>UITKX2314 — a <c>~/</c> specifier resolves outside the project root.</summary>
        RootEscape,
    }

    /// <summary>Resolution result: a status plus the project-relative candidate path (for diagnostics/recording).</summary>
    public readonly record struct ImportResolveResult(ImportResolveStatus Status, string? ProjectRelativePath);

    /// <summary>
    /// Resolves an import specifier to a project-relative <c>.uitkx</c> path (plan §6). Only relative
    /// (<c>./</c>, <c>../</c>) and root-alias (<c>~/</c>) forms are legal; engine-native forms
    /// (<c>Assets/</c>, <c>Packages/</c>, absolute) never map → UITKX2300. Comparison is ordinal
    /// (case-sensitive on the project-relative path). Filesystem + owning-asmdef access is injected so
    /// this is unit-testable and host-agnostic (SG and LSP both call it).
    /// </summary>
    public static class ImportResolver
    {
        /// <summary>
        /// Map a specifier to a project-relative candidate <c>.uitkx</c> path (pure, no filesystem).
        /// Returns null for engine-native/bare specifiers (never resolvable) or when the path escapes
        /// above its base (sets <paramref name="escapedRoot"/>). Extensionless specifiers get
        /// <c>.uitkx</c> appended; a specifier already ending in <c>.uitkx</c> is left as-is.
        /// </summary>
        public static string? MapSpecifierToPath(
            string importerProjectRelativeDir, string specifier, string rootProjectRelativeDir,
            out bool escapedRoot)
        {
            escapedRoot = false;
            if (string.IsNullOrEmpty(specifier))
                return null;

            string baseDir, rest;
            if (specifier.StartsWith("./", StringComparison.Ordinal) ||
                specifier.StartsWith("../", StringComparison.Ordinal))
            {
                baseDir = importerProjectRelativeDir;
                rest = specifier;
            }
            else if (specifier.StartsWith("~/", StringComparison.Ordinal))
            {
                baseDir = rootProjectRelativeDir ?? string.Empty;
                rest = specifier.Substring(2);
            }
            else
            {
                return null; // engine-native (Assets/, Packages/, absolute) or bare → never mapped
            }

            var segs = new List<string>();
            string combined = (baseDir + "/" + rest).Replace('\\', '/');
            foreach (var s in combined.Split('/'))
            {
                if (s.Length == 0 || s == ".")
                    continue;
                if (s == "..")
                {
                    if (segs.Count == 0) { escapedRoot = true; return null; }
                    segs.RemoveAt(segs.Count - 1);
                }
                else
                {
                    segs.Add(s);
                }
            }

            string path = string.Join("/", segs);
            if (path.Length == 0)
                return null;
            if (!path.EndsWith(".uitkx", StringComparison.Ordinal))
                path += ".uitkx";
            return path;
        }

        /// <summary>
        /// Full resolution with injected lookups. <paramref name="fileExists"/> and
        /// <paramref name="owningAsmdefOf"/> take a project-relative path. Precedence:
        /// <c>~/</c>-escape → 2314; unresolvable/engine-native or not-found → 2300; different
        /// asmdef → 2308; else Ok.
        /// </summary>
        public static ImportResolveResult Resolve(
            string importerProjectRelativeDir,
            string specifier,
            string rootProjectRelativeDir,
            Func<string, bool> fileExists,
            Func<string, string?> owningAsmdefOf,
            string? importerAsmdef)
        {
            string? path = MapSpecifierToPath(
                importerProjectRelativeDir, specifier, rootProjectRelativeDir, out bool escapedRoot);

            if (escapedRoot && specifier.StartsWith("~/", StringComparison.Ordinal))
                return new ImportResolveResult(ImportResolveStatus.RootEscape, null);
            if (path == null)
                return new ImportResolveResult(ImportResolveStatus.UnknownSpecifier, null);
            if (!fileExists(path))
                return new ImportResolveResult(ImportResolveStatus.UnknownSpecifier, path);

            string? targetAsmdef = owningAsmdefOf(path);
            if (!string.Equals(targetAsmdef, importerAsmdef, StringComparison.Ordinal))
                return new ImportResolveResult(ImportResolveStatus.CrossesBoundary, path);

            return new ImportResolveResult(ImportResolveStatus.Ok, path);
        }
    }
}
