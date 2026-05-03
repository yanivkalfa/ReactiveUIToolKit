export const UITKX_ROUTER_EXAMPLE = `@using ReactiveUITK.Router

component RouterDemo {
  var navigate = RouterHooks.UseNavigate();
  var (query, setQuery) = RouterHooks.UseSearchParams();

  return (
    <Router basename="/app">
      <VisualElement>
        {/* NavLink renders an active style when the route matches. */}
        <NavLink to="/" end={true} label="Home" />
        <NavLink to="/about" label="About" />
        <NavLink to="/users" label="Users" />

        <Button
          text="Open profile 42"
          onClick={_ => navigate("/users/42?tab=profile")}
        />

        {/* Routes picks the single best match using RR's ranking algorithm. */}
        <Routes>
          {/* Index route — matches the parent path exactly. */}
          <Route index={true} element={<Text text="Landing route" />} />

          <Route path="/about" element={<Text text="About route" />} />

          {/* Layout route — element wraps the matched child via <Outlet/>. */}
          <Route path="/users" element={<UsersLayout />}>
            <Route index={true} element={<Text text="Pick a user" />} />
            <Route path=":id" element={<UserDetails />} />
          </Route>

          {/* Declarative redirect (replace=true by default). */}
          <Route path="/old" element={<Navigate to="/about" />} />

          <Route path="*" element={<Text text="Not found" />} />
        </Routes>
      </VisualElement>
    </Router>
  );
}

component UsersLayout {
  return (
    <VisualElement>
      <Text text="Users header" />
      {/* Nested route content renders here. */}
      <Outlet />
    </VisualElement>
  );
}

component UserDetails {
  var parameters = RouterHooks.UseParams();
  var matches = RouterHooks.UseMatches();   // breadcrumb chain
  return <Text text={$"User id: {parameters["id"]}"} />;
}`
