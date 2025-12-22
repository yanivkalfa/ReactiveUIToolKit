using System;
using System.Collections.Generic;
using System.Diagnostics;
using ReactiveUITK.Core;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UIElements;

namespace ReactiveUITK.Core.Fiber
{
    /// <summary>
    /// React Fiber-style reconciler for ReactiveUITK
    /// Handles the work loop, fiber tree building, and commit phase
    /// </summary>
    public class FiberReconciler
    {
        private FiberRoot _root;
        private FiberNode _workInProgressRoot;
        private FiberNode _nextUnitOfWork;
        private HostContext _hostContext;
        private FiberHostConfig _hostConfig;
        private IScheduler _scheduler;
        private bool _isCommitting; // Track if we're in the commit phase
        // Queue for deferred updates during commit
        // Stores target fiber and the vnode (if any)
        private readonly Queue<(FiberNode Fiber, VirtualNode VNode)> _deferredUpdates = new Queue<(FiberNode, VirtualNode)>();
        private const float TimeSliceMs = 2.0f;

        // Metrics
        private int _workUnitCount;
        private int _sliceCount;
        private int _yieldCount;
        private int _commitCount;
        private int _effectsCommitted;
        private readonly Stopwatch _renderStopwatch = new Stopwatch();

        // Stats
        private static readonly CustomSampler RenderPhaseSampler = CustomSampler.Create(
            "Fiber.RenderPhase"
        );
        private static readonly CustomSampler CommitPhaseSampler = CustomSampler.Create(
            "Fiber.CommitPhase"
        );

        public readonly struct FiberReconcilerMetrics
        {
            public readonly long LastRenderMs;
            public readonly int WorkUnits;
            public readonly int Commits;
            public readonly int Slices;
            public readonly int Yields;
            public readonly int EffectsCommitted;

            public FiberReconcilerMetrics(
                long lastRenderMs,
                int workUnits,
                int commits,
                int slices,
                int yields,
                int effectsCommitted
            )
            {
                LastRenderMs = lastRenderMs;
                WorkUnits = workUnits;
                Commits = commits;
                Slices = slices;
                Yields = yields;
                EffectsCommitted = effectsCommitted;
            }
        }

        public static event Action<FiberReconcilerMetrics> MetricsEmitted;

        public FiberReconciler(HostContext hostContext)
        {
            _hostContext = hostContext;
            _hostConfig = new FiberHostConfig(hostContext.ElementRegistry);

            if (
                hostContext?.Environment != null
                && hostContext.Environment.TryGetValue("scheduler", out var schedObj)
                && schedObj is IScheduler scheduler
            )
            {
                _scheduler = scheduler;
            }
        }

        /// <summary>
        /// Create a fiber root and mount a virtual node tree
        /// </summary>
        public FiberRoot CreateRoot(VisualElement container, VirtualNode vnode)
        {
            // Create root fiber
            var rootFiber = new FiberNode
            {
                Tag = FiberTag.HostComponent,
                HostElement = container,
                ElementType = "root",
            };

            var root = new FiberRoot
            {
                ContainerElement = container,
                Current = rootFiber,
                Context = _hostContext,
                Reconciler = this,
                RootVNode = vnode,
            };

            _root = root;

            // Schedule initial render
            ScheduleUpdateOnFiber(rootFiber, vnode);

            return root;
        }

