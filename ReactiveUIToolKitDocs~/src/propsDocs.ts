export type PropsDocsMap = Record<string, string>

declare const __PROPS_DOCS__: PropsDocsMap

export const propsDocs: PropsDocsMap = __PROPS_DOCS__

export const getPropsDoc = (name: string): string => propsDocs[name] ?? ''

