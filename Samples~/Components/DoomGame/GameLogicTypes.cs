namespace ReactiveUITK.Samples.Components.DoomGame {
  internal struct RayHit {
    public float Distance;
    public byte WallTexIdx;
    public float TexU;
    public byte Light;
    public bool HitVertical;
    public bool IsSky;
  }

  internal struct ShotHit {
    public bool HitMobj;
    public int MobjIdx;
    public float X, Y;
  }
}
