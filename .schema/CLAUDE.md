# Changing the schema

[`meta/type.schema.json`](meta/type.schema.json) is the reference for the keys, and describes each one on hover.
[`README.md`](README.md) says what is in this folder and where the rest is written down. This page says what has already
cost a session here.

**After any change, run `kac generate` and then the golden suite.** Every type page has generated `schema-<type>` and
`checks-<type>` blocks derived from these files. A schema edit alone leaves the corpus stale and fails
`generate --check` in CI. The fixtures validate against these files rather than against copies, so the same edit can
move golden expectations in `tooling/tests/fixtures/`. Run `dotnet test --project tooling/kac.goldens` as well as
`kac validate`.

* **A key you invent is rejected, and `notes:` is how you say the thing anyway.** The loader reports any key it never
  asked for as `schema-unknown-key`. A new key means an edit to `tooling/kac.core/Schema.cs` **and** to the code that
  reads what it parsed, because a value nothing dispatches fails the same pass one step later. Declare it in
  [`meta/type.schema.json`](meta/type.schema.json) in the same edit. No build reads that file, so a key missing from it
  is one an editor marks red while the build stays green.

* **Reordering fields can invalidate documents that were correct.** The
  [key-order rule](../docs/reference/checks.md#the-key-order-rule) holds every document to both orders at once, and
  the failure appears in the corpora rather than here.

* **Prose here reaches further than you think.** A field's `description:` renders into the `schema-<type>` table on
  every adopting corpus's type page. A `summary:`, `detail:`, `versus:`, `collision:` or `lineage:` value renders into
  `knowledge-as-code/taxonomy.md` and `lineage.md`. Several `message:` values are pinned verbatim in
  `tooling/tests/fixtures/`. Reword one of these and the golden suite fails until the fixture is regenerated.

* **Moving a type's `export.version` makes its `plugin.json` pins stale.** A component pins the shape it reads as
  `<type>@N`, under `requires:` or `optional:`. The pins are in `template/.plugin/.claude-plugin/plugin.json` and in
  `.plugin/.claude-plugin/plugin.json` in each corpus under `examples/`. Move every pin on the type in the same change.
  Then run `dotnet test --project tooling/kac.tests` with `KAC_UPDATE_DOCS=1`, which rewrites the pins in
  `docs/use/skills.md`. The unit, feature and golden layers pass with a stale pin. Only `kac bundle` refuses one, so CI
  fails at its bundle step.

## Writing a rule

* **Read the field declaration and the `sections:` block first.** The rule may already be answered by a `reciprocal:`, a
  `mirrors-section:`, a `mirrors-citations:`, a `required-when:`, a scalar type, a required section, or a section left
  as a bare heading. Each has been written out as a rule at some point, and each read as outstanding work for as long as
  it survived.

* **`required-when` is a different language and stays one.** It reads `==`, `!=` and `in [...]`, tests one field against
  one other, and lives on the field. A condition needing more than that is a rule with an `expr:`. It also produces an
  *error* at the moment its condition is true, where a rule chooses its own severity. So a fill-this-in-or-else
  obligation is `required-when`, and a should-have-done-this is a rule.

* **A ratio such as `words() <= links() * 40` fails a document that links to nothing, at any length.** For an offering
  or an explanation, that is the intended reading.

* **Before you add an arm to a rule, check that no existing id already reports the same fault.**
  [Which check reports a fault](../docs/reference/checks.md#which-check-reports-a-fault) says how the set divides.

The site states the rest. Each row links the section that states it.

| Before you                                    | Read                                                                                               |
|-----------------------------------------------|----------------------------------------------------------------------------------------------------|
| declare a rule nothing implements yet         | [A rule with no `severity:`](../docs/reference/held-to.md#a-rule-with-no-severity)                 |
| write an `expr:` the grammar will not take    | [Adding a fact](../docs/maintaining/checks.md#adding-a-fact)                                       |
| write a rule one fixed message cannot explain | [When a rule needs C#](../docs/maintaining/checks.md#when-a-rule-needs-c)                          |
| report several faults under one id            | [The three places that must agree](../docs/maintaining/checks.md#the-three-places-that-must-agree) |
| move a threshold                              | [The calibrated windows](../docs/maintaining/checks.md#the-calibrated-windows)                     |
| move a text rule's pattern into C#            | [Decisions](../docs/maintaining/checks.md#decisions)                                               |
| write a `notes:`                              | [What a `notes:` contains](../docs/maintaining/charter.md#what-a-comment-or-a-notes-contains)      |
