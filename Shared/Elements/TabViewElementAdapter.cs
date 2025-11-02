using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using ReactiveUITK.Core;
using ReactiveUITK.Props;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class TabViewElementAdapter : BaseElementAdapter
    {
        private static void SetTabTitle(Tab tab, string title)
        {
            if (tab == null) return;
            if (string.IsNullOrEmpty(title)) title = string.Empty;
            try
            {
                var p = typeof(Tab).GetProperty("title", BindingFlags.Instance | BindingFlags.Public);
                if (p != null && p.CanWrite) { p.SetValue(tab, title); return; }
            }
            catch { }
            try
            {
                var p = typeof(Tab).GetProperty("text", BindingFlags.Instance | BindingFlags.Public);
                if (p != null && p.CanWrite) { p.SetValue(tab, title); return; }
            }
            catch { }
            try
            {
                var label = tab.Q<Label>("title") ?? tab.Q<Label>();
                if (label != null) { label.text = title; return; }
            }
            catch { }
            try { tab.name = string.IsNullOrEmpty(tab.name) ? ("Tab_" + title) : tab.name; } catch { }
        }

        private sealed class Cached
        {
            public List<string> Titles;
            public List<Func<VirtualNode>> ContentFns;
            public List<VirtualNode> StaticNodes;
        }

        private static readonly ConditionalWeakTable<TabView, Cached> cache = new();

        private static HostContext sharedHost;
        private static HostContext GetHost()
        {
            if (sharedHost == null)
            {
                sharedHost = new HostContext(ElementRegistryProvider.GetDefaultRegistry());
            }
            return sharedHost;
        }

        public override VisualElement Create()
        {
            return new TabView();
        }

        public override void ApplyProperties(VisualElement element, IReadOnlyDictionary<string, object> properties)
        {
            if (element is not TabView tv)
            {
                PropsApplier.Apply(element, properties);
                return;
            }

            var parts = cache.GetValue(tv, _ => new Cached());

            if (properties != null)
            {
                if (properties.TryGetValue("tabs", out var tabsObj) && tabsObj is IEnumerable<Dictionary<string, object>> tabs)
                {
                    var titles = new List<string>();
                    var fns = new List<Func<VirtualNode>>();
                    var nodes = new List<VirtualNode>();
                    foreach (var t in tabs)
                    {
                        t.TryGetValue("title", out var titleObj);
                        t.TryGetValue("content", out var contentObj);
                        t.TryGetValue("staticContent", out var staticObj);
                        titles.Add(titleObj as string);
                        fns.Add(contentObj as Func<VirtualNode>);
                        nodes.Add(staticObj as VirtualNode);
                    }
                    bool same = parts.Titles != null && parts.Titles.Count == titles.Count;
                    if (same)
                    {
                        for (int i = 0; i < titles.Count; i++)
                        {
                            if (!string.Equals(parts.Titles[i], titles[i])) { same = false; break; }
                        }
                    }
                    parts.Titles = titles;
                    parts.ContentFns = fns;
                    parts.StaticNodes = nodes;
                    if (!same) RebuildTabs(tv, parts); else RebindAll(tv, parts);
                }
            }

            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(VisualElement element, IReadOnlyDictionary<string, object> previous, IReadOnlyDictionary<string, object> next)
        {
            if (element is not TabView tv)
            {
                PropsApplier.ApplyDiff(element, previous, next);
                return;
            }
            previous ??= new Dictionary<string, object>();
            next ??= new Dictionary<string, object>();
            var parts = cache.GetValue(tv, _ => new Cached());

            previous.TryGetValue("tabs", out var prevTabs);
            next.TryGetValue("tabs", out var nextTabs);
            if (!ReferenceEquals(prevTabs, nextTabs) && nextTabs is IEnumerable<Dictionary<string, object>> tabs)
            {
                var titles = new List<string>();
                var fns = new List<Func<VirtualNode>>();
                var nodes = new List<VirtualNode>();
                foreach (var t in tabs)
                {
                    t.TryGetValue("title", out var titleObj);
                    t.TryGetValue("content", out var contentObj);
                    t.TryGetValue("staticContent", out var staticObj);
                    titles.Add(titleObj as string);
                    fns.Add(contentObj as Func<VirtualNode>);
                    nodes.Add(staticObj as VirtualNode);
                }
                bool same = parts.Titles != null && parts.Titles.Count == titles.Count;
                if (same)
                {
                    for (int i = 0; i < titles.Count; i++)
                    {
                        if (!string.Equals(parts.Titles[i], titles[i])) { same = false; break; }
                    }
                }
                parts.Titles = titles;
                parts.ContentFns = fns;
                parts.StaticNodes = nodes;
                if (!same) RebuildTabs(tv, parts); else RebindAll(tv, parts);
            }

            PropsApplier.ApplyDiff(element, previous, next);
        }

        // Deprecated: replaced by AdapterUtil.EnsureVisualElementRoot

        private static void RebuildTabs(TabView tv, Cached parts)
        {
            tv.Clear();
            if (parts.Titles == null) return;
            for (int i = 0; i < parts.Titles.Count; i++)
            {
                var tab = new Tab();
                SetTabTitle(tab, parts.Titles[i] ?? string.Empty);
                var content = new VisualElement();
                var rr = new VNodeHostRenderer(GetHost(), content);
                try { content.userData = rr; } catch { }
                var fn = parts.ContentFns != null && i < parts.ContentFns.Count ? parts.ContentFns[i] : null;
                var node = parts.StaticNodes != null && i < parts.StaticNodes.Count ? parts.StaticNodes[i] : null;
                var vnode = EnsureVisualElementRoot(fn != null ? fn() : node, "TabView");
                if (vnode != null) rr.Render(vnode);
                try { tab.contentContainer.Add(content); } catch { tab.Add(content); }
                tv.Add(tab);
            }
        }

        private static void RebindAll(TabView tv, Cached parts)
        {
            if (parts?.Titles == null) return;
            int count = Math.Min(parts.Titles.Count, tv.childCount);
            for (int i = 0; i < count; i++)
            {
                var tab = tv[i] as Tab;
                if (tab == null) continue;
                VisualElement content = null;
                try { if (tab.contentContainer != null && tab.contentContainer.childCount > 0) content = tab.contentContainer.ElementAt(0) as VisualElement; } catch { }
                if (content == null)
                {
                    content = new VisualElement();
                    try { tab.contentContainer.Add(content); } catch { tab.Add(content); }
                }
                var rr = content.userData as IVNodeHostRenderer;
                if (rr == null)
                {
                    rr = new VNodeHostRenderer(GetHost(), content);
                    try { content.userData = rr; } catch { }
                }
                var fn = parts.ContentFns != null && i < parts.ContentFns.Count ? parts.ContentFns[i] : null;
                var node = parts.StaticNodes != null && i < parts.StaticNodes.Count ? parts.StaticNodes[i] : null;
                var vnode = EnsureVisualElementRoot(fn != null ? fn() : node, "TabView");
                if (vnode != null) rr.Render(vnode);
            }
        }
    }
}
