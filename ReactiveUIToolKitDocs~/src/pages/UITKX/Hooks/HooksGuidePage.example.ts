export const HOOKS_USESTATE_EXAMPLE = `component CounterDemo {
  var (count, setCount) = useState(0);

  // Direct value
  // setCount(5);

  // Functional updater — safe when batching multiple updates
  // setCount(prev => prev + 1);

  return (
    <VisualElement>
      <Label text={$"Count: {count}"} />
      <Button text="Increment" onClick={_ => setCount(count + 1)} />
      <Button text="Double" onClick={_ => setCount(prev => prev * 2)} />
    </VisualElement>
  );
}`

export const HOOKS_USEREDUCER_EXAMPLE = `// Define reducer and action types in a companion .types.uitkx module:
// enum CounterAction { Increment, Decrement, Reset }
// TState Reducer(TState state, CounterAction action) => action switch {
//   CounterAction.Increment => state + 1,
//   CounterAction.Decrement => state - 1,
//   CounterAction.Reset => 0,
//   _ => state
// };

component ReducerDemo {
  var (count, dispatch) = useReducer(CounterReducer, 0);

  return (
    <VisualElement>
      <Label text={$"Count: {count}"} />
      <Button text="+" onClick={_ => dispatch(CounterAction.Increment)} />
      <Button text="-" onClick={_ => dispatch(CounterAction.Decrement)} />
      <Button text="Reset" onClick={_ => dispatch(CounterAction.Reset)} />
    </VisualElement>
  );
}`

export const HOOKS_USEEFFECT_EXAMPLE = `component EffectDemo {
  var (seconds, setSeconds) = useState(0);

  // Runs once on mount — empty dependency array
  useEffect(() => {
    var cts = new CancellationTokenSource();
    Task.Run(async () => {
      while (!cts.Token.IsCancellationRequested) {
        await Task.Delay(1000, cts.Token);
        setSeconds(s => s + 1);
      }
    }, cts.Token);
    return () => cts.Cancel(); // cleanup on unmount
  });

  return (<Label text={$"Elapsed: {seconds}s"} />);
}`

export const HOOKS_USELAYOUTEFFECT_EXAMPLE = `component LayoutMeasure {
  var elRef = useRef();   // VisualElement ref
  var (width, setWidth) = useState(0f);

  // Runs synchronously before the frame paints
  useLayoutEffect(() => {
    if (elRef != null)
      setWidth(elRef.resolvedStyle.width);
    return null;
  }, elRef);

  return (
    <VisualElement ref={elRef}>
      <Label text={$"Width: {width:F0}px"} />
    </VisualElement>
  );
}`

export const HOOKS_USEMEMO_EXAMPLE = `component ExpensiveList {
  var (filter, setFilter) = useState("");
  var (items, _) = useState(GetAllItems());

  // Only recomputes when filter or items change
  var filtered = useMemo(() =>
    items.Where(i => i.Contains(filter)).ToList(),
    filter, items);

  return (
    <VisualElement>
      <TextField value={filter} onInput={setFilter.ToValueAction()} />
      @foreach (var item in filtered) {
        return (<Label text={item} key={item} />);
      }
    </VisualElement>
  );
}`

export const HOOKS_USECALLBACK_EXAMPLE = `component StableCallback {
  var (count, setCount) = useState(0);

  // Returns Func<int> — identity stable across renders
  var getCount = useCallback(() => count, count);

  return (
    <VisualElement>
      <Label text={$"Count: {getCount()}"} />
      <Button text="Increment" onClick={_ => setCount(count + 1)} />
    </VisualElement>
  );
}`

export const HOOKS_USEREF_EXAMPLE = `component RefDemo {
  // Mutable value ref — persists across renders, no re-render on change
  var renderCount = useRef(0);
  renderCount.Current++;

  // Element ref — gives access to the underlying VisualElement
  var labelRef = useRef();

  useEffect(() => {
    if (labelRef != null)
      Debug.Log($"Label element: {labelRef.name}");
    return null;
  }, labelRef);

  return (
    <VisualElement>
      <Label ref={labelRef}
             text={$"This component rendered {renderCount.Current} times"} />
    </VisualElement>
  );
}`

export const HOOKS_CONTEXT_EXAMPLE = `// Provider component
component ThemeProvider {
  provideContext("theme", "dark");

  return (
    <VisualElement>
      <ThemedCard />
    </VisualElement>
  );
}

// Consumer component — any depth in subtree
component ThemedCard {
  var theme = useContext<string>("theme"); // "dark"

  return (
    <VisualElement style={new Style {
      BackgroundColor = theme == "dark" ? ColorBlack : ColorWhite
    }}>
      <Label text={$"Theme: {theme}"} />
    </VisualElement>
  );
}`

export const HOOKS_STABLE_EXAMPLE = `component EventOptimization {
  var (name, setName) = useState("");

  // UseStableAction wraps the setter so identity never changes
  var onNameChanged = useStableAction<string>(v => setName(v));

  // UseStableCallback for parameterless callbacks
  var onReset = useStableCallback(() => setName(""));

  return (
    <VisualElement>
      <TextField value={name} onInput={onNameChanged} />
      <Button text="Reset" onClick={_ => onReset()} />
    </VisualElement>
  );
}`

export const HOOKS_DEFERRED_EXAMPLE = `component SearchResults {
  var (query, setQuery) = useState("");

  // Deferred value updates at lower priority — prevents blocking input
  var deferredQuery = useDeferredValue(query, query);

  return (
    <VisualElement>
      <TextField value={query} onInput={setQuery.ToValueAction()} />
      <ResultsList filter={deferredQuery} />
    </VisualElement>
  );
}`

export const HOOKS_IMPERATIVE_EXAMPLE = `component FancyInput {
  // Expose an imperative handle to parent via ref
  var handle = useImperativeHandle(() => new FancyInputHandle {
    Focus = () => inputRef?.Focus(),
    Clear = () => setVal(""),
  });

  var inputRef = useRef();
  var (val, setVal) = useState("");

  return (<TextField ref={inputRef} value={val} onInput={setVal.ToValueAction()} />);
}`

export const HOOKS_DEPENDENCY_RULES = `// No dependencies → runs cleanup + effect EVERY render
useEffect(() => { ... return cleanup; });

// Empty dependency array → runs once on mount, cleanup on unmount
useEffect(() => { ... return cleanup; }, /* nothing */);

// With dependencies → runs when any dependency changes
useEffect(() => { ... return cleanup; }, dep1, dep2);

// Dependency comparison uses object.Equals (value equality for value types,
// reference equality for reference types).`
