using System;
using System.Collections.Generic;
using ReactiveUITK.Elements;
using ReactiveUITK.Core.Util;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Core
{
    public sealed class Reconciler
    {
        private readonly HostContext hostContext;
        // Debug / metrics
		public static bool EnableDiffTracing = false; // legacy flag
		public enum DiffTraceLevel { None, Basic, Verbose }
		public static DiffTraceLevel TraceLevel = DiffTraceLevel.None;
        private int nodesReconciled;
        private int nodesSkipped;
        private int effectRuns;
		private int portalsBuilt;
		private int portalsUpdated;
		private System.Diagnostics.Stopwatch diffWatch = new System.Diagnostics.Stopwatch();
		private long lastDiffMs;
		private readonly Dictionary<string, VisualElement> elementCache = new Dictionary<string, VisualElement>();
		private int cacheHits;
		private int cacheMisses;
		private readonly Dictionary<VirtualNodeType, int> nodeTypeCounts = new Dictionary<VirtualNodeType, int>();

        public Reconciler(HostContext hostContext)
        {
            this.hostContext = hostContext;
        }

        internal void ForceFunctionComponentUpdate(NodeMetadata meta)
        {
            if (meta == null || meta.Container == null) return;
            meta.HookIndex = 0;
            RenderFunctionComponent(meta, meta.Container);
        }

        public void BuildSubtree(VisualElement host, VirtualNode node)
        {
			BeginDiffTiming();
            host.Clear();
            BuildChildren(host, node.Children);
			EndDiffTiming();
        }

        public void DiffSubtree(VisualElement host, VirtualNode previous, VirtualNode next)
        {
			BeginDiffTiming();
            DiffChildren(host, previous.Children, next.Children);
			EndDiffTiming();
        }

        private void BuildChildren(VisualElement parent, IReadOnlyList<VirtualNode> children)
        {
			bool warned = false;
			var seenKeys = new HashSet<string>();
			for (int index = 0; index < children.Count; index += 1)
			{
				var c = children[index];
				if (c.NodeType == VirtualNodeType.Fragment && !string.IsNullOrEmpty(c.Key))
				{
					if (!seenKeys.Add(c.Key) && !warned)
					{
						Debug.LogWarning($"ReactiveUITK: Duplicate fragment key '{c.Key}' under parent {parent.name}");
						warned = true;
					}
				}
				BuildNode(parent, c);
			}
        }

		private void BuildNode(VisualElement parent, VirtualNode node)
        {
            switch (node.NodeType)
            {
                case VirtualNodeType.Text:
                    Label label = new Label(node.TextContent ?? string.Empty);
                    label.userData = new NodeMetadata { Key = node.Key };
                    parent.Add(label);
                    return;
                case VirtualNodeType.Fragment:
                    BuildChildren(parent, node.Children);
                    return;
                case VirtualNodeType.Portal:
                    // Add placeholder to preserve indexing
					if (node.PortalTarget == null)
					{
						// No target => skip building placeholder to avoid lingering orphan
						return;
					}
					var placeholder = new VisualElement { name = "PortalPlaceholder" };
					placeholder.userData = new NodeMetadata { Key = node.Key };
					parent.Add(placeholder);
					node.PortalTarget.Clear();
					BuildChildren(node.PortalTarget, node.Children);
					portalsBuilt++;
                    return;
                case VirtualNodeType.Component when node.ComponentType != null:
                    GameObject go = new GameObject(node.ComponentType.Name);
                    var component = (ReactiveComponent)go.AddComponent(node.ComponentType);
                    var container = new VisualElement();
                    var metaComp = new NodeMetadata { Key = node.Key, ComponentInstance = component };
                    container.userData = metaComp;
                    parent.Add(container);
                    component.SetProps(new Dictionary<string, object>(node.Properties));
                    component.Mount(container, hostContext);
                    return;
                case VirtualNodeType.FunctionComponent when node.FunctionRender != null:
                    var funcContainer = new VisualElement();
                    var metaFunc = new NodeMetadata
                    {
                        Key = node.Key,
                        FuncRender = node.FunctionRender,
                        FuncProps = new Dictionary<string, object>(node.Properties),
                        FuncChildren = node.Children,
                        HookStates = new List<object>(),
                        HookIndex = 0,
                        Container = funcContainer,
                        HostContext = hostContext,
                        Reconciler = this
                    };
                    funcContainer.userData = metaFunc;
                    parent.Add(funcContainer);
                    RenderFunctionComponent(metaFunc);
                    return;
            }

            IElementAdapter adapter = hostContext.ElementRegistry.Resolve(node.ElementTypeName);
            if (adapter == null)
            {
                VisualElement fallback = new VisualElement();
                fallback.userData = new NodeMetadata { Key = node.Key };
                parent.Add(fallback);
                BuildChildren(fallback, node.Children);
                return;
            }
            VisualElement element = adapter.Create();
            element.userData = new NodeMetadata { Key = node.Key };
            adapter.ApplyProperties(element, node.Properties);
            parent.Add(element);
            BuildChildren(element, node.Children);
        }

        private void DiffChildren(VisualElement parent, IReadOnlyList<VirtualNode> oldChildren, IReadOnlyList<VirtualNode> newChildren)
        {
            bool hasAnyKey = HasAnyKey(oldChildren) || HasAnyKey(newChildren);
            if (!hasAnyKey)
            {
                DiffChildrenByIndex(parent, oldChildren, newChildren);
                return;
            }
            DiffChildrenByKey(parent, oldChildren, newChildren);
        }

        private void DiffChildrenByIndex(VisualElement parent, IReadOnlyList<VirtualNode> oldChildren, IReadOnlyList<VirtualNode> newChildren)
        {
            int oldCount = oldChildren.Count;
            int newCount = newChildren.Count;
            int shared = oldCount < newCount ? oldCount : newCount;
            for (int i = 0; i < shared; i++)
            {
                DiffNode(parent.ElementAt(i), oldChildren[i], newChildren[i]);
            }
            if (newCount > oldCount)
            {
                for (int i = oldCount; i < newCount; i++) BuildNode(parent, newChildren[i]);
            }
            else if (oldCount > newCount)
            {
                for (int i = oldCount - 1; i >= newCount; i--) parent.ElementAt(i).RemoveFromHierarchy();
            }
        }

        private void DiffChildrenByKey(VisualElement parent, IReadOnlyList<VirtualNode> oldChildren, IReadOnlyList<VirtualNode> newChildren)
        {
            var oldByKey = new Dictionary<string, (VirtualNode vnode, VisualElement element)>();
            var reused = new HashSet<string>();
            var duplicates = new HashSet<string>();
            for (int i = 0; i < oldChildren.Count; i++)
            {
                var oldNode = oldChildren[i];
                var oldElement = parent.ElementAt(i);
                var k = oldNode.Key;
                if (!string.IsNullOrEmpty(k))
                {
                    if (!oldByKey.ContainsKey(k)) oldByKey.Add(k, (oldNode, oldElement)); else duplicates.Add(k);
                }
            }
            var newOrder = new List<VisualElement>(newChildren.Count);
            for (int i = 0; i < newChildren.Count; i++)
            {
                var nextNode = newChildren[i];
                var k = nextNode.Key;
                if (!string.IsNullOrEmpty(k) && oldByKey.TryGetValue(k, out var tuple))
                {
                    DiffNode(tuple.element, tuple.vnode, nextNode);
                    newOrder.Add(tuple.element);
                    reused.Add(k);
                    continue;
                }
                newOrder.Add(CreateDetached(nextNode));
            }
            for (int i = 0; i < parent.childCount; i++)
            {
                var existing = parent.ElementAt(i);
                var existingKey = (existing.userData as NodeMetadata)?.Key;
                if (!string.IsNullOrEmpty(existingKey) && !reused.Contains(existingKey)) existing.RemoveFromHierarchy();
            }
            for (int i = 0; i < newOrder.Count; i++)
            {
                var desired = newOrder[i];
                if (desired.parent != parent) { parent.Insert(i, desired); continue; }
                if (parent.IndexOf(desired) != i) { desired.RemoveFromHierarchy(); parent.Insert(i, desired); }
            }
            if (duplicates.Count > 0)
            {
                Debug.LogWarning($"ReactiveUITK: Duplicate keys detected: {string.Join(",", duplicates)}");
            }
        }

		private VisualElement CreateDetached(VirtualNode node)
        {
            // Build without adding to parent hierarchy containers
			if (!string.IsNullOrEmpty(node.Key) && elementCache.TryGetValue(node.Key, out var cached) && cached.parent == null)
			{
				cacheHits++;
				IncrementNodeType(node.NodeType);
				if (EnableDiffTracing || TraceLevel == DiffTraceLevel.Verbose) Debug.Log($"[Reconciler] Reuse cached element key={node.Key}");
				return cached;
			}
			cacheMisses++;
			IncrementNodeType(node.NodeType);
            switch (node.NodeType)
            {
                case VirtualNodeType.Text:
                    Label label = new Label(node.TextContent ?? string.Empty);
                    label.userData = new NodeMetadata { Key = node.Key };
					Cache(node.Key, label);
					return label;
                case VirtualNodeType.Fragment:
                    // Fragment has no root element: create empty container
					var frag = new VisualElement();
                    frag.userData = new NodeMetadata { Key = node.Key };
                    BuildChildren(frag, node.Children);
					Cache(node.Key, frag);
                    return frag;
                case VirtualNodeType.Portal:
					var portalPlaceholder = new VisualElement { name = "PortalPlaceholder" };
                    portalPlaceholder.userData = new NodeMetadata { Key = node.Key };
                    if (node.PortalTarget != null)
                    {
                        node.PortalTarget.Clear();
                        BuildChildren(node.PortalTarget, node.Children);
                    }
					Cache(node.Key, portalPlaceholder);
                    return portalPlaceholder;
                case VirtualNodeType.Component when node.ComponentType != null:
                    GameObject go = new GameObject(node.ComponentType.Name);
                    var component = (ReactiveComponent)go.AddComponent(node.ComponentType);
					var container = new VisualElement();
                    container.userData = new NodeMetadata { Key = node.Key, ComponentInstance = component };
                    component.SetProps(new Dictionary<string, object>(node.Properties));
                    component.Mount(container, hostContext);
					Cache(node.Key, container);
                    return container;
                case VirtualNodeType.FunctionComponent when node.FunctionRender != null:
                    var funcContainer = new VisualElement();
                    var metaFunc = new NodeMetadata
                    {
                        Key = node.Key,
                        FuncRender = node.FunctionRender,
                        FuncProps = new Dictionary<string, object>(node.Properties),
                        FuncChildren = node.Children,
                        HookStates = new List<object>(),
                        HookIndex = 0,
                        Container = funcContainer,
                        HostContext = hostContext,
                        Reconciler = this
                    };
                    funcContainer.userData = metaFunc;
                    RenderFunctionComponent(metaFunc);
					Cache(node.Key, funcContainer);
                    return funcContainer;
                case VirtualNodeType.Suspense:
					var suspenseContainer = new VisualElement();
                    suspenseContainer.userData = new NodeMetadata { Key = node.Key };
                    bool ready = node.SuspenseReady?.Invoke() ?? true;
                    if (ready)
                    {
                        BuildChildren(suspenseContainer, node.Children);
                    }
                    else if (node.Fallback != null)
                    {
                        BuildChildren(suspenseContainer, new List<VirtualNode> { node.Fallback });
                    }
					Cache(node.Key, suspenseContainer);
                    return suspenseContainer;
            }
            // Element
            IElementAdapter adapter = hostContext.ElementRegistry.Resolve(node.ElementTypeName);
            VisualElement element = adapter != null ? adapter.Create() : new VisualElement();
            element.userData = new NodeMetadata { Key = node.Key };
            if (adapter != null) adapter.ApplyProperties(element, node.Properties);
            BuildChildren(element, node.Children);
			Cache(node.Key, element);
            return element;
        }

        private void DiffNode(VisualElement host, VirtualNode oldNode, VirtualNode newNode)
        {
            if (oldNode.NodeType != newNode.NodeType)
            {
                nodesReconciled++;
                ReplaceNode(host, newNode);
                return;
            }
            if (newNode.NodeType == VirtualNodeType.Text)
            {
                var label = host as Label;
                if (label == null) { ReplaceNode(host, newNode); return; }
                var newText = newNode.TextContent ?? string.Empty;
                if (label.text != newText) label.text = newText;
                else nodesSkipped++;
                return;
            }
            if (newNode.NodeType == VirtualNodeType.Portal)
            {
				var metaPortal = host.userData as NodeMetadata;
				if (newNode.PortalTarget != null)
				{
					// Incremental portal diff using stored previous children
					var prev = metaPortal?.PortalPreviousChildren ?? new List<VirtualNode>();
					DiffChildren(newNode.PortalTarget, prev, newNode.Children);
					if (metaPortal != null) metaPortal.PortalPreviousChildren = new List<VirtualNode>(newNode.Children);
					portalsUpdated++;
				if (EnableDiffTracing || TraceLevel != DiffTraceLevel.None) Debug.Log($"[PortalDiff] Updated portal key={newNode.Key} children={newNode.Children.Count}");
				}
				else
				{
					// Target removed: cleanup existing portal contents tracked previously
					if (metaPortal?.PortalPreviousChildren != null)
					{
						foreach (var prevChild in metaPortal.PortalPreviousChildren)
						{
							// No direct VisualElement ref; rely on target absence so hierarchy already gone
						}
						metaPortal.PortalPreviousChildren.Clear();
					}
				if (EnableDiffTracing || TraceLevel == DiffTraceLevel.Verbose) Debug.Log($"[PortalDiff] Portal key={newNode.Key} target removed; placeholder retained.");
				}
                return;
            }
            if (newNode.NodeType == VirtualNodeType.Element && oldNode.ElementTypeName != newNode.ElementTypeName)
            {
                ReplaceNode(host, newNode);
                return;
            }
            if (newNode.NodeType == VirtualNodeType.Component)
            {
                if (oldNode.ComponentType != newNode.ComponentType) { ReplaceNode(host, newNode); return; }
                var meta = host.userData as NodeMetadata;
                var instance = meta?.ComponentInstance;
                if (instance == null) { ReplaceNode(host, newNode); return; }
                if (newNode.Memoize)
                {
                    bool equal = false;
                    if (newNode.MemoCompare != null) equal = newNode.MemoCompare(oldNode.Properties, newNode.Properties);
                    else if (ReferenceEquals(oldNode.Properties, newNode.Properties)) equal = true;
                    else if (oldNode.Properties is IReadOnlyDictionary<string, object> a && newNode.Properties is IReadOnlyDictionary<string, object> b) equal = ShallowCompare.PropsEqual(a, b);
                    if (equal) return;
                }
                nodesReconciled++;
                try { instance.SetProps(new Dictionary<string, object>(newNode.Properties)); }
                catch (System.Exception ex) { Debug.LogError($"ReactiveUITK: Component update failed ({newNode.ComponentType.Name}): {ex}"); }
                return;
            }
            if (newNode.NodeType == VirtualNodeType.FunctionComponent)
            {
                var meta = host.userData as NodeMetadata;
                if (meta == null || meta.FuncRender == null) { ReplaceNode(host, newNode); return; }
                if (newNode.Memoize)
                {
                    bool equal = false;
                    if (newNode.MemoCompare != null) equal = newNode.MemoCompare(meta.FuncProps, newNode.Properties);
                    else if (ReferenceEquals(meta.FuncProps, newNode.Properties)) equal = true;
                    else if (meta.FuncProps is IReadOnlyDictionary<string, object> a && newNode.Properties is IReadOnlyDictionary<string, object> b) equal = ShallowCompare.PropsEqual(a, b);
                    if (equal) return;
                }
                nodesReconciled++;
                meta.FuncProps = new Dictionary<string, object>(newNode.Properties);
                meta.FuncChildren = newNode.Children;
                meta.HookIndex = 0;
                RenderFunctionComponent(meta, host);
                return;
            }
            // Element diff
            if (newNode.NodeType == VirtualNodeType.Element)
            {
                var adapter = hostContext.ElementRegistry.Resolve(newNode.ElementTypeName);
                if (adapter != null) adapter.ApplyPropertiesDiff(host, oldNode.Properties, newNode.Properties);
                DiffChildren(host, oldNode.Children, newNode.Children);
				if (EnableDiffTracing || TraceLevel == DiffTraceLevel.Verbose) Debug.Log($"[Diff] Element {newNode.ElementTypeName} key={newNode.Key} reconciled");
            }
        }

        private void ReplaceNode(VisualElement host, VirtualNode newNode)
        {
            var parent = host.parent; if (parent == null) return;
            int index = parent.IndexOf(host);
			var oldKey = (host.userData as NodeMetadata)?.Key;
			InvalidateCache(oldKey);
            RunRemovalCleanup(host);
            host.RemoveFromHierarchy();
            var container = new VisualElement();
            BuildNode(container, newNode);
            if (container.childCount > 0)
            {
                var replacement = container.ElementAt(0);
                parent.Insert(index, replacement);
            }
        }

        private bool HasAnyKey(IReadOnlyList<VirtualNode> list)
        {
            for (int i = 0; i < list.Count; i++) if (!string.IsNullOrEmpty(list[i].Key)) return true; return false;
        }

        private void RenderFunctionComponent(NodeMetadata meta, VisualElement reuseContainer = null)
        {
            var target = reuseContainer ?? meta.Container;
            if (target == null || meta.FuncRender == null) return;
            HookContext.Current = meta;
            try
            {
                var next = meta.FuncRender(meta.FuncProps, meta.FuncChildren);
                if (meta.LastRenderedSubtree == null || target.childCount == 0)
                {
                    target.Clear();
                    meta.LastRenderedSubtree = next;
                    if (next != null) BuildChildren(target, new List<VirtualNode> { next });
                    return;
                }
                if (next == null)
                {
                    target.Clear();
                    meta.LastRenderedSubtree = null;
                    return;
                }
                var existingRoot = target.ElementAt(0);
                DiffNode(existingRoot, meta.LastRenderedSubtree, next);
                meta.LastRenderedSubtree = next;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"ReactiveUITK: Function component render failed: {ex}");
            }
            finally { HookContext.Current = null; }

            // Layout effects first (synchronous)
            if (meta.FunctionLayoutEffects != null)
            {
                for (int i = 0; i < meta.FunctionLayoutEffects.Count; i++)
                {
                    var entry = meta.FunctionLayoutEffects[i];
                    bool shouldRun = entry.lastDeps == null || DepsChangedInternal(entry.lastDeps, entry.deps);
                    if (shouldRun)
                    {
                        try { entry.cleanup?.Invoke(); } catch { }
                        Action newCleanup = null;
                        try { newCleanup = entry.factory?.Invoke(); } catch { }
                        meta.FunctionLayoutEffects[i] = (entry.factory, entry.deps, (object[])entry.deps?.Clone(), newCleanup);
                        effectRuns++;
                    }
                }
            }
            // Passive effects batched
            if (meta.FunctionEffects != null)
            {
                for (int i = 0; i < meta.FunctionEffects.Count; i++)
                {
                    var entry = meta.FunctionEffects[i];
                    bool shouldRun = entry.lastDeps == null || DepsChangedInternal(entry.lastDeps, entry.deps);
                    if (shouldRun)
                    {
                        RenderScheduler.Instance.EnqueueBatchedEffect(() =>
                        {
                            try { entry.cleanup?.Invoke(); } catch { }
                            Action newCleanup = null;
                            try { newCleanup = entry.factory?.Invoke(); } catch { }
                            meta.FunctionEffects[i] = (entry.factory, entry.deps, (object[])entry.deps?.Clone(), newCleanup);
                            effectRuns++;
                        });
                    }
                }
            }
        }

        private bool DepsChangedInternal(object[] oldDeps, object[] newDeps)
        {
            if (oldDeps == null || newDeps == null) return true;
            if (oldDeps.Length != newDeps.Length) return true;
            for (int i = 0; i < oldDeps.Length; i++) if (!Equals(oldDeps[i], newDeps[i])) return true;
            return false;
        }

        private void RunRemovalCleanup(VisualElement ve)
        {
            var meta = ve?.userData as NodeMetadata;
            if (meta == null) return;
            if (meta.ComponentInstance != null)
            {
                try { UnityEngine.Object.Destroy(meta.ComponentInstance.gameObject); } catch { }
            }
            if (meta.FunctionEffects != null)
            {
                foreach (var eff in meta.FunctionEffects) { try { eff.cleanup?.Invoke(); } catch { } }
                meta.FunctionEffects.Clear();
            }
            if (meta.FunctionLayoutEffects != null)
            {
                foreach (var eff in meta.FunctionLayoutEffects) { try { eff.cleanup?.Invoke(); } catch { } }
                meta.FunctionLayoutEffects.Clear();
            }
        }

		public (int reconciled, int skipped, int effects, int portalsBuilt, int portalsUpdated, long lastDiffMs) GetMetrics() => (nodesReconciled, nodesSkipped, effectRuns, portalsBuilt, portalsUpdated, lastDiffMs);

		public (int cacheHits, int cacheMisses, Dictionary<VirtualNodeType,int> counts) GetExtendedMetrics() => (cacheHits, cacheMisses, new Dictionary<VirtualNodeType,int>(nodeTypeCounts));

		private void Cache(string key, VisualElement ve)
		{
			if (string.IsNullOrEmpty(key)) return;
			if (!elementCache.ContainsKey(key)) elementCache[key] = ve;
		}

		public void BeginDiffTiming()
		{
			diffWatch.Reset();
			diffWatch.Start();
		}

		public void EndDiffTiming()
		{
			if (diffWatch.IsRunning)
			{
				diffWatch.Stop();
				lastDiffMs = diffWatch.ElapsedMilliseconds;
			}
		}

		private void IncrementNodeType(VirtualNodeType type)
		{
			if (!nodeTypeCounts.ContainsKey(type)) nodeTypeCounts[type] = 0;
			nodeTypeCounts[type]++;
		}

		private void InvalidateCache(string key)
		{
			if (string.IsNullOrEmpty(key)) return;
			if (elementCache.ContainsKey(key)) elementCache.Remove(key);
		}
    }
}
