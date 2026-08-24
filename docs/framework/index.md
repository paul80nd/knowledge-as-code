# The framework

Knowledge as Code is a way of holding engineering knowledge: plain Markdown in git, where every document carries a type
and every type declares a schema. `kac` is the tool that holds a corpus to that schema. This section is the framework
itself, meaning the ideas the tool serves rather than the commands that serve them.

A **corpus** is one repository of knowledge documents, with the schema it runs. Every corpus takes the same framework
and fills it with its own knowledge.

## The pages here

* **[Principles](principles.md)** says why the framework is shaped the way it is, and what the design will not trade
  away.
* **[Automation](automation.md)** says what a pipeline does for a corpus, and what it leaves alone.
* **[Lineage](lineage.md)** says how the types relate to their prior art, and on what terms.

## What a corpus holds instead

Three pages travel with each corpus rather than sitting here, because each carries tables generated from the types that
corpus adopted. A corpus that took five of the framework's types gets five rows, and the rows link to pages it holds.

| The page     | What it answers                                                       |
|--------------|-----------------------------------------------------------------------|
| Taxonomy     | which kinds of knowledge this corpus holds, and where a document goes |
| Metadata     | what the frontmatter of a record carries                              |
| Contributing | how a contribution is made, reviewed and merged                       |
| Lineage      | the prior art behind each type this corpus adopted                    |

Each sits under `knowledge-as-code/` in the corpus, beside a root page that is the way in.
