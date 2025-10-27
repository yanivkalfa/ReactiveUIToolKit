using System;
using System.Collections.Generic;
using System.Diagnostics;
using ReactiveUITK.Core.Util;
using ReactiveUITK.Elements;
using ReactiveUITK.Elements.Pools;
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
            if (
                hostContext.Environment != null
                && hostContext.Environment.TryGetValue("scheduler", out var obj)
            )
            {
                return obj as IScheduler;
            }
            return null;
        }

        // Returns only the children this reconciler created (those tagged with NodeMetadata)
        private static void GetManagedChildren(VisualElement parent, List<VisualElement> buffer)
        {
            buffer.Clear();
            for (int i = 0; i < parent.childCount; i++)
            {
                var ve = parent.ElementAt(i);
                if (ve.userData is NodeMetadata)
                    buffer.Add(ve);
            }
        }

        public static bool EnableDiffTracing = false;

        public enum DiffTraceLevel
        {
            None,
            Basic,
            Verbose,
        }

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
                try
                {
                    UnityEngine.Debug.Log("[ForceUpdate] key=" + nodeMetadata.Key);
                }
                catch { }
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
                if (
                    rootNode.NodeType == VirtualNodeType.Element
                    && !string.IsNullOrEmpty(rootNode.ElementTypeName)
                )
                {
                    // Apply properties to the existing host element instead of creating a nested wrapper.
                    IElementAdapter adapter = hostContext.ElementRegistry.Resolve(
                        rootNode.ElementTypeName
                    );
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

        public void DiffSubtree(
            VisualElement hostElement,
            VirtualNode previousRoot,
            VirtualNode nextRoot
        )
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
            if (
                previousRoot.NodeType == nextRoot.NodeType
                && previousRoot.NodeType != VirtualNodeType.Element
            )
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
            if (
                previousRoot.NodeType == VirtualNodeType.Element
                && nextRoot.NodeType == VirtualNodeType.Element
                && previousRoot.ElementTypeName == nextRoot.ElementTypeName
            )
            {
                // Diff applied directly on hostElement
                IElementAdapter adapter = hostContext.ElementRegistry.Resolve(
                    nextRoot.ElementTypeName
                );
                if (adapter != null)
                {
                    adapter.ApplyPropertiesDiff(
                        hostElement,
                        previousRoot.Properties,
                        nextRoot.Properties
                    );
                }
                DiffChildren(
                    hostElement,
                    previousRoot.Children ?? Array.Empty<VirtualNode>(),
                    nextRoot.Children ?? Array.Empty<VirtualNode>()
                );
            }
            else
            {
                // Replace whole subtree
                hostElement.Clear();
                BuildSubtree(hostElement, nextRoot);
            }
            EndDiffTiming();
        }

        private void BuildChildren(
            VisualElement parentElement,
            IReadOnlyList<VirtualNode> childNodes
        )
        {
            bool duplicateFragmentKeyWarned = false;
            HashSet<string> fragmentKeys = new();
            for (int index = 0; index < childNodes.Count; index += 1)
            {
                VirtualNode currentChild = childNodes[index];
                if (
                    currentChild.NodeType == VirtualNodeType.Fragment
                    && !string.IsNullOrEmpty(currentChild.Key)
                )
                {
                    if (!fragmentKeys.Add(currentChild.Key) && !duplicateFragmentKeyWarned)
                    {
                        UnityEngine.Debug.LogWarning(
                            $"ReactiveUITK: Duplicate fragment key '{currentChild.Key}' under parent {parentElement.name}"
                        );
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
                    Label textLabel = new(virtualNode.TextContent ?? string.Empty)
                    {
                        userData = new NodeMetadata { Key = virtualNode.Key },
                    };
                    parentElement.Add(textLabel);
                    return;
                case VirtualNodeType.Fragment:
                    // Create a container for fragment to aid styling/debugging.
                    VisualElement fragmentRoot = new()
                    {
                        name = string.IsNullOrEmpty(virtualNode.Key)
                            ? "FragmentContainer"
                            : ($"Fragment_{virtualNode.Key}"),
                        userData = new NodeMetadata { Key = virtualNode.Key },
                    };
                    parentElement.Add(fragmentRoot);
                    BuildChildren(fragmentRoot, virtualNode.Children);
                    return;
                case VirtualNodeType.Portal:
                    if (virtualNode.PortalTarget == null)
                    {
                        return;
                    }
                    VisualElement portalPlaceholderElement = new()
                    {
                        name = "PortalPlaceholder",
                        userData = new NodeMetadata { Key = virtualNode.Key },
                    };
                    parentElement.Add(portalPlaceholderElement);
                    virtualNode.PortalTarget.Clear();
                    BuildChildren(virtualNode.PortalTarget, virtualNode.Children);
                    portalBuildCount++;
                    return;
                // Class component nodes removed
                case VirtualNodeType.FunctionComponent when virtualNode.FunctionRender != null:
                    string funcName = virtualNode.FunctionRender.Method.Name;
                    // Always use wrapper container; no pre-render flattening
                    VisualElement functionComponentContainer = new()
                    {
                        name = string.IsNullOrEmpty(funcName)
                            ? "FunctionComponent"
                            : (funcName + "Container"),
                    };
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
                        IsFlattened = false,
                    };
                    functionComponentContainer.userData = functionComponentMetadata;
                    parentElement.Add(functionComponentContainer);
                    RenderFunctionComponent(functionComponentMetadata);
                    return;
            }
            IElementAdapter elementAdapter = hostContext.ElementRegistry.Resolve(
                virtualNode.ElementTypeName
            );
            if (elementAdapter == null)
            {
                VisualElement fallbackElement = new()
                {
                    name = string.IsNullOrEmpty(virtualNode.Key)
                        ? "UnknownElement"
                        : ($"Unknown_{virtualNode.Key}"),
                    userData = new NodeMetadata { Key = virtualNode.Key },
                };
                parentElement.Add(fallbackElement);
                BuildChildren(fallbackElement, virtualNode.Children);
                return;
            }
            VisualElement createdElement = elementAdapter.Create();
            if (string.IsNullOrEmpty(createdElement.name))
            {
                createdElement.name = string.IsNullOrEmpty(virtualNode.Key)
                    ? (virtualNode.ElementTypeName + "Element")
                    : ($"{virtualNode.ElementTypeName}_{virtualNode.Key}");
            }
            createdElement.userData = new NodeMetadata { Key = virtualNode.Key };
            elementAdapter.ApplyProperties(createdElement, virtualNode.Properties);
            parentElement.Add(createdElement);
            var childrenHost = elementAdapter.ResolveChildHost(createdElement);
            BuildChildren(childrenHost, virtualNode.Children);
        }

        private void DiffChildren(
            VisualElement parentElement,
            IReadOnlyList<VirtualNode> previousChildren,
            IReadOnlyList<VirtualNode> nextChildren
        )
        {
            bool anyKeyPresent = HasAnyKey(previousChildren) || HasAnyKey(nextChildren);
            if (!anyKeyPresent)
            {
                DiffChildrenByIndex(parentElement, previousChildren, nextChildren);
                return;
            }
            DiffChildrenByKey(parentElement, previousChildren, nextChildren);
        }

        private void DiffChildrenByIndex(
            VisualElement parentElement,
            IReadOnlyList<VirtualNode> previousChildren,
            IReadOnlyList<VirtualNode> nextChildren
        )
        {
            int previousCount = previousChildren?.Count ?? 0;
            int nextCount = nextChildren?.Count ?? 0;

            var managed = new List<VisualElement>(Math.Max(previousCount, nextCount));
            GetManagedChildren(parentElement, managed);

            int managedCount = managed.Count;
            int shared = Math.Min(Math.Min(previousCount, nextCount), managedCount);

            for (int i = 0; i < shared; i++)
                DiffNode(managed[i], previousChildren[i], nextChildren[i]);

            for (int i = shared; i < nextCount; i++)
                BuildNode(parentElement, nextChildren[i]);

            for (int i = managedCount - 1; i >= nextCount; i--)
            {
                var toRemove = managed[i];
                RunRemovalCleanup(toRemove);
                toRemove.RemoveFromHierarchy();
                GlobalVisualElementPool.Release(toRemove);
            }
        }

        private void DiffChildrenByKey(
            VisualElement parentElement,
            IReadOnlyList<VirtualNode> previousChildren,
            IReadOnlyList<VirtualNode> nextChildren
        )
        {
            var previousChildrenByKey =
                new Dictionary<string, (VirtualNode vnode, VisualElement element)>();
            var unkeyedQueue = new Queue<(VirtualNode vnode, VisualElement element)>();

            int prevCount = previousChildren?.Count ?? 0;
            int nextCount = nextChildren?.Count ?? 0;

            var managed = new List<VisualElement>(prevCount);
            GetManagedChildren(parentElement, managed);

            int mapCount = Math.Min(prevCount, managed.Count);
            for (int i = 0; i < mapCount; i++)
            {
                var prevNode = previousChildren[i];
                var prevElement = managed[i];
                string key = prevNode.Key;

                if (!string.IsNullOrEmpty(key))
                {
                    if (!previousChildrenByKey.ContainsKey(key))
                        previousChildrenByKey.Add(key, (prevNode, prevElement));
                }
                else
                {
                    unkeyedQueue.Enqueue((prevNode, prevElement));
                }
            }

            var orderedElements = new List<VisualElement>(nextCount);
            var reusedKeys = new HashSet<string>();
            var reusedElements = new HashSet<VisualElement>();

            for (int i = 0; i < nextCount; i++)
            {
                var nextChildNode = nextChildren[i];
                var key = nextChildNode.Key;

                if (
                    !string.IsNullOrEmpty(key)
                    && previousChildrenByKey.TryGetValue(key, out var tuple)
                )
                {
                    DiffNode(tuple.element, tuple.vnode, nextChildNode);
                    var resolved = tuple.element;

                    if (resolved.parent != parentElement)
                    {
                        VisualElement replacement = null;
                        for (int j = 0; j < parentElement.childCount; j++)
                        {
                            var md = parentElement.ElementAt(j).userData as NodeMetadata;
                            if (md != null && md.Key == key)
                            {
                                replacement = parentElement.ElementAt(j);
                                break;
                            }
                        }
                        if (replacement != null)
                            resolved = replacement;
                    }

                    orderedElements.Add(resolved);
                    reusedKeys.Add(key);
                    reusedElements.Add(resolved);
                }
                else
                {
                    if (unkeyedQueue.Count > 0)
                    {
                        var (pv, pe) = unkeyedQueue.Dequeue();
                        int oldIndex = parentElement.IndexOf(pe);
                        DiffNode(pe, pv, nextChildNode);
                        var resolved = pe;
                        if (resolved.parent != parentElement)
                        {
                            // Replaced; best-effort: pick element now at old index
                            resolved =
                                (oldIndex >= 0 && oldIndex < parentElement.childCount)
                                    ? parentElement.ElementAt(oldIndex)
                                    : null;
                        }
                        if (resolved == null)
                        {
                            resolved = CreateDetached(nextChildNode);
                        }
                        orderedElements.Add(resolved);
                        reusedElements.Add(resolved);
                    }
                    else
                    {
                        orderedElements.Add(CreateDetached(nextChildNode));
                    }
                }
            }

            // Remove any managed visual child not reused
            var managedAfter = new List<VisualElement>(
                Math.Max(managed.Count, orderedElements.Count)
            );
            GetManagedChildren(parentElement, managedAfter);
            for (int i = managedAfter.Count - 1; i >= 0; i--)
            {
                var existing = managedAfter[i]; // managed => has NodeMetadata
                var md = existing.userData as NodeMetadata;
                bool keep =
                    (!string.IsNullOrEmpty(md.Key) && reusedKeys.Contains(md.Key))
                    || reusedElements.Contains(existing);

                if (!keep)
                {
                    RunRemovalCleanup(existing);
                    existing.RemoveFromHierarchy();
                    GlobalVisualElementPool.Release(existing);
                }
            }

            // Reorder managed relative to each other (don't fight template children)
            VisualElement anchor = null;

            // Ensure parented
            for (int i = 0; i < orderedElements.Count; i++)
                if (orderedElements[i].parent != parentElement)
                    parentElement.Add(orderedElements[i]);

            for (int i = 0; i < orderedElements.Count; i++)
            {
                var el = orderedElements[i];

                if (anchor == null)
                {
                    // Put first managed before the first existing managed (if any)
                    var existingManaged = new List<VisualElement>(orderedElements.Count);
                    GetManagedChildren(parentElement, existingManaged);

                    if (existingManaged.Count > 0)
                    {
                        var firstManaged = existingManaged[0];
                        if (!ReferenceEquals(firstManaged, el))
                        {
                            el.RemoveFromHierarchy();
                            parentElement.Insert(parentElement.IndexOf(firstManaged), el);
                        }
                    }
                }
                else
                {
                    int anchorIndex = parentElement.IndexOf(anchor);
                    int elIndex = parentElement.IndexOf(el);
                    if (elIndex != anchorIndex + 1)
                    {
                        el.RemoveFromHierarchy();
                        parentElement.Insert(anchorIndex + 1, el);
                    }
                }

                anchor = el;
            }
        }

        private VisualElement CreateDetached(VirtualNode virtualNode)
        {
            if (
                !string.IsNullOrEmpty(virtualNode.Key)
                && elementCache.TryGetValue(virtualNode.Key, out VisualElement cachedElement)
                && cachedElement.parent == null
            )
            {
                cacheHitCount++;
                IncrementNodeType(virtualNode.NodeType);
                if (EnableDiffTracing || TraceLevel == DiffTraceLevel.Verbose)
                {
                    UnityEngine.Debug.Log(
                        $"[Reconciler] Reuse cached element key={virtualNode.Key}"
                    );
                }
                return cachedElement;
            }

            cacheMissCount++;
            IncrementNodeType(virtualNode.NodeType);

            switch (virtualNode.NodeType)
            {
                case VirtualNodeType.Text:
                {
                    var detachedTextLabel = new Label(virtualNode.TextContent ?? string.Empty)
                    {
                        userData = new NodeMetadata { Key = virtualNode.Key },
                    };
                    Cache(virtualNode.Key, detachedTextLabel);
                    return detachedTextLabel;
                }

                case VirtualNodeType.Fragment:
                {
                    var fragmentContainer = new VisualElement
                    {
                        name = string.IsNullOrEmpty(virtualNode.Key)
                            ? "FragmentContainer"
                            : $"Fragment_{virtualNode.Key}",
                        userData = new NodeMetadata { Key = virtualNode.Key },
                    };
                    BuildChildren(fragmentContainer, virtualNode.Children);
                    Cache(virtualNode.Key, fragmentContainer);
                    return fragmentContainer;
                }

                case VirtualNodeType.Portal:
                {
                    var portalPlaceholderElement = new VisualElement
                    {
                        name = "PortalPlaceholder",
                        userData = new NodeMetadata { Key = virtualNode.Key },
                    };
                    if (virtualNode.PortalTarget != null)
                    {
                        virtualNode.PortalTarget.Clear();
                        BuildChildren(virtualNode.PortalTarget, virtualNode.Children);
                    }
                    Cache(virtualNode.Key, portalPlaceholderElement);
                    return portalPlaceholderElement;
                }

                case VirtualNodeType.FunctionComponent when virtualNode.FunctionRender != null:
                {
                    string funcName = virtualNode.FunctionRender.Method.Name;
                    var functionComponentContainer = new VisualElement
                    {
                        name = string.IsNullOrEmpty(funcName)
                            ? "FunctionComponent"
                            : (funcName + "Container"),
                    };
                    functionComponentContainer.style.flexGrow = 1f;

                    var wrapperMetadata = new NodeMetadata
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
                        IsFlattened = false,
                    };
                    functionComponentContainer.userData = wrapperMetadata;

                    RenderFunctionComponent(wrapperMetadata);
                    Cache(virtualNode.Key, functionComponentContainer);
                    return functionComponentContainer;
                }

                case VirtualNodeType.Suspense:
                {
                    var suspenseContainerElement = new VisualElement
                    {
                        userData = new NodeMetadata { Key = virtualNode.Key },
                    };
                    bool suspenseReady = virtualNode.SuspenseReady?.Invoke() ?? true;
                    if (suspenseReady)
                    {
                        BuildChildren(suspenseContainerElement, virtualNode.Children);
                    }
                    else if (virtualNode.Fallback != null)
                    {
                        BuildChildren(
                            suspenseContainerElement,
                            new List<VirtualNode> { virtualNode.Fallback }
                        );
                    }
                    Cache(virtualNode.Key, suspenseContainerElement);
                    return suspenseContainerElement;
                }
            }

            // ---- Element case (build into child host) ----
            IElementAdapter elementAdapter = hostContext.ElementRegistry.Resolve(
                virtualNode.ElementTypeName
            );

            var createdElement =
                (elementAdapter != null) ? elementAdapter.Create() : new VisualElement();
            createdElement.userData = new NodeMetadata { Key = virtualNode.Key };

            if (string.IsNullOrEmpty(createdElement.name))
                createdElement.name =
                    (elementAdapter != null)
                        ? (virtualNode.ElementTypeName + "Element")
                        : "GenericElement";

            elementAdapter?.ApplyProperties(createdElement, virtualNode.Properties);

            // Build vnode children into the adapter's stable child host
            var childrenHost = elementAdapter?.ResolveChildHost(createdElement) ?? createdElement;
            if (childrenHost == null)
                childrenHost = createdElement; // robust fallback

            BuildChildren(childrenHost, virtualNode.Children);

            Cache(virtualNode.Key, createdElement);
            return createdElement;
        }

        private void DiffNode(
            VisualElement hostElement,
            VirtualNode previousNode,
            VirtualNode nextNode
        )
        {
            if (previousNode.NodeType != nextNode.NodeType)
            {
                reconciledNodeCount++;
                ReplaceNode(hostElement, nextNode);
                return;
            }

            if (nextNode.NodeType == VirtualNodeType.Text)
            {
                var labelElement = hostElement as Label;
                if (labelElement == null)
                {
                    ReplaceNode(hostElement, nextNode);
                    return;
                }
                string newTextContent = nextNode.TextContent ?? string.Empty;
                if (labelElement.text != newTextContent)
                    labelElement.text = newTextContent;
                else
                    skippedNodeCount++;
                return;
            }

            if (nextNode.NodeType == VirtualNodeType.Portal)
            {
                var portalMetadata = hostElement.userData as NodeMetadata;
                if (nextNode.PortalTarget != null)
                {
                    var previousPortalChildren =
                        portalMetadata?.PortalPreviousChildren ?? new List<VirtualNode>();
                    DiffChildren(nextNode.PortalTarget, previousPortalChildren, nextNode.Children);
                    if (portalMetadata != null)
                        portalMetadata.PortalPreviousChildren = new List<VirtualNode>(
                            nextNode.Children
                        );
                    portalUpdateCount++;
                }
                else
                {
                    portalMetadata?.PortalPreviousChildren?.Clear();
                }
                return;
            }

            if (nextNode.NodeType == VirtualNodeType.Fragment)
            {
                DiffChildren(
                    hostElement,
                    previousNode.Children ?? Array.Empty<VirtualNode>(),
                    nextNode.Children ?? Array.Empty<VirtualNode>()
                );
                return;
            }

            if (
                nextNode.NodeType == VirtualNodeType.Element
                && previousNode.ElementTypeName != nextNode.ElementTypeName
            )
            {
                ReplaceNode(hostElement, nextNode);
                return;
            }

            // Function component
            if (nextNode.NodeType == VirtualNodeType.FunctionComponent)
            {
                NodeMetadata functionMetadata = hostElement.userData as NodeMetadata;
                if (functionMetadata == null || functionMetadata.FuncRender == null)
                {
                    ReplaceNode(hostElement, nextNode);
                    return;
                }

                // React-style memo: shallow props AND shallow children
                bool propsEq = ShallowPropsEqual(functionMetadata.FuncProps, nextNode.Properties);
                bool childrenEq = ShallowChildrenEqual(previousNode.Children, nextNode.Children);
                bool skip = propsEq && childrenEq;

                // Optional trace (fully qualified to avoid ambiguity)
                UnityEngine.Debug.Log(
                    $"[Memo] key={(nextNode.Key ?? "(no-key)")} skip={skip} propsEq={propsEq} childrenEq={childrenEq}"
                );

                if (skip)
                    return;

                reconciledNodeCount++;
                functionMetadata.FuncProps = new Dictionary<string, object>(nextNode.Properties);
                functionMetadata.FuncChildren = nextNode.Children;
                functionMetadata.HookIndex = 0;
                RenderFunctionComponent(functionMetadata, hostElement);
                return;
            }

            // Element
            if (nextNode.NodeType == VirtualNodeType.Element)
            {
                var elementAdapter = hostContext.ElementRegistry.Resolve(nextNode.ElementTypeName);

                // Resolve child host BEFORE updating props (ApplyPropertiesDiff may swap the container)
                var oldChildHost = elementAdapter?.ResolveChildHost(hostElement) ?? hostElement;

                // Minor fast-path: if no vnode children at all, just patch props and return
                var prevKids = previousNode.Children ?? Array.Empty<VirtualNode>();
                var nextKids = nextNode.Children ?? Array.Empty<VirtualNode>();
                bool bothEmpty = (prevKids.Count == 0 && nextKids.Count == 0);

                elementAdapter?.ApplyPropertiesDiff(
                    hostElement,
                    previousNode.Properties,
                    nextNode.Properties
                );

                // Resolve AGAIN AFTER props — container might have changed
                var newChildHost = elementAdapter?.ResolveChildHost(hostElement) ?? hostElement;

                // If the container changed, reparent ONLY our visuals (those we created), not template internals
                if (!ReferenceEquals(newChildHost, oldChildHost))
                {
                    // collect managed kids
                    var buffer = new List<VisualElement>();
                    for (int i = 0; i < oldChildHost.childCount; i++)
                    {
                        var ch = oldChildHost.ElementAt(i);
                        if (ch.userData is NodeMetadata)
                            buffer.Add(ch);
                    }

                    if (buffer.Count > 0)
                    {
                        for (int i = 0; i < buffer.Count; i++)
                        {
                            var ch = buffer[i];
                            ch.RemoveFromHierarchy();
                            newChildHost.Add(ch);
                        }
                    }
                }

                if (!bothEmpty)
                {
                    DiffChildren(newChildHost, prevKids, nextKids);
                }

                if (EnableDiffTracing || TraceLevel == DiffTraceLevel.Verbose)
                {
                    UnityEngine.Debug.Log(
                        $"[Diff] Element {nextNode.ElementTypeName} key={nextNode.Key} reconciled"
                    );
                }
            }
        }

        // Shallow compare of props: count, keys, Equals(value)
        private static bool ShallowPropsEqual(
            IReadOnlyDictionary<string, object> a,
            IReadOnlyDictionary<string, object> b
        )
        {
            if (ReferenceEquals(a, b))
                return true;
            int ac = a?.Count ?? 0,
                bc = b?.Count ?? 0;
            if (ac != bc)
                return false;
            if (ac == 0)
                return true;

            foreach (var kv in a)
            {
                if (!b.TryGetValue(kv.Key, out var bv))
                    return false;
                if (!Equals(kv.Value, bv))
                    return false; // shallow
            }
            return true;
        }

        // Treat "children" like React's props.children: shallow identity/shape check.
        // 1) Fast path: same reference.
        // 2) Same length and each slot is the same instance OR has same key+type signature.
        private static bool ShallowChildrenEqual(
            IReadOnlyList<VirtualNode> a,
            IReadOnlyList<VirtualNode> b
        )
        {
            if (ReferenceEquals(a, b))
                return true;
            int ac = a?.Count ?? 0,
                bc = b?.Count ?? 0;
            if (ac != bc)
                return false;
            if (ac == 0)
                return true;

            for (int i = 0; i < ac; i++)
            {
                var an = a[i];
                var bn = b[i];
                if (ReferenceEquals(an, bn))
                    continue;
                if (an == null || bn == null)
                    return false;

                // Compare a light identity signature (like React's key+type)
                if (an.NodeType != bn.NodeType)
                    return false;
                if (
                    !string.Equals(
                        an.Key ?? string.Empty,
                        bn.Key ?? string.Empty,
                        StringComparison.Ordinal
                    )
                )
                    return false;
                if (
                    an.NodeType == VirtualNodeType.Element
                    && !string.Equals(
                        an.ElementTypeName,
                        bn.ElementTypeName,
                        StringComparison.Ordinal
                    )
                )
                    return false;
                if (
                    an.NodeType == VirtualNodeType.Text
                    && !string.Equals(
                        an.TextContent ?? string.Empty,
                        bn.TextContent ?? string.Empty,
                        StringComparison.Ordinal
                    )
                )
                    return false;
            }
            return true;
        }

        // === USE THIS as your ShouldSkipMemo ===
        private static bool ShouldSkipMemo(
            VirtualNode previousNode,
            VirtualNode nextNode,
            IReadOnlyDictionary<string, object> prevProps,
            IReadOnlyDictionary<string, object> nextProps
        )
        {
            if (!ShallowPropsEqual(prevProps, nextProps))
                return false;
            if (!ShallowChildrenEqual(previousNode.Children, nextNode.Children))
                return false;
            return true; // props AND children look the same -> skip render
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
                try
                {
                    UnityEngine.Debug.Log(
                        "[ReplaceNode] parent="
                            + parentElement.name
                            + ", index="
                            + hostIndex
                            + ", existingKey="
                            + existingKey
                            + ", nextType="
                            + nextNode.NodeType
                            + ", nextKey="
                            + nextNode.Key
                    );
                }
                catch { }
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

        private void RenderFunctionComponent(
            NodeMetadata functionComponentMetadata,
            VisualElement reuseContainer = null
        )
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
            bool initialMount =
                functionComponentMetadata.LastRenderedSubtree == null
                || targetContainer.childCount == 0;
            VirtualNode nextSubtree = null;
            try
            {
                functionComponentMetadata.IsRendering = true;
                if (EnableDiffTracing || TraceLevel == DiffTraceLevel.Verbose)
                {
                    try
                    {
                        UnityEngine.Debug.Log(
                            "[FuncRender:enter] key="
                                + functionComponentMetadata.Key
                                + ", pending="
                                + functionComponentMetadata.PendingUpdate
                        );
                    }
                    catch { }
                }
                nextSubtree = functionComponentMetadata.FuncRender(
                    functionComponentMetadata.FuncProps,
                    functionComponentMetadata.FuncChildren
                );
                // Unified handling: build/diff then continue to effect phase (no early returns) so first render runs effects.
                if (nextSubtree == null)
                {
                    // Clear any existing child and mark as empty
                    targetContainer.Clear();
                    functionComponentMetadata.LastRenderedSubtree = null;
                }
                else if (initialMount)
                {
                    targetContainer.Clear();
                    functionComponentMetadata.LastRenderedSubtree = nextSubtree;
                    BuildChildren(targetContainer, new List<VirtualNode> { nextSubtree });
                }
                else
                {
                    // Diff against previous cached subtree
                    VisualElement existingRootElement =
                        targetContainer.childCount > 0 ? targetContainer.ElementAt(0) : null;
                    if (existingRootElement == null)
                    {
                        targetContainer.Clear();
                        BuildChildren(targetContainer, new List<VirtualNode> { nextSubtree });
                    }
                    else
                    {
                        DiffNode(
                            existingRootElement,
                            functionComponentMetadata.LastRenderedSubtree,
                            nextSubtree
                        );
                    }
                    functionComponentMetadata.LastRenderedSubtree = nextSubtree;
                }
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
            // Layout effects phase
            if (functionComponentMetadata.FunctionLayoutEffects != null)
            {
                for (int i = 0; i < functionComponentMetadata.FunctionLayoutEffects.Count; i++)
                {
                    var entry = functionComponentMetadata.FunctionLayoutEffects[i];
                    bool shouldRun =
                        entry.lastDeps == null || DepsChangedInternal(entry.lastDeps, entry.deps);
                    if (shouldRun)
                    {
                        try
                        {
                            entry.cleanup?.Invoke();
                        }
                        catch { }
                        Action newCleanup = null;
                        try
                        {
                            newCleanup = entry.factory?.Invoke();
                        }
                        catch { }
                        if (i < functionComponentMetadata.FunctionLayoutEffects.Count)
                        {
                            functionComponentMetadata.FunctionLayoutEffects[i] = (
                                entry.factory,
                                entry.deps,
                                (object[])entry.deps?.Clone(),
                                newCleanup
                            );
                        }
                        functionEffectRunCount++;
                    }
                }
            }
            // Passive effects phase
            if (functionComponentMetadata.FunctionEffects != null)
            {
                for (int i = 0; i < functionComponentMetadata.FunctionEffects.Count; i++)
                {
                    var entry = functionComponentMetadata.FunctionEffects[i];
                    bool shouldRun =
                        entry.lastDeps == null || DepsChangedInternal(entry.lastDeps, entry.deps);
                    if (shouldRun)
                    {
                        bool firstRun = entry.lastDeps == null;
                        // Pre-stamp lastDeps to avoid duplicate scheduling across rapid renders
                        functionComponentMetadata.FunctionEffects[i] = (
                            entry.factory,
                            entry.deps,
                            (object[])entry.deps?.Clone(),
                            entry.cleanup
                        );
                        var schedulerC = ResolveScheduler();
                        if (!firstRun && schedulerC != null)
                        {
                            schedulerC.EnqueueBatchedEffect(() =>
                            {
                                try
                                {
                                    entry.cleanup?.Invoke();
                                }
                                catch { }
                                Action newCleanup = null;
                                try
                                {
                                    newCleanup = entry.factory?.Invoke();
                                }
                                catch { }
                                if (i < functionComponentMetadata.FunctionEffects.Count)
                                {
                                    functionComponentMetadata.FunctionEffects[i] = (
                                        entry.factory,
                                        entry.deps,
                                        (object[])entry.deps?.Clone(),
                                        newCleanup
                                    );
                                }
                                functionEffectRunCount++;
                            });
                        }
                        else
                        {
                            try
                            {
                                entry.cleanup?.Invoke();
                            }
                            catch { }
                            Action newCleanup = null;
                            try
                            {
                                newCleanup = entry.factory?.Invoke();
                            }
                            catch { }
                            if (i < functionComponentMetadata.FunctionEffects.Count)
                            {
                                functionComponentMetadata.FunctionEffects[i] = (
                                    entry.factory,
                                    entry.deps,
                                    (object[])entry.deps?.Clone(),
                                    newCleanup
                                );
                            }
                            functionEffectRunCount++;
                        }
                    }
                }
            }
            // Flush one pending update if requested during render
            if (functionComponentMetadata.PendingUpdate)
            {
                functionComponentMetadata.PendingUpdate = false;
                var sched = ResolveScheduler();
                if (EnableDiffTracing || TraceLevel == DiffTraceLevel.Verbose)
                {
                    try
                    {
                        UnityEngine.Debug.Log(
                            "[FuncRender:flush-pending] key=" + functionComponentMetadata.Key
                        );
                    }
                    catch { }
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
                try
                {
                    UnityEngine.Debug.Log("[FuncRender:exit] key=" + functionComponentMetadata.Key);
                }
                catch { }
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
                        try
                        {
                            Props.PropsApplier.NotifyElementRemoved(element);
                        }
                        catch { }
                        try
                        {
                            // Local remove mirrors RemoveEvent logic
                            if (
                                eventPropName == "onClick"
                                && wrapper is EventCallback<ClickEvent> clickCb
                            )
                                element.UnregisterCallback(clickCb);
                            else if (
                                eventPropName == "onPointerDown"
                                && wrapper is EventCallback<PointerDownEvent> pd
                            )
                                element.UnregisterCallback(pd);
                            else if (
                                eventPropName == "onPointerUp"
                                && wrapper is EventCallback<PointerUpEvent> pu
                            )
                                element.UnregisterCallback(pu);
                            else if (
                                eventPropName == "onPointerMove"
                                && wrapper is EventCallback<PointerMoveEvent> pm
                            )
                                element.UnregisterCallback(pm);
                            else if (
                                eventPropName == "onPointerEnter"
                                && wrapper is EventCallback<PointerEnterEvent> pe
                            )
                                element.UnregisterCallback(pe);
                            else if (
                                eventPropName == "onPointerLeave"
                                && wrapper is EventCallback<PointerLeaveEvent> pl
                            )
                                element.UnregisterCallback(pl);
                            else if (
                                eventPropName == "onWheel"
                                && wrapper is EventCallback<WheelEvent> we
                            )
                                element.UnregisterCallback(we);
                            else if (
                                eventPropName == "onFocus"
                                && wrapper is EventCallback<FocusEvent> fe
                            )
                                element.UnregisterCallback(fe);
                            else if (
                                eventPropName == "onBlur"
                                && wrapper is EventCallback<BlurEvent> be
                            )
                                element.UnregisterCallback(be);
                            else if (
                                eventPropName == "onKeyDown"
                                && wrapper is EventCallback<KeyDownEvent> kd
                            )
                                element.UnregisterCallback(kd);
                            else if (
                                eventPropName == "onKeyUp"
                                && wrapper is EventCallback<KeyUpEvent> ku
                            )
                                element.UnregisterCallback(ku);
                            else if (eventPropName == "onChange")
                            {
                                if (wrapper is EventCallback<ChangeEvent<string>> chs)
                                    element.UnregisterCallback(chs);
                                if (wrapper is EventCallback<ChangeEvent<bool>> chb)
                                    element.UnregisterCallback(chb);
                                if (wrapper is EventCallback<ChangeEvent<int>> chi)
                                    element.UnregisterCallback(chi);
                            }
                            else if (
                                eventPropName == "onInput"
                                && wrapper is EventCallback<InputEvent> ie
                            )
                                element.UnregisterCallback(ie);
#if UNITY_EDITOR
                            else if (
                                eventPropName == "onDragEnter"
                                && wrapper is EventCallback<DragEnterEvent> de
                            )
                                element.UnregisterCallback(de);
                            else if (
                                eventPropName == "onDragLeave"
                                && wrapper is EventCallback<DragLeaveEvent> dle
                            )
                                element.UnregisterCallback(dle);
#endif
                            else if (
                                eventPropName == "onScroll"
                                && wrapper is EventCallback<WheelEvent> se
                            )
                                element.UnregisterCallback(se);
                        }
                        catch { }
                    }
                    catch { }
                }
                metadata.EventHandlers.Clear();
                if (metadata.EventHandlerTargets != null)
                    metadata.EventHandlerTargets.Clear();
                if (metadata.EventHandlerSignatures != null)
                    metadata.EventHandlerSignatures.Clear();
            }
            // No class component cleanup needed
            if (metadata.FunctionEffects != null)
            {
                foreach (var effect in metadata.FunctionEffects)
                {
                    try
                    {
                        effect.cleanup?.Invoke();
                    }
                    catch { }
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
                    catch { }
                }
                metadata.FunctionLayoutEffects.Clear();
            }
        }

        public (
            int reconciled,
            int skipped,
            int effects,
            int portalsBuilt,
            int portalsUpdated,
            long lastDiffMs
        ) GetMetrics() =>
            (
                reconciledNodeCount,
                skippedNodeCount,
                functionEffectRunCount,
                portalBuildCount,
                portalUpdateCount,
                lastDiffDurationMs
            );

        public (
            int cacheHits,
            int cacheMisses,
            Dictionary<VirtualNodeType, int> counts
        ) GetExtendedMetrics() =>
            (
                cacheHitCount,
                cacheMissCount,
                new Dictionary<VirtualNodeType, int>(nodeTypeBuildCounts)
            );

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
