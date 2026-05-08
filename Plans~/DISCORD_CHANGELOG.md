**ReactiveUIToolKit `0.5.4` + IDE Extensions `1.2.0`**

🛡️ **Breaking: User components are now strict about attributes.** Anything that isn't a declared parameter (or the universal `key` / `ref`) is rejected. Pass-through of `style`, `name`, `className`, `onClick`, `extraProps`, etc. through a user component used to silently compile to `Style = x` against the generated `AppButtonProps` and explode at C# build time as **CS0117** with no pointer back to the `.uitkx` file. Now you get a proper **UITKX0109** error in the IDE *and* at build time, with an actionable hint.

```jsx
// Before — silent slip-through, then CS0117
<AppButton text="Save" style={btnStyle}/>

// After — UITKX0109: Unknown attribute 'style' on 'AppButton'.
//         Available on 'AppButton': text, onClick.
```

🧠 **Why.** The schema lumped `key`/`ref` together with the 58 `BaseProps` members under one `universalAttributes` list, so every tag appeared to accept the full intrinsic surface. The list is now split:

• **`structuralAttributes`** = `key`, `ref` — apply everywhere (`key` lives on `VirtualNode`; `ref` is routed `forwardRef`-style to the unique `Hooks.MutableRef<T>` parameter).
• **`intrinsicElementAttributes`** = the 58 `BaseProps` members — only valid on built-in `V.*` tags backing a `VisualElement`.

Built-ins are unchanged: `<Button style={...} extraProps={...}/>` still works.

🩹 **Migration.** Forwarding `style` through a user component? Declare it explicitly:

```jsx
component AppButton(string text = "", IStyle? style = null) {
    return (<Button text={text} style={style}/>);
}
```

🧰 Editor (LSP) and build (source generator) now share the same attribute map — no more red-in-editor / yellow-at-build asymmetry. The bad attribute is skipped in the emitted C# so UITKX0109 doesn't cascade into CS0117/CS0246.

✅ **9 new regression tests.** 1187 / 1187 SG tests passing.
