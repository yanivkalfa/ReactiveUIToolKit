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

        // ── Phase 3 — DX surface aligned with React Router v6 ────────────────

        /// <summary>
        /// Returns the typed value passed by the closest enclosing
        /// <c>&lt;Outlet context={…}/&gt;</c>.  Returns <c>default(T)</c>
        /// when no outlet context is in scope or when the type does not match.
        /// </summary>
        public static T UseOutletContext<T>()
        {
            var raw = Hooks.UseContext<object>(RouterContextKeys.OutletContext);
            return raw is T typed ? typed : default;
        }

        /// <summary>
        /// Returns the ordered chain of <see cref="RouteMatch"/> entries from
        /// the root router down to the current route — useful for breadcrumbs
        /// and analytics (RR's <c>useMatches()</c> equivalent).
        /// </summary>
        public static IReadOnlyList<RouteMatch> UseMatches()
        {
            return Hooks.UseContext<IReadOnlyList<RouteMatch>>(RouterContextKeys.MatchChain)
                ?? Array.Empty<RouteMatch>();
        }

        /// <summary>
        /// Resolves <paramref name="to"/> against the current route's
        /// navigation base, returning the absolute path that
        /// <see cref="UseNavigate(bool)"/> would dispatch.  Mirrors RR's
        /// <c>useResolvedPath</c>.
        /// </summary>
        public static string UseResolvedPath(string to)
        {
            var routeEntry = RouteContextEntryHelper.ResolveCurrentEntry();
            string navigationBase = routeEntry?.NavigationBase ?? "/";
            if (string.IsNullOrEmpty(to))
            {
                return navigationBase;
            }
            if (to.StartsWith("/"))
            {
                return RouterPath.Normalize(to);
            }
            return RouterPath.Combine(navigationBase, to);
        }

        /// <summary>
        /// Returns a tuple of (current query parameters, setter that pushes
        /// a new query string while preserving the path).  Mirrors RR's
        /// <c>useSearchParams</c>.  The setter reuses the current location's
        /// path component and replaces only the query.
        /// </summary>
        public static (IReadOnlyDictionary<string, string> Query, Action<IReadOnlyDictionary<string, string>, bool> Set) UseSearchParams()
        {
            var router = UseRouter();
            var current = router?.Location?.Query ?? RouterContextKeys.EmptyParams;
            string currentPath = router?.Location?.Path ?? "/";
            void Set(IReadOnlyDictionary<string, string> next, bool replace)
            {
                if (router == null)
                {
                    return;
                }
                string qs = RouterPath.BuildQuery(next);
                string target = string.IsNullOrEmpty(qs) ? currentPath : currentPath + "?" + qs;
                if (replace)
                {
                    router.Replace?.Invoke(target, router.Location?.State);
                }
                else
                {
                    router.Navigate?.Invoke(target, router.Location?.State);
                }
            }
            return (current, Set);
        }

        /// <summary>
        /// Convenience wrapper that mirrors RR's <c>usePrompt</c> — registers
        /// a blocker that returns <c>true</c> (cancel navigation) whenever
        /// <paramref name="when"/> is <c>true</c>.  The <paramref name="message"/>
        /// argument is currently ignored at runtime (UITKX has no host dialog
        /// surface) but is retained for parity and so applications can opt to
        /// log it themselves from a custom blocker.
        /// </summary>
        public static void UsePrompt(bool when, string message = null)
        {
            UseBlocker(
                blocker: (_, __) =>
                {
                    if (when && !string.IsNullOrEmpty(message))
                    {
#if UNITY_EDITOR
                        UnityEngine.Debug.LogWarning("[Router prompt] " + message);
#endif
                    }
                    return when; // true = cancel transition
                },
                enabled: when
            );
        }

        /// <summary>
        /// Options bag accepted by the overload of <see cref="UseNavigate(NavigateOptions)"/>.
        /// </summary>
        public readonly struct NavigateOptions
        {
            public NavigateOptions(bool replace = false, object state = null)
            {
                Replace = replace;
                State = state;
            }

            public bool Replace { get; }
            public object State { get; }
        }

        /// <summary>
        /// Options-bag overload of <see cref="UseNavigate(bool)"/>.  Returns a
        /// path-only navigator pre-bound to the supplied options (state and
        /// replace flag).
        /// </summary>
        public static Action<string> UseNavigate(NavigateOptions options)
        {
            var inner = UseNavigate(options.Replace);
            object state = options.State;
            return path => inner?.Invoke(path, state);
        }
    }
}
