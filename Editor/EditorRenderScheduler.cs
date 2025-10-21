using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ReactiveUITK.Core;

namespace ReactiveUITK.EditorSupport
{
    [InitializeOnLoad]
    public sealed class EditorRenderScheduler : IScheduler
    {
        private static readonly Queue<Action> highPriorityQueue = new();
        private static readonly Queue<Action> normalPriorityQueue = new();
        private static readonly Queue<Action> lowPriorityQueue = new();
        private static readonly Queue<Action> idlePriorityQueue = new();
        private static readonly List<Action> batchedEffectActions = new();
        private static readonly List<Action> deferredBatchEnqueueActions = new();
        private static bool batchModeEnabled;

        static EditorRenderScheduler()
        {
            EditorApplication.update -= Pump;
            EditorApplication.update += Pump;
        }

        public static EditorRenderScheduler Instance { get; } = new EditorRenderScheduler();

        public void Enqueue(Action action, IScheduler.Priority priority = IScheduler.Priority.Normal)
        {
            if (action == null)
            {
                return;
            }
            if (batchModeEnabled && priority != IScheduler.Priority.High)
            {
                deferredBatchEnqueueActions.Add(() => Enqueue(action, priority));
                return;
            }
            switch (priority)
            {
                case IScheduler.Priority.High:
                    highPriorityQueue.Enqueue(action);
                    break;
                case IScheduler.Priority.Normal:
                    normalPriorityQueue.Enqueue(action);
                    break;
                case IScheduler.Priority.Low:
                    lowPriorityQueue.Enqueue(action);
                    break;
                case IScheduler.Priority.Idle:
                    idlePriorityQueue.Enqueue(action);
                    break;
            }
        }

        public void BeginBatch()
        {
            batchModeEnabled = true;
        }

        public void EndBatch()
        {
            batchModeEnabled = false;
            foreach (Action a in deferredBatchEnqueueActions)
            {
                a();
            }
            deferredBatchEnqueueActions.Clear();
        }

        public void EnqueueBatchedEffect(Action effect)
        {
            if (effect != null)
            {
                batchedEffectActions.Add(effect);
            }
        }

        private static void Pump()
        {
            ExecuteQueue(highPriorityQueue);
            ExecuteQueue(normalPriorityQueue);
            ExecuteQueue(lowPriorityQueue);
            ExecuteQueue(idlePriorityQueue);
            if (batchedEffectActions.Count > 0)
            {
                foreach (var e in batchedEffectActions)
                {
                    try { e(); } catch (Exception ex) { Debug.LogError(ex); }
                }
                batchedEffectActions.Clear();
            }
        }

        private static void ExecuteQueue(Queue<Action> q)
        {
            while (q.Count > 0)
            {
                var a = q.Dequeue();
                try { a(); } catch (Exception ex) { Debug.LogError(ex); }
            }
        }
    }
}

