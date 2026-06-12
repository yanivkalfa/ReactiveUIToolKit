# Issue 14 — Fix Plan (copy-rename cascade → scene duplication)

> **Status:** PARTIALLY IMPLEMENTED.
> - Patch 14.4 — IMPLEMENTED (v0.5.21).
> - Patch 14.1 — IMPLEMENTED (v0.5.21).
> - Patch 14.2 — DEFERRED → `OPTIMIZATIONS.md` #1. Re-analysis showed this is an optimisation, not a correctness fix: after 14.1, cascade-pulled near-clones get cheap trampoline-swap only, no rude reset, no scene duplication. Shipping the narrower self-component-name filter without an empirical reproducer risks false negatives (filtering legitimate cross-module edges).
> - Patch 14.3 — DROPPED. The "`Container` not found" CS0103 in the spec used "Container" as a placeholder for a wrapper class name; no class literally named `Container` is emitted by SG (grep `SourceGenerator~/Emitter/**`). Without a captured reproducer identifying the actual missing symbol, any stub-emission patch is speculative and risks parity drift with SG output. Re-file under `PRETTY_UI_HMR_BUGS.md` if the bug recurs with a concrete error log.
>
> **Source bug report:** `Plans~/PRETTY_UI_HMR_BUGS.md` — Issue 14 (lines ~1330-1610).
> **Verification:** Deep end-to-end pipeline trace 2026-05-19 against current `Editor/HMR/*` source. Every claim below is source-cited with `file:line`.

---

## TL;DR — what we will change

| # | File | Change | Lines | Risk |
|---|---|---|---|---|
| 14.4 | `Editor/HMR/UitkxHmrCompiler.cs`, `Editor/HMR/UitkxHmrController.cs` | Thread `Namespace` through `HmrCompileResult`; stop using `GetTypes().FirstOrDefault().Namespace`. | ~6 lines | None — purely additive |
| 14.1 | `Editor/HMR/UitkxHmrController.cs`, `Editor/HMR/UitkxHmrComponentTrampolineSwapper.cs` | Plumb an `isOriginatingChange` flag from the controller down to `NotifyMatchingFibers`; gate `FullResetComponentState` on it. | ~12 lines | Low — must verify cascade-pulled siblings still trampoline-swap (cheap) but skip the rude reset |
| 14.2 | `Editor/HMR/UitkxFileDependencyIndex.cs` | Drop `ReferencedModules` entries whose name equals the file's own component name. | ~3 lines | None — strictly narrows over-linking |
| 14.3 | `Editor/HMR/UitkxHmrCompiler.cs` | When the component is genuinely new (no project-DLL type), emit a `partial class Container { }` stub into the compilation unit before companion sources. | ~25 lines + 2 helpers | Medium — must not collide with a future SG-emitted partial in the same namespace |

**If only two ship:** **14.4 + 14.1**. That kills the wrong-namespace log spam *and* the scene-duplication symptom — the two things the user actually sees today.

---

## What we verified mechanically (source-cited)

Full pipeline trace produced by a dedicated read-only subagent pass. Highlights:

1. **Entry trigger** — `FileSystemWatcher` → debounce 50 ms → `UitkxHmrFileWatcher.PumpPendingChanges` → `UitkxHmrController.OnUitkxFileChanged` ([UitkxHmrController.cs:306](../Editor/HMR/UitkxHmrController.cs)). No async/deferred steps beyond the debounce.

2. **Cascade walk** — `OnUitkxFileChanged` calls `CollectTransitiveDependents(uitkxPath, includeComponents: true)` ([UitkxHmrController.cs:325](../Editor/HMR/UitkxHmrController.cs)). The walker returns dependents-first then the changed file LAST ([UitkxFileDependencyIndex.cs:316](../Editor/HMR/UitkxFileDependencyIndex.cs)).

3. **Batch decision** — `Count >= 2` → `ProcessBatch` ([UitkxHmrController.cs:347-360](../Editor/HMR/UitkxHmrController.cs)) → `_compiler.CompileBatch` ([UitkxHmrCompiler.cs:562](../Editor/HMR/UitkxHmrCompiler.cs)).

