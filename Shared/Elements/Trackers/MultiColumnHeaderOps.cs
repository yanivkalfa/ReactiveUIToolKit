using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    internal sealed class MultiColumnHeaderOps<TView> : IHeaderOps<TView>
        where TView : VisualElement
    {
        private const string HeaderClass = "unity-multi-column-header";

        public bool IsHeaderElement(VisualElement e)
        {
            var ve = e;
            while (ve != null)
            {
                try
                {
                    if (ve.ClassListContains(HeaderClass))
                        return true;
                }
                catch { }
                ve = ve.parent as VisualElement;
            }
            return false;
        }
    }
}
