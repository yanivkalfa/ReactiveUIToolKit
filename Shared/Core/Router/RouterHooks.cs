using System;
using System.Collections.Generic;
using ReactiveUITK.Core;

namespace ReactiveUITK.Router
{
    public static class RouterHooks
    {
        public static RouterState UseRouter()
        {
            // DEBUG UNCONDITIONAL
             UnityEngine.Debug.Log("[RouterHooks] UseRouter called (Unconditional)");
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
            return Hooks.UseContext<RouteMatch>(RouterContextKeys.RouteMatch)?.Parameters
                ?? RouterContextKeys.EmptyParams;
        }

        public static RouteMatch UseRouteMatch()
        {
            return Hooks.UseContext<RouteMatch>(RouterContextKeys.RouteMatch);
        }

        public static RouterNavigateHandler UseNavigate(bool replace = false)
        {
            var router = UseRouter();
            var routeMatch =
                Hooks.UseContext<RouteMatch>(RouterContextKeys.RouteMatch)
                ?? RouteMatch.CreateRoot(router?.Location?.Path ?? "/");
            var routeEntry = RouteContextEntryHelper.ResolveCurrentEntry();
            string navigationBase = routeEntry?.NavigationBase ?? routeMatch?.Pattern;
            if (router == null)
            {
                return (_, __) => false;
            }

            return (path, state) =>
            {
                string target;
                if (string.IsNullOrEmpty(path))
                {
                    target = navigationBase ?? "/";
                }
                else if (path.StartsWith("/"))
                {
                    target = RouterPath.Normalize(path);
                }
                else
                {
                    target = RouterPath.Combine(navigationBase ?? "/", path);
                }
                return replace
                    ? router.Replace?.Invoke(target, state) ?? false
                    : router.Navigate?.Invoke(target, state) ?? false;
            };
        }

        public static string UseNavigationBase()
        {
            var routeEntry = RouteContextEntryHelper.ResolveCurrentEntry();
            return routeEntry?.NavigationBase ?? "/";
        }

        public static RouterGoHandler UseGo()
        {
            var router = UseRouter();
            if (router == null)
            {
                return _ => false;
            }
            return delta => router.Go?.Invoke(delta) ?? false;
        }

        public static bool UseCanGo(int delta)
        {
            var router = UseRouter();
            return router?.CanGo?.Invoke(delta) ?? false;
        }

        public static void UseBlocker(
            Func<RouterLocation, RouterLocation, bool> blocker,
            bool enabled = true
        )
        {
            var router = UseRouter();
            Hooks.UseEffect(
                () =>
                {
                    if (!enabled || router?.RegisterBlocker == null || blocker == null)
                    {
                        return null;
                    }
                    var subscription = router.RegisterBlocker(blocker);
                    return () => subscription?.Dispose();
                },
                // Depend only on router + enabled so that we
                // register/unregister when those change. The blocker
                // delegate is free to close over local state.
                new object[] { router, enabled }
            );
        }
    }
}
