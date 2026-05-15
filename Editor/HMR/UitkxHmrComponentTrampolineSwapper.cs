// ─────────────────────────────────────────────────────────────────────────────
//  UitkxHmrComponentTrampolineSwapper
//
//  Per-component HMR swap point. Replaces the legacy per-fiber render-delegate
//  walk (UitkxHmrDelegateSwapper.SwapAll) with a single static-field write per
//  changed component type — mirroring the per-hook and per-module-method
//  trampoline pattern.
//
//  ── Why a trampoline? ────────────────────────────────────────────────────────
//  The source generator now emits, for every function-component:
//
//      [EditorBrowsable(Never)]
//      internal static Func<IProps, IReadOnlyList<VirtualNode>, VirtualNode>
//          __hmr_Render = __Render_body;
//
//      public static VirtualNode Render(IProps p, IReadOnlyList<VirtualNode> c)
//      {
//          if (HmrState.IsActive) return __hmr_Render(p, c);
//          return __Render_body(p, c);
//      }
//
//      [EditorBrowsable(Never)]
//      private static VirtualNode __Render_body(IProps p, IReadOnlyList<VirtualNode> c)
//      { ... user code ... }
//
//  HMR swaps `__hmr_Render` to point at the freshly-compiled body. The public
//  `Render` method's identity stays stable, so:
//    * Parent components keep the same compiler-cached method-group delegate
//      (Roslyn caches static method-group conversions in a per-call-site
//      static slot from C# 11+).
//    * `ReferenceEquals(fiber.TypedRender, vnode.TypedFunctionRender)` in
//      `FiberFunctionComponent.CanReuseFiber` keeps holding across HMR cycles
//      — fibers are reused, hooks are preserved, no ghost re-mounts.
//    * The cross-assembly `IsCompatibleType` HMR fallback in the reconciler
//      becomes dead code (and is deleted in the same refactor).
//
//  ── Rollback registry ───────────────────────────────────────────────────────
//  Before each swap we capture the previous `__hmr_Render` value into
//  `s_rollbackByType`. If the new body crashes during render, the reconciler
//  invokes <see cref="TryRollback"/> via the <see cref="HmrState.TryRollbackComponent"/>
//  hook and the field is reverted. This is the type-level analogue of the
//  per-fiber `HmrPreviousRender` rollback the legacy swapper relied on; with
//  trampoline, the per-fiber field is no longer meaningful (every fiber's
//  `TypedRender` points to the SAME stable trampoline method, so rollback
//  must happen at the field, not at the fiber).
//
//  ── Hook signature changes ──────────────────────────────────────────────────
//  Compatible edits (`[HookSignature]` unchanged) only swap the field; every
//  active fiber keeps its hook state and re-renders into the new body —
//  React Fast Refresh's "compatible edit" semantics.
//
//  Incompatible edits (signature changed) additionally walk fibers of the
//  changed component type and call <see cref="FullResetComponentState"/>,
//  modeling React Fast Refresh's "force remount".
//
//  ── Re-render trigger ───────────────────────────────────────────────────────
//  After swapping the field, fibers of the changed component need to render
//  again so the new body actually runs. We schedule this via
//  `ComponentState.OnStateUpdated` on each affected fiber — the same hook
//  that `setState` uses — instead of a global tree walk.
// ─────────────────────────────────────────────────────────────────────────────

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Core.Fiber;
using UnityEditor;
using UnityEngine;

namespace ReactiveUITK.EditorSupport.HMR
{
    /// <summary>
    /// Type-wide swap of generated function-component <c>Render</c> bodies via
    /// the per-component <c>__hmr_Render</c> trampoline field. Replaces the
    /// legacy per-fiber render-delegate walk. See file header for design.
    /// </summary>
    internal static class UitkxHmrComponentTrampolineSwapper
    {
        // Generated SG/HMR shape — keep in lockstep with CSharpEmitter.cs and
        // HmrCSharpEmitter.cs.
        private const string TrampolineFieldName = "__hmr_Render";

        private const BindingFlags TrampolineFieldFlags =
            BindingFlags.Static | BindingFlags.NonPublic;

        // Type-level rollback registry. Populated immediately before every
        // swap; consumed by the reconciler's render-crash catch path through
        // HmrState.TryRollbackComponent. ConcurrentDictionary chosen for
        // cheap thread-safety even though all writers/readers run on the
        // Unity main thread today.
        private static readonly ConcurrentDictionary<Type, Delegate> s_rollbackByType =
            new ConcurrentDictionary<Type, Delegate>();

        /// <summary>
        /// Wires <see cref="HmrState.TryRollbackComponent"/> at editor load
        /// time so the reconciler (which lives in the Shared assembly and
        /// cannot reference the Editor asmdef directly) can invoke
        /// <see cref="TryRollback"/> from its render-crash handler.
        /// </summary>
        [InitializeOnLoadMethod]
        private static void InstallRollbackHook()
        {
            HmrState.TryRollbackComponent = TryRollback;
        }

