import type { FC } from 'react'
import { Box, List, ListItem, ListItemText, Typography } from '@mui/material'
import { CodeBlock } from '../../components/CodeBlock/CodeBlock'
import Styles from './SignalsHooksPage.style'

const SIGNAL_HOOKS_BASIC = `using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using ReactiveUITK.Signals;

public static class SignalHooksDemoFunc
{
  private static readonly Signal<int> CounterSignal =
    Signals.Get<int>("demo.counter", 0);

  // Function component – pass SignalHooksDemoFunc.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    int value = Hooks.UseSignal(CounterSignal);

    void Increment()
    {
      CounterSignal.Dispatch(v => v + 1);
    }

    return V.Column(
      key: null,
      V.Label(new LabelProps { Text = $"Count from signal: {value}" }),
      V.Button(new ButtonProps { Text = "Increment", OnClick = Increment })
    );
  }
}`

export const SignalsHooksPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Special signal hooks
    </Typography>
    <Typography variant="body1" paragraph>
      Signals provide a small, global, observable state primitive. The{' '}
      <code>Hooks.UseSignal</code> family gives you fine-grained reactivity from function
      components, something React does not have out of the box.
    </Typography>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        <code>Hooks.UseSignal</code>
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                <code>Hooks.UseSignal(Signal&lt;T&gt;)</code> – subscribe to a{' '}
                <code>Signal&lt;T&gt;</code> and re-render when it changes.
              </>
            }
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                <code>Hooks.UseSignal&lt;T&gt;(key, initialValue)</code> – shorthand that resolves a{' '}
                <code>Signal&lt;T&gt;</code> from the global registry by key.
              </>
            }
          />
        </ListItem>
      </List>
      <CodeBlock language="tsx" code={SIGNAL_HOOKS_BASIC} />
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Selector overloads
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                <code>Hooks.UseSignal&lt;T, TSlice&gt;(signal, selector, comparer)</code> – project
                a slice of a signal value and control equality.
              </>
            }
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                <code>
                  Hooks.UseSignal&lt;T, TSlice&gt;(key, selector, comparer, initialValue)
                </code>{' '}
                – keyed variant that creates/resolves the signal for you.
              </>
            }
          />
        </ListItem>
      </List>
    </Box>

    <Typography variant="body2" sx={Styles.section}>
      For an end-to-end walkthrough, see the Signals page, which shows how to combine{' '}
      <code>Signals.Get</code>, <code>Hooks.UseSignal</code>, and dispatch helpers in real UIs.
    </Typography>
  </Box>
)
