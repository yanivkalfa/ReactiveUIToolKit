using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    internal sealed class MultiColumnAdjustmentTracker<TView, TState>
        : IElementStateTracker<TView, TState>
        where TView : VisualElement
        where TState : IAdjustmentSuspendState
    {
        private readonly IHeaderOps<TView> _headerOps;
        private readonly Action<
            TView,
            TState,
            IReadOnlyDictionary<string, object>,
            IReadOnlyDictionary<string, object>
        > _flush;

        public MultiColumnAdjustmentTracker(
            IHeaderOps<TView> headerOps,
            Action<
                TView,
                TState,
                IReadOnlyDictionary<string, object>,
                IReadOnlyDictionary<string, object>
            > flush
        )
        {
            _headerOps = headerOps;
            _flush = flush;
        }

        public void Attach(TView view, TState state, IReadOnlyDictionary<string, object> props)
        {
            if (view == null || state == null)
                return;
            if (state.HeaderWired)
                return;
            state.HeaderWired = true;

            view.RegisterCallback<PointerDownEvent>(
                e =>
                {
                    var ve = e?.target as VisualElement;
                    if (ve != null && _headerOps.IsHeaderElement(ve))
                    {
                        state.IsAdjusting = true;
                    }
                },
                TrickleDown.TrickleDown
            );

            void End()
            {
                if (!state.IsAdjusting)
                    return;
                state.IsAdjusting = false;
                var prev = state.PendingPrev;
                var next = state.PendingNext;
                state.PendingPrev = null;
                state.PendingNext = null;
                if (_flush == null)
                    return;
                try
                {
                    _flush(view, state, prev, next);
                }
                catch { }
            }

            view.RegisterCallback<PointerUpEvent>(_ => End(), TrickleDown.TrickleDown);
            view.RegisterCallback<PointerCancelEvent>(_ => End(), TrickleDown.TrickleDown);
            view.RegisterCallback<MouseCaptureOutEvent>(_ => End());
        }

        public void Detach(TView view, TState state)
        {
            if (state == null)
                return;
            // Clear flags and buffers so subsequent attaches start clean
            try { state.IsAdjusting = false; } catch { }
            try { state.HeaderWired = false; } catch { }
            try { state.PendingPrev = null; } catch { }
            try { state.PendingNext = null; } catch { }
            // Note: we do not unregister callbacks here because we registered inline delegates.
            // Adapters typically live for the lifetime of the control; if you need explicit
            // unregistration, store the delegates in state and remove them here.
        }

        public void Reapply(
            TView view,
            TState state,
            IReadOnlyDictionary<string, object> previousProps,
            IReadOnlyDictionary<string, object> nextProps
        )
        {
            if (view == null || state == null)
                return;
            if (state.IsAdjusting)
            {
                state.PendingPrev = previousProps;
                state.PendingNext = nextProps;
            }
        }
    }
}
