using System;
using System.Collections.Generic;
using System.Reflection;
using ReactiveUITK.Core;
using ReactiveUITK.Props;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class TabViewElementAdapter : BaseElementAdapter
    {
        private static void SetTabTitle(Tab tab, string title)
        {
            if (tab == null)
                return;
            if (string.IsNullOrEmpty(title))
                title = string.Empty;
            try
            {
                var p = typeof(Tab).GetProperty(
                    "title",
                    BindingFlags.Instance | BindingFlags.Public
                );
                if (p != null && p.CanWrite)
                {
                    p.SetValue(tab, title);
                    return;
                }
            }
            catch { }
            try
            {
                var p = typeof(Tab).GetProperty(
                    "text",
                    BindingFlags.Instance | BindingFlags.Public
                );
                if (p != null && p.CanWrite)
                {
                    p.SetValue(tab, title);
                    return;
                }
            }
            catch { }
            try
            {
                var label = tab.Q<Label>("title") ?? tab.Q<Label>();
                if (label != null)
                {
                    label.text = title;
                    return;
                }
            }
            catch { }
            try
            {
                tab.name = string.IsNullOrEmpty(tab.name) ? ("Tab_" + title) : tab.name;
            }
            catch { }
        }

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

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (element is not TabView tv)
            {
                PropsApplier.Apply(element, properties);
                return;
            }

            if (
                properties != null
                && properties.TryGetValue("tabs", out var tabsObj)
                && tabsObj is IEnumerable<Dictionary<string, object>> tabs
            )
            {
                RebuildTabs(tv, tabs);
            }

            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is not TabView tv)
            {
                PropsApplier.ApplyDiff(element, previous, next);
                return;
            }
            previous ??= new Dictionary<string, object>();
            next ??= new Dictionary<string, object>();

            if (
                next.TryGetValue("tabs", out var nextTabs)
                && nextTabs is IEnumerable<Dictionary<string, object>> tabs
            )
            {
                RebuildTabs(tv, tabs);
            }
            else if (previous.ContainsKey("tabs"))
            {
                tv.Clear();
            }

            PropsApplier.ApplyDiff(element, previous, next);
        }

        // Deprecated: replaced by AdapterUtil.EnsureVisualElementRoot

        private static void RebuildTabs(
            TabView tv,
            IEnumerable<Dictionary<string, object>> tabs
        )
        {
            if (tv == null)
                return;

            tv.Clear();

            if (tabs == null)
                return;

            foreach (var tabDefinition in tabs)
            {
                var tab = new Tab();
                string title = null;
                Func<VirtualNode> fn = null;
                VirtualNode node = null;

                if (tabDefinition != null)
                {
                    tabDefinition.TryGetValue("title", out var titleObj);
                    tabDefinition.TryGetValue("content", out var contentObj);
                    tabDefinition.TryGetValue("staticContent", out var staticObj);
                    title = titleObj as string;
                    fn = contentObj as Func<VirtualNode>;
                    node = staticObj as VirtualNode;
                }

                SetTabTitle(tab, title ?? string.Empty);

                var content = new VisualElement();
                var rr = new VNodeHostRenderer(GetHost(), content);
                try
                {
                    content.userData = rr;
                }
                catch { }
                var vnode = EnsureVisualElementRoot(fn != null ? fn() : node, "TabView");
                if (vnode != null)
                    rr.Render(vnode);
                try
                {
                    tab.contentContainer.Add(content);
                }
                catch
                {
                    tab.Add(content);
                }
                tv.Add(tab);
            }
        }
    }
}
