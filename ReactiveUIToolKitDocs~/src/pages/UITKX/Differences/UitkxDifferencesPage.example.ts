export const UITKX_STATE_COUNTER_EXAMPLE = `component StateCounterExample {
  var (count, setCount) = useState(0);

  return (
    <VisualElement>
      <Text text={$"Count: {count}"} />
      <Button text="Increment" onClick={_ => setCount(previous => previous + 1)} />
      <Button text="Reset" onClick={_ => setCount(0)} />
    </VisualElement>
  );
}`
