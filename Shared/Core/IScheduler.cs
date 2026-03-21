using System;

namespace ReactiveUITK.Core
{
    public interface IScheduler
    {
        public enum Priority
        {
            High = 0,
            Normal = 1,
            Low = 2,
            Idle = 3,
        }

        void Enqueue(Action action, Priority priority = Priority.Normal);
        void EnqueueBatchedEffect(Action effect);
        void BeginBatch();
        void EndBatch();

        /// <summary>
        /// Synchronously drain any pending scheduled work and batched effects.
        /// Implementations should ignore frame budgets and process all current
        /// queued actions before returning. New work scheduled while draining
        /// may or may not be processed, depending on the implementation.
        /// </summary>
        void PumpNow();
    }
}
