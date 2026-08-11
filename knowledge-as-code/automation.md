# Automation

What CI checks, what it builds, and what it deliberately leaves alone.

> **Part of this is built, part is intent.** Schema validation, link and graph checking, and index generation run on
> every PR today. Drift detection, the rules digest, the reports and the skills do not exist yet. Two devices mark the
> difference: a **Status** column on each table, dropped once every row in it is `Done`, and a **Planned** or
> **Aspirational** marker leading the bullets that describe checks nothing runs. For what is enforced right now,
> `kac checks` lists every check the validator implements — that command is the authority, this page is the intent.

The principle: the pipeline's job is not only to check that documents are *well-formed*, but that they still describe
**reality**. Schema validation catches typos. Drift detection catches a wiki quietly becoming fiction, which is the
failure mode that actually matters.

## Validation

Run on every PR. Failures block merge. A marked bullet is the exception: it describes a check this page intends and
nothing runs yet, and each section carries its marked bullets last.

### Schema

- Frontmatter parses as YAML.
- Required fields present for the document's type (type inferred from folder).
- Enum values valid.
- Dates are quoted strings in `YYYY-MM-DD` form.
- `id` is unique across the wiki, matches the type's prefix, and matches the folder it sits in.
- **Aspirational.** Numeric IDs have no gaps and no reuse — including against IDs retired from withdrawn documents.
- `tier` matches the tier defined for the document's type. A document claiming a tier its folder does not have is a
  placement error, not a metadata error.

### Links and the graph

- Every `id` referenced in a cross-reference field resolves to a document that exists.
- Relative markdown links resolve.
- Bidirectional pairs agree — `supersedes` / `superseded-by`, `promoted-from` / `promoted-to`,
  `verifies` / `verified-by`. A one-sided link fails. `implements` is deliberately not one of these: it points up from a
  standard to a policy and is never answered from the policy side.
- **Aspirational.** No document references one that is `superseded`, `retired` or `expired`, except where the reference
  is explicitly historical (`supersedes`, `promoted-from`, `replaces`).
