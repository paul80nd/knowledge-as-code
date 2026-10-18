# Design intent

This page states the direction the framework is being built towards. Nothing on it is built yet unless a section says
so. Each section links the epic that builds it. A section moves to [Principles](principles.md) in the change that makes
it true, so this page shrinks as the framework catches up with it.

## What `kac` is for

`kac` keeps an organisation's engineering knowledge as validated, identified and versioned Markdown, and generates what
each agent needs at each moment of the work.

The first half is true today. The second half is the direction. Today `kac` packages a corpus as one Claude Code plugin,
and its skills look a record up when an agent decides to. The sections below say what replaces that.

## The moments knowledge serves

Knowledge is used at distinct moments in the work, by different readers. Each moment needs a different slice of the
corpus, delivered a different way.

| Moment         | Who                                        | What they need                                                                  |
|----------------|--------------------------------------------|---------------------------------------------------------------------------------|
| **Design**     | an agent or engineer choosing an approach  | the decisions already made, the policies that bind, the targets to meet         |
| **Build**      | an agent writing code                      | every rule that applies to the code being changed, wherever the rule is filed   |
| **Review**     | a reviewer, human or AI                    | the rules to check the change against, each with an id to cite                  |
| **Operate**    | whoever is on call                         | the runbook, and whether anybody has hit this failure before                    |
| **Contribute** | an agent or engineer writing back          | the level of detail a record is written at, and the route for a finding         |
| **Assure**     | the corpus owner, or an auditor            | which policies have no standard, which rules no control proves, what is stale   |
| **Learn**      | a person new to the team                   | the domain and the house style, read as a wiki                                  |

The use cases for each moment, and for each type, are written as features and measured by a trial under
[`trials/`](https://github.com/paul80nd/knowledge-as-code/tree/main/trials). Tracked in [epic #940][e940].

## What an agent does with each type

Each type has an intent: what an agent should do with a record of it. The intent sets the tone of the skill that
delivers the type, and of anything generated from it.

| Intent      | What the agent does                                                          | Types                                                                                                |
|-------------|------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------|
| **Follow**  | follows it closely, as a person would                                        | runbooks, and fixes once the symptom matches                                                         |
| **Flow**    | follows the shape of the work and judges each step                           | processes                                                                                            |
| **Nudge**   | treats each MUST as the baseline, and each SHOULD as a default it may depart from, saying why | standards, nfrs                                                                     |
| **Respect** | does not go against it silently: honours it, or raises it                    | ADRs, policies, deviations                                                                           |
| **Inform**  | uses it as context where it is relevant                                      | glossary, services, data, integrations, tools, offerings, explanations, postmortems, controls, reports |

Each row is a hypothesis until a trial confirms it.

## MUST and SHOULD in a standard

A MUST is the baseline the corpus owner is accountable for. A review checks it, a control proves it wherever a tool can,
and departing from it takes a deviation. A SHOULD is the usual way, given with its reason. An engineer or an agent may
depart from it and say why in the change.

A standard is the house style and a checklist to review work against. It is a safety net, and it is not there to catch
anyone out. An agent gets the same latitude as an engineer: the baseline is fixed, and inside it the agent uses its
judgement.

Today the standards schema keeps a recommendation out of a standard, so every rule in the example standards is a MUST.
That changes: a standard states SHOULD and MAY, gives a short reason beside each rule, and points to the deviation
route. Tracked in [epic #930][e930].

## Level of detail and change cadence

Each type page states the level of detail a record of that type is written at. A step in a runbook is a command you can
run. A step in a process is an outcome. A rule in a standard states an outcome, with the usual mechanism as a SHOULD.

The writer, the agent and whoever triages a finding all read it. A finding refines a record at its own level, and never
adds detail below it. It is guidance for whether a record feels right, so nothing fails a record on it.

Each type page also states how often its records are likely to change. A type that changes once a year can be built
into what ships. A type that changes with every incident is better read live. Tracked in [epic #930][e930].

## The example corpora as exemplars

The corpora under [`examples/`](https://github.com/paul80nd/knowledge-as-code/tree/main/examples) show what a good
record of each type looks like, and adopters model their own records on them. Each type's worked examples are compared
with well-regarded public ones. Where a record differs from good public practice, the corpus says why. Tracked in
[epic #930][e930].

## Views generated for each moment

`kac` generates a **view** for each moment: an export shaped for one use, built once when the plugin is bundled. No
agent has to assemble it from raw records while it works.

* **The build view** writes the MUST rules for the code being changed into rules files scoped by path, which load when
  an agent touches a matching file.
* **The review view** gives a reviewer each rule to check, with its id and its reason, each SHOULD to note a departure
  from, and the controls that already check a rule.

The per-type export stays as the format another corpus imports. Tracked in [epic #940][e940].

## Formats other agents read

Claude Code comes first, and nothing generated is exclusive to it. `kac` generates formats other agents already read:
skills in the open `SKILL.md` format, Markdown in the shape of an `AGENTS.md`, and rules scoped by path.

The review view is written where review tools already look for a repository's standards. A code-review skill, or an AI
reviewer, reads it without being forked or replaced.

What loads by itself is kept short. A map of the corpus is always on. The MUST rules for the files being changed load by
path. Everything else is fetched when it is needed. Tracked in [epic #940][e940].

## Skills that state intent

A skill states what it is for and why, and leaves the steps to the model's judgement. It sets out steps only for a type
an agent follows: a runbook, a fix, or the flow of a process.

A finding that a skill or a process did not cover a case is answered with a reason the agent can apply to the next case,
or by removing a step. It is never answered by adding a step. Tracked in [epic #940][e940].

## Where to go next

[Principles](principles.md) says what the framework does today, and why.

[e930]: https://github.com/paul80nd/knowledge-as-code/issues/930
[e940]: https://github.com/paul80nd/knowledge-as-code/issues/940
