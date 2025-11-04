using System;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    internal sealed class MultiColumnTreeViewExpansionOps : IExpansionViewOps<MultiColumnTreeView>
    {
        public static readonly MultiColumnTreeViewExpansionOps Instance = new();

        public void Subscribe(
            MultiColumnTreeView view,
            Action<TreeViewExpansionChangedArgs> handler
        )
        {
            if (view == null || handler == null)
                return;
            try
            {
                view.itemExpandedChanged += handler;
            }
            catch { }
        }

        public void Unsubscribe(
            MultiColumnTreeView view,
            Action<TreeViewExpansionChangedArgs> handler
        )
        {
            if (view == null || handler == null)
                return;
            try
            {
                view.itemExpandedChanged -= handler;
            }
            catch { }
        }

        public void ExpandItem(MultiColumnTreeView view, int id, bool expandAllChildren)
        {
            if (view == null)
                return;
            try
            {
                view.ExpandItem(id, expandAllChildren, false);
            }
            catch { }
        }

        public void Refresh(MultiColumnTreeView view)
        {
            if (view == null)
                return;
            try
            {
                view.RefreshItems();
            }
            catch { }
        }
    }
}
