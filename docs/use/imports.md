# Imports

`kac` lets one corpus cite records another corpus published, without keeping a copy of them. A payments team writes a
standard naming a clause of an organisation-wide policy, and `kac validate` fails that citation if it does not resolve.
Rename the clause upstream, and the team's build fails on the version that carries the rename.

A **corpus** is one repository of knowledge records kept in git, and a **record** is one Markdown document in it with
YAML frontmatter above its prose. This page follows a citation from the producer's export to the consumer's build.

This page uses **layer** for a kind of corpus. [Layers](layers.md) uses the same word for who owns one file inside a
corpus.

## The three layers

| Layer             | Holds                                                                        | Owned by                    |
|-------------------|------------------------------------------------------------------------------|-----------------------------|
| framework         | the schema, the validator and the agent skills                               | the framework's maintainers |
| governance corpus | what the whole organisation is bound by: policies, standards and controls    | the organisation            |
| domain corpus     | one bounded context's own knowledge, such as its services and its decisions | the team running it         |

`kac` reaches a corpus from nuget.org, and the rest of the framework arrives as a template. A domain corpus consumes a
governance corpus, and the governance corpus keeps no list of who reads it.

## The round trip

The producer runs the first three steps and the consumer runs the last two.

1. **`kac export`** writes `.dist/export/`: a manifest, one JSON file per record, and a flat file of parts for each type
   that keeps them, together with whatever this corpus consumes in turn.
2. **`kac pack`** zips that tree into a `.nupkg`, named for the corpus and its
   [`content-version`](corpus-descriptor.md#content-version).
3. **CI publishes** the file to a registry, which stores every version it has accepted.
4. **`kac restore`** reads the consumer's [`consumes:`](corpus-descriptor.md#consumes) block, resolves each version
   range, fetches the package and unpacks it under `.imports/<shortcode>/`.
5. **`kac validate`** resolves every citation against the local records and the restored imports together.

## A missing restore

`validate` fails a declared import whose folder under `.imports/` holds no export, and the message names `kac restore`.
It prints one line for that corpus and none for each citation into it.

## What an export inherits

An export carries the corpora its own corpus consumes, so a third corpus consuming this one inherits them too, at any
depth. [The export format](../reference/export.md#a-consumer-inherits-what-its-producers-published) says what the merge
settles and what it refuses.

`export.exclude` in [`.corpus.yaml`](corpus-descriptor.md#export) applies to the corpus's own records alone. A package
therefore republishes the inherited records whole, under this corpus's name and version. Check that before you publish
where the two corpora sit under different licences or owners.

## An import that has fallen behind

`restore` keeps the locked version for as long as the range admits it. `validate` asks each source what it publishes
now, once per run:

| The newest version                     | `validate` reports           |
|----------------------------------------|------------------------------|
| sits inside the declared range         | `import-behind`, a warning   |
| sits outside the range                 | `import-capped`, an info     |
| could not be asked of the source       | `import-unreachable`, an info |

A version behind is never an error.

## Spelling a citation

Cite a record this corpus owns bare, and cite an imported one with its producer's shortcode, as
`implements: eng:pol-VURM.TIMEBOX`. Writing either the other way is an error, and the message gives the spelling to
write. A code span in prose and a frontmatter field resolve the same way. [Referring to an
id](../framework/metadata.md#referring-to-an-id) sets the notation out.

**The producer is whoever wrote the record.** A record arriving through a chain keeps the shortcode of the corpus that
wrote it, so `gp:pol-OLD` resolves in a corpus whose `consumes:` names only `eng:`. `eng:pol-OLD` is the error.

## What a check may ask across the boundary

A check reads four things of an imported record: its id, its type, its path and the parts it has. It never reads a
producer's prose, frontmatter or git history, so an imported record is never held to this corpus's rules.

## Where to go next

[`restore`](cli/restore.md) is the page for declaring an import and fetching it.
