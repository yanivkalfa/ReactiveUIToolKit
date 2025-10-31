using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public interface IElementStateTracker<TElement, TState>
        where TElement : VisualElement
    {
        void Attach(TElement element, TState state, IReadOnlyDictionary<string, object> props);
        void Detach(TElement element, TState state);
        void Reapply(
            TElement element,
            TState state,
            IReadOnlyDictionary<string, object> previousProps,
            IReadOnlyDictionary<string, object> nextProps
        );
    }

    internal sealed class TreeViewExpansionTracker
        : IElementStateTracker<TreeView, TreeViewElementAdapter.Cached>
    {
        public void Attach(
            TreeView tv,
            TreeViewElementAdapter.Cached state,
            IReadOnlyDictionary<string, object> props
        )
        {
            if (props != null && props.TryGetValue("stopTrackingUserChange", out var stopObj))
                state.TrackUserExpansion = !(stopObj is bool b && b);

            // User handler wiring (pass-through)
            if (props != null && props.TryGetValue("itemExpandedChanged", out var userHandler))
            {
                if (!ReferenceEquals(state.UserExpandedHandler, userHandler))
                {
                    if (state.UserExpandedHandler is Action<TreeViewExpansionChangedArgs> prev)
                    {
                        try
                        {
                            tv.itemExpandedChanged -= prev;
                        }
                        catch { }
                    }
                    state.UserExpandedHandler = userHandler as Delegate;
                    if (state.UserExpandedHandler is Action<TreeViewExpansionChangedArgs> nextH)
                    {
                        try
                        {
                            tv.itemExpandedChanged += nextH;
                        }
                        catch { }
                    }
                }
            }

            // Internal tracker (only if enabled and no user handler)
            bool shouldAttach = state.TrackUserExpansion && state.UserExpandedHandler == null;
            if (shouldAttach && !state.OurHandlerAttached)
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
                    tv.itemExpandedChanged += h;
                    state.OurHandlerAttached = true;
                }
                catch { }
            }
        }

        public void Detach(TreeView tv, TreeViewElementAdapter.Cached state)
        {
            // No-op for now (safe to leave internal handler attached across adapter lifetime)
        }

        public void Reapply(
            TreeView tv,
            TreeViewElementAdapter.Cached state,
            IReadOnlyDictionary<string, object> previousProps,
            IReadOnlyDictionary<string, object> nextProps
        )
        {
            // Override via prop if provided
            if (nextProps != null && nextProps.TryGetValue("expandedItemIds", out var expObj))
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

            foreach (var id in state.DesiredExpanded)
            {
                bool all = state.ExpandAllById.TryGetValue(id, out var v) && v;
                try
                {
                    tv.ExpandItem(id, all, false);
                }
                catch { }
            }
            try
            {
                tv.RefreshItems();
            }
            catch { }
        }

        // Coercion now centralized in BaseElementAdapter.CoerceIds
    }
}
