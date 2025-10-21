using System;

namespace ReactiveUITK.Core
{
    public interface IScheduler
    {
        public enum Priority { High = 0, Normal = 1, Low = 2, Idle = 3 }
        void Enqueue(Action action, Priority priority = Priority.Normal);
        void EnqueueBatchedEffect(Action effect);
        void BeginBatch();
        void EndBatch();
    }
}

