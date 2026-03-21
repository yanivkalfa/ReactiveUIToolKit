# UITKX

Language support for `.uitkx` ReactiveUIToolKit component templates in Visual Studio.

## Features

- Syntax highlighting for directives, control flow, tags, attributes, and embedded C# expressions
- Basic editor tooling through the bundled language server
- Formatting support via the UITKX formatter

## Repository

https://github.com/ReactiveUITK/ReactiveUIToolKit

## Changelog

### [1.0.58] - 2026-03-19
- Fix unused diagnostic dimming: Error-severity unused params/vars keep red squiggle instead of being downgraded to blue dots; add build-local.ps1 script


### [1.0.57] - 2026-03-17
- Companion file injection + VisualElementSafe schema fix (fixes CS0103 on Styles and UITKX0105 on VisualElementSafe)


### [1.0.56] - 2026-03-16
- Add visualElementSafe built-in element and inline JSX attribute values


### [1.0.55] - 2026-03-16
- Add visualElementSafe built-in element and inline JSX attribute values


### [1.0.54] - 2026-03-16
- Hover: show only own props for workspace elements; inherited props collapsed to count line


### [1.0.53] - 2026-03-16
- Ctrl+Click go-to-definition with underline; F12 support; unreachable code graying with CS0162; faster diagnostic refresh; 10s timeout for go-to-def RPC


### [1.0.52] - 2026-03-16
- Ctrl+Click go-to-definition with underline; F12 support; unreachable code graying with CS0162; faster diagnostic refresh; 10s timeout for go-to-def RPC
