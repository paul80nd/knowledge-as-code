### Changing the schema

**A schema edit alone leaves the corpus stale.** Every type page carries generated blocks derived from these files, and
`generate --check` fails in CI until you regenerate.

1. **Read [`.schema/CLAUDE.md`](../../../../example/.schema/CLAUDE.md) first.** It carries the closed key space, the
   field-order trap, and the test for whether a rule needs C#.
2. **Check the question is not already answered.** A `reciprocal:`, a `mirrors-section:`, a `required-when:`, a scalar
   type or a required section may already say it. Each has been written out as a rule at some point.
3. **Make the edit.** A new key means an edit to `Schema.cs`, to the code reading what it parsed into, and to
   `meta/type.schema.json` in the same change. A value nothing dispatches fails `schema-dispatch` one step later.
4. **Add the field to `<type>/_template.md` by hand.** Nothing generates a template. `template-fields` catches a
   required field you left out and says nothing about an optional one.
5. **Write the `description:` and `notes:` to `technical-writing` and then `writing-a-record`.** The generator prints
   them onto the type page, so they are read by an author rather than by you.
6. **Run `kac generate` in both corpora**, then `kac validate` in both.
7. **Run the golden suite.** The fixtures validate against the real `.schema/`, so this edit can move expectations in
   `tooling/tests/fixtures/`. Regenerate with `--update` only after reading the diff.
8. **Copy the file across by hand.** `.schema/` is held byte-equal between `template/` and `example/` by
   `TemplateTests`, and `kac mechanism --sync` cannot do it here.
9. Run **[opening-a-pull-request](opening-a-pull-request.md)**.

**Reply:** what the schema now declares, which pass would catch it being wrong, and any golden expectation that moved.