        /// <summary>
        /// Schedule an update on a fiber (triggered by setState, props change, etc.)
        /// </summary>
        public void ScheduleUpdateOnFiber(FiberNode fiber, VirtualNode vnode, bool scheduleWork = true)
        {
            if (_root == null)
            {
                return;
            }

            // Reset metrics for this render
            _workUnitCount = 0;
            _sliceCount = 0;
            _yieldCount = 0;
            _effectsCommitted = 0;
            _renderStopwatch.Restart();

            // If a new root vnode is provided (top-level render), record it;
            // otherwise reuse the last one (for state updates).
            if (vnode != null)
            {
                _root.RootVNode = vnode;
            }
            // NOTE: rootVNode can be null for state-only updates (like setState calls)
            // Don't early return here - we still need to process the update

            // Find the root fiber for this update by walking up the
            // parent chain. Also check for deletion flags along the way.
            FiberNode rootCurrent = fiber;
            bool isDeleted = false;
            
            var fiberName = fiber?.ElementType ?? fiber?.Render?.Method.DeclaringType?.Name ?? "Unknown";
            UnityEngine.Debug.Log($"[Full Tree Rerender][{fiberName}][ScheduleUpdateOnFiber] Called. VNode={(vnode != null ? vnode.NodeType.ToString() : "null")}");
            
            // Mark the target fiber as having an update
            if (fiber != null)
            {
                UnityEngine.Debug.Log($"[Full Tree Rerender][{fiberName}][ScheduleUpdateOnFiber] Marking HasPendingStateUpdate=true");
                fiber.HasPendingStateUpdate = true;
            }

            while (rootCurrent != null)
            {
                if ((rootCurrent.EffectTag & EffectFlags.Deletion) != 0)
                {
                    isDeleted = true;
                    break;
                }
                
                // Mark parent as having a subtree update
                if (rootCurrent.Parent != null)
                {
                    var parentName = rootCurrent.Parent.ElementType ?? rootCurrent.Parent.Render?.Method.DeclaringType?.Name ?? "Unknown";
                    UnityEngine.Debug.Log($"[Full Tree Rerender][{parentName}][ScheduleUpdateOnFiber] Marking SubtreeHasUpdates=true");
                    rootCurrent.Parent.SubtreeHasUpdates = true;
                }

                if (rootCurrent.Parent == null)
                {
                    break;
                }
                rootCurrent = rootCurrent.Parent;
            }

            if (isDeleted)
            {
                UnityEngine.Debug.LogWarning("[FiberReconciler] Attempted update on deleted fiber. Ignoring.");
                return;
            }

            // If we walked up and found a root, check if it matches the active root.
            if (rootCurrent != null)
            {
                if (rootCurrent == _root.Current)
                {
                    // Found the active root. Good.
                }
                else if (rootCurrent == _root.WorkInProgress)
                {
                    // Found the WorkInProgress root.
                    // This means we are scheduling an update on a tree that is currently being built (cascading update).
                    // We should continue using this root as the WIP.
                }
                else if (rootCurrent == _root.Current.Alternate)
                {
                    // Found the alternate root (which is not currently set as WIP).
                    // This is valid during a commit phase or if we are interacting with a tree that is being committed.
                    // We allow it to proceed, as it will create a WIP from this root.
                }
                else
                {
                    // This fiber is detached from the current tree (and its alternate)
                    UnityEngine.Debug.LogWarning($"[FiberReconciler] Attempted update on detached fiber. Ignoring. rootCurrent={rootCurrent.GetHashCode()} _root.Current={_root.Current.GetHashCode()}");
                    return;
                }
            }

            if (rootCurrent == null)
            {
                rootCurrent = _root.Current;
            }
            if (rootCurrent == null)
            {
                // No valid root to update; safely bail out.
                return;
            }

            // If we're in the commit phase, defer this update until commit completes.
            // Resetting Child=null during commit would corrupt the tree being committed.
            if (_isCommitting)
            {
                // Queue the specific fiber update to replay it after commit
                _deferredUpdates.Enqueue((fiber, vnode));
                var fiberName2 = fiber?.ElementType ?? fiber?.Render?.Method.DeclaringType?.Name ?? "Unknown";
                UnityEngine.Debug.Log($"[Full Tree Rerender][{fiberName2}][ScheduleUpdateOnFiber] Deferred during commit, queued in list. Count: {_deferredUpdates.Count}");
                return;
            }

            // Create work-in-progress root
            if (rootCurrent == _root.WorkInProgress)
            {
                // If we are updating the WIP, we don't need to create it.
                // We just need to ensure it has the latest props/vnode if provided.
                _workInProgressRoot = rootCurrent;
                if (vnode != null)
                {
                    _workInProgressRoot.PendingProps = ExtractProps(vnode);
                    _workInProgressRoot.Children = new[] { vnode };
                    _workInProgressRoot.Child = null; // Reset child to force reconciliation
                    _workInProgressRoot.EffectTag = EffectFlags.None;
                    _workInProgressRoot.NextEffect = null;
                    _workInProgressRoot.Deletions = null;
                }
            }
            else
            {
                _workInProgressRoot = CreateWorkInProgress(rootCurrent, vnode);
            }
            _root.WorkInProgress = _workInProgressRoot;
            _nextUnitOfWork = _workInProgressRoot;
            UnityEngine.Debug.Log($"[Full Tree Rerender][ScheduleUpdateOnFiber] _nextUnitOfWork set to: {(_nextUnitOfWork != null ? _nextUnitOfWork.GetHashCode().ToString() : "Null")} (WIP: {(_root.WorkInProgress != null ? "Set" : "Null")})");

            // Start work loop (scheduler-based when available)
            if (scheduleWork)
            {
                if (_scheduler != null)
                {
                    ScheduleRootWork(IScheduler.Priority.Normal);
                }
                else
                {
                    WorkLoop();
                }
            }
        }

