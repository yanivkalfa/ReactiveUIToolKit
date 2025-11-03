using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    internal sealed class MultiColumnHeaderOps<TView> : IHeaderOps<TView>
        where TView : VisualElement
    {
        public bool IsHeaderElement(VisualElement e)
        {
            var ve = e;
            while (ve != null)
            {
                try
                {
                    if (ve.ClassListContains("unity-multi-column-header"))
                        return true;
                }
                catch { }
                ve = ve.parent as VisualElement;
            }
            return false;
        }
    }
}
