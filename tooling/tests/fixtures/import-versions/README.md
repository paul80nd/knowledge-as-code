A corpus whose three imports each stand differently against what their source publishes.

`feed/` is the source, holding one empty file per version under the names `kac pack` writes. A folder publishes no
index, so its listing is the index, and a name is the whole of what a file here has to be. `eng` is locked at a version
its own range would move past, `sec` is locked where the range is what holds it back, and `ops` names a folder that is
not there.

**Every entry reports `import-restored` as well.** Nothing under `.imports/` is committed, so this corpus is in the
state a fresh clone is in. That is the honest pairing: the lock and the range sit in `.corpus.yaml` and are readable
whether or not a restore has run, so a clone learns it is behind before it fetches anything.

**The three severities are the point.** `import-behind` is a warning, because the corpus said it would take that
version and has not. `import-capped` is information, because a range capping a corpus at a major is doing what it was
written to do. `import-unreachable` is information too, because a run that cannot reach a source is not a corpus at
fault. None of the three fails the build, and the exit code here is 1 for the errors alone.

`FreshnessTests` holds the comparison itself, over sources built from values. What this fixture adds is the path a real
run takes: a descriptor read from disk, a folder read as a feed, and the findings landing against the file that
declares both halves.
