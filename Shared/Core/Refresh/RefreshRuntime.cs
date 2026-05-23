// ─────────────────────────────────────────────────────────────────────────────
//  RefreshRuntime — public surface modelled on React's react-refresh/runtime.
//
//  Five primitives (numbered to match the React originals):
//
//    P1  Register(id, body, signature)
//          react-refresh: `register(type, id)`
//          Called by SG-emitted [ModuleInitializer] in each component-bearing
//          assembly. Creates or updates the Family for `id`.
//
//    P2  (HookSignature attribute — SG-only; no runtime call)
//          react-refresh: `createSignatureFunctionForTransform`
//          The SG emits `[HookSignature("...")]` at compile time; the runtime
//          consumes it via the `signature` parameter of Register.
//
//    P3  GetFamily(id)
//          react-refresh: `resolveFamily(type)`
//          Called by SG-emitted `__fam_<TypeName>` static-readonly field
//          initializers. Returns the canonical Family for `id` (or creates a
//          placeholder if Register hasn't run yet).
//
//    P4  PerformRefresh()
//          react-refresh: `performReactRefresh`
//          Called by UitkxHmrController after a successful compile applies
//          new module statics. Walks every live renderer's fiber tree,
//          schedules re-render for fibers whose Family is dirty, and
//          force-remounts those whose signature changed.
//
//    P5  (not exposed as a separate primitive — see Family.Current setter)
//          react-refresh: `module.hot.accept`
//          The "accept" semantics are intrinsic: Register's in-place mutation
//          of Family.Current IS the accept callback.
//
//  Auxiliary:
//    *   TryRollback(family)
//          react-refresh has no direct analogue (React error boundaries
//          handle this differently). UITKX uses Family.Previous as a one-shot
//          rollback slot — the reconciler invokes TryRollback from its
//          render-crash catch path before falling through to ErrorBoundary.
//
//  ── Thread safety ────────────────────────────────────────────────────────
//  All public methods take the registry lock. Writes happen on the Unity
//  main thread (module initializers fire on assembly load, which the
//  Editor serialises; PerformRefresh runs synchronously from
//  UitkxHmrController). Reads from `__fam_X` field initializers also run
//  on the main thread (lazy cctor). The lock is bookkeeping for the rare
//  case of a future background asset import calling Register from a
//  worker thread.
// ─────────────────────────────────────────────────────────────────────────────

//  ── Player builds ────────────────────────────────────────────────────────
//  This entire file is excluded from player compilation via #if UNITY_EDITOR.
//  React Fast Refresh's runtime is dev-only; UITKX matches that split. The
//  source generator emits direct V.Func(MyComp.Render, ...) under
//  #if !UNITY_EDITOR so player builds carry no Refresh code or registry.
// ─────────────────────────────────────────────────────────────────────────────

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using ReactiveUITK.Core;

namespace ReactiveUITK.Refresh
{
    /// <summary>
    /// Process-wide registry of <see cref="Family"/> handles, keyed by stable
    /// component ID. The five public entry points mirror React Fast Refresh's
    /// runtime surface (`register`, `resolveFamily`, `performReactRefresh`,
    /// etc.) and are documented per-method below.
    /// </summary>
    public static class RefreshRuntime
    {
        // Persistent storage keyed by `id`. StringComparer.Ordinal because
        // IDs are mechanically generated (type names today) and case
        // differences would be a bug, not a fuzzy-match opportunity.
        private static readonly Dictionary<string, Family> s_families =
            new Dictionary<string, Family>(StringComparer.Ordinal);

        // Bookkeeping for PerformRefresh: families whose Current was updated
        // since the last PerformRefresh call. Cleared by PerformRefresh.
        private static readonly HashSet<Family> s_dirty = new HashSet<Family>();

        // Bookkeeping for PerformRefresh: subset of s_dirty whose Signature
        // changed — those fibers need a full state reset, not just a
        // re-render.
        private static readonly HashSet<Family> s_forceRemount = new HashSet<Family>();

