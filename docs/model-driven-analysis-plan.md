# Model-Driven Code Analysis Refactor

## Objective

Make the generated architecture model the source queried by analysis rules while preserving Standard Element Type classification, every existing legacy diagnostic, and the established `project.stxjson` structure.

The model follows calls within the current project and stops at the first external-project method. That terminal type and method are recorded as a Dependency boundary. The graph covers the complete application architecture, including HTTP exposures, services, brokers, databases, filesystems, queues, and other external resources.

## Invariants

1. Standard Element Type remains the primary architecture classification.
2. The sample continues to produce every legacy `STX*` diagnostic exactly once.
3. Existing class, property, method, link, and diagnostic output remains compatible.
4. Roslyn-specific state remains in memory and is omitted from JSON.
5. Serialized identities and relationships contain no Roslyn types and remain usable in a browser.
6. Rules migrate individually and must demonstrate parity before their old implementation is removed.
7. RFC diagnostics are additive and are excluded from the legacy parity comparison.

## Delivery phases

1. Capture fingerprints and explicit tests for the current sample architecture and legacy diagnostics.
2. Add stable method identities, direct calls, direct throws, catches, and dependency-boundary facts to the existing model pipeline.
3. Add cycle-safe graph processing for internal calls, exception propagation, wrapping, and first-hop dependencies.
4. Add model query services used by rule implementations.
5. Migrate existing rule families incrementally, comparing old and new diagnostic results.
6. Move RFC rules to model queries and add error-path rules only after legacy parity is proven.
7. Validate compact JSON output and a browser-oriented low-level graph projection.

## Checkpoint policy

Each coherent phase is committed locally on `refactor/model-driven-analysis`. Builds and the full test suite must pass before each checkpoint.
