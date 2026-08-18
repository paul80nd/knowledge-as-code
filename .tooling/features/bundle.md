# `bundle` — the export as something installable

## Intent

An export is data, and data has to be handed to something. `bundle` assembles what `export` wrote, plus the `.plugin/`
tree, into a Claude Code plugin directory under `.dist/plugin/`, and writes a local marketplace beside it so the result
can be installed from a path. What ends up in the plugin is a function of what the export carried: a corpus that ships
no glossary ships no glossary skill either.

## What it is not

**It is not `export`.** `export` reads the corpus and writes data; `bundle` reads that data and writes a package. They
are two commands because they fail differently — an export is wrong about the corpus, a bundle is wrong about what it
shipped — and because they are proved differently: the export has a committed golden of its output, and the bundle has
an assertion that it did not touch that output.

**It does not publish.** It writes a directory and a marketplace that names it, both untracked. Pushing either
anywhere is not this command's, and neither is running `claude plugin validate` over the result.

**It does not read the corpus.** `Corpus.Load` is never called. Everything a bundle decides is a fact about the export
it was handed, and the export is the only thing that will actually travel.

## Approach

**Two directories under one root, and each command replaces its own whole.** `export` owns `.dist/export/` and
`bundle` owns `.dist/plugin/`. Both delete before they write, for the reason `export` already had: an artefact nobody
reviews must not keep a file that nothing backs any more. That rule only stays local if neither command deletes the
other's tree, which is why the export is a named subtree rather than the root. `Dist` is the one statement of the
layout, and `.gitignore` covers all of it in a line.

**The export is copied in, and therefore exists twice.** Once at `.dist/export/` as `export` wrote it, and once inside
the plugin under the directory `metadata.corpusRoot` names. **That duplication is intended.** An installed plugin is
copied into a cache of its own, so a path outside the plugin root does not travel and resolves nowhere at runtime; and
`export` has to stay independently runnable and independently proved without `bundle` having run. The copy is the seam
between the two commands rather than an accident of one.

**`bundle` never edits what it copies.** The export travels byte for byte — the plan carries bytes rather than text,
so a copy cannot acquire an opinion about encoding or line endings on the way through. A difference between the two
copies is a defect, and the golden fixture asserts their equality directly.

**Trimming reads the export, not the corpus.** A component declares under `metadata.components` which record types it
reads, and it travels only where the export carries every one of them. What that catches is the skill that finds
nothing — which reads, to whoever asked it a question, exactly like a corpus that does not define the term. Reading
the export rather than the corpus is what makes the criterion the same in both states `.corpus.yaml` can be in: a
corpus that declared `types:` and one whose adoption is inferred from its folders both reach the export through
`Corpus.Adopted`, and a type that contributed no record is absent from the export's manifest either way.

A trimmed component's files leave with it. A path under no component's is unconditional and travels whatever the
corpus adopted, so a README or a licence in the plugin tree needs no declaration.

**A component may be a directory or a single file.** A skill is a directory and a hook definition is a file, so the
trim matches the declared path itself as well as anything beneath it — and matches on a whole segment, so
`skills/a` does not take `skills/ab` with it.

**The plugin's version is the corpus content version.** It is read off the export's manifest rather than out of
`.corpus.yaml`, so the number on the plugin is the number of the data inside it. The export **format** version stays
where it is, inside the export's own manifest: one says what the corpus knows and the other says which parser will
read it, and stamping the second onto the plugin would describe the wrong thing confidently. A corpus stating no
content version keeps the version its plugin manifest carried, and the run says so rather than stamping nothing.

**The plugin manifest is edited, not rebuilt.** `plugin.json` is forked in the portability manifest — each corpus owns
its own — so it is read as a DOM, given a new version, given the surviving components, and written back with every
other key exactly as the corpus wrote it. Mapping it onto a shape known here would delete whatever the corpus had
added, without a word.

**The bundle records what it shipped.** `bundle.json` at the plugin root names the plugin, its version, the export it
was built around, every component included and every one trimmed with the reason. Two corpora running one plugin name
may ship different component sets — that is correct, and it makes "does this plugin do X" unanswerable from outside
unless the plugin says. It carries no timestamp and no commit: the export inside the plugin states both, and a second
clock would be a second answer to one question.

**Trimming everything warns and still builds.** Refusing would leave a corpus unable to produce the thing that would
have told it why. The empty plugin is itself the report — it installs, does nothing, and `bundle.json` beside it names
each component that was dropped and the type it needed.

**`.dist/` is the marketplace.** A marketplace is a directory holding `.claude-plugin/marketplace.json`, and it
resolves each plugin's source against that directory; a source containing `..` is refused. So a marketplace cannot sit
beside the plugin and point sideways at it, and the root is where it goes. `claude plugin marketplace add ./.dist`
installs what was just built.

## Decisions

**The output is a directory rather than an archive.** A marketplace can point at a path, and there is nothing to
unpack between building and asking the plugin a question.

**A run that cannot answer a question stops before writing.** A plugin tree with no manifest; a manifest that is not
JSON, or that states no name, or no `corpusRoot`, or a `corpusRoot` the plugin tree already uses; and an export with
no manifest — each ends the run with the reason and nothing written. The alternative is a plugin assembled around a
missing answer, which installs and fails later somewhere less obvious. The last of those is the one worth naming: the
export lands under `corpusRoot` inside the plugin, so a collision there would have one side silently overwrite the
other, and whichever won the loser would be missing from an artefact nobody reviews.

**`corpusRoot` is read rather than defaulted.** The plugin's skills address the export as
`${CLAUDE_PLUGIN_ROOT}/<corpusRoot>/…` by that name. A default here would be the tool quietly disagreeing with words
the corpus wrote in its own skill, and the disagreement would surface only when someone asked the installed plugin a
question.

## Known limits

**Nothing validates the assembled plugin.** `claude plugin validate` is not run, and a component misplaced inside
`.claude-plugin/` would load wrong without this command noticing. That, the round-trip lookup and the two-platform CI
matrix are tracked as [issue #186](https://github.com/paul80nd/knowledge-as-code/issues/186).

**A component's `requires` is not held against the schema.** A component naming a type no schema declares is trimmed
with the same message as one naming a type this corpus declined, and the two mean different things: one is a typo and
the other is a decision. Nothing reports the first.

**The export is copied whole.** A component surviving trimming pulls in the entire export, including types no
surviving component names. That costs nothing today, because the trim and the export are driven by the same adoption;
it is worth reopening for a corpus exporting many types where a plugin reads one.
