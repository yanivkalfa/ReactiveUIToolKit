# PrettyUi sample

In-repo mirror of the `Assets/UI` tree of one of OUR OWN consumer projects.
Exists so HMR-time bugs that only surface against this exact shape can be
reproduced and iterated on inside this repo, without round-tripping through
publish → install → Unity launch.

Everything here is original work by this project's authors - the markup, the
styles and the four background images, which were generated with OpenAI image
generation and are owned by us under their terms. The project being mirrored is
another one of ours, not a third party's; nothing in this folder came from
anyone else. Stated explicitly because the previous wording called it "an
external consumer project", which reads as though it had - and that is exactly
what an asset-store review asks about.

## Shape

- `UI/AppRoot.uitkx` + `UI/AppRoot.style.uitkx` — top-level `<Router>` shell with
  a `module` block referencing `Asset<Texture2D>("../Resources/background-01.png")`.
- `UI/Theme.uitkx`, `UI/StyleExtensions.uitkx` — shared module-level constants and
  helpers in namespace `PrettyUi.App`.
- `UI/Components/*` — `AppButton`, `ContentPanel`, `NewGameButton`, `PageShell`,
  `Sidebar`, `TopNav`. Each has a `.uitkx` + `.style.uitkx` pair.
- `UI/Pages/*` — `GamePage`, `HomePage`, `MenuPage`, `NewsPage`, `SettingsPage`.
- `Resources/background-0[1-4].png` — referenced via `../Resources/background-XX.png`
  from `module` / `style` blocks.

All `.uitkx` files use namespace `PrettyUi.App` / `PrettyUi.App.Pages` /
`PrettyUi.App.Components` exactly as in the source project. Do not rename — the
goal is bug-shape parity, not refactoring.

## Mount

1. Create an empty GameObject in a scene.
2. Add a `UIDocument` (point its `Panel Settings` at any runtime panel settings).
3. Add `PrettyUiBootstrap`. Drag the same `UIDocument` into its `Ui Document`
   field. The script auto-adds a `RootRenderer`.

The bootstrap calls `rootRenderer.Render(V.Func(PrettyUi.App.AppRoot.Render))`.

## Known issues being investigated against this sample

See [`Plans~/PRETTY_UI_HMR_BUGS.md`](../../../Plans~/PRETTY_UI_HMR_BUGS.md).
