# `export` — the corpus as data

A consumer of a corpus should not clone it. `export` writes what the corpus knows into `.dist/` as data built for an
agent to read: a manifest saying what the export is, one file per record, and a flat file cheap to grep.

`.dist/` is gitignored and rebuilt whole, so it is never something to review. **The overwrite is delete-then-write.** A
record deleted from the corpus must not leave an entry behind in the output. Nothing else would catch it: the export is
untracked, so no diff shows the orphan and no check goes looking for one.

```
.dist/
  manifest.json          what this export is, and where it came from
  glossary/
    gls-<name>.json      one record: its declared fields, its declared sections, and its links
    terms.jsonl          every term, one to a line, addressed by path and anchor
```

**What travels is the type's decision**, declared in its `export:` block and described in
[`../../.schema/README.md`](../../.schema/README.md). The exporter reads that declaration and nothing else, so a corpus
that adopted no exporting type still writes a manifest, with an empty type list — "nothing" is a valid statement of what
a corpus has.

**The flat file is JSONL because it exists to be grepped.** A hit has to hand back something parseable on its own. A
matching line of an indented document is a fragment, and the reader is left seeking outward for its braces.

So each line repeats what a reader would otherwise look up: the record it came from, the state of that record, and its
cross-references as ids. That costs bytes. It is worth them, because the alternative is a hit that sends the reader to
the very file this one exists to save them opening.

An address is the one thing a line does not repeat. It carries the record's `path` and the part's `anchor`, and the
manifest carries the two templates they go into.

**Records are ordered roots-by-id, each root's chain depth-first beneath it.** Terms sort alphabetically within a
record. Generality holds **within a chain** and nowhere else: `gls-search` narrows `gls-example-libraries`, so a grep
for `title` meets the general entry before the one refining it. Across unrelated roots the order is stable and says
nothing — `record` is defined by `gls-example-libraries` and `gls-knowledge-as-code`, neither narrowing the other, and
reading the first hit as the more general one would give a reader the wrong domain. `narrows` on the owning records is
what tells the two cases apart, and every line names the record it came from.

**A cross-reference is read, never inferred.** A `**Not:**` line pointing at another glossary is a link, and a link's
target is stripped out of the prose — so the export carries the part it names in `seeAlso`, as `gls-search.title`. It
resolves to the term rather than the record, because `redefinitions-are-reciprocal` is about a term and its
counterpart.

A link naming a record and no term inside it leaves nothing to read, so nothing is carried. The obvious guess is the
same word in the other glossary; it is right only for a pair that happens to share a spelling, and silently wrong for a
`Borrower` that redefines a `Patron`. The run therefore names each link it could not read, since an omission in an
artefact nobody reviews is invisible.

**Absent is `null`,** in every file and for every key. A field a record leaves blank and a field it does not carry are
one absence to a consumer. Writing `""` in one file beside `null` in another would leave that consumer checking which
file it had opened before it could test for nothing.

**Prose arrives unwrapped.** The corpus wraps at 120 columns, which is a fact about the file rather than about the
words, and a grep for a phrase straddling the wrap would find nothing. Blank lines are the author's and stay; a list,
heading, quote, table or fence is left exactly as written. That last part is a decision rather than an unfinished case,
because the two mistakes do not cost the same: a list joined onto one line is destroyed and cannot be recovered by the
reader, where a paragraph left wrapped merely arrives as it was written. Every doubtful line therefore goes the safe
way, and a corpus whose sections happen to hold only paragraphs today is not a reason to narrow it.

**Two link forms, both naming a ref.** A person follows the rendered one and an agent fetches the raw one. The rules
joining a base to a path, and the anchor rule for a part, belong to `publishing-target` and live in `Publishing`;
`.corpus.yaml` supplies only the bases. Every link resolves against the commit the export was built from, so a citation
names the version the agent read rather than whatever the branch holds later.

**The manifest states both forms as templates, and a per-record file resolves them.** The templates are
`https://…/blob/<sha>/{path}#{anchor}` and `https://raw…/<sha>/{path}`, and a consumer substitutes the `path` and
`anchor` a term line carries. Three things follow from writing them this way:

* **The ref is inside the template.** A ref left as a placeholder is forty hex characters copied by an agent, and a
  one-digit slip there is a plausible 404 nobody checks. With the commit already in the string, the worst a
  substitution can produce is a wrong path — visible, and correctable.
* **Only the human template takes an anchor.** Raw source has no fragment to honour, so the asymmetry is a property of
  the templates rather than a rule each reader has to remember.
* **The bases are not carried beside them.** They are in the templates already, and a manifest stating one address
  twice is a manifest that can state it two ways.

`Publishing.Links` substitutes into those same templates, so a link the export resolves and a link a consumer builds
for one part are the same string.

Four kinds of corpus have no address the tool can build on: one publishing nowhere, one naming a target nothing builds
links for, one stating a target but no bases, and one git cannot answer for. Each exports without links. The manifest
carries the target it was given and null templates beside it, so a consumer sees the absence stated; the run itself
says which of the four caused it. A term line is unaffected: `path` and `anchor` are facts about the corpus rather than
about where it is published, and they travel either way.

**Two versions, and they are independent.** `formatVersion` in the manifest is the shape of the output, and a consumer
reads it to know whether it can parse what it was handed. `contentVersion` is `content-version` from `.corpus.yaml` —
what the corpus knows, semantically versioned and bumped by hand — and a consumer reads it to know whether to re-read
the words. Neither implies the other: a corpus can rewrite every definition without `formatVersion` moving, and
`formatVersion` can move over a corpus nobody has edited.

**Output is deterministic.** Ordering is `StringComparer.Ordinal` throughout, as the generator's is, and every value
that varies between two runs is confined to the manifest. Two runs from one commit produce identical bytes but for
`generatedAt`.

**An unsettled record travels by default.** A draft glossary, and one whose `review-by` has passed, are both exported
carrying their own state, because filtering them would make the corpus's own condition invisible downstream. A corpus
may exclude either with `export.exclude:` in `.corpus.yaml`. Where it does, the run names every record it withheld,
because a record left out of the output cannot be seen there.

**The manifest records whether the export can be reproduced.** It carries the commit and a dirty flag. The flag is
there because a commit on its own would describe a dirty tree as reproducible.

