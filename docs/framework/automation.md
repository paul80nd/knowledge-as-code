# Automation

A pipeline runs `kac` over a corpus (one repository of knowledge records kept in git) on every pull request. This page
says what those checks are for. `kac checks` lists every check the validator can report against the corpus it runs in,
and that command is the authority on what actually runs.

**Every check reads the corpus. None reads the estate the corpus describes.** A service deleted last month still
validates cleanly. So a green build says the corpus is consistent, not that it is right.

## What a pipeline runs

Every corpus runs two commands. A corpus that also publishes an agent plugin runs two more. The plugin is the corpus
packaged so an AI agent can install it, and the export is the corpus written out as data for that packaging to read.

| The command                              | What it asks                                     |
|------------------------------------------|--------------------------------------------------|
| [`validate`](../cli/validate.md)         | are the records correct against the schema       |
| [`generate --check`](../cli/generate.md) | is the derived content in step with the records  |
| [`export`](../cli/export.md)             | can the corpus still be written out as data      |
| [`bundle`](../cli/bundle.md)             | can that export still be assembled into a plugin |

[Running it in CI](../ci.md) gives you the workflow for GitHub Actions and for Azure Pipelines.

An error fails the build. A warning and an info are printed and do not change the exit code.

## What validation asks

Validation asks four questions. This grouping is for explanation only. The tool does not use it. `kac checks` prints the
checks themselves, and [Checks](../design/checks.md) says where each one comes from.

**Does the record parse and declare itself?** Frontmatter is YAML. Required fields are present. Enum values are ones the
type declares, and dates are quoted. An `id` is unique across the corpus, and it agrees with both its type's prefix and
the folder it sits in. A tier says how far a record may be trusted, and how it must be written. A document claiming a
tier its folder does not have is reported as a placement error, not a metadata error.

**Do the references resolve?** Every id a cross-reference states resolves to a document that exists, and every relative
link resolves. A reciprocal pair agrees in both directions, so a one-sided link fails. `implements` is deliberately not
reciprocal. It points up from a standard to a policy clause, and a policy clause cannot list what implements it.

**Does the record do what its tier asks?** The checks follow the tier, because the tier is what sets how a record must
be written. A standard cites what it derives from. A procedural record states when it was last rehearsed. An observed
record states when it expires, because a record that never expires is not observed.

**Is it safe to publish?** A corpus is broadly readable, so nothing in it may read as a credential or as real data. The
rule is declared on the types where the risk sits, which today means integrations and data records.

### Declared rules that do not run

A type may declare a rule with a description and no severity. `validate` skips it, and the type's own page renders it
under **Declared, not yet enforced**. A reader meets the gap on the page they were already reading. The schema refuses
the opposite case, a severity with nothing behind it, so a rule cannot claim to run and then not.

## What generation protects

Generated content sits between markers inside otherwise hand-written files.
[Generation](../design/generation.md#the-region-between-the-markers) explains what that buys. A block whose markers have
gone is written by nothing, so `validate` checks that both markers are still there.

`kac` generates only the types a corpus adopted, so every generated list points at pages that corpus actually has.
[`generate`](../cli/generate.md) says which blocks exist and what each one is built from.

**A pipeline never commits.** [Contributing](contributing.md#what-a-pipeline-will-not-do) explains why.

## What is not a record

The framework's own documents, the scaffolding folders, a type's `_template.md` and a corpus's root pages are not
records, and no schema is applied to them. Each path is listed one by one, so nobody can fix a validation failure by
widening an exclusion. The `_` prefix is the one deliberate glob. It belongs to the framework's own files, and `kac`
matches it wherever it appears in a path.

Excluding a file as a record does not excuse it from every check. The framework's own documents still link to things, so
`validate` resolves their links like any page's. A template is checked as a template. A defect in a template appears in
every record made from it.
[`validate`](../cli/validate.md) lists each of those passes and what it asks.

## What the corpus decides

`types:` in [`.corpus.yaml`](../corpus-descriptor.md) states the knowledge types a corpus has adopted, and it bounds
both validation and generation. `skip:` states each file the corpus keeps differently on purpose.
[`update`](../cli/update.md) reads both, and checks a copy of the framework against what that corpus declared.

[Running it in CI](../ci.md) is the page for wiring these commands into GitHub Actions or Azure Pipelines.