        // Reverse-edge map: hook family Id → set of consumer family Ids
        // (components OR hooks) that called RegisterHook/Register naming
        // this hook in their `customHookFamilyKeys` argument. Populated on
        // every Register and RegisterHook call so the Phase 3 transitive
        // walk in PropagateHookSignatureChanges can find consumers when a
        // hook's signature changes. Single direction (hook → consumers)
        // because the only direction we ever query is "given this dirty
        // hook, which consumers must I invalidate?".
        private static readonly Dictionary<string, HashSet<string>> s_reverseEdges =
            new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

        // One-shot dedup set for WarnUnresolvedFamilyOnce. Keyed by
        // Family.Id (not the Family reference itself) so that a Family
        // created, resolved, evicted, and recreated under the same Id
        // does not re-spam the Console.
        private static readonly HashSet<string> s_warnedUnresolvedIds =
            new HashSet<string>(StringComparer.Ordinal);

        private static readonly object s_lock = new object();

        // ────────────────────────────────────────────────────────────────────
        //  Diagnostics — one-shot unresolved-family warning
        // ────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Emit a single <c>Debug.LogError</c> the first time
        /// <see cref="Family.Current"/> resolves to the throwing
        /// placeholder for a given <paramref name="fam"/>'s
        /// <see cref="Family.Id"/>. Called from the <c>Family.Current</c>
        /// getter at the moment of resolution, so the developer sees an
        /// actionable Console message BEFORE the eventual render-time
        /// throw. Subsequent reads against the same Id are silent (the
        /// log line would otherwise repeat once per V.Func vnode
        /// construction per render pass).
        ///
        /// The error text names the unresolved Id and points at the two
        /// known causes: a non-SG child whose parent was generated by a
        /// pre-0.6.0 SG (no fallback factory was emitted), and a missing
        /// `[ModuleInitializer]` companion class for an SG-generated
        /// component (likely a build / asmdef wiring problem).
        /// </summary>
        internal static void WarnUnresolvedFamilyOnce(Family fam)
        {
            if (fam == null) return;
            lock (s_lock)
            {
                if (!s_warnedUnresolvedIds.Add(fam.Id))
                    return;
            }

            UnityEngine.Debug.LogError(
                $"[ReactiveUITK Refresh] Unresolved Family '{fam.Id}'. " +
                "No [ModuleInitializer] Register call ran for this component " +
                "and no parent supplied a fallback factory. The next render " +
                "of any component that references this Family will throw. " +
                "Fix: if the component is hand-written (not .uitkx), regenerate " +
                "every parent that uses it (touch its .uitkx file or restart " +
                "Unity); if the component IS .uitkx, verify its asmdef compiled " +
                "successfully and that its generated companion class " +
                "(`{ComponentName}__UitkxRefresh`) is present in the Analyzers/ " +
                "output."
            );
        }

        // ────────────────────────────────────────────────────────────────────
        //  P1 — Register
        // ────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Register or update the canonical render body for the component
        /// identified by <paramref name="id"/>. Called from SG-emitted
        /// <c>[ModuleInitializer]</c> methods.
        ///
        /// First call for an <paramref name="id"/>: creates the Family.
        /// Subsequent calls (HMR): update the existing Family's
        /// <see cref="Family.Current"/> in place, capturing the previous
        /// body in <see cref="Family.Previous"/> for the render-crash
        /// rollback path. The Family object's reference identity is
        /// preserved — every consumer's <c>__fam_X</c> field continues to
        /// point at it.
        /// </summary>
        /// <param name="id">Stable component identifier — currently the
        /// component's simple type name (see Plans~/HMR_FAST_REFRESH_PLAN.md
        /// §5.0 for the rationale and known collision case).</param>
        /// <param name="body">The fresh <c>__Render_body</c> delegate.</param>
        /// <param name="signature">Optional hook-call-shape fingerprint
        /// (see <see cref="Family.Signature"/>). Pass null when no
        /// <c>[HookSignature]</c> was emitted.</param>
        public static void Register(
            string id,
            Func<IProps, IReadOnlyList<VirtualNode>, VirtualNode> body,
            string signature)
            => Register(id, body, signature, null);

