---
id: adr-0010
tier: decided
status: accepted
decided-on: "2026-06-12"
owner: alex.doe
---

# A copy of the template that was never finished

`ADR: adr-0010` `ACCEPTED`

> **In the context of** copying a template, **facing** {{the pressure to get something committed}}, **we decided** to
> leave a placeholder behind, **rather than** filling it in, **to achieve** a document that passes every other check,
> **accepting** that only `placeholder-left` can tell.

## Context

Everything here is well-formed: the id matches the filename, the identity line agrees, every required section is
present. That is what makes the fault worth a check of its own — a half-filled copy reads as complete, and nothing
else notices.

## Decision

Trigger `placeholder-left`, once, naming the first of the placeholders and counting the rest.

## Alternatives Considered

* **Fill it in** — rejected: then there would be nothing to test here.

## Consequences

The fenced block below is the reason the check reads the parsed inlines rather than the source. A document quoting a
templating language is describing one, not failing to finish one, so nothing here is reported:

```yaml
steps:
  - script: echo ${{ variables.buildId }}
```

`${{ variables.buildId }}` in a code span is skipped for the same reason.
