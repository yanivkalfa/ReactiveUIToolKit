using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ReactiveUITK.Core;
using UnityEngine;

namespace ReactiveUITK.Core.Fiber
{
    /// <summary>
    /// Built-in function components used by the Fiber reconciler
    /// for intrinsic nodes like Suspense.
    /// </summary>
    internal static class FiberIntrinsicComponents
    {
        private const string SuspenseNodePropKey = "suspenseNode";

        /// <summary>
        /// Fiber implementation of Suspense.
        /// It chooses between the primary children and the fallback
        /// based on the VirtualNode.SuspenseReady delegate / task
        /// and can reschedule rendering when an async task completes.
        /// </summary>
        public static VirtualNode SuspenseRender(
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children
        )
        {
            if (
                props == null
                || !props.TryGetValue(SuspenseNodePropKey, out var raw)
                || raw is not VirtualNode suspenseNode
            )
            {
                // Fallback: behave like a fragment over the children.
                return WrapChildrenAsFragment(children);
            }

            // Suspense is modeled as a Fiber function component, so we can
            // access the current FunctionComponentState for async book-keeping.
            var state = HookContext.Current;

            bool ready = true;
            bool readyEvaluatorProvided = false;
            try
            {
                if (suspenseNode.SuspenseReady != null)
                {
                    readyEvaluatorProvided = true;
                    ready = suspenseNode.SuspenseReady();
                }
            }
            catch (Exception ex)
            {
                ready = false;
                try
                {
                    Debug.LogWarning($"ReactiveUITK Fiber: Suspense ready function threw: {ex}");
                }
                catch { }
            }

            Task suspenderTask = suspenseNode.SuspenseReadyTask;
            if (suspenderTask == null && state?.SuspensePendingTask != null)
            {
                suspenderTask = state.SuspensePendingTask;
            }

            // If there is a pending task stored on the component state, treat
            // the boundary as not ready until it completes (even if no
            // explicit SuspenseReady delegate was provided).
            if (state?.SuspensePendingTask != null && !state.SuspensePendingTask.IsCompleted)
            {
                suspenderTask ??= state.SuspensePendingTask;
                ready = false;
            }

            if (!ready && suspenderTask == null && !readyEvaluatorProvided)
            {
                // No way to know readiness; follow legacy behavior:
                // treat as "ready" to avoid permanent fallback.
                ready = true;
            }

            if (suspenderTask != null)
            {
                if (suspenderTask.IsCompleted)
                {
                    ready = EvaluateSuspenseTaskResult(suspenderTask);
                    if (state != null)
                    {
                        state.SuspensePendingTask = null;
                    }
                }
                else if (!ready && state != null)
                {
                    RegisterPendingSuspenseTask(state, suspenderTask);
                }
            }

            bool renderFallback = !ready;

            if (renderFallback)
            {
                if (suspenseNode.Fallback != null)
                {
                    return suspenseNode.Fallback;
                }

                // No explicit fallback - render nothing.
                return null;
            }

            // Ready -> render primary children.
            return WrapChildrenAsFragment(suspenseNode.Children);
        }

        private static bool EvaluateSuspenseTaskResult(Task suspenderTask)
        {
            if (suspenderTask == null)
            {
                return true;
            }

            if (!suspenderTask.IsCompleted)
            {
                return false;
            }

            if (suspenderTask.IsFaulted)
            {
                try
                {
                    Debug.LogWarning(
                        $"ReactiveUITK Fiber: Suspense task faulted: {suspenderTask.Exception}"
                    );
                }
                catch { }
                return true;
            }

            if (suspenderTask.IsCanceled)
            {
                return false;
            }

            if (suspenderTask is Task<bool> boolTask)
            {
                try
                {
                    return boolTask.Result;
                }
                catch
                {
                    return true;
                }
            }

            return true;
        }

        private static void RegisterPendingSuspenseTask(
            FunctionComponentState state,
            Task suspenderTask
        )
        {
            if (state == null || suspenderTask == null)
            {
                return;
            }

            if (ReferenceEquals(state.SuspensePendingTask, suspenderTask))
            {
                return;
            }

            state.SuspenseTaskLock ??= new object();
            state.SuspensePendingTask = suspenderTask;
            int version;
            lock (state.SuspenseTaskLock)
            {
                version = ++state.SuspenseTaskVersion;
            }

            SynchronizationContext syncContext = SynchronizationContext.Current;
            IScheduler scheduler = ResolveScheduler(state);

            suspenderTask.ContinueWith(
                _ =>
                {
                    void Publish()
                    {
                        bool shouldPublish;
                        lock (state.SuspenseTaskLock)
                        {
                            shouldPublish = state.SuspenseTaskVersion == version;
                            if (shouldPublish)
                            {
                                state.SuspensePendingTask = null;
                            }
                        }

                        if (!shouldPublish)
                        {
                            return;
                        }

                        var onStateUpdated = state.OnStateUpdated;
                        if (onStateUpdated == null)
                        {
                            return;
                        }

                        onStateUpdated();
                    }

                    if (scheduler != null)
                    {
                        try
                        {
                            scheduler.Enqueue(Publish, IScheduler.Priority.Normal);
                            return;
                        }
                        catch { }
                    }

                    if (syncContext != null)
                    {
                        try
                        {
                            syncContext.Post(static s => ((Action)s)(), (Action)Publish);
                            return;
                        }
                        catch { }
                    }

                    Publish();
                },
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default
            );
        }

        private static IScheduler ResolveScheduler(FunctionComponentState state)
        {
            var hostContext = state?.HostContext;
            if (hostContext?.Environment == null)
            {
                return null;
            }

            if (
                hostContext.Environment.TryGetValue("scheduler", out var obj)
                && obj is IScheduler scheduler
            )
            {
                return scheduler;
            }

            return null;
        }

        private static VirtualNode WrapChildrenAsFragment(IReadOnlyList<VirtualNode> children)
        {
            if (children == null || children.Count == 0)
            {
                return null;
            }

            if (children.Count == 1)
            {
                return children[0];
            }

            var buffer = new VirtualNode[children.Count];
            for (int i = 0; i < children.Count; i++)
            {
                buffer[i] = children[i];
            }

            return V.Fragment(null, buffer);
        }

        internal static Dictionary<string, object> CreateSuspenseProps(VirtualNode suspenseNode)
        {
            var dict = new Dictionary<string, object>(1);
            dict[SuspenseNodePropKey] = suspenseNode;
            return dict;
        }
    }
}
