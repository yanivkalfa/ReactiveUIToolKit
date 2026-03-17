# UITKX — Rider Plugin

Adds language support for `.uitkx` ReactiveUIToolKit component templates in **Rider 2024.1+**.

## Features

- `.uitkx` file type registration
- Completions and hover documentation via the shared UITKX LSP server
- Syntax highlighting (via IntelliJ TextMate grammar support)

## Requirements

- **JetBrains Rider** 2024.1 or later
- **.NET 8+** runtime on PATH

## Installation

### From JetBrains Marketplace (recommended)
1. Open **Rider → Settings → Plugins → Marketplace**
2. Search for **UITKX**
3. Click **Install** and restart Rider

### From disk
1. Download `uitkx-rider-x.x.x.zip` from the [Releases page](https://github.com/your-org/ReactiveUIToolKit/releases)
2. Open **Rider → Settings → Plugins → ⚙ → Install Plugin from Disk…**
3. Select the zip file and restart Rider

## Development

```bash
cd rider
./gradlew runIde     # launches a sandboxed Rider instance with the plugin loaded
./gradlew buildPlugin  # builds the distributable zip
```

Requires JDK 17 and a Rider 2024.1 download (handled automatically by
`intellijPlatform` Gradle plugin).

## License

MIT
