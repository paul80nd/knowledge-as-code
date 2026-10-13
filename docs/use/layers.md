# Layers

This page says which files in a corpus belong to the framework, which belong to the corpus, and what happens to each
when a newer framework arrives. [`new`](cli/new.md) writes them the first time and [`update`](cli/update.md) writes
them again, and both read the same declaration.

A **corpus** is one repository of knowledge records kept in git. The **template** is the repository or folder a corpus
takes the framework from. A **layer** says who owns a file, and what a difference from upstream means.

## The two halves

`kac` itself comes from nuget.org, and `dotnet tool update KnowledgeAsCode.Tool` moves it. Everything else comes from
the template `.corpus.yaml` points at, and `update` moves that. The template's manifest states the oldest tool that can
read it in `minimum-tool`, and an older tool stops there.

## One file, one layer

The template's `manifest.yaml` declares the rules, and the first matching rule wins. A rule with no `to:` lands its file
on the path it was read from, which is how `.schema/` reaches a corpus root. `to:` sends a file somewhere else, which is
how `template/knowledge-as-code/` lands at a corpus's own `knowledge-as-code/`.

| Layer      | What happens                                                                                   |
|------------|------------------------------------------------------------------------------------------------|
| `overlay`  | Written wherever the two copies differ. This is framework property and an edit to it is drift. |
| `seed`     | Written when absent. Written again only under `update-policy: full`.                           |
| `removed`  | Deleted. A tombstone in the manifest, so a removal is stated rather than inferred.             |
| `withheld` | Never written. The template's own machinery.                                                   |

**Drift** is a file in the overlay layer that no longer matches the upstream it came from.

## Seed files

A type's root page and its `_template.md` arrive with the framework's wording, and you rewrite them in the corpus's own
domain. [`update-policy`](corpus-descriptor.md#update-policy) decides whether an update writes them again, and
`--policy` overrides it for one run. A seed belongs to the corpus once written.

## A seeded record that moved

The template seeds a record at the top of its type's folder, as `policies/devi-deviations.md`, and a corpus may file it
deeper. `kac` looks for a seed absent from its path by the id it states, anywhere under its type's folder, and leaves
a match alone under either policy. To take the template's newer copy of a moved record, rewrite it by hand and fix its
relative links, which were written for the depth it was seeded at.

## Deletions

A file missing from the template is not deleted. Only `layer: removed` deletes, and it deletes exactly what it names.

## `skip:`

A path listed under [`skip:`](corpus-descriptor.md#skip) is one you own and `update` never writes, even in the overlay
layer.

## A shared plugin tree

A corpus naming [`plugin.from`](corpus-descriptor.md#plugin) receives none of the shared plugin tree from an update.
Its own `.plugin/.claude-plugin/plugin.json` is a seed, and arrives all the same. `update` reports an old copy of a
shared file, left under the corpus's `.plugin/`, as one the template sends nothing to. `--check` fails on it, because
`bundle` would read that copy over the shared one.

## CI starters

A manifest rule may declare `ci:`, naming the system its files serve, and `new` writes the matching starter alone. An
update leaves a starter the corpus does not have where it is. A corpus that wants one copies it across by hand.

## Three files no template can send

**`.corpus.yaml` is composed, never copied.** `new` builds the file from the answers it was given and stamps it with
the [`upstream:`](corpus-descriptor.md#upstream) block, which `update` reads later. `shortcode:` always arrives bare.

**`README.md` is written, because the template's own is withheld.** It gives the corpus's name, what it contains, and
how to run the tool against it. It arrives with the markers for the block it may contain, and it is the one page a
corpus may decline a block on.

**The breadcrumb hook arrives with its execute bit.** `.plugin/hooks/breadcrumb` is run, and a hook arriving without
its mode bit fails silently on Unix.

## `--check`

`update --check` computes the plan, prints what would change, writes nothing, and exits non-zero if anything would.

It also reports the reverse: a file the corpus keeps in the overlay layer that the template sends nothing to. That is a
framework change made in the wrong tree. Move it upstream, or claim it with a `skip:` entry.

## Where to go next

[The corpus descriptor](corpus-descriptor.md) is the reference for every key `new` writes and `update` reads.
