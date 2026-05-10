using System;
using UnityEngine;

namespace ReactiveUITK.Core
{
    /// <summary>
    /// Schedules a one-shot main-thread callback to run on the next frame
    /// regardless of whether any VisualElement is currently attached to a
    /// panel. Used to defer panel-detach teardown so that transient
    /// reparenting (Unity rebuilding rootVisualElement on undo, asset
    /// swap, disable/enable, or playmode editor-selection storms) does not
    /// destroy retained state.
    ///
    /// In editor mode (any play state) the timer drives off
    /// EditorApplication.update with self-removal. In built players it
    /// piggy-backs on MediaHost.SubscribeTick so no extra hidden GameObject
    /// is created.
    ///
    /// The returned Action cancels the pending callback if invoked before
    /// the timer fires; calling it after the callback has already run is a
    /// no-op. Cancelling is the mechanism used by AttachToPanelEvent to
    /// abort a previously-scheduled teardown.
    /// </summary>
    internal static class MainThreadTimer
    {
        public static Action OneFrameLater(Action callback)
        {
            if (callback == null)
            {
                return static () => { };
            }

#if UNITY_EDITOR
            UnityEditor.EditorApplication.CallbackFunction wrapper = null;
            bool fired = false;
            wrapper = () =>
            {
                if (fired)
                {
                    return;
                }
                fired = true;
                UnityEditor.EditorApplication.update -= wrapper;
                try
                {
                    callback();
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }
            };
            UnityEditor.EditorApplication.update += wrapper;
            return () =>
            {
                if (fired)
                {
                    return;
                }
                fired = true;
                UnityEditor.EditorApplication.update -= wrapper;
            };
#else
            Action unsub = null;
            bool firedRt = false;
            unsub = ReactiveUITK.Core.Media.MediaHost.Instance.SubscribeTick(() =>
            {
                if (firedRt)
                {
                    return;
                }
                firedRt = true;
                unsub?.Invoke();
                try
                {
                    callback();
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }
            });
            return () =>
            {
                if (firedRt)
                {
                    return;
                }
                firedRt = true;
                unsub?.Invoke();
            };
#endif
        }
    }
}
