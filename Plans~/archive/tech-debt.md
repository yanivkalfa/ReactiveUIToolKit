# Tech Debt

## Source Generator — CRITICAL

### Early `return` in component body is ignored — always uses last top-level markup
**Priority: Critical**

The source generator always uses the **last** top-level markup block as the Render method's return expression, ignoring any earlier `return` statements. All three cases below silently compile but don't work:

1. **`return null;`** — should render nothing, but the component renders the last markup block instead
2. **`return (<></>);`** — empty fragment, same problem
3. **`return (<Box><Label text="aasd" /></Box>);`** — early return with markup, same problem

No compilation error is produced. The bottom-most top-level markup is always what renders.

**Root cause**: The parser/emitter separates top-level markup nodes from code blocks. All `return (markup)` statements inside code blocks are treated as `ReturnMarkup` nodes, but the final top-level markup always becomes the generated `return` expression — there's no mechanism to suppress it when an earlier return exists.

**Expected behavior** (like JSX/React):
- Early `return` should exit the Render method immediately
- `return null` / `return (<></>)` should render nothing
- Only the first reached `return` should execute

**Affected files**:
- `SourceGenerator~/Emitter/CSharpEmitter.cs` — `BuildSource()` always emits last markup as return
- `ide-extensions~/language-lib/Parser/UitkxParser.cs` — how returns are parsed vs top-level markup

---

## IDE Extensions — CRITICAL

### Ctrl+Click Go-To-Definition navigates to `dist~/` instead of source files
**Priority: Critical**

When Ctrl+clicking a component reference (e.g., `<ShowcaseTopBar />`) in a `.uitkx` file, the LSP/extension navigates to the copy inside `dist~/Samples~/` instead of the actual source file under `Samples/UITKX/Components/`.

The `dist~` folder is a build output ignored by Unity (tilde suffix). Editing files there has no effect — Unity never imports them. This sends users to a dead-end file.

**Expected behavior**: Go-To-Definition should resolve to the source `.uitkx` file, never to files inside `dist~/` or other tilde-suffixed folders.

**Affected files**:
- `ide-extensions~/lsp-server/WorkspaceIndex.cs` — likely indexes `dist~/` files alongside source files
- `ide-extensions~/vscode/` and `ide-extensions~/visual-studio/` — definition providers
