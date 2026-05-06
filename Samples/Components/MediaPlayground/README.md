# MediaPlayground sample (scaffold)

Demonstrates the v0.4.20 media surface:

- `<Video>` element — pooled `VideoPlayer` + `RenderTexture`, autoplay, loop, imperative controller.
- `<Audio>` element — pooled `AudioSource` for background music.
- `useSfx()` hook — fire-and-forget one-shot for UI button clicks.

## Status

This folder ships **as a scaffold only**. Two pieces are intentionally missing:

1. **`MediaPlayground.uitkx`** — the actual sample component.
2. **`Samples/Showcase/Editor/EditorUitkxMediaPlaygroundDemoWindow.cs`** — the demo window bootstrap.

Neither was committed because the sample needs binary media files (mp4/wav)
to be useful, and the .uitkx-formatter snapshot tests require any committed
`.uitkx` to be exactly idempotent through `AstFormatter.Format` — best
written by the contributor who's also dropping in the assets.

## To complete the sample

### 1. Drop royalty-free media files into this folder

| File | Type | Notes |
|---|---|---|
| `demo.mp4` | Video | Any short clip (≤ 1 MB). Unity imports `.mp4` / `.webm` as `VideoClip`. |
| `music.ogg` | Audio | Looping ambient track. `.ogg` / `.mp3` / `.wav` all import as `AudioClip`. |
| `click.wav` | Audio | Short SFX, ≤ 200 ms. |
| `pickup.wav` | Audio | Short SFX, ≤ 300 ms. |
| `error.wav` | Audio | Short SFX, ≤ 300 ms. |

Suggested sources (royalty-free / CC0):
- [freesound.org](https://freesound.org)
- [Kenney audio packs](https://kenney.nl/assets/category:Audio)
- [Pixabay video](https://pixabay.com/videos/)

### 2. Author `MediaPlayground.uitkx`

See [Plans~/MEDIA_USAGE.md](../../../Plans~/MEDIA_USAGE.md) for ready-to-paste
fragments. Reference `Samples/Components/UitkxCounterFunc/UitkxCounterFunc.uitkx`
for the simplest working component shape.

After writing, run `dotnet test SourceGenerator~/Tests/...csproj
--filter FormatterSnapshot` to confirm the file is formatter-idempotent
(both the legacy and the Roslyn-delegate paths). Adjust whitespace / line
breaks as needed.

### 3. Add the bootstrap

Mirror `Samples/Showcase/Editor/EditorUitkxAnimationsDemoWindow.cs`:
register a `[MenuItem("ReactiveUITK/Demos/Media Playground")]`, call
`EditorRootRendererUtility.Render(rootVisualElement, V.Func(MediaPlayground.Render, key: "media-playground"))`,
unmount in `OnDisable`.

## Asset registry behaviour

After copying media files into this folder, Unity will auto-import them
into `UitkxAssetRegistry` (the `Editor/UitkxAssetRegistrySync.cs` watcher
picks up the new entries on the next domain reload).
`Asset<VideoClip>("./demo.mp4")` / `Asset<AudioClip>("./click.wav")` then
resolve at both compile-time (SG) and HMR (live edit).

If asset files are missing, the components mount but render nothing /
play silence — no exceptions; `Asset<T>(...)` returns `null` and the
runtime guards against null clips.
