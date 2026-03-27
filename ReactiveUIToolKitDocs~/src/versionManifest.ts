/**
 * Version manifest — single source of truth for Unity version awareness
 * in the documentation website.
 *
 * When adding a new Unity version, add an entry to SUPPORTED_VERSIONS and
 * populate the feature maps below for any non-floor additions.
 *
 * Floor version = the minimum Unity version the library supports. Features
 * at or below the floor have no entry here (they are always available).
 */

// ---------------------------------------------------------------------------
// Version registry
// ---------------------------------------------------------------------------

export interface VersionInfo {
  /** Internal version string matching package.json "unity" format, e.g. "6000.3" */
  version: string
  /** Human-readable label shown in the UI, e.g. "6.3" */
  label: string
}

/**
 * Ordered list of Unity versions the library has explicit support for.
 * The first entry is the floor version (minimum supported).
 * Automation appends new entries here when a version is added.
 */
export const SUPPORTED_VERSIONS: VersionInfo[] = [
  { version: '6000.2', label: '6.2' },
  { version: '6000.3', label: '6.3' },
]

export const FLOOR_VERSION = SUPPORTED_VERSIONS[0]

/** Latest version in the list — used as default when "All versions" is selected. */
export const LATEST_VERSION = SUPPORTED_VERSIONS[SUPPORTED_VERSIONS.length - 1]

// ---------------------------------------------------------------------------
// Feature version tags
// ---------------------------------------------------------------------------

export interface FeatureVersion {
  /** The version where this feature was introduced (e.g. "6000.3"). */
  sinceUnity: string
  /** The version where this feature was deprecated (optional). */
  deprecatedIn?: string
  /** The version where this feature was removed (optional). */
  removedIn?: string
}

/**
 * Elements (VisualElement subclasses) introduced after the floor version.
 * If an element is NOT listed here, it is assumed to be available since floor.
 */
export const ELEMENT_VERSIONS: Record<string, FeatureVersion> = {
  // Currently all 61 elements exist since floor (6000.2).
  // When a new element is added in a future Unity version:
  // CalendarPicker: { sinceUnity: '6000.5' },
}

/**
 * IStyle properties introduced after the floor version.
 * Keys are camelCase IStyle property names matching StyleKeys.
 */
export const STYLE_PROPERTY_VERSIONS: Record<string, FeatureVersion> = {
  // These 3 IStyle properties are confirmed by assembly diff (6000.2 → 6000.3):
  aspectRatio: { sinceUnity: '6000.3' },
  filter: { sinceUnity: '6000.3' },
  unityMaterial: { sinceUnity: '6000.3' },
}

/**
 * CssHelpers shortcuts introduced after the floor version.
 * Keys are the helper function/property names.
 */
export const CSS_HELPER_VERSIONS: Record<string, FeatureVersion> = {
  // Filter function helpers (Unity 6.3+):
  Blur: { sinceUnity: '6000.3' },
  Grayscale: { sinceUnity: '6000.3' },
  Contrast: { sinceUnity: '6000.3' },
  HueRotate: { sinceUnity: '6000.3' },
  Invert: { sinceUnity: '6000.3' },
  Opacity: { sinceUnity: '6000.3' },
  Sepia: { sinceUnity: '6000.3' },
  Tint: { sinceUnity: '6000.3' },
}

/**
 * Doc pages introduced after the floor version.
 * Keys are page canonicalId values from docs.tsx / pages.tsx.
 */
export const PAGE_VERSIONS: Record<string, FeatureVersion> = {
  // When a new component page is added for a 6.3+ element:
  // 'calendar-picker': { sinceUnity: '6000.5' },
}

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

/** Compare two version strings numerically (e.g. "6000.3" > "6000.2"). */
export function compareVersions(a: string, b: string): number {
  const pa = a.split('.').map(Number)
  const pb = b.split('.').map(Number)
  for (let i = 0; i < Math.max(pa.length, pb.length); i++) {
    const diff = (pa[i] ?? 0) - (pb[i] ?? 0)
    if (diff !== 0) return diff
  }
  return 0
}

