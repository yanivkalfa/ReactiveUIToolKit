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

        private static string BuildPathWithQuery(RouterLocation location)
        {
            if (location == null)
            {
                return "/";
            }

            string path = string.IsNullOrEmpty(location.Path) ? "/" : location.Path;
            var query = location.Query;
            if (query == null || query.Count == 0)
            {
                return path;
            }

            var parts = new List<string>(query.Count);
            foreach (var kv in query)
            {
                string key = Uri.EscapeDataString(kv.Key ?? string.Empty);
                string value = Uri.EscapeDataString(kv.Value ?? string.Empty);
                parts.Add($"{key}={value}");
            }

            return parts.Count == 0 ? path : $"{path}?{string.Join("&", parts)}";
        }

        public static VirtualNode Render(IProps rawProps, IReadOnlyList<VirtualNode> children)
        {
            return V.Router(
                children: new[]
                {
                    V.VisualElement(
                        new VisualElementProps { Style = PaddedContainer },
                        null,
                        V.Text("Lightweight Router Demo"),
                        BuildNavigationBar(),
                        V.VisualElement(
                            new VisualElementProps { Style = CardStyle },
                            null,
                            V.Func(LocationBanner),
                            V.Func(NavigatePanel),
                            V.Func(QuickAccessPanel.Render)
                        ),
                        V.VisualElement(
                            new VisualElementProps { Style = CardStyle },
                            null,
                            V.Func(HistoryPanel),
                            V.Func(NavigationGuardPanel, key: "navigation-guard")
                        ),
                        V.VisualElement(
                            new VisualElementProps { Style = CardStyle },
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
                                        new VisualElementProps
                                        {
                                            Style = new Style
                                            {
                                                (StyleKeys.MarginTop, 6f),
                                                (StyleKeys.FlexDirection, "column"),
                                            },
                                        },
                                        null,
                                        V.Func(UserDetails),
                                        V.Route(
                                            path: "/users/:id/details",
                                            element: V.Text(
                                                "Nested /details route matched for this user."
                                            )
                                        )
                                    ),
                                }
                            ),
                            V.Route(path: "/settings/*", children: new[] { V.Func(SettingsPanel) }),
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
                new VisualElementProps
                {
                    Style = new Style
                    {
                        (StyleKeys.FlexDirection, "row"),
                        (StyleKeys.MarginBottom, 4f),
                    },
                },
                null,
                NavLink.Create("/", "Home", exact: true),
                NavLink.Create("/about", "About"),
                NavLink.Create("/users", "Users"),
                NavLink.Create("/settings/profile", "Settings (Nested)"),
                NavLink.Create("/users/99", "Replace 99", exact: true, replace: true)
            );
        }

        private static VirtualNode LocationBanner(
            IProps rawProps,
            IReadOnlyList<VirtualNode> children
        )
        {
            var location = RouterHooks.UseLocationInfo();
            var query = RouterHooks.UseQuery();
            var navState = RouterHooks.UseNavigationState();

            return V.VisualElement(
                new VisualElementProps
                {
                    Style = new Style { (StyleKeys.FlexDirection, "column") },
                },
                null,
                V.Text($"Current path: {location?.Path ?? "/"}"),
                V.Text($"Query params: {DescribeQuery(query)}"),
                V.Text($"Navigation state: {DescribeState(navState)}")
            );
        }

        private static VirtualNode NavigatePanel(
            IProps rawProps,
            IReadOnlyList<VirtualNode> children
        )
        {
            Ref<TextField> pathRef = Hooks.UseRef<TextField>();
            Ref<TextField> stateRef = Hooks.UseRef<TextField>();

            var navigate = RouterHooks.UseNavigate();
            var replace = RouterHooks.UseNavigate(replace: true);
            return V.VisualElement(
                new VisualElementProps
                {
                    Style = new Style
                    {
                        (StyleKeys.FlexDirection, "column"),
                        (StyleKeys.MarginTop, 4f),
                    },
                },
                null,
                V.Text("Jump to any path:"),
                V.TextField(
                    new TextFieldProps
                    {
                        // Initial suggestion; field is otherwise uncontrolled.
                        Value = "/users/42?tab=overview",
                        Placeholder = "/users/42 or /settings/profile",
                        Ref = pathRef,
                    }
                ),
                V.TextField(
                    new TextFieldProps
                    {
                        Placeholder = "Optional state payload (stored with navigation)",
                        Ref = stateRef,
                    }
                ),
                V.VisualElement(
                    new VisualElementProps
                    {
                        Style = new Style
                        {
                            (StyleKeys.FlexDirection, "row"),
                            (StyleKeys.MarginTop, 4f),
                            (StyleKeys.MarginBottom, 2f),
                        },
                    },
                    null,
                    V.Button(
                        new ButtonProps
                        {
                            Text = "Push",
                            Style = new Style { (StyleKeys.Width, 80f) },
                            OnClick = _ =>
                            {
                                string rawPath = pathRef?.Current?.value;
                                string effectivePath = string.IsNullOrEmpty(rawPath)
                                    ? "/"
                                    : rawPath;

                                string rawState = stateRef?.Current?.value;
                                object statePayload = string.IsNullOrEmpty(rawState)
                                    ? null
                                    : rawState;

                                navigate(effectivePath, statePayload);
                            },
                        }
                    ),
                    V.Button(
                        new ButtonProps
                        {
                            Text = "Replace",
                            Style = new Style { (StyleKeys.Width, 80f) },
                            OnClick = _ =>
                            {
                                string rawPath = pathRef?.Current?.value;
                                string effectivePath = string.IsNullOrEmpty(rawPath)
                                    ? "/"
                                    : rawPath;

                                string rawState = stateRef?.Current?.value;
                                object statePayload = string.IsNullOrEmpty(rawState)
                                    ? null
                                    : rawState;

                                replace(effectivePath, statePayload);
                            },
                        }
                    )
                )
            );
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

            public static VirtualNode Render(IProps rawProps, IReadOnlyList<VirtualNode> children)
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
                    new VisualElementProps
                    {
                        Style = new Style
                        {
                            (StyleKeys.FlexDirection, "row"),
                            (StyleKeys.FlexWrap, "wrap"),
                        },
                    },
                    null,
                    linkNodes.ToArray()
                );
            }
        }

        private static VirtualNode UserDetails(IProps rawProps, IReadOnlyList<VirtualNode> children)
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

        private static VirtualNode SettingsPanel(
            IProps rawProps,
            IReadOnlyList<VirtualNode> children
        )
        {
            var locationPath = RouterHooks.UseLocation();

            string settingsContent;
            if (locationPath.StartsWith("/settings/profile", StringComparison.OrdinalIgnoreCase))
            {
                settingsContent = "Profile settings go here.";
            }
            else if (
                locationPath.StartsWith("/settings/preferences", StringComparison.OrdinalIgnoreCase)
            )
            {
                settingsContent = "Preferences settings go here.";
            }
            else
            {
                settingsContent = "Select a settings section above.";
            }

            return V.VisualElement(
                new VisualElementProps { Style = new Style { (StyleKeys.MarginTop, 6f) } },
                null,
                V.Text("Settings route (demonstrates nested sub-routes):"),
                V.VisualElement(
                    new VisualElementProps
                    {
                        Style = new Style
                        {
                            (StyleKeys.FlexDirection, "row"),
                            (StyleKeys.MarginTop, 4f),
                        },
                    },
                    null,
                    NavLink.Create("/settings/profile", "Profile", exact: true),
                    NavLink.Create("/settings/preferences", "Preferences", exact: true)
                ),
                V.Text(settingsContent)
            );
        }

        private static class NavLink
        {
            public sealed class Props : IProps
            {
                public string Path { get; set; }
                public string Label { get; set; }
                public bool Exact { get; set; }
                public bool Replace { get; set; }
            }

            public static VirtualNode Create(
                string path,
                string label,
                bool exact = false,
                bool replace = false
            )
            {
                return V.Func<Props>(
                    Render,
                    new Props
                    {
                        Path = path,
                        Label = label,
                        Exact = exact,
                        Replace = replace,
                    }
                );
            }

            public static VirtualNode Render(IProps rawProps, IReadOnlyList<VirtualNode> children)
            {
                var p = rawProps as Props;
                string path = p?.Path ?? "/";
                string label = p?.Label ?? path;
                bool exact = p?.Exact ?? false;
                bool replace = p?.Replace ?? false;

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

        private static VirtualNode HistoryPanel(
            IProps rawProps,
            IReadOnlyList<VirtualNode> children
        )
        {
            var go = RouterHooks.UseGo();
            bool canBack = RouterHooks.UseCanGo(-1);
            bool canForward = RouterHooks.UseCanGo(1);
            return V.VisualElement(
                new VisualElementProps { Style = new Style { (StyleKeys.MarginTop, 4f) } },
                null,
                V.Text("History controls:"),
                V.Text($"Can go back: {canBack}, can go forward: {canForward}"),
                V.VisualElement(
                    new VisualElementProps
                    {
                        Style = new Style
                        {
                            (StyleKeys.FlexDirection, "row"),
                            (StyleKeys.MarginTop, 4f),
                        },
                    },
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
                            OnClick = _ =>
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
                            OnClick = _ =>
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

        private static VirtualNode NavigationGuardPanel(
            IProps rawProps,
            IReadOnlyList<VirtualNode> children
        )
        {
            var (enabled, setEnabled) = Hooks.UseState(false);
            var (message, setMessage) = Hooks.UseState("Guard disabled; all navigation allowed.");
            var (clicks, setClicks) = Hooks.UseState(0);
            var (lastBlockedFrom, setLastBlockedFrom) = Hooks.UseState<RouterLocation>(null);
            var (lastBlockedTo, setLastBlockedTo) = Hooks.UseState<RouterLocation>(null);

            // Used to allow a single navigation to bypass the guard
            Ref<bool> allowNextRef = Hooks.UseRef<bool>();

            // For re-issuing confirmed navigations
            var navigate = RouterHooks.UseNavigate();

            // When enabled, register a router blocker that prevents navigation
            // and updates the guard message whenever a transition is blocked.
            RouterHooks.UseBlocker(
                (from, to) =>
                {
                    if (!enabled)
                    {
                        return true;
                    }

                    // If a confirmation just set allowNextRef, let this
                    // transition pass and reset the flag.
                    if (allowNextRef.Current)
                    {
                        allowNextRef.Current = false;
                        return true;
                    }

                    setLastBlockedFrom(from);
                    setLastBlockedTo(to);
                    string fromPath = from?.Path ?? "(unknown)";
                    string toPath = to?.Path ?? "(unknown)";
                    setMessage(
                        $"Blocked navigation from '{fromPath}' to '{toPath}'. Disable the guard to allow navigation."
                    );

                    return false;
                },
                enabled: enabled
            );

            return V.VisualElement(
                new VisualElementProps
                {
                    Style = new Style
                    {
                        (StyleKeys.MarginTop, 8f),
                        (StyleKeys.FlexDirection, "column"),
                    },
                },
                null,
                V.Toggle(
                    new ToggleProps
                    {
                        Text = "Require confirmation before navigation",
                        Value = enabled,
                        OnChange = evt =>
                        {
                            bool next = evt.newValue;
                            setEnabled(next);
                            if (next)
                            {
                                setMessage("Guard enabled; navigation requires confirmation.");
                            }
                            else
                            {
                                setMessage("Guard disabled; all navigation allowed.");
                            }
                        },
                    }
                ),
                V.Button(
                    new ButtonProps
                    {
                        Text = $"Test clicks: {clicks}",
                        OnClick = _ => setClicks(clicks + 1),
                        Style = new Style { (StyleKeys.MarginTop, 4f) },
                    }
                ),
                V.Text($"Guard enabled: {enabled}"),
                V.Text(message),
                V.Text("Use the button above to require confirmation before navigation."),
                V.VisualElement(
                    new VisualElementProps
                    {
                        Style = new Style
                        {
                            (StyleKeys.MarginTop, 4f),
                            (StyleKeys.FlexDirection, "row"),
                        },
                    },
                    null,
                    V.Button(
                        new ButtonProps
                        {
                            Text =
                                lastBlockedTo != null
                                    ? $"Allow last: {lastBlockedFrom?.Path ?? "(unknown)"} → {lastBlockedTo.Path}"
                                    : "Allow last blocked",
                            Enabled = lastBlockedTo != null,
                            OnClick = _ =>
                            {
                                if (lastBlockedTo == null)
                                {
                                    return;
                                }

                                string targetPath = BuildPathWithQuery(lastBlockedTo);
                                allowNextRef.Current = true;

                                navigate(targetPath, lastBlockedTo.State);
                                setLastBlockedFrom(default);
                                setLastBlockedTo(default);
                                setMessage("Guard enabled; last navigation was allowed.");
                            },
                            Style = new Style { (StyleKeys.Width, 180f) },
                        }
                    ),
                    V.Button(
                        new ButtonProps
                        {
                            Text = "Dismiss",
                            Enabled = lastBlockedTo != null,
                            OnClick = _ =>
                            {
                                setLastBlockedFrom(default);
                                setLastBlockedTo(default);
                                setMessage(
                                    enabled
                                        ? "Guard enabled; navigation requires confirmation."
                                        : "Guard disabled; all navigation allowed."
                                );
                            },
                            Style = new Style
                            {
                                (StyleKeys.MarginLeft, 4f),
                                (StyleKeys.Width, 100f),
                            },
                        }
                    )
                )
            );
        }
    }
}