- **Planned.** `.index.json` is present, parses, and is not stale relative to the frontmatter it was built from; every
  `path` in it resolves, every `id` is unique, and every id in a `related` array exists in the file. The artefact it
  reads is in the [generation table](#generation) and is not built either. Tracked in
  [knowledge-as-code#7](https://github.com/paul80nd/knowledge-as-code/issues/7).

### Per-tier rules

- **Normative** — every standard cites an ADR in `derived-from` or a policy in `implements`.
- **Descriptive** — see [drift](#drift-detection).
- **Procedural** — `last-rehearsed` present, `"never"` permitted.
- **Observed** — `expires` present; `provenance` present when `source: dreamed`.
- **Planned. Decided** — an accepted document's content has not changed. Only status transitions and frontmatter
  corrections are permitted after acceptance; substantive edits fail with a pointer to the supersession process. It
  needs the diff against the committed content, so it belongs in a git-aware step rather than in the static validator.
  Tracked in [knowledge-as-code#10](https://github.com/paul80nd/knowledge-as-code/issues/10).
- **Aspirational. Normative** — every **MUST** / **MUST NOT** rule is claimed by a control, or the standard declares the
  gap. `standards.yaml` carries it as `rules-have-controls`, declared and not enforced.
- **Aspirational. Normative** — `review-by` is present and in the future at time of merge. Only the descriptive types
  and FAQs carry the field today, so this needs the field before it needs the check.

### Hygiene

- No credentials, and no data that reads as real. Declared per type, where the risk lives: integrations and data
  documents today.
- Generated regions are not stale relative to their source.
- **Planned.** Glossary terms used consistently; a term appearing repeatedly without a glossary entry is flagged
  (warning, not a failure). Tracked in
  [knowledge-as-code#14](https://github.com/paul80nd/knowledge-as-code/issues/14).

## Drift detection

The checks that compare documents against the estate rather than against the schema. These are the reason Descriptive is
its own tier.

| Check             | Compares                                   | Flags                                                          | Status  |
|-------------------|--------------------------------------------|----------------------------------------------------------------|---------|
| Service catalogue | `services/` vs the ADO repository list     | Services documented but deleted; repos with no document        | Planned |
| Tooling register  | `tools/` vs package manifests across repos | Packages in use never approved; approved tools now unused      | Planned |
| Capabilities      | `feature-files` paths vs the repos         | Paths that don't exist; feature files claimed by no capability | Planned |
| Integrations      | `used-by` vs service dependencies          | Undocumented external calls                                    | Planned |
| NFRs              | `measured-by` vs monitoring configuration  | Targets with no corresponding alert                            | Planned |

Drift checks run on a schedule rather than per-PR — they depend on state outside this repo, and a failure is a prompt to
investigate, not a reason to block someone's documentation change.

## Generation

Generated content lives inside marker blocks in otherwise hand-written files:

```markdown
<!-- BEGIN GENERATED: adrs-index -->
...built content...
<!-- END GENERATED: adrs-index -->
```

CI rewrites only what's between the markers and fails the build if a block is stale. This keeps one file per purpose —
humans keep their prose, the machine keeps the tables current, and nobody has to choose.

| Artefact                                    | Built from                               | Lives in                                    | Status  |
|---------------------------------------------|------------------------------------------|---------------------------------------------|---------|
| Type indexes                                | Frontmatter across the folder            | `<type>/_index.md`                          | Done    |
| Repository & launchpad tables               | `services/`                              | Root `README.md`                            | Planned |
| Per-type frontmatter reference              | `.schema/`                               | `<type>.md` `schema-*` block                | Done    |
| Universal frontmatter reference             | `.schema/_universal.yaml`                | `metadata.md` `schema-universal` block      | Done    |
| The way on to each type's fields            | `.schema/` + the types stood up          | `metadata.md` `types-metadata` block        | Done    |
| Where a document goes                       | `.schema/` + the types stood up          | `taxonomy.md` `types-placement` block       | Done    |
| The types at length, by tier                | `.schema/` + `_tiers.yaml`               | `taxonomy.md` `types-detail` block          | Done    |
| The calls that are close                    | `.schema/` + the types stood up          | `taxonomy.md` `types-versus` block          | Done    |
| How the types relate                        | `ref:` across the schema                 | `taxonomy.md` `types-graph` block (mermaid) | Done    |
| The same edges, field by field              | `ref:` across the schema                 | `taxonomy.md` `types-edges` block           | Done    |
| Where the names came from                   | `.schema/` + the types stood up          | `lineage.md` `types-lineage` block          | Done    |
| What this corpus holds                      | `.schema/` + the types stood up          | Root `README.md` `types-index` block        | Done    |
| Rules digest                                | Active standards                         | Root `CLAUDE.md` `rules-digest` block       | Planned |
| Control coverage report                     | `controls/` + standards' rules           | `controls/_index.md`                        | Planned |
| Framework alignment matrix                  | Policy clause tables' `Alignment`        | `policies/_index.md`                        | Planned |
| Staleness report                            | `review-by`, `last-rehearsed`, `expires` | `_reports/staleness.md`                     | Planned |
| Orphan report                               | The link graph                           | `_reports/orphans.md`                       | Planned |
| Service dependency diagram                  | `depends-on`                             | `services/_index.md` (mermaid)              | Planned |
| `.order` files                              | Folder contents + type ordering          | Each folder                                 | Planned |
| `.index.json` — machine-readable corpus map | Frontmatter across all types             | Repo root                                   | Planned |

Eight of those blocks describe the corpus rather than the schema. Everything the taxonomy holds — the decision table,
the types at length, the disambiguations, the graph and the edges beneath it — along with the lineage table, the strip
on `metadata.md` and the index at the repository root, all cover the types **this** corpus holds. A corpus that adopted
five of the framework's types is offered five, and every row opens.

Which types those are is the corpus's own decision, recorded in `types:` in `.mechanism.lock`. A corpus that has not
declared is read off its folders instead — a type counts where both halves are there, the page and the folder — which is
the weaker answer, because it cannot tell a type nobody wanted from one somebody has not finished adding.

That the blocks differ between corpora is also why they are safe to share: the mechanism check compares the authored
half of a page and ignores what lies between the markers, so the prose stays byte-identical everywhere while what sits
beneath it does not.

The graph is written to the subset of Mermaid an Azure DevOps wiki renders, which is narrower than Mermaid's own and
fails silently where it is exceeded: `graph` rather than `flowchart`, no subgraphs, and no arrow longer than `-->`. It
uses a fenced block rather than ADO's `:::` container, which GitHub would render as literal text.

### The rules digest — a block inside root `CLAUDE.md`

Root `CLAUDE.md` is hand-written: it is the file an agent always reads, and most of what it needs there — which
repository this is, what to run before committing, the conventions nothing enforces — is not derivable from the corpus.
The digest is generated *into* it as a `rules-digest` block, the way a type page carries its schema table, so that
standing guidance and generated rules arrive together instead of competing for the same filename.

It is the one generated artefact with a hard constraint on its contents:

- **Active standards only** — not draft, not planned.
- **MUST and MUST NOT only.** SHOULD and MAY stay in the standard.
- **One line per rule, plus a link.** The digest's job is to make an agent aware the rule exists and know where to read
  it — not to contain it.
- **The glossary is included in full.** Highest value per byte in the corpus.
- **A hard line budget, enforced by CI.**

The budget is the constraint worth arguing about. A rulebook accumulates: every standard adopted adds **MUST** and
**MUST NOT** clauses and none of them expire, while the size at which an always-loaded context file stays effective does
not move. Any corpus that keeps writing standards eventually has more rules than fit. Past that size adherence degrades,
so an oversized digest is worse than a short one: it dilutes everything in it.

The budget is therefore a forcing function. When it is exceeded, the answer is not to raise it. It is to decide which
rules are genuinely always-on and which belong in an on-demand skill. That conversation is the point.

*Open question: whether the digest needs tiering — a small always-on core plus per-stack digests loaded by skill. Likely
yes, once the budget first bites.*

## Exclusions

Not part of the taxonomy, carry no taxonomy frontmatter, and are excluded from schema validation:

| Path                 | Why                                                |
|----------------------|----------------------------------------------------|
| `knowledge-as-code/` | Describes the system; is not governed by it        |
| `_plan/`             | Temporary migration scaffolding; deleted when done |
| `_reports/`          | Generated output                                   |
| `**/_template.md`    | Not a record; checked as a template — see below    |
| Root `README.md`     | Orientation page, not a knowledge record           |
| Root `CLAUDE.md`     | Agent guidance, not a knowledge record             |

Stated explicitly rather than left implicit in a glob, so that a validation failure is never resolved by quietly
widening an exclusion. The `_` rows are the one deliberate glob: the prefix is reserved for the framework's own
artefacts, and the tool tests the prefix rather than the names — see [taxonomy](taxonomy.md#layout).

**Excluded as a record is not excluded from every check.** The framework's own documents — `knowledge-as-code.md` and
those beneath it — carry no frontmatter and are validated against no schema, but they still link to things: their links
and fragments are resolved like any page's, and `framework-names-types` holds them to naming a type rather than linking
to one. Their generated blocks are emptied before either question is asked, since `index --check` already answers for
those and their links are written from this corpus.

**A template is excluded as a record and checked as a template.** It holds no id, claims no place in an index and
answers to nothing that needs a filename, so discovering it as a record would report a dozen faults that are the file
doing its job. What it is held to is everything a copy of it inherits: the fields the type declares, the values that are
not placeholders, the identity line, the required sections, and the links that point at real documents. A defect there
is not one document's problem but every document's, and it is found by the next author rather than by whoever last
edited the file.

## Scheduled tasks

Run outside CI, on a schedule.

**Staleness sweep** — documents past `review-by`, runbooks past their rehearsal frequency, discoveries past `expires`.
Expired discoveries with no promotion are closed with a note rather than deleted.

**Drift detection** — the table above.

**The dreamer** — reads local session logs, distills candidate discoveries, and proposes promotions of existing
discoveries that have accumulated corroboration.

Two rules on the dreamer, both non-negotiable:

1. **It opens a pull request. It never commits.** It proposes knowledge; a human accepts it.
2. **Every proposal carries `provenance`** — a reference to the session and passage it came from, so review is a
   thirty-second check rather than an act of faith.

The reason for both: session logs are full of things that were confidently believed and then disproved twenty minutes
later. A distillation pass that can't tell "we concluded X" from "we briefly thought X" will manufacture doctrine out of
dead ends. Provenance is what makes that detectable.

## Skills

The agent-facing machinery. Not automation in the CI sense, but part of the same system.

| Skill           | Purpose                                                                                     | Status  |
|-----------------|---------------------------------------------------------------------------------------------|---------|
| `kb-search`     | Where to look by question type; what frontmatter is grep-able; how to follow the link graph | Planned |
| `kb-contribute` | How to add a document of type X — template, frontmatter, validation                         | Planned |
| `adr`           | Draft an ADR: next number, template, supersession handling                                  | Planned |
| `note`          | Capture a discovery mid-session                                                             | Planned |
| `save` / `load` | Session handover (local storage — see [taxonomy](taxonomy.md))                              | Planned |
| `conformance`   | Run the relevant conformance checklists against a diff                                      | Planned |
| `dream`         | The distillation pass                                                                       | Planned |

These are skills rather than per-folder `CLAUDE.md` files for a specific reason: a subdirectory `CLAUDE.md` loads only
when a session reads a file in that directory. A session working in a service repository would never trigger one in the
corpus's `standards/` folder. Skills are selected by description matching and work across repositories, which is the
actual use case.

## Portability

Everything in this document describes **mechanism**, not corpus content. The validators, generators, schema and skills
are deliberately free of organisation specifics so they can be lifted to another organisation as a unit.

Which files that covers is not a matter of judgement. [`manifest.yaml`](manifest.yaml) declares it and
`kac mechanism --check` enforces it: a file in the `synced` layer carrying corpus-specific content is a defect, not a
customisation. Anything organisation-specific belongs in the corpus, or in the `forked` layer — the type root pages,
templates and publishing config — where local content is the whole point.
