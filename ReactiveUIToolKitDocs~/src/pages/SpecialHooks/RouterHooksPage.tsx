import type { FC } from 'react'
import { Box, List, ListItem, ListItemText, Typography } from '@mui/material'
import { CodeBlock } from '../../components/CodeBlock/CodeBlock'
import Styles from './RouterHooksPage.style'

const ROUTER_HOOKS_BASIC = `using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using ReactiveUITK.Router;

// Demonstrates RouterHooks.UseNavigate, UseParams, and UseQuery.
public static class RouterHooksDemoFunc
{
  // Function component – pass RouterHooksDemoFunc.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var navigate = RouterHooks.UseNavigate();
    var parameters = RouterHooks.UseParams();
    var query = RouterHooks.UseQuery();

    string userId = parameters.TryGetValue("id", out var id) ? id : "(none)";

    void ToUser42()
    {
      navigate("/users/42?tab=details");
    }

    return V.Column(
      key: null,
      V.Row(
        key: "actions",
        V.Button(new ButtonProps { Text = "Go to User 42", OnClick = ToUser42 })
      ),
      V.Label(new LabelProps { Text = $"User id param: {userId}" }),
      V.Label(new LabelProps { Text = $"Query keys: {string.Join(\", \", query.Keys)}" })
    );
  }
}`

export const RouterHooksPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Special router hooks
    </Typography>
    <Typography variant="body1" paragraph>
      The router in ReactiveUIToolKit ships with a set of hooks that mirror React Router&apos;s
      ergonomics but are implemented entirely in C# for Unity UI Toolkit.
    </Typography>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Reading router state
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                <code>RouterHooks.UseLocation()</code> / <code>UseLocationInfo()</code> – current
                path, query, and optional navigation state.
              </>
            }
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                <code>RouterHooks.UseParams()</code> – path parameters extracted from the active
                route template.
              </>
            }
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                <code>RouterHooks.UseQuery()</code> – parsed query-string key/value pairs.
              </>
            }
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                <code>RouterHooks.UseNavigationState()</code> – arbitrary state object provided when
                navigating.
              </>
            }
          />
        </ListItem>
      </List>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Navigation helpers
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                <code>RouterHooks.UseNavigate(replace = false)</code> – imperative navigation,
                similar to React Router&apos;s <code>useNavigate</code>.
              </>
            }
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                <code>RouterHooks.UseGo()</code> – navigate relative to the history stack (for
                example, <code>go(-1)</code>, <code>go(1)</code>).
              </>
            }
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                <code>RouterHooks.UseCanGo(delta)</code> – returns whether a given delta is
                available for back/forward UI.
              </>
            }
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                <code>RouterHooks.UseBlocker(blocker, enabled)</code> – intercepts transitions to
                implement confirmation prompts.
              </>
            }
          />
        </ListItem>
      </List>
      <CodeBlock language="tsx" code={ROUTER_HOOKS_BASIC} />
    </Box>

    <Typography variant="body2" sx={Styles.section}>
      See the main Router documentation for complete examples of composing <code>V.Router</code>,{' '}
      <code>V.Route</code>, <code>V.Link</code>, and these hooks in editor and runtime apps.
    </Typography>
  </Box>
)
