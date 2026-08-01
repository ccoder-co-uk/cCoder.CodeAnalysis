# Compliance Rule Candidate Catalog

## Purpose

This catalog separates requirements that CodeAnalysis can prove from source from controls that require configuration, deployment, runtime, or organizational evidence. A diagnostic must have a deterministic trigger and deterministic evidence of compliance. The generated architecture model is the source of those facts: Standard Element Type classification, HTTP exposure metadata, method calls, escaping exception types, catch-to-response mappings, nullable outcomes, and first-hop Dependency boundaries.

The prefixes identify the authority behind each rule family:

- `RFC`: IETF HTTP semantics, principally RFC 9110.
- `ODATA`: OASIS OData Version 4.01.
- `OWASP`: OWASP API Security Top 10 or ASVS guidance.
- `ISO`: ISO controls. This prefix is reserved; no source-only rule is presently justified by the public normative material reviewed.

`RFC` rules may include an explicitly adopted cCoder policy where RFC 9110 permits several valid responses. Such rules must be described as policy profiles rather than universal RFC requirements.

Implemented diagnostics within each compliance family use a contiguous sequence beginning at `0001`. Research candidates do not reserve diagnostic numbers; they receive a code only when implemented and published. A published diagnostic that is later retired keeps its historical number, and that number is never reused. `STXAPP005` is the explicit current example of such a retired, reserved gap.

## Recommended implementation wave

