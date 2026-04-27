// Editor-only: PropertyField and InspectorElement
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using ReactiveUITK.Props;
using ReactiveUITK.Props.Typed;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class PropertyFieldElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create() => new PropertyField();

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
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

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is PropertyField pf)
            {
                string pl =
                    previous != null && previous.TryGetValue("label", out var p)
                        ? p as string
                        : null;
                string nl =
                    next != null && next.TryGetValue("label", out var n) ? n as string : null;
                if (!string.Equals(pl, nl, StringComparison.Ordinal))
                {
                    pf.label = nl ?? string.Empty;
                }
                object pt =
                    previous != null && previous.TryGetValue("target", out var pto) ? pto : null;
                object nt = next != null && next.TryGetValue("target", out var nto) ? nto : null;
                object pp =
                    previous != null && previous.TryGetValue("bindingPath", out var ppo)
                        ? ppo
                        : null;
                object np =
                    next != null && next.TryGetValue("bindingPath", out var npo) ? npo : null;
                if (!ReferenceEquals(pt, nt) || !Equals(pp, np))
                {
                    ApplyProperties(element, next);
                }
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }

        public override void ApplyTypedFull(VisualElement element, BaseProps props)
        {
            if (element is PropertyField pf && props is PropertyFieldProps fp)
            {
                if (fp.Label != null)
                    pf.label = fp.Label;
                if (fp.Target != null && !string.IsNullOrEmpty(fp.BindingPath))
                {
                    try
                    {
                        var so = new SerializedObject(fp.Target);
                        var sp = so.FindProperty(fp.BindingPath);
                        if (sp != null)
                            pf.BindProperty(sp);
                    }
                    catch { }
                }
            }
            base.ApplyTypedFull(element, props);
        }

        public override void ApplyTypedDiff(VisualElement element, BaseProps prev, BaseProps next)
        {
            if (
                element is PropertyField pf
                && prev is PropertyFieldProps fp
                && next is PropertyFieldProps fn
            )
            {
                if (fp.Label != fn.Label && fn.Label != null)
                    pf.label = fn.Label;
                if (fp.Target != fn.Target || fp.BindingPath != fn.BindingPath)
                {
                    if (fn.Target != null && !string.IsNullOrEmpty(fn.BindingPath))
                    {
                        try
                        {
                            var so = new SerializedObject(fn.Target);
                            var sp = so.FindProperty(fn.BindingPath);
                            if (sp != null)
                                pf.BindProperty(sp);
                        }
                        catch { }
                    }
                }
            }
            base.ApplyTypedDiff(element, prev, next);
        }
    }

    public sealed class InspectorElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create() => new InspectorElement();

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
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

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is InspectorElement)
            {
                object pt =
                    previous != null && previous.TryGetValue("target", out var pto) ? pto : null;
                object nt = next != null && next.TryGetValue("target", out var nto) ? nto : null;
                if (!ReferenceEquals(pt, nt))
                {
                    ApplyProperties(element, next);
                }
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }

        public override void ApplyTypedFull(VisualElement element, BaseProps props)
        {
            if (element is InspectorElement ins && props is InspectorElementProps fp)
            {
                if (fp.Target != null)
                {
                    ins.Clear();
                    ins.Add(new InspectorElement(fp.Target));
                }
            }
            base.ApplyTypedFull(element, props);
        }

        public override void ApplyTypedDiff(VisualElement element, BaseProps prev, BaseProps next)
        {
            if (
                element is InspectorElement ins
                && prev is InspectorElementProps fp
                && next is InspectorElementProps fn
            )
            {
                if (fp.Target != fn.Target && fn.Target != null)
                {
                    ins.Clear();
                    ins.Add(new InspectorElement(fn.Target));
                }
            }
            base.ApplyTypedDiff(element, prev, next);
        }
    }
}
#endif
