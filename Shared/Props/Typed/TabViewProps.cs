using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine.UIElements;

namespace ReactiveUITK.Props.Typed
{
    public sealed class TabViewProps
    {
        public int? SelectedIndex { get; set; }
        public int? SelectedTabIndex { get; set; }
        public List<TabDef> Tabs { get; set; }
        public Style Style { get; set; }
        public Delegate SelectedIndexChanged { get; set; }
        public Delegate ActiveTabChanged { get; set; }
        public object Ref { get; set; }

        public sealed class TabDef
        {
            public string Title { get; set; }
            public Func<VirtualNode> Content { get; set; }
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

        public Dictionary<string, object> ToDictionary()
        {
            var d = new Dictionary<string, object>();
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
            if (Style != null)
            {
                d["style"] = Style;
            }
            if (SelectedIndexChanged != null)
            {
                if (SelectedIndexChanged is Hooks.StateSetter<int> setter)
                {
                    var action = setter.ToValueAction();
                    if (action != null)
                    {
                        d["selectedIndexChanged"] = action;
                    }
                }
                else if (SelectedIndexChanged is Action<int> actionInt)
                {
                    d["selectedIndexChanged"] = actionInt;
                }
                else if (SelectedIndexChanged is Delegate generic)
                {
                    d["selectedIndexChanged"] = generic;
                }
            }
            if (ActiveTabChanged != null)
            {
                if (ActiveTabChanged is Action<Tab> single)
                {
                    d["activeTabChanged"] = single;
                }
                else if (ActiveTabChanged is Action<Tab, Tab> dual)
                {
                    d["activeTabChanged"] = dual;
                }
                else if (ActiveTabChanged is Delegate generic)
                {
                    d["activeTabChanged"] = generic;
                }
            }
            if (Ref != null)
            {
                d["ref"] = Ref;
            }
            return d;
        }
    }
}
