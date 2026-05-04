// ════════════════════════════════════════════════════════════════════════════
//  UitkxHmrModuleStaticSwapper — re-bind `static readonly` module fields after
//  a hot-reload compile.
// ════════════════════════════════════════════════════════════════════════════
//
//  Problem this solves (Issue 13 in Plans~/PRETTY_UI_HMR_BUGS.md)
//  ─────────────────────────────────────────────────────────────
//
//  `module` declarations in .uitkx files are emitted by both the source
//  generator (SourceGenerator~/Emitter/ModuleEmitter.cs) and the HMR pipeline
//  (Editor/HMR/HmrHookEmitter.EmitModules) as a `public partial class` whose
//  body is the user's module body verbatim:
//
//      module MenuPage {
//          public static readonly Style Root = new Style { BackgroundColor = ... };
//      }
//
//  When the project is cold-built, the CLR runs `MenuPage`'s type initializer
//  (cctor) ONCE and pins `MenuPage.Root` to a `Style` instance produced by the
//  initializer expression. That instance lives in the project's
//  `Assembly-CSharp.dll` for the duration of the AppDomain.
//
//  When the user edits the module body and saves, HMR rebuilds a *new* dynamic
//  assembly that contains a fresh `MenuPage` type with the NEW initializer
//  expression — and loading that assembly runs its cctor, producing a fresh
//  `Style` instance bound to the HMR-`MenuPage.Root` field.
//
//  But the project type's `MenuPage.Root` still references the ORIGINAL
//  cold-build instance: nothing in the existing HMR pipeline copies values
//  across the assembly boundary for plain `static readonly` fields.
//
//  `UitkxHmrDelegateSwapper.SwapAll` and `SwapHooks` only re-bind fields whose
//  names start with `__hmr_*` (the synthesized delegate slots). Module fields
//  are user-named (`Root`, `Bar`, `BgColor`, …) and are skipped. The result
//  observed in PrettyUi: deleting `BackgroundColor = Theme.BgPanel,` from a
//  style and saving produces a successful HMR cycle but the rendered UI still
//  shows the old background — until the user exits Play mode, which forces a
//  full assembly reload (re-running every cctor).
//
//
//  Strategy (Option C in the design discussion)
//  ────────────────────────────────────────────
//
//  The HMR-compiled assembly's cctor *already* runs the new initializer
//  expressions and produces fresh values bound to the HMR type's static
//  readonly fields. We simply copy those values into the project type via
//  reflection. `FieldInfo.SetValue` bypasses the `readonly` runtime check on
//  both .NET Core and Mono, which is the documented escape hatch the BCL
//  itself uses for record InitOnly support.
//
//  Why this is preferable to alternatives:
//
//    • "Synthesize a `__hmr_reinit_module()` method" — would require parsing
//      the module body (freeform user C#) at HMR-emit time and rewriting it
//      into discrete field-assignment statements. That's a Roslyn-level
//      rewrite with parity risk against ModuleEmitter, and gains nothing over
//      simply copying the values produced by the HMR cctor.
//
//    • "Re-run the project type's cctor" — `RuntimeHelpers.RunClassConstructor`
//      is a no-op on second call (CLR enforces type-initializer-runs-once),
//      and invoking the cctor `MethodInfo` directly would re-fire any
//      side-effects (event subscriptions, registry pushes, …) the user wrote
//      in the module body, producing duplicate handlers / corrupt state.
//      The cross-assembly copy approach side-steps both issues.
//
//
//  Safety guardrails
//  ─────────────────
//
//    • Only fields where `FieldInfo.IsInitOnly == true` (i.e. `static readonly`)
//      are copied. Mutable `static` fields are preserved across HMR — they
//      represent user runtime state and clobbering them would be a regression.
//
//    • `FieldInfo.IsLiteral` (i.e. `const`) is skipped — consts have no
//      runtime field slot to set.
//
//    • Fields whose name starts with `__hmr_` are skipped — already handled
//      by UitkxHmrDelegateSwapper (avoids double-write / fighting the
//      delegate-swap pipeline).
//
//    • Compiler-generated types (`<Module>`, `<>c__DisplayClass*`, …) are
//      skipped — never user-authored, never module material.
//
//    • Each field is wrapped in its own try/catch. A single field with a
//      cross-assembly type-identity mismatch (rare; only happens if the user
//      defines a nested type inside the module body) cannot break the rest
//      of the HMR cycle — we log a warning and continue.
//
//    • Called BEFORE delegate swap so that when the new render delegate
//      first executes, the static fields it reads are already updated.
//
//
//  What this does NOT cover (intentional, follow-up work)
//  ──────────────────────────────────────────────────────
//
//    • Static *methods* in modules (e.g. `StyleExtensions.Extend(...)`) — would
//      need synthesized `__hmr_*` delegate fields like hooks/components have.
//      Currently changes to a module's static methods only take effect after
//      a full assembly reload.
//
//    • Newly-added `static readonly` fields the user introduces during a
//      session — they exist in the HMR assembly but have no slot in the
//      project type. Anything compiled fresh against the HMR assembly sees
//      them; anything compiled against the project type pre-edit does not.
//      A full assembly reload is required to materialise them on the project
//      type.

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ReactiveUITK.EditorSupport.HMR
{
    /// <summary>
    /// Result of a single <see cref="UitkxHmrModuleStaticSwapper.SwapModuleStatics"/>
    /// call.
    /// </summary>
    /// <remarks>
    /// <see cref="AddedFieldsDetected"/> is the count of <c>static readonly</c> fields
    /// that exist on the HMR-compiled type but have no corresponding slot on
    /// the project-loaded type. These are CLR-level rude edits: the runtime
    /// seals type metadata at load time, so the project type cannot grow new
    /// fields without a full assembly reload. The swapper logs a once-per-session
    /// warning for each newly-detected field; the controller may opt into
    /// auto-triggering a domain reload when this count is &gt; 0.
    /// </remarks>
    internal readonly struct ModuleStaticSwapResult
    {
        public readonly int Copied;
        public readonly int AddedFieldsDetected;

        public ModuleStaticSwapResult(int copied, int addedFieldsDetected)
        {
            Copied = copied;
            AddedFieldsDetected = addedFieldsDetected;
        }

        public static readonly ModuleStaticSwapResult Empty = default;
    }

    /// <summary>
    /// Copies <c>static readonly</c> field values from the freshly-HMR-compiled
    /// assembly into the corresponding types of the project's loaded assemblies,
    /// so that <c>module</c>-declared constants (e.g. <c>Style</c>, <c>Color</c>)
    /// are re-initialised after a hot-reload edit.
    /// </summary>
    internal static class UitkxHmrModuleStaticSwapper
    {
        // BindingFlags used everywhere in this file — pre-cached so we don't
        // re-allocate on every invocation.
        private const BindingFlags StaticFieldFlags =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        // Once-per-session dedup of "added field" warnings. Key is
        // "{Type.FullName}.{FieldName}". Cleared automatically on domain reload
        // because all static state in the HMR Editor assembly resets then.
        private static readonly HashSet<string> s_warnedAddedFields =
            new HashSet<string>(StringComparer.Ordinal);

        /// <summary>
        /// For every type in <paramref name="hmrAssembly"/>, locate a project
        /// type with the same <see cref="Type.FullName"/>, copy each
        /// <c>static readonly</c> field's value across, and detect any
        /// <c>static readonly</c> fields that exist only on the HMR side
        /// (CLR rude edit — project type metadata is sealed at load time).
        /// Safe to call when no module fields exist — returns <see cref="ModuleStaticSwapResult.Empty"/>.
        /// </summary>
        public static ModuleStaticSwapResult SwapModuleStatics(Assembly hmrAssembly)
        {
            if (hmrAssembly == null) return ModuleStaticSwapResult.Empty;

            int copied = 0;
            int addedFieldsDetected = 0;

            // GetTypes() can throw ReflectionTypeLoadException if a type fails
            // to load — but the HMR pipeline only continues past compile when
            // the assembly is well-formed, so this should be safe. Still wrap
            // defensively.
            Type[] hmrTypes;
            try
            {
                hmrTypes = hmrAssembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                Debug.LogWarning(
                    "[HMR] UitkxHmrModuleStaticSwapper: could not enumerate types " +
                    "from HMR assembly — module statics will not be re-initialised. " +
                    $"({ex.Message})"
                );
                return ModuleStaticSwapResult.Empty;
            }

            foreach (var hmrType in hmrTypes)
            {
                if (hmrType == null) continue;
                if (ShouldSkipType(hmrType)) continue;

                var projectType = FindProjectType(hmrType.FullName);
                if (projectType == null) continue;

                copied += CopyStaticReadonlyFields(hmrType, projectType);
                addedFieldsDetected += DetectAndWarnAddedFields(hmrType, projectType);
            }

            return new ModuleStaticSwapResult(copied, addedFieldsDetected);
        }

        /// <summary>
        /// Counts <c>static readonly</c> fields that exist on the HMR-compiled
        /// type but NOT on the project-loaded type. For each such field, logs a
        /// once-per-session warning explaining the CLR rude-edit constraint.
        /// Returns the count of newly-detected fields (after dedup).
        /// </summary>
        private static int DetectAndWarnAddedFields(Type hmrType, Type projectType)
        {
            int newlyDetected = 0;

            FieldInfo[] hmrFields;
            try { hmrFields = hmrType.GetFields(StaticFieldFlags); }
            catch { return 0; }

            foreach (var hmrField in hmrFields)
            {
                if (!IsHotReinitableField(hmrField)) continue;

                // Field exists on the project type — already handled by Copy.
                if (projectType.GetField(hmrField.Name, StaticFieldFlags) != null)
                    continue;

                string key = hmrType.FullName + "." + hmrField.Name;
                if (!s_warnedAddedFields.Add(key))
                    continue; // already warned this session

                newlyDetected++;

                Debug.LogWarning(
                    $"[HMR] '{hmrType.FullName}' has a newly-added 'static readonly' " +
                    $"field '{hmrField.Name}' that does not exist on the project-loaded " +
                    "type. The CLR seals type metadata at load time and cannot grow new " +
                    "fields without a full assembly reload. Code recompiled by HMR (the " +
                    "render delegate from this cycle) can use the field, but references " +
                    "from non-HMR code (.cs scripts, untouched .uitkx files) will throw " +
                    "MissingFieldException. Trigger a domain reload to materialise the " +
                    "field everywhere — or enable EditorPref 'UITKX_HMR_AutoReloadOnRudeEdit'. " +
                    "(Shown once per session per field.)"
                );
            }

            return newlyDetected;
        }

        /// <summary>
        /// Test/diagnostic helper — clears the once-per-session warning dedup
        /// set so a subsequent <see cref="SwapModuleStatics"/> call will
        /// re-warn for the same fields.
        /// </summary>
        internal static void ResetWarnDedupForTests() => s_warnedAddedFields.Clear();

        // ── Helpers ───────────────────────────────────────────────────────────

        private static bool ShouldSkipType(Type t)
        {
            // Compiler-generated infrastructure: <Module>, <>f__AnonymousType,
            // <>c__DisplayClass*, etc. None of these are user module material.
            if (string.IsNullOrEmpty(t.FullName)) return true;
            if (t.FullName.StartsWith("<", StringComparison.Ordinal)) return true;
            if (t.FullName.IndexOf("<>", StringComparison.Ordinal) >= 0) return true;

            // Compiler-generated attribute marker — produced for things like
            // closures and iterator state machines. Never has user-written
            // module statics.
            if (t.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), inherit: false))
                return true;

            return false;
        }

        /// <summary>
        /// Locates the type with the given <paramref name="fullName"/> in the
        /// project's loaded (non-dynamic) assemblies. Skips dynamic assemblies
        /// (which includes the HMR assembly itself and prior HMR generations).
        /// </summary>
        private static Type FindProjectType(string fullName)
        {
            if (string.IsNullOrEmpty(fullName)) return null;

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm.IsDynamic) continue;

                Type t;
                try { t = asm.GetType(fullName, throwOnError: false); }
                catch { continue; }

                if (t != null) return t;
            }
            return null;
        }

        /// <summary>
        /// Iterates <c>static readonly</c> fields declared on
        /// <paramref name="hmrType"/> and copies their values into the
        /// equally-named field on <paramref name="projectType"/>.
        /// Returns the count of successful copies.
        /// </summary>
        private static int CopyStaticReadonlyFields(Type hmrType, Type projectType)
        {
            int copied = 0;

            FieldInfo[] hmrFields;
            try { hmrFields = hmrType.GetFields(StaticFieldFlags); }
            catch { return 0; }

            foreach (var hmrField in hmrFields)
            {
                if (!IsHotReinitableField(hmrField)) continue;

                var projectField = projectType.GetField(hmrField.Name, StaticFieldFlags);
                if (projectField == null) continue;
                if (!IsHotReinitableField(projectField)) continue;

                // Field-type sanity check across assemblies. Same FullName +
                // same defining assembly name => same type identity. If the
                // user *changed* a field's type, this guards against an
                // ArgumentException on SetValue.
                if (!AreCompatibleFieldTypes(hmrField.FieldType, projectField.FieldType))
                    continue;

                object value;
                try { value = hmrField.GetValue(null); }
                catch (Exception ex)
                {
                    Debug.LogWarning(
                        $"[HMR] Failed to read HMR field '{hmrType.FullName}.{hmrField.Name}': {ex.Message}"
                    );
                    continue;
                }

                try
                {
                    // FieldInfo.SetValue bypasses the readonly check — same
                    // mechanism the BCL uses for record InitOnly fields.
                    projectField.SetValue(null, value);
                    copied++;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(
                        $"[HMR] Failed to re-init module field '{projectType.FullName}.{projectField.Name}': {ex.Message}"
                    );
                }
            }

            return copied;
        }

        /// <summary>
        /// True for fields the swapper should re-initialise: <c>static readonly</c>,
        /// not <c>const</c>, not the synthesized <c>__hmr_*</c> delegate slots.
        /// </summary>
        private static bool IsHotReinitableField(FieldInfo f)
        {
            if (f == null) return false;
            if (!f.IsStatic) return false;
            if (f.IsLiteral) return false;        // const — no runtime slot
            if (!f.IsInitOnly) return false;       // mutable static — preserve user state
            if (f.Name.StartsWith("__hmr_", StringComparison.Ordinal))
                return false;                      // owned by UitkxHmrDelegateSwapper
            return true;
        }

        /// <summary>
        /// Conservative same-identity check. Rejects assignments where the
        /// field type itself was redefined (e.g. user added a property to a
        /// nested type inside the module body — both assemblies define a
        /// distinct version and reflection assignment would throw).
        /// </summary>
        private static bool AreCompatibleFieldTypes(Type hmrFieldType, Type projectFieldType)
        {
            if (hmrFieldType == projectFieldType) return true; // identical
            if (hmrFieldType == null || projectFieldType == null) return false;

            // Same canonical full name AND same defining assembly => same type
            // across the HMR and project sides (e.g. `Style` from Shared.dll,
            // `Color` from UnityEngine.dll). This is the common case.
            if (hmrFieldType.FullName == projectFieldType.FullName
                && hmrFieldType.Assembly.FullName == projectFieldType.Assembly.FullName)
            {
                return true;
            }

            return false;
        }
    }
}
