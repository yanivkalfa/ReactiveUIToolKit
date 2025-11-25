import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import fs from 'node:fs'
import path from 'node:path'

const unityPackageJsonPath = path.resolve(process.cwd(), '..', 'package.json')
const unityPackageJsonRaw = fs.readFileSync(unityPackageJsonPath, 'utf-8')
const unityPackageJson = JSON.parse(unityPackageJsonRaw.replace(/^\uFEFF/, ''))

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  define: {
    __PACKAGE_VERSION__: JSON.stringify(unityPackageJson.version),
  },
  css: {
    postcss: './postcss.config.cjs',
  },
})
