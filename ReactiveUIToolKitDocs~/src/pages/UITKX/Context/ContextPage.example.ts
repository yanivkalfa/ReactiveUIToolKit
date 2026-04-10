export const CONTEXT_BASIC_EXAMPLE = `// Provider — makes a value available to all descendants
component AppRoot {
  provideContext("userName", "Alice");
  provideContext("theme", "dark");

  return (
    <VisualElement>
      <Sidebar />
      <MainContent />
    </VisualElement>
  );
}

// Consumer — reads the value anywhere in the subtree
component Sidebar {
  var userName = useContext<string>("userName");
  var theme = useContext<string>("theme");

  return (
    <VisualElement style={new Style {
      BackgroundColor = theme == "dark" ? Hex("#1e1e1e") : ColorWhite
    }}>
      <Label text={$"Logged in as {userName}"} />
    </VisualElement>
  );
}`

export const CONTEXT_SHADOWING_EXAMPLE = `component OuterProvider {
  provideContext("theme", "light");

  return (
    <VisualElement>
      <Label text={useContext<string>("theme")} />   // "light"
      <InnerProvider />
    </VisualElement>
  );
}

component InnerProvider {
  provideContext("theme", "dark");  // shadows outer

  return (
    <VisualElement>
      <Label text={useContext<string>("theme")} />   // "dark"
    </VisualElement>
  );
}`

export const CONTEXT_DYNAMIC_EXAMPLE = `component ThemeToggle {
  var (isDark, setIsDark) = useState(true);
  provideContext("theme", isDark ? "dark" : "light");

  return (
    <VisualElement>
      <Toggle label="Dark mode" value={isDark}
              onChange={e => setIsDark(e.newValue)} />
      <ThemedPanel />
    </VisualElement>
  );
}

component ThemedPanel {
  var theme = useContext<string>("theme");
  // Automatically re-renders when the provided value changes

  return (
    <VisualElement style={new Style {
      BackgroundColor = theme == "dark" ? Hex("#1e1e1e") : ColorWhite,
      Padding = Px(16)
    }}>
      <Label text={$"Current theme: {theme}"} />
    </VisualElement>
  );
}`

export const CONTEXT_VS_SIGNALS = `// Use Context when:
// - Data is scoped to a subtree (e.g., theme for a panel)
// - Different parts of the tree need different values
// - Provider/consumer relationship is 1-to-many within a branch

// Use Signals when:
// - Data is truly global (e.g., user session, app-wide settings)
// - Multiple independent trees need the same value
// - You want process-wide reactivity without component hierarchy`

export const CONTEXT_TYPED_EXAMPLE = `// Predefined context keys for type safety
static class AppContextKeys
{
    public const string Theme = "app.theme";
    public const string Locale = "app.locale";
    public const string Auth = "app.auth";
}

// Provider
component AppShell {
  provideContext(AppContextKeys.Theme, currentTheme);
  provideContext(AppContextKeys.Locale, "en-US");
  provideContext(AppContextKeys.Auth, authState);
  // ...
}

// Consumer
component LocalizedLabel {
  var locale = useContext<string>(AppContextKeys.Locale);
  // ...
}`
