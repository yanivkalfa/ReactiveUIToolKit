// ─────────────────────────────────────────────────────────────────────────────
//  Family — stable identity handle for a UITKX function component, modeled
//           on React Fast Refresh's "family" concept (react-refresh/runtime).
//
//  A Family is a single mutable cell whose `Current` field points at the
//  most recently registered render body for one component (identified by
//  the persistent `Id` string).
//
//  ── Lifecycle ────────────────────────────────────────────────────────────
//
//      ┌──────────────────────────────────────────────────────────────────┐
//      │ Initial load                                                     │
//      │   Assembly loads → [ModuleInitializer] fires →                   │
//      │   RefreshRuntime.Register("MyComp", MyComp.__Render_body, sig)   │
//      │   → Family created, Current = __Render_body                      │
//      └──────────────────────────────────────────────────────────────────┘
//      ┌──────────────────────────────────────────────────────────────────┐
//      │ Consumer wires up                                                │
//      │   Parent.cctor runs → __fam_MyComp = RefreshRuntime.GetFamily(...)│
//      │   → field holds reference to the SAME Family object              │
//      └──────────────────────────────────────────────────────────────────┘
//      ┌──────────────────────────────────────────────────────────────────┐
//      │ HMR cycle                                                        │
//      │   Recompiled HMR DLL loads → [ModuleInitializer] fires →         │
//      │   Register sees existing Family by Id → updates Current to point │
//      │   at the new __Render_body → every consumer's __fam_MyComp       │
//      │   reference is automatically routed to the new body              │
//      └──────────────────────────────────────────────────────────────────┘
//
//  ── Why this beats per-type field swapping ──────────────────────────────
//  Today's HMR walks every loaded assembly to find every prior hmr_*.dll
//  copy of MyComp and writes `MyComp.__hmr_Render = newBody` on each. With
//  Family, all consumers (regardless of which DLL generation baked their
//  IL) reference the same Family object — a single `Family.Current = body`
//  write reaches every consumer atomically. The cross-DLL identity bug
//  that motivated this refactor is impossible by construction.
//
//  ── Player builds ───────────────────────────────────────────────────────
//  This type and its registry are editor-only — the entire file is
//  excluded from player compilation via #if UNITY_EDITOR. The source
//  generator emits direct V.Func(MyComp.Render, ...) calls under
//  #if !UNITY_EDITOR, so player builds have zero trace of Family,
//  RefreshRuntime, or the ModuleInitializer Register call. This matches
//  React Fast Refresh's dev/prod split (Babel injects $RefreshReg$ in
//  dev only).
// ────────────────────────────────────────────────────────────────────────────────────────────────────

#if UNITY_EDITOR

using System;
using System.Collections.Generic;

namespace ReactiveUITK.Refresh
{
    /// <summary>
    /// Stable identity handle for a UITKX function component across HMR cycles.
    /// Modelled on React Fast Refresh's family. Use
    /// <see cref="RefreshRuntime.GetFamily"/> to obtain instances —
    /// constructors are intentionally not part of the public surface.
    /// </summary>
    public sealed class Family
    {
        /// <summary>
        /// Stable persistent identifier — currently the component's simple
        /// type name. Used as the key in the Family registry. Renaming a
        /// component naturally produces a new Family (i.e. a remount), which
        /// matches React's behaviour for renamed exports.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The live render body. Mutated in-place by
        /// <see cref="RefreshRuntime.Register"/> on every successful HMR
        /// compile. Consumers invoke this via <c>V.Func</c> overloads that
        /// store the Family on the vnode; the fiber reconciler resolves the
        /// actual delegate at render time by reading <c>Current</c>.
        ///
        /// Lazy resolution: if Register was called with a factory overload
        /// (the SG-emitted ModuleInitializer path), the factory is invoked
        /// on first read and the result cached. This defers the read of
        /// the component's <c>__Render_body</c> static field -- and thus
        /// the component type's <c>.cctor</c> -- until the first render
        /// pass, AFTER Unity's <c>[InitializeOnLoadMethod]</c> hooks have
        /// populated registries like <c>UitkxAssetRegistry</c>. Without
        /// this, eager resolution during ModuleInitializer would trigger
        /// the component <c>.cctor</c>, which runs user static field
        /// initializers (e.g. <c>static readonly Texture2D bg = Asset&lt;T&gt;(...)</c>)
        /// against an empty asset registry.
        /// </summary>
        public Func<Core.IProps, IReadOnlyList<Core.VirtualNode>, Core.VirtualNode> Current
        {
            get
            {
                if (_current == null && _factory != null)
                    _current = _factory();
                if (_current != null)
                    return _current;

                // No body and no factory means neither Register nor a
                // parent-supplied fallback (via GetFamily(id, factory))
                // ever ran. Emit a one-shot LogError naming this Family.Id
                // so the developer sees an actionable diagnostic in the
                // Console immediately, then return a per-instance throwing
                // placeholder whose exception message ALSO names this Id
                // (so even if the LogError is missed, the eventual throw
                // is self-describing). The placeholder delegate is cached
                // per Family instance to avoid per-render allocation.
                RefreshRuntime.WarnUnresolvedFamilyOnce(this);
                return _namedPlaceholder ??= NamedPlaceholderRender;
            }
            internal set
            {
                _current = value;
                // An eager assignment supersedes any pending factory so we
                // don't re-resolve to a stale snapshot on the next read.
                _factory = null;
            }
        }

