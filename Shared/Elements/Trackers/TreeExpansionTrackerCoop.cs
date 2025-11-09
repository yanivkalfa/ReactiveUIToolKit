using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    // Cooperative generic tracker: adapters call Attach/Reapply at exact lifecycle points
    internal sealed class ExpansionStateTracker<TView, TState>
        where TView : VisualElement
        where TState : IExpansionState
    {
        public void Attach(
            TView view,
            TState state,
            IReadOnlyDictionary<string, object> props,
            IExpansionViewOps<TView> hooks
        )
        {
            if (view == null || state == null || hooks == null)
                return;

            // stopTrackingUserChange
            state.TrackUserExpansion = true;
            if (props != null && props.TryGetValue("stopTrackingUserChange", out var stopObj))
                state.TrackUserExpansion = !(stopObj is bool b && b);

            // User-provided handler
            if (props != null && props.TryGetValue("itemExpandedChanged", out var userHandler))
            {
                if (!ReferenceEquals(state.UserExpandedHandler, userHandler))
                {
                    if (state.UserExpandedHandler is Action<TreeViewExpansionChangedArgs> prev)
                    {
                        try
                        {
                            hooks.Unsubscribe(view, prev);
                        }
                        catch { }
                    }
                    state.UserExpandedHandler = userHandler as Delegate;
                    if (state.UserExpandedHandler is Action<TreeViewExpansionChangedArgs> nextH)
                    {
                        try
                        {
                            hooks.Subscribe(view, nextH);
                        }
                        catch { }
                    }
                }
            }

            // Our internal tracker only when no user handler
            bool shouldAttach = state.TrackUserExpansion && !state.OurHandlerAttached;
            if (shouldAttach)
            {
                Action<TreeViewExpansionChangedArgs> h = e =>
                {
                    try
                    {
                        if (e.isExpanded)
                            state.DesiredExpanded.Add(e.id);
                        else
                            state.DesiredExpanded.Remove(e.id);
                        state.ExpandAllById[e.id] = e.isAppliedToAllChildren;
                    }
                    catch { }
                };
                try
                {
                    hooks.Subscribe(view, h);
                    state.OurHandlerAttached = true;
                }
                catch { }
            }
        }

        public void Reapply(
            TView view,
            TState state,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next,
            IExpansionViewOps<TView> hooks
        )
        {
            if (view == null || state == null || hooks == null)
                return;

            // expandedItemIds override
            if (next != null && next.TryGetValue("expandedItemIds", out var expObj))
            {
                try
                {
                    var ids = BaseElementAdapter.CoerceIds(expObj);
                    state.DesiredExpanded.Clear();
                    state.ExpandAllById.Clear();
                    if (ids != null)
                    {
                        foreach (var id in ids)
                            state.DesiredExpanded.Add(id);
                    }
                }
                catch { }
            }

            // Apply desired expansion
            try
            {
                foreach (var id in state.DesiredExpanded)
                {
                    bool all = state.ExpandAllById.TryGetValue(id, out var v) && v;
                    try
                    {
                        hooks.ExpandItem(view, id, all);
                    }
                    catch { }
                }
                hooks.Refresh(view);
            }
            catch { }
        }
    }
}
