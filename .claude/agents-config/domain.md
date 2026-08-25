# Domain Docs

How the engineering skills consume this repository's domain documentation before they explore the codebase.

## Before exploring, read these

- **[`CLAUDE.md`](../../CLAUDE.md) at the root.** It routes the work to the tree that holds it, and it names what has
  already cost a session here.
- **The `CLAUDE.md` of the tree you are changing.** [`tooling/CLAUDE.md`](../../tooling/CLAUDE.md) covers `kac` and its
  tests. [`.schema/CLAUDE.md`](../../.schema/CLAUDE.md) covers the schema.
  [`examples/README.md`](../../examples/README.md) covers a record.
- **The `i-want-to` skill.** It carries a playbook per kind of change, and the root `CLAUDE.md` says to load it before
  you plan.

There is no `CONTEXT.md` and no `CONTEXT-MAP.md` here. Proceed without them.

## `adrs/` records the estate, not the tool

Every `adrs/` folder in this repository sits inside a corpus: `template/adrs`, `examples/*/adrs`, and the test fixtures.
They are records a worked example holds, written by a fictional engineering estate. None of them decides anything about
`kac`.

A decision about the tool goes to `tooling/CLAUDE.md` or to the commit message. Do not read an `adrs/` record as a
constraint on the code, and do not add one to record a change you make to the code.

## Use the glossary's vocabulary

A knowledge corpus glossary travels with this repository. Where your output names a domain concept (an issue title, a
refactor proposal, a test name), ask the `glossary-lookup` skill for the term before you infer it from usage.

A concept the glossary does not hold is a signal. Either you are inventing language the project does not use, or there
is a real gap.
