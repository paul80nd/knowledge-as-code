### Changing the tool

**Ask first whether it needs C# at all.** A check that is a predicate over frontmatter, sections, links or length is an
`expr:` on a rule in the schema, and costs the YAML and a fixture.

1. **Read [`tooling/CLAUDE.md`](../../../../tooling/CLAUDE.md).** It carries the test for expression against class, the
   two rule interfaces, and where the console boundary sits.
2. **There are three rungs, and only the first skips this page.**
   * An `expr:` over facts that already exist is schema alone. Run
     [changing-the-schema](changing-the-schema.md) and stop here.
   * **A new fact plus an `expr:`** is both. Adding a fact is one method on `Facts`, one row in `RuleExpr.Functions`
     and one row in the fact table in `.schema/README.md`, which `DocumentationTests` holds equal. The grammar never
     changes. Take steps 5 to 9 below for the C# half, fixture included, then run `changing-the-schema` for the rule.
   * A question no expression can ask is a rule class. Carry on down this page.
   Check the fact table before assuming a rung: `words()` is whole-document, and there is no fact measuring a section.
3. **Write the class in `kac.core/Rules/`**, where the third rung was the answer. Its unit tests sit beside it and
   a line goes in the registry. Take the narrower interface wherever it will do.
4. **Declare what it reports.** An entry in `_checks.yaml`, and either a row in `Generator.DocRows` or
   `on-type-page: false`. Three places have to agree and each fails a different meta-test.
5. **Write a fixture that trips it**, one per check id it emits. The coverage gate reads ids rather than branches.
6. **Load `technical-writing`, then `writing-in-the-tool`** for the comments and the test names. Read the code under
   every comment you touch.
7. **Update the command's page in [`docs/cli/`](../../../../docs/cli/)**, loading `writing-the-docs` for it, and
   [`tooling/README.md`](../../../../tooling/README.md) where the change reaches it. The usage block at the head of the
   page is generated, so an option that moved is regenerated with `KAC_UPDATE_DOCS=1 dotnet test tooling/kac.tests`
   rather than typed.
8. **Run all four layers**, one `kac` invocation at a time. Unit, behaviour specs, golden fixtures, then `validate` and
   `generate --check` in both corpora. Run the goldens as CI sees them: they read the environment, so
   `GITHUB_ACTIONS=true` is what reproduces a pass here and a failure there.
9. Run **[opening-a-pull-request](opening-a-pull-request.md)**.

**Reply:** what the tool now does, which layer proves it, and what you decided against building in C#.
