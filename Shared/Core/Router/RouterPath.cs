using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ReactiveUITK.Router
{
    public static class RouterPath
    {
        public static RouterLocation Parse(string raw, object state = null)
        {
            string working = raw ?? "/";
            int queryIndex = working.IndexOf('?');
            string pathPart = queryIndex >= 0 ? working.Substring(0, queryIndex) : working;
            string queryPart = queryIndex >= 0 && queryIndex + 1 < working.Length
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
    }
}
