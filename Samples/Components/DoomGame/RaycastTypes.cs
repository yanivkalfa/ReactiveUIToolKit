using UnityEngine;

namespace ReactiveUITK.Samples.Components.DoomGame {
  public struct WallHit {
    public float Distance; // ray parameter t at hit
    public Vector2 Hit; // world-space (x,y)
    public int LinedefId;
    public int FromSector; // sector we were traversing when we hit
    public int ToSector; // -1 if solid wall, else neighbor sector
    public bool IsBackside; // ray hit the line from its back side
    public float U; // 0..1 along the linedef (V1→V2)
    public float SegLength; // |V2 − V1| in world units
    public bool IsSky; // back sector has IsSky and ceiling above
  }
}
