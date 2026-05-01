# DOOM_BSP_SECTOR_ENGINE_UPGRADE.md

> **Goal**: Upgrade the current grid-tile raycaster Doom sample to a proper
> sector + linedef engine, similar to id Tech 1 (Doom 1993), then extend it
> beyond vanilla Doom's 2.5D limit with **stacked sectors / 3D floors** so
> we can build basements, multi-story buildings, and balconies.

---

## Research findings (2026-04-28 audit)

Current code is **3573 LOC** across 12 files, ~2840 LOC of game/types/render.

**Engine baseline**: Wolfenstein-3D-style uniform-grid DDA raycaster. Every
tile is fully solid or fully empty; floor/ceiling are flat at z=0/z=1; no
sectors, no linedefs, no portals, no Z movement, no LOS variation.

**Camera math**: `forward = (cosA, sinA)`, `right = (-sinA, cosA)`. Sprite
projection: `ty = dx·cosA + dy·sinA` (depth), `tx = -dx·sinA + dy·cosA`
(lateral). Pitch is screen-pixels of Y-shear, not radians.

**Critical invariants** (see audit §8 in [code-explorer report]):
- `OwnerId == -1` ⇒ projectile fired by player
- `Apply()` flips texture pixels before upload (UI Toolkit y=0 top vs Texture2D y=0 bottom)
- JSX `style={{cursor:...}}` is no-op — must use `el.style.cursor` C# direct
- `InputSystem.onAfterUpdate` is the only viable mouse-look channel under cursor lock
- Mob pool slots with `Id==0` are free; iterators must skip
- The wall-strip foreach in [DoomGameScreen.uitkx](Samples/Components/DoomGame/DoomGameScreen.uitkx) lines 90–110 is currently malformed — fix as part of phase 2

**Hotspots needing rewrite**: `CastFrame`/`CastRay` (full replace), `MoveActor`/`CollidesAt` (XYZ + line-segment), `UpdatePlayer`/`UpdateMonster`/`UpdateProjectile` (Z + sector context), `UpdateDoors` (per-sector animation), `BuildSpriteList` (true-3D Y + per-sector ordering), `MapBuilder` + all 3 levels (full DSL replacement).

---

## Why the upgrade

The grid raycaster is fast and clean but caps fidelity:

| Feature                          | Current (grid)           | Target (sector + 3D floors) |
| -------------------------------- | ------------------------ | --------------------------- |
| Variable floor/ceiling heights   | ❌ all walls full height | ✅ stepped floors, archways |
| Per-sector light levels          | ⚠ stored, ignored        | ✅ true sector lighting     |
| Non-orthogonal walls             | ❌                       | ✅ any 2D linedef           |
| Skylights (open ceilings)        | ❌                       | ✅                          |
| Lifts / crushers                 | ❌                       | ✅                          |
| Sprite occlusion order           | ⚠ painter's-algorithm    | ✅ per-sector + per-column  |
| Player Z (jump/crouch/fall)      | ❌                       | ✅                          |
| **Multi-story buildings**        | ❌                       | ✅ via 3D floors            |
| **Basements / overlapping levels** | ❌                     | ✅ via stacked sectors      |

We use the **ZDoom 3D-floor model** for verticality — extra floor planes
inside a sector create another walkable surface, with separate light/textures.
This avoids the complexity of full id Tech 2 brushes / true room-over-room
portals while still letting us build a 2-story balcony or a basement.

---

## Architecture overview

