# Automation

What CI checks, what it builds, and what it deliberately leaves alone.

> **Part of this is built, part is intent.** Schema validation, link and graph checking, and index generation run on
> every PR today. Drift detection, the rules digest, the reports and the skills do not exist yet. The **Status** column
> on each table tracks it, and a column is dropped once every row in its table is `Done`. For what is enforced right
> now, `kac checks` lists every check the validator implements — that command is the authority, this page is the intent.

The principle: the pipeline's job is not only to check that documents are *well-formed*, but that they still describe
**reality**. Schema validation catches typos. Drift detection catches a wiki quietly becoming fiction, which is the
failure mode that actually matters.

## Validation

Run on every PR. Failures block merge.

### Schema

- Frontmatter parses as YAML.
- Required fields present for the document's type (type inferred from folder).
- Enum values valid.
- Dates are quoted strings in `YYYY-MM-DD` form.
- `id` is unique across the wiki, matches the type's prefix, and matches the folder it sits in.
- Numeric IDs have no gaps and no reuse — including against IDs retired from withdrawn documents.
- `tier` matches the tier defined for the document's type. A document claiming a tier its folder doesn't have is a
  placement error, not a metadata error.

### Links and the graph

- Every `id` referenced in a cross-reference field resolves to a document that exists.
- No document references one that is `superseded`, `retired` or `expired`, except where the reference is explicitly
  historical (`supersedes`, `promoted-from`, `replaces`).
- Relative markdown links resolve.
- Bidirectional pairs agree — `supersedes` / `superseded-by`, `promoted-from` / `promoted-to`,
  `verifies` / `verified-by`. A one-sided link fails. `implements` is deliberately not one of these: it points up
  from a standard to a policy and is never answered from the policy side.
- `.index.json` is present, parses, and is not stale relative to the frontmatter it was built from.
- Every `path` in it resolves; every `id` is unique; every id in a `related` array exists in the file.

### Per-tier rules

- **Decided** — an accepted document's content has not changed. Only status transitions and frontmatter corrections are
  permitted after acceptance; substantive edits fail with a pointer to the supersession process.
- **Normative** — every standard has at least one `derived-from`; every **MUST** / **MUST NOT** rule is claimed by a
  control, or the standard declares the gap; `review-by` is present and in the future at time of merge.
- **Descriptive** — see [drift](#drift-detection).
- **Procedural** — `last-rehearsed` present, `"never"` permitted.
- **Observed** — `expires` present; `provenance` present when `source: dreamed`.

### Hygiene

- No secrets — scan for tokens, connection strings, keys.
- Glossary terms used consistently; a term appearing more than N times without a glossary entry is flagged (warning, not
  a failure).
- Generated regions are not stale relative to their source.

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

| Artefact                                    | Built from                               | Lives in                               | Status  |
|---------------------------------------------|------------------------------------------|----------------------------------------|---------|
| Type indexes                                | Frontmatter across the folder            | `<type>/INDEX.md`                      | Done    |
| Repository & launchpad tables               | `services/`                              | Root `README.md`                       | Planned |
| Per-type frontmatter reference              | `.schema/`                               | `<type>.md` `schema-*` block           | Done    |
| Universal frontmatter reference             | `.schema/_universal.yaml`                | `metadata.md` `schema-universal` block | Done    |
| Rules digest                                | Active standards                         | Root `CLAUDE.md`                       | Planned |
| Control coverage report                     | `controls/` + standards' rules           | `controls/INDEX.md`                    | Planned |
| ISO alignment matrix                        | `policies/` `aligns-with`                | `policies/INDEX.md`                    | Planned |
| Staleness report                            | `review-by`, `last-rehearsed`, `expires` | `_reports/staleness.md`                | Planned |
| Orphan report                               | The link graph                           | `_reports/orphans.md`                  | Planned |
| Service dependency diagram                  | `depends-on`                             | `services/INDEX.md` (mermaid)          | Planned |
| `.order` files                              | Folder contents + type ordering          | Each folder                            | Planned |
| `.index.json` — machine-readable corpus map | Frontmatter across all types             | Repo root                              | Planned |

### The rules digest — root `CLAUDE.md`

The one generated artefact with a hard constraint on it.

- **Active standards only** — not draft, not planned.
- **MUST and MUST NOT only.** SHOULD and MAY stay in the standard.
- **One line per rule, plus a link.** The digest's job is to make an agent aware the rule exists and know where to read
  it — not to contain it.
- **The glossary is included in full.** Highest value per byte in the corpus.
- **A hard line budget, enforced by CI.**

That last point deserves explaining rather than just asserting. There are currently ~190 **MUST** / **MUST NOT** rules
across the standards. They will not fit inside the size where an always-loaded context file remains effective, and past
that point adherence degrades — an oversized digest is worse than a short one, because it dilutes everything in it.

So the budget is a forcing function. When it's exceeded, the answer is not to raise it; it's to decide which rules are
genuinely always-on and which belong in an on-demand skill. That conversation is the point.

*Open question: whether the digest needs tiering — a small always-on core plus per-stack digests loaded by skill. Likely
yes, once the budget first bites.*

## Exclusions

Not part of the taxonomy, carry no taxonomy frontmatter, and are excluded from schema validation:

| Path                 | Why                                                |
|----------------------|----------------------------------------------------|
| `knowledge-as-code/` | Describes the system; is not governed by it        |
| `_plan/`             | Temporary migration scaffolding; deleted when done |
| `_reports/`          | Generated output                                   |
| `**/template.md`     | Templates carry placeholder frontmatter by design  |
| Root `README.md`     | Orientation page, not a knowledge record           |
| Root `CLAUDE.md`     | Generated                                          |

Stated explicitly rather than left implicit in a glob, so that a validation failure is never resolved by quietly
widening an exclusion.

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
