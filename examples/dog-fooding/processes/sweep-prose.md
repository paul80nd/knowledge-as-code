---
id: prc-sweep-prose
tier: procedural
status: active
last-rehearsed: "never"
rehearsal-frequency:
requires-access:
owner: paul.law
tags: [ agents, prose ]
---

# Sweep the writing rules across a folder

`Process: prc-sweep-prose` `ACTIVE`

Apply the writing rules to every file in a folder, one agent per folder.

## When to use this

A folder's prose has to answer to the current rules and nobody has read it against them. Testing a rule is a different
job, and it does not belong here.

## Prerequisites

* The main checkout, so an agent can read what it needs. One agent per folder.
* `technical-writing` and the voice skill for the surface, loaded with the Read tool.
* `kac` run from inside a corpus, and nobody else running it at the same time.

## Steps

1. Count first, from the files. A folder nobody listed is a folder nobody swept. Count the type templates and the
   glossary as files rather than as folders.
2. Put a version check at the top of the prompt. Name three or four lines only the current skills carry, and tell the
   agent to stop where any is missing.
3. Name the trap for that batch. A schema sweep meets plain YAML scalars where a colon is a parse error. A type page
   meets generated regions. A policy meets the clause override. A named trap has never fired.
4. Say which files belong to somebody else, including a human reading a folder right now.
5. Forbid `kac` and `dotnet` while agents run in parallel. They build the same project and contend over its output.
6. Verify from the files rather than from the report. Count the marks, then diff headings, frontmatter and generated
   regions against `HEAD`.
7. Read each whole file rather than the diff. A sweep leaving a document in two voices has failed even where every rule
   was obeyed.
8. Copy the overlay files across. A root page and a `_template.md` are `seed`, so nothing catches drift and the copy is
   yours.
9. Ask the agents where a rule failed them. A rule two readers understand differently is a defect, however good either
   result looks.
10. Run [prc-pull-request].

## Verification

`kac validate` and `kac generate --check` both report clean in every corpus you touched, and
`kac update --check --from ../../` reports the overlay files in step.

Close by naming the count before and after per file, what you deliberately left and the rule exempting it, and every
place a rule did not decide it.

## Related

* [std-PROSE] carries the rules a sweep applies.
* [std-CONFIG.a-value-living-in-more-than-one-tree-is-copied-and-proved] says what step 8 owes you.
* [prc-pull-request] is how the sweep lands.

[prc-pull-request]: pull-request.md
[std-CONFIG.a-value-living-in-more-than-one-tree-is-copied-and-proved]: ../standards/configuration.md#a-value-living-in-more-than-one-tree-is-copied-and-proved
[std-PROSE]: ../standards/prose.md
