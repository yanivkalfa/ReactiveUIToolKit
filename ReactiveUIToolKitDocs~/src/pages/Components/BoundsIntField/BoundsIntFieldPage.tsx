import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './BoundsIntFieldPage.style'
import { UnityDocsSection } from '../../../components/UnityDocsSection/UnityDocsSection'
import { BOUNDS_INT_FIELD_BASIC } from './BoundsIntFieldPage.example'

export const BoundsIntFieldPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      BoundsIntField
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.BoundsIntField</code> wraps the Unity <code>BoundsIntField</code> control using{' '}
      <code>BoundsIntFieldProps</code> for working with integer bounds in both runtime UI and editor
      tools.
    </Typography>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="tsx" code={getPropsDoc('BoundsIntFieldProps')} />
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <Typography variant="body1" paragraph>
        Pass a <code>BoundsIntFieldProps</code> with an initial <code>BoundsInt</code> to render the
        field. Combine it with <code>Hooks.UseState</code> or signals to keep the value controlled.
      </Typography>
      <CodeBlock language="tsx" code={BOUNDS_INT_FIELD_BASIC} />
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Children
      </Typography>
      <Typography variant="body1">
        <code>BoundsIntField</code> does not support child nodes. Use the label slot to add context.
      </Typography>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Slots (label / visual input)
      </Typography>
      <Typography variant="body1" paragraph>
        Use the <code>Label</code> and <code>VisualInput</code> properties on{' '}
        <code>BoundsIntFieldProps</code> to configure the label and the internal input container.
        Both expect dictionaries; for example, you can build a label with{' '}
        <code>new LabelProps &#123; Text = "BoundsInt" &#125;.ToDictionary()</code> or provide a
        <code>VisualInput</code> dictionary that contains a nested <code>Style</code>.
      </Typography>
    </Box>
    <UnityDocsSection componentName="BoundsIntField" />
  </Box>
)

