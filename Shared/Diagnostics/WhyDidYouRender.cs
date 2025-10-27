using System.Collections.Generic;
using UnityEngine;

namespace ReactiveUITK.Diagnostics
{
    public static class WhyDidYouRender
    {
        public static bool Enabled = false;

        public static void Log(
            string componentName,
            IReadOnlyDictionary<string, object> prev,
            IReadOnlyDictionary<string, object> next,
            bool forced
        )
        {
            if (!Enabled)
                return;
            if (forced)
            {
                Debug.Log($"[WDYR] {componentName} forced render");
                return;
            }
            if (ReferenceEquals(prev, next))
            {
                Debug.Log(
                    $"[WDYR] {componentName} rendered with identical props (reference equal)."
                );
                return;
            }
            if (prev == null || next == null)
            {
                Debug.Log($"[WDYR] {componentName} rendered; prev or next props were null.");
                return;
            }
            if (prev.Count != next.Count)
            {
                Debug.Log(
                    $"[WDYR] {componentName} rendered; prop count changed {prev.Count} -> {next.Count}."
                );
                return;
            }
            foreach (var kv in prev)
            {
                if (!next.TryGetValue(kv.Key, out var v) || !Equals(v, kv.Value))
                {
                    Debug.Log($"[WDYR] {componentName} prop changed: {kv.Key}");
                    return;
                }
            }
            Debug.Log($"[WDYR] {componentName} rendered but props shallow-equal. Consider memo.");
        }
    }
}
