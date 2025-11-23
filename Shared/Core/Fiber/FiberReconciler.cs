using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine.Profiling;
using ReactiveUITK.Core;

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
        
        // Stats
        private static readonly CustomSampler RenderPhaseSampler = CustomSampler.Create("Fiber.RenderPhase");
        private static readonly CustomSampler CommitPhaseSampler = CustomSampler.Create("Fiber.CommitPhase");

        public FiberReconciler(HostContext hostContext)
        {
            _hostContext = hostContext;
            _hostConfig = new FiberHostConfig(hostContext.ElementRegistry);
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
                ElementType = "root"
            };

            var root = new FiberRoot
            {
                ContainerElement = container,
                Current = rootFiber,
                Context = _hostContext,
                Reconciler = this,
                RootVNode = vnode
            };

            _root = root;

            // Schedule initial render
            ScheduleUpdateOnFiber(rootFiber, vnode);
            
            return root;
        }

        /// <summary>
        /// Schedule an update on a fiber (triggered by setState, props change, etc.)
        /// </summary>
        public void ScheduleUpdateOnFiber(FiberNode fiber, VirtualNode vnode)
        {
            if (_root == null) return;

            // If a new root vnode is provided (top-level render), record it;
            // otherwise reuse the last one (for state updates).
            if (vnode != null)
            {
                _root.RootVNode = vnode;
            }
            var rootVNode = _root.RootVNode;

            // Create work-in-progress root
            _workInProgressRoot = CreateWorkInProgress(_root.Current, rootVNode);
            _root.WorkInProgress = _workInProgressRoot;
            _nextUnitOfWork = _workInProgressRoot;
            
            // Start work loop
            WorkLoop();
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
        /// Perform a unit of work - process one fiber
        /// Returns next unit of work
        /// </summary>
        private FiberNode PerformUnitOfWork(FiberNode unitOfWork)
        {
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
            if (FiberConfig.EnableFiberLogging)
            {
                UnityEngine.Debug.Log($"[Fiber] BeginWork: {fiber.ElementType} ({fiber.Tag})");
            }

            switch (fiber.Tag)
            {
                case FiberTag.HostComponent:
                    return UpdateHostComponent(fiber);
                
                case FiberTag.FunctionComponent:
                    return UpdateFunctionComponent(fiber);
                
                case FiberTag.Fragment:
                    return UpdateFragment(fiber);
                
                default:
                    return null;
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
                    if (FiberConfig.EnableFiberLogging)
                    {
                        UnityEngine.Debug.Log($"[Fiber] {completedWork.ElementType} has sibling: {siblingFiber.ElementType} ({siblingFiber.Tag})");
                    }
                    // Work on sibling next
                    _nextUnitOfWork = siblingFiber;
                    return;
                }

                if (FiberConfig.EnableFiberLogging)
                {
                    UnityEngine.Debug.Log($"[Fiber] {completedWork.ElementType} has NO sibling, moving to parent: {completedWork.Parent?.ElementType}");
                }

                // No more siblings, go back to parent
                completedWork = completedWork.Parent;
            }

            // Reached root, we're done
            _nextUnitOfWork = null;
        }

        /// <summary>
        /// Complete work on a fiber - finalize any side effects
        /// </summary>
        private void CompleteWork(FiberNode fiber)
        {
            if (FiberConfig.EnableFiberLogging)
            {
                UnityEngine.Debug.Log($"[Fiber] CompleteWork: {fiber.ElementType}");
            }

            switch (fiber.Tag)
            {
                case FiberTag.HostComponent:
                    if (fiber.HostElement == null && fiber.ElementType != "root")
                    {
                        // Create the element using HostConfig
                        fiber.HostElement = _hostConfig.CreateElement(fiber.ElementType);
                        if (FiberConfig.EnableFiberLogging)
                        {
                            UnityEngine.Debug.Log($"[Fiber] Created host element for {fiber.ElementType}: {fiber.HostElement}");
                        }
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
                    Alternate = current
                };
                current.Alternate = workInProgress;
            }

            // Update props for new render
            workInProgress.PendingProps = ExtractProps(vnode);
            // The passed vnode IS the child of the root, so we wrap it in a list
            workInProgress.Children = vnode != null ? new[] { vnode } : Array.Empty<VirtualNode>();
            workInProgress.Child = null; // Will be reconciled
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
                _root.WorkInProgress = null;
                _root.FirstEffect = null;
                _root.LastEffect = null;
            }
            finally
            {
                CommitPhaseSampler.End();
            }
        }

        /// <summary>
        /// Recursively commit deletions from a fiber
        /// </summary>
        private void CommitDeletions(FiberNode fiber)
        {
            if (fiber == null) return;

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
        }

        /// <summary>
        /// Commit placement - insert element into DOM
        /// </summary>
        private void CommitPlacement(FiberNode fiber)
        {
            if (fiber.HostElement == null) return;

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
                        UnityEngine.Debug.Log($"[Fiber] Appending {fiber.ElementType} to {parentFiber.ElementType}");
                    }
                    
                    // Apply initial properties before appending
                    if (fiber.PendingProps != null)
                    {
                        if (FiberConfig.EnableFiberLogging)
                        {
                            var propsStr = string.Join(", ", fiber.PendingProps.Keys);
                            UnityEngine.Debug.Log($"[Fiber] Applying props to {fiber.ElementType}: [{propsStr}]");
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
                            UnityEngine.Debug.LogWarning($"[Fiber] NO props for {fiber.ElementType}");
                        }
                    }
                    
                    _hostConfig.AppendChild(parentFiber.HostElement, fiber.HostElement);
                }
            }
            else
            {
                if (FiberConfig.EnableFiberLogging)
                {
                    UnityEngine.Debug.LogWarning($"[Fiber] Could not find host parent for {fiber.ElementType}");
                }
            }
        }

        /// <summary>
        /// Commit update - update element properties
        /// </summary>
        private void CommitUpdate(FiberNode fiber)
        {
            if (fiber.HostElement == null) return;

            if (fiber.ElementType == "Label")
            {
                string oldText = null;
                string newText = null;

                if (fiber.Props != null && fiber.Props.TryGetValue("text", out var ov) && ov is string os)
                {
                    oldText = os;
                }
                if (fiber.PendingProps != null && fiber.PendingProps.TryGetValue("text", out var nv) && nv is string ns)
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
                fiber.PendingProps);
            fiber.Props = fiber.PendingProps;
        }

        /// <summary>
        /// Commit deletion - remove element from DOM
        /// </summary>
        private void CommitDeletion(FiberNode fiber)
        {
            if (fiber.HostElement != null)
            {
                var parent = _hostConfig.GetParent(fiber.HostElement);
                if (parent != null)
                {
                    _hostConfig.RemoveChild(parent, fiber.HostElement);
                }
            }
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

        // ===== Host component updates =====

        private FiberNode UpdateHostComponent(FiberNode fiber)
        {
            // For now, just reconcile children
            // In full implementation, this would handle element creation too
            if (fiber.Children != null && fiber.Children.Count > 0)
            {
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

        // ===== Child reconciliation (The heart of the reconciler) =====

        private void ReconcileChildren(FiberNode wipFiber, IReadOnlyList<VirtualNode> vnodes)
        {
            if (FiberConfig.EnableFiberLogging)
            {
                UnityEngine.Debug.Log($"[Fiber] ReconcileChildren for {wipFiber.ElementType}: {vnodes?.Count ?? 0} children");
            }

            // Get current children from alternate (if exists)
            var currentFirstChild = wipFiber.Alternate?.Child;
            
            // Use the full reconciliation algorithm
            FiberChildReconciliation.ReconcileChildren(wipFiber, currentFirstChild, vnodes);

            if (FiberConfig.EnableFiberLogging)
            {
                int childCount = 0;
                var child = wipFiber.Child;
                while (child != null)
                {
                    childCount++;
                    child = child.Sibling;
                }
                UnityEngine.Debug.Log($"[Fiber] After reconciliation: {wipFiber.ElementType} has {childCount} child fibers");
            }
        }

        private FiberNode CreateFiberFromVNode(VirtualNode vnode)
        {
            if (vnode == null) return null;

            var fiber = new FiberNode
            {
                Key = vnode.Key,
                PendingProps = ExtractProps(vnode),
                Children = vnode.Children
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
            return vnode?.Properties ?? new Dictionary<string, object>();
        }
    }
}
