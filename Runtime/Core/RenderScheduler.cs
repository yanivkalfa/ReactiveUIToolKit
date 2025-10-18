using System;
using System.Collections.Generic;
using UnityEngine;

namespace ReactiveUITK
{
    public sealed class RenderScheduler : MonoBehaviour
    {
		public enum RenderPriority { High = 0, Normal = 1, Low = 2, Idle = 3 }

		private readonly Queue<Action> high = new Queue<Action>();
		private readonly Queue<Action> normal = new Queue<Action>();
		private readonly Queue<Action> low = new Queue<Action>();
		private readonly Queue<Action> idle = new Queue<Action>();
        [SerializeField] private float frameBudgetMs = 4.0f; // time slice budget
        private readonly List<Action> batchedEffects = new List<Action>();
		private readonly List<Action> batchedUpdates = new List<Action>();
		private bool batching;
        private int framesRendered;
        private int actionsExecuted;
        private float lastFrameStartMs;
		private int escalations;
		private int lowCancelled;
		private int idleExecuted;

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

		public void Enqueue(Action action, RenderPriority priority = RenderPriority.Normal)
        {
            if (action == null)
            {
                return;
            }
			if (batching && priority != RenderPriority.High)
			{
				batchedUpdates.Add(() => Enqueue(action, priority));
				return;
			}
            switch (priority)
            {
                case RenderPriority.High: high.Enqueue(action); break;
                case RenderPriority.Normal: normal.Enqueue(action); break;
                case RenderPriority.Low: low.Enqueue(action); break;
				case RenderPriority.Idle: idle.Enqueue(action); break;
            }
        }

		public void BeginBatch() { batching = true; }
		public void EndBatch()
		{
			batching = false;
			foreach (var up in batchedUpdates) up();
			batchedUpdates.Clear();
		}

		private void LateUpdate()
        {
            float start = Time.realtimeSinceStartup * 1000f;
            lastFrameStartMs = start;
			// Escalate if user input (heuristic: any high items present) -> cancel remaining low queue to prioritize responsiveness
			if (high.Count > 0 && low.Count > 0)
			{
				lowCancelled += low.Count;
				low.Clear();
			}
			ExecuteQueue(high, ref start);
			if (high.Count == 0) ExecuteQueue(normal, ref start); else escalations++;
			ExecuteQueue(low, ref start);
			// Idle work only if budget remains (strict)
			if ((Time.realtimeSinceStartup * 1000f) - start < frameBudgetMs * 0.5f)
			{
				idleExecuted += ExecuteQueue(idle, ref start, allowOverBudget:false);
			}
            FlushBatchedEffects();
            framesRendered++;
        }

		private int ExecuteQueue(Queue<Action> q, ref float startMs, bool allowOverBudget = true)
        {
			int executed = 0;
            while (q.Count > 0)
            {
				if (!allowOverBudget && (Time.realtimeSinceStartup * 1000f) - startMs > frameBudgetMs)
					break;
				if (allowOverBudget && (Time.realtimeSinceStartup * 1000f) - startMs > frameBudgetMs)
					break;
                var a = q.Dequeue();
                try { a(); } catch (Exception ex) { Debug.LogError("RenderScheduler action exception: " + ex); }
                actionsExecuted++;
				executed++;
            }
			return executed;
        }

        public void EnqueueBatchedEffect(Action effect)
        {
            if (effect != null) batchedEffects.Add(effect);
        }

        private void FlushBatchedEffects()
        {
            if (batchedEffects.Count == 0) return;
            foreach (var e in batchedEffects)
            {
                try { e(); } catch (Exception ex) { Debug.LogError("Effect exception: " + ex); }
            }
            batchedEffects.Clear();
        }

		public (int frames, int actions, int escalations, int lowCancelled, int idleRan) GetMetrics() => (framesRendered, actionsExecuted, escalations, lowCancelled, idleExecuted);
    }
}