        /// <summary>
        /// Phase-1 overload that also wires the
        /// <paramref name="customHookFamilyKeys"/> reverse-edge map. SG /
        /// HMR emitters of components that call custom hooks pass the
        /// list of hook family IDs they reference at first level; Phase 3
        /// uses this to propagate transitive hook-signature changes.
        /// </summary>
        public static void Register(
            string id,
            Func<IProps, IReadOnlyList<VirtualNode>, VirtualNode> body,
            string signature,
            string[] customHookFamilyKeys)
        {
            if (string.IsNullOrEmpty(id) || body == null)
                return;

            lock (s_lock)
            {
                if (!s_families.TryGetValue(id, out var f))
                {
                    f = new Family(id, body, signature);
                    f.CustomHookFamilyKeys = customHookFamilyKeys ?? Array.Empty<string>();
                    s_families[id] = f;
                    UpdateReverseEdges(id, oldKeys: null, newKeys: f.CustomHookFamilyKeys);
                    return;
                }

                // Update path — preserve Family identity, swap Current.
                f.Previous = f.Current;
                f.Current = body;
                bool sigChanged = !string.Equals(
                    f.Signature ?? string.Empty,
                    signature ?? string.Empty,
                    StringComparison.Ordinal);
                f.Signature = signature;
                var oldKeys = f.CustomHookFamilyKeys;
                var newKeys = customHookFamilyKeys ?? Array.Empty<string>();
                f.CustomHookFamilyKeys = newKeys;
                UpdateReverseEdges(id, oldKeys, newKeys);
                s_dirty.Add(f);
                if (sigChanged)
                    s_forceRemount.Add(f);
            }
        }

        /// <summary>
        /// Lazy-factory overload of <see cref="Register(string, Func{IProps, IReadOnlyList{VirtualNode}, VirtualNode}, string)"/>.
        /// Invoked by SG-emitted <c>[ModuleInitializer]</c> methods with a
        /// closure of the form <c>() =&gt; __Render_body</c>. The factory is
        /// stored on the Family and evaluated lazily on first
        /// <see cref="Family.Current"/> read.
        ///
        /// Why factory instead of direct delegate: reading
        /// <c>ComponentClass.__Render_body</c> from the ModuleInitializer
        /// would trigger <c>ComponentClass</c>'s <c>.cctor</c>, which runs
        /// user static field initializers such as
        /// <c>static readonly Texture2D bg = AssetHelpers.Asset&lt;Texture2D&gt;(...)</c>.
        /// At ModuleInitializer time, Unity has NOT yet populated
        /// <c>UitkxAssetRegistry</c> via its
        /// <c>[InitializeOnLoadMethod]</c>, so the asset lookup fails and
        /// the static field is permanently null. Wrapping the read in a
        /// lambda defers the field access -- and the cctor trigger --
        /// until the first render pass, by which point the editor's
        /// load-time hooks have all run.
        ///
        /// First call for <paramref name="id"/>: creates the Family with
        /// the factory; <see cref="Family.Current"/> resolves on first
        /// read. Subsequent calls (HMR): resolve the factory immediately
        /// (HMR runs long after editor init is complete, so eager
        /// resolution is safe) and treat as a normal Register update so
        /// rollback / dirty tracking work uniformly.
        /// </summary>
        public static void Register(
            string id,
            Func<Func<IProps, IReadOnlyList<VirtualNode>, VirtualNode>> bodyFactory,
            string signature)
            => Register(id, bodyFactory, signature, null);

