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

None of the three names a real organisation, and every hostname in them is under `example.com`, which
[RFC 2606](https://www.rfc-editor.org/rfc/rfc2606) reserves so that it can never be registered.

## What each one is for

**[`library/`](library/) is the whole thing in one repository.** It holds its own vocabulary, its own service catalogue
and the decisions behind them, and it takes nothing from outside. This is the ordinary case, and the shape a first
corpus arrives in. It adopts `adrs`, `capabilities`, `data`, `glossary`, `integrations`, `processes`, `runbooks` and
`services`.

**[`engineering/`](engineering/) is the governance layer.** Its policies are written to be principle-level and
stack-agnostic, so they name no service and invent no estate. That is what lets them be published and read by a team
that runs something else entirely. It adopts `adrs`, `controls`, `glossary`, `policies`, `standards` and `tools`.

**[`payments/`](payments/) is a domain corpus, and it is thin on purpose.** A domain corpus inherits its governance
rather than restating it, and thin is what makes that visible: there is nothing here that `engineering/` already says.
It adopts `nfrs`, `services` and `standards`, and holds no records in them yet.

## What they share

`.schema/` is authored once at this repository's root and read from there by all three corpora and by `template/`. A
corpus created anywhere else carries its own copy at its own root, which is where `kac` looks first.

Each corpus declares `types:` in its own `.corpus.yaml`. Adoption is a decision rather than the shape the folders happen
to have, and `kac validate` holds each corpus to standing up everything it declared and nothing it did not.

Four of the framework's seventeen types are adopted by none of the three: `discoveries`, `explanations`, `faqs` and
`postmortems`. They keep their schema, their root page and their template in [`../template/`](../template/), and no
worked record exercises them.

## What is not built yet

**`payments/` does not consume `engineering/`.** No key in `.corpus.yaml` names another corpus, and no `kac` verb reads
one corpus's export into another. The inheritance is a convention here rather than a declaration, and
[#93](https://github.com/paul80nd/knowledge-as-code/issues/93) is where it gets built.

**Only `library/` publishes a plugin.** All three build one, because `kac bundle` runs over each of them in CI, but the
[`marketplace`](https://github.com/paul80nd/knowledge-as-code/tree/marketplace) branch carries `library/` alone.
