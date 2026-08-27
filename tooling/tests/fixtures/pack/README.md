# `pack`

What `kac pack` seals, asserted through the CLI. The corpus is the [`bundle` fixture](../bundle/README.md)'s three
glossaries, because what a package holds is decided by the export and not by the words inside it. One key differs: this
descriptor declares a `shortcode`, which a pack is refused without.

## What the corpus is shaped to reach

**Three keys a pack cannot do without.** `corpus:` is the id a registry files the package under, `content-version:` is
the version it orders releases by, and `shortcode:` is what a consumer will cite the import as. The descriptor states
all three, so this fixture proves the path where a pack succeeds. Each refusal has a unit test in `PackerTests`, where
an export manifest can be malformed one way at a time.

**A corpus publishing nowhere.** The runner assembles the corpus in a temp directory and initialises no repository, so
the export carries no links. A package holds whatever the export held, which is the point: nothing here depends on the
corpus having an address.

## What is asserted where

`expected-entries.txt` is the archive's whole entry list, read back out of the built file and compared both ways round.
An entry that stopped travelling fails as loudly as one that started, because either is a change to what a consumer
imports.

`expected-nuspec.xml` is the envelope, committed whole. It is the one part of the archive the corpus did not author, so
a change to it would show up nowhere else in the suite.

**The payload is not committed a second time.** Every `corpus/…` entry is compared against the export in the same temp
tree, byte for byte, in both directions. [`export`](../export/README.md) already holds a tracked copy of those bytes,
and a second one here would be a second thing to keep in step. What this fixture adds is that the pack carried them
unedited.

The scenario runs twice. The second run happens over a directory seeded with a package no run backs, which pins the
overwrite as delete-then-write, and the two archives are compared byte for byte. That comparison is the one worth
reading twice: a registry keeps a published version forever, so a byte that moves between two packs of one export is a
byte nobody can account for afterwards.

`expected-pack.txt` holds lines the run must print. `expected-files.txt` names what must be there afterwards, and what
must not.

Regenerate with `dotnet run tooling/kac-tests.cs -- --update pack`, then read the diff. A filter is matched as a
substring, so that command touches this fixture alone.
