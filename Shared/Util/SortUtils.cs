using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

namespace ReactiveUITK.Shared.Util
{
    public static class SortUtils
    {
        public static List<T> MultiSort<T>(
            IEnumerable<MultiColumnTreeViewProps.SortedColumnDef> descriptors,
            IEnumerable<T> rows,
            Func<T, string, object> valueGetter,
            StringComparer comparer = null
        )
        {
            comparer ??= StringComparer.OrdinalIgnoreCase;
            var source = rows ?? Enumerable.Empty<T>();
            if (descriptors == null)
                return source.ToList();

            IOrderedEnumerable<T> query = null;
            foreach (var d in descriptors.OrderBy(d => d.Index ?? 0))
            {
                var colName = d?.Name ?? string.Empty;
                bool desc = d?.Direction == SortDirection.Descending;
                Func<T, string> keyFn = r =>
                    (valueGetter?.Invoke(r, colName))?.ToString() ?? string.Empty;

                query =
                    query == null
                        ? (
                            desc
                                ? source.OrderByDescending(keyFn, comparer)
                                : source.OrderBy(keyFn, comparer)
                        )
                        : (
                            desc
                                ? query.ThenByDescending(keyFn, comparer)
                                : query.ThenBy(keyFn, comparer)
                        );
            }

            return (query ?? source.OrderBy(_ => 0)).ToList();
        }
    }
}
