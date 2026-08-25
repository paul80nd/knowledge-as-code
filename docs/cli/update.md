# `update` take a newer framework into a corpus

<!-- BEGIN GENERATED: usage-update -->

```text
kac update [--add-type <TYPE>] [--check] [--drop-type <TYPE>] [--from <URL|PATH>] [--no-color] [--path <PATH>] [--policy <POLICY>] [--ref <REF>] [--yes]
```

| Option               | What it does                                                                             |
|----------------------|------------------------------------------------------------------------------------------|
| `--add-type <TYPE>`  | Adopt a type the template declares, and write its schema, root page and template.        |
| `--check`            | Report what would change and write nothing. Fails where anything would.                  |
| `--drop-type <TYPE>` | Give up a type. Refused where its folder still holds records.                            |
| `--from <URL\|PATH>` | The repository or folder serving the template. Defaults to upstream.url in .corpus.yaml. |
| `--no-color`         | Turn colour off. NO_COLOR in the environment does the same.                              |
| `--path <PATH>`      | The folder inside that repository holding manifest.yaml. Defaults to upstream.path.      |
| `--policy <POLICY>`  | How far this run goes: cautious or full. Defaults to update-policy in .corpus.yaml.      |
| `--ref <REF>`        | The branch or tag to take the template from. Defaults to upstream.ref.                   |
| `--yes`              | Never wait on a credential prompt, for a run with nobody at the keyboard.                |

<!-- END GENERATED: usage-update -->

## What it is for

`update` takes a newer framework into a corpus that already has one, and is where a corpus adopts a type or gives one
up. It reads where the corpus took its framework from, fetches that template again at its ref, decides file by file what
the corpus receives, writes it, and records what it took.

Its reader is whoever maintains a corpus. The command's promise is narrow and worth stating plainly: **it leaves every
change in the working tree and commits nothing.** Git is the review step, so `update` can be liberal where a tool
without that safety net would have to be timid.

## What it is not

**It is not how the tool is updated.** The framework is the schema, the tooling and the documentation that travel
between corpora. Its two halves reach a corpus by different routes. `kac` itself comes from nuget.org, and
`dotnet tool update KnowledgeAsCode.Tool` is what moves it. Everything else comes from the template `.corpus.yaml`
points at, and `update` is what moves that.

The two halves move independently. A corpus can run a new tool over an old copy of the framework's files, or the
reverse, and neither is a fault on its own. The template's manifest names the oldest tool that can read it in
`minimum-tool`, which is the one place the two are held together.

**It is not a merge tool.** It writes files and stops. Where the result is wrong, the answer is `git checkout` on the
file, or a `skip:` entry saying the corpus owns it. Nothing here resolves a conflict, because a clean tree means there
are none.

**It is not `validate`.** Whether a corpus's records are correct is a separate question with a separate answer, and a
corpus can be perfectly in step with its framework and full of bad records.

**It is not `generate`.** That recomputes what a corpus derives from its own frontmatter. This one brings in what the
corpus derives from somebody else's framework. A corpus can be fresh and behind, or in step and stale.

## How it works

### Preconditions

1. **A corpus.** A `.corpus.yaml` at or above the working directory. Standing one up where there is none is
   [`new`](new.md).
2. **A clean tree.** This is the whole safety model: everything the command writes has to be distinguishable from
   everything the person wrote, and only a clean tree makes that true. `--check` writes nothing, so it runs over a tree
   in any state.
3. **The template**, cloned shallow at the ref `.corpus.yaml` records, or at `--ref` for a single run.
4. **The tool**, checked against the template's `minimum-tool`.

### The plan

Every file the template names resolves to exactly one layer, and the first matching rule wins. `to:` sends a file
somewhere other than where it sat upstream. The manifest deciding all of this sits at the upstream repository's root, or
in the folder `upstream.path` names.

