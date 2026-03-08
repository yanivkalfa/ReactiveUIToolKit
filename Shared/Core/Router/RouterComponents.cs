using System;
using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Core.Diagnostics;
using ReactiveUITK.Props.Typed;
using UnityEngine;

namespace ReactiveUITK.Router
{
    public static class RouterFunc
    {
        public static VirtualNode Render(
            IProps rawProps,
            IReadOnlyList<VirtualNode> children
        )
        {
            var p = rawProps as RouterFuncProps;
            var providedHistory = p?.History;
            string initialPath = p?.InitialPath ?? "/";

            var resolvedHistory = Hooks.UseMemo(
                () => providedHistory ?? new MemoryHistory(initialPath),
                new object[] { providedHistory, initialPath }
            );

            var (location, setLocation) = Hooks.UseState(
                resolvedHistory?.Location ?? RouterPath.Parse("/")
            );
            Hooks.UseEffect(
                () =>
                {
                    if (resolvedHistory == null)
                    {
                        return null;
                    }
                    var subscription = resolvedHistory.Listen(loc => setLocation(loc));
                    return () => subscription?.Dispose();
                },
                new object[] { resolvedHistory }
            );

            var routerState = Hooks.UseMemo(
                () =>
                    resolvedHistory != null
                        ? new RouterState(
                            location,
                            (path, state) =>
                            {
                                resolvedHistory.Push(path, state);
                                return true;
                            },
                            (path, state) =>
                            {
                                resolvedHistory.Replace(path, state);
                                return true;
                            },
                            delta =>
                            {
                                if (!resolvedHistory.CanGo(delta))
                                {
                                    return false;
                                }
                                resolvedHistory.Go(delta);
                                return true;
                            },
                            delta => resolvedHistory.CanGo(delta),
                            blocker => resolvedHistory.RegisterBlocker(blocker)
                        )
                        : new RouterState(
                            location,
                            (_, __) => false,
                            (_, __) => false,
                            _ => false,
                            _ => false,
                            _ => Disposable.Empty
                        ),
                new object[] { resolvedHistory, location }
            );

            Hooks.ProvideContext(RouterContextKeys.RouterState, routerState);
            var rootMatch = RouteMatch.CreateRoot(location?.Path ?? "/");
            Hooks.ProvideContext(RouterContextKeys.RouteMatch, rootMatch);
            Hooks.ProvideContext(RouterContextKeys.RoutePattern, "/");
            var rootEntry = new RouteContextEntry(rootMatch, "/", null, HookContext.Current);
            Hooks.ProvideContext(RouterContextKeys.RouteContextEntry, rootEntry);

            return RouterRenderUtils.Fragment(children);
        }
    }

    public static class RouteFunc
    {
        public static VirtualNode Render(
            IProps rawProps,
            IReadOnlyList<VirtualNode> children
        )
        {
            var router = RouterHooks.UseRouter();
            if (router == null)
            {
                return null;
            }

            var p = rawProps as RouteFuncProps;
            string path = p?.Path;
            bool exact = p?.Exact ?? false;
            var elementObj = p?.Element;
            var renderFunc = p?.RenderFunc;
            var parentEntry = RouteContextEntryHelper.ResolveCurrentEntry();
            string parentNavigationBase = parentEntry?.NavigationBase ?? "/";
            var parentMatch = parentEntry?.Match ?? RouteMatch.CreateRoot(router.Location.Path);
            string parentPattern = parentMatch?.Pattern ?? "/";

            string resolvedPath = path;
            if (!string.IsNullOrEmpty(path))
            {
                resolvedPath = RouterPath.Combine(parentMatch?.Pattern ?? "/", path);
            }

            var match = Hooks.UseMemo(
                () => RouteMatcher.Match(router.Location.Path, resolvedPath, exact, parentMatch),
                new object[] { router.Location.Path, resolvedPath, exact, parentMatch }
            );

            if (match == null)
            {
                return null;
            }

            Hooks.ProvideContext(RouterContextKeys.RouteMatch, match);
            string providedPattern = string.IsNullOrEmpty(resolvedPath)
                ? match?.Pattern ?? parentPattern
                : resolvedPath;
            Hooks.ProvideContext(RouterContextKeys.RoutePattern, providedPattern);
            string baseSeed = string.IsNullOrEmpty(resolvedPath)
                ? parentNavigationBase
                : resolvedPath;
            string navigationBase = RouterPath.Combine(baseSeed ?? "/", string.Empty);

            var routeEntry = Hooks.UseMemo(
                () =>
                    new RouteContextEntry(match, navigationBase, parentEntry, HookContext.Current),
                new object[] { match, navigationBase, parentEntry, HookContext.Current }
            );
            Hooks.ProvideContext(RouterContextKeys.RouteContextEntry, routeEntry);

            if (renderFunc != null)
            {
                return renderFunc(match);
            }

            if (elementObj is VirtualNode vnode)
            {
                return vnode;
            }

            return RouterRenderUtils.Fragment(children);
        }
    }

