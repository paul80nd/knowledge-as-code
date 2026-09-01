# Working in Example Payments

[`../../CLAUDE.md`](../../CLAUDE.md) at the repository root carries the conventions, the commands and the writing rules
for every corpus here. This page carries what is this corpus's alone.

* **Extend one fictional system**: Example Payments, on `example.com`, which RFC 2606 reserves.
  [`README.md`](README.md) explains why.
* **Cite the inherited clause rather than restating it.** [`../engineering/`](../engineering/) carries the policies. A
  standard here says what one of its clauses means for a payment, and names the clause on a `Covers` line.
* **Restore before you validate.** Run `export` and then `pack` in [`../engineering/`](../engineering/), then here:

```bash
dotnet run --project ../../tooling/kac -- restore
```
