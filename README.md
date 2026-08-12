# knowledge-as-code

A structured, validated engineering wiki that both people and AI sessions can read from and contribute to.

Plain markdown in git, reviewed by PR, published as a wiki. What makes it more than a folder of documents is that
**every document has a type, and every type has a schema** — so an index can be generated rather than maintained, a
broken cross-reference fails CI rather than rotting quietly, and an agent can be told where a thing goes instead of
guessing.

This repository is the **framework master**: the mechanism, and a demonstration corpus to fork. Why it is built this way
is in [`knowledge-as-code.md`](knowledge-as-code.md) and the documents beneath it.

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

## What is here

Two halves, and the split is the point.

**The mechanism** — `kac`, a validator and generator, plus the machine-readable schema it enforces. Generic, portable to
any organisation, shared byte-for-byte between every corpus running this framework.

**The seeds** — a demonstration corpus: seventeen knowledge types, each with a root page explaining what it is and when
to use it, and a template to copy. Opinionated, and meant to be forked — you take them once, localise the examples to
your own domain, and never reconcile them again.

Which files fall on which side is declared in [`.tooling/manifest.yaml`](.tooling/manifest.yaml), not asserted in prose.

**What does not live here** is anybody's actual knowledge. Every record in this repository is illustrative and is meant
to be deleted. A corpus derived from this one holds its organisation's policies, services and decisions; this one holds
the shapes they go in.

## The seeds come in two kinds

**Some are close to real.** The [policies](policies.md) are the clearest case, and the part worth reading on their own
terms. The clause model, the mnemonic ids, the per-clause alignment and the gap analysis that closed it were worked out
on them rather than assumed. They are principle-level and stack-agnostic by design, so they name no service and invent
no domain — which is why they would survive adoption with only the specifics rewritten.

**Others need somewhere to stand.** A service catalogue demonstrates nothing without an estate; an NFR has to apply to
something; a postmortem needs an incident. Those use **one fictional organisation throughout — Example Libraries, a
public-library consortium** — so the records form a graph instead of a list.

