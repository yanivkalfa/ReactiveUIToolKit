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

    
    internal interface ISortState
    {
        List<(string name, SortDirection direction, int index)> SortedColumns { get; set; }
        Delegate UserSortNotify { get; set; }
        Action InternalSortHandler { get; set; }
    }

    
    internal interface IColumnLayoutState
    {
        Dictionary<string, float> ColumnWidths { get; set; }
        Dictionary<string, bool> ColumnVisibility { get; set; }
        Dictionary<string, int> ColumnDisplayIndex { get; set; }
    }

    
    internal interface IExpansionState
    {
        HashSet<int> DesiredExpanded { get; set; }
        Dictionary<int, bool> ExpandAllById { get; set; }
        bool OurHandlerAttached { get; set; }
        Delegate UserExpandedHandler { get; set; }
        bool TrackUserExpansion { get; set; }
    }

    
    public interface IExpansionViewOps<TView>
        where TView : VisualElement
    {
        void Subscribe(TView view, Action<TreeViewExpansionChangedArgs> handler);
        void Unsubscribe(TView view, Action<TreeViewExpansionChangedArgs> handler);
        void ExpandItem(TView view, int id, bool expandAllChildren);
        void Refresh(TView view);
    }

    
    internal interface IAdjustmentSuspendState
    {
        bool IsAdjusting { get; set; }
        bool HeaderWired { get; set; }
        IReadOnlyDictionary<string, object> PendingPrev { get; set; }
        IReadOnlyDictionary<string, object> PendingNext { get; set; }
    }

    
    public interface IHeaderOps<TView>
        where TView : VisualElement
    {
        bool IsHeaderElement(VisualElement e);
    }

    
    internal interface IScrollState
    {
        bool IsScrolling { get; set; }
        bool ScrollWired { get; set; }
        IReadOnlyDictionary<string, object> PendingPrev { get; set; }
        IReadOnlyDictionary<string, object> PendingNext { get; set; }
        float ScrollX { get; set; }
        float ScrollY { get; set; }
        int ScrollActivityId { get; set; }
    }

    
    public interface IScrollOps<TView>
        where TView : VisualElement
    {
        ScrollView GetScrollView(TView view);
    }
}
