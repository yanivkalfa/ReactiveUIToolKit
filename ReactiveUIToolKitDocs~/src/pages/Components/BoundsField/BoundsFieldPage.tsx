import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './BoundsFieldPage.style'
import { UnityDocsSection } from '../../../components/UnityDocsSection/UnityDocsSection'
import { BOUNDS_FIELD_BASIC } from './BoundsFieldPage.example'

export const BoundsFieldPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      BoundsField
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.BoundsField</code> wraps the Unity <code>BoundsField</code> control using{' '}
      <code>BoundsFieldProps</code>. It is useful for editing <code>Bounds</code> values in both
      runtime UI and editor tools.
    </Typography>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="jsx" code={getPropsDoc('BoundsFieldProps')} />
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <Typography variant="body1" paragraph>
        Pass a <code>BoundsFieldProps</code> instance to <code>V.BoundsField</code>. The{' '}
        <code>Value</code> property controls the current bounds.
      </Typography>
      <CodeBlock language="jsx" code={BOUNDS_FIELD_BASIC} />
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Children
      </Typography>
      <Typography variant="body1">
        <code>BoundsField</code> does not accept child nodes; all configuration is done through{' '}
        <code>BoundsFieldProps</code>.
      </Typography>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Slots (label / visual input)
      </Typography>
      <Typography variant="body1" paragraph>
        Use the <code>Label</code> and <code>VisualInput</code> properties to style the label and
        the internal input container. Both expect dictionaries – you can compose them using other
        typed props (for example <code>LabelProps.ToDictionary()</code>) or by building a{' '}
        <code>Style</code> instance.
      </Typography>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Controlled value
      </Typography>
      <Typography variant="body1">
        Use <code>Hooks.UseState</code> (or a signal) to hold the current <code>Bounds</code> and
        update it from a change handler. The example above uses a local state tuple and updates the
        value via <code>setBounds(evt.newValue)</code> (you can also use the optional{' '}
        <code>StateSetterExtensions.Set</code> helper if you prefer method syntax).
      </Typography>
    </Box>
    <UnityDocsSection componentName="BoundsField" />
  </Box>
)

