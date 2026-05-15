using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ReactiveUITK.EditorSupport.HMR
{
    /// <summary>
    /// Hooks Unity's <c>AssetPostprocessor.OnPostprocessAllAssets</c> as a
    /// redundant event source for the active HMR file watcher.
    ///
    /// Why this exists: <see cref="System.IO.FileSystemWatcher"/> on Mono +
    /// Windows uses an 8 KB internal buffer that overflows under save bursts
    /// on a deep <c>Assets/</c> tree (every save also touches <c>.meta</c>
    /// files, etc.). On overflow the OS silently drops events for arbitrary
    /// files, which manifests as "save the .uitkx file but HMR never reacts"
    /// for whichever file happened to be in the dropped batch — particularly
    /// for files several folders deep.
    ///
    /// Touching FSW configuration to fix this (raising InternalBufferSize,
    /// reordering EnableRaisingEvents, subscribing Error) has proven fragile
    /// on some Mono versions where it leaves the watcher unable to deliver
    /// events at all. So instead of changing FSW, we add a parallel event
    /// source: AssetPostprocessor fires synchronously on the main thread
    /// whenever Unity refreshes the asset database after a save, never drops
    /// events, and does not depend on Mono FSW. The watcher's
    /// <c>_pendingChanges</c> dictionary already deduplicates by path, so
    /// redundant events from FSW + AssetPostprocessor are harmless — the
    /// debounce window coalesces them into a single swap.
    ///
    /// The watcher is registered on Start and unregistered on Stop. While
    /// no watcher is active, this postprocessor is a no-op.
    /// </summary>
    internal sealed class UitkxHmrAssetPostprocessor : AssetPostprocessor
    {
        // HMR is singleton-scoped via UitkxHmrController, so there is at
        // most one live watcher at any time. A list keeps registration
        // symmetric and avoids races on Start before Stop fully completes.
        private static readonly List<UitkxHmrFileWatcher> s_watchers = new();

        public static void Register(UitkxHmrFileWatcher watcher)
        {
            if (watcher == null) return;
            lock (s_watchers)
            {
                if (!s_watchers.Contains(watcher))
                    s_watchers.Add(watcher);
            }
        }

        public static void Unregister(UitkxHmrFileWatcher watcher)
        {
            if (watcher == null) return;
            lock (s_watchers)
            {
                s_watchers.Remove(watcher);
            }
        }

        // Unity invokes this on the main thread after every asset import
        // pass that touches at least one asset. Paths are project-relative
        // ("Assets/Foo/Bar.uitkx"), forward-slash separated.
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths
        )
        {
            UitkxHmrFileWatcher[] snapshot;
            lock (s_watchers)
            {
                if (s_watchers.Count == 0)
                    return;
                snapshot = s_watchers.ToArray();
            }

            string projectRoot = Path.GetDirectoryName(
                Path.GetFullPath(Application.dataPath)
            );

            void Forward(string assetPath)
            {
                if (string.IsNullOrEmpty(assetPath))
                    return;
                if (!HasInterestingExtension(assetPath))
                    return;
                string abs = Path.GetFullPath(Path.Combine(projectRoot, assetPath));
                for (int i = 0; i < snapshot.Length; i++)
                    snapshot[i].EnqueueAssetChange(abs);
            }

            // Deletion / move-from forwarding. Without this, the controller's
            // _pendingRetryPaths can hold a key for a path that no longer
            // exists (rename moves the file out from under it), and every
            // retry pass throws FileNotFoundException forever. Routing the
            // old paths to a dedicated cleanup sink evicts those keys.
            // See Plans~/HMR_NEW_COMPONENT_LIVE_SWAP_PLAN.md §4.2.
            void ForwardDeletion(string assetPath)
            {
                if (string.IsNullOrEmpty(assetPath))
                    return;
                if (!HasInterestingExtension(assetPath))
                    return;
                string abs = Path.GetFullPath(Path.Combine(projectRoot, assetPath));
                for (int i = 0; i < snapshot.Length; i++)
                    snapshot[i].EnqueueAssetDeletion(abs);
            }

            for (int i = 0; i < importedAssets.Length; i++)
                Forward(importedAssets[i]);
            for (int i = 0; i < movedAssets.Length; i++)
                Forward(movedAssets[i]);
            for (int i = 0; i < deletedAssets.Length; i++)
                ForwardDeletion(deletedAssets[i]);
            for (int i = 0; i < movedFromAssetPaths.Length; i++)
                ForwardDeletion(movedFromAssetPaths[i]);
        }

        private static bool HasInterestingExtension(string assetPath)
        {
            if (assetPath.EndsWith(".uitkx", System.StringComparison.OrdinalIgnoreCase)) return true;
            if (assetPath.EndsWith(".uss", System.StringComparison.OrdinalIgnoreCase)) return true;
            if (assetPath.EndsWith(".cs", System.StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
    }
}
