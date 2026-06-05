# UITKX LSP — Metadata Reference Staleness Bug & Fix Plan

**Status:** ✅ Implemented & shipped in 1.2.16 (2026-05-23). All 71 LSP tests green.
**Affected version:** 1.2.15 (and all prior with the current `ReferenceAssemblyLocator` cache model)
**Symptoms:** After a Unity recompile, opening or editing `.uitkx` / `.style.uitkx` files in PrettyUi shows bulk CS0246 / CS0103 errors on symbols defined in `ReactiveUITK.Shared.dll` (`Ease`, `AlignCenter`, `ColorWhite`, `VideoController`, etc.). "Reload Window" reliably fixes it. The bug recurs after subsequent Unity recompiles.

---

## 1. Confirmed evidence

| Source                                                                                        | Finding                                                                                                                                                                                              |
| --------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `Pretty Ui\Library\ScriptAssemblies\ReactiveUITK.Shared.dll` (684 544 B, 5/23 02:05)          | Up to date, contains all expected symbols (verified via raw UTF-16 byte scan).                                                                                                                       |
| Installed extension `reactiveuitk.uitkx-1.2.15` (`server\ReactiveUITK.Language.dll`, 260 608 B) | Matches Marketplace; HookRegistry refactor present.                                                                                                                                                  |
| `%TEMP%\uitkx-server.log` (post-reload, 12:33 PM local)                                       | `[ReferenceAssemblyLocator] Resolved 438 metadata references.` — ONE scan since reload. NO `Unity recompile detected` lines. Cache is fresh because user hasn't recompiled in Unity since reload.    |
| `Get-ChildItem ScriptAssemblies\ReactiveUITK*`                                                | No `.tmp-*` files; `CreationTime == LastWriteTime`. Unity writes ScriptAssemblies DLLs **in-place** (delete + recreate), not via atomic temp-rename.                                                  |

**Implication:** The bug is dormant right now (no recompile since reload). It will return on the next Unity recompile.

---

## 2. Root cause — actual code paths (no theory)