        /// <summary>
        /// Main work loop - processes units of work until done
        /// React uses scheduler here for time slicing, we'll keep it simple for now
        /// </summary>
        private void WorkLoop()
        {
            RenderPhaseSampler.Begin();
            try
            {
                while (_nextUnitOfWork != null)
                {
                    _nextUnitOfWork = PerformUnitOfWork(_nextUnitOfWork);
                }
            }
            finally
            {
                RenderPhaseSampler.End();
            }

            // Render phase complete, commit the changes
            if (_workInProgressRoot != null)
            {
                CommitRoot();
            }
        }

        /// <summary>
        /// Schedule a slice of work on the scheduler.
        /// </summary>
        private void ScheduleRootWork(IScheduler.Priority priority)
        {
            if (_scheduler == null)
            {
                WorkLoop();
                return;
            }
            // SAFETY: Remove optimization check. 
            // We observed cases where work was marked "scheduled" but never ran.
            // Allowing multiple slices to enqueue is safer than dropping one.
            /*
            if (_workScheduled)
            {
                UnityEngine.Debug.Log($"[Full Tree Rerender][ScheduleRootWork] Skipped - work already scheduled");
                return;
            }
            */
            UnityEngine.Debug.Log($"[Full Tree Rerender][ScheduleRootWork] Scheduling work slice with priority {priority}");
            void Slice()
            {
                UnityEngine.Debug.Log($"[Full Tree Rerender][Slice] Callback fired. NextUnit: {(_nextUnitOfWork != null ? "Set" : "Null")}");
                ProcessWorkUntilDeadline();

                if (_nextUnitOfWork != null)
                {
                    ScheduleRootWork(priority);
                }
            }

            _scheduler.Enqueue(Slice, priority);
        }

        /// <summary>
        /// Process work units until the time slice budget is exhausted.
        /// </summary>
        private void ProcessWorkUntilDeadline()
        {
            if (_nextUnitOfWork == null)
            {
                return;
            }

            _sliceCount++;

            RenderPhaseSampler.Begin();
            try
            {
                float startMs = Time.realtimeSinceStartup * 1000f;
                bool yielded = false;
                while (_nextUnitOfWork != null)
                {
                    _nextUnitOfWork = PerformUnitOfWork(_nextUnitOfWork);

                    float nowMs = Time.realtimeSinceStartup * 1000f;
                    if (nowMs - startMs >= TimeSliceMs)
                    {
                        yielded = true;
                        break;
                    }
                }

                if (yielded && _nextUnitOfWork != null)
                {
                    _yieldCount++;
                }
            }
            finally
            {
                RenderPhaseSampler.End();
            }

            // Render phase complete, commit the changes
            if (_nextUnitOfWork == null && _workInProgressRoot != null)
            {
                CommitRoot();
            }
        }

        /// <summary>
        /// Perform a unit of work - process one fiber
        /// Returns next unit of work
        /// </summary>
        private FiberNode PerformUnitOfWork(FiberNode unitOfWork)
        {
            _workUnitCount++;

            // BeginWork: reconcile this fiber's children
            var next = BeginWork(unitOfWork);

            if (next != null)
            {
                // Has children, work on first child next
                return next;
            }

            // CompleteWork: finish this fiber (may set _nextUnitOfWork to sibling)
            CompleteUnitOfWork(unitOfWork);

            // Return whatever CompleteUnitOfWork set (could be sibling or null)
            return _nextUnitOfWork;
        }

        /// <summary>
        /// Begin work on a fiber - reconcile children and return next child
        /// </summary>
        private FiberNode BeginWork(FiberNode fiber)
        {
            try
            {
                switch (fiber.Tag)
                {
                    case FiberTag.HostComponent:
                        return UpdateHostComponent(fiber);

                    case FiberTag.FunctionComponent:
                        return UpdateFunctionComponent(fiber);

                    case FiberTag.Fragment:
                        return UpdateFragment(fiber);

                    case FiberTag.HostPortal:
                        return UpdatePortal(fiber);

                    case FiberTag.ErrorBoundary:
                        return UpdateErrorBoundary(fiber);

                    default:
                        return null;
                }
            }
            catch (FiberSuspenseSuspendException)
            {
                // Suspense control-flow: rendering for this branch is
                // intentionally suspended. A re-render has already been
                // scheduled via Hooks.SuspendUntil / Suspense, so we just
                // stop traversing this subtree for the current render.
                return null;
            }
            catch (Exception ex)
            {
                var boundary = FindNearestErrorBoundary(fiber);
                var boundaryVNode = boundary?.LastRenderedVNode;

                if (boundary != null && boundaryVNode != null)
                {
                    if (TryActivateErrorBoundary(boundary, boundaryVNode, ex))
                    {
                        // Re-run work for the boundary using its updated state.
                        return UpdateErrorBoundary(boundary);
                    }
                }

                // No boundary handled the error - rethrow so it surfaces clearly.
                throw;
            }
        }

