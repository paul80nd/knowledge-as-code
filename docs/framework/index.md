# The framework

Knowledge as Code is a way of holding engineering knowledge: plain Markdown in git, where every document carries a type
and every type declares a schema. `kac` is the tool that holds a corpus to that schema. This section is the framework
itself, meaning the ideas the tool serves rather than the commands that serve them.

A **corpus** is one repository of knowledge documents, with the schema it runs. Every corpus takes the same framework
and fills it with its own knowledge.

## The pages here

* **[Principles](principles.md)** says why the framework is shaped the way it is, and what the design will not trade
  away.
* **[Taxonomy](taxonomy.md)** says what a type and a tier are, what the five tiers ask, and the shape a type takes on
  disk.
* **[The default types](types.md)** introduces every type that ships, grouped by tier, one line each.
* **[Metadata](metadata.md)** says what a record carries in frontmatter, how ids are formed, and how a citation reaches
  a part of one.
* **[Contributing](contributing.md)** says how knowledge is added, reviewed and merged, and where the rules for the
  words themselves live.
* **[Automation](automation.md)** says what a pipeline does for a corpus, and what it leaves alone.
* **[Lineage](lineage.md)** says how the types relate to their prior art, and on what terms.

## What a corpus holds beside these

Four pages travel with each corpus as well. Three of them carry tables generated from the types that corpus adopted, so
a corpus that took five of the framework's types gets five rows linking to pages it holds. Contributing carries no
generated block: it is the way in, and what a corpus adds locally.

| In the corpus | Answers                                                              |
|---------------|----------------------------------------------------------------------|
| Taxonomy      | which types this corpus adopted, and where a record goes             |
| Metadata      | which fields this corpus's own types carry                           |
| Lineage       | the prior art behind each type this corpus adopted                   |
| Contributing  | the way in to all of the above, and anything the corpus asks locally |

Each sits under `knowledge-as-code/` in the corpus, beside a root page that is the way in. Every one of them is the
narrow, corpus-facing half of a page here.
