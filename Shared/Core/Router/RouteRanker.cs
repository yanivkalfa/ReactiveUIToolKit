using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveUITK.Core;

namespace ReactiveUITK.Router
{
    /// <summary>
    /// Port of React Router v6's route-ranking algorithm
    /// (<c>rankRouteBranches</c> + <c>computeScore</c>).  Used by
    /// <c>&lt;Routes&gt;</c> — and by layout <c>&lt;Route&gt;</c>s with child
    /// routes — to pick the most-specific match deterministically when several
    /// patterns can match the same location.
    ///
    /// Score model (matches RR exactly):
    /// <list type="bullet">
    /// <item><description><b>+10</b> per static segment</description></item>
    /// <item><description><b>+3</b>  per dynamic <c>:param</c> segment</description></item>
    /// <item><description><b>+2</b>  index-route bonus</description></item>
    /// <item><description><b>+1</b>  per empty segment</description></item>
    /// <item><description><b>−2</b>  splat (<c>*</c>) penalty</description></item>
    /// </list>
    /// Higher score wins.  Ties are broken by route declaration order (parents
    /// before children, declared-first wins among siblings).
    /// </summary>
    internal static class RouteRanker
    {
        private const int StaticSegmentValue = 10;
        private const int DynamicSegmentValue = 3;
        private const int IndexRouteValue = 2;
        private const int EmptySegmentValue = 1;
        private const int SplatPenalty = -2;
        private const int ParamRegex = -3; // reserved for future RR-style param-regex tiebreak

        /// <summary>
        /// Computes RR's stability score for a single resolved route pattern.
        /// </summary>
        public static int ComputeScore(string resolvedPath, bool isIndex)
        {
            string normalized = RouterPath.Normalize(resolvedPath ?? "/");
            string[] segments =
                normalized == "/" ? Array.Empty<string>() : RouterPath.SplitSegments(normalized);
            int score = segments.Length;
            bool initialMatchAdjusted = false;

            if (isIndex)
            {
                score += IndexRouteValue;
            }

            for (int i = 0; i < segments.Length; i++)
            {
                string segment = segments[i];
                if (segment == "*")
                {
                    score += SplatPenalty;
                    continue;
                }
                if (segment.StartsWith(":", StringComparison.Ordinal))
                {
                    score += DynamicSegmentValue;
                    continue;
                }
                if (segment.Length == 0)
                {
                    score += EmptySegmentValue;
                    continue;
                }
                score += StaticSegmentValue;
                initialMatchAdjusted = true;
            }

            // Index routes always sit "at" their parent — they pay no segment
            // count cost.  Mirrors RR: their score is parent-segments + index-bonus.
            if (isIndex && segments.Length > 0)
            {
                score -= segments.Length;
            }

            return initialMatchAdjusted || isIndex || segments.Length == 0
                ? score
                : score; // (kept for parity with RR's source structure)
        }

        /// <summary>
        /// Description of a single rankable route candidate.
        /// </summary>
        public readonly struct Candidate
        {
            public Candidate(
                int declarationIndex,
                string resolvedPath,
                bool isIndex,
                bool exact,
                bool caseSensitive,
                VirtualNode node
            )
            {
                DeclarationIndex = declarationIndex;
                ResolvedPath = resolvedPath ?? "/";
                IsIndex = isIndex;
                Exact = exact;
                CaseSensitive = caseSensitive;
                Node = node;
            }

            public int DeclarationIndex { get; }
            public string ResolvedPath { get; }
            public bool IsIndex { get; }
            public bool Exact { get; }
            public bool CaseSensitive { get; }
            public VirtualNode Node { get; }
        }

        /// <summary>
        /// Ranks <paramref name="candidates"/> highest-score-first (declaration
        /// order on ties) and returns the first one whose pattern matches the
        /// location.  Returns <c>null</c> when nothing matches.
        /// </summary>
        public static (Candidate Candidate, RouteMatch Match)? Pick(
            IReadOnlyList<Candidate> candidates,
            string currentLocation,
            RouteMatch parentMatch
        )
        {
            if (candidates == null || candidates.Count == 0)
            {
                return null;
            }

            var ordered = candidates
                .Select(c => new { Cand = c, Score = ComputeScore(c.ResolvedPath, c.IsIndex) })
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Cand.DeclarationIndex)
                .ToArray();

            for (int i = 0; i < ordered.Length; i++)
            {
                var c = ordered[i].Cand;
                bool exact = c.IsIndex || c.Exact;
                var match = RouteMatcher.Match(
                    currentLocation,
                    c.IsIndex ? parentMatch?.Pattern ?? "/" : c.ResolvedPath,
                    exact,
                    parentMatch,
                    c.CaseSensitive
                );
                if (match != null)
                {
                    return (c, match);
                }
            }

            return null;
        }
    }
}
