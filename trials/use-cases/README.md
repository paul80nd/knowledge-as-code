# Use cases

This folder contains the use cases for [epic #940](https://github.com/paul80nd/knowledge-as-code/issues/940): what
each reader needs from a corpus (one repository of knowledge records) at each moment of the work. There is one
Gherkin `.feature` file per moment, and every scenario names the trial case that measures it.

Whoever decides what #940 builds next reads them, and so does whoever writes a trial case for one.
[Design intent](https://paul80nd.github.io/knowledge-as-code/framework/intent/) states the moments, and what an agent
does with each type. [The landscape review](landscape-review.md) says which of these use cases the trial should test,
and which common problem none of them covers.

## What a scenario states

Each scenario is a hypothesis. Nothing runs these files, and no step definition binds them. A scenario holds where its
trial case passes at the threshold its `Then` states.

* **The reader and the need** open each file, in the feature's description, because every scenario in it shares them.
* **The baseline** is the first line of a scenario's description, cited to a run file where one exists. A proposed
  case has none.
* **The pass threshold** is in the `Then`, such as "in at least 2 of 3 runs".

Every scenario has four tags, the first through the feature it sits in. A scenario that tests the tool takes `@check`
in place of `@intent`, because no agent uses a type in it:

| Tag                     | Value                                                                                 |
|-------------------------|---------------------------------------------------------------------------------------|
| `@moment:<moment>`      | the moment: `design`, `build`, `review`, `operate`, `contribute`, `assure` or `learn` |
| `@intent:<intent>`      | what an agent does with the type: `follow`, `flow`, `nudge`, `respect` or `inform`    |
| `@check:<command>`      | the `kac` command whose check the scenario tests, such as `validate` or `restore`     |
| `@type:<type>`          | the type whose records the scenario reads, as `.schema/` names it                     |
| `@trial:<trial>/<case>` | the trial and the case that measures it. A plugin trial case also names its corpus.   |

A scenario whose case does not exist yet also has `@proposed`. Its `@trial` tag is the path the case would take.

## The moments

| File                                       | Reader                                       | Scenarios | On a case that exists |
|--------------------------------------------|----------------------------------------------|----------:|----------------------:|
| [`design.feature`](design.feature)         | an agent or an engineer choosing an approach |         5 |                     5 |
| [`build.feature`](build.feature)           | an agent writing code                        |         7 |                     7 |
| [`review.feature`](review.feature)         | a reviewer, human or AI                      |         4 |                     4 |
| [`operate.feature`](operate.feature)       | whoever is on call                           |         5 |                     5 |
| [`contribute.feature`](contribute.feature) | an agent or an engineer writing back         |         4 |                     3 |
| [`assure.feature`](assure.feature)         | the corpus owner, or an auditor              |         4 |                     3 |
| [`learn.feature`](learn.feature)           | a person new to the team                     |         3 |                     3 |

`grep -c '^  Scenario:' trials/use-cases/*.feature` reports the scenarios in each file.

## Scenarios on a case that exists

These map to a case a trial has already run, and each gives its baseline from that run:

| Scenario                                                                                  | Case                                                    | Run                                                           |
|-------------------------------------------------------------------------------------------|---------------------------------------------------------|---------------------------------------------------------------|
| A rule filed under another topic reaches the session                                      | `plugin/engineering/house/health-endpoint`              | [full pass](../plugin/runs/full-pass.md)                      |
| A tracing rule reaches a session writing an outbound call                                 | `plugin/engineering/house/pricing-call`                 | [full pass](../plugin/runs/full-pass.md)                      |
| A policy clause reaches a session writing infrastructure                                  | `plugin/engineering/house/audit-log-retention`          | [full pass](../plugin/runs/full-pass.md)                      |
| A departure from a MUST goes to a deviation                                               | `plugin/engineering/house/flaky-test-skip`              | [full pass](../plugin/runs/full-pass.md)                      |
| A deviation licenses nothing outside its scope                                            | `plugin/dog-fooding/house/second-package-unscanned`     | [987 build](../plugin/runs/2026-10-21-987-build.md)           |
| Code storing personal data keeps the retention its data record states                     | `plugin/library/house/loan-history`                     | [987 build](../plugin/runs/2026-10-21-987-build.md)           |
| New code takes the estate's word for a concept                                            | `plugin/library/house/patron-reminders`                 | [987 build](../plugin/runs/2026-10-21-987-build.md)           |
| A process written as outcomes draws no finding asking for a step                          | `process-level/prc-change-the-schema`                   | [baseline](../process-level/runs/2026-10-18/baseline.md)      |
| A review of a change that broke a MUST cites the clause it broke                          | `review/engineering/health-endpoint`                    | [2026-10-19-2](../review/runs/2026-10-19-2.md)                |
| A review of a MUST broken in the change's own lines cites it, and asks for it to be fixed | `review/engineering/must-in-added-lines`                | [2026-10-19-2](../review/runs/2026-10-19-2.md)                |
| A review notes a departure from a SHOULD that gives its reason, and does not fail it      | `review/engineering/should-departure`                   | [2026-10-19-2](../review/runs/2026-10-19-2.md)                |
| A review says which rule no control checks                                                | `review/engineering/health-endpoint`                    | [2026-10-19-2](../review/runs/2026-10-19-2.md)                |
| A question about coverage gets the published report, with its date and its limits         | `plugin/dog-fooding/house/uncovered-clauses`            | [990 assure](../plugin/runs/2026-10-20-990-assure.md)         |
| A question about enforcement gets the control, and its mechanism                          | `plugin/engineering/house/is-deploy-enforced`           | [990 assure](../plugin/runs/2026-10-20-990-assure.md)         |
| A question about open departures gets each one with its review date                       | `plugin/dog-fooding/house/deviations-due`               | [990 assure](../plugin/runs/2026-10-20-990-assure.md)         |
| A design the ADR log ruled out follows the ADR or raises it                               | `plugin/library/house/adr-ruled-out`                    | [986 design](../plugin/runs/2026-10-20-986-design.md)         |
| A design departing from a policy SHOULD says why, and requests no deviation               | `plugin/engineering/house/public-path-between-services` | [986 design](../plugin/runs/2026-10-20-986-design.md)         |
| A design touching a path an agreed target binds says how it meets the target              | `plugin/payments/house/authorisation-extra-call`        | [986 design](../plugin/runs/2026-10-20-986-design.md)         |
| A design that needs a library chooses an approved tool                                    | `plugin/dog-fooding/house/new-command-line-verb`        | [986 design](../plugin/runs/2026-10-20-986-design.md)         |
| A design that would use a retired integration learns its successor                        | `plugin/library/house/text-the-borrower`                | [986 design](../plugin/runs/2026-10-20-986-design.md)         |
| A symptom a runbook opens on gets the runbook's steps, in its order                       | `plugin/dog-fooding/house/golden-red-in-ci`             | [988 operate](../plugin/runs/2026-10-21-988-operate.md)       |
| A symptom that matches a verified fix gets the fix's resolution                           | `plugin/payments/house/two-pending-amounts`             | [988 operate](../plugin/runs/2026-10-21-988-operate.md)       |
| A draft fix is a lead, not a resolution                                                   | `plugin/payments/house/unverified-fix`                  | [988 operate](../plugin/runs/2026-10-21-988-operate.md)       |
| A question about a past failure gets the incident's account                               | `plugin/payments/house/has-this-happened-before`        | [988 operate](../plugin/runs/2026-10-21-988-operate.md)       |
| A question about whom to page learns that no record names an on-call contact              | `plugin/library/house/notices-down`                     | [988 operate](../plugin/runs/2026-10-21-988-operate.md)       |
| A session that must depart from a MUST requests a deviation, and accepts nothing itself   | `plugin/engineering/house/request-a-deviation`          | [989 contribute](../plugin/runs/2026-10-21-989-contribute.md) |
| A term the glossary lacks is offered as a finding, and its meaning marked unconfirmed     | `plugin/library/house/undefined-term`                   | [989 contribute](../plugin/runs/2026-10-21-989-contribute.md) |
| A new reader tells two of the estate's words apart                                        | `comprehension/library`                                 | [library](../comprehension/runs/2026-10-21-library.md)        |
| A new reader learns how a part of the estate works from its explanation                   | `comprehension/library`                                 | [library](../comprehension/runs/2026-10-21-library.md)        |
| A new reader learns what the estate offers, and what is not live yet                      | `comprehension/library`                                 | [library](../comprehension/runs/2026-10-21-library.md)        |

Two scenarios are `@proposed`, and both test the tool: **a change that breaks the corpus fails before it merges**, and
**a new version of a consumed corpus says what it broke**. [The landscape review](landscape-review.md) says why.

## The types

Every type in `.schema/` has at least one scenario stating its intent. Where the intent holds only under a condition,
the condition is in the Intent column.

| Type           | Intent                                              | Moment     | Scenario                                                                                  |
|----------------|-----------------------------------------------------|------------|-------------------------------------------------------------------------------------------|
| `runbooks`     | follow                                              | operate    | A symptom a runbook opens on gets the runbook's steps, in its order                       |
| `fixes`        | follow, once the symptom matches and it is verified | operate    | A symptom that matches a verified fix gets the fix's resolution                           |
| `fixes`        | inform, while it is a draft                         | operate    | A draft fix is a lead, not a resolution                                                   |
| `processes`    | flow                                                | contribute | A process written as outcomes draws no finding asking for a step                          |
| `standards`    | nudge                                               | build      | A rule filed under another topic reaches the session                                      |
| `standards`    | nudge                                               | build      | A tracing rule reaches a session writing an outbound call                                 |
| `standards`    | nudge                                               | build      | A departure from a MUST goes to a deviation                                               |
| `standards`    | nudge                                               | review     | A review of a change that broke a MUST cites the clause it broke                          |
| `standards`    | nudge                                               | review     | A review of a MUST broken in the change's own lines cites it, and asks for it to be fixed |
| `standards`    | nudge                                               | review     | A review notes a departure from a SHOULD that gives its reason, and does not fail it      |
| `policies`     | nudge                                               | design     | A design departing from a policy SHOULD says why, and requests no deviation               |
| `policies`     | nudge                                               | build      | A policy clause reaches a session writing infrastructure                                  |
| `adrs`         | respect                                             | design     | A design the ADR log ruled out follows the ADR or raises it                               |
| `nfrs`         | respect, where `agreed`                             | design     | A design touching a path an agreed target binds says how it meets the target              |
| `tools`        | respect: `approved` for new work                    | design     | A design that needs a library chooses an approved tool                                    |
| `deviations`   | respect, inside its scope and period                | build      | A deviation licenses nothing outside its scope                                            |
| `deviations`   | respect, inside its scope and period                | contribute | A session that must depart from a MUST requests a deviation, and accepts nothing itself   |
| `deviations`   | respect, inside its scope and period                | assure     | A question about open departures gets each one with its review date                       |
| `glossary`     | inform                                              | build      | New code takes the estate's word for a concept                                            |
| `glossary`     | inform                                              | contribute | A term the glossary lacks is offered as a finding, and its meaning marked unconfirmed     |
| `glossary`     | inform                                              | learn      | A new reader tells two of the estate's words apart                                        |
| `services`     | inform                                              | operate    | A question about whom to page learns that no record names an on-call contact              |
| `data`         | inform                                              | build      | Code storing personal data keeps the retention its data record states                     |
| `integrations` | inform                                              | design     | A design that would use a retired integration learns its successor                        |
| `offerings`    | inform                                              | learn      | A new reader learns what the estate offers, and what is not live yet                      |
| `explanations` | inform                                              | learn      | A new reader learns how a part of the estate works from its explanation                   |
| `postmortems`  | inform                                              | operate    | A question about a past failure gets the incident's account                               |
| `controls`     | inform                                              | review     | A review says which rule no control checks                                                |
| `controls`     | inform                                              | assure     | A question about enforcement gets the control, and its mechanism                          |
| `reports`      | inform                                              | assure     | A question about coverage gets the published report, with its date and its limits         |

## Adding a use case

1. Add the scenario to the file for its moment, with all four tags.
2. State the baseline from a run file, or say there is no run yet and tag it `@proposed`.
3. Put the pass threshold in the `Then`. Grade what the reader produced: a file, a command, a drafted request, or the
   answer it was asked for.
4. Add a row for it to the types table above, unless it takes `@check`. Add it to the table of existing cases when its
   case has run.