        /// <summary>
        /// Apply a single component HMR swap.
        /// </summary>
        /// <param name="hmrAssembly">The freshly-compiled HMR assembly that
        /// contains the new component type and body.</param>
        /// <param name="componentName">The <c>[UitkxElement]</c> name of the
        /// component to swap (file basename without extension).</param>
        /// <param name="uitkxFilePath">Absolute path of the source
        /// <c>.uitkx</c> file. Used as a stable identifier when the user
        /// renamed the component (the file path stays put while the class
        /// name changes); matched against <c>[UitkxSource]</c>.</param>
        /// <returns>Number of live fibers that were notified to re-render
        /// (informational only — a return of zero on a successful swap just
        /// means no instances of the component were mounted).</returns>
        public static int SwapAll(
            Assembly hmrAssembly,
            string componentName,
            string uitkxFilePath = null
        )
        {
            if (hmrAssembly == null || string.IsNullOrEmpty(componentName))
                return 0;

            // ── 1. Resolve the new (HMR-loaded) type and its Render method ───
            Type newType = ResolveComponentType(hmrAssembly, componentName);
            if (newType == null)
            {
                Debug.LogWarning(
                    $"[HMR] Could not find component type for '{componentName}' " +
                    "in HMR assembly."
                );
                return 0;
            }

            var newRender = ResolveRenderDelegate(newType);
            if (newRender == null)
            {
                Debug.LogWarning(
                    $"[HMR] Could not bind Render delegate on '{newType.FullName}'."
                );
                return 0;
            }

            // ── 2. Resolve every live consumer type for this component:
            //       project-loaded type (post domain reload) PLUS any prior
            //       hmr_*.dll types that earlier HMR cycles produced. The
            //       latter is essential when the user creates a brand-new
            //       component live — its type only exists in prior HMR DLLs
            //       (the SG hasn't run, so the project assembly has nothing),
            //       and parents that consumed it bound their method-group
            //       delegates to those HMR types' Render. We must update the
            //       trampoline on every such type or subsequent edits silently
            //       no-op. See Plans~/HMR_NEW_COMPONENT_LIVE_SWAP_PLAN.md. ──
            var oldTypes = FindAllSwapTargetTypes(
                hmrAssembly, componentName, uitkxFilePath
            );
            if (oldTypes.Count == 0)
            {
                // No consumer type yet — first compile of a brand-new component.
                // The trampoline gets installed when a parent first compiles
                // against this component (which loads the new HMR DLL with its
                // own __hmr_Render = __Render_body initial value).
                Debug.Log(
                    $"[HMR] Compiled {componentName} — no live consumer types yet " +
                    "(component is brand-new; subsequent edits will hot-swap once " +
                    "a parent compiles against it)."
                );
                return 0;
            }

            // ── 3. Decide remount policy via [HookSignature] BEFORE swapping.
            //       Per-fiber check happens in NotifyMatchingFibers because
            //       different prior HMR generations may have different sigs;
            //       the hoisted bool is only for the single summary log. ───
            bool anySignatureChanged = false;
            for (int i = 0; i < oldTypes.Count; i++)
            {
                if (HasHookSignatureChanged(oldTypes[i], newType))
                {
                    anySignatureChanged = true;
                    break;
                }
            }
            if (anySignatureChanged)
            {
                Debug.Log(
                    $"[HMR] Hook signature changed in {componentName} — " +
                    "resetting state on affected instances."
                );
            }

            // ── 4. Swap trampoline on every matched type. Each prior HMR DLL
            //       carries its own __hmr_Render static field (SG emits it
            //       unconditionally), so each version's Render path now
            //       routes through the new delegate. Rollback is captured
            //       per-type so a render crash on any specific type can
            //       revert that type's field independently. ────────────────
            int trampolinesSwapped = 0;
            foreach (var oldType in oldTypes)
            {
                FieldInfo hmrField = oldType.GetField(
                    TrampolineFieldName, TrampolineFieldFlags
                );
                if (hmrField == null)
                {
                    // Pre-trampoline-refactor SG output — only possible for
                    // project-loaded types from a Library/ScriptAssemblies
                    // built before the refactor landed. Prior HMR DLLs always
                    // have the field (they're emitted by current HmrCSharpEmitter).
                    Debug.LogWarning(
                        $"[HMR] Component '{oldType.FullName}' has no '{TrampolineFieldName}' " +
                        "field — it was compiled before the trampoline refactor. " +
                        "Recompile the project (full domain reload) to enable hot-swap on this type."
                    );
                    continue;
                }

                var prev = hmrField.GetValue(null) as Delegate;
                if (prev != null)
                    s_rollbackByType[oldType] = prev;

                try
                {
                    hmrField.SetValue(null, newRender);
                    trampolinesSwapped++;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(
                        $"[HMR] Failed to swap '{TrampolineFieldName}' on " +
                        $"'{oldType.FullName}': {ex.Message}"
                    );
                }
            }
            if (trampolinesSwapped == 0)
                return 0;

            // ── 5. Notify every fiber whose component type is one of the
            //       swapped types so it re-renders into the new body. Bounded
            //       to those types — much cheaper than a global tree walk.
            //       Per-fiber signature comparison handles mixed-generation
            //       trees (rare but valid). ─────────────────────────────────
            var oldTypeSet = oldTypes.Count == 1
                ? null
                : new HashSet<Type>(oldTypes);
            Type singleOldType = oldTypes.Count == 1 ? oldTypes[0] : null;
            int notified = 0;
            foreach (var renderer in EditorRootRendererUtility.GetAllRenderers())
            {
                var fr = renderer?.FiberRendererInternal;
                if (fr?.Root?.Current == null)
                    continue;
                NotifyMatchingFibers(
                    fr.Root.Current, singleOldType, oldTypeSet, newType, ref notified
                );
            }
            foreach (var rootRenderer in RootRenderer.AllInstances)
            {
                var vhr = rootRenderer?.VNodeHostRendererInternal;
                if (vhr?.FiberRendererInternal?.Root?.Current == null)
                    continue;
                NotifyMatchingFibers(
                    vhr.FiberRendererInternal.Root.Current,
                    singleOldType, oldTypeSet, newType, ref notified
                );
            }

            if (notified > 0)
                Debug.Log($"[HMR] Swapped {componentName} ({notified} instance(s)).");
            else
                Debug.Log($"[HMR] Compiled {componentName} — no active instances to refresh.");

            return notified;
        }

