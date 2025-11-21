export const SIGNALS_EXAMPLE = `var counter = Signals.Create<int>(0);
var unsub = counter.Subscribe(v => Debug.Log($"value: {v}"));
counter.Value++;
unsub();`

