# Changing the schema

[`meta/type.schema.json`](meta/type.schema.json) is the reference for the keys and [`README.md`](README.md) is the
reasoning behind them. This is what will bite you. In a corpus that declares `role: consumer`, these files arrive from
upstream: author the change here and take it down with `kac mechanism --sync`, because a local edit is drift.

**After any change, run `./kac index` and then the golden suite.** Every type page carries generated `schema-<type>`
and `checks-<type>` blocks derived from these files, so a schema edit alone leaves the corpus stale and fails
`index --check` in CI. The fixtures validate against these files rather than against copies, so the same edit can move
golden expectations in `.tooling/tests/fixtures/`: run `dotnet run .tooling/kac-tests.cs` as well as `./kac validate`.

* **A key you invent is rejected, and `notes:` is how you say the thing anyway.** The key space is closed at every
  level: the loader records what it asks each mapping for, and anything left over fails as `schema-unknown-key`. A new
  key means an edit to `.tooling/kac.core/Schema.cs` **and** to the code that reads what it parsed into — finding it
  parsed is not enough, since a value nothing dispatches fails the same pass one step later. Declare it in
  [`meta/type.schema.json`](meta/type.schema.json) in the same edit: nothing in CI reads that file, so a key missing
  from it is one an editor marks red while the build stays green, and the mistake surfaces as distrust of the tooling
  rather than as a finding.

* **Field order is load-bearing.** `key-order` requires a document's frontmatter to be a topological extension of the
  universal order followed by the type's. Reordering fields here can invalidate documents that were correct, and the
  failure surfaces in the corpus rather than here.

* **Templates do not follow, and are held to this file.** Nothing generates `<type>/_template.md`, so a field added here
  has to be added there by hand. A **required** one is caught — `template-fields` fails when the template omits it,
  because every document copied from it would fail `required-field`. An **optional** one is not: a template is curated,
  and leaving one out is an editorial choice. A field *removed* here is caught from the other side, since the template
  would then carry a key the type does not declare.

## Writing a rule

* **Read the field declaration and the `sections:` block first — the rule may already be answered.** A `reciprocal:`, a
  `mirrors-section:`, a `required-when:`, a scalar type, a required section, a section left as a bare heading: each has
  been written out as a rule at some point, and each read as outstanding work for as long as it survived.

* **A rule you have not built declares no `severity:`.** That absence is what says "declared, not enforced", and the
  type page renders it as such. Naming a level nothing fires at fails the schema-load pass, because it reads as enforced
  everywhere a reader looks — the checks table, `kac checks`, the catalogue — and is not.

* **`required-when` is a different language and stays one.** It reads `==`, `!=` and `in [...]`, tests one field against
  one other, and lives on the field. A condition needing more than that is a rule with an `expr:`. It also produces an
  *error* at the moment its condition holds, where a rule chooses its own severity — so a fill-this-in-or-else
  obligation is `required-when`, and a should-have-done-this is a rule.

* **Thresholds are judgements**, and each is pinned by a fixture so moving one is visible.
  [`../.tooling/features/checks.md`](../.tooling/features/checks.md) says where the numbers came from. A ratio like
  `words() <= links() * 40` fails a document linking to nothing at any length, which for a capability or an explanation
  is the intended reading.

* **The text rules are heuristics** and will be tuned wrong first. Their patterns belong here rather than in C# for
  that reason, argued in [`../.tooling/features/checks.md`](../.tooling/features/checks.md).

* **A rule reporting several faults under one id needs a fixture for each**, because one fixture turns the whole id
  green.

* **A rule whose question needs C# is a class, not an `expr:`.** [`../.tooling/CLAUDE.md`](../.tooling/CLAUDE.md) holds
  the test for which, and the two interfaces to write it against.
