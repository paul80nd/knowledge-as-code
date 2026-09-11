# Changing the schema

[`meta/type.schema.json`](meta/type.schema.json) is the reference for the keys, and describes each one on hover.
[`README.md`](README.md) says what is in this folder and where the rest is written down. This page says what has already
cost a session here.

In a corpus created from a template, these files arrive with it, so a local edit is drift. Make the change in the
template and take it down with `kac update`. Where a line below points at a C# file, it means this same repository,
because a corpus has `.schema/` and an installed `kac` and nothing else.

**After any change, run `kac generate` and then the golden suite.** Every type page has generated `schema-<type>` and
`checks-<type>` blocks derived from these files. A schema edit alone leaves the corpus stale and fails
`generate --check` in CI. The fixtures validate against these files rather than against copies, so the same edit can
move golden expectations in `tooling/tests/fixtures/`. Run `dotnet run tooling/kac-tests.cs` as well as `kac validate`.

* **A key you invent is rejected, and `notes:` is how you say the thing anyway.** The key space is closed at every
  level. The loader records what it asked each mapping for, and anything left over fails as `schema-unknown-key`. A new
  key means an edit to `tooling/kac.core/Schema.cs` **and** to the code that reads what it parsed. Finding it parsed is
  not enough: a value nothing dispatches fails the same pass one step later. Declare it in
  [`meta/type.schema.json`](meta/type.schema.json) in the same edit. Nothing in CI reads that file, so a key missing
  from it is one an editor marks red while the build stays green.

* **Field order is load-bearing.** `key-order` requires a document's frontmatter to be a topological extension of the
  universal order followed by the type's. Reordering fields here can invalidate documents that were correct. The failure
  appears in the corpus rather than here.

* **A template does not follow a schema change.** Nothing generates `<type>/_template.md`, so you add a field there by
  hand. A **required** one is caught: `template-fields` fails when the template omits it, because every document copied
  from it would fail `required-field`. An **optional** one is not, because a template is curated and leaving one out is
  an editorial choice. A field *removed* here is caught from the other side, since the template would then have a key
  the type does not declare.

* **Prose here reaches further than you think.** A field's `description:` renders into the `schema-<type>` table on
  every adopting corpus's type page. A `summary:`, `detail:`, `versus:`, `collision:` or `lineage:` value renders into
  `knowledge-as-code/taxonomy.md` and `lineage.md`. Several `message:` values are pinned verbatim in
  `tooling/tests/fixtures/`. Reword one of these and the golden suite fails until the fixture is regenerated.

## Writing a rule

* **Read the field declaration and the `sections:` block first.** The rule may already be answered by a `reciprocal:`, a
  `mirrors-section:`, a `mirrors-citations:`, a `required-when:`, a scalar type, a required section, or a section left
  as a bare heading. Each has been written out as a rule at some point, and each read as outstanding work for as long as
  it survived.

* **A rule you have not built declares no `severity:`.** That absence is what says "declared, not enforced", and the
  type page renders it as such. Naming a level nothing fires at fails the schema-load pass, because it would read as
  enforced in the checks table, in `kac checks` and in the catalogue.

* **`required-when` is a different language and stays one.** It reads `==`, `!=` and `in [...]`, tests one field against
  one other, and lives on the field. A condition needing more than that is a rule with an `expr:`. It also produces an
  *error* at the moment its condition is true, where a rule chooses its own severity. So a fill-this-in-or-else
  obligation is `required-when`, and a should-have-done-this is a rule.

* **The grammar an `expr:` is written in is frozen.**
  <https://paul80nd.github.io/knowledge-as-code/design/expressions/> is the reference for it: what the grammar allows,
  the facts a rule may call, and the guard a field that may be absent needs. A rule that will not fit wants a new fact
  rather than a wider grammar.

* **Thresholds are judgements**, and a fixture pins each one, so moving it is visible.
  <https://paul80nd.github.io/knowledge-as-code/design/checks/> says where the numbers came from. A ratio such as
  `words() <= links() * 40` fails a document that links to nothing, at any length. For a capability or an explanation,
  that is the intended reading.

* **The text rules are heuristics** and will be tuned wrong first. Their patterns live here rather than in C# for that
  reason, argued at <https://paul80nd.github.io/knowledge-as-code/design/checks/>.

* **One fault is reported once, by whichever check owns it.** Before you add an arm to a rule, check that no existing id
  already reports the same thing.
  <https://paul80nd.github.io/knowledge-as-code/design/checks/#which-check-reports-a-fault> says how the set divides.

* **A rule reporting several faults under one id needs a fixture for each**, because one fixture turns the whole id
  green.

* **A rule whose question needs C# is a class, not an `expr:`.** `tooling/CLAUDE.md` has the test for which, and the two
  interfaces to write it against.

## Where the reasoning lives

A `notes:` states what is local to one field or one check, in one paragraph. The argument behind a type's shape lives on
the site, at <https://paul80nd.github.io/knowledge-as-code/design/shaping-a-type/>. The argument behind a check lives
at <https://paul80nd.github.io/knowledge-as-code/design/checks/>. Put a new argument on the page and cite the URL,
rather than growing a `notes:` into a design page.
