# Automation

> What CI checks, what it builds, and what it leaves alone.

`kac checks` lists every check the validator implements, with its severity and what it asks. That command is the
authority on what runs. This page groups the same checks so you can see what the pipeline is for, and it covers the
generation and the exclusions that sit beside them.

Every one of those checks reads the corpus. None reads the estate the corpus describes. A service deleted last month
still validates cleanly, and a descriptive record is only as true as the person who last read it.

## Validation

Run on every PR. Failures block merge.

### Schema

- Frontmatter parses as YAML.
- Required fields present for the document's type, which the folder gives.
- Enum values valid.
- Dates are quoted strings in `YYYY-MM-DD` form.
- `id` is unique across the corpus, matches the type's prefix, and matches the folder it sits in.
- `tier` matches the tier the document's type declares. A document claiming a tier its folder does not have is a
  placement error, not a metadata error.

### Links and the graph

- Every `id` referenced in a cross-reference field resolves to a document that exists.
- Relative markdown links resolve.
- Bidirectional pairs agree: `supersedes` / `superseded-by`, `promoted-from` / `promoted-to`,
  `verifies` / `verified-by`. A one-sided link fails. `implements` is deliberately not one of these. It points up from a
  standard to a policy, and the policy never points back.

### Per-tier rules

- **Normative.** Every standard cites an ADR in `derived-from` or a policy in `implements`.
- **Procedural.** `last-rehearsed` is present, and `"never"` is permitted.
- **Observed.** `expires` is present, and `provenance` is present when `source: dreamed`.

### Hygiene

- No credentials, and no data that reads as real. Each type declares this rule where the risk lives, which today means
  integrations and data documents.
- A generated region matches the source it was built from.

### Rules the schema declares and nothing runs

A type may declare a rule with a description and no severity. `kac validate` skips it. The type page renders it beneath
the checks table under *Declared, not yet enforced*, so a reader meets the gap on the page they are already on. Prose
about such a rule says what the schema declares, never what CI does.

## Generation

Generated content lives inside marker blocks in otherwise hand-written files:

```markdown
<!-- BEGIN GENERATED: adrs-index -->
...built content...
<!-- END GENERATED: adrs-index -->
```

CI rewrites only what sits between the markers, and fails the build if a block is stale. So one file serves one purpose:
you keep your prose, and the generator keeps the tables current.

| Artefact                         | Built from                     | Lives in                                    |
|----------------------------------|--------------------------------|---------------------------------------------|
| Type indexes                     | Frontmatter across the folder  | `<type>/_index.md`                          |
| Per-type frontmatter reference   | `.schema/`                     | `<type>.md` `schema-*` block                |
| Universal frontmatter reference  | `.schema/_universal.yaml`      | `metadata.md` `schema-universal` block      |
| The way on to each type's fields | `.schema/` + the types adopted | `metadata.md` `types-metadata` block        |
| Where a document goes            | `.schema/` + the types adopted | `taxonomy.md` `types-placement` block       |
| The types at length, by tier     | `.schema/` + `_tiers.yaml`     | `taxonomy.md` `types-detail` block          |
| The calls that are close         | `.schema/` + the types adopted | `taxonomy.md` `types-versus` block          |
| How the types relate             | `ref:` across the schema       | `taxonomy.md` `types-graph` block (mermaid) |
| The same edges, field by field   | `ref:` across the schema       | `taxonomy.md` `types-edges` block           |
| Where the names came from        | `.schema/` + the types adopted | `lineage.md` `types-lineage` block          |
| Where a name collides            | `.schema/` + the types adopted | `lineage.md` `types-collisions` block       |
| What this corpus holds           | `.schema/` + the types adopted | Root `README.md` `types-index` block        |

The table above says what each block is and where it lands. The repository the tool is built from carries the same list
from the generator's side, in `tooling/features/generate.md`, giving the rule that governs each block in place of its
address.

Most of those blocks describe the corpus. Everything on the taxonomy page covers the types *this* corpus holds: the
decision table, the types at length, the disambiguations, and the graph with the edges beneath it. So do the lineage
table, the strip on `metadata.md` and the index at the repository root. A corpus that adopted five of the framework's
types gets five rows, and each row links to a page it holds.

The corpus chooses which types it holds, and records them in `types:` in `.corpus.yaml`. Where a corpus has not declared
them, `kac` reads them off the folders instead: a type counts when both halves are there, the page and the folder. That
answer is the weaker one, because it cannot tell a type nobody wanted from one somebody has not finished adding.

