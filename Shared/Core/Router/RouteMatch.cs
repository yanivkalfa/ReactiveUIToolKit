using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ReactiveUITK.Router
{
    public sealed class RouteMatch
    {
        private static readonly IReadOnlyDictionary<string, string> EmptyParameters =
            new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());

        public RouteMatch(
            string location,
            string pattern,
            IReadOnlyDictionary<string, string> parameters
        )
        {
            Location = RouterPath.Normalize(location);
            Pattern = string.IsNullOrEmpty(pattern) ? "/" : RouterPath.Normalize(pattern);
            Parameters = parameters ?? EmptyParameters;
        }

        public string Location { get; }

        public string Pattern { get; }

        public IReadOnlyDictionary<string, string> Parameters { get; }

        public static RouteMatch CreateRoot(string location)
        {
            return new RouteMatch(location, "/", EmptyParameters);
        }
    }
}
