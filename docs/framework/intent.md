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

The use cases for each moment, and for each type, are written as features under
[`trials/use-cases/`](https://github.com/paul80nd/knowledge-as-code/tree/main/trials/use-cases). Each one names the
trial case that measures it. Tracked in [epic #940][e940].

## What an agent does with each type

Each type has an intent: what an agent should do with a record of it. The intent sets the tone of the skill that
delivers the type, and of anything generated from it.

| Intent      | What the agent does                                                                           | Types                                                                                                            |
|-------------|-----------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------|
| **Follow**  | follows it closely, as a person would                                                         | runbooks, and fixes once the symptom matches and somebody has verified the fix                                   |
| **Flow**    | follows the shape of the work and judges each step                                            | processes                                                                                                        |
| **Nudge**   | treats each MUST as the baseline, and each SHOULD as a default it may depart from, saying why | standards, policies                                                                                              |
| **Respect** | does not go against it silently: honours it, or raises it                                     | ADRs, NFRs once `agreed`, tools (only an `approved` tool for new work), deviations inside their scope and period |
| **Inform**  | uses it as context where it is relevant                                                       | glossary, services, data, integrations, offerings, explanations, postmortems, controls, reports, draft fixes     |

A Nudge type grades each clause by its keyword, so one record states obligations and latitude side by side. A Respect
type is one obligation as a whole: a decision, a target, a choice of tool, or an accepted departure.

Each row is a hypothesis until a trial confirms it. The
[per-type index](https://github.com/paul80nd/knowledge-as-code/tree/main/trials/use-cases#the-types) lists the use cases
that test each row.

## Cadence and delivery

A type whose records change once a year can be built into what ships. A type whose records change with every incident
is better read live. [Story #946][s946] decides how the fast types reach an agent, using the cadence each type page
states. Tracked in [epic #940][e940].

## The example corpora as exemplars

The corpora under [`examples/`](https://github.com/paul80nd/knowledge-as-code/tree/main/examples) show what a good
record of each type looks like, and adopters model their own records on them. Each type's worked examples are compared
with well-regarded public ones. Where a record differs from good public practice, the corpus says why. Almost every rule
in the example standards is still a MUST, and they are rewritten first, in the shape [Principles](principles.md)
describes. Tracked in [epic #930][e930].

## Delivery at the moment of use

The plugin gives an agent the smallest set of relevant knowledge at the moment the agent is likely to use it. Text
loaded early and left unused draws on the agent's attention for the rest of the session. So the plugin pushes a short
pointer at the moment of use, and the agent reads the full text the pointer names. This follows Anthropic's guidance on
[context engineering][ctx] and on [progressive disclosure in skills][skills].

| Moment                                  | When it arrives                        | What reaches the agent                             | Where the full text is              |
|-----------------------------------------|----------------------------------------|----------------------------------------------------|-------------------------------------|
| **Build**                               | the session's first tool call          | an index of every standard rule, by title          | the standard, read through a lookup |
| **Review**                              | somebody asks for a review             | the review view, in full, in a reviewer of its own | the review view                     |
| **Design, operate, contribute, assure** | the agent decides to look something up | a lookup skill's answer                            | the record                          |
| **Learn**                               | a person opens the corpus              | the published corpus, read as a wiki               | the record                          |

A moment whose lookups keep missing in a trial moves to a pointer the plugin pushes. Tracked in [epic #940][e940].

## Views generated for each moment

`kac` generates a **view** (an export shaped for one use) when it bundles the plugin. The corpus declares each view in
its plugin manifest. Each entry in a view states its record's status, so a draft rule reads as a draft.

* **The build index** lists every standard rule by title, under its standard's id. A hook the plugin ships pushes it
  once a session.
* **The review view** lists each rule's obligations: each MUST to check, and each SHOULD to note a departure from. It
  lists the controls that check each standard, with each control's mechanism and status. A control can name a whole
  standard, so the reviewer reads the control's record to learn which obligations it checks.

The per-type export stays the format another corpus imports. Tracked in [epic #940][e940].

## Formats other agents read

Claude Code comes first, and nothing generated is exclusive to it. Skills use the open `SKILL.md` format, and each view
is Markdown.

The plugin ships its own reviewer, modelled on the Standards review in [`code-review`][cr]. A repository can also copy
the review view to where its own review tools read standards, such as `CODING_STANDARDS.md`.

No record states which files a rule applies to, so nothing loads by path. [Issue #993][s993] keeps that option open
until the build trial shows whether the index is enough. Tracked in [epic #940][e940].

## Where to go next

[Principles](principles.md) says what the framework does today, and why.

[cr]: https://github.com/mattpocock/skills/tree/main/skills/engineering/code-review
[ctx]: https://www.anthropic.com/engineering/effective-context-engineering-for-ai-agents
[e930]: https://github.com/paul80nd/knowledge-as-code/issues/930
[e940]: https://github.com/paul80nd/knowledge-as-code/issues/940
[s946]: https://github.com/paul80nd/knowledge-as-code/issues/946
[s993]: https://github.com/paul80nd/knowledge-as-code/issues/993
[skills]: https://www.anthropic.com/engineering/equipping-agents-for-the-real-world-with-agent-skills
