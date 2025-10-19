
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Core.Util;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK
{
    public abstract class ReactiveComponent : MonoBehaviour
    {
        public VisualElement MountedElement { get; private set; }
        public Dictionary<string, object> Props { get; private set; }
        protected HostContext HostContext { get; private set; }

        private VirtualNode previousRenderedTree;
        private bool updateQueuedFlag;
        private Reconciler componentReconciler;
        private Dictionary<string, object> previousPropsSnapshot;
        private readonly List<System.Func<System.Action>> pendingEffectFactories = new();
        private readonly List<System.Action> effectCleanupActions = new();
        private readonly List<(System.Func<System.Action> factory, object[] deps, object[] lastDeps, System.Action cleanup)> dependencyAwareEffects = new();
        private readonly List<System.Action> pendingStateMutations = new();
        private readonly Dictionary<string, object> providedContextValues = new();

        protected abstract VirtualNode Render();

        // Lifecycle (React inspired) - prefer overriding the Component* variants.
        protected virtual void OnWillMount() { }
        protected virtual void OnDidMount() { }
        protected virtual void OnMounted() { }
        protected virtual bool ShouldUpdate(Dictionary<string, object> nextProps)
        {
            if (previousPropsSnapshot == null)
            {
                return true;
            }
            return !ShallowCompare.PropsEqual(previousPropsSnapshot, nextProps);
        }
        protected virtual void OnWillUpdate() { }
        protected virtual void OnDidUpdate() { }
        protected virtual void OnUpdated() { }
        protected virtual void OnWillUnmount() { }

        // React-like naming (aliases). Override these for familiarity; they route to the On* versions.
        protected virtual void ComponentWillMount() { OnWillMount(); }
        protected virtual void ComponentDidMount() { OnDidMount(); }
        protected virtual bool ShouldComponentUpdate(Dictionary<string, object> nextProps) { return ShouldUpdate(nextProps); }
        protected virtual void ComponentWillUpdate(Dictionary<string, object> nextProps) { OnWillUpdate(); }
        protected virtual void ComponentDidUpdate(Dictionary<string, object> prevProps) { OnDidUpdate(); }
        protected virtual void ComponentWillUnmount() { OnWillUnmount(); }

        public void Mount(VisualElement parentElement, HostContext hostContext)
        {
            if (componentReconciler == null)
            {
                componentReconciler = new Reconciler(hostContext);
            }
            HostContext = hostContext;
            if (MountedElement != null)
            {
                return;
            }
            // Create a temporary container; may be hoisted (flattened) if root is a single element.
            MountedElement = new VisualElement { name = GetType().Name + "Container" };
            parentElement.Add(MountedElement);
            OnWillMount();
            ComponentWillMount();
            VirtualNode nextRenderedTree = Render();
            componentReconciler.BuildSubtree(MountedElement, nextRenderedTree);
            previousRenderedTree = nextRenderedTree;

            // Flatten: if root virtual node is a single Element, hoist it (React-style no wrapper).
            if (nextRenderedTree != null && nextRenderedTree.NodeType == Core.VirtualNodeType.Element && MountedElement.childCount == 1)
            {
                var rootChild = MountedElement.ElementAt(0);
                int insertIndex = parentElement.IndexOf(MountedElement);
                // Move child before removing wrapper
                MountedElement.Remove(rootChild);
                parentElement.Insert(insertIndex, rootChild);
                // Remove wrapper container
                MountedElement.RemoveFromHierarchy();
                MountedElement = rootChild; // MountedElement now points to actual root element
            }
            OnDidMount();
            ComponentDidMount();
            OnMounted();
        }

        public void Unmount()
        {
            if (MountedElement == null)
            {
                return;
            }
            OnWillUnmount();
            ComponentWillUnmount();
            foreach (System.Action cleanup in effectCleanupActions)
            {
                try
                {
                    cleanup?.Invoke();
                }
                catch
                {
                }
            }
            effectCleanupActions.Clear();
            MountedElement.RemoveFromHierarchy();
            MountedElement = null;
            previousRenderedTree = null;
            updateQueuedFlag = false;
        }

        protected void SetState(System.Action stateMutation)
        {
            if (stateMutation != null)
            {
                pendingStateMutations.Add(stateMutation);
            }
            if (!updateQueuedFlag)
            {
                updateQueuedFlag = true;
                var scheduler = RenderScheduler.Instance;
                if (scheduler == null)
                {
                    // Fallback: run update immediately if scheduler not yet available.
                    try
                    {
                        if (MountedElement == null)
                        {
                            updateQueuedFlag = false;
                            return;
                        }
                        if (!ShouldUpdate(Props) || !ShouldComponentUpdate(Props))
                        {
                            updateQueuedFlag = false;
                            return;
                        }
                        OnWillUpdate();
                        ComponentWillUpdate(Props);
                        foreach (System.Action mutation in pendingStateMutations)
                        {
                            try { mutation(); } catch { }
                        }
                        pendingStateMutations.Clear();
                        VirtualNode nextRenderedTreeImmediate = Render();
                        componentReconciler.DiffSubtree(MountedElement, previousRenderedTree, nextRenderedTreeImmediate);
                        previousRenderedTree = nextRenderedTreeImmediate;
                        updateQueuedFlag = false;
                        OnDidUpdate();
                        ComponentDidUpdate(previousPropsSnapshot);
                        OnUpdated();
                        FlushEffects();
                    }
                    catch { updateQueuedFlag = false; }
                    return;
                }
                scheduler.Enqueue(() =>
                {
                    if (MountedElement == null)
                    {
                        updateQueuedFlag = false;
                        return;
                    }
                    if (!ShouldUpdate(Props) || !ShouldComponentUpdate(Props))
                    {
                        updateQueuedFlag = false;
                        return;
                    }
                    OnWillUpdate();
                    ComponentWillUpdate(Props);
                    foreach (System.Action mutation in pendingStateMutations)
                    {
                        try { mutation(); } catch { }
                    }
                    pendingStateMutations.Clear();
                    VirtualNode nextRenderedTree = Render();
                    componentReconciler.DiffSubtree(MountedElement, previousRenderedTree, nextRenderedTree);
                    previousRenderedTree = nextRenderedTree;
                    updateQueuedFlag = false;
                    OnDidUpdate();
                    ComponentDidUpdate(previousPropsSnapshot);
                    OnUpdated();
                    FlushEffects();
                });
            }
        }

        public void ForceUpdate()
        {
            if (MountedElement == null)
            {
                return;
            }
            if (updateQueuedFlag)
            {
                return;
            }
            updateQueuedFlag = true;
            var scheduler = RenderScheduler.Instance;
            if (scheduler == null)
            {
                // Immediate fallback
                try
                {
                    if (MountedElement == null)
                    {
                        updateQueuedFlag = false;
                        return;
                    }
                    OnWillUpdate();
                    ComponentWillUpdate(Props);
                    foreach (System.Action mutation in pendingStateMutations)
                    {
                        try { mutation(); } catch { }
                    }
                    pendingStateMutations.Clear();
                    VirtualNode nextRenderedTreeImmediate = Render();
                    componentReconciler.DiffSubtree(MountedElement, previousRenderedTree, nextRenderedTreeImmediate);
                    previousRenderedTree = nextRenderedTreeImmediate;
                    updateQueuedFlag = false;
                    OnDidUpdate();
                    ComponentDidUpdate(previousPropsSnapshot);
                    OnUpdated();
                    FlushEffects();
                }
                catch { updateQueuedFlag = false; }
                return;
            }
            scheduler.Enqueue(() =>
            {
                if (MountedElement == null)
                {
                    updateQueuedFlag = false;
                    return;
                }
                OnWillUpdate();
                ComponentWillUpdate(Props);
                foreach (System.Action mutation in pendingStateMutations)
                {
                    try { mutation(); } catch { }
                }
                pendingStateMutations.Clear();
                VirtualNode nextRenderedTree = Render();
                componentReconciler.DiffSubtree(MountedElement, previousRenderedTree, nextRenderedTree);
                previousRenderedTree = nextRenderedTree;
                updateQueuedFlag = false;
                OnDidUpdate();
                ComponentDidUpdate(previousPropsSnapshot);
                OnUpdated();
                FlushEffects();
            });
        }

        public void SetProps(Dictionary<string, object> nextProps)
        {
            previousPropsSnapshot = Props;
            Props = nextProps;
            if (MountedElement != null)
            {
                SetState(() => { });
            }
        }

        protected void ProvideContext(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }
            providedContextValues[key] = value;
            if (HostContext != null)
            {
                HostContext.Environment[key] = value;
            }
        }

        protected T ConsumeContext<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return default;
            }
            if (providedContextValues.TryGetValue(key, out object existing) && existing is T existingTyped)
            {
                return existingTyped;
            }
            if (HostContext != null)
            {
                HostContext.Subscribe(key, this);
                object resolved = HostContext.ResolveContext(key);
                if (resolved is T resolvedTyped)
                {
                    return resolvedTyped;
                }
            }
            return default;
        }

        // Ref helper returns underlying VisualElement host
        protected VisualElement UseRef() => MountedElement;

        // Effect that returns an optional cleanup action via Func<System.Action>
        protected void UseEffect(System.Func<System.Action> effectFactory, bool immediate = false)
        {
            if (effectFactory == null)
            {
                return;
            }
            if (immediate)
            {
                QueueEffect(effectFactory);
                return;
            }
            pendingEffectFactories.Add(effectFactory);
        }

        // Dependency-aware effect (runs when deps change). Null deps => run after every commit.
        protected void UseEffect(System.Func<System.Action> effectFactory, params object[] deps)
        {
            if (effectFactory == null)
            {
                return;
            }
            dependencyAwareEffects.Add((effectFactory, deps, null, null));
        }

        protected void SetState<T>(ref T field, T newValue)
        {
            field = newValue;
            SetState(() => { });
        }

        protected void SetState<T>(ref T field, System.Func<T, T> updater)
        {
            if (updater == null) return;
            field = updater(field);
            SetState(() => { });
        }

        private void QueueEffect(System.Func<System.Action> effectFactory)
        {
            try
            {
                System.Action cleanup = effectFactory();
                if (cleanup != null)
                {
                    effectCleanupActions.Add(cleanup);
                }
            }
            catch
            {
            }
        }

        private void FlushEffects()
        {
            if (pendingEffectFactories.Count > 0)
            {
                foreach (System.Func<System.Action> factory in pendingEffectFactories)
                {
                    QueueEffect(factory);
                }
                pendingEffectFactories.Clear();
            }
            for (int i = 0; i < dependencyAwareEffects.Count; i++)
            {
                var effect = dependencyAwareEffects[i];
                bool shouldRun = false;
                if (effect.deps == null)
                {
                    shouldRun = true;
                }
                else if (effect.lastDeps == null)
                {
                    shouldRun = true;
                }
                else if (DepsChanged(effect.lastDeps, effect.deps))
                {
                    shouldRun = true;
                }
                if (shouldRun)
                {
                    if (effect.cleanup != null)
                    {
                        try
                        {
                            effect.cleanup();
                        }
                        catch
                        {
                        }
                    }
                    System.Action newCleanup = null;
                    try
                    {
                        newCleanup = effect.factory();
                    }
                    catch
                    {
                    }
                    dependencyAwareEffects[i] = (effect.factory, effect.deps, (object[])effect.deps.Clone(), newCleanup);
                }
            }
        }

        private bool DepsChanged(object[] previousDependencies, object[] nextDependencies)
        {
            if (previousDependencies == null || nextDependencies == null)
            {
                return true;
            }
            if (previousDependencies.Length != nextDependencies.Length)
            {
                return true;
            }
            for (int i = 0; i < previousDependencies.Length; i++)
            {
                if (!Equals(previousDependencies[i], nextDependencies[i]))
                {
                    return true;
                }
            }
            return false;
        }

        internal void NotifyContextKeyChanged(string key)
        {
            ForceUpdate();
        }
    }
}
