# The worked corpora

Three corpora, each one whole in its own right, and each demonstrating a different deployment shape. `kac` runs over all
three on every commit, so a schema change that breaks any of them fails CI here rather than in somebody's repository.

**Read these, copy [`../template/`](../template/).** The template is the same corpus with the content taken out, and it
is what `kac new` sends. Everything here is a worked example to borrow ideas from.

| Corpus                          | Shape                                      | Bounded context               |
|---------------------------------|--------------------------------------------|-------------------------------|
| [`library/`](library/)          | A single corpus, self-contained            | A public-library consortium   |
| [`engineering/`](engineering/)  | A governance layer, publishing an export   | An engineering organisation   |
| [`payments/`](payments/)        | A domain corpus, inheriting its governance | A payments system             |

None of the three names a real organisation, and every hostname they hold is under `example.com`, which
[RFC 2606](https://www.rfc-editor.org/rfc/rfc2606) reserves so that it can never be registered.

## What each one is for

**[`library/`](library/) is the whole thing in one repository.** It holds its own vocabulary, its own service catalogue
and the decisions behind them, and it takes nothing from outside. A first corpus arrives in this shape and most stay in
it. It adopts `adrs`, `capabilities`, `data`, `glossary`, `integrations`, `processes`, `runbooks` and `services`.

**[`engineering/`](engineering/) is the governance layer.** Its policies are written to be principle-level and
stack-agnostic, so they name no service and invent no estate. That is what lets them be published and read by a team
that runs something else entirely. It adopts `adrs`, `controls`, `glossary`, `policies`, `standards` and `tools`.

**[`payments/`](payments/) is a domain corpus, and it is thin on purpose.** It declares `engineering/` in `consumes:`,
so its standards cite `eng:pol-SCRT.STORE` rather than restating what that clause binds. Thin is what makes the
inheritance visible: there is nothing here that `engineering/` already says. It adopts `nfrs`, `services` and
`standards`, and holds six records between them.

## How payments consumes engineering

`payments/.corpus.yaml` names `example-engineering` in `consumes:`, at a version range, and gives a `source:` of
`../engineering/.dist/package`. That folder is where `kac pack` writes, so the two corpora exchange a sealed package
without a registry between them. `kac restore` unpacks it under `payments/.imports/eng/`, which is not committed.

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

`.schema/` is authored once at this repository's root and read from there by all three corpora and by `template/`. A
corpus created anywhere else carries its own copy at its own root, which is where `kac` looks first.

Each corpus declares `types:` in its own `.corpus.yaml`. Adoption is a decision rather than the shape the folders happen
to have, and `kac validate` holds each corpus to standing up everything it declared and nothing it did not.

Four of the framework's seventeen types are adopted by none of the three: `discoveries`, `explanations`, `faqs` and
`postmortems`. They keep their schema, their root page and their template in [`../template/`](../template/), and no
worked record exercises them.

## What is not built yet

**Only `library/` publishes a plugin.** All three build one, because `kac bundle` runs over each of them in CI, but the
[`marketplace`](https://github.com/paul80nd/knowledge-as-code/tree/marketplace) branch carries `library/` alone.

**Only `engineering/` publishes a package.** It is the one corpus declaring a `shortcode`, which is the word a consumer
cites an import by and which `kac pack` refuses a corpus without. So it is the only one the gate packs, and the only one
`publish-corpus.yml` pushes to GitHub Packages.
