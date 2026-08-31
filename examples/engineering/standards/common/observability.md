---
id: std-OBS
tier: normative
status: draft
implements:
  - pol-OBSV.ALERTS
  - pol-OBSV.BLIND
  - pol-OBSV.CENTRAL
  - pol-OBSV.CLOCKS
  - pol-OBSV.CORREL
  - pol-OBSV.HEALTH
  - pol-OBSV.RETAIN
  - pol-OBSV.SECRETS
  - pol-OBSV.SLO
applies-to:
  - all
review-by: "2027-08-31"
owner: paul.law
tags: [ alerting, logging, tracing ]
---

# A service ships its telemetry to one place, keyed by one id

`Standard: std-OBS` `DRAFT`

## Summary

Every service emits structured logs, traces and metrics to the central platform, stamped with a UTC timestamp and a
trace id that follows the request across services. An alert names an owner and asks for an action.

## Rules

### Everything lands in the central store

- A service **MUST** emit its logs, traces and metrics to the central platform ([pol-OBSV].CENTRAL).
- A service **MUST NOT** hold the only copy of a log, so that losing the host loses the evidence ([pol-OBSV].CENTRAL).
- A service **MUST NOT** reach production without health monitoring and at least one alert on it
  ([pol-OBSV].BLIND).

### One request reads as one timeline

- A service **MUST** stamp every record with a UTC timestamp taken from a synchronised clock ([pol-OBSV].CLOCKS).
- A service **MUST** accept an inbound `traceparent`, and pass it to everything it calls ([pol-OBSV].CORREL).
- A log line **MUST** carry the trace id, so a search on one request returns every service that touched it
  ([pol-OBSV].CORREL).
- A service **MUST** emit structured fields rather than a formatted sentence, so a search can filter on a value
  ([pol-OBSV].CORREL).

### Telemetry carries no secret

- A service **MUST** redact credentials, tokens and unmasked personal data before the record is written
  ([pol-OBSV].SECRETS).

### Somebody acts on every alert

- A service **MUST** publish availability and latency objectives, and alert when it is on course to miss one
  ([pol-OBSV].SLO).
- An alert **MUST** name the accountable owner and the first action to take ([pol-OBSV].HEALTH).
- A team **MUST** delete or fix an alert nobody acts on, rather than leaving it to be filtered ([pol-OBSV].ALERTS).
- Telemetry **MUST** be retained for the period the repository records against the service ([pol-OBSV].RETAIN).

## Examples

```
Good
  {"ts":"2026-08-31T09:14:02.118Z","level":"warn","trace":"4bf92f3577b34da6",
   "svc":"covers-api","event":"upstream_slow","upstream":"psp","ms":1840}

Avoid
  2026-08-31 09:14:02 WARN psp took 1840ms
```

The avoided line has a local timestamp, no trace id and one string to search, so correlating it with the request that
caused it means reading by eye.

## Conformance checklist

- [ ] A search for one trace id returns records from every service the request passed through.
- [ ] Every timestamp in the central store is UTC, and the hosts agree to within a second.
- [ ] The service has a health check, a dashboard and at least one alert.
- [ ] Each alert names an owner and a first action.
- [ ] Every alert raised in the last 30 days was acted on or removed.
- [ ] The retention period for this service is recorded, and the platform is set to it.

## Rationale and provenance

An incident is answered from what was already being recorded. Adding a log line during the incident tells you about the
next one.

- [pol-OBSV] commits us to central, correlated telemetry and to alerts somebody acts on.

## Sources and further reading

- **Normative.** [W3C Trace Context] defines the `traceparent` header this standard passes between services.
- **Informative.** [Google SRE, Service Level Objectives] covers how an objective is chosen, which this standard
  requires and does not teach.

[Google SRE, Service Level Objectives]: https://sre.google/sre-book/service-level-objectives/
[W3C Trace Context]: https://www.w3.org/TR/trace-context/
[pol-OBSV]: ../../policies/operations/obsv-observability.md#clauses
