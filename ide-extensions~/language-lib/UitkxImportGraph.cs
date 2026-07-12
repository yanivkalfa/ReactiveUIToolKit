using System.Collections.Generic;

namespace ReactiveUITK.Language
{
    /// <summary>
    /// The <c>.uitkx</c> import graph (import/export grammar, leg 3, plan §6/§8). Pure and
    /// host-agnostic: nodes are file paths, edges are import relationships. Used for the value-import
    /// cycle diagnostic (UITKX2306) — hooks and modules load eagerly, so a cycle among their imports
    /// is a TDZ-parity error family-wide (component-only cycles are legal). The same forward/reverse
    /// edge model backs HMR's cross-file refresh propagation.
    /// </summary>
    public static class UitkxImportGraph
    {
        /// <summary>
        /// Find the first cycle in the directed graph described by <paramref name="edges"/>
        /// (node → the nodes it points to). Returns the cycle as an ordered list that starts and ends
        /// at the same node (e.g. <c>[A, B, A]</c>), or <c>null</c> when the graph is acyclic. Node
        /// comparison is ordinal-case-insensitive (filesystem parity).
        /// </summary>
        public static IReadOnlyList<string>? FindCycle(
            IReadOnlyDictionary<string, IReadOnlyList<string>> edges)
        {
            var state = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase); // 0=unseen,1=on-stack,2=done
            var stack = new List<string>();

            foreach (var node in edges.Keys)
            {
                var cycle = Visit(node, edges, state, stack);
                if (cycle != null)
                    return cycle;
            }
            return null;
        }

        private static IReadOnlyList<string>? Visit(
            string node,
            IReadOnlyDictionary<string, IReadOnlyList<string>> edges,
            Dictionary<string, int> state,
            List<string> stack)
        {
            state.TryGetValue(node, out int s);
            if (s == 2) return null;      // fully explored, no cycle through here
            if (s == 1)                    // back-edge → cycle: slice the stack from this node
            {
                int idx = stack.FindLastIndex(n =>
                    string.Equals(n, node, System.StringComparison.OrdinalIgnoreCase));
                if (idx < 0) return null;
                var cycle = new List<string>();
                for (int i = idx; i < stack.Count; i++) cycle.Add(stack[i]);
                cycle.Add(node); // close the loop
                return cycle;
            }

            state[node] = 1;
            stack.Add(node);

            if (edges.TryGetValue(node, out var targets))
            {
                foreach (var t in targets)
                {
                    var cycle = Visit(t, edges, state, stack);
                    if (cycle != null) return cycle;
                }
            }

            stack.RemoveAt(stack.Count - 1);
            state[node] = 2;
            return null;
        }
    }
}
