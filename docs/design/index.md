# Design

Why `kac` works the way it does. Each page here takes one part of the tool and sets out the design behind it: what that
part decides, what it refuses, and what the choice costs. Read one when a command page has told you what happens and
you need to know why, or when you are about to change the part it covers.

A **corpus** is one repository of knowledge records kept in git. A **record** is one Markdown document in it, carrying
YAML frontmatter above its prose.

These pages are the specification `kac` is built to. Where a page and the tool disagree, one of the two is a defect.

## The pages here

* **[Discovery](discovery.md)** says which files `kac` opens, which of those count as records, and which it reads for
  something narrower.
* **[Checks](checks.md)** says where a check comes from, what the schema pass refuses, and why a rule is data wherever
  it can be.
* **[Rule expressions](expressions.md)** is the reference for the one-line conditions a type declares, and the facts one
  may ask about a record.
* **[What the schema is held to](held-to.md)** says what `kac` refuses when it loads the schema, and why a declaration
  the tool ignores counts as a defect.
* **[Generation](generation.md)** says what a corpus derives from its own records, where each derived thing lands, and
  why generation writes into hand-written files.
* **[The export format](export.md)** is the contract an export answers to: what each file holds, what a type decides
  about its own records, and which version number moves when either changes.
* **[The plugin bundle](plugin.md)** says how an export becomes something a consumer can install, and what decides which
  parts of it ship.
* **[Layers](layers.md)** says which files in a corpus belong to the framework, which belong to the corpus, and what
  happens to each when a newer framework arrives.

## The schema

`kac` holds every record in a corpus to a schema, and the corpus carries its own. The schema is a folder of YAML files
named `.schema/`, sitting at or above the corpus. One file declares one **type**, meaning one kind of record: what its
records are called, where they live, what fields they carry, and what CI holds them to. Four shared files sit beside
those.

Nothing here is hard-coded in the tool. A corpus that adds a type file gets a validated type, and a corpus that adds a
rule to one gets a check, with no release of `kac` in between.

### The keys a type file may carry

They are described in
[`meta/type.schema.json`](https://github.com/paul80nd/knowledge-as-code/blob/main/.schema/meta/type.schema.json). Each
type file opens with a modeline pointing at it, so an editor with YAML language-server support offers the keys,
describes each one on hover, and marks a wrong one as you type:

```yaml
# yaml-language-server: $schema=./meta/type.schema.json
```

No build reads that file. It advises an author and gates nothing, and a type file written outside an editor meets the
same checks as one written in it.

### What each file in `.schema/` holds

Which file holds what, and what the generator writes from each, is
[`.schema/README.md`](https://github.com/paul80nd/knowledge-as-code/blob/main/.schema/README.md). It travels with the
schema, so a corpus reads its own copy.

## Where to go next

[The CLI reference](../cli/index.md) gives a page to each command, for running any of this.