        /// <summary>
        /// Complete a unit of work - bubbles up to parent
        /// </summary>
        private void CompleteUnitOfWork(FiberNode unitOfWork)
        {
            var completedWork = unitOfWork;

            while (completedWork != null)
            {
                CompleteWork(completedWork);

                var siblingFiber = completedWork.Sibling;
                if (siblingFiber != null)
                {
                    // Work on sibling next
                    _nextUnitOfWork = siblingFiber;
                    return;
                }

                // No more siblings, go back to parent
                completedWork = completedWork.Parent;
            }

            // Reached root, we're done
            _nextUnitOfWork = null;
            // Reached root, we're done
            _nextUnitOfWork = null;
        }

        /// <summary>
        /// Complete work on a fiber - finalize any side effects
        /// </summary>
        private void CompleteWork(FiberNode fiber)
        {
            switch (fiber.Tag)
            {
                case FiberTag.HostComponent:
                    if (fiber.HostElement == null && fiber.ElementType != "root")
                    {
                        // Create the element using HostConfig
                        fiber.HostElement = _hostConfig.CreateElement(fiber.ElementType);
                        fiber.EffectTag |= EffectFlags.Placement;
                    }
                    else if (fiber.PendingProps != fiber.Props)
                    {
                        // Props changed, mark for update
                        fiber.EffectTag |= EffectFlags.Update;
                    }
                    break;
            }

            // Collect effects
            if (fiber.EffectTag != EffectFlags.None)
            {
                AppendToEffectList(fiber);
            }
        }

        /// <summary>
        /// Create work-in-progress fiber from current
        /// </summary>
        private FiberNode CreateWorkInProgress(FiberNode current, VirtualNode vnode)
        {
            var workInProgress = current.Alternate;

            if (workInProgress == null)
            {
                // Create new WIP fiber
                workInProgress = new FiberNode
                {
                    Tag = current.Tag,
                    ElementType = current.ElementType,
                    HostElement = current.HostElement,
                    Alternate = current,
                };
                current.Alternate = workInProgress;
            }
            else
            {
                // When reusing the alternate, ensure Alternate points back to current.
                // This is critical: after a commit, Current becomes the finished tree,
                // and its Alternate (which we're reusing) must point to Current so
                // reconciliation can find the children via wipFiber.Alternate.Child.
                workInProgress.Alternate = current;
            }

            // Propagate update flags to WIP - root is special case, doesn't use factory
            var componentName = current.ElementType ?? current.Render?.Method.DeclaringType?.Name ?? "root";
            UnityEngine.Debug.Log($"[Full Tree Rerender][{componentName}][CreateWIP] Propagating flags - HasPending:{current.HasPendingStateUpdate}, Subtree:{current.SubtreeHasUpdates}, ReadsContext:{current.ReadsContext}");
            
            workInProgress.HasPendingStateUpdate = current.HasPendingStateUpdate;
            workInProgress.SubtreeHasUpdates = current.SubtreeHasUpdates;
            workInProgress.ReadsContext = current.ReadsContext;

            // CRITICAL FIX: Copy existing props to WIP so we have a baseline for ArePropsEqual comparison
            // Without this, WIP.Props is null, so comparison against PendingProps always fails
            workInProgress.Props = current.Props;

            // Update props for new render
            workInProgress.PendingProps = ExtractProps(vnode);
            // The passed vnode IS the child of the root, so we wrap it in a list
            // Fix: If vnode is null (state update), preserve existing children to avoid wiping the tree
            workInProgress.Children = vnode != null ? new[] { vnode } : current.Children;
            
            // Fix: Preserve Child link so Hooks.ResolveAnimationTarget can find the host element
            // during the render phase (before ReconcileChildren overwrites it).
            workInProgress.Child = current.Child;
            
            workInProgress.EffectTag = EffectFlags.None;
            workInProgress.NextEffect = null;
            workInProgress.Deletions = null;

            return workInProgress;
        }

        /// <summary>
        /// Append fiber to effect list
        /// </summary>
        private void AppendToEffectList(FiberNode fiber)
        {
            if (_root.LastEffect != null)
            {
                _root.LastEffect.NextEffect = fiber;
                _root.LastEffect = fiber;
            }
            else
            {
                _root.FirstEffect = fiber;
                _root.LastEffect = fiber;
            }
        }

