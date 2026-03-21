# UITKX Formatter — Whitespace & Indentation Normalisation

**Status:** Living document — update after every formatter fix.  
**Scope:** `ide-extensions~/language-lib/Formatter/AstFormatter.cs` (shared across VS Code, Rider, Visual Studio).  
**Last updated:** v1.0.169  
**Test file:** `SourceGenerator~/Tests/FormatterSnapshotTests.cs`

---

## Table of Contents

1. [Architecture Overview](#1-architecture-overview)
2. [The Two Code Paths](#2-the-two-code-paths)
3. [Key Methods & Their Roles](#3-key-methods--their-roles)
4. [How Indentation Normalisation Works](#4-how-indentation-normalisation-works)
5. [How Whitespace Normalisation Works](#5-how-whitespace-normalisation-works)
6. [How to Diagnose a New Formatting Bug](#6-how-to-diagnose-a-new-formatting-bug)
7. [How to Fix a Formatting Bug — Step by Step](#7-how-to-fix-a-formatting-bug--step-by-step)
8. [Test Infrastructure](#8-test-infrastructure)
9. [Pitfalls & Design Constraints](#9-pitfalls--design-constraints)
10. [History of Fixes (v1.0.162–169)](#10-history-of-fixes-v10162169)
11. [Quick Reference: Where Things Live](#11-quick-reference-where-things-live)

---

## 1. Architecture Overview

The UITKX formatter converts `.uitkx` source files into canonical form. It works in three stages:

```
Source text ──► Parser (UitkxParser) ──► AST (AstNode tree) ──► AstFormatter ──► Formatted output
```

The formatter handles **two fundamentally different content types** inside a single `.uitkx` file:

| Content type | Example | Formatting strategy |
|---|---|---|
| **JSX / Markup** | `<Box style={style}>`, `<Text text="hi"/>` | AST-driven: walk the `AstNode` tree and emit canonical tags, attributes, indentation |
| **C# setup code** | `var count = Hooks.UseState(0);` | Line-by-line re-indentation: the formatter works on raw lines, NOT on a C# AST |

The C# setup code lives inside the function-style `component Name { ... return (...); }` block, above the `return (` line. This is the part that causes most formatting bugs, because:

- **We do NOT use Roslyn** to format C# code. Roslyn was tested and rejected because it changes brace style from K&R to Allman, reformats lambda arguments, and generally destroys the relative indentation that UITKX conventions require.
- Instead, we use a **custom line-by-line re-indentation engine** that normalises indentation depths while preserving continuation-line offsets.

---

## 2. The Two Code Paths

When the formatter encounters C# setup code, it routes through **one of two methods** depending on whether the setup code contains embedded JSX (markup inside parentheses):

```
component MyComp {
    var x = 5;                          // ─┐
    var items = GetStuff();             //  │── This is "setup code"
    MenuBuilderHandler buildMenu = dm => // │
    {                                   //  │
        dm.AppendAction("Open", ...);   // ─┘
    };

    var panel = (                       // ─┐── This is "JSX in setup"
        <Box>                           //  │   (C# that contains markup)
            <Text text="hi"/>           //  │
        </Box>                          //  │
    );                                  // ─┘

    return (
        <Root>...</Root>
    );
}
```

### Decision point (in `AstFormatter.Format` → `FormatFunctionStyleComponent`):

```
if (hasJsxInSetup)
    EmitSetupCodeWithJsx(...)    →  splits at JSX boundaries, formats each C# segment
                                     via EmitCSharpLines(), formats JSX via FormatNodeList()
else
    EmitSetupCodeNormalized(...) →  formats the entire setup block as pure C#
```

### CRITICAL: Both paths must apply the same normalisation rules

If you fix something in `EmitSetupCodeNormalized`, you **must** also check whether the same fix is needed in `EmitCSharpLines` (and vice versa). These two methods are near-identical in structure but are NOT shared code. Bugs that are fixed in only one path will surface when the other path is triggered.

**How to tell which path a file uses:**
- Open the `.uitkx` file
- If the setup code contains `var x = (<SomeTag ...>)` or similar markup-in-parentheses → `EmitSetupCodeWithJsx` path
- If the setup code is pure C# (no JSX) → `EmitSetupCodeNormalized` path

---

## 3. Key Methods & Their Roles

### `EmitSetupCodeNormalized(string code, string tabExp)` (~line 708)
- Handles setup code that is **pure C#** (no embedded JSX)
- Line-by-line loop with stack-based block tracking
- Calls `CollapseIntraLineSpaces()` on each line
- Uses `IsStatementStarter()` to classify depth-0 lines
- Uses `prevWasStatementStarter` flag for Allman brace handling

### `EmitCSharpLines(string code, string tabExp, bool firstLineStripped, bool suppressLastNewline)` (~line 444)
- Handles C# segments that come from `EmitSetupCodeWithJsx`
- Nearly identical logic to `EmitSetupCodeNormalized` but with additional features:
  - `firstLineStripped` parameter (first line was already trimmed by caller)
  - `suppressLastNewline` parameter (last line may continue on the same line as JSX paren-open)
  - `caseExtra` / `caseExtraStack` for switch-case indentation
  - `blockAnchorStack` for continuation lines inside blocks
  - `isLambdaStack` to distinguish lambda blocks from object-initialiser blocks

### `CollapseIntraLineSpaces(string line)` (~line 938)
- **Normalises whitespace within a single line** (not leading indent — that's handled by the caller)
- Three transformations:
  1. Collapses runs of 2+ consecutive spaces → single space
  2. Strips spaces immediately after `(`
  3. Strips spaces immediately before `)`
- Preserves content inside string literals (`"..."`, `@"..."`, `$@"..."`) and after `//` comments
- Called by both `EmitCSharpLines` and `EmitSetupCodeNormalized`

### `IsStatementStarter(string s)` (~line 871)
- Determines whether a trimmed depth-0 line begins a **new C# statement** vs. being a continuation
- Used to decide whether a line should be placed at `rel=0` (base indent) or offset from the previous statement's indent
- Three recognition strategies:
  1. **Keyword prefix:** matches against `s_statementKeywords` array (all C# keywords + type names)
  2. **Trailing `;`:** any line ending with semicolon is a complete statement
  3. **Trailing `{` with content:** e.g., `if (x) {` — but NOT a bare `{` alone
  4. **Contains ` = ` (space-equals-space):** catches custom-type assignments like `MenuBuilderHandler buildMenu = dm =>`

### `s_statementKeywords` array (~line 906)
- Comprehensive list of C# keyword prefixes that signal a statement opener
- Includes: `var `, `void `, `if (`, `foreach `, `return `, all primitive types (`int `, `string `, `bool `, etc.), access modifiers, `async `, `await `, etc.
- **Does NOT include custom type names** — those are handled by the ` = ` assignment check instead

---

## 4. How Indentation Normalisation Works

### Depth-0 Lines (outside any `{ }` block)

```
DEPTH 0 DECISION TREE:
┌──────────────────────────────────────────────────────────┐
│ Is this line a "statement starter"?                      │
│   (keyword prefix, ends with ;, ends with {+content,    │
│    or contains ' = ')                                    │
├── YES ──────────────────────────────────────────────────►│  rel = 0 (flush to base indent)
│   Record this line's INPUT indent as the new anchor      │
│   Set prevWasStatementStarter = true                     │
├── NO ───────────────────────────────────────────────────►│
│   Is this line a bare "{" AND prevWasStatementStarter?   │
│   ├── YES ──────────────────────────────────────────────►│  rel = 0 (Allman brace at same level)
│   ├── NO ───────────────────────────────────────────────►│  rel = inputLead - anchor
│   │   (preserves continuation line offset relative       │   (ternary arms, method chains, etc.)
│   │    to the most-recent statement opener)              │
└──────────────────────────────────────────────────────────┘
```

Example:
```csharp
var result = someCondition          // ← statement starter → rel=0, sets anchor
    ? trueValue                     // ← continuation → rel = (inputLead - anchor)
    : falseValue;                   // ← continuation → rel = (inputLead - anchor)
MenuBuilderHandler buildMenu = dm =>  // ← statement starter (has ' = ') → rel=0
{                                   // ← bare '{', prevWasStatementStarter → rel=0
    dm.AppendAction("Open", ...);   // ← inside block, handled by blockStack
}                                   // ← leading '}', pops blockStack
```

### Inside `{ }` Blocks (depth > 0)

Lines inside blocks are normalised to `blockStack.Peek() + caseExtra`:
- `blockStack.Peek()` = the column at which the block opener placed its content (opener's emitted column + `IndentSize`)
- `caseExtra` = additional indent for `case`/`default` bodies inside `switch` blocks
- Continuation lines (starting with `?`, `:`, `.`) preserve their relative offset from the last non-continuation line via `lastBlockAnchor`

### The `prevWasStatementStarter` Flag

This flag enables **Allman-style brace handling**:
```csharp
// Input (user typed):
MenuBuilderHandler buildMenu = dm =>
    {                                    // ← user indented this incorrectly
        dm.AppendAction("Open", ...);    // ← this gets over-indented as a result
    };

// Output (formatter fixes):
MenuBuilderHandler buildMenu = dm =>
{                                        // ← pulled back to rel=0
    dm.AppendAction("Open", ...);        // ← normalised inside block
};
```

Without the flag, the bare `{` would be treated as a continuation line and would keep whatever offset the user gave it, causing all inner lines to be over-indented.

---

## 5. How Whitespace Normalisation Works

### `CollapseIntraLineSpaces` — The State Machine

This method processes each character of a line with two boolean flags:

| Flag | Meaning |
|---|---|
| `afterOpen` | We just emitted a `(` outside strings. Any spaces that follow should be skipped entirely. |
| `pendingSp` | We encountered a space but haven't emitted it yet. We defer emission so we can absorb it if the next non-space char is `)`. |

**Character handling rules (outside strings/comments):**

| Character | Action |
|---|---|
| `(` | Flush any pending space, append `(`, set `afterOpen = true` |
| space | If `afterOpen`: skip entirely. Otherwise: set `pendingSp = true` (defer emission) |
| `)` | Absorb pending space (set `pendingSp = false`), append `)` |
| Any other char | If `pendingSp`: emit the space first. Append the char. Set `afterOpen = (char == '(')` |
| `"` | Enters string-literal mode (`inStr = true`). All chars inside strings pass through unchanged. |
| `//` | Flush pending space, then append the rest of the line verbatim. |

**Quick-exit optimisation:** If the line contains none of `"  "` (double space), `"( "` (open-paren-space), or `" )"` (space-close-paren), return the line immediately without allocating a `StringBuilder`.

### Examples:

| Input | Output | Rule |
|---|---|---|
| `for ( int i = 0; i > 0; i-- )` | `for (int i = 0; i > 0; i--)` | `( ` stripped, ` )` stripped |
| `setCount(v => v + 1 );` | `setCount(v => v + 1);` | ` )` stripped |
| `( StyleKeys.FlexGrow, 1f ),` | `(StyleKeys.FlexGrow, 1f),` | `( ` stripped, ` )` stripped |
| `"Hello ( world )"` | `"Hello ( world )"` | Inside string: unchanged |
| `DoWork( a, b, c );` | `DoWork(a, b, c);` | `( ` stripped, ` )` stripped, multi-space collapsed |

### Tab Handling

Before `CollapseIntraLineSpaces` is called, intra-line tabs (`\t` within the stripped content, not leading whitespace tabs) are converted to spaces:
```csharp
if (stripped.IndexOf('\t') >= 0)
    stripped = stripped.Replace('\t', ' ');
```
This ensures that `DoWork(\ta,\tb,\tc\t)` becomes `DoWork(a, b, c)`.

---

## 6. How to Diagnose a New Formatting Bug

### Step 1: Determine which code path is involved

1. Open the `.uitkx` file that has the formatting issue
2. Look at the setup code (everything above `return (`)
3. If it contains `var x = (<SomeTag ...>)` or similar JSX-in-parens → **EmitSetupCodeWithJsx** → **EmitCSharpLines**
4. If it's pure C# → **EmitSetupCodeNormalized**
5. If the issue is in the JSX/markup below `return (` → it's in the AST-driven path (`FormatNodeList`), not the line-based C# formatter

### Step 2: Classify the bug type

| Symptom | Likely cause | Where to look |
|---|---|---|
| Line is indented too much or too little | `IsStatementStarter` not recognising the line, or `baseSpaces` / `anchor` calculation wrong | `EmitCSharpLines` / `EmitSetupCodeNormalized` depth-0 logic |
| Bare `{` on its own line is indented wrong | `prevWasStatementStarter` not being set for the preceding line | `IsStatementStarter` — add recognition for the new pattern |
| Inner lines of a `{ }` block are over/under-indented | `blockStack` push/pop mismatch, or `caseExtra` not being reset | Block-tracking logic in `EmitCSharpLines` |
| Extra spaces inside a line (e.g., `(  x,  y )`) | `CollapseIntraLineSpaces` not handling the pattern | `CollapseIntraLineSpaces` state machine |
| Formatting is correct in one file but wrong in another with similar code | Different code path being used (JSX vs. non-JSX) | Fix must be applied to BOTH methods |

### Step 3: Create a minimal reproduction

Write a test case that captures the exact input and expected output. Use the test helpers:

```csharp
// For the EmitSetupCodeNormalized path (no JSX):
[Fact]
public void MyBug_Description()
{
    string input = N(@"component Test {
    // paste the problematic setup code here with its bad formatting
    return (<Box/>);
}
");
    string expected = N(@"component Test {
  // paste what the correctly-formatted output should look like
  return (
    <Box/>
  );
}
");
    Assert.Equal(expected, Format(input));
}
```

```csharp
// For the EmitCSharpLines path (with JSX in setup):
[Fact]
public void MyBug_JSX_Path()
{
    string input = N(@"component Test {
    // code that triggers JSX path
    var panel = (
        <Box/>
    );
    // more code with the bug
    return (<Box/>);
}
");
    // ... same pattern
}
```

### Step 4: Run the failing test in isolation

```powershell
cd SourceGenerator~/Tests
dotnet test --filter "MyBug_Description"
```

### Step 5: Debug with the decision tree

For indentation issues, trace through the depth-0 decision tree manually:
1. What does `IsStatementStarter(stripped)` return for the problem line?
2. What is `prevWasStatementStarter` at that point?
3. What is `lastStatementInputIndent`?
4. What `rel` value is computed?

For whitespace issues, trace through `CollapseIntraLineSpaces` character by character with the `afterOpen`/`pendingSp` flags.

---

## 7. How to Fix a Formatting Bug — Step by Step

### 1. Write failing test(s) FIRST

Write at least one test that reproduces the exact problem. Verify it fails before making code changes. Place the test in the appropriate section of `FormatterSnapshotTests.cs` (see [Section 8](#8-test-infrastructure) for the section layout).

### 2. Identify the fix location

Use the [diagnostic guide](#6-how-to-diagnose-a-new-formatting-bug) above to determine exactly which method and which logic branch is involved.

### 3. Make the MINIMAL change

Common fix patterns:

**A) A new type of line isn't recognised as a statement starter:**
- Add a check to `IsStatementStarter()` — either a new keyword to `s_statementKeywords` or a new heuristic check
- **DANGER:** If adding a broad pattern (like "line ends with `=>`"), test it against ALL sample files! Some patterns match continuation lines too. Prefer narrow checks (like `" = "` which naturally excludes `+=`, `==`, etc.)

**B) A whitespace pattern isn't being normalised:**
- Modify `CollapseIntraLineSpaces()` — add detection in the quick-exit check AND handling in the state machine loop
- **DANGER:** Always preserve content inside string literals and after `//` comments

**C) Block indentation is wrong:**
- Check the `blockStack` push/pop logic — ensure `leadClose` counting and stack operations are correct
- Check `caseExtra` / `caseExtraStack` for switch-case related issues

### 4. Apply the fix to BOTH code paths

If you changed `EmitCSharpLines`, check `EmitSetupCodeNormalized` and vice versa. If you changed `CollapseIntraLineSpaces`, it's automatically shared. If you changed `IsStatementStarter`, it's automatically shared.

### 5. Run the FULL test suite

```powershell
cd SourceGenerator~/Tests
dotnet test
```

Expect some existing tests to fail because their assertions now reflect the old (wrong) behaviour. Update those assertions to match the new correct output.

### 6. Check all sample files

The idempotency tests (Section A) format every `.uitkx` file in `Samples/UITKX/` and verify the output is identical to the input. If a sample file has the same bad pattern you just fixed, those tests will fail. You need to:

1. Format the sample file to see what the new output looks like
2. If the new output is correct, update the sample file to match (re-baseline it)
3. If the new output is wrong, your fix is too broad — refine it

### 7. Add regression tests for the new fix

Add tests that cover:
- The exact pattern that was broken (positive test)
- Edge cases (empty parens, nested structures, strings containing the pattern)
- Both code paths (non-JSX and JSX) if applicable
- Patterns that should NOT be affected by your fix (negative tests)

### 8. Rebuild and publish

```powershell
cd <workspace-root>
powershell -ExecutionPolicy Bypass -File scripts/publish-extension.ps1 `
    -ChangelogEntry "Fix: <description>" -BumpVersion
```

This builds the language server (dotnet publish), builds the VS Code extension (npm run build), packages the VSIX, installs it locally, and publishes to the VS Marketplace.

---

## 8. Test Infrastructure

### Test file location
`SourceGenerator~/Tests/FormatterSnapshotTests.cs`

### Test runner
```powershell
cd SourceGenerator~/Tests
dotnet test                          # run all tests
dotnet test --filter "V01"           # run a specific test
dotnet test --filter "FAIL"          # find all failing tests
dotnet test --filter "DisplayName~Section_V"  # run a section
```

### Section layout (as of v1.0.169, 693 tests)

| Section | Topic | Count | Description |
|---------|-------|-------|-------------|
| A | Idempotency | 54 | Every sample `.uitkx` file formatted twice must be identical |
| B–L | Structural regression | ~200 | JSX structure, directives, control flow, components, edge cases |
| M | C# setup indentation (FMT-1) | 20 | The core indentation normalisation tests |
| N | Blank-line preservation | 8 | Blank lines between setup statements |
| O | Mixed depth | 8 | Nested blocks, case statements |
| P | Continuation lines | 10 | Ternary, method chains, multi-line expressions |
| Q | (reserved) | — | — |
| R | Trailing-brace handling | 6 | `},` continuation after lambda vs. object initialiser |
| S | Multi-space collapse | 12 | `CollapseIntraLineSpaces` for non-JSX path |
| T | Multi-space collapse (JSX) | 5 | Same tests through the `EmitCSharpLines` path |
| U | Allman brace normalisation | 10 | Bare `{` pulled back when preceded by a statement starter |
| V | Paren-space normalisation | 10 | `( x )` → `(x)` stripping |
| W | Custom-type assignment | 10 | `MyType name = val =>` recognised as statement starter |

### How to add a new test section

1. Pick the next unused letter (e.g., X)
2. Add tests **before the `// ═══ R)` section** or at the end — sections are ordered alphabetically in the file
3. Use `[Fact]` for single tests or `[Theory] + [InlineData]` for parameterised tests
4. Follow the naming convention: `X01_ShortDescription`, `X02_AnotherCase`
5. Use the helper `Format(input)` for full-file formatting, or construct the component wrapper manually for targeted tests

### Writing effective test assertions

```csharp
// GOOD: Show the full component wrapper so the code path is clear
string input = N(@"component Test {
    for ( int i = 0; i < n; i++ ) {
        DoWork( i );
    }
    return (<Box/>);
}
");
string expected = N(@"component Test {
  for (int i = 0; i < n; i++) {
    DoWork(i);
  }
  return (
    <Box/>
  );
}
");

// BAD: Partial snippet — unclear which code path runs
string input = "for ( int i = 0; i < n; i++ )";
```

---

## 9. Pitfalls & Design Constraints

### Why not Roslyn?

Roslyn (`Microsoft.CodeAnalysis.CSharp`) was evaluated for C# formatting and **rejected** because:
1. It changes brace style from K&R (`{ ` on same line) to Allman (`{` on new line) — conflicts with UITKX conventions
2. It reformats lambda arguments and method-call indentation in ways that break the relative indentation we need
3. It's fundamentally incompatible with the "preserve user's relative indentation" philosophy

The Roslyn infrastructure still exists in `ide-extensions~/language-lib/Roslyn/` and the `ICSharpFormatterDelegate` interface, but it is **not used** by the formatter.

### The "too broad" trap

When adding pattern recognition to `IsStatementStarter`:
- **`=>` as line-ending was tried and failed.** Lines like `(from, to) =>` inside `Hooks.UseBlocker()` callbacks are continuation lines, not statement starters. Adding `=>` as a line-ending check caused these to be pulled to `rel=0`, destroying the indentation.
- **` = ` (space-equals-space) works** because it naturally excludes compound operators (`+=`, `-=`, `==`, `!=`, `>=`, `<=`, `??=`, `=>`) — none of these produce the pattern space-equals-space.
- **Lesson:** Always test new heuristics against ALL 54 sample files. A pattern that looks safe on one file may break another.

### Two code paths must stay in sync

`EmitCSharpLines` and `EmitSetupCodeNormalized` share the same high-level logic but are separate methods. Any fix to one must be verified against the other. `EmitCSharpLines` has more features (caseExtra, blockAnchorStack, isLambdaStack) because it was enhanced later — consider whether `EmitSetupCodeNormalized` needs the same enhancements.

### String literals are sacred

`CollapseIntraLineSpaces` tracks `inStr` / `verbatim` state to avoid modifying content inside string literals. Any change to the whitespace normalisation logic **must** preserve this. Test with strings containing `( `, ` )`, and multiple spaces.

### The `baseSpaces` calculation

Both emit methods compute `baseSpaces` — the minimum leading whitespace of depth-0, non-continuation, non-comment lines. This anchors continuation lines relative to their statement opener. Getting this wrong causes cascading indentation errors. The calculation intentionally excludes:
- Lines starting with `?`, `:`, `.` (continuation lines)
- Lines starting with `//` or `/*` (comments — CSharpier often indents these differently)
- Blank lines

### IndentSize = 2

The formatter uses `IndentSize = 2` (from `FormatterOptions.Default`). The internal `_indent` counter represents indent LEVELS, not spaces. `IndentStr()` = `new string(' ', _indent * _opts.IndentSize)`.

---

## 10. History of Fixes (v1.0.162–169)

| Version | Problem | Fix | Methods changed |
|---------|---------|-----|-----------------|
| 1.0.162 | Formatter first introduced for C# setup code | Initial `EmitSetupCodeNormalized` with block-stack | `EmitSetupCodeNormalized` |
| 1.0.163–166 | Various structural fixes | Iterative improvements | Multiple |
| 1.0.167 | Block-stack normalisation in JSX path, `caseExtra` for switch-case, lambda detection, continuation line handling | Added `caseExtraStack`, `blockAnchorStack`, `isLambdaStack` to `EmitCSharpLines` | `EmitCSharpLines` |
| 1.0.168 | Multi-space collapse (2+ spaces → 1), conditional bare-brace pulling (Allman `{`), tab→space conversion | Added `CollapseIntraLineSpaces()`, `prevWasStatementStarter` flag | `CollapseIntraLineSpaces`, `EmitCSharpLines`, `EmitSetupCodeNormalized` |
| 1.0.169 | Spaces after `(` / before `)` not stripped; custom-type declarations not recognised as statements | Rewrote `CollapseIntraLineSpaces` with `afterOpen`/`pendingSp` state machine; added ` = ` check to `IsStatementStarter`; removed old standalone `( ` stripping | `CollapseIntraLineSpaces`, `IsStatementStarter`, `EmitCSharpLines`, `EmitSetupCodeNormalized` |

### v1.0.169 changes in detail

**1. `CollapseIntraLineSpaces` rewrite:**
- Old: only collapsed runs of 2+ spaces to 1 space (using `lastSp` boolean)
- New: uses `pendingSp` (deferred space emission) + `afterOpen` (after `(`, skip all spaces) state machine
- New: strips `( x` → `(x` and `x )` → `x)`
- Quick-exit updated to also check for `"( "` and `" )"` patterns

**2. Standalone `( ` stripping removed:**
- Old: both `EmitCSharpLines` (line ~544) and `EmitSetupCodeNormalized` (line ~810) had code that stripped 3+ spaces after `(` at line start only
- Removed because `CollapseIntraLineSpaces` now handles ALL positions and 1+ spaces (not just 3+)

**3. `IsStatementStarter` — ` = ` check added:**
- Added `if (t.IndexOf(" = ", StringComparison.Ordinal) >= 0) return true;`
- Catches: `MenuBuilderHandler buildMenu = dm =>`, `Action<int> cb = x =>`, `Func<bool> check = () =>`
- Does NOT catch (by design): `x += 5;`, `x == y`, `x != y`, `x >= y`, `x <= y`, `x ??= y`, `(from, to) =>`
- First attempt used `=>` as line-ending check — this was **reverted** because it was too broad (broke `Hooks.UseBlocker()` continuations)

---

## 11. Quick Reference: Where Things Live

| What | File | Line (approx) |
|------|------|---------------|
| Formatter entry point | `AstFormatter.cs` | `Format()` ~line 50 |
| Function-style component routing | `AstFormatter.cs` | `FormatFunctionStyleComponent()` ~line 135 |
| C# lines (JSX path) | `AstFormatter.cs` | `EmitCSharpLines()` ~line 444 |
| C# setup (no JSX) | `AstFormatter.cs` | `EmitSetupCodeNormalized()` ~line 708 |
| Statement starter detection | `AstFormatter.cs` | `IsStatementStarter()` ~line 871 |
| Keyword list | `AstFormatter.cs` | `s_statementKeywords` ~line 906 |
| Whitespace normalisation | `AstFormatter.cs` | `CollapseIntraLineSpaces()` ~line 938 |
| JSX block scanning | `AstFormatter.cs` | `EmitSetupCodeWithJsx()` ~line 1386 |
| Formatter options | `FormatterOptions.cs` | `IndentSize`, `InsertSpaceBeforeSelfClose`, etc. |
| Config loading | `ConfigLoader.cs` | `LoadFormatterOptions()` |
| Tests | `FormatterSnapshotTests.cs` | Sections A–W, ~693 tests |
| Sample files (idempotency) | `Samples/UITKX/` | 54 `.uitkx` files |
| Publish script | `scripts/publish-extension.ps1` | Builds, packages, publishes |

---

## Appendix: Common Patterns for New AI Agents

If you are an AI agent sitting down to fix a formatter bug for the first time, here is the exact workflow:

1. **Read this document entirely.**
2. **Read `AstFormatter.cs`** — focus on the methods listed in Section 3.
3. **Reproduce the bug** — write a failing test in `FormatterSnapshotTests.cs`.
4. **Trace the logic** — use the decision tree in Section 4 to understand why the current output is wrong.
5. **Make the minimal fix** — change as little code as possible.
6. **Test both paths** — if the bug is in `EmitCSharpLines`, also check `EmitSetupCodeNormalized`.
7. **Run `dotnet test`** — fix any broken assertions by updating them to the new correct output.
8. **Check sample files** — if any idempotency tests fail, re-baseline the sample file if the new output is correct.
9. **Add regression tests** — at least 5–10 tests covering the fix, edge cases, and negative cases.
10. **Publish** — use the publish script with `-ChangelogEntry` and `-BumpVersion`.

**The most important rule:** Never add a broad heuristic without testing against ALL 54 sample files. The formatter is used on every save — a false positive causes infinite format loops.
