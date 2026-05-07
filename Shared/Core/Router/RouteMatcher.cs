using System;
using System.Collections.Generic;

namespace ReactiveUITK.Router
{
    internal static class RouteMatcher
    {
        public static RouteMatch Match(
            string currentLocation,
            string pattern,
            bool exact,
            RouteMatch parentMatch
        )
        {
            return Match(currentLocation, pattern, exact, parentMatch, caseSensitive: false);
        }

        public static RouteMatch Match(
            string currentLocation,
            string pattern,
            bool exact,
            RouteMatch parentMatch,
            bool caseSensitive
        )
        {
            string normalizedLocation = RouterPath.Normalize(currentLocation);
            if (string.IsNullOrEmpty(pattern))
            {
                return new RouteMatch(
                    normalizedLocation,
                    normalizedLocation,
                    MergeParameters(parentMatch?.Parameters, null)
                );
            }

            if (pattern == "*" || pattern == "/*")
            {
                return new RouteMatch(
                    normalizedLocation,
                    normalizedLocation,
                    MergeParameters(parentMatch?.Parameters, null)
                );
            }

            string normalizedPattern = RouterPath.Normalize(pattern);
            var locationSegments = RouterPath.SplitSegments(normalizedLocation);
            var patternSegments = RouterPath.SplitSegments(normalizedPattern);

            bool hasWildcard =
                patternSegments.Length > 0 && patternSegments[patternSegments.Length - 1] == "*";
            int matchSegmentCount = hasWildcard
                ? patternSegments.Length - 1
                : patternSegments.Length;
            int locationSegmentCount = locationSegments.Length;

            if (matchSegmentCount > locationSegmentCount)
            {
                return null;
            }

            StringComparison comparison = caseSensitive
                ? StringComparison.Ordinal
                : StringComparison.OrdinalIgnoreCase;

            Dictionary<string, string> parameters = null;
            for (int i = 0; i < matchSegmentCount; i++)
            {
                if (i >= locationSegmentCount)
                {
                    return null;
                }
                string patternSegment = patternSegments[i];
                string locationSegment = locationSegments[i];
                if (patternSegment.StartsWith(":", StringComparison.Ordinal))
                {
                    string key = patternSegment.Substring(1);
                    if (!string.IsNullOrEmpty(key))
                    {
                        parameters ??= new Dictionary<string, string>();
                        parameters[key] = locationSegment;
                    }
                    continue;
                }
                if (!string.Equals(patternSegment, locationSegment, comparison))
                {
                    return null;
                }
            }

            if (exact && !hasWildcard && locationSegments.Length != matchSegmentCount)
            {
                return null;
            }

            var merged = MergeParameters(parentMatch?.Parameters, parameters);
            return new RouteMatch(normalizedLocation, normalizedPattern, merged);
        }

        private static IReadOnlyDictionary<string, string> MergeParameters(
            IReadOnlyDictionary<string, string> parent,
            Dictionary<string, string> current
        )
        {
            if ((parent == null || parent.Count == 0) && (current == null || current.Count == 0))
            {
                return RouterContextKeys.EmptyParams;
            }

            var merged = new Dictionary<string, string>();
            if (parent != null)
            {
                foreach (var kvp in parent)
                {
                    merged[kvp.Key] = kvp.Value;
                }
            }
            if (current != null)
            {
                foreach (var kvp in current)
                {
                    merged[kvp.Key] = kvp.Value;
                }
            }
            return merged;
        }
    }
}