    public static class LinkFunc
    {
        public static VirtualNode Render(
            IProps rawProps,
            IReadOnlyList<VirtualNode> children
        )
        {
            var router = RouterHooks.UseRouter();
            if (router == null)
            {
                return null;
            }
            var routeMatch =
                Hooks.UseContext<RouteMatch>(RouterContextKeys.RouteMatch)
                ?? RouteMatch.CreateRoot(router.Location?.Path ?? "/");
            var routeEntry = RouteContextEntryHelper.ResolveCurrentEntry();

            var p = rawProps as LinkFuncProps;
            string to = p?.To ?? "/";
            string label = p?.Label ?? to;
            bool replace = p?.Replace ?? false;
            Style style = p?.Style;
            object stateObj = p?.State;
            string navigationBase = routeEntry?.NavigationBase ?? routeMatch?.Pattern;

            Action navigate = () =>
            {
                string target;
                if (string.IsNullOrEmpty(to))
                {
                    target = navigationBase ?? "/";
                }
                else if (to.StartsWith("/"))
                {
                    target = RouterPath.Normalize(to);
                }
                else
                {
                    target = RouterPath.Combine(navigationBase ?? "/", to);
                }
                if (replace)
                {
                    router.Replace?.Invoke(target, stateObj);
                }
                else
                {
                    router.Navigate?.Invoke(target, stateObj);
                }
            };

            var button = V.Button(
                new ButtonProps
                {
                    Text = label,
                    Style = style,
                    OnClick = _ => navigate(),
                }
            );
            return button;
        }
    }

    public sealed class RouterFuncProps : IProps
    {
        public IRouterHistory History { get; set; }
        public string InitialPath { get; set; }
    }

    public sealed class RouteFuncProps : IProps
    {
        public string Path { get; set; }
        public bool Exact { get; set; }
        public VirtualNode Element { get; set; }
        public Func<RouteMatch, VirtualNode> RenderFunc { get; set; }
    }

    public sealed class LinkFuncProps : IProps
    {
        public string To { get; set; }
        public string Label { get; set; }
        public bool Replace { get; set; }
        public Style Style { get; set; }
        public object State { get; set; }
    }

    internal static class RouterRenderUtils
    {
        public static VirtualNode Fragment(IReadOnlyList<VirtualNode> children)
        {
            if (children == null || children.Count == 0)
            {
                return V.Fragment();
            }

            // Optimization / compatibility: if there is only a single child,
            // return it directly instead of wrapping in an extra Fragment.
            // This avoids exercising Fragment handling in Fiber for the
            // common Router case where there is exactly one root element.
            if (children.Count == 1)
            {
                return children[0];
            }

            var buffer = new VirtualNode[children.Count];
            for (int i = 0; i < children.Count; i++)
            {
                buffer[i] = children[i];
            }
            return V.Fragment(null, buffer);
        }
    }

    internal sealed class RouteContextEntry
    {
        public static readonly RouteContextEntry Root = new RouteContextEntry(
            RouteMatch.CreateRoot("/"),
            "/",
            null,
            owner: null
        );

        public RouteContextEntry(
            RouteMatch match,
            string navigationBase,
            RouteContextEntry parent,
            FunctionComponentState owner
        )
        {
            Match = match;
            NavigationBase = string.IsNullOrEmpty(navigationBase) ? "/" : navigationBase;
            Parent = parent;
            Owner = owner;
        }

        public RouteMatch Match { get; }
        public string NavigationBase { get; }
        public RouteContextEntry Parent { get; }
        public FunctionComponentState Owner { get; }
    }

    internal static class RouteContextEntryHelper
    {
        public static RouteContextEntry ResolveCurrentEntry()
        {
            var entry =
                Hooks.UseContext<RouteContextEntry>(RouterContextKeys.RouteContextEntry)
                ?? RouteContextEntry.Root;
            var state = HookContext.Current;
            if (entry != null && ReferenceEquals(entry.Owner, state))
            {
                return entry.Parent ?? RouteContextEntry.Root;
            }
            return entry ?? RouteContextEntry.Root;
        }
    }

    internal sealed class Disposable : IDisposable
    {
        public static readonly IDisposable Empty = new Disposable();

        public void Dispose() { }
    }
}
