# The worked corpora

Whole corpora, each one complete in its own right, and between them every deployment shape the framework supports.
`kac` runs over all of them on every commit, so a schema change that breaks any of them fails CI here rather than in
somebody's repository.

**Read these, copy [`../template/`](../template/).** The template is the same corpus with the content taken out, and it
is what `kac new` sends. Everything here is a worked example to borrow ideas from.

| Corpus                          | Shape                                       | Bounded context               |
|---------------------------------|---------------------------------------------|-------------------------------|
| [`library/`](library/)          | A single corpus, self-contained             | A public-library consortium   |
| [`engineering/`](engineering/)  | A governance layer, publishing an export    | An engineering organisation   |
| [`payments/`](payments/)        | A domain corpus, inheriting its governance  | A payments system             |
| [`dog-fooding/`](dog-fooding/)  | A domain corpus, describing this repository | knowledge-as-code itself      |

Every one but `dog-fooding/` names no real organisation, and every hostname those hold is under `example.com`, which
[RFC 2606](https://www.rfc-editor.org/rfc/rfc2606) reserves so that it can never be registered. `dog-fooding/` takes
this repository as its estate, so anything written there is checkable against the tree around it.

[One corpus or several](https://paul80nd.github.io/knowledge-as-code/one-corpus-or-several/) is the page for choosing
between one corpus and several, and says what the split costs.

## What each one is for

**[`library/`](library/) is the whole thing in one repository.** It holds its own vocabulary and its own service
catalogue, and it takes nothing from outside. A first corpus arrives in this shape and most stay in
it. It adopts `adrs`, `capabilities`, `data`, `glossary`, `integrations`, `processes`, `runbooks` and `services`.

**[`engineering/`](engineering/) is the governance layer.** Its policies are written to be principle-level and
stack-agnostic, so they name no service and invent no estate. That is what lets them be published and read by a team
that runs something else entirely. It adopts `adrs`, `controls`, `glossary`, `policies`, `standards` and `tools`.

**[`payments/`](payments/) is a domain corpus, and it is thin on purpose.** It declares `engineering/` in `consumes:`,
so its standards cite `eng:pol-SCRT.STORE` rather than restating what that clause binds. Thin is what makes the
inheritance visible: there is nothing here that `engineering/` already says. It adopts `nfrs`, `services` and
`standards`, and declines the rest.

**[`dog-fooding/`](dog-fooding/) is the same shape and the estate is this repository.** It consumes `engineering/` as
`payments/` does, and it adopts `controls`, `runbooks`, `services`, `standards` and `tools`. It stands each of them up
and holds no record yet, which is also what a corpus looks like on the day it is created.

## How payments consumes engineering

`payments/.corpus.yaml` names `example-engineering` in `consumes:`, at a version range, and gives a `source:` of
`../engineering/.dist/package`. That folder is where `kac pack` writes, so a producer and its consumer exchange a
sealed package without a registry between them. `kac restore` unpacks it under `payments/.imports/eng/`, which is not
committed. `dog-fooding/` consumes the same corpus the same way, so everything in this section holds for it too.

`kac validate` then resolves a citation carrying the `eng:` shortcode against what arrived, and reports a clause the
governance layer does not carry exactly as it reports a broken local reference. A declared import that has not been
restored is an error naming `kac restore`.

**The producer packs before the consumer restores.** In this repository:

```sh
cd examples/engineering && kac export && kac pack
cd ../payments && kac restore && kac validate
```

A corpus consuming across repositories names a registry's service index as its `source:` instead, and
[the `restore` page](https://paul80nd.github.io/knowledge-as-code/cli/restore/) covers both forms.

## What they share

`.schema/` is authored once at this repository's root and read from there by every corpus here and by `template/`. A
corpus created anywhere else carries its own copy at its own root, which is where `kac` looks first.

Each corpus declares `types:` in its own `.corpus.yaml`. Adoption is a decision rather than the shape the folders happen
to have, and `kac validate` holds each corpus to standing up everything it declared and nothing it did not.

The framework's `discoveries`, `explanations`, `faqs` and `postmortems` are adopted by none of them. They keep
their schema, their root page and their template in [`../template/`](../template/), and no worked record exercises
them.

## What they publish

**All of them publish.** A push to `main` packs each corpus to GitHub Packages and bundles each into the
[`marketplace`](https://github.com/paul80nd/knowledge-as-code/tree/marketplace) branch, which offers them as
installable plugins. That is how this repository proves the publishing half of its own tool against a real registry,
and how somebody deciding whether to adopt the framework can install one and ask it questions.

A plugin carries the export and whatever in the plugin tree can read it, so what each one does follows from the types
that reach its export. `library/` ships the glossary lookup alone. Every other corpus here ships every lookup, because
`engineering/` adopts every type those skills read, and a corpus consuming `engineering/` carries its records into its
own export. A skill whose type reaches neither the corpus nor its imports is trimmed, and `bundle.json` inside the
plugin names every component that was dropped and the type it needed.

Each corpus says in its own `description` what it is. `kac pack` and `kac bundle` write that into the package and the
plugin from [`.corpus.yaml`](library/.corpus.yaml), so no reader meets an invented estate without being told.

**Move `content-version` when you change a corpus.** Both publishers take the version the corpus states, so one that
has not moved publishes nothing and says nothing.
