using System;
using System.Collections.Generic;
using ReactiveUITK.Core;

namespace ReactiveUITK.Router
{
    public static class RouterHooks
    {
        public static RouterState UseRouter()
        {
            return Hooks.UseContext<RouterState>(RouterContextKeys.RouterState);
        }

        public static RouterLocation UseLocationInfo()
        {
            return UseRouter()?.Location;
        }

        public static string UseLocation()
        {
            return UseLocationInfo()?.Path ?? "/";
        }

        public static IReadOnlyDictionary<string, string> UseQuery()
        {
            return UseLocationInfo()?.Query ?? RouterContextKeys.EmptyParams;
        }

        public static object UseNavigationState()
        {
            return UseLocationInfo()?.State;
        }

        public static IReadOnlyDictionary<string, string> UseParams()
        {
            return Hooks
                    .UseContext<RouteMatch>(RouterContextKeys.RouteMatch)
                    ?.Parameters
                ?? RouterContextKeys.EmptyParams;
        }

        public static RouteMatch UseRouteMatch()
        {
            return Hooks.UseContext<RouteMatch>(RouterContextKeys.RouteMatch);
        }

        public static RouterNavigateHandler UseNavigate(bool replace = false)
        {
            var router = UseRouter();
            if (router == null)
            {
                return (_, __) => false;
            }

            return (path, state) =>
            {
                string target = string.IsNullOrWhiteSpace(path) ? "/" : path;
                return replace
                    ? router.Replace?.Invoke(target, state) ?? false
                    : router.Navigate?.Invoke(target, state) ?? false;
            };
        }
    }
}
