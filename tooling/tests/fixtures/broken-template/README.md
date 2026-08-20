A template with defects, and the far longer list of things a template is not asked.

`adrs/_template.md` carries `priority`, which the type does not declare, and does not carry `owner`, which it
requires — the two directions of `template-fields`. Both are the same fault seen from the schema's side and the
template's: every document copied from this file would fail a check on a line its author never wrote.

Its `decided-on` is the third: a placeholder opening a value, which YAML reads as a flow mapping rather than as
text. The template looks right and parses as something else, so the finding names the fix — quote it — rather
than letting the date checks report an empty string back at whoever wrote it.

It also links to `0404-superseded-and-deleted.md`, which is not there. A template's links are checked like any
other document's, because the ones that are not placeholders are real — this is how a template pointing into a
part of the corpus that has since been deleted is caught.

**What the golden pins by leaving out is the point of the scenario.** Every one of these would be an error in a
record and is silent here, so removing an exemption fails this fixture rather than passing quietly:

| Left out                     | Why                                                                           |
|------------------------------|-------------------------------------------------------------------------------|
| `id-format`                  | `adr-{{nnnn}}` is the instruction to allocate an id, not an id                |
| `filename-pattern`           | `_template.md` is a reserved name and matches no type's pattern               |
| `slug-length`                | same — there is no slug to measure                                            |
| `required-field`             | a template's values are all bare or placeholders, which is what a template is |
| `ref-resolves`               | `related` names an ADR the author will choose                                 |
| `related-matches-section`    | both halves of the reconciliation are examples                                |
| `link-resolves`              | for `{{a}}.md` only — the placeholder target, not the real one above it       |
| `unused-definition`          | a template's definitions are exemplars of where definitions go                |
| `alternatives-have-verdicts` | the type's own rules judge a filled-in document                               |
