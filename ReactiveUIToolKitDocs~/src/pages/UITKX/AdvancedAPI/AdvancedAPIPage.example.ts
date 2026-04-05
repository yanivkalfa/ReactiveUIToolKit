export const PROPTYPES_EXAMPLE = `// Define prop types for a component (in a companion .utils.cs file)
public static readonly PropTypeDefinition[] CardPropTypes = new[]
{
    PropTypes.String("title", required: true),
    PropTypes.Number("width"),
    PropTypes.Boolean("showBorder"),
    PropTypes.Enum("variant", "filled", "outlined", "ghost"),
    PropTypes.InstanceOf<Texture2D>("icon"),
    PropTypes.Custom("data", v => v is IList, "Must be a list"),
};

// Attach to a VirtualNode for dev-time validation
return V.Func(CardComponent.Render)
    .WithPropTypes(CardPropTypes);`

export const HOSTCONTEXT_EXAMPLE = `// HostContext is created internally by RootRenderer.
// Access it via the environment callback when initializing:
var rootRenderer = gameObject.AddComponent<RootRenderer>();
rootRenderer.Initialize(root, env =>
{
    // env.Environment is a Dictionary<string, object>
    env.Environment["app.debug"] = true;
    env.Environment["app.version"] = "1.0.0";

    // Context values set here are available via UseContext
    env.SetContextValue("theme", "dark");
});
rootRenderer.Render(V.Func(App.Render));

// In a component:
// var theme = useContext<string>("theme"); // "dark"
// var debug = useContext<bool>("app.debug"); // true`

export const SCHEDULER_EXAMPLE = `// IScheduler controls how render work is dispatched.
// The default RenderScheduler processes work per Unity frame.

// Priority levels (from highest to lowest):
//   High   — critical updates (user input response)
//   Normal — standard state updates (default)
//   Low    — deferred work (background computation)
//   Idle   — lowest priority (analytics, logging)

// Manual synchronous flush:
Hooks.FlushSync(() =>
{
    // All state updates inside this callback are
    // batched and flushed synchronously before returning.
    setCount(count + 1);
    setName("Alice");
});

// Flush without new work (drain pending queue):
Hooks.FlushSync();`

export const SNAPSHOT_EXAMPLE = `// SnapshotAssert compares two VirtualNode trees.
// Useful for unit-testing component output.

var expected = V.VisualElement(key: "root", children: new[]
{
    V.Label(new LabelProps { Text = "Hello" }),
});

var actual = MyComponent.Render(props, children);

// Option 1: get a result struct
var result = SnapshotAssert.Compare(expected, actual);
if (!result.Pass)
    Debug.LogError($"Mismatch:\\n{result.Diff}");

// Option 2: assert directly (logs error on mismatch)
SnapshotAssert.AssertEqual(expected, actual);`

export const ELEMENT_REGISTRY_EXAMPLE = `// The default registry includes all 61 built-in elements.
ElementRegistry defaultRegistry = ElementRegistryProvider.GetDefaultRegistry();

// Create a filtered registry (e.g., for sandboxed rendering):
var safeRegistry = ElementRegistryProvider.CreateFilteredRegistry(
    new[] { "VisualElement", "Label", "Button", "TextField" }
);

// Register a custom element adapter:
defaultRegistry.Register("MyCustomElement", new MyCustomAdapter());

// Resolve an adapter by tag name:
IElementAdapter adapter = defaultRegistry.Resolve("Button");`

export const DEPTH_GUARD_EXAMPLE = `// The reconciler has a built-in render depth guard.
// Maximum depth: 25 nested renders.
//
// If a component calls setState unconditionally during render,
// it creates an infinite loop. The guard catches this:
//
// [Fiber] Maximum render depth (25) exceeded in 'BrokenComponent'.
// A component may be calling setState unconditionally during render.
//
// The component returns null, preventing a crash.
// Fix: ensure state updates are inside event handlers or effects,
// not in the setup code that runs every render.`

export const VIRTUALNODE_EXAMPLE = `// VirtualNode is the return type of all render functions.
// In .uitkx files, the source generator creates VirtualNodes for you.
// In the C# API, use V.* factory methods:

// VirtualNodeType enum values:
//   Element, Text, FunctionComponent, Fragment,
//   Portal, Suspense, ErrorBoundary, Host

// Key properties:
//   NodeType         — what kind of node this is
//   ElementTypeName  — "Button", "Label", etc.
//   Key              — reconciler identity key
//   Properties       — IReadOnlyDictionary<string, object>
//   Children         — IReadOnlyList<VirtualNode>
//   TextContent      — for text nodes
//   PortalTarget     — for Portal nodes
//   Fallback         — for Suspense/ErrorBoundary nodes

// Typically you never construct VirtualNode directly.
// Use V.Button(...), V.Label(...), V.Func(...) etc.`

export const FLUSHSYNC_EXAMPLE = `component SearchForm {
  var (query, setQuery) = useState("");
  var (results, setResults) = useState(new List<string>());

  // FlushSync forces synchronous re-render before continuing
  void OnSearch() {
    Hooks.FlushSync(() => {
      setQuery(inputRef.value);
      setResults(Search(inputRef.value));
    });
    // At this point, both query and results are already rendered
  }

  var inputRef = useRef();

  return (
    <VisualElement>
      <TextField ref={inputRef} value={query} />
      <Button text="Search" onClick={_ => OnSearch()} />
      @foreach (var r in results) {
        return (<Label text={r} key={r} />);
      }
    </VisualElement>
  );
}`

export const ERROR_PATTERNS_EXAMPLE = `// Pattern 1: ErrorBoundary with fallback UI
component SafeApp {
  return (
    <ErrorBoundary
      fallback={V.Label(new LabelProps { Text = "Something went wrong" })}
      onError={ex => Debug.LogError(ex)}
      resetKey={resetToken}>
      <RiskyContent />
    </ErrorBoundary>
  );
}

// Pattern 2: Nested boundaries for granular recovery
component Dashboard {
  return (
    <VisualElement>
      <ErrorBoundary fallback={V.Label(new LabelProps { Text = "Chart failed" })}>
        <ChartWidget />
      </ErrorBoundary>
      <ErrorBoundary fallback={V.Label(new LabelProps { Text = "List failed" })}>
        <DataList />
      </ErrorBoundary>
    </VisualElement>
  );
}

// Pattern 3: Reset via key change
component RecoverablePanel {
  var (resetKey, setResetKey) = useState("v1");

  return (
    <VisualElement>
      <Button text="Retry" onClick={_ => setResetKey(Guid.NewGuid().ToString())} />
      <ErrorBoundary resetKey={resetKey}
                     fallback={V.Label(new LabelProps { Text = "Error — click Retry" })}>
        <UnstableContent />
      </ErrorBoundary>
    </VisualElement>
  );
}`
