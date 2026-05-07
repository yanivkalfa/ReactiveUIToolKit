using System.Collections.Generic;

namespace ReactiveUITK.Props.Typed
{
    public sealed class ProgressBarProps : BaseProps
    {
        public float? Value { get; set; }
        public string Title { get; set; }
        public Dictionary<string, object> Progress { get; set; }
        public Dictionary<string, object> TitleElement { get; set; }

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not ProgressBarProps o)
                return false;
            if (Value != o.Value)
                return false;
            if (Title != o.Title)
                return false;
            if (!ReferenceEquals(Progress, o.Progress))
                return false;
            if (!ReferenceEquals(TitleElement, o.TitleElement))
                return false;
            return true;
        }

        public override Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> map = base.ToDictionary();
            if (Value.HasValue)
            {
                map["value"] = Value.Value;
            }
            if (!string.IsNullOrEmpty(Title))
            {
                map["title"] = Title;
            }
            if (Progress != null)
            {
                map["progress"] = Progress;
            }
            if (TitleElement != null)
            {
                map["titleElement"] = TitleElement;
            }
            return map;
        }

        internal override void __ResetFields()
        {
            Value = null;
            Title = null;
            Progress = null;
            TitleElement = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<ProgressBarProps>.Return(this);
        }
    }
}