4. **Per-file apply loop** — `ApplySuccessfulCompileResult` is invoked **once per file in the batch** ([UitkxHmrController.cs:745-757](../Editor/HMR/UitkxHmrController.cs)). For each call, `SwapAll(result.LoadedAssembly, result.ComponentName, uitkxPath)` runs against the union assembly.

5. **`HmrCompileResult` shape** ([UitkxHmrCompiler.cs:2372-2415](../Editor/HMR/UitkxHmrCompiler.cs)) — has `ComponentName`, `HookContainerClass`, `LoadedAssembly`, but **no `Namespace` field**. The namespace is captured into `ComponentBuildArtifacts.Namespace` ([UitkxHmrCompiler.cs:545-546](../Editor/HMR/UitkxHmrCompiler.cs)) but **dropped on the floor** when the per-file `HmrCompileResult` is built in `CompileBatch` ([UitkxHmrCompiler.cs:746-755](../Editor/HMR/UitkxHmrCompiler.cs)).

6. **The non-deterministic namespace fallback** ([UitkxHmrController.cs:548-559](../Editor/HMR/UitkxHmrController.cs)):
   ```csharp
   var firstType = result.LoadedAssembly.GetTypes().FirstOrDefault();
   if (firstType != null) ns = firstType.Namespace;
   ```
   Roslyn's embedded compiler types (`Microsoft.CodeAnalysis.EmbeddedAttribute` and friends) materialise first in the assembly metadata, so `ns` becomes `"Microsoft.CodeAnalysis"`. Confirmed in source — nothing protects against this.

7. **`FullResetComponentState`** ([UitkxHmrComponentTrampolineSwapper.cs:510-549](../Editor/HMR/UitkxHmrComponentTrampolineSwapper.cs)):
   - Iterates `state.FunctionEffects` calling each `cleanup` (line 522)
   - Calls `state.FunctionEffects.Clear()` (line 524)
   - Same for `FunctionLayoutEffects`
   - Calls `Hooks.DisposeSignalSubscriptions(state)`
   - Clears `HookStates`, `HookOrderSignatures`, etc.

   **Scene-duplication chain is mechanically valid**: a `useEffect(() => { SceneManager.LoadSceneAsync("GameScene", Additive); return null; }, [])` returns null cleanup → cleanup is a no-op → `Clear()` empties the effect list → next render re-walks setup → fresh effect pushed → reconciler schedules it → scene loads again.

8. **`NotifyMatchingFibers`** ([UitkxHmrComponentTrampolineSwapper.cs:451-493](../Editor/HMR/UitkxHmrComponentTrampolineSwapper.cs)) — matches fibers by `declaring == singleOldType` / `oldTypeSet.Contains(declaring)`. **No component-name filter exists**. When the cascade batch puts e.g. `Marker` (a real ancestor) into the per-file apply loop, `SwapAll("Marker")` is invoked → `NotifyMatchingFibers` matches every `Marker` fiber in the live tree → `HasHookSignatureChanged` returns true on any whitespace drift between SG-emitted and HMR-emitted signatures → `FullResetComponentState` fires on the ancestor → its mount-scene `useEffect` re-fires → scene duplicates.

9. **The `HookSignature` regex is byte-for-byte identical** between SG (`SourceGenerator~/Emitter/CSharpEmitter.cs:512-517`) and HMR (`Editor/HMR/HmrCSharpEmitter.cs:3034-3038`). Drift comes from *upstream parser differences* in how `FunctionSetupCode` is extracted, not from the regex itself. This is the realistic divergence vector. (Not in scope for Issue 14 — separate Issue 15 candidate.)

10. **`EmitCompanionUitkxSources`** ([UitkxHmrCompiler.cs:928-1013](../Editor/HMR/UitkxHmrCompiler.cs)) inlines module bodies + hook bodies for every `ComponentName.*.uitkx` sibling. **It does not emit a `partial class Container { }` stub.** For brand-new components with no project-DLL `partial class Container`, references in companion sources fail with CS0103.

