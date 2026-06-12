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

#if UNITY_EDITOR
        // Editor-only UIDocument host-rebuild tracking. In the editor Unity
        // silently replaces UIDocument.rootVisualElement on undo, asset swap,
        // disable/enable, HMR, and the 6.3 InspectorWindow selection storm
        // (UUM-127851) — hookless mutations with no callback to observe. When
        // Initialize(UIDocument, ...) is used we poll once per frame via
        // AnimationTicker and reparent the mounted fiber tree onto the new
        // root via VNodeHostRenderer.RetargetHost when the reference changes.
        //
        // Built players have none of these hookless swaps — every runtime
        // panel change is developer-initiated through their own code — so the
        // poll is compiled out of player builds entirely. Editor cost is one
        // ReferenceEquals per RootRenderer per frame.
        private UIDocument hostDocument;
        private System.Action hostDocumentTickUnsubscribe;

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
            UnsubscribeFromHostDocument();
#endif
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
        /// UIDocument-aware overload. In the <b>editor</b> this polls the
        /// document's <c>rootVisualElement</c> once per frame and reparents
        /// the mounted fiber tree onto the new root whenever Unity rebuilds
        /// the panel (undo, asset swap, disable/enable, HMR, and the 6.3
        /// <c>InspectorWindow</c> selection storm). Those are hookless,
        /// editor-only mutations with no callback to observe, so polling is
        /// the only correct detection mechanism; the cost is one reference
        /// compare per frame on a tick source already running.
        ///
        /// In <b>player builds</b> the poll is compiled out entirely: a
        /// running game has no hookless panel swaps (every runtime panel
        /// change is developer-initiated), so this overload simply seeds the
        /// initial root from <paramref name="hostDoc"/>, exactly like
        /// <see cref="Initialize(VisualElement, Action{HostContext})"/>. A
        /// build that deliberately rebuilds a UIDocument at runtime should
        /// re-call <see cref="Render"/> (or this overload) from the code that
        /// triggers the rebuild.
        /// </summary>
        public void Initialize(UIDocument hostDoc, Action<HostContext> env = null)
        {
            EnsureSetup();
            rootElement = hostDoc != null ? hostDoc.rootVisualElement : null;
            env?.Invoke(sharedHostContext);
#if UNITY_EDITOR
            UnsubscribeFromHostDocument();
            hostDocument = hostDoc;
            SubscribeToHostDocument();
#endif
        }

#if UNITY_EDITOR
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
#endif

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
#if UNITY_EDITOR
            UnsubscribeFromHostDocument();
#endif
            if (vnodeHostRenderer != null)
            {
                vnodeHostRenderer.Unmount();
                vnodeHostRenderer = null;
            }
        }
    }
}
