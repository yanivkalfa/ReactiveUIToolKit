export const EVENTS_CLICK_EXAMPLE = `component ClickDemo {
  var (message, setMessage) = useState("Click the button");

  return (
    <VisualElement>
      <Label text={message} />
      <Button text="Click me"
              onClick={e => setMessage($"Clicked at {e.Position}")} />
    </VisualElement>
  );
}`

export const EVENTS_POINTER_EXAMPLE = `component PointerTracker {
  var (pos, setPos) = useState(Vector2.zero);

  return (
    <VisualElement style={new Style { Width = Pct(100), Height = Px(200) }}
                   onPointerMove={e => setPos(e.Position)}>
      <Label text={$"Pointer: {pos.x:F0}, {pos.y:F0}"} />
    </VisualElement>
  );
}`

export const EVENTS_KEYBOARD_EXAMPLE = `component KeyboardDemo {
  var (lastKey, setLastKey) = useState("None");

  return (
    <VisualElement focusable={true}
                   onKeyDown={e => setLastKey(e.KeyCode.ToString())}>
      <Label text={$"Last key: {lastKey}"} />
    </VisualElement>
  );
}`

export const EVENTS_FOCUS_EXAMPLE = `component FocusDemo {
  var (focused, setFocused) = useState(false);

  return (
    <TextField label="Name"
               onFocus={_ => setFocused(true)}
               onBlur={_ => setFocused(false)}
               style={new Style {
                 BorderColor = focused ? ColorBlue : ColorGray
               }} />
  );
}`

export const EVENTS_GEOMETRY_EXAMPLE = `component ResizeWatcher {
  var (size, setSize) = useState(Rect.zero);

  return (
    <VisualElement onGeometryChanged={e => setSize(e.NewRect)}
                   style={new Style { FlexGrow = 1f }}>
      <Label text={$"Size: {size.width:F0} x {size.height:F0}"} />
    </VisualElement>
  );
}`

export const EVENTS_CHANGE_EXAMPLE = `// Toggle — onChange receives ChangeEvent<bool>
<Toggle label="Enable" value={enabled}
        onChange={e => setEnabled(e.newValue)} />

// Slider — onChange receives ChangeEvent<float>
<Slider lowValue={0} highValue={100} value={volume}
        onChange={e => setVolume(e.newValue)} />

// TextField — onInput receives the new string directly
<TextField value={name}
           onInput={newValue => setName(newValue)} />`

export const EVENTS_PROPAGATION_EXAMPLE = `component PropagationDemo {
  return (
    <VisualElement onClick={_ => Debug.Log("Parent clicked")}>
      <Button text="Stop propagation"
              onClick={e => {
                e.StopPropagation();
                Debug.Log("Button clicked — parent won't fire");
              }} />
    </VisualElement>
  );
}`

export const EVENTS_DRAG_EXAMPLE = `// Editor-only — requires UNITY_EDITOR
component DragTarget {
  var (isDragOver, setIsDragOver) = useState(false);

  return (
    <VisualElement
      onDragEnter={_ => setIsDragOver(true)}
      onDragLeave={_ => setIsDragOver(false)}
      onDragPerform={_ => {
        setIsDragOver(false);
        Debug.Log("Drop received!");
      }}
      style={new Style {
        BackgroundColor = isDragOver ? Rgba(0, 120, 255, 0.2f) : ColorTransparent,
        Width = Px(200), Height = Px(200),
        BorderWidth = Px(2),
        BorderColor = isDragOver ? ColorBlue : ColorGray,
      }} />
  );
}`

export const EVENTS_CAPTURE_EXAMPLE = `// Capture-phase handler fires BEFORE the target's bubble handler.
// Useful for intercepting events before children process them.
component CaptureDemo {
  var (log, setLog) = useState("");

  return (
    <VisualElement
      onClickCapture={e => setLog("Capture on parent")}
      onClick={_ => setLog(log + " → Bubble on parent")}>
      <Button text="Click me"
              onClick={e => {
                setLog(log + " → Bubble on button");
                e.StopPropagation();
              }} />
      <Label text={log} />
    </VisualElement>
  );

  // Clicking the button logs: "Capture on parent → Bubble on button"
  // The parent's bubble handler is skipped because StopPropagation()
  // only stops further bubbling — capture already ran.
}`