        /// <summary>
        /// Phase-1 lazy-factory overload that also wires the
        /// <paramref name="customHookFamilyKeys"/> reverse-edge map.
        /// </summary>
        public static void Register(
            string id,
            Func<Func<IProps, IReadOnlyList<VirtualNode>, VirtualNode>> bodyFactory,
            string signature,
            string[] customHookFamilyKeys)
        {
            if (string.IsNullOrEmpty(id) || bodyFactory == null)
                return;

            lock (s_lock)
            {
                if (!s_families.TryGetValue(id, out var f))
                {
                    f = new Family(id, bodyFactory, signature);
                    f.CustomHookFamilyKeys = customHookFamilyKeys ?? Array.Empty<string>();
                    s_families[id] = f;
                    UpdateReverseEdges(id, oldKeys: null, newKeys: f.CustomHookFamilyKeys);
                    return;
                }

                // HMR update path -- resolve eagerly so Previous tracking
                // and the dirty / force-remount sets are accurate. By the
                // time an HMR compile runs the user code that triggers the
                // component .cctor has long since executed, so there's no
                // ordering hazard here.
                var body = bodyFactory();
                if (body == null)
                    return;
                f.Previous = f.Current;
                f.Current = body;
                bool sigChanged = !string.Equals(
                    f.Signature ?? string.Empty,
                    signature ?? string.Empty,
                    StringComparison.Ordinal);
                f.Signature = signature;
                var oldKeys = f.CustomHookFamilyKeys;
                var newKeys = customHookFamilyKeys ?? Array.Empty<string>();
                f.CustomHookFamilyKeys = newKeys;
                UpdateReverseEdges(id, oldKeys, newKeys);
                s_dirty.Add(f);
                if (sigChanged)
                    s_forceRemount.Add(f);
            }
        }

        // ────────────────────────────────────────────────────────────────────
        //  P1b — RegisterHook (Phase 1)
        // ────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Register or update the signature-only Family for a custom hook.
        /// Called from SG-emitted (and HMR-emitted) <c>[ModuleInitializer]</c>
        /// companions on every hook container assembly. The Family has no
        /// render body — hooks are invoked through their generated static
        /// trampoline, not via <see cref="Family.Current"/> — and exists
        /// purely to track <see cref="Family.Signature"/> changes so the
        /// Phase 3 transitive walk can propagate "this hook's shape changed"
        /// up to every consumer component.
        ///
        /// First call for <paramref name="id"/>: creates a hook Family
        /// (<see cref="Family.IsHook"/> = true). Subsequent calls (HMR):
        /// update <see cref="Family.Signature"/> and
        /// <see cref="Family.CustomHookFamilyKeys"/> in place, marking the
        /// Family dirty / force-remount as appropriate. <c>PerformRefresh</c>
        /// will not iterate hook families directly — it picks them up
        /// through the reverse-edge map when computing transitive
        /// invalidation of consumer components.
        /// </summary>
        /// <param name="id">Hook family ID — convention
        /// <c>{ContainerFQN}::{HookName}</c>,
        /// e.g. <c>"PrettyUi.UIHooks.UseUiDocumentSlotHooks::UseUiDocumentSlot"</c>.</param>
        /// <param name="signature">Hook-call-shape fingerprint extracted
        /// from the hook body source (same emitter as components).</param>
        /// <param name="customHookFamilyKeys">First-level custom hooks that
        /// this hook itself calls (a hook may call other hooks). Pass null
        /// when the hook only calls built-ins.</param>
        public static void RegisterHook(
            string id,
            string signature,
            string[] customHookFamilyKeys = null)
        {
            if (string.IsNullOrEmpty(id))
                return;

            lock (s_lock)
            {
                if (!s_families.TryGetValue(id, out var f))
                {
                    f = new Family(
                        id,
                        current: (Func<IProps, IReadOnlyList<VirtualNode>, VirtualNode>)null,
                        signature: signature);
                    f.IsHook = true;
                    f.CustomHookFamilyKeys = customHookFamilyKeys ?? Array.Empty<string>();
                    s_families[id] = f;
                    UpdateReverseEdges(id, oldKeys: null, newKeys: f.CustomHookFamilyKeys);
                    return;
                }

                // Existing Family — should also be IsHook=true, but tolerate
                // the case where a stale component Family with the same id
                // somehow precedes us (would only happen via developer error
                // naming collision; we promote it to hook semantics rather
                // than throw, which is the least-surprising recovery).
                f.IsHook = true;
                bool sigChanged = !string.Equals(
                    f.Signature ?? string.Empty,
                    signature ?? string.Empty,
                    StringComparison.Ordinal);
                f.Signature = signature;
                var oldKeys = f.CustomHookFamilyKeys;
                var newKeys = customHookFamilyKeys ?? Array.Empty<string>();
                f.CustomHookFamilyKeys = newKeys;
                UpdateReverseEdges(id, oldKeys, newKeys);
                s_dirty.Add(f);
                if (sigChanged)
                    s_forceRemount.Add(f);
            }
        }

