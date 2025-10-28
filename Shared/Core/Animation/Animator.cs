using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Core.Animation
{
    public sealed class AnimationHandle
    {
        internal IVisualElementScheduledItem item;
        internal Action onStop;

        public void Stop()
        {
            try
            {
                item?.Pause();
            }
            catch { }
            try
            {
                onStop?.Invoke();
            }
            catch { }
            item = null;
            onStop = null;
        }
    }

    public sealed class AnimateTrack
    {
        public string Property; // e.g., "opacity", "backgroundColor", "width" ...
        public object From; // float or Color (optional; null means read current)
        public object To; // float or Color
        public float Duration; // seconds
        public float Delay; // seconds
        public Ease Ease = Ease.EaseInOutCubic;

        // Playback
        public int Repeat; // number of extra cycles after first (0 = play once)
        public bool Loop; // ignore Repeat and loop indefinitely
        public bool Yoyo; // reverse direction every cycle
        public float TimeScale = 1f; // scale time for this track

        // Callbacks
        public Action<float> OnUpdate; // normalized 0..1 per cycle (after easing)
        public Action OnComplete; // called when finished (not for Loop)
    }

    internal static class Animator
    {
        // Track one active animation per element per property
        private static readonly ConditionalWeakTable<
            VisualElement,
            Dictionary<string, AnimationHandle>
        > active = new();

        public static List<AnimationHandle> PlayTracks(
            VisualElement ve,
            IReadOnlyList<AnimateTrack> tracks
        )
        {
            var list = new List<AnimationHandle>();
            if (ve == null || tracks == null || tracks.Count == 0)
                return list;
            foreach (var t in tracks)
            {
                if (t == null)
                    continue;
                var h = PlayTrack(ve, t);
                if (h != null)
                    list.Add(h);
            }
            return list;
        }

        private static AnimationHandle PlayTrack(VisualElement ve, AnimateTrack track)
        {
            if (ve == null || track == null)
                return null;
            float duration = Mathf.Max(0f, track.Duration);
            float delay = Mathf.Max(0f, track.Delay);
            string prop = (track.Property ?? string.Empty).Trim();

            // Determine current value if From is null
            object from = track.From ?? ReadCurrent(ve, prop);
            object to = track.To;
            if (to == null || from == null)
                return null;

            var handle = new AnimationHandle();
            double startTime = 0;
            bool started = false;

            // Stop any previous animation running on the same property
            var map = active.GetValue(
                ve,
                _ => new Dictionary<string, AnimationHandle>(StringComparer.Ordinal)
            );
            if (map.TryGetValue(prop, out var existing) && existing != null)
            {
                try
                {
                    existing.Stop();
                }
                catch { }
            }
            map[prop] = handle;

            // Per-frame scheduler
            handle.item = ve
                .schedule.Execute(() =>
                {
                    if (ve.panel == null)
                    {
                        // Element detached: stop animation
                        handle.Stop();
                        return;
                    }

                    double now;
                    try
                    {
                        now = Time.realtimeSinceStartupAsDouble;
                    }
                    catch
                    {
                        now = (double)Time.realtimeSinceStartup;
                    }

                    if (!started)
                    {
                        startTime = now + delay;
                        started = true;
                    }
                    if (now < startTime)
                    {
                        return;
                    }

                    float scaledDuration = duration / Mathf.Max(0.0001f, track.TimeScale);
                    if (scaledDuration <= 0f)
                        scaledDuration = 0.0001f;

                    // Compute per-cycle progress for repeats/yoyo
                    double elapsed = now - startTime;
                    float cycleT = Mathf.Clamp01(
                        (float)(elapsed % scaledDuration) / scaledDuration
                    );
                    int cycleIndex = (int)Math.Floor(elapsed / scaledDuration);
                    bool reversed = track.Yoyo && (cycleIndex % 2 == 1);
                    float t = reversed ? (1f - cycleT) : cycleT;
                    float eased = Easing.Evaluate(track.Ease, t);
                    object v = Lerp(from, to, eased);
                    Apply(ve, prop, v);
                    try
                    {
                        track.OnUpdate?.Invoke(eased);
                    }
                    catch { }

                    // Decide termination
                    if (!track.Loop)
                    {
                        int totalCycles = 1 + Math.Max(0, track.Repeat);
                        if (cycleIndex >= totalCycles - 1 && cycleT >= 1f - 1e-4f)
                        {
                            // Final value is 'to' unless odd yoyo leaves us at 'from'
                            object final = (track.Yoyo && ((totalCycles - 1) % 2 == 1)) ? from : to;
                            Apply(ve, prop, final);
                            try
                            {
                                track.OnComplete?.Invoke();
                            }
                            catch { }
                            handle.Stop();
                        }
                    }
                })
                .Every(16); // ~60 FPS

            handle.onStop = () =>
            {
                // Clear active slot if we are the current one
                if (active.TryGetValue(ve, out var m))
                {
                    if (m.TryGetValue(prop, out var h) && object.ReferenceEquals(h, handle))
                    {
                        m.Remove(prop);
                    }
                }
            };
            return handle;
        }

        private static object ReadCurrent(VisualElement ve, string prop)
        {
            switch (prop)
            {
                case "opacity":
                {
                    float v = 1f;
                    try
                    {
                        v = ve.resolvedStyle.opacity;
                    }
                    catch { }
                    return v;
                }
                case "backgroundColor":
                {
                    Color c = ve.resolvedStyle.backgroundColor;
                    return c;
                }
                case "color":
                {
                    Color c = ve.resolvedStyle.color;
                    return c;
                }
            }
            return null;
        }

        private static object Lerp(object from, object to, float t)
        {
            if (from is float ff && to is float tf)
            {
                return Mathf.Lerp(ff, tf, t);
            }
            if (from is Color fc && to is Color tc)
            {
                return Color.Lerp(fc, tc, t);
            }
            return to;
        }

        private static void Apply(VisualElement ve, string prop, object value)
        {
            if (value == null)
                return;
            switch (prop)
            {
                case "opacity":
                {
                    if (value is float f)
                    {
                        ve.style.opacity = f;
                    }
                    break;
                }
                case "width":
                {
                    if (TryToFloat(value, out var f))
                        ve.style.width = new Length(f, LengthUnit.Pixel);
                    break;
                }
                case "height":
                {
                    if (TryToFloat(value, out var f))
                        ve.style.height = new Length(f, LengthUnit.Pixel);
                    break;
                }
                case "minWidth":
                {
                    if (TryToFloat(value, out var f))
                        ve.style.minWidth = new Length(f, LengthUnit.Pixel);
                    break;
                }
                case "minHeight":
                {
                    if (TryToFloat(value, out var f))
                        ve.style.minHeight = new Length(f, LengthUnit.Pixel);
                    break;
                }
                case "maxWidth":
                {
                    if (TryToFloat(value, out var f))
                        ve.style.maxWidth = new Length(f, LengthUnit.Pixel);
                    break;
                }
                case "maxHeight":
                {
                    if (TryToFloat(value, out var f))
                        ve.style.maxHeight = new Length(f, LengthUnit.Pixel);
                    break;
                }
                case "marginLeft":
                {
                    if (TryToFloat(value, out var f))
                        ve.style.marginLeft = new Length(f, LengthUnit.Pixel);
                    break;
                }
                case "marginRight":
                {
                    if (TryToFloat(value, out var f))
                        ve.style.marginRight = new Length(f, LengthUnit.Pixel);
                    break;
                }
                case "marginTop":
                {
                    if (TryToFloat(value, out var f))
                        ve.style.marginTop = new Length(f, LengthUnit.Pixel);
                    break;
                }
                case "marginBottom":
                {
                    if (TryToFloat(value, out var f))
                        ve.style.marginBottom = new Length(f, LengthUnit.Pixel);
                    break;
                }
                case "paddingLeft":
                {
                    if (TryToFloat(value, out var f))
                        ve.style.paddingLeft = new Length(f, LengthUnit.Pixel);
                    break;
                }
                case "paddingRight":
                {
                    if (TryToFloat(value, out var f))
                        ve.style.paddingRight = new Length(f, LengthUnit.Pixel);
                    break;
                }
                case "paddingTop":
                {
                    if (TryToFloat(value, out var f))
                        ve.style.paddingTop = new Length(f, LengthUnit.Pixel);
                    break;
                }
                case "paddingBottom":
                {
                    if (TryToFloat(value, out var f))
                        ve.style.paddingBottom = new Length(f, LengthUnit.Pixel);
                    break;
                }
                case "borderLeftWidth":
                {
                    if (TryToFloat(value, out var f))
                        ve.style.borderLeftWidth = f;
                    break;
                }
                case "borderRightWidth":
                {
                    if (TryToFloat(value, out var f))
                        ve.style.borderRightWidth = f;
                    break;
                }
                case "borderTopWidth":
                {
                    if (TryToFloat(value, out var f))
                        ve.style.borderTopWidth = f;
                    break;
                }
                case "borderBottomWidth":
                {
                    if (TryToFloat(value, out var f))
                        ve.style.borderBottomWidth = f;
                    break;
                }
                case "letterSpacing":
                {
                    if (TryToFloat(value, out var f))
                        ve.style.letterSpacing = f;
                    break;
                }
                case "backgroundColor":
                {
                    if (value is Color c)
                    {
                        ve.style.backgroundColor = c;
                    }
                    break;
                }
                case "color":
                {
                    if (value is Color cc)
                    {
                        ve.style.color = cc;
                    }
                    break;
                }
                case "unityTextOutlineColor":
                {
                    if (value is Color oc)
                    {
                        try
                        {
                            ve.style.unityTextOutlineColor = oc;
                        }
                        catch { }
                    }
                    break;
                }
                case "unityTextOutlineWidth":
                {
                    if (TryToFloat(value, out var f))
                    {
                        try
                        {
                            ve.style.unityTextOutlineWidth = f;
                        }
                        catch { }
                    }
                    break;
                }
                case "translateX":
                {
                    if (TryToFloat(value, out var f))
                    {
                        try
                        {
                            var st = ve.style.translate;
                            var cur = st.value; // struct copy
                            var next = new Translate(new Length(f, LengthUnit.Pixel), cur.y, cur.z);
                            ve.style.translate = new StyleTranslate(next);
                        }
                        catch { }
                    }
                    break;
                }
                case "translateY":
                {
                    if (TryToFloat(value, out var f))
                    {
                        try
                        {
                            var st = ve.style.translate;
                            var cur = st.value; // struct copy
                            var next = new Translate(cur.x, new Length(f, LengthUnit.Pixel), cur.z);
                            ve.style.translate = new StyleTranslate(next);
                        }
                        catch { }
                    }
                    break;
                }
            }
        }

        private static bool TryToFloat(object value, out float f)
        {
            if (value is float ff)
            {
                f = ff;
                return true;
            }
            if (value is int ii)
            {
                f = ii;
                return true;
            }
            if (value is double dd)
            {
                f = (float)dd;
                return true;
            }
            f = 0f;
            return false;
        }
    }
}