```
DoomTypes
├─ Vertex      { Vector2 P }
├─ Linedef     { int V1, V2, FrontSector, BackSector, Flags, MidTex,
│                UpperTex, LowerTex, Special, Tag }
├─ Sector      { float FloorZ, CeilingZ, byte Light, int FloorTex,
│                int CeilingTex, int Special, int Tag,
│                List<int> LineIds, List<ExtraFloor> ExtraFloors }
├─ ExtraFloor  { float BottomZ, TopZ, byte SideTex, byte TopTex,
│                byte BottomTex, byte Light } // ← multi-story
├─ Mobj        { Vector2 Pos, float Z, float ZVel, float Height,
│                int SectorId, MobjKind, ...existing... }
├─ PlayerState { ...existing..., float Z, float ZVel, float ViewHeight,
│                int SectorId }
├─ WallSeg     { ...existing..., float TopZ, float BotZ, byte SegKind }
│              // SegKind ∈ { Mid, Upper, Lower, ExtraTop, ExtraBot, ExtraSide }
├─ ColumnInfo  { List<WallSeg> Segs }     // ← multiple per column now
├─ FloorSpan   { int Y, int LeftCol, int RightCol, int SectorId,
│                bool IsCeiling, byte Light, byte TexIdx }
└─ FrameData   { ColumnInfo[] Columns, List<FloorSpan> FloorSpans,
                 float[,] DepthBuffer  // [col, ySpan]
               }

DoomMaps  (new MapBuilder)
└─  .Vertex(x, y) → vertexId
    .Sector(floorZ, ceilZ, light, floorTex, ceilTex) → sectorId
    .Line(v1, v2, frontSec, backSec, midTex, upperTex, lowerTex, special)
    .ExtraFloor(sectorId, bottomZ, topZ, sideTex, topTex, bottomTex, light)
    .Door(sectorId, lockKind)
    .Lift(sectorId, lowZ, highZ)
    .PlayerStart(x, y, angle, sectorId)
    .Spawn(kind, x, y, angle, sectorId, z?)

Raycast  (new module replacing DDA)
├─ struct WallHit  { float Distance, Vector2 Hit, int LinedefId,
│                    int FrontSec, int BackSec, float U,
│                    float FloorZ, float CeilZ, byte Flags }
└─ Cast(state, origin, originSec, angle) → List<WallHit>
        Walks origin sector → adjacent (via two-sided lines) → ...
        Stops at first one-sided/solid line or after MAX_HOPS=12.

Renderer  (rewritten CastFrame)
├─ For each of 320 columns:
│    hits = Raycast.Cast(...)
│    For each hit back-to-front:
│      compute screen Y for floor, ceil at this sector heights
│      emit mid wall seg (solid lines) OR upper+lower segs (portals)
│      if sector has ExtraFloors: emit extraTop/extraBot/extraSide segs
│      record floor/ceil clip Y per column
│  build FloorSpans by horizontal-scan of the per-column clip arrays
└─ Render: Sky, FloorSpans, sprites interleaved per sector, WallSegs
```

---

## Phased implementation plan

Each phase ships a runnable game. **Do not skip**. Phases 1-3 are the riskiest;
budget conservatively and verify at each step.

### Phase 1 — Data model swap (no rendering change) ★ checkpoint

- Add `Vertex`, `Linedef`, `Sector`, `ExtraFloor`, `MapData` to [DoomTypes.uitkx](Samples/Components/DoomGame/DoomTypes.uitkx).
- Add Z fields to `Mobj` and `PlayerState` (default 0; `Height` defaults to 1.6 player / per-kind for monsters).
- Keep existing `Cell[]` and `MapDef` intact alongside the new types.
- Write `MapData.FromTiles(MapDef)` that converts the existing 3 levels into vertex/linedef/sector form (1 sector per tile cell, 1 linedef per tile-to-tile boundary).
- Renderer / gameplay unchanged. Verify `MapData` matches grid via console log of sector/linedef counts.

**Deliverable**: same game, dual data model. ~300 LOC added.

### Phase 2 — Sector-aware portal renderer ★ checkpoint

