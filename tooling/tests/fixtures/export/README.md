# `export`

What `kac export` writes, asserted through the CLI. The corpus reads as three ordinary glossaries, and each thing it
pins is a property of that content rather than a note left inside it — a fixture whose prose describes the test is a
fixture nobody can read as an example.

## What the corpus is shaped to reach

**Two roots and a chain.** `gls-estate` and `gls-framework` both carry an empty `narrows:`, and `gls-search` narrows
`gls-estate`. That is the shape the ordering has to be right about, and the two halves of it mean different things:

* Within the chain, generality holds. A term `gls-estate` and `gls-search` both defined would meet the general entry
  first.
* Across the two roots it means nothing. Both define `record` — a catalogue description in one, a knowledge document in
  the other — and neither narrows the other, so the order between them is stable and says nothing about which is more
  general. Nothing here asserts that it does. Reading the first hit as the more general one is the mistake this pair
  exists to keep visible.

**A reciprocal pair, written one way each.** Both roots define `record`, and each `**Not:**` line points at the other.
`gls-estate` names the anchor and `gls-framework` names only the file, which is the pair half-finished — a state a real
corpus passes through.

The export reads the first into `seeAlso` as `gls-framework.record` and carries nothing for the second, naming it in
the run instead. Both roots happening to say `record` is what makes this pair worth having: it is the case where a guess
would have been right, so a fixture that passed on it would prove nothing. `/tooling/README.md` says why the guess is
refused.

**Wrapped prose and a paragraph break.** `gls-estate` carries a `**Not:**` line wrapped across two source lines, and a
`Scope` of two paragraphs. The export joins the first and keeps the second, which is the whole of the unwrap rule.

**No git repository.** The runner assembles the corpus in a temp directory and never initialises one, so no ref
resolves and no link is written. That is the corpus-publishes-nowhere path, and it is the only publishing state a
fixture can reach. The link forms belong to `PublishingTests`, which supplies a ref of its own.

## What is asserted where

`expected-dist/` is the export itself, committed file for file. It is the tracked copy of an untracked artefact:
`.dist/export/` is rebuilt whole on every run and reviewed by nobody, so this is where a change to it becomes
something a person can see. [The suite README](../../README.md) describes the diff and what it normalises away.

**A diff under `expected-dist/` is a change to a published contract.** Regenerate it deliberately with
`dotnet run tooling/kac-tests.cs -- --update export`, read what moved, and say in the commit message why it moved. A
key added, renamed or dropped also moves `Exporter.FormatVersion`, which is what a consumer reads to know whether it can
still parse what it was handed.

The other three files carry what a whole-file diff cannot, and none of them lists the export again.
`expected-export.txt` holds lines the run must print — an export says what it could not carry, and none of that reaches
`.dist/export/`. `expected-files.txt` names what must be there afterwards outside the export, which is the corpus,
unchanged.
`expected-content.txt` holds `<path> :: <text>` pairs, read against the emitted file, which is where the timestamp and
the commit are pinned. A `!` prefix inverts a line in either of the last two.

The scenario runs twice. The second run happens over an output seeded with a file no record backs, which is what pins
the overwrite as delete-then-write.
