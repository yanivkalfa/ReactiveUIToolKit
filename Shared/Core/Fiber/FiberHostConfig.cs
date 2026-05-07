using System.Collections.Generic;
using ReactiveUITK.Elements;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

namespace ReactiveUITK.Core.Fiber
{
    /// <summary>
    /// Host configuration - handles platform-specific operations
    /// Uses Element Registry adapters for proper element creation and property application
    /// </summary>
    public class FiberHostConfig
    {
        private readonly ElementRegistry _registry;

        public FiberHostConfig(ElementRegistry registry)
        {
            _registry = registry;
        }

        /// <summary>
        /// Create a VisualElement of the given type using the adapter registry
        /// </summary>
        public VisualElement CreateElement(string elementType)
        {
            var adapter = _registry.Resolve(elementType);
            VisualElement element;
            if (adapter != null)
            {
                element = adapter.Create();
            }
            else
            {
                // Fallback to generic VisualElement
                element = new VisualElement { name = elementType };
            }
            return element;
        }

        /// <summary>
        /// Apply properties to an element using the adapter
        /// </summary>
        public void ApplyProperties(
            VisualElement element,
            string elementType,
            IReadOnlyDictionary<string, object> oldProps,
            IReadOnlyDictionary<string, object> newProps
        )
        {
            var adapter = _registry.Resolve(elementType);
            if (adapter != null && newProps != null)
            {
                if (oldProps != null)
                {
                    adapter.ApplyPropertiesDiff(element, oldProps, newProps);
                }
                else
                {
                    adapter.ApplyProperties(element, newProps);
                }
            }
        }

        /// <summary>
        /// Apply typed properties to an element using the adapter's typed pipeline.
        /// Avoids dictionary allocation and iteration.
        /// </summary>
        public void ApplyTypedProperties(
            VisualElement element,
            string elementType,
            BaseProps oldProps,
            BaseProps newProps
        )
        {
            var adapter = _registry.Resolve(elementType);
            if (adapter is ITypedElementAdapter typed && newProps != null)
            {
                if (oldProps != null)
                    typed.ApplyTypedDiff(element, oldProps, newProps);
                else
                    typed.ApplyTypedFull(element, newProps);
            }
        }

        /// <summary>
        /// Append child to parent
        /// </summary>
        public void AppendChild(VisualElement parent, VisualElement child)
        {
            parent.Add(child);
        }

        /// <summary>
        /// Insert child before reference child
        /// </summary>
        public void InsertBefore(
            VisualElement parent,
            VisualElement child,
            VisualElement beforeChild
        )
        {
            if (beforeChild == null)
            {
                parent.Add(child);
            }
            else
            {
                int index = parent.IndexOf(beforeChild);
                if (index >= 0)
                {
                    parent.Insert(index, child);
                }
                else
                {
                    parent.Add(child);
                }
            }
        }

        /// <summary>
        /// Remove child from parent
        /// </summary>
        public void RemoveChild(VisualElement parent, VisualElement child)
        {
            if (child != null && child.parent == parent)
            {
                parent.Remove(child);
            }
        }

        /// <summary>
        /// Get parent element
        /// </summary>
        public VisualElement GetParent(VisualElement element)
        {
            return element?.parent;
        }

        /// <summary>
        /// Clear all children
        /// </summary>
        public void ClearChildren(VisualElement element)
        {
            element?.Clear();
        }
    }
}
