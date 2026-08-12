# Glossary

The words we use, and what we mean by them.

**[→ Index](glossary/_index.md)**

> **Two of the records in `glossary/` are an example estate, not your estate.** `example-libraries.md` and `search.md`
> describe a fictional public-library consortium, and they are here to demonstrate what a second glossary buys: one
> word, `title`, that the estate and one system inside it mean differently. **Delete them before you add your first real
> glossary.** `knowledge-as-code.md` stays — it is the framework's own vocabulary rather than an example of one, it
> arrives synced, and editing it here is drift.

## What is a glossary?

The ubiquitous language of one bounded context — one entry per term whose meaning is specific to us, or which is easily
confused with a neighbouring term.

One file per context, collected under `glossary/`, and each of them is read end to end. The index gives a row per
glossary, so a reader who knows which context they are working in opens that one and reads its vocabulary whole.

## Why we use them

It is the highest value-per-byte content in the wiki, and the corpus-wide glossary is the one page a contributor can
read in full before starting work.

The reason is specific. The terms particular to the domain are often not interchangeable, and neighbouring terms are
easily confused. A contributor — human or agent — who doesn't know the distinctions will produce work that is plausible,
confident and subtly wrong, in code and in documentation alike. Every other document in this wiki assumes these terms
mean something precise; this is where that precision lives.

## Scope

A term belongs here if it is **specific to the domain, or easily confused with something else**. General industry
vocabulary does not — we are not writing a dictionary, and every entry lengthens the page everyone reads.

Not the place for:

* **A component** — that is a [service](/services). The glossary may define the *concept* the service is named after.
* **A rule about using the term** — that is a [standard](/standards).
* **A full explanation of a pattern** — that is an [explanation](/explanations). A glossary entry is a sentence, and
  links out for the rest.

**Split by bounded context or product surface, never by topic.** A glossary covers the language of one context — the
framework itself, a product area, a system that names things its own way. A file called "infrastructure terms" starts an
argument about placement every time somebody adds a word, and the words drift while the argument runs.

**A term goes in the most general glossary that admits it.** A glossary admits a term when the term belongs to its
context: a word the whole estate uses sits in [gls-example-libraries], and a word only search uses sits with search. A
narrower glossary redefines a term the general one carries only where the meaning genuinely differs. Each of the two
entries then names the other, and without that `title` has two definitions and a reader who finds one has no way of
knowing about the other.

**A glossary every corpus shares points upward only.** [gls-knowledge-as-code] holds the framework's own vocabulary and
is synced, so an entry in it cannot name a record this corpus alone has. The narrower entry carries the reference, and
the shared one still reads correctly in a corpus that never had the other half.

## Adding a term

1. Choose the glossary — the most general one that admits the term.
2. Where no glossary covers the context, copy [`_template.md`](glossary/_template.md) to a kebab-case filename named for
   the context.
3. Add an H3 in alphabetical position.
4. One sentence of definition. If it needs a paragraph, the paragraph belongs in an
   [explanation](/explanations) and the entry links to it.
5. Add a `**Not:**` line wherever confusion is plausible. Those lines are the most useful content here.
6. Name the owning [service](/services) where the concept has one.
7. Where a narrower glossary redefines a term, reference the other entry from each. Where the general glossary is
   shared, only the narrower entry carries a reference.

**Conventions**

* **Cross-references name the owning glossary and the term's anchor** — `[title](/glossary/search.md#title)`. The anchor
  is the term's identifier; there are no numeric ids.
* **Terms are singular and in canonical casing.** `Term`, not `terms`.

**Declared.** `carried-in-full-by-digest` holds an entry to one paragraph, and takes the glossaries into the digest
[adr-0001] provides for, most general first. The digest cuts off when its budget is spent rather than overrunning it,
and three areas are enough to spend it. So the ordering decides which vocabulary a session arrives holding. Nothing
generates a digest, so nothing runs the rule and the limit is yours to keep.

## Metadata

<!-- BEGIN GENERATED: schema-glossary -->

