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

**A cross-reference.** `gls-estate.record` points at `gls-framework`, and the link names the file rather than the
anchor, as this corpus's own glossaries do. The export resolves it to `gls-framework.record` — the counterpart, not the
record — so the fixture pins the inference as well as the output. See
[knowledge-as-code#194](https://github.com/paul80nd/knowledge-as-code/issues/194) for why that inference is temporary.

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
