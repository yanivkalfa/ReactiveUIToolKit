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
