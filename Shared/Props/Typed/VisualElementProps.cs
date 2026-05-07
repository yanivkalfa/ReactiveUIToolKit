using System.Collections.Generic;

namespace ReactiveUITK.Props.Typed
{
    /// <summary>
    /// Props for a plain VisualElement. All shared element properties are inherited from BaseProps.
    /// </summary>
    public sealed class VisualElementProps : BaseProps
    {
        public override bool ShallowEquals(BaseProps other)
        {
            return base.ShallowEquals(other) && other is VisualElementProps;
        }

        public override Dictionary<string, object> ToDictionary()
        {
            return base.ToDictionary();
        }

        internal override void __ResetFields() { }

        internal override void __ReturnToPool()
        {
            Pool<VisualElementProps>.Return(this);
        }
    }
}
