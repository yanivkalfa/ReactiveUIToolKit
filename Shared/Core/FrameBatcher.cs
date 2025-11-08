using System;
using System.Collections.Generic;
using UnityEngine;

namespace ReactiveUITK.Core
{
    /// <summary>
    /// Coalesces multiple component state updates into a single flush per frame.
    /// Runtime version uses UnityEngine callbacks; editor can still piggyback on Editor scheduler.
    /// </summary>
    internal static class FrameBatcher
    {
        private static readonly HashSet<NodeMetadata> pending = new();
        private static bool scheduled;
        private static int batchedUpdateCountThisFrame;
        private static int lastFlushedCount;
        private static int lastFrameId;

        public static int LastFlushComponentCount => lastFlushedCount;
        public static int LastFrameBatchedUpdates => batchedUpdateCountThisFrame;

        public static void Enqueue(NodeMetadata metadata)
        {
            if (metadata == null || metadata.Reconciler == null)
            {
                return;
            }
            if (pending.Add(metadata))
            {
                metadata.UpdateQueued = true;
            }
            batchedUpdateCountThisFrame++;
            if (!scheduled)
            {
                ScheduleFlush();
            }
        }

        private static void ScheduleFlush()
        {
            scheduled = true;
            // Runtime: hidden driver MonoBehaviour for per-frame Update.
            FrameBatchDriver.Ensure();
#if UNITY_EDITOR
            // Editor (edit mode, not playing): MonoBehaviour.Update won't fire unless in play mode, so use EditorApplication.update.
            if (!UnityEngine.Application.isPlaying)
            {
                EnsureEditorHook();
            }
#endif
        }

#if UNITY_EDITOR
        private static bool editorHooked;

        private static void EnsureEditorHook()
        {
            if (editorHooked)
                return;
            UnityEditor.EditorApplication.update -= EditorPump;
            UnityEditor.EditorApplication.update += EditorPump;
            editorHooked = true;
        }

        private static void EditorPump()
        {
            if (UnityEngine.Application.isPlaying)
            {
                // Play mode handled by driver MonoBehaviour Update.
                return;
            }
            if (scheduled)
            {
                Flush();
            }
        }
#endif

        private static void Flush()
        {
            scheduled = false;
            if (pending.Count == 0)
            {
                lastFlushedCount = 0;
                return;
            }

            // Snapshot pending updates before executing; components may enqueue new updates during flush.
            var toFlush = new List<NodeMetadata>(pending);
            pending.Clear();
            lastFlushedCount = toFlush.Count;

            foreach (var meta in toFlush)
            {
                if (meta == null)
                {
                    continue;
                }
                try
                {
                    meta.UpdateQueued = false;
                    meta.HookIndex = 0;
                    meta.Reconciler?.ForceFunctionComponentUpdate(meta);
                }
                catch (Exception ex)
                {
                    try
                    {
                        Debug.LogWarning($"[FrameBatcher] Flush failure: {ex}");
                    }
                    catch { }
                }
            }
            pending.Clear();
        }

        private sealed class FrameBatchDriver : MonoBehaviour
        {
            private static FrameBatchDriver instance;

            internal static void Ensure()
            {
                if (instance != null)
                    return;
                var go = new GameObject("__ReactiveUITK_FrameBatchDriver");
                go.hideFlags = HideFlags.HideAndDontSave;
                instance = go.AddComponent<FrameBatchDriver>();
            }

            private void Update()
            {
                int currentFrame = Time.frameCount;
                if (currentFrame != lastFrameId)
                {
                    lastFrameId = currentFrame;
                    batchedUpdateCountThisFrame = 0; // reset counter for metrics per frame
                }
                if (scheduled)
                {
                    Flush();
                }
            }

            private void OnDisable()
            {
                if (scheduled)
                {
                    Flush();
                }
            }
        }
    }
}