11. **Cascade pollution by copy-rename is real** — `s_pascalDottedRegex` ([UitkxFileDependencyIndex.cs:62](../Editor/HMR/UitkxFileDependencyIndex.cs)) matches any `PascalCase.Member`. The strip regex doesn't remove USS block bodies, asset-path arguments, or module-static initializer expressions. After copy-rename, leftover `Marker.X` tokens in `Bob.uitkx` register `Bob.uitkx` as a referrer of module `Marker` in `s_moduleReverse`. The next save in the subtree pulls in the whole near-clone set.

---

## Inaccuracies discovered vs. the original bug report

1. **`HmrCompileResult` does NOT currently expose `Namespace`.** The original spec implied it "already knows" the namespace and the fix was a one-liner read. In reality `Namespace` is captured *in `ComponentBuildArtifacts` only*, never copied to the per-file result. The fix is *still* tiny (add a field + populate at two sites + read at one site), but it's structural, not a one-liner.

2. **The HookSignature emitter strings are byte-for-byte identical.** The spec's "silent emitter divergence" hypothesis is partially wrong — the regex and normalisation are shared. The realistic divergence point is the **`FunctionSetupCode` extraction in the parser** (different code paths produce slightly different stripped input). Drift can still happen, but the root is the parser, not the emitter. This does **not** change the fix plan — Fix 14.1 (gate the reset) still cuts the symptom, regardless of which level the drift comes from.

3. **The bonus `FileNotFoundException` ancestor-path** symptom is a downstream consequence of Step-1 index pollution. Fix 14.2 resolves it indirectly; no separate patch needed.

Everything else in the spec is accurate.

---

## Patch specifications

### Patch 14.4 — Thread `Namespace` through `HmrCompileResult`

**Goal:** kill the "`Microsoft.CodeAnalysis.*Hooks` not found" warning spam. Prerequisite for everything else because (a) it eliminates a noise source that masks real failures and (b) `result.Namespace` is the correct value to compare in Fix 14.1's gating logic.

**Edits:**

1. **`Editor/HMR/UitkxHmrCompiler.cs` (~line 2372)** — add to `HmrCompileResult`:
   ```csharp
   public string Namespace;
   ```

2. **`Editor/HMR/UitkxHmrCompiler.cs` (~line 180, inside single-file `Compile`)** — after `ns` is resolved from directives:
   ```csharp
   result.Namespace = ns ?? string.Empty;
   ```

3. **`Editor/HMR/UitkxHmrCompiler.cs` (~line 746-755, inside `CompileBatch` per-file result loop)** — when constructing `perFile`:
   ```csharp
   var perFile = new HmrCompileResult
   {
       Success = true,
       ComponentName = art.ComponentName,
       Namespace = art.Namespace,        // <-- add
       LoadedAssembly = asm,
       ...
   };
   ```

4. **`Editor/HMR/UitkxHmrController.cs` (~line 548-559, inside `ApplySuccessfulCompileResult`)** — replace the reflection block with:
   ```csharp
   string ns = result.Namespace;
   if (string.IsNullOrEmpty(ns))
   {
       // Defensive fallback for paths that don't populate Namespace yet
       // (e.g., legacy single-file results before patch lands fully).
       try
       {
           var containerType = result.LoadedAssembly.GetTypes()
               .FirstOrDefault(t => t.Name == result.HookContainerClass
                                    || t.Name == result.ComponentName);
           ns = containerType?.Namespace;
       }
       catch { }
   }
   ```

**Risk:** none. Field is additive; controller reads with null-fallback.

**Test:** save a `.uitkx` whose namespace is e.g. `PrettyUi.App`, observe no `Microsoft.CodeAnalysis.*Hooks` warnings, observe successful swap log mentions `PrettyUi.App.<ContainerClass>`.

---

### Patch 14.1 — Gate `FullResetComponentState` to the originating change

**Goal:** kill scene duplication. Cascade-pulled siblings/ancestors must still trampoline-swap (so their next render uses the new IL), but must NOT have their hook state / effects nuked.