        /// <summary>
        /// Commit phase - apply all effects to the DOM (VisualElement tree)
        /// </summary>
        private void CommitRoot()
        {
            _commitCount++;
            _isCommitting = true; // Prevent ScheduleUpdateOnFiber from corrupting WIP

            CommitPhaseSampler.Begin();
            try
            {
                var finishedWork = _root.WorkInProgress;

                // Process deletions first (from root down)
                CommitDeletions(_root.WorkInProgress);

                // Process effect list
                var effect = _root.FirstEffect;
                while (effect != null)
                {
                    CommitWork(effect);
                    effect = effect.NextEffect;
                }

                // Swap current and work-in-progress
                _root.Current = finishedWork;

                // Only clear WIP if it hasn't been updated by a synchronous effect (e.g. navigate)
                if (_root.WorkInProgress == finishedWork)
                {
                    _root.WorkInProgress = null;
                }

                if (_workInProgressRoot == finishedWork)
                {
                    _workInProgressRoot = null;
                }

                _root.FirstEffect = null;
                _root.LastEffect = null;

                // PHASE 3: Commit props and clear remaining flags (SubtreeHasUpdates, ReadsContext)
                // CRITICAL: We clear flags HERE, before processing deferred updates
                // This cleans up the render we just finished.
                // Any deferred updates processed next will set NEW flags on the tree, which we must NOT clear.
                CommitPropsAndClearFlags(_root.Current);

                EmitMetrics();
            }
            finally
            {
                CommitPhaseSampler.End();
                _isCommitting = false;

                // Process any deferred updates scheduled during commit
                bool pendingUpdates = false;
                while (_deferredUpdates.Count > 0)
                {
                    var (fiber, vnode) = _deferredUpdates.Dequeue();
                    var name = fiber?.ElementType ?? fiber?.Render?.Method.DeclaringType?.Name ?? "Unknown";
                    UnityEngine.Debug.Log($"[Full Tree Rerender][{name}][CommitRoot] Processing deferred update from queue (Batching flags)");
                    // Update flags/WIP but DON'T schedule work yet
                    ScheduleUpdateOnFiber(fiber, vnode, scheduleWork: false);
                    pendingUpdates = true;
                }

                // Now that all deferred updates are processed and flags are set/merged,
                // schedule the work loop ONCE.
                // Now that all deferred updates are processed and flags are set/merged,
                // schedule the work loop ONCE.
                if (pendingUpdates)
                {
                    UnityEngine.Debug.Log($"[Full Tree Rerender][CommitRoot] Deferred updates processed. Restarting loop if needed.");

                    if (_scheduler == null)
                    {
                        // Sync mode: We must restart the loop manually because the previous WorkLoop exited
                        WorkLoop();
                    }
                    // Async mode: Do NOTHING. 
                    // The 'Slice' callback that called us will see _nextUnitOfWork != null and reschedule automatically.
                    // Explicitly calling ScheduleRootWork here causes double-scheduling and race conditions.
                }
            }
        }

        /// <summary>
        /// Recursively commit deletions from a fiber
        /// </summary>
        private void CommitDeletions(FiberNode fiber)
        {
            if (fiber == null)
                return;

            // Process deletions on this fiber
            if (fiber.Deletions != null)
            {
                foreach (var deletion in fiber.Deletions)
                {
                    CommitDeletion(deletion);
                }
                fiber.Deletions = null;
            }

            // Recurse to children
            var child = fiber.Child;
            while (child != null)
            {
                CommitDeletions(child);
                child = child.Sibling;
            }
        }

        /// <summary>
        /// Commit a single effect
        /// </summary>
        private void CommitWork(FiberNode fiber)
        {
            if ((fiber.EffectTag & EffectFlags.Placement) != 0)
            {
                CommitPlacement(fiber);
            }

            if ((fiber.EffectTag & EffectFlags.Update) != 0)
            {
                CommitUpdate(fiber);
            }

            if ((fiber.EffectTag & EffectFlags.Deletion) != 0)
            {
                CommitDeletion(fiber);
            }

            // Layout effects
            if ((fiber.EffectTag & EffectFlags.LayoutEffect) != 0)
            {
                CommitLayoutEffects(fiber);
            }

            // Passive effects
            if ((fiber.EffectTag & EffectFlags.PassiveEffect) != 0)
            {
                SchedulePassiveEffects(fiber);
            }

            if ((fiber.EffectTag & (EffectFlags.LayoutEffect | EffectFlags.PassiveEffect)) != 0)
            {
                _effectsCommitted++;
            }
        }

