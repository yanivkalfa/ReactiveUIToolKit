using System.Collections.Generic;
using ReactiveUITK.Core;
using UnityEngine;

namespace ReactiveUITK.Examples.FunctionalComponents
{
    public static class MemoizedListFunc
    {
        public static VirtualNode Render(Dictionary<string, object> props, System.Collections.Generic.IReadOnlyList<VirtualNode> children)
        {
            var (filter, setFilter) = Hooks.UseState<string>("");
            var allItems = new List<string>{"apple","apricot","banana","blueberry","cherry"};
            var filtered = Hooks.UseMemo(() => allItems.FindAll(i => i.StartsWith(filter)), filter);
            var onType = Hooks.UseStableAction<string>(val => setFilter(val));

            // Simple input simulation (no real TextField adapter, demonstration only)
            var filterViewProps = new Dictionary<string, object>{{"style.marginBottom",6f}};
            var listChildren = new List<VirtualNode>();
            foreach (var item in filtered)
                listChildren.Add(V.Text(item));

            return V.View(new Dictionary<string, object>{{"style.padding",10f}}, null,
                V.Text("Filter (starts with): " + (filter==""?"<empty>":filter)),
                V.View(filterViewProps, null, V.Text("(Imagine input here)")),
                V.Fragment(null, listChildren.ToArray())
            );
        }
    }
}