| Code | Requirement | Deterministic trigger | Required evidence | Exclusions | Model support | Authority |
|---|---|---|---|---|---|---|
| RFC0001 | A successful OData entity creation returns `201 Created` and the created representation. | OData controller CRUD `POST` with an entity body. | Every successful return path is `Created(...)`/201 and supplies the created entity. | Actions that do not create a resource; asynchronous acceptance. | Yes: HTTP metadata and response paths. | RFC 9110 [§15.3.2](https://www.rfc-editor.org/rfc/rfc9110.html#section-15.3.2); OData 4.01 [§9.1.2](https://docs.oasis-open.org/odata/odata/v4.01/os/part1-protocol/odata-v4.01-os-part1-protocol.html), [§11.4.2](https://docs.oasis-open.org/odata/odata/v4.01/os/part1-protocol/odata-v4.01-os-part1-protocol.html) |
| RFC0002 | A successful CRUD deletion returns `204 No Content`. | OData controller CRUD `DELETE`. | Every successful return path is `NoContent()`/204 and supplies no content. | Delete operations intentionally returning a representation under a separately adopted contract. | Yes. | RFC 9110 [§9.3.5](https://www.rfc-editor.org/rfc/rfc9110.html#section-9.3.5), [§15.3.5](https://www.rfc-editor.org/rfc/rfc9110.html#section-15.3.5) |
| RFC0003 | A successful retrieval returning a representation uses `200 OK`. | OData controller CRUD `GET`/`GetAll`. | Successful entity/collection return paths are `Ok(...)`/200. | Conditional `304`, range `206`, null-valued OData properties using 204, redirects. | Yes. | RFC 9110 [§9.3.1](https://www.rfc-editor.org/rfc/rfc9110.html#section-9.3.1), [§15.3.1](https://www.rfc-editor.org/rfc/rfc9110.html#section-15.3.1) |
| RFC0004 | cCoder update profile: an update returning the updated representation uses `200 OK`. | OData CRUD `PUT`/`PATCH` whose success contract returns the updated entity. | Every representation-returning success path uses `Ok(entity)` or OData `Updated(entity)`. | `Prefer: return=minimal`/204; a `PUT` that creates a resource/201. | Yes. | RFC 9110 [§9.3.4](https://www.rfc-editor.org/rfc/rfc9110.html#section-9.3.4); OData 4.01 §9.1 and §11.4 |
| RFC0005 | Request validation failure maps to `400 Bad Request`. | An HTTP exposure has an escaping exception whose semantic category or transitional type-name convention is `Validation`. | A reachable catch at the exposure maps that exception to `BadRequest(...)`/400. | Authentication, authorization, conflict, and framework model-binding paths already terminated before the action. | Yes after catch/response queries; name matching is transitional. | RFC 9110 [§15.5.1](https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.1) |
| RFC0006 | An authentication failure maps to `401` and includes an authentication challenge. | A Security exposure handles an explicitly classified authentication/token exception. | Catch maps to 401 and the response path supplies/configures `WWW-Authenticate`. | Authenticated-but-forbidden callers; non-Security business authorization. | Partial: response headers/configuration must be added to the model. | RFC 9110 [§11.6.1](https://www.rfc-editor.org/rfc/rfc9110.html#section-11.6.1), [§15.5.2](https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.2) |
| RFC0007 | An authenticated caller denied an operation maps to `403 Forbidden`. | An HTTP exposure transitively calls an `AuthorizationBroker` operation or has an escaping authorization exception. | A reachable catch maps the classified authorization exception to `Forbid()`/403. | Authentication/token failures, deliberately concealed resources returned as 404. | Yes after call-chain and catch/response queries. | RFC 9110 [§15.5.4](https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.4) |
| RFC0008 | A keyed retrieval with no matching entity returns `404 Not Found`. | HTTP `GET` has a route/key parameter and its reachable retrieval result is nullable or compared with null. | All null/not-found branches terminate in `NotFound()`/404. | Collection GET; OData null-valued property semantics; deliberately concealed forbidden resource is still compliant. | Partial: needs result/null-flow facts. | RFC 9110 [§15.5.5](https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.5); OData 4.01 §11.2.3 |
| RFC0009 | A state or concurrency conflict maps to `409 Conflict`. | An HTTP mutation has an escaping exception explicitly classified as conflict, or transitionally containing `Concurrency` in its type name. | A reachable catch maps it to `Conflict(...)`/409. | ETag precondition failures, which are 412; missing required preconditions, which are 428. | Yes after catch/response queries. | RFC 9110 [§15.5.10](https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.10) |
| RFC0010 | A handler must not expose an unclassified exception as a successful response. | Any HTTP exposure catches `Exception` or another unclassified failure. | The path rethrows to approved terminal middleware or produces 500 without sensitive exception details. | Approved terminal exception middleware itself. | Partial: needs response-payload and handler-role facts. | RFC 9110 [§15.6.1](https://www.rfc-editor.org/rfc/rfc9110.html#section-15.6.1) |
| ODATA0001 | Creating an entity returns the created resource in the 201 response body. | OData entity-set create operation. | The created entity is passed to the result and serialized as the response representation. | `Prefer: return=minimal`, if the project adopts and implements that branch. | Yes. | OData 4.01 §9.1.2 and §11.4.2 |
| ODATA0002 | A request for a non-existent entity URL returns 404. | Keyed OData entity retrieval. | Same null-flow evidence as RFC0008. | Null structural properties (204) and single `$ref` cases where OData permits 204 or 404. | Partial. | OData 4.01 §11.2.3, §11.2.4, and §11.2.8 |
| ODATA0003 | Unsupported OData functionality is rejected, preferably with 501. | Exposure explicitly recognizes an OData query option/operation but the reachable implementation marks it unsupported. | Failure path returns `NotImplemented`/501. | Malformed syntax/400; capabilities that are simply not advertised and cannot reach the handler. | Partial: requires query-option/capability facts. | OData 4.01 [§9.3.1](https://docs.oasis-open.org/odata/odata/v4.01/os/part1-protocol/odata-v4.01-os-part1-protocol.html) and §11.2.5 |
| ODATA0004 | ETag-protected modification enforces `If-Match`; a mismatch returns 412 without mutation. | Model/metadata marks the resource as requiring optimistic concurrency. | The exposure checks `If-Match` before any mutating Dependency call; absent header maps to 428 and mismatch maps to 412. | Resources not configured for optimistic concurrency. | Not yet: requires ordered control-flow, headers, metadata annotations, and mutation facts. | OData 4.01 §8.2.4 and §11.4.1.1 |
| ODATA0005 | OData functions are side-effect free. | OData operation is classified as a function. | No reachable mutating broker or Dependency edge. | OData actions, which may have side effects; calls proven read-only. | Partial: needs function/action and dependency-effect classification. | OData 4.01 §6.3 |
| ODATA0006 | Successful actions with no return type use 204. | OData action has no declared return type. | Success path returns `NoContent()`/204. | Actions returning a value; actions creating a resource/201. | Yes once OData action metadata is modeled. | OData 4.01 §11.5.5.2 |
| OWASP0001 | API error responses do not disclose stack traces or internal exception details. | HTTP handler constructs a response from an exception object, stack trace, or unrestricted exception message. | Only approved problem codes/public messages reach the response payload. | Development-only handlers provably excluded from production; protected internal diagnostic channels. | Yes for directly modeled response expressions. | OWASP API Security 2023 [API8](https://owasp.org/API-Security/editions/2023/en/0xa8-security-misconfiguration/) |
| OWASP candidate: object authorization | Keyed API operations perform object-level authorization. | Public HTTP exposure accepts an object identifier and reaches a data/resource Dependency. | Before the first resource access or mutation, every path reaches the standardized authorization component for that object. | Explicitly anonymous/public resources; authentication endpoints; authorization wholly enforced by a proven external boundary. | Partial: needs ordered/dominance queries, not mere call presence. | OWASP API Security 2023 [API1](https://owasp.org/API-Security/editions/2023/en/0xa1-broken-object-level-authorization/) |
| OWASP candidate: function authorization | Privileged functions use the standardized authorization mechanism. | Administrative/privileged HTTP exposure, based on Standard Element Type or an explicit policy marker. | Every path invokes the authorization component or inherits a proven policy-bearing base exposure. | Explicitly public functions. | Partial: needs policy markers and dominance. | OWASP API Security 2023 [API5](https://owasp.org/API-Security/editions/2023/en/0xa5-broken-function-level-authorization/) |
| OWASP candidate: outbound destinations | Client-controlled outbound destinations are validated before the first external HTTP Dependency call. | Tainted HTTP input can flow to a URI/host argument of an outbound HTTP Dependency. | Every path passes the value through an approved URI validation/allow-list operation before the dependency edge. | Constant/configuration-owned destinations and typed identifiers that cannot encode a destination. | Not yet: requires parameter/data-flow and sanitization facts. | OWASP API Security 2023 [API7](https://owasp.org/API-Security/editions/2023/en/0xa7-server-side-request-forgery/) |
| OWASP candidate: resource limits | User-controlled collection/page/upload sizes are bounded before resource-intensive calls. | HTTP input controls a count, page size, collection size, upload, or repeated external operation. | An approved finite upper-bound validation dominates the resource-consuming edge. | Framework-enforced limits represented in the model/configuration. | Not yet: requires range/data-flow and configuration facts. | OWASP API Security 2023 [API4](https://owasp.org/API-Security/editions/2023/en/0xa4-unrestricted-resource-consumption/) |

## Important semantic boundaries

### Presence is not enough

An `AuthorizationBroker` call somewhere in a reachable chain does not prove authorization. The deferred object- and function-authorization candidates need ordered control flow: the check must dominate the protected resource access on every applicable path. The same applies to validation before mutation and URI validation before outbound HTTP.

### Exceptions require categories

Type-name matching (`Validation`, `Concurrency`) is useful for baseline discovery but is not a durable contract. The model should preserve fully qualified exception identities and an exception category derived first from inheritance or an explicit marker, then from naming only as a compatibility fallback.

### Middleware and controllers share an HTTP boundary, not a shape

All modeled HTTP request/response handlers are subject to response-semantic and disclosure rules. CRUD method-shape rules apply only to controller actions. Terminal middleware may translate an exception to 500; ordinary controllers and middleware should normally rethrow unexpected failures to that terminal handler.

## Deferred controls: useful, but not honest static diagnostics yet

The following controls should appear in a compliance report and runtime test suite, not as compiler diagnostics based only on the current method/call/exception model:

| Control | Why static proof is insufficient | Official source |
|---|---|---|
| TLS for all API and downstream communication | Deployment endpoints, proxies, certificate policy, and environment configuration determine the result. | OWASP API8:2023 |
| CORS and security headers | Effective policy is assembled by host configuration and middleware order and must be exercised over HTTP. | OWASP API8:2023 |
| Rate limiting, memory/time/upload/page limits, and third-party spending limits | Several controls live outside source or vary by deployment and business policy. | OWASP API4:2023 |
| Complete deployed API/version inventory | Source inventory is only one input; gateways and deployments determine exposure. | OWASP API9:2023 |
| Safe third-party API consumption | Requires trust-boundary, schema, TLS, timeout, redirect, and runtime response validation evidence. | OWASP API10:2023 |

## ISO position

Do not emit `ISOxxxx` diagnostics yet. [ISO/IEC 25010:2023](https://www.iso.org/standard/78176.html) defines a product quality model for specifying and evaluating quality characteristics; the public ISO material does not prescribe the concrete controller, call-chain, or status-code shapes needed for a deterministic source diagnostic. ISO/IEC 27001 and related management-system standards similarly depend on organizational scope, risk treatment, operating evidence, and often licensed normative text.

Claiming that a local code pattern is “ISO compliant” would therefore overstate what the analyzer proved. Reserve the `ISO` prefix and add rules only when cCoder adopts a named ISO requirement with licensed text, a documented applicability decision, and machine-verifiable evidence. Until then, model-derived measurements—complexity, dependency coupling, reliability/error-path coverage, security boundary coverage, and maintainability indicators—may support an ISO-oriented assessment but must not be labeled conformance diagnostics.

## Implementation priority

1. Complete model queries for escaping exceptions, catches, HTTP results, and null outcomes; implement RFC0005, RFC0007, RFC0008, and RFC0009.
2. Add authentication-challenge evidence and implement RFC0006 only for explicitly classified Security authentication paths.
3. Add OData action/function and optimistic-concurrency metadata; implement ODATA0001, ODATA0002, ODATA0005, and ODATA0006. Keep ODATA0004 as an audit until ordered mutation facts exist.
4. Add control-flow dominance and effect classification before implementing the deferred object- and function-authorization candidates.
5. Add taint and range data flow before implementing the deferred outbound-destination and resource-limit candidates.
6. Cover all deferred controls with aggregate runtime conformance tests and deployment evidence.
