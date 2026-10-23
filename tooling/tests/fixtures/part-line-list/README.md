A part line naming a list field, over a record that fills the field with two entries.

`sprockets.yaml` is laid over the real schema. Its `export.parts.line:` reads `shelves` from `front.shelves`, a
`type: list`. One sprocket holds two shelves and two teeth, so the export would write `null` under `shelves` on each
tooth's line. The golden is the one `schema-shape` finding against the type file, and the record reports nothing.

No real type names a list on a part line, so this type exists to reach the refusal. The other `line:` sources are
pinned by `SchemaCheckTests`, beside the own, universal and `of: object` forms of this one.
