export const UITKX_ROUTER_EXAMPLE = `@using ReactiveUITK.Router

component RouterDemo {
  var navigate = RouterHooks.UseNavigate();
  var parameters = RouterHooks.UseParams();
  var query = RouterHooks.UseQuery();

  return (
    <Router>
      <VisualElement>
        <RouterNavLink path="/" label="Home" exact={true} />
        <RouterNavLink path="/about" label="About" />
        <RouterNavLink path="/users" label="Users" />
        <Button
          text="Open profile 42"
          onClick={_ => navigate("/users/42?tab=profile")}
        />

        <Route
          path="/"
          exact={true}
          element={<Text text="Landing route" />}
        />
        <Route
          path="/about"
          element={<Text text="About route" />}
        />
        <Route path="/users/:id">
          <VisualElement>
            <Text text={$"User id: {parameters["id"]}"} />
            <Text text={$"Tab: {query["tab"] ?? "summary"}"} />
            <RouterUserDetails />
          </VisualElement>
        </Route>
        <Route
          path="*"
          element={<Text text="Not found" />}
        />
      </VisualElement>
    </Router>
  );
}`
