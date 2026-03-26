import type { FC } from 'react'
import { AppBar, Toolbar, Chip, Paper, InputBase, Box, IconButton, Link, Typography, Select, MenuItem } from '@mui/material'
import { Link as RouterLink, useLocation } from 'react-router-dom'
import SearchIcon from '@mui/icons-material/Search'
import GitHubIcon from '@mui/icons-material/GitHub'
import Styles from './TopBar.style'
import { PACKAGE_VERSION } from '../../version'
import { allFlat, getMatchingPathInTrack, getTrackFromPath } from '../../docs'
import { SUPPORTED_VERSIONS } from '../../versionManifest'
import { useSelectedVersion } from '../../contexts/VersionContext'

export type TopBarProps = { onOpenSearch: () => void }

export const TopBar: FC<TopBarProps> = ({ onOpenSearch }) => {
  const { pathname } = useLocation()
  const track = getTrackFromPath(pathname)
  const currentPage = allFlat.find((page) => page.path === pathname)
  const canonicalId = currentPage?.canonicalId ?? 'introduction'
  const uitkxTarget = getMatchingPathInTrack('uitkx', canonicalId)
  const csharpTarget = getMatchingPathInTrack('csharp', canonicalId)
  const { selectedVersion, setSelectedVersion } = useSelectedVersion()

  return (
    <AppBar position="sticky" color="default" elevation={0} sx={Styles.appBar}>
      <Toolbar sx={Styles.toolbar}>
        <Box sx={Styles.left}>
          <Link component={RouterLink} to="/" underline="none" sx={Styles.titleLink}>
            <Box
              component="img"
              src="/logo.png"
              alt="ReactiveUIToolKit logo"
              sx={Styles.logo}
            />
            <Typography variant="h6" sx={Styles.title}>
              ReactiveUIToolKit
            </Typography>
          </Link>
          <Chip label={`v${PACKAGE_VERSION}`} size="small" />
          <Chip
            label="UITKX"
            size="small"
            color={track === 'uitkx' ? 'primary' : 'default'}
            component={RouterLink}
            to={uitkxTarget}
            clickable
          />
          <Chip
            label="C#"
            size="small"
            color={track === 'csharp' ? 'primary' : 'default'}
            component={RouterLink}
            to={csharpTarget}
            clickable
          />
        </Box>
        <Box sx={Styles.center}>
          <Paper sx={Styles.searchPaper} variant="outlined" onClick={onOpenSearch}>
            <SearchIcon fontSize="small" />
            <InputBase placeholder={`Search ${track === 'uitkx' ? 'UITKX' : 'C#'} docs…`} sx={Styles.inputFlex} readOnly autoFocus />
          </Paper>
        </Box>
        <Box sx={Styles.right}>
          <Select
            value={selectedVersion}
            onChange={(e) => setSelectedVersion(e.target.value)}
            size="small"
            sx={Styles.versionSelect}
          >
            {SUPPORTED_VERSIONS.map((v) => (
              <MenuItem key={v.version} value={v.version}>
                Unity {v.label}+
              </MenuItem>
            ))}
          </Select>
          <IconButton component={Link} href="https://github.com/yanivkalfa/ReactiveUIToolKit.git" target="_blank" rel="noreferrer">
            <GitHubIcon />
          </IconButton>
        </Box>
      </Toolbar>
    </AppBar>
  )
}
