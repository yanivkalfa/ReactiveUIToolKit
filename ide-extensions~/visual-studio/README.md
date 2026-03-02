# UITKX — Visual Studio Extension (VSIX)

Adds language support for `.uitkx` ReactiveUIToolKit component templates in **Visual Studio 2022**.

## Features

- `.uitkx` file association (content type `uitkx`)
- Completions and hover documentation via the shared UITKX LSP server
- TextMate syntax highlighting (`uitkx.tmLanguage.json` bundled)

## Requirements

- **Visual Studio 2022** (17.0+)
- **.NET 8+** runtime on PATH

## Installation

1. Download `UitkxVsix.vsix` from the [Releases page](https://github.com/your-org/ReactiveUIToolKit/releases)
2. Double-click the `.vsix` file or run:
   ```powershell
   vsixinstaller.exe UitkxVsix.vsix
   ```
3. Restart Visual Studio

## Server Location

After installation the VSIX bundle contains `server/UitkxLanguageServer.dll`.  
Visual Studio automatically launches it when you open a `.uitkx` file.

## Development

Open `UitkxVsix.csproj` in Visual Studio 2022 with the **Visual Studio extension
development** workload installed, then press **F5** to launch an Experimental
instance with the extension loaded.

## License

MIT
