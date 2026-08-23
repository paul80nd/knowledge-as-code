### Changing the tool

**Ask first whether it needs C# at all.** A check that is a predicate over frontmatter, sections, links or length is an
`expr:` on a rule in the schema, and costs the YAML and a fixture.

1. **Read [`tooling/CLAUDE.md`](../../../../tooling/CLAUDE.md).** It carries the test for expression against class, the
   two rule interfaces, and where the console boundary sits.
2. **Where the answer is an `expr:`, stop here and run
   [changing-the-schema](changing-the-schema.md) instead.** Nothing else on this page applies.
3. **Write the class in `kac.core/Rules/`**, with its unit tests beside it and a line in the registry. Take the
   narrower interface wherever it will do.
4. **Declare what it reports.** An entry in `_checks.yaml`, and either a row in `Generator.DocRows` or
   `on-type-page: false`. Three places have to agree and each fails a different meta-test.
5. **Write a fixture that trips it**, one per check id it emits. The coverage gate reads ids rather than branches.
6. **Load `technical-writing`, then `writing-in-the-tool`** for the comments, the test names and the feature document.
   Read the code under every comment you touch.
7. **Update [`tooling/features/`](../../../../tooling/features/)** for the command you changed, and
   [`tooling/README.md`](../../../../tooling/README.md) where it maps them.
8. **Run all four layers**, one `kac` invocation at a time. Unit, behaviour specs, golden fixtures, then `validate` and
   `generate --check` in both corpora.
9. Run **[opening-a-pull-request](opening-a-pull-request.md)**.

**Reply:** what the tool now does, which layer proves it, and what you decided against building in C#.
