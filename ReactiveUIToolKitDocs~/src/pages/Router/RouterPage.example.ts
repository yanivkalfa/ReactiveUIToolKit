export const ROUTER_EDITOR_EXAMPLE = `using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Elements;
using ReactiveUITK.Props.Typed;
using ReactiveUITK.Props.Typed.EditorRootRendererUtility;
using ReactiveUITK.Router;
using ReactiveUITK.EditorSupport;
using UnityEditor;
using UnityEngine.UIElements;

// EditorWindow with Router
[MenuItem(\"Window/ReactiveUITK/Router Demo\")]
public static void Open()
{
  var window = GetWindow<EditorWindow>(\"Router Demo\");

  Render(
    window.rootVisualElement,
    V.Router(
      children: new[]
      {
        V.VisualElement(
          new Style { (StyleKeys.FlexDirection, \"row\"), (StyleKeys.MarginBottom, 6f) },
          null,
          V.Link(\"/\", \"Home\"),
          V.Link(\"/about\", \"About\"),
          V.Link(\"/users/42\", \"User 42\")
        ),
        V.Route(path: \"/\", exact: true, element: V.Text(\"Home route\")),
        V.Route(path: \"/about\", element: V.Text(\"About route\")),
        V.Route(
          path: \"/users/:id\",
          children: new[] { V.Func(UserProfileFunc.Render) }
        ),
        V.Route(path: \"*\", element: V.Text(\"Not found\")),
      }
    )
  );
}`

export const ROUTER_RUNTIME_EXAMPLE = `using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Elements;
using ReactiveUITK.Props.Typed;
using ReactiveUITK.Router;

// Function component using Router in runtime
public static class RouterDemoFunc
{
  private static readonly Style LinkBarStyle = new Style
  {
    (StyleKeys.FlexDirection, \"row\"),
    (StyleKeys.MarginBottom, 6f),
  };

  // Function component entrypoint �?\" pass RouterDemoFunc.Render
  // directly to V.Func when mounting.
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    return V.Router(
      children: new[]
      {
        V.VisualElement(
          LinkBarStyle,
          null,
          V.Link(\"/\", \"Home\"),
          V.Link(\"/about\", \"About\"),
          V.Link(\"/users/42\", \"User 42\")
        ),
        V.Route(path: \"/\", exact: true, element: V.Text(\"Home route\")),
        V.Route(path: \"/about\", element: V.Text(\"About route\")),
        V.Route(
          path: \"/users/:id\",
          children: new[] { V.Func(UserProfile) }
        ),
        V.Route(path: \"*\", element: V.Text(\"Not found\")),
      }
    );
  }
}

// Mounted through RootRenderer elsewhere:
// rootRenderer.Render(V.Func(RouterDemoFunc.Example));`

export const ROUTER_LINKS_AND_NAV_EXAMPLE = `using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using ReactiveUITK.Router;

// Demonstrates links, programmatic navigation, params, query, and state.
public static class RouterLinksFunc
{
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var navigate = RouterHooks.UseNavigate();
    var go = RouterHooks.UseGo();
    bool canBack = RouterHooks.UseCanGo(-1);

    var location = RouterHooks.UseLocationInfo();
    var routeMatch = RouterHooks.UseRouteMatch();
    var parameters = RouterHooks.UseParams();
    var query = RouterHooks.UseQuery();
    var navState = RouterHooks.UseNavigationState();

    void ToUser42()
    {
      // Push a new location and attach a small state payload
      navigate(\"/users/42?tab=details\", new { from = \"nav-button\" });
    }

    void GoBack()
    {
      go(-1);
    }

    string userId = parameters.TryGetValue(\"id\", out var id) ? id : \"(none)\";

    return V.Column(
      key: null,
      V.Row(
        key: \"links\",
        V.Link(\"/\", \"Home\"),
        V.Link(\"/about\", \"About\"),
        V.Link(\"/users/42?tab=details\", \"User 42 (details)\")
      ),
      V.Row(
        key: \"actions\",
        V.Button(new ButtonProps { Text = \"To User 42 (code)\", OnClick = ToUser42 }),
        V.Button(new ButtonProps { Text = \"Back\", Enabled = canBack, OnClick = GoBack })
      ),
      V.Label(new LabelProps { Text = $\"Path: {location?.Path}\" }),
      V.Label(new LabelProps { Text = $\"User id param: {userId}\" }),
      V.Label(new LabelProps { Text = $\"Query keys: {string.Join(\\", \\", query.Keys)}\" }),
      V.Label(new LabelProps { Text = $\"Nav state type: {navState?.GetType().Name ?? \\"(none)\\"}\" })
    );
  }
}`