        // ────────────────────────────────────────────────────────────────────
        //  Reverse-edge map maintenance (Phase 1)
        // ────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Maintain <see cref="s_reverseEdges"/> when a Family's
        /// <see cref="Family.CustomHookFamilyKeys"/> changes between
        /// Register calls. Adds <paramref name="consumerId"/> under every
        /// new key and removes it from every dropped key (so a hook that
        /// stops being called by this consumer no longer points back at
        /// it). Must be called inside <see cref="s_lock"/>.
        ///
        /// Phase 1 only populates the map. Phase 3 reads it in
        /// <c>PropagateHookSignatureChanges</c> to invalidate consumer
        /// <c>FullSignature</c> caches and add consumers to
        /// <see cref="s_forceRemount"/> when a hook's signature changes.
        /// </summary>
        private static void UpdateReverseEdges(
            string consumerId,
            string[] oldKeys,
            string[] newKeys)
        {
            if (oldKeys != null && oldKeys.Length > 0)
            {
                for (int i = 0; i < oldKeys.Length; i++)
                {
                    var k = oldKeys[i];
                    if (string.IsNullOrEmpty(k)) continue;
                    if (newKeys != null && Array.IndexOf(newKeys, k) >= 0) continue;
                    if (s_reverseEdges.TryGetValue(k, out var consumers))
                    {
                        consumers.Remove(consumerId);
                        if (consumers.Count == 0)
                            s_reverseEdges.Remove(k);
                    }
                }
            }
            if (newKeys != null && newKeys.Length > 0)
            {
                for (int i = 0; i < newKeys.Length; i++)
                {
                    var k = newKeys[i];
                    if (string.IsNullOrEmpty(k)) continue;
                    if (!s_reverseEdges.TryGetValue(k, out var consumers))
                    {
                        consumers = new HashSet<string>(StringComparer.Ordinal);
                        s_reverseEdges[k] = consumers;
                    }
                    consumers.Add(consumerId);
                }
            }
        }

        // ────────────────────────────────────────────────────────────────────
        //  Phase 3 — transitive hook-signature propagation
        // ────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Walk every hook Family currently in <see cref="s_dirty"/> and
        /// propagate dirty-ness (and force-remount, when the hook's own
        /// Signature changed) outward through <see cref="s_reverseEdges"/>
        /// to every transitive consumer Family. Hook→hook chains are
        /// followed via a BFS queue; cycles are bounded by the
        /// <c>visited</c> set keyed on hook Family Id.
        ///
        /// React-Fast-Refresh parity rationale: React stores a
        /// <c>signature.fullKey</c> string per type, recomputed lazily by
        /// walking each <c>customHooks</c> array and concatenating their
        /// keys; the comparison <c>haveEqualSignatures(prev, next)</c>
        /// determines whether to force-remount the consumer. UITKX uses
        /// the equivalent reverse-edge walk: instead of recomputing a
        /// concatenated key per consumer on every PerformRefresh, we
        /// pre-built the inverse map at Register / RegisterHook time and
        /// here simply mark every consumer of a dirty hook. The outcome
        /// is identical: any component that called a hook whose body or
        /// signature changed receives the appropriate re-render or
        /// state-reset.
        ///
        /// Must run inside <see cref="s_lock"/>.
        /// </summary>
        private static void PropagateHookSignatureChanges()
        {
            if (s_dirty.Count == 0 || s_reverseEdges.Count == 0)
                return;

            Queue<Family> queue = null;
            foreach (var f in s_dirty)
            {
                if (!f.IsHook) continue;
                if (queue == null) queue = new Queue<Family>();
                queue.Enqueue(f);
            }
            if (queue == null) return;

            var visited = new HashSet<string>(StringComparer.Ordinal);
            while (queue.Count > 0)
            {
                var hookFam = queue.Dequeue();
                if (!visited.Add(hookFam.Id)) continue;
                bool hookForcedRemount = s_forceRemount.Contains(hookFam);
                if (!s_reverseEdges.TryGetValue(hookFam.Id, out var consumerIds)) continue;

                foreach (var consumerId in consumerIds)
                {
                    if (!s_families.TryGetValue(consumerId, out var consumerFam))
                        continue;
                    s_dirty.Add(consumerFam);
                    if (hookForcedRemount)
                        s_forceRemount.Add(consumerFam);
                    if (consumerFam.IsHook && !visited.Contains(consumerFam.Id))
                        queue.Enqueue(consumerFam);
                }
            }
        }

