# Contributing

How knowledge is added, reviewed and merged. The rules are the same whether a person or an AI session is holding the
keyboard, which is the point: a corpus grows at the rate work happens only if both can write to it, and it stays worth
reading only if both answer to the same bar.

This page is the model. The rules for the words themselves are skills, which an agent loads beside the work at the
moment of writing and which cost nothing on a session that writes no prose. A person contributing by hand reads them
as the full rule list.

| The skill           | Carries                                                                     |
|---------------------|-----------------------------------------------------------------------------|
| `technical-writing` | the floor: how to build a sentence, and how to write a commit message       |
| `writing-a-record`  | what a corpus adds to the floor, and what each tier asks on top of that     |
| `writing-the-docs`  | the public face of the framework, which is this site                        |

## What outranks what

Four sources of rules, in this order.

1. **The schema and the validator.** `.schema/*.yaml`, and what `kac validate` and `kac checks` report. These are
   executable, so they are the authority on anything mechanical: required sections, clause modals, id and filename
   formats, link forms, and which text rules a type actually declares.
2. **The type's own pages.** `<type>.md` for what the type holds, and `<type>/_template.md` for the sections a record
   of it must have.
3. **The corpus's own contributing page**, for the conventions it sets locally.
4. **The skills**, for the prose.

**Report a contradiction, do not act on it.** Where a prose rule contradicts the schema, say which of the two is wrong
and leave both alone. Read literally, one wrong bullet in a rulebook can strip a keyword from every normative record in
a corpus while claiming the rulebook's authority.

## The shape of a contribution

1. Work out where it goes. The corpus's own taxonomy page has a decision table.
2. Copy the type's `_template.md`. It marks the parts you supply as `{{placeholder}}` and fences its own guidance
   between `DELETE FROM HERE` and `DELETE TO HERE` comments. A finished record has neither left in it.
3. Allocate an id in the style that type uses: the next unused number, a four-character mnemonic for the concept, or a
   slug. The folder's index says what is already taken.
4. Fill in the frontmatter.
5. Write the content. Follow the template's section structure, which exists so records of a type are comparable. The
   tier rules are why a runbook step and an ADR paragraph are held to different constraints.
6. Open a pull request. What review it needs follows the tier.

Nobody edits generated content by hand. Where an index looks wrong, the frontmatter it was built from is wrong.

## Review follows the tier

The review bar follows what a record *is*, and never who wrote it.

| Tier            | Review required                    | Merge criteria                                                                   |
|-----------------|------------------------------------|----------------------------------------------------------------------------------|
| **Decided**     | Two reviewers                      | Alternatives genuinely weighed. Consequences stated including the unwelcome ones |
| **Normative**   | The record's owner                 | Rules are testable. RFC 2119 keywords used correctly. Changelog updated          |
| **Descriptive** | One reviewer                       | Cross-references resolve. Content matches the estate as it actually is           |
| **Procedural**  | One reviewer who has done the task | Steps are followable by someone who hasn't. Rollback stated                      |
| **Observed**    | None                               | Merges on CI passing. Authority comes at promotion, not capture                  |

**Observed content is unreviewed by design.** A discovery is cheap because nobody gates it, and a capture step with a
review attached is a capture step that does not happen. The tier's low authority is what makes the low bar safe.

**Decided content is immutable after merge.** Corrections are limited to typos and status transitions. To change a
decision, write a new one that supersedes it.

**How much rigour each type needs is the corpus's to decide.** The table above is what the tiers ask; a branch policy
is how a corpus enforces it, and the owners set that. Every corpus starts from the suggestion its own contributing page
carries.

## An agent contributes, and a human accepts

An agent proposing knowledge has an identity of its own: a service account that can open pull requests and cannot merge
them. A human accepts what it proposes, and a branch policy enforces that so nobody has to remember.

What an agent may write follows from that. It captures a discovery rather than an FAQ, because it cannot confirm its
own observations. It proposes a superseding record rather than editing a Decided one. It asks where a record goes
rather than guessing, because a well-written record in the wrong folder is a cost.

## What a pipeline will not do

**CI does not commit.** Where generated content is stale the build fails and names the command to run locally. A
pipeline that pushed fixes into the branch would produce bot commits, re-trigger itself, and make "who changed this"
unanswerable. That is a bad trade for a repository whose value is a trustworthy history.

## What does not belong in a corpus

* Content duplicated from a work item. Link to it instead.
* Anything holding secrets, connection strings, tokens or customer data. A corpus is broadly readable.
* Raw session logs. Distilled discoveries only.
* Speculative documentation for work not yet started. That belongs in the backlog.
* A record that fits no type. Raise the gap, because a missing type is a taxonomy conversation and a `misc/` folder is
  a failure nobody notices until it is large.
