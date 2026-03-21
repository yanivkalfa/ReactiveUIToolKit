# UITKX Latency Targets

> Internal reference — performance thresholds for the UITKX language server and toolchain.

## LSP Response Targets

| Operation | Target | Notes |
|-----------|--------|-------|
| Tier-1/2 diagnostics (structural) | < 200 ms after keystroke | Language-lib only, no Roslyn |
| Tier-3 diagnostics (Roslyn) | < 2 s | Full Roslyn workspace rebuild; debounced |
| Completion response | < 500 ms | Includes Roslyn dot-completion |
| Hover response | < 500 ms | Roslyn type resolution |
| Format-on-save | < 1 s | Full file format + per-line TextEdit emission |
| Semantic tokens (full) | < 500 ms | UITKX structural + Roslyn combined |

## Source Generator Targets

| Operation | Target | Notes |
|-----------|--------|-------|
| Single-file incremental generation | < 500 ms | Per-keystroke in IDE |
| Full project generation | < 5 s | Cold start, all .uitkx files |

## Measurement Notes

- All targets are for a mid-range developer machine (8-core, 16GB RAM).
- LSP targets are measured from the moment the server receives the request to the moment the response is sent.
- Debounce delays (typically 300 ms) are not included in the target — they are additive.
- The Roslyn workspace rebuild is the primary bottleneck for Tier-3 diagnostics and completions.
- The language-lib (netstandard2.0) is designed to be allocation-light for the fast path.
