using System;
using UnityEngine;

namespace ReactiveUITK.Core.Animation
{
    /// <summary>
    /// Panel-independent per-frame ticker for animations and tweens.
    /// VisualElement.schedule is bound to its panel and pauses (and is in
    /// some cases reset) when the element is removed from a panel — this
    /// breaks any animation whose host UI tree is transiently reparented
    /// by a UIDocument rebuild (undo, asset swap, disable/enable, editor
    /// playmode selection storm).
    ///
    /// AnimationTicker fires every frame regardless of panel state. In
    /// editor builds it drives off EditorApplication.update (which fires
    /// in both edit mode and editor play mode); in standalone builds it
    /// piggy-backs on MediaHost.SubscribeTick. Subscribers receive a
    /// disposer Action they MUST invoke when the animation completes or
    /// is cancelled — the ticker holds no weak references.
    ///
    /// Subscribers are responsible for gating style writes on
    /// target.panel != null; AnimationTicker only delivers the tick.
    /// </summary>
    internal static class AnimationTicker
    {
        private static event Action s_tick;
        private static bool s_wired;
#if !UNITY_EDITOR
        private static Action s_runtimeUnsubscribe;
#endif

        public static Action Subscribe(Action onTick)
        {
            if (onTick == null)
            {
                return static () => { };
            }
            EnsureWired();
            s_tick += onTick;
            return () => s_tick -= onTick;
        }

        private static void EnsureWired()
        {
            if (s_wired)
            {
                return;
            }
            s_wired = true;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.update += Pump;
#else
            s_runtimeUnsubscribe = ReactiveUITK.Core.Media.MediaHost.Instance.SubscribeTick(Pump);
#endif
        }

        private static void Pump()
        {
            var handler = s_tick;
            if (handler == null)
            {
                return;
            }
            try
            {
                handler();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }
    }
}