        /// <summary>
        /// Reverts <c>__hmr_Render</c> on <paramref name="componentType"/> to
        /// the previous-known value, if one was captured. Invoked by the
        /// reconciler's render-crash catch path via
        /// <see cref="HmrState.TryRollbackComponent"/>. The previous value is
        /// removed from the registry on rollback so a second crash on the
        /// same cycle falls through to the error boundary instead of
        /// thrashing.
        /// </summary>
        public static bool TryRollback(Type componentType)
        {
            if (componentType == null)
                return false;
            if (!s_rollbackByType.TryRemove(componentType, out var prev) || prev == null)
                return false;

            FieldInfo hmrField = componentType.GetField(
                TrampolineFieldName,
                TrampolineFieldFlags
            );
            if (hmrField == null)
                return false;

            try
            {
                hmrField.SetValue(null, prev);
            }
            catch
            {
                return false;
            }
            return true;
        }

        // ── Type / delegate resolution ───────────────────────────────────────

        private static Type ResolveComponentType(Assembly asm, string componentName)
        {
            Type[] types;
            try { types = asm.GetTypes(); }
            catch (ReflectionTypeLoadException ex) { types = ex.Types?.Where(t => t != null).ToArray() ?? Array.Empty<Type>(); }

            foreach (var t in types)
            {
                if (t == null) continue;
                var attr = t.GetCustomAttribute<UitkxElementAttribute>();
                if (attr != null && attr.ComponentName == componentName)
                    return t;
            }
            // Fallback: simple type-name match (rare — only when the attribute
            // scan failed for some reason).
            return types.FirstOrDefault(t => t != null && t.Name == componentName);
        }

        /// <summary>
        /// Returns every loaded type that represents the component named
        /// <paramref name="componentName"/> — both the project-loaded type
        /// (if any) AND every prior <c>hmr_*.dll</c> type with the matching
        /// <c>[UitkxElement]</c> name or <c>[UitkxSource]</c> file path.
        /// <para>
        /// The just-loaded HMR assembly (<paramref name="skipAssembly"/>) is
        /// excluded: its <c>__hmr_Render</c> already points at the brand-new
        /// body so swapping it onto itself is a no-op. The point of the swap
        /// is to redirect the OLD types' trampolines so existing parent
        /// bindings (compiler-cached method groups in already-emitted IL) hit
        /// the new body on the next render.
        /// </para>
        /// <para>
        /// This is the single change that enables creating new components
        /// live without a domain reload. Without it, brand-new components
        /// have no project-side type, so the legacy lookup returned null and
        /// every subsequent save silently no-op'd.
        /// </para>
        /// </summary>
        private static List<Type> FindAllSwapTargetTypes(
            Assembly skipAssembly,
            string componentName,
            string uitkxFilePath
        )
        {
            var results = new List<Type>(2);
            HashSet<Type> seen = null;

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm.IsDynamic) continue;
                if (ReferenceEquals(asm, skipAssembly)) continue;

