using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine;

namespace ReactiveUITK.Examples.FunctionalComponents
{
    public static class MemoizedListFunc
    {
        public static VirtualNode Render(Dictionary<string, object> props, System.Collections.Generic.IReadOnlyList<VirtualNode> children)
        {
            var (filter, setFilter) = Hooks.UseState("");
            var allItems = new List<string>{"apple","apricot","banana","blueberry","cherry"};
            var filteredItems = Hooks.UseMemo(() => allItems.FindAll(i => i.StartsWith(filter)), filter);
            var onType = Hooks.UseStableAction<string>(val => setFilter(val));
            var listChildren = new List<VirtualNode>();
            foreach (var item in filteredItems)
            {
                listChildren.Add(V.Text(item));
            }
            var outerStyle = new Dictionary<string, object>{{"padding",10f}};
            var filterStyle = new Dictionary<string, object>{{"marginBottom",6f}};
            var outerProps = new Dictionary<string, object>{{"style", outerStyle}};
            var filterViewProps = new Dictionary<string, object>{{"style", filterStyle}};
            string filterLabel = filter == string.Empty ? "<empty>" : filter;
            return V.VisualElement(outerProps, null,
                V.Text("Filter (starts with): " + filterLabel),
                V.VisualElement(filterViewProps, null, V.Text("(Imagine input here)")),
                V.Fragment(null, listChildren.ToArray())
            );
        }
    }
}
