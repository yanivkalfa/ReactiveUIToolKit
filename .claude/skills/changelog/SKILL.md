---
name: changelog
description: The centralized IDE-extension changelog system. Use when adding a changelog entry, bumping extension versions, releasing, or editing any of ide-extensions~/changelog.json, scripts/changelog.mjs, the per-IDE CHANGELOG.md, vscode/README.md, or UitkxVsix/overview.md. Covers the add/extract/extract-overview/verify commands, the generated-marketplace-pages rule (edit templates, never outputs), and the Windows argv mojibake pitfalls.
---

# Centralized Changelog System

The source of truth for all IDE extension changelogs is `ide-extensions~/changelog.json`.
Everything else is **generated** from it:

| Generated file | Template | Regenerate with |
|---|---|---|
| `ide-extensions~/vscode/CHANGELOG.md` | (none — plain extract) | `extract --ide vscode` |
| `ide-extensions~/vscode/README.md` (VS Code Marketplace + Open VSX page body) | `ide-extensions~/vscode/readme-template.md` | `extract-overview --ide vscode` |
| `ide-extensions~/visual-studio/UitkxVsix/overview.md` (VS2022 Marketplace page body) | `ide-extensions~/visual-studio/UitkxVsix/overview-template.md` | `extract-overview --ide vs2022` |

**Never hand-edit a generated file.** Edit the TEMPLATE (or `changelog.json`),
regenerate, and commit template + output together. `node scripts/changelog.mjs verify`
is the drift gate — it recomposes each generated page from its template + the json and
byte-compares against the committed file, printing the exact regeneration command on
drift. Run it before any commit that touches this system.

## Adding a changelog entry

```bash
# inline message (ASCII-only — see Windows note below)
node scripts/changelog.mjs add --scope <shared|vscode|vs2022|rider> --message "description" [--vscode X.Y.Z] [--vs2022 X.Y.Z] [--rider X.Y.Z] [--date YYYY-MM-DD]

# or read message from a UTF-8 file (preferred for any non-ASCII content)
node scripts/changelog.mjs add --scope <shared|vscode|vs2022|rider> --message-file <path-to-utf8-file> [--vscode X.Y.Z] [--vs2022 X.Y.Z] [--rider X.Y.Z] [--date YYYY-MM-DD]
```

If an entry with the same date + versions already exists, `add` appends the message
to it (that's how one release accumulates multiple bullets).

### Windows / PowerShell argv pitfalls (use `--message-file`)

When invoking the script through PowerShell or `cmd.exe`, argv is transcoded
through the active code page (typically CP1252) before Node receives it. This
silently corrupts non-ASCII characters in `--message`:

- `—` (em-dash, U+2014) becomes mojibake
- `é`, curly quotes, NBSP, ellipsis, etc. all undergo similar damage
- Embedded double-quotes are stripped by PowerShell's argv parser, which
  truncates the message at the first quote

The script guards itself: it **refuses** messages containing CP1252→UTF-8 mojibake
fingerprints or U+FFFD, and points at `--message-file`, which reads the bytes
verbatim as UTF-8 from disk (CRLF normalised to LF, trailing newlines stripped).

**Rule of thumb:** anything beyond plain ASCII (em-dashes, accented letters, quoted
phrases, backticks, multi-line content) → write the message to a scratch file and
use `--message-file`.

### Scope rules

- `--scope shared` — LSP server, language-lib, grammar changes that affect **all** IDEs. Include version flags for every IDE that will ship this change.
- `--scope vscode` — VS Code extension-specific changes (extension.ts, package.json, etc.)
- `--scope vs2022` — Visual Studio extension-specific changes (UitkxVsix, ActivateAsync, etc.)
- `--scope rider` — Rider plugin-specific changes

### Version flags

- Include `--vscode X.Y.Z` and/or `--vs2022 X.Y.Z` for every IDE releasing this change
- The version must match `package.json` (VS Code) / `source.extension.vsixmanifest`
  (VS 2022) — bump those first, then add the entry with the new numbers

## Regenerating outputs

```bash
# VS Code CHANGELOG.md
node scripts/changelog.mjs extract --ide vscode --out ide-extensions~/vscode/CHANGELOG.md

# VS Code Marketplace README (template + changelog section)
node scripts/changelog.mjs extract-overview --ide vscode --template ide-extensions~/vscode/readme-template.md --out ide-extensions~/vscode/README.md

# VS 2022 overview.md (template + changelog section)
node scripts/changelog.mjs extract-overview --ide vs2022 --template ide-extensions~/visual-studio/UitkxVsix/overview-template.md --out ide-extensions~/visual-studio/UitkxVsix/overview.md

# Drift gate — run before committing
node scripts/changelog.mjs verify
```

## Message format

- Start with category: `Fix:`, `Feature:`, `Breaking:`, `Revert:`
- Be user-facing: describe what changed for the user, not implementation details
- End shared entries with the test totals (`SG suite N/N, LSP suite N/N`)

## Related (but separate) changelogs — keep them ALL in sync per release

- `CHANGELOG.md` (repo root) — the Unity PACKAGE changelog (source of truth for
  package versions; hand-written, Keep-a-Changelog format)
- `Plans~/DISCORD_CHANGELOG.md` — Discord release posts (see the `discord-changelog`
  skill; hard 2000-char-per-entry cap)
- The mental model: feature → version bumps → all changelogs → docs, in one commit
  wave — never let them drift.

## Marketplace listing constraint

The vsixmanifest `<Description>` must stay **under 280 characters** — the VS
Marketplace hard-rejects longer (VsixPub0024, learned in Publish #109). VS Code's
`description` has no such cap but is kept identical for parity.
