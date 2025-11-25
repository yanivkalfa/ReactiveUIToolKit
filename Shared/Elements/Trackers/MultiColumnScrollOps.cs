using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    internal sealed class MultiColumnScrollOps<TView> : IScrollOps<TView>
        where TView : VisualElement
    {
        public ScrollView GetScrollView(TView view)
        {
            if (view == null)
            {
                return null;
            }
            try
            {
                return view.Q<ScrollView>();
            }
            catch { }
            return null;
        }
    }
}
