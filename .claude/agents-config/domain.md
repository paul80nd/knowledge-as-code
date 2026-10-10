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

## `adrs/` records the estate, not the tool

Every `adrs/` folder in this repository sits inside a corpus: `template/adrs`, `examples/*/adrs`, and the test fixtures.
They are records a worked example holds, written by a fictional engineering estate. None of them decides anything about
`kac`.

A decision about the tool goes to `tooling/CLAUDE.md` or to the commit message. Do not read an `adrs/` record as a
constraint on the code, and do not add one to record a change you make to the code.

## `GLOSSARY.md` is the corpus glossary

Where a skill reads `GLOSSARY.md`, ask the `glossary-lookup` skill instead. It searches the glossary records that
travel with the `example-dogfooding` plugin. Ask it wherever your output names a domain concept: an issue title, a
refactor proposal, a test name.

A concept the glossary does not hold means one of two things. Either you are inventing language the project does not
use, or the glossary has a gap. Where a skill tells you to add a term to `GLOSSARY.md`, file the gap with the
`raise-finding` skill. This repository has no root `GLOSSARY.md` or `GLOSSARY-MAP.md`, and a session creates neither.
