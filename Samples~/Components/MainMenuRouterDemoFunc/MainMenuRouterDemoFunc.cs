using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using ReactiveUITK.Router;
using UnityEngine;

namespace ReactiveUITK.Samples.FunctionalComponents
{
    public static class MainMenuRouterDemoFunc
    {
        private static readonly Style RootStyle = new()
        {
            (StyleKeys.FlexGrow, 1f),
            (StyleKeys.FlexDirection, "column"),
            (StyleKeys.Padding, 12f),
        };

        private static readonly Style LayoutRow = new()
        {
            (StyleKeys.FlexGrow, 1f),
            (StyleKeys.FlexDirection, "row"),
            (StyleKeys.MarginTop, 8f),
        };

        private static readonly Style NavRowStyle = new()
        {
            (StyleKeys.FlexDirection, "row"),
            (StyleKeys.MarginTop, 6f),
            (StyleKeys.MarginBottom, 4f),
        };

        private static readonly Style NavButtonStyle = new() { (StyleKeys.MarginRight, 6f) };

        private static readonly Style SidebarStyle = new()
        {
            (StyleKeys.Width, 220f),
            (StyleKeys.Padding, 10f),
            (StyleKeys.FlexDirection, "column"),
            (StyleKeys.JustifyContent, "flex-start"),
            (StyleKeys.AlignItems, "stretch"),
            (StyleKeys.BorderWidth, 1f),
            (StyleKeys.BorderColor, new Color(0.25f, 0.25f, 0.25f, 1f)),
            (StyleKeys.BorderRadius, 6f),
        };

        private static readonly Style SidebarButtonStyle = new() { (StyleKeys.MarginTop, 6f) };

        private static readonly Style OutletStyle = new()
        {
            (StyleKeys.FlexGrow, 1f),
            (StyleKeys.Padding, 12f),
            (StyleKeys.MarginLeft, 12f),
            (StyleKeys.BorderWidth, 1f),
            (StyleKeys.BorderColor, new Color(0.3f, 0.3f, 0.3f, 1f)),
            (StyleKeys.BorderRadius, 6f),
            (StyleKeys.FlexDirection, "column"),
        };

        public static VirtualNode Render(
            IProps rawProps,
            IReadOnlyList<VirtualNode> children
        )
        {
            return V.Router(
                children: new[]
                {
                    V.VisualElement(
                        RootStyle,
                        null,
                        V.Text("Main menu router shell"),
                        V.Func(NavigationRow),
                        V.Route(
                            path: "/",
                            exact: true,
                            element: V.Text("Landing route: go to /mainMenu to open the layout.")
                        ),
                        V.Route(path: "/mainMenu/*", children: new[] { V.Func(MainMenuLayout) }),
                        V.Route(
                            path: "*",
                            element: V.Text("No matching route in the shell router.")
                        )
                    ),
                }
            );
        }

        private static VirtualNode NavigationRow(
            IProps rawProps,
            IReadOnlyList<VirtualNode> children
        )
        {
            var navigate = RouterHooks.UseNavigate();
            return V.VisualElement(
                NavRowStyle,
                null,
                V.Button(
                    new ButtonProps
                    {
                        Text = "Go Home (/)",
                        OnClick = () => navigate("/"),
                        Style = NavButtonStyle,
                    }
                ),
                V.Button(
                    new ButtonProps
                    {
                        Text = "Open Main Menu (/mainMenu)",
                        OnClick = () => navigate("/mainMenu"),
                        Style = NavButtonStyle,
                    }
                )
            );
        }

        private static VirtualNode MainMenuLayout(
            IProps rawProps,
            IReadOnlyList<VirtualNode> children
        )
        {
            var location = RouterHooks.UseLocationInfo();
            var navigationBase = RouterHooks.UseNavigationBase();
            return V.VisualElement(
                LayoutRow,
                null,
                BuildSidebar(),
                V.VisualElement(
                    OutletStyle,
                    null,
                    V.Text($"Outlet (current: {location?.Path ?? "/"})"),
                    V.Route(
                        path: ":id/edit",
                        exact: true,
                        element: V.Text("this will be /mainMenu/15/edit")
                    ),
                    V.Route(
                        path: "profile",
                        element: V.Text("Profile content rendered in the outlet.")
                    ),
                    V.Route(
                        path: "store",
                        element: V.Text("Store content rendered in the outlet.")
                    ),
                    V.Route(
                        path: "settings",
                        element: V.Text("Settings content rendered in the outlet.")
                    )
                )
            );
        }

        private static VirtualNode BuildSidebar()
        {
            var navigate = RouterHooks.UseNavigate();
            var navigationBase = RouterHooks.UseNavigationBase();
            var currentPath = RouterHooks.UseLocation();

            return V.VisualElement(
                SidebarStyle,
                null,
                V.Text("Sidebar"),
                V.Button(
                    new ButtonProps
                    {
                        Text = "Home",
                        OnClick = () => navigate(string.Empty),
                        Style = SidebarButtonStyle,
                    }
                ),
                V.Button(
                    new ButtonProps
                    {
                        Text = "Profile",
                        OnClick = () => navigate("profile"),
                        Style = SidebarButtonStyle,
                    }
                ),
                V.Button(
                    new ButtonProps
                    {
                        Text = "Store",
                        OnClick = () => navigate("store"),
                        Style = SidebarButtonStyle,
                    }
                ),
                V.Button(
                    new ButtonProps
                    {
                        Text = "Settings",
                        OnClick = () => navigate("settings"),
                        Style = SidebarButtonStyle,
                    }
                )
            );
        }
    }
}
