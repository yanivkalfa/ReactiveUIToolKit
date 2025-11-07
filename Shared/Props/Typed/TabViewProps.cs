using System;
using System.Collections.Generic;
using ReactiveUITK.Core;

namespace ReactiveUITK.Props.Typed
{
    public sealed class TabViewProps
    {
        public int? SelectedIndex { get; set; }
        public List<TabDef> Tabs { get; set; }
        public Style Style { get; set; }
    public object Ref { get; set; }

        public sealed class TabDef
        {
            public string Title { get; set; }
            public Func<VirtualNode> Content { get; set; }
            public VirtualNode StaticContent { get; set; }

            public Dictionary<string, object> ToDictionary()
            {
                var d = new Dictionary<string, object>();
                if (!string.IsNullOrEmpty(Title)) d["title"] = Title;
                if (Content != null) d["content"] = Content;
                if (StaticContent != null) d["staticContent"] = StaticContent;
                return d;
            }
        }

        public Dictionary<string, object> ToDictionary()
        {
            var d = new Dictionary<string, object>();
            if (SelectedIndex.HasValue) d["selectedIndex"] = SelectedIndex.Value;
            if (Tabs != null)
            {
                var list = new List<Dictionary<string, object>>(Tabs.Count);
                foreach (var t in Tabs) list.Add(t?.ToDictionary());
                d["tabs"] = list;
            }
            if (Style != null) d["style"] = Style;
            if (Ref != null) d["ref"] = Ref;
            return d;
        }
    }
}

