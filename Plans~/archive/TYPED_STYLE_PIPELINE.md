# Typed Style Pipeline — Implementation Plan

> **Date:** April 24, 2026
> **Status:** ✅ IMPLEMENTED — April 25, 2026
> **Objective:** Eliminate all boxing allocations and dictionary overhead from `Style`.
> Replace `Dictionary<string, object>` backing with typed backing fields + bitmask tracking.
> **Estimated gain:** +3-5 FPS in stress test (3000 boxes), eliminates ~18,000 boxing allocs + ~3,000 dict allocs per frame.
> **Prerequisite:** Typed Props Pipeline (OPT-1 Layer 2) — completed.
>
> **Key decisions (confirmed):**
> - `Style` stays a **class** (not struct). Nullable, stored on `BaseProps.Style` (reference field). Reference-equality fast-path in diff.
> - Typed **backing fields** for all ~80 properties. No `Dictionary<string, object>` base class.
> - Two `ulong` **bitmasks** track which properties are set (≥78 properties → 2 × 64 bits).
> - **Backward-compatible** tuple collection initializer syntax: `new Style { (StyleKeys.Left, 5f) }` continues to work via `Add((string, object))`.
> - **Shorthand properties** (margin, padding, borderWidth, borderColor, borderRadius) expand to their individual sides at set time.
> - `TypedPropsApplier.DiffStyle` gains a **typed fast-path** comparing only set bits.
> - `BaseProps.StyleEquals` gains a **typed fast-path** — no dict iteration.
> - `PropsApplier` styleSetters/styleResetters remain for the **dict fallback path** (V.Host, VisualElementSafe, slot sub-elements).
> - Source generator, HMR, IDE/LSP: **no changes required** (they only emit `using static StyleKeys`).

---

## Table of Contents

