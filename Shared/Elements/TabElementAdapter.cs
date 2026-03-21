using System.Collections.Generic;
using System.Reflection;
using ReactiveUITK.Props;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class TabElementAdapter : BaseElementAdapter
    {
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
                var titleLabel = tab.Q<Label>("title") ?? tab.Q<Label>();
                if (titleLabel != null)
                {
                    titleLabel.text = title;
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

        public override VisualElement Create()
        {
            return new Tab();
        }

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (element is Tab tab && properties != null)
            {
                if (properties.TryGetValue("text", out var t) && t is string txt)
                {
                    SetTabTitle(tab, txt);
                }

                if (
                    properties.TryGetValue("contentContainer", out var cc)
                    && cc is Dictionary<string, object> ccMap
                )
                {
                    PropsApplier.Apply(tab.contentContainer, ccMap);
                }
            }
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is Tab tab)
            {
                previous ??= s_emptyProps;
                next ??= s_emptyProps;
                previous.TryGetValue("text", out var p);
                next.TryGetValue("text", out var n);
                if (!Equals(p, n) && n is string txt)
                {
                    SetTabTitle(tab, txt);
                }
                previous.TryGetValue("contentContainer", out var pcc);
                next.TryGetValue("contentContainer", out var ncc);
                if (!ReferenceEquals(pcc, ncc) && ncc is Dictionary<string, object> ccMap)
                {
                    PropsApplier.Apply(tab.contentContainer, ccMap);
                }
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }
    }
}
