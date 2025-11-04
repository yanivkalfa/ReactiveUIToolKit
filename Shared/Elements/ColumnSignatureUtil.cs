using System;
using System.Collections;
using System.Collections.Generic;
using ReactiveUITK.Core;

namespace ReactiveUITK.Elements
{
    internal static class ColumnSignatureUtil
    {
        public static (
            List<ColumnSignature> sig,
            List<Func<int, object, VirtualNode>> fns
        ) Extract(IEnumerable cols)
        {
            var list = new List<ColumnSignature>();
            var fns = new List<Func<int, object, VirtualNode>>();
            if (cols == null)
                return (list, fns);
            foreach (var co in cols)
            {
                if (co is not IDictionary<string, object> colMap)
                    continue;
                colMap.TryGetValue("name", out var n);
                colMap.TryGetValue("title", out var t);
                Func<int, object, VirtualNode> fn = null;
                if (colMap.TryGetValue("cell", out var c) && c is Func<int, object, VirtualNode> cf)
                    fn = cf;
                list.Add(new ColumnSignature { Name = n as string, Title = t as string });
                fns.Add(fn);
            }
            return (list, fns);
        }

        public static bool Equal(List<ColumnSignature> a, List<ColumnSignature> b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a == null || b == null)
                return false;
            if (a.Count != b.Count)
                return false;
            for (int i = 0; i < a.Count; i++)
            {
                if (!string.Equals(a[i].Name, b[i].Name, StringComparison.Ordinal))
                    return false;
                if (!string.Equals(a[i].Title, b[i].Title, StringComparison.Ordinal))
                    return false;
            }
            return true;
        }
    }
}

