# Example Libraries — a knowledge corpus

> **Everything here is invented.** Example Libraries is a fictional public-library consortium. Its services, incidents,
> decisions and obligations were written to give each knowledge type something real-shaped to hold, and to push the
> schema into its awkward corners. Nothing in this corpus describes anyone, and no hostname in it resolves: every one
> is under `example.com`, which [RFC 2606](https://www.rfc-editor.org/rfc/rfc2606) reserves so that it can never be
> registered. **Delete these records before you write your first real one.**

A corpus is plain markdown in git where every document has a type and every type has a schema. This one is what
[`kac`](../tooling/) is run against on every commit. It proves the tool works over real content, and not only over
its fixtures.

Why it is built this way is in [`knowledge-as-code.md`](knowledge-as-code.md) and the documents beneath it.
[`../README.md`](../README.md) is the repository this corpus sits in.

## The knowledge types

<!-- BEGIN GENERATED: types-index -->

| Type                         | Tier        | What it holds                                                                                                    |
|------------------------------|-------------|------------------------------------------------------------------------------------------------------------------|
| [ADR](/adrs)                 | decided     | An architecturally significant decision affecting more than one repository, and the reasoning behind it.         |
| [Capability](/capabilities)  | descriptive | What we offer a customer and why, as a hub linking to what implements, tests and constrains it.                  |
| [Control](/controls)         | normative   | How a standard's rules are verified: the mechanism, the frequency, and the evidence it leaves.                   |
| [Data](/data)                | descriptive | Which service owns which data, how long it is kept, how sensitive it is, and where personal data flows.          |
| [Discovery](/discoveries)    | observed    | Something noticed during work and not yet verified, captured cheaply and expiring unless promoted.               |
| [Explanation](/explanations) | descriptive | Narrative that helps you understand how something works, or why it is shaped the way it is.                      |
| [FAQ](/faqs)                 | normative   | A problem with a confirmed fix, promoted from a discovery once a human has verified it.                          |
| [Glossary](/glossary)        | descriptive | The ubiquitous language — terms whose meaning is specific to us, or which are easily confused.                   |
| [Integration](/integrations) | descriptive | An external system we depend on: the contract, the auth, the failure modes, their SLA and our fallback.          |
| [NFR](/nfrs)                 | normative   | A non-functional requirement — availability, latency, RPO, RTO — stated with how it is measured.                 |
| [Policy](/policies)          | normative   | A high-level engineering commitment: the what and the why, largely stack-agnostic and changing rarely.           |
| [Postmortem](/postmortems)   | decided     | What actually happened during an incident — timeline, impact, root cause, contributing factors, actions.         |
| [Process](/processes)        | procedural  | A planned procedure followed deliberately — releasing, onboarding, provisioning, rotating a secret.              |
| [Runbook](/runbooks)         | procedural  | An incident-time procedure read under pressure: terse, imperative, structured as a decision tree.                |
| [Service](/services)         | descriptive | One deployable component: purpose, repo, platform, environments, dependencies, data stores, owner.               |
| [Standard](/standards)       | normative   | The rulebook — imperative, RFC 2119, with concrete examples and a conformance checklist.                         |
| [Tool](/tools)               | descriptive | The approved-software register — what is chosen, rejected or deprecated, and the version ranges we stand behind. |

**Where does a document go?** The [taxonomy](knowledge-as-code/taxonomy.md) has the decision table, what each type is
and is not, and the calls that are genuinely close.

<!-- END GENERATED: types-index -->

## Working in this corpus

Needs `kac` on your path. [`../README.md`](../README.md#running-the-tool) covers the ways to get one.

```bash
kac validate     # frontmatter, links, structure, clauses and the graph
kac index        # regenerate the indexes and generated blocks
kac export       # write the corpus to .dist/export/ as data a consumer reads
kac bundle       # assemble that export and .plugin/ into a plugin under .dist/plugin/
kac checks       # list every check the validator implements
```

A `kac` and a `kac.cmd` sit beside this file, wrapping `dotnet run --project ../tooling/kac`. Run them as
`./kac validate` while you are changing the tool, because they reach the working tree and an installed `kac` does
not.

The example records are every `<type>/*.md` that is not `_index.md` or `_template.md`. `kac validate` covers them, so
they are held to the same standard as real content. A schema change that breaks them fails CI here rather than in
somebody's repository.

## The seeds come in two kinds

**Some are close to real.** The [policies](policies.md) are the clearest case, and the part worth reading on their own
terms. The clause model, the mnemonic ids, the per-clause alignment and the gap analysis that closed it were worked out
on them rather than assumed. They are principle-level and stack-agnostic by design, so they name no service and invent
no domain. That is why they would survive adoption with only the specifics rewritten.

**Others need somewhere to stand.** A service catalogue demonstrates nothing without an estate; an NFR has to apply to
something; a postmortem needs an incident. Those use the consortium above, so the records form a graph instead of a
list.

**Delete these records; do not adapt them.** Each seeded type page says so at the top. They are chosen to exercise the
schema's awkward corners rather than to resemble your estate: a monorepo shipping three deployables, a CDN whose
`repo` cannot answer where its content comes from, and a service coupled to the whole estate with no dependency edges.

## Maturity

**Proven types: ADRs, policies and services.** The remaining schemas are written and have never met real content.
That is the honest limit: a schema is wrong in ways only real content reveals.

Policies alone forced the mnemonic id style, a category field, the identity line, the clause table and the checks that
hold it. Services were proven elsewhere: a consumer repository built a full catalogue against this type and returned a
run of findings, tracked as issues upstream. Treat the rest as drafts.

## What the corpus claims about its organisation

Showing how framework alignment works requires something to align with, so this corpus takes a position: ISO/IEC 27001
registered, obliged by the UK GDPR, running on Azure, part of a management system whose other halves belong to
facilities, HR and IT. That posture and the estate fit each other. A public library is a public-sector body, so the
accessibility obligations the policies cite genuinely bind it. None of it describes anyone. It is there so that
[`frameworks.md`](frameworks.md) and the policies' `Alignment` columns have something to point at. Rewrite it on the
way through.

The Azure assumption is the demonstration content's alone: the Azure Well-Architected entry in
[`frameworks.md`](frameworks.md) and the pillars the policies cite from it. AWS and Google publish near-identical
pillars, so changing it is a relabelling rather than a re-mapping. No clause's wording names a provider, only the
references beside it.

## What this corpus declares about itself

[`.corpus.yaml`](.corpus.yaml) says what this corpus is, which of the framework's types it has adopted, where its
published form is served from, and any deviation from the shared baseline it has deliberately accepted. Three versions
live in it, each named for what it versions, and the file's own comments say what each one is for.

Declaring `types:` states a decision rather than the shape the folders happen to have. `validate` then holds the corpus
to standing up everything it declared, and every generated list of types is written from that declaration. A corpus
that declares nothing still works: the tool reads adoption off the folders instead.

## The plugin

This corpus travels to another repository as a Claude Code plugin, so a session there can ask it what a term means
without cloning it. `kac export` writes the corpus as data. `kac bundle` assembles that data and the
[`.plugin/`](.plugin/) tree into an installable plugin. CI publishes the result to the
[`marketplace`](https://github.com/paul80nd/knowledge-as-code/tree/marketplace) branch on every push to `main`. That
branch is orphaned and never merged back, so nothing CI builds can reach the source.

Install it into a session with:

```
/plugin marketplace add paul80nd/knowledge-as-code@marketplace
/plugin install knowledge-as-code@knowledge-as-code
```

Then ask about a term this corpus defines, such as *what does Borrower mean here?* The answer comes from the glossary
rather than from the model. A session is told the glossary is there when it starts, so it does not have to know to ask
first.

What is published carries this corpus's `content-version`, which is below `1.0.0` and means it: the records are
illustrative and the schema still moves. Claude Code treats that string as the whole answer to whether an update
exists. A change merged without moving `content-version` in `.corpus.yaml` would reach nobody, so the publish stops
before pushing. Bump it with the change.

### Proving a change before it ships

Build the plugin, and install the build rather than the branch:

```bash
kac export       # write the corpus to .dist/export/
kac bundle       # assemble .dist/plugin/, with .dist/ as the marketplace holding it
```

Then, in a session:

```
/plugin marketplace add ./.dist
/plugin install knowledge-as-code@knowledge-as-code
```

and ask it the question your change was about. **Uninstall before you rebuild.** The version is what pins the copy
Claude Code holds, so a second build at the same `content-version` leaves the first one installed and your change
invisible.

`sh ../tooling/tests/round-trip.sh` walks the same path without a session, and without touching your own
configuration. It installs into a config directory of its own, looks a term up, fetches a record through the raw link
the export wrote, and says which step failed. CI runs it on Linux and Windows.

### What the plugin does not do yet

**It carries one type.** A term is the smallest useful thing one corpus can hand another, so the glossary went
first. The rest is agreed and unbuilt:

* **Every other type.** Only the glossary is exported and only the glossary is read. An ADR or a policy travels nowhere.
* **Publishing from Azure DevOps.** [`azure-pipelines.yml`](azure-pipelines.yml) validates and publishes nothing, so a
  corpus hosted there builds its bundle by hand. That is where this has to work for the first adopters, and it is a
  pipeline of its own rather than a translation of the GitHub one.
* **Anything that writes back.** The plugin answers questions. Nothing yet lets a session contribute a record
  ([#21](https://github.com/paul80nd/knowledge-as-code/issues/21)) or report what it looked for and could not find
  ([#13](https://github.com/paul80nd/knowledge-as-code/issues/13)).
* **The distillation pass** that would fold what sessions learned back into the corpus
  ([#24](https://github.com/paul80nd/knowledge-as-code/issues/24)).
* **A hook that matches a prompt against the glossary**, so a term is defined where it is used rather than when someone
  thinks to ask. The `SessionStart` breadcrumb says the glossary is there; nothing yet reads what was typed.

## Layout

```
<type>.md              # what the type is, why it exists, how to contribute — one per type
<type>/
  ├── _index.md        # GENERATED from frontmatter
  ├── _template.md     # what humans and agents copy
  └── <records>.md

knowledge-as-code.md   # the approach, and the way in to everything below
knowledge-as-code/     # the system's own documentation
  ├── taxonomy.md      # the seventeen types and where things go
  ├── metadata.md      # the frontmatter fields
  ├── contributing.md  # how a contribution is made and reviewed
  ├── style.md         # how we write, in every document and every comment
  ├── authoring.md     # what a document's tier adds to that
  ├── principles.md    # why the framework is shaped this way
  ├── lineage.md       # where the taxonomy's names came from
  └── automation.md    # what is generated, validated and scheduled
kac, kac.cmd           # launchers that wrap `dotnet run --project ../tooling/kac`
.corpus.yaml           # what this corpus is, and where it publishes
.claude/skills/        # agent skills for working on this corpus
.plugin/               # source for the plugin that carries this corpus's export to another repository
.schema/               # the machine-readable schema — the source of truth
```

The machinery is dot-prefixed: `.schema/`, `.corpus.yaml`, `.plugin/`. The markdown stays the visible half, so an
Azure DevOps wiki published from this tree shows knowledge rather than mechanism. `knowledge-as-code/` holds
documentation and nothing else: what the tool reads lives beside the tool.

Adding a knowledge type is adding a YAML file to `.schema/` and a line to `.corpus.yaml`, not editing the tool.
