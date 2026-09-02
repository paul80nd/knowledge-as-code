# The example corpora

Whole corpora, each one complete in its own right, and between them every deployment shape the framework supports.
`kac` runs over all of them on every commit, so a schema change that breaks any of them fails CI.

**Read these, take inspiration from these, but always `kac new` to start your own corpus.** Everything here is a worked
example to prove the pattern rather than a starter corpus to use as is.

```text
                         ┌─────────────────┐
                     ┏━► │ library         │
┌─────────────────┐  ┃   └─────────────────┘      ┌─────────────────┐
│ kac framework   │ ━┫                        ┏━► │ dog-fooding     │
└─────────────────┘  ┃   ┌─────────────────┐  ┃   └─────────────────┘
                     ┗━► │ engineering     │ ━┫
                         └─────────────────┘  ┃   ┌─────────────────┐
                                              ┗━► │ payments        │
                                                  └─────────────────┘
```

| Corpus                                  | Shape                                       | Bounded context             |
|-----------------------------------------|---------------------------------------------|-----------------------------|
| [`library`](library/README.md)          | A single corpus, self-contained             | A public-library consortium |
| [`engineering/`](engineering/README.md) | A governance layer, publishing an export    | An engineering organisation |
| [`dog-fooding/`](dog-fooding/README.md) | A domain corpus, describing this repository | knowledge-as-code itself    |
| [`payments/`](payments/README.md)       | A domain corpus, inheriting its governance  | A payments system           |

Every example names no real organisation, and every hostname those hold is under `example.com`, which
[RFC 2606](https://www.rfc-editor.org/rfc/rfc2606) reserves so that it can never be registered. `dog-fooding/` takes
this repository as its estate, so anything written there is checkable against the tree around it.

[One corpus or several](https://paul80nd.github.io/knowledge-as-code/one-corpus-or-several/) is the page for choosing
between one corpus or tiering several, and says what the split costs.

## What each example is for

**[`library`](library/README.md) is the whole thing in one repository.** It holds its own vocabulary and its own service
catalogue, and it takes nothing from outside. A first corpus arrives in this shape and most stay in it. It adopts
`adrs`, `capabilities`, `data`, `glossary`, `integrations`, `processes`, `runbooks` and `services`.

**[`engineering`](engineering/README.md) is the governance layer.** Its policies are written to be principle-level and
stack-agnostic, so they name no service and invent no estate. That is what lets them be published and read by a team
that runs something else entirely. It adopts `adrs`, `controls`, `glossary`, `policies`, `standards` and `tools`.

**[`payments`](payments/README.md) is a domain corpus, and it is thin on purpose.** It declares `engineering` in
`consumes:`, so its standards cite `eng:pol-SCRT.STORE` rather than restating what that clause binds. Thin is what makes
the inheritance visible: there is nothing here that `engineering` already says. It adopts `nfrs`, `services` and
`standards`, and declines the rest.

**[`dog-fooding`](dog-fooding/README.md) takes the same shape and its estate is this repository.** It consumes
`engineering` as `payments` does, and it adopts `controls`, `runbooks`, `services`, `standards` and `tools`.

## What they share

`.schema/` is authored once at this repository's root and read from there by each example corpus. A corpus created
anywhere else carries its own copy at its own root, which is where `kac` looks first.

Each corpus declares `types:` in its own `.corpus.yaml`. Adoption is a decision rather than the shape the folders happen
to have, and `kac validate` holds each corpus to standing up everything it declared and nothing it did not.

## What they publish

**All of them publish.** A push to `main` packs each corpus to GitHub Packages and bundles each into the
[`marketplace`](https://github.com/paul80nd/knowledge-as-code/tree/marketplace) branch, which offers them as installable
plugins. That is how this repository proves the publishing half of its own tool against a real registry, and how
somebody deciding whether to adopt the framework can install one and ask it questions.

A plugin carries the export and whatever is in the plugin's tree can read it, so what each one does follows from the
types that reach its export. `library` ships the glossary lookup alone. Every other corpus here ships every lookup,
because `engineering` adopts every type those skills read, and a corpus consuming `engineering` carries its records
into its own export. A skill whose type reaches neither the corpus nor its imports is trimmed, and `bundle.json` inside
the plugin names every component that was dropped and the type it needed.

Each corpus says in its own `description` what it is. `kac pack` and `kac bundle` write that into the package and the
plugin from [`.corpus.yaml`](library/.corpus.yaml), so no reader meets an invented estate without being told.