        /// <summary>
        /// Commit placement - insert element into DOM
        /// </summary>
        private void CommitPlacement(FiberNode fiber)
        {
            // Portals use an existing VisualElement as their host and
            // should not be inserted into the normal parent hierarchy.
            if (fiber.Tag == FiberTag.HostPortal)
            {
                return;
            }

            if (fiber.HostElement == null)
                return;

            // Find parent host fiber
            var parentFiber = fiber.Parent;
            while (parentFiber != null && parentFiber.HostElement == null)
            {
                parentFiber = parentFiber.Parent;
            }

            if (parentFiber?.HostElement != null)
            {
                if (_hostConfig.GetParent(fiber.HostElement) == null)
                {
                    if (FiberConfig.EnableFiberLogging)
                    {
                        UnityEngine.Debug.Log(
                            $"[Fiber] Appending {fiber.ElementType} to {parentFiber.ElementType}"
                        );
                    }

                    // Apply initial properties before appending
                    if (fiber.PendingProps != null)
                    {
                        if (FiberConfig.EnableFiberLogging)
                        {
                            var propsStr = string.Join(", ", fiber.PendingProps.Keys);
                            UnityEngine.Debug.Log(
                                $"[Fiber] Applying props to {fiber.ElementType}: [{propsStr}]"
                            );
                        }

                        _hostConfig.ApplyProperties(
                            fiber.HostElement,
                            fiber.ElementType,
                            null, // oldProps
                            fiber.PendingProps
                        );
                        fiber.Props = fiber.PendingProps;
                    }
                    else
                    {
                        if (FiberConfig.EnableFiberLogging)
                        {
                            UnityEngine.Debug.LogWarning(
                                $"[Fiber] NO props for {fiber.ElementType}"
                            );
                        }
                    }

                    _hostConfig.AppendChild(parentFiber.HostElement, fiber.HostElement);
                }
            }
            else
            {
                if (FiberConfig.EnableFiberLogging)
                {
                    UnityEngine.Debug.LogWarning(
                        $"[Fiber] Could not find host parent for {fiber.ElementType}"
                    );
                }
            }
        }

        /// <summary>
        /// Commit update - update element properties
        /// </summary>
        private void CommitUpdate(FiberNode fiber)
        {
            if (fiber.HostElement == null)
                return;

            if (FiberConfig.EnableFiberLogging && fiber.ElementType == "Label")
            {
                string oldText = null;
                string newText = null;

                if (
                    fiber.Props != null
                    && fiber.Props.TryGetValue("text", out var ov)
                    && ov is string os
                )
                {
                    oldText = os;
                }
                if (
                    fiber.PendingProps != null
                    && fiber.PendingProps.TryGetValue("text", out var nv)
                    && nv is string ns
                )
                {
                    newText = ns;
                }

                UnityEngine.Debug.Log(
                    $"[Fiber] CommitUpdate Label oldText='{oldText}' newText='{newText}'"
                );
            }

            // Apply property changes using HostConfig
            _hostConfig.ApplyProperties(
                fiber.HostElement,
                fiber.ElementType,
                fiber.Props,
                fiber.PendingProps
            );
            fiber.Props = fiber.PendingProps;
        }

        /// <summary>
        /// Commit layout effects
        /// </summary>
        private void CommitLayoutEffects(FiberNode fiber)
        {
            if (fiber.Tag == FiberTag.FunctionComponent)
            {
                FiberFunctionComponent.CommitLayoutEffects(fiber);
            }
        }

        /// <summary>
        /// Schedule passive effects
        /// </summary>
        private void SchedulePassiveEffects(FiberNode fiber)
        {
            if (fiber.Tag == FiberTag.FunctionComponent)
            {
                FiberFunctionComponent.SchedulePassiveEffects(fiber);
            }
        }

        /// <summary>
        /// Commit deletion - remove element from DOM
        /// </summary>
        private void CommitDeletion(FiberNode fiber)
        {
            if (fiber == null)
            {
                return;
            }

            // Depth-first delete: clean up subtree before removing the current node.

            // If this is a function component, clean up effects and signal subscriptions.
            if (fiber.Tag == FiberTag.FunctionComponent && fiber.ComponentState != null)
            {
                var state = fiber.ComponentState;

                // Run and clear passive effects (UseEffect)
                if (state.FunctionEffects != null)
                {
                    for (int i = 0; i < state.FunctionEffects.Count; i++)
                    {
                        var effect = state.FunctionEffects[i];
                        try
                        {
                            effect.cleanup?.Invoke();
                        }
                        catch { }
                    }
                    state.FunctionEffects.Clear();
                }

                // Run and clear layout effects (UseLayoutEffect)
                if (state.FunctionLayoutEffects != null)
                {
                    for (int i = 0; i < state.FunctionLayoutEffects.Count; i++)
                    {
                        var effect = state.FunctionLayoutEffects[i];
                        try
                        {
                            effect.cleanup?.Invoke();
                        }
                        catch { }
                    }
                    state.FunctionLayoutEffects.Clear();
                }

                Hooks.DisposeSignalSubscriptions(state);
            }

            // Recurse into children so that all host descendants are removed.
            var child = fiber.Child;
            while (child != null)
            {
                CommitDeletion(child);
                child = child.Sibling;
            }

            // If this fiber has a HostElement, remove it from its parent
            if (fiber.HostElement != null)
            {
                var parentFiber = fiber.Parent;
                while (parentFiber != null && parentFiber.HostElement == null)
                {
                    parentFiber = parentFiber.Parent;
                }

                if (parentFiber?.HostElement != null)
                {
                    if (FiberConfig.EnableFiberLogging)
                    {
                        var name = fiber.ElementType ?? fiber.Render?.Method.DeclaringType?.Name ?? "Unknown";
                        UnityEngine.Debug.Log($"[Full Tree Rerender][{name}][CommitDeletion] Removing host child from parent");
                    }
                    _hostConfig.RemoveChild(parentFiber.HostElement, fiber.HostElement);
                }
            }
        }

