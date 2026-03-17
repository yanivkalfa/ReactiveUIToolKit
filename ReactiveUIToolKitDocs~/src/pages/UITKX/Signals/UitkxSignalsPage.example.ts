export const UITKX_SIGNALS_COMPONENT_EXAMPLE = `@using ReactiveUITK.Signals
@using System

component SignalCounterDemo {
  var counterSignal = useMemo(() => SignalFactory.Get<int>("demo.counter", 0), Array.Empty<object>());
  var count = useSignal(counterSignal);

  return (
    <VisualElement>
      <Text text="Signal Counter" />
      <Text text={$"Count: {count}"} />
      <VisualElement style={new Style { (StyleKeys.FlexDirection, "row") }}>
        <Button text="Increment" onClick={_ => counterSignal.Dispatch(v => v + 1)} />
        <Button text="Reset" onClick={_ => counterSignal.Dispatch(0)} />
      </VisualElement>
    </VisualElement>
  );
}`

export const UITKX_SIGNALS_RUNTIME_EXAMPLE = `using ReactiveUITK.Signals;

SignalsRuntime.EnsureInitialized();
var counter = Signals.Get<int>("demo.counter", 0);
counter.Dispatch(previous => previous + 1);`
