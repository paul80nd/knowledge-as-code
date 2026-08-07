# knowledge-as-code

A structured, validated engineering wiki that both people and AI sessions can read from and contribute to.

Plain markdown in git, reviewed by PR, published as a wiki. What makes it more than a folder of documents is that
**every document has a type, and every type has a schema** — so an index can be generated rather than maintained, a
broken cross-reference fails CI rather than rotting quietly, and an agent can be told where a thing goes instead of
guessing.

## What is here

Two halves, and the split is the point.

**The mechanism** — `kac`, a validator and generator, plus the machine-readable schema it enforces. Generic. Portable to
any organisation. Shared byte-for-byte between every corpus running this framework.

**The seeds** — a demonstration corpus: seventeen knowledge types, each with a root page explaining what it is and when
to use it, and a template to copy. Opinionated, and meant to be forked: you take them once, then localise the examples
to your own domain and never reconcile them again.

Which files fall on which side is declared in
[`knowledge-as-code/manifest.yaml`](knowledge-as-code/manifest.yaml), not asserted in prose.

### The seeds come in two kinds, and the difference matters

**Some seeds are close to real.** The twenty-one **policies** are the clearest case, and the part worth reading on their
own terms. The clause model, the mnemonic ids, the per-clause alignment and the gap analysis that closed it were worked
out on them rather than assumed. They are principle-level and stack-agnostic by design, so they name no service and
invent no domain — which is exactly why they would survive adoption with only the specifics rewritten.

**Other seeds need somewhere to stand.** A service catalogue demonstrates nothing without an estate; an NFR has to apply
to something; a postmortem needs an incident. For those, this corpus uses **one fictional organisation throughout —
Example Libraries, a public-library consortium** — so that the records form a graph instead of a list. Nine of them are
in [`services/`](services.md) today, and the same estate will carry the example records for the other types as they are
seeded.

It cannot be mistaken for anyone real, by construction: every hostname is under `example.com`, which
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
genuinely bind it — but none of it describes anyone real. It is scaffolding, so that
[`frameworks.md`](frameworks.md) and the policies' `Alignment` columns do something instead of sitting empty — rewrite
it on the way through.

## Status

**Early.** The mechanism is real and tested; the taxonomy is only partly proven.

|                |                                                                                                              |
|----------------|--------------------------------------------------------------------------------------------------------------|
| `kac validate` | 43 checks — frontmatter against schema, identity, structure, clauses, links, graph reciprocity, type setup   |
| `kac index`    | Generates `<type>/INDEX.md` and the schema/checks tables inside each type root page                          |
| `kac checks`   | Lists every check the validator implements                                                                   |
| Tests          | Three layers — unit (`kac.tests`), Reqnroll feature specs (`kac.features`), golden fixtures (`kac-tests.cs`) |
| Proven types   | **ADRs, policies and services.** The other fourteen schemas are written but have never met real content      |

That last row is the honest limit. The schema will be wrong in ways only real content reveals, and three types have met
it so far — policies alone forced the mnemonic id style, a category field, the identity line, the clause table and the
seven checks that hold it. Services were proven elsewhere: a consumer repository built a full catalogue against this
type and sent back sixteen findings, which are tracked as issues here. Treat the other fourteen as drafts.

## Provenance

Developed against a real engineering wiki and extracted once the mechanism was separable from the content. History
starts fresh here by design — the original commits are interleaved with a client's decisions and belong with them.

---

_Everything above orients you to this framework and where it came from. Everything below is the operational README a
corpus carries — generic, and the part a derived copy keeps and adapts as its own._

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

`./kac` (Windows: `kac.cmd`) is a launcher at the repo root that wraps `dotnet run .tooling/kac.cs`; add the repo root
to your `PATH` to run it as `kac`.

To start your own corpus: clone, drop the types you don't want, **delete the example records in the ones you keep**,
rewrite the root pages' examples in your own domain, and start adding records. Keep `.tooling/` as it is — that is the
half you want to receive updates to.

**Dropping a type means deleting both `<type>.md` and `<type>/`**, because a type is stood up as both or as neither and
`./kac validate` says so. You may leave the schema file in place: `.schema/` declares what the tool *manages*, not what
this corpus has built, so a type you have not stood up yet is a valid, silent state. That is what makes it possible to
take the whole schema and grow into it one type at a time.

The example records are every `<type>/*.md` that is not `INDEX.md` or `template.md`. `./kac validate` covers them, so
they are held to the same standard as real content and a schema change that breaks them fails CI here rather than in
your repository.

## Layout

```
<type>.md              # what the type is, why it exists, how to contribute — one per type
<type>/
  ├── INDEX.md         # GENERATED from frontmatter
  ├── template.md      # what humans and agents copy
  └── <records>.md

knowledge-as-code/     # the system's own documentation
  ├── taxonomy.md      # the seventeen types and where things go
  ├── metadata.md      # the frontmatter fields
  ├── contributing.md  # how a contribution is made and reviewed
  ├── automation.md    # what is generated, validated and scheduled
  └── manifest.yaml    # which files are shared, which are local
kac, kac.cmd           # launchers that wrap `dotnet run .tooling/kac.cs`
.mechanism.lock        # this corpus's sync state
.schema/               # the machine-readable schema — the source of truth
.tooling/              # the kac tool: entrypoint + kac.core library, plus its tests and fixtures
```

The mechanism is dot-prefixed — `.schema/`, `.tooling/`, `.mechanism.lock` — so that the markdown stays the visible half
of the repository, and so an Azure DevOps wiki published from this tree shows knowledge rather than machinery. What
remains in `knowledge-as-code/` is documentation — bar `manifest.yaml`, which stays because the README and
`automation.md` both cite it as the authority on the shared/local split.

Adding a knowledge type is adding a YAML file to `.schema/`, not editing the tool.

## Keeping copies in step

This framework is designed to be **copied, not depended on**. An organisation adopting it gets its own cut, free to
diverge, with no runtime dependency on this repository and nothing to remove if they later want to go their own way.

The cost of that is drift, which is what the manifest is for. Every file resolves to exactly one layer — `synced`,
`forked`, `generated`, `local` or `ignored` — and each layer has a rule about what divergence means. `.mechanism.lock`
records which version of the shared layer a given corpus is on, and any deviation it has deliberately accepted.

## Opinions

Stated openly, because they are load-bearing:

* **Azure DevOps wiki is the primary publishing target.** Frontmatter renders as a metadata table there, `.order`
  drives navigation, and `/`-rooted links resolve from the repo root. Everything degrades to plain markdown elsewhere,
  but the sharp edges were filed against ADO.
* **The corpus assumes Azure, which is a separate assumption from the one above.** The publishing target shaped the
  mechanism; the cloud shapes only the demonstration content — the Azure Well-Architected entry in
  [`frameworks.md`](frameworks.md) and the pillars four policies cite from it. AWS and Google publish near-identical
  pillars, so changing it is a relabelling rather than a re-mapping — no clause's wording names a provider, only the
  references beside it.
* **Seventeen types is a lot.** It is the most likely thing to be wrong here, and the mitigation is a decision table
  plus a standing willingness to merge types that aren't earning their place.
* **Trust matters more than coverage.** The failure mode of a wiki is not too little content, it is content nobody
  believes. Generated indexes, validated links and immutable decisions all serve that.

## Licence

Released under the [MIT licence](LICENSE), so that any organisation adopting this keeps an unencumbered copy.
