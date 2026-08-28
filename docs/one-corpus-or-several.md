# One corpus or several

`kac` runs two ways. One corpus holds everything a team knows, in one repository. Several corpora hold it in layers,
where one publishes what the organisation is bound by and the others consume it. This page says which one to reach for,
and what the second costs.

A **corpus** is one repository of knowledge records kept in git, and a **record** is one Markdown document in it
carrying YAML frontmatter above its prose.

## Start with one corpus

One repository holds every type you adopt: your decisions, your services, your vocabulary, your runbooks. Every id
resolves inside it. `kac validate` reads the working tree and nothing else, and CI is one command over one checkout.

```bash
kac new         # asks what the corpus is called and which types it adopts
kac validate    # frontmatter, links, structure and the graph
```

This is the shape a first corpus arrives in, and the shape most stay in. It is also what
[One corpus, one bounded context](framework/principles.md#one-corpus-one-bounded-context) asks for: a corpus that needs
nothing outside itself is one a session can clone and read whole.
[`examples/library/`](https://github.com/paul80nd/knowledge-as-code/tree/main/examples/library) is a worked one.

## Split when two groups own the knowledge

The question is ownership, and never size. Reach for a second corpus when all three of these are true.

**Two groups approve at different speeds.** An organisation-wide policy is agreed broadly and changes over months. A
payments team's runbook is theirs and changes this afternoon. Held in one repository, the slow half gates the fast half
on every commit.

**The boundary is already a repository boundary.** The teams keep separate repositories and separate pipelines today. A
corpus per bounded context follows a line that exists, rather than drawing a new one.

**The alternative is copying.** Without a split, every team copies the standards it is bound by, and the copies drift
until nobody can say which one binds.
[One authoritative owner](framework/principles.md#one-authoritative-owner) is what the layering keeps, and it is the
only thing it keeps that a folder inside one corpus could not.

Two of the three is not enough. A large corpus with one owner stays one corpus.

## What the split costs

**The producer publishes and the consumer restores.** `kac export` and `kac pack` seal a version, and `kac restore`
fetches it. A pipeline that ran one command now runs three, and a fresh clone validates nothing until it has restored.

**A version sits between the two corpora.** The consumer names a range and records what it resolved to, so a governance
change reaches a team when that team takes it. That delay is the point, and it is still a thing to keep an eye on.

**A shortcode never changes.** The producer picks the word every downstream citation is written against, and renaming it
would break repositories the producer cannot edit.

**A citation gains a prefix.** `pol-VURM.TIMEBOX` becomes `eng:pol-VURM.TIMEBOX` in every corpus that does not hold the
policy.

None of that is heavy, and all of it is charged on a corpus that did not need to split.

## The two shapes side by side

|                        | One corpus       | Layered corpora                                   |
|------------------------|------------------|---------------------------------------------------|
| Repositories           | one              | one per corpus                                    |
| Owners                 | one group        | one per corpus                                    |
| Every id               | resolves locally | resolves locally, or under a producer's shortcode |
| A pipeline runs        | `kac validate`   | `kac restore`, then `kac validate`                |
| The producer also runs | nothing          | `kac export`, `kac pack`                          |
| A published version    | is not involved  | is what a consumer validates against              |

## The worked corpora

[`examples/`](https://github.com/paul80nd/knowledge-as-code/tree/main/examples) holds one of each, and CI runs `kac`
over all three on every commit.

* **`library/`** is a single corpus, self-contained, taking nothing from outside.
* **`engineering/`** is a governance layer. Its policies name no service and invent no estate, which is what lets a team
  running something else entirely read them.
* **`payments/`** is a domain corpus, and thin on purpose. It consumes `engineering/` and holds nothing that corpus
  already says.

CI publishes from `engineering/`, restores into `payments/`, and then renames a clause upstream to assert that the
downstream build goes red naming the citation nobody downstream touched. That is the promise the layering exists to
keep.

## Where to go next

Staying with one corpus, read [Getting started](getting-started.md). Splitting, read
[Imports](design/imports.md), which says how the mechanism works once you have chosen.
