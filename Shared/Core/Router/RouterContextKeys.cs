using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ReactiveUITK.Router
{
    internal static class RouterContextKeys
    {
        public const string RouterState = "__router_state";
        public const string RouteMatch = "__router_route_match";
        public const string RoutePattern = "__router_route_pattern";
        public const string RouteContextEntry = "__router_route_context_entry";

        /// <summary>
        /// Identity stamp of the <see cref="RouterFunc"/> instance that owns
        /// the surrounding <see cref="RouterState"/>.  Used to distinguish
        /// "my own previous render's published value" (legal) from "an
        /// ancestor Router" (illegal nesting) in the context-propagation model.
        /// </summary>
        public const string RouterOwner = "__router_owner";

        /// <summary>
        /// VirtualNode published by a parent layout route for its descendant
        /// <c>&lt;Outlet/&gt;</c> to render.  Null when no child route is active.
        /// </summary>
        public const string OutletElement = "__router_outlet_element";

        /// <summary>
        /// Arbitrary value handed from a parent layout route down to its
        /// outlet's subtree via <see cref="RouterHooks.UseOutletContext{T}"/>.
        /// </summary>
        public const string OutletContext = "__router_outlet_context";

        /// <summary>
        /// Ordered chain of <see cref="RouteMatch"/> instances from root
        /// router → current route, surfaced via <see cref="RouterHooks.UseMatches"/>.
        /// </summary>
        public const string MatchChain = "__router_match_chain";

        internal static IReadOnlyDictionary<string, string> EmptyParams { get; } =
            new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
    }
}
