# Unity Asset Store publishing plan — ReactiveUIToolKit

> **Status: RESEARCHED (2026-07-05), not started.** How to get ReactiveUIToolKit onto the Unity
> Asset Store, and how much of it can be automated through a publish workflow. Bottom line up
> front: **listing + first submission are manual; per-release artifact build + validation are
> fully automatable in CI; the final upload+submit click is manual today** (Unity ships no
> official CLI/API as of Asset Store Publishing Tools v12.0.0, Jan 2025) — same situation as
> Godot's new Asset Store, and we solve it the same way: automate everything up to the click.

## 1. What the store requires (one-time)

1. **Publisher account** at publisher.unity.com (create publisher profile: name, URL, support
   email; payout/tax info only needed if the asset is paid).
2. **Package draft** (the listing): title, description, category (Tools/GUI or
   Scripting/Integration), keywords, and **key images** — icon 160×160, card 420×280, cover
   1950×1300, social 1200×630, plus screenshots (and optionally a YouTube link — the demo video
   exists already). Same discipline as the Godot store thumbnails; assets can be generated from
   the existing branding.
3. **Documentation is a hard requirement** — a docs link satisfies it: https://reactiveuitoolkit.info/.
4. **Quality bars from the submission guidelines**: no errors/warnings from package content after
   setup, professional presentation, no trademark misuse, ≤6 GB (irrelevant here). Content is
   human-reviewed: **~10 business days for a new submission, ~2 for updates**.

## 2. This repo's package shape (mostly ready)

- The store uploader wants "all the assets in a top-level folder that has the same name as your
  package" — we already are exactly that: `Assets/ReactiveUIToolKit/`.
- **`~` folders solve the monorepo problem for free**: `SourceGenerator~/`, `ide-extensions~/`,
  `ReactiveUIToolKitDocs~/`, `Plans~/` are not assets, so they are not part of an asset-store
  export. What ships is the Unity-visible surface: `Runtime/`, `Shared/`, `Editor/`, `Samples/`,
  `Diagnostics/`, `Analyzers/` (the committed generator DLLs ship as plugins — correct, they are
  the product). **Validate on first export** that nothing tilde'd leaks and nothing needed is
  missing.
- Pre-submission checklist specific to us:
  - [ ] Fresh-project import test: package imports with **zero errors/warnings** (the guideline
        reviewers enforce), samples compile, a sample scene renders.
  - [ ] `.uitkx` files import inertly for users without the toolchain (they do — generator DLLs
        handle them).
  - [ ] README/quick-start inside the package folder points at the docs site.
  - [ ] Decide Unity floor to declare (docs currently say 6.2+).

## 3. Upload mechanics (the official path)

Per the official docs (docs.unity.com → Asset Store → upload): install the **Asset Store
Publishing Tools** package (github.com/Unity-Technologies/com.unity.asset-store-tools, v12.0.0),
then in the Editor: **Tools → Asset Store → Validator** (checks guideline compliance) → **Tools →
Asset Store → Uploader** → log in → pick the package draft → select `Assets/ReactiveUIToolKit` →
**Export and Upload** (it builds and uploads the `.unitypackage`) → publisher portal → **Submit
for review**.

## 4. Automation — the honest tiers

**Tier A (recommended now, fully supported): automate everything except the click.**
- Extend the repo's publish workflow with a `build-unitypackage` job:
  1. Run Unity headless in CI (game-ci `unity-builder`/docker images or a self-hosted runner;
     needs a `UNITY_LICENSE`/`UNITY_EMAIL`+`UNITY_PASSWORD` secret set — personal license
     activation is the standard game-ci flow).
  2. `-batchmode -executeMethod` a small editor script that runs our own validation (import
     clean, no console errors, version in `package.json` matches the tag) and
     `AssetDatabase.ExportPackage("Assets/ReactiveUIToolKit", recurse)` →
     `ReactiveUIToolKit-<version>.unitypackage`.
  3. Attach it to the GitHub release. This artifact doubles as a non-store distribution channel.
  4. Final step mirrors the Godot new-store reminder: emit a `::notice` "upload
     ReactiveUIToolKit-<ver>.unitypackage via Tools → Asset Store → Uploader and Submit" —
     a ~5-minute manual step per release, and updates only take ~2 business days of review.
- Note: the store uploader re-exports from the project itself rather than consuming a prebuilt
  `.unitypackage`, so the CI artifact's role is validation + distribution + drift-proofing (the
  human uploads from a checkout of the same tag).

**Tier B (experimental, evaluate later): headless upload via internal APIs.**
- No official CLI exists (verified against the v12 README and docs — UI-only). Community projects
  drive the internal uploader from `-batchmode -executeMethod`:
  - `FredericRP/BatchSubmitter` — configurable package uploads in batch mode;
  - `thinksquirrel/asset-store-batch-mode` — the original, now ancient (pre-v11 backend, likely dead).
  Since the official tools are source-available on GitHub, the durable variant of this tier is a
  small editor script calling **the v12 tools' own upload internals** (what BatchSubmitter does,
  but pinned to the current package). Risks, eyes open: undocumented API (can break on any tools
  release), CI holds real Unity credentials (use a dedicated automation account with publisher
  org access), and "Submit for review" still happens in the portal. Do this only after Tier A has
  run smoothly for a few releases and the manual click is genuinely the bottleneck.

**Tier C (watch): an official API.** Nothing announced as of mid-2026. Revisit at each Asset
Store Tools major.

**Complementary channels that ARE fully automatable today** (worth doing regardless):
- **OpenUPM** — publishes from git tags automatically once the package is registered; gives UPM
  users `openupm add` installs with proper versioning.
- **Git-URL UPM installs** — already work from the repo; document them on the listing.

## 5. Release-flow integration

- Versioning stays as-is (`package.json`, SemVer, patch-by-default; CHANGELOG.md via
  `scripts/changelog.mjs`). The store listing's version field mirrors `package.json` at upload.
- Cadence guard: store updates cost a manual click + ~2-day review, so batch store pushes to
  meaningful versions (minor releases + important patches) rather than every patch — the GitHub
  release/UPM channels stay per-release.
- Sequence for going live: create publisher + draft + images (manual, ~1 evening) → Tier A CI job
  lands → first submission → survive review (expect one rejection round; fix, resubmit) → add the
  "store update" checklist item to the release process → evaluate Tier B later.

## 6. Open decisions (user's)

| Decision | Options | Notes |
|---|---|---|
| Price | Free vs paid | Free maximizes adoption pre-1.0; store allows switching later. Paid requires payout/tax setup. |
| Publisher name | "ReactiveUITK" (matches VS marketplaces) vs personal | Match existing branding. |
| Declared Unity floor | 6.2+ (current docs claim) | Reviewer will test on the floor version. |
| Store cadence | Every release vs minors-only | See §5. |

## Sources

- Official upload flow + validator: https://docs.unity.com/en-us/asset-store/publishing/asset-packages/upload
- Asset Store Publishing Tools (v12.0.0, no CLI): https://github.com/Unity-Technologies/com.unity.asset-store-tools
- Submission guidelines: https://assetstore.unity.com/publishing/submission-guidelines
- Review timelines (~10bd new / ~2bd updates): https://support.unity.com/hc/en-us/articles/210569723-How-long-will-it-take-for-my-Asset-to-be-approved
- Community batch uploaders: https://github.com/FredericRP/BatchSubmitter , https://github.com/thinksquirrel/asset-store-batch-mode
- Publisher onboarding: https://assetstore.unity.com/publishing/publish-and-sell-assets
