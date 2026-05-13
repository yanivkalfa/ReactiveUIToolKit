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

            // ── 2. Resolve the live (project-loaded) type via [UitkxElement]
            //       (primary) and [UitkxSource] file path (rename fallback). ─
            Type oldType = FindProjectComponentType(componentName, uitkxFilePath);
            if (oldType == null)
            {
                // Component never compiled into the project — nothing to swap.
                // (E.g. brand-new file not yet picked up by the SG.)
                return 0;
            }

            // ── 3. Locate the trampoline field on the live type. Components
            //       compiled with a pre-trampoline SG won't have it and need a
            //       full domain reload to pick up the new shape. ─────────────
            FieldInfo hmrField = oldType.GetField(TrampolineFieldName, TrampolineFieldFlags);
            if (hmrField == null)
            {
                Debug.LogWarning(
                    $"[HMR] Component '{oldType.FullName}' has no '{TrampolineFieldName}' " +
                    "field — it was compiled before the trampoline refactor. " +
                    "Recompile the project (full domain reload) to enable hot-swap."
                );
                return 0;
            }

            // ── 4. Capture rollback BEFORE the swap so a crash in the new
            //       body can revert via TryRollback. ─────────────────────────
            var prev = hmrField.GetValue(null) as Delegate;
            if (prev != null)
                s_rollbackByType[oldType] = prev;

            try
            {
                hmrField.SetValue(null, newRender);
            }
            catch (Exception ex)
            {
                Debug.LogWarning(
                    $"[HMR] Failed to swap '{TrampolineFieldName}' on " +
                    $"'{oldType.FullName}': {ex.Message}"
                );
                return 0;
            }

            // ── 5. Decide remount policy via [HookSignature]. Compatible
            //       edits preserve hook state; incompatible edits force a
            //       full reset (React Fast Refresh semantics). ──────────────
            bool signatureChanged = HasHookSignatureChanged(oldType, newType);
            if (signatureChanged)
            {
                Debug.Log(
                    $"[HMR] Hook signature changed in {componentName} — " +
                    "resetting state on all instances."
                );
            }

            // ── 6. Notify every fiber of the changed component type so it
            //       re-renders into the new body. Bounded to instances of
            //       this type — much cheaper than the legacy global tree
            //       walk. ────────────────────────────────────────────────────
            int notified = 0;
            foreach (var renderer in EditorRootRendererUtility.GetAllRenderers())
            {
                var fr = renderer?.FiberRendererInternal;
                if (fr?.Root?.Current == null)
                    continue;
                NotifyMatchingFibers(fr.Root.Current, oldType, signatureChanged, ref notified);
            }
            foreach (var rootRenderer in RootRenderer.AllInstances)
            {
                var vhr = rootRenderer?.VNodeHostRendererInternal;
                if (vhr?.FiberRendererInternal?.Root?.Current == null)
                    continue;
                NotifyMatchingFibers(
                    vhr.FiberRendererInternal.Root.Current,
                    oldType,
                    signatureChanged,
                    ref notified
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

        private static Type FindProjectComponentType(string componentName, string uitkxFilePath)
        {
            Type byName = null;
            Type bySource = null;

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm.IsDynamic) continue;
                // Skip HMR assemblies — they're newer than the project copy.
                if (asm.GetName().Name?.StartsWith("hmr_", StringComparison.Ordinal) == true)
                    continue;

                Type[] types;
                try { types = asm.GetTypes(); }
                catch (ReflectionTypeLoadException ex) { types = ex.Types?.Where(t => t != null).ToArray() ?? Array.Empty<Type>(); }
                catch { continue; }

                foreach (var t in types)
                {
                    if (t == null) continue;
                    var elementAttr = t.GetCustomAttribute<UitkxElementAttribute>();
                    if (elementAttr != null && elementAttr.ComponentName == componentName)
                    {
                        byName = t;
                        // Don't break: prefer the source-path match if both exist.
                    }

                    if (uitkxFilePath != null)
                    {
                        var sourceAttr = t.GetCustomAttribute<UitkxSourceAttribute>();
                        if (sourceAttr != null && string.Equals(
                                sourceAttr.SourcePath,
                                uitkxFilePath,
                                StringComparison.OrdinalIgnoreCase))
                        {
                            bySource = t;
                        }
                    }
                }
            }

            // Source-path takes precedence: it survives renames where the
            // class name changes but the file path is stable.
            return bySource ?? byName;
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

        private static void NotifyMatchingFibers(
            FiberNode fiber,
            Type oldType,
            bool signatureChanged,
            ref int count
        )
        {
            if (fiber == null) return;

            if (fiber.Tag == FiberTag.FunctionComponent
                && fiber.TypedRender?.Method?.DeclaringType == oldType)
            {
                if (signatureChanged)
                    FullResetComponentState(fiber);

                try
                {
                    fiber.ComponentState?.OnStateUpdated?.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(
                        $"[HMR] Re-render scheduling failed on '{oldType.Name}': {ex.Message}"
                    );
                }
                count++;
            }

            NotifyMatchingFibers(fiber.Child, oldType, signatureChanged, ref count);
            NotifyMatchingFibers(fiber.Sibling, oldType, signatureChanged, ref count);
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