| Field       | Req | Type   | Notes                                                                                |
|-------------|-----|--------|--------------------------------------------------------------------------------------|
| `id` †      | ●   | string | Stable, unique across the wiki, never reused. Format set by the type.                |
| `tier` †    | ●   | enum   | Fixed for the type — a trust signal for the reader. CI checks it matches the folder. |
| `status` †  | ●   | enum   | `draft` while the terms are still settling.                                          |
| `owner` †   | ●   | string | A named person, never a team alias.                                                  |
| `tags` †    |     | list   | Free-form, lowercase, hyphenated. Used for cross-cutting search.                     |
| `review-by` | ●   | date   | Quoted. A glossary is reviewed whole, rather than a term at a time.                  |

**Enum values**

| Field    | Values                                                              |
|----------|---------------------------------------------------------------------|
| `tier`   | `decided` · `normative` · `descriptive` · `procedural` · `observed` |
| `status` | `draft` · `active`                                                  |

† Carried by every document in the taxonomy — see [Metadata](/knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-glossary -->

## What CI checks

<!-- BEGIN GENERATED: checks-glossary -->

| Check                       | Level   | What it verifies                                                                                                |
|-----------------------------|---------|-----------------------------------------------------------------------------------------------------------------|
| `frontmatter-parses`        | error   | Frontmatter is present and is a valid YAML mapping.                                                             |
| `unknown-key`               | error   | Every frontmatter key is a schema field or a reserved ADO key.                                                  |
| `key-order`                 | error   | Key order is a topological extension of the schema's field order.                                               |
| `required-field`            | error   | Required and conditionally-required fields are present.                                                         |
| `bare-key`                  | error   | An absent value is a bare key, never `null`, `~`, `""` or `—`.                                                  |
| `date-quoted / date-format` | error   | Date fields are quoted `YYYY-MM-DD`.                                                                            |
| `enum`                      | error   | Enum values are in range and lowercase.                                                                         |
| `field-pattern`             | error   | Values match the pattern their field declares (e.g. `tags`).                                                    |
| `list-order`                | warning | List entries read in alphabetical order, with numbers compared as numbers.                                      |
| `tier-matches-type`         | error   | `tier` matches the tier the type declares.                                                                      |
| `id`                        | error   | `id` carries the type's prefix, takes the shape the type declares, and names the same document as the filename. |
| `id-unique`                 | error   | `id` is unique across the whole wiki.                                                                           |
| `filename / slug-length`    | error   | Filename matches the pattern; the slug is within 30 characters.                                                 |
| `h1`                        | error   | The document has an H1.                                                                                         |
| `identity`                  | error   | An identity line beneath the H1 names the type, id and status, and all three agree with the frontmatter.        |
| `required-section`          | error   | Every required section heading is present.                                                                      |
| `placeholder-left`          | error   | No `{{…}}` from the template is left unfilled, outside code.                                                    |
| `link-resolves`             | error   | Every internal link resolves (all forms, `.md` optional), and a `#fragment` names a heading there.              |
| `undefined-label`           | error   | Every shortcut reference has a link definition.                                                                 |
| `label-canonical`           | error   | A shortcut label that names a document is written as that document's id.                                        |
| `unused-definition`         | warning | A link definition that nothing references.                                                                      |

**Declared, not yet enforced** — carried by the schema, run by nothing.

| Rule                           | What it would verify                                                                        |
|--------------------------------|---------------------------------------------------------------------------------------------|
| `redefinitions-are-reciprocal` | Where a narrower glossary redefines a term, each of the two entries names the other.        |
| `carried-in-full-by-digest`    | No entry runs beyond one paragraph, and a digest carries the glossaries most general first. |
| `undefined-terms`              | Reports terms appearing more than N times across the corpus with no glossary entry.         |
| `unused-terms`                 | Reports entries nothing uses, where a use inside the term's own context counts as one.      |
| `terms-are-singular`           | Entry headings are singular and in canonical casing.                                        |

<!-- END GENERATED: checks-glossary -->

[adr-0001]: /adrs/0001-knowledge-as-code.md
[gls-example-libraries]: glossary/example-libraries.md
[gls-knowledge-as-code]: glossary/knowledge-as-code.md
