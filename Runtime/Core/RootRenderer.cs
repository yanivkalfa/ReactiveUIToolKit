using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Core.Diagnostics;
using ReactiveUITK.Elements;
using ReactiveUITK.Signals;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Core
{
    public sealed class RootRenderer : MonoBehaviour
    {
        public static RootRenderer Instance { get; private set; }
        private HostContext sharedHostContext;
        private ElementRegistry elementRegistry;
        private VisualElement rootElement;
        private VNodeHostRenderer vnodeHostRenderer;

        // UIDocument host-rebuild tracking. Unity rebuilds the panel
        // (replacing rootVisualElement) on undo, asset swap, disable/enable,
        // and the editor playmode selection storm. UIDocument exposes no
        // event for this, so when Initialize(UIDocument, ...) is used we
        // poll once per frame via AnimationTicker (a panel-independent
        // tick source already running for animations) and reparent the
        // mounted fiber tree onto the new root via VNodeHostRenderer
        // .RetargetHost when the reference changes.
        //
        // Cost is one ReferenceEquals per RootRenderer per frame (~3 ns);
        // the editor selection storm collapses to a single retarget thanks
        // to the dedupe gate.
        private UIDocument hostDocument;
        private System.Action hostDocumentTickUnsubscribe;

#if UNITY_EDITOR
        /// <summary>HMR: all active RootRenderer instances for multi-tree walking.</summary>
        private static readonly HashSet<RootRenderer> s_allInstances = new();
        internal static IEnumerable<RootRenderer> AllInstances => s_allInstances;

        /// <summary>HMR: exposes the VNodeHostRenderer for tree walking.</summary>
        internal VNodeHostRenderer VNodeHostRendererInternal => vnodeHostRenderer;
#endif

        private void EnsureSetup()
        {
            if (elementRegistry == null)
            {
                elementRegistry = ElementRegistryProvider.GetDefaultRegistry();
            }
            if (sharedHostContext == null)
            {
                if (RenderScheduler.Instance == null)
                {
                    var go = new GameObject("RenderScheduler");
                    go.hideFlags = HideFlags.DontSave;
                    go.AddComponent<RenderScheduler>();
                }
                SignalsRuntime.EnsureInitialized();
                sharedHostContext = new HostContext(elementRegistry);
                sharedHostContext.Environment["scheduler"] = RenderScheduler.Instance;
                sharedHostContext.Environment["isEditor"] = false;

                sharedHostContext.Environment["env"] = BuildDefinesConfig.ResolveEnvironment();

                // Initialize global diagnostics configuration from build defines.
                DiagnosticsConfig.CurrentTraceLevel = BuildDefinesConfig.ResolveTraceLevel();
                DiagnosticsConfig.EnableDiffTracing = BuildDefinesConfig.ResolveEnableDiffTracing();
                DiagnosticsConfig.UseExceptionBoundaryFlow =
                    BuildDefinesConfig.ResolveExceptionBoundaryFlow();

                // For now, drive internal logs off the verbose trace level.
                InternalLogOptions.EnableInternalLogs =
                    DiagnosticsConfig.CurrentTraceLevel == DiagnosticsConfig.TraceLevel.Verbose;
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
#if UNITY_EDITOR
            s_allInstances.Add(this);
#endif
            EnsureSetup();
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR
            s_allInstances.Remove(this);
#endif
            UnsubscribeFromHostDocument();
            if (Instance == this)
            {
                Instance = null;
            }
            Unmount();
        }

        /// <summary>
        /// Configures the root VisualElement and optionally seeds named environment slots
        /// (portal targets, feature flags, etc.) into the shared <see cref="HostContext"/>.
        /// Must be called before the first <see cref="Render"/> call.
        /// </summary>
        /// <param name="uiRootElement">The VisualElement that acts as the React root.</param>
        /// <param name="env">
        /// Optional callback invoked with the <see cref="HostContext"/> after built-in keys
        /// (scheduler, env, etc.) are set.  Use this to seed named portal target slots:
        /// <code>
        /// rootRenderer.Initialize(uiDoc.rootVisualElement,
        ///     env: ctx => ctx.Environment[PortalContextKeys.ModalRoot] = overlayLayer);
        /// </code>
        /// </param>
        public void Initialize(VisualElement uiRootElement, Action<HostContext> env = null)
        {
            EnsureSetup();
            rootElement = uiRootElement;
            env?.Invoke(sharedHostContext);
        }

        /// <summary>
        /// UIDocument-aware overload. Polls the document's
        /// <c>rootVisualElement</c> once per frame and reparents the
        /// mounted fiber tree onto the new root whenever Unity rebuilds
        /// the panel (undo, asset swap, disable/enable, scene reload,
        /// <c>panelSettings</c>/<c>visualTreeAsset</c> reassignment, and
        /// the editor-selection panel-rebuild storm in playmode). Without
        /// this the rendered UI vanishes after any of those events because
        /// the VisualElement that <see cref="Render"/> was first called
        /// against has been replaced by Unity.
        ///
        /// The retarget is dedupe-gated by reference-equality so the
        /// per-frame editor selection storm collapses to a single reparent.
        /// UIDocument exposes no panel-rebuild event, so polling is the
        /// only correct mechanism. The cost is one pointer compare per
        /// frame on a tick source that is already running.
        /// </summary>
        public void Initialize(UIDocument hostDoc, Action<HostContext> env = null)
        {
            EnsureSetup();
            UnsubscribeFromHostDocument();
            hostDocument = hostDoc;
            rootElement = hostDoc != null ? hostDoc.rootVisualElement : null;
            env?.Invoke(sharedHostContext);
            SubscribeToHostDocument();
        }

        private void SubscribeToHostDocument()
        {
            if (hostDocument == null || hostDocumentTickUnsubscribe != null)
            {
                return;
            }
            hostDocumentTickUnsubscribe = ReactiveUITK.Core.Animation.AnimationTicker.Subscribe(
                PollHostDocument
            );
        }

        private void UnsubscribeFromHostDocument()
        {
            if (hostDocumentTickUnsubscribe == null)
            {
                return;
            }
            hostDocumentTickUnsubscribe.Invoke();
            hostDocumentTickUnsubscribe = null;
        }

        private void PollHostDocument()
        {
            if (hostDocument == null)
            {
                UnsubscribeFromHostDocument();
                return;
            }
            var nextRoot = hostDocument.rootVisualElement;
            if (ReferenceEquals(nextRoot, rootElement))
            {
                return;
            }
            rootElement = nextRoot;
            if (nextRoot == null)
            {
                return;
            }
            // Move the live tree onto the freshly-built root. If we have
            // not yet rendered we just record the new root for the first
            // Render() call.
            if (vnodeHostRenderer != null)
            {
                vnodeHostRenderer.RetargetHost(nextRoot);
            }
        }

        public void Render(VirtualNode rootNode)
        {
            EnsureSetup();
            if (rootElement == null)
            {
                return;
            }
            if (vnodeHostRenderer == null)
            {
                vnodeHostRenderer = new VNodeHostRenderer(sharedHostContext, rootElement);
            }
            vnodeHostRenderer.Render(rootNode);
        }

        public void Unmount()
        {
            UnsubscribeFromHostDocument();
            if (vnodeHostRenderer != null)
            {
                vnodeHostRenderer.Unmount();
                vnodeHostRenderer = null;
            }
        }
    }
}
