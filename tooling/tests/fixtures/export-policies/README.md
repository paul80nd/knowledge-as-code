# `export-policies`

The policies type, exported on its own. [`export`](../export/README.md) proves that a glossary and a policy travel
side by side. This fixture proves the policy half stands with no glossary beside it, over a clause table carrying the
rows a tidy one never has.

## What the corpus is shaped to reach

**All four levels, in the order a policy declares them.** `COPY` and `RESTORE` are `MUST`, `SHARED` is `MUST NOT`,
`DRILL` is `SHOULD`, and `MEASURE` is `COULD`. Every one reaches its clause line as a `level`, and `MUST NOT` is the one
worth pinning: it opens with the `MUST` that is a level of its own, so a consumer matching the shorter modal first files
a prohibition as an obligation. The scenario runs `kac export` alone, so nothing here exercises `clause-order`. The
order is the corpus's, and `broken-parts` is where a table breaking it is reported.

**A clause naming another clause.** `RESTORE` cites `pol-BKUP.COPY`. A table row has no body, so nothing reads a
cross-reference out of one. The id travels inside the words and the line carries no `seeAlso` to look it up by. The
glossary's half of that pair belongs to `export`.

**Two ids at the pattern's limit.** `RESTORE` and `MEASURE` are seven characters, which is as long as `id-pattern`
admits. A clause id is half of every citation that reaches it, so nothing may shorten one on the way out.

**Alignment stated twice, and travelling neither time.** `COPY` and `SHARED` cite `[ISO 27001:2022].A.8.13`, and
`aligns-with` rolls that reference up. `RESTORE`, `DRILL` and `MEASURE` leave the cell empty, which is the honest
answer where no genuine mapping exists. The golden holds neither the cells nor the roll-up: a reference resolves
through `frameworks.md`, which says what our standing against the framework is, and no consumer receives that page.
It is the only export fixture whose roll-up is populated, so it is the only one where that absence means anything.

**Both fidelities on one record.** `Purpose` travels as its opening paragraph, and `pol-BKUP` writes two paragraphs,
so the one left behind is visible in the golden. `Scope` and `Exceptions` travel whole.

**An active policy.** `pol-RTNT` in `export` is a draft. A `status` written as a constant rather than read from the
record would pass that fixture and fail this one.

## What is asserted where

The expectations [the suite README](../../README.md) describes for an `export` scenario, and it says what each one
carries.

`expected-dist/` is the export itself, committed file for file. A diff under it is a change to a published contract.
Regenerate it with `dotnet run tooling/kac-tests.cs -- --update export-policies`, read what moved, and say in the commit
message why it moved.
