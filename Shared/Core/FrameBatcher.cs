using System;
using System.Collections.Generic;
using UnityEngine;

namespace ReactiveUITK.Core
{
    
    
    
    
    internal static class FrameBatcher
    {
        private static readonly HashSet<NodeMetadata> pending = new();
        private static bool scheduled;
        private static int batchedUpdateCountThisFrame;
        private static int lastFlushedCount;
        private static int lastFrameId;
        private static readonly List<NodeMetadata> flushBuffer = new();

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
                var state = metadata.EnsureComponentState();
                if (state != null)
                {
                    state.UpdateQueued = true;
                    metadata.SyncComponentState(state);
                }
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
            
            FrameBatchDriver.Ensure();
#if UNITY_EDITOR
            
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
            {
                return;
            }
            UnityEditor.EditorApplication.update -= EditorPump;
            UnityEditor.EditorApplication.update += EditorPump;
            editorHooked = true;
        }

        private static void EditorPump()
        {
            if (UnityEngine.Application.isPlaying)
            {
                
                return;
            }
            if (scheduled)
            {
                FlushPendingNow();
            }
        }
#endif

        private static void FlushPendingNow()
        {
            scheduled = false;
            if (pending.Count == 0)
            {
                lastFlushedCount = 0;
                return;
            }

            
            flushBuffer.Clear();
            flushBuffer.AddRange(pending);
            pending.Clear();
            lastFlushedCount = flushBuffer.Count;

            foreach (var meta in flushBuffer)
            {
                if (meta == null)
                {
                    continue;
                }
                try
                {
                    var state = meta.EnsureComponentState();
                    if (state != null)
                    {
                        state.UpdateQueued = false;
                        state.HookIndex = 0;
                        meta.SyncComponentState(state);
                    }
                    meta.Reconciler?.ForceFunctionComponentUpdate(meta);
                }
                catch (Exception ex)
                {
                    try
                    {
                        Debug.LogWarning($"[FrameBatcher] Flush failure: {ex}");
                    }
                    catch
                    {
                    }
                }
            }
            flushBuffer.Clear();
        }

        public static void FlushSync(Action action = null)
        {
            if (action != null)
            {
                try
                {
                    action();
                }
                finally
                {
                    FlushPendingNow();
                }
            }
            else
            {
                FlushPendingNow();
            }
        }

        private sealed class FrameBatchDriver : MonoBehaviour
        {
            private static FrameBatchDriver instance;

            internal static void Ensure()
            {
                if (instance != null)
                {
                    return;
                }
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
                    batchedUpdateCountThisFrame = 0; 
                }
                if (scheduled)
                {
                    FlushPendingNow();
                }
            }

            private void OnDisable()
            {
                if (scheduled)
                {
                    FlushPendingNow();
                }
            }
        }
    }
}
