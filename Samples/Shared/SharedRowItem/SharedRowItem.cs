using System;
using System.Collections;
using System.Collections.Generic;

namespace ReactiveUITK.Samples.Shared
{
    public class SharedRowItem
    {
        public string Id;
        public string Text;

        public override string ToString() => Text;
    }

    [Serializable]
    public class SharedTreeRowItem : SharedRowItem
    {
        public bool IsChild { get; set; } = false;
        public bool ShouldOverrideElement { get; set; } = false;
    }
}
