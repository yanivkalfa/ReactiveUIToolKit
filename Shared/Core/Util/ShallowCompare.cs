using System.Collections.Generic;

namespace ReactiveUITK.Core.Util
{
    public static class ShallowCompare
    {
        public static bool PropsEqual(IReadOnlyDictionary<string, object> first, IReadOnlyDictionary<string, object> second)
        {
            if (ReferenceEquals(first, second))
            {
                return true;
            }
            if (first == null || second == null)
            {
                return false;
            }
            if (first.Count != second.Count)
            {
                return false;
            }
            foreach (KeyValuePair<string, object> entry in first)
            {
                if (!second.TryGetValue(entry.Key, out object other))
                {
                    return false;
                }
                if (!Equals(entry.Value, other))
                {
                    return false;
                }
            }
            return true;
        }
        public static bool ShallowEqual<T>(IReadOnlyList<T> first, IReadOnlyList<T> second)
        {
            if (ReferenceEquals(first, second))
            {
                return true;
            }
            if (first == null || second == null)
            {
                return false;
            }
            if (first.Count != second.Count)
            {
                return false;
            }
            for (int i = 0; i < first.Count; i++)
            {
                if (!Equals(first[i], second[i]))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
