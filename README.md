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

**The seeds** — seventeen knowledge types, each with a root page explaining what it is and when to use it, and a
template to copy. Opinionated, and meant to be forked: you take them once, then localise the examples to your own domain
and never reconcile them again.

Which files fall on which side is declared in
[`knowledge-as-code/manifest.yaml`](knowledge-as-code/manifest.yaml), not asserted in prose.

## Status

**Early.** The mechanism is real and tested; the taxonomy is only partly proven.

|                |                                                                                                              |
|----------------|--------------------------------------------------------------------------------------------------------------|
| `kac validate` | ~30 checks — frontmatter against schema, identity, structure, link resolution, graph reciprocity             |
| `kac index`    | Generates `<type>/INDEX.md` and the schema/checks tables inside each type root page                          |
| `kac checks`   | Lists every check the validator implements                                                                   |
| Tests          | Three layers — unit (`kac.tests`), Reqnroll feature specs (`kac.features`), golden fixtures (`kac-tests.cs`) |
| Proven types   | **ADRs only.** The other sixteen schemas are written but have never validated a real document                |

That last row is the honest limit. The schema will be wrong in ways only real content reveals, and only one type has met
real content so far. Treat the other sixteen as drafts.

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

To start your own corpus: clone, delete the type folders you don't want, rewrite the root pages' examples in your own
domain, and start adding records. Keep `.tooling/` and `.schema/` as they are — those are the half you want to receive
updates to.

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
  ├── manifest.yaml    # which files are shared, which are local
  └── mechanism.lock   # this corpus's sync state
kac, kac.cmd           # launchers that wrap `dotnet run .tooling/kac.cs`
.schema/               # the machine-readable schema — the source of truth
.tooling/              # the kac tool: entrypoint + kac.core library, plus its tests and fixtures
```

The mechanism is dot-prefixed — `.schema/` and `.tooling/` — so that the markdown stays the visible half of the
repository, and so an Azure DevOps wiki published from this tree shows knowledge rather than machinery.

Adding a knowledge type is adding a YAML file to `.schema/`, not editing the tool.

## Keeping copies in step

This framework is designed to be **copied, not depended on**. An organisation adopting it gets its own cut, free to
diverge, with no runtime dependency on this repository and nothing to remove if they later want to go their own way.

The cost of that is drift, which is what the manifest is for. Every file resolves to exactly one layer — `synced`,
`forked`, `generated`, `local` or `ignored` — and each layer has a rule about what divergence means. `mechanism.lock`
records which version of the shared layer a given corpus is on, and any deviation it has deliberately accepted.

## Opinions

Stated openly, because they are load-bearing:

* **Azure DevOps wiki is the primary publishing target.** Frontmatter renders as a metadata table there, `.order`
  drives navigation, and `/`-rooted links resolve from the repo root. Everything degrades to plain markdown elsewhere,
  but the sharp edges were filed against ADO.
* **Seventeen types is a lot.** It is the most likely thing to be wrong here, and the mitigation is a decision table
  plus a standing willingness to merge types that aren't earning their place.
* **Trust matters more than coverage.** The failure mode of a wiki is not too little content, it is content nobody
  believes. Generated indexes, validated links and immutable decisions all serve that.

## Licence

Released under the [MIT licence](LICENSE), so that any organisation adopting this keeps an unencumbered copy.
