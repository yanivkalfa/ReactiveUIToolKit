using System.Collections.Generic;

namespace ReactiveUITK.Props.Typed
{
    /// <summary>
    /// Props for a plain VisualElement. All shared element properties are inherited from BaseProps.
    /// </summary>
    public sealed class VisualElementProps : BaseProps
    {
        public override Dictionary<string, object> ToDictionary()
        {
            return base.ToDictionary();
        }
    }
}
