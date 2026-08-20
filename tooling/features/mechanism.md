# `mechanism` — portability

## Intent

A corpus takes the framework as a copy — the validator, the schema, and the documents describing how the system works —
and a copy drifts. `mechanism` is what makes a copy answerable to a declaration.
[`manifest.yaml`](../manifest.yaml) declares each file's layer — `synced`, `verification`, `forked`, `generated`,
`local`, `ignored` — and `mechanism` enforces that declaration from both ends. `--check` reports how far a corpus has
moved from a reference. `--sync` takes the shared layers from one. Its reader is whoever maintains a corpus downstream
of this one.

## What it is not

**It is not `git pull`.** The two trees are separate repositories with separate histories. `--sync` copies the shared
layers file by file and records what it took; it merges nothing, and neither half reads either side's commits.

**It is not `generate --check`.** Both recompute and compare, and they compare different halves of a file. `mechanism`
empties every generated block before it compares, so what it judges is the authored prose. What sits between the markers
is `generate --check`'s alone, and a shared page can be byte-identical everywhere and stale everywhere.

**It is not `validate`.** Drift is not invalidity. A corpus that has edited its copy of the schema has drifted and may
be entirely valid; a corpus in step with upstream may be full of broken records.

## Approach

The two halves read one manifest and share one vocabulary of layers. `--check` decides and reports; `--sync` decides and
then writes. Neither touches a layer the manifest says a corpus owns.

### `--check`

`mechanism --check` resolves every tracked file against the manifest and compares the shared layers against a reference
corpus. It follows the same discipline as `generate --check`: recompute, compare, name what differs, exit non-zero,
never write.

```bash
kac mechanism --check --against ../other-corpus
```

The reference defaults to `upstream.url` in `.corpus.yaml`, so a corpus that recorded where it synced from can run a
bare `mechanism --check`. It reports:

- **synced** and **verification** files that differ, are missing on either side, or match no manifest rule — each an
  **error** (exit `1`).
- **forked** files that differ — counted, never failed on, because a forked file is meant to diverge.
- **generated**, **local** and **ignored** files — skipped, because each corpus owns its own.
- **accepted divergences** named in `.corpus.yaml` — honoured rather than flagged, and reported as `RESOLVED` once they
  match the reference again, so you can delete the stale entry.
- **what the descriptor declines** — skipped, and counted where the corpus holds it anyway.

It opens by reporting the three versions the descriptor states: `content-version`, `descriptor-version` and
`upstream.mechanism-version`. A version the corpus has not stated is reported as not declared, because only the corpus
can say what it knows. A descriptor still carrying the older `version:` key stops the command outright, in either half.
The message names the old key, the new one and the file, and the rename is the corpus's to make — nothing rewrites this
file on a corpus's behalf.

A corpus declines in two ways, and both work alike. Leaving a type out of `types:` leaves out its `.schema/<type>.yaml`,
so that file is neither missing nor drifted. Setting `role:` to `consumer` does the same for the `verification` layer,
because a consumer runs a tool proven upstream instead of proving it. These are the only ways a corpus may hold less of
a shared layer than upstream does, and the descriptor is where it says so. Without that entry the same absence reads as
a deletion nobody recorded. A descriptor that declares neither takes the whole shared layer.

`--check` normalises line endings before it compares, so a working copy checked out with CRLF never reads as drift. It
then compares the **authored half** of each file, emptying everything between `BEGIN GENERATED` and `END GENERATED`
first. A shared page may therefore carry a block built from the corpus holding it — the taxonomy's tables list the types
that corpus adopted — while the prose around the block stays byte-identical everywhere. The markers themselves are
compared, so deleting a block rather than regenerating it is still drift. `generate --check` stays the one voice on
whether the generated half is right.

### `--sync`

`mechanism --sync` takes the shared layers from the reference, records what it took, and regenerates.

```bash
kac mechanism --sync                      # from upstream.url
kac mechanism --sync --against ../source  # …or from a local checkout of it
```

`--against` says which copy of the upstream to read. `upstream.url` says the corpus takes from an upstream at all. A
corpus that names none sits at the head of the chain — changes leave it and none arrive — so `--sync` refuses to run
there. A corpus that names one syncs from it whatever its role, so a mirror of the framework takes the tooling and the
tests down like anything else.

In one pass over both trees:

- **synced** and **verification** files come down whole where their authored halves differ. A file already in step stays
  as it is, so a page's generated block survives when the prose around it has not moved.
- **forked** files are *seeded*: copied only where this corpus has none. Sync never reconciles a forked file that is
  already here.
- **What the descriptor declines** never comes down. Leaving a type out withholds its `.schema/<type>.yaml`, its root
  page and everything under its folder, so adopting one means adding a line to `types:` and syncing.
- **Accepted divergences** are skipped and named, with their recorded reason beside them. Delete the entry to take the
  upstream copy, which keeps the decision in one place.
- Files this corpus holds and the reference does not are **named, not deleted**. Sync copies. Emptying a corpus because
  an upstream tree was smaller is not a decision a tool makes.

Sync then stamps `descriptor-version`, `upstream.mechanism-version`, `synced-from` and `synced-on` into `.corpus.yaml`.
It rewrites those four lines rather than re-serialising the file, so the descriptor's commentary survives. The file's
own format is the mechanism's to state, because a corpus cannot know the shape a newer one writes. `content-version` is
left alone: what a corpus knows is not something an upstream can tell it. Finally it runs `generate`. Copying a page
whole is only safe because of that last step: the page arrives carrying the reference's generated block, and is right
only once rebuilt against the types the receiving corpus holds. A passing `generate --check` is sync's postcondition.

