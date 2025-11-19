using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ReactiveUITK.Core.Util;
using ReactiveUITK.Elements;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

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

        
        private static void GetManagedChildren(VisualElement parent, List<VisualElement> buffer)
        {
            buffer.Clear();
            for (int i = 0; i < parent.childCount; i++)
            {
                var ve = parent.ElementAt(i);
                if (ve.userData is NodeMetadata)
                {
                    buffer.Add(ve);
                }
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
        public static bool WarnOnMixedKeySiblings = false;
        public static bool UseExceptionBoundaryFlow = false;

        private int reconciledNodeCount;
        private int skippedNodeCount;
        private int functionEffectRunCount;
        private int portalBuildCount;
        private int portalUpdateCount;
        private Stopwatch diffStopwatch = new();
        private long lastDiffDurationMs; 

        

        private readonly Dictionary<VirtualNodeType, int> nodeTypeBuildCounts = new();

        private static int metricsSampleInterval = 10;
        private static int metricsMinIntervalMs = 200;

        public static int MetricsSampleInterval
        {
            get => metricsSampleInterval;
            set => metricsSampleInterval = Math.Max(1, value);
        }

        public static int MetricsMinIntervalMs
        {
            get => metricsMinIntervalMs;
            set => metricsMinIntervalMs = Math.Max(0, value);
        }

        private int metricsSampleCounter;
        private long lastMetricsEmitTimestamp;

        
        public readonly struct ReconcilerMetrics
        {
            public readonly long LastDiffMs;
            public readonly int Reconciled;
            public readonly int Skipped;
            public readonly int EffectsRan;
            public readonly int PortalsBuilt;
            public readonly int PortalsUpdated;
            public readonly int BatchedComponentUpdates;

            public ReconcilerMetrics(
                long lastDiffMs,
                int reconciled,
                int skipped,
                int effectsRan,
                int portalsBuilt,
                int portalsUpdated,
                int batchedComponentUpdates
            )
            {
                LastDiffMs = lastDiffMs;
                Reconciled = reconciled;
                Skipped = skipped;
                EffectsRan = effectsRan;
                PortalsBuilt = portalsBuilt;
                PortalsUpdated = portalsUpdated;
                BatchedComponentUpdates = batchedComponentUpdates;
            }
        }

        public static event Action<ReconcilerMetrics> MetricsEmitted;

        
        private static readonly ProfilerMarker DiffNodeMarker = new ProfilerMarker(
            "ReactiveUITK.DiffNode"
        );
        private static readonly ProfilerMarker RenderFunctionComponentMarker = new ProfilerMarker(
            "ReactiveUITK.RenderFunctionComponent"
        );

        
        public Reconciler(HostContext hostContext)
        {
            this.hostContext = hostContext;
        }

        
        internal void ForceFunctionComponentUpdate(NodeMetadata metadata)
        {
            if (metadata == null || metadata.FuncRender == null || metadata.Container == null)
            {
                return;
            }
            try
            {
                Hooks.FlushQueuedStateUpdates(metadata);
                var state = metadata.ComponentState ?? metadata.EnsureComponentState();
                if (state != null)
                {
                    state.PendingUpdate = false;
                    state.HookIndex = 0;
                    metadata.SyncComponentState(state);
                }
                RenderFunctionComponent(metadata, metadata.Container, restoreAncestorContext: true);
            }
            catch (Exception ex)
            {
                try
                {
                    Debug.LogWarning($"ReactiveUITK: Force update failed: {ex}");
                }
                catch
                {
                }
            }
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
                    BuildSubtree(hostElement, nextRoot); 
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
            if (childNodes == null || parentElement == null)
            {
                return;
            }
            bool duplicateFragmentKeyWarned = false;
            HashSet<string> fragmentKeys = new();
            bool anyKeyed = false;
            bool anyUnkeyed = false;
            for (int index = 0; index < childNodes.Count; index++)
            {
                var currentChild = childNodes[index];
                if (currentChild == null)
                {
                    continue; 
                }
                if (string.IsNullOrEmpty(currentChild.Key))
                {
                    anyUnkeyed = true;
                }
                else
                {
                    anyKeyed = true;
                }
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

            
            if (WarnOnMixedKeySiblings && anyKeyed && anyUnkeyed)
            {
                try
                {
                    int pid = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(
                        parentElement
                    );
                    _warnedMixedKeyParents ??= new HashSet<int>();
                    if (_warnedMixedKeyParents.Add(pid))
                    {
                        UnityEngine.Debug.LogWarning(
                            $"ReactiveUITK: Mixed keyed and unkeyed siblings under '{parentElement.name}'. Add keys to all dynamic items for stable state."
                        );
                    }
                }
                catch
                {
                }
            }
        }

        private static HashSet<int> _warnedMixedKeyParents;
        private static HashSet<int> _warnedMissingElementTypes;

        

        private void BuildNode(VisualElement parentElement, VirtualNode virtualNode)
        {
            if (parentElement == null || virtualNode == null)
            {
                return; 
            }
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
                    var portalMetadata = portalPlaceholderElement.userData as NodeMetadata;
                    AttachPortalTarget(portalMetadata, virtualNode.PortalTarget);
                    virtualNode.PortalTarget.Clear();
                    BuildChildren(virtualNode.PortalTarget, virtualNode.Children);
                    if (portalMetadata != null)
                    {
                        portalMetadata.PortalPreviousChildren = new List<VirtualNode>(
                            virtualNode.Children ?? Array.Empty<VirtualNode>()
                        );
                    }
                    portalBuildCount++;
                    return;
                
                case VirtualNodeType.FunctionComponent when virtualNode.FunctionRender != null:
                    string funcName = virtualNode.FunctionRender.Method.Name;
                    
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
                        FuncPropTypes = virtualNode.PropTypes,
                        Container = functionComponentContainer,
                        HostContext = hostContext,
                        Reconciler = this,
                        IsFlattened = false,
                    };
                    functionComponentMetadata.ComponentState = new FunctionComponentState(
                        functionComponentMetadata
                    );
                    functionComponentContainer.userData = functionComponentMetadata;
                    parentElement.Add(functionComponentContainer);
                    RenderFunctionComponent(functionComponentMetadata);
                    return;
                case VirtualNodeType.ErrorBoundary:
                    VisualElement errorBoundaryElement = CreateErrorBoundaryElement(virtualNode);
                    parentElement.Add(errorBoundaryElement);
                    return;
                case VirtualNodeType.Suspense:
                {
                    VisualElement suspenseContainerElement = new VisualElement();
                    var suspenseMetadata = new NodeMetadata
                    {
                        Key = virtualNode.Key,
                        Container = suspenseContainerElement,
                        HostContext = hostContext,
                        Reconciler = this,
                    };
                    suspenseContainerElement.userData = suspenseMetadata;
                    try
                    {
                        RenderSuspenseNode(suspenseContainerElement, suspenseMetadata, virtualNode);
                    }
                    catch (SuspenseSuspendException)
                    {
                        RenderSuspenseFallbackContents(
                            suspenseContainerElement,
                            suspenseMetadata,
                            virtualNode
                        );
                    }
                    parentElement.Add(suspenseContainerElement);
                    return;
                }
            }
            if (string.IsNullOrWhiteSpace(virtualNode.ElementTypeName))
            {
                try
                {
                    int nodeId = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(
                        virtualNode
                    );
                    _warnedMissingElementTypes ??= new HashSet<int>();
                    if (_warnedMissingElementTypes.Add(nodeId))
                    {
                        Debug.LogWarning(
                            $"ReactiveUITK: Element node missing type. Key='{virtualNode.Key ?? "<null>"}'. Rendering fallback container."
                        );
                    }
                }
                catch
                {
                }

                VisualElement missingElementFallback = new()
                {
                    name = string.IsNullOrEmpty(virtualNode.Key)
                        ? "UnknownElement"
                        : ($"Unknown_{virtualNode.Key}"),
                    userData = new NodeMetadata { Key = virtualNode.Key },
                };
                parentElement.Add(missingElementFallback);
                BuildChildren(missingElementFallback, virtualNode.Children);
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
            }
        }

        private void DiffChildrenByKey(
            VisualElement parentElement,
            IReadOnlyList<VirtualNode> previousChildren,
            IReadOnlyList<VirtualNode> nextChildren
        )
        {
            if (parentElement == null)
            {
                return;
            }
            previousChildren ??= Array.Empty<VirtualNode>();
            nextChildren ??= Array.Empty<VirtualNode>();
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
                if (prevNode == null || prevElement == null)
                {
                    continue;
                }
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
                if (nextChildNode == null)
                {
                    continue; 
                }
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
                        {
                            resolved = replacement;
                        }
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
                        VisualElement resolved;
                        if (pv == null)
                        {
                            
                            BuildNode(parentElement, nextChildNode);
                            resolved = parentElement.ElementAt(parentElement.childCount - 1);
                        }
                        else
                        {
                            DiffNode(pe, pv, nextChildNode);
                            resolved = pe;
                        }
                        if (resolved.parent != parentElement)
                        {
                            
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

            
            var managedAfter = new List<VisualElement>(
                Math.Max(managed.Count, orderedElements.Count)
            );
            GetManagedChildren(parentElement, managedAfter);
            for (int i = managedAfter.Count - 1; i >= 0; i--)
            {
                var existing = managedAfter[i]; 
                if (existing == null)
                {
                    continue;
                }
                var md = existing.userData as NodeMetadata;
                if (md == null)
                {
                    continue;
                }
                bool keep =
                    (!string.IsNullOrEmpty(md.Key) && reusedKeys.Contains(md.Key))
                    || reusedElements.Contains(existing);

                if (!keep)
                {
                    RunRemovalCleanup(existing);
                    existing.RemoveFromHierarchy();
                }
            }

            var stableElements = ComputeStableElementSet(parentElement, orderedElements);
            VisualElement anchor = null;

            for (int i = 0; i < orderedElements.Count; i++)
            {
                var element = orderedElements[i];
                if (element == null)
                {
                    continue;
                }

                bool alreadyParented = element.parent == parentElement;
                bool shouldStay = alreadyParented && stableElements.Contains(element);

                if (alreadyParented && shouldStay)
                {
                    anchor = element;
                    continue;
                }

                InsertElementRelative(parentElement, element, anchor);
                anchor = element;
            }
        }

        private static HashSet<VisualElement> ComputeStableElementSet(
            VisualElement parentElement,
            List<VisualElement> orderedElements
        )
        {
            var existing = new List<VisualElement>();
            var positions = new List<int>();
            for (int i = 0; i < orderedElements.Count; i++)
            {
                var element = orderedElements[i];
                if (element == null || element.parent != parentElement)
                {
                    continue;
                }
                existing.Add(element);
                positions.Add(parentElement.IndexOf(element));
            }

            var stableElements = new HashSet<VisualElement>();
            if (existing.Count == 0)
            {
                return stableElements;
            }

            var lisIndexes = ComputeLisPositions(positions);
            foreach (var idx in lisIndexes)
            {
                if (idx >= 0 && idx < existing.Count)
                {
                    stableElements.Add(existing[idx]);
                }
            }

            return stableElements;
        }

        private static void InsertElementRelative(
            VisualElement parentElement,
            VisualElement element,
            VisualElement anchor
        )
        {
            if (element == null)
            {
                return;
            }

            void InsertAt(int index)
            {
                element.RemoveFromHierarchy();
                if (index < 0 || index >= parentElement.childCount)
                {
                    parentElement.Add(element);
                }
                else
                {
                    parentElement.Insert(index, element);
                }
            }

            if (anchor == null)
            {
                var firstManaged = GetFirstManagedChild(parentElement);
                if (firstManaged == null)
                {
                    element.RemoveFromHierarchy();
                    parentElement.Add(element);
                }
                else
                {
                    var targetIndex = parentElement.IndexOf(firstManaged);
                    var currentIndex =
                        element.parent == parentElement ? parentElement.IndexOf(element) : -1;
                    if (currentIndex != targetIndex)
                    {
                        InsertAt(Math.Max(0, targetIndex));
                    }
                }
                return;
            }

            var anchorIndex = parentElement.IndexOf(anchor);
            if (anchorIndex < 0)
            {
                element.RemoveFromHierarchy();
                parentElement.Add(element);
                return;
            }

            var desiredIndex = anchorIndex + 1;
            var existingIndex =
                element.parent == parentElement ? parentElement.IndexOf(element) : -1;
            if (existingIndex != desiredIndex)
            {
                InsertAt(desiredIndex);
            }
        }

        private static VisualElement GetFirstManagedChild(VisualElement parent)
        {
            if (parent == null)
            {
                return null;
            }
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.ElementAt(i);
                if (child?.userData is NodeMetadata)
                {
                    return child;
                }
            }
            return null;
        }

        private VisualElement CreateDetached(VirtualNode virtualNode)
        {
            IncrementNodeType(virtualNode.NodeType);

            switch (virtualNode.NodeType)
            {
                case VirtualNodeType.Text:
                {
                    var detachedTextLabel = new Label(virtualNode.TextContent ?? string.Empty)
                    {
                        userData = new NodeMetadata { Key = virtualNode.Key },
                    };
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
                        Container = functionComponentContainer,
                        HostContext = hostContext,
                        Reconciler = this,
                        IsFlattened = false,
                    };
                    wrapperMetadata.ComponentState = new FunctionComponentState(wrapperMetadata);
                    functionComponentContainer.userData = wrapperMetadata;

                    RenderFunctionComponent(wrapperMetadata);
                    return functionComponentContainer;
                }

                case VirtualNodeType.ErrorBoundary:
                {
                    var errorBoundaryElement = CreateErrorBoundaryElement(virtualNode);
                    return errorBoundaryElement;
                }

                case VirtualNodeType.Suspense:
                {
                    var suspenseContainerElement = new VisualElement();
                    var suspenseMetadata = new NodeMetadata
                    {
                        Key = virtualNode.Key,
                        Container = suspenseContainerElement,
                        HostContext = hostContext,
                        Reconciler = this,
                    };
                    suspenseContainerElement.userData = suspenseMetadata;
                    try
                    {
                        RenderSuspenseNode(suspenseContainerElement, suspenseMetadata, virtualNode);
                    }
                    catch (SuspenseSuspendException)
                    {
                        RenderSuspenseFallbackContents(
                            suspenseContainerElement,
                            suspenseMetadata,
                            virtualNode
                        );
                    }
                    return suspenseContainerElement;
                }
            }

            
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

            
            var childrenHost = elementAdapter?.ResolveChildHost(createdElement) ?? createdElement;
            if (childrenHost == null)
            {
                childrenHost = createdElement; 
            }

            BuildChildren(childrenHost, virtualNode.Children);

            return createdElement;
        }

        private VisualElement CreateErrorBoundaryElement(VirtualNode virtualNode)
        {
            var boundaryElement = new VisualElement
            {
                name = string.IsNullOrEmpty(virtualNode.Key)
                    ? "ErrorBoundary"
                    : ($"ErrorBoundary_{virtualNode.Key}"),
            };

            var metadata = new NodeMetadata
            {
                Key = virtualNode.Key,
                Container = boundaryElement,
                HostContext = hostContext,
                Reconciler = this,
                ErrorBoundaryResetKey = virtualNode.ErrorResetToken,
            };
            boundaryElement.userData = metadata;
            try
            {
                TryRenderErrorBoundaryInitial(boundaryElement, metadata, virtualNode);
            }
            catch (ErrorBoundaryCapturedException captured)
            {
                RenderErrorBoundaryFallbackContents(
                    boundaryElement,
                    virtualNode,
                    metadata,
                    captured.CapturedException,
                    captured.NotifyHandler,
                    captured.LogException
                );
            }
            return boundaryElement;
        }

        private void TryRenderErrorBoundaryInitial(
            VisualElement boundaryElement,
            NodeMetadata metadata,
            VirtualNode boundaryNode
        )
        {
            ClearHostElement(boundaryElement);
            metadata.ErrorBoundaryResetKey = boundaryNode.ErrorResetToken;
            metadata.ErrorBoundaryActive = false;
            metadata.ErrorBoundaryShowingFallback = false;
            metadata.ErrorBoundaryLastException = null;

            var children = boundaryNode.Children ?? Array.Empty<VirtualNode>();
            if (children.Count == 0)
            {
                return;
            }

            try
            {
                BuildChildren(boundaryElement, children);
            }
            catch (Exception ex)
            {
                ActivateErrorBoundary(
                    boundaryElement,
                    metadata,
                    boundaryNode,
                    ex,
                    notifyHandler: true,
                    logException: true
                );
            }
        }

        private void DiffErrorBoundary(
            VisualElement hostElement,
            NodeMetadata metadata,
            VirtualNode previousNode,
            VirtualNode nextNode
        )
        {
            bool resetRequested = !string.Equals(
                metadata.ErrorBoundaryResetKey,
                nextNode.ErrorResetToken,
                StringComparison.Ordinal
            );

            if (resetRequested)
            {
                metadata.ErrorBoundaryActive = false;
                metadata.ErrorBoundaryShowingFallback = false;
                metadata.ErrorBoundaryLastException = null;
            }

            if (metadata.ErrorBoundaryActive && !resetRequested)
            {
                metadata.ErrorBoundaryResetKey = nextNode.ErrorResetToken;
                ActivateErrorBoundary(
                    hostElement,
                    metadata,
                    nextNode,
                    metadata.ErrorBoundaryLastException,
                    notifyHandler: false,
                    logException: false
                );
                return;
            }

            var previousChildren = previousNode.Children ?? Array.Empty<VirtualNode>();
            var nextChildren = nextNode.Children ?? Array.Empty<VirtualNode>();

            bool shouldRebuild =
                resetRequested
                || metadata.ErrorBoundaryShowingFallback
                || hostElement.childCount == 0
                || previousChildren.Count == 0;

            try
            {
                if (shouldRebuild)
                {
                    ClearHostElement(hostElement);
                    if (nextChildren.Count > 0)
                    {
                        BuildChildren(hostElement, nextChildren);
                    }
                }
                else
                {
                    DiffChildren(hostElement, previousChildren, nextChildren);
                }

                metadata.ErrorBoundaryActive = false;
                metadata.ErrorBoundaryShowingFallback = false;
                metadata.ErrorBoundaryLastException = null;
                metadata.ErrorBoundaryResetKey = nextNode.ErrorResetToken;
            }
            catch (Exception ex)
            {
                ActivateErrorBoundary(
                    hostElement,
                    metadata,
                    nextNode,
                    ex,
                    notifyHandler: true,
                    logException: true
                );
            }
        }

        private void ActivateErrorBoundary(
            VisualElement hostElement,
            NodeMetadata metadata,
            VirtualNode boundaryNode,
            Exception exception,
            bool notifyHandler,
            bool logException
        )
        {
            metadata.ErrorBoundaryActive = true;
            metadata.ErrorBoundaryShowingFallback = true;
            metadata.ErrorBoundaryLastException = exception;
            metadata.ErrorBoundaryResetKey = boundaryNode.ErrorResetToken;

            if (UseExceptionBoundaryFlow)
            {
                throw new ErrorBoundaryCapturedException(exception, notifyHandler, logException);
            }

            RenderErrorBoundaryFallbackContents(
                hostElement,
                boundaryNode,
                metadata,
                exception,
                notifyHandler,
                logException
            );
        }

        private void RenderErrorBoundaryFallbackContents(
            VisualElement hostElement,
            VirtualNode boundaryNode,
            NodeMetadata metadata,
            Exception exception,
            bool notifyHandler,
            bool logException
        )
        {
            if (logException && exception != null)
            {
                try
                {
                    Debug.LogError($"ReactiveUITK: Error boundary captured exception: {exception}");
                }
                catch
                {
                }
            }

            ClearHostElement(hostElement);

            if (boundaryNode.ErrorFallback != null)
            {
                try
                {
                    BuildChildren(
                        hostElement,
                        new List<VirtualNode> { boundaryNode.ErrorFallback }
                    );
                }
                catch (Exception fallbackEx)
                {
                    try
                    {
                        Debug.LogError(
                            $"ReactiveUITK: Error boundary fallback render failed: {fallbackEx}"
                        );
                    }
                    catch
                    {
                    }
                }
            }

            bool handled = boundaryNode.ErrorFallback != null;

            if (notifyHandler && boundaryNode.ErrorHandler != null)
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
                        Debug.LogError($"ReactiveUITK: Error boundary handler threw: {handlerEx}");
                    }
                    catch
                    {
                    }
                }
            }

            if (!handled && exception != null)
            {
                throw exception;
            }
        }

        private void RenderSuspenseNode(
            VisualElement hostElement,
            NodeMetadata metadata,
            VirtualNode suspenseNode
        )
        {
            if (hostElement == null || metadata == null || suspenseNode == null)
            {
                return;
            }

            metadata.SuspenseState ??= new SuspenseRenderState();
            SuspenseRenderState suspenseState = metadata.SuspenseState;

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
                    Debug.LogWarning($"ReactiveUITK: Suspense ready function threw: {ex}");
                }
                catch
                {
                }
            }

            Task suspenderTask = suspenseNode.SuspenseReadyTask;
            if (suspenderTask == null && metadata.SuspensePendingTask != null)
            {
                suspenderTask = metadata.SuspensePendingTask;
            }

            if (!ready && suspenderTask == null && !readyEvaluatorProvided)
            {
                ready = true;
            }

            if (suspenderTask != null)
            {
                if (suspenderTask.IsCompleted)
                {
                    ready = EvaluateSuspenseTaskResult(suspenderTask);
                    metadata.SuspensePendingTask = null;
                }
                else if (!ready)
                {
                    RegisterPendingSuspenseTask(metadata, suspenderTask);
                }
            }

            bool renderFallback = !ready;

            if (renderFallback)
            {
                if (UseExceptionBoundaryFlow)
                {
                    throw new SuspenseSuspendException();
                }

                RenderSuspenseFallbackContents(hostElement, metadata, suspenseNode);
                return;
            }

            IReadOnlyList<VirtualNode> targetChildren =
                suspenseNode.Children ?? Array.Empty<VirtualNode>();

            ClearHostElement(hostElement);
            if (targetChildren.Count > 0)
            {
                BuildChildren(hostElement, targetChildren);
            }

            suspenseState.LastRenderedChildren = targetChildren;
            suspenseState.ShowingFallback = false;
        }

        private void RenderSuspenseFallbackContents(
            VisualElement hostElement,
            NodeMetadata metadata,
            VirtualNode suspenseNode
        )
        {
            if (hostElement == null || metadata == null || suspenseNode == null)
            {
                return;
            }

            IReadOnlyList<VirtualNode> fallbackChildren =
                suspenseNode.Fallback != null
                    ? new[] { suspenseNode.Fallback }
                    : Array.Empty<VirtualNode>();

            ClearHostElement(hostElement);
            if (fallbackChildren.Count > 0)
            {
                BuildChildren(hostElement, fallbackChildren);
            }

            metadata.SuspenseState ??= new SuspenseRenderState();
            metadata.SuspenseState.LastRenderedChildren = fallbackChildren;
            metadata.SuspenseState.ShowingFallback = true;
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
                        $"ReactiveUITK: Suspense task faulted: {suspenderTask.Exception}"
                    );
                }
                catch
                {
                }
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

        private void RegisterPendingSuspenseTask(NodeMetadata metadata, Task suspenderTask)
        {
            if (metadata == null || suspenderTask == null)
            {
                return;
            }

            if (ReferenceEquals(metadata.SuspensePendingTask, suspenderTask))
            {
                return;
            }

            metadata.SuspenseTaskLock ??= new object();
            metadata.SuspensePendingTask = suspenderTask;
            int version;
            lock (metadata.SuspenseTaskLock)
            {
                version = ++metadata.SuspenseTaskVersion;
            }

            SynchronizationContext syncContext = SynchronizationContext.Current;
            IScheduler scheduler = ResolveScheduler();

            suspenderTask.ContinueWith(
                _ =>
                {
                    void Publish()
                    {
                        bool shouldEnqueue;
                        lock (metadata.SuspenseTaskLock)
                        {
                            shouldEnqueue = metadata.SuspenseTaskVersion == version;
                            if (shouldEnqueue)
                            {
                                metadata.SuspensePendingTask = null;
                            }
                        }

                        if (!shouldEnqueue || metadata.Container == null)
                        {
                            return;
                        }

                        FrameBatcher.Enqueue(metadata);
                    }

                    if (scheduler != null)
                    {
                        try
                        {
                            scheduler.Enqueue(Publish, IScheduler.Priority.Normal);
                            return;
                        }
                        catch
                        {
                        }
                    }

                    if (syncContext != null)
                    {
                        try
                        {
                            syncContext.Post(static state => ((Action)state)(), (Action)Publish);
                            return;
                        }
                        catch
                        {
                        }
                    }

                    Publish();
                },
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default
            );
        }

        private void ClearHostElement(VisualElement hostElement)
        {
            if (hostElement == null)
            {
                return;
            }

            for (int i = hostElement.childCount - 1; i >= 0; i--)
            {
                var child = hostElement.ElementAt(i);
                bool managed = child.userData is NodeMetadata;
                RunRemovalCleanup(child);
                child.RemoveFromHierarchy();
            }
        }

        private void DiffNode(
            VisualElement hostElement,
            VirtualNode previousNode,
            VirtualNode nextNode
        )
        {
            using var diffScope = DiffNodeMarker.Auto();
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
                var portalMetadata = hostElement.userData as NodeMetadata;
                if (nextNode.PortalTarget != null)
                {
                    AttachPortalTarget(portalMetadata, nextNode.PortalTarget);
                    var previousPortalChildren =
                        portalMetadata?.PortalPreviousChildren ?? new List<VirtualNode>();
                    DiffChildren(nextNode.PortalTarget, previousPortalChildren, nextNode.Children);
                    if (portalMetadata != null)
                    {
                        portalMetadata.PortalPreviousChildren = new List<VirtualNode>(
                            nextNode.Children
                        );
                    }
                    portalUpdateCount++;
                }
                else
                {
                    ClearPortalTargetChildren(portalMetadata);
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

            if (nextNode.NodeType == VirtualNodeType.Suspense)
            {
                NodeMetadata suspenseMetadata = hostElement.userData as NodeMetadata;
                if (suspenseMetadata == null)
                {
                    ReplaceNode(hostElement, nextNode);
                    return;
                }
                try
                {
                    RenderSuspenseNode(hostElement, suspenseMetadata, nextNode);
                }
                catch (SuspenseSuspendException)
                {
                    RenderSuspenseFallbackContents(hostElement, suspenseMetadata, nextNode);
                }
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

            
            if (nextNode.NodeType == VirtualNodeType.FunctionComponent)
            {
                NodeMetadata functionMetadata = hostElement.userData as NodeMetadata;
                if (functionMetadata == null || functionMetadata.FuncRender == null)
                {
                    ReplaceNode(hostElement, nextNode);
                    return;
                }

                
                bool childrenEq = ShallowChildrenEqual(previousNode.Children, nextNode.Children);
                bool skip;
                if (nextNode.MemoCompare != null)
                {
                    bool compareResult = false;
                    try
                    {
                        compareResult = nextNode.MemoCompare(
                            functionMetadata.FuncProps,
                            nextNode.Properties
                        );
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning(
                            $"ReactiveUITK: MemoCompare threw for component key={nextNode.Key}: {ex}"
                        );
                        compareResult = false;
                    }
                    skip = compareResult && childrenEq;
                }
                else
                {
                    bool propsEq = ShallowPropsEqual(
                        functionMetadata.FuncProps,
                        nextNode.Properties
                    );
                    skip = propsEq && childrenEq;
                }

                if (skip)
                {
                    functionMetadata.InheritedContextFrame = hostContext.CaptureFrame();
                    return;
                }

                reconciledNodeCount++;
                functionMetadata.FuncProps = new Dictionary<string, object>(nextNode.Properties);
                functionMetadata.FuncChildren = nextNode.Children;
                functionMetadata.FuncPropTypes = nextNode.PropTypes;
                var functionState =
                    functionMetadata.ComponentState ?? functionMetadata.EnsureComponentState();
                if (functionState != null)
                {
                    functionState.HookIndex = 0;
                    functionMetadata.SyncComponentState(functionState);
                }
                RenderFunctionComponent(functionMetadata, hostElement);
                return;
            }

            if (nextNode.NodeType == VirtualNodeType.ErrorBoundary)
            {
                NodeMetadata errorBoundaryMetadata = hostElement.userData as NodeMetadata;
                if (errorBoundaryMetadata == null)
                {
                    ReplaceNode(hostElement, nextNode);
                    return;
                }
                try
                {
                    DiffErrorBoundary(hostElement, errorBoundaryMetadata, previousNode, nextNode);
                }
                catch (ErrorBoundaryCapturedException captured)
                {
                    RenderErrorBoundaryFallbackContents(
                        hostElement,
                        nextNode,
                        errorBoundaryMetadata,
                        captured.CapturedException,
                        captured.NotifyHandler,
                        captured.LogException
                    );
                }
                return;
            }

            
            if (nextNode.NodeType == VirtualNodeType.Element)
            {
                var elementAdapter = hostContext.ElementRegistry.Resolve(nextNode.ElementTypeName);

                
                var oldChildHost = elementAdapter?.ResolveChildHost(hostElement) ?? hostElement;

                
                var prevKids = previousNode.Children ?? Array.Empty<VirtualNode>();
                var nextKids = nextNode.Children ?? Array.Empty<VirtualNode>();
                bool bothEmpty = (prevKids.Count == 0 && nextKids.Count == 0);

                elementAdapter?.ApplyPropertiesDiff(
                    hostElement,
                    previousNode.Properties,
                    nextNode.Properties
                );

                
                var newChildHost = elementAdapter?.ResolveChildHost(hostElement) ?? hostElement;

                
                if (!ReferenceEquals(newChildHost, oldChildHost))
                {
                    
                    var buffer = new List<VisualElement>();
                    for (int i = 0; i < oldChildHost.childCount; i++)
                    {
                        var ch = oldChildHost.ElementAt(i);
                        if (ch.userData is NodeMetadata)
                        {
                            buffer.Add(ch);
                        }
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
            {
                return false;
            }
            if (ac == 0)
            {
                return true;
            }

            foreach (var kv in a)
            {
                if (!b.TryGetValue(kv.Key, out var bv))
                    return false;
                if (!Equals(kv.Value, bv))
                    return false; 
            }
            return true;
        }

        
        
        
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
            {
                return false;
            }
            if (ac == 0)
            {
                return true;
            }

            for (int i = 0; i < ac; i++)
            {
                var an = a[i];
                var bn = b[i];
                if (ReferenceEquals(an, bn))
                    continue;
                if (an == null || bn == null)
                {
                    return false;
                }

                
                if (an.NodeType != bn.NodeType)
                {
                    return false;
                }
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
            return true; 
        }

        private void ReplaceNode(VisualElement hostElement, VirtualNode nextNode)
        {
            VisualElement parentElement = hostElement.parent;
            if (parentElement == null)
            {
                return;
            }
            int hostIndex = parentElement.IndexOf(hostElement);
            if (EnableDiffTracing || TraceLevel != DiffTraceLevel.None)
            {
                try
                {
                    UnityEngine.Debug.Log(
                        "[ReplaceNode] parent="
                            + parentElement.name
                            + ", index="
                            + hostIndex
                            + ", nextType="
                            + nextNode.NodeType
                            + ", nextKey="
                            + nextNode.Key
                    );
                }
                catch
                {
                }
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
            VisualElement reuseContainer = null,
            bool restoreAncestorContext = false
        )
        {
            using (RenderFunctionComponentMarker.Auto())
            {
                VisualElement targetContainer =
                    reuseContainer ?? functionComponentMetadata.Container;
                if (targetContainer == null || functionComponentMetadata.FuncRender == null)
                {
                    return;
                }
                HostContext.ContextFrameHandle originalFrame = default;
                if (
                    restoreAncestorContext
                    && functionComponentMetadata.InheritedContextFrame.IsValid
                )
                {
                    originalFrame = hostContext.CaptureFrame();
                    hostContext.RestoreFrame(functionComponentMetadata.InheritedContextFrame);
                }
                else if (!restoreAncestorContext)
                {
                    functionComponentMetadata.InheritedContextFrame = hostContext.CaptureFrame();
                }
                Hooks.FlushQueuedStateUpdates(functionComponentMetadata);
                var componentState =
                    functionComponentMetadata.ComponentState
                    ?? functionComponentMetadata.EnsureComponentState();
                
                componentState.HookIndex = 0;
                componentState.EffectIndex = 0;
                componentState.LayoutEffectIndex = 0;
                functionComponentMetadata.PendingProvidedContext = null;
                
                if (componentState.HookOrderPrimed)
                {
                    componentState.HookOrderPrimed =
                        Hooks.EnableHookValidation
                        && componentState.HookOrderSignatures != null
                        && componentState.HookOrderSignatures.Count > 0;
                }
                HookContext.Current = componentState;
                if (
                    functionComponentMetadata.FuncPropTypes != null
                    && functionComponentMetadata.FuncPropTypes.Count > 0
                )
                {
                    string componentName =
                        functionComponentMetadata.FuncRender?.Method?.DeclaringType?.Name
                        ?? functionComponentMetadata.FuncRender?.Method?.Name
                        ?? "FunctionComponent";
                    PropTypeValidator.Validate(
                        componentName,
                        functionComponentMetadata.FuncProps,
                        functionComponentMetadata.FuncPropTypes
                    );
                }
                bool initialMount =
                    functionComponentMetadata.LastRenderedSubtree == null
                    || targetContainer.childCount == 0;
                VirtualNode nextSubtree = null;
                bool renderCompleted = false;
                IReadOnlyDictionary<string, object> providerSnapshot = null;
                HostContext.ContextFrameHandle providerHandle = default;
                bool providerApplied = false;
                try
                {
                    componentState.IsRendering = true;
                    if (EnableDiffTracing || TraceLevel == DiffTraceLevel.Verbose)
                    {
                        try
                        {
                            UnityEngine.Debug.Log(
                                "[FuncRender:enter] key="
                                    + functionComponentMetadata.Key
                                    + ", pending="
                                    + componentState.PendingUpdate
                            );
                        }
                        catch
                        {
                        }
                    }
                    nextSubtree = functionComponentMetadata.FuncRender(
                        functionComponentMetadata.FuncProps,
                        functionComponentMetadata.FuncChildren
                    );
                    providerSnapshot = SnapshotContext(
                        functionComponentMetadata.PendingProvidedContext
                    );
                    functionComponentMetadata.PendingProvidedContext = null;
                    if (providerSnapshot != null)
                    {
                        providerHandle = hostContext.PushProvider(
                            providerSnapshot,
                            ref functionComponentMetadata.ContextProviderId
                        );
                        providerApplied = true;
                    }
                    
                    try
                    {
                        if (nextSubtree == null)
                        {
                            
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
                            
                            VisualElement existingRootElement =
                                targetContainer.childCount > 0
                                    ? targetContainer.ElementAt(0)
                                    : null;
                            if (existingRootElement == null)
                            {
                                targetContainer.Clear();
                                BuildChildren(
                                    targetContainer,
                                    new List<VirtualNode> { nextSubtree }
                                );
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
                    finally
                    {
                        if (providerApplied)
                        {
                            hostContext.PopProvider(providerHandle);
                            providerApplied = false;
                        }
                    }
                    renderCompleted = true;
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError(
                        $"ReactiveUITK: Function component render failed: {ex}"
                    );
                    throw;
                }
                finally
                {
                    componentState.IsRendering = false;
                    HookContext.Current = null;
                    functionComponentMetadata.PendingProvidedContext = null;
                    if (renderCompleted && Hooks.EnableHookValidation)
                    {
                        componentState.HookOrderPrimed = true;
                    }
                    else if (!renderCompleted)
                    {
                        componentState.HookOrderPrimed = false;
                    }
                    if (restoreAncestorContext && originalFrame.IsValid)
                    {
                        hostContext.RestoreFrame(originalFrame);
                    }
                }
                functionComponentMetadata.SyncComponentState(componentState);
                if (!renderCompleted)
                {
                    return;
                }
                HandleContextNotifications(functionComponentMetadata, providerSnapshot);
                
                if (componentState.FunctionLayoutEffects != null)
                {
                    for (int i = 0; i < componentState.FunctionLayoutEffects.Count; i++)
                    {
                        var entry = componentState.FunctionLayoutEffects[i];
                        bool shouldRun =
                            entry.lastDeps == null
                            || DepsChangedInternal(entry.lastDeps, entry.deps);
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
                            if (i < componentState.FunctionLayoutEffects.Count)
                            {
                                componentState.FunctionLayoutEffects[i] = (
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
                
                if (componentState.FunctionEffects != null)
                {
                    for (int i = 0; i < componentState.FunctionEffects.Count; i++)
                    {
                        var entry = componentState.FunctionEffects[i];
                        bool shouldRun =
                            entry.lastDeps == null
                            || DepsChangedInternal(entry.lastDeps, entry.deps);
                        if (shouldRun)
                        {
                            
                            componentState.FunctionEffects[i] = (
                                entry.factory,
                                entry.deps,
                                (object[])entry.deps?.Clone(),
                                entry.cleanup
                            );
                            var schedulerC = ResolveScheduler();
                            int capturedIndex = i;
                            var capturedEntry = entry;
                            void RunEffect()
                            {
                                try
                                {
                                    capturedEntry.cleanup?.Invoke();
                                }
                                catch
                                {
                                }
                                Action newCleanup = null;
                                try
                                {
                                    newCleanup = capturedEntry.factory?.Invoke();
                                }
                                catch
                                {
                                }
                                if (capturedIndex < componentState.FunctionEffects.Count)
                                {
                                    componentState.FunctionEffects[capturedIndex] = (
                                        capturedEntry.factory,
                                        capturedEntry.deps,
                                        (object[])capturedEntry.deps?.Clone(),
                                        newCleanup
                                    );
                                }
                                functionEffectRunCount++;
                            }

                            if (schedulerC != null)
                            {
                                schedulerC.EnqueueBatchedEffect(RunEffect);
                            }
                            else
                            {
                                RunEffect();
                            }
                        }
                    }
                }
                
                if (componentState.PendingUpdate)
                {
                    componentState.PendingUpdate = false;
                    if (componentState.UpdateQueued)
                    {
                        return;
                    }
                    componentState.UpdateQueued = true;
                    var sched = ResolveScheduler();
                    void FlushPending()
                    {
                        try
                        {
                            componentState.HookIndex = 0;
                            ForceFunctionComponentUpdate(functionComponentMetadata);
                        }
                        finally
                        {
                            componentState.UpdateQueued = false;
                            functionComponentMetadata.SyncComponentState(componentState);
                        }
                    }
                    if (EnableDiffTracing || TraceLevel == DiffTraceLevel.Verbose)
                    {
                        try
                        {
                            UnityEngine.Debug.Log(
                                "[FuncRender:flush-pending] key=" + functionComponentMetadata.Key
                            );
                        }
                        catch
                        {
                        }
                    }
                    if (sched != null)
                    {
                        sched.Enqueue(FlushPending);
                    }
                    else
                    {
                        FlushPending();
                    }
                }
                else if (EnableDiffTracing || TraceLevel == DiffTraceLevel.Verbose)
                {
                    try
                    {
                        UnityEngine.Debug.Log(
                            "[FuncRender:exit] key=" + functionComponentMetadata.Key
                        );
                    }
                    catch
                    {
                    }
                }
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

        private static HashSet<int> ComputeLisPositions(IReadOnlyList<int> indices)
        {
            var result = new HashSet<int>();
            if (indices == null || indices.Count == 0)
            {
                return result;
            }

            int length = indices.Count;
            var predecessors = new int[length];
            var lisEnds = new int[length];
            int lisLength = 0;

            for (int i = 0; i < length; i++)
            {
                int value = indices[i];
                if (value < 0)
                {
                    predecessors[i] = -1;
                    continue;
                }

                int low = 0;
                int high = lisLength;
                while (low < high)
                {
                    int mid = (low + high) >> 1;
                    if (indices[lisEnds[mid]] < value)
                    {
                        low = mid + 1;
                    }
                    else
                    {
                        high = mid;
                    }
                }

                if (low == lisLength)
                {
                    lisLength++;
                }

                lisEnds[low] = i;
                predecessors[i] = low > 0 ? lisEnds[low - 1] : -1;
            }

            if (lisLength == 0)
            {
                return result;
            }

            int cursor = lisEnds[lisLength - 1];
            while (cursor >= 0)
            {
                result.Add(cursor);
                cursor = predecessors[cursor];
            }

            return result;
        }

        private void RunRemovalCleanup(VisualElement element)
        {
            NodeMetadata metadata = element?.userData as NodeMetadata;
            if (metadata == null)
            {
                return;
            }
            ClearPortalTargetChildren(metadata);
            DetachPortalTarget(metadata);
            try
            {
                metadata.HostContext?.UnregisterContextConsumer(metadata);
            }
            catch
            {
            }
            try
            {
                Hooks.DisposeSignalSubscriptions(metadata);
            }
            catch
            {
            }
            
            if (metadata.EventHandlers != null && metadata.EventHandlers.Count > 0)
            {
                var snapshot = new List<KeyValuePair<string, Delegate>>(metadata.EventHandlers);
                foreach (var kv in snapshot)
                {
                    try
                    {
                        string eventPropName = kv.Key;
                        Delegate wrapper = kv.Value;
                        
                        try
                        {
                            Props.PropsApplier.NotifyElementRemoved(element);
                        }
                        catch
                        {
                        }
                        try
                        {
                            
                            if (
                                eventPropName == "onClick"
                                && wrapper is EventCallback<ClickEvent> clickCb
                            )
                            {
                                element.UnregisterCallback(clickCb);
                            }
                            else if (
                                eventPropName == "onPointerDown"
                                && wrapper is EventCallback<PointerDownEvent> pd
                            )
                            {
                                element.UnregisterCallback(pd);
                            }
                            else if (
                                eventPropName == "onPointerUp"
                                && wrapper is EventCallback<PointerUpEvent> pu
                            )
                            {
                                element.UnregisterCallback(pu);
                            }
                            else if (
                                eventPropName == "onPointerMove"
                                && wrapper is EventCallback<PointerMoveEvent> pm
                            )
                            {
                                element.UnregisterCallback(pm);
                            }
                            else if (
                                eventPropName == "onPointerEnter"
                                && wrapper is EventCallback<PointerEnterEvent> pe
                            )
                            {
                                element.UnregisterCallback(pe);
                            }
                            else if (
                                eventPropName == "onPointerLeave"
                                && wrapper is EventCallback<PointerLeaveEvent> pl
                            )
                            {
                                element.UnregisterCallback(pl);
                            }
                            else if (
                                eventPropName == "onWheel"
                                && wrapper is EventCallback<WheelEvent> we
                            )
                            {
                                element.UnregisterCallback(we);
                            }
                            else if (
                                eventPropName == "onFocus"
                                && wrapper is EventCallback<FocusEvent> fe
                            )
                            {
                                element.UnregisterCallback(fe);
                            }
                            else if (
                                eventPropName == "onBlur"
                                && wrapper is EventCallback<BlurEvent> be
                            )
                            {
                                element.UnregisterCallback(be);
                            }
                            else if (
                                eventPropName == "onKeyDown"
                                && wrapper is EventCallback<KeyDownEvent> kd
                            )
                            {
                                element.UnregisterCallback(kd);
                            }
                            else if (
                                eventPropName == "onKeyUp"
                                && wrapper is EventCallback<KeyUpEvent> ku
                            )
                            {
                                element.UnregisterCallback(ku);
                            }
                            else if (eventPropName == "onChange")
                            {
                                if (wrapper is EventCallback<ChangeEvent<string>> chs)
                                {
                                    element.UnregisterCallback(chs);
                                }
                                if (wrapper is EventCallback<ChangeEvent<bool>> chb)
                                {
                                    element.UnregisterCallback(chb);
                                }
                                if (wrapper is EventCallback<ChangeEvent<int>> chi)
                                {
                                    element.UnregisterCallback(chi);
                                }
                            }
                            else if (
                                eventPropName == "onInput"
                                && wrapper is EventCallback<InputEvent> ie
                            )
                            {
                                element.UnregisterCallback(ie);
                            }
#if UNITY_EDITOR
                            else if (
                                eventPropName == "onDragEnter"
                                && wrapper is EventCallback<DragEnterEvent> de
                            )
                            {
                                element.UnregisterCallback(de);
                            }
                            else if (
                                eventPropName == "onDragLeave"
                                && wrapper is EventCallback<DragLeaveEvent> dle
                            )
                            {
                                element.UnregisterCallback(dle);
                            }
#endif
                            else if (
                                eventPropName == "onScroll"
                                && wrapper is EventCallback<WheelEvent> se
                            )
                            {
                                element.UnregisterCallback(se);
                            }
                        }
                        catch
                        {
                        }
                    }
                    catch
                    {
                    }
                }
                metadata.EventHandlers.Clear();
                if (metadata.EventHandlerTargets != null)
                {
                    metadata.EventHandlerTargets.Clear();
                }
                if (metadata.EventHandlerSignatures != null)
                {
                    metadata.EventHandlerSignatures.Clear();
                }
            }
            
            var state = metadata.ComponentState ?? metadata.EnsureComponentState();
            if (state?.FunctionEffects != null)
            {
                foreach (var effect in state.FunctionEffects)
                {
                    try
                    {
                        effect.cleanup?.Invoke();
                    }
                    catch
                    {
                    }
                }
                state.FunctionEffects.Clear();
            }
            if (state?.FunctionLayoutEffects != null)
            {
                foreach (var effect in state.FunctionLayoutEffects)
                {
                    try
                    {
                        effect.cleanup?.Invoke();
                    }
                    catch
                    {
                    }
                }
                state.FunctionLayoutEffects.Clear();
            }
            metadata.SyncComponentState(state);
        }

        private void AttachPortalTarget(NodeMetadata metadata, VisualElement target)
        {
            if (metadata == null)
            {
                return;
            }
            if (ReferenceEquals(metadata.PortalTarget, target))
            {
                return;
            }
            DetachPortalTarget(metadata);
            metadata.PortalTarget = target;
            if (target == null)
            {
                return;
            }
            EventCallback<DetachFromPanelEvent> handler = _ => ClearPortalTargetChildren(metadata);
            metadata.PortalDetachHandler = handler;
            metadata.PortalDetachWired = true;
            try
            {
                target.RegisterCallback(handler);
            }
            catch
            {
                metadata.PortalDetachWired = false;
                metadata.PortalDetachHandler = null;
            }
        }

        private void DetachPortalTarget(NodeMetadata metadata)
        {
            if (metadata == null)
            {
                return;
            }
            if (
                metadata.PortalDetachWired
                && metadata.PortalDetachHandler != null
                && metadata.PortalTarget != null
            )
            {
                try
                {
                    metadata.PortalTarget.UnregisterCallback(metadata.PortalDetachHandler);
                }
                catch
                {
                }
            }
            metadata.PortalDetachWired = false;
            metadata.PortalDetachHandler = null;
            metadata.PortalTarget = null;
        }

        private void ClearPortalTargetChildren(NodeMetadata metadata)
        {
            var target = metadata?.PortalTarget;
            if (target == null)
            {
                return;
            }
            for (int i = target.childCount - 1; i >= 0; i--)
            {
                var child = target.ElementAt(i);
                bool managed = child.userData is NodeMetadata;
                RunRemovalCleanup(child);
                child.RemoveFromHierarchy();
            }
            metadata?.PortalPreviousChildren?.Clear();
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

        public void BeginDiffTiming()
        {
            diffStopwatch.Reset();
            diffStopwatch.Start();
        }

        public void EndDiffTiming()
        {
            if (!diffStopwatch.IsRunning)
            {
                return;
            }

            diffStopwatch.Stop();
            lastDiffDurationMs = diffStopwatch.ElapsedMilliseconds;

            if (metricsSampleCounter < int.MaxValue)
            {
                metricsSampleCounter++;
            }

            if (metricsSampleCounter < metricsSampleInterval)
            {
                return;
            }

            long nowTicks = Stopwatch.GetTimestamp();
            bool allowEmit = metricsMinIntervalMs <= 0;

            if (!allowEmit)
            {
                if (lastMetricsEmitTimestamp == 0)
                {
                    allowEmit = true;
                }
                else
                {
                    double elapsedMs =
                        (nowTicks - lastMetricsEmitTimestamp) * 1000.0 / Stopwatch.Frequency;
                    allowEmit = elapsedMs >= metricsMinIntervalMs;
                }
            }

            if (!allowEmit)
            {
                metricsSampleCounter = Math.Min(metricsSampleCounter, metricsSampleInterval);
                return;
            }

            metricsSampleCounter = 0;
            lastMetricsEmitTimestamp = nowTicks;

            try
            {
                MetricsEmitted?.Invoke(
                    new ReconcilerMetrics(
                        lastDiffDurationMs,
                        reconciledNodeCount,
                        skippedNodeCount,
                        functionEffectRunCount,
                        portalBuildCount,
                        portalUpdateCount,
                        FrameBatcher.LastFlushComponentCount
                    )
                );
            }
            catch
            {
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

        private void HandleContextNotifications(
            NodeMetadata metadata,
            IReadOnlyDictionary<string, object> snapshot
        )
        {
            if (metadata == null)
            {
                return;
            }
            var previous = metadata.LastProvidedContextSnapshot;
            bool hadPrevious = previous != null && previous.Count > 0;
            bool hasNew = snapshot != null && snapshot.Count > 0;

            if (hasNew)
            {
                metadata.LastProvidedContextSnapshot = snapshot;
                if (!ContextValuesEqual(previous, snapshot))
                {
                    hostContext?.NotifyContextChanged(metadata.ContextProviderId, snapshot);
                }
            }
            else
            {
                metadata.LastProvidedContextSnapshot = null;
                if (hadPrevious)
                {
                    hostContext?.NotifyContextChanged(metadata.ContextProviderId, previous);
                }
            }
        }

        private static IReadOnlyDictionary<string, object> SnapshotContext(
            Dictionary<string, object> source
        )
        {
            if (source == null || source.Count == 0)
            {
                return null;
            }
            var copy = new Dictionary<string, object>(source.Count);
            foreach (var kv in source)
            {
                copy[kv.Key] = kv.Value;
            }
            return new ReadOnlyDictionary<string, object>(copy);
        }

        private static bool ContextValuesEqual(
            IReadOnlyDictionary<string, object> first,
            IReadOnlyDictionary<string, object> second
        )
        {
            if (ReferenceEquals(first, second))
            {
                return true;
            }
            if (first == null || second == null)
            {
                return false;
            }
            if (first.Count != second.Count)
            {
                return false;
            }
            foreach (var kv in first)
            {
                if (!second.TryGetValue(kv.Key, out var other))
                {
                    return false;
                }
                if (!Equals(kv.Value, other))
                {
                    return false;
                }
            }
            return true;
        }

        

        private sealed class ErrorBoundaryCapturedException : Exception
        {
            public Exception CapturedException { get; }
            public bool NotifyHandler { get; }
            public bool LogException { get; }

            public ErrorBoundaryCapturedException(
                Exception capturedException,
                bool notifyHandler,
                bool logException
            )
                : base("ReactiveUITK error boundary control flow")
            {
                CapturedException = capturedException;
                NotifyHandler = notifyHandler;
                LogException = logException;
            }
        }

        private sealed class SuspenseSuspendException : Exception
        {
            public SuspenseSuspendException()
                : base("ReactiveUITK suspense control flow") { }
        }
    }
}