/** Check if a feature is available for the given selected version. */
export function isAvailableIn(feature: FeatureVersion | undefined, selectedVersion: string): boolean {
  // No version info → floor feature → always available
  if (!feature) return true
  if (compareVersions(feature.sinceUnity, selectedVersion) > 0) return false
  if (feature.removedIn && compareVersions(feature.removedIn, selectedVersion) <= 0) return false
  return true
}

/** Get a display label like "6.3+" for a feature version. Returns undefined for floor features. */
export function getVersionBadge(feature: FeatureVersion | undefined): string | undefined {
  if (!feature) return undefined
  const info = SUPPORTED_VERSIONS.find((v) => v.version === feature.sinceUnity)
  return info ? `${info.label}+` : `${feature.sinceUnity}+`
}

// ---------------------------------------------------------------------------
// Style property details (rich metadata for docs)
// ---------------------------------------------------------------------------

export interface StylePropertyDetail extends FeatureVersion {
  /** The C# type shown to the user, e.g. "StyleRatio" */
  type: string
  /** One-line description of what the property does. */
  description: string
  /** C# code example using the typed Style API. */
  example: string
  /** Related CssHelper function names (for filter functions etc.) */
  relatedHelpers?: string[]
}

export const STYLE_PROPERTY_DETAILS: Record<string, StylePropertyDetail> = {
  aspectRatio: {
    sinceUnity: '6000.3',
    type: 'StyleRatio',
    description:
      'Sets the preferred aspect ratio (width\u00A0/\u00A0height) for the element. ' +
      'The layout engine uses this when one dimension is auto.',
    example: [
      'new Style {',
      '    AspectRatio = new StyleRatio(new Ratio(16, 9)),',
      '}',
    ].join('\n'),
  },
  filter: {
    sinceUnity: '6000.3',
    type: 'StyleList<FilterFunction>',
    description:
      'Applies one or more graphical filter effects to the element\u2019s rendering. ' +
      'Multiple filters can be chained in a single list.',
    example: [
      'using static ReactiveUITK.Props.Typed.CssHelpers;',
      '',
      'new Style {',
      '    Filter = new StyleList<FilterFunction>(',
      '        new List<FilterFunction> { Blur(4), Grayscale(0.5f) }',
      '    ),',
      '}',
    ].join('\n'),
    relatedHelpers: [
      'Blur', 'Grayscale', 'Contrast', 'HueRotate',
      'Invert', 'Opacity', 'Sepia', 'Tint',
    ],
  },
  unityMaterial: {
    sinceUnity: '6000.3',
    type: 'StyleMaterialDefinition',
    description:
      'Assigns a Unity Material (Shader Graph or built-in) for custom rendering of the element. ' +
      'Useful for shader-driven UI effects.',
    example: [
      'new Style {',
      '    UnityMaterial = new StyleMaterialDefinition(',
      '        new MaterialDefinition(myMaterial)',
      '    ),',
      '}',
    ].join('\n'),
  },
}

/**
 * Build version-aware search keywords for the Styling page.
 * Returns style property names and CssHelper names that are available
 * for the given version — so search results are version-accurate.
 */
export function getStyleSearchTerms(selectedVersion: string): string {
  const terms: string[] = []
  for (const [name, fv] of Object.entries(STYLE_PROPERTY_VERSIONS)) {
    if (isAvailableIn(fv, selectedVersion)) {
      terms.push(name)
      const detail = STYLE_PROPERTY_DETAILS[name]
      if (detail) {
        terms.push(detail.type, detail.description)
        if (detail.relatedHelpers) terms.push(...detail.relatedHelpers)
      }
    }
  }
  for (const [name, fv] of Object.entries(CSS_HELPER_VERSIONS)) {
    if (isAvailableIn(fv, selectedVersion)) terms.push(name)
  }
  return terms.join(' ')
}
