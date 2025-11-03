using System;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    internal sealed class TreeViewExpansionHooks : IExpansionHooks<TreeView>
    {
        public static readonly TreeViewExpansionHooks Instance = new TreeViewExpansionHooks();

        public void Subscribe(TreeView view, Action<TreeViewExpansionChangedArgs> handler)
        {
            if (view == null || handler == null)
                return;
            try
            {
                view.itemExpandedChanged += handler;
            }
            catch { }
        }

        public void Unsubscribe(TreeView view, Action<TreeViewExpansionChangedArgs> handler)
        {
            if (view == null || handler == null)
                return;
            try
            {
                view.itemExpandedChanged -= handler;
            }
            catch { }
        }

        public void ExpandItem(TreeView view, int id, bool expandAllChildren)
        {
            if (view == null)
                return;
            try
            {
                view.ExpandItem(id, expandAllChildren);
            }
            catch { }
        }

        public void Refresh(TreeView view)
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
