---
id: std-API
tier: normative
status: draft
implements:
  - pol-INTC.BREAK
  - pol-INTC.DEPREC
  - pol-INTC.EXPOSE
  - pol-INTC.HOLDS
  - pol-INTC.SECURE
  - pol-INTC.SPEC
  - pol-INTC.VERSION
applies-to:
  - all
review-by: "2027-08-31"
owner: paul.law
tags: [ api-versioning, contracts, interfaces ]
---

# An interface is described by a contract in the repository

`Standard: std-API` `DRAFT`

## Summary

Every interface we publish has a machine-readable contract committed beside the code. The build checks the
implementation against it, and a change that consumers cannot absorb ships as a new version.

## Rules

### The contract is the source of truth

- A service **MUST** publish a machine-readable contract for every interface it exposes, committed to the repository
  that builds the service ([pol-INTC].SPEC).
- Where the implementation and the contract disagree, a team **MUST** treat the implementation as the defect
  ([pol-INTC].SPEC).
- A build **MUST** fail where the implementation no longer answers the contract ([pol-INTC].HOLDS).
- A consumer **MUST** test against the published contract rather than against a running instance of the provider
  ([pol-INTC].HOLDS).

### Every endpoint authenticates and validates

- An endpoint **MUST** authenticate the caller before it does any work ([pol-INTC].SECURE).
- An endpoint **MUST** authorise the operation before it does any work ([pol-INTC].SECURE).
- An endpoint **MUST** validate each field it receives against the contract, and reject a request that does not match
  ([pol-INTC].SECURE).
- An interface **MUST NOT** serve personal data, a secret, or an operation that changes state, to an unauthenticated
  caller ([pol-INTC].EXPOSE).

### A change carries a version and a notice

- A published interface **MUST** carry a version a consumer can pin to ([pol-INTC].VERSION).
- A breaking change **MUST** ship as a new version, with the previous version still answering ([pol-INTC].BREAK).
- A team **MUST** give notice before it removes a version, on the timescale the deprecation approach states
  ([pol-INTC].DEPREC).
- A deprecated version **MUST** answer with a `Deprecation` header and a `Sunset` header, so a consumer meets the
  notice in the traffic ([pol-INTC].DEPREC).

## Examples

```
Good
  GET /v2/covers/{isbn}
  Deprecation: Sat, 01 Aug 2026 00:00:00 GMT      on /v1 only
  Sunset: Tue, 01 Dec 2026 00:00:00 GMT

Avoid
  GET /covers/{isbn}
  # the response gained a required field on Tuesday
```

The avoided form has no version to pin, so a consumer written on Monday breaks on Tuesday and learns about it from a
support ticket.

```
Good
  covers-api/contract/openapi.yaml        committed, and the build asserts the routes against it

Avoid
  a Swagger page generated from the running service
```

A document generated from the implementation agrees with the implementation by construction, so it can never report a
change nobody meant to make.

## Conformance checklist

- [ ] The repository holds the contract, and a reviewer can read the diff to it in the pull request.
- [ ] The build fails when a route, a field or a status code drifts from the contract.
- [ ] Every path carries a version segment, and the previous version still answers.
- [ ] Each endpoint rejects an unauthenticated call, confirmed by a test.
- [ ] A request carrying an unknown or malformed field is refused rather than accepted and ignored.
- [ ] Each deprecated version answers with a removal date in the response headers.

## Rationale and provenance

A consumer plans against what we published. We cannot ask every consumer to re-read the implementation each morning, so
the contract carries the promise and the version carries the change.

- [pol-INTC] commits us to defining, versioning and verifying the interfaces we publish.

## Sources and further reading

- **Normative.** [OpenAPI 3.1] sets the form a contract in this estate takes. This standard says what the document
  covers and changes nothing about its grammar.
- **Normative.** [RFC 9745] defines the `Deprecation` header field, and [RFC 8594] defines the `Sunset` header field
  carrying the removal date.

[OpenAPI 3.1]: https://spec.openapis.org/oas/v3.1.1.html
[RFC 8594]: https://www.rfc-editor.org/rfc/rfc8594.html
[RFC 9745]: https://www.rfc-editor.org/rfc/rfc9745.html
[pol-INTC]: ../../policies/delivery/intc-interface-contracts.md#clauses