**Key insight from the trace:** the right boundary is **"is this `ApplySuccessfulCompileResult` invocation for the file the user actually saved, or for a file pulled in by cascade?"** — *not* "is the declaring type equal to the swap target name". The latter would over-fire on real ancestor edits.

The cascade walker already orders results **dependents-first, originator LAST** ([UitkxFileDependencyIndex.cs:316](../Editor/HMR/UitkxFileDependencyIndex.cs)). The controller can pass `isOriginatingChange: (i == paths.Count - 1)` into `ApplySuccessfulCompileResult`.

**Edits:**

1. **`Editor/HMR/UitkxHmrController.cs` (~line 745-757)** — pass an originator flag:
   ```csharp
   for (int i = 0; i < paths.Count; i++)
   {
       var per = batchResult.PerFileResults[i];
       if (per != null && per.Success)
       {
           bool isOriginator = (i == paths.Count - 1);  // <-- walker orders originator last
           ApplySuccessfulCompileResult(per, paths[i], isOriginator);
           _pendingRetryPaths.Remove(paths[i]);
       }
   }
   ```
   *Single-file path* (`ProcessFileChange`) calls `ApplySuccessfulCompileResult(..., isOriginator: true)`.

2. **`Editor/HMR/UitkxHmrController.cs`** — update `ApplySuccessfulCompileResult` signature to accept `bool isOriginatingChange`, and forward it:
   ```csharp
   swapped = UitkxHmrComponentTrampolineSwapper.SwapAll(
       result.LoadedAssembly,
       result.ComponentName,
       uitkxPath,
       allowFullStateReset: isOriginatingChange   // <-- new arg
   );
   ```

3. **`Editor/HMR/UitkxHmrComponentTrampolineSwapper.cs` (~line 103)** — add the parameter:
   ```csharp
   public static int SwapAll(
       Assembly hmrAssembly,
       string componentName,
       string uitkxFilePath = null,
       bool allowFullStateReset = true)   // <-- default preserves single-file behaviour
   ```

4. **`Editor/HMR/UitkxHmrComponentTrampolineSwapper.cs` (~line 238 and ~line 250)** — pass it to `NotifyMatchingFibers`:
   ```csharp
   NotifyMatchingFibers(fr.Root.Current, singleOldType, oldTypeSet, newType,
       allowFullStateReset, ref notified);
   ```

5. **`Editor/HMR/UitkxHmrComponentTrampolineSwapper.cs` (~line 451-493)** — add the gate:
   ```csharp
   private static void NotifyMatchingFibers(
       FiberNode fiber,
       Type singleOldType,
       HashSet<Type> oldTypeSet,
       Type newType,
       bool allowFullStateReset,    // <-- new
       ref int count)
   {
       if (fiber == null) return;

       if (fiber.Tag == FiberTag.FunctionComponent)
       {
           var declaring = fiber.TypedRender?.Method?.DeclaringType;
           bool isMatch = declaring != null && (
               (singleOldType != null && declaring == singleOldType) ||
               (oldTypeSet != null && oldTypeSet.Contains(declaring)));

           if (isMatch)
           {
               if (allowFullStateReset && HasHookSignatureChanged(declaring, newType))
                   FullResetComponentState(fiber);

               try { fiber.ComponentState?.OnStateUpdated?.Invoke(); }
               catch (Exception ex)
               { Debug.LogWarning($"[HMR] Re-render scheduling failed on '{declaring.Name}': {ex.Message}"); }
               count++;
           }
       }

       NotifyMatchingFibers(fiber.Child, singleOldType, oldTypeSet, newType, allowFullStateReset, ref count);
       NotifyMatchingFibers(fiber.Sibling, singleOldType, oldTypeSet, newType, allowFullStateReset, ref count);
   }
   ```

**Invariant preserved:** the trampoline `__hmr_Render` field is still swapped for cascade-pulled types (line 222 work happens *before* `NotifyMatchingFibers`). Hook state and effects survive across the cascade swap; the next normal re-render (from any state change or scheduler tick) will pick up the new IL. The originating-change file still gets the rude-reset semantics it had before — so authored signature changes in the file the user edited remain detected.

