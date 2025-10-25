using UnityEngine;

namespace ReactiveUITK.Core.Util
{
    public struct SafeAreaInsets
    {
        public float Left;
        public float Right;
        public float Top;
        public float Bottom;

        public bool EqualsApproximately(SafeAreaInsets other, float tolerance = 0.5f)
        {
            return Mathf.Abs(Left - other.Left) <= tolerance &&
                   Mathf.Abs(Right - other.Right) <= tolerance &&
                   Mathf.Abs(Top - other.Top) <= tolerance &&
                   Mathf.Abs(Bottom - other.Bottom) <= tolerance;
        }
    }

    public static class SafeAreaUtility
    {
        public static Rect GetSafeAreaRect() => Screen.safeArea;

        public static SafeAreaInsets GetInsets()
        {
            Rect sa = Screen.safeArea;
            float left = sa.x;
            float right = Mathf.Max(0f, Screen.width - sa.xMax);
            float bottom = sa.y;
            float top = Mathf.Max(0f, Screen.height - sa.yMax);
            return new SafeAreaInsets { Left = left, Right = right, Top = top, Bottom = bottom };
        }
    }
}

