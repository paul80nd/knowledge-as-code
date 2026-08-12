# Automation

What CI checks, what it builds, and what it deliberately leaves alone.

`kac checks` lists every check the validator implements, each with its severity and what it asks. That command is the
authority on what runs. This page groups the same checks so a reader can see what the pipeline is for, and covers the
generation and the exclusions that sit beside them.

Every one of those checks reads the corpus. None reads the estate the corpus describes, so a service deleted last month
still validates cleanly, and a descriptive record is only as true as the person who last read it.

## Validation

Run on every PR. Failures block merge.

### Schema

- Frontmatter parses as YAML.
- Required fields present for the document's type (type inferred from folder).
- Enum values valid.
- Dates are quoted strings in `YYYY-MM-DD` form.
- `id` is unique across the wiki, matches the type's prefix, and matches the folder it sits in.
- `tier` matches the tier defined for the document's type. A document claiming a tier its folder does not have is a
  placement error, not a metadata error.

### Links and the graph

- Every `id` referenced in a cross-reference field resolves to a document that exists.
- Relative markdown links resolve.
- Bidirectional pairs agree — `supersedes` / `superseded-by`, `promoted-from` / `promoted-to`,
  `verifies` / `verified-by`. A one-sided link fails. `implements` is deliberately not one of these: it points up from a
  standard to a policy and is never answered from the policy side.

### Per-tier rules

- **Normative** — every standard cites an ADR in `derived-from` or a policy in `implements`.
- **Procedural** — `last-rehearsed` present, `"never"` permitted.
- **Observed** — `expires` present; `provenance` present when `source: dreamed`.

### Hygiene

- No credentials, and no data that reads as real. Declared per type, where the risk lives: integrations and data
  documents today.
- Generated regions are not stale relative to their source.

### Rules the schema declares and nothing runs

A type may declare a rule with a description and no severity. `kac validate` skips it and the type page renders it
beneath the checks table under *Declared, not yet enforced*, so the gap is reported on the page a reader is already on.
Prose about such a rule states what the schema declares, never what CI does.

## Generation

Generated content lives inside marker blocks in otherwise hand-written files:

```markdown
<!-- BEGIN GENERATED: adrs-index -->
...built content...
<!-- END GENERATED: adrs-index -->
```

CI rewrites only what's between the markers and fails the build if a block is stale. This keeps one file per purpose —
humans keep their prose, the machine keeps the tables current, and nobody has to choose.

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

Most of those blocks describe the corpus rather than the schema. Everything the taxonomy holds — the decision table, the
types at length, the disambiguations, the graph and the edges beneath it — along with the lineage table, the strip on
`metadata.md` and the index at the repository root, covers the types **this** corpus holds. A corpus that adopted five
of the framework's types gets five rows, and each one links to a page it holds.

The corpus decides which five, and records that in `types:` in `.corpus.yaml`. A corpus that has not declared is read
off its folders instead: a type counts where both halves are there, the page and the folder. That answer is the weaker
one, because it cannot tell a type nobody wanted from one somebody has not finished adding.

The same five bound generation itself. A type the corpus declined gets no index and no reference tables, whatever
`.schema/` still says about it, so nothing is written that the lists above would not name. A page or folder left behind
from a type the corpus once held is left where it is, and `validate` says it should not be there.

Blocks that differ between corpora are still safe to share. The mechanism check compares the authored half of a page and
ignores what lies between the markers, so the prose stays byte-identical everywhere while what sits beneath it does not.

The graph is written to the subset of Mermaid an Azure DevOps wiki renders. That subset is narrower than Mermaid's own,
and a diagram exceeding it renders nothing at all, with no error to say why: `graph` rather than `flowchart`, no
subgraphs, and no arrow longer than `-->`. A fenced block carries it rather than ADO's `:::` container, which GitHub
shows as literal text.

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

The markers are a question of their own, and are asked of the file as it stands. A block that has lost one stops being
written, and `index --check` reads the file as fresh, so a document holding a generated block is held to still having
the markers to write between however it is otherwise excluded.

**A template is excluded as a record and checked as a template.** It holds no id, claims no place in an index and
answers to nothing that needs a filename, so discovering it as a record would report a dozen faults that are the file
doing its job. What it is held to is everything a copy of it inherits: the fields the type declares, the values that are
not placeholders, the identity line, the required sections, and the links that point at real documents. A defect there
becomes every document's problem, and the next author is the one who finds it.

## Skills

The agent-facing machinery. None of it runs in CI, and it belongs to the same system.

[`kb-review`](../.claude/skills/kb-review/SKILL.md) reads a record against the tier rules in
[authoring](authoring.md) and the sentence rules in [style](style.md), and proposes rewrites. Somebody asks for it, so
what it returns is a reading. Everything that blocks a merge is above.

It is a skill rather than a per-folder `CLAUDE.md` for a specific reason: a subdirectory `CLAUDE.md` loads only when a
session reads a file in that directory. A session working in a service repository would never trigger one in the
corpus's `standards/` folder. Skills are selected by description matching and work across repositories, which is what
reviewing a record needs.

## Portability

Everything in this document describes **mechanism**, not corpus content. The validators, generators, schema and skills
are deliberately free of organisation specifics so they can be lifted to another organisation as a unit.

Which files that covers is not a matter of judgement. [`manifest.yaml`](../.tooling/manifest.yaml) declares it and
`kac mechanism --check` enforces it: a file in the `synced` layer carrying corpus-specific content is a defect, not a
customisation. Anything organisation-specific belongs in the corpus, or in the `forked` layer — the type root pages,
templates and publishing config — which exist to be filled with local content.

What a corpus takes is its own decision, recorded in `.corpus.yaml` rather than inferred from what it happens to hold.
`types:` names the knowledge types it has adopted. `role:` says whether it answers for the mechanism or only runs it,
which settles whether it carries the `verification` layer — the tests and fixtures that prove the tool.

`kac mechanism --sync` reads both keys, brings down what the descriptor asked for, seeds the forked files a new type
needs, records what it took, and regenerates. So a corpus adopts a type by adding a line to the descriptor and syncing.
It declines one by leaving the line out, not by deleting files afterwards.
