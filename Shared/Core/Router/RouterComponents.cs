using System;
using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;

namespace ReactiveUITK.Router
{
    public static class RouterFunc
    {
        public static VirtualNode Render(
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children
        )
        {
            props ??= new Dictionary<string, object>();
            props.TryGetValue("history", out var historyObj);
            props.TryGetValue("initialPath", out var initialPathObj);
            var providedHistory = historyObj as IRouterHistory;
            string initialPath = initialPathObj as string ?? "/";

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

            RouterState routerState =
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
                    );

            Hooks.ProvideContext(RouterContextKeys.RouterState, routerState);
            Hooks.ProvideContext(
                RouterContextKeys.RouteMatch,
                RouteMatch.CreateRoot(location?.Path ?? "/")
            );

            return RouterRenderUtils.Fragment(children);
        }
    }

    public static class RouteFunc
    {
        public static VirtualNode Render(
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children
        )
        {
            var router = RouterHooks.UseRouter();
            if (router == null)
            {
                return null;
            }

            props ??= new Dictionary<string, object>();
            props.TryGetValue("path", out var pathObj);
            props.TryGetValue("exact", out var exactObj);
            props.TryGetValue("element", out var elementObj);
            props.TryGetValue("render", out var renderObj);

            string path = pathObj as string;
            bool exact = exactObj is bool flag && flag;

            var parentMatch =
                Hooks.UseContext<RouteMatch>(RouterContextKeys.RouteMatch)
                ?? RouteMatch.CreateRoot(router.Location.Path);

            var match = RouteMatcher.Match(router.Location.Path, path, exact, parentMatch);
            if (match == null)
            {
                return null;
            }

            Hooks.ProvideContext(RouterContextKeys.RouteMatch, match);

            if (renderObj is Func<RouteMatch, VirtualNode> renderFunc)
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
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children
        )
        {
            var router = RouterHooks.UseRouter();
            if (router == null)
            {
                return null;
            }

            props ??= new Dictionary<string, object>();
            props.TryGetValue("to", out var toObj);
            props.TryGetValue("label", out var labelObj);
            props.TryGetValue("replace", out var replaceObj);
            props.TryGetValue("style", out var styleObj);
            props.TryGetValue("state", out var stateObj);

            string to = toObj as string ?? "/";
            string label = labelObj as string ?? to;
            bool replace = replaceObj is bool replaceFlag && replaceFlag;
            Style style = styleObj as Style;

            Action navigate = () =>
            {
                string target = string.IsNullOrWhiteSpace(to) ? "/" : to;
                if (replace)
                {
                    router.Replace?.Invoke(target, stateObj);
                }
                else
                {
                    router.Navigate?.Invoke(target, stateObj);
                }
            };

            return V.Button(
                new ButtonProps
                {
                    Text = label,
                    Style = style,
                    OnClick = navigate,
                }
            );
        }
    }

    internal static class RouterRenderUtils
    {
        public static VirtualNode Fragment(IReadOnlyList<VirtualNode> children)
        {
            if (children == null || children.Count == 0)
            {
                return V.Fragment();
            }

            var buffer = new VirtualNode[children.Count];
            for (int i = 0; i < children.Count; i++)
            {
                buffer[i] = children[i];
            }
            return V.Fragment(null, buffer);
        }
    }

    internal sealed class Disposable : IDisposable
    {
        public static readonly IDisposable Empty = new Disposable();

        public void Dispose() { }
    }
}
