using System.Text;
using System.Collections.Generic;
using System;

namespace ReactiveUITK.Core.Util
{
    public static class VNodeSnapshot
    {
        public static string Serialize(VirtualNode rootNode)
        {
            StringBuilder builder = new();
            SerializeNode(builder, rootNode, 0);
            return builder.ToString();
        }

        public static string Diff(VirtualNode first, VirtualNode second)
        {
            StringBuilder builder = new();
            DiffNode(builder, first, second, 0);
            return builder.ToString();
        }

        private static void DiffNode(StringBuilder builder, VirtualNode first, VirtualNode second, int depth)
        {
            string indent = new string(' ', depth * 2);
            if (first == null && second == null)
            {
                builder.AppendLine(indent + "= <null>");
                return;
            }
            if (first == null)
            {
                builder.AppendLine(indent + "+ " + NodeSummary(second));
                return;
            }
            if (second == null)
            {
                builder.AppendLine(indent + "- " + NodeSummary(first));
                return;
            }
            bool sameTypeAndProps = first.NodeType == second.NodeType && first.ElementTypeName == second.ElementTypeName && first.Key == second.Key && first.TextContent == second.TextContent;
            builder.Append(indent).Append(sameTypeAndProps ? "= " : "~ ").Append(NodeSummary(first)).Append(" -> ").Append(NodeSummary(second)).AppendLine();
            IReadOnlyList<VirtualNode> firstChildren = first.Children ?? (IReadOnlyList<VirtualNode>)System.Array.Empty<VirtualNode>();
            IReadOnlyList<VirtualNode> secondChildren = second.Children ?? (IReadOnlyList<VirtualNode>)System.Array.Empty<VirtualNode>();
            int maxCount = firstChildren.Count > secondChildren.Count ? firstChildren.Count : secondChildren.Count;
            for (int i = 0; i < maxCount; i++)
            {
                VirtualNode firstChild = i < firstChildren.Count ? firstChildren[i] : null;
                VirtualNode secondChild = i < secondChildren.Count ? secondChildren[i] : null;
                DiffNode(builder, firstChild, secondChild, depth + 1);
            }
        }

        private static string NodeSummary(VirtualNode node)
        {
            if (node == null)
            {
                return "<null>";
            }
            string textPart = string.IsNullOrEmpty(node.TextContent) ? string.Empty : (" text=" + node.TextContent.Replace('\n', ' '));
            return $"{node.NodeType} key={node.Key ?? "?"} elem={node.ElementTypeName ?? "?"}{textPart}";
        }

        private static void SerializeNode(StringBuilder builder, VirtualNode node, int depth)
        {
            if (node == null)
            {
                builder.AppendLine(new string(' ', depth * 2) + "<null>");
                return;
            }
            string indent = new string(' ', depth * 2);
            builder.Append(indent)
                   .Append(node.NodeType)
                   .Append(" key=").Append(node.Key ?? "?")
                   .Append(" elem=").Append(node.ElementTypeName ?? "?");
            if (!string.IsNullOrEmpty(node.TextContent))
            {
                builder.Append(" text=").Append(node.TextContent.Replace('\n', ' '));
            }
            if (node.Properties != null && node.Properties.TryGetValue("style", out object styleObj) && styleObj is IDictionary<string, object> styleMap && styleMap.Count > 0)
            {
                builder.Append(" styles={");
                bool firstEntry = true;
                foreach (KeyValuePair<string, object> styleEntry in styleMap)
                {
                    if (!firstEntry)
                    {
                        builder.Append(",");
                    }
                    firstEntry = false;
                    builder.Append(styleEntry.Key).Append(":").Append(styleEntry.Value);
                }
                builder.Append("}");
            }
            builder.AppendLine();
            if (node.Children != null)
            {
                foreach (VirtualNode child in node.Children)
                {
                    SerializeNode(builder, child, depth + 1);
                }
            }
        }
    }
}
