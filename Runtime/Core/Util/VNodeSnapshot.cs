using System.Text;
using System.Collections.Generic;

namespace ReactiveUITK.Core.Util
{
    public static class VNodeSnapshot
    {
        public static string Serialize(VirtualNode root)
        {
            var sb = new StringBuilder();
            SerializeNode(sb, root, 0);
            return sb.ToString();
        }

        public static string Diff(VirtualNode a, VirtualNode b)
        {
            var sb = new StringBuilder();
            DiffNode(sb, a, b, 0);
            return sb.ToString();
        }

        private static void DiffNode(StringBuilder sb, VirtualNode a, VirtualNode b, int depth)
        {
            string indent = new string(' ', depth * 2);
            if (a == null && b == null)
            {
                sb.AppendLine(indent + "= <null>");
                return;
            }
            if (a == null)
            {
                sb.AppendLine(indent + "+ " + NodeSummary(b));
                return;
            }
            if (b == null)
            {
                sb.AppendLine(indent + "- " + NodeSummary(a));
                return;
            }
            bool same = a.NodeType == b.NodeType && a.ElementTypeName == b.ElementTypeName && a.Key == b.Key && a.TextContent == b.TextContent;
            sb.Append(indent).Append(same ? "= " : "~ ").Append(NodeSummary(a)).Append(" -> ").Append(NodeSummary(b)).AppendLine();
            var aChildren = a.Children ?? (IReadOnlyList<VirtualNode>)System.Array.Empty<VirtualNode>();
            var bChildren = b.Children ?? (IReadOnlyList<VirtualNode>)System.Array.Empty<VirtualNode>();
            int max = aChildren.Count > bChildren.Count ? aChildren.Count : bChildren.Count;
            for (int i = 0; i < max; i++)
            {
                var ac = i < aChildren.Count ? aChildren[i] : null;
                var bc = i < bChildren.Count ? bChildren[i] : null;
                DiffNode(sb, ac, bc, depth + 1);
            }
        }

        private static string NodeSummary(VirtualNode n)
        {
            if (n == null) return "<null>";
            var txt = string.IsNullOrEmpty(n.TextContent) ? "" : (" text=" + n.TextContent.Replace('\n',' '));
            return $"{n.NodeType} key={n.Key ?? "?"} elem={n.ElementTypeName ?? "?"}{txt}";
        }

        private static void SerializeNode(StringBuilder sb, VirtualNode node, int depth)
        {
            if (node == null)
            {
                sb.AppendLine(new string(' ', depth * 2) + "<null>");
                return;
            }
            string indent = new string(' ', depth * 2);
            sb.Append(indent)
              .Append(node.NodeType)
              .Append(" key=").Append(node.Key ?? "?")
              .Append(" elem=").Append(node.ElementTypeName ?? "?");
            if (!string.IsNullOrEmpty(node.TextContent)) sb.Append(" text=").Append(node.TextContent.Replace('\n', ' '));
            sb.AppendLine();
            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    SerializeNode(sb, child, depth + 1);
                }
            }
        }
    }
}
