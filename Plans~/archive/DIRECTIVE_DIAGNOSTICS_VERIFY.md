# Directive Diagnostics — Verification Checklist

All 11 diagnostics that fire for directive/component body issues.  
Verify each one fires correctly, in the right place, with the right message.

**Unity** = Source Generator DLL (fires during C# compilation)  
**Extension** = Language server (0013–0016 and 0018 also fire instantly via DiagnosticsAnalyzer before Roslyn)

| Status | Code | Name | Severity | Unity | Extension | Trigger example |
|--------|------|------|----------|-------|-----------|-----------------|
| ☐ | UITKX0009 | @foreach child missing key | Warning | ✅ | ✅ | `@foreach(var item in list) {` `<Label text={item}/>` `}` — no `key` attribute |
| ☐ | UITKX0010 | Duplicate sibling key | Warning | ✅ | ✅ | `<Label key="a"/>` `<Button key="a"/>` — same literal key on siblings |
| ☐ | UITKX0013 | Hook in conditional | Error | ✅ | ✅ (fast) | `@if(show) {` `var s = Hooks.UseState(0);` `}` |
| ☐ | UITKX0014 | Hook in loop | Error | ✅ | ✅ (fast) | `@foreach(var x in list) {` `var s = Hooks.UseState(0);` `}` |
| ☐ | UITKX0015 | Hook in switch | Error | ✅ | ✅ (fast) | `@switch(tab) {` `@case 0:` `var s = Hooks.UseState(0);` `}` |
| ☐ | UITKX0016 | Hook in attribute expression | Error | ✅ | ✅ (fast) | `<Button click={_ => { var s = Hooks.UseState(0); }}/>` |
| ☐ | UITKX0017 | Multiple root elements | Error | ✅ | ✅ | `return (<Label/>);` `return (<Button/>);` — two root nodes |
| ☐ | UITKX0018 | UseEffect missing dependency array | Warning | ✅ | ✅ (fast) | `Hooks.UseEffect(() => { DoWork(); });` — no second argument |
| ☐ | UITKX0019 | Loop variable used as key | Warning | ✅ | ✅ | `@foreach(var i in list) {` `<Label key={i}/>` `}` — loop var directly as key |
| ☐ | UITKX0020 | ref on component with no Ref\<T\> param | Error | ✅ | ✅ | `<MyComp ref={r}/>` where `MyComp` declares no `Ref<T>` parameter |
| ☐ | UITKX0021 | ref ambiguous — multiple Ref\<T\> params | Error | ✅ | ✅ | `<MyComp ref={r}/>` where `MyComp` declares two or more `Ref<T>` parameters |

---

**(fast)** = also fires immediately via DiagnosticsAnalyzer (no Roslyn compile needed)
