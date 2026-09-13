---
id: prc-sweep-prose
type: process
tier: procedural
status: active
last-rehearsed: "never"
owner: human:paul.law
tags: [ agents, prose ]
---

# Sweep the writing rules across a folder

`Process: prc-sweep-prose` `ACTIVE`

Apply the writing rules to every file in a folder, one agent per folder.

## When to use this

A folder's prose has to follow the current rules, and nobody has read it against them. Testing a rule is a different
job.

## Prerequisites

* The main checkout, so an agent can read what it needs. One agent per folder.
* `technical-writing` and the voice skill for the surface, loaded with the Read tool.
* `kac` run from inside a corpus, and nobody else running it at the same time.

## Steps

1. List every file in the folder first. A folder nobody listed is a folder nobody swept. List a type template and the
   glossary as files, not as folders.
2. Put a version check at the top of the prompt. Quote three or four lines only the current skills contain, and tell
   the agent to stop where any is missing. Tell it to load the skills with the Read tool, because the Skill tool has
   served a stale render.
3. State the trap for that batch. A trap named in advance catches nobody out.
   * A schema sweep meets plain YAML scalars, where a colon is a parse error.
   * A type page meets generated regions.
   * A policy meets the clause override.
4. Say which files belong to somebody else, including a human reading a folder right now.
5. Forbid `kac` and `dotnet` while agents run in parallel. They build the same project and contend over its output.
6. Verify from the files, not from the report. Count the words in each file, then diff headings, frontmatter and
   generated regions against `HEAD`.
7. Read each whole file, not the diff. A sweep leaving a document in two voices has failed, even where every rule was
   obeyed.
8. Copy the overlay files across. A root page and a `_template.md` are `seed`, so nothing catches drift and you copy
   them by hand.
9. Ask the agents where a rule failed them. A rule two readers understand differently is a defect, however good either
   result looks.
10. Run [prc-pull-request].

## Verification

`kac validate` and `kac generate --check` both report clean in every corpus you touched.
`kac update --check --from ../../` reports every overlay copy equal to its template.

Close by stating each file's word count before and after. Name what you deliberately left, and the rule exempting it.
Name every place a rule did not decide the answer.

## Related

* [std-PROSE] states the rules a sweep applies.
* [std-CONFIG.a-value-living-in-more-than-one-tree-is-copied-and-proved] states what step 8 requires of you.
* [prc-pull-request] merges the sweep.

[prc-pull-request]: pull-request.md
[std-CONFIG.a-value-living-in-more-than-one-tree-is-copied-and-proved]: ../standards/configuration.md#a-value-living-in-more-than-one-tree-is-copied-and-proved
[std-PROSE]: ../standards/prose.md
