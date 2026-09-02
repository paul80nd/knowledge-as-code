# `payments` ... a tiered example corpus for a payments domain

> **Everything here is invented.** Example Payments is a fictional payments system, and nothing in this corpus
> describes anyone. A record written here takes its hostnames from `example.com`, which
> [RFC 2606](https://www.rfc-editor.org/rfc/rfc2606) reserves so that it can never be registered.

```text
                                              YOU ARE HERE
                                                   ▼
┌───────────────┐      ┌─────────────┐      ┌─────────────┐
│ kac framework │ ━━━► │ engineering │ ━┳━► │ payments    │
└───────────────┘      └─────────────┘  ┃   └─────────────┘
        ╎                       ╎       ┃   ┌─────────────┐
   The framework defines how    ╎       ┗━► │ dog-fooding │
   knowledge is structured      ╎           └─────────────┘
                                ╎               ╎
        Parent corpus defines cross-cutting     ╎
        engineering expectations and policies   ╎
                                                ╎
             Downstream corpora adopt those expectations
             and extend them within a specific domain
```

> [/examples/README.md](../README.md) provides the overview of the KaC example corpora - read it first.

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

`export` writes this corpus's standards, because `standards` is the one type it adopts that declares an `export:`
block. `bundle` then assembles them into a plugin carrying `standards-lookup`, and trims the skills whose types this
corpus declined.

While you are changing the tool, run `dotnet run --project ../../tooling/kac -- validate` instead. That reaches the
working tree, and an installed `kac` does not.

## What it demonstrates

**A domain corpus consuming a governance one.** `.corpus.yaml` names `example-engineering` in `consumes:`, and
`kac restore` unpacks that corpus's published export under `.imports/eng/`. A standard here then cites
`eng:pol-SCRT.STORE`, and `kac validate` holds that citation to naming a policy and a clause that really exist. Cite a
clause the governance layer does not carry and the build fails, exactly as it would for a local id naming a record that
does not exist.

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
most here: this corpus declined more types than it adopted, and the file is where that decision is written down.

[std-TELEM]: standards/operations/payment-telemetry.md
