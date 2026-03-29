export type PropsDocsMap = Record<string, string>
export type PropEntry = { name: string; type: string; inherited: boolean }
export type PropsTableMap = Record<string, PropEntry[]>

declare const __PROPS_DOCS__: PropsDocsMap
declare const __PROPS_TABLE__: PropsTableMap

export const propsDocs: PropsDocsMap = __PROPS_DOCS__
export const propsTableData: PropsTableMap = __PROPS_TABLE__

export const getPropsDoc = (name: string): string => propsDocs[name] ?? ''
export const getPropsTable = (name: string): PropEntry[] => propsTableData[name] ?? []
