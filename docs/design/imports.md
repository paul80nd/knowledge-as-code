# Imports

`kac` lets one corpus cite records another corpus published, without holding a copy of them. A payments team writes a
standard naming a clause of an organisation-wide policy, and `kac validate` holds that citation to resolving. Rename
the clause upstream, and the team's build fails on the version that carries the rename.

A **corpus** is one repository of knowledge records kept in git, and a **record** is one Markdown document in it
carrying YAML frontmatter above its prose. This page is the round trip: what each step decides, what it refuses, and
where the boundary between two corpora sits.

This page uses **layer** for a kind of corpus. [Layers](layers.md) uses the same word for who owns one file inside a
corpus.

## The three layers

The **framework** holds the schema, the validator and the agent skills. `kac` reaches a corpus from nuget.org, and
everything else reaches it as a template.

A **governance corpus** holds what the whole organisation is bound by: policies, standards, controls and the
vocabulary they share.
It changes slowly and is approved broadly.

A **domain corpus** holds one bounded context's own knowledge, such as its services, its data and its local decisions.
It changes quickly, and the team running it owns it.

**A domain corpus consumes a governance corpus.** It does not sit inside one. The team stays free to hold decisions the
governance layer has no opinion about, and the governance corpus keeps no list of who reads it.

## The round trip is five steps

The producer runs the first three and the consumer runs the last two.

1. **`kac export`** writes `.dist/export/`: a manifest, one JSON file per record, and a flat file of parts for each type
   that keeps them.
2. **`kac pack`** seals that tree into one file, named for the corpus and the version its content is on.
3. **CI publishes** the file to a registry, which stores every version it has accepted and hands one back to whoever
   asks for it.
4. **`kac restore`** reads the consumer's `consumes:` block, resolves each version range, fetches the package and
   unpacks it under `.imports/<shortcode>/`.
5. **`kac validate`** assembles the local records and the restored imports into one graph, then resolves every citation
   against it.

## A package is versioned by `content-version`

`content-version` in [`.corpus.yaml`](../corpus-descriptor.md) is the number a producer moves by hand when its records
change meaning. A package takes that number, so a consumer depends on a released version of somebody's knowledge.

A registry already solves storage, immutability and serving a named version, so `kac` builds none of that. The envelope
is a `.nupkg`, which is a zip carrying a small XML manifest, and the payload inside it is the export. Neither `pack`
nor anything reading its result needs a NuGet client.

## `consumes:` carries the range and the lock

`version:` is the range the consumer means. `resolved:` is the version the last restore actually took. Intent and lock
sit on one entry, so `.corpus.yaml` stays the single description of what a corpus is.

`restore` refuses two entries claiming one shortcode, because a shortcode names one corpus and both would unpack into
one folder. It also refuses a package whose stamped shortcode disagrees with the entry that fetched it. The producer
owns that spelling: [A shortcode is the half before the
colon](../framework/metadata.md#a-shortcode-is-the-half-before-the-colon) says why it never changes.

## A missing restore is an error

`validate` fails a declared import whose folder under `.imports/` holds no export, and the message names `kac restore`.
Every citation into that shortcode then stays quiet, so a run that has not restored prints one line for the corpus and
none for each reference into it.

A skip would be cheaper and worse. A local run that checks less than the pipeline does is how a broken reference
reaches a default branch, by which time whoever wrote it has stopped looking.

## An import that has fallen behind

`restore` takes the locked version for as long as the range admits it and never asks the registry, so two restores of
an unchanged descriptor write the same bytes. Reproducibility and drift are the same property read twice, so something
else has to ask what the source publishes now. `validate` does, once per run.

**A warning where a newer version sits inside the declared range.** The corpus said it would take that version and has
not, and one command moves it.

**Information where the newer version sits outside the range.** The corpus capped itself on purpose, and reporting a
decision as a problem teaches a reader to skim the output the warnings travel through.

**Never an error.** A version behind is not a broken corpus. Failing on somebody else's release would turn every
downstream red the day the governance layer ships, which is how a build stops meaning anything.

**A source that could not be asked reports too.** A run behind a firewall, or without the token a private feed wants,
cannot tell a current lock from a stale one. Saying so is the alternative to reporting every import as current, which
is the one answer that reads as an assurance.

## One resolution path serves local and imported ids

`Resolver` indexes the records this corpus holds beside the records it imported, and every check walking a reference
asks that one index. A code span in prose and a frontmatter field declaring a `ref:` resolve the same way. So
`implements: eng:pol-VURM.TIMEBOX` names one clause of an imported policy, and both halves are held to existing.

A second path would drift, and it would drift one way. A corpus judged more loosely for having imported the record it
cites is a corpus whose citations are worth less the further they travel.

## Each side keeps its own spelling

A record this corpus holds is cited bare, and one it imported carries its producer's shortcode. Writing either the
other way is an error, and the message names the spelling to write. Two spellings of one obligation defeat every search
anybody runs for it. [Referring to an id](../framework/metadata.md#referring-to-an-id) sets the notation out.

## What a check may ask across the boundary

An export carries each record's declared fields and sections. The graph reads far less: an imported record's id, its
type, its path and the parts it carries, beside the producer's own template for a link into their published form. A
citation needs those, and every export states them whatever type wrote the record.

So a check wanting a producer's prose, frontmatter or git history stops at the boundary. That the words sit in the
export is no argument for reading them. Holding another corpus's records to this corpus's rules would judge a
repository whose owner never agreed to them.

## Where to go next

[`restore`](../cli/restore.md) is the page for declaring an import and fetching it.
