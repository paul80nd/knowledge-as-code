# Taxonomy

> The kinds of knowledge this corpus holds, what each is for, and what each is not.

The [decision table](#where-does-this-go) below is the quickest route to the right answer, and the
[disambiguations](#disambiguations) explain the calls that are genuinely close. Both cover the types this corpus adopted
and no others.

[Taxonomy][taxonomy] carries what is true of every corpus: what a tier is and what the five ask, the shape a type takes
on disk, and what changing a taxonomy costs.

## Where does this go?

The types this corpus holds, generated from the schema. The table is ordered by what you are holding, so scan the left
column for your row.

<!-- BEGIN GENERATED: types-placement -->

| You have…                                                            | It goes in                   |
|----------------------------------------------------------------------|------------------------------|
| A check that proves a rule is being followed                         | [Controls](../controls.md)   |
| A commitment about how we engineer, at principle level               | [Policies](../policies.md)   |
| A decision that affects more than one repo, and its reasoning        | [ADRs](../adrs.md)           |
| A rule people must follow when building                              | [Standards](../standards.md) |
| A term whose meaning isn't obvious, or that we use in a specific way | [Glossaries](../glossary.md) |
| A tool or package we've approved, rejected, or are trialling         | [Tools](../tools.md)         |

<!-- END GENERATED: types-placement -->

If nothing fits, raise it. A missing type is a taxonomy conversation, and the answer is sometimes a type this corpus has
not adopted.

## The types

Grouped by [tier][tiers], because tier determines how each behaves, and generated from the same schema as the table
above. The fuller account of a type, meaning what it looks like here and the records already filed under it, is on the
type's own page.

<!-- BEGIN GENERATED: types-detail -->

### Decided: immutable once accepted

Superseded rather than rewritten, so what was thought at the time survives being wrong.

**[ADRs](../adrs.md).** An architecturally significant decision affecting more than one repository, and the reasoning
behind it. The context, the choice, the alternatives weighed, the consequences. Immutable once accepted and superseded
by a new ADR rather than rewritten. A decision local to a single repository belongs in the repo that holds it, not here.

### Normative: living, owned, reviewed

**[Controls](../controls.md).** How a standard's rules are verified: the mechanism, the frequency, and the evidence it
leaves. Every control names the rules it covers. A rule no control claims is recorded as `not-enforced`, which is the
honest state and the number worth watching.

**[Policies](../policies.md).** A high-level engineering commitment: the what and the why, largely stack-agnostic and
changing rarely. Alignment to an external framework is stated clause by clause, as alignment rather than certification.

**[Standards](../standards.md).** The rulebook, imperative, RFC 2119, with concrete examples and a conformance
checklist. Imperative throughout: **MUST**, **SHOULD**, **MAY**. Composed rather than read alone: the rules for a piece
of work are the union of the layers that apply to it.

### Descriptive: living, must mirror reality

These are the types CI can check against the estate rather than merely against themselves, which matters because they
rot faster than anything else.

**[Glossaries](../glossary.md).** The ubiquitous language. Terms whose meaning is specific to us, or which are easily
confused. One glossary per bounded context, each small enough to read end to end. A term that needs explaining every
time it appears belongs in the most general glossary that admits it, and everything else links to it.

**[Tools](../tools.md).** The approved-software register. What is chosen, rejected or deprecated, and the version ranges
we stand behind. Rejections are first-class content. Knowing what was turned down, and why, saves the next person the
evaluation.

<!-- END GENERATED: types-detail -->

## How the types relate

The edges carry as much value as the nodes, and they are the part that breaks silently. Every one below is a
cross-reference field the schema declares, so CI can check that it resolves to a document that exists.

<!-- BEGIN GENERATED: types-graph -->

```mermaid
graph LR;
  t_adrs[ADR];
  t_controls[Control];
  t_glossary[Glossary];
  t_policies[Policy];
  t_standards[Standard];
  t_tools[Tool];
  t_adrs -- related --> t_adrs;
  t_adrs -- superseded-by --> t_adrs;
  t_controls -- verifies --> t_standards;
  t_glossary -- narrows --> t_glossary;
  t_standards -- derived-from --> t_adrs;
  t_standards -- implements --> t_policies;
  t_tools -- decided-in --> t_adrs;
  t_tools -- replaces --> t_tools;
```

<!-- END GENERATED: types-graph -->

The spine runs down the normative hierarchy: a standard implements a policy, a control verifies a standard, and both
land on a service. Everything else hangs off that. The same edges, field by field:

<!-- BEGIN GENERATED: types-edges -->

| From     | Field           | Points at | Answered by     |
|----------|-----------------|-----------|-----------------|
| ADR      | `related`       | ADR       |                 |
| ADR      | `superseded-by` | ADR       | `supersedes`    |
| ADR      | `supersedes`    | ADR       | `superseded-by` |
| Control  | `verifies`      | Standard  | `verified-by`   |
| Glossary | `narrows`       | Glossary  |                 |
| Standard | `derived-from`  | ADR       |                 |
| Standard | `implements`    | Policy    |                 |
| Standard | `verified-by`   | Control   | `verifies`      |
| Tool     | `decided-in`    | ADR       |                 |
| Tool     | `replaces`      | Tool      | `successor`     |
| Tool     | `successor`     | Tool      | `replaces`      |

<!-- END GENERATED: types-edges -->

Reciprocal pairs must agree in both directions: `supersedes` / `superseded-by`, `verifies` / `verified-by`,
`promoted-from` / `promoted-to`. A one-sided link fails the build. Read that off the last column above. An empty cell
means nobody answers that edge, and nobody has to keep it in step.

Not every edge is a pair. A standard's `implements` points up at a policy, and the policy never points back. Policies
are the layer a downstream corpus inherits, and standards are the layer it writes for itself, so nobody sitting at the
policy can know what implements it.

Nor does every edge leave from a whole document. A policy aligns with a framework through a single **clause** rather
than in its entirety, so the edge leaves the clause table and lands on a control: `pol-SCRT.KEYS` to Annex A A.8.24.
[Frameworks](../frameworks.md) is the far end of every one of those edges, and the only page that records our standing
against a framework. It carries no `ref:` and so appears in no row above.

## Disambiguations

The calls that are actually close. Each is written once, on the type its heading names first, and appears only where
this corpus holds both sides of it.

<!-- BEGIN GENERATED: types-versus -->

**ADR vs Standard.** The ADR is the decision and its reasoning, frozen. The standard is the rule that results, kept
current. If you are writing "we considered X and rejected it", that is an ADR. If you are writing "you **MUST** do Y",
that is a standard. Most substantial changes produce both.

**Policy vs Standard.** A policy is true regardless of stack, framework or year: "we do not store secrets in source
control". A standard is specific enough to check: "read secrets from the vault via workload identity". If it would still
be true after replacing the entire technology estate, it is a policy.

**Standard vs Control.** The standard says what to do. The control says how we know it happened. "Secrets **MUST** come
from the vault" is a standard. "CI runs secret scanning on every PR" is a control. If it can fail a build, it is a
control.

**Tool vs ADR.** Adopting a tool is often a decision worth an ADR *and* an entry in the register. The ADR carries the
reasoning. The register carries the current state and the version range. Small, uncontroversial adoptions need only the
register.

<!-- END GENERATED: types-versus -->

## Status of this taxonomy

Not all types are proven. Where that matters, this corpus's own `README.md` records which have met real content.

[taxonomy]: https://paul80nd.github.io/knowledge-as-code/framework/taxonomy/
[tiers]: https://paul80nd.github.io/knowledge-as-code/framework/taxonomy/#the-five-tiers
