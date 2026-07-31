# CodeAnalysis Documentation Coverage Audit

## Scope and safety

Audit date: 31 July 2026.

This was a read-only comparison of:

1. diagnostic codes registered in `DiagnosticCodeStandardPageIndex` and referenced by the current rule processors;
2. the candidate compliance catalog in this directory; and
3. live pages returned by `GET https://ccoder.co.uk/Api/ContentManagement/Page?$expand=PageInfo,Contents&$top=1000`.

Authentication used the approved `ccoder/security` Windows Credential Manager entry in memory. No credential or bearer token was persisted. No POST, PUT, PATCH, or DELETE request was made against content.

## Current live structure

The documentation root is:

| Property | Value |
|---|---|
| Page ID | `12876` |
| Parent ID | `37` (Documentation) |
| App ID | `1` |
| Name/path | `CodeAnalysis` / `Documentation/CodeAnalysis` |
| Layout | `Documentation` |
| Resource key | `Default` |
| Cultures | invariant (`CultureId == ""`) only |

It has 17 diagnostic-family children and 90 rule-page grandchildren. The live families are `STX`, `STXA`, `STXAPI`, `STXAPP`, `STXB`, `STXC`, `STXD`, `STXE`, `STXEX`, `STXF`, `STXFORMAT`, `STXM`, `STXMG`, `STXO`, `STXP`, `STXSTRUCT`, and `STXTEST`.

Live structural preflight passed:

- every family page has exactly one `[component[DetailedNav]]`;
- all 90 rule pages have `What the Standard says` and `Why it matters` sections;
- all 90 rule pages contain language-classed `<pre><code>` examples;
- no rule page contains `public-actions` or `docs-rule-actions`;
- family and rule pages currently use only invariant `PageInfo` and invariant `body` content;
- all rule-page grandchildren have diagnostic-code names.

Rule pages intentionally end after their good example and do not contain `DetailedNav`; family pages own navigation for the rule set. This matches the established live structure and the rule-page ordering in the authoring guidance.

## Coverage result

| Set | Count |
|---|---:|
| Registered/indexed diagnostics | 106 |
| Live rule pages | 90 |
| Indexed diagnostics without a live page | 17 |
| Live rule pages without an indexed diagnostic | 1 |

### Indexed diagnostics missing live pages

| Family | Missing pages |
|---|---|
| RFC | `RFC0001`, `RFC0002`, `RFC0003`, `RFC0004` |
| STX | `STX0024` |
| STXAPP | `STXAPP010`, `STXAPP011`, `STXAPP012`, `STXAPP013`, `STXAPP014`, `STXAPP015` |
| STXD | `STXD003`, `STXD004` |
| STXE | `STXE006`, `STXE007` |
| STXSTRUCT | `STXSTRUCT002`, `STXSTRUCT003` |

The RFC family page itself is also absent. The other 13 missing rule pages belong under existing family pages.

### Live page without a registered/indexed diagnostic

`STXAPP005` exists at `Documentation/CodeAnalysis/STXAPP/STXAPP005`, but it is absent from both the diagnostic index and current rule code. Treat it as a historical-page reconciliation item, not an automatic deletion. Before publication, determine whether the rule was retired, accidentally omitted, or renumbered. Preserve or redirect historical documentation deliberately.

## Candidate-catalog coverage

The research catalog contains these proposed families and ranges:

| Family | Catalog range | Current registration | Live documentation |
|---|---|---|---|
| RFC | `RFC0001`–`RFC0010` | `RFC0001`–`RFC0004` | No family or rule pages |
| ODATA | `ODATA0001`–`ODATA0006` | None | No family or rule pages |
| OWASP | `OWASP0001`–`OWASP0005` | None | No family or rule pages |
| ISO | Prefix reserved; no proposed diagnostic | None | None required |

Candidate entries are not promises to publish. Several require control-flow, taint, configuration, or runtime evidence that the model does not yet provide. There is also intentional semantic overlap in the research catalog—for example, RFC0001 and candidate ODATA0001 both address the created representation. The implementation should avoid producing duplicate diagnostics for one defect. Documentation pages must be generated from the final registered rule inventory, not from every research candidate.

If every candidate were ultimately accepted, the maximum additional compliance documentation would be three family pages and 21 rule pages. At present only the RFC family and its four implemented rules are publication-ready in principle; their wording must still be checked against the final model-driven implementation.

## Publication preflight plan

### Gate 1: freeze the final diagnostic inventory

Do not write live content until all of the following are green in the same commit:

- every registered rule is indexed to its authoritative source;
- the sample project triggers every rule exactly once;
- no diagnostic is emitted without an index entry;
- no indexed diagnostic lacks a rule implementation unless explicitly marked reserved;
- final codes, descriptions, severity, trigger, and exclusions are stable;
- overlapping RFC/OData/OWASP candidates have been consolidated or assigned distinct evidence.

