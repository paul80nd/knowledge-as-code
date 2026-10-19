### Reworking a type

You are comparing one type's worked examples with public exemplars, and fixing what the comparison finds. The work
reaches the records, the type page, the `_template.md` and, where the shape moves, the schema.

**The prose rules live in the skills.** `technical-writing` is the floor, and `writing-a-record` is the corpus voice and
the tier rules. This page is the order of work.

## The procedure

1. **Count what is in scope.** List the type's records in every corpus, and the type page and `_template.md` in every
   tree that adopted the type. A type folder here usually holds no records.
2. **Send a subagent on a read-only first pass.** It reads the records, the schema with its `lineage:`, the type page
   and the `_template.md` whole. It reads 4 to 6 public exemplars from primary sources, measures the shape, and lists
   findings by record id. It edits nothing.
3. **Check its claims against the files and the sources** before you act on any of them.
4. **Grill each finding with the developer**, using `mattpocock-skills:grilling`. Each one is fixed, or stated as a
   deliberate difference.
5. **Make the edits.**
   * A record follows [prc-add-a-record](../../../../examples/dog-fooding/processes/add-a-record.md).
   * A schema change follows [prc-change-the-schema](../../../../examples/dog-fooding/processes/change-the-schema.md).
   * The type page and the `_template.md` change in every tree, `## Level of detail and cadence` included. Both are
     `seed`, so copy each by hand and `diff` it against `template/`.
6. **Post the comparison on the epic**: the measurements, each finding with its verdict, and the recommended shape.
7. **Run [prc-pull-request](../../../../examples/dog-fooding/processes/pull-request.md).**
