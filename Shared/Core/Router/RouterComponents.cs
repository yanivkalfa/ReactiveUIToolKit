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
        public static VirtualNode Render(IProps rawProps, IReadOnlyList<VirtualNode> children)
        {
            // ── Phase 0.2: nested-Router hard error ──────────────────────────
            // Two routers in the same component tree would compete for the
            // same context keys and produce undefined behaviour.  RR forbids
            // this; we follow suit with an explicit, actionable exception.
            //
            // Subtlety: Hooks.UseContext reads from the *alternate* fiber, so
            // on a re-render of the root Router it sees the value the Router
            // itself published last frame — which is NOT a nesting violation.
            // We disambiguate via an owner-stamp that pairs with the
            // RouterState publication below.
            var existingOwner =
                Hooks.UseContext<FunctionComponentState>(RouterContextKeys.RouterOwner);
            if (
                existingOwner != null
                && !ReferenceEquals(existingOwner, HookContext.Current)
            )
            {
                throw new InvalidOperationException(
                    "UITKX <Router> cannot be nested inside another <Router>. "
                        + "Use a single root <Router> and compose <Route>s underneath it. "
                        + "If you need a sub-router for a portion of the tree, use <Routes> instead."
                );
            }

            var p = rawProps as RouterFuncProps;
            var providedHistory = p?.History;
            string initialPath = p?.InitialPath ?? "/";
            string basename = string.IsNullOrEmpty(p?.Basename) ? "/" : p.Basename;

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
                                resolvedHistory.Push(
                                    RouterPath.WithBasename(path, basename),
                                    state
                                );
                                return true;
                            },
                            (path, state) =>
                            {
                                resolvedHistory.Replace(
                                    RouterPath.WithBasename(path, basename),
                                    state
                                );
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
                            blocker => resolvedHistory.RegisterBlocker(blocker),
                            basename
                        )
                        : new RouterState(
                            location,
                            (_, __) => false,
                            (_, __) => false,
                            _ => false,
                            _ => false,
                            _ => Disposable.Empty,
                            basename
                        ),
                new object[] { resolvedHistory, location, basename }
            );

            Hooks.ProvideContext(RouterContextKeys.RouterState, routerState);
            // Stamp ourselves as the owner so subsequent re-renders of this
            // same Router don't trip the nested-Router guard above.
            Hooks.ProvideContext(RouterContextKeys.RouterOwner, HookContext.Current);
            string visiblePath = RouterPath.StripBasename(
                location?.Path ?? "/",
                basename
            );
            var rootMatch = RouteMatch.CreateRoot(visiblePath);
            Hooks.ProvideContext(RouterContextKeys.RouteMatch, rootMatch);
            Hooks.ProvideContext(RouterContextKeys.RoutePattern, "/");
            var rootEntry = new RouteContextEntry(rootMatch, "/", null, HookContext.Current);
            Hooks.ProvideContext(RouterContextKeys.RouteContextEntry, rootEntry);
            Hooks.ProvideContext(
                RouterContextKeys.MatchChain,
                (IReadOnlyList<RouteMatch>)new RouteMatch[] { rootMatch }
            );

            return RouterRenderUtils.Fragment(children);
        }
    }

    public static class RouteFunc
    {
        public static VirtualNode Render(IProps rawProps, IReadOnlyList<VirtualNode> children)
        {
            var router = RouterHooks.UseRouter();
            if (router == null)
            {
                return null;
            }

            var p = rawProps as RouteFuncProps;
            string path = p?.Path;
            bool exact = p?.Exact ?? false;
            bool isIndex = p?.Index ?? false;
            bool caseSensitive = p?.CaseSensitive ?? false;
            var elementObj = p?.Element;
            var renderFunc = p?.RenderFunc;

            // ── Runtime validation ────────────────────────────────────────────
            // Index routes are pinned to their parent's pattern; specifying a
            // path on an index route is meaningless and almost always a bug.
            // (Phase 4 analyzer parity: surfaced as a runtime exception so the
            // failure is loud instead of silently degrading to a non-match.)
            if (isIndex && !string.IsNullOrEmpty(path))
            {
                throw new InvalidOperationException(
                    "UITKX <Route index> cannot also declare a 'path'. "
                        + "Index routes always match the parent route's pattern exactly. "
                        + "Either drop 'path=\"...\"' or remove 'index'."
                );
            }
            var parentEntry = RouteContextEntryHelper.ResolveCurrentEntry();
            string parentNavigationBase = parentEntry?.NavigationBase ?? "/";
            var parentMatch = parentEntry?.Match ?? RouteMatch.CreateRoot(router.Location.Path);
            string parentPattern = parentMatch?.Pattern ?? "/";

            string resolvedPath;
            if (isIndex)
            {
                // Index routes share their parent's pattern but match exactly.
                resolvedPath = parentPattern;
            }
            else if (string.IsNullOrEmpty(path))
            {
                resolvedPath = parentPattern;
            }
            else
            {
                resolvedPath = RouterPath.Combine(parentPattern, path);
            }

            bool effectiveExact = exact || isIndex;
            var match = Hooks.UseMemo(
                () =>
                    RouteMatcher.Match(
                        router.Location.Path,
                        resolvedPath,
                        effectiveExact,
                        parentMatch,
                        caseSensitive
                    ),
                new object[]
                {
                    router.Location.Path,
                    resolvedPath,
                    effectiveExact,
                    parentMatch,
                    caseSensitive,
                }
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

            // Extend the parent's match chain with our match for useMatches().
            var parentChain =
                Hooks.UseContext<IReadOnlyList<RouteMatch>>(RouterContextKeys.MatchChain);
            var ourChain = Hooks.UseMemo(
                () => RouterRenderUtils.AppendChain(parentChain, match),
                new object[] { parentChain, match }
            );
            Hooks.ProvideContext(RouterContextKeys.MatchChain, ourChain);

            // ── Phase 1.1: layout-route co-existence of Element + child Routes ──
            // If we have any nested <Route> children, rank them and publish the
            // best match into the OutletElement context so that descendant
            // <Outlet/>s can render it.  When there are no nested Routes, we
            // behave exactly like before (Element wins, then children, then
            // null) — full backward compatibility.
            var nestedRouteNode = SelectNestedRouteForOutlet(
                children,
                router.Location.Path,
                match,
                resolvedPath
            );
            if (nestedRouteNode != null)
            {
                Hooks.ProvideContext(RouterContextKeys.OutletElement, nestedRouteNode);
            }

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

        /// <summary>
        /// Walks <paramref name="children"/> for nested <c>&lt;Route&gt;</c>
        /// VirtualNodes, ranks them with <see cref="RouteRanker"/>, and
        /// returns the best-matching one (or <c>null</c>).  The returned node
        /// is the original VirtualNode itself — Outlet renders it as-is so
        /// that the inner RouteFunc can re-establish its own context chain.
        /// </summary>
        private static VirtualNode SelectNestedRouteForOutlet(
            IReadOnlyList<VirtualNode> children,
            string currentLocation,
            RouteMatch parentMatch,
            string parentResolvedPath
        )
        {
            if (children == null || children.Count == 0)
            {
                return null;
            }

            List<RouteRanker.Candidate> candidates = null;
            int idx = 0;
            CollectRouteCandidates(
                children,
                parentResolvedPath,
                ref candidates,
                ref idx
            );
            if (candidates == null || candidates.Count == 0)
            {
                return null;
            }
            var picked = RouteRanker.Pick(candidates, currentLocation, parentMatch);
            return picked?.Candidate.Node;
        }

        private static void CollectRouteCandidates(
            IReadOnlyList<VirtualNode> nodes,
            string parentResolvedPath,
            ref List<RouteRanker.Candidate> candidates,
            ref int declarationCounter
        )
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                var child = nodes[i];
                if (child == null)
                {
                    continue;
                }
                if (
                    child.NodeType == VirtualNodeType.Fragment
                    && child.Children != null
                    && child.Children.Count > 0
                )
                {
                    CollectRouteCandidates(
                        child.Children,
                        parentResolvedPath,
                        ref candidates,
                        ref declarationCounter
                    );
                    continue;
                }
                if (child.NodeType != VirtualNodeType.FunctionComponent)
                {
                    continue;
                }
                if (!(child.TypedProps is RouteFuncProps rp))
                {
                    continue;
                }
                string childPath = rp.Path;
                bool isIndex = rp.Index;
                string resolved;
                if (isIndex || string.IsNullOrEmpty(childPath))
                {
                    resolved = parentResolvedPath ?? "/";
                }
                else
                {
                    resolved = RouterPath.Combine(parentResolvedPath ?? "/", childPath);
                }
                candidates ??= new List<RouteRanker.Candidate>();
                candidates.Add(
                    new RouteRanker.Candidate(
                        declarationCounter++,
                        resolved,
                        isIndex,
                        rp.Exact,
                        rp.CaseSensitive,
                        child
                    )
                );
            }
        }
    }

    // ── Outlet ──────────────────────────────────────────────────────────────
    public static class OutletFunc
    {
        public static VirtualNode Render(IProps rawProps, IReadOnlyList<VirtualNode> children)
        {
            // Outlet must live inside a <Router>.  Outside one we render
            // nothing so that misplaced outlets are visible-but-harmless.
            if (RouterHooks.UseRouter() == null)
            {
#if UNITY_EDITOR
                UnityEngine.Debug.LogWarning(
                    "UITKX <Outlet/> rendered outside any <Router>. The outlet will render nothing."
                );
#endif
                return null;
            }

            var p = rawProps as OutletFuncProps;
            // Allow a parent layout to pipe a typed value down to the outlet
            // via UseOutletContext<T>().
            if (p?.Context != null)
            {
                Hooks.ProvideContext(RouterContextKeys.OutletContext, p.Context);
            }

            var slot =
                Hooks.UseContext<VirtualNode>(RouterContextKeys.OutletElement);
            if (slot != null)
            {
                return slot;
            }
            // No nested route resolved → render outlet's own children as the
            // fallback (RR does the same for default outlets).
            return RouterRenderUtils.Fragment(children);
        }
    }

    // ── Routes (first-match-wins selector) ─────────────────────────────────
    public static class RoutesFunc
    {
        public static VirtualNode Render(IProps rawProps, IReadOnlyList<VirtualNode> children)
        {
            var router = RouterHooks.UseRouter();
            if (router == null)
            {
                return null;
            }

            var parentEntry = RouteContextEntryHelper.ResolveCurrentEntry();
            var parentMatch =
                parentEntry?.Match ?? RouteMatch.CreateRoot(router.Location.Path);
            string parentPattern = parentMatch?.Pattern ?? "/";

            List<RouteRanker.Candidate> candidates = null;
            int idx = 0;
            CollectTopLevelRoutes(
                children,
                parentPattern,
                ref candidates,
                ref idx
            );
            if (candidates == null || candidates.Count == 0)
            {
                return null;
            }
            var picked = Hooks.UseMemo(
                () => RouteRanker.Pick(candidates, router.Location.Path, parentMatch),
                new object[] { router.Location.Path, parentMatch, candidates.Count }
            );
            return picked?.Candidate.Node;
        }

        private static void CollectTopLevelRoutes(
            IReadOnlyList<VirtualNode> nodes,
            string parentPattern,
            ref List<RouteRanker.Candidate> candidates,
            ref int declarationCounter
        )
        {
            if (nodes == null)
            {
                return;
            }
            for (int i = 0; i < nodes.Count; i++)
            {
                var child = nodes[i];
                if (child == null)
                {
                    continue;
                }
                if (
                    child.NodeType == VirtualNodeType.Fragment
                    && child.Children != null
                    && child.Children.Count > 0
                )
                {
                    CollectTopLevelRoutes(
                        child.Children,
                        parentPattern,
                        ref candidates,
                        ref declarationCounter
                    );
                    continue;
                }
                if (child.NodeType != VirtualNodeType.FunctionComponent)
                {
                    continue;
                }
                if (!(child.TypedProps is RouteFuncProps rp))
                {
                    continue;
                }
                string resolved;
                if (rp.Index || string.IsNullOrEmpty(rp.Path))
                {
                    resolved = parentPattern;
                }
                else
                {
                    resolved = RouterPath.Combine(parentPattern, rp.Path);
                }
                candidates ??= new List<RouteRanker.Candidate>();
                candidates.Add(
                    new RouteRanker.Candidate(
                        declarationCounter++,
                        resolved,
                        rp.Index,
                        rp.Exact,
                        rp.CaseSensitive,
                        child
                    )
                );
            }
        }
    }

    // ── NavLink ────────────────────────────────────────────────────────────
    public static class NavLinkFunc
    {
        public static VirtualNode Render(IProps rawProps, IReadOnlyList<VirtualNode> children)
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

            var p = rawProps as NavLinkFuncProps;
            string to = p?.To ?? "/";
            string label = p?.Label ?? to;
            bool replace = p?.Replace ?? false;
            bool end = p?.End ?? false;
            bool caseSensitive = p?.CaseSensitive ?? false;
            Style style = p?.Style;
            Style activeStyle = p?.ActiveStyle;
            object stateObj = p?.State;
            string navigationBase = routeEntry?.NavigationBase ?? routeMatch?.Pattern;

            string resolvedTarget;
            if (string.IsNullOrEmpty(to))
            {
                resolvedTarget = navigationBase ?? "/";
            }
            else if (to.StartsWith("/"))
            {
                resolvedTarget = RouterPath.Normalize(to);
            }
            else
            {
                resolvedTarget = RouterPath.Combine(navigationBase ?? "/", to);
            }

            bool isActive = NavLinkIsActive(
                router.Location?.Path ?? "/",
                resolvedTarget,
                end,
                caseSensitive
            );

            Action navigate = () =>
            {
                if (replace)
                {
                    router.Replace?.Invoke(resolvedTarget, stateObj);
                }
                else
                {
                    router.Navigate?.Invoke(resolvedTarget, stateObj);
                }
            };

            Style finalStyle = isActive && activeStyle != null ? activeStyle : style;

            var button = V.Button(
                new ButtonProps
                {
                    Text = label,
                    Style = finalStyle,
                    OnClick = _ => navigate(),
                }
            );
            return button;
        }

        /// <summary>
        /// Returns true when the current location should activate a NavLink
        /// pointing at <paramref name="resolvedTarget"/>.  Mirrors RR's
        /// activation rules:
        /// <list type="bullet">
        /// <item><description><c>end == true</c> → exact match required.</description></item>
        /// <item><description>otherwise → location starts with target on a segment boundary.</description></item>
        /// <item><description><c>to == "/"</c> only activates on <c>"/"</c> exactly (special-case).</description></item>
        /// </list>
        /// </summary>
        internal static bool NavLinkIsActive(
            string currentLocation,
            string resolvedTarget,
            bool end,
            bool caseSensitive
        )
        {
            string normLoc = RouterPath.Normalize(currentLocation);
            string normTarget = RouterPath.Normalize(resolvedTarget);
            StringComparison cmp = caseSensitive
                ? StringComparison.Ordinal
                : StringComparison.OrdinalIgnoreCase;
            if (normTarget == "/")
            {
                return string.Equals(normLoc, "/", cmp);
            }
            if (string.Equals(normLoc, normTarget, cmp))
            {
                return true;
            }
            if (end)
            {
                return false;
            }
            // Prefix-with-segment-boundary check.
            return normLoc.Length > normTarget.Length
                && normLoc.StartsWith(normTarget, cmp)
                && normLoc[normTarget.Length] == '/';
        }
    }

    // ── Navigate (declarative redirect) ────────────────────────────────────
    public static class NavigateFunc
    {
        public static VirtualNode Render(IProps rawProps, IReadOnlyList<VirtualNode> children)
        {
            var p = rawProps as NavigateFuncProps;
            string to = p?.To ?? "/";
            bool replace = p?.Replace ?? true; // RR: <Navigate> defaults to replace
            object stateObj = p?.State;
            var navigate = RouterHooks.UseNavigate(replace);

            // Effect runs after commit so we don't navigate from inside render.
            Hooks.UseEffect(
                () =>
                {
                    navigate?.Invoke(to, stateObj);
                    return null;
                },
                new object[] { to, replace, stateObj }
            );
            return null;
        }
    }

    public static class LinkFunc
    {
        public static VirtualNode Render(IProps rawProps, IReadOnlyList<VirtualNode> children)
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
        /// <summary>
        /// Optional URL prefix that the router treats as the root.  Locations
        /// passed in/out are stripped of this prefix; navigation calls
        /// re-attach it transparently.  Defaults to <c>"/"</c>.
        /// </summary>
        public string Basename { get; set; }
    }

    public sealed class RouteFuncProps : IProps
    {
        public string Path { get; set; }
        public bool Exact { get; set; }
        public VirtualNode Element { get; set; }
        public Func<RouteMatch, VirtualNode> RenderFunc { get; set; }
        /// <summary>
        /// Index route — matches the parent pattern exactly (no extra segment).
        /// </summary>
        public bool Index { get; set; }
        /// <summary>
        /// When true, segment matching is case-sensitive (default: case-insensitive).
        /// </summary>
        public bool CaseSensitive { get; set; }
    }

    public sealed class LinkFuncProps : IProps
    {
        public string To { get; set; }
        public string Label { get; set; }
        public bool Replace { get; set; }
        public Style Style { get; set; }
        public object State { get; set; }
    }

    public sealed class OutletFuncProps : IProps
    {
        /// <summary>
        /// Optional value passed to descendants via
        /// <see cref="RouterHooks.UseOutletContext{T}"/>.
        /// </summary>
        public object Context { get; set; }
    }

    public sealed class NavLinkFuncProps : IProps
    {
        public string To { get; set; }
        public string Label { get; set; }
        public bool Replace { get; set; }
        public bool End { get; set; }
        public bool CaseSensitive { get; set; }
        public Style Style { get; set; }
        /// <summary>Style applied when the link's target matches the current location.</summary>
        public Style ActiveStyle { get; set; }
        public object State { get; set; }
    }

    public sealed class NavigateFuncProps : IProps
    {
        public string To { get; set; }
        /// <summary>Defaults to <c>true</c> — declarative redirects shouldn't grow history.</summary>
        public bool Replace { get; set; } = true;
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

        /// <summary>
        /// Returns a new immutable chain consisting of <paramref name="parent"/>
        /// followed by <paramref name="match"/>.  Stable-sharing-friendly:
        /// when <paramref name="parent"/> is null, returns a single-element
        /// list; never mutates the input.
        /// </summary>
        public static IReadOnlyList<RouteMatch> AppendChain(
            IReadOnlyList<RouteMatch> parent,
            RouteMatch match
        )
        {
            if (match == null)
            {
                return parent ?? Array.Empty<RouteMatch>();
            }
            if (parent == null || parent.Count == 0)
            {
                return new RouteMatch[] { match };
            }
            var arr = new RouteMatch[parent.Count + 1];
            for (int i = 0; i < parent.Count; i++)
            {
                arr[i] = parent[i];
            }
            arr[parent.Count] = match;
            return arr;
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
