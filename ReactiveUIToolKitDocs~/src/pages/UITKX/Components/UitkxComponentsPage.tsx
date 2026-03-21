import type { FC } from 'react'
import { Box, List, ListItem, ListItemText, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from '../../Components/Button/ButtonPage.style'

const COMPONENT_SAMPLE = `component ButtonShowcase {
  var (enabled, setEnabled) = useState(true);

  return (
    <VisualElement>
      <Text text={$"Enabled: {enabled}"} />
      <Button
        text={enabled ? "Disable" : "Enable"}
        enabled={true}
        onClick={_ => setEnabled(previous => !previous)}
      />
      <Button
        text="Secondary action"
        enabled={enabled}
        onClick={_ => UnityEngine.Debug.Log("Clicked")}
      />
    </VisualElement>
  );
}`

export const UitkxComponentsPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Components in UITKX
    </Typography>
    <Typography variant="body1" paragraph>
      In the UITKX track, components are authored as markup using intrinsic tags like{' '}
      <code>&lt;VisualElement&gt;</code>, <code>&lt;Button&gt;</code>, <code>&lt;Text&gt;</code>,
      router tags, and your own custom components.
    </Typography>
    <Typography variant="body1" paragraph>
      The practical rule is simple: use intrinsic tags for built-in elements, and use PascalCase
      names for your own components. If you wrap a native element, consumers should use your custom
      component name, not the native one.
    </Typography>

    <CodeBlock language="tsx" code={COMPONENT_SAMPLE} />

    <Typography variant="h5" component="h2" gutterBottom>
      Authoring guidelines
    </Typography>
    <List>
      <ListItem disablePadding>
        <ListItemText primary="Prefer direct tag props over hand-building props objects when authoring UITKX." />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary="Keep setup code small and close to the returned markup tree." />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary="Use custom component names whenever a native tag name would collide." />
      </ListItem>
    </List>
  </Box>
)
