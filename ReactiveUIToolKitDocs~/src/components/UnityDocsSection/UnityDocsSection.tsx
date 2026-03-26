import type { FC } from 'react'
import { Box, Link, Typography } from '@mui/material'
import { UNITY_DOC_LINKS, buildUnityDocUrl, type UnityComponentName } from './unityDocLinks'
import { useSelectedVersion } from '../../contexts/VersionContext'

interface UnityDocsSectionProps {
  componentName: UnityComponentName
}

export const UnityDocsSection: FC<UnityDocsSectionProps> = ({ componentName }) => {
  const info = UNITY_DOC_LINKS[componentName]
  const { selectedVersion } = useSelectedVersion()
  if (!info) {
    return null
  }
  const linkLabel = info.label ?? `${componentName} entry`
  const href = buildUnityDocUrl(info.unityElement, selectedVersion)
  return (
    <Box sx={{ mt: 2 }}>
      <Typography variant="h5" component="h2" gutterBottom>
        Unity docs
      </Typography>
      <Typography variant="body1" paragraph>
        Review the{' '}
        <Link href={href} target="_blank" rel="noreferrer">
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
