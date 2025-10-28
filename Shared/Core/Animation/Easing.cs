using System;

namespace ReactiveUITK.Core.Animation
{
    public enum Ease
    {
        Linear,
        EaseInSine,
        EaseOutSine,
        EaseInOutSine,
        EaseInQuad,
        EaseOutQuad,
        EaseInOutQuad,
        EaseInCubic,
        EaseOutCubic,
        EaseInOutCubic,
        EaseInQuint,
        EaseOutQuint,
        EaseInOutQuint,
        EaseInExpo,
        EaseOutExpo,
        EaseInOutExpo,
        EaseInBack,
        EaseOutBack,
        EaseInOutBack,
    }

    public static class Easing
    {
        public static float Evaluate(Ease ease, float t)
        {
            t = UnityEngine.Mathf.Clamp01(t);
            switch (ease)
            {
                case Ease.EaseInSine:
                    return 1f - UnityEngine.Mathf.Cos((t * UnityEngine.Mathf.PI) / 2f);
                case Ease.EaseOutSine:
                    return UnityEngine.Mathf.Sin((t * UnityEngine.Mathf.PI) / 2f);
                case Ease.EaseInOutSine:
                    return -(UnityEngine.Mathf.Cos(UnityEngine.Mathf.PI * t) - 1f) / 2f;

                case Ease.EaseInQuad:
                    return t * t;
                case Ease.EaseOutQuad:
                    return 1f - (1f - t) * (1f - t);
                case Ease.EaseInOutQuad:
                    return t < 0.5f ? 2f * t * t : 1f - UnityEngine.Mathf.Pow(-2f * t + 2f, 2f) / 2f;

                case Ease.EaseInCubic:
                    return t * t * t;
                case Ease.EaseOutCubic:
                {
                    float u = 1f - t;
                    return 1f - (u * u * u);
                }
                case Ease.EaseInOutCubic:
                    return t < 0.5f ? 4f * t * t * t : 1f - UnityEngine.Mathf.Pow(-2f * t + 2f, 3f) / 2f;

                case Ease.EaseInQuint:
                    return t * t * t * t * t;
                case Ease.EaseOutQuint:
                    return 1f - UnityEngine.Mathf.Pow(1f - t, 5f);
                case Ease.EaseInOutQuint:
                    return t < 0.5f ? 16f * t * t * t * t * t : 1f - UnityEngine.Mathf.Pow(-2f * t + 2f, 5f) / 2f;

                case Ease.EaseInExpo:
                    return t == 0f ? 0f : UnityEngine.Mathf.Pow(2f, 10f * t - 10f);
                case Ease.EaseOutExpo:
                    return t == 1f ? 1f : 1f - UnityEngine.Mathf.Pow(2f, -10f * t);
                case Ease.EaseInOutExpo:
                    return t == 0f
                        ? 0f
                        : t == 1f
                            ? 1f
                            : t < 0.5f
                                ? UnityEngine.Mathf.Pow(2f, 20f * t - 10f) / 2f
                                : (2f - UnityEngine.Mathf.Pow(2f, -20f * t + 10f)) / 2f;

                case Ease.EaseInBack:
                {
                    const float c1 = 1.70158f; const float c3 = c1 + 1f;
                    return c3 * t * t * t - c1 * t * t;
                }
                case Ease.EaseOutBack:
                {
                    const float c1 = 1.70158f; const float c3 = c1 + 1f; float u = t - 1f;
                    return 1f + c3 * u * u * u + c1 * u * u;
                }
                case Ease.EaseInOutBack:
                {
                    const float c1 = 1.70158f; const float c2 = c1 * 1.525f;
                    return t < 0.5f
                        ? (UnityEngine.Mathf.Pow(2f * t, 2f) * ((c2 + 1f) * 2f * t - c2)) / 2f
                        : (UnityEngine.Mathf.Pow(2f * t - 2f, 2f) * ((c2 + 1f) * (t * 2f - 2f) + c2) + 2f) / 2f;
                }

                case Ease.Linear:
                default:
                    return t;
            }
        }
    }
}