        // ────────────────────────────────────────────────────────────────────
        //  P3 — GetFamily
        // ────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Return the canonical Family for <paramref name="id"/>, creating
        /// a placeholder if Register hasn't run yet. Called from SG-emitted
        /// <c>__fam_&lt;TypeName&gt;</c> static-readonly field initializers
        /// in consumer parents.
        ///
        /// The placeholder's <see cref="Family.Current"/> throws on
        /// invocation with a message that names the unresolved
        /// <see cref="Family.Id"/>; the moment of resolution also emits
        /// a one-shot <c>Debug.LogError</c> via
        /// <see cref="WarnUnresolvedFamilyOnce"/>. In
        /// practice, module-initializer ordering within and across
        /// assemblies guarantees Register runs before the consumer's
        /// cctor reads this Family, so the placeholder is replaced before
        /// any render ever occurs.
        /// </summary>
        public static Family GetFamily(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            lock (s_lock)
            {
                if (s_families.TryGetValue(id, out var existing))
                    return existing;

                // Create an empty Family. Family.Current will emit a
                // one-shot LogError via WarnUnresolvedFamilyOnce and
                // return a throwing per-instance placeholder (whose
                // exception message includes the Family.Id) until
                // either a Register call sets _current OR a parent
                // supplies a fallback factory via the GetFamily(id,
                // factory) overload below.
                var placeholder = new Family(
                    id,
                    current: (Func<Core.IProps, IReadOnlyList<Core.VirtualNode>, Core.VirtualNode>)null,
                    signature: null);
                s_families[id] = placeholder;
                return placeholder;
            }
        }

