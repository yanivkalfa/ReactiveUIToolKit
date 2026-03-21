using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class TabViewProps : BaseProps
    {
        public int? SelectedIndex { get; set; }
        public int? SelectedTabIndex { get; set; }
        public List<TabDef> Tabs { get; set; }
        public TabIndexEventHandler SelectedIndexChanged { get; set; }
        public TabChangedEventHandler ActiveTabChanged { get; set; }

        public sealed class TabDef : global::ReactiveUITK.Core.IProps
        {
            public string Title { get; set; }
            public ContentRenderer Content { get; set; }
            public VirtualNode StaticContent { get; set; }

            public Dictionary<string, object> ToDictionary()
            {
                var d = new Dictionary<string, object>();
                if (!string.IsNullOrEmpty(Title))
                {
                    d["title"] = Title;
                }
                if (Content != null)
                {
                    d["content"] = Content;
                }
                if (StaticContent != null)
                {
                    d["staticContent"] = StaticContent;
                }
                return d;
            }
        }

        public override Dictionary<string, object> ToDictionary()
        {
            var d = base.ToDictionary();
            int? selected = SelectedTabIndex ?? SelectedIndex;
            if (selected.HasValue)
            {
                d["selectedTabIndex"] = selected.Value;
            }
            if (Tabs != null)
            {
                var list = new List<Dictionary<string, object>>(Tabs.Count);
                foreach (var t in Tabs)
                {
                    list.Add(t?.ToDictionary());
                }
                d["tabs"] = list;
            }
            if (SelectedIndexChanged != null)
            {
                d["selectedIndexChanged"] = SelectedIndexChanged;
            }
            if (ActiveTabChanged != null)
            {
                d["activeTabChanged"] = ActiveTabChanged;
            }
            return d;
        }
    }
}