| Layer      | What happens                                                                                   |
|------------|------------------------------------------------------------------------------------------------|
| `overlay`  | Written wherever the two copies differ. This is framework property and an edit to it is drift. |
| `seed`     | Written when absent. Written again only under `update-policy: full`.                           |
| `removed`  | Deleted. A tombstone in the manifest, so a removal is stated rather than inferred.             |
| `withheld` | Never written. The template's own machinery.                                                   |

**Seed files are the corpus's own words.** A type's root page and its `_template.md` arrive carrying the framework's
wording and are rewritten in the corpus's domain. Refreshing them on every run would open each update with three dozen
files to revert by hand, which is noise rather than control. `update-policy: cautious` in `.corpus.yaml` is the default;
`full` refreshes them and hands the reconciliation to the diff. `--policy` overrides either for one run.

**A deletion is declared, never guessed.** A file missing from the template is not evidence it was dropped. It is as
likely to be evidence of a mistake upstream. Only `layer: removed` deletes, and it deletes exactly what it names. Write
a tombstone for a file the framework owned. A seed belongs to the corpus once written, so retiring one is the corpus's
own call.

**`skip:` is how a corpus takes a file back.** A path listed there is not read and not written, in either direction. It
is the one way to say "I own this and I mean it" about a file the overlay would otherwise reclaim on every run:

```yaml
skip:
  - path: .plugin/hooks/breadcrumb
    reason: Patched for our proxy.
```

**A continuous integration starter is refreshed and never introduced.** A manifest rule may declare `ci:`, naming the
system its files serve, and `new` writes the matching starter alone. Which system builds a repository is that
repository's own answer, so an update leaves a starter the corpus does not hold where it is. A corpus that wants one
copies it across by hand.

### Adopting and giving up a type

`--add-type` writes the type's schema, its root page and its template, and adds the name to `types:`. It is the same
machinery as any other write, pointed at one type's files.

`--drop-type` is the asymmetric half, and it refuses where the folder holds records. Deleting records is deleting
knowledge, and everything else in a corpus exists to serve them. The message names the count and offers the two honest
ways forward: delete them deliberately, or leave the type adopted.

With no record to lose, a drop is the inverse of an adoption. The type's schema file, its root page and its folder all
go, and `types:` stops naming it. Nothing else in the corpus belonged to that type.

### `--check`

Compute the plan, print what would change, write nothing, and exit non-zero if anything would. The same discipline as
`generate --check`, and for the same reason: a pipeline says whether a corpus has fallen behind, and never pushes.

It answers in the other direction too. A file the corpus keeps where the rules call the area `overlay`, that the
template sends nothing to, is a framework change made in the wrong tree. It would reach no other corpus, and nothing in
this one reads as though anything is missing, so the check is the only place it surfaces. Move it upstream, or claim it
with a `skip:` entry.

This is what proves the framework's own repository, where `example/` holds a materialised copy of what the template
sends a corpus. A file whose destination is where it was already read from is shared with both corpora there rather than
copied into either, and `.schema/` is that file.

### What it records

Four keys are rewritten: `descriptor-version`, and `upstream.commit`, `upstream.template-version` and
`upstream.taken-on`. The ref is followed, not pinned: the commit is written down as what was taken, and nothing reads it
back. A template read from a folder resolves no commit, so that key is left as it stands. The file is rewritten one line
at a time, because most of its value is the commentary explaining what each key means.

`--add-type` and `--drop-type` rewrite `types:` as well, and a run passing neither leaves the list alone.

## Known limits

**A type this corpus has not adopted is reported, not adopted.** Silence would hide it and adoption would put folders in
a corpus nobody asked for, so the run names it and the `--add-type` that would take it. It cannot tell a type the
framework has just added from one declined at creation, so it names both and lets you decide.

**The descriptor's own shape is stamped, not migrated.** Where `descriptor-version` has moved, the keys the tool owns
are rewritten and anything new or missing is reported for a person to settle. Migration code is infrastructure ahead of
a second version needing it.

**The template has no changelog.** What changed in a framework is read from the diff `update` leaves behind, which is
the same place every other answer here is read from. A second corpus is the point at which that stops being enough.
