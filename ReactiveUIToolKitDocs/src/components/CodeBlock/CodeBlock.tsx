import { Highlight, themes } from 'prism-react-renderer'
import type { Language } from 'prism-react-renderer'
import { Box, IconButton, Tooltip } from '@mui/material'
import ContentCopyIcon from '@mui/icons-material/ContentCopy'
import type { FC } from 'react'
import { useCallback, useState } from 'react'
import Styles from './CodeBlock.style'

type Props = {
  code: string
  language?: Language
}

export const CodeBlock: FC<Props> = ({ code, language = 'tsx' }) => {
  const [copied, setCopied] = useState(false)
  const onCopy = useCallback(() => {
    navigator.clipboard.writeText(code).then(() => {
      setCopied(true)
      setTimeout(() => setCopied(false), 1200)
    })
  }, [code])

  return (
    <Box sx={Styles.wrapper}>
      <Box sx={Styles.copyBtnWrap}>
        <Tooltip title={copied ? 'Copied' : 'Copy'}>
          <IconButton size="small" onClick={onCopy} sx={Styles.copyBtn}>
            <ContentCopyIcon fontSize="inherit" />
          </IconButton>
        </Tooltip>
      </Box>
      <Highlight theme={themes.oneDark} code={code.trim()} language={language}>
        {({ className, style, tokens, getLineProps, getTokenProps }) => (
          <pre className={className} style={style}>
            {tokens.map((line, i) => (
              <div key={i} {...getLineProps({ line })}>
                {line.map((token, key) => (
                  <span key={key} {...getTokenProps({ token })} />
                ))}
              </div>
            ))}
          </pre>
        )}
      </Highlight>
    </Box>
  )
}
