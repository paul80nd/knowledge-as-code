# `update` take a newer framework into a corpus

<!-- BEGIN GENERATED: usage-update -->

```text
kac update [--add-type <TYPES>] [--check] [--drop-type <TYPE>] [--from <URL|PATH>] [--no-color] [--path <PATH>] [--policy <POLICY>] [--ref <REF>] [--yes]
```

| Option               | What it does                                                                                             |
|----------------------|----------------------------------------------------------------------------------------------------------|
| `--add-type <TYPES>` | Adopt types the template declares, comma-separated, and write each one's schema, root page and template. |
| `--check`            | Report what would change and write nothing. Fails where anything would.                                  |
| `--drop-type <TYPE>` | Give up a type. Refused where its folder still holds records.                                            |
| `--from <URL\|PATH>` | The repository or folder serving the template. Defaults to upstream.url in .corpus.yaml.                 |
| `--no-color`         | Turn colour off. NO_COLOR in the environment does the same.                                              |
| `--path <PATH>`      | The folder inside that repository holding manifest.yaml. Defaults to upstream.path.                      |
| `--policy <POLICY>`  | How far this run goes: cautious or full. Defaults to update-policy in .corpus.yaml.                      |
| `--ref <REF>`        | The branch or tag to take the template from. Defaults to upstream.ref.                                   |
| `--yes`              | Never wait on a credential prompt, for a run with nobody at the keyboard.                                |

<!-- END GENERATED: usage-update -->

## What it does

`update` takes a newer framework into a corpus that already has one. It is also where a corpus adopts a type or gives
one up.

It reads where the corpus took its framework from, fetches that template again at its ref, decides file by file what the
corpus receives, writes it, and records what it took. **It leaves every change in the working tree and commits
nothing.** Git is the review step, so run it on a clean tree and read the diff.

[Layers](../layers.md) says how each file is decided. `update` is not a merge tool. Where the result is wrong,
run `git checkout` on the file, or add a `skip:` entry saying the corpus owns it.

`update` moves the framework's files. `dotnet tool update KnowledgeAsCode.Tool` moves `kac` itself, in the
`.config/dotnet-tools.json` the corpus keeps. `update` never writes that file. The two halves move independently.

## Examples

### A newer framework

```bash
kac update
```

Run it on a clean tree. Everything it writes then shows up in `git status` as its own diff.

### A check that writes nothing

```bash
kac update --check
```

`--check` computes the plan, prints it, and exits non-zero if anything would change. It writes nothing, so it runs over
a tree in any state:

<!-- transcript: illustration, kac update --check -->

```text
update: comparing this corpus against https://github.com/paul80nd/knowledge-as-code at 8a048c1.
update: withheld 61 files for types this corpus has not adopted.
update: withheld 3 files for github and azure-devops that this corpus does not hold. Which system builds it is not an update's to decide.
update: this template also declares controls, data, deviations, explanations, fixes, integrations, nfrs, offerings, policies, postmortems, processes, reports, runbooks, services, tools. Take any of them with kac update --add-type <name>,<name>.
this corpus is behind its framework. These would change:
WRITE, framework files this corpus holds differently:
  .schema/adrs.yaml
run:  kac update
```

A corpus already in step says so and exits `0`:

<!-- transcript: illustration, kac update --check -->

```text
update: in step, 56 files compared.
```

Run this in CI to find out that a corpus has fallen behind. It never pushes.

### A type adopted, or given up

```bash
kac update --add-type policies
kac update --add-type policies,controls,processes
kac update --drop-type tools
```

Adopting writes each type's schema, its root page and its template, and adds its name to `types:`. Name several types,
comma-separated, to adopt them in one run and review them as one diff. The arriving pages link to the types the corpus
already has, and name the types it declined without linking to them. The pages already there name a new type without
linking to it, because each was written while the type was still declined. A record a type seeds arrives with you as
its owner, as [A seeded record's owner](../layers.md#a-seeded-records-owner) says. The run says all three, and lists
each arriving line that lost a link:

<!-- transcript: illustration, kac update --add-type policies -->

```text
update: policies arrives linking to the types this corpus holds. The pages already here name policies without linking
to it, and they are yours to change.
update: wrote human:alex.doe as the owner of policies/devi-deviations.md.
update: these lines name a type this corpus declined, and the link to it is gone. Read each one:
  policies.md:13  standards
  policies.md:14  controls, processes
  policies.md:36  standards
  policies.md:43  controls, adrs
  policies/_template.md:74  standards
  policies/devi-deviations.md:61  controls
  policies/devi-deviations.md:63  processes
```

A plain `update` that seeds a page the template has gained lists its lines the same way.

Dropping is the asymmetric half. `update` refuses while the folder still holds records:

<!-- transcript: illustration, kac update --drop-type tools -->

```text
update: tools/ holds 3 records, and deleting a record is deleting knowledge. Delete them yourself and run this
again, or leave 'tools' adopted.
```

The message gives the count and the two ways forward. Deleting records is yours to do deliberately.

Where the folder is empty, dropping asks first. The page is about to go, and every page still naming it keeps a link
that no longer resolves:

<!-- transcript: illustration, kac update --drop-type tools -->

```text
update: giving up tools deletes its page. Every page still linking to it is left holding a dead link, and `kac
validate` reports the ones it can reach.
update: a reference it cannot parse is reported by nobody. Search the corpus for the name as well, and fix what you
find.
```

That question takes no for an answer by default, because the run deletes files. Pass `--yes` to answer it in advance. A
run with no terminal and no `--yes` refuses rather than guessing:

<!-- transcript: illustration, kac update --drop-type tools -->

```text
update: giving up tools needs an answer, and there is no terminal to ask on. Pass --yes to give it up anyway.
```

### The seed files refreshed

```bash
kac update --policy full
```

`full` writes every seed file, for this run only. [`update-policy`](../corpus-descriptor.md#update-policy) gives both
values.

## Known limits

**It needs a clean tree.** This is the whole safety model: everything the command writes has to be distinguishable from
everything you wrote. `--check` is the exception and runs over a tree in any state.

**A type this corpus has not adopted is reported, not adopted.** The run names it and the `--add-type` that would take
it. `update` cannot tell a type the framework has just added from one declined at creation, so it names both and lets
you decide.

**The descriptor's own shape is stamped, not migrated.** `update` writes `descriptor-version` to the format this tool
produces, and compares nothing. It stamps back a file a newer `kac` wrote, and nothing reports that it did. A renamed
key is the one shape question the run does answer. It stops, gives both spellings, and leaves the file alone.

**The template has no changelog.** To find out what changed in a framework, read the diff `update` leaves behind.

**It is not [`validate`](validate.md).** A corpus can be perfectly in step with its framework and full of bad records.

[The corpus descriptor](../corpus-descriptor.md) is the reference for every key this command reads and writes.
