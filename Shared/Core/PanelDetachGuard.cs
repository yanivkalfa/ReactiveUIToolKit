using System;
using UnityEngine.UIElements;

namespace ReactiveUITK.Core
{
    /// <summary>
    /// Wires reusable Attach/Detach callbacks on a VisualElement so that
    /// teardown work runs only when a detach is genuine (the element has
    /// been removed and stays removed across at least one frame), not when
    /// Unity's UI panel rebuilds transiently. Transient detaches occur on
    /// undo, UIDocument asset swaps, disable/enable cycles, and the
    /// editor-selection panel-rebuild storm in playmode.
    ///
    /// On DetachFromPanelEvent the teardown is scheduled via
    /// MainThreadTimer.OneFrameLater. On AttachToPanelEvent any pending
    /// teardown is cancelled. If the element is reparented to a new panel
    /// inside the same frame, teardown never runs and stateful work
    /// (event handler subscriptions, rented pooled resources, animations)
    /// is preserved.
    /// </summary>
    internal static class PanelDetachGuard
    {
        /// <summary>
        /// Registers attach/detach callbacks on <paramref name="ve"/>. The
        /// supplied <paramref name="teardown"/> is invoked at most once per
        /// detach, on the frame after the detach, and only if the element
        /// is still detached at that time. Guard state lives in closure
        /// captures and is GC'd with the element.
        /// </summary>
        public static void Wire(VisualElement ve, Action teardown)
        {
            if (ve == null || teardown == null)
            {
                return;
            }
            Action cancel = null;
            ve.RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                cancel?.Invoke();
                cancel = MainThreadTimer.OneFrameLater(() =>
                {
                    cancel = null;
                    if (ve.panel != null)
                    {
                        return;
                    }
                    teardown();
                });
            });
            ve.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                cancel?.Invoke();
                cancel = null;
            });
        }
    }
}