        // ===== Host component updates =====

        private FiberNode UpdateHostComponent(FiberNode fiber)
        {
            // Reconcile children for this host element.
            if (fiber.Children != null && fiber.Children.Count > 0)
            {
                if (FiberConfig.EnableFiberLogging)
                {
                    UnityEngine.Debug.Log($"[UpdateHost][{fiber.ElementType}] Reconciling {fiber.Children.Count} children");
                }
                ReconcileChildren(fiber, fiber.Children);
            }

            return fiber.Child;
        }

        private FiberNode UpdateFunctionComponent(FiberNode fiber)
        {
            // Render the function component and get child fiber
            return FiberFunctionComponent.RenderFunctionComponent(fiber, _hostContext, this);
        }

        private FiberNode UpdateFragment(FiberNode fiber)
        {
            return FiberFragment.UpdateFragment(fiber);
        }

        private FiberNode UpdatePortal(FiberNode fiber)
        {
            if (fiber.Children != null && fiber.Children.Count > 0)
            {
                ReconcileChildren(fiber, fiber.Children);
            }

            return fiber.Child;
        }

        private FiberNode UpdateErrorBoundary(FiberNode fiber)
        {
            var boundaryNode = fiber.LastRenderedVNode;
            if (boundaryNode == null)
            {
                return null;
            }

            bool resetRequested = !string.Equals(
                fiber.ErrorBoundaryResetKey,
                boundaryNode.ErrorResetToken,
                StringComparison.Ordinal
            );

            if (resetRequested)
            {
                fiber.ErrorBoundaryActive = false;
                fiber.ErrorBoundaryShowingFallback = false;
                fiber.ErrorBoundaryLastException = null;
                fiber.ErrorBoundaryResetKey = boundaryNode.ErrorResetToken;
            }

            IReadOnlyList<VirtualNode> targetChildren;

            if (fiber.ErrorBoundaryActive && !resetRequested)
            {
                if (boundaryNode.ErrorFallback != null)
                {
                    targetChildren = new[] { boundaryNode.ErrorFallback };
                }
                else
                {
                    targetChildren = Array.Empty<VirtualNode>();
                }

                fiber.ErrorBoundaryShowingFallback = true;
            }
            else
            {
                targetChildren = boundaryNode.Children ?? Array.Empty<VirtualNode>();
                fiber.ErrorBoundaryShowingFallback = false;
            }

            fiber.Children = targetChildren;

            if (targetChildren.Count > 0)
            {
                ReconcileChildren(fiber, targetChildren);
            }
            else
            {
                // Ensure any previous children are deleted.
                ReconcileChildren(fiber, Array.Empty<VirtualNode>());
            }

            return fiber.Child;
        }

        private FiberNode FindNearestErrorBoundary(FiberNode fiber)
        {
            var current = fiber;
            while (current != null)
            {
                if (current.Tag == FiberTag.ErrorBoundary)
                {
                    return current;
                }
                current = current.Parent;
            }

            return null;
        }

        private bool TryActivateErrorBoundary(
            FiberNode boundary,
            VirtualNode boundaryNode,
            Exception exception
        )
        {
            if (boundary == null || boundaryNode == null)
            {
                return false;
            }

            // Avoid recursive activation if fallback itself throws.
            if (boundary.ErrorBoundaryActive)
            {
                return false;
            }

            boundary.ErrorBoundaryActive = true;
            boundary.ErrorBoundaryShowingFallback = true;
            boundary.ErrorBoundaryLastException = exception;
            boundary.ErrorBoundaryResetKey = boundaryNode.ErrorResetToken;

            bool handled = false;

            if (boundaryNode.ErrorFallback != null)
            {
                handled = true;
            }

            if (boundaryNode.ErrorHandler != null)
            {
                try
                {
                    boundaryNode.ErrorHandler(exception);
                    handled = true;
                }
                catch (Exception handlerEx)
                {
                    try
                    {
                        UnityEngine.Debug.LogError(
                            $"ReactiveUITK Fiber: Error boundary handler threw: {handlerEx}"
                        );
                    }
                    catch { }
                }
            }

            if (!handled)
            {
                // Reset flags so the boundary doesn't stay in an inconsistent state.
                boundary.ErrorBoundaryActive = false;
                boundary.ErrorBoundaryShowingFallback = false;
                boundary.ErrorBoundaryLastException = null;
                return false;
            }

            try
            {
                if (exception != null)
                {
                    UnityEngine.Debug.LogError(
                        $"ReactiveUITK Fiber: Error boundary captured exception: {exception}"
                    );
                }
            }
            catch { }

            return true;
        }

