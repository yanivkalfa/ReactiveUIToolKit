using System;
using System.Threading.Tasks;

namespace ReactiveUITK.Core.Fiber
{
    /// <summary>
    /// Control-flow exception used by Fiber Suspense to signal that
    /// rendering should be suspended until the associated task completes.
    /// This is analogous to React's "throw a Promise to suspend".
    /// </summary>
    internal sealed class FiberSuspenseSuspendException : Exception
    {
        public Task SuspenderTask { get; }

        public FiberSuspenseSuspendException(Task suspenderTask)
            : base("ReactiveUITK Fiber suspense control flow")
        {
            SuspenderTask = suspenderTask;
        }
    }
}

