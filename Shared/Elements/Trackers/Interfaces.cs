using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    // Generic element state tracker contract used by adapters to persist and reapply per-element state
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

    // Common sort state carried by adapters' Cached
    internal interface ISortState
    {
        List<(string name, SortDirection direction, int index)> SortedColumns { get; set; }
        Delegate UserSortNotify { get; set; }
        Action InternalSortHandler { get; set; }
    }

    // Common column layout state carried by adapters' Cached
    internal interface IColumnLayoutState
    {
        Dictionary<string, float> ColumnWidths { get; set; }
        Dictionary<string, bool> ColumnVisibility { get; set; }
        Dictionary<string, int> ColumnDisplayIndex { get; set; }
    }

    // Common expansion state for tree-like views
    internal interface IExpansionState
    {
        HashSet<int> DesiredExpanded { get; set; }
        Dictionary<int, bool> ExpandAllById { get; set; }
        bool OurHandlerAttached { get; set; }
        Delegate UserExpandedHandler { get; set; }
        bool TrackUserExpansion { get; set; }
    }

    // Adapter-provided operations so a generic expansion tracker can operate without reflection
    public interface IExpansionViewOps<TView>
        where TView : VisualElement
    {
        void Subscribe(TView view, Action<TreeViewExpansionChangedArgs> handler);
        void Unsubscribe(TView view, Action<TreeViewExpansionChangedArgs> handler);
        void ExpandItem(TView view, int id, bool expandAllChildren);
        void Refresh(TView view);
    }

    // Suspension state used to pause heavy updates during user adjustments (e.g., header drag)
    internal interface IAdjustmentSuspendState
    {
        bool IsAdjusting { get; set; }
        bool HeaderWired { get; set; }
        IReadOnlyDictionary<string, object> PendingPrev { get; set; }
        IReadOnlyDictionary<string, object> PendingNext { get; set; }
    }

    // Ops to detect header involvement without binding to concrete header types
    public interface IHeaderOps<TView>
        where TView : VisualElement
    {
        bool IsHeaderElement(VisualElement e);
    }
}
