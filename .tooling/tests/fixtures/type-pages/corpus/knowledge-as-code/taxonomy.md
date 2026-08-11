# Taxonomy

A document every corpus running the framework holds a byte-identical copy of, so it may name a type and may not link
to one. This one does both: [ADRs](/adrs) links to a type page, and
[adr-0001](/adrs/0001-knowledge-as-code.md) links to one corpus's own record.

These documents get the ordinary link pass as well, which they had never had: [nowhere](/nowhere.md) resolves to no
file, and [the wrong anchor](/adrs.md#no-such-heading) resolves to a page that has no such heading.

Two links must stay silent. [The ADR page](/adrs.md) — no, that one is a type as well, so it is reported like the
first. The genuinely silent pair is the link inside the block below, which is written from the types this corpus stood
up rather than from the framework's full range, and naming a type in prose: an ADR, a glossary, a runbook.

<!-- BEGIN GENERATED: types-placement -->

| You have… | It goes in    |
|-----------|---------------|
| A choice  | [ADRs](/adrs) |

<!-- END GENERATED: types-placement -->