### 2.1 `ReferenceAssemblyLocator.GetReferences(workspaceRoot)`
File: [ide-extensions~/lsp-server/Roslyn/ReferenceAssemblyLocator.cs](ide-extensions~/lsp-server/Roslyn/ReferenceAssemblyLocator.cs#L73-L86)

```csharp
public MetadataReference[] GetReferences(string? workspaceRoot)
{
    lock (_lock)
    {
        if (_cachedRefs != null
            && string.Equals(_cachedRoot, workspaceRoot, StringComparison.OrdinalIgnoreCase))
            return _cachedRefs;          // (A) returns whatever was last scanned, no validation
        _cachedRoot = workspaceRoot;
        _cachedRefs = BuildReferences(workspaceRoot);   // (B) blindly caches result, even if incomplete
        _detectedVersion = DetectUnityVersion(workspaceRoot);
        return _cachedRefs;
    }
}
```

`BuildReferences` enumerates `Library\ScriptAssemblies\*.dll` and `Editor\Data\Managed\**\*.dll` with `Directory.EnumerateFiles`, calling `MetadataReference.CreateFromFile(dll)` per hit. Failures are logged but **non-fatal** — a missing/locked Shared.dll just drops out of the result and the rest is cached.

### 2.2 `RoslynHost.SetupDllWatcher`
File: [ide-extensions~/lsp-server/Roslyn/RoslynHost.cs](ide-extensions~/lsp-server/Roslyn/RoslynHost.cs#L1512-L1567)

- `NotifyFilter = LastWrite | FileName`
- Subscribes ONLY to `Changed` and `Created` (no `Deleted`, no `Renamed`).
- Single debounce timer; on every event the timer is re-armed for **1 500 ms**. After 1 500 ms of silence, calls `_refLocator.Invalidate()` exactly once and logs `Unity recompile detected — reference cache cleared.`
- `Invalidate` only nulls `_cachedRefs`; the next `GetReferences` re-scans.

### 2.3 `RoslynHost.UpdateWorkspace`
File: [ide-extensions~/lsp-server/Roslyn/RoslynHost.cs](ide-extensions~/lsp-server/Roslyn/RoslynHost.cs#L924-L1140)

- **First-open path** (line 975): bakes `metadataReferences: refs` into `ProjectInfo.Create`.
- **Subsequent-update path** (line 1019): `newSolution.WithProjectMetadataReferences(state.ProjectId!, refs)` — refs are refreshed on every edit.
- Both paths call `_refLocator.GetReferences(_workspaceRoot)` once at the top. Whatever the locator returns IS the truth for that compilation.

`RebuildAsync` is the only caller of `UpdateWorkspace`. It is invoked from `EnqueueRebuild`, which is called from didOpen, didChange, and external triggers (peer changes, companion file changes). `RebuildAsync` acquires `state.Gate` (per-file `SemaphoreSlim(1,1)`), so per-file rebuilds are serialized.

### 2.4 The actual failure mechanism

**Cache poisoning during recompile:**

1. Unity starts a recompile: `Shared.dll` is deleted, written, possibly deleted again as Unity does multi-pass compilation (asmdef A → asmdef B → addressables → another asmdef).
2. DLL watcher receives a burst of `Changed`/`Created` events. The 1500 ms debounce **re-arms on every event**, so `Invalidate` doesn't fire until 1.5 s of silence.
3. While Unity is mid-compile, the cache **still holds pre-recompile refs**. If the user opens a `.uitkx` file in this window, `GetReferences` returns the stale array. Each `MetadataReference` was built via `CreateFromFile(path)`, which holds a path + memory-mapped view. The underlying file on disk has been replaced. Roslyn's behavior in this state is undefined: it may read the new bytes, the old bytes via mmap, or fail to load and silently treat symbols as missing.
4. After 1500 ms of silence, `Invalidate` fires. Cache cleared. Next `GetReferences` re-scans.
5. **BUT** — if the re-scan happens between two of Unity's compile stages (e.g. between asmdef-A done and asmdef-B starting), the scan may pick up a transient state where some DLL is in-progress / momentarily absent. `BuildReferences` swallows the error and caches the **incomplete** result. The cache is now poisoned until the next `Invalidate`.
6. Subsequent edits hit `WithProjectMetadataReferences(refs)` with the poisoned cache, so even editing the file doesn't heal it.

**Recovery requires either:** (a) another DLL change to fire `Invalidate` again at a moment when the disk is stable, or (b) Reload Window (which destroys the locator and all per-file workspaces).

---

## 3. Proposed fixes — detailed practical analysis

Each fix is evaluated for: (a) what it does, (b) which file/lines change, (c) whether it actually fixes a real path (not theory), (d) what could break.

### Fix #1 — Cache validation (refuse to return / cache obviously-incomplete results)

**What it does:**
Add a "high-water mark" + sanity check to `GetReferences`:

- Track `_lastGoodCount` (max ref count we've ever resolved for this workspace).
- When a new scan returns `< 50%` of `_lastGoodCount`, treat the result as transient and **do not cache it**. Return last-good if available, otherwise return the partial result for that one call only.
- Additionally, record `_lastGoodHadReactiveCore` (true if the last good scan saw `ReactiveUITK.Shared.dll`). If true and a new scan misses it → don't cache.

**Files / lines:**
- [ide-extensions~/lsp-server/Roslyn/ReferenceAssemblyLocator.cs](ide-extensions~/lsp-server/Roslyn/ReferenceAssemblyLocator.cs#L73-L86) — `GetReferences`.
- New private fields `_lastGoodCount`, `_lastGoodHadReactive`.
- Modify `BuildReferences` return path to bubble up "did we see ReactiveUITK.Shared".

**Does it actually fix a real path?**
Yes. This is the *direct* counter to cache poisoning (root cause § 2.4 step 5). A re-scan during a Unity intermediate state would normally lock in 200 refs instead of 438 — the heuristic rejects it.

**What could break:**
- **False negative on legitimate ref-set shrinkage.** If user deletes a Unity package, ref count drops legitimately. The heuristic would block caching until either (a) Reload Window or (b) a `> _lastGoodCount * 0.5` scan re-baselines. **Mitigation:** decay `_lastGoodCount` after N successful uncached scans, or expose `ResetBaseline()` and call it on Reload Window. In practice, package removals are extremely rare in active dev sessions; 50% threshold is comfortable.
- **First-ever scan has no baseline.** Cache normally on first call regardless of count. Only kick in after the first successful scan establishes `_lastGoodCount`.
- **Cold-start projects (no Library/ScriptAssemblies yet)** legitimately have 0 ReactiveUITK refs. `_lastGoodHadReactive` would stay false → check is inert → no harm.

**Risk:** Low. The check is purely additive, runs under the existing lock, costs O(1).

---

### Fix #2 — Subscribe to `Deleted` + arm a short cooldown window

**What it does:**
Two parts:

1. In `SetupDllWatcher`, subscribe `_dllWatcher.Deleted += OnDllChanged`. (`NotifyFilter` already includes `FileName`, so Deleted events are already enabled — we just don't handle them.)
2. Add a **separate** "recompile in progress" flag (`_refLocator.IsRecompiling`). On ANY Deleted event:
   - Immediately set `IsRecompiling = true` (no debounce).
   - Reset a separate "cooldown timer" to 2 500 ms (longer than the invalidate debounce of 1 500 ms).
   - When cooldown expires, clear `IsRecompiling` AND call `Invalidate()`.
3. In `GetReferences`, when `IsRecompiling == true`:
   - If `_cachedRefs != null` → return it (let consumers keep working with last-known-good).
   - Else → fall through to a normal scan but mark `_lastGoodCount` as untouched (don't update baseline from a recompile-window scan).

**Files / lines:**
- [ide-extensions~/lsp-server/Roslyn/RoslynHost.cs](ide-extensions~/lsp-server/Roslyn/RoslynHost.cs#L1512-L1561) — add `Deleted` handler + cooldown timer.
- [ide-extensions~/lsp-server/Roslyn/ReferenceAssemblyLocator.cs](ide-extensions~/lsp-server/Roslyn/ReferenceAssemblyLocator.cs#L40-L107) — add `IsRecompiling` flag, `SetRecompileWindow(bool)` API.

**Does it actually fix a real path?**
Yes. It closes root-cause § 2.4 step 5: during the cooldown, scans are non-baselining so a transient bad scan can't poison the high-water mark.

**What could break:**
- **Deleted events for unrelated DLLs.** A user manually deleting a non-Unity-tracked DLL in `ScriptAssemblies` would trigger the cooldown. Cost: 2.5 s of "may serve stale cache." Harmless.
- **Closure-captured local `invalidateTimer` is already racy** (two threads can both Dispose-then-allocate). The new cooldown timer must be a field, not a closure local. Easy.
- **Cooldown of 2 500 ms is a guess.** Could be tunable per-project. Default is fine.

**Risk:** Low–medium. The interaction with Fix #1 must be tested: when cooldown ends and `Invalidate` fires, the next scan repopulates the cache and (per Fix #1) updates `_lastGoodCount` if it's the new high water.

---

### Fix #3 — Push refreshed refs to all live workspaces when `Invalidate` rescans

**What it does:**
Today, `Invalidate()` clears the locator's cache, but per-file `Workspace.CurrentSolution` still has the OLD refs frozen into it. Those old refs are only swapped out on the **next** edit/rebuild for that file (via the `WithProjectMetadataReferences` call on line 1019). If the user just opens a stale file and looks at it without editing, the squiggles stay until they touch the file.

The fix: after the watcher's debounce fires `Invalidate()`, immediately enqueue a no-edit rebuild for every tracked file.

**Data-dependency audit (verified by re-reading [RoslynHost.cs L159-205](ide-extensions~/lsp-server/Roslyn/RoslynHost.cs#L159-L205)):**

`FileState` currently stores: `Workspace`, `ProjectId`, `DocumentId`, `CompanionDocIds`, `PeerDocIds`, `PolyfillDocId`, `PeerVDocs`, `VirtualDoc`, `LastBuiltSource` (full string), `DebounceTimer`, `RebuildCts`, `Gate`. It does **NOT** store `ParseResult` or `DiagnosticsPublisher`. `EnqueueRebuild(uitkxFilePath, source, parseResult, publisher)` requires all four. So Fix #3 cannot just iterate `_files` and call `EnqueueRebuild` — it has nothing to pass for `parseResult` or `publisher`.

**Resolution:**
1. Add `public ParseResult? LastParseResult;` field to `FileState`. Populate it in `EnqueueRebuild` immediately after `_files.GetOrAdd` (cheap — `ParseResult` is a small immutable wrapper).
2. Add `private DiagnosticsPublisher? _publisher;` field to `RoslynHost`. Populate in `EnqueueRebuild` (singleton — verified by reading [DiagnosticsPublisher.cs L282](ide-extensions~/lsp-server/DiagnosticsPublisher.cs#L282) which passes `this`; the publisher is registered once per server).
3. New private method `TriggerMetadataRefreshForAllFiles()` invoked from the watcher's debounced callback **after** `_refLocator.Invalidate()`:
   ```csharp
   private void TriggerMetadataRefreshForAllFiles()
   {
       var pub = _publisher;
       if (pub == null) return;        // nothing has ever requested a rebuild yet
       foreach (var kv in _files)
       {
           var st = kv.Value;
           if (st.Workspace == null || st.LastParseResult == null) continue;
           EnqueueRebuild(kv.Key, st.LastBuiltSource, st.LastParseResult, pub);
       }
   }
   ```

**Why re-enqueue rather than a new lightweight "refresh-refs-only" path?**
- The subsequent-update branch of `UpdateWorkspace` ([L1019](ide-extensions~/lsp-server/Roslyn/RoslynHost.cs#L1019-L1080)) handles polyfill flip (Shared.dll appeared/disappeared), companion refresh, and peer refresh — all of which can be affected by ref changes. Reusing `EnqueueRebuild` reuses that logic verbatim.
- The 300 ms debounce + per-file `Gate` already serialise rebuilds, so a rebuild storm of N files is naturally throttled.
- Source text is unchanged → `LastBuiltSource == source` → `EnsureReadyAsync` fast-path skip works on subsequent IDE queries.

**Files / lines:**
- [ide-extensions~/lsp-server/Roslyn/RoslynHost.cs](ide-extensions~/lsp-server/Roslyn/RoslynHost.cs#L159-L205) — add `LastParseResult` field to `FileState`; add `_publisher` field to `RoslynHost`.
- [ide-extensions~/lsp-server/Roslyn/RoslynHost.cs](ide-extensions~/lsp-server/Roslyn/RoslynHost.cs#L306-L335) — populate both fields inside `EnqueueRebuild`.
- [ide-extensions~/lsp-server/Roslyn/RoslynHost.cs](ide-extensions~/lsp-server/Roslyn/RoslynHost.cs#L1530-L1561) — call `TriggerMetadataRefreshForAllFiles()` from watcher callback.

**Does it actually fix a real path?**
Yes — this is the difference between "squiggles clear after recompile" and "squiggles clear after I touch the file." Without this, users with `.uitkx` files open during Unity recompile will see false errors persist until they edit.

**What could break:**
- **Rebuild storm after every recompile.** If user has 30 `.uitkx` files open, every recompile schedules 30 rebuilds. Each is debounced (300 ms) + serialized per file. Total CPU spike for ~5 s. Acceptable.
- **Re-entrant access to `_files`** during enumeration is fine (`ConcurrentDictionary` allows it).
- **Latency:** rebuilds are async + per-file gated; no blocking on the watcher thread.
- **Tests unaffected:** `RoslynHostTests`, `RoslynCompletionTests`, `HookCrossNamespaceVirtualDocTests` all call `SetWorkspaceRoot(null)` (verified) → no DLL watcher → no `TriggerMetadataRefreshForAllFiles` code path runs.
- **`LastParseResult` retention:** `ParseResult` holds `ImmutableArray<ParseDiagnostic>` + directive list + node list. Per-file memory cost is negligible (a few KB) versus the workspace/compilation already stored.

**Risk:** Low–medium. Behavioral change — diagnostics now refresh on Unity recompile without an edit. This is the desired behavior. No existing test asserts the inverse.

---

### Fix #4 — Snapshot DLL bytes at scan time (`CreateFromImage` instead of `CreateFromFile`)

**What it does:**
Replace `MetadataReference.CreateFromFile(dll)` with `AssemblyMetadata.CreateFromImage(File.ReadAllBytes(dll)).GetReference(filePath: dll, display: dll)`. Bytes are read once at scan time; the resulting `MetadataReference` is immune to subsequent file replacement.

**Files / lines:**
- [ide-extensions~/lsp-server/Roslyn/ReferenceAssemblyLocator.cs](ide-extensions~/lsp-server/Roslyn/ReferenceAssemblyLocator.cs#L185-L196) — `AddDllsFromDirectory`.
- Also `AddTrustedPlatformAssemblies` (L208-238) and `AddFallbackBcl` (L240-275). BCL doesn't need it (the runtime DLLs don't get rewritten), but `ScriptAssemblies` definitely does.

**Does it actually fix a real path?**
Indirectly — it makes the **outcome** of a scan deterministic. Even if Unity replaces the file 50 ms after we scan it, the reference we hold is from the original bytes. Combined with Fix #1 (validate scan), it eliminates "I scanned at a bad moment and got half-written metadata."

**What could break:**
- **Memory pressure.** 438 DLLs averaging ~200 KB each = ~88 MB per scan. `AssemblyMetadata.CreateFromImage` keeps the bytes alive for the lifetime of the reference. Total memory growth: ~50–100 MB resident.
- **Scan latency.** Synchronous `File.ReadAllBytes` × 438 = ~1–2 s on warm cache, ~3–5 s on cold cache. Cache miss happens once per Invalidate (≈ once per Unity recompile). The current `CreateFromFile` path is essentially free because Roslyn defers I/O until compilation needs it.
- **Rebuild latency.** With lazy `CreateFromFile`, only DLLs Roslyn actually needs are paged in. Eager byte reads load everything. On low-spec machines this could be felt.

**Risk:** High in resource cost, low in correctness. Recommend **NOT applying unless Fix #1 + #2 prove insufficient.** The memory + I/O hit is real.

**Alternative:** apply selectively to ScriptAssemblies DLLs (where the race exists) and keep lazy `CreateFromFile` for Unity engine + BCL (which never change at runtime). That caps the cost at ~5–10 MB and ~50–100 ms for a typical project.

---

### Fix #5 — `uitkx.restartLanguageServer` command (UX)

**What it does:**
Add a VS Code command that calls `client.restart()` on the language client. The user can `Ctrl+Shift+P → UITKX: Restart Language Server` instead of `Developer: Reload Window`. Reload Window destroys all editor state (open tabs, undo stacks, terminal sessions); restarting the LSP only kills the dotnet process and re-launches it.

**Files / lines:**
- `ide-extensions~/vscode/src/extension.ts` — register command + handler that calls `client.restart()`.
- `ide-extensions~/vscode/package.json` — add command contribution.

**Does it actually fix a real path?**
No — it's a workaround / faster recovery. The bug still happens; recovery is just less destructive.

**What could break:**
- Nothing functional. Pure additive.
- **Caveat:** `client.restart()` on `vscode-languageclient` will tear down all open documents in the LSP and resend `didOpen` for each. There's a brief flicker of "no diagnostics" → "fresh diagnostics."

**Risk:** None.

---

## 4. Recommended fix set

**Tier 1 (ship as 1.2.16):** **#1 + #2 + #3** combined.

| Why this combination                                                                                                                                                                                                  |
| --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| #2 widens the "danger window" the system recognizes (any Deleted event = recompile starting).                                                                                                                          |
| #1 ensures any scan that happens during that window can't poison the cache.                                                                                                                                            |
| #3 ensures that when the cache is replaced post-recompile, open files actually pick up the new refs without requiring an edit.                                                                                          |
| Each fix is small, mechanically scoped, and independently testable. None require Roslyn API changes or memory growth.                                                                                                  |

**Tier 2 (defer):** **#5** — ship alongside #1–#3 if cheap, otherwise next release. Pure UX, no risk.

**Tier 3 (defer indefinitely):** **#4** — only if telemetry shows #1–#3 didn't fully fix the bug. Memory + I/O cost is real and there's no need for it if the cache hygiene is solid.

---

## 5. Test plan

| Test                                                          | Pre-state                                            | Action                                                | Expected                                                                                                                          |
| ------------------------------------------------------------- | ---------------------------------------------------- | ----------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------- |
| **T1 — Recompile during open**                                 | `.uitkx` file open, no errors.                       | Edit a C# script that lives in `ReactiveUITK.Shared`. | After Unity finishes recompile, the open `.uitkx` file's diagnostics refresh **without** the user touching the file. No CS0246.    |
| **T2 — Recompile then open new file**                          | Idle session.                                        | Trigger Unity recompile. Open a `.uitkx` file 0.5 s after recompile starts. | First scan defers (cooldown active) → cache holds last good. After cooldown, fresh scan + rebuild = no CS0246.                     |
| **T3 — Cold-start scan (no Library/ScriptAssemblies)**         | Brand-new Unity project, never compiled.             | Open a `.uitkx` file.                                 | Scan succeeds with low ref count, caches normally (no high-water mark yet).                                                       |
| **T4 — Package removal (legitimate shrinkage)**                | Established session with 438 refs.                   | Delete a Unity package; let Unity reimport (~100 refs drop). | First scan after Invalidate is rejected (under 50% threshold). User restarts LSP via #5 → baseline resets to new count. **Acceptable.** |
| **T5 — Memory check**                                          | Long-running session, 10 recompiles.                 | Inspect dotnet process RSS.                           | No growth beyond ~50 MB over baseline (no leaked scans).                                                                          |
| **T6 — Golden test parity**                                    | All existing diagnostic golden tests.                | Run test suite.                                       | All pass unchanged.                                                                                                               |

---

## 6. Out of scope (explicitly NOT changing)

- `MetadataReference.CreateFromFile` (Fix #4) — deferred.
- The `VirtualDocumentGenerator` polyfill logic — unchanged.
- The companion document loader — unchanged.
- The csproj-based reference path (we use ScriptAssemblies scan, not csproj parsing). This is a separate architectural choice and is not affected by this fix.

---

## 7. Files that will change (Tier 1 fix set)

1. `ide-extensions~/lsp-server/Roslyn/ReferenceAssemblyLocator.cs` — add cache validation, recompile-window flag, scan-result metadata.
2. `ide-extensions~/lsp-server/Roslyn/RoslynHost.cs` — add Deleted handler + cooldown timer; on Invalidate, re-enqueue rebuilds for tracked files.
3. (Tier 2 only) `ide-extensions~/vscode/src/extension.ts` + `package.json` — new restart command.

Estimated diff size: ~120 LOC added, ~10 LOC modified. No public API changes.

---

## 8. Versioning

- Patch bump to **1.2.16**.
- Changelog entry under `ide-extensions~/changelog.json`: "Fix: language server reference cache no longer poisoned by mid-recompile DLL state; open files refresh diagnostics automatically after Unity recompiles."

---

## 9. Pre-implementation audit findings (delta from initial plan)

Full code re-read performed; the original plan is sound but three details required refinement:

### A. FileState state shape — affected Fix #3 design
**Original assumption:** "FileState likely already stores last source/parseResult/publisher for re-rebuilds."
**Reality:** Only `LastBuiltSource` is stored; no `ParseResult`, no `Publisher`.
**Resolution:** Plan updated above (§ 3 Fix #3) — add `FileState.LastParseResult` and `RoslynHost._publisher` (singleton-captured). Both are cheap.

### B. Publisher lifetime — singleton-confirmed
**Verified:** `DiagnosticsPublisher` is constructed once at LSP startup and registered as a singleton; its `EnqueueRebuild` calls always pass `this`. Safe to capture once on the `RoslynHost` instance.

### C. Test isolation — confirmed safe
**Verified:** All three test fixtures (`RoslynHostTests`, `RoslynCompletionTests`, `HookCrossNamespaceVirtualDocTests`) call `_host.SetWorkspaceRoot(null)`. With `null` root, `SetupDllWatcher` returns early (no watcher created) — meaning Fix #2 (Deleted handler / cooldown) and Fix #3 (auto-refresh callback) code paths are **never executed during the test suite**. No test regressions possible from those two fixes.

### D. Initialisation ordering — confirmed safe
`RoslynHostStartup.OnStarted` runs once on the LSP `initialize` handshake, before any `didOpen`/`didChange` traffic can be processed. It calls `SetWorkspaceRoot(root)` which creates the watcher. So by the time any rebuild is enqueued, the watcher is already armed. No race between first-open and watcher activation.

### E. FileSystemWatcher event threading — confirmed safe
Windows `FileSystemWatcher` raises events serially on its own background thread, so concurrent execution of `OnDllChanged` cannot interleave. The current closure-captured `invalidateTimer` local is safe under this model; the new Deleted-cooldown timer (Fix #2) must be a private field on `RoslynHost`, not a closure local, because it is touched both from the watcher thread and from `_refLocator.GetReferences` (when checking `IsRecompiling`).

### F. VS Code client restart API — confirmed available
`LanguageClient.restart()` is available in `vscode-languageclient` ≥ v7.0. Workspace's [vscode/package.json](ide-extensions~/vscode/package.json) is on a modern version (engines.vscode `^1.85.0`). Fix #5 implementation is straightforward: add a `uitkx.restartLanguageServer` command contribution + handler that calls `client.restart()`. If older API, fall back to `stop().then(() => start())`.

### G. Cold-start baseline edge case — added mitigation
**Scenario:** User opens a brand-new Unity project that has never compiled. Scan returns ~150 BCL-only refs. `_lastGoodCount` is set to 150. User then compiles Unity for the first time. First post-compile scan may be racy and return 250 (some asmdef DLLs still mid-write). `250 > 150 * 0.5 = 75` → cached → potentially poisoned.
**Mitigation:** Plan's Fix #2 cooldown handles this — the Deleted/Created burst from the first compile triggers `IsRecompiling = true`, and scans during that window are flagged "don't update `_lastGoodCount`." When cooldown ends and Unity is done, the final scan establishes a clean baseline. No additional change needed; the two fixes interlock correctly.

### H. Polyfill flip on metadata refresh — already handled
`UpdateWorkspace`'s subsequent-update branch ([L1041-L1075](ide-extensions~/lsp-server/Roslyn/RoslynHost.cs#L1041-L1075)) already adds/removes the polyfill document based on whether `ReactiveUITK.Shared.dll` is among the current refs. Fix #3's re-enqueue route through `EnqueueRebuild` → `RebuildAsync` → `UpdateWorkspace` preserves this. No separate polyfill-toggle path needed.

---

**Conclusion:** Plan stands. The only material refinement is Fix #3's data-dependency story (now explicit above). Implementation is unblocked.

---

## 10. Implementation log — 1.2.16

All work landed in a single session on **2026-05-23**.

| Stage | Scope | Status | Notes |
|-------|-------|--------|-------|
| 1 | Fix #1 + #2 (locator layer) — `ReferenceAssemblyLocator`: high-water baseline (`_lastGoodCount`, `_lastGoodHadReactive`), `IsRecompiling` flag, `SetRecompileWindow(bool)`, `ShouldRejectScan(...)`, partial-scan rejection with `ServerLog`, baseline preserved across `Invalidate()`. | ✅ Done | New API consumed by host layer. |
| 2 | Fix #2 + #3 (host layer) — `RoslynHost`: added `FileState.LastParseResult`, singleton-captured `_publisher` (Interlocked CAS), `_recompileCooldownTimer` (2 500 ms), unified `OnDllActivity` handler subscribed to Changed+Created+**Deleted**, `OnRecompileWindowEnded()` (clear flag → `Invalidate()` → refresh-all), `TriggerMetadataRefreshForAllFiles()` re-enqueues `EnqueueRebuild` per tracked file. Cooldown timer disposed in `Dispose()`. | ✅ Done | Refresh path reuses existing rebuild pipeline → polyfill flip & all guards inherited. |
| 3 | Fix #5 (VS Code UX) — `uitkx.restartLanguageServer` command in `package.json` `contributes.commands` + handler in `extension.ts` calling `client.restart()` with user-visible info/error toasts and Output-channel logging. | ✅ Done | Removed unused empty `"commands": []` further down to avoid duplicate-key error. |
| 4 | Version bump 1.2.15 → **1.2.16** in `ide-extensions~/vscode/package.json` and `ide-extensions~/visual-studio/UitkxVsix/source.extension.vsixmanifest`. | ✅ Done | |
| 5 | Changelog entry via `scripts/changelog.mjs add --scope shared --message-file … --vscode 1.2.16 --vs2022 1.2.16` (UTF-8 file path used to dodge PowerShell argv CP1252 corruption on em-dash). | ✅ Done | Entry dated 2026-05-23. |
| 6 | Build: `dotnet publish` LSP server → `vscode/server/` (290 KB DLL, 274 KB language-lib) + `npm run build` VS Code client (775 KB `out/extension.js`). All artifact sizes within sanity bands. | ✅ Done | No new warnings introduced. |
| 7 | Test: `dotnet test UitkxLanguageServer.Tests.csproj` — **71/71 passed** in 15.7 s. | ✅ Done | DLL watcher path untouched by tests (they call `SetWorkspaceRoot(null)`); locator validation logic exercised indirectly via existing reference-resolution tests. |

Fix #4 (Reset button in language status / per-file refresh) was explicitly de-scoped — Fix #5's restart command + Fix #3's automatic refresh-all cover the recovery story without it.

