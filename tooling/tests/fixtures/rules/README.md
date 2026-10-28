One record per rule a type's own schema declares, each tripping that rule and, as far as it can, no other.

This is where the coverage gate finds most of the type-specific rules. A rule declared in a `.schema/<type>.yaml`,
whether by an `expr:` or by a C# class, gets its fixture here. The filename says which rule a record trips, so a new
rule is a new record in its type's folder. Most types here have a `review-overdue.md`, for that type's `*-in-date` rule.

Some records need another record to trip their rule, and those are the findings to read with care.

* `dependency-cycle` takes two services listing each other. It is reported once, against the lower id on the loop.
* `clause-quoted-faithfully` takes a control quoting one clause twice, once in the clause's words and once in words it
  does not use.
* `alert-after-needs-an-alert` takes an NFR stating a delay against a service that emits nothing.
* `alignment-unlinked` takes a policy citing a framework whose label nothing defines. It is reported once for both
  clauses citing it.
* `redefinition-unlinked` takes a glossary narrowing another and redefining two of its terms, linking one and not the
  other.
* `decision-cites-descriptive` takes an ADR linking a service from `Context`, which passes, and from `Decision`, which
  does not.

A finding beside the one a record is for is there because the rule cannot be reached without it. Each runbook states
`never` or a walk far enough past to trip `staleness-loud`, and the pair on `walk-method-stated` pins both sides of it.
`ctl-0004` names a process as `by` and states a `method`, so it trips `verified-by-a-person-or-agent` and
`check-states-no-method` together. An NFR writing `target` as an em dash reports `bare-key` and `required-field`
together.

`reports/own-corpus-first.md` trips nothing. Its `sources` lists `fixture-corpus` ahead of an import that sorts
before it, as `kac report` writes them, and pins that `list-order` passes over a field declaring `order: as-written`.

`.corpus.yaml` states a name and a content version for `report-stale` to compare against, a `repos-base` for
`mirrors-repo-links` to resolve against, and the `platform` and `component-type` ranges the services use. A rule
measured against today reads a date `FixtureDateTests` holds well clear of it.
