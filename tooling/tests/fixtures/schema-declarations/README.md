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
  `index.order: newest-first`, `tier: experimental`, a `values:` list on a `type: list` field, a `min-items:` on a
  `type: string` field, and a `part-required:` on a field naming no `ref:` to resolve the part against.
* **Shape** — a type with no `folder:`, `mirrors-section: See also` where the only section the type declares
  is `Summary`, `tier: experimental` where `_tiers.yaml` declares no such tier, `part-required: true` on a field
  pointing at `widgets`, which keeps no parts, a `summary:` too long for the table cell it becomes, and no
  `goes-here:` at all — the two ways one key can fail, taken one each so both are pinned.
  `label-plural:` and `detail:` are present, since a type with nothing to say about itself would report the same fault
  four times and pin nothing extra.
* **Export** — a projection every part of which resolves to nothing: `colour` where the type declares no such field,
  `Provenance` where it declares no such section, `parts: full` where it locates no parts, `Summary` at a fidelity
  nothing carries, and no `version:` for a consumer to read the type's files against. `Summary` is a section the type
  really does declare, so what it pins is the fidelity alone. An entry declaring no fidelity is pinned by a unit test
  instead. The type declares one section, `Summary` has spent it, and hanging a second fault on `Provenance` would
  report two faults from one line.

  **The `line:` vocabulary is pinned by unit tests.** Reaching those checks needs a type that locates its parts, and
  giving `widgets` a `parts:` block trades the `export.parts:` fault above for two others: a fidelity read from
  nowhere, and the missing `line:`. `SchemaCheckTests` covers each source, and a fixture type existing only to host
  them would pin those same findings under a second name.
* **Versus** — a disambiguation against `widgets` itself. The other way a pair goes wrong, both sides declaring it,
  needs two types and is pinned by a unit test instead.
* **Unknown** — `stability:` at the top of the file and `unique:` on a field, at two levels, because the vocabulary is
  per-level. The `notes:` on the `id` block is the other half of that assertion: it is parsed nowhere and reported
  nowhere, which is what makes closing the rest of the key space possible.

`gizmos` is the type this corpus declined, named by a `versus:` and by the second entry of `ref: [widgets, gizmos]`.
Neither appears in the golden, and that silence is the assertion: a corpus adopts as many types as it has use for, so a
reference at one it turned down enforces nothing and is reported nowhere. A name that is simply misspelled is caught in
the repository that authors the schema, by `SchemaReferenceTests`.
