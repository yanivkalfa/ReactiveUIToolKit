using System;
using System.Collections.Generic;
using UnityEngine;

using ReactiveUITK.Core;

namespace ReactiveUITK
{
    public sealed class RenderScheduler : MonoBehaviour, IScheduler
    {
        private readonly Queue<Action> highPriorityQueue = new();
        private readonly Queue<Action> normalPriorityQueue = new();
        private readonly Queue<Action> lowPriorityQueue = new();
        private readonly Queue<Action> idlePriorityQueue = new();
        [SerializeField] private float frameBudgetMs = 4.0f;
        private readonly List<Action> batchedEffectActions = new();
        private readonly List<Action> deferredBatchEnqueueActions = new();
        private bool batchModeEnabled;
        private int renderedFrameCount;
        private int executedActionCount;
        private float lastFrameStartTimestampMs;
        private int escalationCount;
        private int lowPriorityCancelledCount;
        private int idleExecutedCount;

        public static RenderScheduler Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

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
            foreach (Action enqueueAction in deferredBatchEnqueueActions)
            {
                enqueueAction();
            }
            deferredBatchEnqueueActions.Clear();
        }

        private void LateUpdate()
        {
            float frameStart = Time.realtimeSinceStartup * 1000f;
            lastFrameStartTimestampMs = frameStart;
            if (highPriorityQueue.Count > 0 && lowPriorityQueue.Count > 0)
            {
                lowPriorityCancelledCount += lowPriorityQueue.Count;
                lowPriorityQueue.Clear();
            }
            ExecuteQueue(highPriorityQueue, ref frameStart);
            if (highPriorityQueue.Count == 0)
            {
                ExecuteQueue(normalPriorityQueue, ref frameStart);
            }
            else
            {
                escalationCount++;
            }
            ExecuteQueue(lowPriorityQueue, ref frameStart);
            if ((Time.realtimeSinceStartup * 1000f) - frameStart < frameBudgetMs * 0.5f)
            {
                idleExecutedCount += ExecuteQueue(idlePriorityQueue, ref frameStart, allowOverBudget: false);
            }
            FlushBatchedEffects();
            renderedFrameCount++;
        }

        private int ExecuteQueue(Queue<Action> queue, ref float frameStartTimestampMs, bool allowOverBudget = true)
        {
            int executedCount = 0;
            while (queue.Count > 0)
            {
                if ((Time.realtimeSinceStartup * 1000f) - frameStartTimestampMs > frameBudgetMs)
                {
                    // Respect frame budget (unchanged behavior); parameter currently unused
                    break;
                }
                Action action = queue.Dequeue();
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Debug.LogError("RenderScheduler action exception: " + ex);
                }
                executedActionCount++;
                executedCount++;
            }
            return executedCount;
        }

        public void EnqueueBatchedEffect(Action effect)
        {
            if (effect != null)
            {
                batchedEffectActions.Add(effect);
            }
        }

        private void FlushBatchedEffects()
        {
            if (batchedEffectActions.Count == 0)
            {
                return;
            }
            foreach (Action effectAction in batchedEffectActions)
            {
                try
                {
                    effectAction();
                }
                catch (Exception ex)
                {
                    Debug.LogError("Effect exception: " + ex);
                }
            }
            batchedEffectActions.Clear();
        }

        public (int frames, int actions, int escalations, int lowCancelled, int idleRan) GetMetrics() => (renderedFrameCount, executedActionCount, escalationCount, lowPriorityCancelledCount, idleExecutedCount);
    }
}
