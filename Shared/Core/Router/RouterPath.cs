using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ReactiveUITK.Router
{
    public static class RouterPath
    {
        public static string Combine(string basePath, string relativePath)
        {
            string normalizedBase = Normalize(basePath);
            normalizedBase = TrimTrailingWildcard(normalizedBase);

            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return normalizedBase;
            }

            if (relativePath == "*" || relativePath == "/*")
            {
                if (normalizedBase == "/")
                {
                    return "/*";
                }
                return Normalize(normalizedBase.TrimEnd('/') + "/*");
            }

            if (relativePath.StartsWith("/"))
            {
                return Normalize(relativePath);
            }

            string combined = normalizedBase == "/"
                ? "/" + relativePath
                : normalizedBase.TrimEnd('/') + "/" + relativePath;

            return Normalize(combined);
        }

        public static RouterLocation Parse(string raw, object state = null)
        {
            string working = raw ?? "/";
            int queryIndex = working.IndexOf('?');
            string pathPart = queryIndex >= 0 ? working.Substring(0, queryIndex) : working;
            string queryPart =
                queryIndex >= 0 && queryIndex + 1 < working.Length
                    ? working.Substring(queryIndex + 1)
                    : null;

            string normalizedPath = Normalize(pathPart);
            var query = ParseQuery(queryPart);
            return new RouterLocation(normalizedPath, query, state);
        }

        public static string Normalize(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return "/";
            }

            string sanitized = path.Replace('\\', '/').Trim();
            if (sanitized == "/")
            {
                return "/";
            }

            var segments = SplitSegmentsInternal(sanitized);
            if (segments.Count == 0)
            {
                return "/";
            }

            return "/" + string.Join("/", segments);
        }

        public static string[] SplitSegments(string path)
        {
            var segments = SplitSegmentsInternal(path);
            return segments.Count == 0 ? Array.Empty<string>() : segments.ToArray();
        }

        public static IReadOnlyDictionary<string, string> ParseQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return RouterContextKeys.EmptyParams;
            }

            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string[] parts = query.Split('&');
            foreach (var part in parts)
            {
                if (string.IsNullOrEmpty(part))
                {
                    continue;
                }
                var kvp = part.Split(new[] { '=' }, 2);
                string key = Uri.UnescapeDataString(kvp[0]);
                string value = kvp.Length > 1 ? Uri.UnescapeDataString(kvp[1]) : string.Empty;
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }
                dict[key] = value;
            }

            return dict.Count == 0
                ? RouterContextKeys.EmptyParams
                : new ReadOnlyDictionary<string, string>(dict);
        }

        /// <summary>
        /// Serialises a query dictionary back into a <c>k=v&amp;k2=v2</c> string
        /// (without the leading <c>?</c>).  Returns the empty string when the
        /// dictionary is null or empty.  Both keys and values are URL-escaped.
        /// </summary>
        public static string BuildQuery(IReadOnlyDictionary<string, string> query)
        {
            if (query == null || query.Count == 0)
            {
                return string.Empty;
            }
            var sb = new System.Text.StringBuilder();
            bool first = true;
            foreach (var kvp in query)
            {
                if (string.IsNullOrEmpty(kvp.Key))
                {
                    continue;
                }
                if (!first)
                {
                    sb.Append('&');
                }
                first = false;
                sb.Append(Uri.EscapeDataString(kvp.Key));
                if (kvp.Value != null)
                {
                    sb.Append('=').Append(Uri.EscapeDataString(kvp.Value));
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// If <paramref name="path"/> begins with the supplied
        /// <paramref name="basename"/> (case-insensitive), returns the path
        /// with the basename removed (always normalised to start with
        /// <c>"/"</c>).  Otherwise returns the input unchanged.
        /// </summary>
        public static string StripBasename(string path, string basename)
        {
            if (string.IsNullOrEmpty(basename) || basename == "/")
            {
                return Normalize(path);
            }
            string normalizedBasename = Normalize(basename);
            string normalizedPath = Normalize(path);
            if (
                normalizedPath.StartsWith(
                    normalizedBasename,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                if (normalizedPath.Length == normalizedBasename.Length)
                {
                    return "/";
                }
                if (normalizedPath[normalizedBasename.Length] == '/')
                {
                    return Normalize(normalizedPath.Substring(normalizedBasename.Length));
                }
            }
            return normalizedPath;
        }

        /// <summary>
        /// Prefixes <paramref name="path"/> with <paramref name="basename"/>
        /// (when non-empty and not already present).  Used when the router
        /// dispatches navigation calls to the underlying history.
        /// </summary>
        public static string WithBasename(string path, string basename)
        {
            if (string.IsNullOrEmpty(basename) || basename == "/")
            {
                return Normalize(path);
            }
            string normalizedBasename = Normalize(basename);
            string normalizedPath = Normalize(path);
            if (
                normalizedPath.StartsWith(
                    normalizedBasename,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return normalizedPath;
            }
            if (normalizedPath == "/")
            {
                return normalizedBasename;
            }
            return Normalize(normalizedBasename.TrimEnd('/') + normalizedPath);
        }

        private static List<string> SplitSegmentsInternal(string path)
        {
            List<string> buffer = new List<string>();
            if (string.IsNullOrEmpty(path))
            {
                return buffer;
            }

            string working = path.Replace('\\', '/');
            string[] raw = working.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var segment in raw)
            {
                var trimmed = segment.Trim();
                if (trimmed.Length == 0)
                {
                    continue;
                }
                buffer.Add(trimmed);
            }
            return buffer;
        }

        private static string TrimTrailingWildcard(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return "/";
            }

            if (path.EndsWith("/*", StringComparison.Ordinal))
            {
                if (path.Length <= 2)
                {
                    return "/";
                }
                return path.Substring(0, path.Length - 2);
            }

            return path;
        }
    }
}