- New module `Raycast.uitkx`. Implement `Cast(state, origin, originSec, angle)` that walks linedefs by ray-segment intersection, hopping to the back sector through two-sided lines. Stops at a solid line or `MAX_HOPS=12`.
- Replace the DDA grid hit loop in `GameLogic.CastFrame` with `Raycast.Cast`. For phase 2, every sector still has FloorZ=0 / CeilingZ=1 (uniform), every linedef is one-sided. Visually identical to phase 1.
- Fix the malformed wall-strip block in [DoomGameScreen.uitkx](Samples/Components/DoomGame/DoomGameScreen.uitkx) lines 90–110 while we're rewriting it.
- Rewrite `MoveActor`/`CollidesAt` to test against linedef segments (point-to-segment distance + side test) instead of cell scan. Sector lookup via `PointInSector(x, y, hint)` walking from a hint sector through neighboring sectors (cheap because we know which sector we were in last frame).

**Deliverable**: same visuals, new traversal. ~600 LOC. Verify wall pixels match grid renderer within ±1 px on 50+ test camera positions.

### Phase 3 — Variable floor/ceiling heights ★ checkpoint

- Per-sector `FloorZ` / `CeilingZ` consumed by renderer.
- For two-sided lines where neighbor floor is higher → emit lower wall seg textured with `LowerTex`. Where neighbor ceiling is lower → emit upper wall seg with `UpperTex`.
- Player `Z` snaps to `sectors[player.SectorId].FloorZ` each tick. Step-up: when crossing into a sector whose `FloorZ - player.Z <= 0.4`, allow it; otherwise block.
- Monsters/mobs same.
- Add gravity for projectiles only (player still snaps to floor).
- Convert level 2 to demonstrate stepped rooms (sunken nukage, raised platform).

**Deliverable**: visible step-up rooms. ~250 LOC.

### Phase 4 — Sector lighting ★ checkpoint

- Modulate wall/floor/ceiling/sprite tint by `sector.Light / 255`.
- Add a light-flicker sector-special (random light pulse).
- 1 dim and 1 bright sector per level for visual variety.

**Deliverable**: dynamic lighting variance. ~80 LOC.

### Phase 5 — Doors as moving sectors ★ checkpoint

- Replace `Cell.DoorState/DoorTimer` with **door = sector with animated CeilingZ**.
- `E` on a door-tagged linedef triggers the back sector's CeilingZ to lerp from FloorZ → FloorZ + 1.0.
- Renderer naturally handles it (just a moving ceiling).
- Locked doors check `Player.Keys` against the linedef's `Special` field.
- Auto-close after 4 seconds (doom standard).
- Add lifts (animated FloorZ).

**Deliverable**: smooth door + lift mechanics. ~150 LOC.

### Phase 6 — Sprite ordering via sector graph ★ checkpoint