                Type[] types;
                try { types = asm.GetTypes(); }
                catch (ReflectionTypeLoadException ex) { types = ex.Types?.Where(t => t != null).ToArray() ?? Array.Empty<Type>(); }
                catch { continue; }

                foreach (var t in types)
                {
                    if (t == null) continue;

                    bool match = false;
                    var elementAttr = t.GetCustomAttribute<UitkxElementAttribute>();
                    if (elementAttr != null && elementAttr.ComponentName == componentName)
                        match = true;

                    if (!match && uitkxFilePath != null)
                    {
                        var sourceAttr = t.GetCustomAttribute<UitkxSourceAttribute>();
                        if (sourceAttr != null && string.Equals(
                                sourceAttr.SourcePath,
                                uitkxFilePath,
                                StringComparison.OrdinalIgnoreCase))
                        {
                            match = true;
                        }
                    }

                    if (!match) continue;

                    seen ??= new HashSet<Type>();
                    if (seen.Add(t))
                        results.Add(t);
                }
            }

            return results;
        }

        private static Func<IProps, IReadOnlyList<VirtualNode>, VirtualNode> ResolveRenderDelegate(
            Type type
        )
        {
            var renderMethod = type.GetMethod(
                "Render",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(IProps), typeof(IReadOnlyList<VirtualNode>) },
                null
            );
            if (renderMethod == null)
                return null;

            try
            {
                return (Func<IProps, IReadOnlyList<VirtualNode>, VirtualNode>)
                    Delegate.CreateDelegate(
                        typeof(Func<IProps, IReadOnlyList<VirtualNode>, VirtualNode>),
                        renderMethod
                    );
            }
            catch
            {
                return null;
            }
        }

        // ── Fiber notification ───────────────────────────────────────────────

        // Per-fiber notify: matches against either a single old-type fast path
        // (most common case — exactly one project-loaded type) OR a HashSet of
        // multiple types when prior HMR generations produced their own copies.
        // Per-fiber signature comparison ensures mixed-generation fibers each
        // get the correct compatible / force-reset treatment.
        private static void NotifyMatchingFibers(
            FiberNode fiber,
            Type singleOldType,
            HashSet<Type> oldTypeSet,
            Type newType,
            ref int count
        )
        {
            if (fiber == null) return;

            if (fiber.Tag == FiberTag.FunctionComponent)
            {
                var declaring = fiber.TypedRender?.Method?.DeclaringType;
                bool isMatch = declaring != null && (
                    (singleOldType != null && declaring == singleOldType) ||
                    (oldTypeSet != null && oldTypeSet.Contains(declaring))
                );

                if (isMatch)
                {
                    if (HasHookSignatureChanged(declaring, newType))
                        FullResetComponentState(fiber);

                    try
                    {
                        fiber.ComponentState?.OnStateUpdated?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning(
                            $"[HMR] Re-render scheduling failed on '{declaring.Name}': {ex.Message}"
                        );
                    }
                    count++;
                }
            }

            NotifyMatchingFibers(fiber.Child, singleOldType, oldTypeSet, newType, ref count);
            NotifyMatchingFibers(fiber.Sibling, singleOldType, oldTypeSet, newType, ref count);
        }

        // ── Hook signature & state-reset helpers ─────────────────────────────
        //
        // Moved here from UitkxHmrDelegateSwapper as part of the trampoline
        // refactor. These two helpers are the only remaining state-mutation
        // primitives the swapper needs; everything else flows through field
        // reflection.

        private static bool HasHookSignatureChanged(Type oldType, Type newType)
        {
            if (oldType == null || newType == null) return false;
            var oldAttr = oldType.GetCustomAttribute<HookSignatureAttribute>();
            var newAttr = newType.GetCustomAttribute<HookSignatureAttribute>();
            if (oldAttr == null || newAttr == null) return false;
            return !string.Equals(oldAttr.Signature, newAttr.Signature, StringComparison.Ordinal);
        }

        /// <summary>
        /// Comprehensive component state reset: runs effect cleanups, clears
        /// all hook state, queued updates, caches, and context dependencies.
        /// Mirrors the unmount flow in <c>FiberReconciler</c> so a forced
        /// remount after an incompatible HMR edit behaves like an honest
        /// fresh mount.
        /// </summary>
        internal static void FullResetComponentState(FiberNode fiber)
        {
            var state = fiber.ComponentState;
            if (state == null) return;

            if (state.FunctionEffects != null)
            {
                for (int i = 0; i < state.FunctionEffects.Count; i++)
                {
                    try { state.FunctionEffects[i].cleanup?.Invoke(); }
                    catch { }
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
    }
}