**Risk:** if the user saves a file whose component is *also* a transitive consumer of itself (recursive component), the walker ordering still puts the changed file last; `isOriginator = true` is correct. Worth a test fixture.

**Test:**
- Repro from the spec (copy-rename Bob/Handle, save `Bob.style.uitkx`) → assert no second `GameScene` in Hierarchy.
- Regression: edit a real signature in an isolated component (no cascade) → assert `FullResetComponentState` still fires (single-file path → `isOriginator: true`).
- Regression: edit a component that has live useEffect cleanup → assert cleanup runs on a *real* signature change.

---

### Patch 14.2 — Don't link near-clones in the dependency index

**Goal:** narrow `s_moduleReverse` so copy-rename files don't get pulled into cascade batches.

**Edits:**

**`Editor/HMR/UitkxFileDependencyIndex.cs` (~line 389-396, inside `TryIndexFile`)**:
```csharp
foreach (Match m in s_pascalDottedRegex.Matches(stripped))
{
    string name = m.Groups["name"].Value;

    // Existing: skip self-module references.
    if (node.DeclaredModules.Contains(name))
        continue;

    // NEW: skip if name matches this file's own component class name.
    // A near-clone file (e.g. Bob.uitkx copied from Marker.uitkx) often
    // has leftover `Marker.X` tokens — registering them as cross-module
    // references creates a false cascade edge into the source file.
    if (!string.IsNullOrEmpty(node.ComponentName)
        && string.Equals(name, node.ComponentName, StringComparison.Ordinal))
        continue;

    node.ReferencedModules.Add(name);
}
```

Verify `node.ComponentName` is populated before this loop runs (`TryIndexFile` extracts it earlier — confirm during implementation).

