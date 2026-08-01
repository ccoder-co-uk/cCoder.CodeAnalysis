# CodeAnalysis Documentation Coverage Audit

## Scope and result

Publication and verification date: 1 August 2026.

The live `Documentation/CodeAnalysis` tree on `https://ccoder.co.uk` documents every diagnostic registered in `DiagnosticCodeStandardPageIndex`:

| Set | Count |
|---|---:|
| Registered diagnostics | 123 |
| Matching live rule pages | 123 |
| Registered diagnostics without a live page | 0 |
| Historical live pages not currently registered | 1 (`STXAPP005`) |

`STXAPP005` remains at `Documentation/CodeAnalysis/STXAPP/STXAPP005`. It was deliberately preserved as historical documentation and was not renamed, redirected, or deleted.

## Pages added

Three compliance families were added beneath CodeAnalysis:

| ID | Family | Route |
|---:|---|---|
| 13203 | RFC | `Documentation/CodeAnalysis/RFC` |
| 13204 | ODATA | `Documentation/CodeAnalysis/ODATA` |
| 13205 | OWASP | `Documentation/CodeAnalysis/OWASP` |

The following previously missing Standard rule pages were added:

| IDs | Rules |
|---|---|
| 13206 | `STX0024` |
| 13207–13212 | `STXAPP010`–`STXAPP015` |
| 13213–13214 | `STXD003`–`STXD004` |
| 13215–13216 | `STXE006`–`STXE007` |
| 13217–13218 | `STXSTRUCT002`–`STXSTRUCT003` |
| 13238 | `STXAPI005` |
| 13239 | `STXF004` |

The final implemented compliance inventory was added:

| IDs | Rules |
|---|---|
| 13219–13228 | `RFC0001`–`RFC0010` |
| 13229–13231 | `ODATA0001`–`ODATA0003` |
| 13232 | `OWASP0001` |
| 13233–13237 | `RFC0011`–`RFC0015` |

All paths use the canonical pattern `Documentation/CodeAnalysis/{Family}/{Code}`.

## Publication controls

- The production page tree and current Guests role were re-read immediately before publication.
- Every page was posted as a complete graph containing invariant `PageInfo` and invariant `body` content.
- Each page was re-fetched from its keyed API endpoint after creation.
- The current App 1 Guests role (`1596ce23-b9e7-4a81-6f24-08d70c49f59a`) was applied through `Api/ContentManagement/PageRole` during creation.
- Page 13232, its invariant PageInfo, and its invariant body were later updated in place to correct `OWASP0004` to `OWASP0001`; stable identity, parent, order, layout, role assignments, and article structure were preserved.
- Credentials and bearer tokens remained inside the approved runtime helper and were not persisted.

## Stored-content verification

The post-publication API read returned 37 new pages with IDs 13203–13239. Structural checks passed:

- each new family has exactly one `[component[DetailedNav]]`;
- every new rule has `What the Standard says` and `Why it matters` headings;
- every new rule has bad and good `<pre><code class="language-csharp">` examples;
- no new rule contains `public-actions`, `docs-rule-actions`, or `DetailedNav`;
- the final live rule set contains all 123 registered codes;
- `STXAPP005` is still present.

## Anonymous rendering verification

Every new canonical route was fetched without authentication and with a cache-bypass query value. All 37 returned HTTP 200 and contained the expected family or diagnostic name. The returned documents contained the normal public navigation login link and theme CSS, but no rendered Login component in the page body.

Representative routes verified include:

- `https://ccoder.co.uk/Documentation/CodeAnalysis/RFC`
- `https://ccoder.co.uk/Documentation/CodeAnalysis/RFC/RFC0006`
- `https://ccoder.co.uk/Documentation/CodeAnalysis/RFC/RFC0015`
- `https://ccoder.co.uk/Documentation/CodeAnalysis/ODATA/ODATA0003`
- `https://ccoder.co.uk/Documentation/CodeAnalysis/OWASP/OWASP0001`
- `https://ccoder.co.uk/Documentation/CodeAnalysis/STXAPP/STXAPP015`
- `https://ccoder.co.uk/Documentation/CodeAnalysis/STXAPI/STXAPI005`
- `https://ccoder.co.uk/Documentation/CodeAnalysis/STXF/STXF004`

## Deferred families

No ISO family or rule page was created. An ISO-labelled diagnostic should be published only after the project adopts a licensed, machine-verifiable requirement and implements a corresponding analyzer rule. Research-only OData and OWASP candidates were likewise not documented as implemented diagnostics.

## Post-publication numbering correction

The implemented OWASP diagnostic and its live page were renumbered from `OWASP0004` to `OWASP0001`, because unpublished research candidates do not reserve diagnostic numbers. The updated canonical route returned HTTP 200 anonymously with the corrected title and article references; the obsolete `OWASP0004` route returned HTTP 404. `STXAPP005` remains a retired, reserved historical code and is not available for reuse.
