# The framework

This section is for the framework reader: whoever wants to know how the types are laid out, where they came from, and
how to write a record. Knowledge as Code keeps engineering knowledge as plain Markdown in git, where every document has
a type and every type declares a schema. `kac` is the tool that checks a corpus against that schema.

A **corpus** is one repository of knowledge records, with the schema it runs. Every corpus takes the same framework and
fills it with its own knowledge.

## The pages here

* **[Principles](principles.md)** says why the framework is shaped the way it is, and what the design will not trade
  away.
* **[Taxonomy](taxonomy.md)** says what a type and a tier are, what the four tiers ask, and the shape a type takes on
  disk.
* **[The default types](types.md)** introduces every type that ships, grouped by tier, one line each.
* **[Metadata](metadata.md)** says what a record has in frontmatter, how ids are formed, and how a citation reaches a
  part of one.
* **[Contributing](contributing.md)** says how knowledge is added, reviewed and merged, and where the rules for the
  words themselves live.
* **[Lineage](lineage.md)** says how the types relate to their prior art, and on what terms.
* **[Shaping a type](shaping-a-type.md)** says what a record of each type is for, and why its fields and sections
  serve that.

## What a corpus has beside these

Four pages travel with each corpus as well. Three of them have tables generated from the types that corpus adopted, so a
corpus that took five of the framework's types gets five rows, each linking to a page it holds. Contributing has no
generated block. It is the way in, and what a corpus adds locally.

| In the corpus | What it answers                                                      |
|---------------|----------------------------------------------------------------------|
| Taxonomy      | which types this corpus adopted, and where a record goes             |
| Metadata      | which fields this corpus's own types carry                           |
| Lineage       | the prior art behind each type this corpus adopted                   |
| Contributing  | the way in to all of the above, and anything the corpus asks locally |

Each sits under `knowledge-as-code/` in the corpus, beside a root page that is the way in. Every one of them is the
narrow, corpus-facing half of a page here.
