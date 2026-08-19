---
id: gls-knowledge-as-code
tier: descriptive
status: draft
owner: paul.law
narrows:
review-by: "2027-08-12"
tags: [ framework ]
---

# Knowledge as code

`Glossary: gls-knowledge-as-code` `DRAFT`

The words the framework uses about itself, which every corpus running it inherits unchanged.

## Scope

The mechanism: corpora, types, records, the layers a file travels in, and what the tool does to them. A word about the
estate a corpus describes belongs in that corpus's own glossary, however often it appears here.

A type's name is not defined here. Each type says what it holds in its own schema file, and the framework renders that
onto the type's page, into the taxonomy, and into the list of collisions where the name already means something else to
a reader. A hand-written second account would be the copy that goes stale.

Every corpus shares this file, so an entry names a type rather than linking to one and cites no record. A corpus that
adopted three types reads the same page as one that adopted them all.

## Terms

### Check

A validation the tool runs, reported under its own id and listed in the generated table on each type's page.

### Clause

An addressable row in a policy or standard, carrying one obligation and an id something else can cite.

**Not:** a section. A clause sits in one, and its id lets an auditor quote the obligation on its own.

### Consumer

A corpus that takes the framework from somewhere else, holding the tool without the tests that prove it.

**Not:** a lesser corpus. Any corpus may author a change, and only real content reveals that a schema is wrong.

### Corpus

One repository of knowledge records, with the schema and tooling it runs and a descriptor saying which part it plays.

**Not:** the wiki, which is how a corpus is published and read.

### Document

The loose word for anything written down here; the tool's summary line uses it to count records.

**Not:** a synonym for record wherever the difference matters. A page and a template are written down and are neither.

### Drift

A local edit to a file the framework owns, which the mechanism check reports as a defect.

**Not:** a change to a forked file. A fork is a corpus's to change, and nothing compares it against upstream.

### Forked

A file the framework provides as a starting point and the corpus then owns, so a difference from upstream is a decision
rather than a defect.

### Framework

The shared mechanism a corpus runs: the schema, the tooling and the documentation that travel between corpora.

**Not:** an external framework such as ISO 27001, which is what the frameworks register means by the word. Both senses
are live in every corpus, and only the register carries a compliance posture.

### Identity line

The line beneath a record's H1 naming its type, id and status, written for a person where the frontmatter above it is
written for a machine.

### Layer

Which kind of file this is: synced, forked, generated, local, verification or ignored. The portability manifest declares
it once, and the mechanism check reads it to decide what a difference from upstream means.

### Lifecycle

Whether a type's records stay current or become immutable once accepted, fixed by the type.

**Not:** status, which is where one record has got to in its own life.

### Mechanism

The framework's files as they sit in one corpus: the half that travels.

**Not:** the knowledge. `kac mechanism` compares that half against an upstream corpus and reads no record.

### Page

A type's root page — what the type holds, what it is not, and how to add one — carrying no frontmatter of its own.

**Not:** a record. It describes the records rather than being one, so nothing gives it an id or an identity line.

### Record

A knowledge document filed under a type, carrying frontmatter, an id and an identity line.

**Not:** every file in a type's folder. The generated index and the template sit there too.

### Rule

A behaviour a type declares in its schema, which either dispatches to a check or stands as a declared intention.

**Not:** a check. A check runs; a rule may be a statement of intent, and the type's page says which under *Declared, not
yet enforced*.

### Source

A corpus that answers for the framework, carrying the tests and fixtures that prove the tool.

**Not:** upstream. The role says what part a corpus plays rather than which way content travels. A source may itself
sync from a source further upstream.

### Synced

A file identical in every corpus running the framework. A local edit to one is drift: change it upstream and sync it
down.

### Template

The file a contributor copies to start a record, held to the fields its type declares.

**Not:** a record. It carries no id and appears in no index. The tool checks it anyway, so that every copy starts sound.

### Tier

What a record's type says about how far it may be trusted and how it must be written — decided, normative, descriptive,
procedural or observed.

**Not:** type. Several types share a tier, and the writing rules and the review bar follow the tier.

### Type

The kind of knowledge a record holds, taken from the folder it sits in and given its fields and rules by one schema
file.

### Upstream

The corpus this one takes the framework from, named in the descriptor at the root.
