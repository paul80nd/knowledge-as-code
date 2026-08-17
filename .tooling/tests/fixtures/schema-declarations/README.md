A schema that declares things the tool cannot do, and no documents at all.

Every other fixture lays its corpus over the real `.schema/`; this one lays a *type file* over it — `widgets.yaml`, a
type that exists only to be wrong. That is the only way to reach these checks, because what they read is the schema
rather than any document, and the real schema is expected to be clean.

The corpus is empty on purpose. With no records and no folders, `type-setup` and every per-document check stay silent,
so the golden is the schema pass and nothing else.

`widgets.yaml` carries one instance of each fault:

* **Unreadable** — an `expr:` that does not compile, an `expr:` with no `severity:`, an `expr:` with no `message:`, a
  `required-when:` outside the vocabulary, and `values: $enums.sensitivity` where `_enums.yaml` declares no such enum.
  The loader asks for severity first, so a rule omitting both reports only the missing severity. The two faults
  therefore need a rule each.
* **Undispatched** — a rule claiming `severity: warning` that nothing implements, `id.style: roman-numeral`,
  `index.order: newest-first`, `tier: experimental`, `ref: gizmos` at a folder no schema covers, a `values:` list on a
  `type: list` field, and a `min-items:` on a `type: string` field.
* **Shape** — a type with no `folder:`, `mirrors-section: See also` where the only section the type declares
  is `Summary`, `tier: experimental` where `_tiers.yaml` declares no such tier, a `summary:` too long for the table
  cell it becomes, and no `goes-here:` at all — the two ways one key can fail, taken one each so both are pinned.
  `label-plural:` and `detail:` are present, since a type with nothing to say about itself would report the same fault
  four times and pin nothing extra.
* **Export** — a projection every part of which resolves to nothing: `colour` where the type declares no such field,
  `Provenance` where it declares no such section, `parts: full` where it locates no parts, and `Summary` at a fidelity
  nothing carries. `Summary` is a section the type really does declare, so what it pins is the fidelity alone. An entry
  declaring no fidelity is pinned by a unit test instead: the type declares one section, that entry has already spent
  it, and hanging the fault on the other would report two faults from one line.
* **Versus** — a disambiguation against `gizmos`, which no schema covers, and one against `widgets` itself. The third
  way a pair goes wrong — both sides declaring it — needs two types and is pinned by a unit test instead.
* **Unknown** — `stability:` at the top of the file and `unique:` on a field, at two levels, because the vocabulary is
  per-level. The `notes:` on the `id` block is the other half of that assertion: it is parsed nowhere and reported
  nowhere, which is what makes closing the rest of the key space possible.

`ref: [widgets, gizmos]` is the pair that matters: the first entry resolves and only the second is reported, which is
what pins that a list `ref:` is read entry by entry rather than dropped whole.
