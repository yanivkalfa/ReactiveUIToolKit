# Third-Party Licenses

This document lists all third-party dependencies used by ReactiveUIToolKit and
its associated tooling.

---

## Runtime Dependencies

### Unity Package (ReactiveUIToolKit)

No third-party runtime dependencies. The core framework relies only on Unity's
built-in UI Toolkit APIs.

### Source Generator (ReactiveUITK.SourceGenerator)

| Package | Version | License |
|---------|---------|---------|
| Microsoft.CodeAnalysis.CSharp | 4.3.1 | MIT |
| System.Collections.Immutable | 6.0.0 | MIT |

### Language Library (ReactiveUITK.Language)

| Package | Version | License |
|---------|---------|---------|
| System.Collections.Immutable | 6.0.0 | MIT |

### LSP Server (UitkxLanguageServer)

| Package | Version | License |
|---------|---------|---------|
| OmniSharp.Extensions.LanguageServer | 0.19.9 | MIT |
| Microsoft.Extensions.Logging.Console | 8.0.0 | MIT |
| Microsoft.CodeAnalysis.CSharp | 4.9.2 | MIT |
| Microsoft.CodeAnalysis.CSharp.Workspaces | 4.9.2 | MIT |
| Microsoft.CodeAnalysis.Workspaces.Common | 4.9.2 | MIT |
| Microsoft.CodeAnalysis.CSharp.Features | 4.9.2 | MIT |

### VS Code Extension

| Package | Version | License |
|---------|---------|---------|
| vscode-languageclient | 9.0.1 | MIT |

### Visual Studio 2022 Extension

| Package | Version | License |
|---------|---------|---------|
| Microsoft.VSSDK.BuildTools | 17.14.2120 | Microsoft Software License |
| Microsoft.VisualStudio.SDK | 17.14.40265 | Microsoft Software License |

> The VS SDK packages are build-time tooling for producing the VSIX. They are
> not redistributed at runtime.

### Rider Plugin

| Package | Version | License |
|---------|---------|---------|
| org.jetbrains.kotlin.jvm | 1.9.24 | Apache-2.0 |
| org.jetbrains.intellij.platform | 2.0.1 | Apache-2.0 |

---

## Documentation Site (ReactiveUIToolKitDocs)

| Package | Version | License |
|---------|---------|---------|
| @emotion/react | 11.14.0 | MIT |
| @emotion/styled | 11.14.1 | MIT |
| @mui/icons-material | 7.3.5 | MIT |
| @mui/lab | 7.0.1-beta.19 | MIT |
| @mui/material | 7.3.5 | MIT |
| prism-react-renderer | 2.4.1 | MIT |
| react | 19.2.0 | MIT |
| react-dom | 19.2.0 | MIT |
| react-router-dom | 7.9.6 | MIT |

---

## Dev / Test Only

These packages are used during development and testing. They are never shipped
to end users.

| Package | Version | License | Used By |
|---------|---------|---------|---------|
| xunit | 2.9.3 | Apache-2.0 | Tests |
| xunit.runner.visualstudio | 3.0.2 | Apache-2.0 | Tests |
| Microsoft.NET.Test.Sdk | 17.12.0 | MIT | Tests |
| OmniSharp.Extensions.LanguageProtocol.Testing | 0.19.9 | MIT | LSP Tests |
| TypeScript | 5.9.3 | Apache-2.0 | Docs, VS Code Extension |
| esbuild | 0.27.3 | MIT | VS Code Extension |
| @vscode/vsce | 2.24.0 | MIT | VS Code Extension |
| vite (rolldown-vite) | 7.2.5 | MIT | Docs |
| eslint | 9.39.1 | MIT | Docs |

---

## License Texts

All MIT-licensed packages use the standard MIT license:
https://opensource.org/licenses/MIT

All Apache-2.0-licensed packages use the standard Apache License 2.0:
https://www.apache.org/licenses/LICENSE-2.0

Microsoft Software License terms for VS SDK packages:
https://visualstudio.microsoft.com/license-terms/
