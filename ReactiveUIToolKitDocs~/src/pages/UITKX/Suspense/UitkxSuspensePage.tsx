import type { FC } from 'react'
import { Alert, Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from '../../GettingStarted/GettingStartedPage.style'
import { SUSPENSE_CALLBACK, SUSPENSE_TASK } from './UitkxSuspensePage.example'

export const UitkxSuspensePage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Suspense
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.Suspense</code> shows a fallback node while an asynchronous
      operation completes. Once the condition is met, it renders its children
      instead.
    </Typography>

    <Typography variant="h5" component="h2" gutterBottom>
      Overloads
    </Typography>
    <CodeBlock language="jsx" code={`// 1. Callback — re-checks isReady() each render
V.Suspense(Func<bool> isReady, VirtualNode fallback, key?, children)

// 2. Task — waits for the Task to complete
V.Suspense(Task readyTask, VirtualNode fallback, key?, children)

// 3. Both — shows fallback until isReady() && task completes
V.Suspense(Func<bool> isReady, Task readyTask, VirtualNode fallback, key?, children)`} />

    <Typography variant="h5" component="h2" gutterBottom>
      Callback mode
    </Typography>
    <Typography variant="body1" paragraph>
      Pass a <code>{'Func<bool>'}</code> that returns <code>true</code> when
      loading is complete. The reconciler re-evaluates the callback on each
      render cycle.
    </Typography>
    <CodeBlock language="jsx" code={SUSPENSE_CALLBACK} />

    <Typography variant="h5" component="h2" gutterBottom>
      Task mode
    </Typography>
    <Typography variant="body1" paragraph>
      Pass a <code>Task</code> directly. The fallback is shown until the task
      transitions to a completed state.
    </Typography>
    <CodeBlock language="jsx" code={SUSPENSE_TASK} />

    <Alert severity="info" sx={{ mt: 2 }}>
      You can also use <code>Hooks.SuspendUntil(task)</code> inside a component
      to trigger suspension imperatively. The nearest <code>Suspense</code>
      ancestor will catch it and show its fallback.
    </Alert>
  </Box>
)
