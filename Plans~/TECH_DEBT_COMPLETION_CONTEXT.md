# Tech Debt - Completion Context Leakage (`@code`)

## Summary
`@code` completion has repeatedly leaked into non-header contexts due to multiple overlapping completion paths (cursor-kind routing, context stack inference, and schema-driven item sources).

## Current Mitigation
A final post-filter in the LSP completion handler now removes `@code` from results whenever the cursor is not in the strict header zone.

## Why This Is Tech Debt
- The behavior is currently protected by a safety filter rather than a single canonical context classifier.
- Context decisions are split across several helpers, making regressions likely when one path changes.

## Follow-up (when prioritized)
- Consolidate all completion context gating into one authoritative function.
- Add explicit completion snapshot tests for: header, after `@code`, first markup lines, and embedded markup inside `@code`.
- Remove the safety filter after canonical gating + tests are stable.

## Current Deferred Behavior
- `@code` suggestion currently still appears at the header boundary/start area by design.
- We still need a stricter rule that hides `@code` specifically in the transition zone
	between regular code content and first markup when context classification is ambiguous.

---

# Tech Debt - Dead `memoize` / `memoCompare` Fields

## Summary
`VirtualNode.Memoize` and `VirtualNode.TypedMemoCompare` are stored but never read by the reconciler or fiber component machinery. The `memoize` and `memoCompare` parameters on `V.Func(...)` / `V.Func<TProps>(...)` are therefore no-ops.

## How It Works Today
Every function component already bails out unconditionally when:
- no pending state update, AND
- `IProps.Equals(pendingProps)` is true, AND
- no context change

This means all components are effectively memo'd by default through `IProps.Equals`. The `memoize` flag adds nothing on top of this.

## Affected Code (to remove)
- `VirtualNode.Memoize` property and all constructor assignments
- `VirtualNode.TypedMemoCompare` property and all constructor assignments
- `memoize` / `memoCompare` parameters on all `V.Func` overloads in `V.cs`
- `VirtualNode` copy-constructor line: `Memoize = template.Memoize` / `TypedMemoCompare = template.TypedMemoCompare`

## Why This Is Tech Debt
- The fields are populated through the entire call chain (`V.Func` -> `VirtualNode` -> `FiberNode`) but silently ignored at the point of use.
- Keeping them creates false confidence that `memoize: true` does something.
- Removing them simplifies the API surface and eliminates dead constructor parameters.

## Follow-up (when prioritized)
- Search all call sites for `memoize: true` in Samples and game projects before deleting.
- Remove all `memoize` / `memoCompare` parameters and fields.
- If a custom-comparator escape hatch is ever needed, design it as a real named feature, not a silent no-op parameter.
