# `dog-fooding` ... a tiered example corpus defined against the KaC framework itself

> **Everything here is real.** Unlike the other example corpora this one describes the repository it sits in: the tool,
> the docs site, the marketplace branch, and the rules a change to any of them answers to. It's here to test the
> framework against itself as per 'eating your own dogfood'.

```text
                                              YOU ARE HERE
                                                   ▼
┌───────────────┐      ┌─────────────┐      ┌─────────────┐
│ kac framework │ ━━━► │ engineering │ ━┳━► │ dog-fooding │
└───────────────┘      └─────────────┘  ┃   └─────────────┘
        ╎                       ╎       ┃   ┌─────────────┐
   The framework defines how    ╎       ┗━► │ payments    │
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

| Type                       | Tier        | What it holds                                                                                                   |
|----------------------------|-------------|-----------------------------------------------------------------------------------------------------------------|
| [Control](controls.md)     | normative   | How a standard's rules are verified: the mechanism, the frequency, and the evidence it leaves.                  |
| [Deviation](deviations.md) | normative   | A knowing departure from a rule, the person who accepted the risk, and the date it is reviewed.                 |
| [Process](processes.md)    | procedural  | A planned procedure followed deliberately (releasing, onboarding, provisioning, rotating a secret).             |
| [Report](reports.md)       | descriptive | A question about the corpus answered over the whole of it, with the judgement a person added.                   |
| [Runbook](runbooks.md)     | procedural  | An incident-time procedure read under pressure: terse, imperative, structured as a decision tree.               |
| [Service](services.md)     | descriptive | One deployable component: purpose, repo, platform, environments, dependencies, data stores, owner.              |
| [Standard](standards.md)   | normative   | The rulebook, imperative, RFC 2119, with concrete examples and a conformance checklist.                         |
| [Tool](tools.md)           | descriptive | The approved-software register. What is chosen, rejected or deprecated, and the version ranges we stand behind. |

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

While you are changing the tool, run `dotnet run --project ../../tooling/kac -- validate` instead. That reaches the
working tree, and an installed `kac` does not.

## Reading this corpus as a plugin while you change it

The plugin a session here already carries is served from the `marketplace` branch, which holds what is on `main`. A
branch editing this corpus leaves that copy behind, so `standards-lookup` answers with a record you have already
rewritten. Build the plugin here and install it as a marketplace of your own, and a session reads the working tree
instead. `export` reads the `.imports/` that `restore` fetches, so a clean clone runs the block above first.

```bash
kac export       # write the records this branch holds
kac bundle       # assemble them and ../../template/.plugin/ into .dist/plugin/
claude plugin marketplace add ./.dist
claude plugin install example-dogfooding@example-dogfooding
```

`bundle` writes `.dist/` as a marketplace offering one plugin, and names the marketplace and the plugin alike after
the corpus. `claude plugin marketplace add` resolves whatever path you type and stores the absolute one in your own
user settings. That absolute path is a fact about your machine, so the install stays yours and no commit carries it.

Leaving the branch copy enabled as well gives you two of every skill. Turn it off in the settings file git ignores:

```bash
claude plugin disable example-dogfooding@knowledge-as-code --scope local
```

Then restart the session. Claude Code reads a skill when a session starts, so nothing installed part way through one is
reachable until the next.

### Rebuilding after a change

| You changed                                                               | Run                             |
|---------------------------------------------------------------------------|---------------------------------|
| a record here, or the `.corpus.yaml` describing this corpus               | `kac export`, then `kac bundle` |
| a skill or a hook in [`../../template/.plugin/`](../../template/.plugin/) | `kac bundle`                    |

`bundle` reads what `export` last wrote, so a skill change needs no second export. Reinstall either way:

```bash
claude plugin uninstall example-dogfooding@example-dogfooding
claude plugin install example-dogfooding@example-dogfooding
```

**Uninstall before you install.** The install caches the plugin under the `content-version` it carries, and neither
`claude plugin update` nor `claude plugin marketplace update` looks past that number. A rebuild leaving
`content-version` where it was refreshes nothing and reports success. Restart the session again to pick the new copy
up.

## What it demonstrates

**A corpus about the repository holding it.** The corpora beside it prove that the framework holds for an invented
estate, which is a weaker claim than it sounds: nothing pushes back when the fiction is convenient. Here the estate
answers back. A service record naming the wrong workflow is caught by whoever next reads the workflow, and a standard
nobody follows is visible as a standard nobody follows.

**A graph the tool can check.** Every type this corpus adopted holds records, and the edges between them are
frontmatter rather than prose. `implements:` on a standard names a clause in `../engineering/`, `verifies:` on a
control names the standard, and `applies-to:` on a runbook names the service it covers. `kac validate` fails the day
one of those stops resolving.

## What this corpus declares about itself

[`.corpus.yaml`](.corpus.yaml) says what this corpus is, which of the framework's types it has adopted, where its
published form is served from, and any deviation from the shared baseline it has deliberately accepted.

It declines `adrs`, because a decision about `kac` is recorded in `tooling/CLAUDE.md` and in the commit that made it,
and moving those here is a separate call. It declines `glossary`, because the framework's own vocabulary belongs to
`../engineering/` and this corpus cites it as `eng:`.

## How this corpus stands against `eng:`

[`../engineering/`](../engineering/) states 22 policies carrying 207 clauses, and [rpt-clause-coverage] carries a
verdict for every one of them. `kac report coverage` fills the mechanical half and a person writes the rest, so the
answer is regenerated rather than kept by hand.

**Every gap is deviated.** [deviations](deviations.md) holds a record for each departure that report found, with an
owner and a date somebody comes back to it. One record holds every clause the same departure reaches, so the register
is shorter than the list of gaps.

[rpt-clause-coverage]: reports/clause-coverage.md

