using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ReactiveUITK.Router
{
    internal static class RouterContextKeys
    {
        public const string RouterState = "__router_state";
        public const string RouteMatch = "__router_route_match";

        internal static IReadOnlyDictionary<string, string> EmptyParams { get; } =
            new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
    }
}
