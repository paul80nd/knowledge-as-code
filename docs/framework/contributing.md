# Contributing

A corpus, meaning one repository of knowledge records, grows at the rate work happens only if a person and an AI session
can both write to it. So the rules below are the same whichever is holding the keyboard.

The rules for the words themselves are skills, which an agent loads at the moment of writing and a person reads as the
full rule list. [Skills](../use/skills.md) lists them. Start with `technical-writing`, the floor under all the others.

## What outranks what

Four sources of rules, in this order.

1. **The schema and the validator.** `.schema/*.yaml`, and what `kac validate` and `kac checks` report. These are
   executable. So they are the authority on anything mechanical: required sections, clause modals, id and filename
   formats, link forms, and the text rules a type declares.
2. **The type's own pages.** `<type>.md` for what the type covers, and `<type>/_template.md` for the sections a record
   of it must have.
3. **The corpus's own contributing page**, for the conventions it sets locally.
4. **The skills**, for the prose.

**Report a contradiction, do not act on it.** Where a prose rule contradicts the schema, say which of the two is wrong
and leave both alone.

## The shape of a contribution

1. Work out where it goes. The corpus's own taxonomy page has a decision table.
2. Copy the type's `_template.md`. It marks the parts you supply as `{{placeholder}}` and fences its own guidance
   between `DELETE FROM HERE` and `DELETE TO HERE` comments. A finished record has neither left in it.
3. Allocate an id in the style that type uses: the next unused number, a mnemonic for the concept, or a slug. The
   folder's index says what is already taken.
4. Fill in the frontmatter.
5. Write the content. Follow the template's section structure, which exists so records of a type are comparable. The
   tier rules are why a runbook step and an ADR paragraph are held to different constraints.
6. Open a pull request. What review it needs follows the tier.

Do not edit generated content by hand. Where an index looks wrong, the frontmatter it was built from is wrong.

## Review by tier

The bar below follows what a record *is*, and never who wrote it. Each corpus sets how much of it a branch policy
enforces, on its own contributing page.

| Tier            | Review required                    | Merge criteria                                                                   |
|-----------------|------------------------------------|----------------------------------------------------------------------------------|
| **Decided**     | Two reviewers                      | Alternatives genuinely weighed. Consequences stated including the unwelcome ones |
| **Normative**   | The record's owner                 | Rules are testable. BCP 14 keywords used correctly. Changelog updated            |
| **Descriptive** | One reviewer                       | Cross-references resolve. Content matches the estate as it actually is           |
| **Procedural**  | One reviewer who has done the task | Someone who has never done it can follow the steps. Rollback stated              |

**The table covers what the corpus keeps.** An observation nobody has checked goes to the tracker, as
[In through the tracker, out through the export](principles.md#in-through-the-tracker-out-through-the-export) says.

**A decision is immutable after merge.** Change it only by writing a new record that supersedes it. Edit everything
else in place: a status transition, a typo, a broken link, or a sentence that no longer matches the decision. The commit
message says what you changed.

## What an agent may write

An agent proposes and a human accepts. An agent proposing knowledge has an identity of its own: a service account that
can open pull requests and cannot merge them. A human accepts what it proposes, and a branch policy enforces that so
nobody has to remember.

What an agent may write follows from that. An observation goes to the tracker, because an agent cannot verify its own
observations. A pull request goes to the corpus, for a record a human will accept. Where a Decided record is wrong, the
agent proposes the record that supersedes it. And it asks where a record goes, because a record in the wrong folder is
never found by the search that needed it.

## What a pipeline will not do

A pipeline never commits. Where generated content is stale the build fails and names the command to run locally, as
[Read-only permission](../use/ci.md#read-only-permission) says. A pipeline that pushed fixes would re-trigger itself,
and make "who changed this" unanswerable.

## What does not belong in a corpus

* Content duplicated from a work item. Link to it instead.
* Anything with secrets, connection strings, tokens or customer data in it. A corpus is broadly readable.
* Raw session logs. Distilled, reviewed records only.
* Speculative documentation for work not yet started. That belongs in the backlog.
* An observation nobody has verified. That belongs in the tracker.
* A record that fits no type. Raise the gap: a missing type is a taxonomy conversation.

[Taxonomy](taxonomy.md#which-types-a-corpus-holds) is the page for working out where a record goes, and for the case
where nothing fits.
