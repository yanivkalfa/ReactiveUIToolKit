using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using ReactiveUITK.Router;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Samples.FunctionalComponents
{
    public static class RouterDemoFunc
    {
        private static readonly Style PaddedContainer = new Style
        {
            (StyleKeys.FlexGrow, 1f),
            (StyleKeys.FlexDirection, "column"),
            (StyleKeys.Padding, 12f),
        };

        private static readonly Style CardStyle = new Style
        {
            (StyleKeys.Padding, 10f),
            (StyleKeys.BorderColor, new Color32(210, 210, 210, 255)),
            (StyleKeys.BorderWidth, 1f),
            (StyleKeys.BorderRadius, 6f),
            (StyleKeys.MarginTop, 6f),
        };

        private static string DescribeQuery(IReadOnlyDictionary<string, string> query)
        {
            if (query == null || query.Count == 0)
            {
                return "(none)";
            }
            var parts = new List<string>(query.Count);
            foreach (var kv in query)
            {
                parts.Add($"{kv.Key}={kv.Value}");
            }
            return string.Join(", ", parts);
        }

        private static string DescribeState(object state)
        {
            if (state == null)
            {
                return "(none)";
            }
            if (state is string s)
            {
                return string.IsNullOrEmpty(s) ? "(empty string)" : s;
            }
            if (state is IReadOnlyDictionary<string, object> roDict)
            {
                return DescribeObjectDictionary(roDict);
            }
            if (state is IDictionary<string, object> dict)
            {
                return DescribeObjectDictionary(dict);
            }
            return state.ToString();
        }

        private static string DescribeObjectDictionary(
            IEnumerable<KeyValuePair<string, object>> dict
        )
        {
            if (dict == null)
            {
                return "(none)";
            }
            var parts = new List<string>();
            foreach (var kv in dict)
            {
                parts.Add($"{kv.Key}={kv.Value}");
            }
            return string.Join(", ", parts);
        }

        public static VirtualNode Render(
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children
        )
        {
            return V.Router(
                children: new[]
                {
                    V.VisualElement(
                        PaddedContainer,
                        null,
                        V.Text("Lightweight Router Demo"),
                        BuildNavigationBar(),
                        V.VisualElement(
                            CardStyle,
                            null,
                            V.Func(LocationBanner.Render),
                            V.Func(NavigatePanel.Render),
                            V.Func(QuickAccessPanel.Render)
                        ),
                        V.VisualElement(
                            CardStyle,
                            null,
                            V.Func(HistoryPanel.Render),
                            V.Func(NavigationGuardPanel.Render)
                        ),
                        V.VisualElement(
                            CardStyle,
                            null,
                            V.Route(
                                path: "/",
                                exact: true,
                                element: V.Text(
                                    "Landing route: use the navigation above to explore nested routing, params, and programmatic navigation."
                                )
                            ),
                            V.Route(
                                path: "/about",
                                element: V.Text(
                                    "About route: this is a static screen rendered whenever the path is /about."
                                )
                            ),
                            V.Route(
                                path: "/users",
                                exact: true,
                                element: V.Text(
                                    "Users route: pick a user ID from the quick-links or type one in the navigation panel."
                                )
                            ),
                            V.Route(
                                path: "/users/:id",
                                children: new[]
                                {
                                    V.VisualElement(
                                        new Style
                                        {
                                            (StyleKeys.MarginTop, 6f),
                                            (StyleKeys.FlexDirection, "column"),
                                        },
                                        null,
                                        V.Func(UserDetails.Render),
                                        V.Route(
                                            path: "/users/:id/details",
                                            element: V.Text(
                                                "Nested /details route matched for this user."
                                            )
                                        )
                                    ),
                                }
                            ),
                            V.Route(
                                path: "/settings/*",
                                children: new[] { V.Func(SettingsPanel.Render) }
                            ),
                            V.Route(
                                path: "*",
                                element: V.Text(
                                    "No matching route. Use the nav links above to continue."
                                )
                            )
                        )
                    ),
                }
            );
        }

        private static VirtualNode BuildNavigationBar()
        {
            return V.VisualElement(
                new Style { (StyleKeys.FlexDirection, "row"), (StyleKeys.MarginBottom, 4f) },
                null,
                NavLink.Create("/", "Home", exact: true),
                NavLink.Create("/about", "About"),
                NavLink.Create("/users", "Users"),
                NavLink.Create("/settings/profile", "Settings (Nested)"),
                NavLink.Create("/users/99", "Replace 99", exact: true, replace: true)
            );
        }

        private static class LocationBanner
        {
            public static VirtualNode Render(
                Dictionary<string, object> props,
                IReadOnlyList<VirtualNode> children
            )
            {
                var location = RouterHooks.UseLocationInfo();
                var query = RouterHooks.UseQuery();
                var navState = RouterHooks.UseNavigationState();

                return V.VisualElement(
                    new Style { (StyleKeys.FlexDirection, "column") },
                    null,
                    V.Text($"Current path: {location?.Path ?? "/"}"),
                    V.Text($"Query params: {DescribeQuery(query)}"),
                    V.Text($"Navigation state: {DescribeState(navState)}")
                );
            }
        }

        private static class NavigatePanel
        {
            public static VirtualNode Render(
                Dictionary<string, object> props,
                IReadOnlyList<VirtualNode> children
            )
            {
                var (path, setPath) = Hooks.UseState("/users/42?tab=overview");
                var (stateValue, setStateValue) = Hooks.UseState(string.Empty);
                var navigate = RouterHooks.UseNavigate();
                var replace = RouterHooks.UseNavigate(replace: true);
                return V.VisualElement(
                    new Style { (StyleKeys.FlexDirection, "column"), (StyleKeys.MarginTop, 4f) },
                    null,
                    V.Text("Jump to any path:"),
                    V.TextField(
                        new TextFieldProps
                        {
                            Value = path,
                            Placeholder = "/users/42 or /settings/profile",
                            OnChange = evt =>
                                setPath(string.IsNullOrEmpty(evt.newValue) ? "/" : evt.newValue),
                        }
                    ),
                    V.TextField(
                        new TextFieldProps
                        {
                            Value = stateValue,
                            Placeholder = "Optional state payload (stored with navigation)",
                            OnChange = evt => setStateValue(evt.newValue),
                        }
                    ),
                    V.VisualElement(
                        new Style
                        {
                            (StyleKeys.FlexDirection, "row"),
                            (StyleKeys.MarginTop, 4f),
                            (StyleKeys.MarginBottom, 2f),
                        },
                        null,
                        V.Button(
                            new ButtonProps
                            {
                                Text = "Push",
                                Style = new Style { (StyleKeys.Width, 80f) },
                                OnClick = () =>
                                    navigate(
                                        path,
                                        string.IsNullOrEmpty(stateValue) ? null : stateValue
                                    ),
                            }
                        ),
                        V.Button(
                            new ButtonProps
                            {
                                Text = "Replace",
                                Style = new Style { (StyleKeys.Width, 80f) },
                                OnClick = () =>
                                    replace(
                                        path,
                                        string.IsNullOrEmpty(stateValue) ? null : stateValue
                                    ),
                            }
                        )
                    )
                );
            }
        }

        private static class QuickAccessPanel
        {
            private readonly struct QuickLink
            {
                public QuickLink(string path, string label, object state = null)
                {
                    Path = path;
                    Label = label;
                    State = state;
                }

                public string Path { get; }
                public string Label { get; }
                public object State { get; }
            }

            private static readonly QuickLink[] QuickLinks =
            {
                new QuickLink("/users/10", "User 10"),
                new QuickLink("/users/20?tab=activity", "User 20 (tab=activity)"),
                new QuickLink("/users/20/details?tab=history", "User 20 details (query)"),
                new QuickLink(
                    "/users/30",
                    "User 30 (state)",
                    new Dictionary<string, object> { { "source", "quick-access" } }
                ),
                new QuickLink("/settings/profile?mode=compact", "Settings compact view"),
                new QuickLink("/settings/preferences?panel=alerts", "Settings alerts panel"),
            };

            public static VirtualNode Render(
                Dictionary<string, object> props,
                IReadOnlyList<VirtualNode> children
            )
            {
                var linkNodes = new List<VirtualNode>(QuickLinks.Length);
                foreach (var link in QuickLinks)
                {
                    linkNodes.Add(
                        V.Link(
                            link.Path,
                            link.Label,
                            style: new Style
                            {
                                (StyleKeys.MarginRight, 6f),
                                (StyleKeys.MarginBottom, 4f),
                            },
                            state: link.State
                        )
                    );
                }
                return V.VisualElement(
                    new Style { (StyleKeys.FlexDirection, "row"), (StyleKeys.FlexWrap, "wrap") },
                    null,
                    linkNodes.ToArray()
                );
            }
        }

        private static class UserDetails
        {
            public static VirtualNode Render(
                Dictionary<string, object> props,
                IReadOnlyList<VirtualNode> children
            )
            {
                var match = RouterHooks.UseRouteMatch();
                var query = RouterHooks.UseQuery();
                var navState = RouterHooks.UseNavigationState();
                string id =
                    match?.Parameters != null && match.Parameters.TryGetValue("id", out var value)
                        ? value
                        : "(unknown)";
                string tab =
                    query != null && query.TryGetValue("tab", out var tabValue)
                        ? tabValue
                        : "(default)";
                return V.Text(
                    $"User route matched with id: {id} (tab={tab}, state={DescribeState(navState)})"
                );
            }
        }

        private static class SettingsPanel
        {
            public static VirtualNode Render(
                Dictionary<string, object> props,
                IReadOnlyList<VirtualNode> children
            )
            {
                return V.VisualElement(
                    new Style { (StyleKeys.MarginTop, 6f) },
                    null,
                    V.Text("Settings route (demonstrates nested sub-routes):"),
                    V.VisualElement(
                        new Style { (StyleKeys.FlexDirection, "row"), (StyleKeys.MarginTop, 4f) },
                        null,
                        NavLink.Create("/settings/profile", "Profile", exact: true),
                        NavLink.Create("/settings/preferences", "Preferences", exact: true)
                    ),
                    V.Route(
                        path: "/settings/profile",
                        element: V.Text("Profile settings go here.")
                    ),
                    V.Route(
                        path: "/settings/preferences",
                        element: V.Text("Preferences settings go here.")
                    ),
                    V.Route(path: "*", element: V.Text("Select a settings section above."))
                );
            }
        }

        private static class NavLink
        {
            public static VirtualNode Create(
                string path,
                string label,
                bool exact = false,
                bool replace = false
            )
            {
                return V.Func(
                    Render,
                    new Dictionary<string, object>
                    {
                        { "path", path },
                        { "label", label },
                        { "exact", exact },
                        { "replace", replace },
                    }
                );
            }

            public static VirtualNode Render(
                Dictionary<string, object> props,
                IReadOnlyList<VirtualNode> children
            )
            {
                string path = props.TryGetValue("path", out var pathObj) ? pathObj as string : "/";
                string label = props.TryGetValue("label", out var labelObj)
                    ? labelObj as string
                    : path;
                bool exact =
                    props.TryGetValue("exact", out var exactObj)
                    && exactObj is bool exactFlag
                    && exactFlag;
                bool replace =
                    props.TryGetValue("replace", out var replaceObj)
                    && replaceObj is bool replaceFlag
                    && replaceFlag;

                string normalizedPath = RouterPath.Normalize(path);
                string location = RouterHooks.UseLocation();
                bool isActive = exact
                    ? string.Equals(
                        location,
                        normalizedPath,
                        System.StringComparison.OrdinalIgnoreCase
                    )
                    : location.StartsWith(
                        normalizedPath,
                        System.StringComparison.OrdinalIgnoreCase
                    );

                var style = new Style { (StyleKeys.MarginRight, 6f) };
                if (isActive)
                {
                    style[StyleKeys.BackgroundColor] = new Color32(64, 123, 255, 60);
                    style[StyleKeys.BorderRadius] = 4f;
                    style[StyleKeys.PaddingLeft] = 4f;
                    style[StyleKeys.PaddingRight] = 4f;
                }

                return V.Link(normalizedPath, label, replace: replace, style: style);
            }
        }

        private static class HistoryPanel
        {
            public static VirtualNode Render(
                Dictionary<string, object> props,
                IReadOnlyList<VirtualNode> children
            )
            {
                var go = RouterHooks.UseGo();
                bool canBack = RouterHooks.UseCanGo(-1);
                bool canForward = RouterHooks.UseCanGo(1);
                return V.VisualElement(
                    new Style { (StyleKeys.MarginTop, 4f) },
                    null,
                    V.Text("History controls:"),
                    V.Text($"Can go back: {canBack}, can go forward: {canForward}"),
                    V.VisualElement(
                        new Style { (StyleKeys.FlexDirection, "row"), (StyleKeys.MarginTop, 4f) },
                        null,
                        V.Button(
                            new ButtonProps
                            {
                                Text = "Back",
                                Style = new Style
                                {
                                    (StyleKeys.MarginRight, 6f),
                                    (StyleKeys.Width, 80f),
                                },
                                OnClick = () =>
                                {
                                    if (canBack)
                                    {
                                        go(-1);
                                    }
                                },
                            }
                        ),
                        V.Button(
                            new ButtonProps
                            {
                                Text = "Forward",
                                Style = new Style { (StyleKeys.Width, 80f) },
                                OnClick = () =>
                                {
                                    if (canForward)
                                    {
                                        go(1);
                                    }
                                },
                            }
                        )
                    )
                );
            }
        }

        private static class NavigationGuardPanel
        {
            public static VirtualNode Render(
                Dictionary<string, object> props,
                IReadOnlyList<VirtualNode> children
            )
            {
                var (enabled, setEnabled) = Hooks.UseState(false);
                var (message, setMessage) = Hooks.UseState("No navigation blocked yet.");

                var blocker = Hooks.UseMemo(
                    () =>
                        new Func<RouterLocation, RouterLocation, bool>(
                            (from, to) =>
                            {
                                setMessage(
                                    $"Blocked navigation to {to?.Path ?? "/"} at {DateTime.Now:HH:mm:ss}"
                                );
                                return false;
                            }
                        ),
                    Array.Empty<object>()
                );

                RouterHooks.UseBlocker(blocker, enabled);

                Hooks.UseEffect(
                    () =>
                    {
                        if (!enabled)
                        {
                            setMessage("Guard disabled; all navigation allowed.");
                        }
                        return null;
                    },
                    enabled
                );

                return V.VisualElement(
                    new Style { (StyleKeys.MarginTop, 8f), (StyleKeys.FlexDirection, "column") },
                    null,
                    V.Toggle(
                        new ToggleProps
                        {
                            Text = "Require confirmation before navigation",
                            Value = enabled,
                            OnChange = evt => setEnabled(evt.newValue),
                        }
                    ),
                    V.Text(message),
                    enabled
                        ? V.Text("Turn off the toggle to allow navigation to proceed.")
                        : V.Text("Toggle on to block navigation attempts.")
                );
            }
        }
    }
}
