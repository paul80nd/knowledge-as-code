# `update`: take a newer framework into a corpus

> **Draft, pending implementation.** This page is the specification `kac update` is built to, written before the command
> exists. It describes the command as it will be, in the same voice as its siblings, and becomes an ordinary feature
> page the moment the command ships.

## Intent

`update` takes a newer framework into a corpus that already has one, and is where a corpus adopts a type or gives one
up. It reads where the corpus took its framework from, fetches that template again at its ref, decides file by file what
the corpus receives, writes it, and records what it took.

Its reader is whoever maintains a corpus. The command's promise is narrow and worth stating plainly: **it leaves every
change in the working tree and commits nothing.** Git is the review step, so `update` can be liberal where a tool
without that safety net would have to be timid.

## What it is not

**It is not a merge tool.** It writes files and stops. Where the result is wrong, the answer is `git checkout` on the
file, or a `skip:` entry saying the corpus owns it. Nothing here resolves a conflict, because a clean tree means there
are none.

**It is not `validate`.** Whether a corpus's records are correct is a separate question with a separate answer, and a
corpus can be perfectly in step with its template and full of bad records.

**It is not `generate`.** That recomputes what a corpus derives from its own frontmatter. This one brings in what the
corpus derives from somebody else's framework. A corpus can be fresh and behind, or in step and stale.

## Approach

### Preconditions

1. **A corpus.** A `.corpus.yaml` at or above the working directory. Without one there is nothing to update, and the
   message names `new`.
2. **A clean tree.** This is the whole safety model and it is not negotiable: everything the command writes has to be
   distinguishable from everything the person wrote, and only a clean tree makes that true.
3. **The template**, cloned shallow at the ref `.corpus.yaml` records, or at `--ref` for a single run.
4. **The tool**, checked against the template's `minimum-tool`.

### The plan

Every file the template holds resolves to exactly one layer, and the first matching rule wins. `to:` sends a file
somewhere other than where it sat upstream; the manifest at the template's root is what decides.

| Layer      | What happens                                                                         |
|------------|--------------------------------------------------------------------------------------|
| `overlay`  | Always written. This is framework property and an edit to it is drift, not a change. |
| `seed`     | Written when absent. Written again only under `update-policy: full`.                 |
| `removed`  | Deleted. A tombstone in the manifest, so a removal is stated rather than inferred.   |
| `withheld` | Never written. The template's own machinery.                                         |

**Seed files are the corpus's own words.** A type's root page and its `_template.md` arrive carrying the framework's
wording and are rewritten in the corpus's domain. Refreshing them on every run would open each update with three dozen
files to revert by hand, which is noise rather than control. `update-policy: cautious` in `.corpus.yaml` is the default;
`full` refreshes them and hands the reconciliation to the diff. `--policy` overrides either for one run.

**A deletion is declared, never guessed.** A file missing from the template is not evidence it was dropped. It is as
likely to be evidence of a mistake upstream. Only `layer: removed` deletes, and only within the overlay: once a seed
file has been written, it belongs to the corpus.

**`skip:` is how a corpus takes a file back.** A path listed there is not read and not written, in either direction. It
is the one way to say "I own this and I mean it" about a file the overlay would otherwise reclaim on every run:

```yaml
skip:
  - path: .plugin/hooks/breadcrumb
    reason: Patched for our proxy.
```

### Adopting and giving up a type

`--add-type` writes the type's schema, its root page and its template, and adds the name to `types:`. It is the same
machinery as any other write, pointed at one type's files.

`--drop-type` is the asymmetric half, and it refuses where the folder holds records. Deleting records is deleting
knowledge, and everything else in a corpus exists to serve them. The message names the count and offers the two honest
ways forward: delete them deliberately, or leave the type adopted. Removing the type from `types:` and leaving the
folder standing is a legitimate half-step, and `validate` will say so.

### `--check`

Compute the plan, print what would change, write nothing, and exit non-zero if anything would. The same discipline as
`generate --check`, and for the same reason: a pipeline says whether a corpus has fallen behind, and never pushes.

This is what proves the framework's own repository, where `example/` holds a materialised copy of everything the
template says a corpus receives.

### What it records

The `upstream:` block in `.corpus.yaml` is rewritten: the commit resolved this run, the template's version, and the
date. The ref is followed, not pinned: the commit is written down as what was taken, and nothing reads it back. The
file is rewritten line by line rather than re-serialised, because most of its value is the commentary explaining what
each key means.

## Known limits

**A type the framework has newly added is reported, not adopted.** Silence would hide it and adoption would put folders
in a corpus nobody asked for, so the run names it and the `--add-type` that would take it.

**The descriptor's own shape is stamped, not migrated.** Where `descriptor-version` has moved, the keys the tool owns
are rewritten and anything new or missing is reported for a person to settle. Migration code is infrastructure ahead of
a second version needing it.

**The template has no changelog.** What changed in a framework is read from the diff `update` leaves behind, which is
the same place every other answer here is read from. A second corpus is the point at which that stops being enough.
