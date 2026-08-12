---
id: gls-knowledge-as-code
tier: descriptive
status: draft
owner: paul.law
review-by: "2027-08-12"
tags: [ framework ]
---

# Knowledge as code

`Glossary: gls-knowledge-as-code` `DRAFT`

The words the framework uses about itself, and which a corpus running it inherits unchanged.

## Scope

The mechanism: corpora, types, records, the layers a file travels in, and what the tool does to them. A word about the
estate a corpus describes belongs in that corpus's own glossary, however often it appears here.

Every corpus shares this file, so an entry names a type rather than linking to one and cites no record. A corpus that
adopted three types reads the same page as one that adopted them all.

## Terms

### Check

A validation the tool runs, reported under its own id and listed in the generated table on each type's page.

### Consumer

A corpus that takes the framework from somewhere else, holding the tool without the tests that prove it.

**Not:** a lesser corpus. Any corpus may author a change, and real content is what reveals a schema is wrong.

### Corpus

One repository of knowledge records, with the schema and tooling it runs and the descriptor at its root that says which
part it plays.

**Not:** the wiki, which is how a corpus is published and read.

### Drift

A local edit to a file the framework owns, which the mechanism check reports as a defect.

**Not:** a change to a forked file. A fork is a corpus's to change, and nothing compares it against upstream.

### Framework

The shared mechanism a corpus runs: the schema, the tooling and the documentation that travel between corpora.

**Not:** an external framework such as ISO 27001, which is what the frameworks register means by the word. Both senses
are live in every corpus, and the register is the one that carries a compliance posture.

### Record

A knowledge document filed under a type, carrying frontmatter, an id and an identity line.

**Not:** every file the tool reads. A template is checked and is not a record, and a type's root page describes records
rather than being one.

### Rule

A behaviour a type declares in its schema, which either dispatches to a check or stands as a declared intention.

**Not:** a check. A check runs; a rule may be a statement of intent, and the type's page says which under *Declared,
not yet enforced*.

### Source

A corpus that answers for the framework, carrying the tests and fixtures that prove the tool.

**Not:** upstream. The role says what part a corpus plays rather than which way content travels, and a source may itself
sync from a source further upstream.
