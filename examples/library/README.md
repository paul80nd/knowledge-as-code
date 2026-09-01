# Example Libraries: a knowledge corpus

> **Everything here is invented.** Example Libraries is a fictional public-library consortium. Its services, decisions
> and vocabulary were written to give the schema something real-shaped to hold, and to push it into its
> awkward corners. Nothing in this corpus describes anyone, and no hostname in it resolves: every one is under
> `example.com`, which [RFC 2606](https://www.rfc-editor.org/rfc/rfc2606) reserves so that it can never be registered.
> **Delete these records before you write your first real one.**

A corpus is plain markdown in git where every document has a type and every type has a schema. This one is
self-contained: it consumes nothing.
[`../README.md`](../README.md) sets it beside the others here and says what each one demonstrates.

**Read this one, copy [`../../template/`](../../template/).** The template is the same corpus with the content taken
out, and it is what a new corpus starts from. Everything here is a worked example to borrow ideas from.

Why it is built this way is in [`knowledge-as-code.md`](knowledge-as-code.md) and the documents beneath it.

## The knowledge types

<!-- BEGIN GENERATED: types-index -->

| Type                           | Tier        | What it holds                                                                                            |
|--------------------------------|-------------|----------------------------------------------------------------------------------------------------------|
| [ADR](adrs.md)                 | decided     | An architecturally significant decision affecting more than one repository, and the reasoning behind it. |
| [Capability](capabilities.md)  | descriptive | What we offer a customer and why, as a hub linking to what implements, tests and constrains it.          |
| [Data](data.md)                | descriptive | Which service owns which data, how long it is kept, how sensitive it is, and where personal data flows.  |
| [Glossary](glossary.md)        | descriptive | The ubiquitous language. Terms whose meaning is specific to us, or which are easily confused.            |
| [Integration](integrations.md) | descriptive | An external system we depend on: the contract, the auth, the failure modes, their SLA and our fallback.  |
| [Process](processes.md)        | procedural  | A planned procedure followed deliberately (releasing, onboarding, provisioning, rotating a secret).      |
| [Runbook](runbooks.md)         | procedural  | An incident-time procedure read under pressure: terse, imperative, structured as a decision tree.        |
| [Service](services.md)         | descriptive | One deployable component: purpose, repo, platform, environments, dependencies, data stores, owner.       |

**Where does a document go?** The [taxonomy](knowledge-as-code/taxonomy.md) has the decision table, what each type is
and is not, and the calls that are genuinely close.

<!-- END GENERATED: types-index -->

## Working in this corpus

Needs `kac` on your path. [`../../README.md`](../../README.md#running-the-tool) covers the ways to get one.

```bash
kac validate     # frontmatter, links, structure, clauses and the graph
kac generate     # regenerate the indexes and generated blocks
kac export       # write the corpus to .dist/export/ as data a consumer reads
kac bundle       # assemble that export and .plugin/ into a plugin under .dist/plugin/
kac checks       # list every check the validator implements
```

While you are changing the tool, run `dotnet run --project ../../tooling/kac -- validate` instead. That reaches the
working tree, and an installed `kac` does not.

The example records are every `<type>/*.md` that is not `_index.md` or `_template.md`. `kac validate` covers them, so
they are held to the same standard as real content. A schema change that breaks them fails CI here rather than in
somebody's repository.

## The estate these records stand on

A service catalogue demonstrates nothing without an estate. A glossary term needs a service that owns it. So these
records describe one consortium: a public catalogue, a lending system, a search index, a CDN serving jacket images, and
the jobs around them. They form a graph instead of a list.

**Delete these records rather than adapting them.** They are chosen to exercise the schema's awkward corners, and will
not resemble your estate: a monorepo shipping three deployables, a CDN whose `repo` cannot answer where its content
comes from, and a service coupled to the whole estate with no dependency edges.

## Maturity

**Services are the proven type here.** A consumer repository built a full catalogue against that schema and returned a
run of findings, tracked as issues upstream. The other seven schemas have met little real content, and five of them hold
none at all. That is the honest limit: a schema is wrong in ways only real content reveals.

## What this corpus declares about itself

[`.corpus.yaml`](.corpus.yaml) says what this corpus is, which of the framework's types it has adopted, where its
published form is served from, and any deviation from the shared baseline it has deliberately accepted. Three versions
live in it, each named for what it versions, and the file's own comments say what each one is for.

Declaring `types:` states a decision rather than the shape the folders happen to have. `validate` then holds the corpus
to standing up everything it declared, and every generated list of types is written from that declaration. A corpus that
declares nothing still works: the tool reads adoption off the folders instead.

## The plugin

This corpus travels to another repository as a Claude Code plugin, so a session there can ask it what a term means
without cloning it. `kac export` writes the corpus as data. `kac bundle` assembles that data and the
[`.plugin/`](.plugin/) tree into an installable plugin. CI publishes the result to the
[`marketplace`](https://github.com/paul80nd/knowledge-as-code/tree/marketplace) branch on every push to `main`. That
branch is orphaned and never merged back, so nothing CI builds can reach the source.

Install it into a session with:

```
/plugin marketplace add paul80nd/knowledge-as-code@marketplace
/plugin install example-libraries@example-libraries
```

Then ask about a term this corpus defines, such as *what does Borrower mean here?* The answer comes from the glossary
rather than from the model. A session is told the glossary is there when it starts, so it does not have to know to ask
first.

What is published carries this corpus's `content-version`, which is below `1.0.0` and means it: the records are
illustrative and the schema still moves. Claude Code treats that string as the whole answer to whether an update exists.
A change merged without moving `content-version` in `.corpus.yaml` would reach nobody, so the publish stops before
pushing. Bump it with the change.

### Proving a change before it ships

Build the plugin, and install the build rather than the branch:

```bash
kac export       # write the corpus to .dist/export/
kac bundle       # assemble .dist/plugin/, with .dist/ as the marketplace holding it
```

Then, in a session:

```
/plugin marketplace add ./.dist
/plugin install example-libraries@example-libraries
```

and ask it the question your change was about. **Uninstall before you rebuild.** The version is what pins the copy
Claude Code holds, so a second build at the same `content-version` leaves the first one installed and your change
invisible.

`sh ../../tooling/tests/round-trip.sh` walks the same path without a session, and without touching your own
configuration. It installs into a config directory of its own, checks that this corpus shipped the glossary skill and
not the policy skill it has no records for, looks a term up, fetches a record's source from the base, prefix and ref the
export wrote, and says which step failed. CI runs it on Linux and Windows.

### What the plugin does not do yet

**This plugin carries one type.** `library/` adopts glossary and exports it, so `glossary-lookup` ships and the policy
and standards skills are trimmed. What is agreed and unbuilt:

* **A type with no `export:` block.** `glossary`, `policies` and `standards` each declare one, so their records travel
  and their parts reach a lookup skill. An ADR travels nowhere.
* **Publishing from Azure DevOps.** The [Azure Pipelines starter](../../template/azure-pipelines.yml) builds the bundle
  and publishes nothing, so a corpus hosted there has no route to a marketplace. That is where this has to work for the
  first adopters, and it is a pipeline of its own rather than a translation of the GitHub one.
* **Anything that writes back.** The plugin answers questions. Nothing yet lets a session contribute a record
  ([#21](https://github.com/paul80nd/knowledge-as-code/issues/21)).
* **The distillation pass** that would fold what sessions learned back into the corpus
  ([#24](https://github.com/paul80nd/knowledge-as-code/issues/24)).
* **A hook that matches a prompt against the glossary**, so a term is defined where it is used rather than when someone
  thinks to ask. The `SessionStart` breadcrumb says the glossary is there, and nothing yet reads what was typed.

## Layout

```
<type>.md              # what the type is, why it exists, how to contribute. One per type
<type>/
  ├── _index.md        # GENERATED from frontmatter
  ├── _template.md     # what humans and agents copy
  └── <records>.md

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

The `.schema/` this corpus is judged against sits at the repository root, one copy shared with every corpus here and
with `template/`. A corpus outside this repository carries its own at its own root, which is where `kac` looks first.

Adding a knowledge type is adding a YAML file to `.schema/` and a line to `.corpus.yaml`, not editing the tool.