**Risk:** none — strictly narrows over-linking. Real cross-module references (where the referenced name is genuinely *not* the file's own component name) are unaffected.

**Test:** in `Editor/HMR/Tests/UitkxFileDependencyIndexTests.cs` (create if missing) — create a copy-renamed fixture `Bob.uitkx` containing leftover `Marker.Style` token; assert `CollectTransitiveDependents("Marker.uitkx")` does *not* include `Bob.uitkx`.

**Note:** this is the *interim* version — only filters the file's own component name. The fuller "executable-position only" regex restriction is deferred (USS-lexer-shaped problem, separate effort).

---

### Patch 14.3 — Emit `Container` stub for genuinely-new HMR-only components

**Goal:** eliminate CS0103 on `Container.X` references when the project DLL has no `partial class Container` for a brand-new sub-component.

**Edits:**

1. **`Editor/HMR/UitkxHmrCompiler.cs`** — add private helper:
   ```csharp
   private bool IsGenuinelyNewComponent(string componentName, string ns)
   {
       string fqn = string.IsNullOrEmpty(ns) ? componentName : ns + "." + componentName;
       foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
       {
           if (asm.IsDynamic) continue;
           var asmName = asm.GetName().Name ?? "";
           if (asmName.StartsWith("hmr_", StringComparison.OrdinalIgnoreCase)) continue;
           try
           {
               if (asm.GetType(fqn, throwOnError: false) != null) return false;
           }
           catch { }
       }
       return true;
   }

   private static string EmitContainerStub(string componentName, string ns)
   {
       // Empty partial — members come from the regularly-emitted component class.
       // The stub exists only so Roslyn can resolve `Container` as a type name
       // when companion bodies dereference `Container.X` before the SG has
       // emitted its own partial.
       var sb = new StringBuilder();
       if (!string.IsNullOrEmpty(ns))
       {
           sb.AppendLine($"namespace {ns} {{");
       }
       sb.AppendLine("    internal partial class Container { }");
       if (!string.IsNullOrEmpty(ns))
       {
           sb.AppendLine("}");
       }
       return sb.ToString();
   }
   ```

2. **`Editor/HMR/UitkxHmrCompiler.cs`** — call it inside `BuildComponentArtifacts` (the shared path for both single-file and batch), right after the main component source is emitted and before companion inlining (~line 530):
   ```csharp
   if (IsGenuinelyNewComponent(componentName, ns))
   {
       string stub = EmitContainerStub(componentName, ns);
       if (!string.IsNullOrEmpty(stub))
           artifacts.CompanionUitkxSources.Add(stub);
   }
   ```

**Risk:** medium.
- If the project later builds and the SG emits its own `partial class Container` in the same namespace, partial-class merging is fine (multiple `partial` parts of the same class are legal). No conflict.
- **Open question:** if the project-built `Container` has different accessibility (`public` vs `internal`), the partials must agree. Verify by reading the SG's `Container` emitter and matching the stub's accessibility exactly. The subagent did **not** locate a dedicated `ContainerEmitter`; the SG likely emits `Container` as a nested type of the component or as a sibling — confirm during implementation and align the stub accordingly.

**Test:** create a brand-new `Bob.uitkx` in a subtree with no prior `partial class Container`, save its companion `.style.uitkx`, assert no CS0103 in console and the swap completes.

---

## Implementation order with go/no-go gates

Sequential. Don't parallelise — each step de-risks the next.

| Order | Patch | Go gate |
|---|---|---|
| 1 | **14.4** | Build green; saving a component logs the *correct* namespace; no `Microsoft.CodeAnalysis.*Hooks` warning. |
| 2 | **14.1** | Cascade reproduce: save a near-clone subtree; Hierarchy contains exactly one `GameScene`; ancestor `useEffect`s are NOT torn down. Single-file edit still rude-resets on real signature change. |
| 3 | **14.2** | Unit test: copy-rename fixture file does not appear in `CollectTransitiveDependents` of its source. |
| 4 | **14.3** | Brand-new sub-component compiles live without CS0103. |

After step 2, the user-visible blocker is gone — steps 3 & 4 are hygiene.

---

## Regression test matrix

| Test | File / location | Verifies |
|---|---|---|
| `CopyRenameCascade_NoSceneDuplication` | new `Editor/HMR/Tests/UitkxHmrIntegrationTests.cs` | 14.1 + 14.2 |
| `OriginatingEdit_StillRudeResetsOnSignatureChange` | same file | 14.1 negative — fix didn't over-narrow |
| `Namespace_ResolvedFromCompileResult_NotReflection` | same file | 14.4 |
| `Index_NearCloneDoesNotPolluteReverseMap` | new `Editor/HMR/Tests/UitkxFileDependencyIndexTests.cs` | 14.2 |
| `NewComponent_CompilesWithoutCS0103` | `UitkxHmrIntegrationTests` | 14.3 |
| `AncestorMountEffectIdempotency_Probe` | new sample scene under `Samples/HMR/RegressionScenes/` | 14.1 end-to-end |

The `AncestorMountEffectIdempotency_Probe` scene is the highest-signal manual repro and should be added as a checked-in scene so future regressions are caught.

---

## What this plan deliberately does NOT fix

- **Emitter parity across hook signatures** (Issue 15 territory — SG vs HMR `ExtractHookSignature` *inputs* can drift even when the regex is identical, via parser-level `FunctionSetupCode` differences).
- **General cascade over-pull for legitimate cross-module references** (Fix 14.2 only handles the component-name self-clone case; the broader USS-aware stripping problem is Issue 12.2 territory).
- **`HookContainerRegistry.Seed` early-abort on `UnauthorizedAccessException`** (Bug 16.12).
- **Hand-copied SG/HMR table duplication** (Bug 16.3 / 16.5 — single source-of-truth refactor, separate effort).

These are catalogued in `Plans~/PRETTY_UI_HMR_BUGS.md` Issues 15 and 16. They do not block Issue 14's user-visible symptoms.

---

## Owner / next action

Owner: framework HMR maintainer.
Next action: implement Patch 14.4 (smallest, unblocks the log noise), validate via the namespace-resolution test, then Patch 14.1 with the originating-change flag plumbing.
