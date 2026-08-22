# Taxonomy

## Where does this go?

<!-- BEGIN GENERATED: types-placement -->

| You have…                                                     | It goes in         |
|---------------------------------------------------------------|--------------------|
| A decision that affects more than one repo, and its reasoning | [ADRs](../adrs.md) |

<!-- END GENERATED: types-placement -->

## The types

<!-- BEGIN GENERATED: types-detail -->

### Decided: immutable once accepted

Superseded rather than rewritten, so what was thought at the time survives being wrong.

**[ADRs](../adrs.md).** An architecturally significant decision affecting more than one repository, and the reasoning
behind it. The context, the choice, the alternatives weighed, the consequences. Immutable once accepted and superseded
by a new ADR rather than rewritten. A decision local to a single repository belongs in the repo that holds it, not here.

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
