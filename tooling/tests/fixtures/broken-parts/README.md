One record per way a record's addressable parts can be wrong, from each of the three sources a part comes from.

The filename's first word says which fault a record carries. Every other part of the record is sound, so each file in
the golden has one finding, or one per instance of its fault.

* **A clause table**, in `policies/`. Each `clause-table` fault: no table, bullets where the table should be, the wrong
  headers, and a header with no rows. `clause-modal` fires on a clause opening with no modal and on a `MUST` written
  plain. `clause-id-format` fires on a lower-case id and on an id outside a code span. `clause-compound` and
  `clause-order` take one record each.
* **A heading**, in `glossary/`. Two terms that slug to one address, a term with nothing under it, and a `## Terms`
  section with no term in it, for `part-id-unique`, `part-empty` and `part-none`.
* **A bullet rule**, in `standards/`. A rule with no modal, a modal written plain, a heading with no bullet under it,
  and a list nested inside a rule, for `part-modal` and `part-nested`. A rule heading opening with no id, for
  `part-id-format`.

Both the table and the heading sources reach `part-id-unique`. `part-ref` fires five ways: a citation of a document
that does not exist, of a clause or a term its document does not have, one written as a part id beside a link, and one
separated by a colon.

`Parts.feature` reads this corpus for what `validate` promises about each of the three sources.
