// Editor-only: PropertyField and InspectorElement
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using ReactiveUITK.Props;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class PropertyFieldElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create() => new PropertyField();

        public override void ApplyProperties(VisualElement element, IReadOnlyDictionary<string, object> properties)
        {
            if (element is PropertyField pf && properties != null)
            {
                if (properties.TryGetValue("label", out var lo) && lo is string ls)
                {
                    pf.label = ls ?? string.Empty;
                }
                // Bind via (target, bindingPath)
                if (
                    properties.TryGetValue("target", out var to)
                    && to is UnityEngine.Object target
                    && properties.TryGetValue("bindingPath", out var po)
                    && po is string path
                )
                {
                    try
                    {
                        var so = new SerializedObject(target);
                        var sp = so.FindProperty(path);
                        if (sp != null)
                        {
                            pf.BindProperty(sp);
                        }
                    }
                    catch { }
                }
            }
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(VisualElement element, IReadOnlyDictionary<string, object> previous, IReadOnlyDictionary<string, object> next)
        {
            if (element is PropertyField pf)
            {
                string pl = previous != null && previous.TryGetValue("label", out var p) ? p as string : null;
                string nl = next != null && next.TryGetValue("label", out var n) ? n as string : null;
                if (!string.Equals(pl, nl, StringComparison.Ordinal))
                {
                    pf.label = nl ?? string.Empty;
                }
                object pt = previous != null && previous.TryGetValue("target", out var pto) ? pto : null;
                object nt = next != null && next.TryGetValue("target", out var nto) ? nto : null;
                object pp = previous != null && previous.TryGetValue("bindingPath", out var ppo) ? ppo : null;
                object np = next != null && next.TryGetValue("bindingPath", out var npo) ? npo : null;
                if (!ReferenceEquals(pt, nt) || !Equals(pp, np))
                {
                    ApplyProperties(element, next);
                }
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }
    }

    public sealed class InspectorElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create() => new InspectorElement();

        public override void ApplyProperties(VisualElement element, IReadOnlyDictionary<string, object> properties)
        {
            if (element is InspectorElement ins && properties != null)
            {
                if (properties.TryGetValue("target", out var to) && to is UnityEngine.Object obj)
                {
                    // Rebuild the displayed inspector by clearing and adding a fresh instance
                    ins.Clear();
                    ins.Add(new InspectorElement(obj));
                }
            }
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(VisualElement element, IReadOnlyDictionary<string, object> previous, IReadOnlyDictionary<string, object> next)
        {
            if (element is InspectorElement)
            {
                object pt = previous != null && previous.TryGetValue("target", out var pto) ? pto : null;
                object nt = next != null && next.TryGetValue("target", out var nto) ? nto : null;
                if (!ReferenceEquals(pt, nt))
                {
                    ApplyProperties(element, next);
                }
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }
    }
}
#endif
