Records whose frontmatter parses and whose shape is wrong: the filename, the id, the headings and the identity line.

Each record has one defect cluster, and its filename says which. The three types use three id forms, so the id
checks run against each: a numbered ADR, a mnemonic policy and a slugged tool.

* **The ADRs** take the structural checks. `slug-length`, `id-prefix`, `id-format` and `filename-pattern` take one
  record each. `0004` lacks `## Consequences` and carries another number in its id, so `required-section` and
  `id-matches-filename` land together. `0005` has no H1, which also forces the `y-statement` finding. `0010` keeps a
  placeholder from the template, for `placeholder-left`, and `0011` has headings with nothing under them, for
  `empty-section`.
* **The policies** take the mnemonic id checks and each branch of the identity line: absent and malformed for
  `identity`, the wrong id for `identity-id`, the wrong type for `identity-type`, and disagreeing and wrong-cased for
  `identity-status`. `label-canonical` fires on labels in the wrong case and on labels leading to a record with another
  id. `derived-key` fires on a written `category`, in a record and in the template.
* **The tools** take `id-matches-filename` and `id-format` for a slugged id.

`policies/dirs-directory-link.md` links to `media/`, a folder that exists, and `link-resolves` reports it. A link to a
directory has to fail on every machine, because git cannot keep an empty one. `media/note.md` is there only to make the
folder exist, and no schema declares `media/`, so nothing reads it.

`Structure.feature` reads this corpus for what `validate` promises about a record's shape.