        /// <summary>
        /// Fallback-factory overload of <see cref="GetFamily(string)"/>.
        /// Used by SG-emitted parents so that children which lack an
        /// `[ModuleInitializer]` Register call (hand-written components in
        /// other packages, e.g. <c>ReactiveUITK.Router.Route</c>) still
        /// resolve to a real render delegate.
        ///
        /// The <paramref name="fallbackFactory"/> is invoked lazily on
        /// first <see cref="Family.Current"/> read -- not at parent cctor
        /// time -- so the lambda's `ldftn Child.Render` instruction (which
        /// per ECMA-335 §I.8.9.5 does NOT trigger the child's `.cctor`,
        /// and which Mono honors for `ldftn` specifically) runs only at
        /// first render, after Unity's editor init hooks have populated
        /// registries.
        ///
        /// If a real Register call has already published a body for
        /// <paramref name="id"/>, the fallback is ignored.
        /// </summary>
        public static Family GetFamily(
            string id,
            Func<Func<Core.IProps, IReadOnlyList<Core.VirtualNode>, Core.VirtualNode>> fallbackFactory)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            lock (s_lock)
            {
                if (s_families.TryGetValue(id, out var existing))
                {
                    existing.TrySetFallbackFactory(fallbackFactory);
                    return existing;
                }

                // Create with the fallback factory pre-installed; if a
                // Register call lands later it will supersede (Current
                // setter clears _factory).
                var fam = new Family(id, fallbackFactory, signature: null);
                s_families[id] = fam;
                return fam;
            }
        }

#if UNITY_EDITOR
        // ────────────────────────────────────────────────────────────────────
        //  P4 — PerformRefresh
        // ────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Walk every live renderer's fiber tree and schedule re-render for
        /// fibers whose <see cref="Core.Fiber.FiberNode.Family"/> is in the
        /// dirty set (Register has updated <see cref="Family.Current"/>
        /// since the last call). Fibers whose Family is in the force-remount
        /// set (signature changed) get a full state reset before re-render.
        ///
        /// Idempotent: clears the dirty + force-remount sets at the end.
        /// Called by UitkxHmrController after every successful HMR compile.
        ///
        /// Editor-only: in player builds there is no HMR compile, the
        /// Family path is never instantiated, and this method is excluded
        /// from compilation.
        /// </summary>
        public static int PerformRefresh()
        {
            HashSet<Family> dirty;
            HashSet<Family> forceRemount;
            lock (s_lock)
            {
                // Phase 3 — propagate dirty-ness from any hook family
                // currently in s_dirty out to its transitive consumers via
                // the reverse-edge map populated by Register / RegisterHook.
                // Must run inside the lock so the dirty / force-remount /
                // reverse-edge sets stay consistent. Performed BEFORE the
                // snapshot below so the consumers picked up here participate
                // in this same PerformRefresh pass.
                PropagateHookSignatureChanges();

                if (s_dirty.Count == 0)
                    return 0;
                dirty = new HashSet<Family>(s_dirty);
                forceRemount = new HashSet<Family>(s_forceRemount);
                s_dirty.Clear();
                s_forceRemount.Clear();
            }

            int notified = 0;
            // The renderer registries live in the EditorSupport asmdef which
            // depends on Shared -- we cannot reference them directly. The
            // controller wires a callback at editor load time via
            // RootRendererProvider; if it hasn't been wired, PerformRefresh
            // is a no-op (no live renderers).
            var provider = s_rootRendererProvider;
            if (provider == null)
                return 0;

            foreach (var rootFiber in provider())
            {
                if (rootFiber == null) continue;
                RefreshFiberTree(rootFiber, dirty, forceRemount, ref notified);
            }
            return notified;
        }

        private static void RefreshFiberTree(
            Core.Fiber.FiberNode fiber,
            HashSet<Family> dirty,
            HashSet<Family> forceRemount,
            ref int notified)
        {
            if (fiber == null) return;

            if (fiber.Family != null && dirty.Contains(fiber.Family))
            {
                if (forceRemount.Contains(fiber.Family))
                {
                    FullResetComponentState(fiber);
                }

                // Refresh the fiber's cached render delegate from the freshly
                // published Family.Current BEFORE scheduling the re-render.
                // Without this, the next render pass invokes fiber.TypedRender
                // -- the delegate captured at mount time (or the previous
                // V.Func vnode), which is the OLD body. Symptom: a parent
                // that was already mounted and visible when HMR added a new
                // child component re-renders using the old IL, the new child
                // never appears on screen, and only an unmount + remount of
                // the parent (e.g. closing and reopening a dialog) picks up
                // the new body -- because remount routes through V.Func which
                // reads Family.Current fresh. Mirrors the same refresh that
                // the render-crash rollback path does at FiberReconciler L514.
                fiber.TypedRender = fiber.Family.Current;

                try
                {
                    fiber.ComponentState?.OnStateUpdated?.Invoke();
                    notified++;
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogWarning(
                        $"[HMR] Re-render scheduling failed on '{fiber.Family.Id}': {ex.Message}"
                    );
                }
            }

            RefreshFiberTree(fiber.Child, dirty, forceRemount, ref notified);
            RefreshFiberTree(fiber.Sibling, dirty, forceRemount, ref notified);
        }

