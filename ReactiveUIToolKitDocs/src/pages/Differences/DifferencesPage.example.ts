export const STATE_COUNTER_EXAMPLE = `using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;

// Function component with UseState
public static VirtualNode CounterFunc(
  Dictionary<string, object> props,
  IReadOnlyList<VirtualNode> children
)
{
  var (count, setCount) = Hooks.UseState(0);

  // Direct value update
  void Reset() => setCount.Set(0);

  // Functional update using previous value
  void Increment() => setCount.Set(previous => previous + 1);

  return V.Column(
    key: null,
    V.Label(new LabelProps { Text = $"Count: {count}" }),
    V.Button(new ButtonProps { Text = "Increment", OnClick = Increment }),
    V.Button(new ButtonProps { Text = "Reset", OnClick = Reset })
  );
}`

export const SIGNAL_COUNTER_EXAMPLE = `using System;
using ReactiveUITK.Signals;
using UnityEngine;

// Global counter signal shared across components
var counter = Signals.Get<int>("global-counter", 0);

// Subscribe anywhere (editor or runtime)
var subscription = counter.Subscribe(value =>
{
  Debug.Log($"Counter changed to {value}");
});

// Update by value
counter.Set(counter.Value + 1);

// Or update using a function
counter.Dispatch(previous => previous + 1);

// Don't forget to dispose subscriptions you create manually
subscription.Dispose();`

export const USE_SIGNAL_HOOK_EXAMPLE = `using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using ReactiveUITK.Signals;

// Using a signal from a function component
public static VirtualNode CounterFromSignal(
  Dictionary<string, object> props,
  IReadOnlyList<VirtualNode> children
)
{
  // Reads the current value and re-renders when it changes
  int value = Hooks.UseSignal<int>("global-counter", initialValue: 0);

  void Increment()
  {
    var signal = Signals.Get<int>("global-counter", 0);
    signal.Dispatch(previous => previous + 1);
  }

  return V.Row(
    key: null,
    V.Label(new LabelProps { Text = $"Signal value: {value}" }),
    V.Button(new ButtonProps { Text = "Increment", OnClick = Increment })
  );
}`
