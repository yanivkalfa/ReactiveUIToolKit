# Automation & AI Tooling

This project includes automation tooling for maintaining Unity version compatibility.

## When a New Unity Version is Released

### For AI (Copilot Chat)

In VS Code, open Copilot Chat and type:

```
/add-unity-version Unity 6.5 (6000.5) has been released
```

This runs a structured prompt that walks through discovery, classification, and implementation.
The prompt is at `.github/prompts/add-unity-version.prompt.md`.

### For Humans

1. **Run the diff script** to see what changed in UI Toolkit:

   ```powershell
   .\automation~\unity-api-diff.ps1 -From 6000.4 -To 6000.5 -OutFile .\automation~\diff-reports\6000.4-to-6000.5.json
   ```

   Both Unity versions must be installed via Unity Hub. The script uses assembly
   reflection — no web scraping, no fragile HTML parsing. 100% accurate.

2. **Review the JSON report** — it lists every added/removed/changed IStyle property,
   VisualElement subclass, enum, and struct.

3. **Follow the implementation checklists** in `Plans~/VERSIONING_PROCESS.md` §4.

## Folder Layout

| Path | What | Audience |
|------|------|----------|
| `.github/prompts/add-unity-version.prompt.md` | Copilot prompt — full runbook | AI |
| `automation~/unity-api-diff.ps1` | Assembly diff script (PowerShell) | Both |
| `automation~/diff-reports/` | Generated JSON reports (gitignored) | Both |
| `automation~/README.md` | Detailed usage docs | Both |
| `Plans~/VERSIONING_PROCESS.md` | Full reference: sources, checklists, matrix | Human |
| `ReactiveUIToolKitDocs~/src/versionManifest.ts` | Docs version manifest (single source of truth) | Both |

---

## Documentation Website Versioning

The docs site (`reactiveuitoolkit.info`) has a version-aware system driven by
`ReactiveUIToolKitDocs~/src/versionManifest.ts`.

### How it works

- A **version dropdown** in the top bar lets users select their Unity version.
- The **sidebar and search** filter pages to only show content available for
  that version.
- **Unity doc links** point to the correct docs version (e.g. 6000.2 vs 6000.3).
- **Style property tables** show version badges for non-floor properties.

### When adding a new Unity version to docs

1. Add an entry to `SUPPORTED_VERSIONS` in `versionManifest.ts`
2. Add entries to `STYLE_PROPERTY_VERSIONS` for any new IStyle properties
3. Add rich entries to `STYLE_PROPERTY_DETAILS` for each new IStyle property
   (type, description, code example, related CssHelpers)
4. Add entries to `ELEMENT_VERSIONS` for any new elements
5. Add entries to `CSS_HELPER_VERSIONS` for any new CssHelpers
6. Add entries to `PAGE_VERSIONS` for any new doc pages
7. If new elements were added, create a **full component page** with usage
   examples, props table, and `sinceUnity` set — not just a table entry
8. Update the **C# track** pages as well (`pages.tsx` / legacy sections) —
   the C# side of the docs is a separate track and also needs version content
9. Verify: `cd ReactiveUIToolKitDocs~ && npm run build`

### Conventions

- **Code block language:** Always use `language="tsx"` for embedded code
  examples (matches the site-wide highlighting config). Never use `"csharp"`.
  Exception: the CssHelpers / Enum shortcuts tables are rendered as MUI tables
  (not CodeBlocks) so this rule doesn't apply there.
- **Style properties:** New style properties need rich documentation cards
  (type, description, example, related helpers) — not just a name + badge table.
- **New elements:** Each new element/component requires a dedicated doc page
  with usage examples, not just a row in a table.

### Styling Page Architecture

The Styling page (`/uitkx/styling`) has a data-driven design with three layers:

```
stylePropertyCatalog.ts  ──>  StylingPage.tsx  ──>  rendered cards
StyleKeys.cs             ──>  (must stay in sync)
versionManifest.ts       ──>  version filtering
```

**Files and their roles:**

| File | Purpose |
|------|------|
| `ReactiveUIToolKitDocs~/src/pages/UITKX/Styling/stylePropertyCatalog.ts` | **Single source of truth** for all style properties shown on the page. Array of `PropertyCard` objects with: `key` (camelCase), `name` (PascalCase), `type`, `description`, `typedExample`, `untypedExample`, `helpers?`, `category`, `sinceUnity?`, `shorthand?` |
| `ReactiveUIToolKitDocs~/src/pages/UITKX/Styling/StylingPage.tsx` | Renders the page. Filters the catalog by selected Unity version, applies search, renders collapsible MUI Accordion cards. Has anchor sections: `#patterns`, `#type-reference`, `#csshelpers-reference`, `#enum-shortcuts` |
| `ReactiveUIToolKitDocs~/src/pages/UITKX/Styling/StylingPage.example.ts` | Large code example constants used by the page (setup, patterns, inline, both APIs) |
| `Shared/Props/Typed/StyleKeys.cs` | C# `const string` definitions — **must have a 1:1 match** with the catalog. Every `name` in the catalog must exist as a `StyleKeys.{Name}` constant. 6.3+ keys use `#if UNITY_6000_3_OR_NEWER` guards. |
| `ReactiveUIToolKitDocs~/src/versionManifest.ts` | Provides `isAvailableIn()` used to filter cards by version. Properties with `sinceUnity` are hidden when the user selects an older version. |

**How cards render:**

- Each card is a MUI `Accordion` (collapsed by default — header shows name + version chip + caret).
- **Version chips**: floor properties get a grey `"6.2"` chip (`color="default"`), non-floor get a blue `"6.3+"` chip (`color="info"`).
- Cards are sorted: version first (floor before newer), then alphabetically.
- A `TextField` at the top filters cards by name, key, or description.
- The `untypedExample` field uses `StyleKeys.{Name}` format (not raw strings) so docs match actual C# usage.

**When adding a new Unity version — Styling Page checklist:**

1. Add entries to `STYLE_PROPERTY_CATALOG` in `stylePropertyCatalog.ts`:
   ```ts
   { key: 'camelKey', name: 'PascalName', type: 'StyleType',
     category: 'Filter & Effects', sinceUnity: '6000.X',
     description: '...', typedExample: 'PascalName = ...',
     untypedExample: '(StyleKeys.PascalName, ...)', helpers: [...] }
   ```
2. Add matching constant to `StyleKeys.cs` inside a `#if UNITY_6000_X_OR_NEWER` guard:
   ```csharp
   public const string PascalName = "camelKey";
   ```
3. The page auto-renders — no changes to `StylingPage.tsx` needed.
4. The CssHelpers / Enum tables in `StylingPage.tsx` are inline JSX (MUI tables),
   not data-driven. If a new enum or helper is added, update the table rows directly.