- Group mobjs by `SectorId`.
- Renderer interleaves sprites between wall passes for each sector (per-sector painter's order, but sectors are visited front-to-back via the raycaster traversal).
- Per-column horizontal clip range so sprites get partially occluded by pillars instead of popping.

**Deliverable**: sprites correctly hidden behind walls and pillars. ~120 LOC.

### Phase 7 — 3D floors / multi-story / basements ★ checkpoint ★ headline feature

- Implement `Sector.ExtraFloors` rendering: each ExtraFloor produces a top plane, bottom plane, and side wall in the column renderer.
- Player/mob collision considers ExtraFloors: stepping onto a 3D floor changes effective `floorZ`, ceiling becomes ExtraFloor.BottomZ.
- LOS / projectiles clip against ExtraFloor planes.
- Add `Jump` (Space) and `Crouch` (Ctrl) input.
- Player gains `ZVel` + gravity when not standing on a surface.

**Demo levels**:
- **Level 1**: Add a 2-story watchtower outside the exit room (climb stairs to upper level, snipe from above).
- **Level 2**: Add a basement under the central courtyard (descend stairs in the SE corner, basement holds the mega-armor).
- **Level 3**: Add a 2-story Baron tower in the center (Baron on top, BFG on the ground floor).

**Deliverable**: actual multi-story gameplay. ~400 LOC.

### Phase 8 — Polish

- Translucent middle textures on two-sided lines (windows, bars).
- Linedef specials (cross-line teleports, switch-activated remote doors).
- Sky picker per sector (different ceilings show different skies).
- HUD ARMS panel index bug fix.
- `MELEE_RANGE` constant unification.
- Mob pool middle-hole defragment.

**Deliverable**: consistent, polished build. ~200 LOC.

---

## Risks & gotchas

1. **Sector lookup correctness** — every player/mob movement needs `MoveAndUpdateSector` that re-resolves which sector after crossing a linedef. Bug here = falling through floors.
2. **Convex sectors only** — phase 2-3 require sectors be convex. Concave breaks the "walk-adjacent" cast. Add a `MapBuilder.AssertConvex(sectorId)` in Phase 1.
3. **Ray hop limit** — cap at 12 to avoid runaway loops in pathological topology.
4. **Two-sided line texture coords** — Doom convention top-down differs from our `Apply()` flip. Document and unit-test before any wall-pixel-match check.
5. **Performance** — sector raycaster is `O(rays × hops × lines_per_sector)` ≈ 320 × 4 × 6 = 7,680 line tests/frame. Should hit 60 FPS easily on the test maps. **Watch `Vector2` allocations** — keep intersection math in `struct Vector2` value types only.
6. **3D floor sprite ordering** — a sprite on an upper 3D floor must not draw over a wall in front of it. Solution: emit one painter's pass per (sector, ExtraFloor index) tier.
7. **Player crossing 3D-floor edge** — must check both the main sector floor AND each ExtraFloor at the player's `(x, y)`. Pick the highest floor below `player.Z + step` and the lowest ceiling above `player.Z + height`.
8. **Wall strip foreach is currently malformed** — fixing it is mandatory in Phase 2.

---

## Test plan

- **Phase 1**: log shows sector/linedef counts; game still runs unchanged.
- **Phase 2**: pick 50 camera positions, ray-trace under both engines, assert ±1 px wall agreement.
- **Phase 3**: visual review — climb steps in level 2, can't walk through high steps.
- **Phase 4**: dim/bright sector boundary clearly visible.
- **Phase 5**: door animates closed→open in ~1.0s, auto-closes in 4s, key locks rejected.
- **Phase 6**: enemy fully hidden behind a wall, partially clipped behind a pillar.
- **Phase 7**: player jumps onto a 2nd-story balcony, drops down, and the BFG-in-basement is reachable only by descending stairs.
- **Phase 8**: window-mid-textures show translucency, kills/score consistent.

---

## Files added / changed

```
NEW   Samples/Components/DoomGame/Raycast.uitkx                  (~400 LOC)
EDIT  Samples/Components/DoomGame/DoomTypes.uitkx                (~+300)
EDIT  Samples/Components/DoomGame/DoomMaps.uitkx                 (full DSL rewrite)
EDIT  Samples/Components/DoomGame/GameLogic.uitkx                (~+800 net new)
EDIT  Samples/Components/DoomGame/DoomGameScreen.uitkx           (renderer refactor)
EDIT  Samples/Components/DoomGame/DoomGameScreenLogic.uitkx      (3D Y + sector ordering)
EDIT  Samples/Components/DoomGame/DoomGameScreen.hooks.uitkx     (jump/crouch input)
```

---

## Out of scope

- Real BSP tree construction (sector-adjacency walk is sufficient for these maps).
- WAD file loading (procedural maps only).
- Multiplayer / netcode.
- Mod / DEH support.
- Slopes (we keep horizontal floors/ceilings).
- True non-Euclidean portals (would need a separate matrix stack).

---

## Estimate

Sequenced — do not parallelize. Total ≈ 2000 net LOC of new code spread across
the 8 phases. The risky middle (phases 2-3 + 7) is where most surprises will
land. Start with phase 1 since it doesn't change behavior and gives a clean
rollback point.
