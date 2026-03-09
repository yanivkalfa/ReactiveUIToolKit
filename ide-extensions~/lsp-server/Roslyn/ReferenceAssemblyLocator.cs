using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace UitkxLanguageServer.Roslyn
{
    /// <summary>
    /// Locates the <see cref="MetadataReference"/> assemblies that Roslyn needs to
    /// type-check the C# regions inside .uitkx files.
    ///
    /// <para><b>Resolution strategy (highest priority first):</b></para>
    /// <list type="number">
    ///   <item>
    ///     Unity project's compiled output:
    ///     <c>{workspaceRoot}/Library/ScriptAssemblies/*.dll</c>
    ///     — covers all user-defined scripts, Unity packages, and UnityEngine.dll.
    ///   </item>
    ///   <item>
    ///     Unity Editor managed folder:
    ///     <c>{unityInstall}/Editor/Data/Managed/*.dll</c> (if discoverable)
    ///     — supplements the above with Unity public-API assemblies that may not
    ///     be in ScriptAssemblies.
    ///   </item>
    ///   <item>
    ///     .NET BCL via <c>TRUSTED_PLATFORM_ASSEMBLIES</c>:
    ///     the runtime's own assembly list, covering <c>System.*</c> and friends.
    ///   </item>
    ///   <item>
    ///     Fallback: type-of references from the types statically known to the
    ///     LSP server process (<c>System.Object</c>, <c>System.Collections.Generic.List&lt;&gt;</c>,
    ///     etc.) — guarantees that at least the BCL compiles even when the Unity
    ///     project has not yet been built.
    ///   </item>
    /// </list>
    ///
    /// <para>References are cached after the first successful resolve for a given
    /// workspace root.  Call <see cref="Invalidate"/> to clear the cache when
    /// Unity recompiles (DLL change detected).</para>
    /// </summary>
    public sealed class ReferenceAssemblyLocator
    {
        private readonly object   _lock   = new object();
        private string?           _cachedRoot;
        private MetadataReference[]? _cachedRefs;

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the set of <see cref="MetadataReference"/> objects to pass to
        /// <c>CSharpCompilation.Create</c>.
        ///
        /// The result is memoised: repeated calls with the same
        /// <paramref name="workspaceRoot"/> are free after the first call.
        /// </summary>
        /// <param name="workspaceRoot">
        /// Absolute path to the Unity project root (the folder that contains
        /// <c>Assets/</c> and <c>Library/</c>).
        /// Pass <c>null</c> to skip Unity-specific discovery and return only BCL refs.
        /// </param>
        public MetadataReference[] GetReferences(string? workspaceRoot)
        {
            lock (_lock)
            {
                if (_cachedRefs != null
                    && string.Equals(_cachedRoot, workspaceRoot, StringComparison.OrdinalIgnoreCase))
                    return _cachedRefs;

                _cachedRoot = workspaceRoot;
                _cachedRefs = BuildReferences(workspaceRoot);
                return _cachedRefs;
            }
        }

        /// <summary>
        /// Clears the cached reference list so the next call to
        /// <see cref="GetReferences"/> re-scans the file system.
        /// Call this after Unity recompiles assemblies.
        /// </summary>
        public void Invalidate()
        {
            lock (_lock)
            {
                _cachedRefs = null;
            }
        }

        // ── Reference discovery ───────────────────────────────────────────────

        private static MetadataReference[] BuildReferences(string? workspaceRoot)
        {
            var refs  = new Dictionary<string, MetadataReference>(
                StringComparer.OrdinalIgnoreCase);
            var errors = new List<string>();

            // Tier 1: Unity ScriptAssemblies (compiled project + packages).
            // Walk up the directory tree from workspaceRoot to find the Unity project root
            // (the folder that contains Library/ScriptAssemblies).  This handles the common
            // case where VS Code is opened on a subfolder of the Unity project (e.g.
            // Assets/MyPackage/) rather than the project root itself.
            if (!string.IsNullOrEmpty(workspaceRoot))
            {
                string? unityProjectRoot = FindUnityProjectRoot(workspaceRoot);
                string scriptAssembliesDir = Path.Combine(
                    unityProjectRoot ?? workspaceRoot, "Library", "ScriptAssemblies");
                AddDllsFromDirectory(scriptAssembliesDir, refs, errors);

                // Tier 2: Unity Editor managed DLLs.
                // Pass A — top-level Managed/ only (non-recursive).
                //   This picks up UnityEngine.dll (the monolithic forwarder) and other
                //   top-level engine assemblies without touching the module subdirectory.
                // Pass B — subdirectories of Managed/ (recursive from one level down).
                //   UnityEngine.UIElementsModule.dll lives in Managed/UnityEngine/.
                //   We only include it when UnityEngine.dll is NOT present (i.e. the
                //   monolithic DLL is absent and we need the split modules).  When both
                //   would be present Roslyn sees duplicate type definitions → CS0433.
                string searchRoot = unityProjectRoot ?? workspaceRoot;
                string? unityInstall = TryFindUnityInstall(searchRoot);
                if (!string.IsNullOrEmpty(unityInstall))
                {
                    string managedDir = Path.Combine(unityInstall, "Editor", "Data", "Managed");
                    // Pass A: top-level Managed/ DLLs, but skip UnityEngine.dll itself.
                    //   In Unity 6+, UnityEngine.dll is a type-forwarder shell whose types
                    //   are redundantly defined in the module DLLs (e.g. UnityEngine.UIElements
                    //   Module.dll). Including both causes Roslyn CS0433 "type exists in two
                    //   assemblies". We get all real type definitions from the module DLLs in
                    //   Pass B, so the forwarder shell is not needed.
                    AddDllsFromDirectory(managedDir, refs, errors, recursive: false,
                        skipPredicate: (string name) =>
                            string.Equals(name, "UnityEngine.dll", StringComparison.OrdinalIgnoreCase));
                    // Pass B: subdirectories of Managed/ — picks up split module assemblies
                    //   such as UnityEngine.UIElementsModule.dll in Managed/UnityEngine/.
                    foreach (var subDir in Directory.EnumerateDirectories(managedDir))
                        AddDllsFromDirectory(subDir, refs, errors, recursive: true);
                }
            }

            // Tier 3: .NET BCL via trusted-platform-assemblies list
            AddTrustedPlatformAssemblies(refs, errors);

            // Tier 4: Fallback BCL from well-known types in this process
            AddFallbackBcl(refs);

            if (errors.Count > 0)
                ServerLog.Log(
                    $"[ReferenceAssemblyLocator] {errors.Count} error(s) during reference scan:\n"
                    + string.Join("\n", errors));

            ServerLog.Log(
                $"[ReferenceAssemblyLocator] Resolved {refs.Count} metadata references.");

            return refs.Values.ToArray();
        }

        private static void AddDllsFromDirectory(
            string dir,
            Dictionary<string, MetadataReference> target,
            List<string> errors,
            bool recursive = false,
            Func<string, bool>? skipPredicate = null)
        {
            if (!Directory.Exists(dir))
                return;

            var option = recursive
                ? SearchOption.AllDirectories
                : SearchOption.TopDirectoryOnly;

            foreach (var dll in Directory.EnumerateFiles(dir, "*.dll", option))
            {
                string key = Path.GetFileName(dll);
                if (target.ContainsKey(key))
                    continue; // higher-priority tier wins

                if (skipPredicate != null && skipPredicate(key))
                    continue; // caller-supplied exclusion

                try
                {
                    target[key] = MetadataReference.CreateFromFile(dll);
                }
                catch (Exception ex)
                {
                    errors.Add($"  {dll}: {ex.Message}");
                }
            }
        }

        private static void AddTrustedPlatformAssemblies(
            Dictionary<string, MetadataReference> target,
            List<string> errors)
        {
            // AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") returns a
            // semicolon-separated list of absolute DLL paths that the current
            // .NET runtime trusts — this is the BCL on .NET Core / .NET 5+.
            string? tpa = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
            if (string.IsNullOrEmpty(tpa))
                return;

            foreach (var path in tpa.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                if (!File.Exists(path))
                    continue;

                string key = Path.GetFileName(path);
                if (target.ContainsKey(key))
                    continue;

                try
                {
                    target[key] = MetadataReference.CreateFromFile(path);
                }
                catch (Exception ex)
                {
                    errors.Add($"  {path}: {ex.Message}");
                }
            }
        }

        private static void AddFallbackBcl(Dictionary<string, MetadataReference> target)
        {
            // Static fallback: the exact assemblies we know are loaded in this process.
            // This guarantees basic BCL types compile even when TRUSTED_PLATFORM_ASSEMBLIES
            // is unavailable (e.g. when running on older .NET runtimes).
            var fallbackTypes = new[]
            {
                typeof(object),                    // System.Private.CoreLib / mscorlib
                typeof(System.Linq.Enumerable),    // System.Linq
                typeof(System.Collections.Generic.List<>), // System.Collections
                typeof(System.IO.File),            // System.IO
                typeof(System.Text.StringBuilder), // System.Text
                typeof(System.Threading.Thread),   // System.Threading
            };

            foreach (var t in fallbackTypes)
            {
                try
                {
                    string? location = t.Assembly.Location;
                    if (string.IsNullOrEmpty(location) || !File.Exists(location))
                        continue;

                    string key = Path.GetFileName(location);
                    if (target.ContainsKey(key))
                        continue;

                    target[key] = MetadataReference.CreateFromFile(location);
                }
                catch
                {
                    // Best-effort fallback — silent on failure
                }
            }
        }

        // ── Unity installation discovery ──────────────────────────────────────

        /// <summary>
        /// Walks up the directory tree from <paramref name="startDir"/> to find the
        /// Unity project root — the ancestor folder whose <c>Library/ScriptAssemblies</c>
        /// subdirectory exists.  Returns <c>null</c> if no such ancestor is found.
        /// </summary>
        private static string? FindUnityProjectRoot(string startDir)
        {
            var dir = new DirectoryInfo(startDir);
            while (dir != null)
            {
                string candidate = Path.Combine(dir.FullName, "Library", "ScriptAssemblies");
                if (Directory.Exists(candidate))
                    return dir.FullName;
                dir = dir.Parent;
            }
            return null;
        }

        /// <summary>
        /// Attempts to locate the Unity Editor install directory from a
        /// <c>ProjectSettings/ProjectVersion.txt</c> file in the workspace.
        /// Returns <c>null</c> if the install directory cannot be determined.
        /// </summary>
        private static string? TryFindUnityInstall(string workspaceRoot)
        {
            // Read the Unity version used by this project
            string versionFile = Path.Combine(workspaceRoot, "ProjectSettings", "ProjectVersion.txt");
            if (!File.Exists(versionFile))
                return null;

            string? version = null;
            foreach (var line in File.ReadLines(versionFile))
            {
                // Line format: "m_EditorVersion: 2022.3.12f1"
                var trimmed = line.Trim();
                if (trimmed.StartsWith("m_EditorVersion:", StringComparison.Ordinal))
                {
                    version = trimmed.Substring("m_EditorVersion:".Length).Trim();
                    break;
                }
            }

            if (string.IsNullOrEmpty(version))
                return null;

            // Well-known install locations per OS
            var candidates = new List<string>();

            if (OperatingSystem.IsWindows())
            {
                candidates.Add($@"C:\Program Files\Unity\Hub\Editor\{version}");
                candidates.Add($@"C:\Program Files\Unity\{version}");
                candidates.Add($@"C:\Program Files (x86)\Unity\{version}");
            }
            else if (OperatingSystem.IsMacOS())
            {
                candidates.Add($"/Applications/Unity/Hub/Editor/{version}/Unity.app/Contents");
                candidates.Add($"/Applications/Unity/{version}/Unity.app/Contents");
            }
            else // Linux
            {
                candidates.Add($"/opt/unity/hub/editor/{version}");
                candidates.Add($"{Environment.GetFolderPath(Environment.SpecialFolder.Personal)}/Unity/Hub/Editor/{version}");
            }

            return candidates.FirstOrDefault(Directory.Exists);
        }
    }
}
