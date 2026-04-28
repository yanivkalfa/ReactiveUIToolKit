# DOOM_BSP_SECTOR_ENGINE_UPGRADE.md

> **Goal**: Upgrade the current grid-tile raycaster Doom sample to a proper
> BSP-lite / sector-based engine, closer to vanilla Doom (1993) rather than
> Wolfenstein 3D (1992).
>
> Current state lives in `Samples/Components/DoomGame/` and is a Wolf3D-style
> uniform-grid DDA raycaster with full-height walls and flat floors. This
> upgrade replaces the world model with **sectors + linedefs**, keeps the
> column renderer, and adds variable floor/ceiling heights, sector lighting,
> doors-as-sector-movement, and basic BSP-style traversal for sprite ordering.

---

## Why the upgrade

The grid raycaster is fast and clean but caps the level of fidelity:

| Feature                          | Grid raycaster (current) | BSP-lite (target)       |
| -------------------------------- | ------------------------ | ----------------------- |
| Variable floor/ceiling heights   | ❌ all walls full height | ✅ stepped floors, lifts |
| Per-sector light levels          | ⚠ hacked via distance    | ✅ true sector lighting  |
| Non-orthogonal walls             | ❌                       | ✅ any 2D linedef        |
| Skylights (open ceilings)        | ❌                       | ✅                       |
| Vertical translucent windows     | ❌                       | ✅                       |
| Lifts / crushers / moving floors | ❌                       | ✅                       |
| Sprite occlusion order           | ⚠ painter's-by-distance  | ✅ proper front-to-back  |

The cost is: **map data complexity grows ~10×**, and the renderer needs a
**segment clip list per column** instead of a single hit. We mitigate by
keeping levels small (≤ 32 sectors) and using a simple convex-sector
constraint (BSP-lite — no full BSP tree build, just sector adjacency).

---

## Architecture overview

```
DoomTypes
├─ Vertex      { Vector2 P }
├─ Linedef     { int V1, V2, FrontSector, BackSector, Flags, MidTex, UpperTex, LowerTex }
├─ Sector      { float FloorH, CeilingH, byte Light, int FloorTex, int CeilingTex,
│                int SpecialKind, List<int> LineIds }
├─ Mobj        { Vector2 Pos, float Z, int SectorId, MobjKind, ...existing... }
└─ MapData     { Vertex[] Vertices, Linedef[] Lines, Sector[] Sectors, Mobj[] Things }

DoomMaps
└─ MapBuilder DSL extended:
      .Sector(floor, ceiling, light, floorTex, ceilingTex)
      .Vertex(x, y) → vertexId
      .Line(v1, v2, frontSec, backSec, midTex)
      .Door(v1, v2, frontSec, backSec)        // helper: closed sector w/ tag
      .Lift(v1, v2, frontSec, backSec, lo, hi)

Raycast (NEW module)
├─ struct WallHit { float Distance, Vector2 Hit, int LinedefId, bool IsBack,
│                   int Sector, float U, bool IsTwoSided, int LowerStart,
│                   int LowerEnd, int UpperStart, int UpperEnd }
├─ struct ColumnSegments { List<WallHit> SolidHits } // up to ~6 per column
└─ Cast(state, originPos, originSector, angle) → ColumnSegments
        Walks sector → adjacent sector through linedef intersections.
        Stops at solid (one-sided) wall or after MAX_HOPS (~8).

DoomGameScreen renderer
├─ For each of 320 columns:
│    cs = Raycast.Cast(...)
│    For each segment back-to-front:
│      compute ScreenTop/ScreenBottom for floor & ceiling at sector heights
│      emit 1..3 textured slices (mid wall, upper step, lower step)
│      for two-sided lines: peek through to next sector
└─ Floor/ceiling: use **per-column horizon spans** (textured floor slabs,
   one band per visible sector). Replace the current 10 hardcoded bands.

DoomGameScreen.hooks: unchanged input pipeline, but mouse-look pitch
becomes a true Y-shear on the floor projection (already partially there).
```

---

## Phased implementation plan

Each phase ships a working game. Don't attempt all at once.

### Phase 1 — Data model swap (no rendering change)

- Add `Vertex`, `Linedef`, `Sector` types to `DoomTypes.uitkx`.
- Add `MapData` alongside the existing `MapTile[,]`.
- Write `MapBuilder.FromTiles()` that converts the existing 3 levels into
  vertex/linedef/sector form (each tile cell → 1 sector with FloorH=0,
  CeilingH=64; each tile-to-tile boundary → 1 linedef).
- Renderer still uses the tile grid. No visible change.

**Deliverable**: same game, dual data model. Verify `MapData` matches grid
via console log of sector count vs floor area.

### Phase 2 — Sector-aware raycaster

