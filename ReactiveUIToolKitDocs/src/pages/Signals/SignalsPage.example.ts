export const SIGNAL_RUNTIME_EXAMPLE = `using System;
using ReactiveUITK.Signals;
using UnityEngine;

// Runtime: global signal and subscription

public sealed class SignalsDemo : MonoBehaviour
{
  private IDisposable _subscription;

  private void Start()
  {
    // Ensure the runtime host exists
    SignalsRuntime.EnsureInitialized();

    var counter = Signals.Get<int>("demo-counter", 0);
    _subscription = counter.Subscribe(v => Debug.Log($"Counter changed to {v}"));

    // Update via functional Dispatch using previous value
    counter.Dispatch(previous => previous + 1);

    // Or assign a value directly
    counter.Dispatch(42);
  }

  private void OnDestroy()
  {
    _subscription?.Dispose();
  }
}`

export const SIGNAL_EDITOR_COMPONENT_EXAMPLE = `using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using ReactiveUITK.Signals;

// Function component bound to a signal
public static class SignalCounterFunc
{
  // Function component – pass SignalCounterFunc.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    // Reads and subscribes to the signal by key
    int value = Hooks.UseSignal<int>("demo-counter", initialValue: 0);

    void Increment()
    {
      var signal = Signals.Get<int>("demo-counter", 0);
      signal.Dispatch(previous => previous + 1);
    }

    return V.Row(
      key: null,
      V.Label(new LabelProps { Text = $"Value: {value}" }),
      V.Button(new ButtonProps { Text = "Increment", OnClick = Increment })
    );
  }
}`
