using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public abstract class BaseElementAdapter : IElementAdapter
    {
        protected const string MountName = "__ru_mount";

        public abstract VisualElement Create();

        public virtual void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        ) { }

        public virtual VisualElement ResolveChildHost(VisualElement element) => element;

        protected virtual VisualElement EnsureMount(VisualElement parent)
        {
            var mount = parent.Q<VisualElement>(MountName);
            if (mount == null)
            {
                mount = new VisualElement { name = MountName };
                parent.Add(mount);
            }
            return mount;
        }

        public virtual void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        ) { }

        protected bool TryApplyProp<T>(
            IReadOnlyDictionary<string, object> properties,
            string key,
            Action<T> assign
        )
        {
            if (properties == null)
            {
                return false;
            }
            if (!properties.TryGetValue(key, out var raw))
            {
                return false;
            }
            if (raw is T typed)
            {
                assign?.Invoke(typed);
                return true;
            }
            return false;
        }

        protected bool TryDiffProp<T>(
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next,
            string key,
            Action<T> assign
        )
        {
            previous ??= new Dictionary<string, object>();
            next ??= new Dictionary<string, object>();
            previous.TryGetValue(key, out var prevRaw);
            next.TryGetValue(key, out var nextRaw);
            // Fast path: exact same reference
            if (ReferenceEquals(prevRaw, nextRaw))
            {
                return false;
            }
            // Value equality: avoid reassigning when values are equal (e.g., boxed value types, strings)
            if (prevRaw != null && nextRaw != null && Equals(prevRaw, nextRaw))
            {
                return false;
            }
            if (nextRaw is T typed)
            {
                assign?.Invoke(typed);
                return true;
            }
            return false;
        }

        // Helpers shared by adapters
        protected static VirtualNode EnsureVisualElementRoot(
            VirtualNode vnode,
            string contextTag = null
        )
        {
            if (vnode == null)
            {
                return null;
            }
            bool isRootVE =
                vnode.NodeType == VirtualNodeType.Element
                && string.Equals(vnode.ElementTypeName, "VisualElement", StringComparison.Ordinal);
            if (!isRootVE)
            {
                Debug.LogWarning(
                    $"[ReactiveUITK][{contextTag ?? "Adapter"}] Root was not a 'VisualElement'. Wrapping automatically."
                );
                return ReactiveUITK.V.VisualElement(null, null, vnode);
            }
            return vnode;
        }

        // Coerce various enumerable/object inputs into a List<int> of ids
        // Public for reuse by trackers that don't inherit from BaseElementAdapter
        public static List<int> CoerceIds(object value)
        {
            if (value == null)
                return null;
            try
            {
                var list = new List<int>();
                if (value is IEnumerable<int> gen)
                {
                    foreach (var v in gen)
                        list.Add(v);
                    return list;
                }
                if (value is System.Collections.IEnumerable any)
                {
                    foreach (var o in any)
                    {
                        try
                        {
                            list.Add(Convert.ToInt32(o));
                        }
                        catch { }
                    }
                    return list;
                }
            }
            catch { }
            return null;
        }
    }
}