It cannot be mistaken for anyone real, by construction. Every hostname is under `example.com`, which
[RFC 2606](https://www.rfc-editor.org/rfc/rfc2606) reserves so that it can never be registered by anybody. The domain is
deliberately non-commercial, and no repository carries an organisation prefix because there is no organisation to prefix
with.

**Delete these records; do not adapt them.** Each seeded type page says so at the top. They are chosen to exercise the
schema's awkward corners — a monorepo shipping three deployables, a CDN whose `repo` cannot answer where its content
comes from, a service coupled to the whole estate with no dependency edges — not to resemble your estate.

**What the corpus claims about its organisation is illustrative in the same way.** Showing how framework alignment works
requires something to align with, so this corpus takes a position: ISO/IEC 27001 registered, obliged by the UK GDPR,
running on Azure, part of a management system whose other halves belong to facilities, HR and IT. That posture and the
estate fit each other — a public library is a public-sector body, so the accessibility obligations the policies cite
genuinely bind it — but none of it describes anyone real. It is scaffolding, so that [`frameworks.md`](frameworks.md)
and the policies' `Alignment` columns do something instead of sitting empty. Rewrite it on the way through.

## Maturity

**Early.** The mechanism is real and tested. `kac` validates the schema, frontmatter, identity, structure, clauses,
links, the graph and the type setup, and generates the indexes and reference tables from the same pass. Three test
layers stand behind it — unit tests, Reqnroll feature specs and golden fixtures — and
[Getting started](#getting-started) has the commands. The taxonomy is the half that is only partly proven.

Every document here describes what exists today, and the
[issue tracker](https://github.com/paul80nd/knowledge-as-code/issues) holds everything agreed and unbuilt.
[Write what exists](knowledge-as-code/authoring.md#write-what-exists) is the rule, and says why.

**Proven types: ADRs, policies and services.** The remaining schemas are written but have never met real content. That
is the honest limit — the schema will be wrong in ways only real content reveals. Policies alone forced the mnemonic id
style, a category field, the identity line, the clause table and the checks that hold it. Services were proven
elsewhere: a consumer repository built a full catalogue against this type and returned a run of findings, tracked as
issues here. Treat the rest as drafts.

## Getting started

Requires the **.NET 10 SDK**. `kac` runs via `dotnet run` — no build step to manage.

```bash
git clone https://github.com/paul80nd/knowledge-as-code.git my-wiki
cd my-wiki

./kac validate                     # validate the corpus
./kac index                        # regenerate indexes and generated blocks
./kac checks                       # list the checks
dotnet run .tooling/kac-tests.cs   # run the golden test suite
```

`./kac` (Windows: `kac.cmd`) is a launcher at the repo root wrapping `dotnet run .tooling/kac.cs`; add the repo root to
your `PATH` to run it as `kac`.

**To start your own corpus:** clone, drop the types you do not want, **delete the example records in the ones you
keep**, rewrite the root pages' examples in your own domain, and start adding records. Keep `.tooling/` and `.schema/`
as they are — that is the half you want to receive updates to.

**Dropping a type means deleting both `<type>.md` and `<type>/`**, because a type is stood up as both or as neither and
`./kac validate` says so. You may leave the schema file in place: `.schema/` declares what the tool *manages*, not what
this corpus has built, so a type you have not stood up yet is a valid, silent state. That is what makes it possible to
take the whole schema and grow into it one type at a time.

**Say which types you kept** in `types:` in `.corpus.yaml`. Until you do, the tool reads your folders, and it cannot
tell a type you did not want from one you have not finished adding. Once you do, it holds you to the list, and every
generated page is written from it.

The example records are every `<type>/*.md` that is not `_index.md` or `_template.md`. `./kac validate` covers them, so
they are held to the same standard as real content and a schema change that breaks them fails CI here rather than in
your repository.

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
kac, kac.cmd           # launchers that wrap `dotnet run .tooling/kac.cs`
.corpus.yaml           # what this corpus is, and where it takes the framework from
.claude/skills/        # agent skills, shared with every corpus
.schema/               # the machine-readable schema — the source of truth
.tooling/              # the kac tool: entrypoint + kac.core library, the manifest, its tests and fixtures
```

The mechanism is dot-prefixed — `.schema/`, `.tooling/`, `.corpus.yaml` — so the markdown stays the visible half of the
repository, and an Azure DevOps wiki published from this tree shows knowledge rather than machinery.
`knowledge-as-code/` holds documentation and nothing else: what the tool reads lives beside the tool.

Adding a knowledge type is adding a YAML file to `.schema/` and a line to `.corpus.yaml`, not editing the tool.

## Keeping copies in step

This framework is **copied, not depended on**. An organisation adopting it gets its own cut, free to diverge, with no
runtime dependency on this repository and nothing to remove if they later want to go their own way.

Copies drift, and the manifest is how we see it happening. Every file resolves to exactly one layer — `synced`,
`verification`, `forked`, `generated`, `local` or `ignored` — and each layer has a rule about what divergence means.
`.corpus.yaml` records which version of the shared layer a corpus is on, any deviation it has deliberately accepted,
which of the framework's types it has adopted, and whether it is answerable for the mechanism or only runs it.

A corpus that declares its types states a decision it made, rather than the shape it happens to have. `validate` then
holds it to standing up everything it declared. Every generated list of types is written from that declaration. A schema
file for a type the corpus left out no longer reads as something missing.

`role:` asks what a corpus took of the mechanism, as `types:` asks what it took of the knowledge. A consumer holds the
tool but not the tests and fixtures that prove it, because the tool arrived proven and a fixture tree nobody runs is
noise between a reader and the code they came for.

`kac mechanism --check` reports how far a copy has drifted. `kac mechanism --sync` brings the shared layers down from
upstream, seeds the pages a newly adopted type needs, records what it took, and regenerates. Adopting a type is a line
in the descriptor and a sync.

A descriptor that says nothing still works: the tool reads adoption off the folders, and expects every shared file to be
there. Every corpus starts that way.

## Opinions

Stated openly, because they are load-bearing:

* **Azure DevOps wiki is the primary publishing target.** Frontmatter renders as a metadata table there, `.order` drives
  navigation, and `/`-rooted links resolve from the repo root. Everything degrades to plain markdown elsewhere, but the
  sharp edges were filed against ADO.
* **The corpus assumes Azure, which is a separate assumption from the one above.** The publishing target shaped the
  mechanism; the cloud shapes only the demonstration content — the Azure Well-Architected entry in
  [`frameworks.md`](frameworks.md) and the pillars the policies cite from it. AWS and Google publish near-identical
  pillars, so changing it is a relabelling rather than a re-mapping. No clause's wording names a provider, only the
  references beside it.
* **Seventeen types is a lot.** It is the most likely thing to be wrong here, and the mitigation is a decision table
  plus a standing willingness to merge types that are not earning their place.
* **Trust matters more than coverage.** The failure mode of a wiki is not too little content, it is content nobody
  believes. Generated indexes, validated links and immutable decisions all serve that.

## Provenance

Developed against a real engineering wiki and extracted once the mechanism was separable from the content. History
starts fresh here by design — the original commits are interleaved with a client's decisions and belong with them.

## Licence

Released under the [MIT licence](LICENSE), so that any organisation adopting this keeps an unencumbered copy.
