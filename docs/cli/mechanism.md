# `mechanism` compare a corpus against upstream, or sync from it

<!-- BEGIN GENERATED: usage-mechanism -->

```text
kac mechanism [--against <PATH>] [--check] [--no-color] [--sync]
```

| Option             | What it does                                                                         |
|--------------------|--------------------------------------------------------------------------------------|
| `--against <PATH>` | Name the reference corpus by path. Defaults to upstream.url in .corpus.yaml.         |
| `--check`          | Compare the shared layers against a reference and report drift. Never writes.        |
| `--no-color`       | Turn colour off. NO_COLOR in the environment does the same.                          |
| `--sync`           | Take the shared layers from the reference, then record what it took in .corpus.yaml. |

<!-- END GENERATED: usage-mechanism -->

## What it is for

A corpus takes the framework as a copy (the validator, the schema, and the documents describing how the system works),
and a copy drifts. `mechanism` is what makes a copy answerable to a declaration.
[`manifest.yaml`](https://github.com/paul80nd/knowledge-as-code/blob/main/tooling/manifest.yaml) declares each file's
layer: `synced`, `verification`, `forked`, `generated`, `local` or `ignored`. `mechanism` enforces that declaration
from both ends. `--check` reports how far a corpus has moved from a reference. `--sync` takes the shared layers from
one. Its reader is whoever maintains a corpus downstream of this one.

## What it is not

**It is not `git pull`.** The two trees are separate repositories with separate histories. `--sync` copies the shared
layers file by file and records what it took. It merges nothing, and neither half reads either side's commits.

**It is not `generate --check`.** Both compare, and they compare different halves of a file. `mechanism`
empties every generated block before it compares, so what it judges is the authored prose. What sits between the markers
is `generate --check`'s alone, and a shared page can be byte-identical everywhere and stale everywhere.

**It is not `validate`.** Drift is not invalidity. A corpus that has edited its copy of the schema has drifted, and may
be entirely valid. A corpus in step with upstream may be full of broken records.

## How it works

The two halves read one manifest and share one vocabulary of layers. `--check` decides and reports. `--sync` decides
and then writes. Neither touches a layer the manifest says a corpus owns.

### `--check`

`mechanism --check` resolves every tracked file against the manifest and compares the shared layers against a reference
corpus. It follows the same discipline as `generate --check`: compare, name what differs, exit non-zero,
never write.

```bash
kac mechanism --check --against ../other-corpus
```

#### What it reports

The reference defaults to `upstream.url` in `.corpus.yaml`, so a corpus that recorded where it synced from can run a
bare `mechanism --check`. It reports:

- **synced** and **verification** files that differ, or are missing on either side. Each is an **error** (exit `1`).
- any file at all matching no manifest rule, whatever layer it would otherwise have taken.
- **forked** files that differ are counted, and never failed on, because a forked file is meant to diverge.
- **generated**, **local** and **ignored** files are skipped, because a corpus builds its own, owns its own, or has no
  business comparing them.
- **files named under `skip:`** in `.corpus.yaml` are honoured rather than flagged, and reported as `RESOLVED` once
  they match the reference again, so you can delete the stale entry.
- **what the descriptor declines** is skipped, and counted where the corpus holds it anyway.

#### The versions it opens with

It reports the three versions the descriptor states: `content-version`, `descriptor-version` and
`upstream.template-version`. A version the corpus has not stated is reported as not declared, because only the corpus
can say what it knows. A descriptor still carrying the older `version:` key stops the command outright, in either half.
The message names the old key, the new one and the file. The rename is the corpus's to make: nothing rewrites this file
on a corpus's behalf.

#### What a corpus may decline

A corpus declines in two ways, and both work alike. Leaving a type out of `types:` leaves out its `.schema/<type>.yaml`,
so that file is neither missing nor drifted. Setting `role:` to `consumer` does the same for the `verification` layer,
because a consumer runs a tool proven upstream instead of proving it. These are the only ways a corpus may hold less of
a shared layer than upstream does, and the descriptor is where it says so. Without that entry the same absence reads as
a deletion nobody recorded. A descriptor that declares neither takes the whole shared layer.

#### What it compares

`--check` normalises line endings before it compares, so a working copy checked out with CRLF never reads as drift. It
then compares the **authored half** of each file, emptying everything between `BEGIN GENERATED` and `END GENERATED`
first. So a shared page may carry a block built from the corpus holding it. The taxonomy's tables are the case, because
they list the types that corpus adopted. The prose around the block stays byte-identical everywhere. The markers
themselves are compared, so deleting a block instead of regenerating it is still drift. `generate --check` stays the
one voice on whether the generated half is right.

### `--sync`

`mechanism --sync` takes the shared layers from the reference, records what it took, and regenerates.

```bash
kac mechanism --sync                      # from upstream.url
kac mechanism --sync --against ../source  # …or from a local checkout of it
```

#### Where it takes from

`--against` says which copy of the upstream to read. `upstream.url` says the corpus takes from an upstream at all. A
corpus that names none sits at the head of the chain: changes leave it and none arrive, so `--sync` refuses to run
there. A corpus that names one syncs from it whatever its role, so a mirror of the framework takes the tooling and the
tests down like anything else.

#### What comes down

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

#### What it records

Sync then stamps `descriptor-version`, `upstream.template-version` and `upstream.taken-on` into `.corpus.yaml`. It
leaves `upstream.commit` alone: a sync reads a directory rather than a git ref, so it has no commit to record.
It rewrites those four lines rather than re-serialising the file, so the descriptor's commentary survives. The file's
own format is the mechanism's to state, because a corpus cannot know the shape a newer one writes. `content-version` is
left alone: what a corpus knows is not something an upstream can tell it.

#### It regenerates last

The run finishes by calling `generate`, and copying a page whole is only safe because of that step. The page
arrives carrying the reference's generated block, and it is right only once rebuilt against the types the
receiving corpus holds. A passing `generate --check` is sync's postcondition.

## Known limits

**`--sync` can exit `1` after it has written.** Regeneration runs last, and a failure there leaves the files in place
with the generated blocks unrebuilt. The message says so, and `kac generate` on its own finishes the job.

**A reference whose own manifest cannot place its own tree stops the run.** That is a defect upstream rather than in
the corpus syncing from it, and nothing is written.
