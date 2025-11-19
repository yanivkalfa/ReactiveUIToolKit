using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine;

namespace ReactiveUITK
{
    public sealed class RenderScheduler : MonoBehaviour, IScheduler
    {
        private readonly Queue<Action> highPriorityQueue = new();
        private readonly Queue<Action> normalPriorityQueue = new();
        private readonly Queue<Action> lowPriorityQueue = new();
        private readonly Queue<Action> idlePriorityQueue = new();
        private readonly HashSet<Action> highPriorityTracker = new();
        private readonly HashSet<Action> normalPriorityTracker = new();
        private readonly HashSet<Action> lowPriorityTracker = new();
        private readonly HashSet<Action> idlePriorityTracker = new();

        [SerializeField]
        private float frameBudgetMs = 4.0f;
        private readonly List<Action> batchedEffectActions = new();
        private readonly List<Action> deferredBatchEnqueueActions = new();
        private int batchDepth;
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

        public void Enqueue(
            Action action,
            IScheduler.Priority priority = IScheduler.Priority.Normal
        )
        {
            if (action == null)
            {
                return;
            }
            if (batchDepth > 0 && priority != IScheduler.Priority.High)
            {
                deferredBatchEnqueueActions.Add(() => Enqueue(action, priority));
                return;
            }
            switch (priority)
            {
                case IScheduler.Priority.High:
                    if (highPriorityTracker.Add(action))
                    {
                        highPriorityQueue.Enqueue(action);
                    }
                    break;
                case IScheduler.Priority.Normal:
                    if (normalPriorityTracker.Add(action))
                    {
                        normalPriorityQueue.Enqueue(action);
                    }
                    break;
                case IScheduler.Priority.Low:
                    if (lowPriorityTracker.Add(action))
                    {
                        lowPriorityQueue.Enqueue(action);
                    }
                    break;
                case IScheduler.Priority.Idle:
                    if (idlePriorityTracker.Add(action))
                    {
                        idlePriorityQueue.Enqueue(action);
                    }
                    break;
            }
        }

        public void BeginBatch()
        {
            batchDepth++;
        }

        public void EndBatch()
        {
            if (batchDepth == 0)
            {
                return;
            }
            batchDepth--;
            if (batchDepth > 0)
            {
                return;
            }
            if (deferredBatchEnqueueActions.Count == 0)
            {
                return;
            }
            var snapshot = deferredBatchEnqueueActions.ToArray();
            deferredBatchEnqueueActions.Clear();
            foreach (Action enqueueAction in snapshot)
            {
                enqueueAction();
            }
        }

        private void LateUpdate()
        {
            float frameStart = Time.realtimeSinceStartup * 1000f;
            lastFrameStartTimestampMs = frameStart;
            if (highPriorityQueue.Count > 0 && lowPriorityQueue.Count > 0)
            {
                lowPriorityCancelledCount += lowPriorityQueue.Count;
                while (lowPriorityQueue.Count > 0)
                {
                    var removed = lowPriorityQueue.Dequeue();
                    lowPriorityTracker.Remove(removed);
                }
            }
            int highRan = ExecuteQueue(highPriorityQueue, highPriorityTracker, ref frameStart);
            int normalRan = 0;
            if (highPriorityQueue.Count == 0)
            {
                normalRan = ExecuteQueue(
                    normalPriorityQueue,
                    normalPriorityTracker,
                    ref frameStart
                );
            }
            else
            {
                escalationCount++;
            }
            int lowRan = ExecuteQueue(lowPriorityQueue, lowPriorityTracker, ref frameStart);
            bool ranForeground = highRan > 0 || normalRan > 0 || lowRan > 0;
            bool queuesEmpty =
                highPriorityQueue.Count == 0
                && normalPriorityQueue.Count == 0
                && lowPriorityQueue.Count == 0;
            if (
                !ranForeground
                && queuesEmpty
                && (Time.realtimeSinceStartup * 1000f) - frameStart < frameBudgetMs * 0.5f
            )
            {
                idleExecutedCount += ExecuteQueue(
                    idlePriorityQueue,
                    idlePriorityTracker,
                    ref frameStart,
                    allowOverBudget: false
                );
            }
            FlushBatchedEffects();
            renderedFrameCount++;
        }

        private int ExecuteQueue(
            Queue<Action> queue,
            HashSet<Action> tracker,
            ref float frameStartTimestampMs,
            bool allowOverBudget = true
        )
        {
            if (queue == null || queue.Count == 0)
            {
                return 0;
            }
            float budgetLimit = allowOverBudget ? frameBudgetMs : frameBudgetMs * 0.5f;
            if (budgetLimit < 0f)
            {
                budgetLimit = 0f;
            }
            int executedCount = 0;
            while (queue.Count > 0)
            {
                var nowMs = Time.realtimeSinceStartup * 1000f;
                if (budgetLimit > 0f && nowMs - frameStartTimestampMs > budgetLimit)
                {
                    
                    break;
                }
                Action action = queue.Dequeue();
                tracker?.Remove(action);
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

        public (
            int frames,
            int actions,
            int escalations,
            int lowCancelled,
            int idleRan
        ) GetMetrics() =>
            (
                renderedFrameCount,
                executedActionCount,
                escalationCount,
                lowPriorityCancelledCount,
                idleExecutedCount
            );
    }
}
