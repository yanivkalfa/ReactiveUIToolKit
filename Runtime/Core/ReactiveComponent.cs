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

        private VirtualNode previousTree;
        private bool updateQueued;
        private Reconciler reconciler;
        private Dictionary<string, object> previousProps;
        private readonly List<System.Func<System.Action>> pendingEffects = new List<System.Func<System.Action>>();
        private readonly List<System.Action> effectCleanups = new List<System.Action>();
        private readonly List<(System.Func<System.Action> factory, object[] deps, object[] lastDeps, System.Action cleanup)> registeredEffects = new List<(System.Func<System.Action>, object[], object[], System.Action)>();
        private readonly List<System.Action> pendingStateMutations = new List<System.Action>();
        private readonly Dictionary<string, object> contextValues = new Dictionary<string, object>();

        protected abstract VirtualNode Render();

        // Lifecycle (React inspired) - prefer overriding the Component* variants.
        protected virtual void OnWillMount() { }
        protected virtual void OnDidMount() { }
        protected virtual void OnMounted() { } // backward compatibility
        protected virtual bool ShouldUpdate(Dictionary<string, object> nextProps)
        {
            if (previousProps == null) return true;
            return !ShallowCompare.PropsEqual(previousProps, nextProps);
        }
        protected virtual void OnWillUpdate() { }
        protected virtual void OnDidUpdate() { }
        protected virtual void OnUpdated() { } // backward compatibility
        protected virtual void OnWillUnmount() { }

        // React-like naming (aliases). Override these for familiarity; they route to the On* versions.
        protected virtual void ComponentWillMount() { OnWillMount(); }
        protected virtual void ComponentDidMount() { OnDidMount(); }
        protected virtual bool ShouldComponentUpdate(Dictionary<string, object> nextProps) { return ShouldUpdate(nextProps); }
        protected virtual void ComponentWillUpdate(Dictionary<string, object> nextProps) { OnWillUpdate(); }
        protected virtual void ComponentDidUpdate(Dictionary<string, object> prevProps) { OnDidUpdate(); }
        protected virtual void ComponentWillUnmount() { OnWillUnmount(); }

        public void Mount(VisualElement parent, HostContext hostContext)
        {
            if (reconciler == null)
            {
                reconciler = new Reconciler(hostContext);
            }
            HostContext = hostContext;

            if (MountedElement != null)
            {
                return;
            }

            MountedElement = new VisualElement
            {
                name = GetType().Name
            };

            parent.Add(MountedElement);
            OnWillMount();
            ComponentWillMount();
            VirtualNode nextTree = Render();
            reconciler.BuildSubtree(MountedElement, nextTree);
            previousTree = nextTree;
            OnDidMount();
            ComponentDidMount();
            OnMounted(); // backward compatibility
        }

        public void Unmount()
        {
            if (MountedElement == null)
            {
                return;
            }

            OnWillUnmount();
            ComponentWillUnmount();

            // Run effect cleanups
            foreach (var cleanup in effectCleanups)
            {
                try { cleanup?.Invoke(); } catch { }
            }
            effectCleanups.Clear();

            MountedElement.RemoveFromHierarchy();
            MountedElement = null;
            previousTree = null;
            updateQueued = false;
        }

        protected void SetState(System.Action stateMutation)
        {
            if (stateMutation != null)
            {
                pendingStateMutations.Add(stateMutation);
            }

            if (updateQueued == false)
            {
                updateQueued = true;
                RenderScheduler.Instance.Enqueue(() =>
                {
                    if (MountedElement == null)
                    {
                        updateQueued = false;
                        return;
                    }

                    if (ShouldUpdate(Props) == false || ShouldComponentUpdate(Props) == false)
                    {
                        updateQueued = false;
                        return;
                    }

                    OnWillUpdate();
                    ComponentWillUpdate(Props);
                    // apply all pending mutations before render
                    foreach (var mut in pendingStateMutations) { try { mut(); } catch { } }
                    pendingStateMutations.Clear();
                    VirtualNode nextTree = Render();
                    reconciler.DiffSubtree(MountedElement, previousTree, nextTree);
                    previousTree = nextTree;

                    updateQueued = false;
                    OnDidUpdate();
                    ComponentDidUpdate(previousProps);
                    OnUpdated(); // backward compatibility

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
            if (updateQueued)
            {
                return; // already queued; avoid duplicate
            }
            updateQueued = true;
            RenderScheduler.Instance.Enqueue(() =>
            {
                if (MountedElement == null)
                {
                    updateQueued = false;
                    return;
                }
                OnWillUpdate();
                ComponentWillUpdate(Props);
                foreach (var mut in pendingStateMutations) { try { mut(); } catch { } }
                pendingStateMutations.Clear();
                VirtualNode nextTree = Render();
                reconciler.DiffSubtree(MountedElement, previousTree, nextTree);
                previousTree = nextTree;
                updateQueued = false;
                OnDidUpdate();
                ComponentDidUpdate(previousProps);
                OnUpdated();
                FlushEffects();
            });
        }

        public void SetProps(Dictionary<string, object> nextProps)
        {
            previousProps = Props;
            Props = nextProps;
            SetState(() => { }); // trigger re-render pipeline
        }

        // Context API (simple key-value)
        protected void ProvideContext(string key, object value)
        {
            if (string.IsNullOrEmpty(key)) return;
            contextValues[key] = value;
            if (HostContext != null)
            {
                HostContext.Environment[key] = value;
            }
        }

        protected T ConsumeContext<T>(string key)
        {
            if (string.IsNullOrEmpty(key)) return default;
            if (contextValues.TryGetValue(key, out object val) && val is T typed) return typed;
            if (HostContext != null)
            {
                HostContext.Subscribe(key, this);
                var resolved = HostContext.ResolveContext(key);
                if (resolved is T gTyped) return gTyped;
            }
            return default;
        }

        // Ref helper returns underlying VisualElement host
        protected VisualElement UseRef() => MountedElement;

        // Effect that returns an optional cleanup action via Func<System.Action>
        protected void UseEffect(System.Func<System.Action> effectFactory, bool immediate = false)
        {
            if (effectFactory == null) return;
            if (immediate)
            {
                QueueEffect(effectFactory);
                return;
            }
            pendingEffects.Add(effectFactory);
        }

        // Dependency-aware effect (runs when deps change). Null deps => run after every commit.
        protected void UseEffect(System.Func<System.Action> effectFactory, params object[] deps)
        {
            if (effectFactory == null)
            {
                return;
            }
            registeredEffects.Add((effectFactory, deps, null, null));
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
                var cleanup = effectFactory();
                if (cleanup != null) effectCleanups.Add(cleanup);
            }
            catch { }
        }

        private void FlushEffects()
        {
            if (pendingEffects.Count == 0) return;
            foreach (var factory in pendingEffects)
            {
                QueueEffect(factory);
            }
            pendingEffects.Clear();

            // Registered dependency-aware effects
            for (int i = 0; i < registeredEffects.Count; i += 1)
            {
                var tuple = registeredEffects[i];
                bool shouldRun = false;
                if (tuple.deps == null)
                {
                    shouldRun = true; // always run
                }
                else if (tuple.lastDeps == null)
                {
                    shouldRun = true; // first time
                }
                else if (DepsChanged(tuple.lastDeps, tuple.deps))
                {
                    shouldRun = true;
                }

                if (shouldRun)
                {
                    // Cleanup previous specific to this effect
                    if (tuple.cleanup != null)
                    {
                        try { tuple.cleanup(); } catch { }
                    }
                    System.Action newCleanup = null;
                    try
                    {
                        newCleanup = tuple.factory();
                    }
                    catch { }
                    registeredEffects[i] = (tuple.factory, tuple.deps, (object[])tuple.deps.Clone(), newCleanup);
                }
            }
        }

        private bool DepsChanged(object[] oldDeps, object[] newDeps)
        {
            if (oldDeps == null || newDeps == null) return true;
            if (oldDeps.Length != newDeps.Length) return true;
            for (int i = 0; i < oldDeps.Length; i += 1)
            {
                if (!Equals(oldDeps[i], newDeps[i])) return true;
            }
            return false;
        }

        internal void NotifyContextKeyChanged(string key)
        {
            // Could add filter if component tracks which keys matter
            ForceUpdate();
        }
    }
}
