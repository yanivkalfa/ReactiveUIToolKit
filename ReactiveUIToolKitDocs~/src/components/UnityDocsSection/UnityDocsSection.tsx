import type { FC } from 'react'
import { Box, Link, Typography } from '@mui/material'
import { UNITY_DOC_LINKS, type UnityComponentName } from './unityDocLinks'

interface UnityDocsSectionProps {
  componentName: UnityComponentName
}

export const UnityDocsSection: FC<UnityDocsSectionProps> = ({ componentName }) => {
  const info = UNITY_DOC_LINKS[componentName]
  if (!info) {
    return null
  }
  const linkLabel = info.label ?? `${componentName} entry`
  return (
    <Box sx={{ mt: 2 }}>
      <Typography variant="h5" component="h2" gutterBottom>
        Unity docs
      </Typography>
      <Typography variant="body1" paragraph>
        Review the{' '}
        <Link href={info.href} target="_blank" rel="noreferrer">
          {linkLabel}
        </Link>{' '}
        in the Unity manual for the official UI Toolkit reference.
      </Typography>
      {info.note && (
        <Typography variant="body2" color="text.secondary" paragraph>
          {info.note}
        </Typography>
      )}
    </Box>
  )
}