        /// <summary>
        /// Comprehensive component state reset: runs effect cleanups, clears
        /// all hook state, queued updates, caches, and context dependencies.
        /// Mirrors the unmount flow in FiberReconciler so a forced remount
        /// after an incompatible HMR edit behaves like an honest fresh mount.
        ///
        /// Moved from the (now deleted) UitkxHmrComponentTrampolineSwapper
        /// during the Family refactor — same semantics, called from a new
        /// place.
        /// </summary>
        internal static void FullResetComponentState(Core.Fiber.FiberNode fiber)
        {
            var state = fiber.ComponentState;
            if (state == null) return;

            if (state.FunctionEffects != null)
            {
                for (int i = 0; i < state.FunctionEffects.Count; i++)
                {
                    try { state.FunctionEffects[i].cleanup?.Invoke(); }
                    catch { /* swallow — cleanup failures must not stop reset */ }
                }
                state.FunctionEffects.Clear();
            }
            if (state.FunctionLayoutEffects != null)
            {
                for (int i = 0; i < state.FunctionLayoutEffects.Count; i++)
                {
                    try { state.FunctionLayoutEffects[i].cleanup?.Invoke(); }
                    catch { }
                }
                state.FunctionLayoutEffects.Clear();
            }

            Hooks.DisposeSignalSubscriptions(state);

            state.HookStates.Clear();
            state.HookOrderSignatures?.Clear();
            state.HookOrderPrimed = false;
            state.HookStateQueues?.Clear();
            state.PendingHookStatePreviews?.Clear();
            state.StateSetterDelegateCache?.Clear();
            state.ContextDependencies?.Clear();
        }

        // ────────────────────────────────────────────────────────────────────
        //  Rollback — invoked by FiberReconciler render-crash catch path
        // ────────────────────────────────────────────────────────────────────

        /// <summary>
        /// One-shot rollback: revert <see cref="Family.Current"/> to
        /// <see cref="Family.Previous"/> if Previous is populated.
        /// Wired into <see cref="Core.HmrState.TryRollbackFamily"/> by
        /// <see cref="InstallRollbackHook"/>. The reconciler invokes this
        /// from its render-crash handler; on success, the previous body is
        /// reinstalled and the reconciler retries the render once.
        ///
        /// Clears <see cref="Family.Previous"/> after rollback so a second
        /// crash on the same cycle falls through to the nearest
        /// ErrorBoundary instead of thrashing.
        /// </summary>
        public static bool TryRollback(Family family)
        {
            if (family == null) return false;
            lock (s_lock)
            {
                var prev = family.Previous;
                if (prev == null) return false;
                family.Current = prev;
                family.Previous = null;
                s_dirty.Remove(family);
                s_forceRemount.Remove(family);
            }
            return true;
        }

        // ────────────────────────────────────────────────────────────────────
        //  Editor-side wiring
        // ────────────────────────────────────────────────────────────────────

        // The renderer-walk callback is supplied by the Editor asmdef at
        // load time. Shared/ cannot reference the EditorSupport renderer
        // registries directly, so we accept a delegate that yields the
        // root FiberNode of every live renderer.
        private static Func<IEnumerable<Core.Fiber.FiberNode>> s_rootRendererProvider;

        /// <summary>
        /// Editor-only wiring entry point. The UITKX EditorSupport asmdef
        /// supplies a delegate that enumerates the root fiber of every
        /// live renderer. Called once at editor load time via
        /// <c>[InitializeOnLoadMethod]</c> in the Editor asmdef.
        /// </summary>
        public static void RegisterRootRendererProvider(
            Func<IEnumerable<Core.Fiber.FiberNode>> provider)
        {
            s_rootRendererProvider = provider;
            HmrState.TryRollbackFamily = TryRollback;
        }
#endif
    }
}

#endif // UNITY_EDITOR