        /// <summary>
        /// Install a fallback body factory IFF no body and no factory is
        /// already present. Called from <see cref="RefreshRuntime.GetFamily(string, Func{Func{Core.IProps, IReadOnlyList{Core.VirtualNode}, Core.VirtualNode}})"/>
        /// so parents can supply a direct `() =&gt; Child.Render` factory
        /// for hand-written components that don't have an SG-emitted
        /// `[ModuleInitializer]` Register call (e.g. router types in the
        /// ReactiveUITK package). A subsequent Register call still takes
        /// precedence and clears the factory.
        /// </summary>
        internal bool TrySetFallbackFactory(
            Func<Func<Core.IProps, IReadOnlyList<Core.VirtualNode>, Core.VirtualNode>> factory)
        {
            if (factory == null) return false;
            if (_current != null || _factory != null) return false;
            _factory = factory;
            return true;
        }

        private Func<Core.IProps, IReadOnlyList<Core.VirtualNode>, Core.VirtualNode> _current;
        internal Func<Func<Core.IProps, IReadOnlyList<Core.VirtualNode>, Core.VirtualNode>> _factory;

        // Per-instance placeholder delegate, lazily allocated on first
        // unresolved Current read. Cached so repeated reads (one per
        // V.Func vnode construction per render pass) don't re-allocate.
        private Func<Core.IProps, IReadOnlyList<Core.VirtualNode>, Core.VirtualNode> _namedPlaceholder;

        /// <summary>
        /// Hook-call-shape fingerprint emitted by the source generator
        /// (<c>[HookSignature]</c>). When this changes between Register
        /// calls, the reconciler force-remounts existing fibers — analogous
        /// to React's "incompatible edit" classification.
        /// Null when the SG opted not to emit a signature.
        /// </summary>
        public string Signature { get; internal set; }

        /// <summary>
        /// Family IDs of every custom hook this Family's body calls at
        /// first level. Populated by the SG/HMR emitter via the
        /// <c>customHookFamilyKeys</c> argument of
        /// <see cref="RefreshRuntime.Register(string, Func{Core.IProps, IReadOnlyList{Core.VirtualNode}, Core.VirtualNode}, string, string[])"/>
        /// or <see cref="RefreshRuntime.RegisterHook"/>. Used by Phase 3's
        /// transitive <c>FullSignature</c> walk to detect downstream hook
        /// signature changes — mirrors React Fast Refresh's
        /// <c>signature.getCustomHooks()</c> return value.
        /// Never null after construction; empty array when none.
        /// </summary>
        public string[] CustomHookFamilyKeys { get; internal set; } = Array.Empty<string>();

        /// <summary>
        /// True when this Family represents a custom hook (registered via
        /// <see cref="RefreshRuntime.RegisterHook"/>) rather than a render
        /// body. Hook families track <see cref="Signature"/> and
        /// <see cref="CustomHookFamilyKeys"/> for transitive change
        /// propagation but never have their <see cref="Current"/> invoked
        /// (hooks are called through their generated static trampoline,
        /// not via the Family). The reconciler skips hook families when
        /// walking fibers; <see cref="Current"/> never resolves for them
        /// so no unresolved-family warning fires.
        /// </summary>
        public bool IsHook { get; internal set; }

        /// <summary>
        /// Last-known-working render body, captured on each Register call
        /// for use by the render-crash rollback path
        /// (<see cref="RefreshRuntime.TryRollback"/>). One-shot: cleared
        /// after a successful rollback so a second crash falls through to
        /// the nearest ErrorBoundary instead of thrashing.
        /// </summary>
        internal Func<Core.IProps, IReadOnlyList<Core.VirtualNode>, Core.VirtualNode> Previous;

        internal Family(
            string id,
            Func<Core.IProps, IReadOnlyList<Core.VirtualNode>, Core.VirtualNode> current,
            string signature)
        {
            Id = id;
            _current = current;
            Signature = signature;
        }

        /// <summary>
        /// Lazy-factory constructor used by the SG-emitted ModuleInitializer
        /// path. The factory is invoked on first <see cref="Current"/> read
        /// rather than at Register time -- see the <see cref="Current"/>
        /// documentation for why this matters for asset-registry timing.
        /// </summary>
        internal Family(
            string id,
            Func<Func<Core.IProps, IReadOnlyList<Core.VirtualNode>, Core.VirtualNode>> factory,
            string signature)
        {
            Id = id;
            _factory = factory;
            Signature = signature;
        }

        /// <summary>
        /// Per-instance throwing placeholder returned from <see cref="Current"/>
        /// when no Register call and no fallback factory have published a
        /// body for this Family. The exception message includes
        /// <see cref="Id"/> so call-site stack traces point directly at the
        /// unresolved component. The companion one-shot
        /// <see cref="RefreshRuntime.WarnUnresolvedFamilyOnce"/> emits a
        /// Console error at the moment the placeholder is RESOLVED, which
        /// fires earlier than the throw and lists every unresolved Family
        /// per render pass.
        /// </summary>
        private Core.VirtualNode NamedPlaceholderRender(
            Core.IProps _,
            IReadOnlyList<Core.VirtualNode> __)
        {
            throw new InvalidOperationException(
                $"[ReactiveUITK Refresh] Family '{Id}' has no registered body. " +
                "Either no [ModuleInitializer] Register call ran for this " +
                "component (likely cause: it lives in a package without UITKX " +
                "source generation), or a parent component was emitted by a " +
                "pre-0.6.0 generator that did not supply a fallback factory. " +
                "Fix: regenerate parent components by touching their .uitkx " +
                "files, or restart Unity to force a full recompile."
            );
        }
    }
}

#endif // UNITY_EDITOR
