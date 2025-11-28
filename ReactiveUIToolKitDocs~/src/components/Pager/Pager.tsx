import type { FC } from 'react'
import { useMemo } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { Box, Button } from '@mui/material'
import { flat } from '../../pages'
import Styles from './Pager.style'

export const Pager: FC = () => {
  const nav = useNavigate()
  const { pathname } = useLocation()
  const idx = useMemo(() => flat.findIndex((p) => p.path === pathname), [pathname])
  const prev = idx > 0 ? flat[idx - 1] : undefined
  const next = idx >= 0 && idx < flat.length - 1 ? flat[idx + 1] : undefined
  return (
    <Box sx={Styles.root}>
      <span>{prev && <Button onClick={() => nav(prev.path)} variant="text">← {prev.title}</Button>}</span>
      <span>{next && <Button onClick={() => nav(next.path)} variant="text">{next.title} →</Button>}</span>
    </Box>
  )
}
