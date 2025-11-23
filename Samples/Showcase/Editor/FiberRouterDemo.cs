using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Core.Fiber;
using ReactiveUITK.Props.Typed;
using ReactiveUITK.Router;
using UnityEditor;
using UnityEngine.UIElements;

namespace ReactiveUITK.Samples.Showcase.Editor
{
    public class FiberRouterDemo : EditorWindow
    {
        private const string RouteContextKey = "FiberRouterDemo.Route";
        private const string StateContextKey = "FiberRouterDemo.State";

        [MenuItem("ReactiveUITK/Demos/Router Demo")]
        public static void ShowWindow()
        {
            GetWindow<FiberRouterDemo>("Fiber Router Demo");
        }

        private VNodeHostRenderer renderer;

        private void CreateGUI()
        {
            var registry = ReactiveUITK.Elements.ElementRegistryProvider.GetDefaultRegistry();
            renderer = new VNodeHostRenderer(new HostContext(registry), rootVisualElement);
            renderer.Render(V.Func(SimpleRouterApp));
        }

        private void OnDestroy()
        {
            renderer?.Unmount();
        }

        /// <summary>
        /// Minimal Fiber router demo: three buttons and simple "child route" content.
        /// This does not use the full Router API yet; it is just state + context.
        /// </summary>
        private static VirtualNode SimpleRouterApp(
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children
        )
        {
            var (route, setRoute) = Hooks.UseState("/");
            var (navState, setNavState) = Hooks.UseState<object>(null);
            var (history, setHistory) = Hooks.UseState(new List<string> { "/" });

            // Expose the current route via context so child components can read it.
            Hooks.ProvideContext(RouteContextKey, route);
            Hooks.ProvideContext(StateContextKey, navState);

            void Navigate(string newRoute, object newState = null)
            {
                // Update route state
                setRoute(newRoute);
                setNavState(newState);

                // Append to history
                var next = new List<string>(history ?? new List<string>());
                next.Add(newRoute);
                if (next.Count > 5)
                {
                    next.RemoveRange(0, next.Count - 5);
                }
                setHistory(next);
            }

            return V.VisualElement(
                null,
                null,
                V.Button(
                    new ButtonProps
                    {
                        Text = "Go Home (/)",
                        OnClick = () => Navigate("/"),
                    }
                ),
                V.Button(
                    new ButtonProps
                    {
                        Text = "Go About (/about)",
                        OnClick = () => Navigate("/about"),
                    }
                ),
                V.Button(
                    new ButtonProps
                    {
                        Text = "Go User 42 (/user/42?tab=details) with state",
                        OnClick = () =>
                            Navigate(
                                "/user/42?tab=details",
                                new Dictionary<string, object> { { "from", "User42Button" } }
                            ),
                    }
                ),
                V.Func(RouteLabel),
                V.Func(RouteContent),
                V.Func(RouteHistory, new Dictionary<string, object> { { "history", history } })
            );
        }

        private static VirtualNode RouteLabel(
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children
        )
        {
            var route = Hooks.UseContext<string>(RouteContextKey) ?? "<none>";

            return V.Label(
                new LabelProps
                {
                    Text = $"Current route (from context): {route}"
                }
            );
        }

        /// <summary>
        /// Very simple "child route" content based on the current route string.
        /// </summary>
        private static VirtualNode RouteContent(
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children
        )
        {
            var rawRoute = Hooks.UseContext<string>(RouteContextKey) ?? "/";
            var navState = Hooks.UseContext<object>(StateContextKey);

            // Use RouterPath to parse path + query + state.
            var location = RouterPath.Parse(rawRoute, navState);
            var path = location.Path;
            var query = location.Query;
            var state = location.State;

            // Very simple param extraction: /user/:id
            string userId = null;
            var segments = RouterPath.SplitSegments(path);
            if (segments.Length >= 2 && segments[0] == "user")
            {
                userId = segments[1];
            }

            // Build a human-readable query string
            string queryText = "(none)";
            if (query != null && query.Count > 0)
            {
                var parts = new List<string>();
                foreach (var kv in query)
                {
                    parts.Add($"{kv.Key}={kv.Value}");
                }
                queryText = string.Join(", ", parts);
            }

            string stateText = state switch
            {
                null => "(none)",
                Dictionary<string, object> dict => string.Join(
                    ", ",
                    System.Linq.Enumerable.Select(dict, kv => $"{kv.Key}={kv.Value}")
                ),
                _ => state.ToString()
            };

            var nodes = new List<VirtualNode>
            {
                V.Label(new LabelProps { Text = $"Parsed path: {path}" }),
                V.Label(new LabelProps { Text = $"Query: {queryText}" }),
                V.Label(new LabelProps { Text = $"State: {stateText}" })
            };

            if (userId != null)
            {
                nodes.Add(V.Label(new LabelProps { Text = $"User route param id: {userId}" }));
            }

            return V.VisualElement(null, null, nodes.ToArray());
        }

        /// <summary>
        /// Shows the last few visited routes using props passed from the parent.
        /// </summary>
        private static VirtualNode RouteHistory(
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children
        )
        {
            props ??= new Dictionary<string, object>();
            props.TryGetValue("history", out var historyObj);
            var history = historyObj as List<string> ?? new List<string>();

            var nodes = new List<VirtualNode>
            {
                V.Label(new LabelProps { Text = "Recent routes:" })
            };

            if (history.Count == 0)
            {
                nodes.Add(V.Label(new LabelProps { Text = "(none)" }));
            }
            else
            {
                foreach (var r in history)
                {
                    nodes.Add(V.Label(new LabelProps { Text = $"• {r}" }));
                }
            }

            return V.VisualElement(null, null, nodes.ToArray());
        }
    }
}
