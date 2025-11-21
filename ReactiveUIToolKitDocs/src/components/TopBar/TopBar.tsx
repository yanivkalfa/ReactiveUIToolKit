import type { FC } from 'react'
import { AppBar, Toolbar, Chip, Paper, InputBase, Box, IconButton, Link, Typography } from '@mui/material'
import { Link as RouterLink } from 'react-router-dom'
import SearchIcon from '@mui/icons-material/Search'
import GitHubIcon from '@mui/icons-material/GitHub'
import Styles from './TopBar.style'
import { PACKAGE_VERSION } from '../../version'

export type TopBarProps = { onOpenSearch: () => void }

export const TopBar: FC<TopBarProps> = ({ onOpenSearch }) => (
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
      </Box>
      <Box sx={Styles.center}>
        <Paper sx={Styles.searchPaper} variant="outlined" onClick={onOpenSearch}>
          <SearchIcon fontSize="small" />
          <InputBase placeholder="Search docs…" sx={Styles.inputFlex} readOnly autoFocus />
        </Paper>
      </Box>
      <Box sx={Styles.right}>
        <Chip label="Unity 6.2+" size="small" />
        <IconButton component={Link} href="https://github.com/yanivkalfa/ReactiveUIToolKit.git" target="_blank" rel="noreferrer">
          <GitHubIcon />
        </IconButton>
      </Box>
    </Toolbar>
  </AppBar>
)
