using System;

namespace ReactiveUITK.Samples.UITKXShared
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
        public bool IsChild { get; set; }
        public bool ShouldOverrideElement { get; set; }
    }

    public sealed class ListViewRowState
    {
        public string Id;
        public string Text;
        public bool ShouldOverrideElement;
    }

    public sealed class MultiColumnListViewRowState
    {
        public string Id;
        public string Text;
        public bool ShouldOverrideElement;
    }

    public sealed class TreeViewRowState
    {
        public SharedTreeRowItem Parent;
        public SharedTreeRowItem Child;
        public bool HasChild;
        public int Pid;
    }

    public sealed class MultiColumnTreeViewRowState
    {
        public SharedTreeRowItem Parent;
        public SharedTreeRowItem Child;
        public bool HasChild;
        public int Pid;
    }
}