1. [Current Flow (dict-based Style)](#current-flow)
2. [Proposed Flow (typed Style)](#proposed-flow)
3. [Style Class Redesign](#style-redesign)
4. [Implementation Steps](#implementation-steps)
5. [System-by-System Impact](#system-impact)
6. [What Breaks](#what-breaks)
7. [What Does NOT Break](#what-does-not-break)
8. [Risk Mitigations](#risk-mitigations)
9. [Testing Strategy](#testing-strategy)
10. [File-by-File Change List](#file-changes)
11. [Allocation Budget (Before vs After)](#allocation-budget)

---

## <a name="current-flow"></a>1. Current Flow (dict-based Style)

### Per-render allocation path for a single styled element:

```
User code:           new Style { (StyleKeys.Left, box.x), (StyleKeys.Top, box.y), ... }
                                     ↓
Style constructor:   Dictionary<string, object> base ctor  → 1 dict alloc (72+ bytes)
                                     ↓
Tuple Add:           this["left"] = value;                  → box.x (float) boxed to object → 1 box alloc (20 bytes)
                     this["top"] = value;                   → box.y (float) boxed → 1 box alloc
                     ... × N properties                     → N box allocs
                                     ↓
Stored on:           BaseProps.Style = style;               → style is reference in typed prop
                                     ↓
ShallowEquals:       BaseProps.StyleEquals(prev, next)
                     → a.Count != b.Count check
                     → foreach (kv in a) → b.TryGetValue → object.Equals(kv.Value, bv)
                     → dict enumeration + boxed value comparison
                                     ↓ if not equal
DiffStyle:           TypedPropsApplier.DiffStyle(element, prev, next)
                     → cast to IDictionary<string, object>
                     → foreach prevMap: nextMap.ContainsKey → PropsApplier.ResetStyle
                     → foreach nextMap: prevMap.TryGetValue → PropsApplier.ApplyStyle
                                     ↓
ApplyStyle:          PropsApplier.ApplyStyle(element, key, value)
                     → Canonicalize(key)
                     → previousStyles[element][key] = value
                     → styleSetters[key](element, value)
                     → inside setter: unbox value → call element.style.X = ...
```

### Stress test numbers (3000 boxes × 7 style props):

| Source | Count/frame | Bytes/frame (est.) |
|--------|------------|-------------------|
| `new Style` dict allocs | 3,000 | ~216,000 (72 bytes each) |
| Boxing (float, Color, enum) | 18,000 (6 per box × 3000) | ~360,000 (20 bytes each) |
| Dict entry overhead | 21,000 (7 per box) | ~672,000 (32 bytes each) |
| **Total style-only** | **~42,000 allocs** | **~1.25 MB** |

Note: `Position = "absolute"` uses a string literal (interned), not boxed. The other 6 properties
(Left, Top, Width, Height, BackgroundColor, BorderRadius) are all value types → boxed.

### Where Style is consumed (exhaustive):

| Location | File | Pattern |
|----------|------|---------|
| `BaseProps.StyleEquals` | BaseProps.cs:201-211 | `foreach (kv in a) → b.TryGetValue → object.Equals` |
| `TypedPropsApplier.DiffStyle` | TypedPropsApplier.cs:244-268 | `cast to IDictionary → foreach → ContainsKey/TryGetValue` |
| `PropsApplier.ApplyDiff` | PropsApplier.cs:1090-1121 | Skips "style" in main loop, extracts prev/next style dicts, iterates both |
| `PropsApplier.Apply` (full) | PropsApplier.cs:1326-1335 | `propertyValue is IDictionary<string,object> styleMap → foreach` |
| `V.BuildSafeAreaStyle` | V.cs:170-230 | `TryGetValue` on original, `foreach (kv in originalStyle)`, indexer write |
| `V.MergeProperties` | V.cs:1255-1265 | `source["style"] is IDictionary → foreach → clone` |
| `VNodeSnapshot` | VNodeSnapshot.cs:108-114 | `styleObj is IDictionary → foreach` for debug snapshot |
| Adapter slot forwarding | 7 adapters | `slotMap.TryGetValue("style") → IDictionary → PropsApplier` |
| `PropsApplier.previousStyles` | PropsApplier.cs:14-16 | `Dictionary<VisualElement, Dictionary<string, object>>` — per-element last-applied cache |

---

## <a name="proposed-flow"></a>2. Proposed Flow (typed Style)

```
User code:           new Style { (StyleKeys.Left, box.x), (StyleKeys.Top, box.y), ... }
                                     ↓
Style Add:           _left = (StyleLength)value;            → NO boxing — direct typed field write
                     _setBits0 |= BIT_LEFT;                 → bitmask flag
                     ... × N properties
                     (fallback: string key → switch → typed field write)
                                     ↓
Stored on:           BaseProps.Style = style;               → same reference as before
                                     ↓
ShallowEquals:       Style.TypedEquals(prev, next)
                     → if (_setBits0 != other._setBits0 || _setBits1 != other._setBits1) return false
                     → compare only fields where bits are set
                     → NO dict enumeration, NO boxing, NO object.Equals
                                     ↓ if not equal
DiffStyle:           TypedPropsApplier.DiffStyle(element, prev, next)
                     → ulong removedBits = prev._setBits0 & ~next._setBits0
                     → ulong addedOrChanged = next._setBits0 & (changed mask)
                     → for each set bit: compare typed field directly, call typed setter
                     → NO Canonicalize, NO previousStyles lookup, NO dict iteration
                                     ↓
Typed setter:        element.style.left = next._left;       → direct struct assignment, NO unboxing
```

### Stress test numbers (3000 boxes × 7 style props) — AFTER:

| Source | Count/frame | Savings |
|--------|------------|---------|
| `new Style` | 3,000 (class alloc, no dict) | ~144,000 bytes saved (no dict bucket array) |
| Boxing | **0** | **18,000 allocs eliminated** |
| Dict entry overhead | **0** | **21,000 entries eliminated** |
| **Total style-only** | **3,000 allocs** (just the Style object) | **~39,000 allocs eliminated** |

The remaining 3,000 Style class allocs could be further reduced by object pooling or
struct-ifying Style in a future step. But eliminating 39,000 allocs (boxing + dict entries)
is the dominant win.

---

## <a name="style-redesign"></a>3. Style Class Redesign

### 3.1 Remove Dictionary inheritance

```csharp
// BEFORE:
public class Style : Dictionary<string, object>

// AFTER:
public class Style : IEnumerable<KeyValuePair<string, object>>
```

Style **no longer IS a dictionary**. It implements `IEnumerable<KVP>` for backward compat with
existing `foreach` consumers (VNodeSnapshot, PropsApplier dict fallback path, V.MergeProperties).

### 3.2 Bitmask tracking

With ~78 style properties (pre-Unity 6.3) and ~81 (post-Unity 6.3), we need two `ulong` fields:

```csharp
internal ulong _setBits0;    // bits 0-63:  properties 0-63
internal ulong _setBits1;    // bits 0-17+: properties 64-81
```

Each property gets a compile-time constant bit index. Property setter sets the corresponding bit.

### 3.3 Typed backing fields

Group by type to minimize struct padding:

```csharp
// ── StyleLength (27 fields × 8 bytes = 216 bytes) ──────────────
internal StyleLength _width, _height, _minWidth, _minHeight, _maxWidth, _maxHeight;
internal StyleLength _flexBasis;
internal StyleLength _left, _top, _right, _bottom;
internal StyleLength _margin, _marginLeft, _marginRight, _marginTop, _marginBottom;
internal StyleLength _padding, _paddingLeft, _paddingRight, _paddingTop, _paddingBottom;
internal StyleLength _borderRadius, _borderTopLeftRadius, _borderTopRightRadius;
internal StyleLength _borderBottomLeftRadius, _borderBottomRightRadius;
internal StyleLength _fontSize, _letterSpacing;

// ── StyleFloat (9 fields × 8 bytes = 72 bytes) ─────────────────
internal StyleFloat _flexGrow, _flexShrink, _opacity;
internal StyleFloat _borderWidth, _borderLeftWidth, _borderRightWidth;
internal StyleFloat _borderTopWidth, _borderBottomWidth;
internal StyleFloat _unityTextOutlineWidth;

// ── Color (9 fields × 16 bytes = 144 bytes) ────────────────────
internal Color _color, _backgroundColor, _backgroundImageTint;
internal Color _borderColor, _borderLeftColor, _borderRightColor;
internal Color _borderTopColor, _borderBottomColor;
internal Color _unityTextOutlineColor;

// ── Enums (16 fields × 4 bytes = 64 bytes) ─────────────────────
internal FlexDirection _flexDirection;
internal Wrap _flexWrap;
internal Justify _justifyContent;
internal Align _alignItems, _alignSelf, _alignContent;
internal Position _position;
internal DisplayStyle _display;
internal Visibility _visibility;
internal Overflow _overflow;
internal WhiteSpace _whiteSpace;
internal TextAnchor _textAlign;
internal TextOverflow _textOverflow;
internal FontStyle _unityFontStyle;
internal TextOverflowPosition _unityTextOverflowPosition;
internal TextAutoSizeMode _unityTextAutoSize;

// ── float (2 fields × 4 bytes = 8 bytes) ───────────────────────
internal float _rotate, _scale;

// ── Compound structs ────────────────────────────────────────────
internal BackgroundRepeat _backgroundRepeat;
internal BackgroundPosition _backgroundPositionX, _backgroundPositionY;
internal BackgroundSize _backgroundSize;
internal TransformOrigin _transformOrigin;
internal Translate _translate;

// ── Reference types (no boxing concern) ─────────────────────────
internal Texture2D _backgroundImage;
internal Font _fontFamily;

// ── Transitions (StyleList<T> — already reference-typed wrappers) ──
internal StyleList<TimeValue> _transitionDelay;
internal StyleList<TimeValue> _transitionDuration;
internal StyleList<StylePropertyName> _transitionProperty;
internal StyleList<EasingFunction> _transitionTimingFunction;

// ── Unity 6.3+ ─────────────────────────────────────────────────
#if UNITY_6000_3_OR_NEWER
internal StyleRatio _aspectRatio;
internal StyleList<FilterFunction> _filter;
internal StyleMaterialDefinition _unityMaterial;
#endif
```

**Estimated object size:** ~550 bytes + object header (16 bytes) ≈ 566 bytes.
Compare to current: dict object (72 bytes) + bucket array (~224 bytes for 7 entries) + 7 boxed values (140 bytes) ≈ 436 bytes.

**The typed Style is ~130 bytes larger per object**, but eliminates **6 heap allocations** (the
boxed values) per Style. In GC terms, fewer objects = fewer roots to trace = dramatically less
GC pressure, even though total byte count is similar. The GC cost is dominated by **object count**,
not byte count.

### 3.4 Property setters (typed, no boxing)

```csharp
public StyleLength Left
{
    set { _left = value; _setBits0 |= (1UL << BIT_LEFT); }
}
```

No dictionary write, no boxing. Direct field assignment + bitmask update.

### 3.5 Tuple collection initializer (backward compat)

The existing syntax `new Style { (StyleKeys.Left, 5f) }` works via the `Add((string, object))` method.
This method must remain, but now it dispatches through a string→field switch:

```csharp
public void Add((string key, object value) entry)
{
    SetByKey(entry.key, entry.value);
}

internal void SetByKey(string key, object value)
{
    switch (key)
    {
        case "left":         _left = PropsApplier.ConvertToStyleLength(value); _setBits0 |= (1UL << BIT_LEFT); break;
        case "top":          _top = PropsApplier.ConvertToStyleLength(value); _setBits0 |= (1UL << BIT_TOP); break;
        case "width":        _width = PropsApplier.ConvertToStyleLength(value); _setBits0 |= (1UL << BIT_WIDTH); break;
        // ... all 78+ keys
        default:             break; // unknown key — silently ignore (matches current behavior)
    }
}
```

**Boxing still happens** in the tuple initializer path because the tuple is `(string, object)`.
But this is a **transitional** path. The recommended path is the typed property setter:

```csharp
// PREFERRED (zero boxing):
new Style { Left = 5f, Top = 10f, Width = 50f }

// STILL WORKS (boxes — backward compat):
new Style { (StyleKeys.Left, 5f), (StyleKeys.Top, 10f) }
```

The source generator can be updated in a follow-up step to emit the property setter syntax
instead of the tuple syntax for zero-boxing style construction.

### 3.6 IEnumerable implementation (dict-compat fallback)

For consumers that iterate Style as `IDictionary<string, object>` (PropsApplier dict path,
VNodeSnapshot, V.MergeProperties), Style implements `IEnumerable<KeyValuePair<string, object>>`:

```csharp
public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
{
    // Yield only set properties — boxes values on yield (fallback path only)
    if ((_setBits0 & (1UL << BIT_WIDTH)) != 0) yield return new KeyValuePair<string, object>("width", _width);
    if ((_setBits0 & (1UL << BIT_HEIGHT)) != 0) yield return new KeyValuePair<string, object>("height", _height);
    // ... all properties
}
```

This **does** box values, but it's only used by fallback paths (V.Host, VisualElementSafe, debug
snapshots). The hot path (typed adapters) never calls this.

Also implement `IDictionary<string, object>` for full backward compat with existing cast patterns:

```csharp
public class Style : IDictionary<string, object>
{
    public object this[string key]
    {
        get => GetByKey(key);
        set => SetByKey(key, value);
    }
    public bool ContainsKey(string key) => IsSet(key);
    public bool TryGetValue(string key, out object value) { ... }
    public int Count => BitCount(_setBits0) + BitCount(_setBits1);
    public ICollection<string> Keys => GetSetKeys(); // lazy, fallback only
    public ICollection<object> Values => GetSetValues(); // lazy, fallback only
    // ... etc
}
```

### 3.7 Shorthand properties

Shorthand properties (margin, padding, borderWidth, borderColor, borderRadius) expand to all
four sides. This matches CSS behavior and the current PropsApplier setter behavior:

```csharp
public StyleLength Margin
{
    set
    {
        _marginLeft = value; _setBits0 |= (1UL << BIT_MARGIN_LEFT);
        _marginRight = value; _setBits0 |= (1UL << BIT_MARGIN_RIGHT);
        _marginTop = value; _setBits0 |= (1UL << BIT_MARGIN_TOP);
        _marginBottom = value; _setBits0 |= (1UL << BIT_MARGIN_BOTTOM);
        _setBits0 |= (1UL << BIT_MARGIN); // track shorthand was used (for enumeration)
    }
}
```

For the shorthand bit (BIT_MARGIN), it's used **only** in the `GetEnumerator` fallback path
to decide whether to yield a single "margin" entry vs four individual entries. The typed diff
path always compares the four individual side fields, ignoring the shorthand bit.

### 3.8 Typed equality

```csharp
public bool TypedEquals(Style other)
{
    if (other == null) return false;
    if (_setBits0 != other._setBits0 || _setBits1 != other._setBits1) return false;

    // Only compare fields where bits are set
    ulong bits = _setBits0;
    while (bits != 0)
    {
        int bit = BitOperations.TrailingZeroCount(bits);
        if (!FieldEquals(bit, other)) return false;
        bits &= bits - 1; // clear lowest set bit
    }
    bits = _setBits1;
    while (bits != 0)
    {
        int bit = BitOperations.TrailingZeroCount(bits);
        if (!FieldEquals(64 + bit, other)) return false;
        bits &= bits - 1;
    }
    return true;
}
```

Where `FieldEquals(int bitIndex, Style other)` is a switch dispatching to typed field comparison:

```csharp
private bool FieldEquals(int bit, Style other)
{
    return bit switch
    {
        BIT_WIDTH => _width.IsEqual(other._width),   // struct Equals — no boxing
        BIT_HEIGHT => _height.IsEqual(other._height),
        BIT_COLOR => _color == other._color,
        // ... etc
        _ => true
    };
}
```

**Important:** `StyleLength.Equals` and `StyleFloat.Equals` exist on the structs already (they're
Unity types). We use direct comparison without boxing. `Color` supports `==` operator. Enums use
integer comparison.

### 3.9 Typed diff-and-apply

```csharp
public static void DiffStyle(VisualElement element, Style prev, Style next)
{
    if (ReferenceEquals(prev, next)) return;

    bool prevNull = prev == null;
    bool nextNull = next == null;

    ulong prevBits0 = prevNull ? 0UL : prev._setBits0;
    ulong prevBits1 = prevNull ? 0UL : prev._setBits1;
    ulong nextBits0 = nextNull ? 0UL : next._setBits0;
    ulong nextBits1 = nextNull ? 0UL : next._setBits1;

    // Removed: bits set in prev but not in next
    ulong removed0 = prevBits0 & ~nextBits0;
    ulong removed1 = prevBits1 & ~nextBits1;

    // Added or potentially changed: bits set in next
    ulong maybeChanged0 = nextBits0;
    ulong maybeChanged1 = nextBits1;

    // Process removed bits → reset style
    ResetBits(element, removed0, 0);
    ResetBits(element, removed1, 64);

    // Process added/changed bits → apply if value differs
    ApplyChangedBits(element, prev, next, maybeChanged0, prevBits0, 0);
    ApplyChangedBits(element, prev, next, maybeChanged1, prevBits1, 64);
}
```

Where `ApplyChangedBits` iterates set bits, compares typed fields, and calls typed setters directly:

```csharp
static void ApplyChangedBits(VisualElement el, Style prev, Style next, ulong bits, ulong prevBits, int offset)
{
    while (bits != 0)
    {
        int localBit = BitOperations.TrailingZeroCount(bits);
        int bit = offset + localBit;

        bool wasSet = (prevBits & (1UL << localBit)) != 0;
        if (!wasSet || !next.FieldEquals(bit, prev))
        {
            ApplyTypedStyleField(el, next, bit);
        }

        bits &= bits - 1;
    }
}
```

Where `ApplyTypedStyleField` is a switch calling the VisualElement style setter directly:

```csharp
static void ApplyTypedStyleField(VisualElement el, Style style, int bit)
{
    switch (bit)
    {
        case Style.BIT_WIDTH:  el.style.width = style._width; break;
        case Style.BIT_HEIGHT: el.style.height = style._height; break;
        case Style.BIT_LEFT:   el.style.left = style._left; break;
        case Style.BIT_COLOR:  el.style.color = style._color; break;
        // ... all properties
    }
}
```

**This completely bypasses:**
- `PropsApplier.Canonicalize` (no string keys)
- `PropsApplier.styleSetters` dictionary lookup
- `PropsApplier.previousStyles` dictionary tracking
- All unboxing inside setters

### 3.10 previousStyles tracking

Currently `PropsApplier.previousStyles` is `Dictionary<VisualElement, Dictionary<string, object>>`.
It tracks which styles were last applied to each element, so that on diff, removed styles can be
detected even if the previous Style object is unavailable.

In the typed path, `previousStyles` is **not needed** because:
- The fiber always stores `fiber.HostProps` (committed) and `fiber.PendingHostProps` (pending).
- `HostProps.Style` and `PendingHostProps.Style` are always available for the typed diff.
- Removed styles are detected via bitmask: `prev._setBits0 & ~next._setBits0`.

`previousStyles` remains for the **dict fallback path** only (V.Host, VisualElementSafe).

---

## <a name="implementation-steps"></a>4. Implementation Steps

### Step 1: Add bit constants and backing fields to Style

**File:** `Shared/Props/Typed/Style.cs`

- Remove `: Dictionary<string, object>` inheritance.
- Add `: IDictionary<string, object>` interface.
- Add `_setBits0`, `_setBits1` fields.
- Add all ~80 typed backing fields (grouped by type).
- Add bit index constants (`BIT_WIDTH = 0`, `BIT_HEIGHT = 1`, ...).
- Remove the old constructors that take `IDictionary<string, object>` or `int capacity`.
- Keep parameterless constructor.

**Risk:** Medium. Removing Dictionary base class is a breaking change for any code that uses
Style as a Dictionary directly. Mitigated by implementing `IDictionary<string, object>`.

### Step 2: Convert property setters to typed backing fields

**File:** `Shared/Props/Typed/Style.cs`

Change every property setter from:
```csharp
public StyleLength Left { set => this["left"] = value; }
```
To:
```csharp
public StyleLength Left { set { _left = value; _setBits0 |= (1UL << BIT_LEFT); } }
```

Also add **getters** (needed for typed diff/equality):
```csharp
public StyleLength Left
{
    get => _left;
    set { _left = value; _setBits0 |= (1UL << BIT_LEFT); }
}
```

**Risk:** Low. Setter behavior is identical (stores value + marks as set). Getter is new but
matches expected semantics.

### Step 3: Implement IDictionary<string, object> interface

**File:** `Shared/Props/Typed/Style.cs`

Implement all `IDictionary<string, object>` members:
- `this[string key]` → dispatches to `GetByKey`/`SetByKey` switch
- `ContainsKey(key)` → checks bitmask via `IsSet(key)` switch
- `TryGetValue(key, out value)` → switch + bitmask check
- `Count` → `PopCount(_setBits0) + PopCount(_setBits1)`
- `Add(string key, object value)` → `SetByKey(key, value)`
- `Add((string key, object value) entry)` → `SetByKey(entry.key, entry.value)` (existing, updated)
- `Remove(string key)` → clear bit + reset field to default
- `Keys`, `Values` → lazy enumeration of set fields
- `GetEnumerator()` → yield return for each set bit
- `Clear()` → reset both bitmasks to 0

The `SetByKey(string key, object value)` method uses a switch statement on the key string to
dispatch to the correct typed field. Inside each case, it calls the appropriate `ConvertTo*`
method from PropsApplier to handle the various input types (float → StyleLength, string → enum,
etc.).

**Risk:** Medium. The `SetByKey` switch must handle all the same input types that PropsApplier
setters handle. This is the most error-prone part — each case needs proper conversion.

### Step 4: Implement TypedEquals on Style

**File:** `Shared/Props/Typed/Style.cs`

Add `TypedEquals(Style other)` method as described in Section 3.8.
Add private `FieldEquals(int bit, Style other)` switch method.

**Risk:** Low. Pure comparison logic. Bugs would cause false dirty detection (extra re-renders),
not crashes.

### Step 5: Update BaseProps.StyleEquals to use typed path

**File:** `Shared/Props/Typed/BaseProps.cs`

```csharp
private static bool StyleEquals(Style a, Style b)
{
    if (a == null && b == null) return true;
    if (a == null || b == null) return false;
    return a.TypedEquals(b);
}
```

Removes: dict iteration, `object.Equals` boxing.

**Risk:** Low. Direct replacement.

### Step 6: Add typed DiffStyle to TypedPropsApplier

**File:** `Shared/Props/TypedPropsApplier.cs`

Replace the current `DiffStyle` method with the bitmask-based version described in Section 3.9.
Keep the old method as `DiffStyleDict` for the fallback path if needed.

Add static methods:
- `ResetBits(VisualElement, ulong bits, int offset)` — iterates set bits, calls typed resetters
- `ApplyChangedBits(VisualElement, Style prev, Style next, ulong bits, ulong prevBits, int offset)`
- `ApplyTypedStyleField(VisualElement, Style, int bit)` — switch dispatching to `el.style.X = ...`
- `ResetTypedStyleField(VisualElement, int bit)` — switch dispatching to `el.style.X = StyleKeyword.Null`

**Risk:** Medium. The `ApplyTypedStyleField` switch must match PropsApplier's setter behavior
exactly. Each setter in PropsApplier does conversion (e.g., ConvertToStyleLength, ConvertToColor)
before applying. The typed path bypasses conversion because values are already the correct type
in the backing fields.

### Step 7: Update V.BuildSafeAreaStyle for typed Style

**File:** `Shared/Core/V.cs`

BuildSafeAreaStyle currently uses:
- `originalStyle.TryGetValue(key, out var value)` — works via IDictionary interface
- `foreach (var kv in originalStyle)` — works via IEnumerable
- `merged[kv.Key] = kv.Value` — works via IDictionary indexer

These all work through the `IDictionary<string, object>` interface. **No changes needed** for
BuildSafeAreaStyle if Style implements IDictionary.

However, for optimal performance, BuildSafeAreaStyle could be updated to use typed accessors:

```csharp
private static Style BuildSafeAreaStyle(Style originalStyle)
{
    var insets = SafeAreaUtility.GetInsets();
    var merged = new Style();

    if (originalStyle != null)
    {
        originalStyle.CopyTo(merged);  // typed bitmask copy, no boxing
    }

    merged.PaddingLeft = Mathf.Max(
        originalStyle?._paddingLeft.value ?? 0f, insets.Left);
    merged.PaddingRight = Mathf.Max(
        originalStyle?._paddingRight.value ?? 0f, insets.Right);
    merged.PaddingTop = Mathf.Max(
        originalStyle?._paddingTop.value ?? 0f, insets.Top);
    merged.PaddingBottom = Mathf.Max(
        originalStyle?._paddingBottom.value ?? 0f, insets.Bottom);

    return merged;
}
```

**Risk:** Low. This is an optimization, not a requirement. The IDictionary path works as fallback.
However, since VisualElementSafe (the only caller of BuildSafeAreaStyle) uses the dict path anyway,
this optimization has minimal impact on the hot path.

### Step 8: Update PropsApplier dict path for new Style type

**File:** `Shared/Props/PropsApplier.cs`

The PropsApplier dict path (`Apply`, `ApplyDiff`) casts style values to `IDictionary<string, object>`:
```csharp
if (propertyValue is IDictionary<string, object> styleMap)
```

Since Style now implements `IDictionary<string, object>`, these casts continue to work.
**No changes needed in PropsApplier.**

The only potential issue is the `previousStyles` dictionary:
```csharp
static Dictionary<VisualElement, Dictionary<string, object>> previousStyles
```

Currently stores `Dictionary<string, object>`. The dict path stores `style` (which is now a Style
object, not a plain Dictionary) under the VisualElement key. As long as the value type is widened
to `IDictionary<string, object>`, this works:

```csharp
// CHANGE: Dictionary<string, object> → IDictionary<string, object>
static Dictionary<VisualElement, IDictionary<string, object>> previousStyles
```

**Risk:** Low. Type widening is backward compatible.

### Step 9: Add Style.CopyTo method

**File:** `Shared/Props/Typed/Style.cs`

```csharp
public void CopyTo(Style target)
{
    // Copy all set bits and their values
    ulong bits = _setBits0;
    target._setBits0 |= bits;
    while (bits != 0)
    {
        int bit = BitOperations.TrailingZeroCount(bits);
        CopyField(bit, target);
        bits &= bits - 1;
    }
    // Same for _setBits1
}
```

Used by `BuildSafeAreaStyle` and potentially by future style merging/extending features.

**Risk:** None. New additive method.

### Step 10: Verify and fix all IDictionary cast sites

**Files:** Multiple

Run a workspace-wide search for `IDictionary<string, object>` casts on Style objects.
Verify each cast still works with the new Style type. Known cast sites:

| Location | Status |
|----------|--------|
| `TypedPropsApplier.DiffStyle` | Will be replaced by typed path (Step 6) |
| `PropsApplier.Apply` (line 1326) | Works — Style implements IDictionary |
| `PropsApplier.ApplyDiff` (line 1090) | Works — Style implements IDictionary |
| `PropsApplier.previousStyles` | Needs type widening (Step 8) |
| `V.MergeProperties` (line 1255) | Works — Style implements IDictionary |
| `VNodeSnapshot` (line 108) | Works — Style implements IDictionary |
| All 7 adapter slot handlers | Works — they cast to IDictionary, Style implements it |

### Step 11: Add Style.Of factory with typed overloads (optional, follow-up)

**File:** `Shared/Props/Typed/Style.cs`

The current `Style.Of(params (string, object)[] entries)` boxes values. Could add typed overloads
or just rely on property initializer syntax. This is a low-priority follow-up.

---

## <a name="system-impact"></a>5. System-by-System Impact

### Source Generator

**Impact: NONE**

The source generator emits:
```csharp
using static ReactiveUITK.Props.Typed.StyleKeys;
```

It does **not** emit Style class references, Style constructors, or style property setters.
Style construction is entirely in user `.uitkx` code and passes through the C# compiler
with the `Style` class as-is. The source generator doesn't need to know about Style's internals.

### HMR (Hot Module Replacement)

**Impact: NONE**

HMR re-emits the same code as the source generator. It emits `using static StyleKeys;` and
doesn't reference the Style class directly. HMR recompiles user `.uitkx` code at runtime,
and the recompiled code uses whatever `Style` class is available in the assembly.

### IDE / LSP Extensions

**Impact: NONE**

The IDE extensions validate style by:
- Schema JSON: maps camelCase style key strings → valid values. No C# type references.
- Completion: suggests `{ }` for style attributes, autocompletes `StyleKeys.*` values.
- Diagnostics: version-checks style keys by string name.
- Virtual document: emits `using static StyleKeys;`, untyped `object` wrapper for style expression.

**None of these reference the `Style` class or its inheritance hierarchy.** Changing Style from
Dictionary to a typed class with IDictionary interface has zero impact.

### Fiber Reconciler

**Impact: INDIRECT — via TypedPropsApplier and BaseProps**

The reconciler calls:
1. `BaseProps.ShallowEquals` → calls `StyleEquals` → now uses `TypedEquals` (faster)
2. `adapter.ApplyTypedDiff` → calls `TypedPropsApplier.DiffStyle` → now uses bitmask diff (faster)

No direct changes needed in the reconciler itself.

### PropsApplier (dict path)

**Impact: MINIMAL — type widening of previousStyles**

The dict path (V.Host, VisualElementSafe, slot sub-elements) continues to work through
`IDictionary<string, object>`. The only change is widening `previousStyles` value type from
`Dictionary<string, object>` to `IDictionary<string, object>`.

### Tests

**Impact: NONE on syntax, potential BEHAVIORAL changes**

All test files use `new Style { (StyleKeys.X, value) }` syntax. This syntax continues to work
via the `Add((string, object))` method. The tuple still boxes the value, but the Add method
now dispatches to a typed field via `SetByKey`. The round-trip (write via tuple → read via
IDictionary getter → compare) must produce identical results.

Key test areas to verify:
- `FormatterStyleTests` — test formatting of Style objects in debug output
- Any test that compares Style objects via dict-like iteration
- Any test that creates Style via `new Style(existingDict)` constructor (this constructor is removed)

### User Code (.uitkx files)

**Impact: NONE (backward compatible)**

All user code patterns work unchanged:
- `new Style { (StyleKeys.Left, 5f) }` — works via `Add((string, object))`
- `new Style { Left = 5f }` — works via typed property setter (now the preferred zero-boxing path)
- `var s = new Style { ... }` — works identically
- `static readonly Style s = new Style { ... }` — works identically
- Conditional values inside Style — works identically

Users can **optionally** switch from tuple syntax to property setter syntax for zero-boxing:
```csharp
// Before (boxes 5f):
new Style { (StyleKeys.Left, 5f) }

// After (zero boxing):
new Style { Left = 5f }
```

### V.BuildSafeAreaStyle

**Impact: LOW — works via IDictionary, optional typed optimization**

BuildSafeAreaStyle uses `TryGetValue`, `foreach`, and indexer on Style. All work via IDictionary.
Can optionally be updated to use typed accessors for performance, but VisualElementSafe is not
on the hot path.

---

## <a name="what-breaks"></a>6. What Breaks

### 6.1 `new Style(IDictionary<string, object> dict)` constructor

**Removed.** This constructor copies from a plain dict into Style. Since Style no longer extends
Dictionary, this needs to be replaced with:

```csharp
public static Style FromDictionary(IDictionary<string, object> dict)
{
    var style = new Style();
    foreach (var kv in dict)
        style.SetByKey(kv.key, kv.value);
    return style;
}
```

**Usage:** Search found zero usages in UITKX framework code. Only appears in the Style class itself.

### 6.2 `new Style(int capacity)` constructor

**Removed.** No longer meaningful (no underlying dictionary with buckets). Replaced with
parameterless constructor.

**Usage:** Only used in `Style.Of()` factory method. Updated in Step 1.

### 6.3 Direct Dictionary method calls on Style

Any code calling `style.Add(string key, object value)` (the Dictionary's native Add, not the
tuple overload) now goes through the IDictionary interface implementation. Behavior is identical
but performance differs (switch dispatch vs dict insert).

**Usage:** No framework code calls `style.Add(key, value)` with two separate args. All use
tuple syntax.

### 6.4 `Style` cast to `Dictionary<string, object>`

Any code that casts Style to `Dictionary<string, object>` (not `IDictionary`) will fail at runtime.

**Usage:** Search found **zero** direct casts to `Dictionary<string, object>`. All casts use
`IDictionary<string, object>`.

---

## <a name="what-does-not-break"></a>7. What Does NOT Break

| Feature | Why it's safe |
|---------|--------------|
| `new Style { (StyleKeys.X, val) }` | `Add((string, object))` method preserved |
| `new Style { Left = 5f }` | Typed property setter preserved |
| `Style.Of(...)` | Updated to use `SetByKey` internally |
| `foreach (var kv in style)` | `IEnumerable<KVP>` implemented |
| `style.ContainsKey(key)` | `IDictionary` implemented |
| `style.TryGetValue(key, out val)` | `IDictionary` implemented |
| `style["key"] = value` | `IDictionary` indexer implemented |
| `style is IDictionary<string, object>` | Style implements the interface |
| `style as IDictionary<string, object>` | Same |
| Source generator output | Doesn't reference Style class |
| HMR recompilation | Doesn't reference Style class |
| IDE autocomplete | Uses string keys, not Style type |
| All existing tests | Syntax unchanged, behavior preserved |
| PropsApplier dict path | IDictionary interface works |
| VNodeSnapshot debug | IEnumerable works |
| Adapter slot handling | IDictionary interface works |

---

## <a name="risk-mitigations"></a>8. Risk Mitigations

### 8.1 Conversion correctness in SetByKey

**Risk:** The `SetByKey` switch must handle all input types that PropsApplier setters handle.
If a case is missing or wrong, styles set via tuple syntax will silently fail.

**Mitigation:** Each case in `SetByKey` calls the same `ConvertTo*` methods that PropsApplier uses.
Add unit tests for each property with each valid input type (float, int, string, enum, StyleLength, etc.).

### 8.2 Equality false negatives

**Risk:** If `FieldEquals` has a bug for a specific type, two identical styles would be considered
different, causing unnecessary re-renders (performance regression, not correctness bug).

**Mitigation:** Add unit tests for TypedEquals with each property type. Run stress test and
verify no increase in CommitUpdate calls.

### 8.3 Object size increase

**Risk:** Style objects are ~130 bytes larger than before. With 3000 styles/frame, that's
~390 KB more per frame in the nursery.

**Mitigation:** This is offset by eliminating ~39,000 allocations (boxing + dict entries).
The net effect is strongly positive for GC because object **count** (not byte count) drives
GC scan time. Verify with stress test profiler.

### 8.4 Shorthand property expansion

**Risk:** Shorthand properties (margin, padding, etc.) expand to 4 sides. If a user sets
`Margin = 10` and then `MarginLeft = 5`, the order matters. This matches CSS cascade behavior
but could surprise users.

**Mitigation:** This is **identical to current behavior** — PropsApplier's `margin` setter already
expands to all four sides. No behavioral change.

---

## <a name="testing-strategy"></a>9. Testing Strategy

### 9.1 Unit tests for Style class

- **TypedEquals:** same style → true, different values → false, different set bits → false,
  null handling, empty style equals empty style.
- **SetByKey round-trip:** set via tuple, read via getter, verify value.
- **IDictionary interface:** ContainsKey, TryGetValue, Count, foreach — all consistent with set bits.
- **Shorthand expansion:** set Margin, verify MarginLeft/Right/Top/Bottom all set.
- **Property setter types:** float → StyleLength, Color → Color, enum → enum, string → enum.

### 9.2 Integration tests

- **Stress test:** Run 3000-box stress test. Compare FPS, GC alloc count, frame time.
- **Visual regression:** Run sample gallery, verify all elements render identically.
- **HMR:** Edit a .uitkx file with styles, verify hot reload works.

### 9.3 Profiler verification

- **Deep profile:** Before and after comparison of GC.Alloc in the style path.
- **Expected reduction:** ~39,000 allocs/frame → ~3,000 allocs/frame (Style objects only).
- **Frame time:** Should improve by 2-4ms in stress test.

---

## <a name="file-changes"></a>10. File-by-File Change List

### Core Style changes

| File | Change | Risk |
|------|--------|------|
| `Shared/Props/Typed/Style.cs` | Remove Dictionary base, add typed fields, bitmasks, IDictionary impl, TypedEquals, SetByKey, GetByKey, CopyTo | **HIGH** — largest change, most error-prone |
| `Shared/Props/Typed/StyleKeys.cs` | **No change** — string constants remain identical |
| `Shared/Props/Typed/CssHelpers.cs` | **No change** — helper constants are independent of Style internals |

### Consumer updates

| File | Change | Risk |
|------|--------|------|
| `Shared/Props/Typed/BaseProps.cs` | `StyleEquals` → use `TypedEquals` | **LOW** |
| `Shared/Props/TypedPropsApplier.cs` | `DiffStyle` → bitmask-based typed diff, add `ApplyTypedStyleField`, `ResetTypedStyleField` | **MEDIUM** |
| `Shared/Props/PropsApplier.cs` | Widen `previousStyles` value type to `IDictionary<string, object>` | **LOW** |
| `Shared/Core/V.cs` | `BuildSafeAreaStyle` — optional typed optimization | **LOW** |

### No changes needed

| File | Why |
|------|-----|
| `Shared/Core/Util/VNodeSnapshot.cs` | Uses `IDictionary` cast — still works |
| `Shared/Core/V.cs` (MergeProperties) | Uses `IDictionary` cast — still works |
| All adapter files (~50) | Style handling delegated to TypedPropsApplier |
| Source generator | Doesn't reference Style |
| HMR | Doesn't reference Style |
| IDE/LSP | Uses string keys only |
| `Shared/Props/Typed/StyleKeys.cs` | String constants unchanged |
| `Shared/Props/Typed/CssHelpers.cs` | Helper constants unchanged |
| All .uitkx files | Syntax unchanged |

---

## <a name="allocation-budget"></a>11. Allocation Budget (Before vs After)

### Per styled element per render frame:

| Allocation | Before (dict Style) | After (typed Style) |
|------------|-------------------|-------------------|
| Style object | 1 dict (72+ bytes) | 1 class (~566 bytes) |
| Dict bucket array | 1 array (~224 bytes for 7 entries) | **0** |
| Boxed values | N (20 bytes each) | **0** |
| Dict entry structs | N (32 bytes each, in bucket array) | **0** |
| **Total objects** | **1 + 1 + N = 2 + N** | **1** |

### Stress test (3000 boxes × 7 style props):

| Metric | Before | After | Savings |
|--------|--------|-------|---------|
| Style objects | 3,000 | 3,000 | 0 |
| Bucket arrays | 3,000 | 0 | **3,000 allocs** |
| Boxed values | 18,000 | 0 | **18,000 allocs** |
| Dict entries | 21,000 (in arrays) | 0 | — (no separate allocs) |
| **Total GC objects** | **~24,000** | **~3,000** | **~21,000 allocs/frame** |
| **Total bytes** | **~1.25 MB** | **~1.70 MB** | -0.45 MB (more bytes, fewer objects) |

**Net effect:** Byte count increases slightly because typed Style objects are larger than
dict+boxed values. But GC object count drops by **~87.5%**. Since GC pause time is proportional
to **live object count** (not bytes), this is a massive GC improvement.

### Combined with Typed Props Pipeline:

| Optimization | Allocs eliminated/frame | Cumulative |
|-------------|----------------------|-----------|
| Typed Props (OPT-1 Layer 2) | ~6,000 (ToDictionary) | ~6,000 |
| Typed Style (this plan) | ~21,000 (boxing + bucket arrays) | ~27,000 |
| **Remaining per frame** | ~3,000 (Style class) + ~3,000 (BaseProps class) | ~6,000 |

The remaining ~6,000 allocs (Style + BaseProps objects) could be eliminated by:
- **Object pooling** (reuse Style/BaseProps from previous frame)
- **Struct-ifying** Style (would require careful nullable handling)
- **useMemo-style caching** at the user level

These are future optimizations beyond the scope of this plan.

---

## Appendix A: Bit Index Assignment

Assigning bit indices to match the order in StyleKeys.cs for consistency:

```
// _setBits0 (bits 0-63):
BIT_WIDTH                    = 0
BIT_HEIGHT                   = 1
BIT_MIN_WIDTH                = 2
BIT_MIN_HEIGHT               = 3
BIT_MAX_WIDTH                = 4
BIT_MAX_HEIGHT               = 5
BIT_FLEX_GROW                = 6
BIT_FLEX_SHRINK              = 7
BIT_FLEX_BASIS               = 8
BIT_FLEX_DIRECTION           = 9
BIT_FLEX_WRAP                = 10
BIT_JUSTIFY_CONTENT          = 11
BIT_ALIGN_ITEMS              = 12
BIT_ALIGN_SELF               = 13
BIT_ALIGN_CONTENT            = 14
BIT_POSITION                 = 15
BIT_LEFT                     = 16
BIT_TOP                      = 17
BIT_RIGHT                    = 18
BIT_BOTTOM                   = 19
BIT_DISPLAY                  = 20
BIT_VISIBILITY               = 21
BIT_OVERFLOW                 = 22
BIT_OPACITY                  = 23
BIT_MARGIN                   = 24
BIT_MARGIN_LEFT              = 25
BIT_MARGIN_RIGHT             = 26
BIT_MARGIN_TOP               = 27
BIT_MARGIN_BOTTOM            = 28
BIT_PADDING                  = 29
BIT_PADDING_LEFT             = 30
BIT_PADDING_RIGHT            = 31
BIT_PADDING_TOP              = 32
BIT_PADDING_BOTTOM           = 33
BIT_FONT_SIZE                = 34
BIT_FONT_FAMILY              = 35
BIT_TEXT_ALIGN                = 36
BIT_COLOR                    = 37
BIT_BACKGROUND_COLOR         = 38
BIT_BACKGROUND_IMAGE         = 39
BIT_BACKGROUND_IMAGE_TINT    = 40
BIT_WHITE_SPACE              = 41
BIT_LETTER_SPACING           = 42
BIT_TEXT_OVERFLOW             = 43
BIT_UNITY_FONT_STYLE         = 44
BIT_UNITY_TEXT_OUTLINE_COLOR  = 45
BIT_UNITY_TEXT_OUTLINE_WIDTH  = 46
BIT_UNITY_TEXT_OVERFLOW_POS   = 47
BIT_UNITY_TEXT_AUTO_SIZE      = 48
BIT_BORDER_WIDTH             = 49
BIT_BORDER_COLOR             = 50
BIT_BORDER_LEFT_WIDTH        = 51
BIT_BORDER_RIGHT_WIDTH       = 52
BIT_BORDER_TOP_WIDTH         = 53
BIT_BORDER_BOTTOM_WIDTH      = 54
BIT_BORDER_LEFT_COLOR        = 55
BIT_BORDER_RIGHT_COLOR       = 56
BIT_BORDER_TOP_COLOR         = 57
BIT_BORDER_BOTTOM_COLOR      = 58
BIT_BORDER_RADIUS            = 59
BIT_BORDER_TOP_LEFT_RADIUS   = 60
BIT_BORDER_TOP_RIGHT_RADIUS  = 61
BIT_BORDER_BOTTOM_LEFT_RAD   = 62
BIT_BORDER_BOTTOM_RIGHT_RAD  = 63

// _setBits1 (bits 0-17+):
BIT_ROTATE                   = 64  (stored as bit 0 of _setBits1)
BIT_SCALE                    = 65
BIT_TRANSLATE                = 66
BIT_BACKGROUND_REPEAT        = 67
BIT_BACKGROUND_POSITION_X    = 68
BIT_BACKGROUND_POSITION_Y    = 69
BIT_BACKGROUND_SIZE          = 70
BIT_TRANSFORM_ORIGIN         = 71
BIT_TRANSITION_DELAY         = 72
BIT_TRANSITION_DURATION      = 73
BIT_TRANSITION_PROPERTY      = 74
BIT_TRANSITION_TIMING_FUNC   = 75
// Unity 6.3+:
BIT_ASPECT_RATIO             = 76
BIT_FILTER                   = 77
BIT_UNITY_MATERIAL           = 78
```

Total: 79 properties (76 pre-Unity 6.3, 79 post-Unity 6.3). Fits in 2 × ulong (128 bits).

---

## Appendix B: Migration Path for User Code

### Phase 1 (this plan): Zero-disruption

- Tuple syntax `new Style { (StyleKeys.Left, 5f) }` continues working (boxes, but functional).
- Property setter syntax `new Style { Left = 5f }` works and is zero-boxing.
- No user code changes required.

### Phase 2 (follow-up): Source generator optimization

- Update source generator to emit property setter syntax instead of tuple syntax.
- User `.uitkx` files: `style={{ left: 5, top: 10 }}` compiles to `new Style { Left = 5f, Top = 10f }`.
- This eliminates boxing in the construction path too.
- **Breaking change:** None. The `.uitkx` syntax is unchanged. Only the generated C# differs.

### Phase 3 (future): Style pooling or struct

- Pool Style objects between frames (return previous frame's styles to pool after commit).
- Or convert Style to a struct (requires careful nullable handling — BaseProps.Style becomes `Style?`).
- Eliminates the remaining 3,000 Style class allocs per frame.
