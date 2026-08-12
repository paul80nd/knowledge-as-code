---
id: glossary
tier: descriptive
status: draft
review-by: "2027-08-07"
owner: paul.law
---

# Glossary

The words we use, and what we mean by them.

## What is the glossary?

The ubiquitous language of the domain — one entry per term whose meaning is specific to us, or which is easily confused
with a neighbouring term.

Unlike every other type, this is a **single document rather than a collection**. It is meant to be read end to end, and
it carries its own frontmatter as one descriptive document; there are no per-term files and no per-term metadata.

## Why we use it

It is the highest value-per-byte content in the wiki, and the one page a contributor can read in full before starting
work.

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

## Terms

_(One entry per term, alphabetical, flat — no A–Z subheadings. Each is an H3, singular, in its canonical casing:
a one-sentence definition, an optional `**Not:**` line naming what it is confused with, and links out where the detail
lives. One paragraph maximum — this file is read whole.)_

### Example term

A one-sentence definition of what this means in the domain.

**Not:** the neighbouring term it is most often confused with, and the difference in a few words.

Owned by [svc-lending]. See [adr-0001].

## Adding a term

1. Add an H3 in alphabetical position. Do not create a file — this type has no folder.
2. One sentence of definition. If it needs a paragraph, the paragraph belongs in an
   [explanation](/explanations) and the entry links to it.
3. Add a `**Not:**` line wherever confusion is plausible. Those lines are the most useful content here.
4. Name the owning [service](/services) where the concept has one.

**Conventions**

* **Cross-references use the heading anchor** — `[tenant](/glossary#tenant)`. The anchor is the term's identifier; there
  are no numeric ids.
* **Terms are singular and in canonical casing.** `Term`, not `terms`.
* **Keep it tight.** The schema declares `carried-in-full-by-digest` — no entry beyond one paragraph — and nothing runs
  it, so the limit is yours to keep.

## Metadata

The only type whose frontmatter belongs to this page rather than to a record — there is no folder and no per-term file,
so these fields describe `glossary.md` itself.

<!-- BEGIN GENERATED: schema-glossary -->

| Field       | Req | Type   | Notes                                                                                |
|-------------|-----|--------|--------------------------------------------------------------------------------------|
| `id` †      | ●   | string | Stable, unique across the wiki, never reused. Format set by the type.                |
| `tier` †    | ●   | enum   | Fixed for the type — a trust signal for the reader. CI checks it matches the folder. |
| `status` †  | ●   | enum   | `draft` while the terms are still settling.                                          |
| `owner` †   | ●   | string | A named person, never a team alias.                                                  |
| `tags` †    |     | list   | Free-form, lowercase, hyphenated. Used for cross-cutting search.                     |
| `review-by` | ●   | date   | Quoted. The whole file is reviewed at once.                                          |

**Enum values**

| Field    | Values                                                              |
|----------|---------------------------------------------------------------------|
| `tier`   | `decided` · `normative` · `descriptive` · `procedural` · `observed` |
| `status` | `draft` · `active`                                                  |

† Carried by every document in the taxonomy — see [Metadata](/knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-glossary -->

## What CI checks

<!-- BEGIN GENERATED: checks-glossary -->

| Check                       | Level   | What it verifies                                                                                   |
|-----------------------------|---------|----------------------------------------------------------------------------------------------------|
| `frontmatter-parses`        | error   | Frontmatter is present and is a valid YAML mapping.                                                |
| `unknown-key`               | error   | Every frontmatter key is a schema field or a reserved ADO key.                                     |
| `key-order`                 | error   | Key order is a topological extension of the schema's field order.                                  |
| `required-field`            | error   | Required and conditionally-required fields are present.                                            |
| `bare-key`                  | error   | An absent value is a bare key, never `null`, `~`, `""` or `—`.                                     |
| `date-quoted / date-format` | error   | Date fields are quoted `YYYY-MM-DD`.                                                               |
| `enum`                      | error   | Enum values are in range and lowercase.                                                            |
| `field-pattern`             | error   | Values match the pattern their field declares (e.g. `tags`).                                       |
| `list-order`                | warning | List entries read in alphabetical order, with numbers compared as numbers.                         |
| `tier-matches-type`         | error   | `tier` matches the tier the type declares.                                                         |
| `id`                        | error   | `id` is the one value the type declares.                                                           |
| `id-unique`                 | error   | `id` is unique across the whole wiki.                                                              |
| `filename / slug-length`    | error   | Filename matches the pattern; the slug is within 30 characters.                                    |
| `h1`                        | error   | The document has an H1.                                                                            |
| `required-section`          | error   | Every required section heading is present.                                                         |
| `placeholder-left`          | error   | No `{{…}}` from the template is left unfilled, outside code.                                       |
| `link-resolves`             | error   | Every internal link resolves (all forms, `.md` optional), and a `#fragment` names a heading there. |
| `undefined-label`           | error   | Every shortcut reference has a link definition.                                                    |
| `label-canonical`           | error   | A shortcut label that names a document is written as that document's id.                           |
| `unused-definition`         | warning | A link definition that nothing references.                                                         |

**Declared, not yet enforced** — carried by the schema, run by nothing.

| Rule                        | What it would verify                                                                |
|-----------------------------|-------------------------------------------------------------------------------------|
| `carried-in-full-by-digest` | No glossary entry runs beyond one paragraph.                                        |
| `undefined-terms`           | Reports terms appearing more than N times across the corpus with no glossary entry. |
| `unused-terms`              | Reports glossary entries not used anywhere else.                                    |
| `terms-are-singular`        | Entry headings are singular and in canonical casing.                                |

<!-- END GENERATED: checks-glossary -->

[adr-0001]: /adrs/0001-knowledge-as-code.md
[svc-lending]: /services/lending.md
