### Changing the schema

**A schema edit alone leaves the corpus stale.** Every type page carries generated blocks derived from these files, and
`generate --check` fails in CI until you regenerate.

1. **Read [`.schema/CLAUDE.md`](../../../../.schema/CLAUDE.md) first.** It carries the closed key space, the field-order
   trap, and the test for whether a rule needs C#.
2. **Check the question is not already answered.** A `reciprocal:`, a `mirrors-section:`, a `required-when:`, a scalar
   type or a required section may already say it. Each has been written out as a rule at some point.
3. **Make the edit.** A field using keys the schema language already has costs nothing in C#. A key the language does
   **not** have means an edit to `Schema.cs`, to the code reading what it parsed into, and to `meta/type.schema.json`
   in the same change. A value nothing dispatches fails `schema-dispatch` one step later. Where the obligation is
   conditional, `required-when:` is the existing key and it always reports an error. A should-have-done-this is a rule
   with an `expr:`, which chooses its own severity. Check what the type's neighbouring obligations do before making this
   one harsher than they are.
4. **Add the field to `<type>/_template.md` by hand, in both trees.** Nothing generates a template, and a template is
   `seed`, so nothing holds the two copies equal. `template-fields` reads `Required` alone: it catches a required field
   you left out, and says nothing about an optional one **or a `required-when:` one**. For a conditionally required
   field this step is the only thing standing between it and every document copied from the template.
5. **Write the `description:` and `notes:` to `technical-writing` and then `writing-a-record`.** The generator prints
   them onto the type page, so they are read by an author rather than by you.
6. **Run `kac generate` in both corpora**, then `kac validate` in both.
7. **Write a fixture that trips a new rule**, under `tooling/tests/fixtures/rules/`, one per check id it reports. The
   coverage gate builds its catalogue from `kac checks` and fails a rule nothing exercises.
8. **Run the golden suite.** The fixtures validate against the real `.schema/`, so this edit can also move expectations
   already committed there. Regenerate with `--update` only after reading the diff.
9. **Nothing to copy.** `.schema/` is authored once at this root and read from there by every corpus, so a schema
   change lands in one place. A type page or a `_template.md` you also touched does live in every tree, and
   `kac update --check --from ../../` inside each corpus under `examples/` is what proves the copies match.
10. Run **[opening-a-pull-request](opening-a-pull-request.md)**.

**Reply:** what the schema now declares, which pass would catch it being wrong, and any golden expectation that moved.
