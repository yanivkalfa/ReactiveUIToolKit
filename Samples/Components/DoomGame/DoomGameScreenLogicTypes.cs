using UnityEngine;
using static ReactiveUITK.Samples.Components.DoomGame.DoomTypes;

namespace ReactiveUITK.Samples.Components.DoomGame {
  public struct SpriteEntry {
    public int Id;
    public int SpriteIdx;
    public float ScreenX;
    public float ScreenY;
    public float Size;
    public float Distance;
    public Color32 Tint;
    public float Light;
  }

  public struct ExtraSegEntry {
    public int ColIndex;
    public int SegIndex; // index within the column's Extras array (stable key)
    public WallSeg Seg;
  }

  public struct FloorBandEntry {
    public int ColIndex;
    public FloorBand Band;
  }

  public struct MergedFloorBand {
    public int ColStart;
    public int ColEnd; // inclusive
    public float TopPx;
    public float BotPx;
    public int SlabId;
    public byte Light;
    public byte FloorTex;
    public float FloorZ;
    public int BehindSlabId; // slab immediately behind in the ray (=SlabId if none)
    public float BehindFloorZ; // FloorZ of slab immediately behind
    public bool RimAtFar; // far edge is a visible step-down
  }

  public struct CeilingBandEntry {
    public int ColIndex;
    public int SlabId;
    public CeilingBand Band;
  }

  public struct TracerEntry {
    public int Slot;
    public float Left, Top;
    public float Length;
    public float AngleDeg;
    public float Alpha;
    public byte ColorIdx;
  }
}
