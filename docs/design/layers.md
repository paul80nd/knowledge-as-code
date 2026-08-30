# Layers

Which files in a corpus belong to the framework, which belong to the corpus, and what happens to each when a newer
framework arrives. [`new`](../cli/new.md) writes them the first time and [`update`](../cli/update.md) writes them
again, and both read the same declaration.

A **corpus** is one repository of knowledge records kept in git. The **template** is the repository or folder a corpus
takes the framework from. A **layer** says who owns a file and what a difference from upstream means.

## The two halves move apart

The framework reaches a corpus by two routes. `kac` itself comes from nuget.org, and
`dotnet tool update KnowledgeAsCode.Tool` moves it. Everything else comes from the template `.corpus.yaml` points at,
and `update` moves that.

A corpus can run a new tool over an old copy of the framework's files, or the reverse, and neither is a fault on its
own. The template's manifest names the oldest tool that can read it in `minimum-tool`, which is the one place the two
are held together. An older tool meets that key and stops, because half-reading a template it cannot understand would
be worse.

## Every file resolves to exactly one layer

The template's `manifest.yaml` declares the rules, and the first matching rule wins. A rule with no `to:` lands its
file on the path it was read from, which is how `.schema/` reaches a corpus root. `to:` sends a file somewhere else,
which is how `template/knowledge-as-code/` lands at a corpus's own `knowledge-as-code/`.

| Layer      | What happens                                                                                   |
|------------|------------------------------------------------------------------------------------------------|
| `overlay`  | Written wherever the two copies differ. This is framework property and an edit to it is drift. |
| `seed`     | Written when absent. Written again only under `update-policy: full`.                           |
| `removed`  | Deleted. A tombstone in the manifest, so a removal is stated rather than inferred.             |
| `withheld` | Never written. The template's own machinery.                                                   |

**Drift** is a file in the overlay layer that no longer matches the upstream it came from. A manifest and a descriptor
answer it, so a necessary deviation does not look like an accident.
[The mechanism is separable](../framework/principles.md#the-mechanism-is-separable-from-the-knowledge) argues why the
split is worth carrying.

## A seed is the corpus's own words

A type's root page and its `_template.md` arrive carrying the framework's wording and are rewritten in the corpus's own
domain. Refreshing them on every run would open each update with three dozen files to revert by hand.

So `update-policy: cautious` in [`.corpus.yaml`](../corpus-descriptor.md) is the default, and writes a seed only where
the corpus has none. `full` refreshes them and hands the reconciliation to the diff. `--policy` overrides either for
one run.

A seed belongs to the corpus once written, so retiring one is the corpus's own call.

## A seeded record is matched by its id

The template seeds a record at the top of its type's folder, as `policies/devi-deviations-are-recorded.md`. A corpus
is free to file that record deeper, and a type may read the folder below it as the record's category, so
`policies/governance/devi-deviations-are-recorded.md` is an ordinary place for it to end up.

Compared by path alone, that corpus reads as holding no such seed. The update would offer a copy at the seeded path,
and accepting it leaves two records carrying one id, which `kac validate` then fails on `id-unique`.

So a seed absent from its path is looked for by the id it carries, anywhere under its type's folder. A match is the
corpus's copy of that record: `cautious` leaves it alone, and `full` refreshes it where the corpus filed it.

## A deletion is declared, never guessed

A file missing from the template is not evidence it was dropped. It is as likely to be evidence of a mistake upstream.
Only `layer: removed` deletes, and it deletes exactly what it names.

## `skip:` is how a corpus takes a file back

A path listed there is neither read nor written, in either direction. It is the one way to say that you own a file the
overlay would otherwise reclaim on every run:

```yaml
skip:
  - path: .plugin/hooks/breadcrumb
    reason: Patched for our proxy.
```

`update` steps over each one and reports what it stepped over. The reason is for whoever opens the file next, and it is
the only thing standing between a deliberate divergence and one nobody remembers making.

## A shared plugin tree is withheld

`plugin.from` in `.corpus.yaml` sends [`bundle`](../cli/bundle.md) to one tree elsewhere, so a corpus naming it holds
none of that tree and an update writes none of it. Its own `.plugin/.claude-plugin/plugin.json` is a seed and arrives
all the same, because the manifest names the plugin and lists the components that corpus declares.

A corpus adopting the key with the old copies still on disk is told. Each one is reported as a file the template sends
nothing to, and `--check` fails on it, because a corpus's own file wins the merge and a leftover would go on shipping
after every upstream change.

## A continuous integration starter is refreshed and never introduced

A manifest rule may declare `ci:`, naming the system its files serve, and `new` writes the matching starter alone.
Which system builds a repository is that repository's own answer, so an update leaves a starter the corpus does not
hold where it is. A corpus that wants one copies it across by hand.

A GitHub Actions workflow reaching a corpus built elsewhere is worse than unread. On github.com it runs uninvited.

## Three files no template can send

**`.corpus.yaml` is composed, never copied.** No template can carry a descriptor without carrying somebody else's name
in it. So `new` builds the file from the answers it was given and stamps it with the `upstream:` block: the URL, the
path within it, the ref followed, the commit resolved, the template's version and the date. That block is what `update`
reads later.

`shortcode:` always arrives bare. `new` neither asks for it nor invents one, because a shortcode cannot be changed
once another corpus has cited it. `path:`, `ref:` and `commit:` arrive bare too wherever the take could not answer
them, which is every `--from` naming a folder, since a folder resolves no commit.

**`README.md` is written, because the template's own is withheld.** The template's README describes the template rather
than a corpus, so a corpus that copied everything would arrive with no README at all. What `new` writes is short: the
corpus's name, what it holds, and how to run the tool against it.

It arrives carrying the markers for the block it may hold, and a line saying so. It is the one page a corpus may
decline a block on. A README written without them would decline on every new corpus's behalf, and nobody would have
chosen that.

**The breadcrumb hook arrives with its execute bit.** `.plugin/hooks/breadcrumb` is run rather than read, and a hook
arriving without its mode bit fails silently on Unix.

## `--check` answers in both directions

`update --check` computes the plan, prints what would change, writes nothing, and exits non-zero if anything would.

It also reports the reverse. A file the corpus keeps, where the rules call the area `overlay`, that the template sends
nothing to, is a framework change made in the wrong tree. It would reach no other corpus, and nothing in this one reads
as though anything is missing, so the check is the only place it surfaces. Move it upstream, or claim it with a `skip:`
entry.

This is what proves the framework's own repository, where each corpus under `examples/` holds a materialised copy of
what the template sends. A file whose destination is where it was already read from is shared with every corpus there
rather than copied into each. `.schema/` and the two writing skills are the rules in that position.

## Where to go next

[The corpus descriptor](../corpus-descriptor.md) is the reference for every key `new` writes and `update` reads.
