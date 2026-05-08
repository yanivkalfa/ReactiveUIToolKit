**ReactiveUIToolKit `0.5.2` + IDE Extensions `1.1.14`**

✨ **JSX literals work in any C# expression position now** — matching React/Babel semantics.

Patterns that previously emitted raw JSX and tripped the C# compiler now splice cleanly to `V.Tag(...)` calls:

```jsx
// ternary branches
<Box>{cond ? <A/> : <B/>}</Box>

// null-coalescing
<Box>{fallback ?? <Default/>}</Box>

// JSX in attribute expressions
<Box icon={active ? <Check/> : <X/>}/>

// JSX inside lambda bodies
attr={items.Select(x => <Item key={x.Id}/>)}
```

The scanner powering this (`FindBareJsxRanges` + `FindJsxBlockRanges`) was already proven on component preambles and directive bodies — it's now wired into the two remaining emit sites (`EmitExpressionNode` and the `CSharpExpressionValue` branch of attribute emission) plus mirrored in HMR and the IDE virtual-document generator.

**No runtime change** — same `V.Tag(...)` factory + `__Rent<TProps>()` pooled shape. The whole splice is emit-time only and short-circuits when an expression contains no JSX.

🐛 **Fix: `Texture2D ? iconName = null` no longer drops the `?` on save.** Whitespace before `?` in a nullable component param made the tokenizer leave it unconsumed; format-on-save then re-emitted the parameter as non-nullable. Same pathology as the recent `@else` blank-line bug — formatter re-emit is lossy when the parser drops tokens. Tokenizer now peeks past whitespace, canonicalises the captured type, and emits a clean `Texture2D? name` regardless of input spacing.

🔧 **HMR + IDE parity:** HMR mirrors the splice end-to-end. The virtual-document generator strips embedded JSX to typed-`(VirtualNode)null!` stubs so the IDE no longer shows phantom Roslyn errors on files that compile cleanly under SG.

🧪 **Tests:** 14 new (10 SG `JsxInExpressionTests`, 1 HMR parity tripwire, 3 VDG). 1178/1178 passing.