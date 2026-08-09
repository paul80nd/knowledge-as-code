A schema that declares things the tool cannot do, and no documents at all.

Every other fixture lays its corpus over the real `.schema/`; this one lays a *type file* over it — `widgets.yaml`, a
type that exists only to be wrong. That is the only way to reach these checks, because what they read is the schema
rather than any document, and the real schema is expected to be clean.

The corpus is empty on purpose. With no records and no folders, `type-setup` and every per-document check stay silent,
so the golden is the schema pass and nothing else.

`widgets.yaml` carries one instance of each fault:

* **Unreadable** — an `expr:` that does not compile, an `expr:` with no `severity:`, a `required-when:` outside the
  vocabulary, and `values: $enums.sensitivity` where `_enums.yaml` declares no such enum.
* **Undispatched** — a rule claiming `severity: warning` that nothing implements, `id.style: roman-numeral`,
  `index.order: newest-first`, `ref: gizmos` at a folder no schema covers, a `values:` list on a `type: list` field, and
  a `min-items:` on a `type: string` field.
* **Shape** — a collection type with no `folder:`, and `mirrors-section: See also` where the only section the type
  declares is `Summary`.
* **Unknown** — `stability:` at the top of the file and `unique:` on a field, at two levels, because the vocabulary is
  per-level. The `notes:` on the `id` block is the other half of that assertion: it is parsed nowhere and reported
  nowhere, which is what makes closing the rest of the key space possible.

`ref: [widgets, gizmos]` is the pair that matters: the first entry resolves and only the second is reported, which is
what pins that a list `ref:` is read entry by entry rather than dropped whole.
