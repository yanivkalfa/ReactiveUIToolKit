**ReactiveUIToolKit `0.5.3` + IDE Extensions `1.1.15`**

🧹 **Breaking: `@(expr)` markup syntax is gone.** From here on, the only embed form for arbitrary C# expressions inside markup is `{expr}` — same as JSX/Babel/React. The `@` prefix survives only as the directive marker (`@if`, `@else`, `@for`, `@foreach`, `@while`, `@switch`, `@case`, `@default`, `@using`, `@namespace`, `@component`, `@props`, `@key`, `@inject`, `@uss`).

```jsx
// before
<Box>@(items.Count)</Box>
<Tag attr=@(value) />

// now
<Box>{items.Count}</Box>
<Tag attr={value} />
```

Files containing legacy `@(expr)` raise hard parse error **UITKX0306**. Migration is mechanical: every `@(` → `{`, matching `)` → `}`. Unification touches parser, formatter, analyzer, IntelliSense, VDG, HMR, source generator, TextMate grammar, all 12 shipped samples, and the tests.

🐛 **Fix: pool-rent decls no longer hide inside line comments.** Naive backward-scan picking the splice point for `var __p_N = __Rent<TProps>()` stopped at the first `;` or `}` — including `}` inside `// see {catBadge}` comments. Compiler ate the decls as comment text → downstream CS0103. All four sites (SG ×2, HMR ×2) replaced with a shared `FindLastTopLevelStatementBoundary` lexer-aware scanner that skips comments, strings (regular/verbatim/interpolated with brace-depth in holes), and char literals. Pre-Phase 2 the comment had `@(catBadge)` so the bug was masked — unification exposed it.

🎨 **Fix: TextMate grammar was completely dead.** The JSON file picked up a UTF-8 BOM during the edit pass. `vscode-textmate` rejects BOM → grammar fails to load → every scoped token fell back to plain text (keywords, properties, operators rendering white in module bodies). Both grammar copies are now BOM-free; 37 rules load.

🧪 **1178/1178 SG tests passing.** HMR↔SG parity green.