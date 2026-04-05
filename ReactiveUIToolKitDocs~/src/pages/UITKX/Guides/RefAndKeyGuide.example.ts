export const REF_BASIC_EXAMPLE = `component MeasureDemo {
  var labelRef = useRef();  // VisualElement ref
  var (width, setWidth) = useState(0f);

  useLayoutEffect(() => {
    if (labelRef != null)
      setWidth(labelRef.resolvedStyle.width);
    return null;
  }, labelRef);

  return (
    <VisualElement>
      <Label ref={labelRef} text="Measure me" />
      <Label text={$"Width: {width:F0}px"} />
    </VisualElement>
  );
}`

export const REF_MUTABLE_EXAMPLE = `component RenderCounter {
  var renderCount = useRef(0);
  renderCount.Current++;

  var (_, forceUpdate) = useState(0);

  return (
    <VisualElement>
      <Label text={$"Rendered {renderCount.Current} times"} />
      <Button text="Force re-render"
              onClick={_ => forceUpdate(prev => prev + 1)} />
    </VisualElement>
  );
}`

export const REF_FOCUS_EXAMPLE = `component AutoFocusInput {
  var inputRef = useRef();

  useEffect(() => {
    inputRef?.Focus();
    return null;
  });

  return (
    <TextField ref={inputRef}
               label="Auto-focused on mount" />
  );
}`

export const REF_IMPERATIVE_EXAMPLE = `// Child exposes an imperative handle
component FancyInput {
  var inputRef = useRef();
  var (val, setVal) = useState("");

  var handle = useImperativeHandle(() => new FancyInputHandle {
    Focus = () => inputRef?.Focus(),
    Clear = () => setVal(""),
    Value = val,
  });

  return (<TextField ref={inputRef} value={val}
                     onInput={setVal.ToValueAction()} />);
}

// Parent uses the handle
component FormHost {
  // The child's useImperativeHandle return value is not accessed via
  // ref — it's available through the component's internal wiring.
  // This pattern is useful when you need to call imperative methods
  // on a child component without exposing its entire element.
  return (
    <VisualElement>
      <FancyInput />
    </VisualElement>
  );
}`

export const KEY_BASIC_EXAMPLE = `component TodoList {
  var (items, setItems) = useState(new List<string> { "Buy milk", "Walk dog" });

  return (
    <VisualElement>
      @foreach (var item in items) {
        // key preserves element identity across re-renders
        return (<Label text={item} key={item} />);
      }
    </VisualElement>
  );
}`

export const KEY_INDEX_ANTIPATTERN = `// BAD — using index as key causes issues when list order changes
@for (var i = 0; i < items.Count; i++) {
  return (<Label text={items[i]} key={i.ToString()} />);
}

// GOOD — use a stable, unique identifier
@foreach (var todo in todos) {
  return (<TodoItem todo={todo} key={todo.Id.ToString()} />);
}`

export const KEY_REORDER_EXAMPLE = `component ReorderDemo {
  var (items, setItems) = useState(new List<TodoItem> {
    new("A", "First"), new("B", "Second"), new("C", "Third")
  });

  return (
    <VisualElement>
      <Button text="Reverse" onClick={_ => setItems(prev => {
        var copy = new List<TodoItem>(prev);
        copy.Reverse();
        return copy;
      })} />

      @foreach (var item in items) {
        // Stable key ensures the reconciler moves elements, not recreates
        return (
          <VisualElement key={item.Id}>
            <Label text={item.Label} />
          </VisualElement>
        );
      }
    </VisualElement>
  );
}`

export const KEY_RESET_EXAMPLE = `component UserProfile(string userId) {
  // Changing key forces full unmount + remount of the component
  return (
    <ProfileContent key={userId} userId={userId} />
  );
}

component ProfileContent(string userId) {
  // All hooks reset when key changes — fresh state for each user
  var (data, setData) = useState<UserData>(null);

  useEffect(() => {
    LoadUserAsync(userId, setData);
    return null;
  }, userId);

  return (
    <VisualElement>
      @if (data != null) {
        return (<Label text={$"Name: {data.Name}"} />);
      } @else {
        return (<Label text="Loading..." />);
      }
    </VisualElement>
  );
}`
