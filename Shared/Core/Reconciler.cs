using System;
using System.Collections.Generic;
using System.Diagnostics;
using ReactiveUITK.Elements;
using ReactiveUITK.Core.Util;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Core
{
    public sealed class Reconciler
    {
        private readonly HostContext hostContext;
        private IScheduler ResolveScheduler()
        {
            if (hostContext == null)
            {
                return null;
            }
            if (hostContext.Environment != null && hostContext.Environment.TryGetValue("scheduler", out var obj))
            {
                return obj as IScheduler;
            }
            return null;
        }
        public static bool EnableDiffTracing = false;
        public enum DiffTraceLevel { None, Basic, Verbose }
        public static DiffTraceLevel TraceLevel = DiffTraceLevel.None;
        private int reconciledNodeCount;
        private int skippedNodeCount;
        private int functionEffectRunCount;
        private int portalBuildCount;
        private int portalUpdateCount;
        private Stopwatch diffStopwatch = new();
        private long lastDiffDurationMs;
        private readonly Dictionary<string, VisualElement> elementCache = new();
        private int cacheHitCount;
        private int cacheMissCount;
        private readonly Dictionary<VirtualNodeType, int> nodeTypeBuildCounts = new();

        public Reconciler(HostContext hostContext)
        {
            this.hostContext = hostContext;
        }

        internal void ForceFunctionComponentUpdate(NodeMetadata nodeMetadata)
        {
            if (nodeMetadata == null || nodeMetadata.Container == null)
            {
                return;
            }
            if (EnableDiffTracing || TraceLevel == DiffTraceLevel.Verbose)
            {
                try { UnityEngine.Debug.Log("[ForceUpdate] key=" + nodeMetadata.Key); } catch { }
            }
            nodeMetadata.HookIndex = 0;
            RenderFunctionComponent(nodeMetadata, nodeMetadata.Container);
        }

        public void BuildSubtree(VisualElement hostElement, VirtualNode rootNode)
        {
            BeginDiffTiming();
            hostElement.Clear();
            if (rootNode != null)
            {
                if (rootNode.NodeType == VirtualNodeType.Element && !string.IsNullOrEmpty(rootNode.ElementTypeName))
                {
                    // Apply properties to the existing host element instead of creating a nested wrapper.
                    IElementAdapter adapter = hostContext.ElementRegistry.Resolve(rootNode.ElementTypeName);
                    if (adapter != null)
                    {
                        adapter.ApplyProperties(hostElement, rootNode.Properties);
                    }
                    BuildChildren(hostElement, rootNode.Children ?? Array.Empty<VirtualNode>());
                }
                else
                {
                    // Non-element root: create as child
                    BuildNode(hostElement, rootNode);
                }
            }
            EndDiffTiming();
        }

        public void DiffSubtree(VisualElement hostElement, VirtualNode previousRoot, VirtualNode nextRoot)
        {
            BeginDiffTiming();
            if (previousRoot == null)
            {
                hostElement.Clear();
                if (nextRoot != null)
                {
                    BuildSubtree(hostElement, nextRoot); // reuse build logic
                }
                EndDiffTiming();
                return;
            }
            if (nextRoot == null)
            {
                hostElement.Clear();
                EndDiffTiming();
                return;
            }
            // Special-case non-element roots (e.g., FunctionComponent, Fragment, Text):
            // Diff against the first child element instead of clearing the host.
            if (previousRoot.NodeType == nextRoot.NodeType && previousRoot.NodeType != VirtualNodeType.Element)
            {
                if (hostElement.childCount > 0)
                {
                    DiffNode(hostElement.ElementAt(0), previousRoot, nextRoot);
                }
                else
                {
                    BuildSubtree(hostElement, nextRoot);
                }
                EndDiffTiming();
                return;
            }
            if (previousRoot.NodeType == VirtualNodeType.Element && nextRoot.NodeType == VirtualNodeType.Element && previousRoot.ElementTypeName == nextRoot.ElementTypeName)
            {
                // Diff applied directly on hostElement
                IElementAdapter adapter = hostContext.ElementRegistry.Resolve(nextRoot.ElementTypeName);
                if (adapter != null)
                {
                    adapter.ApplyPropertiesDiff(hostElement, previousRoot.Properties, nextRoot.Properties);
                }
                DiffChildren(hostElement, previousRoot.Children ?? Array.Empty<VirtualNode>(), nextRoot.Children ?? Array.Empty<VirtualNode>());
            }
            else
            {
                // Replace whole subtree
                hostElement.Clear();
                BuildSubtree(hostElement, nextRoot);
            }
            EndDiffTiming();
        }

        private void BuildChildren(VisualElement parentElement, IReadOnlyList<VirtualNode> childNodes)
        {
            bool duplicateFragmentKeyWarned = false;
            HashSet<string> fragmentKeys = new();
            for (int index = 0; index < childNodes.Count; index += 1)
            {
                VirtualNode currentChild = childNodes[index];
                if (currentChild.NodeType == VirtualNodeType.Fragment && !string.IsNullOrEmpty(currentChild.Key))
                {
                    if (!fragmentKeys.Add(currentChild.Key) && !duplicateFragmentKeyWarned)
                    {
                        UnityEngine.Debug.LogWarning($"ReactiveUITK: Duplicate fragment key '{currentChild.Key}' under parent {parentElement.name}");
                        duplicateFragmentKeyWarned = true;
                    }
                }
                BuildNode(parentElement, currentChild);
            }
        }

        private void BuildNode(VisualElement parentElement, VirtualNode virtualNode)
        {
            switch (virtualNode.NodeType)
            {
                case VirtualNodeType.Text:
                    Label textLabel = new(virtualNode.TextContent ?? string.Empty) { userData = new NodeMetadata { Key = virtualNode.Key } };
                    parentElement.Add(textLabel);
                    return;
                case VirtualNodeType.Fragment:
                    // Create a container for fragment to aid styling/debugging.
                    VisualElement fragmentRoot = new() { name = string.IsNullOrEmpty(virtualNode.Key) ? "FragmentContainer" : ($"Fragment_{virtualNode.Key}") , userData = new NodeMetadata { Key = virtualNode.Key } };
                    parentElement.Add(fragmentRoot);
                    BuildChildren(fragmentRoot, virtualNode.Children);
                    return;
                case VirtualNodeType.Portal:
                    if (virtualNode.PortalTarget == null)
                    {
                        return;
                    }
                    VisualElement portalPlaceholderElement = new() { name = "PortalPlaceholder", userData = new NodeMetadata { Key = virtualNode.Key } };
                    parentElement.Add(portalPlaceholderElement);
                    virtualNode.PortalTarget.Clear();
                    BuildChildren(virtualNode.PortalTarget, virtualNode.Children);
                    portalBuildCount++;
                    return;
                case VirtualNodeType.Component when virtualNode.ComponentType != null:
                    bool isEditorHost = hostContext != null && hostContext.Environment != null && hostContext.Environment.TryGetValue("isEditor", out var isEdVal) && isEdVal is bool bb && bb;
                    GameObject componentGameObject = new(virtualNode.ComponentType.Name);
                    if (isEditorHost)
                    {
                        componentGameObject.hideFlags = HideFlags.HideAndDontSave;
                    }
                    IReactiveComponent reactiveComponentInstance = (IReactiveComponent)componentGameObject.AddComponent(virtualNode.ComponentType);
                    VisualElement componentContainer = new() { name = virtualNode.ComponentType.Name + "Container" };
                    NodeMetadata componentMetadata = new() { Key = virtualNode.Key, ComponentInstance = reactiveComponentInstance };
                    componentContainer.userData = componentMetadata;
                    parentElement.Add(componentContainer);
                    reactiveComponentInstance.SetProps(new Dictionary<string, object>(virtualNode.Properties));
                    reactiveComponentInstance.Mount(componentContainer, hostContext);
                    return;
                case VirtualNodeType.FunctionComponent when virtualNode.FunctionRender != null:
                    string funcName = virtualNode.FunctionRender.Method.Name;
                    // Always use wrapper container; no pre-render flattening
                    VisualElement functionComponentContainer = new() { name = string.IsNullOrEmpty(funcName) ? "FunctionComponent" : (funcName + "Container") };
                    functionComponentContainer.style.flexGrow = 1f;
                    NodeMetadata functionComponentMetadata = new()
                    {
                        Key = virtualNode.Key,
                        FuncRender = virtualNode.FunctionRender,
                        FuncProps = new Dictionary<string, object>(virtualNode.Properties),
                        FuncChildren = virtualNode.Children,
                        HookStates = new List<object>(),
                        HookIndex = 0,
                        Container = functionComponentContainer,
                        HostContext = hostContext,
                        Reconciler = this,
                        IsFlattened = false
                    };
                    functionComponentContainer.userData = functionComponentMetadata;
                    parentElement.Add(functionComponentContainer);
                    RenderFunctionComponent(functionComponentMetadata);
                    return;
            }
            IElementAdapter elementAdapter = hostContext.ElementRegistry.Resolve(virtualNode.ElementTypeName);
            if (elementAdapter == null)
            {
                VisualElement fallbackElement = new() { name = string.IsNullOrEmpty(virtualNode.Key) ? "UnknownElement" : ($"Unknown_{virtualNode.Key}"), userData = new NodeMetadata { Key = virtualNode.Key } };
                parentElement.Add(fallbackElement);
                BuildChildren(fallbackElement, virtualNode.Children);
                return;
            }
            VisualElement createdElement = elementAdapter.Create();
            if (string.IsNullOrEmpty(createdElement.name))
            {
                createdElement.name = string.IsNullOrEmpty(virtualNode.Key) ? (virtualNode.ElementTypeName + "Element") : ($"{virtualNode.ElementTypeName}_{virtualNode.Key}");
            }
            createdElement.userData = new NodeMetadata { Key = virtualNode.Key };
            elementAdapter.ApplyProperties(createdElement, virtualNode.Properties);
            parentElement.Add(createdElement);
            BuildChildren(createdElement, virtualNode.Children);
        }

        private void DiffChildren(VisualElement parentElement, IReadOnlyList<VirtualNode> previousChildren, IReadOnlyList<VirtualNode> nextChildren)
        {
            bool anyKeyPresent = HasAnyKey(previousChildren) || HasAnyKey(nextChildren);
            if (!anyKeyPresent)
            {
                DiffChildrenByIndex(parentElement, previousChildren, nextChildren);
                return;
            }
            DiffChildrenByKey(parentElement, previousChildren, nextChildren);
        }

        private void DiffChildrenByIndex(VisualElement parentElement, IReadOnlyList<VirtualNode> previousChildren, IReadOnlyList<VirtualNode> nextChildren)
        {
            int previousCount = previousChildren.Count;
            int nextCount = nextChildren.Count;
            int sharedCount = previousCount < nextCount ? previousCount : nextCount;
            for (int i = 0; i < sharedCount; i++)
            {
                DiffNode(parentElement.ElementAt(i), previousChildren[i], nextChildren[i]);
            }
            if (nextCount > previousCount)
            {
                for (int i = previousCount; i < nextCount; i++)
                {
                    BuildNode(parentElement, nextChildren[i]);
                }
            }
            else if (previousCount > nextCount)
            {
                for (int i = previousCount - 1; i >= nextCount; i--)
                {
                    var toRemove = parentElement.ElementAt(i);
                    RunRemovalCleanup(toRemove);
                    toRemove.RemoveFromHierarchy();
                }
            }
        }

        private void DiffChildrenByKey(VisualElement parentElement, IReadOnlyList<VirtualNode> previousChildren, IReadOnlyList<VirtualNode> nextChildren)
        {
            // Map keyed previous children and collect unkeyed pairs to preserve their relative order
            Dictionary<string, (VirtualNode vnode, VisualElement element)> previousChildrenByKey = new();
            Queue<(VirtualNode vnode, VisualElement element)> unkeyedQueue = new();
            HashSet<string> duplicateKeys = new();
            for (int i = 0; i < previousChildren.Count; i++)
            {
                VirtualNode prevNode = previousChildren[i];
                VisualElement prevElement = parentElement.ElementAt(i);
                string key = prevNode.Key;
                if (!string.IsNullOrEmpty(key))
                {
                    if (!previousChildrenByKey.ContainsKey(key))
                    {
                        previousChildrenByKey.Add(key, (prevNode, prevElement));
                    }
                    else
                    {
                        duplicateKeys.Add(key);
                    }
                }
                else
                {
                    unkeyedQueue.Enqueue((prevNode, prevElement));
                }
            }

            List<VisualElement> orderedElements = new(nextChildren.Count);
            HashSet<string> reusedKeys = new();
            HashSet<VisualElement> reusedElements = new();

            for (int i = 0; i < nextChildren.Count; i++)
            {
                VirtualNode nextChildNode = nextChildren[i];
                string key = nextChildNode.Key;
                if (!string.IsNullOrEmpty(key) && previousChildrenByKey.TryGetValue(key, out var tuple))
                {
                    int oldIndex = parentElement.IndexOf(tuple.element);
                    DiffNode(tuple.element, tuple.vnode, nextChildNode);
                    VisualElement resolved = tuple.element;
                    if (resolved.parent != parentElement)
                    {
                        // Element was replaced; find new one by key
                        VisualElement found = null;
                        for (int c = 0; c < parentElement.childCount; c++)
                        {
                            var md = parentElement.ElementAt(c).userData as NodeMetadata;
                            if (md != null && md.Key == key)
                            {
                                found = parentElement.ElementAt(c);
                                break;
                            }
                        }
                        resolved = found ?? (oldIndex >= 0 && oldIndex < parentElement.childCount ? parentElement.ElementAt(oldIndex) : null);
                    }
                    if (resolved == null)
                    {
                        resolved = CreateDetached(nextChildNode);
                    }
                    orderedElements.Add(resolved);
                    reusedKeys.Add(key);
                    reusedElements.Add(resolved);
                    continue;
                }
                // Unkeyed: try to reuse previous unkeyed element in order
                if (string.IsNullOrEmpty(key) && unkeyedQueue.Count > 0)
                {
                    var pair = unkeyedQueue.Dequeue();
                    int oldIndex = parentElement.IndexOf(pair.element);
                    DiffNode(pair.element, pair.vnode, nextChildNode);
                    VisualElement resolved = pair.element;
                    if (resolved.parent != parentElement)
                    {
                        // Replaced; best-effort: pick element now at old index
                        resolved = (oldIndex >= 0 && oldIndex < parentElement.childCount) ? parentElement.ElementAt(oldIndex) : null;
                    }
                    if (resolved == null)
                    {
                        resolved = CreateDetached(nextChildNode);
                    }
                    orderedElements.Add(resolved);
                    reusedElements.Add(resolved);
                    continue;
                }
                // No reusable element, create new
                orderedElements.Add(CreateDetached(nextChildNode));
            }

            // Remove any previous child not reused (both keyed and unkeyed)
            for (int i = parentElement.childCount - 1; i >= 0; i--)
            {
                VisualElement existingElement = parentElement.ElementAt(i);
                string existingKey = (existingElement.userData as NodeMetadata)?.Key;
                bool keep = (!string.IsNullOrEmpty(existingKey) && reusedKeys.Contains(existingKey)) || reusedElements.Contains(existingElement);
                if (!keep)
                {
                    RunRemovalCleanup(existingElement);
                    existingElement.RemoveFromHierarchy();
                }
            }

            // Ensure correct order
            for (int i = 0; i < orderedElements.Count; i++)
            {
                VisualElement desiredElement = orderedElements[i];
                if (desiredElement.parent != parentElement)
                {
                    parentElement.Insert(i, desiredElement);
                    continue;
                }
                if (parentElement.IndexOf(desiredElement) != i)
                {
                    desiredElement.RemoveFromHierarchy();
                    parentElement.Insert(i, desiredElement);
                }
            }

            if (duplicateKeys.Count > 0)
            {
                UnityEngine.Debug.LogWarning($"ReactiveUITK: Duplicate keys detected: {string.Join(",", duplicateKeys)}");
            }
        }

        private VisualElement CreateDetached(VirtualNode virtualNode)
        {
            if (!string.IsNullOrEmpty(virtualNode.Key) && elementCache.TryGetValue(virtualNode.Key, out VisualElement cachedElement) && cachedElement.parent == null)
            {
                cacheHitCount++;
                IncrementNodeType(virtualNode.NodeType);
                if (EnableDiffTracing || TraceLevel == DiffTraceLevel.Verbose)
                {
                    UnityEngine.Debug.Log($"[Reconciler] Reuse cached element key={virtualNode.Key}");
                }
                return cachedElement;
            }
            cacheMissCount++;
            IncrementNodeType(virtualNode.NodeType);
            switch (virtualNode.NodeType)
            {
                case VirtualNodeType.Text:
                    Label detachedTextLabel = new(virtualNode.TextContent ?? string.Empty) { userData = new NodeMetadata { Key = virtualNode.Key } };
                    Cache(virtualNode.Key, detachedTextLabel);
                    return detachedTextLabel;
                case VirtualNodeType.Fragment:
                    VisualElement fragmentContainer = new() { name = string.IsNullOrEmpty(virtualNode.Key) ? "FragmentContainer" : ($"Fragment_{virtualNode.Key}"), userData = new NodeMetadata { Key = virtualNode.Key } };
                    BuildChildren(fragmentContainer, virtualNode.Children);
                    Cache(virtualNode.Key, fragmentContainer);
                    return fragmentContainer;
                case VirtualNodeType.Portal:
                    VisualElement portalPlaceholderElement = new() { name = "PortalPlaceholder", userData = new NodeMetadata { Key = virtualNode.Key } };
                    if (virtualNode.PortalTarget != null)
                    {
                        virtualNode.PortalTarget.Clear();
                        BuildChildren(virtualNode.PortalTarget, virtualNode.Children);
                    }
                    Cache(virtualNode.Key, portalPlaceholderElement);
                    return portalPlaceholderElement;
                case VirtualNodeType.Component when virtualNode.ComponentType != null:
                    bool isEditorHostDet = hostContext != null && hostContext.Environment != null && hostContext.Environment.TryGetValue("isEditor", out var isEdValDet) && isEdValDet is bool bbb && bbb;
                    GameObject componentGameObject = new(virtualNode.ComponentType.Name);
                    if (isEditorHostDet)
                    {
                        componentGameObject.hideFlags = HideFlags.HideAndDontSave;
                    }
                    IReactiveComponent reactiveComponentInstance = (IReactiveComponent)componentGameObject.AddComponent(virtualNode.ComponentType);
                    VisualElement componentContainer = new() { name = virtualNode.ComponentType.Name + "Container", userData = new NodeMetadata { Key = virtualNode.Key, ComponentInstance = reactiveComponentInstance } };
                    reactiveComponentInstance.SetProps(new Dictionary<string, object>(virtualNode.Properties));
                    reactiveComponentInstance.Mount(componentContainer, hostContext);
                    Cache(virtualNode.Key, componentContainer);
                    return componentContainer;
                case VirtualNodeType.FunctionComponent when virtualNode.FunctionRender != null:
                    string funcName = virtualNode.FunctionRender.Method.Name;
                    // Wrapper-only semantics: never pre-render/flatten
                    VisualElement functionComponentContainer = new() { name = string.IsNullOrEmpty(funcName) ? "FunctionComponent" : (funcName + "Container") };
                    functionComponentContainer.style.flexGrow = 1f;
                    NodeMetadata wrapperMetadata = new()
                    {
                        Key = virtualNode.Key,
                        FuncRender = virtualNode.FunctionRender,
                        FuncProps = new Dictionary<string, object>(virtualNode.Properties),
                        FuncChildren = virtualNode.Children,
                        HookStates = new List<object>(),
                        HookIndex = 0,
                        Container = functionComponentContainer,
                        HostContext = hostContext,
                        Reconciler = this,
                        IsFlattened = false
                    };
                    functionComponentContainer.userData = wrapperMetadata;
                    RenderFunctionComponent(wrapperMetadata);
                    Cache(virtualNode.Key, functionComponentContainer);
                    return functionComponentContainer;
                case VirtualNodeType.Suspense:
                    VisualElement suspenseContainerElement = new() { userData = new NodeMetadata { Key = virtualNode.Key } };
                    bool suspenseReady = virtualNode.SuspenseReady?.Invoke() ?? true;
                    if (suspenseReady)
                    {
                        BuildChildren(suspenseContainerElement, virtualNode.Children);
                    }
                    else if (virtualNode.Fallback != null)
                    {
                        BuildChildren(suspenseContainerElement, new List<VirtualNode> { virtualNode.Fallback });
                    }
                    Cache(virtualNode.Key, suspenseContainerElement);
                    return suspenseContainerElement;
            }
            IElementAdapter elementAdapter = hostContext.ElementRegistry.Resolve(virtualNode.ElementTypeName);
            VisualElement createdElement = elementAdapter != null ? elementAdapter.Create() : new VisualElement();
            createdElement.userData = new NodeMetadata { Key = virtualNode.Key };
            if (string.IsNullOrEmpty(createdElement.name))
            {
                createdElement.name = elementAdapter != null ? (virtualNode.ElementTypeName + "Element") : "GenericElement";
            }
            if (elementAdapter != null)
            {
                elementAdapter.ApplyProperties(createdElement, virtualNode.Properties);
            }
            BuildChildren(createdElement, virtualNode.Children);
            Cache(virtualNode.Key, createdElement);
            return createdElement;
        }

        private void DiffNode(VisualElement hostElement, VirtualNode previousNode, VirtualNode nextNode)
        {
			if (previousNode.NodeType != nextNode.NodeType)
			{
				reconciledNodeCount++;
				ReplaceNode(hostElement, nextNode);
				return;
			}
			if (nextNode.NodeType == VirtualNodeType.Text)
			{
				Label labelElement = hostElement as Label;
				if (labelElement == null)
				{
					ReplaceNode(hostElement, nextNode);
					return;
				}
				string newTextContent = nextNode.TextContent ?? string.Empty;
				if (labelElement.text != newTextContent)
				{
					labelElement.text = newTextContent;
				}
				else
				{
					skippedNodeCount++;
				}
				return;
			}
			if (nextNode.NodeType == VirtualNodeType.Portal)
			{
				NodeMetadata portalMetadata = hostElement.userData as NodeMetadata;
				if (nextNode.PortalTarget != null)
				{
					List<VirtualNode> previousPortalChildren = portalMetadata?.PortalPreviousChildren ?? new List<VirtualNode>();
					DiffChildren(nextNode.PortalTarget, previousPortalChildren, nextNode.Children);
					if (portalMetadata != null)
					{
						portalMetadata.PortalPreviousChildren = new List<VirtualNode>(nextNode.Children);
					}
					portalUpdateCount++;
				}
				else
				{
					if (portalMetadata?.PortalPreviousChildren != null)
					{
						portalMetadata.PortalPreviousChildren.Clear();
					}
				}
				return;
			}
			if (nextNode.NodeType == VirtualNodeType.Fragment)
			{
				// hostElement is the fragment container created in BuildNode; diff its children
				DiffChildren(hostElement, previousNode.Children ?? Array.Empty<VirtualNode>(), nextNode.Children ?? Array.Empty<VirtualNode>());
				return;
			}
			if (nextNode.NodeType == VirtualNodeType.Element && previousNode.ElementTypeName != nextNode.ElementTypeName)
			{
				ReplaceNode(hostElement, nextNode);
				return;
			}
			if (nextNode.NodeType == VirtualNodeType.Component)
			{
				if (previousNode.ComponentType != nextNode.ComponentType)
				{
					ReplaceNode(hostElement, nextNode);
					return;
				}
                NodeMetadata componentMetadata = hostElement.userData as NodeMetadata;
                IReactiveComponent componentInstance = componentMetadata?.ComponentInstance;
                if (componentInstance == null)
                {
                    ReplaceNode(hostElement, nextNode);
                    return;
                }
				if (ShouldSkipMemo(previousNode, nextNode, previousNode.Properties, nextNode.Properties))
				{
					return;
				}
				reconciledNodeCount++;
				try
				{
					componentInstance.SetProps(new Dictionary<string, object>(nextNode.Properties));
				}
				catch (Exception ex)
				{
					UnityEngine.Debug.LogError($"ReactiveUITK: Component update failed ({nextNode.ComponentType.Name}): {ex}");
				}
				return;
			}
			if (nextNode.NodeType == VirtualNodeType.FunctionComponent)
			{
				NodeMetadata functionMetadata = hostElement.userData as NodeMetadata;
				if (functionMetadata == null || functionMetadata.FuncRender == null)
				{
					ReplaceNode(hostElement, nextNode);
					return;
				}
				if (ShouldSkipMemo(previousNode, nextNode, functionMetadata.FuncProps, nextNode.Properties))
				{
					return;
				}
				reconciledNodeCount++;
				functionMetadata.FuncProps = new Dictionary<string, object>(nextNode.Properties);
				functionMetadata.FuncChildren = nextNode.Children;
				functionMetadata.HookIndex = 0;
				RenderFunctionComponent(functionMetadata, hostElement);
				return;
			}
			if (nextNode.NodeType == VirtualNodeType.Element)
			{
				IElementAdapter elementAdapter = hostContext.ElementRegistry.Resolve(nextNode.ElementTypeName);
				if (elementAdapter != null)
				{
					elementAdapter.ApplyPropertiesDiff(hostElement, previousNode.Properties, nextNode.Properties);
				}
				DiffChildren(hostElement, previousNode.Children, nextNode.Children);
				if (EnableDiffTracing || TraceLevel == DiffTraceLevel.Verbose)
				{
					UnityEngine.Debug.Log($"[Diff] Element {nextNode.ElementTypeName} key={nextNode.Key} reconciled");
				}
			}
		}

		private bool ShouldSkipMemo(VirtualNode previousNode, VirtualNode nextNode, IReadOnlyDictionary<string, object> previousProps, IReadOnlyDictionary<string, object> nextProps)
		{
			if (!nextNode.Memoize)
			{
				return false;
			}
			bool arePropsEqual = false;
			if (nextNode.MemoCompare != null)
			{
				arePropsEqual = nextNode.MemoCompare(previousProps, nextProps);
			}
			else if (ReferenceEquals(previousProps, nextProps))
			{
				arePropsEqual = true;
			}
			else if (previousProps is IReadOnlyDictionary<string, object> previousPropsDict && nextProps is IReadOnlyDictionary<string, object> nextPropsDict)
			{
				arePropsEqual = ShallowCompare.PropsEqual(previousPropsDict, nextPropsDict);
			}
			return arePropsEqual;
		}

        private void ReplaceNode(VisualElement hostElement, VirtualNode nextNode)
        {
            VisualElement parentElement = hostElement.parent;
            if (parentElement == null)
            {
                return;
            }
            int hostIndex = parentElement.IndexOf(hostElement);
            string existingKey = (hostElement.userData as NodeMetadata)?.Key;
            InvalidateCache(existingKey);
            if (EnableDiffTracing || TraceLevel != DiffTraceLevel.None)
            {
                try { UnityEngine.Debug.Log("[ReplaceNode] parent=" + parentElement.name + ", index=" + hostIndex + ", existingKey=" + existingKey + ", nextType=" + nextNode.NodeType + ", nextKey=" + nextNode.Key); } catch { }
            }
            RunRemovalCleanup(hostElement);
            hostElement.RemoveFromHierarchy();
            VisualElement buildContainer = new();
            BuildNode(buildContainer, nextNode);
            if (buildContainer.childCount > 0)
            {
                VisualElement replacementElement = buildContainer.ElementAt(0);
                parentElement.Insert(hostIndex, replacementElement);
            }
        }

        private bool HasAnyKey(IReadOnlyList<VirtualNode> nodes)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (!string.IsNullOrEmpty(nodes[i].Key))
                {
                    return true;
                }
            }
            return false;
        }

        private void RenderFunctionComponent(NodeMetadata functionComponentMetadata, VisualElement reuseContainer = null)
        {
            VisualElement targetContainer = reuseContainer ?? functionComponentMetadata.Container;
            if (targetContainer == null || functionComponentMetadata.FuncRender == null)
            {
                return;
            }
            // Reset hook indices before render
            functionComponentMetadata.HookIndex = 0;
            functionComponentMetadata.EffectIndex = 0;
            functionComponentMetadata.LayoutEffectIndex = 0;
            HookContext.Current = functionComponentMetadata;
            try
            {
                functionComponentMetadata.IsRendering = true;
                if (EnableDiffTracing || TraceLevel == DiffTraceLevel.Verbose)
                {
                    try { UnityEngine.Debug.Log("[FuncRender:enter] key=" + functionComponentMetadata.Key + ", pending=" + functionComponentMetadata.PendingUpdate); } catch { }
                }
                VirtualNode nextSubtree = functionComponentMetadata.FuncRender(functionComponentMetadata.FuncProps, functionComponentMetadata.FuncChildren);
                // Wrapper-only semantics (no flattened root)
                if (functionComponentMetadata.LastRenderedSubtree == null || targetContainer.childCount == 0)
                {
                    targetContainer.Clear();
                    functionComponentMetadata.LastRenderedSubtree = nextSubtree;
                    if (nextSubtree != null)
                    {
                        BuildChildren(targetContainer, new List<VirtualNode> { nextSubtree });
                    }
                    return;
                }
                if (nextSubtree == null)
                {
                    targetContainer.Clear();
                    functionComponentMetadata.LastRenderedSubtree = null;
                    return;
                }
                VisualElement existingRootElement = targetContainer.ElementAt(0);
                DiffNode(existingRootElement, functionComponentMetadata.LastRenderedSubtree, nextSubtree);
                functionComponentMetadata.LastRenderedSubtree = nextSubtree;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"ReactiveUITK: Function component render failed: {ex}");
            }
            finally
            {
                functionComponentMetadata.IsRendering = false;
                HookContext.Current = null;
            }
            if (functionComponentMetadata.FunctionLayoutEffects != null)
            {
                for (int i = 0; i < functionComponentMetadata.FunctionLayoutEffects.Count; i++)
                {
                    var entry = functionComponentMetadata.FunctionLayoutEffects[i];
                    bool shouldRun = entry.lastDeps == null || DepsChangedInternal(entry.lastDeps, entry.deps);
                    if (shouldRun)
                    {
                        try
                        {
                            entry.cleanup?.Invoke();
                        }
                        catch
                        {
                        }
                        Action newCleanup = null;
                        try
                        {
                            newCleanup = entry.factory?.Invoke();
                        }
                        catch
                        {
                        }
                        if (i < functionComponentMetadata.FunctionLayoutEffects.Count)
                        {
                            functionComponentMetadata.FunctionLayoutEffects[i] = (entry.factory, entry.deps, (object[])entry.deps?.Clone(), newCleanup);
                        }
                        functionEffectRunCount++;
                    }
                }
            }
            if (functionComponentMetadata.FunctionEffects != null)
            {
                for (int i = 0; i < functionComponentMetadata.FunctionEffects.Count; i++)
                {
                    var entry = functionComponentMetadata.FunctionEffects[i];
                    bool shouldRun = entry.lastDeps == null || DepsChangedInternal(entry.lastDeps, entry.deps);
                    if (shouldRun)
                    {
                        bool firstRun = entry.lastDeps == null;
                        // Pre-stamp lastDeps to avoid duplicate scheduling across rapid renders
                        functionComponentMetadata.FunctionEffects[i] = (entry.factory, entry.deps, (object[])entry.deps?.Clone(), entry.cleanup);
                        var schedulerC = ResolveScheduler();
                        if (!firstRun && schedulerC != null)
                        {
                            // Subsequent runs: schedule batched
                            schedulerC.EnqueueBatchedEffect(() =>
                            {
                                try { entry.cleanup?.Invoke(); } catch { }
                                Action newCleanup = null;
                                try { newCleanup = entry.factory?.Invoke(); } catch { }
                                if (i < functionComponentMetadata.FunctionEffects.Count)
                                {
                                    functionComponentMetadata.FunctionEffects[i] = (entry.factory, entry.deps, (object[])entry.deps?.Clone(), newCleanup);
                                }
                                functionEffectRunCount++;
                            });
                        }
                        else
                        {
                            // First run or no scheduler: run immediately after commit
                            try { entry.cleanup?.Invoke(); } catch { }
                            Action newCleanup = null;
                            try { newCleanup = entry.factory?.Invoke(); } catch { }
                            if (i < functionComponentMetadata.FunctionEffects.Count)
                            {
                                functionComponentMetadata.FunctionEffects[i] = (entry.factory, entry.deps, (object[])entry.deps?.Clone(), newCleanup);
                            }
                            functionEffectRunCount++;
                        }
                    }
                }
            }
            // After commit, flush one pending update if requested during render
            if (functionComponentMetadata.PendingUpdate)
            {
                functionComponentMetadata.PendingUpdate = false;
                var sched = ResolveScheduler();
                if (EnableDiffTracing || TraceLevel == DiffTraceLevel.Verbose)
                {
                    try { UnityEngine.Debug.Log("[FuncRender:flush-pending] key=" + functionComponentMetadata.Key); } catch { }
                }
                if (sched != null)
                {
                    sched.Enqueue(() =>
                    {
                        functionComponentMetadata.HookIndex = 0;
                        ForceFunctionComponentUpdate(functionComponentMetadata);
                    });
                }
                else
                {
                    functionComponentMetadata.HookIndex = 0;
                    ForceFunctionComponentUpdate(functionComponentMetadata);
                }
            }
            else if (EnableDiffTracing || TraceLevel == DiffTraceLevel.Verbose)
            {
                try { UnityEngine.Debug.Log("[FuncRender:exit] key=" + functionComponentMetadata.Key); } catch { }
            }
        }

        private bool DepsChangedInternal(object[] previousDependencies, object[] nextDependencies)
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

        private void RunRemovalCleanup(VisualElement element)
        {
            NodeMetadata metadata = element?.userData as NodeMetadata;
            if (metadata == null)
            {
                return;
            }
            // Unregister all registered event wrappers to avoid duplicate invocations on reused visuals
            if (metadata.EventHandlers != null && metadata.EventHandlers.Count > 0)
            {
                var snapshot = new List<KeyValuePair<string, Delegate>>(metadata.EventHandlers);
                foreach (var kv in snapshot)
                {
                    try
                    {
                        string eventPropName = kv.Key;
                        Delegate wrapper = kv.Value;
                        // Reuse removal helper to unregister by type
                        try { Props.PropsApplier.NotifyElementRemoved(element); } catch { }
                        try
                        {
                            // Local remove mirrors RemoveEvent logic
                            if (eventPropName == "onClick" && wrapper is EventCallback<ClickEvent> clickCb) element.UnregisterCallback(clickCb);
                            else if (eventPropName == "onPointerDown" && wrapper is EventCallback<PointerDownEvent> pd) element.UnregisterCallback(pd);
                            else if (eventPropName == "onPointerUp" && wrapper is EventCallback<PointerUpEvent> pu) element.UnregisterCallback(pu);
                            else if (eventPropName == "onPointerMove" && wrapper is EventCallback<PointerMoveEvent> pm) element.UnregisterCallback(pm);
                            else if (eventPropName == "onPointerEnter" && wrapper is EventCallback<PointerEnterEvent> pe) element.UnregisterCallback(pe);
                            else if (eventPropName == "onPointerLeave" && wrapper is EventCallback<PointerLeaveEvent> pl) element.UnregisterCallback(pl);
                            else if (eventPropName == "onWheel" && wrapper is EventCallback<WheelEvent> we) element.UnregisterCallback(we);
                            else if (eventPropName == "onFocus" && wrapper is EventCallback<FocusEvent> fe) element.UnregisterCallback(fe);
                            else if (eventPropName == "onBlur" && wrapper is EventCallback<BlurEvent> be) element.UnregisterCallback(be);
                            else if (eventPropName == "onKeyDown" && wrapper is EventCallback<KeyDownEvent> kd) element.UnregisterCallback(kd);
                            else if (eventPropName == "onKeyUp" && wrapper is EventCallback<KeyUpEvent> ku) element.UnregisterCallback(ku);
                            else if (eventPropName == "onChange")
                            {
                                if (wrapper is EventCallback<ChangeEvent<string>> chs) element.UnregisterCallback(chs);
                                if (wrapper is EventCallback<ChangeEvent<bool>> chb) element.UnregisterCallback(chb);
                                if (wrapper is EventCallback<ChangeEvent<int>> chi) element.UnregisterCallback(chi);
                            }
                            else if (eventPropName == "onInput" && wrapper is EventCallback<InputEvent> ie) element.UnregisterCallback(ie);
                            #if UNITY_EDITOR
                            else if (eventPropName == "onDragEnter" && wrapper is EventCallback<DragEnterEvent> de) element.UnregisterCallback(de);
                            else if (eventPropName == "onDragLeave" && wrapper is EventCallback<DragLeaveEvent> dle) element.UnregisterCallback(dle);
                            #endif
                            else if (eventPropName == "onScroll" && wrapper is EventCallback<WheelEvent> se) element.UnregisterCallback(se);
                        }
                        catch { }
                    }
                    catch { }
                }
                metadata.EventHandlers.Clear();
                if (metadata.EventHandlerTargets != null) metadata.EventHandlerTargets.Clear();
                if (metadata.EventHandlerSignatures != null) metadata.EventHandlerSignatures.Clear();
            }
            if (metadata.ComponentInstance != null)
            {
                try
                {
                    if (metadata.ComponentInstance is UnityEngine.MonoBehaviour mb)
                    {
                        UnityEngine.Object.Destroy(mb.gameObject);
                    }
                }
                catch
                {
                }
            }
            if (metadata.FunctionEffects != null)
            {
                foreach (var effect in metadata.FunctionEffects)
                {
                    try
                    {
                        effect.cleanup?.Invoke();
                    }
                    catch
                    {
                    }
                }
                metadata.FunctionEffects.Clear();
            }
            if (metadata.FunctionLayoutEffects != null)
            {
                foreach (var effect in metadata.FunctionLayoutEffects)
                {
                    try
                    {
                        effect.cleanup?.Invoke();
                    }
                    catch
                    {
                    }
                }
                metadata.FunctionLayoutEffects.Clear();
            }
        }

        public (int reconciled, int skipped, int effects, int portalsBuilt, int portalsUpdated, long lastDiffMs) GetMetrics() => (reconciledNodeCount, skippedNodeCount, functionEffectRunCount, portalBuildCount, portalUpdateCount, lastDiffDurationMs);

        public (int cacheHits, int cacheMisses, Dictionary<VirtualNodeType, int> counts) GetExtendedMetrics() => (cacheHitCount, cacheMissCount, new Dictionary<VirtualNodeType, int>(nodeTypeBuildCounts));

        private void Cache(string key, VisualElement element)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }
            if (!elementCache.ContainsKey(key))
            {
                elementCache[key] = element;
            }
        }

        public void BeginDiffTiming()
        {
            diffStopwatch.Reset();
            diffStopwatch.Start();
        }

        public void EndDiffTiming()
        {
            if (diffStopwatch.IsRunning)
            {
                diffStopwatch.Stop();
                lastDiffDurationMs = diffStopwatch.ElapsedMilliseconds;
            }
        }

        private void IncrementNodeType(VirtualNodeType nodeType)
        {
            if (!nodeTypeBuildCounts.ContainsKey(nodeType))
            {
                nodeTypeBuildCounts[nodeType] = 0;
            }
            nodeTypeBuildCounts[nodeType]++;
        }

        private void InvalidateCache(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }
            if (elementCache.ContainsKey(key))
            {
                elementCache.Remove(key);
            }
        }

        // (Converters removed – styles no longer applied directly here.)
    }
}