- New `Raycast.uitkx` module. Implement `Cast()` that, given a starting
  sector, walks linedefs by ray-segment intersection until it hits a solid
  one-sided line.
- Replace the DDA grid hit loop in `GameLogic.CastFrame` with `Raycast.Cast`.
- For phase 2: every linedef is one-sided + every sector has FloorH=0 +
  CeilingH=64. Visually identical to phase 1.

**Deliverable**: same visuals, new traversal. Profile to confirm ≤ 1.5×
slowdown vs grid DDA on the 32×32 test map.

### Phase 3 — Variable heights (steps + ceilings)

- Pick 2-3 sectors per map and bump their FloorH (steps) or lower their
  CeilingH (low arches).
- In renderer: emit upper-wall slices when adjacent sector ceiling is
  lower; emit lower-wall slices when adjacent floor is higher. Texture
  with `UpperTex`/`LowerTex` from the linedef.
- Player Z-snap: `player.Z = sectors[player.Sector].FloorH` each tick.

**Deliverable**: visible step-up rooms in level 2.

### Phase 4 — Sector lighting

- Add `Sector.Light` (0..255). Modulate column tint by `sector.Light/255f`
  in addition to existing distance shading.
- Add 1 dim sector + 1 bright sector per level for testing.

**Deliverable**: dynamic light variance across rooms.

### Phase 5 — Doors as sector movement

- Replace `MapTile.IsDoor` flag with **door = sector with CeilingH animating
  between FloorH (closed) and FloorH+64 (open)**.
- `E` key on a door-tagged linedef triggers the adjacent sector's
  CeilingH lerp.
- Renderer naturally handles this — it's just a moving ceiling.

**Deliverable**: doors animate smoothly via sector mechanics, not tile flips.

### Phase 6 — Sprite ordering via sector graph

- Group mobjs by sector. When rendering each ray segment, also draw the
  sprites in that sector clipped to the segment's screen range.
- Removes the painter's-algorithm distance sort entirely.

**Deliverable**: sprites correctly hidden behind walls (currently they pop
through corners).

### Phase 7 — Polish (optional)

- Lifts (animated FloorH).
- Translucent middle textures on two-sided lines (windows, bars).
- Linedef specials (triggers): cross-line teleports, switch-activated doors.
- `Z`-aware projectiles (rocket arcs over short walls).

---

## Risks & gotchas

1. **Sector lookup**: every player/mob movement needs `MoveAndUpdateSector`
   that re-resolves which sector it's in after crossing a linedef. Bug here
   = falling through floors.
2. **Convex sectors only** in phase 2-3. Concave sectors break the
   "walk-adjacent" cast. Add a "convex assert" in MapBuilder.
3. **Ray hop limit**: a ray through 8+ sectors per column is rare but
   possible. Cap at 16 to avoid runaway loops.
4. **Two-sided line texture coords**: `UpperTex`/`LowerTex` Y-origin
   conventions differ between Doom (top-down) and our `Apply()` flip.
   Document and unit-test.
5. **Performance**: sector raycaster is `O(rays × hops × lines_per_sector)`.
   For 320 rays × 4 avg hops × 6 lines/sector ≈ 7,680 line tests/frame.
   Should still hit 60 FPS easily. Watch the `Vector2` allocations — keep
   intersection math in stack structs.

---

## Test plan

- **Phase 1**: golden-master byte-compare a `MapData` snapshot.
- **Phase 2**: same wall pixels as grid raycaster within ±1 pixel for
  100 sample camera positions.
- **Phase 3**: visual review (step rooms render correctly).
- **Phase 4**: a known dim-then-bright doorway shows clear contrast.
- **Phase 5**: door animation completes in the scripted duration.
- **Phase 6**: an enemy behind a wall is invisible from all angles.

---

## Files added / changed

```
NEW   Samples/Components/DoomGame/Raycast.uitkx
NEW   Samples/Components/DoomGame/DoomMapData.uitkx       (Vertex/Linedef/Sector)
EDIT  Samples/Components/DoomGame/DoomTypes.uitkx         (Sector field on Mobj)
EDIT  Samples/Components/DoomGame/DoomMaps.uitkx          (MapBuilder DSL extended)
EDIT  Samples/Components/DoomGame/GameLogic.uitkx         (CastFrame → Raycast.Cast)
EDIT  Samples/Components/DoomGame/DoomGameScreen.uitkx    (segmented column renderer)
```

---

## Out of scope

- Real BSP tree construction (we use sector adjacency walk instead).
- WAD file loading (procedural maps only).
- Multiplayer / netcode.
- Mod / DEH support.
- True 3D floors / room-over-room (would need slopes + portals).

---

## Estimate

Sequenced — do not parallelize. Phases 2 & 3 are the risky ones; budget
generously.

When ready, start with **Phase 1** since it doesn't change behavior and
gives us a clean rollback point if the model needs revision.
