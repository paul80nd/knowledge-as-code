# Automation

What a pipeline does for a corpus, and what it deliberately leaves alone. `kac checks` lists every check the validator
can emit against the corpus it is run in, and that command is the authority on what runs. This page says what the checks
are for.

**Every check reads the corpus. None reads the estate the corpus describes.** A service deleted last month still
validates cleanly, and a descriptive record is only as true as the person who last read it. That limit is worth
carrying: a green build says the corpus is consistent, rather than that it is right.

## What a pipeline runs

Two commands answer for every corpus, and a corpus shipping an agent plugin adds two more.

| The command                              | Answers                                          |
|------------------------------------------|--------------------------------------------------|
| [`validate`](../cli/validate.md)         | are the records correct against the schema       |
| [`generate --check`](../cli/generate.md) | is the derived content in step with the records  |
| [`export`](../cli/export.md)             | can the corpus still be written out as data      |
| [`bundle`](../cli/bundle.md)             | can that export still be assembled into a plugin |

[Running it in CI](../ci.md) carries the workflow for GitHub Actions and for Azure Pipelines.

Failing rather than warning is the whole of the trade. A warning nobody reads is a rule nobody keeps, and the value of a
corpus is that a reader can believe it.

## What validation asks

Four questions. The grouping is this page's, and no file carries it. `kac checks` prints the checks themselves, and
[Checks](../design/checks.md) says where each one comes from.

**Does the record parse and declare itself?** Frontmatter is YAML, required fields are present, enum values are ones the
type declares, and dates are quoted. An `id` is unique across the corpus and agrees with both its type's prefix and the
folder it sits in. A document claiming a tier its folder does not have is a placement error rather than a metadata
error, and reads better reported as one.

**Does the graph hold?** Every id a cross-reference names resolves to a document that exists, and every relative link
resolves. Reciprocal pairs agree in both directions, so a one-sided link fails. `implements` is deliberately not
reciprocal: it points up from a standard to a policy, and nobody sitting at the policy can know what implements it.

**Does the record do what its tier asks?** Behaviour sets the rule, so the checks follow the tier. A standard cites what
it derives from. A procedural one records when it was last rehearsed. An observed one carries an expiry, because a
record that never expires is not observed.

**Is it safe to publish?** A corpus is broadly readable, so nothing may read as a credential or as real data. The rule
is declared on the types where the risk actually lives, which today means integrations and data records.

### Rules the schema declares and nothing runs

A type may declare a rule with a description and no severity. `validate` skips it, and the type's own page renders it
under *Declared, not yet enforced*, so a reader meets the gap on the page they were already reading. What the schema
refuses is a severity with nothing behind it, so a rule cannot claim to run and then not.

## What generation protects

Generated content sits between markers inside otherwise hand-written files, so one file serves one purpose.
[Generation](../design/generation.md#only-the-region-between-the-markers-is-rewritten) says what that buys. A block
whose markers have gone is written by nothing, so their presence is itself checked.

Only the types a corpus adopted are generated, so every generated list names pages that corpus actually holds.
[`generate`](../cli/generate.md) says which blocks exist and what each is built from.

**A pipeline never commits.** Where generated content is stale the build fails and names the command to run locally.
[Contributing](contributing.md#what-a-pipeline-will-not-do) says what that trade buys.

## What is not a record

The framework's own documents, the scaffolding folders, a type's `_template.md` and a corpus's root pages are not
records, and no schema is applied to them. Each path is named, so nobody answers a validation failure by quietly
widening an exclusion. The `_` prefix is the one deliberate glob: it belongs to the framework's own artefacts, and the
tool matches on the prefix itself wherever it appears in a path.

Excluding a file as a record does not excuse it from every check. The framework's own documents still link to things, so
their links are resolved like any page's. A template is checked as a template: a defect in one becomes every record's
problem, and the next author is the one who finds it.
[`validate`](../cli/validate.md) lists each of those passes and what it asks.

## What the corpus decides

`types:` in [`.corpus.yaml`](../corpus-descriptor.md) names the knowledge types a corpus has adopted, and bounds both
validation and generation. `skip:` names each file the corpus holds differently on purpose.
[`update`](../cli/update.md) reads both, and is what holds a copy of the framework answerable to its declaration.
