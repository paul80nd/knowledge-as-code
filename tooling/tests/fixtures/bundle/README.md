# `bundle`

What `kac bundle` assembles, asserted through the CLI. The corpus holds the same three glossaries the export fixture
does, because a bundle is built from an export and a second shape of export here would leave two accounts of what one
contains. `export-type` narrows the run to `glossary`, which is also where the suite pins `--type`.

## What the fixture is shaped to reach

**A plugin declaring two components, one of which the corpus cannot support.** `skills/glossary-lookup` requires
`glossary` and the corpus holds three of them; `skills/decision-lookup` requires `adrs` and the corpus holds none. So
every run in this mode exercises both arms of the trim, rather than one fixture proving the plugin assembles and a
second proving anything ever leaves it.

**A file under no component's path.** `plugin/README.md` belongs to no component, so nothing decides whether it
travels and it always does. Trimming is a statement about components, and a tree where every file happened to sit
under one would not show that.

**A hook, so the breadcrumb is rendered rather than only planned.** `plugin/hooks/` carries the two scripts and the
hook definition; `hooks/breadcrumb.txt` is in neither the fixture nor the plugin tree, because `bundle` writes it. The
corpus behind this fixture holds three glossaries with distinct titles, which is what lets `expected-content.txt`
assert that the line names the contexts rather than only counting them. The trimmed arm is `bundle-empty`'s, where the
hook goes and the breadcrumb goes with it.

**A corpus stating a content version.** `.corpus.yaml` here carries `content-version: "2.3.4"` and the plugin manifest
carries `version: "0.0.1"`. The two differ so that the stamp is visible: a fixture where they agreed would pass
whether the version was replaced or left alone. `bundle-empty` is the corpus that states none, and pins the fallback.

## What is asserted where

**There is no committed copy of the assembled plugin.** Most of one is the export, and
[the export fixture](../export/README.md) already commits that file for file; a second copy here would be one more
thing to regenerate and one more place for the two to disagree.

What the runner asserts instead is the part a bundle adds. `expected-bundle.txt` holds lines the run must print — the
trimmed component is named there, because a component dropped from an artefact nobody reviews is invisible.
`expected-files.txt` names what is there afterwards and, with a `!` prefix, what is not: the trimmed skill's words
never reach the plugin, and the export the bundle read is left where it was. `expected-content.txt` reads
`<path> :: <text>` pairs against the emitted files, which is where the version stamp, the surviving manifest keys and
`bundle.json` are pinned.

**The copy of the export inside the plugin is compared byte for byte with the export it came from.** That is the one
assertion this fixture exists for. The duplication is intended — an installed plugin is copied into a cache, so a path
outside the plugin root does not travel — and a difference between the two copies is therefore a defect rather than
something to interpret. `corpus-root` names where the copy landed, which is the fixture manifest's
`metadata.corpusRoot` and the one thing the runner cannot read for itself.

The scenario runs twice. The second run happens over a plugin seeded with a skill no component backs, which is what
pins the overwrite as delete-then-write.
