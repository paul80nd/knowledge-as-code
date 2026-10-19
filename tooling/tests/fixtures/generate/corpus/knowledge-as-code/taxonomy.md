# Taxonomy

## Where does this go?

<!-- BEGIN GENERATED: types-placement -->

| You have…                                                        | It goes in         |
|------------------------------------------------------------------|--------------------|
| A decision affecting more than one repository, and its reasoning | [ADRs](../adrs.md) |

<!-- END GENERATED: types-placement -->

## The types

<!-- BEGIN GENERATED: types-detail -->

### Decided: immutable once accepted, except to correct it

A decided record is about one moment: a decision, or an incident. Once accepted, it changes only to correct what it
misstates about that moment, or to fix its prose and spelling. It never follows what changed afterwards, and a changed
decision is a new record that supersedes it. A decided record states the constraints it asserts. It cites a descriptive
record for background, and never as the authority behind a constraint.

**[ADRs](../adrs.md).** An architecturally significant decision affecting more than one repository, and the reasoning
behind it. The context, the choice, the alternatives weighed, and the consequences. An accepted ADR is immutable, so a
later ADR supersedes it. A decision that affects only one repository belongs in that repository.

<!-- END GENERATED: types-detail -->

## Disambiguations

<!-- BEGIN GENERATED: types-versus -->
<!-- END GENERATED: types-versus -->

## How the types relate

<!-- BEGIN GENERATED: types-graph -->

```mermaid
graph LR;
  t_adrs[ADR];
  t_adrs -- related --> t_adrs;
  t_adrs -- superseded-by --> t_adrs;
```

<!-- END GENERATED: types-graph -->

<!-- BEGIN GENERATED: types-edges -->

| From | Field           | Points at | Answered by     |
|------|-----------------|-----------|-----------------|
| ADR  | `related`       | ADR       |                 |
| ADR  | `superseded-by` | ADR       | `supersedes`    |
| ADR  | `supersedes`    | ADR       | `superseded-by` |

<!-- END GENERATED: types-edges -->
