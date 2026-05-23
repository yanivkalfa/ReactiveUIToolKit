# Media surface — `<Video>`, `<Audio>`, `useSfx`

Status: implemented in v0.4.20-dev (additive, no breaking changes).

## Overview

Three new pieces that share a single pooled peer host (`MediaHost`):

| Surface | Kind | Purpose |
|---|---|---|
| `<Video>` | Func-Component element | Real positionable VE rendering a pooled `VideoPlayer` into a pooled `RenderTexture` set as `style.backgroundImage`. Accepts overlay children. |
| `<Audio>` | Func-Component element (no visual output) | Side-effect-only — rents an `AudioSource` from the pool, configures it from props, returns to pool on unmount. |
| `useSfx` | Hook | Returns a stable `Action<AudioClip, float>` delegate that calls `AudioSource.PlayOneShot` on the shared `MediaHost.Instance.SfxSource`. |

## Architecture

```
+---------------------------------------------------+
|  Shared/Core/Media/MediaHost.cs                   |
|                                                   |
|   ┌──────────────────────────────────────────────┐|
|   │  Hidden DontDestroyOnLoad GameObject         ││
|   │   ├─ pool: Stack<VideoPlayer>                ││
|   │   ├─ pool: Stack<AudioSource>                ││
|   │   ├─ pool: Dictionary<RTKey,Stack<RT>>       ││
|   │   ├─ singleton: SfxSource (PlayOneShot)      ││
|   │   └─ MonoBehaviour: MediaHostTicker (Update) ││
|   └──────────────────────────────────────────────┘|
|                                                   |
|   Reset on play-mode enter via                    |
|   RuntimeInitializeOnLoadMethod(Subsystem...).    |
+---------------------------------------------------+
        ▲                ▲                ▲
        │                │                │
   <Video>          <Audio>          useSfx
   VideoFunc        AudioFunc        Hooks.UseSfx
   VideoController  AudioController  (no controller)
```

## `<Video>` example

```jsx
@using UnityEngine.Video
@using ReactiveUITK.Core.Media

component VideoBackground {
  var ctrl = useRef<VideoController>(null);

  return (
    <Video
      Clip={Asset<VideoClip>("./bg.mp4")}
      Loop={true}
      Autoplay={true}
      Muted={true}
      ScaleMode="scaleAndCrop"
      Controller={ctrl}
      style={new Style { FlexGrow = 1f }}
    />
  );
}
```

Controller methods: `Play() / Pause() / Stop() / Seek(seconds)`. Properties:
`IsPlaying`, `IsPrepared`, `Duration`, `Time`, `Frame`, `Volume`, `Muted`,
`PlaybackSpeed`. Getters return defaults when the controller is detached
(`IsAttached == false`).

`ScaleMode` accepts `"scaleToFit"` (default), `"scaleAndCrop"`,
`"stretchToFill"` — mapped to `BackgroundSize` Contain / Cover / 100% × 100%.

`<Video>` accepts overlay children — they're rendered above the video
texture inside the same `VisualElement`, useful for play/pause buttons
or captions.

## `<Audio>` example

```jsx
@using UnityEngine
@using ReactiveUITK.Core.Media

component MusicTrack {
  var ctrl = useRef<AudioController>(null);

  return (
    <Audio
      Clip={Asset<AudioClip>("./theme.ogg")}
      Loop={true}
      Autoplay={true}
      Volume={0.5f}
      Controller={ctrl}
    />
  );
}
```

`<Audio>` renders a Fragment — no visual output. All work happens in the
`Hooks.UseEffect` of `AudioFunc.Render`. The component is safe to nest
anywhere in the tree; mount and unmount are tracked by the reconciler
exactly as for any other element.

`OnEnded` fires for non-looping sources via per-frame polling on
`MediaHost.MediaHostTicker` (AudioSource has no native end-of-clip event).

## `useSfx` example

```jsx
@using UnityEngine
@using UnityEngine.Audio

component SfxButton {
  var playSfx = useSfx();              // optional mixer arg available

  return (
    <Button text="Click me" onClick={_ =>
      playSfx(Asset<AudioClip>("./click.wav"), 1f)
    } />
  );
}
```

The returned delegate has stable identity across renders (cached in the
hook's slot), so it's safe to use in `useEffect` dependency arrays.
Changing the optional `AudioMixerGroup` argument between renders rebuilds
the cached delegate.

The hook is registered as `UseSfx` in the component's
`HookSignatureAttribute` for HMR rude-edit detection — the regex
whitelists at `CSharpEmitter.cs` and `HmrCSharpEmitter.cs` are kept in
lock-step.

## Asset loading

`Asset<VideoClip>` and `Asset<AudioClip>` work zero-config — the
runtime `UitkxAssetRegistry` indexes any `UnityEngine.Object` subtype.
Unity's native importers turn `.mp4` / `.webm` into `VideoClip` and
`.mp3` / `.wav` / `.ogg` into `AudioClip`.

If the asset isn't present, `Asset<T>` returns `null` and the components
guard against null clips at mount — no exceptions are thrown; the
component just stays silent / produces no picture.

## HMR behaviour

All three surfaces are HMR-reloadable thanks to the v0.4.20 structural
fix in `Editor/HMR/HmrCSharpEmitter.cs`. The reflection-based built-in
tag discovery scans `typeof(V).GetMethods()` at HMR controller startup
and picks up `V.Audio` / `V.Video` automatically — no `s_tagMap` edit
was needed for either tag to become hot-reloadable.

A `.uitkx` save while a `<Video>` is playing rents a fresh
`VideoPlayer` (the effect re-runs because `Clip`/`Url`/`Loop`/...  are
in its dependency list). This means the video restarts at t=0 after a
hot reload — matches `<Animate>` behaviour. To preserve playback
position across reloads, pull the time off `Controller.Time` before the
edit and seek after.

## Pool lifetime

`MediaHost.Instance` is lazily created on first access. Pools survive
scene changes via `DontDestroyOnLoad`. On play-mode enter (and on
`MediaHost.ResetForTests()`) all peers are destroyed and the singleton
is reset.

`RentVideoPlayer` / `RentAudioSource` apply neutral defaults each rent
so that left-over state from the previous user can never leak.
`RentRenderTexture` rounds dimensions up to the nearest 16 px to limit
bucket explosion under live drag-resize.

## Caveats

- **Browser autoplay restrictions (WebGL):** browsers block autoplay
  until a user gesture. `<Audio Autoplay>` will fail silently on
  WebGL pre-gesture; `useSfx` (triggered by click) always works.
- **AudioListener required:** any scene playing `<Audio>` needs an
  `AudioListener` (typically on Main Camera). Editor demo windows
  may not have one — add a temporary GO with `AudioListener` if
  needed.
- **Video resize cost:** resizing the host VE rents a new RT each
  time the rounded dimensions change. Returned RTs are pooled by
  `(w, h, format, depth)` but the GPU still creates them on first use.
