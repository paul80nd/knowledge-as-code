# Example Payments: a domain corpus

> **Everything here is invented.** Example Payments is a fictional payments system, and nothing in this corpus
> describes anyone. A record written here takes its hostnames from `example.com`, which
> [RFC 2606](https://www.rfc-editor.org/rfc/rfc2606) reserves so that it can never be registered.

A corpus is plain markdown in git where every document has a type and every type has a schema. This one is a domain
corpus, and it is thin on purpose. It adopts three types, and it writes rules about handling a payment and nothing
else. A domain corpus inherits its governance rather than restating it, and a thin corpus shows that where a full one
hides it.
[`../README.md`](../README.md) sets it beside the other two corpora here and says what each one demonstrates.

**A new corpus starts from [`../../template/`](../../template/)**, which is the same corpus with the content taken out.
This one is closer to that template than either of its neighbours, and it is still a corpus rather than a copy of one.

Why it is built this way is in [`knowledge-as-code.md`](knowledge-as-code.md) and the documents beneath it.

## The knowledge types

<!-- BEGIN GENERATED: types-index -->

| Type                     | Tier        | What it holds                                                                                      |
|--------------------------|-------------|----------------------------------------------------------------------------------------------------|
| [NFR](nfrs.md)           | normative   | A non-functional requirement (availability, latency, RPO, RTO) stated with how it is measured.     |
| [Service](services.md)   | descriptive | One deployable component: purpose, repo, platform, environments, dependencies, data stores, owner. |
| [Standard](standards.md) | normative   | The rulebook, imperative, RFC 2119, with concrete examples and a conformance checklist.            |

**Where does a document go?** The [taxonomy](knowledge-as-code/taxonomy.md) has the decision table, what each type is
and is not, and the calls that are genuinely close.

<!-- END GENERATED: types-index -->

## Working in this corpus

Needs `kac` on your path. [`../../README.md`](../../README.md#running-the-tool) covers the ways to get one.

```bash
kac restore      # fetch the corpora this one consumes into .imports/
kac validate     # frontmatter, links, structure, clauses and the graph
kac generate     # regenerate the indexes and generated blocks
kac export       # write the corpus to .dist/export/ as data a consumer reads
kac bundle       # assemble that export and .plugin/ into a plugin under .dist/plugin/
kac checks       # list every check the validator implements
```

`restore` comes first, and it needs a package to take. Run `kac export` and then `kac pack` in
[`../engineering/`](../engineering/) once, and this corpus's `source:` has a folder to read.

While you are changing the tool, run `dotnet run --project ../../tooling/kac -- validate` instead. That reaches the
working tree, and an installed `kac` does not.

## What it demonstrates

**A domain corpus consuming a governance one.** `.corpus.yaml` names `example-engineering` in `consumes:`, and
`kac restore` unpacks that corpus's published export under `.imports/eng/`. A standard here then cites
`eng:pol-SCRT.STORE`, and `kac validate` holds that citation to naming a policy and a clause that really exist. Cite a
clause the governance layer does not carry and the build fails, exactly as it would for a record next door.

**Run `kac restore` before anything else.** A declared import that has not arrived is an error naming the command. The
folder it fills is not committed, so a fresh clone holds none of it.

**Thinness is the point.** There are no policies here and no compliance posture. Both belong to `../engineering/`, and
a payments team reads them from there rather than keeping a copy that drifts. The whole corpus is two services, the
NFRs they carry, and the standards saying what the inherited policies mean for a payment. Everything else a payments
team is bound by is one repository away and cited by id.

**The same obligation is met at two layers.** `eng:pol-SCRT.LOGS` prohibits writing a secret to a log. The governance
corpus's own secret-handling standard discharges it for the whole estate, and [std-TELEM] discharges it again for a PSP
key and a card token. Neither restates the other, and both name the clause.

## What this corpus declares about itself

[`.corpus.yaml`](.corpus.yaml) says what this corpus is, which of the framework's types it has adopted, where its
published form is served from, and any deviation from the shared baseline it has deliberately accepted. Three versions
live in it, each named for what it versions, and the file's own comments say what each one is for.

Declaring `types:` states a decision rather than the shape the folders happen to have. `validate` then holds the corpus
to standing up everything it declared, and every generated list of types is written from that declaration. That matters
most here: fourteen of the seventeen types this framework offers were declined, and the file is where that decision is
written down.

## Layout

```
<type>.md              # what the type is, why it exists, how to contribute. One per type
<type>/
  ├── _index.md        # GENERATED from frontmatter
  ├── _template.md     # what humans and agents copy
  └── <records>.md

frameworks.md          # the external frameworks this corpus refers to, and what each one obliges
knowledge-as-code.md   # the approach, and the way in to everything below
knowledge-as-code/     # the system's own documentation
  ├── taxonomy.md      # the types and where things go
  ├── metadata.md      # the frontmatter fields
  ├── contributing.md  # the way in for somebody adding to this corpus
  └── lineage.md       # where the taxonomy's names came from
.corpus.yaml           # what this corpus is, and where it publishes
.plugin/               # source for the plugin that carries this corpus's export to another repository
```

The machinery is dot-prefixed: `.corpus.yaml` and `.plugin/`. The markdown stays the visible half, so an Azure DevOps
wiki published from this tree shows knowledge rather than mechanism. `knowledge-as-code/` holds documentation and
nothing else: what the tool reads lives beside the tool.

The `.schema/` this corpus is judged against sits at the repository root, one copy shared with the other two corpora and
with `template/`. A corpus outside this repository carries its own at its own root, which is where `kac` looks first.

Adding a knowledge type is adding a YAML file to `.schema/` and a line to `.corpus.yaml`, not editing the tool.

[std-TELEM]: standards/operations/payment-telemetry.md