        // ===== Child reconciliation (The heart of the reconciler) =====

        private void ReconcileChildren(FiberNode wipFiber, IReadOnlyList<VirtualNode> vnodes)
        {
            // Get current children from alternate (if exists)
            var currentFirstChild = wipFiber.Alternate?.Child;

            // Use the full reconciliation algorithm
            FiberChildReconciliation.ReconcileChildren(wipFiber, currentFirstChild, vnodes);

            // Optional debug metrics (child counts, etc.) can be added here
            // behind FiberConfig.EnableFiberLogging if needed.
        }

        private FiberNode CreateFiberFromVNode(VirtualNode vnode)
        {
            if (vnode == null)
                return null;

            var fiber = new FiberNode
            {
                Key = vnode.Key,
                PendingProps = ExtractProps(vnode),
                Children = vnode.Children,
            };

            switch (vnode.NodeType)
            {
                case VirtualNodeType.Element:
                    fiber.Tag = FiberTag.HostComponent;
                    fiber.ElementType = vnode.ElementTypeName;
                    break;

                case VirtualNodeType.FunctionComponent:
                    fiber.Tag = FiberTag.FunctionComponent;
                    fiber.Render = vnode.FunctionRender;
                    break;

                case VirtualNodeType.Fragment:
                    fiber.Tag = FiberTag.Fragment;
                    break;
            }

            return fiber;
        }

        // ===== Helper methods =====

        private IReadOnlyDictionary<string, object> ExtractProps(VirtualNode vnode)
        {
            if (vnode == null)
            {
                return new Dictionary<string, object>();
            }

            switch (vnode.NodeType)
            {
                case VirtualNodeType.Suspense:
                    return FiberIntrinsicComponents.CreateSuspenseProps(vnode);

                case VirtualNodeType.Text:
                    return new Dictionary<string, object>
                    {
                        { "text", vnode.TextContent ?? string.Empty },
                    };

                default:
                    return vnode.Properties ?? new Dictionary<string, object>();
            }
        }

        private void EmitMetrics()
        {
            if (!MetricsEmittedHasSubscribers())
            {
                _renderStopwatch.Reset();
                return;
            }

            _renderStopwatch.Stop();
            long elapsedMs = _renderStopwatch.ElapsedMilliseconds;

            var snapshot = new FiberReconcilerMetrics(
                elapsedMs,
                _workUnitCount,
                _commitCount,
                _sliceCount,
                _yieldCount,
                _effectsCommitted
            );

            try
            {
                MetricsEmitted?.Invoke(snapshot);
            }
            catch
            {
                // Swallow listener exceptions to avoid breaking rendering.
            }
            finally
            {
                _renderStopwatch.Reset();
            }
        }

        private static bool MetricsEmittedHasSubscribers() => MetricsEmitted != null;

        /// <summary>
        /// Phase 3: Commit props and clear flags after commit
        /// </summary>
        private void CommitPropsAndClearFlags(FiberNode fiber)
        {
            if (fiber == null)
                return;

            var name = fiber.ElementType ?? fiber.Render?.Method.DeclaringType?.Name ?? "Unknown";
            UnityEngine.Debug.Log($"[FLAGS][{name}][CommitClear] BEFORE - HasPending:{fiber.HasPendingStateUpdate}, Subtree:{fiber.SubtreeHasUpdates}, ReadsContext:{fiber.ReadsContext}");

            // Commit props for next comparison
            fiber.Props = fiber.PendingProps;

            // Clear remaining flags (HasPendingStateUpdate already cleared in bailout check)
            fiber.SubtreeHasUpdates = false;
            // Note: ReadsContext is permanent, don't clear it

            UnityEngine.Debug.Log($"[FLAGS][{name}][CommitClear] AFTER - Subtree:{fiber.SubtreeHasUpdates}");

            // Recursively process children
            var child = fiber.Child;
            while (child != null)
            {
                CommitPropsAndClearFlags(child);
                child = child.Sibling;
            }
        }
    }
}
