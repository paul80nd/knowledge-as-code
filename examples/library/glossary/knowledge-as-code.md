---
id: gls-knowledge-as-code
type: glossary
tier: descriptive
status: draft
owner: human:paul.law
review-by: "2027-08-12"
tags: [ framework ]
---

# Knowledge as code

`Glossary: gls-knowledge-as-code` `DRAFT`

The words the framework uses about itself. Every corpus running the framework inherits them unchanged.

## Scope

This glossary covers the mechanism: corpora, types, records, the layers a file travels in, and what the tool does to
them. A word about the estate a corpus describes belongs in that corpus's own glossary, however often it appears here.

A type's name is not defined here. Each type states what it contains in its own schema file. The framework renders that
onto the type's page, into the taxonomy, and into the list of collisions where the name already means something else to
a reader. A second, hand-written account would go stale.

Every corpus shares this file. An entry cites no record, and names a type instead of linking to one. A corpus that
adopted three types reads the same page as one that adopted them all.

## Terms

### Check

A validation the tool runs, reported under its own id and listed in the generated table on each type's page.

### Clause

An addressable row in a policy or standard, with one obligation and an id something else can cite.

**Not:** a section. A clause is part of one, and its id lets an auditor quote the obligation on its own.

### Corpus

One repository of knowledge records, with the schema and tooling it runs, and a descriptor stating what it is and where
it takes the framework from.

**Not:** the wiki, which is how a corpus is published and read.

### Document

The general word for anything written down here. The tool's summary line uses it to count records.

**Not:** a synonym for record wherever the difference matters. A page and a template are written down and are neither.

### Drift

A local edit to a file the framework owns. `kac update --check` reports it, and the next update overwrites it.

**Not:** a change to a seed file. A seed belongs to the corpus from the moment it arrives, and nothing checks it
against the template.

### Export

The corpus written out as data by `kac export`, for a consumer that reads it without cloning the repository. Each type
decides which of its fields, sections and parts travel.

**Not:** the package or the plugin. Both are built from an export.

### Framework

The shared mechanism a corpus runs: the schema, the tooling and the documentation that travel between corpora.

**Not:** an external framework such as ISO 27001, which is what the frameworks register means by the word. Both senses
are in use in every corpus, and only the register states a compliance posture.

### Identity line

The line beneath a record's H1 stating its type, id and status. It is written for a person, and the frontmatter above
it is written for a machine.

### KaC

Knowledge as Code, abbreviated: the framework itself, and what the tool `kac` is named after.

**Not:** `kac`, which is the tool alone.

### Layer

What a corpus receives of a file, and what happens to it next: overlay, seed, removed or withheld. The template
manifest declares it once, and `kac update` reads it file by file.

### Lifecycle

Whether a type's records stay current or become immutable once accepted. The type fixes which.

**Not:** status, which is the stage one record has reached.

### Mechanism

The framework's files as they appear in one corpus: the half that travels.

**Not:** the knowledge. `kac update` refreshes the mechanism from a template and reads no record.

### Overlay

A file identical in every corpus running the framework, owned by the framework and not by the corpus. An edit to one is
drift, and the next update overwrites it.

### Package

An export that `kac pack` zips into a versioned file. A registry stores it, and another corpus fetches it with
`kac restore`.

**Not:** the plugin. A package is read by a corpus. A plugin is installed by an agent.

### Page

A type's root page, with no frontmatter of its own. It states what the type contains, what it excludes, and how to add
a record.

**Not:** a record. It describes the records, so it has no id and no identity line.

### Plugin

An export plus the skills and hooks under `.plugin/`, which `kac bundle` assembles into something an agent installs. A
skill in it answers questions from the export bundled with it.

**Not:** the corpus. A plugin is a frozen copy, so an agent writes back by raising an issue.

### Record

A knowledge document filed under a type, with frontmatter, an id and an identity line.

**Not:** every file in a type's folder. The generated index and the template are there too.

### Rule

A behaviour a type declares in its schema. It either dispatches to a check or remains a declared intention.

**Not:** a check. A check runs. A rule may be a statement of intent, and the type's page lists those under *Declared,
not yet enforced*.

### Seed

A file the framework provides as a starting point, which the corpus then owns. A difference from the template is a
decision, not a defect.

### Template

The file a contributor copies to start a record, checked against the fields its type declares.

**Not:** a record. It has no id and appears in no index. The tool checks it anyway, so every copy starts sound.

### Tier

What a record's type states about how far it may be trusted and how it is written: decided, normative, descriptive,
procedural or observed.

**Not:** type. Several types share a tier, and the writing rules and the review bar follow the tier.

### Type

The kind of knowledge a record contains. The folder decides it, the record repeats it in its `type` field, and one
schema file gives every record of that type its fields and rules.

### Upstream

The repository this corpus takes the framework from, stated in the descriptor at the root.
