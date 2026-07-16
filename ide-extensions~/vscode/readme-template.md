# Reactive UI - Unity - VS Code (UITKX)

Syntax highlighting + language intelligence for `.uitkx` markup (ReactiveUIToolKit for Unity). Completions, hover, diagnostics and formatting from the bundled language server — fully offline, no running Unity editor required.

💬 Join the community on Discord: https://discord.gg/Knedqu4Wyv

## Features

- **Syntax highlighting** — directives, control flow, element tags, attributes, embedded C# expressions
- **Completions** — element tags, attribute names, `@` directives
- **Hover documentation** — element descriptions, attribute types and descriptions
- **Bracket matching and auto-close** for `{`, `(`, `"`, `<!--`
- **Folding** for element hierarchies

## Requirements

- **.NET 8+** runtime on PATH (`dotnet --version` must succeed)
- The bundled `server/UitkxLanguageServer.dll` is included in the extension