Generate a machine-checked manifest containing code, family, exact diagnostic description, authority URLs, bad-example trigger, good-example evidence, and sample location. Use that manifest as the page preflight input.

### Gate 2: reconcile existing documentation

Before adding compliance pages:

1. Create the 13 missing current Standard rule pages under their existing family IDs.
2. Resolve `STXAPP005` explicitly. Do not silently delete it.
3. Re-query the page tree and assert that the current registered Standard diagnostic set has exact page coverage.
4. Preserve the existing family and rule HTML structure.

### Gate 3: preflight compliance families

Create a family only if at least one rule in it is registered in the final build. Proposed placement beneath page `12876`:

| Family | Proposed title | Purpose |
|---|---|---|
| RFC | `RFC: HTTP semantics` | Status-code, authentication, error-path, and HTTP-boundary semantics adopted from IETF standards. |
| ODATA | `ODATA: OData protocol compliance` | OASIS OData-specific response, operation, query, and concurrency rules not already represented by RFC diagnostics. |
| OWASP | `OWASP: API security controls` | Statically provable API authorization, trust-boundary, resource, and disclosure protections. |

Place these after the current Standard families unless the user chooses another navigation policy. Allocate final `Order` values only after re-reading all CodeAnalysis children immediately before publication.

Each family page must:

- explain what the family protects and the consumer/maintainer value;
- distinguish normative protocol requirements from adopted cCoder policy profiles;
- tell readers to choose a diagnostic for sources, rationale, and examples;
- contain exactly one `[component[DetailedNav]]` in the standard navigation section;
- use invariant `PageInfo` and invariant `body` unless a localized content policy is adopted.

### Gate 4: draft each final rule page

For each final diagnostic, prepare one complete atomic page graph with invariant `PageInfo` and `body`. The body must follow the live rule-page order:

1. `docs-page-header` with code and exact analyzer description, with no action buttons;
2. `What the Standard says` in one self-contained paragraph;
3. inline authoritative `read it here` links;
4. `Why it matters` in one rule-specific paragraph;
5. one bad C# example that breaks only this diagnostic;
6. one good C# example that satisfies it.

Examples must follow every unrelated CodeAnalysis rule so that they correspond to the exactly-once diagnostic sample. Use Students, Courses, and Schools where neutral domain examples work; preserve `ValueTask` and Standard Element Type conventions. Use `<pre><code class="language-csharp">` and the existing syntax token classes.

Draft-specific requirements for the compliance families:

- RFC pages must quote or closely paraphrase the exact RFC section and identify cCoder policy where the RFC permits alternatives. RFC0004 must state that 200 applies when the updated representation is returned; 204 remains valid for a no-content update contract.
- RFC0006 must document that a 401 response includes `WWW-Authenticate`; RFC0007 must distinguish authenticated-but-forbidden 403 from authentication failure.
- RFC0009 must distinguish application concurrency conflict 409 from OData ETag mismatch 412 and missing required precondition 428.
- ODATA pages must cite the OASIS 4.01 OData Standard, not secondary Microsoft documentation.
- OWASP pages must describe exactly what static evidence is proved and must not claim that one analyzer rule establishes complete API security.
- Do not create ISO rule pages until an adopted, licensed, machine-verifiable ISO requirement exists.

### Gate 5: atomic creation and public access

Immediately before the first write:

- re-fetch the CodeAnalysis page, family children, rule children, cultures, and content records;
- assert the expected missing-page set and absence of name/path collisions;
- assert that every final registered diagnostic has exactly one draft and no extra draft exists;
- resolve the current `Guests` role for App ID 1 rather than assuming a role ID;
- POST each new page with its invariant `PageInfo` and invariant `body` in the same atomic graph;
- re-fetch the stored page and apply the resolved guest PageRole;
- never create a bare page to fill later.

### Gate 6: verification

After publication:

- re-fetch every created record and compare stored code, description, links, headings, examples, and component count with the approved drafts;
- confirm exact registered-diagnostic/page coverage;
- verify anonymous canonical rendering for every new family and at least every new rule-page batch;
- treat a 200 page containing the Login component as a visibility failure;
- confirm family pages have one DetailedNav and rule pages have no action rows;
- confirm no credentials, tokens, or temporary publisher files remain.

## Publication decision

No live mutation is appropriate yet. The model-driven refactor and final new rule inventory are still changing. Publishing now would create wording/code drift and might document research candidates that never become deterministic analyzers. The site should be updated in one preflighted pass after implementation, sample parity, and the full test suite are final.
