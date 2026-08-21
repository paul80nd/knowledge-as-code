# Style

> How we write — every document and every commit message.

**Load the `technical-writing` skill first.** It carries the floor: how to build a sentence, what to cut, which mark
states which relation, how to keep a sentence to one reading, and how to write a commit message. This page adds only
what is true of writing *here*.

[Authoring](authoring.md) adds what a record's tier asks on top of both.

Clear writing and warm writing are the same discipline. Writing gets hard to read when we perform — reaching for an
abstraction, a flourish, a passive that hides who acts. Name who does what, in a short sentence, with an ordinary word,
and the reader follows without effort. There is no trade-off to manage: the plain choice and the precise choice are
nearly always the same sentence.

## Scope

Everything the corpus holds, and everything written about it: records, README files, commit messages, pull request
descriptions and review comments. Where a rule applies to only one of those, it says so.

The rules bind, and the judgement stays yours. A rule that makes a particular sentence worse is a rule to break, and to
say why in review.

## Keep one job per document

A document that drifts between explaining, deciding and instructing is three documents wearing one heading. What a
record is for is fixed by its tier — see [authoring](authoring.md) — so a description that starts giving orders, or a
decision that explains background instead of deciding, is not a style problem to smooth over. It is a sign the content
belongs in two documents, not one.

## Anchor the abstract in something real

A line built entirely from abstractions rarely lands. A line anchored to something the reader can picture does.

> Undocumented knowledge is a single point of failure that no amount of redundancy elsewhere compensates for.

> A backup that has never been restored is not a backup.

Both reach for something true. Only the second names something you could go and check this afternoon. The same move
works on any claim — replace a category with the things inside it:

> Work produced by an AI agent enters our estate as a proposal.

> An agent can write code, change configuration, draft documentation, or report something it noticed about a system.

We cut nothing and simplified nothing. We swapped a class name for its members.

## Use the corpus's own words

**Check the glossary before treating two words as interchangeable.** Where it distinguishes near-synonyms, the
distinction is load-bearing. Otherwise, pick one word and keep it.

These three read as precise and are not. Each has a plainer form that says more:

| Instead of              | Write                            |
|-------------------------|----------------------------------|
| blast radius            | how far a breach reaches         |
| escape hatch            | the exception a policy allows    |
| single point of failure | the only person who knows        |

## Rules you can ignore

Split an infinitive, open with *and* or *but*, end on a preposition, use the same word twice. Each is a half-remembered
school rule, and a sentence contorted to obey one is worse for it.

## Warmth, without softening the obligation

We write as a team that expects people to do the right thing and says why, not as one that assumes they will not.

* **Give the reason, not the suspicion.** Where a rule exists because something went wrong, name what went wrong.
* **Skip the escalation.** *At all times*, *under no circumstances*, *strictly prohibited* — a rule that shouts is no
  clearer than one that states.
* **State exceptions early.** An exception written up front says we thought about the hard case.
* **Assume the reader wants to get it right.**

None of this softens an obligation. A clause can be unambiguous and unhedged and still be written by someone who likes
their colleagues.

## Reviews

[Contributing](contributing.md) covers the process. Review the change, not the person.

This page's language, and the tier rules in [authoring](authoring.md), are informed by outside work on plain and
controlled language. The debt and its limits are recorded in [lineage](lineage.md#language).
