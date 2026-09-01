# Working in Example Dogfooding

[`../../CLAUDE.md`](../../CLAUDE.md) at the repository root carries the conventions, the commands and the writing rules
for every corpus here. This page carries what is this corpus's alone.

* **This corpus describes the repository it sits in.** Every other corpus here is fiction, so a convenient record is
  good enough there. A record here names a real workflow file, binds a real pull request, or is followed by whoever is
  on the end of a red build. Check the claim against the repository before you write it down, and correct the record
  when the repository moves.
* **A finding about the framework belongs to the framework.** Where a schema field has no honest value for something
  real here, raise it on the issue tracker and say what the record carries meanwhile.
* **Cite the inherited clause rather than restating it.** [`../engineering/`](../engineering/) carries the policies. A
  standard here says what one of its clauses means for this repository, and names the clause on a `Covers` line.
* **Restore before you validate.** Run `export` and then `pack` in [`../engineering/`](../engineering/), then here:

```bash
dotnet run --project ../../tooling/kac -- restore
```
