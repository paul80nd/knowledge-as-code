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
would have been right, so a fixture that passed on it would prove nothing. `/.tooling/README.md` says why the guess is
refused.

**Wrapped prose and a paragraph break.** `gls-estate` carries a `**Not:**` line wrapped across two source lines, and a
`Scope` of two paragraphs. The export joins the first and keeps the second, which is the whole of the unwrap rule.

**No git repository.** The runner assembles the corpus in a temp directory and never initialises one, so no ref
resolves and no link is written. That is the corpus-publishes-nowhere path, and it is the only publishing state a
fixture can reach. The link forms belong to `PublishingTests`, which supplies a ref of its own.

## What is asserted where

`expected-export.txt` holds lines the run must print, `expected-files.txt` the paths that must exist afterwards, and
`expected-content.txt` the `<path> :: <text>` pairs each file must say. A `!` prefix inverts a line in either of the
last two.

The scenario runs twice. The second run happens over an output seeded with a file no record backs, which is what pins
the overwrite as delete-then-write.
