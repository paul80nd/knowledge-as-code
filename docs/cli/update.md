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

## What it does

`update` takes a newer framework into a corpus, meaning one repository of knowledge records kept in git, that already
has one. It is also where a corpus adopts a type or gives one up.

It reads where the corpus took its framework from, fetches that template again at its ref, decides file by file what
the corpus receives, writes it, and records what it took. **It leaves every change in the working tree and commits
nothing.** Git is the review step, so run it on a clean tree and read the diff.

[Layers](../design/layers.md) says how each file is decided. It is not a merge tool: where the result is wrong, the
answer is `git checkout` on the file, or a `skip:` entry saying the corpus owns it.

`update` moves the framework's files. `dotnet tool update KnowledgeAsCode.Tool` moves `kac` itself. The two halves move
independently.

## Examples

### Take the newer framework

```bash
kac update
```

Run it on a clean tree. Everything it writes then shows up in `git status` as its own diff.

### Ask what would change, and write nothing

```bash
kac update --check
```

It computes the plan, prints it, and exits non-zero if anything would change. `--check` writes nothing, so it runs over
a tree in any state:

```text
update: comparing this corpus against https://github.com/paul80nd/knowledge-as-code.
update: withheld 61 file(s) for types this corpus has not adopted.
update: withheld 1 continuous integration starter(s) this corpus does not hold.
this corpus is behind its framework. these would change:
WRITE, framework files this corpus holds differently:
  .schema/adrs.yaml
run:  kac update
```

A corpus already in step says so and exits `0`:

```text
update: in step, 40 file(s) compared.
```

Run this in CI to find out that a corpus has fallen behind. It never pushes.

### Adopt a type, or give one up

```bash
kac update --add-type policies
kac update --drop-type tools
```

Adopting writes the type's schema, its root page and its template, and adds the name to `types:`. Dropping is the
asymmetric half, and it refuses where the folder still holds records:

Deleting records is deleting knowledge. The message names the count and offers the two honest ways forward: delete them
deliberately, or leave the type adopted.

### Refresh the seed files too

```bash
kac update --policy full
```

A type's root page and its `_template.md` arrive carrying the framework's wording and are rewritten in your own domain,
so `update-policy: cautious` is the default and leaves them alone. `full` refreshes them and hands the reconciliation to
the diff.

## Known limits

**It needs a clean tree.** This is the whole safety model: everything the command writes has to be distinguishable from
everything you wrote. `--check` is the exception and runs over a tree in any state.

**A type this corpus has not adopted is reported, not adopted.** The run names it and the `--add-type` that would take
it. It cannot tell a type the framework has just added from one declined at creation, so it names both and lets you
decide.

**The descriptor's own shape is stamped, not migrated.** Where `descriptor-version` has moved, the keys the tool owns
are rewritten and anything new or missing is reported for a person to settle.

**The template has no changelog.** What changed in a framework is read from the diff `update` leaves behind.

**It is not [`validate`](validate.md).** A corpus can be perfectly in step with its framework and full of bad records.

[The corpus descriptor](../corpus-descriptor.md) is the reference for every key this command reads and writes.
