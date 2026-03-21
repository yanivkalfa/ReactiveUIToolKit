using System;
using System.Collections.Generic;
using System.Reflection;
using ReactiveUITK.Core;
using ReactiveUITK.Elements.Trackers;
using ReactiveUITK.Props;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class TabViewElementAdapter
        : StatefulElementAdapter<TabView, TabViewElementAdapter.Cached>
    {
        public sealed class Cached
        {
            public TabViewSelectionState SelectionState { get; } = new();
            public TabViewSelectionTracker SelectionTracker { get; } = new();
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
            if (element is not TabView tabView)
            {
                PropsApplier.Apply(element, properties);
                return;
            }

            var cached = GetState(tabView);
            var tracker = cached.SelectionTracker;
            var selectionState = cached.SelectionState;

            tracker.Attach(tabView, selectionState, properties);
            ApplyTabs(tabView, tracker, selectionState, previousProps: null, nextProps: properties);

            bool shouldSuppress = tracker.ShouldSuppressForProps(properties);
            if (shouldSuppress)
            {
                tracker.BeginSuppression(selectionState);
            }

            try
            {
                PropsApplier.Apply(element, properties);
            }
            finally
            {
                if (shouldSuppress)
                {
                    tracker.EndSuppression(selectionState);
                    tracker.SyncFromView(tabView, selectionState);
                }
            }
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            previous ??= s_emptyProps;
            next ??= s_emptyProps;

            if (element is not TabView tabView)
            {
                PropsApplier.ApplyDiff(element, previous, next);
                return;
            }

            var cached = GetState(tabView);
            var tracker = cached.SelectionTracker;
            var selectionState = cached.SelectionState;

            tracker.Attach(tabView, selectionState, next);
            ApplyTabs(tabView, tracker, selectionState, previous, next);

            bool shouldSuppress = tracker.ShouldSuppressForProps(next);
            if (shouldSuppress)
            {
                tracker.BeginSuppression(selectionState);
            }

            try
            {
                PropsApplier.ApplyDiff(element, previous, next);
            }
            finally
            {
                if (shouldSuppress)
                {
                    tracker.EndSuppression(selectionState);
                    tracker.SyncFromView(tabView, selectionState);
                }
            }
        }

        private static void ApplyTabs(
            TabView tabView,
            TabViewSelectionTracker tracker,
            TabViewSelectionState selectionState,
            IReadOnlyDictionary<string, object> previousProps,
            IReadOnlyDictionary<string, object> nextProps
        )
        {
            bool tabsChanged = TabViewSelectionTracker.TabsChanged(previousProps, nextProps);
            if (!tabsChanged)
            {
                tracker.Reapply(tabView, selectionState, previousProps, nextProps);
                tracker.SyncFromView(tabView, selectionState);
                return;
            }

            tracker.BeginSuppression(selectionState);
            try
            {
                RebuildTabs(tabView, nextProps);
                tracker.Reapply(tabView, selectionState, previousProps, nextProps);
                tracker.SyncFromView(tabView, selectionState);
            }
            finally
            {
                tracker.EndSuppression(selectionState);
            }
        }

        private static void RebuildTabs(TabView tabView, IReadOnlyDictionary<string, object> props)
        {
            if (tabView == null)
            {
                return;
            }

            tabView.Clear();
            if (props == null || !props.TryGetValue("tabs", out var tabsObj))
            {
                return;
            }

            if (tabsObj is not IEnumerable<Dictionary<string, object>> tabs)
            {
                return;
            }

            foreach (var tabDefinition in tabs)
            {
                var tab = new Tab();
                string title = null;
                ContentRenderer dynamicContent = null;
                VirtualNode staticContent = null;

                object titleObj = null;
                object contentObj = null;
                object staticObj = null;

                tabDefinition?.TryGetValue("title", out titleObj);
                tabDefinition?.TryGetValue("content", out contentObj);
                tabDefinition?.TryGetValue("staticContent", out staticObj);

                title = titleObj as string;
                dynamicContent = contentObj as ContentRenderer;
                staticContent = staticObj as VirtualNode;

                SetTabTitle(tab, title ?? string.Empty);
                tabView.Add(tab);

                var contentRoot = new VisualElement();
                var renderer = new VNodeHostRenderer(GetHost(), contentRoot);
                try
                {
                    contentRoot.userData = renderer;
                }
                catch { }

                var vnode = dynamicContent != null ? dynamicContent() : staticContent;
                vnode = EnsureVisualElementRoot(vnode, "TabView");
                if (vnode != null)
                {
                    renderer.Render(vnode);
                }

                try
                {
                    tab.contentContainer.Add(contentRoot);
                }
                catch
                {
                    tab.Add(contentRoot);
                }
            }
        }

        private static void SetTabTitle(Tab tab, string title)
        {
            if (tab == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(title))
            {
                title = string.Empty;
            }

            try
            {
                var property = typeof(Tab).GetProperty(
                    "title",
                    BindingFlags.Instance | BindingFlags.Public
                );
                if (property != null && property.CanWrite)
                {
                    property.SetValue(tab, title);
                    return;
                }
            }
            catch { }

            try
            {
                var property = typeof(Tab).GetProperty(
                    "text",
                    BindingFlags.Instance | BindingFlags.Public
                );
                if (property != null && property.CanWrite)
                {
                    property.SetValue(tab, title);
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
                tab.name = string.IsNullOrEmpty(tab.name) ? $"Tab_{title}" : tab.name;
            }
            catch { }
        }
    }
}
