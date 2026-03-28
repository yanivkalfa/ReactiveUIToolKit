export const EXAMPLE_BASIC = `@using UnityEngine

component Avatar(string imagePath) {
  return (
    <Image texture={Asset<Texture2D>(imagePath)} />
  );
}`

export const EXAMPLE_RELATIVE = `@using UnityEngine

component Card {
  // Relative to this .uitkx file's location
  var bg = Asset<Texture2D>("./images/card-bg.png");
  var icon = Asset<Sprite>("./images/icon.png");

  return (
    <VisualElement>
      <Image texture={bg} />
      <Image sprite={icon} />
    </VisualElement>
  );
}`

export const EXAMPLE_SHORTHAND = `@using UnityEngine

component Badge {
  // Ast<T> is a shorter alias for Asset<T>
  var star = Ast<Sprite>("./star.png");

  return (
    <Image sprite={star} />
  );
}`

export const EXAMPLE_INLINE = `@using UnityEngine

component Logo {
  // Use Asset<T> directly in an attribute expression
  return (
    <Image texture={Asset<Texture2D>("./logo.png")} />
  );
}`

export const EXAMPLE_USS = `@uss "./Card.uss"

component StyledCard {
  return (
    <VisualElement>
      <Label text="Styled via USS" />
    </VisualElement>
  );
}`

export const EXAMPLE_AUTOIMPORT = `// Automatic TextureImporter configuration:
//
// Asset<Sprite>("./icon.png")
//   → Sets TextureImporter to Sprite mode automatically
//
// Asset<Texture2D>("./bg.png")
//   → Ensures Default (non-Sprite) import mode
//
// No manual import settings needed — the editor sync handles it.`

export const EXAMPLE_DIAGNOSTICS = `// UITKX0022 — File not found (Source Generator, Error)
// Fires when the referenced file does not exist on disk.
Asset<Texture2D>("./missing.png")   // ← UITKX0022

// UITKX0023 — Type mismatch (Source Generator, Error)
// Fires when the file extension is incompatible with the type.
Asset<AudioClip>("./bg.png")        // ← UITKX0023: AudioClip vs .png

// UITKX0120 — File not found (LSP, Error)
// Same check, but in the IDE as a real-time squiggle.

// UITKX0121 — Type mismatch (LSP, Error)
// Same check, but in the IDE as a real-time squiggle.`

export const EXAMPLE_SUPPORTED_TYPES = `// Extension → Valid Types
// ─────────────────────────────────────────
// .png .jpg .jpeg .bmp .tga .psd .gif
//   .tif .tiff .exr .hdr          → Sprite, Texture2D
// .wav .mp3 .ogg .aiff .flac      → AudioClip
// .ttf .otf                        → Font
// .mat                             → Material
// .prefab                          → GameObject
// .asset                           → ScriptableObject
// .uss                             → StyleSheet
// .anim                            → AnimationClip
// .controller                      → RuntimeAnimatorController
// .mesh                            → Mesh
// .physicMaterial                   → PhysicMaterial
// .shader                          → Shader
// .compute                         → ComputeShader
// .cubemap                         → Cubemap`

export const EXAMPLE_REGISTRY = `// The asset registry is a ScriptableObject that maps
// asset paths to Unity Objects at edit time.
//
// Editor sync (UitkxAssetRegistrySync) runs automatically:
//   - On .uitkx file save
//   - On domain reload
//   - On full project rescan
//
// HMR sync runs on every hot-reload compilation.
//
// At runtime, Asset<T>() reads from the static cache
// populated by the registry — no Resources or Addressables needed.`