The same list bounds generation itself. A type the corpus declined gets no index and no reference tables, whatever
`.schema/` still says about it, so the generator writes nothing the lists above would not name. It leaves a page or
folder stranded by a type the corpus once held exactly where it is, and `kac validate` reports that it should not be
there.

Blocks that differ between corpora are still safe to share. The mechanism check compares the authored half of a page and
ignores what lies between the markers. So the prose stays byte-identical everywhere, and the generated content beneath
it varies by corpus.

The generator writes the graph to the subset of Mermaid an Azure DevOps wiki renders. That subset is narrower than
Mermaid's own, and a diagram that exceeds it renders nothing at all, with no error to say why. So write `graph` rather
than `flowchart`, use no subgraphs, and keep every arrow to `-->`. A fenced block carries the diagram. ADO's `:::`
container shows on GitHub as literal text.

## The export

Built on every PR. `kac export` writes the corpus into `.dist/export/` as data a consumer reads instead of cloning the
repository. The export holds a manifest saying what it is, one file per record, and a flat file cheap to grep.
`tooling/features/export.md` is the reference for its contents.

A change that breaks the export therefore fails its own build. Nothing is kept. `.dist/` is gitignored, the build
rewrites it whole each run, and the pipeline discards it with the job.

## Exclusions

These paths are not part of the taxonomy. They carry no taxonomy frontmatter, and `kac validate` checks none of them
against a schema.

| Path                 | Why                                                |
|----------------------|----------------------------------------------------|
| `knowledge-as-code/` | Describes the system, and is not governed by it    |
| `_plan/`             | Temporary migration scaffolding, deleted when done |
| `_reports/`          | Generated output                                   |
| `**/_template.md`    | Not a record. Checked as a template instead        |
| Root `README.md`     | Orientation page                                   |
| Root `CLAUDE.md`     | Agent guidance                                     |

We name each path rather than hide it in a glob, so nobody answers a validation failure by quietly widening an
exclusion. The `_` rows are the one deliberate glob. That prefix belongs to the framework's own artefacts, and the tool
matches on the prefix itself. See [taxonomy](taxonomy.md#layout).

Excluding a file as a record does not excuse it from every check. The framework's own documents carry no frontmatter, so
`kac validate` holds them to no schema. They still link to things, so it resolves their links and fragments like any
page's. A file holding a generated block must still carry the markers the generator writes between, however it is
otherwise excluded. `tooling/features/validate.md` lists each of those extra passes and what it asks.

A template is excluded as a record and checked as a template. It holds no id, claims no place in an index, and answers
to nothing that needs a filename. Discovering it as a record would report a run of faults that are the file doing its
job. It is held instead to everything a copy of it inherits: the fields the type declares, the values that are not
placeholders, the identity line, the required sections, and the links that point at real documents. A defect there
becomes every document's problem, and the next author is the one who finds it.

## Skills

The agent-facing machinery. None of it runs in CI, and the framework ships it alongside the checks above.

[`kb-review`](../.claude/skills/kb-review/SKILL.md) reads a record against `technical-writing` and `writing-a-record`,
then proposes rewrites. You ask for it, so what it hands back is a reading. Everything that blocks a merge is above.

We made it a skill rather than a per-folder `CLAUDE.md` for one reason. A subdirectory `CLAUDE.md` loads only when a
session reads a file in that directory, so a session working in a service repository would never trigger one in the
corpus's `standards/` folder. An agent picks a skill by matching its description, and a skill works across repositories.
Reviewing a record needs both.

## Portability

Everything on this page describes **mechanism**, not corpus content. We keep the validators, generators, schema and
skills free of organisation specifics, so you can lift them to another organisation as a unit.

Which files that covers is not a matter of judgement. `tooling/manifest.yaml` declares it and `kac mechanism --check`
enforces it. A file in the `synced` layer carrying corpus-specific content is a defect, not a customisation. Anything
organisation-specific belongs in the corpus, or in the `forked` layer. That layer holds the type root pages, the
templates and the publishing config, and each of them exists to be filled with local content.

What a corpus takes is its own decision, recorded in `.corpus.yaml`. `types:` names the knowledge types the corpus has
adopted. `role:` says whether the corpus answers for the mechanism or only runs it, which settles whether it carries the
`verification` layer, the tests and fixtures that prove the tool.

`kac mechanism --sync` reads both keys. It brings down what the descriptor asked for, seeds the forked files a new type
needs, records what it took, and regenerates. So a corpus adopts a type by adding a line to the descriptor and syncing,
and declines one by leaving the line out rather than deleting files afterwards.
