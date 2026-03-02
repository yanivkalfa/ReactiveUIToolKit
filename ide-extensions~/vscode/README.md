# UITKX — VS Code Extension

Adds language support for `.uitkx` ReactiveUIToolKit component templates.

## Features

- **Syntax highlighting** — directives, control flow, element tags, attributes, embedded C# expressions
- **Completions** — element tags, attribute names, `@` directives
- **Hover documentation** — element descriptions, attribute types and descriptions
- **Bracket matching and auto-close** for `{`, `(`, `"`, `<!--`
- **Folding** for element hierarchies

## Requirements

- **.NET 8+** runtime on PATH (`dotnet --version` must succeed)
- The bundled `server/UitkxLanguageServer.dll` is included in the extension

## Extension Settings

| Setting | Default | Description |
|---------|---------|-------------|
| `uitkx.server.path` | `""` | Override the path to `UitkxLanguageServer.dll` |
| `uitkx.server.dotnetPath` | `"dotnet"` | Path to the `dotnet` executable |
| `uitkx.trace.server` | `"off"` | LSP trace level (`off` / `messages` / `verbose`) |

## Quick Start

1. Install the extension
2. Open any `.uitkx` file — syntax highlighting activates immediately
3. Type `<` to get element tag completions
4. Type `@` at the start of a line for directive completions
5. Hover any tag name for its props type and available attributes

## Known Limitations

- Diagnostics (red squiggles) are produced by the Roslyn source generator, not
  the LSP server. Open the Unity Editor to see generator diagnostics in the
  Unity console.
- The LSP server does not yet support Go-To-Definition or Find References.

## Development

```bash
npm install
npm run watch          # incremental TypeScript build
# Press F5 in VS Code to launch the Extension Development Host
```

## License

MIT
