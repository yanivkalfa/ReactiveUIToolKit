using System.Collections.Generic;

namespace ReactiveUITK.Core.Util
{
    public static class ShallowCompare
    {
        public static bool PropsEqual(IReadOnlyDictionary<string, object> a, IReadOnlyDictionary<string, object> b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null) return false;
            if (a.Count != b.Count) return false;
            foreach (var kv in a)
            {
                if (!b.TryGetValue(kv.Key, out var other)) return false;
                if (Equals(kv.Value, other) == false) return false;
            }
            return true;
        }

        public static bool ShallowEqual<T>(IReadOnlyList<T> a, IReadOnlyList<T> b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null) return false;
            if (a.Count != b.Count) return false;
            for (int i = 0; i < a.Count; i++) if (!Equals(a[i], b[i])) return false; return true;
        }
    }
}
