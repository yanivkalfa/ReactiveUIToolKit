---
name: 'Discord changelog'
description: 'Style and constraints for Plans~/DISCORD_CHANGELOG.md entries.'
applyTo: '**/DISCORD_CHANGELOG.md'
---

# Discord changelog

Rules for editing [Plans~/DISCORD_CHANGELOG.md](../../Plans~/DISCORD_CHANGELOG.md).
The file is the source for what gets pasted into the project's Discord
release-notes channel.

## Hard constraints

- **Each release entry must be <= 2000 characters** (Discord per-message
  limit). Count from the release header up to - but not including - the next
  entry's header or the trailing `---` separator. If the entry is too long,
  cut prose, not facts.
- **Always prepend.** New entries go at the top of the file. Never reorder
  or delete prior entries.
- **ASCII-only.** No emojis, no bullet glyphs (`*`, `x`, `>`, etc.), and
  **no non-ASCII characters at all** - including em-dash, en-dash, arrows,
  smart quotes, ellipsis, middot, or any decorative typography. Use plain
  ASCII alternatives:
  - em-dash / en-dash -> ` - ` (hyphen with spaces) or `--`
  - right arrow -> `->`
  - left/right arrow -> `<->`
  - middot separator -> ` | ` or ` * ` or ` . `
  - ellipsis -> `...`
  - curly quotes -> straight `'` and `"`
  This rule has no exceptions. Windows/PowerShell pipelines and Discord
  syntax highlighting can mangle non-ASCII bytes silently, leaving
  replacement chars (`?` or U+FFFD) in production posts.
- **Backticks for diagnostic IDs and code identifiers** (`UITKX0026`,
  `CS0019`, `FindLhsStartForLogicalAnd`).
- **No `//` C# line comments inside fenced code blocks** unless they carry
  meaning a reader needs (most lines should be self-explanatory). Use plain
  prose lines instead.

## Entry shape

```md
## [x.y.z] - YYYY-MM-DD

### Subject - short descriptor

**Bold lead-in.** One paragraph describing the user-facing impact and why
it mattered.

```jsx
// optional minimal repro fenced block, only when the syntax change benefits
<Box>{flag && <Label text="hi"/>}</Box>
```

Follow-up paragraph(s) describing implementation strategy at a high level
(walker, splicer, parity), diagnostic IDs, and any deferred work tracked
in `Plans~/TECH_DEBT_V2.md`.

**Fix - short headline.** Paragraph for any secondary fix shipping in the
same release.

**Tests.** One line summarising the test delta and the SG suite total
(`1198/1198 SG passing`).

VS Code **a.b.c -> a.b.d** | VS 2022 **a.b.c -> a.b.d**.

---
```

## Style notes

- The `---` separator at the bottom of each entry is **not** counted toward
  the 2000-char budget but must be present.
- Reference the previous release closest to the new entry as the
  authoritative style template - match its register and density.
- Don't restate boilerplate facts (`React idiom`, `walker is precedence-aware`)
  more than once per entry.
- If a fix and a feature ship together, lead with the feature, then a
  `**Fix -**` paragraph for the fix.
- Verify size after writing: count chars from `## [x.y.z]` up to (not
  including) the trailing `---`.