export const ROUTER_SPLIT_LAYOUT_EXAMPLE = `using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using ReactiveUITK.Router;

public static class SplitShellDemo
{
  private static readonly Style Shell = new()
  {
    (StyleKeys.FlexGrow, 1f),
    (StyleKeys.FlexDirection, \"column\"),
    (StyleKeys.Padding, 12f),
  };

  private static readonly Style ContentRow = new()
  {
    (StyleKeys.FlexGrow, 1f),
    (StyleKeys.FlexDirection, \"row\"),
    (StyleKeys.MarginTop, 8f),
  };

  private static readonly Style Sidebar = new()
  {
    (StyleKeys.Width, 220f),
    (StyleKeys.FlexDirection, \"column\"),
    (StyleKeys.Padding, 10f),
    (StyleKeys.BorderWidth, 1f),
    (StyleKeys.BorderRadius, 6f),
  };

  private static readonly Style Outlet = new()
  {
    (StyleKeys.FlexGrow, 1f),
    (StyleKeys.MarginLeft, 12f),
    (StyleKeys.Padding, 12f),
    (StyleKeys.BorderWidth, 1f),
    (StyleKeys.BorderRadius, 6f),
  };

  public static VirtualNode Render(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    return V.Router(
      children: new[]
      {
        BuildNavRow(),
        V.Route(path: \"/\", exact: true, element: V.Text(\"Landing route\")),
        V.Route(path: \"/mainMenu/*\", children: new[] { V.Func(MainMenuLayout) }),
        V.Route(path: \"*\", element: V.Text(\"Not found\")),
      }
    );
  }

  private static VirtualNode BuildNavRow()
  {
    var navigate = RouterHooks.UseNavigate();
    return V.VisualElement(
      new Style { (StyleKeys.FlexDirection, \"row\"), (StyleKeys.MarginBottom, 4f) },
      null,
      V.Button(new ButtonProps { Text = \"Home (/)\", OnClick = () => navigate(\"/\") }),
      V.Button(new ButtonProps { Text = \"Open Main Menu\", OnClick = () => navigate(\"/mainMenu\") })
    );
  }

  private static VirtualNode MainMenuLayout(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var location = RouterHooks.UseLocationInfo();
    var navigate = RouterHooks.UseNavigate();
    return V.VisualElement(
      ContentRow,
      null,
      V.VisualElement(
        Sidebar,
        null,
        V.Text(\"Sidebar\"),
        V.Button(new ButtonProps { Text = \"Home\", OnClick = () => navigate(string.Empty) }),
        V.Button(new ButtonProps { Text = \"Profile\", OnClick = () => navigate(\"profile\") }),
        V.Button(new ButtonProps { Text = \"Store\", OnClick = () => navigate(\"store\") }),
        V.Button(new ButtonProps { Text = \"Settings\", OnClick = () => navigate(\"settings\") })
      ),
      V.VisualElement(
        Outlet,
        null,
        V.Text($\"Outlet (current path: {location?.Path ?? \"/\"})\"),
        V.Route(path: string.Empty, exact: true, element: V.Text(\"Pick a submenu from the left.\")),
        V.Route(path: \":id/edit\", element: V.Text(\"Editing view with route params\")),
        V.Route(path: \"profile\", element: V.Text(\"Profile content\")),
        V.Route(path: \"store\", element: V.Text(\"Store content\")),
        V.Route(path: \"settings\", element: V.Text(\"Settings content\"))
      )
    );
  }
}
`;
