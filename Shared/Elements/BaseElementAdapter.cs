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

            if (ReferenceEquals(prevRaw, nextRaw))
            {
                return false;
            }

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
                _rootWrapWarned ??= new HashSet<string>();
                string tag = contextTag ?? "Adapter";
                if (_rootWrapWarned.Add(tag))
                {
                    Debug.LogWarning(
                        $"[ReactiveUITK][{tag}] Root was not a 'VisualElement'. Wrapping automatically (further wraps suppressed)."
                    );
                }
                return ReactiveUITK.V.VisualElement(null, null, vnode);
            }
            return vnode;
        }

        private static HashSet<string> _rootWrapWarned;

        public static List<int> CoerceIds(object value)
        {
            if (value == null)
            {
                return null;
            }
            try
            {
                var list = new List<int>();
                if (value is IEnumerable<int> gen)
                {
                    foreach (var v in gen)
                    {
                        list.Add(v);
                    }
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
